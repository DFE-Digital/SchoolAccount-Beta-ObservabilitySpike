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

    client.Timeout = TimeSpan.FromSeconds(5);
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

app.Run();