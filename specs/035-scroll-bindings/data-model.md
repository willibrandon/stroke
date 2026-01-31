# Data Model: Scroll Bindings

**Feature**: 035-scroll-bindings
**Date**: 2026-01-30

## Entities

### ScrollBindings (Static Class)

A stateless static class containing 8 scroll functions. Each function takes a `KeyPressEvent` and returns `NotImplementedOrNone?`.

| Function | Python Equivalent | Description |
|----------|------------------|-------------|
| `ScrollForward` | `scroll_forward(event, half=False)` | Scroll down by full window height |
| `ScrollBackward` | `scroll_backward(event, half=False)` | Scroll up by full window height |
| `ScrollHalfPageDown` | `scroll_half_page_down(event)` | Delegates to ScrollForward with half=true |
| `ScrollHalfPageUp` | `scroll_half_page_up(event)` | Delegates to ScrollBackward with half=true |
| `ScrollOneLineDown` | `scroll_one_line_down(event)` | Increment viewport scroll, adjust cursor if needed |
| `ScrollOneLineUp` | `scroll_one_line_up(event)` | Decrement viewport scroll, adjust cursor if needed |
| `ScrollPageDown` | `scroll_page_down(event)` | Scroll viewport to last visible line, reposition cursor |
| `ScrollPageUp` | `scroll_page_up(event)` | Scroll viewport to first visible line, reposition cursor |

**Internal method**: `ScrollForwardInternal(event, half)` and `ScrollBackwardInternal(event, half)` — shared implementation accepting the `half` parameter, called by the public wrappers.

### PageNavigationBindings (Static Class)

A stateless static class containing 3 binding loader methods. Each returns an `IKeyBindingsBase`.

| Method | Python Equivalent | Description |
|--------|------------------|-------------|
| `LoadPageNavigationBindings` | `load_page_navigation_bindings()` | Merged Emacs+Vi bindings, guarded by BufferHasFocus |
| `LoadEmacsPageNavigationBindings` | `load_emacs_page_navigation_bindings()` | Emacs-mode bindings (Ctrl-V, Escape-V, PageDown, PageUp) |
| `LoadViPageNavigationBindings` | `load_vi_page_navigation_bindings()` | Vi-mode bindings (Ctrl-F/B/D/U/E/Y, PageDown, PageUp) |

## Relationships

```
KeyPressEvent
  └─ GetApp() → Application<object>
       ├─ .Layout.CurrentWindow → Window
       │    ├─ .VerticalScroll (int, get/set)
       │    └─ .RenderInfo → WindowRenderInfo?
       │         ├─ .WindowHeight (int)
       │         ├─ .ContentHeight (int)
       │         ├─ .CursorPosition (Point)
       │         ├─ .ConfiguredScrollOffsets (ScrollOffsets)
       │         ├─ .UIContent.LineCount (int)
       │         ├─ .GetHeightForLine(int) → int
       │         ├─ .FirstVisibleLine() → int
       │         └─ .LastVisibleLine() → int
       └─ .CurrentBuffer → Buffer
            ├─ .CursorPosition (int, get/set)
            └─ .Document → Document
                 ├─ .CursorPositionRow (int)
                 ├─ .TranslateRowColToIndex(row, col) → int
                 ├─ .GetCursorDownPosition() → int
                 ├─ .GetCursorUpPosition() → int
                 └─ .GetStartOfLinePosition(afterWhitespace) → int
```

## State Transitions

Scroll functions modify two mutable state locations:

1. **Buffer.CursorPosition** (int) — the text index where the cursor is positioned
2. **Window.VerticalScroll** (int) — the viewport scroll offset

| Function | Modifies CursorPosition | Modifies VerticalScroll |
|----------|------------------------|------------------------|
| ScrollForward | Yes (absolute set) | No |
| ScrollBackward | Yes (absolute set) | No |
| ScrollHalfPageDown | Yes (absolute set) | No |
| ScrollHalfPageUp | Yes (absolute set) | No |
| ScrollOneLineDown | Conditionally (relative) | Yes (+1) |
| ScrollOneLineUp | Conditionally (relative) | Yes (-1) |
| ScrollPageDown | Yes (absolute set + relative) | Yes (absolute set) |
| ScrollPageUp | Yes (absolute set + relative) | Yes (set to 0) |

## Validation Rules

- All functions return immediately (no-op) if `window` is null or `window.RenderInfo` is null
- `ScrollForward`/`ScrollBackward`: Cursor clamped to [0, LineCount-1]
- `ScrollOneLineDown`: Only scrolls if `verticalScroll < contentHeight - windowHeight`
- `ScrollOneLineUp`: Only scrolls if `verticalScroll > 0`
- `ScrollPageDown`: Uses `max(lastVisibleLine, verticalScroll + 1)` for new scroll offset
- `ScrollPageUp`: Uses `max(0, min(firstVisibleLine, cursorRow - 1))` for cursor position
