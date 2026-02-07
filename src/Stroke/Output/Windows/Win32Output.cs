using System.Runtime.Versioning;
using Stroke.Core;
using Stroke.Core.Primitives;
using Stroke.CursorShapes;
using Stroke.Input.Windows;
using Stroke.Input.Windows.Win32Types;
using Stroke.Styles;

namespace Stroke.Output.Windows;

/// <summary>
/// Windows Console API-based output implementation for legacy Windows terminals.
/// </summary>
/// <remarks>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>Win32Output</c> class
/// from <c>prompt_toolkit.output.win32</c>.
/// </para>
/// <para>
/// This type is thread-safe. All mutable state is protected by a lock.
/// </para>
/// <para>
/// The Win32 Console API is used for terminals that don't support ANSI/VT100
/// escape sequences. Output is buffered and flushed character-by-character
/// to avoid rendering artifacts.
/// </para>
/// </remarks>
[SupportedOSPlatform("windows")]
public sealed partial class Win32Output : IOutput, IDisposable
{
    private readonly Lock _lock = new();
    private readonly List<string> _buffer = [];
    private readonly TextWriter _stdout;
    private readonly ColorLookupTable _colorLookupTable = new();

    private nint _hConsole;
    private nint _originalHandle;
    private nint _alternateHandle;
    private bool _inAlternateScreen;
    private bool _hidden;
    private int _defaultAttrs;
    private bool _ownsHandle;
    private bool _disposed;

    /// <summary>
    /// Gets whether to use the complete buffer width instead of visible window width.
    /// </summary>
    public bool UseCompleteWidth { get; }

    /// <summary>
    /// Gets the optional override for default color depth.
    /// </summary>
    public ColorDepth? DefaultColorDepth { get; }

    /// <summary>
    /// Initializes a new Win32Output instance.
    /// </summary>
    /// <param name="stdout">The stdout TextWriter to use.</param>
    /// <param name="useCompleteWidth">If true, use full buffer width; otherwise use visible window width.</param>
    /// <param name="defaultColorDepth">Optional override for color depth.</param>
    /// <exception cref="PlatformNotSupportedException">Thrown on non-Windows platforms.</exception>
    /// <exception cref="NoConsoleScreenBufferError">Thrown when not running in a Windows console.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="stdout"/> is null.</exception>
    public Win32Output(
        TextWriter stdout,
        bool useCompleteWidth = false,
        ColorDepth? defaultColorDepth = null)
    {
        if (!OperatingSystem.IsWindows())
        {
            throw new PlatformNotSupportedException(
                "Win32Output is only supported on Windows platforms.");
        }

        ArgumentNullException.ThrowIfNull(stdout);

        _stdout = stdout;
        UseCompleteWidth = useCompleteWidth;
        DefaultColorDepth = defaultColorDepth;

        // Get console handle
        _hConsole = ConsoleApi.GetStdHandle(ConsoleApi.STD_OUTPUT_HANDLE);
        _originalHandle = _hConsole;

        // Verify we have a valid console
        if (!ConsoleApi.GetConsoleScreenBufferInfo(_hConsole, out var info))
        {
            // Fallback: open CONOUT$ directly to bypass stdio redirection.
            // This handles ConEmu/Cmder terminals and test runners that redirect
            // stdout while a console is still attached to the process.
            _hConsole = ConsoleApi.CreateFile("CONOUT$",
                ConsoleApi.GENERIC_READ | ConsoleApi.GENERIC_WRITE,
                ConsoleApi.FILE_SHARE_READ | ConsoleApi.FILE_SHARE_WRITE,
                nint.Zero, ConsoleApi.OPEN_EXISTING, 0, nint.Zero);
            _originalHandle = _hConsole;
            _ownsHandle = true;

            if (_hConsole == ConsoleApi.INVALID_HANDLE_VALUE ||
                !ConsoleApi.GetConsoleScreenBufferInfo(_hConsole, out info))
            {
                if (_hConsole != ConsoleApi.INVALID_HANDLE_VALUE)
                {
                    ConsoleApi.CloseHandle(_hConsole);
                }

                _ownsHandle = false;
                throw new NoConsoleScreenBufferError();
            }
        }

        // Save default attributes for ResetAttributes
        _defaultAttrs = info.Attributes;
    }

    #region Text Output Methods (T013)

    /// <inheritdoc />
    public void Write(string data)
    {
        if (string.IsNullOrEmpty(data))
        {
            return;
        }

        using (_lock.EnterScope())
        {
            if (_hidden)
            {
                // Replace text with spaces matching character width
                var width = UnicodeWidth.GetWidth(data);
                data = new string(' ', width);
            }

            _buffer.Add(data);
        }
    }

