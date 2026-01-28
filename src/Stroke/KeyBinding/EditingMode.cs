namespace Stroke.KeyBinding;

/// <summary>
/// Represents the active key binding set.
/// </summary>
/// <remarks>
/// <para>
/// Equivalent to Python Prompt Toolkit's <c>EditingMode</c> enum from <c>prompt_toolkit.enums</c>.
/// </para>
/// </remarks>
public enum EditingMode
{
    /// <summary>Vi-style modal editing.</summary>
    Vi,

    /// <summary>Emacs-style chord-based editing.</summary>
    Emacs
}
