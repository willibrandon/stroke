using Stroke.Core.Primitives;
using Stroke.KeyBinding;
using Stroke.Layout.Controls;

namespace Stroke.Layout.Containers;

/// <summary>
/// Container that evaluates a callable to get current content at render time.
/// </summary>
/// <remarks>
/// <para>
/// DynamicContainer calls a function at render time to determine which container
/// to display. This allows for runtime switching of container content without
/// rebuilding the layout tree.
/// </para>
/// <para>
/// Port of Python Prompt Toolkit's <c>DynamicContainer</c> class from <c>layout/containers.py</c>.
/// </para>
/// </remarks>
public sealed class DynamicContainer : IContainer
{
    /// <summary>
    /// Gets the function that returns the current container.
    /// </summary>
    public Func<AnyContainer>? GetContainer { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DynamicContainer"/> class.
    /// </summary>
    /// <param name="getContainer">Function returning the current container, or null.</param>
    public DynamicContainer(Func<AnyContainer>? getContainer)
    {
        GetContainer = getContainer;
    }

    /// <summary>
    /// Gets the currently resolved container.
    /// </summary>
    /// <returns>The current container, or null if none is available.</returns>
    private IContainer? GetResolvedContainer()
    {
        if (GetContainer == null)
            return null;

        var anyContainer = GetContainer();
        if (!anyContainer.HasValue)
            return null;

        return anyContainer.ToContainer();
    }

    /// <inheritdoc/>
    public void Reset()
    {
        GetResolvedContainer()?.Reset();
    }

    /// <inheritdoc/>
    public Dimension PreferredWidth(int maxAvailableWidth)
    {
        var container = GetResolvedContainer();
        if (container == null)
            return new Dimension();
        return container.PreferredWidth(maxAvailableWidth);
    }

    /// <inheritdoc/>
    public Dimension PreferredHeight(int width, int maxAvailableHeight)
    {
        var container = GetResolvedContainer();
        if (container == null)
            return new Dimension();
        return container.PreferredHeight(width, maxAvailableHeight);
    }

    /// <inheritdoc/>
    public void WriteToScreen(
        Screen screen,
        MouseHandlers mouseHandlers,
        WritePosition writePosition,
        string parentStyle,
        bool eraseBg,
        int? zIndex)
    {
        var container = GetResolvedContainer();
        container?.WriteToScreen(screen, mouseHandlers, writePosition, parentStyle, eraseBg, zIndex);
    }

    /// <inheritdoc/>
    public bool IsModal
    {
        get
        {
            var container = GetResolvedContainer();
            return container?.IsModal ?? false;
        }
    }

    /// <inheritdoc/>
    public IKeyBindingsBase? GetKeyBindings()
    {
        var container = GetResolvedContainer();
        return container?.GetKeyBindings();
    }

    /// <inheritdoc/>
    public IReadOnlyList<IContainer> GetChildren()
    {
        var container = GetResolvedContainer();
        if (container != null)
            return [container];
        return Array.Empty<IContainer>();
    }

    /// <summary>
    /// Returns a string representation of this container.
    /// </summary>
    public override string ToString()
    {
        var container = GetResolvedContainer();
        return $"DynamicContainer(current={container?.GetType().Name ?? "null"})";
    }
}
