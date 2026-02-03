using System.Runtime.Versioning;
using Stroke.Core.Primitives;
using Stroke.CursorShapes;
using Stroke.Styles;

namespace Stroke.Output.Windows;

/// <summary>
/// ConEmu (Windows) output abstraction combining Win32Output and Vt100Output.
/// </summary>
/// <remarks>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>ConEmuOutput</c> class
/// from <c>prompt_toolkit.output.conemu</c>.
/// </para>
/// <para>
/// ConEmu is a Windows console application that also supports ANSI escape sequences.
/// This output class is a proxy to both <see cref="Win32Output"/> and <see cref="Vt100Output"/>.
/// It uses Win32Output for console sizing, mouse support, scrolling, and bracketed paste,
/// but all cursor movements and rendering happen through Vt100Output.
/// </para>
/// <para>
/// This enables 256-color and true-color support in ConEmu and Cmder terminals while
/// maintaining proper Windows console integration.
/// </para>
/// <para>
/// This class is thread-safe. Thread safety is delegated to the underlying outputs,
/// which are both thread-safe.
/// </para>
/// </remarks>
/// <seealso href="http://conemu.github.io/">ConEmu</seealso>
/// <seealso href="http://gooseberrycreative.com/cmder/">Cmder</seealso>
[SupportedOSPlatform("windows")]
public sealed class ConEmuOutput : IOutput, IDisposable
{
    private readonly Win32Output _win32Output;
    private readonly Vt100Output _vt100Output;

    /// <summary>
    /// Gets the underlying Win32 console output.
    /// </summary>
    /// <remarks>
    /// Used for console sizing, mouse support, scrolling, and bracketed paste operations.
    /// </remarks>
    public Win32Output Win32Output => _win32Output;

    /// <summary>
    /// Gets the underlying VT100 terminal output.
    /// </summary>
    /// <remarks>
    /// Used for all rendering operations (cursor movement, colors, text output, etc.).
    /// </remarks>
    public Vt100Output Vt100Output => _vt100Output;

    /// <inheritdoc />
    /// <remarks>
    /// Always returns <c>false</c>. Cursor Position Report is not needed on Windows.
    /// </remarks>
    public bool RespondsToCpr => false;

    /// <inheritdoc />
    public string Encoding => _vt100Output.Encoding;

    /// <inheritdoc />
    public TextWriter? Stdout => _vt100Output.Stdout;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConEmuOutput"/> class.
    /// </summary>
    /// <param name="stdout">The output stream to write to.</param>
    /// <param name="defaultColorDepth">
    /// Optional override for default color depth. If null, color depth is auto-detected.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="stdout"/> is null.
    /// </exception>
    /// <exception cref="PlatformNotSupportedException">
    /// Thrown on non-Windows platforms.
    /// </exception>
    /// <exception cref="NoConsoleScreenBufferError">
    /// Thrown when not running in a Windows console.
    /// </exception>
    public ConEmuOutput(TextWriter stdout, ColorDepth? defaultColorDepth = null)
    {
        ArgumentNullException.ThrowIfNull(stdout);

        // Create Win32Output first (may throw NoConsoleScreenBufferError)
        _win32Output = new Win32Output(stdout, defaultColorDepth: defaultColorDepth);

        // Create Vt100Output second
        // Pass null for term to auto-detect, and pass the colorDepth
        _vt100Output = Vt100Output.FromPty(stdout, defaultColorDepth: defaultColorDepth);
    }

    #region Writing (delegated to Vt100Output)

    /// <inheritdoc />
    public void Write(string data) => _vt100Output.Write(data);

    /// <inheritdoc />
    public void WriteRaw(string data) => _vt100Output.WriteRaw(data);

    /// <inheritdoc />
    public void Flush() => _vt100Output.Flush();

    #endregion

    #region Screen Control (delegated to Vt100Output)

    /// <inheritdoc />
    public void EraseScreen() => _vt100Output.EraseScreen();

    /// <inheritdoc />
    public void EraseEndOfLine() => _vt100Output.EraseEndOfLine();

    /// <inheritdoc />
    public void EraseDown() => _vt100Output.EraseDown();

