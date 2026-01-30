# Contract: IContainer Interface

**Namespace**: `Stroke.Layout.Containers`
**Python Equivalent**: `prompt_toolkit.layout.containers.Container`

## Interface Definition

```csharp
/// <summary>
/// Base interface for all layout containers.
/// </summary>
/// <remarks>
/// <para>
/// Containers form the hierarchical structure of a terminal UI layout. Each container
/// knows how to calculate its preferred dimensions and render itself to a Screen.
/// </para>
/// <para>
/// Port of Python Prompt Toolkit's <c>Container</c> abstract base class from
/// <c>layout/containers.py</c>.
/// </para>
/// </remarks>
public interface IContainer
{
    /// <summary>
    /// Reset the state of this container and all its children.
    /// </summary>
    /// <remarks>
    /// Called before each render pass to reset scroll positions and cached state.
    /// </remarks>
    void Reset();

    /// <summary>
    /// Calculate the preferred width for this container.
    /// </summary>
    /// <param name="maxAvailableWidth">Maximum width available from parent.</param>
    /// <returns>A Dimension specifying min, max, preferred, and weight.</returns>
    Dimension PreferredWidth(int maxAvailableWidth);

    /// <summary>
    /// Calculate the preferred height for this container.
    /// </summary>
    /// <param name="width">The actual width allocated by the parent.</param>
    /// <param name="maxAvailableHeight">Maximum height available from parent.</param>
    /// <returns>A Dimension specifying min, max, preferred, and weight.</returns>
    Dimension PreferredHeight(int width, int maxAvailableHeight);

    /// <summary>
    /// Render this container to the screen.
    /// </summary>
    /// <param name="screen">The screen buffer to write to.</param>
    /// <param name="mouseHandlers">Mouse handler registry for this region.</param>
    /// <param name="writePosition">The position and size allocated for this container.</param>
    /// <param name="parentStyle">Style inherited from parent containers.</param>
    /// <param name="eraseBg">Whether to erase the background before rendering.</param>
    /// <param name="zIndex">Z-index for layered rendering, or null to use container's default.</param>
    void WriteToScreen(
        Screen screen,
        MouseHandlers mouseHandlers,
        WritePosition writePosition,
        string parentStyle,
        bool eraseBg,
        int? zIndex);

    /// <summary>
    /// Gets whether key bindings in this container are modal.
    /// </summary>
    /// <remarks>
    /// When true, key bindings in parent containers are ignored.
    /// Default implementation returns false.
    /// </remarks>
    bool IsModal { get; }

    /// <summary>
    /// Get the key bindings associated with this container.
    /// </summary>
    /// <returns>Key bindings, or null if none.</returns>
    IKeyBindingsBase? GetKeyBindings();

    /// <summary>
    /// Get the direct children of this container.
    /// </summary>
    /// <returns>List of child containers (may be empty).</returns>
    IReadOnlyList<IContainer> GetChildren();
}
```

## Usage Examples

### Implementing a Simple Container

```csharp
public sealed class SimpleContainer : IContainer
{
    private readonly IContainer _child;

    public SimpleContainer(IContainer child)
    {
        _child = child ?? throw new ArgumentNullException(nameof(child));
    }

    public void Reset() => _child.Reset();

    public Dimension PreferredWidth(int maxAvailableWidth)
        => _child.PreferredWidth(maxAvailableWidth);

    public Dimension PreferredHeight(int width, int maxAvailableHeight)
        => _child.PreferredHeight(width, maxAvailableHeight);

    public void WriteToScreen(
        Screen screen,
        MouseHandlers mouseHandlers,
        WritePosition writePosition,
        string parentStyle,
        bool eraseBg,
        int? zIndex)
    {
        _child.WriteToScreen(screen, mouseHandlers, writePosition, parentStyle, eraseBg, zIndex);
    }

    public bool IsModal => false;
    public IKeyBindingsBase? GetKeyBindings() => null;
    public IReadOnlyList<IContainer> GetChildren() => [_child];
}
```

### Walking a Container Tree

```csharp
public static IEnumerable<IContainer> Walk(IContainer root)
{
    yield return root;
    foreach (var child in root.GetChildren())
    {
        foreach (var descendant in Walk(child))
        {
            yield return descendant;
        }
    }
}
```

## Thread Safety

Implementations must be thread-safe for concurrent render passes. Mutable state (scroll positions, caches) must be protected with `Lock`.

## Related Contracts

- [HSplit.md](./HSplit.md) - Vertical stacking container
- [VSplit.md](./VSplit.md) - Horizontal arrangement container
- [Window.md](./Window.md) - Control wrapper with scrolling
