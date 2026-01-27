using Stroke.Core.Primitives;
using Stroke.CursorShapes;
using Stroke.Styles;

namespace Stroke.Output;

/// <summary>
/// Plain text output for redirected streams (files, pipes).
/// </summary>
/// <remarks>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>PlainTextOutput</c> class
/// from <c>prompt_toolkit.output.plain_text</c>.
/// </para>
/// <para>
/// This class writes plain text without any escape sequences, suitable for
/// output that will be redirected to a file or pipe. Cursor movement is
/// simulated using spaces and newlines.
/// </para>
/// <para>
/// This class is thread-safe. All mutable state is protected by synchronization.
/// Individual method calls are atomic; callers are responsible for synchronizing
/// compound operations.
/// </para>
/// </remarks>
public sealed class PlainTextOutput : IOutput
{
    private readonly TextWriter _stdout;
    private readonly Lock _lock = new();
    private readonly List<string> _buffer = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="PlainTextOutput"/> class.
    /// </summary>
    /// <param name="stdout">The output stream.</param>
    public PlainTextOutput(TextWriter stdout)
    {
        _stdout = stdout ?? throw new ArgumentNullException(nameof(stdout));
    }

    #region Writing

    /// <inheritdoc/>
    public void Write(string data)
    {
        ArgumentNullException.ThrowIfNull(data);

        using (_lock.EnterScope())
        {
            _buffer.Add(data);
        }
    }

    /// <inheritdoc/>
    public void WriteRaw(string data)
    {
        // For plain text output, WriteRaw behaves the same as Write
        // (no escape sequences are interpreted)
        ArgumentNullException.ThrowIfNull(data);

        using (_lock.EnterScope())
        {
            _buffer.Add(data);
        }
    }

    /// <inheritdoc/>
    public void Flush()
    {
        using (_lock.EnterScope())
        {
            if (_buffer.Count == 0)
            {
                return;
            }

            var output = string.Concat(_buffer);
            _buffer.Clear();

            try
            {
                _stdout.Write(output);
                _stdout.Flush();
            }
            catch (IOException)
            {
                // Resilient to I/O exceptions - log and continue
            }
        }
    }

    #endregion

    #region Screen Control

    /// <inheritdoc/>
    public void EraseScreen()
    {
        // No-op for plain text
    }

    /// <inheritdoc/>
    public void EraseEndOfLine()
    {
        // No-op for plain text
    }

    /// <inheritdoc/>
    public void EraseDown()
    {
        // No-op for plain text
    }

    /// <inheritdoc/>
    public void EnterAlternateScreen()
    {
        // No-op for plain text
    }

    /// <inheritdoc/>
    public void QuitAlternateScreen()
    {
        // No-op for plain text
    }

    #endregion

    #region Cursor Movement

    /// <inheritdoc/>
    public void CursorGoto(int row, int column)
    {
        // No-op for plain text (can't position cursor in file)
    }

    /// <inheritdoc/>
    public void CursorUp(int amount)
    {
        // No-op for plain text (can't move cursor up in file)
    }

    /// <inheritdoc/>
    public void CursorDown(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        // Simulate cursor down with newlines
        using (_lock.EnterScope())
        {
            _buffer.Add(new string('\n', amount));
        }
    }

    /// <inheritdoc/>
    public void CursorForward(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        // Simulate cursor forward with spaces
        using (_lock.EnterScope())
        {
            _buffer.Add(new string(' ', amount));
        }
    }

    /// <inheritdoc/>
    public void CursorBackward(int amount)
    {
        // No-op for plain text (can't move cursor backward in file)
    }

    #endregion

    #region Cursor Visibility

    /// <inheritdoc/>
    public void HideCursor()
    {
        // No-op for plain text
    }

    /// <inheritdoc/>
    public void ShowCursor()
    {
        // No-op for plain text
    }

    /// <inheritdoc/>
    public void SetCursorShape(CursorShape shape)
    {
        // No-op for plain text
    }

    /// <inheritdoc/>
    public void ResetCursorShape()
    {
        // No-op for plain text
    }

    #endregion

    #region Attributes

    /// <inheritdoc/>
    public void SetAttributes(Attrs attrs, ColorDepth colorDepth)
    {
        // No-op for plain text (no colors/styles in plain text)
    }

    /// <inheritdoc/>
    public void ResetAttributes()
    {
        // No-op for plain text
    }

    #endregion

    #region Mouse

    /// <inheritdoc/>
    public void EnableMouseSupport()
    {
        // No-op for plain text
    }

    /// <inheritdoc/>
    public void DisableMouseSupport()
    {
        // No-op for plain text
    }

    #endregion

    #region Bracketed Paste

    /// <inheritdoc/>
    public void EnableBracketedPaste()
    {
        // No-op for plain text
    }

    /// <inheritdoc/>
    public void DisableBracketedPaste()
    {
        // No-op for plain text
    }

    #endregion

    #region Title

    /// <inheritdoc/>
    public void SetTitle(string title)
    {
        // No-op for plain text
    }

    /// <inheritdoc/>
    public void ClearTitle()
    {
        // No-op for plain text
    }

    #endregion

    #region Bell

    /// <inheritdoc/>
    public void Bell()
    {
        // No-op for plain text
    }

    #endregion

    #region Autowrap

    /// <inheritdoc/>
    public void DisableAutowrap()
    {
        // No-op for plain text
    }

    /// <inheritdoc/>
    public void EnableAutowrap()
    {
        // No-op for plain text
    }

    #endregion

    #region Cursor Position Reporting

    /// <inheritdoc/>
    public void AskForCpr()
    {
        // No-op for plain text
    }

    /// <inheritdoc/>
    public bool RespondsToCpr => false;

    /// <inheritdoc/>
    public void ResetCursorKeyMode()
    {
        // No-op for plain text
    }

    #endregion

    #region Terminal Information

    /// <inheritdoc/>
    public Size GetSize() => new(40, 80);

    /// <inheritdoc/>
    public int Fileno()
    {
        throw new NotImplementedException("PlainTextOutput does not expose file descriptor.");
    }

    /// <inheritdoc/>
    public string Encoding => "utf-8";

    /// <inheritdoc/>
    public TextWriter? Stdout => _stdout;

    /// <inheritdoc/>
    public ColorDepth GetDefaultColorDepth() => ColorDepth.Depth1Bit;

    #endregion

    #region Windows-Specific

    /// <inheritdoc/>
    public void ScrollBufferToPrompt()
    {
        // No-op for plain text
    }

    /// <inheritdoc/>
    public int GetRowsBelowCursorPosition() => 0;

    #endregion
}
