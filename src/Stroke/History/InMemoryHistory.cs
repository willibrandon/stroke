namespace Stroke.History;

/// <summary>
/// In-memory history storage for session-only command history.
/// </summary>
/// <remarks>
/// <para>
/// Thread-safe: All mutable state is protected by synchronization.
/// </para>
/// <para>
/// Internal storage (<c>_storage</c>) is maintained in oldest-first order,
/// simulating on-disk storage. The inherited <c>_loadedStrings</c> cache
/// is in newest-first order per <see cref="HistoryBase"/> contract.
/// </para>
/// <para>
/// This mirrors Python Prompt Toolkit's <c>InMemoryHistory</c> class exactly.
/// </para>
/// </remarks>
public sealed class InMemoryHistory : HistoryBase
{
    private readonly Lock _storageLock = new();
    private readonly List<string> _storage = [];

    /// <summary>
    /// Gets the singleton empty history instance.
    /// </summary>
    /// <remarks>
    /// This instance should be used for read-only empty history scenarios.
    /// Do not append to this instance.
    /// </remarks>
    public static InMemoryHistory Empty { get; } = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="InMemoryHistory"/> class.
    /// </summary>
    public InMemoryHistory()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InMemoryHistory"/> class
    /// with pre-populated history entries.
    /// </summary>
    /// <param name="historyStrings">
    /// Initial history entries in oldest-first order. If null, creates empty history.
    /// </param>
    /// <remarks>
    /// Items are copied to internal storage in the provided order (oldest-first).
    /// The cache (<c>_loadedStrings</c>) remains empty until <see cref="HistoryBase.LoadAsync"/>
    /// is called for the first time.
    /// </remarks>
    public InMemoryHistory(IEnumerable<string>? historyStrings)
    {
        if (historyStrings is not null)
        {
            using (_storageLock.EnterScope())
            {
                foreach (var item in historyStrings)
                {
                    _storage.Add(item);
                }
            }
        }
    }

    /// <summary>
    /// Load history entries from the in-memory backend storage.
    /// </summary>
    /// <remarks>
    /// Yields items in newest-first order (reverses the oldest-first storage order).
    /// </remarks>
    /// <returns>Enumerable of history strings in newest-first order.</returns>
    public override IEnumerable<string> LoadHistoryStrings()
    {
        // Take a snapshot under lock, then yield outside lock
        List<string> snapshot;
        using (_storageLock.EnterScope())
        {
            snapshot = [.. _storage];
        }

        // Yield in reverse order (newest-first)
        for (int i = snapshot.Count - 1; i >= 0; i--)
        {
            yield return snapshot[i];
        }
    }

    /// <summary>
    /// Store a string to the in-memory backend storage.
    /// </summary>
    /// <param name="value">The string to store.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <remarks>
    /// Appends to storage in oldest-first order (new items go at the end).
    /// </remarks>
    public override void StoreString(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        using (_storageLock.EnterScope())
        {
            _storage.Add(value);
        }
    }
}
