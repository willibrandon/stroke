# Implementation Plan: Vi Key Bindings

**Branch**: `043-vi-key-bindings` | **Date**: 2026-01-31 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/043-vi-key-bindings/spec.md`

## Summary

Implement the complete Vi editing mode key bindings for Stroke, faithfully porting Python Prompt Toolkit's `prompt_toolkit.key_binding.bindings.vi` module. This includes ~151 binding registrations across two public entry points (`LoadViBindings()` and `LoadViSearchBindings()`), the `TextObject` class with its `TextObjectType` enum, the text object and operator decorator factory patterns, and all Vi navigation motions, operators, text objects, mode switches, and miscellaneous commands. The `LoadViSearchBindings()` is already implemented in `SearchBindings.cs`; this feature covers `LoadViBindings()` and the supporting `TextObject`/`TextObjectType` types.

## Technical Context

**Language/Version**: C# 13 / .NET 10
**Primary Dependencies**: Stroke.Core (Buffer, Document, SelectionState, PasteMode), Stroke.KeyBinding (Binding, KeyPressEvent, KeyPress, KeyProcessor, ViState, InputMode, CharacterFind, OperatorFuncDelegate, EditingMode), Stroke.Clipboard (IClipboard, ClipboardData), Stroke.Application (Application, AppContext, AppFilters, ViFilters, SearchFilters, KeyPressEventExtensions), Stroke.Layout (Window, WindowRenderInfo, BufferControl, SearchBufferControl), Stroke.Input (Keys), Stroke.Filters (IFilter, Filter, Condition, FilterOrBool, Always, Never), Stroke.KeyBinding.Bindings (NamedCommands, SearchBindings)
**Storage**: N/A (in-memory binding registry only)
**Testing**: xUnit (no mocks, no FluentAssertions per Constitution VIII)
**Target Platform**: Linux, macOS, Windows 10+ (.NET 10)
**Project Type**: Single project (existing Stroke library)
**Performance Goals**: Binding lookup O(1) via KeyBindings registry; handler execution < 1ms per key press
**Constraints**: Each source file ≤ 1,000 LOC; thread-safe where mutable state exists
**Scale/Scope**: 112 direct `@handle`/`handle()` handler functions + 74 text object registrations (each creating up to 3 internal bindings) + 14 operator registrations (each creating 2 internal bindings), 13 Vi-specific tests from test-mapping.md. `LoadViSearchBindings()` already implemented separately

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| # | Principle | Status | Notes |
|---|-----------|--------|-------|
| I | Faithful Port (100% API Fidelity) | PASS | All bindings from Python vi.py will be ported 1:1 (112 direct handlers + 74 text objects + 14 operators). TextObject, TextObjectType match Python originals. Dot command (.) intentionally excluded — not present in Python source. |
| II | Immutability by Default | PASS | TextObject and TextObjectType are immutable. ViBindings is stateless. Mutable state managed through existing ViState (thread-safe). |
| III | Layered Architecture | PASS | ViBindings lives in Stroke.Application.Bindings (Application layer). TextObject/TextObjectType in Stroke.KeyBinding (KeyBinding layer). No circular dependencies. |
| IV | Cross-Platform Terminal Compatibility | PASS | No platform-specific code; bindings use abstract key identifiers. |
| V | Complete Editing Mode Parity | PASS | This feature directly implements Vi editing mode parity per this principle. |
| VI | Performance-Conscious Design | PASS | Stateless binding loaders; no allocation in hot paths. |
| VII | Full Scope Commitment | PASS | All bindings from Python vi.py (112 + 74 + 14), all text objects, all operators, all handlers implemented. No deferral. |
| VIII | Real-World Testing | PASS | Tests use xUnit with real Buffer/Document instances. No mocks. |
| IX | Adherence to Planning Documents | PASS | API mapping consulted: ViBindings.LoadViBindings() → IKeyBindingsBase, TextObject class, TextObjectType enum. |
| X | Source Code File Size Limits | PASS | ViBindings split across multiple partial class files by category. Target: 6-8 files, each ≤ 1,000 LOC. |
| XI | Thread Safety by Default | PASS | ViBindings is stateless (inherently thread-safe). TextObject is immutable (inherently thread-safe). Mutable state managed by existing thread-safe ViState. |
| XII | Contracts in Markdown Only | PASS | All contracts in markdown code blocks. |

## Project Structure

### Documentation (this feature)

```text
specs/043-vi-key-bindings/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
│   ├── text-object.md
│   └── vi-bindings.md
└── tasks.md             # Phase 2 output (/speckit.tasks)
```

### Source Code (repository root)

```text
src/Stroke/KeyBinding/
├── TextObjectType.cs                    # TextObjectType enum (NEW)
├── TextObject.cs                        # TextObject class (NEW)
├── OperatorFuncDelegate.cs              # Update: object? → TextObject parameter type
├── ViState.cs                           # Existing (no changes needed)
├── CharacterFind.cs                     # Existing (no changes needed)
├── InputMode.cs                         # Existing (no changes needed)
└── Digraphs.cs                          # Existing (used by Ctrl-K handler)

