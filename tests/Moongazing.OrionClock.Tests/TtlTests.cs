namespace Moongazing.OrionClock.Tests;

using Moongazing.OrionClock.Testing;

using Xunit;

public sealed class TtlTests
{
    private static readonly DateTimeOffset Start =
        DateTimeOffset.Parse("2026-07-01T00:00:00Z", CultureInfo.InvariantCulture);

    [Fact]
    public void The_golden_test_a_five_minute_ttl_expires_after_six_minutes_not_four()
    {
        // The package's headline guarantee, from the plan: a 5-minute TTL issued under a fake
        // clock is expired after advancing 6 minutes and NOT after 4.
        var clock = new FakeOrionClock(Start);
        var ttl = clock.TtlFor(TimeSpan.FromMinutes(5));

        clock.Advance(TimeSpan.FromMinutes(4));
        Assert.False(ttl.IsExpired(clock));

        clock.Advance(TimeSpan.FromMinutes(2)); // now +6m total
        Assert.True(ttl.IsExpired(clock));
    }

    [Fact]
    public void TtlFor_captures_the_issue_and_expiry_instants()
    {
        var clock = new FakeOrionClock(Start);

        var ttl = clock.TtlFor(TimeSpan.FromMinutes(5));

        Assert.Equal(Start, ttl.IssuedAt);
        Assert.Equal(Start + TimeSpan.FromMinutes(5), ttl.ExpiresAt);
        Assert.Equal(TimeSpan.FromMinutes(5), ttl.Duration);
    }

    [Fact]
    public void Remaining_counts_down_and_is_clamped_to_zero_once_expired()
    {
        var clock = new FakeOrionClock(Start);
        var ttl = clock.TtlFor(TimeSpan.FromMinutes(5));

        Assert.Equal(TimeSpan.FromMinutes(5), ttl.Remaining(clock));

        clock.Advance(TimeSpan.FromMinutes(2));
        Assert.Equal(TimeSpan.FromMinutes(3), ttl.Remaining(clock));

        clock.Advance(TimeSpan.FromMinutes(10)); // well past expiry
        Assert.Equal(TimeSpan.Zero, ttl.Remaining(clock)); // never negative
        Assert.True(ttl.IsExpired(clock));
    }

    [Fact]
    public void Exactly_at_expiry_the_ttl_is_expired()
    {
        var clock = new FakeOrionClock(Start);
        var ttl = clock.TtlFor(TimeSpan.FromMinutes(5));

        clock.Advance(TimeSpan.FromMinutes(5)); // exactly at ExpiresAt

        Assert.True(ttl.IsExpired(clock));
        Assert.Equal(TimeSpan.Zero, ttl.Remaining(clock));
    }

    [Fact]
    public void A_ttl_cannot_expire_before_it_was_issued()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Ttl(Start, Start - TimeSpan.FromSeconds(1)));
    }

    [Fact]
    public void A_negative_ttl_duration_is_rejected()
    {
        var clock = new FakeOrionClock(Start);
        Assert.Throws<ArgumentOutOfRangeException>(() => clock.TtlFor(TimeSpan.FromSeconds(-1)));
    }

    [Fact]
    public void A_zero_duration_ttl_is_expired_immediately()
    {
        var clock = new FakeOrionClock(Start);
        var ttl = clock.TtlFor(TimeSpan.Zero);

        Assert.True(ttl.IsExpired(clock));
    }

    [Fact]
    public void Ttls_compare_by_value()
    {
        var a = new Ttl(Start, Start + TimeSpan.FromMinutes(5));
        var b = new Ttl(Start, Start + TimeSpan.FromMinutes(5));
        var c = new Ttl(Start, Start + TimeSpan.FromMinutes(6));

        Assert.Equal(a, b);
        Assert.True(a == b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
        Assert.NotEqual(a, c);
        Assert.True(a != c);
    }

    [Fact]
    public void ToCancellationTokenSource_cancels_when_the_ttl_expires_under_the_fake_clock()
    {
        var clock = new FakeOrionClock(Start);
        var ttl = clock.TtlFor(TimeSpan.FromMinutes(5));
        using var cts = ttl.ToCancellationTokenSource(clock);

        clock.Advance(TimeSpan.FromMinutes(4));
        Assert.False(cts.IsCancellationRequested);

        clock.Advance(TimeSpan.FromMinutes(2)); // +6m, past expiry
        Assert.True(cts.IsCancellationRequested);
    }

    [Fact]
    public void ToCancellationTokenSource_of_an_already_expired_ttl_is_already_cancelled()
    {
        var clock = new FakeOrionClock(Start);
        var ttl = clock.TtlFor(TimeSpan.FromMinutes(5));
        clock.Advance(TimeSpan.FromMinutes(10));

        using var cts = ttl.ToCancellationTokenSource(clock);

        Assert.True(cts.IsCancellationRequested);
    }

    [Fact]
    public void IsExpired_and_Remaining_reject_a_null_clock()
    {
        var ttl = new Ttl(Start, Start + TimeSpan.FromMinutes(5));
        Assert.Throws<ArgumentNullException>(() => ttl.IsExpired(null!));
        Assert.Throws<ArgumentNullException>(() => ttl.Remaining(null!));
    }
}
