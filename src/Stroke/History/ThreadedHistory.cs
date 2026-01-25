using System.Runtime.CompilerServices;

namespace Stroke.History;

/// <summary>
/// Wrapper that provides background history loading for non-blocking startup.
/// </summary>
/// <remarks>
/// <para>
/// Thread-safe: All mutable state is protected by synchronization.
/// </para>
/// <para>
/// ThreadedHistory wraps any <see cref="IHistory"/> implementation and loads
/// history in a background daemon thread. Items are yielded progressively
/// via <see cref="LoadAsync"/> as they become available.
/// </para>
/// <para>
/// The background thread is only started on the first <see cref="LoadAsync"/> call.
/// Multiple concurrent <see cref="LoadAsync"/> calls share the same loaded data.
/// </para>
/// <para>
/// This mirrors Python Prompt Toolkit's <c>ThreadedHistory</c> class exactly.
/// </para>
/// </remarks>
public sealed class ThreadedHistory : IHistory
{
    private readonly IHistory _history;
    private readonly Lock _lock = new();
    private Thread? _loadThread;
    private bool _loading;
    private bool _loaded;
    private List<string> _loadedStrings = [];
    private readonly List<string> _pendingAppends = [];
    private readonly List<ManualResetEventSlim> _stringLoadEvents = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="ThreadedHistory"/> class.
    /// </summary>
    /// <param name="history">The history implementation to wrap.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="history"/> is null.</exception>
    public ThreadedHistory(IHistory history)
    {
        ArgumentNullException.ThrowIfNull(history);
        _history = history;
    }

    /// <summary>
    /// Gets the wrapped history instance.
    /// </summary>
    public IHistory History => _history;

    /// <summary>
    /// Load history entries from the wrapped history.
    /// </summary>
    /// <remarks>
    /// Delegates directly to the wrapped history's <see cref="IHistory.LoadHistoryStrings"/>.
    /// </remarks>
    /// <returns>Enumerable of history strings in newest-first order.</returns>
    public IEnumerable<string> LoadHistoryStrings()
    {
        return _history.LoadHistoryStrings();
    }

    /// <summary>
    /// Store a string to the wrapped history.
    /// </summary>
    /// <param name="value">The string to store.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <remarks>
    /// Delegates directly to the wrapped history's <see cref="IHistory.StoreString"/>.
    /// </remarks>
    public void StoreString(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        _history.StoreString(value);
    }

    /// <summary>
    /// Append a string to history.
    /// </summary>
    /// <param name="value">The string to append.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <remarks>
    /// Inserts at index 0 of the cache (newest-first) and calls <see cref="StoreString"/>
    /// for persistence. The item is immediately visible to any active <see cref="LoadAsync"/> consumers.
    /// </remarks>
    public void AppendString(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        using (_lock.EnterScope())
        {
            if (_loading)
            {
                // During loading, collect appends separately to avoid index shifting
                _pendingAppends.Add(value);
            }
            else
            {
                _loadedStrings.Insert(0, value);
            }
            SignalAllEvents();
        }

        StoreString(value);
    }

    /// <summary>
    /// Get all history strings that are loaded so far.
    /// </summary>
    /// <returns>Read-only list of history entries in oldest-first order.</returns>
    public IReadOnlyList<string> GetStrings()
    {
        using (_lock.EnterScope())
        {
            // Return reversed copy (oldest-first)
            var result = new List<string>(_loadedStrings.Count);
            for (int i = _loadedStrings.Count - 1; i >= 0; i--)
            {
                result.Add(_loadedStrings[i]);
            }
            return result;
        }
    }

