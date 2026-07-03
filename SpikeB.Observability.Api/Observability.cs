using System.Diagnostics;

namespace SpikeB.Observability.Api;

public static class Observability
{
    public const string ActivitySourceName = "school-account-api";

    public static readonly ActivitySource ActivitySource =
        new(ActivitySourceName);
}