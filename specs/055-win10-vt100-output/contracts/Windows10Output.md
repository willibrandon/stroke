# API Contract: Windows10Output

**Feature**: 055-win10-vt100-output
**Namespace**: `Stroke.Output.Windows`
**Date**: 2026-02-03

## Windows10Output Class

Windows 10 output abstraction that enables and uses VT100 escape sequences.

```csharp
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
public sealed class Windows10Output : IOutput
{
    // Fields
    private readonly Win32Output _win32Output;
    private readonly Vt100Output _vt100Output;
    private readonly nint _hconsole;
    private readonly ColorDepth? _defaultColorDepth;
    private readonly Lock _lock;

    // Properties

    /// <summary>
    /// Gets the underlying Win32 console output.
    /// </summary>
    public Win32Output Win32Output { get; }

    /// <summary>
    /// Gets the underlying VT100 terminal output.
    /// </summary>
    public Vt100Output Vt100Output { get; }

    /// <inheritdoc />
    /// <remarks>
    /// Always returns <c>false</c>. Cursor Position Report is not needed on Windows.
    /// </remarks>
    public bool RespondsToCpr { get; } // => false

    /// <inheritdoc />
    public string Encoding { get; }

    /// <inheritdoc />
    public TextWriter? Stdout { get; }

    // Constructor

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
    public Windows10Output(TextWriter stdout, ColorDepth? defaultColorDepth = null);

    // IOutput Implementation - Writing (delegated to Vt100Output)

    /// <inheritdoc />
    public void Write(string data);

    /// <inheritdoc />
    public void WriteRaw(string data);

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
    public void Flush();

    // IOutput Implementation - Screen Control (delegated to Vt100Output)

    /// <inheritdoc />
    public void EraseScreen();

    /// <inheritdoc />
    public void EraseEndOfLine();

    /// <inheritdoc />
    public void EraseDown();

    /// <inheritdoc />
    public void EnterAlternateScreen();

    /// <inheritdoc />
    public void QuitAlternateScreen();

    // IOutput Implementation - Cursor Movement (delegated to Vt100Output)

    /// <inheritdoc />
    public void CursorGoto(int row, int column);

    /// <inheritdoc />
    public void CursorUp(int amount);

    /// <inheritdoc />
    public void CursorDown(int amount);

    /// <inheritdoc />
    public void CursorForward(int amount);

    /// <inheritdoc />
    public void CursorBackward(int amount);

    // IOutput Implementation - Cursor Visibility (delegated to Vt100Output)

    /// <inheritdoc />
    public void HideCursor();

    /// <inheritdoc />
    public void ShowCursor();

    /// <inheritdoc />
    public void SetCursorShape(CursorShape shape);

    /// <inheritdoc />
    public void ResetCursorShape();

    // IOutput Implementation - Attributes (delegated to Vt100Output)

    /// <inheritdoc />
    public void ResetAttributes();

    /// <inheritdoc />
    public void SetAttributes(Attrs attrs, ColorDepth colorDepth);

    /// <inheritdoc />
    public void DisableAutowrap();

    /// <inheritdoc />
    public void EnableAutowrap();

    // IOutput Implementation - Mouse (delegated to Win32Output)

    /// <inheritdoc />
    public void EnableMouseSupport();

    /// <inheritdoc />
    public void DisableMouseSupport();

    // IOutput Implementation - Bracketed Paste (delegated to Win32Output)

    /// <inheritdoc />
    public void EnableBracketedPaste();

    /// <inheritdoc />
    public void DisableBracketedPaste();

    // IOutput Implementation - Title (delegated to Vt100Output)

    /// <inheritdoc />
    public void SetTitle(string title);

    /// <inheritdoc />
    public void ClearTitle();

    // IOutput Implementation - Bell (delegated to Vt100Output)

    /// <inheritdoc />
    public void Bell();

    // IOutput Implementation - Cursor Position Report (delegated to Vt100Output)

    /// <inheritdoc />
    public void AskForCpr();

    /// <inheritdoc />
    public void ResetCursorKeyMode();

    // IOutput Implementation - Terminal Information

    /// <inheritdoc />
    /// <remarks>
    /// Delegated to Win32Output for accurate Windows console sizing.
    /// </remarks>
    public Size GetSize();

    /// <inheritdoc />
    public int Fileno();

    /// <inheritdoc />
    /// <remarks>
    /// Returns <see cref="ColorDepth.Depth24Bit"/> (true color) by default.
    /// Windows 10 has supported 24-bit color since 2016.
    /// </remarks>
    public ColorDepth GetDefaultColorDepth();

    // IOutput Implementation - Windows-Specific (delegated to Win32Output)

    /// <inheritdoc />
    public void ScrollBufferToPrompt();

    /// <inheritdoc />
    public int GetRowsBelowCursorPosition();
}
```

