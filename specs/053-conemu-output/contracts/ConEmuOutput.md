# Contract: ConEmuOutput

**Feature**: 053-conemu-output
**Namespace**: `Stroke.Output.Windows`
**Date**: 2026-02-02

## Class Definition

```csharp
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
public sealed class ConEmuOutput : IOutput
```

## Constructor

```csharp
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
```

## Public Properties

```csharp
/// <summary>
/// Gets the underlying Win32 console output.
/// </summary>
/// <remarks>
/// Used for console sizing, mouse support, scrolling, and bracketed paste operations.
/// </remarks>
public Win32Output Win32Output { get; }

/// <summary>
/// Gets the underlying VT100 terminal output.
/// </summary>
/// <remarks>
/// Used for all rendering operations (cursor movement, colors, text output, etc.).
/// </remarks>
public Vt100Output Vt100Output { get; }

/// <inheritdoc />
/// <remarks>
/// Always returns <c>false</c>. Cursor Position Report is not needed on Windows.
/// </remarks>
public bool RespondsToCpr => false;

/// <inheritdoc />
public string Encoding { get; }

/// <inheritdoc />
public TextWriter? Stdout { get; }
```

## Method Delegation Summary

### Delegated to Win32Output

```csharp
public Size GetSize();
public int GetRowsBelowCursorPosition();
public void EnableMouseSupport();
public void DisableMouseSupport();
public void ScrollBufferToPrompt();
public void EnableBracketedPaste();
public void DisableBracketedPaste();
```

### Delegated to Vt100Output

```csharp
// Writing
public void Write(string data);
public void WriteRaw(string data);
public void Flush();

// Screen Control
public void EraseScreen();
public void EraseEndOfLine();
public void EraseDown();
public void EnterAlternateScreen();
public void QuitAlternateScreen();

// Cursor Movement
public void CursorGoto(int row, int column);
public void CursorUp(int amount);
public void CursorDown(int amount);
public void CursorForward(int amount);
public void CursorBackward(int amount);

// Cursor Visibility
public void HideCursor();
public void ShowCursor();
public void SetCursorShape(CursorShape shape);
public void ResetCursorShape();

// Attributes
public void ResetAttributes();
public void SetAttributes(Attrs attrs, ColorDepth colorDepth);
public void DisableAutowrap();
public void EnableAutowrap();

// Title
public void SetTitle(string title);
public void ClearTitle();

// Bell
public void Bell();

// Cursor Position Report
public void AskForCpr();
public void ResetCursorKeyMode();

// Terminal Information
public int Fileno();
public ColorDepth GetDefaultColorDepth();
```

## Python Source Reference

```python
# From prompt_toolkit/output/conemu.py

class ConEmuOutput:
    """
    ConEmu (Windows) output abstraction.

    ConEmu is a Windows console application, but it also supports ANSI escape
    sequences. This output class is actually a proxy to both `Win32Output` and
    `Vt100_Output`. It uses `Win32Output` for console sizing and scrolling, but
    all cursor movements and scrolling happens through the `Vt100_Output`.

    This way, we can have 256 colors in ConEmu and Cmder. Rendering will be
    even a little faster as well.

    http://conemu.github.io/
    http://gooseberrycreative.com/cmder/
    """

    def __init__(
        self, stdout: TextIO, default_color_depth: ColorDepth | None = None
    ) -> None:
        self.win32_output = Win32Output(stdout, default_color_depth=default_color_depth)
        self.vt100_output = Vt100_Output(
            stdout, lambda: Size(0, 0), default_color_depth=default_color_depth
        )

    @property
    def responds_to_cpr(self) -> bool:
        return False  # We don't need this on Windows.

    def __getattr__(self, name: str) -> Any:
        if name in (
            "get_size",
            "get_rows_below_cursor_position",
            "enable_mouse_support",
            "disable_mouse_support",
            "scroll_buffer_to_prompt",
            "get_win32_screen_buffer_info",
            "enable_bracketed_paste",
            "disable_bracketed_paste",
        ):
            return getattr(self.win32_output, name)
        else:
            return getattr(self.vt100_output, name)


Output.register(ConEmuOutput)
```

## Usage Example

```csharp
// Typical usage in OutputFactory or application code
if (PlatformUtils.IsConEmuAnsi)
{
    var output = new ConEmuOutput(Console.Out, defaultColorDepth: ColorDepth.Depth8Bit);

    // Rendering uses VT100 escape sequences (256 colors!)
    output.SetAttributes(new Attrs { Color = "ansired" }, ColorDepth.Depth8Bit);
    output.Write("Hello from ConEmu!");
    output.Flush();

    // But sizing uses Win32 APIs for accuracy
    var size = output.GetSize();  // Delegates to Win32Output
}
```
