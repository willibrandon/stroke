# Implementation Plan: Layout Containers, UI Controls, and Window Container

**Branch**: `029-layout-containers-controls-window` | **Date**: 2026-01-29 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/029-layout-containers-controls-window/spec.md`

## Summary

Implement the core layout system for Stroke terminal UI: container hierarchy (HSplit, VSplit, FloatContainer), UI controls (BufferControl, FormattedTextControl), and Window container with scrolling, margins, and cursor display. This is a faithful port of Python Prompt Toolkit's `layout/containers.py`, `layout/controls.py`, and `layout/margins.py` modules, providing the foundation for building terminal user interfaces with nested layouts and editable text buffers.

## Technical Context

**Language/Version**: C# 13 / .NET 10
**Primary Dependencies**: Stroke.Core (Document, Buffer, Primitives, Filters, Caches), Stroke.FormattedText (StyleAndTextTuple, AnyFormattedText), Stroke.KeyBinding (IKeyBindingsBase, KeyPressEvent), Stroke.Input (Keys, MouseEvent), Stroke.Lexers (ILexer), Stroke.Layout (Dimension, Screen, Char, WritePosition, MouseHandlers)
**Storage**: N/A (in-memory only - scroll state, render caches)
**Testing**: xUnit (no mocks per Constitution VIII)
**Target Platform**: Cross-platform (.NET 10+)
**Project Type**: Single project (Stroke library)
**Performance Goals**:
- Render 50 containers in <16ms (60fps capable per SC-002)
- BufferControl with 10,000+ lines scrolls smoothly (<16ms per SC-003)
- Nested layouts to 10 levels deep without degradation (SC-001)
**Constraints**:
- 100% API fidelity to Python Prompt Toolkit (Constitution I)
- Thread-safe mutable state with Lock (Constitution XI)
- 80% test coverage (Constitution VIII)
- Source files ≤1,000 LOC (Constitution X)
**Scale/Scope**: ~25 classes/interfaces, ~500 tests estimated

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Faithful Port | ✅ PASS | All APIs extracted from Python source. Spec defines 43 FRs mapping to Python PTK |
| II. Immutability | ✅ PASS | UIContent, Float, ScrollOffsets, ColorColumn immutable. Mutable state (Window scroll, caches) encapsulated with Lock |
| III. Layered Architecture | ✅ PASS | Stroke.Layout depends on Core, Rendering (Screen/Char). No circular deps |
| IV. Cross-Platform | ✅ PASS | Pure C# implementation, no platform-specific code. Uses UnicodeWidth for CJK |
| V. Editing Mode Parity | N/A | Not applicable to layout system |
| VI. Performance | ✅ PASS | Sparse screen storage exists. Weighted allocation algorithm ported. Caching planned |
| VII. Full Scope | ✅ PASS | All 43 FRs and 10 user stories covered |
| VIII. Real-World Testing | ✅ PASS | No mocks. Tests use real Screen, Buffer, containers |
| IX. Planning Documents | ✅ PASS | api-mapping.md section for layout consulted and followed |
| X. File Size Limits | ✅ PASS | Window class split into partials if needed. Tests split by user story |
| XI. Thread Safety | ✅ PASS | Window scroll state, caches protected by Lock |
| XII. Contracts in Markdown | ✅ PASS | API contracts in data-model.md markdown, not .cs files |

## Project Structure

### Documentation (this feature)

```text
specs/029-layout-containers-controls-window/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output (markdown only)
└── tasks.md             # Phase 2 output (/speckit.tasks command)
```

### Source Code (repository root)

```text
src/Stroke/Layout/
├── Containers/
│   ├── IContainer.cs             # Container interface
│   ├── ContainerUtils.cs         # ToContainer, ToWindow, IsContainer
│   ├── AnyContainer.cs           # Union type with implicit conversions
│   ├── HSplit.cs                 # Vertical stacking container
│   ├── VSplit.cs                 # Horizontal arrangement container
│   ├── FloatContainer.cs         # Background + floating overlays
│   ├── Float.cs                  # Float positioning definition
│   ├── ConditionalContainer.cs   # Filter-based visibility
│   ├── DynamicContainer.cs       # Runtime content switching
│   └── Enums/
│       ├── HorizontalAlign.cs    # Left, Center, Right, Justify
│       ├── VerticalAlign.cs      # Top, Center, Bottom, Justify
│       └── WindowAlign.cs        # Left, Center, Right
├── Controls/
│   ├── IUIControl.cs             # UI control interface
│   ├── UIContent.cs              # Control rendering output
│   ├── DummyControl.cs           # Empty placeholder
│   ├── FormattedTextControl.cs   # Static styled text display
│   ├── BufferControl.cs          # Editable buffer display
│   └── SearchBufferControl.cs    # Search input specialized control
├── Windows/
│   ├── Window.cs                 # Container wrapping UIControl
│   ├── Window.Scroll.cs          # Scroll algorithms (partial)
│   ├── Window.Render.cs          # Rendering pipeline (partial)
│   ├── WindowRenderInfo.cs       # Render state and line mappings
│   ├── ScrollOffsets.cs          # Scroll behavior config
│   └── ColorColumn.cs            # Column highlighting config
├── Margins/
│   ├── IMargin.cs                # Margin interface
│   ├── NumberedMargin.cs         # Line numbers
│   ├── ScrollbarMargin.cs        # Vertical scrollbar
│   ├── ConditionalMargin.cs      # Filter-based visibility
│   └── PromptMargin.cs           # Prompt/continuation display
├── Dimension.cs                  # (existing)
├── DimensionUtils.cs             # (existing)
├── Screen.cs                     # (existing)
├── Char.cs                       # (existing)
├── WritePosition.cs              # (existing)
├── MouseHandlers.cs              # (existing)
└── IWindow.cs                    # (existing - marker interface)

