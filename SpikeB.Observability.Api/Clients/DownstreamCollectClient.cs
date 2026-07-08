namespace SpikeB.Observability.Api.Clients;

public sealed class DownstreamCollectClient(HttpClient httpClient)
{
    public Task<string> NormalAsync(CancellationToken cancellationToken) =>
        GetAsync("/api/collect/normal", cancellationToken);

    public Task<string> SlowAsync(CancellationToken cancellationToken) =>
        GetAsync("/api/collect/slow", cancellationToken);

    public Task<string> ErrorAsync(CancellationToken cancellationToken) =>
        GetAsync("/api/collect/error", cancellationToken);

    public Task<string> TimeoutAsync(CancellationToken cancellationToken) =>
        GetAsync("/api/collect/timeout", cancellationToken);

    public Task<string> RandomLatencyAsync(CancellationToken cancellationToken) =>
        GetAsync("/api/collect/random-latency", cancellationToken);

    public Task<string> RandomFailureAsync(CancellationToken cancellationToken) =>
        GetAsync("/api/collect/random-failure", cancellationToken);

    public Task<string> DbDownAsync(CancellationToken cancellationToken) =>
        GetAsync("/api/collect/sql-down", cancellationToken);

    public Task<string> RubyDownAsync(CancellationToken cancellationToken) =>
        GetAsync("/api/collect/ruby-down", cancellationToken);

    public Task<string> ChainAsync(CancellationToken cancellationToken) =>
        GetAsync("/api/collect/chain", cancellationToken);

    private async Task<string> GetAsync(string path, CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync(path, cancellationToken);

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        response.EnsureSuccessStatusCode();

        return content;
    }
}