# Research: Get Input Example (First Example)

**Feature**: 122-get-input-example
**Date**: 2026-02-03
**Status**: Complete

## Overview

This document captures research findings for implementing the first Stroke example. Since the feature ports a 3-line Python example with no NEEDS CLARIFICATION items in the technical context, research focused on best practices for .NET example project organization.

## Research Tasks

### 1. Python Reference Implementation

**Task**: Analyze `get-input.py` from Python Prompt Toolkit

**Findings**:
```python
#!/usr/bin/env python
"""
The most simple prompt example.
"""

from prompt_toolkit import prompt

if __name__ == "__main__":
    answer = prompt("Give me some input: ")
    print(f"You said: {answer}")
```

**Decision**: Port line-for-line using `Prompt.RunPrompt()` from `Stroke.Shortcuts`
**Rationale**: Constitution I requires 100% API fidelity; the Python example's simplicity is intentional
**Alternatives Considered**: None — the reference implementation is canonical

### 2. Stroke API Availability

**Task**: Verify `Prompt.RunPrompt()` exists in Stroke.Shortcuts

**Findings**:
- `Prompt.RunPrompt()` exists at `src/Stroke/Shortcuts/Prompt.cs:39`
- Takes optional `message` parameter as `AnyFormattedText?`
- Returns `string` (user input)
- Handles Ctrl+C, terminal setup/teardown, key bindings internally

**Decision**: Use `Prompt.RunPrompt("Give me some input: ")` directly
**Rationale**: API matches Python's `prompt("Give me some input: ")` exactly
**Alternatives Considered**: `PromptAsync` — rejected for this basic example; async shown in later examples

### 3. .NET Example Project Best Practices

**Task**: Determine standard structure for .NET example solutions

**Findings**:
- Common pattern: separate `examples/` directory from `src/`
- Each example category as a separate console project
- Shared solution file (`*.sln`) for all examples
- Project references main library via `<ProjectReference>`
- Entry point pattern: selector routing by command-line argument

**Decision**: Follow `docs/examples-mapping.md` structure exactly
**Rationale**: Constitution IX requires adherence to planning documents
**Alternatives Considered**:
- Single mega-project with all examples — rejected; doesn't scale to 129 examples
- No selector pattern — rejected; FR-006 requires named example routing

### 4. Example Selector Entry Point Pattern

**Task**: Design Program.cs routing pattern for named examples

**Findings**:
- .NET convention: `dotnet run -- <args>` passes args to Main
- Reflection-based discovery vs. explicit switch — explicit is simpler, faster
- Error messages should list available examples

**Decision**: Dictionary-based routing with explicit example registration
**Rationale**: Simpler than reflection; clear error messages; easy to extend
**Alternatives Considered**:
- Reflection scanning for `[Example]` attribute — over-engineered for current scope
- Factory pattern — unnecessary abstraction for static `Run()` methods

### 5. TUI Driver Verification

**Task**: Confirm TUI Driver can test example behavior

**Findings**:
- `tui_launch` starts process in PTY session
- `tui_wait_for_text` waits for prompt to appear
- `tui_send_text` types input
- `tui_press_key` sends Enter
- `tui_text` captures final output for assertions

**Decision**: Use TUI Driver verification script from spec (SC-006)
**Rationale**: Constitution VIII requires real-world testing; TUI Driver provides this
**Alternatives Considered**: None — TUI Driver is the designated testing tool

## Summary

| Research Item | Decision | Status |
|---------------|----------|--------|
| Python reference | Port line-for-line | ✅ Complete |
| Stroke API | `Prompt.RunPrompt()` | ✅ Verified |
| Project structure | `docs/examples-mapping.md` layout | ✅ Adopted |
| Entry point pattern | Dictionary-based selector | ✅ Designed |
| Testing approach | TUI Driver verification | ✅ Confirmed |

**All NEEDS CLARIFICATION items resolved**: None were identified in Technical Context.
