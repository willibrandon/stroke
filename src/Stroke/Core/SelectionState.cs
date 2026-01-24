namespace Stroke.Core;

/// <summary>
/// State of the current selection.
/// </summary>
public sealed class SelectionState
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SelectionState"/> class.
    /// </summary>
    /// <param name="originalCursorPosition">Starting position when selection began.</param>
    /// <param name="type">Type of selection.</param>
    public SelectionState(
        int originalCursorPosition = 0,
        SelectionType type = SelectionType.Characters)
    {
        OriginalCursorPosition = originalCursorPosition;
        Type = type;
        ShiftMode = false;
    }

    /// <summary>
    /// Gets the starting position when selection began.
    /// </summary>
    public int OriginalCursorPosition { get; }

    /// <summary>
    /// Gets the type of selection.
    /// </summary>
    public SelectionType Type { get; }

    /// <summary>
    /// Gets a value indicating whether shift key initiated selection.
    /// </summary>
    public bool ShiftMode { get; private set; }

    /// <summary>
    /// Enter shift selection mode.
    /// </summary>
    public void EnterShiftMode() => ShiftMode = true;
}
