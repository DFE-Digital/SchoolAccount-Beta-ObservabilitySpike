namespace Collect.Api.Clients;

public sealed class RubyServiceClient(HttpClient httpClient)
{
    public Task<string> NormalAsync(CancellationToken cancellationToken) =>
        GetAsync("/api/ruby/normal", cancellationToken);

    public Task<string> SlowAsync(CancellationToken cancellationToken) =>
        GetAsync("/api/ruby/slow?delayMs=2000", cancellationToken);

    public Task<string> ErrorAsync(CancellationToken cancellationToken) =>
        GetAsync("/api/ruby/error", cancellationToken);

    public Task<string> TimeoutAsync(CancellationToken cancellationToken) =>
        GetAsync("/api/ruby/timeout?delayMs=12000", cancellationToken);

    public Task<string> RandomLatencyAsync(CancellationToken cancellationToken) =>
        GetAsync("/api/ruby/random-latency", cancellationToken);

    public Task<string> RandomFailureAsync(CancellationToken cancellationToken) =>
        GetAsync("/api/ruby/random-failure?failurePercentage=40", cancellationToken);

    private async Task<string> GetAsync(string path, CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync(path, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        response.EnsureSuccessStatusCode();
        return content;
    }
}
