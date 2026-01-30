# Implementation Plan: Application System

**Branch**: `030-application-system` | **Date**: 2026-01-29 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/030-application-system/spec.md`

## Summary

Implement the Application orchestration layer that ties together layout, key bindings, rendering, input processing, and the event loop for interactive terminal applications. This includes `Application<TResult>` (the central class), `AppSession`/`AppContext` (async-local context management), `Layout` (focus/parent tracking wrapper), `KeyProcessor` (key dispatch state machine), `Renderer` (differential screen rendering), `CombinedRegistry` (key binding aggregation), `RunInTerminal` (UI suspension utilities), `DummyApplication` (no-op fallback), and supporting types. Uses `AsyncLocal<T>` for context flow, `TaskCompletionSource<TResult>` for the application future, `PosixSignalRegistration` for Unix signal handling, and `Lock`-based synchronization for thread-safe cross-thread operations.

## Technical Context

**Language/Version**: C# 13 / .NET 10+
**Primary Dependencies**: Stroke.Core (Document, Buffer, Primitives, Caches, Event, UnicodeWidth), Stroke.Input (Keys, IInput, KeyPress, Typeahead), Stroke.Output (IOutput, ColorDepth, CursorShape), Stroke.Styles (IStyle, StyleMerger, DefaultStyles, IStyleTransformation, Attrs), Stroke.Filters (IFilter, FilterOrBool, Condition), Stroke.KeyBinding (IKeyBindingsBase, KeyBindings, Binding, ConditionalKeyBindings, MergedKeyBindings, GlobalOnlyKeyBindings, ViState, EmacsState, EditingMode, KeyPressEvent), Stroke.Layout (IContainer, Window, BufferControl, FormattedTextControl, SearchBufferControl, DummyControl, Screen, Char, WritePosition, Dimension, IUIControl, UIContent, MouseHandlers, IMargin), Stroke.Clipboard (IClipboard, InMemoryClipboard), Stroke.CursorShapes (ICursorShapeConfig), Stroke.FormattedText (AnyFormattedText), Wcwidth NuGet package
**Storage**: N/A (in-memory only — application state, render buffers, key processor queues)
**Testing**: xUnit (no mocks, no FluentAssertions per Constitution VIII)
**Target Platform**: Linux, macOS, Windows 10+ (cross-platform terminal)
**Project Type**: Single project (Stroke library + Stroke.Tests)
**Performance Goals**: <100ms application startup overhead, differential rendering with minimal terminal I/O, single redraw per invalidation cycle under concurrent access
**Constraints**: No source file >1,000 LOC (Constitution X), thread-safe mutable state (Constitution XI), 80% test coverage (Constitution VIII)
**Scale/Scope**: ~20 new source files, ~5 new test files, 39 functional requirements, 12 user stories

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| # | Principle | Status | Notes |
|---|-----------|--------|-------|
| I | Faithful Port (100% API Fidelity) | PASS | All 39 FRs map to Python PTK `application/` module APIs. Contracts match Python signatures adjusted for C# conventions. |
| II | Immutability by Default | PASS | `AppSession` state transitions are managed; `KeyPressEvent` is sealed; `ColorDepthOption` and `FocusableElement` are readonly structs. Mutable state in Application/Layout/Renderer/KeyProcessor is necessary for state management and properly synchronized. |
| III | Layered Architecture | PASS | `Stroke.Application` is layer 7 — depends on all lower layers (Core, Rendering, Input, KeyBinding, Layout, Completion). No circular dependencies. Renderer placed in `Stroke.Rendering`, KeyProcessor in `Stroke.KeyBinding`, Layout class in `Stroke.Layout`. |
| IV | Cross-Platform Terminal Compatibility | PASS | Signal handling uses `PosixSignalRegistration` (Unix) + polling fallback (Windows). Terminal size polling for non-main-thread and Windows. DummyInput/DummyOutput for testing. |
| V | Complete Editing Mode Parity | PASS | Application manages `ViState` and `EmacsState`, `EditingMode` enum. `CombinedRegistry` merges Vi/Emacs bindings. Default binding loaders are stubs initially (actual bindings are separate features). |
| VI | Performance-Conscious Design | PASS | Differential rendering (Renderer compares previous/current Screen). Invalidation deduplication. Throttled redraw interval. Sparse screen storage. Style caching. CombinedRegistry binding cache. |
| VII | Full Scope Commitment | PASS | All 39 FRs, 12 user stories, 18 edge cases covered. No scope reduction. |
| VIII | Real-World Testing | PASS | Tests use real Layout, real Screen, real KeyBindings, pipe input/output. No mocks. xUnit only. Target 80% coverage. |
| IX | Adherence to Planning Documents | PASS | API mapping consulted: `Application<TResult>`, `AppSession`, `DummyApplication`, `get_app()`, `get_app_or_none()`, `set_app()`, `create_app_session()`, `run_in_terminal()`, `in_terminal()` all mapped in `docs/api-mapping.md` lines 88–151. |
| X | Source Code File Size Limits | PASS | Application class split into 5 partial class files. Renderer split into 2 files. Each file targets <500 LOC. |
| XI | Thread Safety by Default | PASS | `Application.Invalidate()` uses Interlocked/Lock. `AppSession` uses Lock for lazy I/O creation. `Layout` uses Lock for focus stack and parent map. Background task set uses Lock. Event class is already thread-safe. |
| XII | Contracts in Markdown Only | PASS | All contracts in `specs/030-application-system/contracts/*.md`. No .cs contract files. |

**Pre-Phase 0 Gate**: ALL PASS ✓

## Project Structure

### Documentation (this feature)

```text
specs/030-application-system/
├── spec.md              # Feature specification
├── plan.md              # This file
├── research.md          # Phase 0 research findings
├── data-model.md        # Entity model and state transitions
├── quickstart.md        # Usage examples
├── contracts/           # API contracts (markdown)
│   ├── application.md   # Application<TResult> class
│   ├── app-context.md   # AppSession, AppContext, RunInTerminal, DummyApplication
│   ├── layout.md        # Layout class, FocusableElement, DummyLayout
│   ├── key-processor.md # KeyProcessor class
│   ├── renderer.md      # Renderer class, RendererUtils
│   └── combined-registry.md # CombinedRegistry, DefaultKeyBindings, AppFilters
├── checklists/
│   └── requirements.md  # Quality checklist
└── tasks.md             # Phase 2 output (created by /speckit.tasks)
```

### Source Code (repository root)

```text
src/Stroke/
├── Application/
│   ├── Application.cs                  # Application<TResult> constructor + properties
│   ├── Application.RunAsync.cs         # RunAsync, Run, _PreRun (partial)
│   ├── Application.Lifecycle.cs        # Reset, Exit, Invalidate, _Redraw (partial)
│   ├── Application.BackgroundTasks.cs  # CreateBackgroundTask, CancelAndWait (partial)
│   ├── Application.SystemCommands.cs   # RunSystemCommand, SuspendToBackground, PrintText (partial)
│   ├── AppSession.cs                   # AppSession class
│   ├── AppContext.cs                   # Static context utilities (GetApp, SetApp, etc.)
│   ├── DummyApplication.cs            # No-op fallback application
│   ├── RunInTerminal.cs               # RunInTerminal static class
│   ├── CombinedRegistry.cs            # Internal key binding aggregator
│   ├── ColorDepthOption.cs            # Color depth union type
│   ├── InputHook.cs                   # InputHook delegate + InputHookContext
│   ├── DefaultKeyBindings.cs          # LoadKeyBindings stubs
│   └── AppFilters.cs                  # Application-aware filters
├── Layout/
│   ├── Layout.cs                      # NEW: Layout focus/parent management
│   ├── LayoutUtils.cs                 # NEW: Walk utility
│   ├── FocusableElement.cs            # NEW: Union type for focusable elements
│   ├── InvalidLayoutException.cs      # NEW: Validation exception
│   └── DummyLayout.cs                 # NEW: Default layout factory
├── Rendering/
│   ├── Renderer.cs                    # NEW: Renderer class
│   ├── Renderer.Diff.cs              # NEW: Screen diff algorithm (partial)
│   └── RendererUtils.cs              # NEW: PrintFormattedText utility
└── KeyBinding/
    └── KeyProcessor.cs                # NEW: Key processor state machine

tests/Stroke.Tests/
├── Application/
│   ├── ApplicationConstructionTests.cs
│   ├── ApplicationLifecycleTests.cs
│   ├── ApplicationInvalidationTests.cs
│   ├── ApplicationContextTests.cs
│   ├── ApplicationKeyBindingMergingTests.cs
│   ├── ApplicationStyleMergingTests.cs
│   ├── ApplicationBackgroundTaskTests.cs
│   ├── ApplicationRunInTerminalTests.cs
│   ├── ApplicationSignalHandlingTests.cs
│   ├── ApplicationResetTests.cs
│   ├── DummyApplicationTests.cs
│   └── AppSessionTests.cs
├── Layout/
│   └── LayoutFocusTests.cs
├── Rendering/
│   └── RendererTests.cs
└── KeyBinding/
    └── KeyProcessorTests.cs
```

**Structure Decision**: Single-project structure matching existing Stroke layout. New files added to `src/Stroke/Application/`, `src/Stroke/Layout/`, `src/Stroke/Rendering/`, and `src/Stroke/KeyBinding/`. Test files mirror source structure under `tests/Stroke.Tests/`.

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| Application split into 5 partial files | The Python Application class is 1,423 lines. Faithful port with C# verbosity (XML docs, Lock patterns, properties) would exceed 1,000 LOC in a single file. | Single file would violate Constitution X (1,000 LOC limit). |
| DefaultKeyBindings returns empty stubs | Actual Emacs/Vi/Mouse/CPR binding *implementations* are separate features (editing modes, mouse handling). The infrastructure to load and merge them must exist for Application to function. | Implementing all editing mode bindings in this feature would be massive scope bloat — those are separate features with their own specs. |
| Layout class placed in Stroke.Layout (not Stroke.Application) | Layout is a fundamental layout concept used by both Application and standalone code. Python places it in `prompt_toolkit.layout.layout`. | Placing in Application would violate the layered architecture — Layout is layer 5, Application is layer 7. |
| KeyProcessor placed in Stroke.KeyBinding | KeyProcessor is a KeyBinding subsystem component. Python places it in `prompt_toolkit.key_binding.key_processor`. | Placing in Application would violate layered architecture. |
| Renderer placed in Stroke.Rendering | Renderer is a rendering subsystem component. Python places it in `prompt_toolkit.renderer`. | Placing in Application would violate layered architecture. |
| ScrollablePane.MakeWindowVisible completed here | This method was deferred from Feature 029 because it requires `AppContext.GetApp()` (Application layer). Now that the Application layer exists, we complete the integration. | Leaving it incomplete would leave a known functional gap in the layout system. |

## Post-Design Constitution Re-Check

| # | Principle | Status | Notes |
|---|-----------|--------|-------|
| I | Faithful Port | PASS | All APIs from Python `application/`, `renderer`, `key_processor`, `layout.layout`, `layout.dummy`, `key_binding.defaults` are covered in contracts. |
| II | Immutability | PASS | Readonly structs for `ColorDepthOption`, `FocusableElement`. Mutable state justified and synchronized. |
| III | Layered Architecture | PASS | No circular dependencies. Layout (L5) ← Application (L7). KeyBinding (L4) ← Application (L7). Rendering (L2) ← Application (L7). |
| IV | Cross-Platform | PASS | `PosixSignalRegistration` + polling fallback. DummyInput/DummyOutput for testing. |
| V | Editing Mode Parity | PASS | Mode state management complete. Binding stubs in place for future features. |
| VI | Performance | PASS | Differential rendering, invalidation dedup, throttling, caching all designed. |
| VII | Full Scope | PASS | All 39 FRs mapped to contracts. |
| VIII | Real-World Testing | PASS | Pipe input/output for deterministic tests. No mocks. |
| IX | Planning Documents | PASS | API mapping alignment verified. |
| X | File Size | PASS | Application split into 5 files. Renderer split into 2 files. |
| XI | Thread Safety | PASS | Documented in all mutable class contracts. |
| XII | Contracts in Markdown | PASS | 6 contract files, all `.md`. |

**Post-Design Gate**: ALL PASS ✓
