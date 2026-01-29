using Stroke.Core;

namespace Stroke.Lexers;

/// <summary>
/// Always start syntax highlighting from the beginning.
/// </summary>
/// <remarks>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>SyncFromStart</c> class
/// from <c>prompt_toolkit.lexers.pygments</c>.
/// </para>
/// <para>
/// This implementation uses a singleton pattern for efficiency. The class is
/// thread-safe and immutable.
/// </para>
/// <para>
/// Use this strategy for small documents or when accuracy is more important
/// than performance. For large documents, consider <see cref="RegexSync"/>.
/// </para>
/// </remarks>
public sealed class SyncFromStart : ISyntaxSync
{
    /// <summary>
    /// Private constructor to enforce singleton pattern.
    /// </summary>
    private SyncFromStart()
    {
    }

    /// <summary>
    /// Gets the singleton instance.
    /// </summary>
    public static SyncFromStart Instance { get; } = new SyncFromStart();

    /// <inheritdoc/>
    /// <remarks>
    /// Always returns (0, 0), meaning lexing always starts from the beginning
    /// of the document.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="document"/> is <c>null</c>.</exception>
    public (int Row, int Column) GetSyncStartPosition(Document document, int lineNo)
    {
        ArgumentNullException.ThrowIfNull(document);
        return (0, 0);
    }
}