    /// <summary>
    /// Load history entries asynchronously with background loading.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for early termination.</param>
    /// <returns>Async enumerable of history entries in newest-first order.</returns>
    /// <remarks>
    /// <para>
    /// First call starts a background daemon thread that loads from the wrapped history.
    /// Items are yielded progressively as they become available.
    /// </para>
    /// <para>
    /// Subsequent calls yield from cache plus any items still being loaded.
    /// </para>
    /// </remarks>
    public async IAsyncEnumerable<string> LoadAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // Create our event for signaling
        var myEvent = new ManualResetEventSlim(false);

        try
        {
            // Register our event and possibly start loading
            using (_lock.EnterScope())
            {
                _stringLoadEvents.Add(myEvent);

                if (_loadThread is null && !_loaded)
                {
                    StartLoadThread();
                }
            }

            // Use content-based tracking to handle prepending correctly
            // (when items are inserted at front, indices shift but content doesn't)
            var yieldedSet = new HashSet<string>();

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Get current state
                List<string> currentStrings;
                bool isLoaded;

                using (_lock.EnterScope())
                {
                    currentStrings = [.. _loadedStrings];
                    isLoaded = _loaded;
                }

                // Yield any new items not yet yielded
                foreach (var item in currentStrings)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (yieldedSet.Add(item))
                    {
                        yield return item;
                    }
                }

                // If loading is complete, we're done
                if (isLoaded)
                {
                    break;
                }

                // Wait for more items or completion
                try
                {
                    myEvent.Wait(100, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }

                myEvent.Reset();
            }
        }
        finally
        {
            // Unregister our event
            using (_lock.EnterScope())
            {
                _stringLoadEvents.Remove(myEvent);
            }

            myEvent.Dispose();
        }

        await Task.CompletedTask;
    }

    /// <summary>
    /// Start the background loading thread.
    /// </summary>
    private void StartLoadThread()
    {
        _loadThread = new Thread(LoadThreadProc)
        {
            IsBackground = true, // Daemon thread - doesn't prevent app exit
            Name = "ThreadedHistory.LoadThread"
        };
        _loadThread.Start();
    }

    /// <summary>
    /// Background thread procedure that loads from wrapped history.
    /// </summary>
    private void LoadThreadProc()
    {
        List<string> itemsAppendedBeforeLoad = [];
        HashSet<string> loadedFromBackend = [];

        try
        {
            // Mark loading started and capture items appended before load
            using (_lock.EnterScope())
            {
                _loading = true;
                itemsAppendedBeforeLoad = [.. _loadedStrings];
                _loadedStrings = [];
            }

            foreach (var item in _history.LoadHistoryStrings())
            {
                loadedFromBackend.Add(item);
                using (_lock.EnterScope())
                {
                    _loadedStrings.Add(item);
                    SignalAllEvents();
                }
            }
        }
        finally
        {
            using (_lock.EnterScope())
            {
                // Prepend items appended before load that weren't in the backend snapshot
                // (items already in backend were loaded above, so skip them to avoid duplicates)
                var itemsToPreserve = itemsAppendedBeforeLoad
                    .Where(item => !loadedFromBackend.Contains(item))
                    .ToList();

                if (itemsToPreserve.Count > 0)
                {
                    _loadedStrings.InsertRange(0, itemsToPreserve);
                }

                // Prepend items that were appended during loading (newest first)
                // These go at the front since they're the most recent
                if (_pendingAppends.Count > 0)
                {
                    // Reverse because _pendingAppends has oldest-first order
                    // (each append added to end), but we need newest-first
                    for (int i = _pendingAppends.Count - 1; i >= 0; i--)
                    {
                        _loadedStrings.Insert(0, _pendingAppends[i]);
                    }
                    _pendingAppends.Clear();
                }

                _loading = false;
                _loaded = true;
                SignalAllEvents();
            }
        }
    }

    /// <summary>
    /// Signal all waiting events that new items are available.
    /// </summary>
    /// <remarks>
    /// Must be called while holding _lock.
    /// </remarks>
    private void SignalAllEvents()
    {
        foreach (var evt in _stringLoadEvents)
        {
            evt.Set();
        }
    }
}
