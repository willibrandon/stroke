# Feature Specification: Clipboard System

**Feature Branch**: `004-clipboard-system`
**Created**: 2026-01-23
**Status**: Draft
**Input**: User description: "Implement the clipboard abstraction and implementations for storing and retrieving text with selection type information."

## Reference

### Python Source Mapping

| Python File | Python Type | C# Type | C# File |
|-------------|-------------|---------|---------|
| `clipboard/base.py` | `ClipboardData` | `ClipboardData` | `Core/ClipboardData.cs` (existing) |
| `clipboard/base.py` | `Clipboard` (ABC) | `IClipboard` | `Core/IClipboard.cs` |
| `clipboard/base.py` | `DummyClipboard` | `DummyClipboard` | `Core/DummyClipboard.cs` |
| `clipboard/base.py` | `DynamicClipboard` | `DynamicClipboard` | `Core/DynamicClipboard.cs` |
| `clipboard/in_memory.py` | `InMemoryClipboard` | `InMemoryClipboard` | `Core/InMemoryClipboard.cs` |

### Naming Convention Transformations

| Python Convention | C# Convention | Example |
|-------------------|---------------|---------|
| `snake_case` methods | `PascalCase` methods | `set_data` → `SetData` |
| `snake_case` properties | `PascalCase` properties | `max_size` → `MaxSize` |
| `SCREAMING_CASE` enum values | `PascalCase` enum values | `CHARACTERS` → `Characters` |
| ABC (Abstract Base Class) | Interface | `Clipboard` → `IClipboard` |

### Deviation from Python

| Deviation | Rationale |
|-----------|-----------|
| Python ABC → C# Interface | `docs/api-mapping.md` specifies IClipboard; interfaces provide cleaner composition in C# |
| Python `deque` → C# `LinkedList<T>` | Equivalent O(1) operations for front/back insertion and removal |
| Python `assert` → C# `ArgumentOutOfRangeException` | .NET convention for parameter validation |
| No thread safety → Thread-safe | .NET applications commonly have multi-threaded contexts; defensive programming for clipboard access from background threads |

## API Contracts

### ClipboardData (Existing)

```csharp
namespace Stroke.Core;

/// <summary>
/// Immutable value object representing text content with selection type information.
/// </summary>
public sealed class ClipboardData
{
    /// <summary>
    /// Creates clipboard data with the specified text and selection type.
    /// </summary>
    /// <param name="text">The clipboard text content. Defaults to empty string.</param>
    /// <param name="type">The selection type. Defaults to Characters.</param>
    /// <exception cref="ArgumentNullException">Thrown when text is null.</exception>
    public ClipboardData(string text = "", SelectionType type = SelectionType.Characters);

    /// <summary>Gets the clipboard text content.</summary>
    public string Text { get; }  // get-only, immutable

    /// <summary>Gets the selection type.</summary>
    public SelectionType Type { get; }  // get-only, immutable
}
```

**Notes**:
- Class is `sealed` (not designed for inheritance)
- Properties are get-only (immutable)
- Constructor throws `ArgumentNullException` if `text` is null
- This class already exists in `src/Stroke/Core/ClipboardData.cs`

### IClipboard Interface (New)

```csharp
namespace Stroke.Core;

/// <summary>
/// Interface defining clipboard operations for storing and retrieving text with selection type.
/// Implementations should be thread-safe for use from multiple threads.
/// </summary>
public interface IClipboard
{
    /// <summary>
    /// Stores data on the clipboard.
    /// </summary>
    /// <param name="data">The clipboard data to store.</param>
    /// <exception cref="ArgumentNullException">Thrown when data is null.</exception>
    void SetData(ClipboardData data);

    /// <summary>
    /// Retrieves the current clipboard data.
    /// </summary>
    /// <returns>The current clipboard data, or empty ClipboardData if clipboard is empty.</returns>
    ClipboardData GetData();

    /// <summary>
    /// Shortcut for storing plain text with Characters selection type.
    /// Default implementation calls SetData(new ClipboardData(text)).
    /// </summary>
    /// <param name="text">The text to store.</param>
    /// <exception cref="ArgumentNullException">Thrown when text is null.</exception>
    void SetText(string text) => SetData(new ClipboardData(text));

    /// <summary>
    /// Rotates the kill ring for Emacs-style yank-pop.
    /// Default implementation is a no-op.
    /// </summary>
    void Rotate() { }
}
```

