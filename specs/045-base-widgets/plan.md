# Implementation Plan: Base Widgets

**Branch**: `045-base-widgets` | **Date**: 2026-02-01 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/045-base-widgets/spec.md`

## Summary

Implement 15 reusable widget classes ported from Python Prompt Toolkit's `widgets/base.py` and `widgets/dialogs.py`. These widgets compose existing infrastructure (Buffer, BufferControl, Window, HSplit, VSplit, FloatContainer, ConditionalContainer, DynamicContainer, FormattedTextControl) into high-level UI building blocks: TextArea, Label, Button, Frame, Shadow, Box, RadioList, CheckboxList, Checkbox, ProgressBar, VerticalLine, HorizontalLine, Dialog, and a Border constants class. All widgets implement `IMagicContainer` to integrate with the layout system.

## Technical Context

**Language/Version**: C# 13 / .NET 10+
**Primary Dependencies**: Stroke.Core (Buffer, Document), Stroke.Layout (Window, HSplit, VSplit, FloatContainer, ConditionalContainer, DynamicContainer, Float), Stroke.Layout.Controls (FormattedTextControl, BufferControl), Stroke.Layout.Margins (ScrollbarMargin, NumberedMargin, ConditionalMargin), Stroke.Layout.Processors (PasswordProcessor, AppendAutoSuggestion, BeforeInput, ConditionalProcessor), Stroke.Completion (DynamicCompleter), Stroke.Validation (DynamicValidator), Stroke.AutoSuggest (DynamicAutoSuggest), Stroke.Lexers (DynamicLexer), Stroke.FormattedText (AnyFormattedText, Template, FormattedTextUtils), Stroke.Filters (IFilter, Condition, FilterOrBool), Stroke.KeyBinding (KeyBindings), Stroke.Input (Keys), Stroke.Widgets.Toolbars (SearchToolbar)
**Storage**: N/A (in-memory only)
**Testing**: xUnit v3 (no mocks, no FluentAssertions per Constitution VIII)
**Target Platform**: Linux, macOS, Windows 10+ (cross-platform terminal)
**Project Type**: Single project (existing `src/Stroke/Stroke.csproj` + `tests/Stroke.Tests/Stroke.Tests.csproj`)
**Performance Goals**: Widgets are thin composition wrappers; no hot paths. Lazy evaluation for dynamic properties.
**Constraints**: File size ≤ 1000 LOC per Constitution X. Thread safety for mutable state per Constitution XI.
**Scale/Scope**: 15 widget classes, ~2000-2500 LOC implementation + ~1500-2000 LOC tests

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| # | Principle | Status | Notes |
|---|-----------|--------|-------|
| I | Faithful Port (100% API Fidelity) | ✅ PASS | All 15 classes map 1:1 to Python source. APIs verified against `widgets/base.py` (1081 lines) and `widgets/dialogs.py` (109 lines). Naming follows `snake_case` → `PascalCase`. |
| II | Immutability by Default | ✅ PASS | Border is a static class (immutable). Widget instances hold mutable state (current values, selected index) matching Python behavior. No core data structures affected. |
| III | Layered Architecture | ✅ PASS | Widgets live in `Stroke.Widgets` namespace. Depend on Layout, Core, KeyBinding, Filters, Completion, Validation, AutoSuggest, Lexers — all lower layers. No circular deps. |
| IV | Cross-Platform Compatibility | ✅ PASS | Widgets compose containers; no direct terminal I/O. Cross-platform handled by lower layers. |
| V | Editing Mode Parity | ✅ PASS | TextArea supports both modes via Buffer. DialogList has Vi-like `j`/`k` navigation. |
| VI | Performance-Conscious Design | ✅ PASS | Widgets are thin wrappers. Dynamic evaluation via lambdas/Condition for lazy properties. |
| VII | Full Scope Commitment | ✅ PASS | All 15 classes, all 24 functional requirements, all 14 edge cases will be implemented. |
| VIII | Real-World Testing | ✅ PASS | Tests use xUnit with real widget instances. No mocks. |
| IX | Adherence to Planning Documents | ✅ PASS | API mapping consulted (lines 2381-2502). Test mapping consulted (lines 708-726). |
| X | Source Code File Size Limits | ✅ PASS | Planned file split: 15 source files (10 Base/ + 4 Lists/ + 1 Dialogs/) — one class per file, all ≤ 1000 LOC. |
| XI | Thread Safety | ✅ PASS | DialogList has mutable `_selectedIndex`, `CurrentValue`, `CurrentValues` — will use Lock. ProgressBar has mutable `_percentage`. Other widgets delegate to thread-safe lower layers. |
| XII | Contracts in Markdown Only | ✅ PASS | All contracts in `contracts/*.md`. |

**GATE RESULT: ✅ ALL PASS — proceed to Phase 0**

## Project Structure

### Documentation (this feature)

```text
specs/045-base-widgets/
├── plan.md              # This file
├── research.md          # Phase 0: dependency analysis and design decisions
├── data-model.md        # Phase 1: entity model
├── quickstart.md        # Phase 1: implementation guide
├── contracts/           # Phase 1: API contracts in markdown
│   ├── border.md
│   ├── text-area.md
│   ├── label.md
│   ├── button.md
│   ├── frame.md
│   ├── shadow.md
│   ├── box.md
│   ├── dialog-list.md
│   ├── radio-list.md
│   ├── checkbox-list.md
│   ├── checkbox.md
│   ├── progress-bar.md
│   ├── line-widgets.md
│   └── dialog.md
└── tasks.md             # Phase 2 output (/speckit.tasks command)
```

### Source Code (repository root)

```text
src/Stroke/Widgets/
├── Toolbars/                    # Already exists from Feature 044
│   ├── ArgToolbar.cs
│   ├── CompletionsToolbar.cs
│   ├── CompletionsToolbarControl.cs
│   ├── FormattedTextToolbar.cs
│   ├── SearchToolbar.cs
│   ├── SystemToolbar.cs
│   └── ValidationToolbar.cs
├── Base/                        # NEW — base widget classes
│   ├── Border.cs                # Static box-drawing constants
│   ├── TextArea.cs              # High-level text input widget
│   ├── Label.cs                 # Static text display widget
│   ├── Button.cs                # Clickable button widget
│   ├── Frame.cs                 # Border decorator with optional title
│   ├── Shadow.cs                # Shadow effect decorator
│   ├── Box.cs                   # Padding decorator
│   ├── ProgressBar.cs           # Percentage display
│   ├── VerticalLine.cs          # Vertical separator
│   └── HorizontalLine.cs       # Horizontal separator
├── Lists/                       # NEW — selection list widgets
│   ├── DialogList.cs            # DialogList<T> base class
│   ├── RadioList.cs             # Single-selection list
│   ├── CheckboxList.cs          # Multi-selection list
│   └── Checkbox.cs              # Single checkbox convenience wrapper
└── Dialogs/                     # NEW — dialog composition
    └── Dialog.cs                # Dialog window composing Frame+Shadow+Box+Buttons

tests/Stroke.Tests/Widgets/
├── Base/
│   ├── BorderTests.cs
│   ├── TextAreaTests.cs
│   ├── LabelTests.cs
│   ├── ButtonTests.cs
│   ├── FrameTests.cs
│   ├── ShadowTests.cs
│   ├── BoxTests.cs
│   ├── ProgressBarTests.cs
│   └── LineWidgetTests.cs
├── Lists/
│   ├── DialogListTests.cs
│   ├── RadioListTests.cs
│   ├── CheckboxListTests.cs
│   └── CheckboxTests.cs
└── Dialogs/
    └── DialogTests.cs
```

**Structure Decision**: Widgets are organized into three subdirectories under `src/Stroke/Widgets/` — `Base/` for foundational widgets, `Lists/` for selection widgets, and `Dialogs/` for dialog composition. This mirrors Python PTK's logical grouping while keeping files small per Constitution X.

## Complexity Tracking

> No violations. All principles pass without justification needed.

## Key Design Decisions

### 1. IMagicContainer vs IContainer

All widgets implement `IMagicContainer` (C# equivalent of Python's `__pt_container__` protocol), not `IContainer` directly. This matches the Python pattern where widgets have a `__pt_container__()` method that returns the inner container. The `AnyContainer` union type handles the conversion transparently.

### 2. Constructor Parameter Adaptation

Python uses `FilterOrBool` for many parameters (multiline, password, read_only, etc.) with runtime evaluation. The Stroke `Buffer` constructor already accepts `Func<bool>?` for `readOnly` and `multiline`. Widget constructors will accept `FilterOrBool` and convert to `Func<bool>` via `Condition` lambdas, matching the Python pattern:
```csharp
// Python: read_only=Condition(lambda: is_true(self.read_only))
// C#:     readOnly: () => FilterUtils.IsTrue(ReadOnly)
```

### 3. Window Constructor Adaptation

The Stroke `Window` constructor takes `Dimension?` for width/height (not callables). For widgets needing dynamic dimensions (e.g., Label's width calculation, ProgressBar's weighted bars), use `Func<Dimension?>` getters via Window constructor overloads, or pre-compute Dimension values. Where Python passes callables, use the pattern already established in the codebase.

### 4. Thread Safety Scope

Only `DialogList<T>` (mutable `_selectedIndex`, `CurrentValue`, `CurrentValues`) and `ProgressBar` (mutable `_percentage`) require Lock synchronization. Other widgets (TextArea, Label, Button, Frame, Shadow, Box, Dialog) delegate state to already thread-safe lower layers (Buffer, Window).

### 5. File Organization and 1000 LOC Limit

The Python `base.py` is 1081 lines containing 14 classes. Splitting into subdirectories (`Base/`, `Lists/`, `Dialogs/`) with one class per file ensures all files stay well under 1000 LOC. The largest class (DialogList) is ~200 lines in Python, which will expand to ~300-400 lines in C# with XML docs and thread safety.
