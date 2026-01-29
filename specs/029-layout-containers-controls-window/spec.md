# Feature Specification: Layout Containers, UI Controls, and Window Container

**Feature Branch**: `029-layout-containers-controls-window`
**Created**: 2026-01-29
**Status**: Draft
**Input**: User description: "Implement Layout Containers (HSplit, VSplit, FloatContainer), UI Controls (BufferControl, FormattedTextControl), and Window Container with scrolling, margins, and cursor display for the Stroke terminal UI framework - a faithful port of Python Prompt Toolkit."

---

## API Mapping Reference *(Constitution I Compliance)*

### Naming Convention

All Python PTK API names are translated to C# using these rules:
- `snake_case` methods → `PascalCase` methods (e.g., `preferred_width` → `PreferredWidth`)
- `snake_case` parameters → `camelCase` parameters (e.g., `max_available_width` → `maxAvailableWidth`)
- `snake_case` properties → `PascalCase` properties (e.g., `line_count` → `LineCount`)
- Semantic meaning is preserved exactly; only case conventions change

### Python PTK `layout/containers.py` → C# Mapping

| Python Class/Function | C# Equivalent | Notes |
|-----------------------|---------------|-------|
| `Container` (ABC) | `IContainer` | Interface, not abstract class |
| `HSplit` | `HSplit` | Same |
| `VSplit` | `VSplit` | Same |
| `FloatContainer` | `FloatContainer` | Same |
| `Float` | `Float` | Same |
| `ConditionalContainer` | `ConditionalContainer` | Same |
| `DynamicContainer` | `DynamicContainer` | Same |
| `Window` | `Window` | Implements IContainer, IWindow |
| `WindowRenderInfo` | `WindowRenderInfo` | Same |
| `ScrollOffsets` | `ScrollOffsets` | Same |
| `ColorColumn` | `ColorColumn` | Same |
| `VerticalAlign` | `VerticalAlign` | Enum |
| `HorizontalAlign` | `HorizontalAlign` | Enum |
| `WindowAlign` | `WindowAlign` | Enum |
| `to_container()` | `ContainerUtils.ToContainer()` | Static utility |
| `to_window()` | `ContainerUtils.ToWindow()` | Static utility |
| `is_container()` | `ContainerUtils.IsContainer()` | Static utility |
| `AnyContainer` type alias | `AnyContainer` struct | Implicit conversions |

### Python PTK `layout/controls.py` → C# Mapping

| Python Class/Function | C# Equivalent | Notes |
|-----------------------|---------------|-------|
| `UIControl` (ABC) | `IUIControl` | Interface |
| `UIContent` | `UIContent` | Same |
| `DummyControl` | `DummyControl` | Same |
| `FormattedTextControl` | `FormattedTextControl` | Same |
| `BufferControl` | `BufferControl` | Same |
| `SearchBufferControl` | `SearchBufferControl` | Same |
| `GetLinePrefixCallable` | `GetLinePrefixCallable` | Delegate type |

### Python PTK `layout/margins.py` → C# Mapping

| Python Class/Function | C# Equivalent | Notes |
|-----------------------|---------------|-------|
| `Margin` (ABC) | `IMargin` | Interface |
| `NumberedMargin` | `NumberedMargin` | Same |
| `ScrollbarMargin` | `ScrollbarMargin` | Same |
| `ConditionalMargin` | `ConditionalMargin` | Same |
| `PromptMargin` | `PromptMargin` | Marked obsolete per Python |

---

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Create Vertical Stack Layout (Priority: P1)

A terminal application developer wants to stack multiple UI elements vertically (one above the other) to create a typical application layout with a header, main content area, and footer/status bar.

**Why this priority**: Vertical stacking is the most fundamental layout pattern for terminal applications. Without HSplit, developers cannot create even basic multi-pane layouts.

**Independent Test**: Can be fully tested by creating an HSplit with 3 children and verifying each child renders at the correct vertical position with correct height allocation.

**Acceptance Scenarios**:

