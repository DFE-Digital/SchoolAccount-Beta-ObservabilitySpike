namespace SpikeB.Observability.Api.Clients;

public sealed class DownstreamCollectClient(
    HttpClient httpClient,
    ILogger<DownstreamCollectClient> logger)
{
    public async Task<object> GetStatusAsync(
        CancellationToken cancellationToken)
    {
        using var activity =
            Observability.ActivitySource
                .StartActivity("COLLECT Health Check");

        activity?.SetTag("downstream.service", "collect");

        var response =
            await httpClient.GetAsync(
                "/health",
                cancellationToken);

        activity?.SetTag(
            "http.status_code",
            (int)response.StatusCode);

        logger.LogInformation(
            "COLLECT returned {StatusCode}",
            response.StatusCode);

        return new
        {
            Service = "COLLECT",
            StatusCode = (int)response.StatusCode
        };
    }
}