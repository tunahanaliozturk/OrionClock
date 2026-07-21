namespace Moongazing.OrionClock.Tests;

using Moongazing.OrionClock.Testing;

using Xunit;

public sealed class DeadlineTests
{
    private static readonly DateTimeOffset Start =
        DateTimeOffset.Parse("2026-07-01T00:00:00Z", CultureInfo.InvariantCulture);

    [Fact]
    public void Deadline_captures_the_instant_it_falls()
    {
        var clock = new FakeOrionClock(Start);

        var deadline = clock.Deadline(TimeSpan.FromSeconds(30));

        Assert.Equal(Start + TimeSpan.FromSeconds(30), deadline.At);
    }

    [Fact]
    public void IsPast_flips_when_the_clock_reaches_the_deadline()
    {
        var clock = new FakeOrionClock(Start);
        var deadline = clock.Deadline(TimeSpan.FromSeconds(30));

        clock.Advance(TimeSpan.FromSeconds(29));
        Assert.False(deadline.IsPast(clock));

        clock.Advance(TimeSpan.FromSeconds(1)); // exactly at the deadline
        Assert.True(deadline.IsPast(clock));
    }

    [Fact]
    public void TimeRemaining_counts_down_and_is_clamped_to_zero()
    {
        var clock = new FakeOrionClock(Start);
        var deadline = clock.Deadline(TimeSpan.FromSeconds(30));

        Assert.Equal(TimeSpan.FromSeconds(30), deadline.TimeRemaining(clock));

        clock.Advance(TimeSpan.FromSeconds(45)); // past
        Assert.Equal(TimeSpan.Zero, deadline.TimeRemaining(clock));
    }

    [Fact]
    public void A_negative_timeout_is_rejected()
    {
        var clock = new FakeOrionClock(Start);
        Assert.Throws<ArgumentOutOfRangeException>(() => clock.Deadline(TimeSpan.FromSeconds(-1)));
    }

    [Fact]
    public void ToCancellationTokenSource_cancels_at_the_deadline_under_the_fake_clock()
    {
        var clock = new FakeOrionClock(Start);
        var deadline = clock.Deadline(TimeSpan.FromSeconds(30));
        using var cts = deadline.ToCancellationTokenSource(clock);

        clock.Advance(TimeSpan.FromSeconds(29));
        Assert.False(cts.IsCancellationRequested);

        clock.Advance(TimeSpan.FromSeconds(1));
        Assert.True(cts.IsCancellationRequested);
    }

    [Fact]
    public void Deadlines_compare_by_value()
    {
        var a = new Deadline(Start);
        var b = new Deadline(Start);
        var c = new Deadline(Start + TimeSpan.FromSeconds(1));

        Assert.Equal(a, b);
        Assert.True(a == b);
        Assert.NotEqual(a, c);
        Assert.True(a != c);
    }
}
