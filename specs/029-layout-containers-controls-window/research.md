# Research: Layout Containers, UI Controls, and Window Container

**Feature Branch**: `029-layout-containers-controls-window`
**Research Date**: 2026-01-29

## Executive Summary

Research complete. All Python Prompt Toolkit APIs identified and documented. No unresolved NEEDS CLARIFICATION items. Ready for Phase 1 design.

---

## 1. Python Prompt Toolkit Source Analysis

### 1.1 Source Files Analyzed

| File | Lines | Key Classes |
|------|-------|-------------|
| `layout/containers.py` | ~2731 | Container, HSplit, VSplit, FloatContainer, Float, Window, ConditionalContainer, DynamicContainer, ScrollOffsets, ColorColumn, WindowRenderInfo |
| `layout/controls.py` | ~956 | UIControl, UIContent, FormattedTextControl, DummyControl, BufferControl, SearchBufferControl |
| `layout/margins.py` | ~305 | Margin, NumberedMargin, ScrollbarMargin, ConditionalMargin, PromptMargin |
| `layout/dimension.py` | ~113 | Dimension, sum_layout_dimensions, max_layout_dimensions |

### 1.2 API Extraction Results

#### Container Base Class (lines 82-150)

```python
class Container(metaclass=ABCMeta):
    """Base class for user interface layout."""

    @abstractmethod
    def reset() -> None

    @abstractmethod
    def preferred_width(max_available_width: int) -> Dimension

    @abstractmethod
    def preferred_height(width: int, max_available_height: int) -> Dimension

    @abstractmethod
    def write_to_screen(
        screen: Screen,
        mouse_handlers: MouseHandlers,
        write_position: WritePosition,
        parent_style: str,
        erase_bg: bool,
        z_index: int | None
    ) -> None

    @abstractmethod
    def get_children() -> list[Container]

    def is_modal() -> bool  # default: False
    def get_key_bindings() -> KeyBindingsBase | None  # default: None
```

**C# Mapping**: `IContainer` interface

#### HSplit (lines 234-469)

```python
def __init__(
    self,
    children: Sequence[AnyContainer],
    window_too_small: Container | None = None,
    align: VerticalAlign = VerticalAlign.JUSTIFY,
    padding: AnyDimension = 0,
    padding_char: str | None = None,
    padding_style: str = "",
    width: AnyDimension = None,
    height: AnyDimension = None,
    z_index: int | None = None,
    modal: bool = False,
    key_bindings: KeyBindingsBase | None = None,
    style: str | Callable[[], str] = "",
) -> None
```

**Key Method**: `_divide_heights(write_position: WritePosition) -> list[int] | None`
- Uses weighted allocation via `take_using_weights`
- Returns None if minimum sizes exceed available space

#### VSplit (lines 472-733)

Same constructor signature as HSplit but with `HorizontalAlign` instead of `VerticalAlign`.

**Key Method**: `_divide_widths(width: int) -> list[int] | None`

#### FloatContainer (lines 735-1017)

```python
def __init__(
    self,
    content: AnyContainer,
    floats: list[Float],
    modal: bool = False,
    key_bindings: KeyBindingsBase | None = None,
    style: str | Callable[[], str] = "",
    z_index: int | None = None,
) -> None
```

**Float Positioning Algorithm** (lines 845-987):
1. Horizontal positioning priority: left+width → left+right → width+right → xcursor → width only → center
2. Vertical positioning priority: top+height → top+bottom → height+bottom → ycursor → height only → center
3. Uses `screen.draw_with_z_index()` for deferred rendering when cursor-relative

#### Float (lines 1019-1097)

```python
def __init__(
    self,
    content: AnyContainer,
    top: int | None = None,
    right: int | None = None,
    bottom: int | None = None,
    left: int | None = None,
    width: int | Callable[[], int] | None = None,
    height: int | Callable[[], int] | None = None,
    xcursor: bool = False,
    ycursor: bool = False,
    attach_to_window: AnyContainer | None = None,
    hide_when_covering_content: bool = False,
    allow_cover_cursor: bool = False,
    z_index: int = 1,
    transparent: bool = False,
) -> None
```

#### Window (lines 1385-2594)

