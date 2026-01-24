# Stroke

A .NET 10 port of [Python Prompt Toolkit](https://github.com/prompt-toolkit/python-prompt-toolkit).

## Status

**In Development** — Core foundation complete.

### Completed

- **Project Setup** — Solution structure, .NET 10 configuration, CI/CD pipeline
- **Core Primitives** — `Point`, `Size`, `WritePosition` value types
- **Selection System** — `SelectionState`, `SelectionType`, `PasteMode` for tracking text selections
- **Document Class** — Immutable text buffer with cursor position and selection state
  - Text access (before/after cursor, current line, lines collection)
  - Line/column navigation with row/col translation
  - Word and WORD navigation (Vi-style w/W/b/B/e/E motions)
  - Character search (Vi f/F/t/T motions)
  - Selection handling (Characters, Lines, Block modes)
  - Clipboard paste operations (Emacs, ViBefore, ViAfter)
  - Paragraph navigation
  - Bracket matching
  - Flyweight caching for memory efficiency

### Up Next

- **Buffer Class** — Mutable wrapper with undo/redo stack
- **Screen & Renderer** — Sparse screen buffer with differential updates

## Requirements

- .NET 10 SDK

## Building

```bash
dotnet build
dotnet test
```

## License

MIT
