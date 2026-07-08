namespace SpikeB.Observability.Api.Models.Traffic;

public sealed record TrafficRequest(
    string Scenario = "normal"
);