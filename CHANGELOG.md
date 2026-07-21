<!-- markdownlint-disable MD024 -->

# Changelog

All notable changes to OrionClock are documented in this file. The format is based on
[Keep a Changelog](https://keepachangelog.com/en/1.0.0/) and this project adheres to
[Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.9.0] - 2026-07-21

The first release — the Orion family's Wave 1 foundation clock.

### Added

- **`OrionClock : TimeProvider, IOrionClock`** — the family's clock. It *is* a `TimeProvider`
  (drops into any BCL API that takes one) and *is* the spine's `IOrionClock`, delegating every
  time read to an inner `TimeProvider` (`TimeProvider.System` in production, a fake in tests).
- **`Ttl`** — a time-to-live value type capturing the issue and expiry instants together, with
  `IsExpired`, `Remaining` (clamped to non-negative), value equality, and
  `ToCancellationTokenSource` for TTL-driven, clock-deterministic cancellation. Created with
  `clock.TtlFor(duration)`.
- **`Deadline`** — a "must complete by" instant with `IsPast`, `TimeRemaining`, and
  deadline-driven cancellation. Created with `clock.Deadline(timeout)`.
- **`AddOrionClock(...)`** — registers `OrionClock` as `TimeProvider`, `IOrionClock`, and
  `OrionClock` (one shared instance) via `TryAdd`; binds `Orion.Abstractions`. Options
  (`OrionClockOptions`) let a test point the clock at a controllable fake `TimeProvider`.
- **`FakeOrionClock`** (in `OrionClock.Testing`) — a controllable clock built on Microsoft's
  `FakeTimeProvider`: time only moves on `Advance`/`SetUtcNow`, and advancing fires the timers
  and cancellation sources created through it, so TTL/deadline expiry is deterministic without
  real delays. Rejects moving backwards.
- Multi-targets `net8.0`/`net9.0`/`net10.0`; `IsAotCompatible`; a NativeAOT publish smoke test in
  CI that exercises every public entry point and runs the native binary with `-warnaserror`.
