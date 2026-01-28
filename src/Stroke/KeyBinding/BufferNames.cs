namespace Stroke.KeyBinding;

/// <summary>
/// Standard names for well-known buffers.
/// </summary>
/// <remarks>
/// <para>
/// Equivalent to Python Prompt Toolkit's buffer name constants from <c>prompt_toolkit.enums</c>.
/// </para>
/// </remarks>
public static class BufferNames
{
    /// <summary>Name of the search buffer.</summary>
    public const string Search = "SEARCH_BUFFER";

    /// <summary>Name of the default buffer.</summary>
    public const string Default = "DEFAULT_BUFFER";

    /// <summary>Name of the system buffer.</summary>
    public const string System = "SYSTEM_BUFFER";
}