**Notes**:
- Uses C# 8+ default interface methods for `SetText` and `Rotate`
- `SetText` default calls `SetData` with `ClipboardData(text)` (Characters type)
- `Rotate` default is no-op (empty body)
- Implementations may override defaults
- **Thread safety contract**: All implementations provided by Stroke.Core are thread-safe. Custom implementations should also be thread-safe for consistent behavior.

### DummyClipboard (New)

```csharp
namespace Stroke.Core;

/// <summary>
/// Clipboard implementation that stores nothing and returns empty data.
/// This class is thread-safe (stateless).
/// </summary>
public sealed class DummyClipboard : IClipboard
{
    /// <summary>Creates a new dummy clipboard instance.</summary>
    public DummyClipboard();

    /// <summary>No-op. Data is discarded.</summary>
    public void SetData(ClipboardData data);  // No-op

    /// <summary>No-op. Text is discarded.</summary>
    public void SetText(string text);  // No-op (overrides default)

    /// <summary>No-op.</summary>
    public void Rotate();  // No-op (overrides default)

    /// <summary>Returns empty clipboard data.</summary>
    /// <returns>New ClipboardData with empty string and Characters type.</returns>
    public ClipboardData GetData();  // Returns new ClipboardData()
}
```

**Notes**:
- Class is `sealed`
- All methods are public
- Stateless implementation
- `SetText` overrides default to be explicit no-op (matches Python)
- `GetData` returns `new ClipboardData()` (empty string, Characters)
- **Thread-safe**: Stateless, no synchronization needed

### InMemoryClipboard (New)

```csharp
namespace Stroke.Core;

/// <summary>
/// Default clipboard implementation with kill ring for Emacs-style yank-pop.
/// This class is thread-safe.
/// </summary>
public sealed class InMemoryClipboard : IClipboard
{
    /// <summary>
    /// Creates an in-memory clipboard with optional initial data and configurable ring size.
    /// </summary>
    /// <param name="data">Optional initial clipboard data. Defaults to null (empty ring).</param>
    /// <param name="maxSize">Maximum kill ring capacity. Defaults to 60. Must be >= 1.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when maxSize is less than 1.</exception>
    public InMemoryClipboard(ClipboardData? data = null, int maxSize = 60);

    /// <summary>Gets the maximum kill ring capacity.</summary>
    public int MaxSize { get; }  // get-only, immutable after construction

    /// <summary>
    /// Stores data at the front of the kill ring. Trims oldest if ring exceeds MaxSize.
    /// Thread-safe.
    /// </summary>
    public void SetData(ClipboardData data);

    /// <summary>
    /// Retrieves the current (front) item from the kill ring.
    /// Thread-safe.
    /// </summary>
    /// <returns>Front item, or empty ClipboardData if ring is empty.</returns>
    public ClipboardData GetData();

    /// <summary>
    /// Moves the front item to the back of the ring.
    /// No-op if ring is empty. Thread-safe.
    /// </summary>
    public void Rotate();
}
```

**Notes**:
- Class is `sealed`
- Internal storage: `LinkedList<ClipboardData>` (not exposed)
- `MaxSize` is get-only property (immutable after construction)
- Default `maxSize` is 60 (matches Python)
- If `data` is provided to constructor, it calls `SetData(data)` internally
- Does NOT override `SetText` (uses interface default)
- **Thread-safe**: All public methods use internal lock synchronization
- Uses `private readonly Lock _lock = new();` (`System.Threading.Lock`, .NET 9+) for synchronization
- Lock acquired via `using (_lock.EnterScope())` pattern for automatic release
- Individual operations are atomic; compound operations (e.g., GetData + Rotate) require external synchronization if atomicity is needed

### DynamicClipboard (New)

```csharp
namespace Stroke.Core;

/// <summary>
/// Clipboard wrapper that delegates to a dynamically selected clipboard.
/// Thread safety depends on the underlying clipboard implementation.
/// </summary>
public sealed class DynamicClipboard : IClipboard
{
    /// <summary>
    /// Creates a dynamic clipboard with the specified clipboard provider.
    /// </summary>
    /// <param name="getClipboard">
    /// Function that returns the clipboard to use, or null for dummy behavior.
    /// The delegate itself should be thread-safe if called from multiple threads.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when getClipboard is null.</exception>
    public DynamicClipboard(Func<IClipboard?> getClipboard);

    /// <summary>Delegates SetData to the current clipboard.</summary>
    public void SetData(ClipboardData data);

    /// <summary>Delegates SetText to the current clipboard.</summary>
    public void SetText(string text);  // Overrides default to delegate

    /// <summary>Delegates Rotate to the current clipboard.</summary>
    public void Rotate();  // Overrides default to delegate

    /// <summary>Delegates GetData to the current clipboard.</summary>
    public ClipboardData GetData();
}
```

