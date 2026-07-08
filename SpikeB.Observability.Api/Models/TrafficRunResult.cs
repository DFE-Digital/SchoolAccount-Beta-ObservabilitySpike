namespace SpikeB.Observability.Api.Models;

public sealed record TrafficRunResult(
    int TotalRequests,
    int SuccessCount,
    int FailureCount,
    IReadOnlyList<TrafficRequestResult> Results);

public sealed record TrafficRequestResult(
    int Number,
    string Scenario,
    int StatusCode,
    bool Success,
    long ElapsedMs,
    string Message);