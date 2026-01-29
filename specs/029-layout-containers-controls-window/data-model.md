# Data Model: Layout Containers, UI Controls, and Window Container

**Feature Branch**: `029-layout-containers-controls-window`
**Created**: 2026-01-29

---

## 1. Enumerations

### VerticalAlign

```csharp
namespace Stroke.Layout.Containers;

/// <summary>
/// Vertical alignment for HSplit container children.
/// </summary>
public enum VerticalAlign
{
    /// <summary>Align children to the top.</summary>
    Top,

    /// <summary>Center children vertically.</summary>
    Center,

    /// <summary>Align children to the bottom.</summary>
    Bottom,

    /// <summary>Distribute children evenly (default).</summary>
    Justify
}
```

### HorizontalAlign

```csharp
namespace Stroke.Layout.Containers;

/// <summary>
/// Horizontal alignment for VSplit container children.
/// </summary>
public enum HorizontalAlign
{
    /// <summary>Align children to the left.</summary>
    Left,

    /// <summary>Center children horizontally.</summary>
    Center,

    /// <summary>Align children to the right.</summary>
    Right,

    /// <summary>Distribute children evenly (default).</summary>
    Justify
}
```

### WindowAlign

```csharp
namespace Stroke.Layout.Containers;

/// <summary>
/// Content alignment within a Window.
/// </summary>
public enum WindowAlign
{
    /// <summary>Align content to the left (default).</summary>
    Left,

    /// <summary>Center content horizontally.</summary>
    Center,

    /// <summary>Align content to the right.</summary>
    Right
}
```

---

## 2. Container Interfaces

### IContainer

```csharp
namespace Stroke.Layout.Containers;

/// <summary>
/// Base interface for all layout containers.
/// </summary>
public interface IContainer
{
    /// <summary>
    /// Reset the state of the container and all children.
    /// </summary>
    void Reset();

    /// <summary>
    /// Return the preferred width for this container.
    /// </summary>
    Dimension PreferredWidth(int maxAvailableWidth);

    /// <summary>
    /// Return the preferred height for this container.
    /// </summary>
    Dimension PreferredHeight(int width, int maxAvailableHeight);

    /// <summary>
    /// Write the container content to the screen.
    /// </summary>
    void WriteToScreen(
        Screen screen,
        MouseHandlers mouseHandlers,
        WritePosition writePosition,
        string parentStyle,
        bool eraseBg,
        int? zIndex);

    /// <summary>
    /// Whether key bindings in this container are modal.
    /// </summary>
    bool IsModal { get; }

    /// <summary>
    /// Key bindings for this container.
    /// </summary>
    IKeyBindingsBase? GetKeyBindings();

    /// <summary>
    /// Return the list of direct children.
    /// </summary>
    IReadOnlyList<IContainer> GetChildren();
}
```

### AnyContainer

```csharp
namespace Stroke.Layout.Containers;

/// <summary>
/// Union type that can hold an IContainer or an object implementing __pt_container__() protocol.
/// </summary>
public readonly struct AnyContainer
{
    private readonly object _value;

    public AnyContainer(IContainer container);
    public AnyContainer(IMagicContainer magicContainer);

    public IContainer ToContainer();

    public static implicit operator AnyContainer(HSplit container);
    public static implicit operator AnyContainer(VSplit container);
    public static implicit operator AnyContainer(Window window);
    public static implicit operator AnyContainer(FloatContainer container);
    public static implicit operator AnyContainer(ConditionalContainer container);
    public static implicit operator AnyContainer(DynamicContainer container);
}

/// <summary>
/// Interface for objects that can be converted to containers (like Python's __pt_container__).
/// </summary>
public interface IMagicContainer
{
    IContainer PtContainer();
}
```

---

## 3. Container Classes

### HSplit

```csharp
namespace Stroke.Layout.Containers;

/// <summary>
/// Stacks children vertically (one above the other).
/// </summary>
public sealed class HSplit : IContainer
{
    // Constructor parameters
    public IReadOnlyList<IContainer> Children { get; }
    public IContainer? WindowTooSmall { get; }
    public VerticalAlign Align { get; }
    public Dimension Padding { get; }
    public char? PaddingChar { get; }
    public string PaddingStyle { get; }
    public Dimension? Width { get; }
    public Dimension? Height { get; }
    public int? ZIndex { get; }
    public bool Modal { get; }
    public IKeyBindingsBase? KeyBindings { get; }
    public Func<string> StyleGetter { get; }

    // IContainer implementation
    // Thread-safe: internal cache protected by Lock
}
```

