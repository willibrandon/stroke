# Research: Utilities

**Feature**: 024-utilities
**Date**: 2026-01-28

## Research Tasks

### R1: Event<TSender> Implementation Pattern

**Question**: What is the best pattern for implementing C# event-like functionality with += and -= operators?

**Decision**: Use operator overloading with explicit Add/Remove methods.

**Rationale**:
- C# operators += and -= on custom types require static `operator+` and `operator-` definitions
- Return type must be `Event<TSender>` to allow chaining
- Python's `__iadd__` returns `self`; C# equivalent returns the same instance
- Handler storage uses `List<Action<TSender>>` for order preservation and O(1) append

**Alternatives Considered**:
1. Use standard C# `event` keyword - Rejected because it doesn't match Python API (no Fire() method, no sender pattern)
2. Use `EventHandler<T>` delegates - Rejected because Python uses single-parameter handlers (sender only)

**Thread Safety Decision**: NOT thread-safe, following standard .NET event semantics. The built-in C# `event` keyword is also not thread-safe, and .NET developers expect this behavior when they see += / -= syntax. Callers requiring cross-thread access should add external synchronization.

**Implementation Pattern**:
```csharp
public sealed class Event<TSender>
{
    private readonly TSender _sender;
    private readonly List<Action<TSender>> _handlers = [];

    public static Event<TSender> operator +(Event<TSender> e, Action<TSender> handler)
    {
        e.AddHandler(handler);
        return e;
    }

    public static Event<TSender> operator -(Event<TSender> e, Action<TSender> handler)
    {
        e.RemoveHandler(handler);
        return e;
    }
}
```

---

### R2: Unicode Width Caching Strategy

**Question**: How should string width caching be implemented for optimal performance?

**Decision**: Use dual-layer caching: character-level via Wcwidth package + string-level LRU cache.

**Rationale**:
- Python PTK uses `_CharSizesCache` with deque for long string rotation
- Wcwidth NuGet package handles character-level lookups efficiently
- String caching prevents redundant iteration for repeated width calculations
- LRU eviction for strings > 64 chars prevents unbounded memory growth

**Implementation Details**:
- Short strings (≤64 chars): Keep in cache indefinitely (common substrings)
- Long strings (>64 chars): Track in queue, evict oldest when > 16 long strings
- Thread-safe via `System.Threading.Lock` per Constitution XI
- Convert wcwidth -1 (control chars) to 0 for safe arithmetic

**Reference**: `docs/dependencies-plan.md` section 1.4 defines caching patterns.

---

### R3: Platform Detection Approach

**Question**: How to detect Windows/macOS/Linux and platform capabilities?

**Decision**: Use `System.Runtime.InteropServices.RuntimeInformation` for OS detection; environment variables for terminal detection.

**Rationale**:
- `RuntimeInformation.IsOSPlatform()` is the modern .NET approach
- Python uses `sys.platform` which maps to these checks
- Environment variables (`TERM`, `ConEmuANSI`, `PROMPT_TOOLKIT_BELL`) read at access time per spec

**API Mapping** (from api-mapping.md):
| Python | C# |
|--------|-----|
| `is_windows()` | `PlatformUtils.IsWindows` (property) |
| `suspend_to_background_supported()` | `PlatformUtils.SuspendToBackgroundSupported` (property) |
| `is_conemu_ansi()` | `PlatformUtils.IsConEmuAnsi` (property) |
| `in_main_thread()` | `PlatformUtils.InMainThread` (property) |
| `is_dumb_terminal()` | `PlatformUtils.IsDumbTerminal` (method with optional TERM param) |

**SIGTSTP Detection**: Unix-only; detect via `RuntimeInformation.IsOSPlatform(OSPlatform.Linux)` or `OSPlatform.OSX`.

---

### R4: TakeUsingWeights Algorithm

**Question**: How does the proportional distribution algorithm work?

**Decision**: Port Python algorithm exactly—iterative filling with weight-proportional yielding.

**Rationale**:
- Python code is well-documented in `utils.py` lines 236-287
- Algorithm: Track "already_taken" per item, yield items that are below their proportional threshold
- Must handle zero-weight items by filtering them out
- Must raise error if no positive weights or mismatched lengths

**Algorithm Steps**:
1. Filter items with weight > 0
2. Validate: at least one item with positive weight
3. Track `already_taken[i]` for each item
4. In each iteration, yield items where `already_taken[i] < i * weight[i] / max_weight`

**C# Considerations**:
- Return `IEnumerable<T>` with `yield return` for lazy evaluation
- Use `double` for division to match Python's float division

---

### R5: Lazy Value Conversion (ToStr/ToInt/ToFloat)

**Question**: How to handle callable vs concrete values?

**Decision**: Use `Func<T>` overloads and recursive unwrapping.

**Rationale**:
- Python allows nested callables: `to_str(lambda: lambda: "hello")`
- C# equivalent: `ConversionUtils.ToStr(() => () => "hello")`
- Recursion handles arbitrary nesting
- Null handling: ToStr returns "", ToInt returns 0, ToFloat returns 0.0

**Type Patterns**:
```csharp
public static string ToStr(string? value) => value ?? "";
public static string ToStr(Func<string>? getter) => getter == null ? "" : ToStr(getter());
public static string ToStr(Func<Func<string>>? getter) => getter == null ? "" : ToStr(getter());
// ... similar for ToInt, ToFloat
```

**AnyFloat Type**: Create `AnyFloat` struct with implicit conversions from `double` and `Func<double>` per api-mapping.md.

---

### R6: DummyContext Implementation

**Question**: Best pattern for no-op disposable?

**Decision**: Implement `IDisposable` with no-op Dispose; optionally singleton.

**Rationale**:
- Python's `DummyContext` is a context manager (`__enter__`, `__exit__`)
- C# equivalent is `IDisposable` with `using` statement
- Can be singleton since it's stateless
- Spec mentions singleton is sufficient

**Implementation**:
```csharp
public sealed class DummyContext : IDisposable
{
    public static readonly DummyContext Instance = new();
    private DummyContext() { }
    public void Dispose() { } // No-op
}
```

---

## NEEDS CLARIFICATION Status

All items resolved:
- ✅ Event thread safety: Spec explicitly states not required
- ✅ Cache thresholds: Spec defines 64 chars, 16 long strings
- ✅ Error handling: Spec defines exception propagation for Event handlers
- ✅ API naming: api-mapping.md provides complete mappings

## Dependencies

| Dependency | Version | Purpose | License |
|------------|---------|---------|---------|
| Wcwidth | 4.0.1 | Unicode character width | MIT ✅ |
| System.Runtime.InteropServices | (BCL) | Platform detection | MIT ✅ |

No new dependencies required beyond what's already in the project.

