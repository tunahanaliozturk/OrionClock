namespace Moongazing.OrionClock;

using Moongazing.Orion.Abstractions.Time;

/// <summary>
/// The Orion family's clock. It <em>is</em> a <see cref="TimeProvider"/> (so it drops into any
/// BCL API that accepts one - <see cref="CancellationTokenSource"/>, <c>Task.Delay</c>, timers)
/// and <em>is</em> the family's <see cref="IOrionClock"/>, and it adds the schedule/TTL
/// vocabulary every backend keeps re-deriving (see <see cref="OrionClockExtensions"/>).
/// </summary>
/// <remarks>
/// <para>
/// It is a thin, allocation-light wrapper: every time read delegates to an inner
/// <see cref="TimeProvider"/> - <see cref="TimeProvider.System"/> in production, a controllable
/// fake in tests - so faking time advances the whole suite at once. Subclassing
/// <see cref="TimeProvider"/> rather than replacing it is deliberate: anything that accepts a
/// <see cref="TimeProvider"/> accepts an <see cref="OrionClock"/>.
/// </para>
/// </remarks>
public class OrionClock : TimeProvider, IOrionClock
{
    private readonly TimeProvider inner;

    /// <summary>
    /// Create a clock over <see cref="TimeProvider.System"/>. Prefer resolving the clock from DI
    /// (<c>AddOrionClock</c>); construct one directly only where DI is not available.
    /// </summary>
    public OrionClock()
        : this(System)
    {
    }

    /// <summary>Create a clock over an inner <see cref="TimeProvider"/>.</summary>
    /// <param name="inner">The provider all time reads delegate to. In tests, a controllable fake.</param>
    /// <exception cref="ArgumentNullException"><paramref name="inner"/> is null.</exception>
    public OrionClock(TimeProvider inner)
    {
        ArgumentNullException.ThrowIfNull(inner);
        this.inner = inner;
    }

    /// <inheritdoc cref="IOrionClock.UtcNow" />
    public DateTimeOffset UtcNow => inner.GetUtcNow();

    /// <inheritdoc />
    public override DateTimeOffset GetUtcNow() => inner.GetUtcNow();

    /// <inheritdoc />
    public override long GetTimestamp() => inner.GetTimestamp();

    /// <inheritdoc />
    public override TimeZoneInfo LocalTimeZone => inner.LocalTimeZone;

    /// <inheritdoc />
    public override long TimestampFrequency => inner.TimestampFrequency;

    /// <inheritdoc />
    public override ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period) =>
        inner.CreateTimer(callback, state, dueTime, period);
}
