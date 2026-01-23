# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**Stroke** is a complete, faithful architectural port of [Python Prompt Toolkit](https://github.com/prompt-toolkit/python-prompt-toolkit) to .NET 10. It provides a cross-platform terminal UI framework for building REPLs, database shells, CLI tools, and interactive terminal applications.

- **Target Framework:** .NET 10+
- **Language:** C# 13
- **License:** MIT
- **NuGet Package:** `Stroke`

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

## Development Guidelines

### Porting from Python Prompt Toolkit

When implementing features, reference the Python source in `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/`:
- Maintain API shape parity where idiomatic
- Use C# conventions: PascalCase, async/await, ImmutableArray for readonly collections
- Prefer records for immutable data types
- Use `sealed` on classes not designed for inheritance

### Terminal Compatibility

- VT100/ANSI as primary output (Linux, macOS, Windows 10+)
- WindowsConsoleOutput fallback for legacy Windows
- Handle wide characters (CJK) via UnicodeWidth calculations
- Support mouse tracking, bracketed paste, alternate screen

### Key Bindings

- Both Emacs and Vi modes must be fully implemented
- Vi includes: navigation/insert/visual modes, operators (d/c/y), motions, repeat (.)
- Emacs includes: kill ring, transpose, incremental search
- Use Filter system for mode-conditional bindings
