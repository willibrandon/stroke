# Contract: IOutput Interface

**Feature**: Output System
**Date**: 2026-01-27
**Namespace**: `Stroke.Output`

## Interface Definition

```csharp
namespace Stroke.Output;

using Stroke.Core.Primitives;
using Stroke.CursorShapes;
using Stroke.Styles;

/// <summary>
/// Contract defining all terminal output operations.
/// </summary>
/// <remarks>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>Output</c> abstract base class
/// from <c>prompt_toolkit.output.base</c>.
/// </para>
/// <para>
/// Implementations include:
/// <list type="bullet">
///   <item><see cref="Vt100Output"/> - VT100/ANSI terminal output</item>
///   <item><see cref="PlainTextOutput"/> - Non-escape output for redirected streams</item>
///   <item><see cref="DummyOutput"/> - No-op implementation for testing</item>
/// </list>
/// </para>
/// </remarks>
public interface IOutput
{
    #region Writing

    /// <summary>
    /// Write text to output with VT100 escape sequences escaped.
    /// </summary>
    /// <param name="data">The text to write.</param>
    /// <remarks>
    /// Replaces <c>\x1b</c> (ESC) characters with <c>?</c> to prevent
    /// user-supplied text from containing terminal escape sequences.
    /// </remarks>
    void Write(string data);

    /// <summary>
    /// Write raw data to output without modification.
    /// </summary>
    /// <param name="data">The raw data to write.</param>
    /// <remarks>
    /// Use this method for legitimate escape sequences that should
    /// be passed through verbatim.
    /// </remarks>
    void WriteRaw(string data);

    /// <summary>
    /// Flush the output buffer to the underlying stream.
    /// </summary>
    void Flush();

    #endregion

    #region Screen Control

    /// <summary>
    /// Erase the entire screen and move cursor to home position.
    /// </summary>
    /// <remarks>
    /// VT100: <c>\x1b[2J</c>
    /// </remarks>
    void EraseScreen();

    /// <summary>
    /// Erase from the current cursor position to the end of the current line.
    /// </summary>
    /// <remarks>
    /// VT100: <c>\x1b[K</c>
    /// </remarks>
    void EraseEndOfLine();

    /// <summary>
    /// Erase the screen from the current line down to the bottom.
    /// </summary>
    /// <remarks>
    /// VT100: <c>\x1b[J</c>
    /// </remarks>
    void EraseDown();

    /// <summary>
    /// Enter the alternate screen buffer.
    /// </summary>
    /// <remarks>
    /// Used for full-screen applications. The main screen is preserved
    /// and restored when exiting.
    /// VT100: <c>\x1b[?1049h\x1b[H</c>
    /// </remarks>
    void EnterAlternateScreen();

    /// <summary>
    /// Exit the alternate screen buffer.
    /// </summary>
    /// <remarks>
    /// VT100: <c>\x1b[?1049l</c>
    /// </remarks>
    void QuitAlternateScreen();

    #endregion

    #region Cursor Movement

    /// <summary>
    /// Move the cursor to the specified position.
    /// </summary>
    /// <param name="row">Row (1-based).</param>
    /// <param name="column">Column (1-based).</param>
    /// <remarks>
    /// VT100: <c>\x1b[{row};{col}H</c>
    /// </remarks>
    void CursorGoto(int row = 0, int column = 0);

    /// <summary>
    /// Move the cursor up by the specified amount.
    /// </summary>
    /// <param name="amount">Number of rows to move up.</param>
    /// <remarks>
    /// VT100: <c>\x1b[{n}A</c> or <c>\x1b[A</c> for n=1
    /// </remarks>
    void CursorUp(int amount);

    /// <summary>
    /// Move the cursor down by the specified amount.
    /// </summary>
    /// <param name="amount">Number of rows to move down.</param>
    /// <remarks>
    /// VT100: <c>\x1b[{n}B</c> or <c>\x1b[B</c> for n=1
    /// </remarks>
    void CursorDown(int amount);

    /// <summary>
    /// Move the cursor forward (right) by the specified amount.
    /// </summary>
    /// <param name="amount">Number of columns to move right.</param>
    /// <remarks>
    /// VT100: <c>\x1b[{n}C</c> or <c>\x1b[C</c> for n=1
    /// </remarks>
    void CursorForward(int amount);

    /// <summary>
    /// Move the cursor backward (left) by the specified amount.
    /// </summary>
    /// <param name="amount">Number of columns to move left.</param>
    /// <remarks>
    /// VT100: <c>\x1b[{n}D</c> or <c>\b</c> for n=1
    /// </remarks>
    void CursorBackward(int amount);

    #endregion

    #region Cursor Visibility

    /// <summary>
    /// Hide the cursor.
    /// </summary>
    /// <remarks>
    /// VT100: <c>\x1b[?25l</c>
    /// </remarks>
    void HideCursor();

    /// <summary>
    /// Show the cursor.
    /// </summary>
    /// <remarks>
    /// VT100: <c>\x1b[?12l\x1b[?25h</c> (stop blinking and show)
    /// </remarks>
    void ShowCursor();

    /// <summary>
    /// Set the cursor shape.
    /// </summary>
    /// <param name="cursorShape">The desired cursor shape.</param>
    /// <remarks>
    /// Uses DECSCUSR escape sequences.
    /// </remarks>
    void SetCursorShape(CursorShape cursorShape);

    /// <summary>
    /// Reset the cursor shape to default.
    /// </summary>
    /// <remarks>
    /// Only sends escape sequence if cursor shape was previously changed.
    /// VT100: <c>\x1b[0 q</c>
    /// </remarks>
    void ResetCursorShape();

    #endregion

    #region Attributes

    /// <summary>
    /// Reset all text attributes to default.
    /// </summary>
    /// <remarks>
    /// VT100: <c>\x1b[0m</c>
    /// </remarks>
    void ResetAttributes();

    /// <summary>
    /// Set text attributes (colors and styles).
    /// </summary>
    /// <param name="attrs">The style attributes to apply.</param>
    /// <param name="colorDepth">The color depth to use for rendering.</param>
    void SetAttributes(Attrs attrs, ColorDepth colorDepth);

    /// <summary>
    /// Disable automatic line wrapping.
    /// </summary>
    /// <remarks>
    /// VT100: <c>\x1b[?7l</c>
    /// </remarks>
    void DisableAutowrap();

    /// <summary>
    /// Enable automatic line wrapping.
    /// </summary>
    /// <remarks>
    /// VT100: <c>\x1b[?7h</c>
    /// </remarks>
    void EnableAutowrap();

    #endregion

    #region Terminal Features

    /// <summary>
    /// Enable mouse tracking.
    /// </summary>
    /// <remarks>
    /// Enables basic mouse, drag, urxvt, and SGR mouse modes.
    /// </remarks>
    void EnableMouseSupport();

    /// <summary>
    /// Disable mouse tracking.
    /// </summary>
    void DisableMouseSupport();

    /// <summary>
    /// Enable bracketed paste mode.
    /// </summary>
    /// <remarks>
    /// VT100: <c>\x1b[?2004h</c>
    /// </remarks>
    void EnableBracketedPaste();

    /// <summary>
    /// Disable bracketed paste mode.
    /// </summary>
    /// <remarks>
    /// VT100: <c>\x1b[?2004l</c>
    /// </remarks>
    void DisableBracketedPaste();

    /// <summary>
    /// Set the terminal window title.
    /// </summary>
    /// <param name="title">The title text.</param>
    /// <remarks>
    /// ESC and BEL characters are stripped from the title.
    /// VT100: <c>\x1b]2;{title}\x07</c>
    /// </remarks>
    void SetTitle(string title);

    /// <summary>
    /// Clear the terminal window title.
    /// </summary>
    void ClearTitle();

    /// <summary>
    /// Sound the terminal bell.
    /// </summary>
    /// <remarks>
    /// VT100: <c>\a</c> (BEL character)
    /// </remarks>
    void Bell();

    /// <summary>
    /// Reset cursor key mode (put terminal in cursor mode instead of application mode).
    /// </summary>
    /// <remarks>
    /// VT100: <c>\x1b[?1l</c>
    /// </remarks>
    void ResetCursorKeyMode();

    #endregion

    #region Cursor Position Report

    /// <summary>
    /// Request a cursor position report (CPR).
    /// </summary>
    /// <remarks>
    /// VT100: <c>\x1b[6n</c>
    /// The response comes through the input stream as <c>\x1b[{row};{col}R</c>.
    /// </remarks>
    void AskForCpr();

    /// <summary>
    /// Gets whether this output responds to cursor position report requests.
    /// </summary>
    /// <remarks>
    /// Returns <c>false</c> if CPR is disabled or the output is not a TTY.
    /// </remarks>
    bool RespondsToCpr { get; }

    #endregion

    #region Terminal Information

    /// <summary>
    /// Get the size of the terminal window.
    /// </summary>
    /// <returns>The terminal size in rows and columns.</returns>
    Size GetSize();

    /// <summary>
    /// Get the file descriptor for this output.
    /// </summary>
    /// <returns>The file descriptor number.</returns>
    /// <exception cref="NotImplementedException">
    /// Thrown by implementations that don't have a file descriptor.
    /// </exception>
    int Fileno();

    /// <summary>
    /// Gets the encoding used for output.
    /// </summary>
    /// <remarks>
    /// Typically returns "utf-8".
    /// </remarks>
    string Encoding { get; }

    /// <summary>
    /// Get the default color depth for this output.
    /// </summary>
    /// <returns>The default color depth.</returns>
    ColorDepth GetDefaultColorDepth();

    #endregion

    #region Windows-Specific (Optional)

    /// <summary>
    /// Scroll the buffer to bring the prompt into view.
    /// </summary>
    /// <remarks>
    /// For Windows console only. No-op on other platforms.
    /// </remarks>
    void ScrollBufferToPrompt();

    /// <summary>
    /// Get the number of rows available below the cursor position.
    /// </summary>
    /// <returns>The number of rows below the cursor.</returns>
    /// <remarks>
    /// For Windows console only. Throws <see cref="NotImplementedException"/> on other platforms.
    /// </remarks>
    int GetRowsBelowCursorPosition();

    #endregion
}
```

