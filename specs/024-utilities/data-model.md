# Data Model: Utilities

**Feature**: 024-utilities
**Date**: 2026-01-28

## Entities

### Event<TSender>

**Purpose**: Generic pub/sub event class allowing multiple handlers to subscribe and receive notifications when the event fires.

**Fields**:
| Field | Type | Description |
|-------|------|-------------|
| `_sender` | `TSender` | The object that owns this event (passed to handlers) |
| `_handlers` | `List<Action<TSender>>` | Ordered list of event handlers |

**Relationships**:
- Owned by any class that exposes events (e.g., Buffer, Application)
- Handlers are external delegates provided by subscribers

**Validation Rules**:
- Sender provided at construction (cannot be null for reference types)
- Handlers cannot be null (throw ArgumentNullException)
- Removing non-existent handler is silently ignored

**Thread Safety**: NOT thread-safe, following standard .NET event semantics.

**State Transitions**:
- Initial: Empty handler list
- After `+=`: Handler added to end of list
- After `-=`: First matching handler removed (if exists)
- After `Fire()`: All handlers invoked in order (iterates over snapshot)

**Fire() Behavior**:
- Zero handlers: Completes successfully (no-op)
- Handler throws: Exception propagates, remaining handlers skipped
- Handler modifies list: Changes take effect on next Fire() (snapshot iteration)

---

### StringWidthCache (Internal)

**Purpose**: LRU cache for string width calculations to avoid redundant computation.

**Thread Safety**: Thread-safe via `System.Threading.Lock` (internal cache accessed from rendering code).

**Fields**:
| Field | Type | Description |
|-------|------|-------------|
| `_lock` | `Lock` | Thread synchronization |
| `_cache` | `Dictionary<string, int>` | String → width mappings |
| `_longStrings` | `Queue<string>` | Tracks strings > 64 chars for eviction |

**Constants**:
| Constant | Value | Description |
|----------|-------|-------------|
| `LongStringMinLength` | 64 | Threshold for "long" string classification (>64, not >=64) |
| `MaxLongStrings` | 16 | Maximum long strings before eviction |

**Classification**:
- Strings with length ≤ 64: Short strings, cached indefinitely
- Strings with length > 64: Long strings, subject to FIFO eviction

**Validation Rules**:
- Empty string returns 0
- Control characters (wcwidth returns -1) treated as width 0

**State Transitions**:
- Cache miss: Compute width, store in cache
- Long string added when > MaxLongStrings: Evict oldest long string

---

### DummyContext

**Purpose**: No-op disposable for optional context manager scenarios.

**Fields**: None (singleton pattern)

**Singleton Instance**: `DummyContext.Instance`

---

## Type Aliases

### AnyFloat

**Purpose**: Union type supporting both concrete float values and lazy (callable) values.

**Thread Safety**: Thread-safe (immutable struct); callable invocation is caller's responsibility.

**Implementation**: Struct with implicit conversions

```csharp
public readonly struct AnyFloat : IEquatable<AnyFloat>
{
    private readonly double? _value;
    private readonly Func<double>? _getter;

    public double Value => _value ?? _getter?.Invoke() ?? 0.0;
    public bool HasValue { get; } // false for default(AnyFloat)

    public static implicit operator AnyFloat(double value) => new(value);
    public static implicit operator AnyFloat(Func<double> getter) => new(getter);
}
```

**Equality Semantics**:
- Two concrete values: Compare double values
- Two callables: Reference equality (same delegate instance)
- Concrete vs callable: Not equal
- Default AnyFloat: `HasValue` is false, `Value` is 0.0

---

## Static Utility Classes

### UnicodeWidth

**Purpose**: Calculate display width of characters and strings in terminal cells.

**Static State**:
- `_cache`: `StringWidthCache` instance (singleton, thread-safe)

**Key Methods**:
| Method | Return | Description |
|--------|--------|-------------|
| `GetWidth(char)` | `int` | Single character width (0, 1, or 2) |
| `GetWidth(string)` | `int` | Total width of string |

---

### PlatformUtils

**Purpose**: Detect runtime platform and environment characteristics.

**Static Properties** (all read-only):
| Property | Type | Description |
|----------|------|-------------|
| `IsWindows` | `bool` | True if running on Windows |
| `IsMacOS` | `bool` | True if running on macOS |
| `IsLinux` | `bool` | True if running on Linux |
| `SuspendToBackgroundSupported` | `bool` | True on Unix (SIGTSTP available) |
| `IsConEmuAnsi` | `bool` | True if ConEmuANSI=ON on Windows |
| `InMainThread` | `bool` | True if current thread is main thread |
| `BellEnabled` | `bool` | True if PROMPT_TOOLKIT_BELL is "true" or "1" |

**Static Methods**:
| Method | Return | Description |
|--------|--------|-------------|
| `GetTermEnvironmentVariable()` | `string` | Returns TERM env var (empty if not set) |
| `IsDumbTerminal(string? term = null)` | `bool` | True if term is "dumb" or "unknown" |

---

### ConversionUtils

**Purpose**: Convert lazy values (callables) to concrete values.

**Static Methods**:
| Method | Return | Description |
|--------|--------|-------------|
| `ToStr(string?)` | `string` | Returns value or "" if null |
| `ToStr(Func<string>?)` | `string` | Invokes callable, recursively handles nested |
| `ToInt(int)` | `int` | Returns value |
| `ToInt(Func<int>?)` | `int` | Invokes callable, returns 0 if null |
| `ToFloat(double)` | `double` | Returns value |
| `ToFloat(Func<double>?)` | `double` | Invokes callable, returns 0.0 if null |

---

### CollectionUtils

**Purpose**: Collection manipulation utilities.

**Thread Safety**: Thread-safe (returns new iterator per call; no shared mutable state).

**Static Methods**:
| Method | Return | Description |
|--------|--------|-------------|
| `TakeUsingWeights<T>(IReadOnlyList<T>, IReadOnlyList<int>)` | `IEnumerable<T>` | Infinite generator yielding items proportionally |

**Validation**:
- Throws `ArgumentNullException` if items or weights is null
- Throws `ArgumentException` if items and weights have different lengths
- Throws `ArgumentException` if no items have positive weights

**Weight Handling**:
- Weights ≤ 0 (including negative): Filtered out, item not yielded
- If all weights ≤ 0: Throws `ArgumentException`

---

## Thread Safety Summary

| Type | Thread Safety | Notes |
|------|---------------|-------|
| Event<TSender> | NOT thread-safe | Follows .NET event conventions |
| StringWidthCache | Thread-safe | Uses `System.Threading.Lock` |
| UnicodeWidth | Thread-safe | Delegates to thread-safe cache |
| PlatformUtils | Thread-safe | Stateless; atomic env var reads |
| ConversionUtils | Thread-safe | Stateless static methods |
| CollectionUtils | Thread-safe | Returns new iterator per call |
| AnyFloat | Thread-safe | Immutable struct |
| DummyContext | Thread-safe | Stateless singleton |

