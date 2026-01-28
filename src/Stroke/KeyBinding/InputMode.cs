namespace Stroke.KeyBinding;

/// <summary>
/// Represents Vi input mode states.
/// </summary>
/// <remarks>
/// <para>
/// Equivalent to Python Prompt Toolkit's <c>InputMode</c> enum from <c>prompt_toolkit.key_binding.vi_state</c>.
/// </para>
/// </remarks>
public enum InputMode
{
    /// <summary>Normal text insertion mode.</summary>
    Insert,

    /// <summary>Insert mode for multiple cursors.</summary>
    InsertMultiple,

    /// <summary>Vi normal mode for navigation and commands.</summary>
    Navigation,

    /// <summary>Overwrite mode (like Vi 'R' command).</summary>
    Replace,

    /// <summary>Replace single character (like Vi 'r' command).</summary>
    ReplaceSingle
}