```python
def __init__(
    self,
    content: UIControl | None = None,
    width: AnyDimension = None,
    height: AnyDimension = None,
    z_index: int | None = None,
    dont_extend_width: FilterOrBool = False,
    dont_extend_height: FilterOrBool = False,
    ignore_content_width: FilterOrBool = False,
    ignore_content_height: FilterOrBool = False,
    left_margins: Sequence[Margin] | None = None,
    right_margins: Sequence[Margin] | None = None,
    scroll_offsets: ScrollOffsets | None = None,
    allow_scroll_beyond_bottom: FilterOrBool = False,
    wrap_lines: FilterOrBool = False,
    get_vertical_scroll: Callable[[Window], int] | None = None,
    get_horizontal_scroll: Callable[[Window], int] | None = None,
    always_hide_cursor: FilterOrBool = False,
    cursorline: FilterOrBool = False,
    cursorcolumn: FilterOrBool = False,
    colorcolumns: list[ColorColumn] | Callable[[], list[ColorColumn]] | None = None,
    align: WindowAlign | Callable[[], WindowAlign] = WindowAlign.LEFT,
    style: str | Callable[[], str] = "",
    char: str | Callable[[], str] | None = None,
    get_line_prefix: GetLinePrefixCallable | None = None,
) -> None
```

**Mutable State**:
- `vertical_scroll: int` - requires Lock
- `horizontal_scroll: int` - requires Lock
- `vertical_scroll_2: int` - sub-line scroll for wrapped lines
- `render_info: WindowRenderInfo | None` - cached last render

**Key Methods**:
- `_scroll()` - Dispatcher for scroll algorithm
- `_scroll_when_linewrapping()` - Complex algorithm for wrapped mode
- `_scroll_without_linewrapping()` - Simpler non-wrapped algorithm
- `_copy_body()` - Lines 1923-2163, renders content with alignment
- `_highlight_cursorlines()` - Applies cursorline/cursorcolumn styles

#### UIControl (lines 63-137)

```python
class UIControl(metaclass=ABCMeta):
    @abstractmethod
    def create_content(width: int, height: int) -> UIContent

    def reset() -> None  # default: pass
    def preferred_width(max_available_width: int) -> int | None  # default: None
    def preferred_height(width, max_available_height, wrap_lines, get_line_prefix) -> int | None
    def is_focusable() -> bool  # default: False
    def mouse_handler(mouse_event: MouseEvent) -> NotImplementedOrNone
    def move_cursor_down() -> None
    def move_cursor_up() -> None
    def get_key_bindings() -> KeyBindingsBase | None
    def get_invalidate_events() -> Iterable[Event]
```

#### UIContent (lines 139-248)

```python
def __init__(
    self,
    get_line: Callable[[int], StyleAndTextTuples] = (lambda i: []),
    line_count: int = 0,
    cursor_position: Point | None = None,
    menu_position: Point | None = None,
    show_cursor: bool = True,
) -> None
```

**Key Method**: `get_height_for_line(lineno, width, get_line_prefix, slice_stop=None) -> int`
- Calculates wrapped line height

#### BufferControl (lines 493-927)

```python
def __init__(
    self,
    buffer: Buffer | None = None,
    input_processors: list[Processor] | None = None,
    include_default_input_processors: bool = True,
    lexer: Lexer | None = None,
    preview_search: FilterOrBool = False,
    focusable: FilterOrBool = True,
    search_buffer_control: SearchBufferControl | Callable[[], SearchBufferControl] | None = None,
    menu_position: Callable[[], int | None] | None = None,
    focus_on_click: FilterOrBool = False,
    key_bindings: KeyBindingsBase | None = None,
) -> None
```

**Default Input Processors** (when include_default_input_processors=True):
1. HighlightSearchProcessor
2. HighlightIncrementalSearchProcessor
3. HighlightSelectionProcessor
4. DisplayMultipleCursors

**Note**: Input processors deferred to future feature.

#### SearchBufferControl (lines 929-956)

Extends BufferControl with `ignore_case: FilterOrBool = False` and stores `searcher_search_state`.

#### Margin (lines 32-67)

```python
class Margin(metaclass=ABCMeta):
    @abstractmethod
    def get_width(get_ui_content: Callable[[], UIContent]) -> int

    @abstractmethod
    def create_margin(
        window_render_info: WindowRenderInfo,
        width: int,
        height: int
    ) -> StyleAndTextTuples
```

---

## 2. Alignment Enums