### VSplit

```csharp
namespace Stroke.Layout.Containers;

/// <summary>
/// Arranges children horizontally (side by side).
/// </summary>
public sealed class VSplit : IContainer
{
    // Same structure as HSplit with HorizontalAlign instead of VerticalAlign
    public HorizontalAlign Align { get; }
    // ... other properties same as HSplit
}
```

### FloatContainer

```csharp
namespace Stroke.Layout.Containers;

/// <summary>
/// Container with background content and floating overlays.
/// </summary>
public sealed class FloatContainer : IContainer
{
    public IContainer Content { get; }
    public IReadOnlyList<Float> Floats { get; }
    public bool Modal { get; }
    public IKeyBindingsBase? KeyBindings { get; }
    public Func<string> StyleGetter { get; }
    public int? ZIndex { get; }

    // Float drawing deferred via Screen.DrawWithZIndex
}
```

### Float

```csharp
namespace Stroke.Layout.Containers;

/// <summary>
/// A floating element with positioning configuration.
/// </summary>
public sealed class Float
{
    public IContainer Content { get; }
    public int? Top { get; }
    public int? Right { get; }
    public int? Bottom { get; }
    public int? Left { get; }
    public Func<int>? WidthGetter { get; }
    public Func<int>? HeightGetter { get; }
    public bool XCursor { get; }
    public bool YCursor { get; }
    public Window? AttachToWindow { get; }
    public bool HideWhenCoveringContent { get; }
    public bool AllowCoverCursor { get; }
    public int ZIndex { get; } // default: 1
    public bool Transparent { get; }

    public int? GetWidth();
    public int? GetHeight();
}
```

### ConditionalContainer

```csharp
namespace Stroke.Layout.Containers;

/// <summary>
/// Shows or hides content based on a filter condition.
/// </summary>
public sealed class ConditionalContainer : IContainer
{
    public IContainer Content { get; }
    public IFilter Filter { get; }
    public IContainer? AlternativeContent { get; }

    // When Filter returns false:
    // - PreferredWidth/Height return Dimension.Zero()
    // - WriteToScreen does nothing
}
```

### DynamicContainer

```csharp
namespace Stroke.Layout.Containers;

/// <summary>
/// Evaluates a callable to get current content at render time.
/// </summary>
public sealed class DynamicContainer : IContainer
{
    public Func<AnyContainer?> GetContainer { get; }

    // All methods delegate to the dynamically resolved container
}
```

---

## 4. Control Interfaces

### IUIControl

```csharp
namespace Stroke.Layout.Controls;

/// <summary>
/// Base interface for UI controls that can be rendered in a Window.
/// </summary>
public interface IUIControl
{
    /// <summary>
    /// Create the content to be rendered.
    /// </summary>
    UIContent CreateContent(int width, int height);

    /// <summary>
    /// Reset control state.
    /// </summary>
    void Reset();

    /// <summary>
    /// Return preferred width, or null if any width is acceptable.
    /// </summary>
    int? PreferredWidth(int maxAvailableWidth);

    /// <summary>
    /// Return preferred height, or null if any height is acceptable.
    /// </summary>
    int? PreferredHeight(
        int width,
        int maxAvailableHeight,
        bool wrapLines,
        GetLinePrefixCallable? getLinePrefix);

    /// <summary>
    /// Whether this control can receive focus.
    /// </summary>
    bool IsFocusable { get; }

    /// <summary>
    /// Handle mouse events.
    /// </summary>
    NotImplementedOrNone MouseHandler(MouseEvent mouseEvent);

    /// <summary>
    /// Move cursor down (scroll request).
    /// </summary>
    void MoveCursorDown();

    /// <summary>
    /// Move cursor up (scroll request).
    /// </summary>
    void MoveCursorUp();

    /// <summary>
    /// Key bindings for this control.
    /// </summary>
    IKeyBindingsBase? GetKeyBindings();

    /// <summary>
    /// Events that trigger redraw.
    /// </summary>
    IEnumerable<Event<object>> GetInvalidateEvents();
}
```

---

## 5. Control Classes

### UIContent