**Notes**:
- Class is `sealed`
- Constructor throws `ArgumentNullException` if `getClipboard` is null
- When `getClipboard()` returns null, falls back to `new DummyClipboard()` (matches Python exactly)
- Overrides `SetText` and `Rotate` to properly delegate (not use interface defaults)
- If delegate throws exception, exception propagates to caller (no special handling)
- **Thread safety**: DynamicClipboard itself is thread-safe (delegate reference is immutable). Thread safety of operations depends on:
  1. The delegate being thread-safe (caller responsibility)
  2. The underlying clipboard being thread-safe (InMemoryClipboard is; DummyClipboard is stateless)

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Store and Retrieve Clipboard Data (Priority: P1)

As a text editor component, I need to store text with its selection type (characters, lines, or block) on the clipboard so that paste operations can respect the original selection mode.

**Why this priority**: This is the core functionality of the clipboard system. Without the ability to store and retrieve data with selection type information, the clipboard cannot fulfill its primary purpose.

**Independent Test**: Can be fully tested by storing clipboard data with various selection types and verifying the retrieved data matches exactly what was stored.

**Acceptance Scenarios**:

1. **Given** an empty clipboard, **When** text "hello" with SelectionType.Characters is stored, **Then** retrieving the clipboard returns text "hello" with SelectionType.Characters
2. **Given** an empty clipboard, **When** text "line1\nline2" with SelectionType.Lines is stored, **Then** retrieving the clipboard returns the same text with SelectionType.Lines
3. **Given** an empty clipboard, **When** text with SelectionType.Block is stored, **Then** retrieving the clipboard returns the same text with SelectionType.Block
4. **Given** a clipboard with existing data, **When** new data is stored, **Then** the new data replaces the previous data as the current clipboard content
5. **Given** an InMemoryClipboard constructed with initial data "init" (Lines), **When** GetData is called immediately, **Then** returns ClipboardData with text "init" and SelectionType.Lines
6. **Given** a DummyClipboard, **When** SetData is called then GetData is called, **Then** GetData returns empty ClipboardData (data was discarded)

---

### User Story 2 - Emacs Kill Ring Support (Priority: P2)

As an Emacs-mode user, I need the clipboard to maintain a kill ring so I can cycle through previously killed text using yank-pop (M-y).

**Why this priority**: Kill ring is essential for Emacs mode editing but not required for basic clipboard functionality. Users expect Emacs-style yank-pop to work correctly.

**Independent Test**: Can be fully tested by storing multiple items to the clipboard and verifying rotation cycles through them in the expected order.

**Acceptance Scenarios**:

1. **Given** a clipboard with items [A, B, C] (A most recent), **When** rotate is called, **Then** the current item becomes B, and the ring order is [B, C, A]
2. **Given** a clipboard with items [A, B, C], **When** rotate is called three times, **Then** the current item returns to A
3. **Given** an empty clipboard, **When** rotate is called, **Then** nothing happens (no error, GetData still returns empty)
4. **Given** a clipboard at max capacity (maxSize=3 with items [C, B, A]), **When** a new item D is added, **Then** the oldest item A is removed and the ring becomes [D, C, B]
5. **Given** a clipboard with maxSize=1 and item [A], **When** SetData(B) is called, **Then** the ring becomes [B] (A is removed)
6. **Given** a clipboard with single item [A], **When** rotate is called, **Then** GetData still returns A (rotation of single item is no-op)

---

### User Story 3 - Dynamic Clipboard Selection (Priority: P3)

As an application developer, I need to dynamically select which clipboard implementation to use at runtime so that different contexts can use different clipboard backends (e.g., system clipboard vs. in-memory clipboard).

**Why this priority**: This provides flexibility for advanced use cases but is not required for basic text editing functionality.

**Independent Test**: Can be fully tested by creating a dynamic clipboard with a function that returns different implementations and verifying operations delegate correctly.

**Acceptance Scenarios**:

