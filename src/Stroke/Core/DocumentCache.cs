using System.Collections.Immutable;

namespace Stroke.Core;

/// <summary>
/// Internal cache for Document line data.
/// </summary>
internal sealed class DocumentCache
{
    /// <summary>
    /// Gets or sets the cached lines array (null until computed).
    /// </summary>
    public ImmutableArray<string>? Lines { get; set; }

    /// <summary>
    /// Gets or sets the cached line start indexes (null until computed).
    /// </summary>
    public int[]? LineIndexes { get; set; }
}
