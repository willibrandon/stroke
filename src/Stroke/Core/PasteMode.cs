namespace Stroke.Core;

/// <summary>
/// Mode for paste operations.
/// </summary>
public enum PasteMode
{
    /// <summary>Yank like Emacs (at cursor position).</summary>
    Emacs,

    /// <summary>Vi paste after cursor ('p').</summary>
    ViAfter,

    /// <summary>Vi paste before cursor ('P').</summary>
    ViBefore
}