## Method Contracts

### Write

**Preconditions**:
- `data` is not null

**Postconditions**:
- Data is added to the output buffer with `\x1b` replaced by `?`
- Buffer is not flushed until `Flush()` is called

**Side Effects**:
- Modifies internal buffer

---

### WriteRaw

**Preconditions**:
- `data` is not null

**Postconditions**:
- Data is added to the output buffer without modification

**Side Effects**:
- Modifies internal buffer

---

### Flush

**Preconditions**:
- None

**Postconditions**:
- If buffer is empty, no I/O operation occurs
- If buffer is non-empty, all buffered data is written to stdout and buffer is cleared

**Side Effects**:
- Writes to stdout
- Clears internal buffer

---

### CursorUp / CursorDown / CursorForward / CursorBackward

**Preconditions**:
- None (amount can be any integer)

**Postconditions**:
- If `amount == 0`, no escape sequence is written
- If `amount == 1`, optimized single-character sequence is written
- If `amount > 1`, parameterized sequence is written

---

### HideCursor / ShowCursor

**Preconditions**:
- None

**Postconditions**:
- Escape sequence is only written if cursor visibility state changes
- Internal visibility state is updated

**State Tracking**:
- `null` (unknown) → `false` (hidden): writes sequence
- `null` (unknown) → `true` (visible): writes sequence
- `false` → `false`: no sequence written
- `true` → `true`: no sequence written
- `true` → `false`: writes sequence
- `false` → `true`: writes sequence