1. **Given** an HSplit with 3 child containers, **When** rendered to screen, **Then** children appear stacked vertically in order from top to bottom
2. **Given** an HSplit with children of varying preferred heights, **When** space is limited, **Then** space is allocated proportionally by weight after satisfying minimum sizes
3. **Given** an HSplit with total minimum heights exceeding available space, **When** rendered, **Then** the "window too small" container is displayed instead

---

### User Story 2 - Create Horizontal Split Layout (Priority: P1)

A terminal application developer wants to place UI elements side by side horizontally (left to right) to create editor layouts with sidebars, multi-column views, or split panes.

**Why this priority**: Horizontal splitting is equally fundamental as vertical splitting for creating sophisticated layouts like file explorers, editors with sidebars, and dashboard views.

**Independent Test**: Can be fully tested by creating a VSplit with 3 children and verifying each child renders at the correct horizontal position with correct width allocation.

**Acceptance Scenarios**:

1. **Given** a VSplit with 3 child containers, **When** rendered to screen, **Then** children appear side by side from left to right
2. **Given** a VSplit with horizontal alignment set to Center, **When** children don't fill available width, **Then** children are centered horizontally with empty space on both sides
3. **Given** a VSplit with padding between children, **When** rendered, **Then** padding appears between each child with the specified character and style

---

### User Story 3 - Display Editable Text Buffer (Priority: P1)

A terminal application developer wants to display an editable text buffer with syntax highlighting, showing the buffer content with cursor position and optional selection highlighting.

**Why this priority**: BufferControl is the core control for text editing - REPLs, shells, and text editors all require this capability to function.

**Independent Test**: Can be fully tested by creating a BufferControl with a Buffer containing text, and verifying the text renders correctly with cursor position indicated.

**Acceptance Scenarios**:

1. **Given** a BufferControl with a Buffer containing multi-line text, **When** rendered, **Then** all lines display with correct styling from the lexer
2. **Given** a BufferControl with a Buffer that has a cursor position, **When** rendered, **Then** cursor position is communicated to the containing Window for display
3. **Given** a BufferControl with search highlighting enabled, **When** search text matches buffer content, **Then** matching text is highlighted

---

### User Story 4 - Window with Scrolling Support (Priority: P1)

A terminal application developer wants to wrap a UI control in a Window that provides scrolling when content exceeds the visible area, keeping the cursor visible.

**Why this priority**: Window is required to display any UI control on screen. Without Window, controls cannot be rendered. Scrolling is essential for editing documents longer than screen height.

**Independent Test**: Can be fully tested by creating a Window containing content taller than the Window height, and verifying scroll adjusts to keep cursor visible.

**Acceptance Scenarios**:

1. **Given** a Window containing content with 100 lines in a 20-line visible area, **When** cursor moves to line 50, **Then** vertical scroll adjusts to show line 50 within scroll offsets
2. **Given** a Window with scroll offsets of 3 lines top and bottom, **When** cursor approaches edge, **Then** scroll occurs before cursor reaches the very edge
3. **Given** a Window with line wrapping enabled, **When** a long line exceeds width, **Then** line wraps and height calculation accounts for wrapped segments

---

### User Story 5 - Create Floating Overlays (Priority: P2)

A terminal application developer wants to display floating elements like completion menus, tooltips, or dialogs that overlay the main content at specific positions.

**Why this priority**: Floating containers enable completion menus, dialogs, and tooltips - features expected in modern terminal applications but not strictly required for basic functionality.

**Independent Test**: Can be fully tested by creating a FloatContainer with background content and a Float, verifying the float renders on top at the specified position.

**Acceptance Scenarios**:

1. **Given** a FloatContainer with a Float positioned at left=10, top=5, **When** rendered, **Then** float content appears at column 10, row 5 overlaying background
2. **Given** a Float with xcursor=true, **When** rendered, **Then** float positions horizontally near the cursor position from the attached window
3. **Given** multiple Floats with different z-indices, **When** rendered, **Then** higher z-index floats appear on top of lower z-index floats

---

### User Story 6 - Display Static Formatted Text (Priority: P2)

A terminal application developer wants to display static formatted text like prompts, labels, or help text with styling.

