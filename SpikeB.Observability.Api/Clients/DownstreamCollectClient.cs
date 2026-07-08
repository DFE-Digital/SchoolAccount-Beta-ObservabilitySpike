namespace SpikeB.Observability.Api.Clients;

public sealed class DownstreamCollectClient(
    HttpClient httpClient,
    ILogger<DownstreamCollectClient> logger)
{
    public async Task<object> GetStatusAsync(CancellationToken cancellationToken)
    {
        using var activity =
            Observability.ActivitySource.StartActivity("COLLECT Normal Call");

        activity?.SetTag("downstream.service", "collect-api");
        activity?.SetTag("spike", "spike-b");
        activity?.SetTag("scenario", "normal");

        var response = await httpClient.GetAsync("/api/collect/normal", cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        activity?.SetTag("http.status_code", (int)response.StatusCode);
        activity?.SetTag("downstream.success", response.IsSuccessStatusCode);

        logger.LogInformation("Collect.Api returned {StatusCode}", response.StatusCode);

        return new
        {
            Service = "collect-api",
            Scenario = "normal",
            StatusCode = (int)response.StatusCode,
            Success = response.IsSuccessStatusCode,
            Response = body
        };
    }

    public async Task<string> GetTodoAsync(CancellationToken cancellationToken)
    {
        using var activity =
            Observability.ActivitySource.StartActivity("COLLECT Normal Payload Call");

        activity?.SetTag("downstream.service", "collect-api");
        activity?.SetTag("spike", "spike-b");
        activity?.SetTag("scenario", "normal");

        return await httpClient.GetStringAsync("/api/collect/normal", cancellationToken);
    }

    public async Task<object> GetSlowResponseAsync(CancellationToken cancellationToken)
    {
        using var activity =
            Observability.ActivitySource.StartActivity("COLLECT Slow Call");

        activity?.SetTag("downstream.service", "collect-api");
        activity?.SetTag("spike", "spike-b");
        activity?.SetTag("scenario", "slow-response");

        var response = await httpClient.GetAsync("/api/collect/slow", cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        activity?.SetTag("http.status_code", (int)response.StatusCode);
        activity?.SetTag("downstream.success", response.IsSuccessStatusCode);

        return new
        {
            Service = "collect-api",
            Scenario = "slow-response",
            StatusCode = (int)response.StatusCode,
            Success = response.IsSuccessStatusCode,
            Response = body
        };
    }

    public async Task<object> GetFailingResponseAsync(CancellationToken cancellationToken)
    {
        using var activity =
            Observability.ActivitySource.StartActivity("COLLECT Failing Call");

        activity?.SetTag("downstream.service", "collect-api");
        activity?.SetTag("spike", "spike-b");
        activity?.SetTag("scenario", "error-response");

        try
        {
            var response = await httpClient.GetAsync("/api/collect/error", cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            activity?.SetTag("http.status_code", (int)response.StatusCode);
            activity?.SetTag("downstream.success", response.IsSuccessStatusCode);

            response.EnsureSuccessStatusCode();

            return new
            {
                Service = "collect-api",
                Scenario = "error-response",
                StatusCode = (int)response.StatusCode,
                Response = body
            };
        }
        catch (Exception ex)
        {
            activity?.SetTag("error", true);
            activity?.SetTag("exception.type", ex.GetType().Name);
            activity?.SetTag("exception.message", ex.Message);

            logger.LogError(ex, "Collect.Api downstream failure");

            throw;
        }
    }

    public async Task<object> GetChainedResponseAsync(CancellationToken cancellationToken)
    {
        using var activity =
            Observability.ActivitySource.StartActivity("COLLECT Chained Call");

        activity?.SetTag("downstream.service", "collect-api");
        activity?.SetTag("spike", "spike-b");
        activity?.SetTag("scenario", "service-chain");

        var response = await httpClient.GetAsync("/api/collect/chain", cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        activity?.SetTag("http.status_code", (int)response.StatusCode);
        activity?.SetTag("downstream.success", response.IsSuccessStatusCode);

        return new
        {
            Service = "collect-api",
            Scenario = "service-chain",
            StatusCode = (int)response.StatusCode,
            Success = response.IsSuccessStatusCode,
            Response = body
        };
    }
}