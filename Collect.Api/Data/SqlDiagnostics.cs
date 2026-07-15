using System.Diagnostics;

namespace Collect.Api.Data;

public static class SqlDiagnostics
{
    public const string ActivitySourceName = "Collect.Api.Sql";

    public static readonly ActivitySource ActivitySource =
        new(ActivitySourceName);
}