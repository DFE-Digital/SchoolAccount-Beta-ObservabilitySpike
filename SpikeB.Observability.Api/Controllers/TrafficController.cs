using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SpikeB.Observability.Api.Clients;
using SpikeB.Observability.Api.Models.Traffic;

namespace SpikeB.Observability.Api.Controllers;

[ApiController]
[Route("api/traffic")]
public sealed class TrafficController(DownstreamCollectClient collect) : ControllerBase
{
    [HttpPost("run")]
    public async Task<ActionResult<TrafficResponse>> Run(
        [FromBody] TrafficRequest request,
        CancellationToken cancellationToken)
    {
        var scenario = NormaliseScenario(request.Scenario);
        var stopwatch = Stopwatch.StartNew();

        try
        {
            await RunScenarioAsync(scenario, cancellationToken);

            stopwatch.Stop();

            return Ok(new TrafficResponse(
                scenario,
                StatusCodes.Status200OK,
                true,
                stopwatch.ElapsedMilliseconds,
                "Completed"));
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            return Ok(new TrafficResponse(
                scenario,
                StatusCodes.Status500InternalServerError,
                false,
                stopwatch.ElapsedMilliseconds,
                ex.Message));
        }
    }

    private Task RunScenarioAsync(
        string scenario,
        CancellationToken cancellationToken)
    {
        return scenario switch
        {
            "slow" => collect.SlowAsync(cancellationToken),
            "error" => collect.ErrorAsync(cancellationToken),
            "timeout" => collect.TimeoutAsync(cancellationToken),
            "random-latency" => collect.RandomLatencyAsync(cancellationToken),
            "random-failure" => collect.RandomFailureAsync(cancellationToken),
            "sql-down" => collect.DbDownAsync(cancellationToken),
            "ruby-down" => collect.RubyDownAsync(cancellationToken),
            "chain" => collect.ChainAsync(cancellationToken),
            "random" => RunRandomScenarioAsync(cancellationToken),
            _ => collect.NormalAsync(cancellationToken)
        };
    }

    private Task RunRandomScenarioAsync(CancellationToken cancellationToken)
    {
        var roll = Random.Shared.Next(1, 101);

        return roll switch
        {
            <= 70 => collect.NormalAsync(cancellationToken),
            <= 85 => collect.SlowAsync(cancellationToken),
            <= 95 => collect.ErrorAsync(cancellationToken),
            _ => collect.DbDownAsync(cancellationToken)
        };
    }

    private static string NormaliseScenario(string? scenario)
    {
        if (string.IsNullOrWhiteSpace(scenario))
        {
            return "normal";
        }

        return scenario.Trim().ToLowerInvariant();
    }
}