namespace Moongazing.OrionClock;

using Moongazing.Orion.Abstractions.Time;

/// <summary>
/// A time-to-live as a first-class value: the instant it was issued and the instant it expires,
/// captured together. Beats passing a raw <see cref="DateTimeOffset"/> around and remembering to
/// compare it in UTC at every call site.
/// </summary>
/// <remarks>
/// Create one from a clock with <see cref="OrionClockExtensions.TtlFor(IOrionClock, TimeSpan)"/>
/// so issue-time is captured against the same clock the expiry is later checked against - which
/// makes expiry deterministic under a fake clock.
/// </remarks>
public readonly struct Ttl : IEquatable<Ttl>
{
    /// <summary>Create a TTL from an explicit issue and expiry instant.</summary>
    /// <param name="issuedAt">When the TTL began.</param>
    /// <param name="expiresAt">When it expires. Must not be before <paramref name="issuedAt"/>.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="expiresAt"/> precedes <paramref name="issuedAt"/>.</exception>
    public Ttl(DateTimeOffset issuedAt, DateTimeOffset expiresAt)
    {
        if (expiresAt < issuedAt)
        {
            throw new ArgumentOutOfRangeException(
                nameof(expiresAt), expiresAt, "A TTL cannot expire before it was issued.");
        }

        IssuedAt = issuedAt;
        ExpiresAt = expiresAt;
    }

    /// <summary>When the TTL began, in UTC.</summary>
    public DateTimeOffset IssuedAt { get; }

    /// <summary>When the TTL expires, in UTC.</summary>
    public DateTimeOffset ExpiresAt { get; }

    /// <summary>The total lifetime (<see cref="ExpiresAt"/> minus <see cref="IssuedAt"/>).</summary>
    public TimeSpan Duration => ExpiresAt - IssuedAt;

    /// <summary>True once <paramref name="clock"/> has reached or passed <see cref="ExpiresAt"/>.</summary>
    /// <param name="clock">The clock to read the current instant from.</param>
    public bool IsExpired(IOrionClock clock)
    {
        ArgumentNullException.ThrowIfNull(clock);
        return clock.UtcNow >= ExpiresAt;
    }

    /// <summary>
    /// The time left before expiry against <paramref name="clock"/>, clamped to non-negative
    /// (an expired TTL has <see cref="TimeSpan.Zero"/> remaining, never a negative value).
    /// </summary>
    /// <param name="clock">The clock to read the current instant from.</param>
    public TimeSpan Remaining(IOrionClock clock)
    {
        ArgumentNullException.ThrowIfNull(clock);
        var left = ExpiresAt - clock.UtcNow;
        return left > TimeSpan.Zero ? left : TimeSpan.Zero;
    }

    /// <summary>
    /// A <see cref="CancellationTokenSource"/> that cancels when this TTL expires, driven by
    /// <paramref name="clock"/>'s timer - so it fires deterministically under a fake clock.
    /// Already-expired TTLs produce an already-cancelled source. Dispose it when done.
    /// </summary>
    /// <param name="clock">The clock whose timer drives the cancellation.</param>
    public CancellationTokenSource ToCancellationTokenSource(OrionClock clock)
    {
        ArgumentNullException.ThrowIfNull(clock);
        var remaining = Remaining(clock);
        if (remaining <= TimeSpan.Zero)
        {
            var cancelled = new CancellationTokenSource();
            cancelled.Cancel();
            return cancelled;
        }

        return new CancellationTokenSource(remaining, clock);
    }

    /// <inheritdoc />
    public bool Equals(Ttl other) => IssuedAt == other.IssuedAt && ExpiresAt == other.ExpiresAt;

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is Ttl other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(IssuedAt, ExpiresAt);

    /// <summary>Value equality.</summary>
    public static bool operator ==(Ttl left, Ttl right) => left.Equals(right);

    /// <summary>Value inequality.</summary>
    public static bool operator !=(Ttl left, Ttl right) => !left.Equals(right);

    /// <summary>Renders as <c>{Duration} TTL expiring {ExpiresAt:O}</c>.</summary>
    public override string ToString() => $"{Duration} TTL expiring {ExpiresAt:O}";
}
