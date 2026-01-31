# Implementation Plan: Mouse Bindings

**Branch**: `036-mouse-bindings` | **Date**: 2026-01-30 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/036-mouse-bindings/spec.md`

## Summary

Implement `MouseBindings.LoadMouseBindings()` — a static method that returns a `KeyBindings` instance with 4 registered bindings for handling VT100 mouse events (XTerm SGR, Typical/X10, URXVT protocols), scroll-without-position events, and Windows mouse events. The implementation includes three lookup tables (108 XTerm SGR entries, 10 Typical entries, 4 URXVT entries), protocol-specific coordinate parsing and transformation, modifier key detection via bit-field decoding, and dispatch to the Renderer's mouse handler registry.

## Technical Context

**Language/Version**: C# 13 / .NET 10
**Primary Dependencies**: Stroke.KeyBinding (KeyBindings, KeyProcessor, KeyPress, KeyPressEvent, NotImplementedOrNone, KeyHandlerCallable), Stroke.Input (Keys, MouseEvent, MouseButton, MouseEventType, MouseModifiers), Stroke.Core.Primitives (Point), Stroke.Rendering (Renderer, HeightIsUnknownException), Stroke.Layout (MouseHandlers), Stroke.Application (Application)
**Storage**: N/A (in-memory lookup tables only)
**Testing**: xUnit (no mocks per Constitution VIII)
**Target Platform**: Linux, macOS, Windows 10+ (cross-platform)
**Project Type**: Single .NET solution
**Performance Goals**: O(1) lookup table access for all protocol event codes
**Constraints**: Static, stateless class; immutable lookup tables; thread-safe by design
**Scale/Scope**: 1 source file (MouseBindings.cs), 1-2 test files, ~500-700 LOC implementation

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Faithful Port | PASS | 1:1 port of `prompt_toolkit.key_binding.bindings.mouse`. All 3 lookup tables match Python source exactly. Single public API `LoadMouseBindings()` matches `load_mouse_bindings()`. |
| II. Immutability | PASS | Lookup tables are `static readonly FrozenDictionary`. No mutable state. `MouseModifiers` is a `[Flags]` enum — documented C# adaptation of Python's `frozenset[MouseModifier]`. |
| III. Layered Architecture | PASS | `MouseBindings` in `Stroke.KeyBinding.Bindings` depends on: Core (Point), Input (Keys, MouseEvent types), KeyBinding (KeyBindings, KeyProcessor, KeyPress), Rendering (Renderer, HeightIsUnknownException), Layout (MouseHandlers). These are all lower layers or same layer. Application accessed via `KeyPressEvent.App` at runtime only. |
| IV. Cross-Platform | PASS | VT100 handlers work on all platforms. Windows handler gated by `RuntimeInformation.IsOSPlatform(OSPlatform.Windows)` check — returns `NotImplemented` on non-Windows. |
| V. Editing Mode Parity | N/A | Mouse bindings are mode-independent (no Emacs/Vi conditions). |
| VI. Performance | PASS | `FrozenDictionary` lookup is O(1). All lookup tables are static readonly — no allocation per call. |
| VII. Full Scope | PASS | All 26 functional requirements addressed. No scope reduction. |
| VIII. Real-World Testing | PASS | Tests exercise real lookup tables, real coordinate math, real `KeyBindings` instances. No mocks. |
| IX. Planning Docs | PASS | Feature doc `docs/features/62-mousebindings.md` specifies `Stroke.KeyBinding.Bindings.MouseBindings`. No test-mapping entries exist for this module — tests follow Python source coverage patterns. |
| X. File Size | PASS | Lookup tables (~160 lines) + handlers (~100 lines) + modifier constants (~15 lines) = ~275 lines implementation. Tests split into 2 files if needed to stay under 1,000 LOC. |
| XI. Thread Safety | PASS | Static class, no mutable state. `FrozenDictionary` is immutable and thread-safe. Runtime dependencies (Renderer, MouseHandlers, KeyProcessor) handle their own synchronization. |
| XII. Contracts in Markdown | PASS | All contracts in `contracts/` are `.md` files. |

## Project Structure

### Documentation (this feature)

```text
specs/036-mouse-bindings/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
│   └── mouse-bindings.md
└── tasks.md             # Phase 2 output (/speckit.tasks command)
```

### Source Code (repository root)

```text
src/Stroke/
└── KeyBinding/
    └── Bindings/
        └── MouseBindings.cs         # Static class with LoadMouseBindings()

tests/Stroke.Tests/
└── KeyBinding/
    └── Bindings/
        ├── MouseBindingsLookupTableTests.cs  # Lookup table validation (SC-001, SC-002, SC-003)
        └── MouseBindingsTests.cs             # Protocol parsing, coordinate transform, dispatch tests
```

**Structure Decision**: Follows the established pattern in `src/Stroke/KeyBinding/Bindings/` where `CompletionBindings.cs` and `NamedCommands*.cs` already reside. The `MouseBindings` class is placed in the same namespace `Stroke.KeyBinding.Bindings` per the feature document and api-mapping namespace rule (`prompt_toolkit.key_binding.bindings` → `Stroke.KeyBinding.Bindings`).

## Complexity Tracking

> No Constitution violations. No complexity tracking needed.

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| (none) | — | — |
