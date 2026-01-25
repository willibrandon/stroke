namespace Stroke.History;

/// <summary>
/// Interface for command history storage.
/// </summary>
/// <remarks>
/// <para>
/// Implementations MUST be thread-safe per Constitution XI.
/// </para>
/// <para>
/// Loading yields items in newest-first order (most recent at index 0);
/// <see cref="GetStrings"/> returns oldest-first (chronological order).
/// </para>
/// </remarks>
public interface IHistory
{
    /// <summary>
    /// Load history entries from the backend.
    /// </summary>
    /// <remarks>
    /// Subclasses must implement this to provide backend-specific loading.
    /// MUST yield items in newest-first order (most recent first).
    /// </remarks>
    /// <returns>Enumerable of history strings in newest-first order.</returns>
    IEnumerable<string> LoadHistoryStrings();

    /// <summary>
    /// Store a string to persistent storage.
    /// </summary>
    /// <remarks>
    /// Subclasses must implement this for backend-specific persistence.
    /// </remarks>
    /// <param name="value">The string to store.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    void StoreString(string value);

    /// <summary>
    /// Append a string to history.
    /// </summary>
    /// <remarks>
    /// Adds to in-memory cache and calls <see cref="StoreString"/> for persistence.
    /// </remarks>
    /// <param name="value">The string to append.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    void AppendString(string value);

    /// <summary>
    /// Get all history strings that are loaded so far.
    /// </summary>
    /// <returns>Read-only list of history entries in oldest-first order.</returns>
    IReadOnlyList<string> GetStrings();

    /// <summary>
    /// Load history entries asynchronously.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Yields items in newest-first order (most recent first).
    /// </para>
    /// <para>
    /// First call triggers loading; subsequent calls use cache.
    /// </para>
    /// </remarks>
    /// <param name="cancellationToken">Cancellation token for early termination.</param>
    /// <returns>Async enumerable of history entries in newest-first order.</returns>
    IAsyncEnumerable<string> LoadAsync(CancellationToken cancellationToken = default);
}
