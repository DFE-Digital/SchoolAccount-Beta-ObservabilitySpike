using System.Diagnostics;

namespace SpikeB.Observability.Api;

public static class SimulationDiagnostics
{
    public const string ActivitySourceName = "SpikeB.Observability.Api.Simulation";

    public static readonly ActivitySource ActivitySource = new(ActivitySourceName);
}