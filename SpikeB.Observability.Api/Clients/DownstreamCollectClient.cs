namespace SpikeB.Observability.Api.Clients;

public sealed class DownstreamCollectClient(
    HttpClient httpClient,
    ILogger<DownstreamCollectClient> logger)
{
    public async Task<object> GetStatusAsync(CancellationToken cancellationToken)
    {
        using var activity =
            Observability.ActivitySource.StartActivity("COLLECT Health Check");

        activity?.SetTag("downstream.service", "jsonplaceholder");
        activity?.SetTag("spike", "spike-b");

        var response = await httpClient.GetAsync("/todos/1", cancellationToken);

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

    public async Task<string> GetTodoAsync(CancellationToken cancellationToken)
    {
        using var activity =
            Observability.ActivitySource.StartActivity("Get Todo Item");

        activity?.SetTag("downstream.service", "jsonplaceholder");
        activity?.SetTag("spike", "spike-b");

        return await httpClient.GetStringAsync("/todos/1", cancellationToken);
    }

    public async Task<object> GetSlowResponseAsync(CancellationToken cancellationToken)
    {
        using var activity =
            Observability.ActivitySource.StartActivity("Simulated Slow Downstream Call");

        activity?.SetTag("downstream.service", "simulated-collect");
        activity?.SetTag("spike", "spike-b");
        activity?.SetTag("scenario", "slow-response");

        await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);

        return new
        {
            Service = "simulated-collect",
            Scenario = "slow-response",
            DelayMs = 3000
        };
    }

    public async Task<object> GetFailingResponseAsync(CancellationToken cancellationToken)
    {
        using var activity =
            Observability.ActivitySource.StartActivity("Simulated Failing Downstream Call");

        activity?.SetTag("downstream.service", "simulated-collect");
        activity?.SetTag("spike", "spike-b");
        activity?.SetTag("scenario", "error-response");

        try
        {
            var response = await httpClient.GetAsync("/invalid-endpoint", cancellationToken);

            activity?.SetTag("http.status_code", (int)response.StatusCode);
            activity?.SetTag("downstream.success", response.IsSuccessStatusCode);

            response.EnsureSuccessStatusCode();

            return new
            {
                Service = "simulated-collect",
                StatusCode = (int)response.StatusCode
            };
        }
        catch (Exception ex)
        {
            activity?.SetTag("error", true);
            activity?.SetTag("exception.type", ex.GetType().Name);
            activity?.SetTag("exception.message", ex.Message);

            logger.LogError(ex, "Simulated downstream failure");

            throw;
        }
    }

    public async Task<object> GetChainedResponseAsync(CancellationToken cancellationToken)
    {
        using var activity =
            Observability.ActivitySource.StartActivity("Chained Downstream Call");

        activity?.SetTag("downstream.service", "jsonplaceholder");
        activity?.SetTag("spike", "spike-b");
        activity?.SetTag("scenario", "service-chain");

        var todo = await httpClient.GetStringAsync("/todos/1", cancellationToken);
        var user = await httpClient.GetStringAsync("/users/1", cancellationToken);

        return new
        {
            Scenario = "service-chain",
            ServicesCalled = new[]
            {
                "/todos/1",
                "/users/1"
            },
            TodoLength = todo.Length,
            UserLength = user.Length
        };
    }
}