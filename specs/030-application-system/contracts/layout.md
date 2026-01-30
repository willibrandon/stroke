# Contract: Layout

**Namespace**: `Stroke.Layout`
**Source**: `prompt_toolkit.layout.layout`

## Layout Class

```csharp
/// <summary>
/// The layout wrapper for a Stroke Application. Manages the root container hierarchy,
/// tracks focus, maintains parent-child relationships, and provides search linking.
/// </summary>
/// <remarks>
/// This class is thread-safe. Focus changes, search links, and parent-child mapping
/// updates are synchronized via Lock.
/// </remarks>
public sealed class Layout
{
    /// <summary>
    /// Create a new Layout with the given root container.
    /// </summary>
    /// <param name="container">The root container for the layout.</param>
    /// <param name="focusedElement">Element to focus initially. Defaults to first window.</param>
    /// <exception cref="InvalidLayoutException">Layout contains no Window objects.</exception>
    public Layout(AnyContainer container, FocusableElement? focusedElement = null);

    /// <summary>The root container.</summary>
    public IContainer Container { get; }

    /// <summary>
    /// Map from SearchBufferControl to the original BufferControl it searches.
    /// When a link exists, that search is currently active. Links are added when a
    /// search operation starts (typically by a key binding that creates a search toolbar)
    /// and removed when the search ends (accept/cancel). The <see cref="IsSearching"/>
    /// property returns true when any link maps to the currently focused BufferControl.
    /// Thread-safe: access is synchronized via Lock.
    /// </summary>
    public Dictionary<SearchBufferControl, BufferControl> SearchLinks { get; }

    /// <summary>
    /// List of currently visible windows. Updated by the Renderer during each render cycle
    /// via <see cref="UpdateParentsRelations"/>. Contains only windows that were actually
    /// rendered (not hidden by ConditionalContainer or filtered out). Used by focus
    /// navigation to skip non-visible windows. Empty before the first render.
    /// Thread-safe: access is synchronized via Lock.
    /// </summary>
    public List<Window> VisibleWindows { get; }

    /// <summary>
    /// The currently focused Window. The getter returns the top of the focus stack.
    /// The setter directly sets the focused window by pushing it onto the focus stack,
    /// equivalent to calling <see cref="Focus"/> with the window. Both the setter and
    /// <see cref="Focus"/> perform the same operation: push the target window to the
    /// top of the focus stack. The setter does NOT validate that the window is in the
    /// layout or that it is focusable — this matches Python behavior where
    /// <c>layout.current_window = w</c> directly sets the internal stack.
    /// </summary>
    public Window CurrentWindow { get; set; }

    /// <summary>The UIControl of the currently focused Window.</summary>
    public IUIControl CurrentControl { get; }

    /// <summary>
    /// The currently focused Buffer, or null if the focused control is not a BufferControl.
    /// </summary>
    public Buffer? CurrentBuffer { get; }

    /// <summary>
    /// The currently focused BufferControl, or null if focus is not on a BufferControl.
    /// </summary>
    public BufferControl? CurrentBufferControl { get; }

    /// <summary>Whether the current focused control has a search focus link.</summary>
    public bool IsSearching { get; }

    /// <summary>
    /// The SearchBufferControl that is linked to the current BufferControl, or null.
    /// </summary>
    public SearchBufferControl? CurrentSearchBufferControl { get; }

    // --- Methods ---

    /// <summary>
    /// Focus the given element. Accepts a Window, UIControl, Buffer, buffer name, or container.
    /// Thread-safe: synchronized via Lock. Can be called from background tasks, but focus
    /// changes take effect on the next render cycle. The caller should call
    /// <see cref="Application{TResult}.Invalidate"/> after changing focus to trigger a redraw.
    /// </summary>
    /// <param name="value">The element to focus.</param>
    /// <exception cref="InvalidOperationException">Element is not focusable or not found.</exception>
    public void Focus(FocusableElement value);

    /// <summary>
    /// Focus the previous focusable Window. Thread-safe: synchronized via Lock.
    /// </summary>
    public void FocusPrevious();

    /// <summary>
    /// Focus the next focusable Window. Thread-safe: synchronized via Lock.
    /// </summary>
    public void FocusNext();

    /// <summary>Focus the last focused Window.</summary>
    public void FocusLast();

    /// <summary>Whether the given Window has focus.</summary>
    public bool HasFocus(Window window);

    /// <summary>Whether the given UIControl, Buffer, or buffer name has focus.</summary>
    public bool HasFocus(FocusableElement value);

    /// <summary>Find all Windows in the layout by walking the container tree.</summary>
    public IEnumerable<Window> FindAllWindows();

    /// <summary>Find all UIControls in the layout.</summary>
    public IEnumerable<IUIControl> FindAllControls();

    /// <summary>Walk the entire container tree in depth-first order.</summary>
    public IEnumerable<IContainer> Walk();

    /// <summary>Get the parent container of the given child, or null if root.</summary>
    public IContainer? GetParent(IContainer child);

    /// <summary>
    /// Update parent-child relationships by walking the container tree.
    /// Called after each render cycle.
    /// </summary>
    public void UpdateParentsRelations();

    /// <summary>
    /// Reset layout state. Resets all containers in the tree.
    /// </summary>
    public void Reset();
}
```

## Supporting Types

```csharp
/// <summary>
/// Union type representing elements that can receive focus.
/// </summary>
public readonly struct FocusableElement
{
    public static implicit operator FocusableElement(string bufferName);
    public static implicit operator FocusableElement(Buffer buffer);
    public static implicit operator FocusableElement(IUIControl control);
    public static implicit operator FocusableElement(Window window);
    public static implicit operator FocusableElement(AnyContainer container);
}

/// <summary>
/// Exception thrown when a Layout is invalid (e.g., contains no Windows).
/// </summary>
public sealed class InvalidLayoutException : Exception
{
    public InvalidLayoutException(string message);
}
```

## Standalone Functions

```csharp
/// <summary>
/// Walk a container tree in depth-first order.
/// </summary>
public static class LayoutUtils
{
    /// <summary>
    /// Walk the container tree starting from the given container.
    /// Yields all containers in depth-first order.
    /// </summary>
    public static IEnumerable<IContainer> Walk(IContainer container);
}

/// <summary>
/// Create a dummy layout for use when no layout is specified.
/// </summary>
public static class DummyLayout
{
    /// <summary>
    /// Create a Layout with a single Window displaying "No layout specified. Press ENTER to quit."
    /// The ENTER key binding calls Application.Exit().
    /// </summary>
    public static Layout Create();
}
```
