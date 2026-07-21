namespace Moongazing.OrionClock;

using Moongazing.Orion.Abstractions.Configuration;

/// <summary>
/// Options for <see cref="OrionClockServiceCollectionExtensions.AddOrionClock"/>.
/// </summary>
public sealed class OrionClockOptions : OrionOptions
{
    /// <summary>
    /// The inner <see cref="TimeProvider"/> the clock reads time from. Defaults to
    /// <see cref="TimeProvider.System"/>; set a controllable fake in tests (for example the one
    /// from <c>Moongazing.OrionClock.Testing</c>).
    /// </summary>
    public TimeProvider TimeProvider { get; set; } = TimeProvider.System;

    /// <inheritdoc />
    public override void Validate(OrionOptionsValidationContext context)
    {
        base.Validate(context);
        context.Require(TimeProvider is not null, $"{nameof(TimeProvider)} must not be null.");
    }
}