### VerticalAlign
- `TOP` → `Top` - Align to top
- `CENTER` → `Center` - Center vertically
- `BOTTOM` → `Bottom` - Align to bottom
- `JUSTIFY` → `Justify` - Distribute evenly (default for HSplit)

### HorizontalAlign
- `LEFT` → `Left` - Align to left
- `CENTER` → `Center` - Center horizontally
- `RIGHT` → `Right` - Align to right
- `JUSTIFY` → `Justify` - Distribute evenly (default for VSplit)

### WindowAlign
- `LEFT` → `Left` - Left-align content (default)
- `CENTER` → `Center` - Center content
- `RIGHT` → `Right` - Right-align content

---

## 3. Size Division Algorithm

### Algorithm Overview

Used by HSplit._divide_heights() and VSplit._divide_widths():

```
INPUT:
  - available_space: int
  - dimensions: list[Dimension]

OUTPUT:
  - sizes: list[int] or None (if minimum sizes exceed space)

ALGORITHM:
PHASE 1 - Initialize with minimum sizes
  sizes[i] = dimensions[i].min
  if sum(sizes) > available_space:
    return None  # "window too small"

PHASE 2 - Grow to preferred sizes (weighted)
  remaining = available_space - sum(sizes)
  while remaining > 0:
    # Find items that can grow (below preferred)
    growable = [i for i in items if sizes[i] < dimensions[i].preferred]
    if not growable:
      break
    # Select item via weighted round-robin (take_using_weights)
    i = next_weighted(growable, dimensions[i].weight)
    sizes[i] += 1
    remaining -= 1

PHASE 3 - Grow to maximum sizes (weighted)
  while remaining > 0:
    # Find items that can grow (below max)
    growable = [i for i in items if sizes[i] < dimensions[i].max]
    if not growable:
      break
    i = next_weighted(growable, dimensions[i].weight)
    sizes[i] += 1
    remaining -= 1

return sizes
```

### TakeUsingWeights Implementation

Already exists in `Stroke.Core.CollectionUtils.TakeUsingWeights`:

```csharp
public static IEnumerable<T> TakeUsingWeights<T>(
    IReadOnlyList<T> items,
    IReadOnlyList<double> weights)
```

---

## 4. Thread Safety Requirements

### Classes with Mutable State

| Class | Mutable Fields | Synchronization |
|-------|---------------|-----------------|
| Window | vertical_scroll, horizontal_scroll, vertical_scroll_2, render_info | Lock |
| BufferControl | _fragment_cache, _content_cache, _last_click_timestamp | Lock |
| HSplit | _children_cache, _remaining_space_window | Lock (minimal) |
| VSplit | _children_cache, _remaining_space_window | Lock (minimal) |

### Immutable Types (No Sync Needed)

- UIContent
- Float
- ScrollOffsets
- ColorColumn
- WindowRenderInfo (readonly after creation)

---

## 5. Decisions Log

### Decision 1: Input Processors Deferral

**Decision**: BufferControl will stub input processor integration

**Rationale**:
- Input processors (HighlightSearchProcessor, etc.) are a substantial subsystem
- BufferControl is functional without them (just no search/selection highlighting)
- Cleaner to implement processors in dedicated feature

**Alternative Rejected**: Implement full processor pipeline now
- Would add 500+ lines and delay core layout functionality

### Decision 2: Margin System Scope

**Decision**: Include NumberedMargin, ScrollbarMargin, ConditionalMargin, PromptMargin

**Rationale**:
- Margins are integral to Window functionality (spec FR-026)
- User stories 8 explicitly requires margins
- Without margins, Window.left_margins/right_margins unusable

### Decision 3: MagicContainer Protocol

**Decision**: Implement `AnyContainer` struct with implicit conversions

**Rationale**:
- Python uses `__pt_container__()` protocol for duck typing
- C# implicit conversions provide similar ergonomics
- Matches existing `AnyFormattedText`, `AnyDimension` patterns

### Decision 4: GetLinePrefixCallable

**Decision**: Define delegate type `GetLinePrefixCallable`

**Signature**:
```csharp
public delegate IReadOnlyList<StyleAndTextTuple> GetLinePrefixCallable(
    int lineNumber,
    int wrapCount);
```

**Rationale**: Matches Python's `Callable[[int, int], StyleAndTextTuples]` for continuation prompts.

