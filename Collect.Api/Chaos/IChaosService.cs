namespace Collect.Api.Chaos;

public interface IChaosService
{
    Task ExecuteAsync(ChaosRequest request, CancellationToken cancellationToken);
}