    /// <inheritdoc />
    public void WriteRaw(string data)
    {
        if (string.IsNullOrEmpty(data))
        {
            return;
        }

        using (_lock.EnterScope())
        {
            _buffer.Add(data);
        }
    }

    /// <inheritdoc />
    public void Flush()
    {
        using (_lock.EnterScope())
        {
            if (_buffer.Count == 0)
            {
                return;
            }

            // Write character-by-character to avoid rendering artifacts
            // (Windows Console rendering bug workaround)
            foreach (var data in _buffer)
            {
                foreach (var ch in data)
                {
                    ConsoleApi.WriteConsole(
                        _hConsole,
                        ch.ToString(),
                        1,
                        out _,
                        nint.Zero);
                }
            }

            _buffer.Clear();
        }
    }

    #endregion

    #region Cursor Positioning (T014)

    /// <inheritdoc />
    public void CursorGoto(int row, int column)
    {
        using (_lock.EnterScope())
        {
            // Get buffer info for bounds clamping
            if (!ConsoleApi.GetConsoleScreenBufferInfo(_hConsole, out var info))
            {
                return;
            }

            // Clamp to valid range (0-based coordinates)
            var x = (short)Math.Clamp(column, 0, info.Size.X - 1);
            var y = (short)Math.Clamp(row, 0, info.Size.Y - 1);

            ConsoleApi.SetConsoleCursorPosition(_hConsole, new Coord(x, y));
        }
    }

    /// <inheritdoc />
    public void CursorUp(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        using (_lock.EnterScope())
        {
            if (!ConsoleApi.GetConsoleScreenBufferInfo(_hConsole, out var info))
            {
                return;
            }

            var newY = (short)Math.Max(0, info.CursorPosition.Y - amount);
            ConsoleApi.SetConsoleCursorPosition(_hConsole, new Coord(info.CursorPosition.X, newY));
        }
    }

    /// <inheritdoc />
    public void CursorDown(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        using (_lock.EnterScope())
        {
            if (!ConsoleApi.GetConsoleScreenBufferInfo(_hConsole, out var info))
            {
                return;
            }

            var newY = (short)Math.Min(info.Size.Y - 1, info.CursorPosition.Y + amount);
            ConsoleApi.SetConsoleCursorPosition(_hConsole, new Coord(info.CursorPosition.X, newY));
        }
    }

    /// <inheritdoc />
    public void CursorForward(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        using (_lock.EnterScope())
        {
            if (!ConsoleApi.GetConsoleScreenBufferInfo(_hConsole, out var info))
            {
                return;
            }

            var newX = (short)Math.Min(info.Size.X - 1, info.CursorPosition.X + amount);
            ConsoleApi.SetConsoleCursorPosition(_hConsole, new Coord(newX, info.CursorPosition.Y));
        }
    }

    /// <inheritdoc />
    public void CursorBackward(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        using (_lock.EnterScope())
        {
            if (!ConsoleApi.GetConsoleScreenBufferInfo(_hConsole, out var info))
            {
                return;
            }

            var newX = (short)Math.Max(0, info.CursorPosition.X - amount);
            ConsoleApi.SetConsoleCursorPosition(_hConsole, new Coord(newX, info.CursorPosition.Y));
        }
    }

    #endregion

    #region Size and Position (T015)

    /// <inheritdoc />
    public Size GetSize()
    {
        using (_lock.EnterScope())
        {
            if (!ConsoleApi.GetConsoleScreenBufferInfo(_hConsole, out var info))
            {
                // Return default size if we can't get buffer info
                return new Size(24, 80);
            }

            int width;
            if (UseCompleteWidth)
            {
                width = info.Size.X;
            }
            else
            {
                // Use visible window region (avoid right margin for wrapping)
                width = info.Window.Right - info.Window.Left;
            }

            // Clamp to buffer width - 1 to avoid wrapping issues
            width = Math.Min(width, info.Size.X - 1);

            var height = info.Window.Bottom - info.Window.Top + 1;

            return new Size(height, width);
        }
    }

    /// <inheritdoc />
    public int GetRowsBelowCursorPosition()
    {
        using (_lock.EnterScope())
        {
            if (!ConsoleApi.GetConsoleScreenBufferInfo(_hConsole, out var info))
            {
                return 0;
            }

            // Rows from cursor to bottom of visible window (inclusive, hence +1).
            // Matches Python PTK: info.srWindow.Bottom - info.dwCursorPosition.Y + 1
            return info.Window.Bottom - info.CursorPosition.Y + 1;
        }
    }

