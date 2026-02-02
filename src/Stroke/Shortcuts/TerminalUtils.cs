namespace Stroke.Shortcuts;

/// <summary>
/// Terminal control utilities for clearing the screen and managing the window title.
/// </summary>
/// <remarks>
/// <para>
/// Port of Python Prompt Toolkit's <c>clear</c>, <c>set_title</c>, and
/// <c>clear_title</c> functions from <c>prompt_toolkit.shortcuts.utils</c>.
/// </para>
/// <para>
/// This type is thread-safe. All methods are stateless and delegate to
/// the current session's <see cref="Stroke.Output.IOutput"/>.
/// </para>
/// </remarks>
public static class TerminalUtils
{
    /// <summary>
    /// Clear the terminal screen.
    /// </summary>
    /// <remarks>
    /// Erases the screen, moves the cursor to position (0, 0), and flushes
    /// the output.
    /// </remarks>
    public static void Clear()
    {
        var output = Stroke.Application.AppContext.GetAppSession().Output;
        output.EraseScreen();
        output.CursorGoto(0, 0);
        output.Flush();
    }

    /// <summary>
    /// Set the terminal window title.
    /// </summary>
    /// <param name="text">The title text to set.</param>
    public static void SetTitle(string text)
    {
        var output = Stroke.Application.AppContext.GetAppSession().Output;
        output.SetTitle(text);
    }

    /// <summary>
    /// Clear the terminal window title by setting it to an empty string.
    /// </summary>
    public static void ClearTitle()
    {
        SetTitle("");
    }
}
