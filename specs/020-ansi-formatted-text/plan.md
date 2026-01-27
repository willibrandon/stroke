# Implementation Plan: ANSI Formatted Text - % Operator

**Branch**: `020-ansi-formatted-text` | **Date**: 2026-01-26 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/020-ansi-formatted-text/spec.md`

## Summary

Add `%` operator overloads to the `Ansi` class to achieve 100% API parity with Python Prompt Toolkit's `ANSI.__mod__` method. The implementation will reuse the existing `AnsiFormatter.FormatPercent()` method, following the same pattern already established in the `Html` class.

## Technical Context

**Language/Version**: C# 13 / .NET 10
**Primary Dependencies**: None (Stroke.Core layer with zero external dependencies per Constitution III)
**Storage**: N/A (in-memory only)
**Testing**: xUnit (no mocks, no FluentAssertions per Constitution VIII)
**Target Platform**: Linux, macOS, Windows 10+
**Project Type**: Single .NET solution
**Performance Goals**: N/A (trivial string operations)
**Constraints**: Must match Python Prompt Toolkit behavior exactly per Constitution I
**Scale/Scope**: 2 new operator overloads, ~15 lines of implementation, 15 new tests

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Faithful Port | ✅ PASS | Implements missing `__mod__` API from Python PTK |
| II. Immutability | ✅ PASS | `%` operator returns new `Ansi` instance |
| III. Layered Architecture | ✅ PASS | Stroke.FormattedText is part of Core layer |
| IV. Cross-Platform | ✅ PASS | No platform-specific code |
| V. Editing Mode Parity | N/A | Not related to editing modes |
| VI. Performance | ✅ PASS | Uses existing optimized `FormatPercent()` |
| VII. Full Scope | ✅ PASS | All requirements will be implemented |
| VIII. Real-World Testing | ✅ PASS | Tests will use real `Ansi` instances |
| IX. Planning Documents | ✅ PASS | Follows api-mapping.md |
| X. File Size | ✅ PASS | `Ansi.cs` is 366 LOC, well under 1,000 |
| XI. Thread Safety | ✅ PASS | Operators are stateless; create new immutable instances |
| XII. Contracts in Markdown | ✅ PASS | Contract defined below in markdown |

**Gate Status**: ✅ ALL GATES PASS

## Project Structure

### Documentation (this feature)

```text
specs/020-ansi-formatted-text/
├── plan.md              # This file
├── spec.md              # Feature specification
├── research.md          # Phase 0 output (minimal - no unknowns)
├── data-model.md        # Phase 1 output (minimal - no new data models)
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
│   └── ansi-percent-operator.md
├── checklists/
│   └── requirements.md  # Quality validation checklist
└── tasks.md             # Phase 2 output (created by /speckit.tasks)
```

### Source Code (repository root)

```text
src/
└── Stroke/
    └── FormattedText/
        ├── Ansi.cs              # Add % operator overloads (lines 91-92 area)
        └── AnsiFormatter.cs     # Already has FormatPercent() - no changes

tests/
└── Stroke.Tests/
    └── FormattedText/
        └── AnsiTests.cs         # Add % operator tests (new region T049)
```

**Structure Decision**: Extending existing files in the established `Stroke.FormattedText` namespace. No new files needed.

## Complexity Tracking

No violations to justify - this is a minimal addition following existing patterns.

---

## Phase 0: Research

**Prerequisites**: Technical Context complete

### Research Questions

None identified. All technical details are clear:

1. **Python implementation**: Located at `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/formatted_text/ansi.py` lines 268-276
2. **Existing C# infrastructure**: `AnsiFormatter.FormatPercent()` already implements `%s` substitution with ANSI escaping
3. **Existing pattern**: `Html` class already has `%` operator overloads using identical approach

### Findings

| Decision | Rationale | Alternatives Rejected |
|----------|-----------|----------------------|
| Use `AnsiFormatter.FormatPercent()` | Already implemented and tested; used by `Html` class | Inline implementation would duplicate code |
| Follow `Html` operator pattern exactly | Consistency with existing codebase; proven approach | N/A |
| Single value converts to array internally | Matches Python `__mod__` behavior | N/A |

**Output**: No unknowns. Ready for Phase 1.

---

## Phase 1: Design & Contracts

**Prerequisites**: Research complete

### Data Model

No new data models required. The feature adds behavior to existing `Ansi` class.

### API Contract

#### File: `/specs/020-ansi-formatted-text/contracts/ansi-percent-operator.md`

```csharp
namespace Stroke.FormattedText;

public sealed class Ansi : IFormattedText
{
    // ... existing members ...

    /// <summary>
    /// Formats ANSI text with %s-style substitution (single value).
    /// </summary>
    /// <param name="ansi">The Ansi template.</param>
    /// <param name="value">The value to substitute.</param>
    /// <returns>A new Ansi with the value escaped and substituted.</returns>
    /// <remarks>
    /// ANSI escape sequences (\x1b) and backspaces (\b) in the value are replaced with '?'.
    /// This prevents style injection attacks.
    /// </remarks>
    public static Ansi operator %(Ansi ansi, object value);