    #endregion

    #region Properties (T016)

    /// <inheritdoc />
    public string Encoding => "utf-16";

    /// <inheritdoc />
    public int Fileno() => -1;

    /// <inheritdoc />
    public bool RespondsToCpr => false;

    /// <inheritdoc />
    public TextWriter? Stdout => _stdout;

    /// <inheritdoc />
    public ColorDepth GetDefaultColorDepth()
    {
        // Use override if provided, otherwise 4-bit (16-color) for Win32 console
        return DefaultColorDepth ?? ColorDepth.Depth4Bit;
    }

    #endregion

    #region Screen Erase (Stub - Implemented in T030-T032)

    /// <inheritdoc />
    public void EraseScreen()
    {
        // Implemented in Phase 6 (T030)
        using (_lock.EnterScope())
        {
            if (!ConsoleApi.GetConsoleScreenBufferInfo(_hConsole, out var info))
            {
                return;
            }

            var bufferSize = info.Size.X * info.Size.Y;
            var startCoord = new Coord(0, 0);

            // Fill with spaces
            ConsoleApi.FillConsoleOutputCharacter(
                _hConsole,
                ' ',
                (uint)bufferSize,
                startCoord.ToInt32(),
                out _);

            // Fill with current attributes
            ConsoleApi.FillConsoleOutputAttribute(
                _hConsole,
                (ushort)_defaultAttrs,
                (uint)bufferSize,
                startCoord.ToInt32(),
                out _);

            // Move cursor to home
            ConsoleApi.SetConsoleCursorPosition(_hConsole, startCoord);
        }
    }

    /// <inheritdoc />
    public void EraseEndOfLine()
    {
        // Implemented in Phase 6 (T031)
        using (_lock.EnterScope())
        {
            if (!ConsoleApi.GetConsoleScreenBufferInfo(_hConsole, out var info))
            {
                return;
            }

            var length = info.Size.X - info.CursorPosition.X;
            var startCoord = info.CursorPosition;

            ConsoleApi.FillConsoleOutputCharacter(
                _hConsole,
                ' ',
                (uint)length,
                startCoord.ToInt32(),
                out _);

            ConsoleApi.FillConsoleOutputAttribute(
                _hConsole,
                info.Attributes,
                (uint)length,
                startCoord.ToInt32(),
                out _);
        }
    }

    /// <inheritdoc />
    public void EraseDown()
    {
        // Implemented in Phase 6 (T032)
        using (_lock.EnterScope())
        {
            if (!ConsoleApi.GetConsoleScreenBufferInfo(_hConsole, out var info))
            {
                return;
            }

            // Calculate cells from cursor to end of buffer
            var cursorOffset = info.CursorPosition.Y * info.Size.X + info.CursorPosition.X;
            var bufferSize = info.Size.X * info.Size.Y;
            var length = bufferSize - cursorOffset;

            var startCoord = info.CursorPosition;

            ConsoleApi.FillConsoleOutputCharacter(
                _hConsole,
                ' ',
                (uint)length,
                startCoord.ToInt32(),
                out _);

            ConsoleApi.FillConsoleOutputAttribute(
                _hConsole,
                info.Attributes,
                (uint)length,
                startCoord.ToInt32(),
                out _);
        }
    }

    #endregion

    #region Alternate Screen (Stub - Implemented in T026-T028)

    /// <inheritdoc />
    public void EnterAlternateScreen()
    {
        // Implemented in Phase 5 (T026)
        using (_lock.EnterScope())
        {
            if (_inAlternateScreen)
            {
                return; // Idempotent
            }

            // Create new screen buffer
            _alternateHandle = ConsoleApi.CreateConsoleScreenBuffer(
                ConsoleApi.GENERIC_READ | ConsoleApi.GENERIC_WRITE,
                0, // No sharing
                nint.Zero,
                ConsoleApi.CONSOLE_TEXTMODE_BUFFER,
                nint.Zero);

            if (_alternateHandle == ConsoleApi.INVALID_HANDLE_VALUE)
            {
                throw new NoConsoleScreenBufferError("Failed to create alternate screen buffer.");
            }

            // Activate alternate buffer
            if (!ConsoleApi.SetConsoleActiveScreenBuffer(_alternateHandle))
            {
                ConsoleApi.CloseHandle(_alternateHandle);
                _alternateHandle = nint.Zero;
                throw new NoConsoleScreenBufferError("Failed to activate alternate screen buffer.");
            }

            _hConsole = _alternateHandle;
            _inAlternateScreen = true;
        }
    }