```csharp
namespace Stroke.Layout.Controls;

/// <summary>
/// Represents the rendered content of a UIControl.
/// Immutable snapshot of control output.
/// </summary>
public sealed class UIContent
{
    public Func<int, IReadOnlyList<StyleAndTextTuple>> GetLine { get; }
    public int LineCount { get; }
    public Point? CursorPosition { get; }
    public Point? MenuPosition { get; }
    public bool ShowCursor { get; }

    /// <summary>
    /// Calculate the height of a line when wrapped to the given width.
    /// </summary>
    public int GetHeightForLine(
        int lineNo,
        int width,
        GetLinePrefixCallable? getLinePrefix,
        int? sliceStop = null);
}
```

### DummyControl

```csharp
namespace Stroke.Layout.Controls;

/// <summary>
/// Empty placeholder control that renders nothing.
/// </summary>
public sealed class DummyControl : IUIControl
{
    // Returns UIContent with empty lines
    // IsFocusable = false
}
```

### FormattedTextControl

```csharp
namespace Stroke.Layout.Controls;

/// <summary>
/// Displays static formatted text with optional cursor.
/// </summary>
public sealed class FormattedTextControl : IUIControl
{
    public AnyFormattedText Text { get; }
    public string Style { get; }
    public IFilter Focusable { get; }
    public IKeyBindingsBase? KeyBindings { get; }
    public bool ShowCursor { get; }
    public bool Modal { get; }
    public Func<Point?>? GetCursorPosition { get; }

    // Supports [SetCursorPosition] and [SetMenuPosition] markers
    // Mouse handler invokes fragment handlers
}
```

### BufferControl

```csharp
namespace Stroke.Layout.Controls;

/// <summary>
/// Displays an editable Buffer with syntax highlighting and mouse support.
/// </summary>
public sealed class BufferControl : IUIControl
{
    public Buffer Buffer { get; }
    public IReadOnlyList<IProcessor>? InputProcessors { get; }
    public bool IncludeDefaultInputProcessors { get; }
    public ILexer? Lexer { get; }
    public IFilter PreviewSearch { get; }
    public IFilter Focusable { get; }
    public Func<SearchBufferControl?>? SearchBufferControlGetter { get; }
    public Func<int?>? MenuPosition { get; }
    public IFilter FocusOnClick { get; }
    public IKeyBindingsBase? KeyBindings { get; }

    // Thread-safe: caches protected by Lock
    // Double-click word selection, triple-click line selection
}
```

### SearchBufferControl

```csharp
namespace Stroke.Layout.Controls;

/// <summary>
/// Specialized BufferControl for search input.
/// </summary>
public sealed class SearchBufferControl : BufferControl
{
    public IFilter IgnoreCase { get; }
    public SearchState SearcherSearchState { get; }

    // Inherits from BufferControl with search-specific configuration
}
```

---

## 6. Window Classes

### Window

```csharp
namespace Stroke.Layout.Windows;

/// <summary>
/// Container that wraps a UIControl with scrolling, margins, and cursor display.
/// Implements both IContainer and IWindow.
/// </summary>
public sealed partial class Window : IContainer, IWindow
{
    // Content
    public IUIControl Content { get; }
    public IReadOnlyList<IMargin> LeftMargins { get; }
    public IReadOnlyList<IMargin> RightMargins { get; }

    // Sizing
    public Dimension? Width { get; }
    public Dimension? Height { get; }
    public int? ZIndex { get; }
    public IFilter DontExtendWidth { get; }
    public IFilter DontExtendHeight { get; }
    public IFilter IgnoreContentWidth { get; }
    public IFilter IgnoreContentHeight { get; }

    // Scrolling
    public ScrollOffsets ScrollOffsets { get; }
    public IFilter AllowScrollBeyondBottom { get; }
    public IFilter WrapLines { get; }
    public Func<Window, int>? GetVerticalScroll { get; }
    public Func<Window, int>? GetHorizontalScroll { get; }

    // Cursor display
    public IFilter AlwaysHideCursor { get; }
    public IFilter CursorLine { get; }
    public IFilter CursorColumn { get; }
    public Func<IReadOnlyList<ColorColumn>>? ColorColumnsGetter { get; }

    // Alignment and styling
    public Func<WindowAlign> AlignGetter { get; }
    public Func<string> StyleGetter { get; }
    public Func<string>? CharGetter { get; }
    public GetLinePrefixCallable? GetLinePrefix { get; }

    // Mutable state (thread-safe with Lock)
    public int VerticalScroll { get; set; }
    public int HorizontalScroll { get; set; }
    public WindowRenderInfo? RenderInfo { get; }
}
```

