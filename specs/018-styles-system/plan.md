# Implementation Plan: Styles System

**Branch**: `018-styles-system` | **Date**: 2026-01-26 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/018-styles-system/spec.md`

## Summary

Implement a comprehensive styling system for terminal UI elements that enables developers to define, combine, and transform visual styles for formatted text. The system ports Python Prompt Toolkit's styles module, providing support for ANSI colors, HTML/CSS named colors, hex color codes, style string parsing with class-based rules, style merging with precedence, and post-processing transformations for features like dark mode.

## Technical Context

**Language/Version**: C# 13 / .NET 10
**Primary Dependencies**: None (Stroke.Styles is part of Core layer, zero external dependencies per Constitution III)
**Storage**: N/A (in-memory style definitions and caches only)
**Testing**: xUnit with standard assertions (no mocks per Constitution VIII)
**Target Platform**: Linux, macOS, Windows 10+
**Project Type**: Single .NET library (existing project structure)
**Performance Goals**: Style computation must be fast enough for 60 fps rendering; caching required for repeated lookups
**Constraints**: Thread-safe implementations per Constitution XI; immutable styles per Constitution II
**Scale/Scope**: 140 named colors, 17 ANSI colors + 10 aliases, 8 transformation types, ~50 default UI style rules, ~30 Pygments token rules

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Faithful Port | ✅ PASS | All APIs from `prompt_toolkit.styles` module mapped in `docs/api-mapping.md` lines 1820-1902 |
| II. Immutability by Default | ✅ PASS | Attrs is record struct (immutable), Style is immutable after construction |
| III. Layered Architecture | ✅ PASS | Stroke.Styles in Core layer; depends only on Stroke.Core.Cache (Feature 06) and Stroke.Filters (Feature 17) |
| IV. Cross-Platform | ✅ PASS | Pure C# implementation, no platform-specific code needed |
| V. Editing Mode Parity | N/A | Not applicable to styling system |
| VI. Performance-Conscious | ✅ PASS | Style computation caching via SimpleCache (FR-025) |
| VII. Full Scope | ✅ PASS | All 25 functional requirements will be implemented |
| VIII. Real-World Testing | ✅ PASS | Unit tests with real implementations, no mocks |
| IX. Planning Documents | ✅ PASS | api-mapping.md consulted for Stroke.Styles namespace mapping |
| X. File Size Limits | ✅ PASS | Files will be split as needed to stay under 1,000 LOC |
| XI. Thread Safety | ✅ PASS | Mutable classes will use Lock pattern; immutable types inherently safe |
| XII. Contracts in Markdown | ✅ PASS | All contracts in markdown code blocks only |

## Project Structure

### Documentation (this feature)

```text
specs/018-styles-system/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
└── tasks.md             # Phase 2 output (/speckit.tasks command)
```

### Source Code (repository root)

```text
src/Stroke/
└── Styles/
    ├── Attrs.cs                              # FR-001: Style attributes record struct
    ├── DefaultAttrs.cs                       # FR-002: Default and Empty constants
    ├── AnsiColorNames.cs                     # FR-003: ANSI color names and aliases
    ├── NamedColors.cs                        # FR-004: 140 HTML/CSS named colors
    ├── IStyle.cs                             # FR-005: Style interface
    ├── DummyStyle.cs                         # FR-006: No-op style implementation
    ├── DynamicStyle.cs                       # FR-007: Callable-delegating style
    ├── Priority.cs                           # FR-009: Priority enum for FromDict
    ├── Style.cs                              # FR-008, FR-022-025: Main style class
    ├── StyleParser.cs                        # FR-010: Color parsing utilities
    ├── StyleMerger.cs                        # FR-011: Style merging utilities
    ├── IStyleTransformation.cs               # FR-012: Transformation interface
    ├── DummyStyleTransformation.cs           # FR-013: No-op transformation
    ├── ReverseStyleTransformation.cs         # FR-014: Reverse attribute toggle
    ├── SwapLightAndDarkStyleTransformation.cs # FR-015: Color luminosity inversion
    ├── SetDefaultColorStyleTransformation.cs  # FR-016: Default color fallbacks
    ├── AdjustBrightnessStyleTransformation.cs # FR-017: Brightness constraints
    ├── ConditionalStyleTransformation.cs      # FR-018: Filter-based transformation
    ├── DynamicStyleTransformation.cs          # FR-019: Callable-delegating transformation
    ├── MergedStyleTransformation.cs           # FR-020: Combined transformations (internal)
    ├── StyleTransformationMerger.cs           # FR-020: Transformation merging utilities
    ├── ColorUtils.cs                          # HLS conversion utilities
    ├── DefaultStyles.cs                       # FR-021: Pre-built UI and Pygments styles
    └── PygmentsStyleUtils.cs                  # FR-026-028: Pygments token style utilities

tests/Stroke.Tests/
└── Styles/
    ├── AttrsTests.cs
    ├── AnsiColorNamesTests.cs
    ├── NamedColorsTests.cs
    ├── StyleParserTests.cs
    ├── StyleTests.cs
    ├── StyleMergerTests.cs
    ├── DummyStyleTests.cs
    ├── DynamicStyleTests.cs
    ├── StyleTransformationTests.cs
    ├── DefaultStylesTests.cs
    └── PygmentsStyleUtilsTests.cs
```

**Structure Decision**: Files organized in `Styles/` subdirectory of existing `src/Stroke/` project, following the established pattern for other features (Clipboard/, AutoSuggest/, Filters/, etc.). Test files mirror source structure under `tests/Stroke.Tests/Styles/`.

## Complexity Tracking

> No complexity violations identified. All implementations follow constitutional principles.

## Phase Status

| Phase | Status | Output |
|-------|--------|--------|
| Phase 0: Research | ✅ Complete | [research.md](research.md) |
| Phase 1: Design & Contracts | ✅ Complete | [data-model.md](data-model.md), [contracts/](contracts/), [quickstart.md](quickstart.md) |
| Phase 2: Tasks | ✅ Complete | [tasks.md](tasks.md) |

## Next Steps

Run `/speckit.implement` to begin implementation, or review `tasks.md` and start with Phase 1 (Setup).
