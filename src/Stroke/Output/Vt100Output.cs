using System.Text;
using Stroke.Core.Primitives;
using Stroke.CursorShapes;
using Stroke.Styles;

namespace Stroke.Output;

/// <summary>
/// VT100/ANSI terminal output implementation.
/// </summary>
/// <remarks>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>Vt100_Output</c> class
/// from <c>prompt_toolkit.output.vt100</c>.
/// </para>
/// <para>
/// This class is thread-safe. All mutable state is protected by synchronization.
/// Individual method calls are atomic; callers are responsible for synchronizing
/// compound operations (e.g., CursorGoto + Write + Flush).
/// </para>
/// </remarks>
public sealed partial class Vt100Output : IOutput
{
    private readonly TextWriter _stdout;
    private readonly Func<Size>? _getSize;
    private readonly string? _term;
    private readonly ColorDepth? _defaultColorDepth;
    private readonly bool _enableBell;
    private readonly bool _enableCpr;

    private readonly Lock _lock = new();
    private readonly List<string> _buffer = [];

    private bool? _cursorVisible;
    private bool _cursorShapeChanged;

    /// <summary>
    /// Initializes a new instance of the <see cref="Vt100Output"/> class.
    /// </summary>
    /// <param name="stdout">The output stream.</param>
    /// <param name="getSize">Function to get terminal size, or null to use defaults.</param>
    /// <param name="term">The TERM environment variable value.</param>
    /// <param name="defaultColorDepth">The default color depth, or null to auto-detect.</param>
    /// <param name="enableBell">Whether to enable the terminal bell.</param>
    /// <param name="enableCpr">Whether to enable cursor position reporting.</param>
    private Vt100Output(
        TextWriter stdout,
        Func<Size>? getSize,
        string? term,
        ColorDepth? defaultColorDepth,
        bool enableBell,
        bool enableCpr)
    {
        _stdout = stdout;
        _getSize = getSize;
        _term = term;
        _defaultColorDepth = defaultColorDepth;
        _enableBell = enableBell;
        _enableCpr = enableCpr;
    }

    /// <summary>
    /// Creates a <see cref="Vt100Output"/> instance from a PTY (pseudo-terminal).
    /// </summary>
    /// <param name="stdout">The output stream.</param>
    /// <param name="term">The TERM environment variable value, or null to read from environment.</param>
    /// <param name="defaultColorDepth">The default color depth, or null to auto-detect.</param>
    /// <param name="enableBell">Whether to enable the terminal bell (default: true).</param>
    /// <param name="enableCpr">Whether to enable cursor position reporting (default: true).</param>
    /// <returns>A new <see cref="Vt100Output"/> instance.</returns>
    public static Vt100Output FromPty(
        TextWriter stdout,
        string? term = null,
        ColorDepth? defaultColorDepth = null,
        bool enableBell = true,
        bool enableCpr = true)
    {
        term ??= Environment.GetEnvironmentVariable("TERM");

        return new Vt100Output(
            stdout,
            getSize: null,
            term,
            defaultColorDepth,
            enableBell,
            enableCpr);
    }

    #region Writing

    /// <inheritdoc/>
    public void Write(string data)
    {
        ArgumentNullException.ThrowIfNull(data);

        // Replace escape character with '?' to prevent injection
        var escaped = data.Replace("\x1b", "?");

        using (_lock.EnterScope())
        {
            _buffer.Add(escaped);
        }
    }

    /// <inheritdoc/>
    public void WriteRaw(string data)
    {
        ArgumentNullException.ThrowIfNull(data);

        using (_lock.EnterScope())
        {
            _buffer.Add(data);
        }
    }

    /// <inheritdoc/>
    public void Flush()
    {
        string output;

        using (_lock.EnterScope())
        {
            if (_buffer.Count == 0)
            {
                return;
            }

            output = string.Concat(_buffer);
            _buffer.Clear();
        }

        try
        {
            _stdout.Write(output);
            _stdout.Flush();
        }
        catch (IOException)
        {
            // NFR-006: Be resilient to I/O exceptions during Flush
            // Log and continue (in production, we'd use a logger)
        }
    }

    #endregion

    #region Screen Control

    /// <inheritdoc/>
    public void EraseScreen()
    {
        WriteRaw("\x1b[2J");
    }

    /// <inheritdoc/>
    public void EraseEndOfLine()
    {
        WriteRaw("\x1b[K");
    }

    /// <inheritdoc/>
    public void EraseDown()
    {
        WriteRaw("\x1b[J");
    }

    /// <inheritdoc/>
    public void EnterAlternateScreen()
    {
        WriteRaw("\x1b[?1049h\x1b[H");
    }

    /// <inheritdoc/>
    public void QuitAlternateScreen()
    {
        WriteRaw("\x1b[?1049l");
    }

    #endregion

    #region Cursor Movement

    /// <inheritdoc/>
    public void CursorGoto(int row, int column)
    {
        // VT100 uses 1-based indexing; 0 is treated as 1
        row = Math.Max(1, row);
        column = Math.Max(1, column);
        WriteRaw($"\x1b[{row};{column}H");
    }

    /// <inheritdoc/>
    public void CursorUp(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        // Optimized sequence for amount=1
        WriteRaw(amount == 1 ? "\x1b[A" : $"\x1b[{amount}A");
    }

    /// <inheritdoc/>
    public void CursorDown(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        WriteRaw(amount == 1 ? "\x1b[B" : $"\x1b[{amount}B");
    }

    /// <inheritdoc/>
    public void CursorForward(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        WriteRaw(amount == 1 ? "\x1b[C" : $"\x1b[{amount}C");
    }

