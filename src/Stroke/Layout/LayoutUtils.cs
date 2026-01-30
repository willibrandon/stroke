using Stroke.FormattedText;
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

    /// <summary>
    /// Turn a list of (style, text) fragments into an <see cref="ExplodedList"/> where each
    /// element is exactly one character.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This function is idempotent: calling it on an already-exploded list
    /// returns the same list without re-processing.
    /// </para>
    /// <para>
    /// Port of Python Prompt Toolkit's <c>explode_text_fragments</c> function
    /// from <c>prompt_toolkit.layout.utils</c>.
    /// </para>
    /// </remarks>
    /// <param name="fragments">The fragments to explode.</param>
    /// <returns>An <see cref="ExplodedList"/> with single-character fragments.</returns>
    public static ExplodedList ExplodeTextFragments(IReadOnlyList<StyleAndTextTuple> fragments)
    {
        // When the fragments is already exploded, don't explode again.
        if (fragments is ExplodedList exploded)
        {
            return exploded;
        }

        var result = new List<StyleAndTextTuple>();

        foreach (var fragment in fragments)
        {
            foreach (var c in fragment.Text)
            {
                result.Add(new StyleAndTextTuple(fragment.Style, c.ToString(), fragment.MouseHandler));
            }
        }

        return new ExplodedList(result);
    }
}
