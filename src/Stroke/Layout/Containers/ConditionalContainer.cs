using Stroke.Core.Primitives;
using Stroke.Filters;
using Stroke.KeyBinding;
using Stroke.Layout.Controls;

namespace Stroke.Layout.Containers;

/// <summary>
/// Shows or hides content based on a filter condition.
/// </summary>
/// <remarks>
/// <para>
/// ConditionalContainer wraps another container and shows it only when the
/// filter condition evaluates to true. When false, an optional alternative
/// container is shown instead (or nothing if no alternative is provided).
/// </para>
/// <para>
/// Port of Python Prompt Toolkit's <c>ConditionalContainer</c> class from <c>layout/containers.py</c>.
/// </para>
/// </remarks>
public sealed class ConditionalContainer : IContainer
{
    /// <summary>
    /// Gets the primary content container.
    /// </summary>
    public IContainer Content { get; }

    /// <summary>
    /// Gets the filter that determines visibility.
    /// </summary>
    public IFilter Filter { get; }

    /// <summary>
    /// Gets the alternative content shown when filter is false, or null.
    /// </summary>
    public IContainer? AlternativeContent { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConditionalContainer"/> class.
    /// </summary>
    /// <param name="content">The primary content container.</param>
    /// <param name="filter">The filter condition. Null defaults to Always (always visible).</param>
    /// <param name="alternativeContent">Optional content shown when filter is false.</param>
    public ConditionalContainer(
        AnyContainer content,
        FilterOrBool filter = default,
        AnyContainer alternativeContent = default)
    {
        Content = content.HasValue ? content.ToContainer() : new Window(content: new DummyControl());

        // Null filter defaults to Always (FR-010)
        Filter = filter.HasValue ? FilterUtils.ToFilter(filter) : Filters.Always.Instance;

        AlternativeContent = alternativeContent.HasValue ? alternativeContent.ToContainer() : null;
    }

    /// <summary>
    /// Gets the currently active container based on the filter state.
    /// </summary>
    private IContainer? GetActiveContainer()
    {
        if (Filter.Invoke())
            return Content;
        return AlternativeContent;
    }

    /// <inheritdoc/>
    public void Reset()
    {
        Content.Reset();
        AlternativeContent?.Reset();
    }

    /// <inheritdoc/>
    public Dimension PreferredWidth(int maxAvailableWidth)
    {
        var active = GetActiveContainer();
        if (active == null)
            return new Dimension();
        return active.PreferredWidth(maxAvailableWidth);
    }

    /// <inheritdoc/>
    public Dimension PreferredHeight(int width, int maxAvailableHeight)
    {
        var active = GetActiveContainer();
        if (active == null)
            return new Dimension();
        return active.PreferredHeight(width, maxAvailableHeight);
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
        var active = GetActiveContainer();
        if (active != null)
        {
            active.WriteToScreen(screen, mouseHandlers, writePosition, parentStyle, eraseBg, zIndex);
        }
        // When no active container, nothing is rendered (zero-size)
    }

    /// <inheritdoc/>
    public bool IsModal
    {
        get
        {
            var active = GetActiveContainer();
            return active?.IsModal ?? false;
        }
    }

    /// <inheritdoc/>
    public IKeyBindingsBase? GetKeyBindings()
    {
        var active = GetActiveContainer();
        return active?.GetKeyBindings();
    }

    /// <inheritdoc/>
    public IReadOnlyList<IContainer> GetChildren()
    {
        var active = GetActiveContainer();
        if (active != null)
            return [active];
        return Array.Empty<IContainer>();
    }

    /// <summary>
    /// Returns a string representation of this container.
    /// </summary>
    public override string ToString()
    {
        return $"ConditionalContainer(filter={Filter}, visible={Filter.Invoke()})";
    }
}
