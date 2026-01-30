# Contract: Window Class

**Namespace**: `Stroke.Layout.Windows`
**Python Equivalent**: `prompt_toolkit.layout.containers.Window`

## Class Definition

```csharp
/// <summary>
/// Container that wraps a UIControl with scrolling, margins, and cursor display.
/// </summary>
/// <remarks>
/// <para>
/// Window is the bridge between UIControl (content generation) and IContainer (layout).
/// It handles scrolling to keep the cursor visible, renders margins (line numbers,
/// scrollbars), and applies cursorline/cursorcolumn highlighting.
/// </para>
/// <para>
/// This type is thread-safe. Mutable scroll state is protected by Lock.
/// </para>
/// <para>
/// Port of Python Prompt Toolkit's <c>Window</c> class from <c>layout/containers.py</c>.
/// Due to the complexity (~1200 lines in Python), this is split into partial classes:
/// Window.cs, Window.Scroll.cs, Window.Render.cs.
/// </para>
/// </remarks>
public sealed partial class Window : IContainer, IWindow
{
    /// <summary>
    /// Initializes a new Window instance.
    /// </summary>
    public Window(
        IUIControl? content = null,
        Dimension? width = null,
        Dimension? height = null,
        int? zIndex = null,
        FilterOrBool dontExtendWidth = default,
        FilterOrBool dontExtendHeight = default,
        FilterOrBool ignoreContentWidth = default,
        FilterOrBool ignoreContentHeight = default,
        IReadOnlyList<IMargin>? leftMargins = null,
        IReadOnlyList<IMargin>? rightMargins = null,
        ScrollOffsets? scrollOffsets = null,
        FilterOrBool allowScrollBeyondBottom = default,
        FilterOrBool wrapLines = default,
        Func<Window, int>? getVerticalScroll = null,
        Func<Window, int>? getHorizontalScroll = null,
        FilterOrBool alwaysHideCursor = default,
        FilterOrBool cursorLine = default,
        FilterOrBool cursorColumn = default,
        Func<IReadOnlyList<ColorColumn>>? colorColumns = null,
        WindowAlign align = WindowAlign.Left,
        Func<string>? style = null,
        Func<string>? character = null,
        GetLinePrefixCallable? getLinePrefix = null);

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

    // Mutable state (thread-safe)
    public int VerticalScroll { get; set; }
    public int HorizontalScroll { get; set; }
    public WindowRenderInfo? RenderInfo { get; }

    // IContainer implementation
    public void Reset();
    public Dimension PreferredWidth(int maxAvailableWidth);
    public Dimension PreferredHeight(int width, int maxAvailableHeight);
    public void WriteToScreen(
        Screen screen,
        MouseHandlers mouseHandlers,
        WritePosition writePosition,
        string parentStyle,
        bool eraseBg,
        int? zIndex);
    public bool IsModal => false;
    public IKeyBindingsBase? GetKeyBindings();
    public IReadOnlyList<IContainer> GetChildren();
}
```

## Scroll Algorithms

### With Line Wrapping

When `WrapLines` filter returns true:

1. Calculate total wrapped height by summing `UIContent.GetHeightForLine()` for each line
2. Apply scroll offsets (top/bottom margins)
3. Handle edge case: single line taller than window (use `vertical_scroll_2` for sub-line offset)
4. Ensure cursor row is visible within scroll offset margins
5. Store line mapping in WindowRenderInfo for margin rendering

### Without Line Wrapping

When `WrapLines` filter returns false:

1. Total height = UIContent.LineCount
2. Apply scroll offsets
3. No sub-line scrolling needed
4. Horizontal scroll for lines wider than window

## Rendering Pipeline

```
WriteToScreen():
1. Adjust writePosition for dont_extend flags
2. Get UIContent from Content.CreateContent()
3. Calculate scroll positions via _scroll()
4. Erase/fill background if eraseBg
5. Render left margins
6. Copy body content with alignment and wrapping
7. Render right margins
8. Apply cursorline/cursorcolumn highlighting
9. Store RenderInfo for margin access
10. Register cursor/menu positions with Screen
```

## Usage Examples

### Simple Window with Scrolling

```csharp
var window = new Window(
    content: new BufferControl(buffer),
    scrollOffsets: new ScrollOffsets(top: 3, bottom: 3),
    wrapLines: true,
    cursorLine: true);
```

### Window with Margins

```csharp
var window = new Window(
    content: new BufferControl(buffer),
    leftMargins: [
        new NumberedMargin(relative: false, displayTildes: true),
    ],
    rightMargins: [
        new ScrollbarMargin(displayArrows: true),
    ]);
```

### Window with ColorColumns

```csharp
var window = new Window(
    content: new BufferControl(buffer),
    colorColumns: () => [
        new ColorColumn(80, "class:color-column"),
        new ColorColumn(120, "class:color-column"),
    ]);
```

### Window with Custom Line Prefix

```csharp
var window = new Window(
    content: control,
    getLinePrefix: (lineNumber, wrapCount) =>
        wrapCount == 0
            ? [new StyleAndTextTuple("class:prompt", ">>> ")]
            : [new StyleAndTextTuple("class:prompt.continuation", "... ")]);
```

## Thread Safety

Window maintains mutable scroll state protected by `Lock`:

```csharp
private readonly Lock _lock = new();
private int _verticalScroll;
private int _horizontalScroll;
private int _verticalScroll2;
private WindowRenderInfo? _renderInfo;

public int VerticalScroll
{
    get { using (_lock.EnterScope()) return _verticalScroll; }
    set { using (_lock.EnterScope()) _verticalScroll = value; }
}
```

## Related Contracts

- [IContainer.md](./IContainer.md) - Container interface
- [IUIControl.md](./IUIControl.md) - Control interface
- [WindowRenderInfo.md](./WindowRenderInfo.md) - Render state
- [IMargin.md](./IMargin.md) - Margin interface