### WindowRenderInfo

```csharp
namespace Stroke.Layout.Windows;

/// <summary>
/// Render state for a Window after WriteToScreen.
/// Immutable snapshot of render information.
/// </summary>
public sealed class WindowRenderInfo
{
    // Construction parameters
    public Window Window { get; }
    public UIContent UIContent { get; }
    public int HorizontalScroll { get; }
    public int VerticalScroll { get; }
    public int WindowWidth { get; }
    public int WindowHeight { get; }
    public ScrollOffsets ConfiguredScrollOffsets { get; }
    public IReadOnlyDictionary<int, (int Row, int Col)> VisibleLineToRowCol { get; }
    public IReadOnlyDictionary<(int Row, int Col), (int Y, int X)> RowColToYX { get; }
    public int XOffset { get; }
    public int YOffset { get; }
    public bool WrapLines { get; }

    // Computed properties
    public IReadOnlyDictionary<int, int> VisibleLineToInputLine { get; }
    public Point CursorPosition { get; }
    public ScrollOffsets AppliedScrollOffsets { get; }
    public IReadOnlyList<int> DisplayedLines { get; }
    public IReadOnlyDictionary<int, int> InputLineToVisibleLine { get; }
    public int ContentHeight { get; }
    public bool FullHeightVisible { get; }
    public bool TopVisible { get; }
    public bool BottomVisible { get; }
    public int VerticalScrollPercentage { get; }

    // Methods
    public int FirstVisibleLine(bool afterScrollOffset = false);
    public int LastVisibleLine(bool beforeScrollOffset = false);
    public int CenterVisibleLine(bool beforeScrollOffset = false, bool afterScrollOffset = false);
    public int GetHeightForLine(int lineNo);
}
```

### ScrollOffsets

```csharp
namespace Stroke.Layout.Windows;

/// <summary>
/// Configuration for scroll behavior (cursor margin requirements).
/// </summary>
public sealed class ScrollOffsets
{
    public Func<int> TopGetter { get; }
    public Func<int> BottomGetter { get; }
    public Func<int> LeftGetter { get; }
    public Func<int> RightGetter { get; }

    public int Top => TopGetter();
    public int Bottom => BottomGetter();
    public int Left => LeftGetter();
    public int Right => RightGetter();
}
```

### ColorColumn

```csharp
namespace Stroke.Layout.Windows;

/// <summary>
/// Configuration for highlighting a specific column.
/// </summary>
public sealed class ColorColumn
{
    public int Position { get; }
    public string Style { get; } // default: "class:color-column"
}
```

### GetLinePrefixCallable

```csharp
namespace Stroke.Layout.Windows;

/// <summary>
/// Delegate for getting line prefixes (continuation prompts).
/// </summary>
/// <param name="lineNumber">The line number in the document.</param>
/// <param name="wrapCount">Number of times this line has wrapped (0 for first segment).</param>
public delegate IReadOnlyList<StyleAndTextTuple> GetLinePrefixCallable(
    int lineNumber,
    int wrapCount);
```

---

## 7. Margin Interfaces and Classes

### IMargin

```csharp
namespace Stroke.Layout.Margins;

/// <summary>
/// Interface for Window margins (line numbers, scrollbars, etc.).
/// </summary>
public interface IMargin
{
    /// <summary>
    /// Return the width this margin requires.
    /// </summary>
    int GetWidth(Func<UIContent> getUIContent);

    /// <summary>
    /// Create the margin content for rendering.
    /// </summary>
    IReadOnlyList<StyleAndTextTuple> CreateMargin(
        WindowRenderInfo windowRenderInfo,
        int width,
        int height);
}
```

### NumberedMargin

```csharp
namespace Stroke.Layout.Margins;

/// <summary>
/// Displays line numbers in the margin.
/// </summary>
public sealed class NumberedMargin : IMargin
{
    public IFilter Relative { get; }
    public IFilter DisplayTildes { get; }

    // Width calculated from line count digits
    // Current line highlighted
}
```

### ScrollbarMargin

```csharp
namespace Stroke.Layout.Margins;

/// <summary>
/// Displays a vertical scrollbar.
/// </summary>
public sealed class ScrollbarMargin : IMargin
{
    public IFilter DisplayArrows { get; }
    public string UpArrowSymbol { get; }
    public string DownArrowSymbol { get; }

    // Width: 1 column
    // Scrollbar position calculated from WindowRenderInfo
}
```

