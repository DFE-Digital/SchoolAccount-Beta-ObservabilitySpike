using Azure.Monitor.OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using SpikeB.Observability.Api;
using SpikeB.Observability.Api.Clients;

var builder = WebApplication.CreateBuilder(args);

var appInsightsConnectionString =
    builder.Configuration["ApplicationInsights:ConnectionString"];

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient<DownstreamCollectClient>(client =>
{
    client.BaseAddress =
        new Uri(builder.Configuration["Downstream:CollectBaseUrl"]!);
});

builder.Services
    .AddOpenTelemetry()
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
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddConsoleExporter();

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

app.MapControllers();

app.Run();