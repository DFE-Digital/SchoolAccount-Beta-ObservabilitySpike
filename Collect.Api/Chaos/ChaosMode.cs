namespace Collect.Api.Chaos;

public enum ChaosMode
{
    None,
    Slow,
    Error,
    Timeout,
    RandomLatency,
    RandomFailure,
    SqlDown,
    RubyDown
}