# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**Stroke** is a complete, faithful architectural port of [Python Prompt Toolkit](https://github.com/prompt-toolkit/python-prompt-toolkit) to .NET 10. It provides a cross-platform terminal UI framework for building REPLs, database shells, CLI tools, and interactive terminal applications.

- **Target Framework:** .NET 10+
- **Language:** C# 13
- **License:** MIT
- **NuGet Package:** `Stroke`

## Constitutional Principles

These principles are NON-NEGOTIABLE and govern all development on this project.

### I. Faithful Port (100% API Fidelity)

Stroke MUST be a 100% faithful port of Python Prompt Toolkit:

- **Every public class** in Python Prompt Toolkit MUST have an equivalent class in Stroke
- **Every public method** MUST be ported with matching semantics
- **Every public property** MUST be ported with matching semantics
- **Every public constant/enum** MUST be ported with matching values
- **API names** MUST match the original (adjusted only for C# naming conventions: `snake_case` → `PascalCase`)
- **Module/namespace structure** MUST mirror the original package hierarchy

Before implementing ANY feature:
1. Read the corresponding Python source file(s) at `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/`
2. Identify all public APIs in that module
3. Port each API faithfully without invention or embellishment

**Forbidden behaviors**:
- Inventing APIs that do not exist in Python Prompt Toolkit
- Omitting APIs that exist in Python Prompt Toolkit
- Renaming APIs beyond case convention adjustments
- Changing method signatures beyond type-system requirements
- Adding "improvements" or "enhancements" not present in the original

Deviations are permitted ONLY when C# language constraints or platform differences require adaptation. All deviations MUST be documented with explicit rationale.

### II. Immutability by Default

Core data structures MUST be immutable unless mutation is explicitly required for state management. The `Document` class MUST remain immutable with flyweight caching and lazy property computation. Mutable wrappers (e.g., `Buffer`) MUST encapsulate immutable cores. Use `sealed` on classes not designed for inheritance. Prefer `record` types for value-semantic data. Use `ImmutableArray<T>` for readonly collections exposed in APIs.

### III. Layered Architecture

The codebase MUST follow a strict bottom-to-top dependency hierarchy:
1. **Stroke.Core** (Document, Buffer, Primitives) - zero external dependencies
2. **Stroke.Rendering** (Screen, Renderer, Output) - depends only on Core
3. **Stroke.Input** (Keys, Parsing, Mouse) - depends only on Core
4. **Stroke.KeyBinding** - depends on Core, Input
5. **Stroke.Layout** - depends on Core, Rendering
6. **Stroke.Completion** - depends on Core
7. **Stroke.Application** - orchestration layer, may depend on all lower layers
8. **Stroke.Shortcuts** - high-level API, depends on Application

Circular dependencies are forbidden. Lower layers MUST NOT reference higher layers.

### IV. Cross-Platform Terminal Compatibility

Stroke MUST support Linux, macOS, and Windows 10+ with ANSI/VT100 as the primary output mode. A `WindowsConsoleOutput` fallback MUST exist for legacy Windows terminals. All implementations MUST handle:
- Wide characters (CJK) via UnicodeWidth calculations
- Mouse tracking (click, drag, scroll)
- Bracketed paste mode
- Alternate screen buffer
- True color (24-bit) with graceful degradation to 256/16 colors

### V. Complete Editing Mode Parity

Both Emacs and Vi editing modes MUST be fully implemented to match Python Prompt Toolkit functionality:
- **Vi**: Navigation, Insert, Replace, Visual, VisualLine, VisualBlock modes; operators (d, c, y); motions; repeat (.)
- **Emacs**: Kill ring, transpose, incremental search forward/backward
- Mode-conditional bindings MUST use the Filter system (composable boolean conditions)

### VI. Performance-Conscious Design

Rendering MUST use differential updates (compare previous/current screen, update only changed regions). Screen storage MUST be sparse (dictionary-based, storing only non-empty cells). Character interning MUST be used for common ASCII characters. Lazy evaluation MUST be used for derived Document properties. No global mutable state; multiple independent Application instances MUST coexist in the same process.

### VII. Full Scope Commitment (NON-NEGOTIABLE)

Claude MUST NOT reduce scope, defer, deprioritize, or skip any requirements and/or tasks. This principle is absolute and admits no exceptions:
- **No scope reduction**: All specified requirements MUST be implemented as defined
- **No deferral**: Tasks MUST NOT be postponed to "future work" or "later phases" unless explicitly approved by the user
- **No deprioritization**: All tasks retain their assigned priority; Claude MUST NOT unilaterally lower priority
- **No skipping**: Every task in a task list MUST be completed; none may be omitted

When blockers arise, Claude MUST surface them immediately and await user direction rather than autonomously reducing scope.

### VIII. Real-World Testing

The project MUST target 80% test coverage. Tests MUST exercise real implementations only:
- **No mocks**: Mock objects are forbidden
- **No fakes**: Fake implementations are forbidden
- **No doubles**: Test doubles of any kind are forbidden
- **No simulations**: Simulated dependencies are forbidden

**Forbidden Libraries**:
- Moq MUST NOT be used under any circumstances
- FluentAssertions MUST NOT be used under any circumstances

Tests MUST use xUnit with standard assertions.

### IX. Adherence to Planning Documents (NON-NEGOTIABLE)

Claude MUST strictly adhere to the following planning documents during all development and testing. These documents define exact 1:1 mappings that govern implementation:

| Document | Purpose | Scope |
|----------|---------|-------|
| `docs/api-mapping.md` | Python → C# API mappings | 35+ modules, all classes/methods/properties |
| `docs/test-mapping.md` | Python → C# test mappings | 155 tests with naming conventions |
| `docs/examples-mapping.md` | Python → C# example mappings | 129 examples across 9 projects |
| `docs/doc-plan.md` | Documentation strategy | 30 pages, DocFX + GitHub Pages |
| `docs/dependencies-plan.md` | External dependencies | Wcwidth, TextMateSharp integration |

**Strict Compliance Requirements**:
- Claude MUST consult `api-mapping.md` before implementing ANY module
- Claude MUST consult `test-mapping.md` before implementing ANY tests
- Claude MUST consult `examples-mapping.md` before implementing ANY examples
- Claude MUST consult `doc-plan.md` before creating ANY documentation
- Claude MUST NOT skip, rename, or reorganize mapped items
- Deviations require explicit user approval

### X. Source Code File Size Limits

Source code files MUST be kept to 1,000 lines of code (LOC) or less:

- **Maximum file size**: No single source file MUST exceed 1,000 LOC
- **Proactive splitting**: When a file approaches the limit, refactor into smaller, focused units
- **Logical grouping**: Split files by responsibility (e.g., separate test files by user story)
- **Naming clarity**: Split files MUST have clear, descriptive names indicating their scope

**Rationale**: Smaller files improve navigation, reduce merge conflicts, and encourage single-responsibility design.

**Exceptions**: Generated code or files that cannot be logically split may exceed this limit with documented justification.

### XI. Thread Safety by Default

All Stroke implementations with mutable state MUST be thread-safe. This is a documented deviation from Python Prompt Toolkit, which assumes single-threaded execution:

**Rationale**: .NET applications commonly operate in multi-threaded contexts (async/await, background workers, parallel processing). Defensive thread safety prevents subtle concurrency bugs and makes Stroke suitable for real-world .NET usage patterns.

**Requirements**:
- **Mutable classes**: All classes with mutable state MUST use appropriate synchronization
- **Preferred lock type**: Use `System.Threading.Lock` (.NET 9+) with the `EnterScope()` pattern for automatic release
- **Immutable types**: Immutable types (e.g., `Document`, `ClipboardData`) are inherently thread-safe and require no synchronization
- **Stateless types**: Stateless types (e.g., `DummyClipboard`) are inherently thread-safe and require no synchronization
- **Atomicity scope**: Individual operations MUST be atomic; compound operations (e.g., read-modify-write sequences) require external synchronization by the caller

**Implementation Pattern**:
```csharp
public sealed class SomeStatefulClass
{
    private readonly Lock _lock = new();
    private SomeState _state;

    public void MutateState(SomeValue value)
    {
        using (_lock.EnterScope())
        {
            _state = ComputeNewState(value);
        }
    }
}
```

**Documentation**: All thread-safe classes MUST document their thread safety guarantees in XML documentation comments.

### XII. Contracts in Markdown Only (NON-NEGOTIABLE)

API contracts MUST be specified in markdown format only. This is absolute:

- **No .cs contract files**: Contracts MUST NEVER be created as `.cs` files
- **Markdown only**: All contracts MUST be written in `.md` files
- **Format**: Use markdown code blocks with C# syntax highlighting for signatures

**Rationale**: Markdown contracts are human-readable documentation that can be reviewed without IDE tooling. They serve as design artifacts, not compilable code. The actual implementation comes later during the coding phase.

**Forbidden**:
- Creating `contracts/*.cs` files
- Creating any `.cs` file during the planning phase
- Treating contracts as compilable code

## Planning Documents

### API Mapping (`docs/api-mapping.md`)

The authoritative reference for all Python → C# API translations:
- **35+ Python modules** mapped to .NET namespaces
- **Every class, method, property, constant** with C# equivalents
- **Naming conventions**: `snake_case` → `PascalCase`
- **Type mappings**: Python types → C# types

Before implementing any module, Claude MUST:
1. Find the module section in `api-mapping.md`
2. Identify all mapped APIs
3. Implement each API exactly as documented

### Test Mapping (`docs/test-mapping.md`)

Complete 1:1 mapping of Python Prompt Toolkit tests:
- **155 Python tests** → 155 C# tests
- **Test naming**: `test_foo_bar` → `FooBar`
- **Helper classes**: PromptSessionTestHelper, HandlerTracker, KeyCollector, OutputCapture
- **TestKeys**: Static class with ANSI escape sequences

### Examples Mapping (`docs/examples-mapping.md`)

Complete 1:1 mapping of Python Prompt Toolkit examples:
- **129 Python examples** → 129 C# examples
- **9 example projects**: Prompts, FullScreen, ProgressBar, Dialogs, PrintText, Choices, Telnet, Ssh, Tutorial
- **Naming**: `get-input.py` → `GetInput.cs`
- **Implementation phases**: 5 phases from foundation to advanced

### Documentation Plan (`docs/doc-plan.md`)

Documentation strategy using DocFX + GitHub Pages:
- **30 documentation pages** mirroring Python PTK structure
- **10 namespace override pages** for API enrichment
- **Auto-generated API docs** from XML comments
- **Hosting**: `https://<username>.github.io/stroke`

### Dependencies Plan (`docs/dependencies-plan.md`)

External dependency strategy for Pygments and wcwidth equivalents:
- **Wcwidth NuGet package** (v4.0.1, MIT) for Unicode character width
- **TextMateSharp** (v1.0.70, MIT) for syntax highlighting via TextMate grammars
- **40+ languages** supported via VS Code grammar ecosystem
- **Character width caching** patterns matching Python Prompt Toolkit

Before implementing Unicode width or syntax highlighting, Claude MUST:
1. Consult `docs/dependencies-plan.md` for implementation strategy
2. Use the recommended NuGet packages (Wcwidth, TextMateSharp)
3. Follow the caching patterns defined in the document
4. Map Pygments token types using the TokenTypes constants

## Reference Repositories

The following repositories are cloned locally as architectural references:

- `/Users/brandon/src/python-prompt-toolkit` - The original Python library being ported (primary reference)
- `/Users/brandon/src/spectre.console` - .NET console library for Rich-style output (Spectre.Console)
- `/Users/brandon/src/Terminal.Gui` - .NET console UI toolkit (Terminal.Gui v2)

## Architecture

Stroke mirrors Python Prompt Toolkit's layered architecture with .NET idioms:

### Core Layers (Bottom to Top)

1. **Stroke.Core** - Immutable Document model, mutable Buffer, primitives (Point, Size, WritePosition)
2. **Stroke.Rendering** - Screen buffer (sparse 2D char storage), Renderer with diff updates, IOutput abstraction (VT100, Windows Console)
3. **Stroke.Input** - Key/mouse event parsing, VT100 parser, platform input abstraction
4. **Stroke.KeyBinding** - KeyBindings registry, KeyProcessor state machine, Emacs and Vi binding implementations
5. **Stroke.Layout** - Container hierarchy (HSplit, VSplit, FloatContainer), Window, Dimension system, Margins
6. **Stroke.Completion** - ICompleter interface, WordCompleter, PathCompleter, FuzzyCompleter
7. **Stroke.Application** - Application lifecycle, event loop, context management
8. **Stroke.Shortcuts** - High-level PromptSession API, dialog helpers

### Key Design Patterns

- **Immutable Document**: `Document` class uses flyweight caching, lazy property computation, structural sharing
- **Mutable Buffer**: Wraps Document, manages undo stack, selection, completion state
- **Sparse Screen**: Dictionary-based storage for only non-empty cells, supports wide characters and z-index layering
- **Differential Rendering**: Renderer compares previous/current screen, only updates changed regions
- **Filter System**: Composable boolean conditions (HasFocus, InViMode, etc.) for conditional key bindings and UI

### Namespace Structure

```
Stroke.Core.Document/Buffer/Primitives
Stroke.Rendering.Screen/Renderer/Output
Stroke.Input.Keys/Parsing/Mouse/Abstractions
Stroke.KeyBinding.Bindings/Processor/Emacs/Vi
Stroke.Layout.Containers/Controls/Dimensions/Margins/Menus/Windows
Stroke.Completion.Core/Completers/Fuzzy
Stroke.Application.App/Events/Context
Stroke.Styles.Core/Colors/Formatting/Transformations/Themes
Stroke.History.Core/Implementations/Search
Stroke.Filters.Core/BuiltIn/Operators
Stroke.Validation.Core/Implementations
Stroke.Lexers.Core/Implementations
Stroke.Widgets.Base/Text/Controls/Lists/Containers/Toolbars/Dialogs
Stroke.Shortcuts.Prompt/Dialogs
```

## Design Document

The complete specification is in `docs/design.md` (~65K tokens). It contains:
- Full C# API designs with code examples
- Cross-reference to Python Prompt Toolkit equivalents
- Performance considerations and platform abstractions

## Development Workflow

1. **Reference First**: Before implementing any feature, locate the equivalent in Python Prompt Toolkit source and read it completely
2. **API Inventory**: List all public APIs in the Python module before writing any C# code
3. **Faithful Implementation**: Port each API exactly as defined in Python Prompt Toolkit
4. **Design Review**: Complex subsystems require written design in `/docs/` before implementation
5. **Incremental Delivery**: Features MUST be deliverable in independently testable increments
6. **Constitution Check**: Implementation plans MUST verify compliance with all Core Principles before proceeding
7. **File Size Review**: Before completing any implementation, verify no source file exceeds 1,000 LOC
8. **Thread Safety Review**: For classes with mutable state, verify thread safety implementation and add concurrent tests

## Technical Standards

- **Framework**: .NET 10+
- **Language**: C# 13
- **Naming**: PascalCase for public members, async methods suffixed with `Async`
- **Async**: Prefer `async/await` for I/O-bound operations; use `ValueTask` for hot paths
- **Nullability**: Nullable reference types enabled; explicit null handling required
- **Testing**: Unit tests MUST accompany new public APIs; use xUnit conventions; target 80% coverage
- **Documentation**: Triple-slash XML doc comments (`///`) required for all public types and members
- **Commits**: Do NOT include issue numbers, PR numbers, feature numbers, or spec numbers in commit messages (e.g., no `#31`, `(#33)`, `(031)`, `Feature 031`)

## End-to-End Testing with TUI Driver

The TUI Driver MCP tools provide real terminal automation for testing Stroke applications. These tools align with Principle VIII (Real-World Testing) by exercising actual terminal behavior without mocks or simulations.

### Available TUI Tools

| Tool | Purpose |
|------|---------|
| `tui_launch` | Launch a Stroke application in a controlled PTY session |
| `tui_text` | Get current terminal text content for assertions |
| `tui_snapshot` | Get accessibility-style DOM snapshot with element refs |
| `tui_screenshot` | Capture PNG screenshot for visual verification |
| `tui_press_key` | Send single key (Enter, Tab, Ctrl+c, arrow keys, etc.) |
| `tui_press_keys` | Send key sequences for complex interactions |
| `tui_send_text` | Type raw text into the terminal |
| `tui_click` | Click on elements by reference ID from snapshot |
| `tui_wait_for_text` | Wait for expected text to appear (with timeout) |
| `tui_wait_for_idle` | Wait for screen to stabilize after updates |
| `tui_resize` | Test responsive behavior at different terminal sizes |
| `tui_run_code` | Execute JavaScript for complex automation sequences |
| `tui_get_input` | Get raw input sent to process (escape sequences) for debugging |
| `tui_get_output` | Get raw PTY output (escape sequences) for debugging |
| `tui_close` | Clean up session after test completion |

### When to Use TUI Driver

Use TUI Driver for verification and debugging:

1. **Rendering verification** - Confirm screen output matches expectations
2. **Input handling** - Test keyboard input, Emacs/Vi key bindings, mouse events
3. **Wide character display** - Verify CJK characters render at correct column widths
4. **Differential updates** - Confirm only changed regions are redrawn
5. **Layout behavior** - Test HSplit, VSplit, FloatContainer at various sizes
6. **Completion menus** - Verify dropdown positioning and selection
7. **Dialog workflows** - Test multi-step user interactions
8. **Color output** - Screenshot verification for ANSI color rendering
9. **Debugging** - Inspect terminal state, capture screenshots of failures, trace input/output sequences

### Example Verification Flow

```
1. tui_launch: Start example application (e.g., dotnet run --project examples/Prompts)
2. tui_wait_for_text: Wait for prompt to appear
3. tui_send_text: Type user input
4. tui_press_key: Press Enter to submit
5. tui_wait_for_idle: Wait for response to render
6. tui_text: Capture output and verify expected content
7. tui_screenshot: (optional) Save visual state for regression comparison
8. tui_close: Clean up the session
```

### Example Verification

Claude SHOULD use TUI Driver to verify the 129 examples from `docs/examples-mapping.md` work correctly:

1. Launch each example application with `tui_launch`
2. Interact with it using `tui_send_text`, `tui_press_key`, etc.
3. Verify expected output appears with `tui_text` and `tui_wait_for_text`
4. Capture screenshots for visual confirmation when needed
5. Test edge cases (resize, special keys, wide characters)

This ensures examples behave correctly in real terminal environments before marking implementation complete.

## Active Technologies
- C# 13 / .NET 10 + None for Core layer (xUnit for tests)
- xUnit for testing (no mocks, no FluentAssertions per Constitution VIII)
- C# 13 / .NET 10 + None (Stroke.Core layer has zero external dependencies) (004-clipboard-system)
- In-memory only (no persistence) (004-clipboard-system)
- C# 13 / .NET 10 + None (Stroke.Core layer - zero external dependencies per Constitution III) (005-auto-suggest-system)
- N/A (in-memory only) (005-auto-suggest-system)
- N/A (in-memory only - undo stack, redo stack, working_lines) (007-mutable-buffer)
- C# 13 / .NET 10 + None (Stroke.History is part of Stroke.Core layer) (008-history-system)
- File system for `FileHistory`, in-memory for others (008-history-system)
- C# 13 / .NET 10 + Stroke.Core (Document class) (009-validation-system)
- N/A (stateless validation) (009-validation-system)
- C# 13 / .NET 10 + Stroke.Core (Document, Buffer) - zero external dependencies per Constitution III (010-search-system)
- N/A (in-memory state only) (010-search-system)
- C# 13 / .NET 10 + None (Stroke.Input layer - zero external dependencies per Constitution III) (011-keys-enum)
- N/A (enum values and static readonly data only) (011-keys-enum)
- N/A (stateless completion - completers may access filesystem for PathCompleter) (012-completion-system)
- C# 13 / .NET 10 + Stroke.Core (Point type) (013-mouse-events)
- N/A (in-memory data structures only) (013-mouse-events)
- C# 13 / .NET 10 + None (Stroke.FormattedText layer - zero external dependencies per Constitution III) (015-formatted-text-system)
- C# 13 / .NET 10 + None (Stroke.Layout layer, depends only on Core per Constitution III) (016-layout-dimensions)
- C# 13 / .NET 10+ + None (Stroke.Filters is part of Core layer with zero external dependencies per Constitution III) (017-filter-system-core)
- N/A (in-memory only - filter instances and caches) (017-filter-system-core)
- C# 13 / .NET 10 + None (Stroke.Styles is part of Core layer, zero external dependencies per Constitution III) (018-styles-system)
- N/A (in-memory style definitions and caches only) (018-styles-system)
- N/A (in-memory parsing only) (019-html-formatted-text)
- C# 13 / .NET 10 + None (Stroke.Core layer with zero external dependencies per Constitution III) (020-ansi-formatted-text)
- C# 13 / .NET 10+ + None (Stroke.Output depends only on Stroke.Core and Stroke.Styles per Constitution III) (021-output-system)
- N/A (in-memory output buffers only) (021-output-system)
- C# 13 / .NET 10+ + Stroke.Core (SimpleCache, IFilter, FilterOrBool), Stroke.Input (Keys enum) (022-key-bindings-system)
- C# 13 / .NET 10 + Stroke.Input (KeyPress), Stroke.Clipboard (ClipboardData), Stroke.KeyBinding (KeyPressEvent) (023-editing-modes-state)
- C# 13 / .NET 10 + Wcwidth NuGet package (v4.0.1, MIT) for character width calculation (024-utilities)
- N/A (in-memory caches only) (024-utilities)
- C# 13 / .NET 10 + Stroke.Core (Document), Stroke.FormattedText (StyleAndTextTuple, FormattedTextUtils), Stroke.Filters (IFilter, FilterOrBool) (025-lexer-system)
- N/A (in-memory caches only - line cache, generator tracking) (025-lexer-system)
- C# 13 / .NET 10 + None (Stroke.KeyBinding namespace has no external dependencies for this feature) (026-vi-digraphs)
- N/A (static immutable dictionary populated at static initialization) (026-vi-digraphs)
- C# 13 / .NET 10 + Stroke.Core (Document), Stroke.Completion (ICompleter, Completion, CompleteEvent), Stroke.Lexers (ILexer), Stroke.Validation (IValidator, ValidationError), Stroke.FormattedText (StyleAndTextTuple) (027-regular-languages)
- N/A (in-memory only - compiled regexes and parse trees) (027-regular-languages)
- C# 13 / .NET 10 + Stroke.Core (Point, FastDictCache, UnicodeWidth), Wcwidth NuGet package (028-screen-character-model)
- N/A (in-memory only - sparse dictionary-based screen buffer) (028-screen-character-model)
- C# 13 / .NET 10 + Stroke.Core (Document, Buffer, Primitives, Filters, Caches), Stroke.FormattedText (StyleAndTextTuple, AnyFormattedText), Stroke.KeyBinding (IKeyBindingsBase, KeyPressEvent), Stroke.Input (Keys, MouseEvent), Stroke.Lexers (ILexer), Stroke.Layout (Dimension, Screen, Char, WritePosition, MouseHandlers) (029-layout-containers-controls-window)
- N/A (in-memory only - scroll state, render caches) (029-layout-containers-controls-window)
- C# 13 / .NET 10+ + Stroke.Core (Document, Buffer, Primitives, Caches, Event, UnicodeWidth), Stroke.Input (Keys, IInput, KeyPress, Typeahead), Stroke.Output (IOutput, ColorDepth, CursorShape), Stroke.Styles (IStyle, StyleMerger, DefaultStyles, IStyleTransformation, Attrs), Stroke.Filters (IFilter, FilterOrBool, Condition), Stroke.KeyBinding (IKeyBindingsBase, KeyBindings, Binding, ConditionalKeyBindings, MergedKeyBindings, GlobalOnlyKeyBindings, ViState, EmacsState, EditingMode, KeyPressEvent), Stroke.Layout (IContainer, Window, BufferControl, FormattedTextControl, SearchBufferControl, DummyControl, Screen, Char, WritePosition, Dimension, IUIControl, UIContent, MouseHandlers, IMargin), Stroke.Clipboard (IClipboard, InMemoryClipboard), Stroke.CursorShapes (ICursorShapeConfig), Stroke.FormattedText (AnyFormattedText), Wcwidth NuGet package (030-application-system)
- N/A (in-memory only — application state, render buffers, key processor queues) (030-application-system)
- C# 13 / .NET 10 + Stroke.Core (Document, Buffer, SimpleCache, ConversionUtils), Stroke.FormattedText (StyleAndTextTuple, AnyFormattedText, FormattedTextUtils), Stroke.Filters (IFilter, FilterOrBool, Condition), Stroke.KeyBinding (ViState, InputMode, KeyProcessor), Stroke.Layout (BufferControl, SearchBufferControl, Layout, Window, UIContent), Stroke.Application (Application, AppContext, AppFilters), Stroke.Input (MouseEvent) (031-input-processors)
- N/A (in-memory only — fragment transformation caches) (031-input-processors)
- C# 13 / .NET 10 + Stroke.Filters (IFilter, Condition, Filter, Always, Never), Stroke.Application (Application, AppContext, DummyApplication), Stroke.KeyBinding (ViState, EmacsState, EditingMode, InputMode, KeyProcessor), Stroke.Layout (Layout, LayoutUtils, Window, IContainer, BufferControl, SearchBufferControl, IUIControl), Stroke.Core (Buffer, SelectionState, CompletionState, SimpleCache, Memoization) (032-application-filters)
- N/A (in-memory only — stateless filter lambdas) (032-application-filters)

## Recent Changes
- 031-input-processors: Added input processor pipeline for fragment transformation with IProcessor interface, TransformationInput/Transformation records, ExplodedList (Collection<StyleAndTextTuple> with single-char fragment explosion), 26 processor implementations: HighlightSearchProcessor, HighlightIncrementalSearchProcessor, HighlightSelectionProcessor, DisplayMultipleCursors, BeforeInput/AfterInput/ShowArg, PasswordProcessor, ReverseSearchProcessor, HighlightMatchingBracketProcessor, ConditionalProcessor, DynamicProcessor, TabsProcessor, ShowLeadingWhiteSpaceProcessor/ShowTrailingWhiteSpaceProcessor, AppendAutoSuggestion, MergedProcessor (list-based SourceToDisplay/DisplayToSource composition), ProcessorUtils (MergeProcessors factory), BufferControl.InputProcessors/DefaultInputProcessors integration, Layout.SearchTargetBufferControl property, AppFilters.ViInsertMultipleMode filter, thread-safe via immutable records and sealed classes (6618 tests total, >80% coverage)
- 030-application-system: Added Application<TResult> orchestration layer with RunAsync/Run lifecycle using channel-based event loop for thread-safe redraws, Renderer with differential screen updates (previous/current diff via ScreenDiff), KeyProcessor state machine for key binding dispatch with flush timeout and SIGINT marshaled to event loop via action channel, Layout class with focus management (walk/find utilities, CurrentWindow/CurrentControl, FocusableElement conversions), RunInTerminal for suspending UI during external commands, AppContext/AppSession with proper nested session stack semantics (per-session predecessor linked list), background task tracking with cancellation on exit, invalidation coalescing via bounded channel, refresh intervals, min redraw throttling, Unix signal handling (SIGWINCH resize with platform-correct SIGTSTP: 18 on macOS, 20 on Linux), CombinedRegistry for merged key bindings across editing modes, ScrollablePane visibility toggling, safe command execution via ArgumentList, CancellationTokenSource leak prevention in KeyProcessor (6469 tests total, >80% coverage)
- 029-layout-containers-controls-window: Added layout containers, controls, and window system with HSplit (vertical stacking), VSplit (horizontal arrangement), FloatContainer (overlay positioning with z-index), ConditionalContainer (filter-based show/hide), DynamicContainer (runtime content switching), AnyContainer (IContainer wrapper struct), IContainer/IMagicContainer interfaces, ContainerUtils (walk/find utilities), Window container (UIControl wrapper with scrolling, margins, cursor highlighting, style inheritance), BufferControl (editable text with lexer/search/mouse support), FormattedTextControl (static styled text with fragment mouse handler dispatch), SearchBufferControl (search input specialization), DummyControl (empty placeholder), UIContent (line-based content model with cursor/menu positions), IUIControl interface, IMargin/NumberedMargin/ScrollbarMargin/PromptMargin/ConditionalMargin margin system, ScrollOffsets/WindowRenderInfo/ColorColumn/GetLinePrefixCallable window support types, HorizontalAlign/VerticalAlign/WindowAlign enums, weighted dimension allocation in HSplit/VSplit, thread-safe via Lock on mutable state (6066 tests total, >80% coverage)
- 028-screen-character-model: Added Screen and Character Model with Screen class (sparse 2D buffer via Dictionary<int, Dictionary<int, Char>>), Char sealed class (immutable styled character with FastDictCache interning, control character transformation), CharacterDisplayMappings (66 C0/DEL/C1/NBSP mappings), WritePosition readonly record struct, IWindow marker interface, per-window cursor/menu position tracking, zero-width escape sequences, z-index deferred drawing (DrawWithZIndex/DrawAllFloats), FillArea/AppendStyleToContent region operations, Clear() for screen reuse, thread-safe via Lock (369 tests, >80% coverage)
- 027-regular-languages: Added Regular Languages contrib module with Grammar.Compile() for regex patterns with Python (?P<name>...) syntax, CompiledGrammar with Match/MatchPrefix, Match class with Variables/EndNodes/TrailingInput extraction, MatchVariable for variable metadata, GrammarCompleter (ICompleter) for per-variable completion delegation, GrammarLexer (ILexer) for syntax highlighting, GrammarValidator (IValidator) for input validation, Node parse tree classes (AnyNode, NodeSequence, RegexNode, Lookahead, Variable, Repeat), RegexParser for pattern compilation with prefix generation, thread-safe via immutability (162 tests, >80% coverage)
- 026-vi-digraphs: Added Vi Digraphs with Digraphs static class (1,356 RFC1345 mappings from Python Prompt Toolkit), Lookup(char, char) returning int? code points, GetString(char, char) returning string? characters, Map property for enumeration, FrozenDictionary storage for O(1) lookup, thread-safe via immutability (36 tests, 100% coverage)
- 025-lexer-system: Added Lexer System with ILexer interface, SimpleLexer (single-style text), DynamicLexer (runtime lexer switching), PygmentsLexer (syntax highlighting adapter with line caching, generator reuse), ISyntaxSync interface with SyncFromStart singleton and RegexSync (pattern-based sync position), IPygmentsLexer interface for external lexer implementations, TokenCache (thread-safe token-to-style conversion), MinLinesBackwards=50, ReuseGeneratorMaxDistance=100, MaxBackwards=500, ForLanguage factory, FromFilename fallback (163 tests, >80% coverage)
- 024-utilities: Added utility classes with Event<TSender> (pub/sub with += and -= operators), UnicodeWidth (character/string width with thread-safe LRU cache), PlatformUtils (OS detection, environment variable checks for TERM/STROKE_BELL/ConEmuANSI), CollectionUtils (TakeUsingWeights for proportional distribution), ConversionUtils/AnyFloat (lazy value conversion), DummyContext (no-op IDisposable singleton) (146 tests, >80% coverage)
- 023-editing-modes-state: Added Vi/Emacs editing mode state management with EditingMode enum (Vi, Emacs), InputMode enum (Insert, InsertMultiple, Navigation, Replace, ReplaceSingle), BufferNames static class (SearchBuffer, DefaultBuffer, SystemBuffer), CharacterFind sealed record for f/F/t/T commands, ViState class (thread-safe with Lock, InputMode setter side effects, named registers, Reset()), EmacsState class (thread-safe macro recording with StartMacro/EndMacro/AppendToRecording), OperatorFuncDelegate (116 tests, 100% coverage)
- 022-key-bindings-system: Added Key Bindings System with KeyBindings registry (thread-safe, caching), Binding immutable class, KeyOrChar/KeyPress types, KeyPressEvent, KeyBindingDecorator factory, proxy types (ConditionalKeyBindings, MergedKeyBindings, GlobalOnlyKeyBindings, DynamicKeyBindings), KeyBindingUtils (ParseKey, Merge), KeyBindingsExtensions (WithFilter, GlobalOnly, Merge), FilterOrBool.HasValue property for distinguishing struct default from explicit values (365 tests, >80% coverage)
- 021-output-system: Added VT100 output system with IOutput interface, Vt100Output/PlainTextOutput/DummyOutput implementations, ColorDepth enum with environment detection (NO_COLOR, STROKE_COLOR_DEPTH), thread-safe color caches (16-color, 256-color), CursorShape enum with modal Vi/Emacs support, OutputFactory for platform detection, FlushStdout helper (420 tests, 85.7% coverage)
- 020-ansi-formatted-text: Added % operator overloads to Ansi class for Python-style string interpolation with automatic ANSI escape neutralization (\x1b and \b replaced with ?), 15 new tests (63 total Ansi tests, 96.83% coverage)
- 019-html-formatted-text: Added % operator overloads to Html class for Python-style string interpolation with automatic HTML escaping, fixed FR-022 (fg attribute takes precedence over color attribute), added 7 new tests (46 total Html tests, 98.96% coverage)
- 018-styles-system: Added comprehensive styles system with Style class (rule-based styling with class lookup and inheritance), Attrs record struct (color/formatting attributes), BaseStyle abstract class, StyleAndTextTuples helpers, named colors (148 CSS colors in NamedColors), ANSI colors (16 standard + 256 indexed), color utilities (ParseColor, AnsiColorToRgb), style transformations (SwapLightAndDarkStyleTransformation, ReverseStyleTransformation, SetDefaultColorStyleTransformation, AdjustBrightnessStyleTransformation, ConditionalStyleTransformation, DummyStyleTransformation), DynamicStyle/ConditionalStyle for runtime styling, MergeStyles for combining styles, default styles (PromptToolkitStyle, ColorsStyle, WidgetsStyle, PygmentsDefaultStyle), PygmentsStyleUtils for syntax highlighting integration (542 tests, >80% coverage)
- 017-filter-system-core: Added Filter System with IFilter interface, Filter abstract base class with operators (&, |, ~) and per-instance caching, Always/Never singletons, Condition wrapper for Func<bool>, FilterOrBool union type, FilterUtils static utilities (ToFilter, IsTrue), boolean algebra (identity, annihilation, short-circuit, double negation), thread-safe operations (279 tests, 100% coverage on all filter classes)
- 016-layout-dimensions: Added Dimension class for layout sizing constraints (min/max/preferred/weight), DimensionUtils with SumLayoutDimensions and MaxLayoutDimensions aggregation functions, ToDimension/IsDimension type conversion utilities, D static alias class for shorter syntax (95 tests, >80% coverage)
- 015-formatted-text-system: Added comprehensive formatted text system with Html (HTML-like markup parsing), Ansi (ANSI escape sequence parsing with SGR codes, 256-color, true color), Template (placeholder interpolation with lazy evaluation), PygmentsTokens (Pygments-style token conversion), extended FormattedTextUtils (ZeroWidthEscape, FragmentListWidth with Wcwidth, SplitLines, Merge), extended AnyFormattedText with implicit conversions for Html/Ansi/PygmentsTokens (281 tests, >90% coverage)
- 014-input-system: Added cross-platform input system with IInput interface, Vt100Parser for VT100/ANSI escape sequence parsing, platform-specific implementations (Win32Input for Windows, Vt100Input for POSIX), pipe input for testing (IPipeInput, PosixPipeInput, Win32PipeInput, SimplePipeInput), raw/cooked terminal mode support, InputFactory for platform detection, KeyPress record struct, AnsiSequences dictionary (>80% coverage)
- 013-mouse-events: Added mouse event system with MouseEventType enum (5 values), MouseButton enum (5 values), MouseModifiers [Flags] enum, MouseEvent record struct, MouseHandlers class (thread-safe with Lock), NotImplementedOrNone abstract class for event bubbling (106 tests, 100% coverage)
- 012-completion-system: Added ICompleter interface with 12 completers (WordCompleter, PathCompleter, FuzzyCompleter, FuzzyWordCompleter, DeduplicateCompleter, MergeCompleter, ThreadedCompleter, DummyCompleter, NestedCompleter, ExecutableCompleter, ConditionalCompleter, DynamicCompleter), Completion record, CompleteEvent record (267 tests, >80% coverage); added Stroke.Benchmarks project
- 011-keys-enum: Added Keys enum with 151 key values from Python Prompt Toolkit
- 010-search-system: Enhanced SearchState with thread-safe properties, Invert(), ToString(); added SearchOperations static class stubs (41 tests, 100%/80% coverage)
- 009-validation-system: Added IValidator interface with ValidatorBase, DummyValidator, ConditionalValidator, DynamicValidator, ThreadedValidator implementations (142 tests, 100% coverage)
- 008-history-system: Added IHistory interface with InMemoryHistory, FileHistory, ThreadedHistory, DummyHistory implementations (119 tests, >90% coverage)
- 007-mutable-buffer: Added Buffer class as mutable wrapper for Document with undo/redo, navigation, history, selection, clipboard, completion, validation, auto-suggest, and external editor support (148 tasks, 87% coverage)
- 006-cache-utilities: Added SimpleCache, FastDictCache, Memoization in Stroke.Core namespace (92 tests)
- 005-auto-suggest-system: Added AutoSuggest with history-based suggestions in Stroke.Core namespace
- 004-clipboard-system: Added IClipboard, InMemoryClipboard (kill ring), DynamicClipboard, DummyClipboard in Stroke.Clipboard namespace (59 tests)
- 003-selection-system: Added SelectionState, SelectionType, PasteMode types with ToString() and tests (45 tests)
- 002-immutable-document: Added immutable Document class with full text cursor model (310 tests)
- 001-project-setup-primitives: Added project structure, primitives (Point, Size, WritePosition), CI/CD
