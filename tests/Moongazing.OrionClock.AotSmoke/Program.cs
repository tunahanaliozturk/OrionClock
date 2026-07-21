// NativeAOT smoke test. Publishing this with PublishAot=true must produce zero trim/AOT warnings,
// and running it must exit 0 - that pair is OrionClock's AOT exit criterion. Assertions are
// runtime checks, not a test framework, so the point is to prove these paths survive trimming.
using Microsoft.Extensions.DependencyInjection;
using Moongazing.Orion.Abstractions.Time;
using Moongazing.OrionClock;

var services = new ServiceCollection();
services.AddOrionClock();

using var provider = services.BuildServiceProvider();

// The clock resolves as all three service types, and they are the same instance.
var clock = provider.GetRequiredService<OrionClock>();
var asOrion = provider.GetRequiredService<IOrionClock>();
var asProvider = provider.GetRequiredService<TimeProvider>();
Check(ReferenceEquals(clock, asOrion) && ReferenceEquals(clock, asProvider), "clock is not one shared instance");

// Time reads work. UtcNow and GetUtcNow() read the live system clock back-to-back, so they
// advance monotonically rather than being bit-identical.
var viaProperty = clock.UtcNow;
var viaMethod = clock.GetUtcNow();
Check(viaMethod >= viaProperty && viaMethod - viaProperty < TimeSpan.FromSeconds(1), "clock surfaces disagree");
var start = clock.GetTimestamp();
Check(clock.GetElapsedTime(start) >= TimeSpan.Zero, "elapsed time went backwards");

// Ttl vocabulary.
var ttl = clock.TtlFor(TimeSpan.FromMinutes(5));
Check(ttl.Duration == TimeSpan.FromMinutes(5), "ttl duration wrong");
Check(!ttl.IsExpired(clock), "fresh ttl reports expired");
Check(ttl.Remaining(clock) > TimeSpan.Zero, "fresh ttl has no time remaining");

// Deadline vocabulary + deadline-driven cancellation.
var deadline = clock.Deadline(TimeSpan.FromSeconds(30));
Check(!deadline.IsPast(clock), "fresh deadline reports past");
using var cts = deadline.ToCancellationTokenSource(clock);
Check(!cts.IsCancellationRequested, "deadline cancelled early");

Console.WriteLine("OrionClock AOT smoke test passed.");
return 0;

static void Check(bool condition, string message)
{
    if (!condition)
    {
        Console.Error.WriteLine($"AOT smoke test failed: {message}");
        Environment.Exit(1);
    }
}
