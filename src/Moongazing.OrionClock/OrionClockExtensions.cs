namespace Moongazing.OrionClock;

using Moongazing.Orion.Abstractions.Time;

/// <summary>
/// The schedule/TTL vocabulary layered over any <see cref="IOrionClock"/>: capture a
/// <see cref="Ttl"/> or a <see cref="Deadline"/> against the clock so expiry is checked against
/// the same time source it was issued from.
/// </summary>
public static class OrionClockExtensions
{
    /// <summary>
    /// Capture a <see cref="Ttl"/> of <paramref name="duration"/> starting now. The issue instant
    /// is read once from <paramref name="clock"/> so issue and expiry cannot drift apart.
    /// </summary>
    /// <param name="clock">The clock to issue against.</param>
    /// <param name="duration">How long the TTL lives. Must not be negative.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="duration"/> is negative.</exception>
    public static Ttl TtlFor(this IOrionClock clock, TimeSpan duration)
    {
        ArgumentNullException.ThrowIfNull(clock);
        if (duration < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(duration), duration, "A TTL duration cannot be negative.");
        }

        var now = clock.UtcNow;
        return new Ttl(now, now + duration);
    }

    /// <summary>
    /// Capture a <see cref="Deadline"/> at <paramref name="timeout"/> from now.
    /// </summary>
    /// <param name="clock">The clock to issue against.</param>
    /// <param name="timeout">How far ahead the deadline falls. Must not be negative.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="timeout"/> is negative.</exception>
    public static Deadline Deadline(this IOrionClock clock, TimeSpan timeout)
    {
        ArgumentNullException.ThrowIfNull(clock);
        if (timeout < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(timeout), timeout, "A deadline cannot be in the past.");
        }

        return new Deadline(clock.UtcNow + timeout);
    }
}
