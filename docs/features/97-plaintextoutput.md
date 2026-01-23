# Feature 97: Plain Text Output

## Overview

Implement PlainTextOutput - an output class that strips all ANSI escape sequences, useful when stdout is redirected to a file or when the terminal doesn't support formatting.

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/output/plain_text.py`

## Public API

### PlainTextOutput

```csharp
namespace Stroke.Output;

/// <summary>
/// Output that won't include any ANSI escape sequences.
/// Useful when stdout is not a terminal (e.g., redirected to a file).
/// When print_formatted_text is used, no formatting will be included.
/// </summary>
public sealed class PlainTextOutput : IOutput
{
    /// <summary>
    /// Create a plain text output.
    /// </summary>
    /// <param name="stdout">The underlying text writer.</param>
    public PlainTextOutput(TextWriter stdout);

    /// <summary>
    /// The underlying stdout.
    /// </summary>
    public TextWriter Stdout { get; }

    /// <inheritdoc/>
    public int FileNo();

    /// <inheritdoc/>
    public string Encoding { get; }

    /// <inheritdoc/>
    public void Write(string data);

    /// <inheritdoc/>
    public void WriteRaw(string data);

    /// <inheritdoc/>
    public void SetTitle(string title);

    /// <inheritdoc/>
    public void ClearTitle();

    /// <inheritdoc/>
    public void Flush();

    /// <inheritdoc/>
    public void EraseScreen();

    /// <inheritdoc/>
    public void EnterAlternateScreen();

    /// <inheritdoc/>
    public void QuitAlternateScreen();

    /// <inheritdoc/>
    public void EnableMouseSupport();

    /// <inheritdoc/>
    public void DisableMouseSupport();

    /// <inheritdoc/>
    public void EraseEndOfLine();

    /// <inheritdoc/>
    public void EraseDown();

    /// <inheritdoc/>
    public void ResetAttributes();

    /// <inheritdoc/>
    public void SetAttributes(Attrs attrs, ColorDepth colorDepth);

    /// <inheritdoc/>
    public void DisableAutowrap();

    /// <inheritdoc/>
    public void EnableAutowrap();

    /// <inheritdoc/>
    public void CursorGoto(int row = 0, int column = 0);

    /// <inheritdoc/>
    public void CursorUp(int amount);

    /// <inheritdoc/>
    public void CursorDown(int amount);

    /// <inheritdoc/>
    public void CursorForward(int amount);

    /// <inheritdoc/>
    public void CursorBackward(int amount);

    /// <inheritdoc/>
    public void HideCursor();

    /// <inheritdoc/>
    public void ShowCursor();

    /// <inheritdoc/>
    public void SetCursorShape(CursorShape cursorShape);

    /// <inheritdoc/>
    public void ResetCursorShape();

    /// <inheritdoc/>
    public void AskForCpr();

    /// <inheritdoc/>
    public void Bell();

    /// <inheritdoc/>
    public void EnableBracketedPaste();

    /// <inheritdoc/>
    public void DisableBracketedPaste();

    /// <inheritdoc/>
    public void ScrollBufferToPrompt();

    /// <inheritdoc/>
    public Size GetSize();

    /// <inheritdoc/>
    public int GetRowsBelowCursorPosition();

    /// <inheritdoc/>
    public ColorDepth GetDefaultColorDepth();
}
```

## Project Structure

```
src/Stroke/
└── Output/
    └── PlainTextOutput.cs
tests/Stroke.Tests/
└── Output/
    └── PlainTextOutputTests.cs
```

## Implementation Notes

### PlainTextOutput Implementation

```csharp
public sealed class PlainTextOutput : IOutput
{
    private readonly List<string> _buffer = new();

    public PlainTextOutput(TextWriter stdout)
    {
        Stdout = stdout ?? throw new ArgumentNullException(nameof(stdout));
    }

    public TextWriter Stdout { get; }

    public int FileNo()
    {
        if (Stdout is StreamWriter sw && sw.BaseStream is FileStream fs)
            return (int)fs.SafeFileHandle.DangerousGetHandle();
        throw new NotSupportedException("No sensible default for FileNo()");
    }

    public string Encoding => "utf-8";

