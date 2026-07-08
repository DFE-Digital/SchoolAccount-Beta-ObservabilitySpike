using Collect.Api.Chaos;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IChaosService, ChaosService>();

builder.Services
    .AddOpenTelemetry()
    .ConfigureResource(resource =>
    {
        resource.AddService(
            serviceName: "collect-api",
            serviceVersion: "spike-b");
    })
    .WithTracing(tracing =>
    {
        tracing
            .AddAspNetCoreInstrumentation(options =>
            {
                options.Filter = context =>
                    !context.Request.Path.StartsWithSegments("/swagger");
            })
            .AddHttpClientInstrumentation()
            .AddSource(ChaosDiagnostics.ActivitySourceName)
            .AddConsoleExporter()
            .AddOtlpExporter();
    });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/api/collect/normal", async (
    IChaosService chaosService,
    CancellationToken cancellationToken) =>
{
    await chaosService.ExecuteAsync(
        new ChaosRequest(ChaosMode.None),
        cancellationToken);

    return Results.Ok(new
    {
        service = "Collect.Api",
        scenario = "normal",
        message = "COLLECT dependency responded successfully"
    });
});

app.MapGet("/api/collect/slow", async (
    IChaosService chaosService,
    CancellationToken cancellationToken) =>
{
    await chaosService.ExecuteAsync(
        new ChaosRequest(ChaosMode.Slow, SlowDelayMs: 3_000),
        cancellationToken);

    return Results.Ok(new
    {
        service = "Collect.Api",
        scenario = "slow",
        message = "COLLECT dependency was slow"
    });
});

app.MapGet("/api/collect/error", async (
    IChaosService chaosService,
    CancellationToken cancellationToken) =>
{
    await chaosService.ExecuteAsync(
        new ChaosRequest(ChaosMode.Error),
        cancellationToken);

    return Results.Ok();
});

app.MapGet("/api/collect/timeout", async (
    IChaosService chaosService,
    CancellationToken cancellationToken) =>
{
    await chaosService.ExecuteAsync(
        new ChaosRequest(ChaosMode.Timeout, TimeoutDelayMs: 10_000),
        cancellationToken);

    return Results.Ok(new
    {
        service = "Collect.Api",
        scenario = "timeout",
        message = "COLLECT dependency eventually responded after timeout-style delay"
    });
});

app.MapGet("/api/collect/random-latency", async (
    IChaosService chaosService,
    CancellationToken cancellationToken) =>
{
    await chaosService.ExecuteAsync(
        new ChaosRequest(
            ChaosMode.RandomLatency,
            RandomMinDelayMs: 500,
            RandomMaxDelayMs: 4_000),
        cancellationToken);

    return Results.Ok(new
    {
        service = "Collect.Api",
        scenario = "random-latency",
        message = "COLLECT dependency responded with random latency"
    });
});

app.MapGet("/api/collect/random-failure", async (
    IChaosService chaosService,
    CancellationToken cancellationToken) =>
{
    await chaosService.ExecuteAsync(
        new ChaosRequest(
            ChaosMode.RandomFailure,
            RandomFailurePercentage: 40),
        cancellationToken);

    return Results.Ok(new
    {
        service = "Collect.Api",
        scenario = "random-failure",
        message = "COLLECT dependency randomly succeeded"
    });
});

app.MapGet("/api/collect/sql-down", async (
    IChaosService chaosService,
    CancellationToken cancellationToken) =>
{
    await chaosService.ExecuteAsync(
        new ChaosRequest(ChaosMode.SqlDown),
        cancellationToken);

    return Results.Ok();
});

app.MapGet("/api/collect/ruby-down", async (
    IChaosService chaosService,
    CancellationToken cancellationToken) =>
{
    await chaosService.ExecuteAsync(
        new ChaosRequest(ChaosMode.RubyDown),
        cancellationToken);

    return Results.Ok();
});

app.MapGet("/api/collect/chain", async (
    IChaosService chaosService,
    CancellationToken cancellationToken) =>
{
    await chaosService.ExecuteAsync(
        new ChaosRequest(
            ChaosMode.RandomLatency,
            RandomMinDelayMs: 300,
            RandomMaxDelayMs: 1_500),
        cancellationToken);

    await chaosService.ExecuteAsync(
        new ChaosRequest(ChaosMode.None),
        cancellationToken);

    return Results.Ok(new
    {
        service = "Collect.Api",
        scenario = "chain",
        message = "COLLECT chain endpoint responded successfully",
        nextDependency = "Ruby.Service will be added in Phase 2"
    });
});

app.Run();