# Implementation Plan: Input Processors

**Branch**: `031-input-processors` | **Date**: 2026-01-29 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/031-input-processors/spec.md`

## Summary

Implement the processor pipeline system that transforms styled text fragments (`StyleAndTextTuple` lists) before `BufferControl` renders them to the screen. This involves porting Python Prompt Toolkit's `layout/processors.py` (20+ processor types) and `layout/utils.py` (`explode_text_fragments`) to C# with 100% API fidelity. The system provides an `IProcessor` interface, `TransformationInput`/`Transformation` data carriers, bidirectional position mapping composition, and concrete processors for search highlighting, password masking, tab expansion, whitespace visualization, bracket matching, and more.

## Technical Context

**Language/Version**: C# 13 / .NET 10
**Primary Dependencies**: Stroke.Core (Document, Buffer, SimpleCache, ConversionUtils), Stroke.FormattedText (StyleAndTextTuple, AnyFormattedText, FormattedTextUtils), Stroke.Filters (IFilter, FilterOrBool, Condition), Stroke.KeyBinding (ViState, InputMode, KeyProcessor), Stroke.Layout (BufferControl, SearchBufferControl, Layout, Window, UIContent), Stroke.Application (Application, AppContext, AppFilters), Stroke.Input (MouseEvent)
**Storage**: N/A (in-memory only — fragment transformation caches)
**Testing**: xUnit (no mocks, no FluentAssertions per Constitution VIII)
**Target Platform**: Linux, macOS, Windows 10+ (cross-platform)
**Project Type**: Single project (Stroke library + Stroke.Tests)
**Performance Goals**: Processors run per-line per-render-cycle; must not regress rendering performance
**Constraints**: Thread safety required for mutable state per Constitution XI; file size ≤1000 LOC per Constitution X
**Scale/Scope**: ~20 processor types, ~37 functional requirements, ~1 utility class (ExplodedList + ExplodeTextFragments)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Faithful Port (100% API Fidelity) | ✅ PASS | All 20+ processor types from Python `layout/processors.py` are mapped 1:1. One documented deviation: fix Python typo `"class:training-whitespace"` → `"class:trailing-whitespace"` (clarification session 2026-01-29). |
| II. Immutability by Default | ✅ PASS | `TransformationInput` is immutable (readonly properties). `Transformation` is immutable (readonly properties). `StyleAndTextTuple` is already `readonly record struct`. `ExplodedList` is a mutable collection by design (matching Python's `_ExplodedList`). |
| III. Layered Architecture | ✅ PASS | Processors live in `Stroke.Layout` namespace (layer 5). They depend on Core (layer 1), FormattedText (sub of Core), Filters (sub of Core), and Application (layer 7 — only for `AppContext.GetApp()` in processors that query app state). The Application dependency is the same pattern Python uses (`get_app()`). |
| IV. Cross-Platform Terminal Compatibility | ✅ PASS | No platform-specific code. `ShowLeadingWhiteSpaceProcessor` and `ShowTrailingWhiteSpaceProcessor` use encoding-aware character fallbacks (middot vs dot), matching Python. |
| V. Complete Editing Mode Parity | ✅ PASS | `DisplayMultipleCursors` supports Vi block insert mode. `HighlightSearchProcessor`/`HighlightIncrementalSearchProcessor` support both Emacs and Vi search. |
| VI. Performance-Conscious Design | ✅ PASS | `HighlightMatchingBracketProcessor` uses `SimpleCache` for position caching. `ExplodeTextFragments` is idempotent (checks flag to skip re-explosion). Position mappings are closures (no allocation beyond initial setup). |
| VII. Full Scope Commitment | ✅ PASS | All 37 functional requirements, all 20+ processor types, all 11 edge cases addressed. |
| VIII. Real-World Testing | ✅ PASS | xUnit tests with real processor instances. No mocks. Target ≥80% coverage. |
| IX. Adherence to Planning Documents | ✅ PASS | `api-mapping.md` does not yet contain layout.processors entries (module not yet mapped). This plan defines the mapping. |
| X. Source Code File Size Limits | ✅ PASS | Python `processors.py` is 1017 lines. C# port will be split into multiple files: core types (~100 LOC), individual processor files (~50-150 LOC each). |
| XI. Thread Safety by Default | ✅ PASS | `HighlightMatchingBracketProcessor` has mutable cache — needs Lock. `ExplodedList` is not shared across threads (created per-line per-render). Other processors are stateless or use only constructor-injected immutable state. |
| XII. Contracts in Markdown Only | ✅ PASS | All contracts in `contracts/` directory as `.md` files. |

**Pre-design gate: PASSED** — no violations.

## Project Structure

### Documentation (this feature)

```text
specs/031-input-processors/
├── spec.md              # Feature specification
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
│   ├── processor-interfaces.md
│   ├── transformation-types.md
│   ├── concrete-processors.md
│   ├── utility-types.md
│   └── prerequisite-changes.md
└── tasks.md             # Phase 2 output (/speckit.tasks)
```

### Source Code (repository root)

```text
src/Stroke/Layout/
├── Processors/                      # NEW: Processor implementations
│   ├── IProcessor.cs                # Interface + SourceToDisplay/DisplayToSource delegates
│   ├── TransformationInput.cs       # Input data carrier
│   ├── Transformation.cs            # Output data carrier
│   ├── DummyProcessor.cs            # No-op processor
│   ├── PasswordProcessor.cs         # Password masking
│   ├── HighlightSearchProcessor.cs  # Search match highlighting
│   ├── HighlightIncrementalSearchProcessor.cs  # Incremental search highlighting
│   ├── HighlightSelectionProcessor.cs  # Selection highlighting
│   ├── HighlightMatchingBracketProcessor.cs  # Bracket matching
│   ├── DisplayMultipleCursors.cs    # Vi block insert cursors
│   ├── BeforeInput.cs               # Prefix text insertion
│   ├── ShowArg.cs                   # Arg display (extends BeforeInput)
│   ├── AfterInput.cs                # Suffix text insertion
│   ├── AppendAutoSuggestion.cs      # Auto-suggestion display
│   ├── ShowLeadingWhiteSpaceProcessor.cs   # Leading whitespace visualization
│   ├── ShowTrailingWhiteSpaceProcessor.cs  # Trailing whitespace visualization
│   ├── TabsProcessor.cs             # Tab expansion with position mapping
│   ├── ReverseSearchProcessor.cs    # Reverse search display
│   ├── ConditionalProcessor.cs      # Filter-based conditional
│   ├── DynamicProcessor.cs          # Runtime processor selection
│   └── ProcessorUtils.cs            # MergeProcessors + _MergedProcessor
├── ExplodedList.cs                  # NEW: Auto-exploding fragment list
├── LayoutUtils.cs                   # EXISTING: Add ExplodeTextFragments method
├── Controls/
│   ├── BufferControl.cs             # MODIFY: Add InputProcessors, SearchState, etc.
│   └── SearchBufferControl.cs       # EXISTING: No changes needed
└── Layout.cs                        # MODIFY: Add SearchTargetBufferControl property

