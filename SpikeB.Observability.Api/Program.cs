using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.Extensions.Http.Resilience;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using SpikeB.Observability.Api;
using SpikeB.Observability.Api.Clients;

var builder = WebApplication.CreateBuilder(args);

var appInsightsConnectionString =
    builder.Configuration["ApplicationInsights:ConnectionString"];

builder.Services.AddControllers();
builder.Services.AddRazorPages();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services
    .AddHttpClient<DownstreamCollectClient>(client =>
    {
        client.BaseAddress =
            new Uri(builder.Configuration["Downstream:CollectBaseUrl"]!);
        
        client.Timeout = Timeout.InfiniteTimeSpan;
    })
    .AddStandardResilienceHandler(options =>
    {
        options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(5);
        options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(10);

        options.Retry.MaxRetryAttempts = 2;
        options.Retry.Delay = TimeSpan.FromMilliseconds(250);

        options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(30);
        options.CircuitBreaker.MinimumThroughput = 5;
        options.CircuitBreaker.FailureRatio = 0.5;
        options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(15);
    });

builder.Services
    .AddOpenTelemetry()
    .WithMetrics(m => m
        .AddAspNetCoreInstrumentation()
        .AddOtlpExporter())
    .ConfigureResource(resource =>
    {
        resource.AddService(
            serviceName: "school-account-api",
            serviceVersion: "spike-b");
    })
    .WithTracing(tracing =>
    {
        tracing
            .AddSource(Observability.ActivitySourceName)
            .AddSource(SimulationDiagnostics.ActivitySourceName)
            .AddAspNetCoreInstrumentation(options =>
            {
                options.Filter = context =>
                    !context.Request.Path.StartsWithSegments("/swagger");
            })
            .AddHttpClientInstrumentation()
            .AddConsoleExporter()
            .AddOtlpExporter();

        if (!string.IsNullOrWhiteSpace(appInsightsConnectionString))
        {
            tracing.AddAzureMonitorTraceExporter(options =>
            {
                options.ConnectionString = appInsightsConnectionString;
            });
        }
    });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.MapGet("/", () => Results.Redirect("/demo"));
}

app.MapControllers();
app.MapRazorPages();

app.Run();