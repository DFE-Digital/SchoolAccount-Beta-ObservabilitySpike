using System.Diagnostics;

namespace Collect.Api.Chaos;

public sealed class ChaosService : IChaosService
{
    private readonly ILogger<ChaosService> _logger;

    public ChaosService(ILogger<ChaosService> logger)
    {
        _logger = logger;
    }

    public async Task ExecuteAsync(ChaosRequest request, CancellationToken cancellationToken)
    {
        using var activity = ChaosDiagnostics.ActivitySource.StartActivity(
            "collect.chaos.execute",
            ActivityKind.Internal);

        activity?.SetTag("chaos.mode", request.Mode.ToString());
        activity?.SetTag("chaos.slow_delay_ms", request.SlowDelayMs);
        activity?.SetTag("chaos.timeout_delay_ms", request.TimeoutDelayMs);
        activity?.SetTag("chaos.random_min_delay_ms", request.RandomMinDelayMs);
        activity?.SetTag("chaos.random_max_delay_ms", request.RandomMaxDelayMs);
        activity?.SetTag("chaos.random_failure_percentage", request.RandomFailurePercentage);

        activity?.AddEvent(new ActivityEvent("chaos.started"));

        try
        {
            switch (request.Mode)
            {
                case ChaosMode.None:
                    activity?.AddEvent(new ActivityEvent("chaos.none"));
                    return;

                case ChaosMode.Slow:
                    activity?.AddEvent(new ActivityEvent("chaos.slow.started"));
                    await Task.Delay(request.SlowDelayMs, cancellationToken);
                    activity?.AddEvent(new ActivityEvent("chaos.slow.completed"));
                    return;

                case ChaosMode.Error:
                    activity?.AddEvent(new ActivityEvent("chaos.error.injected"));
                    throw new InvalidOperationException("Chaos error injected by Collect.Api.");

                case ChaosMode.Timeout:
                    activity?.AddEvent(new ActivityEvent("chaos.timeout.started"));
                    await Task.Delay(request.TimeoutDelayMs, cancellationToken);
                    activity?.AddEvent(new ActivityEvent("chaos.timeout.completed"));
                    return;

                case ChaosMode.RandomLatency:
                {
                    var delay = Random.Shared.Next(
                        request.RandomMinDelayMs,
                        request.RandomMaxDelayMs + 1);

                    activity?.SetTag("chaos.random_latency_delay_ms", delay);
                    activity?.AddEvent(new ActivityEvent("chaos.random_latency.started"));

                    await Task.Delay(delay, cancellationToken);

                    activity?.AddEvent(new ActivityEvent("chaos.random_latency.completed"));
                    return;
                }

                case ChaosMode.RandomFailure:
                {
                    var roll = Random.Shared.Next(1, 101);
                    var failed = roll <= request.RandomFailurePercentage;

                    activity?.SetTag("chaos.random_failure_roll", roll);
                    activity?.SetTag("chaos.random_failure_triggered", failed);

                    if (failed)
                    {
                        activity?.AddEvent(new ActivityEvent("chaos.random_failure.injected"));
                        throw new InvalidOperationException(
                            $"Random chaos failure injected. Roll: {roll}.");
                    }

                    activity?.AddEvent(new ActivityEvent("chaos.random_failure.skipped"));
                    return;
                }

                case ChaosMode.SqlDown:
                    activity?.AddEvent(new ActivityEvent("chaos.sql_down.placeholder"));
                    throw new InvalidOperationException(
                        "SQL down placeholder injected by Collect.Api.");

                case ChaosMode.RubyDown:
                    activity?.AddEvent(new ActivityEvent("chaos.ruby_down.placeholder"));
                    throw new InvalidOperationException(
                        "Ruby service down placeholder injected by Collect.Api.");

                default:
                    throw new ArgumentOutOfRangeException(nameof(request.Mode), request.Mode, null);
            }
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.SetTag("exception.type", ex.GetType().FullName);
            activity?.SetTag("exception.message", ex.Message);

            _logger.LogError(ex, "Chaos mode {ChaosMode} failed", request.Mode);

            throw;
        }
        finally
        {
            activity?.AddEvent(new ActivityEvent("chaos.completed"));
        }
    }
}