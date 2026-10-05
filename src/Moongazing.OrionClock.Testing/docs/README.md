# OrionClock.Testing

A controllable `FakeOrionClock` for tests: time only moves when you call `Advance` or `SetUtcNow`, so TTL expiry, deadlines and timer-driven cancellation fire at the exact instant with no real delay.

![A test advances FakeOrionClock and the TTL's timer fires without waiting](https://raw.githubusercontent.com/tunahanaliozturk/OrionClock/main/docs/diagrams/fake-clock-test.png)

## Install

    dotnet add package OrionClock.Testing

Reference it from test projects only. It builds on `OrionClock`.

## Quick start

```csharp
using Moongazing.OrionClock;
using Moongazing.OrionClock.Testing;

var clock = new FakeOrionClock(); // frozen at 2026-01-01T00:00:00Z
var ttl = clock.TtlFor(TimeSpan.FromMinutes(5));
using var cts = ttl.ToCancellationTokenSource(clock);

clock.Advance(TimeSpan.FromMinutes(4));
Assert.False(ttl.IsExpired(clock));
Assert.False(cts.IsCancellationRequested);

clock.Advance(TimeSpan.FromMinutes(2)); // now +6 min
Assert.True(ttl.IsExpired(clock));
Assert.True(cts.IsCancellationRequested);
```

Use it for the whole service graph:

```csharp
var fake = new FakeOrionClock();
services.AddOrionClock(o => o.TimeProvider = fake);
```

## Behaviour

- `new FakeOrionClock(DateTimeOffset? start = null)` starts frozen at `start`, or 2026-01-01Z when omitted.
- `Advance(TimeSpan)` moves time forward and fires every timer that falls due.
- `SetUtcNow(DateTimeOffset)` jumps to an instant at or after the current one.
- Moving backwards (a negative `Advance` or an earlier `SetUtcNow`) throws `ArgumentOutOfRangeException`.
- Built on `FakeTimeProvider` from `Microsoft.Extensions.TimeProvider.Testing`. `FakeOrionClock` is an `OrionClock`, so it is also a `TimeProvider` and an `IOrionClock`.

## Related packages

- `OrionClock` - the clock, `Ttl`, `Deadline` and `AddOrionClock`.

## Links

- Documentation and full README: https://github.com/tunahanaliozturk/OrionClock
- Changelog: https://github.com/tunahanaliozturk/OrionClock/blob/main/CHANGELOG.md
- License: MIT
