using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SpikeB.Observability.Api.Clients;
using SpikeB.Observability.Api.Models;

namespace SpikeB.Observability.Api.Controllers;

[ApiController]
[Route("api/traffic")]
public sealed class TrafficController(
    DownstreamCollectClient collectClient) : ControllerBase
{
    private static readonly string[] WeightedScenarios =
    [
        "normal", "normal", "normal", "normal", "normal", "normal", "normal",
        "slow",
        "random-latency",
        "random-failure",
        "timeout",
        "db-down",
        "ruby-down"
    ];

    [HttpPost("run")]
    public async Task<IActionResult> Run(
        [FromBody] TrafficRunRequest request,
        CancellationToken cancellationToken)
    {
        var totalRequests = Math.Clamp(request.TotalRequests, 1, 250);
        var delayMs = Math.Clamp(request.DelayMs, 0, 5_000);

        var results = new List<TrafficRequestResult>();

        for (var i = 1; i <= totalRequests; i++)
        {
            var scenario = WeightedScenarios[Random.Shared.Next(WeightedScenarios.Length)];

            var result = await ExecuteScenarioAsync(
                i,
                scenario,
                cancellationToken);

            results.Add(result);

            if (delayMs > 0 && i < totalRequests)
            {
                await Task.Delay(delayMs, cancellationToken);
            }
        }

        return Ok(new TrafficRunResult(
            TotalRequests: totalRequests,
            SuccessCount: results.Count(x => x.Success),
            FailureCount: results.Count(x => !x.Success),
            Results: results));
    }

    private async Task<TrafficRequestResult> ExecuteScenarioAsync(
        int number,
        string scenario,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = scenario switch
            {
                "normal" => await collectClient.NormalAsync(cancellationToken),
                "slow" => await collectClient.SlowAsync(cancellationToken),
                "random-latency" => await collectClient.RandomLatencyAsync(cancellationToken),
                "random-failure" => await collectClient.RandomFailureAsync(cancellationToken),
                "timeout" => await collectClient.TimeoutAsync(cancellationToken),
                "db-down" => await collectClient.DbDownAsync(cancellationToken),
                "ruby-down" => await collectClient.RubyDownAsync(cancellationToken),
                _ => await collectClient.NormalAsync(cancellationToken)
            };

            stopwatch.Stop();

            return new TrafficRequestResult(
                Number: number,
                Scenario: scenario,
                StatusCode: StatusCodes.Status200OK,
                Success: true,
                ElapsedMs: stopwatch.ElapsedMilliseconds,
                Message: response);
        }
        catch (TaskCanceledException)
        {
            stopwatch.Stop();

            return new TrafficRequestResult(
                Number: number,
                Scenario: scenario,
                StatusCode: StatusCodes.Status504GatewayTimeout,
                Success: false,
                ElapsedMs: stopwatch.ElapsedMilliseconds,
                Message: "The COLLECT dependency timed out.");
        }
        catch (HttpRequestException ex)
        {
            stopwatch.Stop();

            return new TrafficRequestResult(
                Number: number,
                Scenario: scenario,
                StatusCode: StatusCodes.Status502BadGateway,
                Success: false,
                ElapsedMs: stopwatch.ElapsedMilliseconds,
                Message: ex.Message);
        }
    }
}