using System.Diagnostics;
using Microsoft.Data.SqlClient;

namespace Collect.Api.Data;

public sealed class CollectSqlRepository(
    IConfiguration configuration,
    ILogger<CollectSqlRepository> logger)
    : ICollectSqlRepository
{
    private readonly string _connectionString =
        configuration.GetConnectionString("CollectSql")
        ?? throw new InvalidOperationException(
            "Connection string 'CollectSql' was not found.");

    public Task ExecuteNormalQueryAsync(
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                GETUTCDATE() AS CheckedAtUtc,
                @@SERVERNAME AS ServerName,
                DB_NAME() AS DatabaseName;
            """;

        return ExecuteAsync(
            operationName: "collect-sql-normal",
            sql,
            timeoutSeconds: 5,
            cancellationToken);
    }

    public Task ExecuteSlowQueryAsync(
        CancellationToken cancellationToken)
    {
        const string sql = """
            WAITFOR DELAY '00:00:03';

            SELECT
                GETUTCDATE() AS CheckedAtUtc,
                'Slow query completed' AS Result;
            """;

        return ExecuteAsync(
            operationName: "collect-sql-slow",
            sql,
            timeoutSeconds: 10,
            cancellationToken);
    }

    public Task ExecuteTimeoutQueryAsync(
        CancellationToken cancellationToken)
    {
        const string sql = """
            WAITFOR DELAY '00:00:10';

            SELECT
                GETUTCDATE() AS CheckedAtUtc;
            """;

        return ExecuteAsync(
            operationName: "collect-sql-timeout",
            sql,
            timeoutSeconds: 2,
            cancellationToken);
    }

    private async Task ExecuteAsync(
        string operationName,
        string sql,
        int timeoutSeconds,
        CancellationToken cancellationToken)
    {
        using var activity =
            SqlDiagnostics.ActivitySource.StartActivity(
                operationName,
                ActivityKind.Client);

        activity?.SetTag("peer.service", "sql-server");
        activity?.SetTag("server.address", "sql");
        activity?.SetTag("server.port", 1433);
        activity?.SetTag("db.system", "mssql");
        activity?.SetTag("db.system.name", "mssql");
        activity?.SetTag("db.namespace", "master");
        activity?.SetTag("db.operation.name", "SELECT");
        activity?.SetTag("db.connection_string", "Server=sql,1433;Database=master");
        activity?.SetTag("demo.scenario", operationName);
        activity?.SetTag("demo.sql.timeout_seconds", timeoutSeconds);

        try
        {
            await using var connection =
                new SqlConnection(_connectionString);

            activity?.AddEvent(
                new ActivityEvent("sql.connection.open.started"));

            await connection.OpenAsync(cancellationToken);

            activity?.AddEvent(
                new ActivityEvent("sql.connection.open.completed"));

            await using var command = connection.CreateCommand();

            command.CommandText = sql;
            command.CommandTimeout = timeoutSeconds;

            logger.LogInformation(
                "Executing SQL query {OperationName} with timeout {TimeoutSeconds}s",
                operationName,
                timeoutSeconds);

            await using var reader =
                await command.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                // Consume the result so the command fully executes.
            }

            activity?.SetStatus(ActivityStatusCode.Ok);
            activity?.AddEvent(
                new ActivityEvent("sql.command.completed"));
        }
        catch (Exception exception)
        {
            activity?.SetStatus(
                ActivityStatusCode.Error,
                exception.Message);

            activity?.SetTag(
                "error.type",
                exception.GetType().FullName);

            activity?.SetTag(
                "error.message",
                exception.Message);

            activity?.AddException(exception);

            logger.LogError(
                exception,
                "SQL operation {OperationName} failed",
                operationName);

            throw;
        }
    }
}