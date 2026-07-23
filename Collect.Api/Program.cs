using Collect.Api.Chaos;
using Collect.Api.Clients;
using Collect.Api.Data;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

var resourceBuilder = ResourceBuilder.CreateDefault()
    .AddService("collect-api", serviceVersion: "spike-b")
    .AddAttributes(new KeyValuePair<string, object>[]
    {
        new("deployment.environment", "local"),
        new("host.name", Environment.MachineName),
    });

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

builder.Services.AddHttpClient<RubyServiceClient>(client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["Downstream:RubyServiceBaseUrl"]
        ?? "http://localhost:5003");
    
    client.Timeout = TimeSpan.FromSeconds(5);
});

builder.Services
    .AddOpenTelemetry()
    .WithMetrics(m => m
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation()
        .AddProcessInstrumentation()
        .AddOtlpExporter())
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
            .AddSource(SqlDiagnostics.ActivitySourceName)
            .AddSqlClientInstrumentation(options =>
            {
                options.RecordException = true;
            })
            .AddConsoleExporter()
            .AddOtlpExporter();
    });

builder.Logging.AddOpenTelemetry(logging =>
{
    logging.SetResourceBuilder(resourceBuilder);
    logging.AddOtlpExporter();
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/api/collect/normal", async (
    ILogger<Program> logger,
    IChaosService chaosService,
    StudentRecordsClient studentRecordsClient,
    CancellationToken cancellationToken) =>
{
    await chaosService.ExecuteAsync(new ChaosRequest(ChaosMode.None), cancellationToken);

    var studentRecords = await studentRecordsClient.NormalAsync(cancellationToken);

    logger.LogInformation("Completed collect-api normal endpoint call");
    
    return Results.Ok(new
    {
        service = "Collect.Api",
        scenario = "normal",
        message = "COLLECT called Student Records successfully",
        downstream = studentRecords
    });
});

app.MapGet("/api/collect/slow", async (
    ILogger<Program> logger,
    IChaosService chaosService,
    StudentRecordsClient studentRecordsClient,
    CancellationToken cancellationToken) =>
{
    await chaosService.ExecuteAsync(
        new ChaosRequest(ChaosMode.Slow, SlowDelayMs: 1_000),
        cancellationToken);

    var studentRecords = await studentRecordsClient.SlowAsync(cancellationToken);
    
    logger.LogInformation("Completed collect-api slow endpoint call");

    return Results.Ok(new
    {
        service = "Collect.Api",
        scenario = "slow",
        message = "COLLECT and Student Records were slow",
        downstream = studentRecords
    });
});

app.MapGet("/api/collect/error", async (
    ILogger<Program> logger,
    IChaosService chaosService,
    StudentRecordsClient studentRecordsClient,
    CancellationToken cancellationToken) =>
{
    await chaosService.ExecuteAsync(new ChaosRequest(ChaosMode.None), cancellationToken);

    var studentRecords = await studentRecordsClient.ErrorAsync(cancellationToken);
    
    logger.LogInformation("Completed collect-api error endpoint call");

    return Results.Ok(new
    {
        service = "Collect.Api",
        scenario = "error",
        message = "COLLECT called Student Records error scenario",
        downstream = studentRecords
    });
});

app.MapGet("/api/collect/timeout", async (
    ILogger<Program> logger,
    IChaosService chaosService,
    StudentRecordsClient studentRecordsClient,
    CancellationToken cancellationToken) =>
{
    await chaosService.ExecuteAsync(
        new ChaosRequest(ChaosMode.Timeout, TimeoutDelayMs: 10_000),
        cancellationToken);

    var studentRecords = await studentRecordsClient.TimeoutAsync(cancellationToken);
    
    logger.LogInformation("Completed collect-api timeout endpoint call");

    return Results.Ok(new
    {
        service = "Collect.Api",
        scenario = "timeout",
        message = "COLLECT timeout-style scenario completed",
        downstream = studentRecords
    });
});

app.MapGet("/api/collect/random-latency", async (
    ILogger<Program> logger,
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
    
    logger.LogInformation("Completed collect-api random-latency endpoint call");

    return Results.Ok(new
    {
        service = "Collect.Api",
        scenario = "random-latency",
        message = "COLLECT responded with random latency and called Student Records",
        downstream = studentRecords
    });
});

app.MapGet("/api/collect/random-failure", async (
    ILogger<Program> logger,
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
    
    logger.LogInformation("Completed collect-api random-failure endpoint call");

    return Results.Ok(new
    {
        service = "Collect.Api",
        scenario = "random-failure",
        message = "COLLECT randomly succeeded and called Student Records",
        downstream = studentRecords
    });
});

