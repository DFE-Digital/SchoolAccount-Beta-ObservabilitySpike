namespace SpikeB.Observability.Api.Models.Traffic;

public sealed record TrafficResponse(
    string Scenario,
    int StatusCode,
    bool Success,
    long DurationMs,
    string Message
);