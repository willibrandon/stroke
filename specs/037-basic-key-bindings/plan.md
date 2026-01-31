# Implementation Plan: Basic Key Bindings

**Branch**: `037-basic-key-bindings` | **Date**: 2026-01-30 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/037-basic-key-bindings/spec.md`

## Summary

Implement `BasicBindings.LoadBasicBindings()` — a static factory method that creates and returns a `KeyBindings` instance containing all basic key bindings shared between Emacs and Vi modes. This is a faithful port of Python Prompt Toolkit's `key_binding/bindings/basic.py`, registering 118 bindings across 14 groups: ignored keys, readline movement/editing, self-insert, tab completion, history navigation, auto up/down, selection delete, Ctrl+D, Enter multiline, Ctrl+J re-dispatch, Ctrl+Z, bracketed paste, and quoted insert.

The implementation uses the existing `KeyBindings.Add<T>()` decorator pattern with `NamedCommands.GetByName()` for named command bindings and inline `KeyHandlerCallable` lambdas for custom handlers. Filter composition uses the `Filter` operator overloads (`|`, `&`, `~`) with `ViFilters.ViInsertMode`, `EmacsFilters.EmacsInsertMode`, and `AppFilters` conditions.

## Technical Context

**Language/Version**: C# 13 / .NET 10
**Primary Dependencies**: Stroke.KeyBinding (KeyBindings, Binding, KeyPressEvent, KeyProcessor, KeyPress, KeyOrChar, KeyHandlerCallable, NotImplementedOrNone), Stroke.Input (Keys), Stroke.Filters (IFilter, Filter, Condition, FilterOrBool, Always, Never), Stroke.Application (AppFilters, ViFilters, EmacsFilters, AppContext, Application, KeyPressEventExtensions), Stroke.Core (Buffer), Stroke.Clipboard (IClipboard, ClipboardData)
**Storage**: N/A (in-memory binding registry only)
**Testing**: xUnit (no mocks, no FluentAssertions per Constitution VIII)
**Target Platform**: Linux, macOS, Windows 10+ (cross-platform)
**Project Type**: Single .NET solution with src/ and tests/ directories
**Performance Goals**: Binding registration is one-time startup cost; handler execution must be O(1)
**Constraints**: All source files ≤ 1,000 LOC; thread-safe where applicable
**Scale/Scope**: 118 binding registrations, 1 implementation file, 3 test files, ~1,500-1,800 total LOC

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| # | Principle | Status | Notes |
|---|-----------|--------|-------|
| I | Faithful Port (100% API Fidelity) | ✅ PASS | Direct port of `basic.py`. All bindings mapped 1:1. Public API: `BasicBindings.LoadBasicBindings()` matches Python's `load_basic_bindings()`. |
| II | Immutability by Default | ✅ PASS | `BasicBindings` is a stateless static class. `Binding` is immutable. `KeyBindings` is mutable but thread-safe (existing infrastructure). |
| III | Layered Architecture | ✅ PASS | Placed in `Stroke.Application.Bindings` — correct layer since it depends on AppFilters, ViFilters, EmacsFilters from Application layer. No circular dependencies. |
| IV | Cross-Platform Terminal Compatibility | ✅ PASS | Bindings are platform-agnostic. Bracketed paste handler normalizes line endings for cross-platform compatibility. |
| V | Complete Editing Mode Parity | ✅ PASS | InsertMode filter is `ViInsertMode | EmacsInsertMode`, ensuring correct behavior in both modes. |
| VI | Performance-Conscious Design | ✅ PASS | Shared static handler for ignored keys (no per-key allocation). Static filter instances. `IfNoRepeat` is a static method reference. |
| VII | Full Scope Commitment | ✅ PASS | All 19 functional requirements addressed. No deferrals. |
| VIII | Real-World Testing | ✅ PASS | xUnit tests with real Buffer, real Application, real AppContext. No mocks. |
| IX | Adherence to Planning Documents | ✅ PASS | `api-mapping.md` does not have a section for `basic.py` (gap). Feature doc `59-basicbindings.md` exists and is consistent with this plan. |
| X | Source Code File Size Limits | ✅ PASS | Implementation ~300 LOC. Tests split into 3 files (~400-600 LOC each). |
| XI | Thread Safety by Default | ✅ PASS | `BasicBindings` is stateless (inherently thread-safe). Filters are immutable singletons. Handler lambdas capture no mutable state. |
| XII | Contracts in Markdown Only | ✅ PASS | Contract defined in `contracts/basic-bindings.md`. No `.cs` contract files. |

**Post-Design Re-check**: All gates continue to pass. No violations detected.

## Project Structure

### Documentation (this feature)

```text
specs/037-basic-key-bindings/
├── plan.md                              # This file
├── spec.md                              # Feature specification
├── research.md                          # Phase 0: research decisions
├── data-model.md                        # Phase 1: entity definitions
├── quickstart.md                        # Phase 1: build sequence
├── contracts/
│   └── basic-bindings.md               # Phase 1: API contract
├── checklists/
│   └── requirements.md                 # Spec quality checklist
└── tasks.md                            # Phase 2: task list (created by /speckit.tasks)
```

### Source Code (repository root)

```text
src/Stroke/
└── Application/
    └── Bindings/
        └── BasicBindings.cs             # Implementation (~300 LOC)

tests/Stroke.Tests/
└── Application/
    └── Bindings/
        ├── BasicBindingsIgnoredKeysTests.cs   # Ignored keys tests (~350 LOC)
        ├── BasicBindingsReadlineTests.cs       # Readline + named cmd tests (~450 LOC)
        └── BasicBindingsHandlerTests.cs        # Inline handler tests (~550 LOC)
```

**Structure Decision**: Follows the established pattern of `Stroke.Application.Bindings` for binding loaders (same location as `ScrollBindings.cs`, `PageNavigationBindings.cs`, `MouseBindings.cs`). Tests follow the mirror structure in `tests/Stroke.Tests/Application/Bindings/`, split into 3 files by test category to stay within 1,000 LOC per file.

## Complexity Tracking

No violations. No complexity justifications needed.
