using Microsoft.AspNetCore.Mvc;

namespace SpikeB.Observability.Api.Controllers;

[ApiController]
[Route("api/observability")]
public class ObservabilityController(IHttpClientFactory httpClientFactory) : ControllerBase
{
    [HttpGet("collect")]
    public async Task<IActionResult> GetCollect()
    {
        var client = httpClientFactory.CreateClient();

        var response = await client.GetAsync(
            "https://jsonplaceholder.typicode.com/todos/1");

        var content = await response.Content.ReadAsStringAsync();

        return Ok(content);
    }
}