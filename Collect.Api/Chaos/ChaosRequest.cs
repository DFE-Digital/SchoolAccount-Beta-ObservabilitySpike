namespace Collect.Api.Chaos;

public sealed record ChaosRequest(
    ChaosMode Mode,
    int SlowDelayMs = 2_000,
    int TimeoutDelayMs = 10_000,
    int RandomMinDelayMs = 250,
    int RandomMaxDelayMs = 3_000,
    int RandomFailurePercentage = 35
);