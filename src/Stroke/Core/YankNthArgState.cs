namespace Stroke.Core;

/// <summary>
/// Tracks state for yank-nth-arg and yank-last-arg Emacs operations.
/// </summary>
public sealed class YankNthArgState
{
    /// <summary>
    /// Initializes a new instance of the <see cref="YankNthArgState"/> class.
    /// </summary>
    /// <param name="historyPosition">Position in history (negative index).</param>
    /// <param name="n">Argument index to yank (-1 for last argument).</param>
    /// <param name="previousInsertedWord">Previously inserted word.</param>
    public YankNthArgState(int historyPosition = 0, int n = -1, string previousInsertedWord = "")
    {
        HistoryPosition = historyPosition;
        N = n;
        PreviousInsertedWord = previousInsertedWord;
    }

    /// <summary>
    /// Gets or sets the position in history (negative index).
    /// </summary>
    public int HistoryPosition { get; set; }

    /// <summary>
    /// Gets or sets the argument index to yank.
    /// </summary>
    /// <remarks>
    /// -1 means last argument, 0 means command name, 1 means first argument, etc.
    /// </remarks>
    public int N { get; set; }

    /// <summary>
    /// Gets or sets the previously inserted word.
    /// </summary>
    public string PreviousInsertedWord { get; set; }
}
