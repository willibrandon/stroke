# Implementation Plan: HTML Formatted Text

**Branch**: `019-html-formatted-text` | **Date**: 2026-01-26 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/019-html-formatted-text/spec.md`

## Summary

Implement HTML-like markup parsing for styled formatted text, allowing users to write styled content using familiar XML-like syntax. The feature ports Python Prompt Toolkit's `HTML` class to C#, providing:
- Basic formatting elements (`<b>`, `<i>`, `<u>`, `<s>`)
- Color attributes (`fg`, `bg`, `color`)
- Custom element names as style classes
- Nested element support with style accumulation
- Safe string formatting with HTML escaping

**Current Status**: Implementation is ~95% complete. The `Html` class, `HtmlFormatter`, and comprehensive tests already exist. The remaining gap is the `%` operator overload required by FR-016.

## Technical Context

**Language/Version**: C# 13 / .NET 10
**Primary Dependencies**: None (Stroke.FormattedText layer - zero external dependencies per Constitution III)
**Storage**: N/A (in-memory parsing only)
**Testing**: xUnit (no mocks, no FluentAssertions per Constitution VIII)
**Target Platform**: Linux, macOS, Windows 10+ (cross-platform)
**Project Type**: Single library project
**Performance Goals**: N/A (string parsing is not a hot path)
**Constraints**: N/A
**Scale/Scope**: Single class addition to existing FormattedText namespace

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Faithful Port | ✅ PASS | Porting Python Prompt Toolkit's `HTML` class exactly |
| II. Immutability | ✅ PASS | `Html` class is immutable, stores `ImmutableArray<StyleAndTextTuple>` |
| III. Layered Architecture | ✅ PASS | Lives in Stroke.FormattedText, depends only on Core types |
| IV. Cross-Platform | ✅ PASS | No platform-specific code; uses `System.Xml.Linq` |
| V. Editing Mode Parity | N/A | Not related to editing modes |
| VI. Performance | ✅ PASS | Parsing happens once at construction; no differential rendering concerns |
| VII. Full Scope | ✅ PASS | All 34 functional requirements will be implemented |
| VIII. Real-World Testing | ✅ PASS | Tests use xUnit with standard assertions, no mocks |
| IX. Planning Documents | ✅ PASS | Following api-mapping.md for Python→C# naming |
| X. File Size Limits | ✅ PASS | Html.cs is ~200 LOC, HtmlFormatter.cs is ~95 LOC |
| XI. Thread Safety | ✅ PASS | `Html` is immutable (inherently thread-safe) |

## Project Structure

### Documentation (this feature)

```text
specs/019-html-formatted-text/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output (markdown API contracts)
└── tasks.md             # Phase 2 output (/speckit.tasks command)
```

### Source Code (repository root)

```text
src/Stroke/FormattedText/
├── Html.cs              # [EXISTS] Main class (~200 LOC) - needs % operator
├── HtmlFormatter.cs     # [EXISTS] Internal formatter (~95 LOC) - complete
├── FormattedText.cs     # [EXISTS] Base class for styled text
├── StyleAndTextTuple.cs # [EXISTS] (style, text) record struct
└── IFormattedText.cs    # [EXISTS] Interface for formatted text

tests/Stroke.Tests/FormattedText/
├── HtmlTests.cs         # [EXISTS] 43 tests (~460 LOC) - needs % operator tests
└── HtmlFormatterTests.cs # [NEEDED] Dedicated formatter tests (if not covered)
```

**Structure Decision**: Uses existing single-project structure. Html class extends the FormattedText namespace following the established pattern from Feature 015 (FormattedText System).

## Implementation Gap Analysis

### Existing Implementation (Complete)

| Component | File | Status | LOC |
|-----------|------|--------|-----|
| `Html` class | `Html.cs` | ✅ Complete* | ~200 |
| `HtmlFormatter` | `HtmlFormatter.cs` | ✅ Complete | ~95 |
| `Html` tests | `HtmlTests.cs` | ✅ Complete* | ~460 |

*Missing `%` operator (FR-016)

### Missing Implementation (Gap)

| Requirement | Gap | Effort |
|-------------|-----|--------|
| FR-016: `%` operator | `Html` class missing `operator %` overload | Small |
| User Story 5, Scenario 3 | Test for `%` operator not present | Small |

### Gap Resolution

The `HtmlFormatter.FormatPercent` method already exists but is not exposed via an operator on `Html`. Need to add:

```csharp
// In Html.cs
public static Html operator %(Html html, object value) =>
    new(HtmlFormatter.FormatPercent(html.Value, value));

public static Html operator %(Html html, object[] values) =>
    new(HtmlFormatter.FormatPercent(html.Value, values));
```

## Complexity Tracking

No constitution violations. Implementation is straightforward with minimal remaining work.