**Why this priority**: FormattedTextControl provides read-only styled text display needed for prompts, status text, and labels - common but not as critical as editable content.

**Independent Test**: Can be fully tested by creating a FormattedTextControl with styled text and verifying it renders with correct formatting.

**Acceptance Scenarios**:

1. **Given** a FormattedTextControl with text containing style markup, **When** rendered, **Then** text displays with applied styles
2. **Given** a FormattedTextControl with a custom cursor position getter, **When** rendered, **Then** cursor position is set at the returned coordinates
3. **Given** a FormattedTextControl receiving a mouse click, **When** clicked fragment has a handler, **Then** handler is invoked

---

### User Story 7 - Conditional Container Visibility (Priority: P2)

A terminal application developer wants to show or hide parts of the UI based on application state (e.g., show search bar only when in search mode).

**Why this priority**: Conditional visibility enables dynamic UIs but applications can function without it by rebuilding layouts.

**Independent Test**: Can be fully tested by creating a ConditionalContainer with a filter and verifying visibility toggles based on filter state.

**Acceptance Scenarios**:

1. **Given** a ConditionalContainer with filter returning true, **When** rendered, **Then** contained content is visible
2. **Given** a ConditionalContainer with filter returning false, **When** rendered, **Then** container reports zero size and renders nothing
3. **Given** a ConditionalContainer whose filter changes from false to true, **When** re-rendered, **Then** content becomes visible

---

### User Story 8 - Window Margins (Priority: P2)

A terminal application developer wants to add margins to a Window (e.g., line numbers on the left, scroll indicator on the right).

**Why this priority**: Margins enhance user experience for editors but are not required for basic functionality.

**Independent Test**: Can be fully tested by creating a Window with left margins and verifying margins render alongside content with correct width allocation.

**Acceptance Scenarios**:

1. **Given** a Window with left margins, **When** rendered, **Then** margins appear to the left of main content
2. **Given** a Window with both left and right margins, **When** rendered, **Then** margins reduce available width for main content
3. **Given** a Window whose content scrolls, **When** rendered, **Then** margins receive correct line number information for the visible range

---

### User Story 9 - Dynamic Container Content (Priority: P3)

A terminal application developer wants a container that can change its content dynamically at runtime without rebuilding the entire layout tree.

**Why this priority**: DynamicContainer enables runtime content switching but most applications can achieve similar results through other means.

**Independent Test**: Can be fully tested by creating a DynamicContainer with a callable that returns different containers, and verifying content changes when callable returns different values.

**Acceptance Scenarios**:

1. **Given** a DynamicContainer with a callable returning Container A, **When** rendered, **Then** Container A's content appears
2. **Given** a DynamicContainer whose callable now returns Container B, **When** re-rendered, **Then** Container B's content appears
3. **Given** a DynamicContainer whose callable returns null, **When** rendered, **Then** container renders as empty

---

### User Story 10 - Cursor Line and Column Highlighting (Priority: P3)

