using Microsoft.AspNetCore.Mvc;
using SpikeB.Observability.Api.Clients;

namespace SpikeB.Observability.Api.Controllers;

[ApiController]
[Route("api/observability")]
public class ObservabilityController(
    DownstreamCollectClient collectClient) : ControllerBase
{
    [HttpGet("collect")]
    public async Task<IActionResult> GetCollect(
        CancellationToken cancellationToken)
    {
        var result = await collectClient.GetStatusAsync(cancellationToken);

        return Ok(result);
    }

    [HttpGet("todo")]
    public async Task<IActionResult> GetTodo(
        CancellationToken cancellationToken)
    {
        var result = await collectClient.GetTodoAsync(cancellationToken);

        return Content(result, "application/json");
    }

    [HttpGet("slow")]
    public async Task<IActionResult> GetSlow(
        CancellationToken cancellationToken)
    {
        var result = await collectClient.GetSlowResponseAsync(cancellationToken);

        return Ok(result);
    }

    [HttpGet("error")]
    public async Task<IActionResult> GetError(
        CancellationToken cancellationToken)
    {
        var result = await collectClient.GetFailingResponseAsync(cancellationToken);

        return Ok(result);
    }

    [HttpGet("chain")]
    public async Task<IActionResult> GetChain(
        CancellationToken cancellationToken)
    {
        var result = await collectClient.GetChainedResponseAsync(cancellationToken);

        return Ok(result);
    }
}