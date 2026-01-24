namespace Stroke.Core;

/// <summary>
/// Type of selection.
/// </summary>
public enum SelectionType
{
    /// <summary>Character selection (Visual in Vi).</summary>
    Characters,

    /// <summary>Whole line selection (Visual-Line in Vi).</summary>
    Lines,

    /// <summary>Block/rectangular selection (Visual-Block in Vi).</summary>
    Block
}
