# Implementation Plan: Key Bindings System

**Branch**: `022-key-bindings-system` | **Date**: 2026-01-27 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/022-key-bindings-system/spec.md`

## Summary

Implement a complete key bindings registry system for the Stroke terminal UI framework. The system associates key sequences with handlers, supporting filters for conditional activation, eager matching for immediate execution, global bindings, and registry composition. This is a faithful port of Python Prompt Toolkit's `prompt_toolkit.key_binding.key_bindings` module.

## Technical Context

**Language/Version**: C# 13 / .NET 10+
**Primary Dependencies**: Stroke.Core (SimpleCache, IFilter, FilterOrBool), Stroke.Input (Keys enum)
**Storage**: N/A (in-memory only)
**Testing**: xUnit (no mocks, no FluentAssertions per Constitution VIII)
**Target Platform**: Linux, macOS, Windows 10+ (cross-platform terminal)
**Project Type**: Single library (Stroke.KeyBinding namespace within Stroke project)
**Performance Goals**: <1ms binding lookup for 1,000+ bindings, 95% cache hit rate
**Constraints**: Thread-safe per Constitution XI, 1,000 LOC file limit per Constitution X
**Scale/Scope**: Up to 10,000 registered bindings without performance degradation

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Faithful Port (100% API Fidelity) | ✅ Pass | All classes from `key_bindings.py` will be ported 1:1 |
| II. Immutability by Default | ✅ Pass | Binding class is immutable; KeyBindings is mutable by design (matches Python) |
| III. Layered Architecture | ✅ Pass | Stroke.KeyBinding depends on Core and Input only |
| IV. Cross-Platform Terminal Compatibility | ✅ Pass | No platform-specific code in key bindings |
| V. Complete Editing Mode Parity | ✅ Pass | Foundation for Emacs/Vi bindings |
| VI. Performance-Conscious Design | ✅ Pass | SimpleCache caching, version tracking for invalidation |
| VII. Full Scope Commitment | ✅ Pass | All 8 user stories will be implemented |
| VIII. Real-World Testing | ✅ Pass | xUnit with real implementations only |
| IX. Adherence to Planning Documents | ✅ Pass | Follows api-mapping.md for Stroke.KeyBinding |
| X. Source Code File Size Limits | ✅ Pass | Will split into multiple files <1,000 LOC each |
| XI. Thread Safety by Default | ✅ Pass | Lock-based synchronization for mutable state |

## Project Structure

### Documentation (this feature)

```text
specs/022-key-bindings-system/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
└── tasks.md             # Phase 2 output (/speckit.tasks command)
```

### Source Code (repository root)

```text
src/Stroke/KeyBinding/
├── NotImplementedOrNone.cs      # Already exists - handler return type
├── KeyOrChar.cs                 # Union type (Keys enum or single char)
├── Binding.cs                   # Immutable binding (keys, handler, filter, etc.)
├── IKeyBindingsBase.cs          # Interface for all registry types
├── KeyBindings.cs               # Concrete mutable registry with caching
├── KeyBindingsProxy.cs          # Base class for wrapper types (_Proxy)
├── ConditionalKeyBindings.cs    # Conditional wrapper
├── DynamicKeyBindings.cs        # Dynamic callable wrapper
├── GlobalOnlyKeyBindings.cs     # Global-only filter wrapper
├── MergedKeyBindings.cs         # Merged registries wrapper
├── KeyBindingUtils.cs           # Merge function, ParseKey
├── KeyPress.cs                  # KeyPress record struct
├── KeyPressEvent.cs             # Event passed to handlers
└── KeyBindingDecorator.cs       # key_binding decorator equivalent

