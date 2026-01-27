using Stroke.Core.Primitives;
using Stroke.CursorShapes;
using Stroke.Styles;

namespace Stroke.Output;

/// <summary>
/// No-op output implementation for testing and headless scenarios.
/// </summary>
/// <remarks>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>DummyOutput</c> class
/// from <c>prompt_toolkit.output.defaults</c>.
/// </para>
/// <para>
/// All methods are no-ops. This class is useful for unit testing code that
/// depends on <see cref="IOutput"/> without requiring a real terminal.
/// </para>
/// <para>
/// This class is inherently thread-safe as it maintains no mutable state.
/// </para>
/// </remarks>
public sealed class DummyOutput : IOutput
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DummyOutput"/> class.
    /// </summary>
    public DummyOutput()
    {
    }

    #region Writing

    /// <inheritdoc/>
    public void Write(string data)
    {
        // No-op
    }

    /// <inheritdoc/>
    public void WriteRaw(string data)
    {
        // No-op
    }

    /// <inheritdoc/>
    public void Flush()
    {
        // No-op
    }

    #endregion

    #region Screen Control

    /// <inheritdoc/>
    public void EraseScreen()
    {
        // No-op
    }

    /// <inheritdoc/>
    public void EraseEndOfLine()
    {
        // No-op
    }

    /// <inheritdoc/>
    public void EraseDown()
    {
        // No-op
    }

    /// <inheritdoc/>
    public void EnterAlternateScreen()
    {
        // No-op
    }

    /// <inheritdoc/>
    public void QuitAlternateScreen()
    {
        // No-op
    }

    #endregion

    #region Cursor Movement

    /// <inheritdoc/>
    public void CursorGoto(int row, int column)
    {
        // No-op
    }

    /// <inheritdoc/>
    public void CursorUp(int amount)
    {
        // No-op
    }

    /// <inheritdoc/>
    public void CursorDown(int amount)
    {
        // No-op
    }

    /// <inheritdoc/>
    public void CursorForward(int amount)
    {
        // No-op
    }

    /// <inheritdoc/>
    public void CursorBackward(int amount)
    {
        // No-op
    }

    #endregion

    #region Cursor Visibility

    /// <inheritdoc/>
    public void HideCursor()
    {
        // No-op
    }

    /// <inheritdoc/>
    public void ShowCursor()
    {
        // No-op
    }

    /// <inheritdoc/>
    public void SetCursorShape(CursorShape shape)
    {
        // No-op
    }

    /// <inheritdoc/>
    public void ResetCursorShape()
    {
        // No-op
    }

    #endregion

    #region Attributes

    /// <inheritdoc/>
    public void SetAttributes(Attrs attrs, ColorDepth colorDepth)
    {
        // No-op
    }

    /// <inheritdoc/>
    public void ResetAttributes()
    {
        // No-op
    }

    #endregion

    #region Mouse

    /// <inheritdoc/>
    public void EnableMouseSupport()
    {
        // No-op
    }

    /// <inheritdoc/>
    public void DisableMouseSupport()
    {
        // No-op
    }

    #endregion

    #region Bracketed Paste

    /// <inheritdoc/>
    public void EnableBracketedPaste()
    {
        // No-op
    }

    /// <inheritdoc/>
    public void DisableBracketedPaste()
    {
        // No-op
    }

    #endregion

    #region Title

    /// <inheritdoc/>
    public void SetTitle(string title)
    {
        // No-op
    }

    /// <inheritdoc/>
    public void ClearTitle()
    {
        // No-op
    }

    #endregion

    #region Bell

    /// <inheritdoc/>
    public void Bell()
    {
        // No-op
    }

    #endregion

    #region Autowrap

    /// <inheritdoc/>
    public void DisableAutowrap()
    {
        // No-op
    }

    /// <inheritdoc/>
    public void EnableAutowrap()
    {
        // No-op
    }

    #endregion

    #region Cursor Position Reporting

    /// <inheritdoc/>
    public void AskForCpr()
    {
        // No-op
    }

    /// <inheritdoc/>
    public bool RespondsToCpr => false;

    /// <inheritdoc/>
    public void ResetCursorKeyMode()
    {
        // No-op
    }

    #endregion

    #region Terminal Information

    /// <inheritdoc/>
    public Size GetSize() => new(40, 80);

    /// <inheritdoc/>
    public int Fileno()
    {
        throw new NotImplementedException("DummyOutput does not have a file descriptor.");
    }

    /// <inheritdoc/>
    public string Encoding => "utf-8";

    /// <inheritdoc/>
    public TextWriter? Stdout => null;

    /// <inheritdoc/>
    public ColorDepth GetDefaultColorDepth() => ColorDepth.Depth1Bit;

    #endregion

    #region Windows-Specific

    /// <inheritdoc/>
    public void ScrollBufferToPrompt()
    {
        // No-op
    }

    /// <inheritdoc/>
    public int GetRowsBelowCursorPosition() => 0;

    #endregion
}
