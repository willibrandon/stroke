namespace Stroke.Application;

/// <summary>
/// Discriminated union for items in the <see cref="StdoutProxy"/> flush queue.
/// </summary>
/// <remarks>
/// Port of Python Prompt Toolkit's <c>str | _Done</c> queue item type from
/// <c>patch_stdout.py</c>.
/// </remarks>
internal abstract record FlushItem
{
    /// <summary>
    /// Text to be written to the terminal output.
    /// </summary>
    internal sealed record Text(string Value) : FlushItem;

    /// <summary>
    /// Sentinel value signaling the flush thread to terminate.
    /// </summary>
    internal sealed record Done() : FlushItem;
}
