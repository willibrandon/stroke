using System.Runtime.CompilerServices;

namespace Stroke.History;

/// <summary>
/// Abstract base class providing common caching behavior for history implementations.
/// </summary>
/// <remarks>
/// <para>
/// Thread-safe: All mutable state is protected by synchronization.
/// </para>
/// <para>
/// Subclasses MUST implement <see cref="LoadHistoryStrings"/> and <see cref="StoreString"/>.
/// The base class provides concrete implementations of <see cref="AppendString"/>,
/// <see cref="GetStrings"/>, and <see cref="LoadAsync"/>.
/// </para>
/// </remarks>
public abstract class HistoryBase : IHistory
{
    private readonly Lock _lock = new();
    private bool _loaded;
    private List<string> _loadedStrings = [];

    /// <summary>
    /// Load history entries from the backend.
    /// </summary>
    /// <remarks>
    /// Subclasses MUST implement this to provide backend-specific loading.
    /// MUST yield items in newest-first order (most recent first).
    /// </remarks>
    /// <returns>Enumerable of history strings in newest-first order.</returns>
    public abstract IEnumerable<string> LoadHistoryStrings();

    /// <summary>
    /// Store a string to persistent storage.
    /// </summary>
    /// <remarks>
    /// Subclasses MUST implement this for backend-specific persistence.
    /// </remarks>
    /// <param name="value">The string to store.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public abstract void StoreString(string value);

    /// <summary>
    /// Append a string to history.
    /// </summary>
    /// <remarks>
    /// Inserts at index 0 of the cache (newest-first) and calls <see cref="StoreString"/> for persistence.
    /// </remarks>
    /// <param name="value">The string to append.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public virtual void AppendString(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        using (_lock.EnterScope())
        {
            _loadedStrings.Insert(0, value);
        }

        StoreString(value);
    }

    /// <summary>
    /// Get all history strings that are loaded so far.
    /// </summary>
    /// <remarks>
    /// Returns the cache in oldest-first order (reverses the internal newest-first storage).
    /// </remarks>
    /// <returns>Read-only list of history entries in oldest-first order.</returns>
    public IReadOnlyList<string> GetStrings()
    {
        using (_lock.EnterScope())
        {
            // Return a reversed copy (oldest-first)
            var result = new List<string>(_loadedStrings.Count);
            for (int i = _loadedStrings.Count - 1; i >= 0; i--)
            {
                result.Add(_loadedStrings[i]);
            }
            return result;
        }
    }

    /// <summary>
    /// Load history entries asynchronously.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Yields items in newest-first order (most recent first).
    /// </para>
    /// <para>
    /// First call triggers loading via <see cref="LoadHistoryStrings"/>;
    /// subsequent calls yield from cache.
    /// </para>
    /// </remarks>
    /// <param name="cancellationToken">Cancellation token for early termination.</param>
    /// <returns>Async enumerable of history entries in newest-first order.</returns>
    public async IAsyncEnumerable<string> LoadAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // Ensure history is loaded
        List<string> snapshot;
        using (_lock.EnterScope())
        {
            if (!_loaded)
            {
                _loadedStrings = [.. LoadHistoryStrings()];
                _loaded = true;
            }
            snapshot = [.. _loadedStrings];
        }

        // Yield outside the lock
        foreach (var item in snapshot)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return item;
        }

        await Task.CompletedTask;
    }
}
