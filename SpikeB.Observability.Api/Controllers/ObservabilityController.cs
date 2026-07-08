using Microsoft.AspNetCore.Mvc;
using SpikeB.Observability.Api.Clients;

namespace SpikeB.Observability.Api.Controllers;

[ApiController]
[Route("api/simulation")]
public class ObservabilityController(
    DownstreamCollectClient collectClient) : ControllerBase
{
    [HttpGet("normal")]
    public async Task<IActionResult> GetNormal(CancellationToken cancellationToken)
    {
        var result = await collectClient.NormalAsync(cancellationToken);
        return Content(result, "application/json");
    }

    [HttpGet("slow")]
    public async Task<IActionResult> GetSlow(CancellationToken cancellationToken)
    {
        var result = await collectClient.SlowAsync(cancellationToken);
        return Content(result, "application/json");
    }

    [HttpGet("error")]
    public async Task<IActionResult> GetError(CancellationToken cancellationToken)
    {
        var result = await collectClient.ErrorAsync(cancellationToken);
        return Content(result, "application/json");
    }

    [HttpGet("timeout")]
    public async Task<IActionResult> GetTimeout(CancellationToken cancellationToken)
    {
        var result = await collectClient.TimeoutAsync(cancellationToken);
        return Content(result, "application/json");
    }

    [HttpGet("random-latency")]
    public async Task<IActionResult> GetRandomLatency(CancellationToken cancellationToken)
    {
        var result = await collectClient.RandomLatencyAsync(cancellationToken);
        return Content(result, "application/json");
    }

    [HttpGet("random-failure")]
    public async Task<IActionResult> GetRandomFailure(CancellationToken cancellationToken)
    {
        var result = await collectClient.RandomFailureAsync(cancellationToken);
        return Content(result, "application/json");
    }

    [HttpGet("db-down")]
    public async Task<IActionResult> GetDbDown(CancellationToken cancellationToken)
    {
        var result = await collectClient.DbDownAsync(cancellationToken);
        return Content(result, "application/json");
    }

    [HttpGet("ruby-down")]
    public async Task<IActionResult> GetRubyDown(CancellationToken cancellationToken)
    {
        var result = await collectClient.RubyDownAsync(cancellationToken);
        return Content(result, "application/json");
    }

    [HttpGet("chain")]
    public async Task<IActionResult> GetChain(CancellationToken cancellationToken)
    {
        var result = await collectClient.ChainAsync(cancellationToken);
        return Content(result, "application/json");
    }
}