---

## 6. Dependencies Verified

### Existing Types Confirmed

| Type | Namespace | Feature |
|------|-----------|---------|
| Screen | Stroke.Layout | 028 |
| Char | Stroke.Layout | 028 |
| WritePosition | Stroke.Layout | 028 |
| Dimension | Stroke.Layout | 016 |
| DimensionUtils | Stroke.Layout | 016 |
| IWindow | Stroke.Layout | 028 |
| MouseHandlers | Stroke.Layout | 013 |
| Buffer | Stroke.Core | 007 |
| Document | Stroke.Core | 002 |
| IFilter | Stroke.Filters | 017 |
| FilterOrBool | Stroke.Filters | 017 |
| StyleAndTextTuple | Stroke.FormattedText | 015 |
| AnyFormattedText | Stroke.FormattedText | 015 |
| IKeyBindingsBase | Stroke.KeyBinding | 022 |
| ILexer | Stroke.Lexers | 025 |
| UnicodeWidth | Stroke.Core | 024 |
| CollectionUtils.TakeUsingWeights | Stroke.Core | 024 |
| SimpleCache | Stroke.Core | 006 |
| Point | Stroke.Core.Primitives | 001 |
| SearchState | Stroke.Core | 010 |
| MouseEvent | Stroke.Input | 013 |
| NotImplementedOrNone | Stroke.KeyBinding | 022 |
| Event | Stroke.Core | 024 |

### Types to Add (New in Feature 029)

| Type | Namespace | Python Equivalent |
|------|-----------|-------------------|
| IContainer | Stroke.Layout.Containers | Container ABC |
| AnyContainer | Stroke.Layout.Containers | AnyContainer type alias |
| ContainerUtils | Stroke.Layout.Containers | to_container, to_window, is_container |
| HSplit | Stroke.Layout.Containers | HSplit |
| VSplit | Stroke.Layout.Containers | VSplit |
| FloatContainer | Stroke.Layout.Containers | FloatContainer |
| Float | Stroke.Layout.Containers | Float |
| ConditionalContainer | Stroke.Layout.Containers | ConditionalContainer |
| DynamicContainer | Stroke.Layout.Containers | DynamicContainer |
| VerticalAlign | Stroke.Layout.Containers | VerticalAlign enum |
| HorizontalAlign | Stroke.Layout.Containers | HorizontalAlign enum |
| WindowAlign | Stroke.Layout.Containers | WindowAlign enum |
| IUIControl | Stroke.Layout.Controls | UIControl ABC |
| UIContent | Stroke.Layout.Controls | UIContent |
| DummyControl | Stroke.Layout.Controls | DummyControl |
| FormattedTextControl | Stroke.Layout.Controls | FormattedTextControl |
| BufferControl | Stroke.Layout.Controls | BufferControl |
| SearchBufferControl | Stroke.Layout.Controls | SearchBufferControl |
| Window | Stroke.Layout.Windows | Window |
| WindowRenderInfo | Stroke.Layout.Windows | WindowRenderInfo |
| ScrollOffsets | Stroke.Layout.Windows | ScrollOffsets |
| ColorColumn | Stroke.Layout.Windows | ColorColumn |
| GetLinePrefixCallable | Stroke.Layout.Windows | GetLinePrefixCallable |
| IMargin | Stroke.Layout.Margins | Margin ABC |
| NumberedMargin | Stroke.Layout.Margins | NumberedMargin |
| ScrollbarMargin | Stroke.Layout.Margins | ScrollbarMargin |
| ConditionalMargin | Stroke.Layout.Margins | ConditionalMargin |
| PromptMargin | Stroke.Layout.Margins | PromptMargin |

---

## 7. Risk Assessment

### Low Risk
- Alignment enums: Simple, well-defined
- Float positioning: Algorithm clear from Python source
- ColorColumn, ScrollOffsets: Simple data classes

### Medium Risk
- Window scroll algorithms: Complex but well-documented in Python
- BufferControl.mouse_handler: Double-click detection needs testing
- UIContent.get_height_for_line: Line wrapping calculations

### Mitigated
- Window size (1200+ LOC in Python): Using C# partial classes
- Thread safety: Lock pattern established in prior features

---

## 8. Unresolved Items

**None** - All research complete. Ready for Phase 1 design.
