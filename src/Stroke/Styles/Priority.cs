namespace Stroke.Styles;

/// <summary>
/// The priority of the rules, when a style is created from a dictionary.
/// </summary>
/// <remarks>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>Priority</c> enum
/// from <c>prompt_toolkit.styles.style</c>.
/// </para>
/// <para>
/// In a Style, rules that are defined later will always override
/// previously defined rules. When creating a style from a dictionary, this enum
/// controls how the rules are ordered.
/// </para>
/// </remarks>
public enum Priority
{
    /// <summary>
    /// Use dictionary key order. Rules at the end override rules at the beginning.
    /// </summary>
    /// <remarks>
    /// This corresponds to Python's <c>Priority.DICT_KEY_ORDER</c>.
    /// </remarks>
    DictKeyOrder,

    /// <summary>
    /// Keys defined with more precision (more elements) get higher priority.
    /// </summary>
    /// <remarks>
    /// This corresponds to Python's <c>Priority.MOST_PRECISE</c>.
    /// More precise means: more elements when split by '.' and whitespace.
    /// </remarks>
    MostPrecise
}
