# Implementation Plan: Shortcut Utilities

**Branch**: `046-shortcut-utils` | **Date**: 2026-02-01 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/046-shortcut-utils/spec.md`

## Summary

Implement the `Stroke.Shortcuts` shortcut utility functions — a faithful port of Python Prompt Toolkit's `prompt_toolkit.shortcuts.utils` module. This adds two static classes (`FormattedTextOutput` and `TerminalUtils`) providing 6 public methods: `Print` (2 overloads), `PrintContainer`, `Clear`, `SetTitle`, and `ClearTitle`. These are high-level convenience functions that compose existing infrastructure (RendererUtils, AppContext, RunInTerminal, StyleMerger, OutputFactory) behind simple APIs.

## Technical Context

**Language/Version**: C# 13 / .NET 10+
**Primary Dependencies**: Stroke.Application (AppContext, RunInTerminal, Application), Stroke.Rendering (RendererUtils), Stroke.Styles (StyleMerger, DefaultStyles, IStyle, IStyleTransformation), Stroke.Output (IOutput, OutputFactory, ColorDepth), Stroke.FormattedText (AnyFormattedText, FormattedText, FormattedTextUtils), Stroke.Input (DummyInput), Stroke.Layout (Layout)
**Storage**: N/A (stateless utility functions)
**Testing**: xUnit (no mocks, no FluentAssertions per Constitution VIII)
**Target Platform**: Linux, macOS, Windows 10+ (cross-platform)
**Project Type**: Single project (library module within Stroke)
**Performance Goals**: N/A (simple delegation layer — performance is governed by underlying Renderer and Output)
**Constraints**: Must not exceed 1,000 LOC per file (Constitution X); thread-safe (Constitution XI); 100% API fidelity with Python source (Constitution I)
**Scale/Scope**: 2 static classes, 6 public methods, 1 private helper, ~150 LOC implementation + ~400 LOC tests

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Faithful Port | ✅ PASS | api-mapping.md maps `print_formatted_text` → `FormattedTextOutput.Print`, `print_container` → `FormattedTextOutput.PrintContainer`, `clear` → `TerminalUtils.Clear`, `set_title` → `TerminalUtils.SetTitle`, `clear_title` → `TerminalUtils.ClearTitle`. All 5 Python functions ported to 6 C# methods (Print has 2 overloads matching Python's *values pattern). |
| II. Immutability | ✅ PASS | Both classes are static with no mutable state. All parameters are passed through to underlying infrastructure. |
| III. Layered Architecture | ✅ PASS | Stroke.Shortcuts sits at layer 8 (top), depending on Application (7), Layout (5), Rendering (2), Styles, Output, Input, FormattedText. No circular dependencies. |
| IV. Cross-Platform | ✅ PASS | Delegates to IOutput implementations which already handle platform differences. |
| V. Editing Mode Parity | ✅ N/A | No editing mode involvement. |
| VI. Performance | ✅ PASS | Thin delegation layer. No new data structures, caches, or rendering logic. |
| VII. Full Scope | ✅ PASS | All 6 public methods from api-mapping.md will be implemented. |
| VIII. Real-World Testing | ✅ PASS | Tests will use real IOutput (captured via StringWriter + OutputFactory or Vt100Output) and real style infrastructure. No mocks. |
| IX. Planning Documents | ✅ PASS | api-mapping.md section "Functions - Utilities" consulted. All 5 entries mapped. |
| X. File Size | ✅ PASS | Implementation ~150 LOC split across 2 files. Tests ~400 LOC split across 2 files. Well under 1,000 LOC limit. |
| XI. Thread Safety | ✅ PASS | Both classes are stateless static classes — inherently thread-safe. The underlying `AppContext`, `RunInTerminal`, and `StyleMerger` handle their own synchronization. |
| XII. Contracts in Markdown | ✅ PASS | Contracts defined in `contracts/shortcut-utils-api.md`. |

**Gate result: ALL PASS — no violations requiring complexity tracking.**

## Project Structure

### Documentation (this feature)

```text
specs/046-shortcut-utils/
├── plan.md              # This file
├── spec.md              # Feature specification
├── research.md          # Phase 0: dependency verification
├── data-model.md        # Phase 1: type analysis (minimal — stateless feature)
├── quickstart.md        # Phase 1: implementation quickstart
├── contracts/
│   └── shortcut-utils-api.md  # API contracts
├── checklists/
│   └── requirements.md  # Quality checklist
└── tasks.md             # Phase 2 output (/speckit.tasks command)
```

### Source Code (repository root)

```text
src/Stroke/Shortcuts/
├── FormattedTextOutput.cs    # Print, PrintContainer, CreateMergedStyle
└── TerminalUtils.cs          # Clear, SetTitle, ClearTitle

tests/Stroke.Tests/Shortcuts/
├── FormattedTextOutputTests.cs   # Tests for Print and PrintContainer
└── TerminalUtilsTests.cs         # Tests for Clear, SetTitle, ClearTitle
```

**Structure Decision**: Two source files in `src/Stroke/Shortcuts/` matching the api-mapping.md class split (`FormattedTextOutput` and `TerminalUtils`). Two corresponding test files in `tests/Stroke.Tests/Shortcuts/`. This mirrors the Python module's logical grouping while staying well under the 1,000 LOC limit.

## Complexity Tracking

> No violations detected — table not needed.