tests/Stroke.Tests/Layout/
├── Containers/
│   ├── IContainerTests.cs
│   ├── HSplitTests.cs
│   ├── VSplitTests.cs
│   ├── FloatContainerTests.cs
│   ├── FloatTests.cs
│   ├── ConditionalContainerTests.cs
│   ├── DynamicContainerTests.cs
│   └── ContainerUtilsTests.cs
├── Controls/
│   ├── UIContentTests.cs
│   ├── DummyControlTests.cs
│   ├── FormattedTextControlTests.cs
│   ├── BufferControlTests.cs
│   └── SearchBufferControlTests.cs
├── Windows/
│   ├── WindowTests.cs
│   ├── WindowScrollTests.cs
│   ├── WindowRenderTests.cs
│   ├── WindowRenderInfoTests.cs
│   ├── ScrollOffsetsTests.cs
│   └── ColorColumnTests.cs
└── Margins/
    ├── NumberedMarginTests.cs
    ├── ScrollbarMarginTests.cs
    ├── ConditionalMarginTests.cs
    └── PromptMarginTests.cs
```

**Structure Decision**: Single project structure. Layout classes organized into Containers, Controls, Windows, and Margins subdirectories matching Python PTK module organization. Window class uses C# partial classes to stay under 1,000 LOC limit.

## Complexity Tracking

> **No violations requiring justification**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| - | - | - |

## Dependencies

### Required (already implemented)

| Dependency | Location | Status |
|------------|----------|--------|
| Screen | `Stroke.Layout.Screen` | ✅ Feature 028 |
| Char | `Stroke.Layout.Char` | ✅ Feature 028 |
| WritePosition | `Stroke.Layout.WritePosition` | ✅ Feature 028 |
| Dimension | `Stroke.Layout.Dimension` | ✅ Feature 016 |
| DimensionUtils | `Stroke.Layout.DimensionUtils` | ✅ Feature 016 |
| IWindow | `Stroke.Layout.IWindow` | ✅ Feature 028 (marker) |
| MouseHandlers | `Stroke.Layout.MouseHandlers` | ✅ Feature 013 |
| Buffer | `Stroke.Core.Buffer` | ✅ Feature 007 |
| Document | `Stroke.Core.Document` | ✅ Feature 002 |
| IFilter/FilterOrBool | `Stroke.Filters` | ✅ Feature 017 |
| StyleAndTextTuple | `Stroke.FormattedText` | ✅ Feature 015 |
| AnyFormattedText | `Stroke.FormattedText` | ✅ Feature 015 |
| IKeyBindingsBase | `Stroke.KeyBinding` | ✅ Feature 022 |
| ILexer | `Stroke.Lexers` | ✅ Feature 025 |
| UnicodeWidth | `Stroke.Core.UnicodeWidth` | ✅ Feature 024 |
| CollectionUtils | `Stroke.Core.CollectionUtils` | ✅ Feature 024 |
| SimpleCache | `Stroke.Core.SimpleCache` | ✅ Feature 006 |
| Point | `Stroke.Core.Primitives.Point` | ✅ Feature 001 |
| SearchState | `Stroke.Core.SearchState` | ✅ Feature 010 |

### Out of Scope (future features)

| Component | Notes |
|-----------|-------|
| Layout (manager class) | Feature 030+ (orchestrates full layout lifecycle) |
| Processor (input processors) | Feature 030+ (HighlightSearchProcessor, etc.) |
| CompletionsMenu | Feature 031+ (menus layer) |
| ScrollablePane | Feature 031+ (complex scrolling container) |

## Key Design Decisions

### 1. Container Interface vs Abstract Class

**Decision**: `IContainer` interface (not abstract class)

**Rationale**: Python uses abstract metaclass but C# interfaces provide cleaner composition. Allows Window to implement IContainer while also being a concrete class with substantial implementation.

### 2. Window Partial Classes

**Decision**: Split Window into Window.cs, Window.Scroll.cs, Window.Render.cs

**Rationale**: Window in Python PTK is ~1200 lines. Constitution X requires ≤1000 LOC. Partial classes maintain single logical type while respecting size limits.

### 3. Float Z-Index Handling

**Decision**: Use existing Screen.DrawWithZIndex/DrawAllFloats infrastructure

**Rationale**: Feature 028 implemented deferred z-index drawing. FloatContainer leverages this for cursor-relative float positioning.

### 4. UIContent Immutability

**Decision**: UIContent is immutable (readonly properties, no public setters)

**Rationale**: UIContent represents a snapshot of control output for a render frame. Immutability enables safe caching and concurrent access.

### 5. Size Division Algorithm

**Decision**: Port Python's weighted allocation algorithm using CollectionUtils.TakeUsingWeights

**Rationale**: Feature 024 already implemented TakeUsingWeights. HSplit/VSplit use this for faithful port of size division.

### 6. BufferControl Default Processors

**Decision**: Defer input processor implementation to future feature

**Rationale**: BufferControl.IncludeDefaultInputProcessors will be a no-op initially. Processor system (HighlightSearchProcessor, etc.) is complex and warrants separate feature. BufferControl still functional without processors - just no search/selection highlighting.

### 7. Margin Interface

**Decision**: `IMargin` interface with GetWidth and CreateMargin methods

**Rationale**: Mirrors Python PTK's Margin abstract class. Enables NumberedMargin, ScrollbarMargin, etc. to be composable.