---

### SetCursorShape

**Preconditions**:
- `cursorShape` is a valid `CursorShape` value

**Postconditions**:
- If `cursorShape == NeverChange`, no sequence is written
- Otherwise, appropriate DECSCUSR sequence is written
- `_cursorShapeChanged` flag is set to `true`

---

### ResetCursorShape

**Preconditions**:
- None

**Postconditions**:
- If `_cursorShapeChanged` is `false`, no sequence is written
- If `_cursorShapeChanged` is `true`, reset sequence is written and flag is cleared

---

### SetAttributes

**Preconditions**:
- `attrs` is a valid `Attrs` struct
- `colorDepth` is a valid `ColorDepth` value

**Postconditions**:
- Escape sequence for the given attrs is retrieved from cache (or generated and cached)
- Escape sequence is written to buffer

**Caching**:
- One `EscapeCodeCache` instance per `ColorDepth` value
- Cache key is the `Attrs` struct
- Cache value is the computed escape sequence string

---

### SetTitle

**Preconditions**:
- `title` is not null

**Postconditions**:
- If terminal is "linux" or "eterm-color", no sequence is written (not supported)
- Otherwise, OSC sequence is written with ESC and BEL characters stripped from title

---

### GetSize

**Preconditions**:
- None

**Postconditions**:
- Returns current terminal size
- If size cannot be determined, returns default (24 rows, 80 columns)
- If terminal reports 0x0, returns default

