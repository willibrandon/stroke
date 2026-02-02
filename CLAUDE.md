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

## Workflow Orchestration

### 1. Plan Mode Default
- Enter plan mode for ANY non-trivial task (3+ steps or architectural decisions)
- If something goes sideways, STOP and re-plan immediately – don't keep pushing
- Use plan mode for verification steps, not just building
- Write detailed specs upfront to reduce ambiguity

### 2. Subagent Strategy
- Use subagents liberally to keep main context window clean
- Offload research, exploration, and parallel analysis to subagents
- For complex problems, throw more compute at it via subagents
- One task per subagent for focused execution

### 3. Self-Improvement Loop
- After ANY correction from the user: update `tasks/lessons.md` with the pattern
- Write rules for yourself that prevent the same mistake
- Ruthlessly iterate on these lessons until mistake rate drops
- Review lessons at session start for relevant project

### 4. Verification Before Done
- Never mark a task complete without proving it works
- Diff behavior between main and your changes when relevant
- Ask yourself: "Would a staff engineer approve this?"
- Run tests, check logs, demonstrate correctness

### 5. Demand Elegance (Balanced)
- For non-trivial changes: pause and ask "is there a more elegant way?"
- If a fix feels hacky: "Knowing everything I know now, implement the elegant solution"
- Skip this for simple, obvious fixes – don't over-engineer
- Challenge your own work before presenting it

### 6. Autonomous Bug Fixing
- When given a bug report: just fix it. Don't ask for hand-holding
- Point at logs, errors, failing tests – then resolve them
- Zero context switching required from the user
- Go fix failing CI tests without being told how

## Task Management

1. **Plan First:** Write plan to `tasks/todo.md` with checkable items
2. **Verify Plan:** Check in before starting implementation
3. **Track Progress:** Mark items complete as you go
4. **Explain Changes:** High-level summary at each step
5. **Document Results:** Add review section to `tasks/todo.md`
6. **Capture Lessons:** Update `tasks/lessons.md` after corrections

## Core Principles

- **Simplicity First:** Make every change as simple as possible. Impact minimal code.
- **No Laziness:** Find root causes. No temporary fixes. Senior developer standards.
- **Minimal Impact:** Changes should only touch what's necessary. Avoid introducing bugs.

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
- **Runtime**: C# 13 / .NET 10+ (all layers)
- **Testing**: xUnit (no mocks, no FluentAssertions per Constitution VIII)
- **External NuGet**: Wcwidth (v4.0.1, MIT) for character width; TextMateSharp (v1.0.70, MIT) for syntax highlighting
- **Storage**: In-memory only (except FileHistory which uses filesystem)
- **Layer dependencies**: Follow Constitution III strictly (see Architecture section above)
- C# 13 / .NET 10+ + `Stroke.Application` (AppSession, AppContext, RunInTerminal), `Stroke.Output` (IOutput) (049-patch-stdout)
- N/A (in-memory buffering only) (049-patch-stdout)

## Recent Changes

Most recent first. Total test count: **8,271 tests**.

- 048-dialog-shortcuts: Dialogs static class — 7 factory methods + 7 async wrappers, thread-safe ProgressDialog
- 047-prompt-session: PromptSession<TResult> 44-param constructor, Prompt static class, CompleteStyle enum, confirm dialogs
- 046-shortcut-utils: FormattedTextOutput.Print/PrintContainer, TerminalUtils.Clear/SetTitle/ClearTitle
- 045-base-widgets: 15 widgets — Label, Button, Frame, Shadow, Box, TextArea, ProgressBar, DialogList<T>, RadioList<T>, CheckboxList<T>, Checkbox, Dialog
- 044-toolbar-widgets: 7 toolbars — FormattedText, System, Arg, Search, CompletionsControl, Completions, Validation
- 043-vi-key-bindings: ViBindings ~300 bindings across 8 files (navigation, operators, text objects, visual mode)
- 042-emacs-key-bindings: EmacsBindings 78 core + 34 shift-selection bindings
- 041-open-in-editor: 3 binding loaders (Emacs Ctrl-X Ctrl-E, Vi 'v', combined)
- 040-focus-cpr-bindings: FocusFunctions (FocusNext/Previous), CprBindings (CPR escape parsing)
- 039-auto-suggest-bindings: 4 bindings (3 full accept + 1 partial word accept)
- 038-search-system-bindings: SearchOperations lifecycle, SearchBindings (Emacs + Vi loaders)
- 037-basic-key-bindings: BasicBindings 118 bindings (90 ignored + 28 specific)
- 036-mouse-bindings: MouseBindings, 3 FrozenDictionary lookup tables (XTerm SGR/Typical/URXVT)
- 035-scroll-bindings: ScrollBindings (8 functions), PageNavigationBindings (3 loaders)
- 034-named-commands: NamedCommands 49 Readline commands, CompletionBindings
- 033-completion-menus: CompletionsMenu, MultiColumnCompletionsMenu, MenuUtils
- 032-application-filters: AppFilters, ViFilters, EmacsFilters, SearchFilters
- 031-input-processors: IProcessor pipeline, 26 processors, ExplodedList, MergedProcessor
- 030-application-system: Application<TResult>, Renderer diff updates, KeyProcessor, Layout, AppContext
- 029-layout-containers: HSplit, VSplit, FloatContainer, ConditionalContainer, Window, BufferControl, margins
- 028-screen-character: Screen sparse 2D buffer, Char interning, z-index drawing
- 027-regular-languages: Grammar.Compile, GrammarCompleter/Lexer/Validator
- 026-vi-digraphs: 1,356 RFC1345 digraph mappings
- 025-lexer-system: ILexer, SimpleLexer, DynamicLexer, PygmentsLexer
- 024-utilities: Event<T>, UnicodeWidth, PlatformUtils, CollectionUtils
- 023-editing-modes: EditingMode, InputMode, ViState, EmacsState
- 022-key-bindings: KeyBindings registry, Binding, proxy types, FilterOrBool.HasValue
- 021-output-system: IOutput, Vt100Output, ColorDepth, CursorShape
- 020-ansi-formatted-text: Ansi % operator with escape neutralization
- 019-html-formatted-text: Html % operator with HTML escaping
- 018-styles-system: Style, Attrs, named/ANSI colors, transformations
- 017-filter-system: IFilter, Filter operators, Always/Never, Condition, FilterOrBool
- 016-layout-dimensions: Dimension, DimensionUtils, D alias
- 015-formatted-text: Html, Ansi, Template, PygmentsTokens, FormattedTextUtils
- 014-input-system: IInput, Vt100Parser, Win32Input/Vt100Input, pipe input
- 013-mouse-events: MouseEvent, MouseHandlers, NotImplementedOrNone
- 012-completion-system: ICompleter, 12 completers, Completion/CompleteEvent records
- 011-keys-enum: Keys enum, 151 values
- 010-search-system: SearchState, SearchOperations
- 009-validation: IValidator, 5 implementations
- 008-history: IHistory, 4 implementations
- 007-mutable-buffer: Buffer with undo/redo, navigation, completion, validation
- 006-cache-utilities: SimpleCache, FastDictCache, Memoization
- 005-auto-suggest: AutoSuggest with history-based suggestions
- 004-clipboard: IClipboard, InMemoryClipboard, DummyClipboard
- 003-selection: SelectionState, SelectionType, PasteMode
- 002-immutable-document: Immutable Document with cursor model
- 001-project-setup: Project structure, primitives, CI/CD