A terminal application developer wants to highlight the current line and/or column where the cursor is positioned (like Vim's cursorline/cursorcolumn).

**Why this priority**: Visual cursor indicators are nice-to-have enhancements rather than core functionality.

**Independent Test**: Can be fully tested by creating a Window with cursorline enabled and verifying the entire row containing the cursor has the highlight style applied.

**Acceptance Scenarios**:

1. **Given** a Window with cursorline=true, **When** rendered, **Then** the row containing the cursor has cursor-line style applied
2. **Given** a Window with cursorcolumn=true, **When** rendered, **Then** the column containing the cursor has cursor-column style applied
3. **Given** a Window with colorcolumns at positions 80 and 120, **When** rendered, **Then** those columns have the color-column style applied

---

### Edge Cases

| Edge Case | Expected Behavior | Relevant FR |
|-----------|-------------------|-------------|
| HSplit/VSplit has zero children | Render empty; PreferredWidth/Height return `Dimension.Exact(0)` | FR-002, FR-003 |
| Negative padding values | Treat as zero (no negative spacing) | FR-004 |
| Float z-index < 1 | Default to z-index = 1 | FR-009 |
| BufferControl with null Buffer | Create new empty Buffer instance | FR-019 |
| Window with null content | Use DummyControl | FR-024 |
| Float with left AND right without width | Calculate `width = availableWidth - left - right` | FR-007 |
| Scroll position exceeds content height | Clamp to `max(0, contentHeight - windowHeight)` | FR-025 |
| ConditionalContainer with null filter | Default to `Always` (always visible) | FR-010 |
| DynamicContainer with null callable | Render empty (DummyControl equivalent) | FR-011 |
| DynamicContainer callable returns null | Render empty (DummyControl equivalent) | FR-011 |
| Margin GetWidth with empty content | Return minimum width (e.g., 1 for line numbers) | FR-038 |
| ColorColumn position beyond window width | Ignore (do not render column outside visible area) | FR-033 |

### Wide Character (CJK) Handling

All width calculations in containers and controls MUST use character display width, not character count:

- **UnicodeWidth.GetWidth(char)**: Returns 1 for ASCII, 2 for CJK ideographs and other wide characters
- **UnicodeWidth.GetWidth(string)**: Sum of individual character widths
- **Affected components**:
  - HSplit/VSplit padding character width calculation
  - Window horizontal scroll calculations
  - BufferControl cursor positioning
  - UIContent.GetHeightForLine wrapped height calculation
  - NumberedMargin width calculation (line numbers are ASCII, but content may be CJK)
  - All margin content rendering

**Example**: String "Hello你好" has character count 7 but display width 9 (5 ASCII + 2×2 CJK)

## Requirements *(mandatory)*

### Functional Requirements

#### Container System

- **FR-001**: System MUST provide an IContainer interface defining Reset, PreferredWidth, PreferredHeight, WriteToScreen, IsModal, GetKeyBindings, and GetChildren methods
- **FR-002**: System MUST provide HSplit container that stacks children vertically with configurable alignment via `VerticalAlign` enum:
  - `Top` - Align children to top, empty space at bottom
  - `Center` - Center children vertically, empty space split top/bottom
  - `Bottom` - Align children to bottom, empty space at top
  - `Justify` - Distribute children evenly using weighted allocation (default)
- **FR-003**: System MUST provide VSplit container that arranges children horizontally with configurable alignment via `HorizontalAlign` enum:
  - `Left` - Align children to left, empty space at right
  - `Center` - Center children horizontally, empty space split left/right
  - `Right` - Align children to right, empty space at left
  - `Justify` - Distribute children evenly using weighted allocation (default)
- **FR-004**: HSplit/VSplit MUST support padding between children with configurable character and style
- **FR-005**: HSplit/VSplit MUST display "window too small" container when triggered by the following condition:
  - **Triggering Condition**: `sum(child.PreferredHeight/Width.Min for all children) > availableSpace`
  - **Behavior**: Render the `WindowTooSmall` container instead of children
  - **Default WindowTooSmall**: Display message "Window too small..." centered
- **FR-006**: System MUST provide FloatContainer that renders background content with floating overlays
- **FR-007**: Float elements MUST support positioning via absolute coordinates (top, right, bottom, left) with the following conflict resolution rules:
  - If `left` AND `right` specified without `width`: Calculate `width = availableWidth - left - right`
  - If `left` AND `width` specified: Use `left` as position, ignore `right`
  - If `right` AND `width` specified: Calculate `left = availableWidth - right - width`
  - If only `width` specified: Center horizontally
  - Same rules apply vertically for `top`/`bottom`/`height`
- **FR-008**: Float elements MUST support cursor-relative positioning (xcursor, ycursor flags)
- **FR-009**: Float elements MUST have z-index >= 1 for proper layering
  - **Rationale**: Z-index 0 is reserved for background content; floats must layer above
  - **Default**: z-index = 1 when not specified or when value < 1 provided
- **FR-010**: System MUST provide ConditionalContainer that shows/hides content based on filter state
  - **Null filter behavior**: When filter is null, defaults to `Always` (always visible)
- **FR-011**: System MUST provide DynamicContainer that evaluates a callable to get current content
  - **Null callable behavior**: When callable is null, renders as empty (equivalent to DummyControl)
  - **Null return behavior**: When callable returns null, renders as empty
- **FR-012**: System MUST provide ContainerUtils with ToContainer, ToWindow, and IsContainer utilities

#### UI Controls

- **FR-013**: System MUST provide IUIControl interface defining Reset, PreferredWidth, PreferredHeight, IsFocusable, CreateContent, MouseHandler, MoveCursorDown, MoveCursorUp, GetKeyBindings, and GetInvalidateEvents methods
- **FR-014**: System MUST provide UIContent class representing control output with line getter, line count, cursor position, menu position, and show cursor flag
- **FR-015**: UIContent MUST calculate wrapped line height via `GetHeightForLine(lineNo, width, getLinePrefix, sliceStop?)` using:
  ```
  ALGORITHM GetHeightForLine:
  INPUT: lineNo, width, getLinePrefix (optional), sliceStop (optional)

  1. Get line content: fragments = GetLine(lineNo)
  2. If sliceStop provided, truncate fragments at sliceStop characters
  3. Calculate total character width using UnicodeWidth (handles CJK)
  4. If getLinePrefix provided:
     - Add prefix width for first row
     - Add continuation prefix width for subsequent wrapped rows
  5. Calculate rows = ceil(totalWidth / width)
  6. Return max(1, rows)  // Always at least 1 row
  ```
- **FR-016**: System MUST provide FormattedTextControl for displaying styled static text
- **FR-017**: FormattedTextControl MUST support mouse click handling with fragment handlers:
  - **Event bubbling**: Mouse events propagate from fragments with handlers to control
  - **Return values**: Handler returns `NotImplementedOrNone.None` if handled, `NotImplementedOrNone.NotImplemented` to bubble
  - **Fragment handlers**: Stored in StyleAndTextTuple's metadata, invoked on click at that fragment's position
- **FR-018**: System MUST provide DummyControl that renders empty content
- **FR-019**: System MUST provide BufferControl for displaying editable Buffer content
  - **Null Buffer behavior**: When buffer parameter is null, create a new empty Buffer instance
- **FR-020**: BufferControl MUST integrate with lexer for syntax highlighting:
  - **Null lexer behavior**: When lexer is null, render text with empty style (no highlighting)
  - **Lexer provided**: Call lexer.Lex(document) to get styled fragments per line
- **FR-021**: BufferControl MUST support input processors for highlighting search, selection, and multiple cursors (deferred to future feature; stub implementation)
- **FR-022**: BufferControl MUST handle mouse events with the following click behaviors:
  - **Single click**: Set cursor position to clicked character
  - **Double click**: Select word at clicked position
    - Word boundary: Characters matching `\w` regex (alphanumeric + underscore)
    - Selection extends from word start to word end (exclusive)
  - **Triple click**: Select entire line at clicked position
    - Selection extends from line start to line end (inclusive of newline)
  - **Click timing**: Double/triple click detected within 500ms of previous click at same position
- **FR-023**: System MUST provide SearchBufferControl specialized for search input:
  - **Extends**: BufferControl with additional `IgnoreCase` filter property
  - **Purpose**: Dedicated control for search input fields (e.g., incremental search)
  - **SearcherSearchState**: Maintains reference to associated SearchState for highlighting matches
  - **Default behavior**: Non-focusable by default (focus managed by parent)

#### Window Container

- **FR-024**: System MUST provide Window container that wraps a UIControl with scrolling support
  - **Null content behavior**: When content is null, use DummyControl
- **FR-025**: Window MUST calculate scroll position to keep cursor visible within scroll offsets using two algorithms:

  **Algorithm: Scroll Without Line Wrapping**
  ```
  1. Get cursor row from UIContent.CursorPosition.Y
  2. Apply scroll offsets: visibleTop = scrollOffsets.Top, visibleBottom = height - scrollOffsets.Bottom
  3. If cursorRow < verticalScroll + visibleTop:
       verticalScroll = cursorRow - scrollOffsets.Top
  4. If cursorRow >= verticalScroll + visibleBottom:
       verticalScroll = cursorRow - height + scrollOffsets.Bottom + 1
  5. Clamp: verticalScroll = max(0, min(verticalScroll, lineCount - height))
  ```

  **Algorithm: Scroll With Line Wrapping**
  ```
  1. Calculate total wrapped height by summing GetHeightForLine for each line
  2. Find which wrapped row the cursor is on (accounting for line prefixes)
  3. Apply scroll offsets considering wrapped row, not source line
  4. Handle edge case: single line taller than window uses verticalScroll2 for sub-line offset
  5. Store line-to-row mapping in WindowRenderInfo for margin rendering
  ```

- **FR-026**: Window MUST support left and right margins
- **FR-027**: Window MUST support line wrapping with height recalculation
- **FR-028**: Window MUST support cursorline, cursorcolumn, and colorcolumns highlighting with these style classes:
  - **Cursorline style**: `class:cursor-line` - Applied to entire row containing cursor
  - **Cursorcolumn style**: `class:cursor-column` - Applied to entire column containing cursor
  - **Colorcolumn style**: `class:color-column` - Applied to specified column positions
- **FR-029**: Window MUST support content alignment via `WindowAlign` enum:
  - `Left` - Left-align content (default)
  - `Center` - Center content horizontally
  - `Right` - Right-align content
- **FR-030**: Window MUST provide WindowRenderInfo with the following fields:
  - `Window` - Reference to the Window instance
  - `UIContent` - The rendered UIContent
  - `HorizontalScroll` - Current horizontal scroll position
  - `VerticalScroll` - Current vertical scroll position
  - `WindowWidth` - Width of the window content area
  - `WindowHeight` - Height of the window content area
  - `ConfiguredScrollOffsets` - The ScrollOffsets configuration
  - `VisibleLinesToRowCol` - Mapping of visible lines to (row, col) positions
  - `RowsAboveCursor` - Number of wrapped rows above cursor line
  - `FirstVisibleLine()` - Returns first visible source line number
  - `LastVisibleLine()` - Returns last visible source line number
  - `CursorPosition` - Screen position of cursor
  - `ContentHeight` - Total content height (considering wrapping)
  - `DisplayedLines` - Set of source line numbers currently displayed
- **FR-031**: Window MUST register cursor and menu positions with Screen for float positioning:
  - Call `Screen.SetCursorPosition(window, point)` to register cursor
  - Call `Screen.SetMenuPosition(window, point)` to register menu anchor
  - Float elements with `xcursor`/`ycursor` use these positions for placement
- **FR-032**: System MUST provide ScrollOffsets class for configuring scroll behavior with these defaults:
  - `Top` = 0 (no top offset)
  - `Bottom` = 0 (no bottom offset)
  - `Left` = 0 (no left offset)
  - `Right` = 0 (no right offset)
- **FR-033**: System MUST provide ColorColumn class for column highlighting configuration with:
  - `Position` - Column index (0-based)
  - `Style` - Style string to apply (default: `"class:color-column"`)

#### Size Division Algorithm

- **FR-034**: HSplit/VSplit MUST calculate child dimensions using weighted allocation algorithm:
  ```
  ALGORITHM DivideSizes(availableSpace, dimensions[]):
  INPUT:
    - availableSpace: int (total available width/height)
    - dimensions: list[Dimension] (preferred dimensions from children)

  OUTPUT:
    - sizes: list[int] or null (null if minimum sizes exceed space)

  PHASE 1 - Initialize with minimum sizes:
    for i in 0..dimensions.length:
      sizes[i] = dimensions[i].Min
    if sum(sizes) > availableSpace:
      return null  // Triggers "window too small"

  PHASE 2 - Grow to preferred sizes (weighted):
    remaining = availableSpace - sum(sizes)
    while remaining > 0:
      growable = [i for i where sizes[i] < dimensions[i].Preferred]
      if growable is empty:
        break
      weights = [dimensions[i].Weight for i in growable]
      i = CollectionUtils.TakeUsingWeights(growable, weights).First()
      sizes[i] += 1
      remaining -= 1

  PHASE 3 - Grow to maximum sizes (weighted):
    while remaining > 0:
      growable = [i for i where sizes[i] < dimensions[i].Max]
      if growable is empty:
        break
      weights = [dimensions[i].Weight for i in growable]
      i = CollectionUtils.TakeUsingWeights(growable, weights).First()
      sizes[i] += 1
      remaining -= 1

  return sizes
  ```
- **FR-035**: Size allocation MUST first satisfy minimum sizes, then distribute remaining space by weight
- **FR-036**: Size allocation MUST fill to preferred size before distributing to max

#### Margin System

- **FR-038**: System MUST provide IMargin interface with GetWidth and CreateMargin methods
- **FR-039**: System MUST provide NumberedMargin for line number display:
  - **Relative mode**: When `Relative` filter is true, show line numbers relative to cursor (e.g., 3, 2, 1, [5], 1, 2, 3)
  - **Absolute mode**: When `Relative` filter is false, show actual line numbers (default)
  - **Tilde display**: When `DisplayTildes` filter is true and below document end, show `~` (Vi style)
  - **Width calculation**: `GetWidth = digits(lineCount) + 1` for padding
  - **Current line highlighting**: Apply `class:line-number,current-line-number` to cursor line
- **FR-040**: System MUST provide ScrollbarMargin for vertical scrollbar display:
  - **Thumb position**: `thumbStart = (verticalScroll / contentHeight) * visibleHeight`
  - **Thumb size**: `thumbSize = max(1, (visibleHeight / contentHeight) * visibleHeight)`
  - **Arrow display**: When `DisplayArrows` filter is true, show up/down arrows at top/bottom
  - **Arrow symbols**: Configurable via `UpArrowSymbol` (default: "^") and `DownArrowSymbol` (default: "v")
- **FR-041**: System MUST provide ConditionalMargin that shows/hides a margin based on filter
- **FR-042**: System MUST provide PromptMargin for prompt/continuation display (marked `[Obsolete]`)

#### Margin Style Classes

- **FR-043**: Margins MUST use the following style classes:
  - `class:line-number` - Line number text
  - `class:line-number,current-line-number` - Current line number (cursor line)
  - `class:tilde` - Tilde characters below document end
  - `class:scrollbar.background` - Scrollbar track
  - `class:scrollbar.button` - Scrollbar thumb (draggable indicator)
  - `class:scrollbar.arrow` - Scrollbar arrow buttons

#### Thread Safety

- **FR-037**: All containers with mutable state MUST be thread-safe using appropriate synchronization

**Mutable State Inventory** (requires Lock protection):

| Class | Mutable Fields | Atomicity Scope |
|-------|---------------|-----------------|
| Window | `_verticalScroll`, `_horizontalScroll`, `_verticalScroll2`, `_renderInfo` | Individual property access atomic; compound read-modify-write requires external sync |
| BufferControl | `_fragmentCache`, `_contentCache`, `_lastClickTimestamp`, `_lastClickPosition` | Cache operations atomic; click detection atomic |
| HSplit | `_childrenCache`, `_remainingSpaceWindow` | Cache invalidation atomic |
| VSplit | `_childrenCache`, `_remainingSpaceWindow` | Cache invalidation atomic |

**Immutable Types** (no synchronization needed):
- UIContent, Float, ScrollOffsets, ColorColumn, WindowRenderInfo (readonly after creation)

**Atomicity Boundaries**:
- Individual property get/set operations are atomic
- Compound operations (e.g., read scroll → compute → write scroll) are NOT atomic; caller must synchronize if needed
- Render cycle (WriteToScreen) should be called from single thread; internal state protected

### Key Entities

#### Containers
- **IContainer**: Base interface for layout containers with preferred sizing, screen writing, and child management
- **AnyContainer**: Union struct with implicit conversions from IContainer, Window, or IMagicContainer
- **HSplit**: Vertical stacking container with alignment and padding
- **VSplit**: Horizontal arrangement container with alignment and padding
- **FloatContainer**: Container with background content and floating overlays
- **Float**: Floating element with positioning, size, z-index, and transparency
- **ConditionalContainer**: Container with filter-based visibility
- **DynamicContainer**: Container with runtime content switching
- **ContainerUtils**: Static utilities (ToContainer, ToWindow, IsContainer)

#### Enums
- **VerticalAlign**: Top, Center, Bottom, Justify (for HSplit)
- **HorizontalAlign**: Left, Center, Right, Justify (for VSplit)
- **WindowAlign**: Left, Center, Right (for Window content alignment)

#### Controls
- **IUIControl**: Base interface for renderable controls
- **UIContent**: Output from a control containing lines, cursor, and menu position
- **FormattedTextControl**: Read-only styled text display control
- **DummyControl**: Empty placeholder control
- **BufferControl**: Editable buffer display control with lexer integration
- **SearchBufferControl**: Specialized buffer control for search input with IgnoreCase support

#### Window
- **Window**: Container wrapping a control with scrolling, margins, and cursor display
- **WindowRenderInfo**: Render state including line mappings, scroll info, and cursor position
- **ScrollOffsets**: Configuration for scroll behavior (top, bottom, left, right)
- **ColorColumn**: Configuration for column highlighting (position, style)
- **GetLinePrefixCallable**: Delegate for line prefix/continuation generation

#### Margins
- **IMargin**: Base interface for Window margins (GetWidth, CreateMargin)
- **NumberedMargin**: Line numbers with relative mode and tilde support
- **ScrollbarMargin**: Vertical scrollbar with configurable arrows
- **ConditionalMargin**: Filter-based margin visibility
- **PromptMargin**: Prompt/continuation display (obsolete)

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Developers can create nested HSplit/VSplit layouts up to 10 levels deep without performance degradation
  - **Test method**: Create 10-level nested HSplit/VSplit tree, measure WriteToScreen time
  - **Pass criteria**: Time does not exceed 2x the time for a 5-level tree (linear scaling acceptable)
- **SC-002**: Rendering a layout with 50 containers completes in under 16ms (60fps capable)
  - **Test method**: Create flat HSplit with 50 Window children, call WriteToScreen, measure elapsed time
  - **Pass criteria**: Average of 100 iterations < 16ms on reference hardware
- **SC-003**: BufferControl displays buffers with 10,000+ lines with scroll operations completing in under 16ms
  - **Test method**: Create BufferControl with 10,000-line buffer, measure scroll update time when cursor moves from line 1 to line 5000
  - **Pass criteria**: Scroll position recalculation + content copy < 16ms
- **SC-004**: Float positioning accuracy is within 1 character cell of specified coordinates
  - **Test method**: Place Float at (left=10, top=5), verify rendered position
  - **Pass criteria**: Content starts at column 10 (±1), row 5 (±1)
- **SC-005**: Window scroll maintains cursor visibility 100% of the time when cursor moves
  - **Test method**: Move cursor through entire document, verify cursor row is always within visible range
  - **Pass criteria**: No test case where cursor is outside visible range after scroll calculation
- **SC-006**: All containers correctly calculate preferred dimensions matching Python Prompt Toolkit behavior
  - **Test method**: Port Python PTK dimension tests with known inputs/outputs
  - **Pass criteria**: All ported tests pass
- **SC-007**: Unit test coverage achieves 80% for all container, control, and window classes
  - **Test method**: Run coverage analysis via `dotnet test --collect:"XPlat Code Coverage"`
  - **Pass criteria**: Line coverage >= 80% for `Stroke.Layout.Containers`, `Stroke.Layout.Controls`, `Stroke.Layout.Windows`, `Stroke.Layout.Margins` namespaces
- **SC-008**: Size division algorithm distributes space identically to Python Prompt Toolkit for known test vectors
  - **Test method**: Create test vectors from Python PTK: known inputs (dimensions, available space) → expected outputs (sizes)
  - **Pass criteria**: C# algorithm produces identical output for all test vectors
  - **Test vectors**: Document at least 10 test cases covering edge cases (all min, all max, mixed weights, zero weights)
- **SC-009**: All public APIs match Python Prompt Toolkit signatures adjusted only for C# naming conventions
  - **Test method**: Cross-reference with API Mapping tables in this spec
  - **Pass criteria**: No missing APIs, no semantic changes beyond naming
