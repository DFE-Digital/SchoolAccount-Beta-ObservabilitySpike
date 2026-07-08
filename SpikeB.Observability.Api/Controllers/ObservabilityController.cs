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
        var result = await collectClient.GetStatusAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("slow")]
    public async Task<IActionResult> GetSlow(CancellationToken cancellationToken)
    {
        var result = await collectClient.GetSlowResponseAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("error")]
    public async Task<IActionResult> GetError(CancellationToken cancellationToken)
    {
        var result = await collectClient.GetFailingResponseAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("chain")]
    public async Task<IActionResult> GetChain(CancellationToken cancellationToken)
    {
        var result = await collectClient.GetChainedResponseAsync(cancellationToken);
        return Ok(result);
    }
}