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

        activity?.SetTag("downstream.service", "jsonplaceholder");
        activity?.SetTag("spike", "spike-b");

        var response =
            await httpClient.GetAsync(
                "/todos/1",
                cancellationToken);

        activity?.SetTag("http.status_code", (int)response.StatusCode);
        activity?.SetTag("downstream.success", response.IsSuccessStatusCode);

        logger.LogInformation(
            "Downstream service returned {StatusCode}",
            response.StatusCode);

        return new
        {
            Service = "jsonplaceholder",
            StatusCode = (int)response.StatusCode,
            Success = response.IsSuccessStatusCode
        };
    }

    public async Task<string> GetTodoAsync(
        CancellationToken cancellationToken)
    {
        using var activity =
            Observability.ActivitySource
                .StartActivity("Get Todo Item");

        activity?.SetTag("downstream.service", "jsonplaceholder");
        activity?.SetTag("spike", "spike-b");

        return await httpClient.GetStringAsync(
            "/todos/1",
            cancellationToken);
    }
}