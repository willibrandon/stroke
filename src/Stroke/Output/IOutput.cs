using Stroke.Core.Primitives;
using Stroke.CursorShapes;
using Stroke.Styles;

namespace Stroke.Output;

/// <summary>
/// Interface for all terminal output implementations.
/// </summary>
/// <remarks>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>Output</c> abstract class
/// from <c>prompt_toolkit.output.base</c>.
/// </para>
/// <para>
/// Implementations include:
/// <list type="bullet">
///   <item><description>Vt100Output: VT100/ANSI escape sequence output for terminals.</description></item>
///   <item><description>PlainTextOutput: Plain text output without escape sequences.</description></item>
///   <item><description>DummyOutput: No-op output for testing.</description></item>
/// </list>
/// </para>
/// </remarks>
public interface IOutput
{
    #region Writing

    /// <summary>
    /// Writes text to the output, escaping any VT100 escape sequences.
    /// </summary>
    /// <param name="data">The text to write.</param>
    /// <remarks>
    /// <para>
    /// The escape character (0x1B) is replaced with '?' to prevent injection of
    /// escape sequences in user-supplied text.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="data"/> is null.</exception>
    void Write(string data);

    /// <summary>
    /// Writes raw text to the output without any escaping.
    /// </summary>
    /// <param name="data">The text to write verbatim.</param>
    /// <remarks>
    /// <para>
    /// Use this method to write VT100 escape sequences directly to the terminal.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="data"/> is null.</exception>
    void WriteRaw(string data);

    /// <summary>
    /// Flushes the output buffer to the underlying stream.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Output is buffered until Flush is called. If the buffer is empty, no I/O
    /// operation is performed.
    /// </para>
    /// </remarks>
    void Flush();

    #endregion

    #region Screen Control

    /// <summary>
    /// Erases the entire screen and moves the cursor to the home position.
    /// </summary>
    void EraseScreen();

    /// <summary>
    /// Erases from the cursor position to the end of the current line.
    /// </summary>
    void EraseEndOfLine();

    /// <summary>
    /// Erases from the cursor position to the bottom of the screen.
    /// </summary>
    void EraseDown();

    /// <summary>
    /// Enters the alternate screen buffer.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The alternate screen buffer is used by full-screen applications to preserve
    /// the original terminal content. Call <see cref="QuitAlternateScreen"/> to
    /// restore the original content.
    /// </para>
    /// </remarks>
    void EnterAlternateScreen();

    /// <summary>
    /// Exits the alternate screen buffer.
    /// </summary>
    void QuitAlternateScreen();

    #endregion

    #region Cursor Movement

    /// <summary>
    /// Moves the cursor to the specified position.
    /// </summary>
    /// <param name="row">The row (1-based).</param>
    /// <param name="column">The column (1-based).</param>
    /// <remarks>
    /// <para>
    /// VT100 uses 1-based indexing. A value of 0 is treated as 1.
    /// Values exceeding the terminal size are clipped by the terminal.
    /// </para>
    /// </remarks>
    void CursorGoto(int row, int column);

    /// <summary>
    /// Moves the cursor up by the specified amount.
    /// </summary>
    /// <param name="amount">The number of rows to move up.</param>
    /// <remarks>
    /// <para>
    /// If <paramref name="amount"/> is 0, no action is taken.
    /// If <paramref name="amount"/> is 1, an optimized single-character sequence is used.
    /// </para>
    /// </remarks>
    void CursorUp(int amount);

    /// <summary>
    /// Moves the cursor down by the specified amount.
    /// </summary>
    /// <param name="amount">The number of rows to move down.</param>
    void CursorDown(int amount);

    /// <summary>
    /// Moves the cursor forward (right) by the specified amount.
    /// </summary>
    /// <param name="amount">The number of columns to move right.</param>
    void CursorForward(int amount);

    /// <summary>
    /// Moves the cursor backward (left) by the specified amount.
    /// </summary>
    /// <param name="amount">The number of columns to move left.</param>
    void CursorBackward(int amount);

    #endregion

    #region Cursor Visibility

    /// <summary>
    /// Hides the cursor.
    /// </summary>
    /// <remarks>
    /// <para>
    /// If the cursor is already hidden, no escape sequence is sent.
    /// </para>
    /// </remarks>
    void HideCursor();

    /// <summary>
    /// Shows the cursor.
    /// </summary>
    /// <remarks>
    /// <para>
    /// If the cursor is already visible, no escape sequence is sent.
    /// </para>
    /// </remarks>
    void ShowCursor();

    /// <summary>
    /// Sets the cursor shape.
    /// </summary>
    /// <param name="shape">The cursor shape to set.</param>
    /// <remarks>
    /// <para>
    /// If <paramref name="shape"/> is <see cref="CursorShape.NeverChange"/>, no action is taken.
    /// </para>
    /// </remarks>
    void SetCursorShape(CursorShape shape);

