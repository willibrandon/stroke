namespace Stroke.Shortcuts;

/// <summary>
/// How to display autocompletions for the prompt.
/// </summary>
/// <remarks>
/// Port of Python Prompt Toolkit's <c>CompleteStyle</c> enum from
/// <c>prompt_toolkit.shortcuts.prompt</c>.
/// </remarks>
public enum CompleteStyle
{
    /// <summary>
    /// Display completions in a single-column dropdown menu near the cursor.
    /// </summary>
    Column,

    /// <summary>
    /// Display completions in a multi-column dropdown menu near the cursor.
    /// </summary>
    MultiColumn,

    /// <summary>
    /// Display completions below the input line, similar to GNU Readline's Tab completion.
    /// </summary>
    ReadlineLike,
}
