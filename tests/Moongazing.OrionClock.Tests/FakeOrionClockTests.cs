namespace Moongazing.OrionClock.Tests;

using Moongazing.OrionClock.Testing;

using Xunit;

public sealed class FakeOrionClockTests
{
    private static readonly DateTimeOffset Start =
        DateTimeOffset.Parse("2026-07-01T00:00:00Z", CultureInfo.InvariantCulture);

    [Fact]
    public void A_fresh_fake_clock_is_frozen_at_its_start()
    {
        var clock = new FakeOrionClock(Start);

        Assert.Equal(Start, clock.UtcNow);
        Assert.Equal(Start, clock.UtcNow); // does not move on its own
    }

    [Fact]
    public void The_default_start_is_2026_01_01Z()
    {
        var clock = new FakeOrionClock();
        Assert.Equal(new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero), clock.UtcNow);
    }

    [Fact]
    public void Advance_moves_time_forward()
    {
        var clock = new FakeOrionClock(Start);
        clock.Advance(TimeSpan.FromHours(2));
        Assert.Equal(Start + TimeSpan.FromHours(2), clock.UtcNow);
    }

    [Fact]
    public void SetUtcNow_jumps_to_an_instant()
    {
        var clock = new FakeOrionClock(Start);
        var target = Start + TimeSpan.FromDays(1);

        clock.SetUtcNow(target);

        Assert.Equal(target, clock.UtcNow);
    }

    [Fact]
    public void A_fake_clock_cannot_advance_backwards()
    {
        var clock = new FakeOrionClock(Start);
        Assert.Throws<ArgumentOutOfRangeException>(() => clock.Advance(TimeSpan.FromSeconds(-1)));
    }

    [Fact]
    public void A_fake_clock_cannot_be_set_backwards()
    {
        var clock = new FakeOrionClock(Start);
        clock.Advance(TimeSpan.FromMinutes(10));
        Assert.Throws<ArgumentOutOfRangeException>(() => clock.SetUtcNow(Start));
    }

    [Fact]
    public void A_fake_clock_is_a_usable_TimeProvider_for_a_cancellation_source()
    {
        var clock = new FakeOrionClock(Start);
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30), clock);

        clock.Advance(TimeSpan.FromSeconds(29));
        Assert.False(cts.IsCancellationRequested);

        clock.Advance(TimeSpan.FromSeconds(1));
        Assert.True(cts.IsCancellationRequested);
    }
}