    /// <inheritdoc />
    public void QuitAlternateScreen()
    {
        // Implemented in Phase 5 (T027)
        using (_lock.EnterScope())
        {
            if (!_inAlternateScreen)
            {
                return; // Idempotent
            }

            // Restore original buffer
            ConsoleApi.SetConsoleActiveScreenBuffer(_originalHandle);

            // Close alternate buffer handle
            if (_alternateHandle != nint.Zero)
            {
                ConsoleApi.CloseHandle(_alternateHandle);
                _alternateHandle = nint.Zero;
            }

            _hConsole = _originalHandle;
            _inAlternateScreen = false;
        }
    }

    #endregion

    #region Cursor Visibility (Stub - Implemented in T041)

    /// <inheritdoc />
    /// <remarks>
    /// No-op on Win32 console, matching Python Prompt Toolkit behavior.
    /// </remarks>
    public void HideCursor()
    {
        // No-op - matching Python Prompt Toolkit's pass implementation
    }

    /// <inheritdoc />
    /// <remarks>
    /// No-op on Win32 console, matching Python Prompt Toolkit behavior.
    /// </remarks>
    public void ShowCursor()
    {
        // No-op - matching Python Prompt Toolkit's pass implementation
    }

    /// <inheritdoc />
    public void SetCursorShape(CursorShape shape)
    {
        // No-op on Win32 console (cursor shape not supported)
    }

    /// <inheritdoc />
    public void ResetCursorShape()
    {
        // No-op on Win32 console (cursor shape not supported)
    }

    #endregion

    #region Attributes (Stub - Implemented in T021-T024)

    /// <inheritdoc />
    public void ResetAttributes()
    {
        using (_lock.EnterScope())
        {
            _hidden = false;
            ConsoleApi.SetConsoleTextAttribute(_hConsole, (ushort)_defaultAttrs);
        }
    }

    /// <inheritdoc />
    public void SetAttributes(Attrs attrs, ColorDepth colorDepth)
    {
        using (_lock.EnterScope())
        {
            _hidden = attrs.Hidden ?? false;

            // Start from the default attributes (preserves non-color bits)
            int winAttrs = _defaultAttrs;

            if (colorDepth != ColorDepth.Depth1Bit)
            {
                // Override the last four bits: foreground color
                if (!string.IsNullOrEmpty(attrs.Color))
                {
                    winAttrs &= ~0x0F; // Clear fg bits
                    winAttrs |= _colorLookupTable.LookupFgColor(attrs.Color);
                }

                // Override the next four bits: background color
                if (!string.IsNullOrEmpty(attrs.BgColor))
                {
                    winAttrs &= ~0xF0; // Clear bg bits
                    winAttrs |= _colorLookupTable.LookupBgColor(attrs.BgColor);
                }
            }

            // Reverse: swap the four-bit fg/bg groups
            if (attrs.Reverse ?? false)
            {
                winAttrs = (winAttrs & ~0xFF)
                    | ((winAttrs & 0x0F) << 4)
                    | ((winAttrs & 0xF0) >> 4);
            }

            ConsoleApi.SetConsoleTextAttribute(_hConsole, (ushort)winAttrs);
        }
    }

    #endregion

    #region No-op Methods (T040)

    /// <inheritdoc />
    public void DisableAutowrap()
    {
        // No-op on Win32 console (autowrap controlled by console settings)
    }

    /// <inheritdoc />
    public void EnableAutowrap()
    {
        // No-op on Win32 console (autowrap controlled by console settings)
    }

    /// <inheritdoc />
    public void EnableBracketedPaste()
    {
        // No-op on Win32 console (bracketed paste not supported)
    }

    /// <inheritdoc />
    public void DisableBracketedPaste()
    {
        // No-op on Win32 console (bracketed paste not supported)
    }

    /// <inheritdoc />
    public void ResetCursorKeyMode()
    {
        // No-op on Win32 console (cursor key mode not supported)
    }

    /// <inheritdoc />
    public void AskForCpr()
    {
        // No-op on Win32 console (CPR not supported)
    }

