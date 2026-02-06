# Implementation Plan: Full-Screen Examples

**Branch**: `064-fullscreen-examples` | **Date**: 2026-02-05 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/064-fullscreen-examples/spec.md`

## Summary

Implement all 25 Python Prompt Toolkit full-screen examples as faithful C# ports in the `Stroke.Examples.FullScreen` project. Examples demonstrate the Application<T>, Layout, Container (HSplit, VSplit, FloatContainer), Window, and Widget (TextArea, Button, Frame, Box, Label, RadioList, CheckboxList, Dialog) APIs through progressively complex demonstrations—from basic HelloWorld to advanced TextEditor and FullScreenDemo applications.

## Technical Context

**Language/Version**: C# 13 / .NET 10
**Primary Dependencies**: Stroke library (Application, Layout, Widgets, KeyBinding, Styles, Completion, Lexers namespaces)
**Storage**: N/A (examples only, in-memory)
**Testing**: TUI Driver MCP tools for end-to-end verification
**Target Platform**: Cross-platform (Windows, macOS, Linux)
**Project Type**: Console application (example project)
**Performance Goals**: N/A (demonstration code)
**Constraints**: Each example file ≤1,000 LOC per Constitution X
**Scale/Scope**: 25 examples across 3 categories (Main: 10, ScrollablePanes: 2, SimpleDemos: 13)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Faithful Port | ✅ PASS | All 25 examples map 1:1 to Python PTK full-screen examples per docs/examples-mapping.md |
| II. Immutability | ✅ PASS | Examples use existing immutable Document/Buffer patterns |
| III. Layered Architecture | ✅ PASS | Examples project depends only on Stroke library (no circular dependencies) |
| IV. Cross-Platform | ✅ PASS | All examples use VT100/ANSI-compatible APIs |
| V. Editing Modes | ✅ PASS | TextEditor demonstrates Vi/Emacs modes where applicable |
| VI. Performance | ✅ PASS | Examples use existing diff rendering in Application |
| VII. Full Scope | ✅ PASS | All 25 examples will be implemented (no scope reduction) |
| VIII. Real-World Testing | ✅ PASS | TUI Driver verification scripts; no mocks |
| IX. Planning Documents | ✅ PASS | Follows docs/examples-mapping.md Full-Screen section exactly |
| X. File Size Limits | ✅ PASS | Each example is standalone; largest (TextEditor) will be <400 LOC |
| XI. Thread Safety | ✅ PASS | Examples use thread-safe Application/Buffer classes |
| XII. Contracts in Markdown | ✅ PASS | No contracts needed—examples use existing APIs |

## Project Structure

### Documentation (this feature)

```text
specs/064-fullscreen-examples/
├── plan.md              # This file
├── research.md          # Phase 0: Python example analysis
├── data-model.md        # Phase 1: Example catalog with dependencies
├── quickstart.md        # Phase 1: Running and verifying examples
└── tasks.md             # Phase 2: Implementation tasks
```

### Source Code (repository root)

```text
examples/
├── Stroke.Examples.sln                    # Solution (add new project)
└── Stroke.Examples.FullScreen/            # NEW PROJECT
    ├── Stroke.Examples.FullScreen.csproj
    ├── Program.cs                         # Entry point with dictionary routing
    │
    │   # Main examples (10 files)
    ├── HelloWorld.cs
    ├── DummyApp.cs
    ├── NoLayout.cs
    ├── Buttons.cs
    ├── Calculator.cs
    ├── SplitScreen.cs
    ├── Pager.cs
    ├── FullScreenDemo.cs
    ├── TextEditor.cs
    ├── AnsiArtAndTextArea.cs
    │
    │   # ScrollablePanes subdirectory (2 files)
    ├── ScrollablePanes/
    │   ├── SimpleExample.cs
    │   └── WithCompletionMenu.cs
    │
    │   # SimpleDemos subdirectory (13 files)
    └── SimpleDemos/
        ├── HorizontalSplit.cs
        ├── VerticalSplit.cs
        ├── Alignment.cs
        ├── HorizontalAlign.cs
        ├── VerticalAlign.cs
        ├── Floats.cs
        ├── FloatTransparency.cs
        ├── Focus.cs
        ├── Margins.cs
        ├── LinePrefixes.cs
        ├── ColorColumn.cs
        ├── CursorHighlight.cs
        └── AutoCompletion.cs
```

**Structure Decision**: Single console application project following the established pattern from Stroke.Examples.Dialogs and Stroke.Examples.Choices. Dictionary-based routing in Program.cs allows running any example by name (`dotnet run -- HelloWorld`). Subdirectories mirror Python's directory structure for discoverability.

## Complexity Tracking

> No violations—all examples use existing APIs without new abstractions.
