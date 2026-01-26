# Research: Mouse Events

**Feature**: 013-mouse-events
**Date**: 2026-01-25

## Research Summary

No significant unknowns or external dependencies requiring investigation. This feature ports well-defined Python types to C#.

## Decision Log

### 1. MouseModifiers Type

**Decision**: Use `[Flags] enum MouseModifiers` instead of `IReadOnlySet<MouseModifier>`

**Rationale**:
- `docs/api-mapping.md` explicitly specifies `[Flags]` enum pattern
- More efficient than allocating a set for each event
- Natural bit operations for modifier combination (Shift | Alt)
- Matches .NET conventions for modifier keys (e.g., `System.Windows.Forms.Keys`)

**Alternatives Considered**:
- `IReadOnlySet<MouseModifier>`: Rejected - contradicts api-mapping.md
- `ImmutableHashSet<MouseModifier>`: Rejected - allocation overhead per event

### 2. MouseEvent Type

**Decision**: Use `readonly record struct` instead of `sealed class`

**Rationale**:
- api-mapping.md specifies `readonly record struct`
- Value semantics match the immutable nature
- Stack allocation avoids heap pressure for frequent events
- Automatic equality, GetHashCode, ToString implementations

**Alternatives Considered**:
- `sealed class`: Rejected - heap allocation per event; contradicts api-mapping.md
- `record class`: Rejected - still heap allocated

### 3. NotImplementedOrNone Type

**Decision**: Use abstract class with private nested classes and static singleton instances

**Rationale**:
- Matches Python's pattern where handlers return `NotImplemented` or `None`
- Prevents external subclassing (private nested classes)
- Reference equality for singleton comparison (`is NotImplemented`)
- Type-safe alternative to returning `object` or using nullable types

**Alternatives Considered**:
- `bool` return type: Rejected - less expressive, doesn't match Python API
- Nullable `object?`: Rejected - loses type safety
- Enum: Rejected - doesn't match Python's object-based pattern

### 4. MouseHandlers Storage

**Decision**: Use `Dictionary<int, Dictionary<int, MouseHandler>>` for y→x→handler mapping

**Rationale**:
- Matches Python's `defaultdict(lambda: defaultdict(...))` pattern
- O(1) lookup at any position
- Sparse storage (only non-default handlers stored)
- Memory-efficient for typical terminal sizes

**Alternatives Considered**:
- 2D array: Rejected - wastes memory for sparse grids; requires pre-defined size
- Single flat dictionary with Point keys: Rejected - Point as dict key requires custom comparer or tuple

### 5. MouseHandler Delegate

**Decision**: Use `Func<MouseEvent, NotImplementedOrNone>` directly; no type alias

**Rationale**:
- C# doesn't have true type aliases (using aliases are compile-time only)
- Explicit delegate type is self-documenting
- Matches Python's `MouseHandler` type alias pattern without creating unnecessary delegate type

**Alternatives Considered**:
- Custom delegate type: Rejected - no benefit over Func<>; adds complexity
- Using alias: Not visible at runtime; C# convention is to use Func<> directly

### 6. Namespace Placement

**Decision**:
- MouseEventType, MouseButton, MouseModifiers, MouseEvent → `Stroke.Input`
- MouseHandlers, NotImplementedOrNone → `Stroke.Layout`

**Rationale**:
- api-mapping.md line 45: `prompt_toolkit.mouse_events` → `Stroke.Input`
- Python places MouseHandlers in `prompt_toolkit.layout.mouse_handlers`
- NotImplementedOrNone used primarily by layout/key_binding systems
- Follows layered architecture (Input layer, Layout layer)

**Alternatives Considered**:
- All in Stroke.Input: Rejected - MouseHandlers belongs with layout system
- All in Stroke.Layout: Rejected - contradicts api-mapping.md for mouse event types

## Dependencies Verified

| Dependency | Status | Notes |
|------------|--------|-------|
| `Stroke.Core.Point` | Available | Record struct at `src/Stroke/Core/Primitives/Point.cs` |
| `Stroke.Layout` namespace | Create | Directory doesn't exist; will be created |
| `Stroke.Input` namespace | Available | Directory exists with Keys enum files |

## No External Dependencies

This feature requires no external NuGet packages or system dependencies. All types are pure .NET implementations.
