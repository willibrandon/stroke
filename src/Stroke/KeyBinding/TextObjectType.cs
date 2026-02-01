namespace Stroke.KeyBinding;

/// <summary>
/// Classifies how a Vi text object's boundary positions are interpreted.
/// </summary>
/// <remarks>
/// Port of Python Prompt Toolkit's <c>TextObjectType</c> from
/// <c>prompt_toolkit.key_binding.bindings.vi</c>.
/// </remarks>
public enum TextObjectType
{
    /// <summary>End position is not included in the range.</summary>
    Exclusive,

    /// <summary>End position is included in the range.</summary>
    Inclusive,

    /// <summary>Full lines from start to end are included.</summary>
    Linewise,

    /// <summary>Rectangular column selection.</summary>
    Block
}