src/Stroke/Application/Bindings/
├── ViBindings.cs                        # Main loader + condition helpers (NEW)
├── ViBindings.Navigation.cs             # Navigation motions (h,j,k,l,w,b,e etc.) (NEW)
├── ViBindings.Operators.cs              # Operator/text object decorator factories + operators (NEW)
├── ViBindings.TextObjects.cs            # Text object registrations (~52 text objects) (NEW)
├── ViBindings.ModeSwitch.cs             # Mode switching (i,a,o,v,V,R,r,Escape) (NEW)
├── ViBindings.InsertMode.cs             # Insert mode bindings (Ctrl-V, Ctrl-N, digraphs, etc.) (NEW)
├── ViBindings.VisualMode.cs             # Visual/selection mode handlers (NEW)
├── ViBindings.Misc.cs                   # Macros, digraphs, registers, misc commands (NEW)
├── SearchBindings.cs                    # Existing (LoadViSearchBindings already implemented)
├── EmacsBindings.cs                     # Existing (reference pattern)
└── EmacsBindings.ShiftSelection.cs      # Existing (reference pattern)

tests/Stroke.Tests/KeyBinding/
├── TextObjectTypeTests.cs               # TextObjectType enum tests (NEW)
└── TextObjectTests.cs                   # TextObject class tests (NEW)

tests/Stroke.Tests/Application/ViModeTests.cs  # 13 mapped integration tests from test-mapping.md (NEW)

tests/Stroke.Tests/Application/Bindings/ViBindings/
├── LoadViBindingsNavigationTests.cs     # Supplementary navigation binding tests (NEW)
├── LoadViBindingsOperatorTests.cs       # Supplementary operator binding tests (NEW)
├── LoadViBindingsTextObjectTests.cs     # Supplementary text object binding tests (NEW)
├── LoadViBindingsModeSwitchTests.cs     # Supplementary mode switch binding tests (NEW)
├── LoadViBindingsInsertModeTests.cs     # Supplementary insert mode binding tests (NEW)
├── LoadViBindingsVisualModeTests.cs     # Supplementary visual mode binding tests (NEW)
├── LoadViBindingsMiscTests.cs           # Supplementary misc command binding tests (NEW)
└── ViBindingsIntegrationTests.cs        # Additional integration tests (NEW)
```

**Structure Decision**: Follows existing Stroke project structure. Source files in `src/Stroke/` organized by namespace layer. Test files in `tests/Stroke.Tests/` mirroring source structure. ViBindings uses partial class pattern (same as EmacsBindings) to stay under 1,000 LOC per file. The 13 mapped integration tests from `test-mapping.md` go in `ViModeTests.cs` (per Constitution IX); supplementary binding registration and behavior tests go in per-category files under `ViBindings/`.

## Complexity Tracking

> No Constitution violations. No complexity tracking needed.

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| (none) | — | — |