1. **Given** a dynamic clipboard configured to return an in-memory clipboard, **When** data is stored and retrieved, **Then** the in-memory clipboard is used for both operations
2. **Given** a dynamic clipboard whose function returns null, **When** data is retrieved, **Then** empty clipboard data is returned (dummy clipboard fallback)
3. **Given** a dynamic clipboard whose backing clipboard changes between operations, **When** store is called then the backing clipboard changes then retrieve is called, **Then** each operation uses the clipboard active at that moment
4. **Given** a dynamic clipboard with null getClipboard parameter, **When** constructor is called, **Then** ArgumentNullException is thrown
5. **Given** a dynamic clipboard whose delegate throws InvalidOperationException, **When** SetData is called, **Then** InvalidOperationException propagates to caller

---

### User Story 4 - Convenience Text Storage (Priority: P3)

As a developer using the clipboard API, I need a shortcut method to store plain text without explicitly creating ClipboardData so that common use cases are simpler.

**Why this priority**: This is a convenience feature that improves API ergonomics but does not add new capabilities.

**Independent Test**: Can be fully tested by using the SetText shortcut and verifying the result is equivalent to explicitly creating ClipboardData with Characters selection type.

**Acceptance Scenarios**:

1. **Given** any clipboard implementation, **When** SetText("hello") is called, **Then** GetData returns ClipboardData with text "hello" and SelectionType.Characters
2. **Given** any clipboard implementation, **When** SetText("") is called, **Then** GetData returns ClipboardData with empty text and SelectionType.Characters
3. **Given** any clipboard implementation, **When** SetText(null) is called, **Then** ArgumentNullException is thrown

---

### User Story 5 - Thread-Safe Clipboard Access (Priority: P2)

As a developer building a multi-threaded application, I need the clipboard to be thread-safe so that background threads can safely access the clipboard without data corruption.

**Why this priority**: Thread safety is essential for .NET applications that may access clipboard from multiple threads (e.g., async operations, background workers).

**Independent Test**: Can be fully tested by spawning multiple threads that concurrently read and write to the clipboard and verifying no exceptions or data corruption occurs.

**Acceptance Scenarios**:

1. **Given** an InMemoryClipboard, **When** 10 threads concurrently call SetData with different values, **Then** no exceptions are thrown and the ring contains valid data
2. **Given** an InMemoryClipboard with data, **When** 10 threads concurrently call GetData, **Then** all threads receive valid ClipboardData (no nulls, no partial reads)
3. **Given** an InMemoryClipboard with multiple items, **When** threads concurrently call SetData, GetData, and Rotate, **Then** no exceptions are thrown and all operations complete
4. **Given** a DummyClipboard, **When** multiple threads concurrently call all methods, **Then** no exceptions are thrown (stateless, inherently thread-safe)
5. **Given** a DynamicClipboard wrapping an InMemoryClipboard, **When** multiple threads concurrently access it, **Then** operations are thread-safe (delegated to thread-safe InMemoryClipboard)

---

### Edge Cases

| Edge Case | Behavior |
|-----------|----------|
| Empty string stored | Clipboard stores and returns empty string with specified SelectionType |
| Null text to SetText | Throws `ArgumentNullException` (per .NET conventions) |
| Null data to SetData | Throws `ArgumentNullException` (per .NET conventions) |
| Rotation with one item | No-op; single item remains current |
| Rotation with empty ring | No-op; no error thrown |
| Kill ring maxSize = 1 | Ring holds exactly one item; new items replace previous |
| GetData on empty clipboard | Returns `new ClipboardData()` (empty string, Characters) |
| InMemoryClipboard maxSize < 1 | Constructor throws `ArgumentOutOfRangeException` |
| DynamicClipboard null delegate | Constructor throws `ArgumentNullException` |
| DynamicClipboard delegate returns null | Falls back to `new DummyClipboard()` behavior |
| DynamicClipboard delegate throws exception | Exception propagates to caller (no special handling) |
| Null text to ClipboardData constructor | Throws `ArgumentNullException` |
| Concurrent SetData calls | All complete without exception; ring state is consistent |
| Concurrent GetData calls | All return valid ClipboardData; no partial reads |
| Concurrent mixed operations | SetData, GetData, Rotate all complete without exception |
| Concurrent access during rotation | Lock ensures atomicity; no data corruption |

## Requirements *(mandatory)*

