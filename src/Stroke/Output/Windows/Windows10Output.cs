using System.Runtime.Versioning;
using Stroke.Core;
using Stroke.Core.Primitives;
using Stroke.CursorShapes;
using Stroke.Input.Windows;
using Stroke.Styles;

namespace Stroke.Output.Windows;

/// <summary>
/// Windows 10 output abstraction that enables and uses VT100 escape sequences.
/// </summary>
/// <remarks>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>Windows10_Output</c> class
/// from <c>prompt_toolkit.output.windows10</c>.
/// </para>
/// <para>
/// This output class is a proxy to both <see cref="Win32Output"/> and <see cref="Vt100Output"/>.
/// It uses Win32Output for console sizing, cursor position queries, and scroll operations,
/// but all rendering (text output, cursor movement, colors) happens through Vt100Output.
/// </para>
/// <para>
/// The key difference from <see cref="ConEmuOutput"/> is that this class temporarily enables
/// VT100 processing mode during each flush operation, then restores the original console mode.
/// This allows VT100 escape sequences to work on Windows 10+ consoles that don't have VT100
/// enabled by default.
/// </para>
/// <para>
/// This class is thread-safe. Flush operations are serialized using a per-instance lock
/// to prevent interleaved enable/restore sequences.
/// </para>
/// </remarks>
[SupportedOSPlatform("windows")]
public sealed class Windows10Output : IOutput, IDisposable
{
    private readonly Win32Output _win32Output;
    private readonly Vt100Output _vt100Output;
    private readonly nint _hconsole;
    private readonly ColorDepth? _defaultColorDepth;
    private readonly Lock _lock = new();

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
    /// Initializes a new instance of the <see cref="Windows10Output"/> class.
    /// </summary>
    /// <param name="stdout">The output stream to write to.</param>
    /// <param name="defaultColorDepth">
    /// Optional override for default color depth. If null, <see cref="ColorDepth.Depth24Bit"/>
    /// (true color) is used as the default.
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
    public Windows10Output(TextWriter stdout, ColorDepth? defaultColorDepth = null)
    {
        ArgumentNullException.ThrowIfNull(stdout);

        if (!PlatformUtils.IsWindows)
        {
            throw new PlatformNotSupportedException("Windows10Output is only supported on Windows.");
        }

        // Store the color depth override
        _defaultColorDepth = defaultColorDepth;

        // Create Win32Output first (may throw NoConsoleScreenBufferError)
        _win32Output = new Win32Output(stdout, defaultColorDepth: defaultColorDepth);

        // Create Vt100Output second
        _vt100Output = Vt100Output.FromPty(stdout, defaultColorDepth: defaultColorDepth);

        // Store the console handle once (FR-015: not re-acquired during each Flush)
        _hconsole = ConsoleApi.GetStdHandle(ConsoleApi.STD_OUTPUT_HANDLE);
    }

    #region Writing (delegated to Vt100Output)

    /// <inheritdoc />
    public void Write(string data) => _vt100Output.Write(data);

    /// <inheritdoc />
    public void WriteRaw(string data) => _vt100Output.WriteRaw(data);

    /// <inheritdoc />
    /// <remarks>
    /// <para>
    /// This method temporarily enables VT100 processing mode, flushes the VT100 output,
    /// then restores the original console mode.
    /// </para>
    /// <para>
    /// Thread-safe: Uses per-instance locking to serialize flush operations.
    /// </para>
    /// </remarks>
    public void Flush()
    {
        using (_lock.EnterScope())
        {
            // Get current console mode (may fail if no console attached)
            if (!ConsoleApi.GetConsoleMode(_hconsole, out var originalMode))
            {
                // If GetConsoleMode fails, proceed without VT100 mode switching
                _vt100Output.Flush();
                return;
            }

            // Enable VT100 processing: ENABLE_PROCESSED_INPUT | ENABLE_VIRTUAL_TERMINAL_PROCESSING
            var vt100Mode = ConsoleApi.ENABLE_PROCESSED_INPUT | ConsoleApi.ENABLE_VIRTUAL_TERMINAL_PROCESSING;
            ConsoleApi.SetConsoleMode(_hconsole, vt100Mode);

            try
            {
                _vt100Output.Flush();
            }
            finally
            {
                // Always restore original console mode
                ConsoleApi.SetConsoleMode(_hconsole, originalMode);
            }
        }
    }

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

    #region Mouse (delegated to Vt100Output)

    // NOTE: Now that we use "virtual terminal input" on Windows, both input
    // and output are done through ANSI escape sequences. This means we should
    // enable mouse support by calling the vt100_output, not win32_output.
    // See Python Prompt Toolkit windows10.py lines 68-86.

    /// <inheritdoc />
    public void EnableMouseSupport() => _vt100Output.EnableMouseSupport();

    /// <inheritdoc />
    public void DisableMouseSupport() => _vt100Output.DisableMouseSupport();

    #endregion

    #region Bracketed Paste (delegated to Vt100Output)

    // NOTE: Same rationale as mouse support - with virtual terminal input,
    // bracketed paste uses ANSI escape sequences via vt100_output.

    /// <inheritdoc />
    public void EnableBracketedPaste() => _vt100Output.EnableBracketedPaste();

    /// <inheritdoc />
    public void DisableBracketedPaste() => _vt100Output.DisableBracketedPaste();

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
    /// <remarks>
    /// Returns <see cref="ColorDepth.Depth24Bit"/> (true color) by default.
    /// Windows 10 has supported 24-bit color since 2016.
    /// </remarks>
    public ColorDepth GetDefaultColorDepth() => _defaultColorDepth ?? ColorDepth.Depth24Bit;

    #endregion

    #region Windows-Specific (delegated to Win32Output)

    /// <inheritdoc />
    public void ScrollBufferToPrompt() => _win32Output.ScrollBufferToPrompt();

    /// <inheritdoc />
    public int GetRowsBelowCursorPosition() => _win32Output.GetRowsBelowCursorPosition();

    #endregion

    #region Synchronized Output

    /// <inheritdoc />
    public void BeginSynchronizedOutput() => _vt100Output.BeginSynchronizedOutput();

    /// <inheritdoc />
    public void EndSynchronizedOutput() => _vt100Output.EndSynchronizedOutput();

    #endregion

    #region IDisposable

    /// <summary>
    /// Disposes the underlying Win32Output, releasing any console handles it owns.
    /// </summary>
    public void Dispose() => _win32Output.Dispose();

    #endregion
}
