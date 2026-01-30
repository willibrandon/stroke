# Data Model: Completion Menus

**Feature Branch**: `033-completion-menus`
**Date**: 2026-01-30

## Entities

### CompletionsMenuControl

**Type**: Internal UI control (implements `IUIControl`)
**State**: Stateless (no instance fields)
**Thread Safety**: Inherently thread-safe (stateless)

| Property/Constant | Type | Description |
|-------------------|------|-------------|
| `MinWidth` | `const int = 7` | Minimum width of the menu control (FR-013) |

**Methods**:
- `HasFocus()` → `false` (FR-016)
- `PreferredWidth(int maxAvailableWidth)` → `int?` — menu width + meta width, or 0
- `PreferredHeight(int width, int maxAvailableHeight, bool wrapLines, GetLinePrefixCallable?)` → `int?` — completion count, or 0
- `CreateContent(int width, int height)` → `UIContent` — renders completion lines
- `MouseHandler(MouseEvent)` → `NotImplementedOrNone` — click/scroll handling (FR-008)

**Internal Methods**:
- `ShowMeta(CompletionState)` → `bool`
- `GetMenuWidth(int maxWidth, CompletionState)` → `int`
- `GetMenuMetaWidth(int maxWidth, CompletionState)` → `int`
- `GetMenuItemMetaFragments(Completion, bool isCurrent, int width)` → `IReadOnlyList<StyleAndTextTuple>`

### CompletionsMenu

**Type**: Public container (extends `ConditionalContainer`)
**State**: Immutable after construction (filter composition)
**Thread Safety**: Inherently thread-safe (immutable, delegates to thread-safe base)

| Constructor Parameter | Type | Default | Description |
|----------------------|------|---------|-------------|
| `maxHeight` | `int?` | `null` | Maximum visible rows |
| `scrollOffset` | `int` or `Func<int>` | `0` | Scroll offset for top/bottom |
| `extraFilter` | `FilterOrBool` | `true` | Additional visibility filter |
| `displayArrows` | `FilterOrBool` | `false` | Show scrollbar arrows |
| `zIndex` | `int` | `10^8` | Z-index for overlay positioning |

**Filter Logic**: `extraFilter & HasCompletions & ~IsDone`

### MultiColumnCompletionMenuControl

**Type**: Internal UI control (implements `IUIControl`)
**State**: Mutable (scroll position, cached widths, render state)
**Thread Safety**: Lock required (Constitution XI)

| Field | Type | Description |
|-------|------|-------------|
| `MinRows` | `int` | Minimum rows (constructor parameter, immutable) |
| `SuggestedMaxColumnWidth` | `int` | Max column width hint (constructor parameter, immutable) |
| `_scroll` | `int` | Current horizontal scroll position |
| `_columnWidthForCompletionState` | `ConditionalWeakTable<CompletionState, StrongBox<(int Count, int Width)>>` | Cached column widths |
| `_renderedRows` | `int` | Rows in last render |
| `_renderedColumns` | `int` | Visible columns in last render |
| `_totalColumns` | `int` | Total columns in last render |
| `_renderPosToCompletion` | `Dictionary<(int X, int Y), Completion>` | Position → completion map for mouse clicks |
| `_renderLeftArrow` | `bool` | Left arrow visible in last render |
| `_renderRightArrow` | `bool` | Right arrow visible in last render |
| `_renderWidth` | `int` | Total render width in last render |
| `RequiredMargin` | `const int = 3` | Space for arrows + padding |

**Methods**:
- `Reset()` — resets scroll to 0
- `HasFocus()` → `false` (FR-016)
- `PreferredWidth(int maxAvailableWidth)` → `int?`
- `PreferredHeight(int width, int maxAvailableHeight, bool wrapLines, GetLinePrefixCallable?)` → `int?`
- `CreateContent(int width, int height)` → `UIContent` — renders multi-column grid
- `MouseHandler(MouseEvent)` → `NotImplementedOrNone` — arrow clicks, completion clicks, scroll (FR-009)
- `GetKeyBindings()` → `IKeyBindingsBase` — Left/Right key bindings (FR-010)

**Internal Methods**:
- `GetColumnWidth(CompletionState)` → `int` — cached width computation (FR-011)

### MultiColumnCompletionsMenu

**Type**: Public container (extends `HSplit`)
**State**: Immutable after construction
**Thread Safety**: Inherently thread-safe (delegates to thread-safe base)

| Constructor Parameter | Type | Default | Description |
|----------------------|------|---------|-------------|
| `minRows` | `int` | `3` | Minimum rows for column layout |
| `suggestedMaxColumnWidth` | `int` | `30` | Suggested max column width |
| `showMeta` | `FilterOrBool` | `true` | Show meta information row |
| `extraFilter` | `FilterOrBool` | `true` | Additional visibility filter |
| `zIndex` | `int` | `10^8` | Z-index for overlay positioning |

**Children**: `[completionsWindow, metaWindow]` where:
- `completionsWindow` = `ConditionalContainer(Window(MultiColumnCompletionMenuControl(...)), filter=fullFilter)`
- `metaWindow` = `ConditionalContainer(Window(SelectedCompletionMetaControl()), filter=fullFilter & showMeta & anyCompletionHasMeta)`

### SelectedCompletionMetaControl

**Type**: Internal UI control (implements `IUIControl`)
**State**: Stateless
**Thread Safety**: Inherently thread-safe (stateless)

**Methods**:
- `PreferredWidth(int maxAvailableWidth)` → `int?` — widest meta text + 2, or full width at 30+ completions (FR-018)
- `PreferredHeight(...)` → `1`
- `CreateContent(int width, int height)` → `UIContent` — renders selected completion's meta

### MenuUtils

**Type**: Internal static utility class
**State**: None (stateless)
**Thread Safety**: Inherently thread-safe (stateless, pure functions)

**Static Methods**:
- `GetMenuItemFragments(Completion, bool isCurrent, int width, bool spaceAfter)` → `IReadOnlyList<StyleAndTextTuple>` — styled menu item fragments (FR-017)
- `TrimFormattedText(IReadOnlyList<StyleAndTextTuple>, int maxWidth)` → `(IReadOnlyList<StyleAndTextTuple> Text, int Width)` — trim with "..." ellipsis (FR-007, FR-017)

## Relationships

```text
CompletionsMenu ──inherits──> ConditionalContainer
    └── wraps Window
        └── contains CompletionsMenuControl (IUIControl)

MultiColumnCompletionsMenu ──inherits──> HSplit
    ├── ConditionalContainer (completions window)
    │   └── Window
    │       └── MultiColumnCompletionMenuControl (IUIControl)
    └── ConditionalContainer (meta window)
        └── Window
            └── SelectedCompletionMetaControl (IUIControl)

MenuUtils ──used by──> CompletionsMenuControl
MenuUtils ──used by──> MultiColumnCompletionMenuControl

All controls ──read──> AppContext.GetApp().CurrentBuffer.CompleteState
```

## Style Classes

| Style Class | Applied To |
|------------|-----------|
| `class:completion-menu` | Window style in CompletionsMenu; line style in MultiColumnCompletionMenuControl |
| `class:completion-menu.completion` | Normal completion items |
| `class:completion-menu.completion.current` | Selected completion item |
| `class:completion-menu.meta.completion` | Normal meta column items |
| `class:completion-menu.meta.completion.current` | Selected meta column item |
| `class:completion-menu.multi-column-meta` | Multi-column meta row |
| `class:completion` | Empty column cells in multi-column |
| `class:scrollbar` | Scroll arrow characters ("<", ">") |
