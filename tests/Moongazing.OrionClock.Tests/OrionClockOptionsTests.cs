namespace Moongazing.OrionClock.Tests;

using Moongazing.Orion.Abstractions.Configuration;

using Xunit;

public sealed class OrionClockOptionsTests
{
    [Fact]
    public void The_default_time_provider_is_the_system_provider()
    {
        Assert.Same(TimeProvider.System, new OrionClockOptions().TimeProvider);
    }

    [Fact]
    public void The_default_options_validate()
    {
        var context = OrionOptionsValidationContext.For<OrionClockOptions>();

        new OrionClockOptions().Validate(context);

        Assert.False(context.HasFailures);
    }

    [Fact]
    public void A_null_time_provider_is_rejected()
    {
        var context = OrionOptionsValidationContext.For<OrionClockOptions>();

        new OrionClockOptions { TimeProvider = null! }.Validate(context);

        Assert.True(context.HasFailures);
        Assert.Contains("TimeProvider", context.BuildFailureMessage(), StringComparison.Ordinal);
    }
}
