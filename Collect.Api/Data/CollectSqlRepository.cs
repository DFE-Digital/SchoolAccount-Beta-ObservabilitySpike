using Microsoft.Data.SqlClient;

namespace Collect.Api.Data;

public sealed class CollectSqlRepository(
    IConfiguration configuration,
    ILogger<CollectSqlRepository> logger)
    : ICollectSqlRepository
{
    private readonly string _connectionString = configuration.GetConnectionString("CollectSql")
                                                ?? throw new InvalidOperationException(
                                                    "Connection string 'CollectSql' was not found.");

    public async Task ExecuteNormalQueryAsync(
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                GETUTCDATE() AS CheckedAtUtc,
                @@SERVERNAME AS ServerName,
                DB_NAME() AS DatabaseName;
            """;

        await ExecuteAsync(sql, 5, cancellationToken);
    }

    public async Task ExecuteSlowQueryAsync(
        CancellationToken cancellationToken)
    {
        const string sql = """
            WAITFOR DELAY '00:00:03';

            SELECT
                GETUTCDATE() AS CheckedAtUtc,
                'Slow query completed' AS Result;
            """;

        await ExecuteAsync(sql, 10, cancellationToken);
    }

    public async Task ExecuteTimeoutQueryAsync(
        CancellationToken cancellationToken)
    {
        const string sql = """
            WAITFOR DELAY '00:00:10';

            SELECT
                GETUTCDATE() AS CheckedAtUtc;
            """;

        await ExecuteAsync(sql, 2, cancellationToken);
    }

    private async Task ExecuteAsync(
        string sql,
        int timeoutSeconds,
        CancellationToken cancellationToken)
    {
        await using var connection =
            new SqlConnection(_connectionString);

        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();

        command.CommandText = sql;
        command.CommandTimeout = timeoutSeconds;

        logger.LogInformation(
            "Executing SQL query (timeout {Timeout}s)",
            timeoutSeconds);

        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            // Consume the result so the command fully executes.
        }
    }
}