## WindowsVt100Support Class

Static utility class for detecting VT100 escape sequence support on Windows.

```csharp
namespace Stroke.Output.Windows;

/// <summary>
/// Utility class for detecting VT100 escape sequence support on Windows.
/// </summary>
/// <remarks>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>is_win_vt100_enabled()</c> function
/// from <c>prompt_toolkit.output.windows10</c>.
/// </para>
/// <para>
/// The detection works by temporarily attempting to enable VT100 processing mode on the
/// console output handle. If successful, VT100 escape sequences are supported. The original
/// console mode is always restored after the check.
/// </para>
/// </remarks>
[SupportedOSPlatform("windows")]
public static class WindowsVt100Support
{
    /// <summary>
    /// Returns <c>true</c> when running on Windows and VT100 escape sequences are supported.
    /// </summary>
    /// <returns>
    /// <c>true</c> if VT100 escape sequences can be enabled on the current console;
    /// otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method attempts to enable the <c>ENABLE_VIRTUAL_TERMINAL_PROCESSING</c> console
    /// mode flag on the stdout handle. If the call succeeds, VT100 is supported.
    /// </para>
    /// <para>
    /// The original console mode is always restored after the check, so this method has
    /// no side effects on the console state.
    /// </para>
    /// </remarks>
    public static bool IsVt100Enabled();
}
```

## Constants Used

```csharp
// From Stroke.Input.Windows.ConsoleApi
public const uint ENABLE_PROCESSED_INPUT = 0x0001;
public const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;
public const int STD_OUTPUT_HANDLE = -11;
```

## Delegation Map

| IOutput Method | Delegated To | Notes |
|----------------|--------------|-------|
| Write | Vt100Output | |
| WriteRaw | Vt100Output | |
| Flush | **Custom** | VT100 mode switching |
| EraseScreen | Vt100Output | |
| EraseEndOfLine | Vt100Output | |
| EraseDown | Vt100Output | |
| EnterAlternateScreen | Vt100Output | |
| QuitAlternateScreen | Vt100Output | |
| CursorGoto | Vt100Output | |
| CursorUp | Vt100Output | |
| CursorDown | Vt100Output | |
| CursorForward | Vt100Output | |
| CursorBackward | Vt100Output | |
| HideCursor | Vt100Output | |
| ShowCursor | Vt100Output | |
| SetCursorShape | Vt100Output | |
| ResetCursorShape | Vt100Output | |
| ResetAttributes | Vt100Output | |
| SetAttributes | Vt100Output | |
| DisableAutowrap | Vt100Output | |
| EnableAutowrap | Vt100Output | |
| EnableMouseSupport | Win32Output | |
| DisableMouseSupport | Win32Output | |
| EnableBracketedPaste | Win32Output | |
| DisableBracketedPaste | Win32Output | |
| SetTitle | Vt100Output | |
| ClearTitle | Vt100Output | |
| Bell | Vt100Output | |
| AskForCpr | Vt100Output | |
| ResetCursorKeyMode | Vt100Output | |
| RespondsToCpr | **Constant** | Returns false |
| GetSize | Win32Output | |
| Fileno | Vt100Output | |
| Encoding | Vt100Output | |
| Stdout | Vt100Output | |
| GetDefaultColorDepth | **Custom** | Returns TrueColor by default |
| ScrollBufferToPrompt | Win32Output | |
| GetRowsBelowCursorPosition | Win32Output | |

## Thread Safety

- **Lock Type**: `System.Threading.Lock` (.NET 9+)
- **Scope**: Per-instance
- **Protected Operations**: `Flush()` only (console mode save/set/restore sequence)
- **Rationale**: Prevents interleaved enable/restore sequences when multiple threads call Flush on the same instance

## Error Handling

| Scenario | Behavior |
|----------|----------|
| stdout is null | `ArgumentNullException` |
| Non-Windows platform | `PlatformNotSupportedException` |
| No console buffer | `NoConsoleScreenBufferError` (from Win32Output constructor) |
| GetConsoleMode fails in Flush | VT100 mode not enabled, flush still occurs |
| SetConsoleMode fails in Flush | Original mode restoration still attempted in finally |