src/Stroke/Application/
└── AppFilters.cs                    # MODIFY: Add ViInsertMultipleMode filter

tests/Stroke.Tests/Layout/
└── Processors/                      # NEW: Test directory
    ├── ProcessorCoreTests.cs        # IProcessor, TransformationInput, Transformation
    ├── PasswordProcessorTests.cs
    ├── HighlightSearchProcessorTests.cs
    ├── HighlightSelectionProcessorTests.cs
    ├── HighlightMatchingBracketProcessorTests.cs
    ├── DisplayMultipleCursorsTests.cs
    ├── BeforeInputTests.cs
    ├── AfterInputTests.cs
    ├── AppendAutoSuggestionTests.cs
    ├── WhitespaceProcessorTests.cs  # Leading + Trailing
    ├── TabsProcessorTests.cs
    ├── ReverseSearchProcessorTests.cs
    ├── ConditionalDynamicProcessorTests.cs
    ├── MergeProcessorsTests.cs
    ├── ExplodedListTests.cs          # ExplodedList + ExplodeTextFragments
    └── ExplodeTextFragmentsTests.cs   # (may be combined with ExplodedListTests)
```

**Structure Decision**: Processors live under `src/Stroke/Layout/Processors/` as a new subdirectory, mirroring the Python `layout.processors` module. Each processor gets its own file to comply with the 1000 LOC limit. Tests mirror this structure under `tests/Stroke.Tests/Layout/Processors/`. The `ExplodedList` utility lives in the `Layout/` root (mirroring Python's `layout/utils.py`). Prerequisite changes to `BufferControl`, `Layout`, and `AppFilters` are minimal additions to existing files.

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| Application dependency from Layout layer | Processors like `HighlightSearchProcessor`, `ShowArg`, `HighlightMatchingBracketProcessor`, and `ShowLeadingWhiteSpaceProcessor` call `AppContext.GetApp()` to check `IsDone`, `RenderCounter`, `KeyProcessor.Arg`, and `Output.Encoding`. This mirrors Python's `get_app()` pattern. | Injecting app state via `TransformationInput` would deviate from Python API. The cross-layer call is the faithful port approach. |
| Documented deviation: Fix trailing whitespace typo | Python uses `"class:training-whitespace"` (missing 'l'). C# uses `"class:trailing-whitespace"`. | User decision from clarification session 2026-01-29. |
