namespace Stroke.History;

/// <summary>
/// In-memory history storage.
/// </summary>
/// <remarks>
/// Thread-safe: All mutable state is protected by synchronization.
/// </remarks>
public sealed class InMemoryHistory : IHistory
{
    private readonly Lock _lock = new();
    private readonly List<string> _history = [];

    /// <summary>
    /// Gets the singleton empty history instance.
    /// </summary>
    public static InMemoryHistory Empty { get; } = new();

    /// <inheritdoc />
    public IReadOnlyList<string> GetStrings()
    {
        using (_lock.EnterScope())
        {
            return [.. _history];
        }
    }

    /// <inheritdoc />
    public void AppendString(string text)
    {
        using (_lock.EnterScope())
        {
            _history.Add(text);
        }
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<string> LoadAsync()
    {
        // Snapshot the history under lock
        List<string> snapshot;
        using (_lock.EnterScope())
        {
            snapshot = [.. _history];
        }

        // Yield outside the lock
        foreach (var entry in snapshot)
        {
            yield return entry;
        }

        await Task.CompletedTask;
    }

    /// <summary>
    /// Append a string to history asynchronously.
    /// </summary>
    /// <param name="text">The text to append.</param>
    public ValueTask AppendAsync(string text)
    {
        AppendString(text);
        return ValueTask.CompletedTask;
    }
}