    public void Write(string data)
    {
        _buffer.Add(data);
    }

    public void WriteRaw(string data)
    {
        _buffer.Add(data);
    }

    public void Flush()
    {
        if (_buffer.Count == 0)
            return;

        var data = string.Concat(_buffer);
        _buffer.Clear();
        FlushStdout.Flush(Stdout, data);
    }

    // Terminal control operations are no-ops
    public void SetTitle(string title) { }
    public void ClearTitle() { }
    public void EraseScreen() { }
    public void EnterAlternateScreen() { }
    public void QuitAlternateScreen() { }
    public void EnableMouseSupport() { }
    public void DisableMouseSupport() { }
    public void EraseEndOfLine() { }
    public void EraseDown() { }
    public void ResetAttributes() { }
    public void SetAttributes(Attrs attrs, ColorDepth colorDepth) { }
    public void DisableAutowrap() { }
    public void EnableAutowrap() { }
    public void CursorGoto(int row = 0, int column = 0) { }
    public void CursorUp(int amount) { }
    public void HideCursor() { }
    public void ShowCursor() { }
    public void SetCursorShape(CursorShape cursorShape) { }
    public void ResetCursorShape() { }
    public void AskForCpr() { }
    public void Bell() { }
    public void EnableBracketedPaste() { }
    public void DisableBracketedPaste() { }
    public void ScrollBufferToPrompt() { }

    // Cursor movements that have plain text equivalents
    public void CursorDown(int amount)
    {
        _buffer.Add("\n");
    }

    public void CursorForward(int amount)
    {
        _buffer.Add(new string(' ', amount));
    }

    public void CursorBackward(int amount) { }

    // Default size for plain text output
    public Size GetSize() => new(rows: 40, columns: 80);

    public int GetRowsBelowCursorPosition() => 8;

    public ColorDepth GetDefaultColorDepth() => ColorDepth.Depth1Bit;
}
```

### FlushStdout Helper

```csharp
namespace Stroke.Output;

/// <summary>
/// Helper for safely flushing stdout with proper blocking I/O handling.
/// </summary>
public static class FlushStdout
{
    /// <summary>
    /// Flush data to stdout, handling blocking I/O and interrupts.
    /// </summary>
    /// <param name="stdout">The text writer.</param>
    /// <param name="data">Data to write.</param>
    public static void Flush(TextWriter stdout, string data)
    {
        try
        {
            // Write with proper encoding, replacing unsupported characters
            stdout.Write(data);
            stdout.Flush();
        }
        catch (IOException e) when (e.HResult == EINTR)
        {
            // Interrupted system call (e.g., window resize signal)
            // Just ignore - resize handler will render again
        }
        catch (IOException e) when (e.HResult == 0)
        {
            // Can happen with lots of output and Ctrl+C
            // Just ignore
        }
    }

    private const int EINTR = 4;
}
```

### Usage Examples

```csharp
// Detect if stdout is a terminal and choose appropriate output
IOutput output;
if (Console.IsOutputRedirected)
{
    output = new PlainTextOutput(Console.Out);
}
else
{
    output = new Vt100Output(Console.Out, () => GetConsoleSize());
}

// Use with formatted text output
PrintFormattedText(output, new FormattedText([
    ("bold red", "Error: "),
    ("", "Something went wrong")
]), style);

// With PlainTextOutput, this outputs: "Error: Something went wrong"
// (no formatting codes)
```

## Dependencies

- Feature 15: Output abstraction (IOutput)
- Feature 52: Color depth

## Implementation Tasks

1. Implement PlainTextOutput class
2. Implement buffered write methods
3. Implement no-op terminal control methods
4. Implement cursor movement with spaces/newlines
5. Implement FlushStdout helper
6. Handle I/O interrupts gracefully
7. Write unit tests

## Acceptance Criteria

- [ ] Write and WriteRaw buffer text
- [ ] Flush outputs buffered text
- [ ] Terminal control methods are no-ops
- [ ] CursorDown outputs newline
- [ ] CursorForward outputs spaces
- [ ] GetSize returns default 80x40
- [ ] GetDefaultColorDepth returns 1-bit
- [ ] I/O interrupts handled gracefully
- [ ] Unit tests achieve 80% coverage
