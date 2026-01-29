# Contract: IUIControl Interface

**Namespace**: `Stroke.Layout.Controls`
**Python Equivalent**: `prompt_toolkit.layout.controls.UIControl`

## Interface Definition

```csharp
/// <summary>
/// Base interface for UI controls that can be rendered in a Window.
/// </summary>
/// <remarks>
/// <para>
/// UI controls generate content to be displayed. They do not handle layout or
/// scrolling directly - that is managed by the containing Window. Controls
/// produce UIContent objects that the Window then renders to the Screen.
/// </para>
/// <para>
/// Port of Python Prompt Toolkit's <c>UIControl</c> abstract base class from
/// <c>layout/controls.py</c>.
/// </para>
/// </remarks>
public interface IUIControl
{
    /// <summary>
    /// Create the content to be rendered.
    /// </summary>
    /// <param name="width">Available width in character cells.</param>
    /// <param name="height">Available height in lines.</param>
    /// <returns>UIContent containing the rendered lines and cursor position.</returns>
    UIContent CreateContent(int width, int height);

    /// <summary>
    /// Reset control state.
    /// </summary>
    /// <remarks>Called before each render pass.</remarks>
    void Reset();

    /// <summary>
    /// Return the preferred width for this control.
    /// </summary>
    /// <param name="maxAvailableWidth">Maximum width available.</param>
    /// <returns>Preferred width, or null if any width is acceptable.</returns>
    int? PreferredWidth(int maxAvailableWidth);

    /// <summary>
    /// Return the preferred height for this control.
    /// </summary>
    /// <param name="width">The actual width that will be used.</param>
    /// <param name="maxAvailableHeight">Maximum height available.</param>
    /// <param name="wrapLines">Whether line wrapping is enabled.</param>
    /// <param name="getLinePrefix">Optional line prefix function.</param>
    /// <returns>Preferred height, or null if any height is acceptable.</returns>
    int? PreferredHeight(
        int width,
        int maxAvailableHeight,
        bool wrapLines,
        GetLinePrefixCallable? getLinePrefix);

    /// <summary>
    /// Gets whether this control can receive focus.
    /// </summary>
    bool IsFocusable { get; }

    /// <summary>
    /// Handle a mouse event.
    /// </summary>
    /// <param name="mouseEvent">The mouse event to handle.</param>
    /// <returns>
    /// NotImplementedOrNone.NotImplemented if not handled,
    /// NotImplementedOrNone.None if handled.
    /// </returns>
    NotImplementedOrNone MouseHandler(MouseEvent mouseEvent);

    /// <summary>
    /// Called when the user wants to scroll down.
    /// </summary>
    void MoveCursorDown();

    /// <summary>
    /// Called when the user wants to scroll up.
    /// </summary>
    void MoveCursorUp();

    /// <summary>
    /// Get the key bindings for this control.
    /// </summary>
    /// <returns>Key bindings, or null if none.</returns>
    IKeyBindingsBase? GetKeyBindings();

    /// <summary>
    /// Get events that trigger control invalidation (redraw).
    /// </summary>
    /// <returns>Enumerable of events to subscribe to.</returns>
    IEnumerable<Event<object>> GetInvalidateEvents();
}
```

## Default Implementation Pattern

```csharp
public abstract class UIControlBase : IUIControl
{
    public abstract UIContent CreateContent(int width, int height);

    public virtual void Reset() { }

    public virtual int? PreferredWidth(int maxAvailableWidth) => null;

    public virtual int? PreferredHeight(
        int width,
        int maxAvailableHeight,
        bool wrapLines,
        GetLinePrefixCallable? getLinePrefix) => null;

    public virtual bool IsFocusable => false;

    public virtual NotImplementedOrNone MouseHandler(MouseEvent mouseEvent)
        => NotImplementedOrNone.NotImplemented;

    public virtual void MoveCursorDown() { }
    public virtual void MoveCursorUp() { }

    public virtual IKeyBindingsBase? GetKeyBindings() => null;

    public virtual IEnumerable<Event<object>> GetInvalidateEvents()
        => Enumerable.Empty<Event<object>>();
}
```

## UIContent Class

```csharp
/// <summary>
/// Represents the rendered content of a UIControl.
/// </summary>
/// <remarks>
/// Immutable snapshot of control output for a single render frame.
/// </remarks>
public sealed class UIContent
{
    /// <summary>
    /// Function to get a line by its index.
    /// </summary>
    public Func<int, IReadOnlyList<StyleAndTextTuple>> GetLine { get; }

    /// <summary>
    /// Total number of lines in the content.
    /// </summary>
    public int LineCount { get; }

    /// <summary>
    /// Cursor position within the content, or null if no cursor.
    /// </summary>
    public Point? CursorPosition { get; }

    /// <summary>
    /// Menu anchor position, or null to use cursor position.
    /// </summary>
    public Point? MenuPosition { get; }

    /// <summary>
    /// Whether the cursor should be visible.
    /// </summary>
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

## Usage Examples

### Simple FormattedTextControl

```csharp
public sealed class SimpleTextControl : IUIControl
{
    private readonly string _text;

    public SimpleTextControl(string text) => _text = text;

    public UIContent CreateContent(int width, int height)
    {
        var lines = _text.Split('\n');
        return new UIContent(
            getLine: i => i < lines.Length
                ? [new StyleAndTextTuple("", lines[i])]
                : [],
            lineCount: lines.Length,
            cursorPosition: null,
            menuPosition: null,
            showCursor: false);
    }

    // ... other IUIControl members with default implementations
}
```

### BufferControl with Cursor

```csharp
var control = new BufferControl(
    buffer: myBuffer,
    lexer: new PygmentsLexer("source.csharp"));

var content = control.CreateContent(width: 80, height: 24);
// content.CursorPosition reflects buffer cursor
// content.GetLine returns lexer-styled lines
```

## Thread Safety

UIContent is immutable and thread-safe. UIControl implementations must protect mutable state (caches, click timestamps) with `Lock`.

## Related Contracts

- [UIContent.md](./UIContent.md) - Control content output
- [BufferControl.md](./BufferControl.md) - Editable buffer control
- [FormattedTextControl.md](./FormattedTextControl.md) - Static text control
