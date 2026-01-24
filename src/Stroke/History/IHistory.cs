namespace Stroke.History;

/// <summary>
/// Interface for command history storage.
/// </summary>
/// <remarks>
/// This is a minimal stub; full implementation in History feature.
/// Returns history entries ordered oldest-to-newest; search implementations iterate in reverse.
/// </remarks>
public interface IHistory
{
    /// <summary>
    /// Gets all history strings.
    /// </summary>
    /// <returns>Read-only list of history entries, oldest first.</returns>
    IReadOnlyList<string> GetStrings();

    /// <summary>
    /// Append a string to history.
    /// </summary>
    /// <param name="text">The text to append.</param>
    void AppendString(string text);

    /// <summary>
    /// Load history entries asynchronously.
    /// </summary>
    /// <returns>Async enumerable of history entries.</returns>
    IAsyncEnumerable<string> LoadAsync();
}
