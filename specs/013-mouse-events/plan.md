# Implementation Plan: Mouse Events

**Branch**: `013-mouse-events` | **Date**: 2026-01-25 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/013-mouse-events/spec.md`

## Summary

Implement the mouse event system for Stroke, providing types for mouse event data (MouseEventType, MouseButton, MouseModifier, MouseEvent) in the Stroke.Input namespace, and the mouse handler grid (MouseHandlers, NotImplementedOrNone) in the Stroke.Layout namespace. This is a faithful port of Python Prompt Toolkit's `mouse_events.py` and `layout/mouse_handlers.py` modules.

## Technical Context

**Language/Version**: C# 13 / .NET 10
**Primary Dependencies**: Stroke.Core (Point type)
**Storage**: N/A (in-memory data structures only)
**Testing**: xUnit (no mocks per Constitution VIII)
**Target Platform**: Linux, macOS, Windows 10+ (cross-platform terminal)
**Project Type**: Single library project (Stroke)
**Performance Goals**: O(1) handler lookup at any position
**Constraints**: Per API mapping, MouseModifiers uses [Flags] enum instead of IReadOnlySet
**Scale/Scope**: Typical terminal size (up to ~500x200 cells); handler grid is sparse

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Evidence |
|-----------|--------|----------|
| I. Faithful Port | PASS | API matches Python Prompt Toolkit mouse_events.py and layout/mouse_handlers.py exactly per api-mapping.md |
| II. Immutability | PASS | MouseEvent is immutable record struct; MouseHandlers is mutable by design (renderer populates during layout) |
| III. Layered Architecture | PASS | Mouse types in Stroke.Input (depends on Core); MouseHandlers in Stroke.Layout (depends on Core, Rendering) |
| IV. Cross-Platform | PASS | Pure data types; no platform-specific code |
| V. Editing Mode Parity | N/A | Mouse events are input primitives, not editing features |
| VI. Performance | PASS | Dictionary-based sparse storage for O(1) lookup |
| VII. Full Scope | PASS | All APIs from Python source will be implemented |
| VIII. Real-World Testing | PASS | Tests will use real implementations only |
| IX. Planning Documents | PASS | api-mapping.md consulted; MouseModifiers is [Flags] enum per mapping |
| X. File Size Limits | PASS | Each type in separate file; all under 200 LOC |
| XI. Thread Safety | PASS | MouseEvent is immutable (inherently safe); MouseHandlers uses Lock synchronization for all mutable operations |

## Project Structure

### Documentation (this feature)

```text
specs/013-mouse-events/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
└── contracts/           # Phase 1 output (N/A - no external API contracts)
```

### Source Code (repository root)

```text
src/Stroke/
├── Input/
│   ├── MouseEventType.cs    # NEW: Enum for event types
│   ├── MouseButton.cs       # NEW: Enum for buttons
│   ├── MouseModifiers.cs    # NEW: [Flags] enum for modifiers
│   └── MouseEvent.cs        # NEW: Immutable record struct
└── Layout/
    ├── MouseHandlers.cs     # NEW: 2D handler grid
    └── NotImplementedOrNone.cs  # NEW: Handler return type

tests/Stroke.Tests/
└── Input/
    ├── MouseEventTypeTests.cs
    ├── MouseButtonTests.cs
    ├── MouseModifiersTests.cs
    └── MouseEventTests.cs
└── Layout/
    ├── MouseHandlersTests.cs
    └── NotImplementedOrNoneTests.cs
```

**Structure Decision**: Mouse event types go in `Stroke.Input` namespace (per api-mapping.md line 45). MouseHandlers and NotImplementedOrNone go in `Stroke.Layout` namespace (per Python source organization). The Layout directory will be created as it doesn't exist yet.

## Complexity Tracking

> No violations requiring justification.

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| (none) | - | - |

## API Mapping Compliance

Per `docs/api-mapping.md` lines 1410-1467:

| Python | Stroke | Notes |
|--------|--------|-------|
| `MouseEventType.MOUSE_UP` | `MouseEventType.MouseUp` | Enum value |
| `MouseEventType.MOUSE_DOWN` | `MouseEventType.MouseDown` | Enum value |
| `MouseEventType.MOUSE_MOVE` | `MouseEventType.MouseMove` | Enum value |
| `MouseEventType.SCROLL_UP` | `MouseEventType.ScrollUp` | Enum value |
| `MouseEventType.SCROLL_DOWN` | `MouseEventType.ScrollDown` | Enum value |
| `MouseButton.LEFT` | `MouseButton.Left` | Enum value |
| `MouseButton.MIDDLE` | `MouseButton.Middle` | Enum value |
| `MouseButton.RIGHT` | `MouseButton.Right` | Enum value |
| `MouseButton.NONE` | `MouseButton.None` | Enum value |
| `MouseButton.UNKNOWN` | `MouseButton.Unknown` | Enum value (from Python source) |
| `MouseModifier.SHIFT` | `MouseModifiers.Shift` | [Flags] enum |
| `MouseModifier.ALT` | `MouseModifiers.Alt` | [Flags] enum |
| `MouseModifier.CONTROL` | `MouseModifiers.Control` | [Flags] enum |
| `MouseEvent` | `MouseEvent` | Record struct |

**Important Deviation from Spec**: The feature spec proposed `MouseModifier` enum with `IReadOnlySet<MouseModifier>` for modifiers. However, `api-mapping.md` specifies `MouseModifiers` as a `[Flags]` enum. We follow the API mapping.

## Key Design Decisions

1. **MouseEvent as record struct**: Immutable value type matching Python's class with properties
2. **MouseModifiers as [Flags]**: Per api-mapping.md; replaces IReadOnlySet for efficiency
3. **MouseHandlers storage**: Dictionary<int, Dictionary<int, MouseHandler>> for sparse storage (matching Python's defaultdict pattern)
4. **NotImplementedOrNone**: Abstract class with singleton instances (matches Python's usage pattern)
5. **No MouseHandler delegate type alias**: Use explicit Func<MouseEvent, NotImplementedOrNone> (C# convention)
