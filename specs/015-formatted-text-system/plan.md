# Implementation Plan: Formatted Text System

**Branch**: `015-formatted-text-system` | **Date**: 2026-01-25 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/015-formatted-text-system/spec.md`

## Summary

Port Python Prompt Toolkit's `formatted_text` module to C#, enabling HTML-like markup parsing, ANSI escape sequence parsing, styled text fragments, template interpolation, and text merging. The existing minimal implementation (StyleAndTextTuple, FormattedText, AnyFormattedText, FormattedTextUtils) will be extended to support the full Python API including HTML class, ANSI class, Template class, PygmentsTokens class, and complete utility functions.

## Technical Context

**Language/Version**: C# 13 / .NET 10
**Primary Dependencies**: None (Stroke.FormattedText layer - zero external dependencies per Constitution III)
**Storage**: N/A (in-memory data structures only)
**Testing**: xUnit (no mocks, no FluentAssertions per Constitution VIII)
**Target Platform**: Linux, macOS, Windows 10+
**Project Type**: Single library extending existing namespace
**Performance Goals**: <1ms for typical conversions (<10KB), ANSI parsing >10,000 chars/sec
**Constraints**: Thread-safe operations, immutable core types
**Scale/Scope**: Port of `prompt_toolkit.formatted_text` module (5 Python files, ~800 LOC)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

**Pre-Design Status**: ✅ All gates passed (2026-01-25)
**Post-Design Status**: ✅ All gates passed (2026-01-25)

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Faithful Port (100% API Fidelity) | ✅ PASS | All Python APIs from `formatted_text` module will be ported: `HTML`, `ANSI`, `Template`, `PygmentsTokens`, `FormattedText`, `to_formatted_text`, `is_formatted_text`, `merge_formatted_text`, `fragment_list_len`, `fragment_list_width`, `fragment_list_to_text`, `split_lines`, `to_plain_text`, `html_escape`, `ansi_escape` |
| II. Immutability by Default | ✅ PASS | `StyleAndTextTuple` is record struct (immutable), `FormattedText` is sealed with ImmutableArray, `HTML` and `ANSI` parse on construction and cache results |
| III. Layered Architecture | ✅ PASS | Stroke.FormattedText has zero external dependencies; only references Stroke.Core for MouseEvent type |
| IV. Cross-Platform Terminal Compatibility | ✅ PASS | ANSI parser handles all standard SGR codes; `fragment_list_width` will use UnicodeWidth for CJK; no platform-specific code |
| V. Complete Editing Mode Parity | N/A | Not applicable to formatted text system |
| VI. Performance-Conscious Design | ✅ PASS | Lazy parsing in ANSI, cached results in HTML, no global mutable state |
| VII. Full Scope Commitment | ✅ PASS | All 34 functional requirements from spec will be implemented |
| VIII. Real-World Testing | ✅ PASS | Tests will use xUnit with real implementations; no mocks |
| IX. Adherence to Planning Documents | ✅ PASS | Implementation follows `docs/api-mapping.md` section "Module: prompt_toolkit.formatted_text" |
| X. Source Code File Size Limits | ✅ PASS | Each class in separate file; ANSI parser may approach limit but can be split if needed |
| XI. Thread Safety by Default | ✅ PASS | Core types are immutable (inherently thread-safe); parsing produces new instances |

## Project Structure

### Documentation (this feature)

```text
specs/015-formatted-text-system/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output - API contracts in markdown
└── tasks.md             # Phase 2 output (/speckit.tasks command)
```

### Source Code (repository root)

```text
src/Stroke/FormattedText/
├── StyleAndTextTuple.cs     # Existing - extend with mouse handler overload
├── FormattedText.cs         # Existing - add __pt_formatted_text__ equivalent
├── AnyFormattedText.cs      # Existing - add IFormattedText support
├── FormattedTextUtils.cs    # Existing - add missing utilities
├── IFormattedText.cs        # New - interface for __pt_formatted_text__
├── Html.cs                  # New - HTML markup parser
├── HtmlFormatter.cs         # New - HTML string formatter with escaping
├── Ansi.cs                  # New - ANSI escape sequence parser
├── AnsiFormatter.cs         # New - ANSI string formatter with escaping
├── AnsiColors.cs            # New - ANSI color code mappings
├── Template.cs              # New - String interpolation with {} placeholders
└── PygmentsTokens.cs        # New - Pygments token list conversion

tests/Stroke.Tests/FormattedText/
├── StyleAndTextTupleTests.cs     # Existing - extend
├── FormattedTextTests.cs         # Existing - extend
├── AnyFormattedTextTests.cs      # Existing - extend
├── FormattedTextUtilsTests.cs    # Existing - extend
├── HtmlTests.cs                  # New
├── AnsiTests.cs                  # New
├── TemplateTests.cs              # New
└── PygmentsTokensTests.cs        # New
```

**Structure Decision**: Extends existing `Stroke.FormattedText` namespace with new files for HTML, ANSI, Template, and PygmentsTokens classes. Follows single-file-per-class convention with test files mirroring source structure.

## Complexity Tracking

> No violations requiring justification. Design follows Constitution principles.

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| (none) | — | — |
