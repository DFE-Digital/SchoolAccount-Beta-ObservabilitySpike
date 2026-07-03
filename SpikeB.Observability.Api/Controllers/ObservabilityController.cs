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
        var result = await collectClient.GetStatusAsync(
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("todo")]
    public async Task<IActionResult> GetTodo(
        CancellationToken cancellationToken)
    {
        var result = await collectClient.GetTodoAsync(
            cancellationToken);

        return Content(result, "application/json");
    }
}