    /// <inheritdoc />
    public void EnterAlternateScreen() => _vt100Output.EnterAlternateScreen();

    /// <inheritdoc />
    public void QuitAlternateScreen() => _vt100Output.QuitAlternateScreen();

    #endregion

    #region Cursor Movement (delegated to Vt100Output)

    /// <inheritdoc />
    public void CursorGoto(int row, int column) => _vt100Output.CursorGoto(row, column);

    /// <inheritdoc />
    public void CursorUp(int amount) => _vt100Output.CursorUp(amount);

    /// <inheritdoc />
    public void CursorDown(int amount) => _vt100Output.CursorDown(amount);

    /// <inheritdoc />
    public void CursorForward(int amount) => _vt100Output.CursorForward(amount);

    /// <inheritdoc />
    public void CursorBackward(int amount) => _vt100Output.CursorBackward(amount);

    #endregion

    #region Cursor Visibility (delegated to Vt100Output)

    /// <inheritdoc />
    public void HideCursor() => _vt100Output.HideCursor();

    /// <inheritdoc />
    public void ShowCursor() => _vt100Output.ShowCursor();

    /// <inheritdoc />
    public void SetCursorShape(CursorShape shape) => _vt100Output.SetCursorShape(shape);

    /// <inheritdoc />
    public void ResetCursorShape() => _vt100Output.ResetCursorShape();

    #endregion

    #region Attributes (delegated to Vt100Output)

    /// <inheritdoc />
    public void ResetAttributes() => _vt100Output.ResetAttributes();

    /// <inheritdoc />
    public void SetAttributes(Attrs attrs, ColorDepth colorDepth) => _vt100Output.SetAttributes(attrs, colorDepth);

    /// <inheritdoc />
    public void DisableAutowrap() => _vt100Output.DisableAutowrap();

    /// <inheritdoc />
    public void EnableAutowrap() => _vt100Output.EnableAutowrap();

    #endregion

    #region Mouse (delegated to Win32Output)

    /// <inheritdoc />
    public void EnableMouseSupport() => _win32Output.EnableMouseSupport();

    /// <inheritdoc />
    public void DisableMouseSupport() => _win32Output.DisableMouseSupport();

    #endregion

    #region Bracketed Paste (delegated to Win32Output)

    /// <inheritdoc />
    public void EnableBracketedPaste() => _win32Output.EnableBracketedPaste();

    /// <inheritdoc />
    public void DisableBracketedPaste() => _win32Output.DisableBracketedPaste();

    #endregion

    #region Title (delegated to Vt100Output)

    /// <inheritdoc />
    public void SetTitle(string title) => _vt100Output.SetTitle(title);

    /// <inheritdoc />
    public void ClearTitle() => _vt100Output.ClearTitle();

    #endregion

    #region Bell (delegated to Vt100Output)

    /// <inheritdoc />
    public void Bell() => _vt100Output.Bell();

    #endregion

    #region Cursor Position Report (delegated to Vt100Output)

    /// <inheritdoc />
    public void AskForCpr() => _vt100Output.AskForCpr();

    /// <inheritdoc />
    public void ResetCursorKeyMode() => _vt100Output.ResetCursorKeyMode();

    #endregion

    #region Terminal Information

    /// <inheritdoc />
    /// <remarks>
    /// Delegated to Win32Output for accurate Windows console sizing.
    /// </remarks>
    public Size GetSize() => _win32Output.GetSize();

    /// <inheritdoc />
    public int Fileno() => _vt100Output.Fileno();

    /// <inheritdoc />
    public ColorDepth GetDefaultColorDepth() => _vt100Output.GetDefaultColorDepth();

    #endregion

    #region Windows-Specific (delegated to Win32Output)

    /// <inheritdoc />
    public void ScrollBufferToPrompt() => _win32Output.ScrollBufferToPrompt();

    /// <inheritdoc />
    public int GetRowsBelowCursorPosition() => _win32Output.GetRowsBelowCursorPosition();

    #endregion

    #region IDisposable

    /// <summary>
    /// Disposes the underlying Win32Output, releasing any console handles it owns.
    /// </summary>
    public void Dispose() => _win32Output.Dispose();

    #endregion
}
