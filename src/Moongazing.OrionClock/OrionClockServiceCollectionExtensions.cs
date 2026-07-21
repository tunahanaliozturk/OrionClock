namespace Moongazing.OrionClock;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

using Moongazing.Orion.Abstractions;
using Moongazing.Orion.Abstractions.Configuration;
using Moongazing.Orion.Abstractions.Time;

/// <summary>DI registration for OrionClock.</summary>
public static class OrionClockServiceCollectionExtensions
{
    /// <summary>
    /// Register <see cref="OrionClock"/> as the app's <see cref="TimeProvider"/> <em>and</em> as
    /// the family's <see cref="IOrionClock"/>. Every registration uses <c>TryAdd</c>, so a
    /// consumer who registered their own clock first wins and calling this twice is a no-op.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">
    /// Optional configuration - most usefully to point the clock at a controllable fake
    /// <see cref="TimeProvider"/> in tests. Omit it for the production system clock.
    /// </param>
    /// <returns>The same collection for chaining.</returns>
    public static IServiceCollection AddOrionClock(
        this IServiceCollection services,
        Action<OrionClockOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddOrionOptions(configure);

        // OrionClock is the clock the whole family binds to, so register it FIRST as IOrionClock,
        // then call AddOrionAbstractions - whose own TryAdd of the default SystemOrionClock then
        // no-ops. The single OrionClock instance is shared across all three service types.
        services.TryAddSingleton(sp => new OrionClock(
            sp.GetRequiredService<IOptions<OrionClockOptions>>().Value.TimeProvider));
        services.TryAddSingleton<IOrionClock>(sp => sp.GetRequiredService<OrionClock>());
        services.TryAddSingleton<TimeProvider>(sp => sp.GetRequiredService<OrionClock>());

        services.AddOrionAbstractions();

        return services;
    }
}