### ConditionalMargin

```csharp
namespace Stroke.Layout.Margins;

/// <summary>
/// Shows or hides another margin based on a filter.
/// </summary>
public sealed class ConditionalMargin : IMargin
{
    public IMargin Margin { get; }
    public IFilter Filter { get; }

    // When Filter is false, GetWidth returns 0
}
```

### PromptMargin

```csharp
namespace Stroke.Layout.Margins;

/// <summary>
/// Displays prompt on first line and continuation on subsequent lines.
/// </summary>
[Obsolete("Use Window.GetLinePrefix instead")]
public sealed class PromptMargin : IMargin
{
    public Func<IReadOnlyList<StyleAndTextTuple>> GetPrompt { get; }
    public Func<int, int, bool, IReadOnlyList<StyleAndTextTuple>>? GetContinuation { get; }
}
```

---

## 8. Utility Classes

### ContainerUtils

```csharp
namespace Stroke.Layout.Containers;

/// <summary>
/// Utility functions for container manipulation.
/// </summary>
public static class ContainerUtils
{
    /// <summary>
    /// Convert any container-like object to IContainer.
    /// </summary>
    public static IContainer ToContainer(AnyContainer value);

    /// <summary>
    /// Convert any container-like object to Window.
    /// Throws if not a Window.
    /// </summary>
    public static Window ToWindow(AnyContainer value);

    /// <summary>
    /// Check if value can be converted to IContainer.
    /// </summary>
    public static bool IsContainer(object? value);
}
```

---

## 9. Entity Relationships

```
IContainer
├── HSplit (children: IContainer[])
├── VSplit (children: IContainer[])
├── FloatContainer
│   ├── content: IContainer
│   └── floats: Float[] → each has content: IContainer
├── ConditionalContainer
│   ├── content: IContainer
│   └── alternativeContent: IContainer?
├── DynamicContainer (getContainer → IContainer)
└── Window : IContainer, IWindow
    ├── content: IUIControl
    ├── leftMargins: IMargin[]
    ├── rightMargins: IMargin[]
    └── renderInfo: WindowRenderInfo

IUIControl
├── DummyControl
├── FormattedTextControl
├── BufferControl
│   └── buffer: Buffer
└── SearchBufferControl : BufferControl
    └── searcherSearchState: SearchState

IMargin
├── NumberedMargin
├── ScrollbarMargin
├── ConditionalMargin (wraps IMargin)
└── PromptMargin

Screen
├── uses Char for storage
├── tracks cursor/menu positions per IWindow
└── deferred drawing via DrawWithZIndex

UIContent
├── created by IUIControl.CreateContent()
└── consumed by Window for rendering

WindowRenderInfo
├── created by Window.WriteToScreen()
├── consumed by IMargin.CreateMargin()
└── provides line mappings for scroll/cursor calculations
```

---

## 10. Validation Rules

### Container Validation

1. HSplit/VSplit children must not be null
2. HSplit/VSplit padding must be >= 0
3. Float z_index must be >= 1
4. Float cannot have both xcursor=true and absolute left position
5. Float cannot have both ycursor=true and absolute top position

### Window Validation

1. Window scroll offsets cannot be negative
2. Window with wrap_lines=true ignores horizontal_scroll
3. ColorColumn position must be >= 0

### Control Validation

1. BufferControl.Buffer defaults to new Buffer() if null
2. UIContent.LineCount must be >= 0
3. UIContent.CursorPosition row/col must be within line count

---

## 11. State Transitions

### Window Scroll State

```
Initial State:
  vertical_scroll = 0
  horizontal_scroll = 0
  vertical_scroll_2 = 0

On WriteToScreen:
  1. Calculate scroll position to keep cursor visible
  2. Apply scroll offsets (top/bottom margins)
  3. Store result in render_info

On Reset:
  1. vertical_scroll = 0
  2. horizontal_scroll = 0
  3. vertical_scroll_2 = 0
  4. render_info = null
```

### BufferControl Click State

```
On MouseDown:
  1. Record click timestamp
  2. Set cursor position
  3. Start selection if shift held

On MouseDown (double-click detected):
  1. Select word at position

On MouseDown (triple-click detected):
  1. Select line at position

On MouseMove (with button held):
  1. Extend selection to current position
```
