# Data Model: Progress Bar and Print Text Examples

**Feature**: 068-progressbar-printtext-examples
**Date**: 2026-02-07

## Overview

This feature creates example applications, not library code. There are no new data models, entities, or state machines to define. The examples consume existing Stroke library entities documented below for reference.

## Consumed Entities (from existing Stroke library)

### FormattedTextOutput (Stroke.Shortcuts)

| Method | Signature | Used By |
|--------|-----------|---------|
| Print | `Print(AnyFormattedText, style?, colorDepth?, ...)` | All 9 PrintText examples |
| PrintContainer | `PrintContainer(AnyContainer, style?, ...)` | PrintFrame |

### Formatting Types (Stroke.FormattedText)

| Type | Description | Used By |
|------|-------------|---------|
| `Html` | HTML-like markup (`<b>`, `<ansired>`, `<style>`) | Html, PrintFormattedText, NamedColors, ProgressBar titles |
| `Ansi` | Raw ANSI escape sequences | Ansi, PrintFormattedText, LogoAnsiArt |
| `FormattedText` | List of `(style, text)` tuples | AnsiColors, PrintFormattedText, TrueColorDemo |
| `PygmentsTokens` | List of `(Token, text)` tuples | PygmentsTokens |
| `AnyFormattedText` | Union type for all formatted text | All examples via Print() |

### Style System (Stroke.Styles)

| Type | Description | Used By |
|------|-------------|---------|
| `Style.FromDict()` | Create style from dictionary | PrintFormattedText, Styled1, StyledAptGet, Styled2, StyledTqdm1, StyledTqdm2 |
| `NamedColors.Colors` | Dictionary of all named colors | NamedColors |

### Output System (Stroke.Output)

| Type | Description | Used By |
|------|-------------|---------|
| `ColorDepth.Depth4Bit` | 16 ANSI colors | TrueColorDemo, NamedColors |
| `ColorDepth.Depth8Bit` | 256 colors | TrueColorDemo, NamedColors |
| `ColorDepth.Depth24Bit` | True color (24-bit) | TrueColorDemo, NamedColors |

### Widgets (Stroke.Widgets)

| Type | Description | Used By |
|------|-------------|---------|
| `Frame` | Bordered container with title | PrintFrame |
| `TextArea` | Multi-line text widget | PrintFrame |

### ProgressBar API (Feature 71 — not yet implemented)

| Type | Description | Used By |
|------|-------------|---------|
| `ProgressBar` | Context manager for progress display | All 15 ProgressBar examples |
| `ProgressBarCounter<T>` | Iteration wrapper with label/progress | All 15 ProgressBar examples |
| `Formatter` subclasses | Visual elements (Bar, Label, etc.) | Styled examples |
| `Rainbow` | Rainbow gradient wrapper | StyledRainbow |

### Key Bindings (Stroke.KeyBinding)

| Type | Description | Used By |
|------|-------------|---------|
| `KeyBindings` | Key binding registry | CustomKeyBindings |
| `PatchStdout` | Thread-safe stdout proxy | CustomKeyBindings |

## Entity Relationships

```text
Example (static class)
  └── uses → FormattedTextOutput.Print() / PrintContainer()
               └── accepts → AnyFormattedText (Html | Ansi | FormattedText | PygmentsTokens | string)
               └── accepts → Style (optional)
               └── accepts → ColorDepth (optional)

Example (ProgressBar, static class)
  └── uses → ProgressBar (IAsyncDisposable)
               └── creates → ProgressBarCounter<T> via Iterate()
               └── accepts → Formatter[] (optional, for styled examples)
               └── accepts → KeyBindings (optional, for CustomKeyBindings)
               └── accepts → Html title / bottomToolbar (optional)
```

## State Transitions

No state machines are introduced. ProgressBar examples use a simple lifecycle:

```text
[Created] → [Running (iterating)] → [Completed/Cancelled]
```

PrintText examples have no state — they produce output and exit.
