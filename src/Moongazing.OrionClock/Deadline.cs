namespace Moongazing.OrionClock;

using Moongazing.Orion.Abstractions.Time;

/// <summary>
/// A point in time by which an operation must complete, as a first-class value. Pair it with
/// <see cref="ToCancellationTokenSource(OrionClock)"/> to drive deadline-based cancellation
/// through the clock (deterministic under a fake clock).
/// </summary>
/// <remarks>
/// Create one from a clock with
/// <see cref="OrionClockExtensions.Deadline(IOrionClock, TimeSpan)"/>.
/// </remarks>
public readonly struct Deadline : IEquatable<Deadline>
{
    /// <summary>Create a deadline at an explicit instant.</summary>
    /// <param name="at">The instant the deadline falls, in UTC.</param>
    public Deadline(DateTimeOffset at) => At = at;

    /// <summary>The instant the deadline falls, in UTC.</summary>
    public DateTimeOffset At { get; }

    /// <summary>True once <paramref name="clock"/> has reached or passed the deadline.</summary>
    /// <param name="clock">The clock to read the current instant from.</param>
    public bool IsPast(IOrionClock clock)
    {
        ArgumentNullException.ThrowIfNull(clock);
        return clock.UtcNow >= At;
    }

    /// <summary>
    /// The time left before the deadline against <paramref name="clock"/>, clamped to
    /// non-negative.
    /// </summary>
    /// <param name="clock">The clock to read the current instant from.</param>
    public TimeSpan TimeRemaining(IOrionClock clock)
    {
        ArgumentNullException.ThrowIfNull(clock);
        var left = At - clock.UtcNow;
        return left > TimeSpan.Zero ? left : TimeSpan.Zero;
    }

    /// <summary>
    /// A <see cref="CancellationTokenSource"/> that cancels when this deadline falls, driven by
    /// <paramref name="clock"/>'s timer. An already-passed deadline produces an already-cancelled
    /// source. Dispose it when done.
    /// </summary>
    /// <param name="clock">The clock whose timer drives the cancellation.</param>
    public CancellationTokenSource ToCancellationTokenSource(OrionClock clock)
    {
        ArgumentNullException.ThrowIfNull(clock);
        var remaining = TimeRemaining(clock);
        if (remaining <= TimeSpan.Zero)
        {
            var cancelled = new CancellationTokenSource();
            cancelled.Cancel();
            return cancelled;
        }

        return new CancellationTokenSource(remaining, clock);
    }

    /// <inheritdoc />
    public bool Equals(Deadline other) => At == other.At;

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is Deadline other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => At.GetHashCode();

    /// <summary>Value equality.</summary>
    public static bool operator ==(Deadline left, Deadline right) => left.Equals(right);

    /// <summary>Value inequality.</summary>
    public static bool operator !=(Deadline left, Deadline right) => !left.Equals(right);

    /// <summary>Renders as <c>deadline {At:O}</c>.</summary>
    public override string ToString() => $"deadline {At:O}";
}
