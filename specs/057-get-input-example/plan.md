# Implementation Plan: Get Input Example (First Example)

**Branch**: `122-get-input-example` | **Date**: 2026-02-03 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/122-get-input-example/spec.md`

## Summary

Implement the first and simplest Stroke example: `GetInput.cs`. This establishes the examples infrastructure (directory structure, solution file, project file, entry point) and demonstrates the most basic prompt usage by porting Python Prompt Toolkit's `get-input.py` to C#. The example prompts for text input, accepts it on Enter, and echoes the result.

## Technical Context

**Language/Version**: C# 13 / .NET 10+
**Primary Dependencies**: Stroke library (`Stroke.Shortcuts.Prompt.RunPrompt`)
**Storage**: N/A (in-memory only)
**Testing**: TUI Driver verification (real terminal testing per Constitution VIII)
**Target Platform**: Linux, macOS, Windows 10+ (cross-platform)
**Project Type**: Console example application
**Performance Goals**: Prompt displays within 2 seconds, echo within 100ms of Enter (per SC-002, SC-003)
**Constraints**: Example code under 15 lines excluding namespace/using statements (per SC-004)
**Scale/Scope**: Single example file, entry point with example selector pattern for future 128+ examples

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Evidence |
|-----------|--------|----------|
| **I. Faithful Port (100% API Fidelity)** | ✅ PASS | GetInput.cs ports `get-input.py` exactly — same prompt text, same output format |
| **II. Immutability by Default** | ✅ N/A | No data structures introduced; uses existing Stroke APIs |
| **III. Layered Architecture** | ✅ PASS | Examples depend on Stroke.Shortcuts (highest layer); no circular dependencies |
| **IV. Cross-Platform Terminal Compatibility** | ✅ PASS | Uses `Prompt.RunPrompt()` which handles platform differences internally |
| **V. Complete Editing Mode Parity** | ✅ N/A | Example uses default editing mode; tests will verify both work |
| **VI. Performance-Conscious Design** | ✅ PASS | SC-002, SC-003 define measurable performance criteria |
| **VII. Full Scope Commitment** | ✅ PASS | All 9 functional requirements mapped to tasks; no scope reduction |
| **VIII. Real-World Testing** | ✅ PASS | TUI Driver verification script tests actual terminal behavior; no mocks |
| **IX. Adherence to Planning Documents** | ✅ PASS | Structure matches `docs/examples-mapping.md` exactly |
| **X. Source Code File Size Limits** | ✅ PASS | GetInput.cs is ~10 lines; Program.cs is ~50 lines; well under 1,000 LOC |
| **XI. Thread Safety by Default** | ✅ N/A | Example code is single-threaded; Stroke internals handle thread safety |

**Gate Result**: ✅ ALL PASS — Proceed to Phase 0.

### Post-Design Re-Check (Phase 1)

After completing research.md and quickstart.md, all principles remain compliant:

| Principle | Status | Post-Design Notes |
|-----------|--------|-------------------|
| **I. Faithful Port** | ✅ PASS | quickstart.md confirms line-for-line Python equivalence |
| **VII. Full Scope** | ✅ PASS | research.md maps all functional requirements |
| **VIII. Real-World Testing** | ✅ PASS | TUI Driver verification documented in quickstart.md |
| **IX. Planning Documents** | ✅ PASS | Structure matches examples-mapping.md exactly |

**Post-Design Gate Result**: ✅ ALL PASS — Ready for `/speckit.tasks`.

## Project Structure

### Documentation (this feature)

```text
specs/122-get-input-example/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output (N/A - no data model)
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output (N/A - no API contracts)
└── tasks.md             # Phase 2 output (/speckit.tasks)
```

### Source Code (repository root)

```text
examples/
├── Stroke.Examples.sln                    # Solution file for all examples
│
└── Stroke.Examples.Prompts/
    ├── Stroke.Examples.Prompts.csproj     # Project file referencing Stroke
    ├── Program.cs                          # Entry point with example selector
    └── GetInput.cs                         # The simplest prompt example
```

**Structure Decision**: Follows `docs/examples-mapping.md` structure exactly. The `examples/` directory is separate from `src/` to maintain isolation. Each example category becomes a separate project within the shared solution.

## Complexity Tracking

> No violations — all principles pass.

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| — | — | — |
