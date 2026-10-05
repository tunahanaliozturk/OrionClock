# OrionClock

A `TimeProvider`-based clock for .NET and the Orion family: one clock that is both a `TimeProvider` and the family's `IOrionClock`, plus `Ttl` and `Deadline` values that expire and cancel deterministically under a fake clock.

![How a TTL is issued, checked and turned into a cancellation token](https://raw.githubusercontent.com/tunahanaliozturk/OrionClock/main/docs/diagrams/ttl-lifecycle.png)

## Install

    dotnet add package OrionClock

The package depends only on `Microsoft.Extensions.DependencyInjection.Abstractions`. The quick start builds its own container, so a console app or test project also needs the container package (ASP.NET Core and Generic Host apps already have it):

    dotnet add package Microsoft.Extensions.DependencyInjection

## Quick start

```csharp
using Microsoft.Extensions.DependencyInjection;
using Moongazing.Orion.Abstractions.Time;
using Moongazing.OrionClock;

var services = new ServiceCollection();
services.AddOrionClock(); // registers OrionClock as OrionClock, TimeProvider and IOrionClock

using var provider = services.BuildServiceProvider();
var clock = provider.GetRequiredService<OrionClock>();

Ttl ttl = clock.TtlFor(TimeSpan.FromMinutes(5));
bool expired = ttl.IsExpired(clock);
TimeSpan left = ttl.Remaining(clock); // never negative

Deadline deadline = clock.Deadline(TimeSpan.FromSeconds(30));
using var cts = deadline.ToCancellationTokenSource(clock);
```

## What you get

- `OrionClock` subclasses `TimeProvider`, so it works with any API that takes one (`CancellationTokenSource`, `Task.Delay`, timers). Every read delegates to an inner `TimeProvider`.
- `AddOrionClock(Action<OrionClockOptions>?)` registers `OrionClock`, `TimeProvider` and `IOrionClock` with `TryAdd`. With no earlier registrations, all three resolve to one shared instance. Each `TryAdd` is independent, so a service type you registered first keeps your registration while the other two still resolve to the `OrionClock`, and they no longer share one instance. To change the time source for all three together, set `OrionClockOptions.TimeProvider` instead of registering your own `TimeProvider`.
- `Ttl`: `IssuedAt`, `ExpiresAt`, `Duration`, `IsExpired(clock)`, `Remaining(clock)`, `ToCancellationTokenSource(clock)`.
- `Deadline`: `At`, `IsPast(clock)`, `TimeRemaining(clock)`, `ToCancellationTokenSource(clock)`.

## Options and behaviour

- `OrionClockOptions.TimeProvider` is the inner time source. Default: `TimeProvider.System`. Point it at a fake in tests.
- `TtlFor` and `Deadline` throw `ArgumentOutOfRangeException` for a negative duration.
- `Remaining` and `TimeRemaining` are clamped to `TimeSpan.Zero`.
- `ToCancellationTokenSource` on an expired TTL or a past deadline returns an already-cancelled source; otherwise the source cancels when the clock's timer fires. Dispose it when done.
- AOT- and trim-compatible (`IsAotCompatible`), checked by a NativeAOT smoke test in CI. Targets net8.0, net9.0 and net10.0.

## Related packages

- `OrionClock.Testing` - `FakeOrionClock`, a clock that only moves when a test advances it.
- `Orion.Abstractions` - the family contracts (`IOrionClock`, `OrionOptions`) this package implements.

## Links

- Documentation and full README: https://github.com/tunahanaliozturk/OrionClock
- Changelog: https://github.com/tunahanaliozturk/OrionClock/blob/main/CHANGELOG.md
- License: MIT
