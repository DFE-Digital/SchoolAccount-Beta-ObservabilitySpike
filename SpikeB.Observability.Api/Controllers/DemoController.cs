using Microsoft.AspNetCore.Mvc;

namespace SpikeB.Observability.Api.Controllers;

[ApiController]
public sealed class DemoController : ControllerBase
{
    [HttpGet("/demo")]
    public IActionResult Index()
    {
        var html = """
        <!doctype html>
        <html lang="en">
        <head>
            <meta charset="utf-8" />
            <meta name="viewport" content="width=device-width, initial-scale=1" />
            <title>Spike B Observability Demo</title>
            <link rel="stylesheet" href="/demo.css" />
        </head>
        <body>
            <main class="shell">
                <section class="hero">
                    <p class="eyebrow">Spike B Observability</p>
                    <h1>School Account → COLLECT chaos simulation</h1>
                    <p>
                        Trigger controlled downstream failures and latency from School Account into COLLECT,
                        then inspect the distributed traces in Grafana and Tempo.
                    </p>
                </section>

                <section class="grid">
                    <button data-url="/api/simulation/normal">Normal</button>
                    <button data-url="/api/simulation/slow">Slow</button>
                    <button data-url="/api/simulation/error">Error</button>
                    <button data-url="/api/simulation/timeout">Timeout</button>
                    <button data-url="/api/simulation/random-latency">Random latency</button>
                    <button data-url="/api/simulation/random-failure">Random failure</button>
                    <button data-url="/api/simulation/db-down">SQL down</button>
                    <button data-url="/api/simulation/ruby-down">Ruby down</button>
                    <button data-url="/api/simulation/chain">Chain</button>
                    <button id="run-chaos" type="button">🔥 Chaos run</button>
                </section>

                <section class="result">
                    <div>
                        <h2>Result</h2>
                        <p id="status">Choose a scenario.</p>
                    </div>
                    <pre id="output">{}</pre>
                </section>
            </main>

            <script src="/demo.js"></script>
        </body>
        </html>
        """;

        return Content(html, "text/html");
    }
}