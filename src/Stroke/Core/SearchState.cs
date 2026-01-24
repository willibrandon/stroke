namespace Stroke.Core;

/// <summary>
/// Tracks state for search operations.
/// </summary>
/// <remarks>
/// This is a stub class for Feature 07; full implementation in Feature 10.
/// </remarks>
public sealed class SearchState
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SearchState"/> class.
    /// </summary>
    /// <param name="text">The search text.</param>
    /// <param name="direction">The search direction.</param>
    public SearchState(string text = "", SearchDirection direction = SearchDirection.Forward)
    {
        Text = text;
        Direction = direction;
    }

    /// <summary>
    /// Gets or sets the search text.
    /// </summary>
    public string Text { get; set; }

    /// <summary>
    /// Gets or sets the search direction.
    /// </summary>
    public SearchDirection Direction { get; set; }

    /// <summary>
    /// Gets or sets the filter function for case-insensitive search.
    /// </summary>
    public Func<bool>? IgnoreCaseFilter { get; set; }

    /// <summary>
    /// Gets a value indicating whether to ignore case during search.
    /// </summary>
    /// <returns>True if the search should ignore case; otherwise, false.</returns>
    public bool IgnoreCase() => IgnoreCaseFilter?.Invoke() ?? false;
}
