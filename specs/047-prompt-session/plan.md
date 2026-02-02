# Implementation Plan: Prompt Session

**Branch**: `047-prompt-session` | **Date**: 2026-02-01 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/047-prompt-session/spec.md`

## Summary

Implement `PromptSession<TResult>`, `PromptFunctions` static class, and `CompleteStyle` enum — the high-level prompt API that provides GNU Readline-like terminal input. This is the primary entry point for most Stroke users, composing Buffer, Layout, Application, History, KeyBindings, and all widget/toolbar subsystems into a cohesive prompt experience. The implementation is a faithful port of Python Prompt Toolkit's `prompt_toolkit.shortcuts.prompt` module (1,538 lines).

## Technical Context

**Language/Version**: C# 13 / .NET 10
**Primary Dependencies**: Stroke.Application, Stroke.Core (Buffer, Document), Stroke.Layout (HSplit, FloatContainer, Window, ConditionalContainer), Stroke.Layout.Controls (BufferControl, FormattedTextControl, SearchBufferControl), Stroke.Layout.Menus (CompletionsMenu, MultiColumnCompletionsMenu), Stroke.Layout.Processors (7 processor types), Stroke.KeyBinding (KeyBindings, MergedKeyBindings, ConditionalKeyBindings, DynamicKeyBindings), Stroke.Completion (DynamicCompleter, ThreadedCompleter), Stroke.Validation (DynamicValidator), Stroke.AutoSuggest (DynamicAutoSuggest), Stroke.Lexers (DynamicLexer), Stroke.Styles (DynamicStyle, DynamicStyleTransformation, ConditionalStyleTransformation, SwapLightAndDarkStyleTransformation, StyleTransformationMerger), Stroke.Clipboard (DynamicClipboard, InMemoryClipboard), Stroke.CursorShapes (DynamicCursorShapeConfig), Stroke.History (IHistory, InMemoryHistory), Stroke.Widgets (Frame, SearchToolbar, SystemToolbar, ValidationToolbar), Stroke.FormattedText (AnyFormattedText, FormattedTextUtils), Stroke.Filters (IFilter, Condition, FilterOrBool), Stroke.Output (DummyOutput), Stroke.Input (IInput)
**Storage**: N/A (in-memory session state only; history persistence delegated to IHistory implementations)
**Testing**: xUnit (no mocks, no FluentAssertions per Constitution VIII)
**Target Platform**: Linux, macOS, Windows 10+ (cross-platform terminal)
**Project Type**: Single library project (Stroke) + single test project (Stroke.Tests)
**Performance Goals**: <16ms keystroke response, differential rendering for prompt UI
**Constraints**: File size ≤1,000 LOC per Constitution X; thread safety per Constitution XI
**Scale/Scope**: ~8 source files, ~7 test files, 41 functional requirements (FR-001 through FR-041)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Faithful Port (100% API Fidelity) | ✅ PASS | All public APIs from `prompt.py` mapped 1:1. `_fields` tuple replaced by explicit property-by-property updates (C# lacks Python's dynamic `setattr`). |
| II. Immutability by Default | ✅ PASS | `CompleteStyle` is immutable enum. `PromptSession` holds mutable state (by design — matches Python). Document/ClipboardData remain immutable. |
| III. Layered Architecture | ✅ PASS | `Stroke.Shortcuts` is the highest layer (layer 8), depends on Application and all lower layers. No circular dependencies introduced. |
| IV. Cross-Platform Terminal Compatibility | ✅ PASS | Dumb terminal fallback (FR-014) handles degraded environments. All terminal features delegated to existing Output/Input abstractions. |
| V. Complete Editing Mode Parity | ✅ PASS | Prompt key bindings are editing-mode agnostic. Vi/Emacs bindings already complete in layers 4-5. |
| VI. Performance-Conscious Design | ✅ PASS | Layout uses existing differential rendering. Dynamic conditions use lightweight Condition lambdas. No new global state. |
| VII. Full Scope Commitment | ✅ PASS | All 41 FRs, 11 user stories, 10 edge cases will be implemented. No deferrals. |
| VIII. Real-World Testing | ✅ PASS | Tests use real Buffer, Application, Layout — no mocks. xUnit only. |
| IX. Adherence to Planning Documents | ✅ PASS | API mapping consulted: `PromptSession<TResult>`, `CompleteStyle`, `PromptFunctions` all mapped. Test mapping: 6 prompt tests + helpers mapped. |
| X. Source Code File Size ≤1,000 LOC | ✅ PASS | PromptSession split into partial classes across ~6 files. Each file <600 LOC estimated. |
| XI. Thread Safety by Default | ✅ PASS | PromptSession has mutable state (session properties updated per-prompt). Properties protected by Lock. Single active prompt constraint documented. |
| XII. Contracts in Markdown Only | ✅ PASS | All contracts in `contracts/*.md`. No `.cs` contract files. |

**Gate result: PASS** — All 12 principles satisfied. Proceeding to Phase 0.

## Project Structure

### Documentation (this feature)

```text
specs/047-prompt-session/
├── spec.md              # Feature specification (complete)
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
│   ├── complete-style.md
│   ├── prompt-session.md
│   ├── prompt-functions.md
│   └── internal-helpers.md
├── checklists/
│   └── requirements.md  # Quality checklist (complete)
└── tasks.md             # Phase 2 output (created by /speckit.tasks)
```

### Source Code (repository root)

```text
src/Stroke/Shortcuts/
├── CompleteStyle.cs                     # CompleteStyle enum (Column, MultiColumn, ReadlineLike)
├── PromptSession.cs                     # PromptSession<TResult> — constructor, properties, _dyncond
├── PromptSession.Layout.cs              # _CreateLayout, _GetDefaultBufferControlHeight, _SplitMultilinePrompt
├── PromptSession.Buffers.cs             # _CreateDefaultBuffer, _CreateSearchBuffer
├── PromptSession.Application.cs         # _CreateApplication, _CreatePromptBindings, _DumbPrompt
├── PromptSession.Prompt.cs              # Prompt, PromptAsync, _AddPreRunCallables, per-prompt override logic
├── PromptSession.Helpers.cs             # _GetPrompt, _GetContinuation, _GetLinePrefix, _GetArgText, _InlineArg
└── PromptFunctions.cs                   # Static Prompt, PromptAsync, Confirm, ConfirmAsync, CreateConfirmSession

tests/Stroke.Tests/Shortcuts/
├── CompleteStyleTests.cs                # Enum value tests → FR-001
├── PromptSessionTests.cs                # Constructor, property defaults, _dyncond, session reuse → FR-002, FR-003, FR-016, FR-037, US-2, US-6
├── PromptSessionLayoutTests.cs          # Layout construction, multiline prompt splitting, completion menus → FR-006, FR-012, FR-017, FR-025, FR-028, FR-029, US-4, US-7
├── PromptSessionBindingsTests.cs        # Prompt key bindings (Enter, Ctrl-C, Ctrl-D, Tab, Ctrl-Z) → FR-011, FR-027, FR-041, US-1, US-5
├── PromptSessionPromptTests.cs          # Prompt/PromptAsync, per-prompt overrides, dumb terminal, accept default → FR-008..FR-010, FR-014, FR-015, FR-035..FR-040, US-3, US-6, US-8..US-11
├── PromptFunctionsTests.cs              # Static Prompt, Confirm, CreateConfirmSession → FR-019..FR-021, FR-036, US-3, US-5
└── PromptSessionConcurrencyTests.cs     # Thread safety of session property access → Constitution XI, FR-016
```

**Structure Decision**: Extends existing single-project layout. Source files go into `src/Stroke/Shortcuts/` (same namespace as existing `FormattedTextOutput.cs` and `TerminalUtils.cs`). Test files go into `tests/Stroke.Tests/Shortcuts/`. PromptSession is split into 6 partial class files to stay under 1,000 LOC per file (Constitution X), organized by responsibility: core/constructor, layout, buffers, application/bindings, prompt methods, and helper methods.

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| `RefreshInterval` must become settable on Application | Python sets `self.app.refresh_interval = self.refresh_interval` per-prompt call. Current Application has read-only `RefreshInterval { get; }`. | Making it constructor-only would break per-prompt override semantics. Must add a setter with Lock protection. **Cross-feature change**: Requires modifying `Application<TResult>` (Feature 030) to add a public setter. |
| Custom exception types needed | Python uses configurable `interrupt_exception` and `eof_exception` types (default `KeyboardInterrupt`, `EOFError`). Stroke has no custom equivalents. | Using only `OperationCanceledException` / `EndOfStreamException` would break API fidelity — users must be able to configure custom exception types per Constitution I. Default types should be `KeyboardInterruptException` and `EOFException`. |
| `LayoutUtils.ExplodeTextFragments()` dependency | `SplitMultilinePrompt` uses `ExplodeTextFragments` from `Stroke.Layout.LayoutUtils` to split multi-character fragments for accurate newline detection. | Already implemented in Feature 029. Must be accessible from `Stroke.Shortcuts` (higher layer can access lower layers per Constitution III). |