### Functional Requirements

#### Core Types

- **FR-001**: System MUST provide a ClipboardData class that stores text content and selection type together as an immutable unit with get-only properties
- **FR-002**: System MUST provide an IClipboard interface with the following method signatures:
  - `void SetData(ClipboardData data)` - stores clipboard data
  - `ClipboardData GetData()` - retrieves current data
  - `void SetText(string text)` - default implementation calls `SetData(new ClipboardData(text))`
  - `void Rotate()` - default implementation is no-op (empty body)
- **FR-003**: ClipboardData MUST default to empty string and SelectionType.Characters when constructed with no arguments
- **FR-004**: ClipboardData constructor MUST throw ArgumentNullException when text parameter is null

#### DummyClipboard

- **FR-005**: System MUST provide a DummyClipboard implementation that:
  - `SetData`: No-op (data discarded)
  - `SetText`: No-op (overrides interface default, text discarded)
  - `Rotate`: No-op (overrides interface default)
  - `GetData`: Returns `new ClipboardData()` (empty string, Characters type)

#### InMemoryClipboard

- **FR-006**: System MUST provide an InMemoryClipboard implementation that stores clipboard history as a kill ring using LinkedList<ClipboardData>
- **FR-007**: InMemoryClipboard MUST support configurable maximum kill ring size via constructor parameter `maxSize` (default: 60 items)
- **FR-008**: InMemoryClipboard MUST expose MaxSize as a get-only property
- **FR-009**: InMemoryClipboard constructor MUST throw ArgumentOutOfRangeException when maxSize < 1
- **FR-010**: InMemoryClipboard MUST add new items to the front of the ring (most recent first)
- **FR-011**: InMemoryClipboard MUST remove the oldest item (back of ring) when the ring exceeds MaxSize after adding
- **FR-012**: InMemoryClipboard Rotate MUST move the current (front) item to the back of the ring
- **FR-013**: InMemoryClipboard Rotate MUST be a no-op when ring is empty
- **FR-014**: InMemoryClipboard MUST optionally accept initial ClipboardData in constructor; if provided, calls SetData internally

#### DynamicClipboard

- **FR-015**: System MUST provide a DynamicClipboard implementation that delegates to a clipboard returned by `Func<IClipboard?>`
- **FR-016**: DynamicClipboard constructor MUST throw ArgumentNullException when getClipboard parameter is null
- **FR-017**: DynamicClipboard MUST fall back to `new DummyClipboard()` when the delegate function returns null
- **FR-018**: DynamicClipboard MUST override SetText and Rotate to properly delegate (not use interface defaults)
- **FR-019**: DynamicClipboard MUST propagate exceptions thrown by the delegate function to the caller

#### Common Behaviors

- **FR-020**: SetText MUST be equivalent to SetData with ClipboardData containing the text and SelectionType.Characters
- **FR-021**: GetData on an empty clipboard MUST return ClipboardData with empty string and SelectionType.Characters
- **FR-022**: SetData MUST throw ArgumentNullException when data parameter is null

#### Thread Safety Requirements

- **FR-023**: InMemoryClipboard MUST be thread-safe using internal lock synchronization
- **FR-024**: InMemoryClipboard MUST use `private readonly Lock _lock = new();` (`System.Threading.Lock`, .NET 9+) for synchronization
- **FR-025**: InMemoryClipboard MUST acquire lock via `using (_lock.EnterScope())` pattern for automatic release
- **FR-026**: All InMemoryClipboard public methods (SetData, GetData, SetText, Rotate) MUST acquire the lock before accessing the ring
- **FR-027**: DummyClipboard is inherently thread-safe (stateless, no synchronization needed)
- **FR-028**: DynamicClipboard thread safety depends on the delegate and underlying clipboard being thread-safe
- **FR-029**: ClipboardData is inherently thread-safe (immutable, no synchronization needed)

#### Structural Requirements

- **FR-030**: All clipboard types MUST be placed in the `Stroke.Core` namespace
- **FR-031**: All implementation classes (ClipboardData, DummyClipboard, InMemoryClipboard, DynamicClipboard) MUST be sealed
- **FR-032**: All public types and members MUST have XML documentation comments (triple-slash `///`)
- **FR-033**: File naming MUST follow class names: IClipboard.cs, DummyClipboard.cs, InMemoryClipboard.cs, DynamicClipboard.cs

### Key Entities

