namespace Moongazing.OrionClock.Tests;

using Microsoft.Extensions.DependencyInjection;

using Moongazing.Orion.Abstractions.Time;
using Moongazing.OrionClock.Testing;

using Xunit;

public sealed class OrionClockServiceCollectionExtensionsTests
{
    [Fact]
    public void AddOrionClock_registers_one_clock_as_TimeProvider_IOrionClock_and_OrionClock()
    {
        var services = new ServiceCollection();
        services.AddOrionClock();

        using var provider = services.BuildServiceProvider();
        var asClock = provider.GetRequiredService<OrionClock>();
        var asOrion = provider.GetRequiredService<IOrionClock>();
        var asTimeProvider = provider.GetRequiredService<TimeProvider>();

        // One instance, three service types.
        Assert.Same(asClock, asOrion);
        Assert.Same(asClock, asTimeProvider);
    }

    [Fact]
    public void The_default_registration_uses_the_system_clock()
    {
        var services = new ServiceCollection();
        services.AddOrionClock();

        using var provider = services.BuildServiceProvider();
        var clock = provider.GetRequiredService<IOrionClock>();

        var delta = (clock.UtcNow - DateTimeOffset.UtcNow).Duration();
        Assert.True(delta < TimeSpan.FromMinutes(1));
    }

    [Fact]
    public void The_clock_can_be_pointed_at_a_fake_TimeProvider_via_options()
    {
        var fake = new FakeOrionClock(DateTimeOffset.Parse("2026-07-01T00:00:00Z", CultureInfo.InvariantCulture));
        var services = new ServiceCollection();
        services.AddOrionClock(o => o.TimeProvider = fake);

        using var provider = services.BuildServiceProvider();
        var clock = provider.GetRequiredService<IOrionClock>();

        Assert.Equal(fake.UtcNow, clock.UtcNow);
    }

    [Fact]
    public void A_consumer_registered_clock_wins_over_AddOrionClock()
    {
        var mine = new OrionClock();
        var services = new ServiceCollection();
        services.AddSingleton<IOrionClock>(mine);
        services.AddOrionClock();

        using var provider = services.BuildServiceProvider();

        Assert.Same(mine, provider.GetRequiredService<IOrionClock>());
    }

    [Fact]
    public void AddOrionClock_is_idempotent()
    {
        var services = new ServiceCollection();
        services.AddOrionClock();
        services.AddOrionClock();

        using var provider = services.BuildServiceProvider();

        Assert.Single(provider.GetServices<OrionClock>());
    }

    [Fact]
    public void AddOrionClock_returns_the_same_collection()
    {
        var services = new ServiceCollection();
        Assert.Same(services, services.AddOrionClock());
    }
}
