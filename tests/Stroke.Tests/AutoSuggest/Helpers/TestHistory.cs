using System.Runtime.CompilerServices;
using Stroke.History;

namespace Stroke.Tests.AutoSuggest.Helpers;

/// <summary>
/// Test implementation of IHistory for AutoSuggest tests.
/// </summary>
/// <remarks>
/// This is a real implementation (not a mock) per Constitution VIII.
/// </remarks>
internal sealed class TestHistory : IHistory
{
    private readonly Lock _lock = new();
    private readonly List<string> _strings = [];
    private bool _loaded;

    /// <inheritdoc />
    public IEnumerable<string> LoadHistoryStrings()
    {
        List<string> snapshot;
        using (_lock.EnterScope())
        {
            snapshot = [.. _strings];
        }

        // Return in newest-first order (reverse)
        for (int i = snapshot.Count - 1; i >= 0; i--)
        {
            yield return snapshot[i];
        }
    }

    /// <inheritdoc />
    public void StoreString(string value)
    {
        using (_lock.EnterScope())
        {
            _strings.Add(value);
        }
    }

    /// <inheritdoc />
    public IReadOnlyList<string> GetStrings()
    {
        using (_lock.EnterScope())
        {
            return [.. _strings];
        }
    }

    /// <inheritdoc />
    public void AppendString(string value)
    {
        StoreString(value);
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<string> LoadAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        List<string> snapshot;
        using (_lock.EnterScope())
        {
            if (!_loaded)
            {
                _loaded = true;
            }
            snapshot = [.. _strings];
        }

        // Yield in newest-first order (reverse)
        for (int i = snapshot.Count - 1; i >= 0; i--)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return snapshot[i];
        }

        await Task.CompletedTask;
    }
}