tests/Stroke.Tests/KeyBinding/
├── KeyOrCharTests.cs
├── BindingTests.cs
├── KeyBindingsTests.cs
├── ConditionalKeyBindingsTests.cs
├── DynamicKeyBindingsTests.cs
├── GlobalOnlyKeyBindingsTests.cs
├── MergedKeyBindingsTests.cs
├── KeyBindingUtilsTests.cs
├── KeyPressTests.cs
├── KeyPressEventTests.cs
├── ThreadSafetyTests.cs
└── PerformanceTests.cs
```

**Structure Decision**: Single project structure. All key binding classes reside in `Stroke.KeyBinding` namespace within the existing Stroke library project. Test files in `Stroke.Tests/KeyBinding/` directory.

## Complexity Tracking

> No violations requiring justification. Implementation follows standard patterns.

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| N/A | N/A | N/A |

## API Mapping Reference

From `docs/api-mapping.md` (Module: prompt_toolkit.key_binding):

| Python | Stroke | Notes |
|--------|--------|-------|
| `Binding` | `Binding` | Immutable binding class |
| `KeyBindingsBase` | `IKeyBindingsBase` | Interface (abstract in Python) |
| `KeyBindings` | `KeyBindings` | Concrete registry |
| `ConditionalKeyBindings` | `ConditionalKeyBindings` | Conditional wrapper |
| `DynamicKeyBindings` | `DynamicKeyBindings` | Dynamic wrapper |
| `GlobalOnlyKeyBindings` | `GlobalOnlyKeyBindings` | Global filter wrapper |
| `_MergedKeyBindings` | `MergedKeyBindings` | Internal merged registry |
| `_Proxy` | `KeyBindingsProxy` | Base for wrappers |
| `merge_key_bindings` | `KeyBindingUtils.Merge` | Static merge function |
| `key_binding` | `KeyBindingDecorator.Create` | Decorator function |
| `_parse_key` | `KeyBindingUtils.ParseKey` | Key parsing |
| `KeyPress` | `KeyPress` | Record struct |
| `KeyPressEvent` | `KeyPressEvent` | Event class |
| N/A | `KeyOrChar` | Union type (Keys or char) |

## Dependencies

### Internal Dependencies (Already Implemented)
- `Stroke.Core.SimpleCache<TKey, TValue>` - Caching for binding lookups
- `Stroke.Filters.IFilter` - Filter interface for conditional activation
- `Stroke.Filters.FilterOrBool` - Union type for filter parameters
- `Stroke.Filters.FilterUtils.ToFilter` - Convert FilterOrBool to IFilter
- `Stroke.Filters.Always` / `Stroke.Filters.Never` - Singleton filters
- `Stroke.Input.Keys` - Keys enum (151 values)
- `Stroke.KeyBinding.NotImplementedOrNone` - Handler return type (already exists)

### External Dependencies
None required.

## Implementation Notes

### Key Matching Algorithm
The binding lookup uses a two-phase approach:
1. `GetBindingsForKeys(keys)` - Returns exact matches, sorted by `Any` wildcard count (fewer wildcards = higher priority)
2. `GetBindingsStartingWithKeys(keys)` - Returns bindings with sequences longer than input prefix

### Cache Strategy
- `GetBindingsForKeys`: 10,000 entry cache
- `GetBindingsStartingWithKeys`: 1,000 entry cache
- Cache invalidated on add/remove via version increment

### Filter Composition Rules
When adding an existing `Binding` to a registry:
- `filter`: Combined with AND (`existingFilter & newFilter`)
- `eager`: Combined with OR (`existingEager | newEager`)
- `is_global`: Combined with OR (`existingGlobal | newGlobal`)

### Thread Safety
All mutable classes use `System.Threading.Lock` with `EnterScope()`:
- `KeyBindings`: Lock protects `_bindings` list and caches
- Proxy classes: Lock protects `_last_version` and `_bindings2`

### Handler Return Values
Handlers return `NotImplementedOrNone`:
- `NotImplemented` → Event not handled, UI not invalidated
- `None` → Event handled, UI invalidated

### Async Handler Support
If handler returns an awaitable, create background task via `event.App.CreateBackgroundTask()`.
