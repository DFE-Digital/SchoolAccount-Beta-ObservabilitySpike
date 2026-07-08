using System.Diagnostics;

namespace Collect.Api.Chaos;

public static class ChaosDiagnostics
{
    public const string ActivitySourceName = "Collect.Api.Chaos";

    public static readonly ActivitySource ActivitySource = new(ActivitySourceName);
}