    /// <inheritdoc/>
    public void CursorBackward(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        // Use backspace for amount=1, escape sequence for larger amounts
        WriteRaw(amount == 1 ? "\b" : $"\x1b[{amount}D");
    }

    #endregion

    #region Cursor Visibility

    /// <inheritdoc/>
    public void HideCursor()
    {
        using (_lock.EnterScope())
        {
            if (_cursorVisible == false)
            {
                return; // Already hidden
            }

            _cursorVisible = false;
        }

        WriteRaw("\x1b[?25l");
    }

    /// <inheritdoc/>
    public void ShowCursor()
    {
        using (_lock.EnterScope())
        {
            if (_cursorVisible == true)
            {
                return; // Already visible
            }

            _cursorVisible = true;
        }

        // Stop blinking + show cursor
        WriteRaw("\x1b[?12l\x1b[?25h");
    }

    /// <inheritdoc/>
    public void SetCursorShape(CursorShape shape)
    {
        if (shape == CursorShape.NeverChange)
        {
            return;
        }

        var sequence = shape.GetEscapeSequence();
        if (sequence is not null)
        {
            using (_lock.EnterScope())
            {
                _cursorShapeChanged = true;
            }

            WriteRaw(sequence);
        }
    }

    /// <inheritdoc/>
    public void ResetCursorShape()
    {
        bool shouldReset;

        using (_lock.EnterScope())
        {
            shouldReset = _cursorShapeChanged;
            _cursorShapeChanged = false;
        }

        if (shouldReset)
        {
            WriteRaw("\x1b[0 q");
        }
    }

    #endregion

    #region Terminal Features

    /// <inheritdoc/>
    public void EnableMouseSupport()
    {
        // Enable: basic (1000), button-event/drag (1003), urxvt extended (1015), SGR extended (1006)
        WriteRaw("\x1b[?1000h\x1b[?1003h\x1b[?1015h\x1b[?1006h");
    }

    /// <inheritdoc/>
    public void DisableMouseSupport()
    {
        // Disable all mouse modes
        WriteRaw("\x1b[?1000l\x1b[?1003l\x1b[?1015l\x1b[?1006l");
    }

    /// <inheritdoc/>
    public void EnableBracketedPaste()
    {
        WriteRaw("\x1b[?2004h");
    }

    /// <inheritdoc/>
    public void DisableBracketedPaste()
    {
        WriteRaw("\x1b[?2004l");
    }

    /// <inheritdoc/>
    public void SetTitle(string title)
    {
        // Some terminals don't support title setting
        if (_term is "linux" or "eterm-color")
        {
            return;
        }

        // Strip ESC and BEL characters to prevent escape sequence injection
        var sanitized = title.Replace("\x1b", "").Replace("\x07", "");
        WriteRaw($"\x1b]2;{sanitized}\x07");
    }

    /// <inheritdoc/>
    public void ClearTitle()
    {
        if (_term is "linux" or "eterm-color")
        {
            return;
        }

        WriteRaw("\x1b]2;\x07");
    }

    /// <inheritdoc/>
    public void Bell()
    {
        if (_enableBell)
        {
            WriteRaw("\x07");
        }
    }

    /// <inheritdoc/>
    public void DisableAutowrap()
    {
        WriteRaw("\x1b[?7l");
    }

    /// <inheritdoc/>
    public void EnableAutowrap()
    {
        WriteRaw("\x1b[?7h");
    }

    /// <inheritdoc/>
    public void AskForCpr()
    {
        WriteRaw("\x1b[6n");
    }

    /// <inheritdoc/>
    public bool RespondsToCpr =>
        _enableCpr && !Console.IsOutputRedirected;

    /// <inheritdoc/>
    public void ResetCursorKeyMode()
    {
        WriteRaw("\x1b[?1l");
    }

    #endregion

    #region Terminal Information

    /// <inheritdoc/>
    public Size GetSize()
    {
        if (_getSize is not null)
        {
            return _getSize();
        }

        try
        {
            var width = Console.WindowWidth;
            var height = Console.WindowHeight;

            // Fall back to 80x24 if size is invalid
            if (width <= 0 || height <= 0)
            {
                return new Size(24, 80);
            }

            return new Size(height, width);
        }
        catch
        {
            // Fall back to default size
            return new Size(24, 80);
        }
    }

    /// <inheritdoc/>
    public int Fileno()
    {
        // In .NET, we don't have direct access to file descriptors
        // for Console.Out. Return 1 (stdout) as a convention.
        if (_stdout == Console.Out)
        {
            return 1;
        }

        throw new NotImplementedException("Fileno is only supported for Console.Out");
    }

    /// <inheritdoc/>
    public string Encoding => "utf-8";

    /// <inheritdoc/>
    public ColorDepth GetDefaultColorDepth()
    {
        // First check for explicit default
        if (_defaultColorDepth.HasValue)
        {
            return _defaultColorDepth.Value;
        }

        // Check environment variables
        var fromEnv = ColorDepthExtensions.FromEnvironment();
        if (fromEnv.HasValue)
        {
            return fromEnv.Value;
        }

        // Fall back to TERM-based detection
        return ColorDepthExtensions.GetDefaultForTerm(_term);
    }

    #endregion

    #region Windows-Specific

    /// <inheritdoc/>
    public void ScrollBufferToPrompt()
    {
        // Windows-specific operation - no-op on other platforms
    }

    /// <inheritdoc/>
    public int GetRowsBelowCursorPosition()
    {
        // Windows-specific operation - return 0 on other platforms
        return 0;
    }

    #endregion
}
