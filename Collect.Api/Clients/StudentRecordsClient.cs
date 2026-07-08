namespace Collect.Api.Clients;

public sealed class StudentRecordsClient(HttpClient httpClient)
{
    public Task<string> NormalAsync(CancellationToken cancellationToken) =>
        GetAsync("/api/student-records/normal", cancellationToken);

    public Task<string> SlowAsync(CancellationToken cancellationToken) =>
        GetAsync("/api/student-records/slow", cancellationToken);

    public Task<string> ErrorAsync(CancellationToken cancellationToken) =>
        GetAsync("/api/student-records/error", cancellationToken);

    public Task<string> SqlDownAsync(CancellationToken cancellationToken) =>
        GetAsync("/api/student-records/sql-down", cancellationToken);
    
    public Task<string> TimeoutAsync(CancellationToken cancellationToken) =>
        GetAsync("/api/student-records/timeout", cancellationToken);

    private async Task<string> GetAsync(string path, CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync(path, cancellationToken);

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        response.EnsureSuccessStatusCode();

        return content;
    }
}