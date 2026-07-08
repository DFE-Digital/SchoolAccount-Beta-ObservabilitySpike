using Collect.Api.Chaos;
using Collect.Api.Clients;
using Collect.Api.Data;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IChaosService, ChaosService>();
builder.Services.AddScoped<ICollectSqlRepository, CollectSqlRepository>();

builder.Services.AddHttpClient<StudentRecordsClient>(client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["Downstream:StudentRecordsBaseUrl"]
        ?? "http://localhost:5040");
});

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
            .AddSqlClientInstrumentation(options =>
            {
                options.RecordException = true;
            })
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
    StudentRecordsClient studentRecordsClient,
    CancellationToken cancellationToken) =>
{
    await chaosService.ExecuteAsync(new ChaosRequest(ChaosMode.None), cancellationToken);

    var studentRecords = await studentRecordsClient.NormalAsync(cancellationToken);

    return Results.Ok(new
    {
        service = "Collect.Api",
        scenario = "normal",
        message = "COLLECT called Student Records successfully",
        downstream = studentRecords
    });
});

app.MapGet("/api/collect/slow", async (
    IChaosService chaosService,
    StudentRecordsClient studentRecordsClient,
    CancellationToken cancellationToken) =>
{
    await chaosService.ExecuteAsync(
        new ChaosRequest(ChaosMode.Slow, SlowDelayMs: 1_000),
        cancellationToken);

    var studentRecords = await studentRecordsClient.SlowAsync(cancellationToken);

    return Results.Ok(new
    {
        service = "Collect.Api",
        scenario = "slow",
        message = "COLLECT and Student Records were slow",
        downstream = studentRecords
    });
});

app.MapGet("/api/collect/error", async (
    IChaosService chaosService,
    StudentRecordsClient studentRecordsClient,
    CancellationToken cancellationToken) =>
{
    await chaosService.ExecuteAsync(new ChaosRequest(ChaosMode.None), cancellationToken);

    var studentRecords = await studentRecordsClient.ErrorAsync(cancellationToken);

    return Results.Ok(new
    {
        service = "Collect.Api",
        scenario = "error",
        message = "COLLECT called Student Records error scenario",
        downstream = studentRecords
    });
});

app.MapGet("/api/collect/timeout", async (
    IChaosService chaosService,
    StudentRecordsClient studentRecordsClient,
    CancellationToken cancellationToken) =>
{
    await chaosService.ExecuteAsync(
        new ChaosRequest(ChaosMode.Timeout, TimeoutDelayMs: 10_000),
        cancellationToken);

    var studentRecords = await studentRecordsClient.TimeoutAsync(cancellationToken);

    return Results.Ok(new
    {
        service = "Collect.Api",
        scenario = "timeout",
        message = "COLLECT timeout-style scenario completed",
        downstream = studentRecords
    });
});

app.MapGet("/api/collect/random-latency", async (
    IChaosService chaosService,
    StudentRecordsClient studentRecordsClient,
    CancellationToken cancellationToken) =>
{
    await chaosService.ExecuteAsync(
        new ChaosRequest(
            ChaosMode.RandomLatency,
            RandomMinDelayMs: 500,
            RandomMaxDelayMs: 4_000),
        cancellationToken);

    var studentRecords = await studentRecordsClient.NormalAsync(cancellationToken);

    return Results.Ok(new
    {
        service = "Collect.Api",
        scenario = "random-latency",
        message = "COLLECT responded with random latency and called Student Records",
        downstream = studentRecords
    });
});

app.MapGet("/api/collect/random-failure", async (
    IChaosService chaosService,
    StudentRecordsClient studentRecordsClient,
    CancellationToken cancellationToken) =>
{
    await chaosService.ExecuteAsync(
        new ChaosRequest(
            ChaosMode.RandomFailure,
            RandomFailurePercentage: 40),
        cancellationToken);

    var studentRecords = await studentRecordsClient.NormalAsync(cancellationToken);

    return Results.Ok(new
    {
        service = "Collect.Api",
        scenario = "random-failure",
        message = "COLLECT randomly succeeded and called Student Records",
        downstream = studentRecords
    });
});

app.MapGet("/api/collect/sql-down", async (
    IChaosService chaosService,
    StudentRecordsClient studentRecordsClient,
    CancellationToken cancellationToken) =>
{
    await chaosService.ExecuteAsync(new ChaosRequest(ChaosMode.None), cancellationToken);

    var studentRecords = await studentRecordsClient.SqlDownAsync(cancellationToken);

    return Results.Ok(new
    {
        service = "Collect.Api",
        scenario = "sql-down",
        message = "COLLECT called Student Records SQL down scenario",
        downstream = studentRecords
    });
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
    StudentRecordsClient studentRecordsClient,
    CancellationToken cancellationToken) =>
{
    await chaosService.ExecuteAsync(
        new ChaosRequest(
            ChaosMode.RandomLatency,
            RandomMinDelayMs: 300,
            RandomMaxDelayMs: 1_500),
        cancellationToken);

    var studentRecords = await studentRecordsClient.NormalAsync(cancellationToken);

    return Results.Ok(new
    {
        service = "Collect.Api",
        scenario = "chain",
        message = "COLLECT chain endpoint called Student Records successfully",
        downstream = studentRecords,
        nextDependency = "Ruby.Service will be added in Phase 2"
    });
});

app.MapGet("/api/collect/sql/normal", async (
    ICollectSqlRepository sqlRepository,
    CancellationToken cancellationToken) =>
{
    await sqlRepository.ExecuteNormalQueryAsync(cancellationToken);

    return Results.Ok(new
    {
        Scenario = "sql-normal",
        Message = "SQL query completed successfully"
    });
});

app.MapGet("/api/collect/sql/slow", async (
    ICollectSqlRepository sqlRepository,
    CancellationToken cancellationToken) =>
{
    await sqlRepository.ExecuteSlowQueryAsync(cancellationToken);

    return Results.Ok(new
    {
        Scenario = "sql-slow",
        Message = "Slow SQL query completed successfully"
    });
});

app.MapGet("/api/collect/sql/timeout", async (
    ICollectSqlRepository sqlRepository,
    CancellationToken cancellationToken) =>
{
    await sqlRepository.ExecuteTimeoutQueryAsync(cancellationToken);

    return Results.Ok(new
    {
        Scenario = "sql-timeout",
        Message = "This should normally timeout before returning"
    });
});

app.MapGet("/api/collect/sql/down", async (
    ICollectSqlRepository sqlRepository,
    CancellationToken cancellationToken) =>
{
    await sqlRepository.ExecuteNormalQueryAsync(cancellationToken);

    return Results.Ok(new
    {
        Scenario = "sql-down",
        Message = "This only succeeds if SQL Server is available"
    });
});

app.Run();