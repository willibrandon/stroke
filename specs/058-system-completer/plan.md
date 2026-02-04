# Implementation Plan: System Completer

**Branch**: `058-system-completer` | **Date**: 2026-02-03 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/058-system-completer/spec.md`

## Summary

Implement SystemCompleter - a pre-configured GrammarCompleter that provides completion for shell commands. It compiles a regex grammar with named groups for executable commands and file path arguments (unquoted, double-quoted, single-quoted), then maps each variable to the appropriate completer (ExecutableCompleter or PathCompleter).

## Technical Context

**Language/Version**: C# 13 / .NET 10+
**Primary Dependencies**: `Stroke.Contrib.RegularLanguages` (Grammar, GrammarCompleter), `Stroke.Completion` (ExecutableCompleter, PathCompleter)
**Storage**: N/A (in-memory only)
**Testing**: xUnit (no mocks per Constitution VIII)
**Target Platform**: Linux, macOS, Windows 10+
**Project Type**: Single library (Stroke)
**Performance Goals**: N/A (completion latency dominated by filesystem I/O)
**Constraints**: Must faithfully port Python Prompt Toolkit's SystemCompleter
**Scale/Scope**: Single class ~50 LOC

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Faithful Port | ✅ PASS | Direct 1:1 port of `prompt_toolkit/contrib/completers/system.py` |
| II. Immutability | ✅ PASS | SystemCompleter is stateless after construction (inherits from immutable GrammarCompleter) |
| III. Layered Architecture | ✅ PASS | `Stroke.Contrib.RegularLanguages` depends only on `Stroke.Core` and `Stroke.Completion` |
| IV. Cross-Platform | ✅ PASS | Delegates to ExecutableCompleter/PathCompleter which handle platform differences |
| V. Editing Mode Parity | ✅ N/A | No editing mode functionality |
| VI. Performance | ✅ PASS | Grammar compiled once at construction; completion uses existing optimized completers |
| VII. Full Scope | ✅ PASS | All Python APIs ported without omission |
| VIII. Real-World Testing | ✅ PASS | Tests will use real filesystem/PATH; no mocks |
| IX. Planning Documents | ✅ PASS | Follows contrib namespace structure from api-mapping.md |
| X. File Size | ✅ PASS | Single file ~50 LOC |
| XI. Thread Safety | ✅ PASS | Inherits thread safety from GrammarCompleter (stateless after construction) |

## Project Structure

### Documentation (this feature)

```text
specs/058-system-completer/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
└── tasks.md             # Phase 2 output (/speckit.tasks command)
```

### Source Code (repository root)

```text
src/Stroke/Contrib/Completers/
└── SystemCompleter.cs   # NEW: Single class file

tests/Stroke.Tests/Contrib/Completers/
└── SystemCompleterTests.cs  # NEW: Test file
```

**Structure Decision**: SystemCompleter is placed in `Stroke.Contrib.Completers` namespace, mirroring Python's `prompt_toolkit.contrib.completers.system` module. This follows the existing pattern where contrib components are organized by type (completers, regular_languages, etc.).

## Complexity Tracking

> No violations requiring justification. Implementation is minimal (single class ~50 LOC).

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| N/A | N/A | N/A |
