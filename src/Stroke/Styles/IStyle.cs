namespace Stroke.Styles;

/// <summary>
/// Abstract base interface for prompt_toolkit styles.
/// </summary>
/// <remarks>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>BaseStyle</c> abstract class
/// from <c>prompt_toolkit.styles.base</c>.
/// </para>
/// </remarks>
public interface IStyle
{
    /// <summary>
    /// Return <see cref="Attrs"/> for the given style string.
    /// </summary>
    /// <param name="styleStr">The style string. Can contain inline styling and class names (e.g., "class:title").</param>
    /// <param name="default">Default Attrs to use if no styling was defined. Uses <see cref="DefaultAttrs.Default"/> if null.</param>
    /// <returns>Computed attributes for the style string.</returns>
    Attrs GetAttrsForStyleStr(string styleStr, Attrs? @default = null);

    /// <summary>
    /// The list of style rules used to create this style.
    /// </summary>
    /// <remarks>
    /// Required for DynamicStyle and merged styles to work correctly.
    /// </remarks>
    IReadOnlyList<(string ClassNames, string StyleDef)> StyleRules { get; }

    /// <summary>
    /// Invalidation hash for the style. When this changes over time, the renderer
    /// knows that something in the style changed and everything has to be redrawn.
    /// </summary>
    object InvalidationHash { get; }
}
