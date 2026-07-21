namespace Moongazing.OrionClock.Tests;

using Moongazing.Orion.Abstractions.Time;
using Moongazing.OrionClock.Testing;

using Xunit;

public sealed class OrionClockTests
{
    [Fact]
    public void An_orion_clock_is_a_TimeProvider()
    {
        Assert.IsAssignableFrom<TimeProvider>(new OrionClock());
    }

    [Fact]
    public void An_orion_clock_is_an_IOrionClock()
    {
        Assert.IsAssignableFrom<IOrionClock>(new OrionClock());
    }

    [Fact]
    public void UtcNow_and_GetUtcNow_agree()
    {
        var clock = new FakeOrionClock(DateTimeOffset.Parse("2026-07-01T00:00:00Z", CultureInfo.InvariantCulture));

        Assert.Equal(clock.UtcNow, clock.GetUtcNow());
        Assert.Equal(DateTimeOffset.Parse("2026-07-01T00:00:00Z", CultureInfo.InvariantCulture), clock.UtcNow);
    }

    [Fact]
    public void Time_reads_delegate_to_the_inner_provider()
    {
        var start = DateTimeOffset.Parse("2026-07-01T00:00:00Z", CultureInfo.InvariantCulture);
        var clock = new FakeOrionClock(start);

        clock.Advance(TimeSpan.FromHours(3));

        Assert.Equal(start + TimeSpan.FromHours(3), clock.UtcNow);
    }

    [Fact]
    public void GetElapsedTime_measures_against_the_monotonic_timestamp()
    {
        var clock = new FakeOrionClock();
        var start = clock.GetTimestamp();

        clock.Advance(TimeSpan.FromSeconds(42));

        Assert.Equal(TimeSpan.FromSeconds(42), clock.GetElapsedTime(start));
    }

    [Fact]
    public void The_system_clock_reads_a_plausible_current_instant()
    {
        var clock = new OrionClock();

        var delta = (clock.UtcNow - DateTimeOffset.UtcNow).Duration();

        Assert.True(delta < TimeSpan.FromMinutes(1), $"system clock was {delta} off wall-clock");
    }

    [Fact]
    public void A_null_inner_provider_is_rejected()
    {
        Assert.Throws<ArgumentNullException>(() => new OrionClock(null!));
    }
}
