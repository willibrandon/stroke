using Stroke.Layout.Containers;

namespace Stroke.Layout;

/// <summary>
/// Utility methods for walking the layout container tree.
/// </summary>
/// <remarks>
/// <para>
/// Port of Python Prompt Toolkit's <c>walk</c> function from
/// <c>prompt_toolkit.layout.layout</c>.
/// </para>
/// </remarks>
public static class LayoutUtils
{
    /// <summary>
    /// Walk the container tree starting from the given container.
    /// Yields all containers in depth-first order.
    /// </summary>
    /// <param name="container">The root container to start walking from.</param>
    /// <param name="skipHidden">When true, skip ConditionalContainers whose filter evaluates to false.</param>
    /// <returns>All containers in depth-first order.</returns>
    public static IEnumerable<IContainer> Walk(IContainer container, bool skipHidden = false)
    {
        ArgumentNullException.ThrowIfNull(container);

        // When skipHidden is set, don't go into disabled ConditionalContainer containers.
        if (skipHidden && container is ConditionalContainer cc && !cc.Filter.Invoke())
        {
            yield break;
        }

        yield return container;

        foreach (var child in container.GetChildren())
        {
            foreach (var descendant in Walk(child, skipHidden))
            {
                yield return descendant;
            }
        }
    }
}