    /// <inheritdoc />
    /// <remarks>
    /// Scrolls the console window so the cursor is near the bottom.
    /// Called before drawing the prompt to ensure sufficient visible space.
    /// Port of Python PTK's <c>Win32Output.scroll_buffer_to_prompt</c>.
    /// </remarks>
    public void ScrollBufferToPrompt()
    {
        using (_lock.EnterScope())
        {
            if (!ConsoleApi.GetConsoleScreenBufferInfo(_hConsole, out var info))
            {
                return;
            }

            var sr = info.Window;
            var cursorY = info.CursorPosition.Y;

            // Scroll to the left, keep the same window width.
            short left = 0;
            short right = (short)(sr.Right - sr.Left);

            // Vertical scroll: if cursor is already visible within the window
            // (not at the very bottom edge), keep the current vertical position.
            // Otherwise, scroll so the cursor is at the bottom of the window.
            short winHeight = (short)(sr.Bottom - sr.Top);
            short bottom;
            if (0 < sr.Bottom - cursorY && sr.Bottom - cursorY < winHeight - 1)
            {
                // Cursor is on-screen with margin — no vertical scroll needed.
                bottom = sr.Bottom;
            }
            else
            {
                bottom = (short)Math.Max(winHeight, cursorY);
            }
            short top = (short)(bottom - winHeight);

            var newWindow = new SmallRect(left, top, right, bottom);
            ConsoleApi.SetConsoleWindowInfo(_hConsole, true, in newWindow);
        }
    }

    #endregion

    #region Mouse Support (Stub - Implemented in T034-T035)

    /// <inheritdoc />
    public void EnableMouseSupport()
    {
        // Implemented in Phase 7 (T034)
        var handle = ConsoleApi.GetStdHandle(ConsoleApi.STD_INPUT_HANDLE);
        if (ConsoleApi.GetConsoleMode(handle, out var mode))
        {
            // Enable mouse input, disable quick edit mode
            mode = (mode | ConsoleApi.ENABLE_MOUSE_INPUT) & ~ConsoleApi.ENABLE_QUICK_EDIT_MODE;
            ConsoleApi.SetConsoleMode(handle, mode);
        }
    }

    /// <inheritdoc />
    public void DisableMouseSupport()
    {
        // Implemented in Phase 7 (T035)
        var handle = ConsoleApi.GetStdHandle(ConsoleApi.STD_INPUT_HANDLE);
        if (ConsoleApi.GetConsoleMode(handle, out var mode))
        {
            mode &= ~ConsoleApi.ENABLE_MOUSE_INPUT;
            ConsoleApi.SetConsoleMode(handle, mode);
        }
    }

    #endregion

    #region Title (Stub - Implemented in T042)

    /// <inheritdoc />
    public void SetTitle(string title)
    {
        ConsoleApi.SetConsoleTitle(title ?? string.Empty);
    }

    /// <inheritdoc />
    public void ClearTitle()
    {
        ConsoleApi.SetConsoleTitle(string.Empty);
    }

    #endregion

    #region Bell (Stub - Implemented in T043)

    /// <inheritdoc />
    public void Bell()
    {
        // Write BEL character
        using (_lock.EnterScope())
        {
            ConsoleApi.WriteConsole(_hConsole, "\a", 1, out _, nint.Zero);
        }
    }

    #endregion

    #region Static Methods (T044)

    /// <summary>
    /// Forces a repaint of the console window.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Call this when the application paints background for completion menus.
    /// When the menu disappears, it may leave traces due to a Windows Console bug.
    /// This sends a repaint request to solve it.
    /// </para>
    /// </remarks>
    [SupportedOSPlatform("windows")]
    public static void Win32RefreshWindow()
    {
        var hwnd = ConsoleApi.GetConsoleWindow();
        if (hwnd != nint.Zero)
        {
            ConsoleApi.RedrawWindow(hwnd, nint.Zero, nint.Zero, ConsoleApi.RDW_INVALIDATE);
        }
    }

    #endregion

    #region Synchronized Output

    /// <inheritdoc />
    public void BeginSynchronizedOutput()
    {
        // No-op on Win32 console (no equivalent to DEC Mode 2026)
    }

    /// <inheritdoc />
    public void EndSynchronizedOutput()
    {
        // No-op on Win32 console (no equivalent to DEC Mode 2026)
    }

    #endregion

    #region IDisposable

    /// <summary>
    /// Releases the console handle if it was opened by this instance via <c>CreateFileW("CONOUT$")</c>.
    /// </summary>
    /// <remarks>
    /// Handles obtained from <c>GetStdHandle</c> are shared process-wide and must not be closed.
    /// Only handles opened via <c>CreateFileW</c> as a fallback are owned by this instance.
    /// </remarks>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        if (_inAlternateScreen)
        {
            QuitAlternateScreen();
        }

        if (_ownsHandle && _originalHandle != nint.Zero && _originalHandle != ConsoleApi.INVALID_HANDLE_VALUE)
        {
            ConsoleApi.CloseHandle(_originalHandle);
            _originalHandle = nint.Zero;
            _hConsole = nint.Zero;
        }
    }

    #endregion
}
