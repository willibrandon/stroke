# Implementation Plan: Choices Examples (Complete Set)

**Branch**: `062-choices-examples` | **Date**: 2026-02-04 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/062-choices-examples/spec.md`

## Summary

Implement all 8 Python Prompt Toolkit choices examples in a new `Stroke.Examples.Choices` project demonstrating `Dialogs.Choice<T>()` capabilities: basic selection, default values, custom styling, frames, bottom toolbars, style changes on accept, scrollable lists, and mouse support. Each example is a direct port of the corresponding Python example file.

## Technical Context

**Language/Version**: C# 13 / .NET 10
**Primary Dependencies**: Stroke library (Stroke.Shortcuts.Dialogs, Stroke.Styles, Stroke.FormattedText, Stroke.Filters)
**Storage**: N/A (console applications, no persistence)
**Testing**: TUI Driver MCP (real terminal automation, no unit tests for examples)
**Target Platform**: Cross-platform (Windows, macOS, Linux)
**Project Type**: Example console application
**Performance Goals**: <1s navigation response, <100ms mouse clicks (per SC-004, SC-007)
**Constraints**: All examples must handle Ctrl+C/Ctrl+D gracefully (FR-014)
**Scale/Scope**: 8 example files + 1 Program.cs entry point

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Faithful Port (100% API Fidelity) | ✅ PASS | All 8 examples map 1:1 to Python PTK choices examples |
| II. Immutability by Default | ✅ PASS | Examples are stateless entry points, no mutable data structures |
| III. Layered Architecture | ✅ PASS | Examples depend only on Stroke.Shortcuts (topmost layer) |
| IV. Cross-Platform Terminal Compatibility | ✅ PASS | Uses standard Stroke APIs which handle platform differences |
| V. Complete Editing Mode Parity | ✅ N/A | Choices use arrow/Enter navigation, not vi/emacs editing |
| VI. Performance-Conscious Design | ✅ PASS | Uses existing differential rendering in Stroke |
| VII. Full Scope Commitment | ✅ PASS | All 8 examples required, none deferred |
| VIII. Real-World Testing | ✅ PASS | TUI Driver verification, no mocks |
| IX. Adherence to Planning Documents | ✅ PASS | Follows examples-mapping.md patterns |
| X. Source Code File Size Limits | ✅ PASS | Each example ~15-35 lines, well under 1000 LOC |
| XI. Thread Safety by Default | ✅ PASS | Examples are stateless, no mutable state |

**Gate Status**: ✅ ALL PASS — Proceed to Phase 0

## Project Structure

### Documentation (this feature)

```text
specs/062-choices-examples/
├── plan.md              # This file
├── research.md          # Phase 0 output (N/A - no clarifications needed)
├── data-model.md        # Phase 1 output (N/A - no new entities)
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output (N/A - no API contracts)
└── tasks.md             # Phase 2 output (/speckit.tasks command)
```

### Source Code (repository root)

```text
examples/
├── Stroke.Examples.sln                    # Existing solution (add new project)
├── Stroke.Examples.Prompts/               # Existing project (pattern reference)
│   ├── Program.cs
│   └── ...
└── Stroke.Examples.Choices/               # NEW PROJECT
    ├── Stroke.Examples.Choices.csproj     # Project file
    ├── Program.cs                         # Entry point with routing
    ├── SimpleSelection.cs                 # Example 1: basic selection
    ├── Default.cs                         # Example 2: default value + HTML
    ├── Color.cs                           # Example 3: custom styling
    ├── WithFrame.cs                       # Example 4: conditional frame
    ├── FrameAndBottomToolbar.cs           # Example 5: frame + toolbar
    ├── GrayFrameOnAccept.cs               # Example 6: style change on accept
    ├── ManyChoices.cs                     # Example 7: 99 options scrolling
    └── MouseSupport.cs                    # Example 8: mouse clicks
```

**Structure Decision**: Single console application project following the established `Stroke.Examples.Prompts` pattern. Dictionary-based routing in `Program.cs` maps example names to static `Run()` methods.

## Complexity Tracking

> No violations to track — all Constitution principles pass.

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| (none)    | —          | —                                   |