    /// <summary>
    /// Resets the cursor shape to the terminal default.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Only sends the reset sequence if <see cref="SetCursorShape"/> was previously called
    /// with a shape other than <see cref="CursorShape.NeverChange"/>.
    /// </para>
    /// </remarks>
    void ResetCursorShape();

    #endregion

    #region Attributes

    /// <summary>
    /// Resets all text attributes to their default values.
    /// </summary>
    void ResetAttributes();

    /// <summary>
    /// Sets text attributes (colors, bold, italic, etc.).
    /// </summary>
    /// <param name="attrs">The attributes to set.</param>
    /// <param name="colorDepth">The color depth to use for rendering.</param>
    void SetAttributes(Attrs attrs, ColorDepth colorDepth);

    /// <summary>
    /// Disables automatic line wrapping.
    /// </summary>
    void DisableAutowrap();

    /// <summary>
    /// Enables automatic line wrapping.
    /// </summary>
    void EnableAutowrap();

    #endregion

    #region Mouse

    /// <summary>
    /// Enables mouse support with all tracking modes.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Enables: basic mouse tracking (1000), button-event tracking (1003),
    /// extended coordinates (1015), and SGR extended mode (1006).
    /// </para>
    /// </remarks>
    void EnableMouseSupport();

    /// <summary>
    /// Disables all mouse tracking modes.
    /// </summary>
    void DisableMouseSupport();

    #endregion

    #region Bracketed Paste

    /// <summary>
    /// Enables bracketed paste mode.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When enabled, pasted text is wrapped with escape sequences that allow the
    /// application to distinguish pasted text from typed text.
    /// </para>
    /// </remarks>
    void EnableBracketedPaste();

    /// <summary>
    /// Disables bracketed paste mode.
    /// </summary>
    void DisableBracketedPaste();

    #endregion

    #region Title

    /// <summary>
    /// Sets the terminal window title.
    /// </summary>
    /// <param name="title">The title to set.</param>
    /// <remarks>
    /// <para>
    /// Control characters (ESC and BEL) are stripped from the title to prevent
    /// escape sequence injection.
    /// </para>
    /// <para>
    /// On some terminals (linux console, eterm-color), title setting is not supported
    /// and this method does nothing.
    /// </para>
    /// </remarks>
    void SetTitle(string title);

    /// <summary>
    /// Clears the terminal window title.
    /// </summary>
    void ClearTitle();

    #endregion

    #region Bell

    /// <summary>
    /// Sounds the terminal bell.
    /// </summary>
    /// <remarks>
    /// <para>
    /// If bell is disabled in the output configuration, this method does nothing.
    /// </para>
    /// </remarks>
    void Bell();

    #endregion

    #region Cursor Position Report

    /// <summary>
    /// Requests a cursor position report from the terminal.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The terminal responds with an escape sequence containing the current
    /// cursor position: <c>\x1b[{row};{col}R</c>.
    /// </para>
    /// <para>
    /// Check <see cref="RespondsToCpr"/> before calling to determine if the
    /// terminal supports CPR.
    /// </para>
    /// </remarks>
    void AskForCpr();

    /// <summary>
    /// Gets whether this output responds to cursor position report requests.
    /// </summary>
    bool RespondsToCpr { get; }

    /// <summary>
    /// Resets the cursor key mode to normal (non-application) mode.
    /// </summary>
    void ResetCursorKeyMode();

    #endregion

    #region Terminal Information

    /// <summary>
    /// Gets the terminal size in rows and columns.
    /// </summary>
    /// <returns>The terminal size.</returns>
    /// <remarks>
    /// <para>
    /// Returns a default size of 80 columns by 24 rows if the actual size
    /// cannot be determined.
    /// </para>
    /// </remarks>
    Size GetSize();

    /// <summary>
    /// Gets the file descriptor number for the output stream.
    /// </summary>
    /// <returns>The file descriptor number.</returns>
    /// <exception cref="NotImplementedException">
    /// Thrown if the output is not backed by a file (e.g., DummyOutput).
    /// </exception>
    int Fileno();

    /// <summary>
    /// Gets the output encoding.
    /// </summary>
    string Encoding { get; }

    /// <summary>
    /// Gets the default color depth for this output.
    /// </summary>
    /// <returns>The default color depth.</returns>
    ColorDepth GetDefaultColorDepth();

    #endregion

    #region Windows-Specific (Optional)

    /// <summary>
    /// Scrolls the buffer to position the prompt at the top (Windows-specific).
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is a Windows-specific operation for Console scrollback manipulation.
    /// On other platforms or non-Windows outputs, this method does nothing.
    /// </para>
    /// </remarks>
    void ScrollBufferToPrompt();

    /// <summary>
    /// Gets the number of rows below the current cursor position (Windows-specific).
    /// </summary>
    /// <returns>The number of rows below the cursor, or 0 if not applicable.</returns>
    int GetRowsBelowCursorPosition();

    #endregion
}