**Error Handling**:
- OSError during size query returns default size

---

### GetDefaultColorDepth

**Preconditions**:
- None

**Postconditions**:
- If `_defaultColorDepth` is set, returns it
- If `TERM` is null, returns `Default` (Depth8Bit)
- If `TERM` is "dumb", returns `Depth1Bit`
- If `TERM` is "linux" or "eterm-color", returns `Depth4Bit`
- Otherwise, returns `Default` (Depth8Bit)

---

## Interface Member Summary

**Total Members**: 37 (35 methods + 2 properties)

### Methods (35)

| # | Method | Return Type | Category |
|---|--------|-------------|----------|
| 1 | `Write(string data)` | `void` | Writing |
| 2 | `WriteRaw(string data)` | `void` | Writing |
| 3 | `Flush()` | `void` | Writing |
| 4 | `EraseScreen()` | `void` | Screen Control |
| 5 | `EraseEndOfLine()` | `void` | Screen Control |
| 6 | `EraseDown()` | `void` | Screen Control |
| 7 | `EnterAlternateScreen()` | `void` | Screen Control |
| 8 | `QuitAlternateScreen()` | `void` | Screen Control |
| 9 | `CursorGoto(int row, int column)` | `void` | Cursor Movement |
| 10 | `CursorUp(int amount)` | `void` | Cursor Movement |
| 11 | `CursorDown(int amount)` | `void` | Cursor Movement |
| 12 | `CursorForward(int amount)` | `void` | Cursor Movement |
| 13 | `CursorBackward(int amount)` | `void` | Cursor Movement |
| 14 | `HideCursor()` | `void` | Cursor Visibility |
| 15 | `ShowCursor()` | `void` | Cursor Visibility |
| 16 | `SetCursorShape(CursorShape shape)` | `void` | Cursor Visibility |
| 17 | `ResetCursorShape()` | `void` | Cursor Visibility |
| 18 | `ResetAttributes()` | `void` | Attributes |
| 19 | `SetAttributes(Attrs attrs, ColorDepth depth)` | `void` | Attributes |
| 20 | `DisableAutowrap()` | `void` | Attributes |
| 21 | `EnableAutowrap()` | `void` | Attributes |
| 22 | `EnableMouseSupport()` | `void` | Terminal Features |
| 23 | `DisableMouseSupport()` | `void` | Terminal Features |
| 24 | `EnableBracketedPaste()` | `void` | Terminal Features |
| 25 | `DisableBracketedPaste()` | `void` | Terminal Features |
| 26 | `SetTitle(string title)` | `void` | Terminal Features |
| 27 | `ClearTitle()` | `void` | Terminal Features |
| 28 | `Bell()` | `void` | Terminal Features |
| 29 | `ResetCursorKeyMode()` | `void` | Terminal Features |
| 30 | `AskForCpr()` | `void` | Cursor Position Report |
| 31 | `GetSize()` | `Size` | Terminal Information |
| 32 | `Fileno()` | `int` | Terminal Information |
| 33 | `GetDefaultColorDepth()` | `ColorDepth` | Terminal Information |
| 34 | `ScrollBufferToPrompt()` | `void` | Windows-Specific |
| 35 | `GetRowsBelowCursorPosition()` | `int` | Windows-Specific |

### Properties (2)

| # | Property | Type | Category |
|---|----------|------|----------|
| 1 | `RespondsToCpr` | `bool` | Cursor Position Report |
| 2 | `Encoding` | `string` | Terminal Information |