    /// <summary>
    /// Formats ANSI text with %s-style substitution (multiple values).
    /// </summary>
    /// <param name="ansi">The Ansi template.</param>
    /// <param name="values">The values to substitute.</param>
    /// <returns>A new Ansi with all values escaped and substituted.</returns>
    /// <remarks>
    /// ANSI escape sequences (\x1b) and backspaces (\b) in values are replaced with '?'.
    /// If there are more placeholders than values, extra placeholders remain as literal %s.
    /// If there are more values than placeholders, extra values are ignored.
    /// </remarks>
    public static Ansi operator %(Ansi ansi, object[] values);
}
```

### Implementation Reference

The implementation follows the exact pattern from `Html.cs` (lines 85-95):

```csharp
// In Ansi.cs, add after line 89 (after Escape method, before ToString):

/// <summary>
/// Formats ANSI text with %s-style substitution (single value).
/// </summary>
/// <param name="ansi">The Ansi template.</param>
/// <param name="value">The value to substitute.</param>
/// <returns>A new Ansi with the value escaped and substituted.</returns>
public static Ansi operator %(Ansi ansi, object value) =>
    new(AnsiFormatter.FormatPercent(ansi.Value, value));

/// <summary>
/// Formats ANSI text with %s-style substitution (multiple values).
/// </summary>
/// <param name="ansi">The Ansi template.</param>
/// <param name="values">The values to substitute.</param>
/// <returns>A new Ansi with all values escaped and substituted.</returns>
public static Ansi operator %(Ansi ansi, object[] values) =>
    new(AnsiFormatter.FormatPercent(ansi.Value, values));
```

### Test Plan

Add new test region `#region T049: % operator tests` to `AnsiTests.cs`:

| Test Name | Scenario | Expected Result | Requirement |
|-----------|----------|-----------------|-------------|
| `PercentOperator_WithSingleValue_SubstitutesAndEscapes` | `new Ansi("\x1b[1m%s\x1b[0m") % "hello"` | "hello" with bold styling | FR-001, FR-003 |
| `PercentOperator_WithArray_SubstitutesAllPlaceholders` | `new Ansi("%s and %s") % new object[] { "foo", "bar" }` | "foo and bar" | FR-002, FR-003 |
| `PercentOperator_WithAnsiInValue_EscapesControlChars` | `new Ansi("%s") % "\x1b[31mred"` | Contains "?" instead of escape char | FR-004, SEC-T001 |
| `PercentOperator_WithBackspaceInValue_EscapesBackspace` | `new Ansi("%s") % "a\bb"` | Contains "?" instead of backspace | FR-005, SEC-T002 |
| `PercentOperator_WithCombinedEscape_NeutralizesBoth` | `new Ansi("%s") % "\x1b\b"` | Contains "??" | FR-004, FR-005, SEC-T003 |
| `PercentOperator_WithInsufficientArgs_LeavesPlaceholders` | `new Ansi("%s and %s") % "only one"` | "only one and %s" | FR-007 |
| `PercentOperator_WithExtraArgs_IgnoresExtra` | `new Ansi("%s") % new object[] { "first", "second" }` | "first" only | FR-007 |
| `PercentOperator_WithEmptyArray_LeavesTemplate` | `new Ansi("%s") % new object[] { }` | "%s" (unchanged) | FR-007 |
| `PercentOperator_WithNullValue_ConvertsToEmpty` | `new Ansi("Hello %s") % (object?)null` | "Hello " | Edge case |
| `PercentOperator_WithNullArray_ThrowsArgumentNull` | `new Ansi("%s") % (object[]?)null` | ArgumentNullException | Edge case |
| `PercentOperator_PreservesOriginalStyling` | `new Ansi("\x1b[1mHello %s\x1b[0m") % "World"` | "Hello World" with bold on "Hello" | FR-007 |
| `PercentOperator_ReturnsNewInstance` | Compare original and result references | Different instances | FR-006 |
| `PercentOperator_WithTerminalReset_NeutralizesEscape` | `new Ansi("%s") % "\x1b[0m\x1b[H\x1b[J"` | All `\x1b` replaced with `?` | SEC-T004 |
| `PercentOperator_WithNonStringValue_CallsToString` | `new Ansi("%s") % 42` | `"42"` | Edge case |
| `PercentOperator_WithNoPlaceholders_ReturnsUnchanged` | `new Ansi("Hello") % "ignored"` | `"Hello"` (unchanged) | Edge case |

### Quickstart

After implementation, developers can use:

```csharp
using Stroke.FormattedText;

// Single value substitution
var greeting = new Ansi("\x1b[1mHello %s!\x1b[0m") % "World";
// Result: Bold "Hello World!" with World inserted

// Multiple value substitution
var message = new Ansi("\x1b[32m%s\x1b[0m said: %s") % new object[] { "Alice", "Hello!" };
// Result: "Alice" in green, followed by " said: Hello!"

// Automatic ANSI escape neutralization (security)
var safe = new Ansi("User input: %s") % "\x1b[31mmalicious";
// Result: "User input: ?[31mmalicious" - escape char replaced with ?
```

---

## Constitution Re-Check (Post-Design)

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Faithful Port | ✅ PASS | Matches Python `ANSI.__mod__` exactly |
| II. Immutability | ✅ PASS | Returns new `Ansi` instance |
| III. Layered Architecture | ✅ PASS | No new dependencies |
| VII. Full Scope | ✅ PASS | All FR-001 through FR-007 addressed |
| VIII. Real-World Testing | ✅ PASS | 15 tests with real instances |
| X. File Size | ✅ PASS | Adds ~15 lines; `Ansi.cs` remains under 400 LOC |

**Final Gate Status**: ✅ ALL GATES PASS - Ready for `/speckit.tasks`
