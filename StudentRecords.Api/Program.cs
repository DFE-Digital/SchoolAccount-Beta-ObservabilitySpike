using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

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

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/api/student-records/normal", () =>
{
    return Results.Ok(new
    {
        service = "StudentRecords.Api",
        scenario = "normal",
        message = "Student records returned successfully"
    });
});

app.MapGet("/api/student-records/slow", async (CancellationToken cancellationToken) =>
{
    await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);

    return Results.Ok(new
    {
        service = "StudentRecords.Api",
        scenario = "slow",
        message = "Student records query was slow"
    });
});

app.MapGet("/api/student-records/error", () =>
{
    return Results.Problem(
        detail: "Student records dependency failed",
        statusCode: StatusCodes.Status500InternalServerError);
});

app.MapGet("/api/student-records/sql-down", () =>
{
    return Results.Problem(
        detail: "SQL Server is unavailable",
        statusCode: StatusCodes.Status500InternalServerError);
});

app.MapGet("/api/student-records/timeout", async (CancellationToken cancellationToken) =>
{
    await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);

    return Results.Ok(new
    {
        service = "StudentRecords.Api",
        scenario = "timeout",
        message = "Student records dependency eventually responded after timeout-style delay"
    });
});

app.Run();