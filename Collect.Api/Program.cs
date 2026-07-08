using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
            .AddConsoleExporter()
            .AddOtlpExporter();
    });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/api/collect/normal", () =>
{
    return Results.Ok(new
    {
        service = "Collect.Api",
        scenario = "normal",
        message = "COLLECT dependency responded successfully"
    });
});

app.MapGet("/api/collect/slow", async () =>
{
    await Task.Delay(3000);

    return Results.Ok(new
    {
        service = "Collect.Api",
        scenario = "slow",
        message = "COLLECT dependency was slow"
    });
});

app.MapGet("/api/collect/chain", () =>
{
    return Results.Ok(new
    {
        service = "Collect.Api",
        scenario = "chain",
        message = "COLLECT chain endpoint responded successfully",
        nextDependency = "Ruby.Service will be added in Phase 2"
    });
});

app.MapGet("/api/collect/error", () =>
{
    throw new InvalidOperationException("Simulated COLLECT downstream failure");
});

app.Run();