namespace Collect.Api.Data;

public interface ICollectSqlRepository
{
    Task ExecuteNormalQueryAsync(CancellationToken cancellationToken);
    Task ExecuteSlowQueryAsync(CancellationToken cancellationToken);
    Task ExecuteTimeoutQueryAsync(CancellationToken cancellationToken);
}