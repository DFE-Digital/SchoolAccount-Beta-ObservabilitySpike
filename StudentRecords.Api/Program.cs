using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

var resourceBuilder = ResourceBuilder.CreateDefault()
    .AddService("student-records-api", serviceVersion: "spike-b")
    .AddAttributes(new KeyValuePair<string, object>[]
    {
        new("deployment.environment", "local"),
        new("host.name", Environment.MachineName),
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
            serviceName: "student-records-api",
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

app.MapGet("/api/student-records/normal", (ILogger<Program> logger) =>
{
    logger.LogInformation("Completed student-records-api normal endpoint call");
    
    return Results.Ok(new
    {
        service = "StudentRecords.Api",
        scenario = "normal",
        message = "Student records returned successfully"
    });
});

app.MapGet("/api/student-records/slow", async (ILogger<Program> logger, CancellationToken cancellationToken) =>
{
    await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);
    
    logger.LogInformation("Completed student-records-api slow endpoint call");

    return Results.Ok(new
    {
        service = "StudentRecords.Api",
        scenario = "slow",
        message = "Student records query was slow"
    });
});

app.MapGet("/api/student-records/error", (ILogger<Program> logger) =>
{
    logger.LogInformation("Completed student-records-api error endpoint call");
    
    return Results.Problem(
        detail: "Student records dependency failed",
        statusCode: StatusCodes.Status500InternalServerError);
});

app.MapGet("/api/student-records/sql-down", (ILogger<Program> logger) =>
{
    logger.LogInformation("Completed student-records-api sql-down endpoint call");
    
    return Results.Problem(
        detail: "SQL Server is unavailable",
        statusCode: StatusCodes.Status500InternalServerError);
});

app.MapGet("/api/student-records/timeout", async (ILogger<Program> logger, CancellationToken cancellationToken) =>
{
    await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
    
    logger.LogInformation("Completed student-records-api timeout endpoint call");

    return Results.Ok(new
    {
        service = "StudentRecords.Api",
        scenario = "timeout",
        message = "Student records dependency eventually responded after timeout-style delay"
    });
});

app.Run();