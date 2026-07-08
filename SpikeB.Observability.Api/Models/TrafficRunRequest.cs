namespace SpikeB.Observability.Api.Models;

public sealed record TrafficRunRequest(
    int TotalRequests = 50,
    int DelayMs = 250);