| Entity | Type | Description |
|--------|------|-------------|
| `ClipboardData` | `sealed class` | Immutable value object containing Text (string) and Type (SelectionType). Represents a single clipboard entry. |
| `IClipboard` | `interface` | Defines clipboard operations: SetData, GetData, SetText (default impl), Rotate (default impl). |
| `DummyClipboard` | `sealed class : IClipboard` | Stateless no-op implementation. All writes discarded, reads return empty. |
| `InMemoryClipboard` | `sealed class : IClipboard` | Default clipboard with kill ring (LinkedList<ClipboardData>). Supports Emacs-style yank-pop. |
| `DynamicClipboard` | `sealed class : IClipboard` | Wrapper that delegates to a clipboard returned by Func<IClipboard?>. |
| Kill Ring | Internal concept | FIFO buffer in InMemoryClipboard. Front = most recent/current, Back = oldest. |

## Non-Functional Requirements

### Performance

- **NFR-001**: All clipboard operations (SetData, GetData, SetText, Rotate) MUST complete in O(1) constant time
- **NFR-002**: Kill ring trimming on SetData MUST be O(1) (LinkedList.RemoveLast)
- **NFR-003**: Memory usage MUST be bounded by MaxSize * sizeof(ClipboardData)

### Quality

- **NFR-004**: Unit test coverage MUST achieve 80% or higher for all clipboard types
- **NFR-005**: All public APIs MUST have XML documentation comments
- **NFR-006**: No single source file MUST exceed 1,000 lines of code (per Constitution X)

### Compatibility

- **NFR-007**: MUST run on .NET 10+
- **NFR-008**: MUST be cross-platform (Linux, macOS, Windows 10+)
- **NFR-009**: All Stroke.Core clipboard implementations MUST be thread-safe
- **NFR-010**: No persistence or serialization (in-memory only)

### Thread Safety

- **NFR-011**: Individual clipboard operations MUST be atomic (thread-safe)
- **NFR-012**: Compound operations (e.g., GetData followed by Rotate for yank-pop) are NOT atomic; callers requiring atomicity MUST use external synchronization
- **NFR-013**: Lock contention SHOULD be minimized; lock scope MUST be as narrow as possible

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: All clipboard operations (SetData, GetData, SetText, Rotate) complete in constant time O(1)
- **SC-002**: Unit test coverage achieves 80% or higher for all clipboard classes
- **SC-003**: All acceptance scenarios pass automated testing
- **SC-004**: Kill ring correctly maintains ordering through 100+ consecutive set/rotate operations
- **SC-005**: API matches Python Prompt Toolkit semantics exactly (method behavior, default values, edge cases), except for documented thread safety deviation
- **SC-006**: Concurrent stress test with 10+ threads performing 1000+ operations completes without exceptions or data corruption

## Assumptions

### Dependencies

| Dependency | Version/Source | Notes |
|------------|---------------|-------|
| SelectionType enum | Stroke.Core.Selection (Feature 003) | Characters, Lines, Block values |
| .NET | 10+ | Target framework |
| C# | 13 | Required for default interface methods (C# 8+) |
| System.Threading.Lock | .NET 9+ (BCL) | Efficient lock type for InMemoryClipboard thread safety |

### Design Decisions (Resolved Ambiguities)

| Decision | Resolution | Rationale |
|----------|------------|-----------|
| Abstract class vs Interface | Interface (`IClipboard`) | Per `docs/api-mapping.md`; cleaner composition |
| Namespace | `Stroke.Clipboard` | Per `docs/api-mapping.md` mapping of `prompt_toolkit.clipboard` |
| Null text behavior | `ArgumentNullException` | .NET convention for null arguments |
| ClipboardData type | `sealed class` (not record) | Matches existing implementation; intentional immutability |
| Kill ring data structure | `LinkedList<ClipboardData>` | O(1) front/back operations; equivalent to Python deque |
| Thread safety | Required (deviation from Python) | .NET applications commonly multi-threaded; defensive programming |

### Constraints

- **Thread safety**: REQUIRED for all mutable implementations. Deviation from Python Prompt Toolkit for .NET multi-threaded contexts.
- **Atomicity**: Individual operations are atomic; compound operations require external synchronization
- **Persistence**: None. In-memory only. No serialization or file storage
- **Platform**: Cross-platform (Linux, macOS, Windows 10+) as Stroke.Core has no platform dependencies