app.MapGet("/api/collect/sql-down", async (
    ILogger<Program> logger,
    ICollectSqlRepository sqlRepository,
    CancellationToken cancellationToken) =>
{
    await sqlRepository.ExecuteNormalQueryAsync(cancellationToken);
    
    logger.LogInformation("Completed collect-api sql-down endpoint call");

    return Results.Ok(new
    {
        service = "Collect.Api",
        scenario = "sql-down",
        message = "SQL query completed because SQL Server is available"
    });
});

app.MapGet("/api/collect/ruby", async (
    ILogger<Program> logger,
    IChaosService chaosService,
    RubyServiceClient rubyServiceClient,
    CancellationToken cancellationToken) =>
{
    await chaosService.ExecuteAsync(new ChaosRequest(ChaosMode.None), cancellationToken);
    var ruby = await rubyServiceClient.NormalAsync(cancellationToken);
    
    logger.LogInformation("Completed collect-api ruby endpoint call");

    return Results.Ok(new
    {
        service = "Collect.Api",
        scenario = "ruby",
        message = "COLLECT called the Ruby service successfully",
        downstream = ruby
    });
});

app.MapGet("/api/collect/ruby-slow", async (
    ILogger<Program> logger,
    RubyServiceClient rubyServiceClient,
    CancellationToken cancellationToken) =>
{
    var ruby = await rubyServiceClient.SlowAsync(cancellationToken);
    
    logger.LogInformation("Completed collect-api ruby-slow endpoint call");

    return Results.Ok(new
    {
        service = "Collect.Api",
        scenario = "ruby-slow",
        message = "COLLECT called a slow Ruby service",
        downstream = ruby
    });
});

app.MapGet("/api/collect/ruby-error", async (
    ILogger<Program> logger,
    RubyServiceClient rubyServiceClient,
    CancellationToken cancellationToken) =>
{
    var ruby = await rubyServiceClient.ErrorAsync(cancellationToken);
    logger.LogInformation("Completed collect-api ruby-error endpoint call");
    return Results.Ok(new { service = "Collect.Api", scenario = "ruby-error", downstream = ruby });
});

app.MapGet("/api/collect/ruby-timeout", async (
    ILogger<Program> logger,
    RubyServiceClient rubyServiceClient,
    CancellationToken cancellationToken) =>
{
    var ruby = await rubyServiceClient.TimeoutAsync(cancellationToken);
    logger.LogInformation("Completed collect-api ruby-timeout endpoint call");
    return Results.Ok(new { service = "Collect.Api", scenario = "ruby-timeout", downstream = ruby });
});

app.MapGet("/api/collect/ruby-random-latency", async (
    ILogger<Program> logger,
    RubyServiceClient rubyServiceClient,
    CancellationToken cancellationToken) =>
{
    var ruby = await rubyServiceClient.RandomLatencyAsync(cancellationToken);
    logger.LogInformation("Completed collect-api ruby-random-latency endpoint call");
    return Results.Ok(new { service = "Collect.Api", scenario = "ruby-random-latency", downstream = ruby });
});

app.MapGet("/api/collect/ruby-random-failure", async (
    ILogger<Program> logger,
    RubyServiceClient rubyServiceClient,
    CancellationToken cancellationToken) =>
{
    var ruby = await rubyServiceClient.RandomFailureAsync(cancellationToken);
    logger.LogInformation("Completed collect-api ruby-random-failure endpoint call");
    return Results.Ok(new { service = "Collect.Api", scenario = "ruby-random-failure", downstream = ruby });
});

app.MapGet("/api/collect/ruby-down", async (
    ILogger<Program> logger,
    RubyServiceClient rubyServiceClient,
    CancellationToken cancellationToken) =>
{
    var ruby = await rubyServiceClient.ErrorAsync(cancellationToken);
    logger.LogInformation("Completed collect-api ruby-down endpoint call");
    return Results.Ok(new { service = "Collect.Api", scenario = "ruby-down", downstream = ruby });
});

app.MapGet("/api/collect/chain", async (
    ILogger<Program> logger,
    IChaosService chaosService,
    StudentRecordsClient studentRecordsClient,
    RubyServiceClient rubyServiceClient,
    CancellationToken cancellationToken) =>
{
    await chaosService.ExecuteAsync(
        new ChaosRequest(
            ChaosMode.RandomLatency,
            RandomMinDelayMs: 300,
            RandomMaxDelayMs: 1_500),
        cancellationToken);

    var studentRecords = await studentRecordsClient.NormalAsync(cancellationToken);
    var ruby = await rubyServiceClient.NormalAsync(cancellationToken);
    
    logger.LogInformation("Completed collect-api chain endpoint call");

    return Results.Ok(new
    {
        service = "Collect.Api",
        scenario = "chain",
        message = "COLLECT called Student Records and Ruby successfully",
        studentRecords,
        ruby
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

app.Run();