namespace Moongazing.OrionClock.Testing;

using Microsoft.Extensions.Time.Testing;

/// <summary>
/// A controllable <see cref="OrionClock"/> for tests. Time is frozen at construction and only
/// moves when you call <see cref="Advance(TimeSpan)"/> or <see cref="SetUtcNow(DateTimeOffset)"/>,
/// so a test can drive TTL expiry, deadlines, and timer-driven cancellation to the exact instant
/// without real delays or flakiness.
/// </summary>
/// <remarks>
/// Built on Microsoft's <see cref="FakeTimeProvider"/>: because timers (and the
/// <see cref="CancellationTokenSource"/>s built from <c>Ttl</c>/<c>Deadline</c>) are created
/// through this provider, advancing it fires them deterministically.
/// </remarks>
public sealed class FakeOrionClock : OrionClock
{
    private readonly FakeTimeProvider fake;

    /// <summary>Create a fake clock frozen at <paramref name="start"/> (default: 2026-01-01Z).</summary>
    /// <param name="start">The initial UTC instant.</param>
    public FakeOrionClock(DateTimeOffset? start = null)
        : this(new FakeTimeProvider(start ?? new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero)))
    {
    }

    private FakeOrionClock(FakeTimeProvider fake)
        : base(fake) => this.fake = fake;

    /// <summary>
    /// Move time forward by <paramref name="delta"/>, firing any timers whose due time is reached.
    /// </summary>
    /// <param name="delta">How far to advance. Must not be negative.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="delta"/> is negative.</exception>
    public void Advance(TimeSpan delta)
    {
        if (delta < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(delta), delta, "A fake clock cannot advance backwards.");
        }

        fake.Advance(delta);
    }

    /// <summary>
    /// Set the current UTC instant. Must not move time backwards (a real clock never does).
    /// </summary>
    /// <param name="value">The new UTC instant. Must be at or after the current instant.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> precedes the current instant.</exception>
    public void SetUtcNow(DateTimeOffset value)
    {
        if (value < fake.GetUtcNow())
        {
            throw new ArgumentOutOfRangeException(nameof(value), value, "A fake clock cannot be set backwards.");
        }

        fake.SetUtcNow(value);
    }
}
