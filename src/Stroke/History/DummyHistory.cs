using System.Runtime.CompilerServices;

namespace Stroke.History;

/// <summary>
/// History implementation that doesn't remember anything.
/// </summary>
/// <remarks>
/// <para>
/// Thread-safe: This class is stateless and therefore inherently thread-safe.
/// </para>
/// <para>
/// DummyHistory is useful for privacy-sensitive contexts where command history
/// should not be retained, or for testing scenarios where history is not needed.
/// </para>
/// <para>
/// All operations are no-ops:
/// <list type="bullet">
/// <item><see cref="AppendString"/> and <see cref="StoreString"/> discard their input</item>
/// <item><see cref="GetStrings"/> always returns an empty list</item>
/// <item><see cref="LoadAsync"/> and <see cref="LoadHistoryStrings"/> yield nothing</item>
/// </list>
/// </para>
/// <para>
/// This mirrors Python Prompt Toolkit's <c>DummyHistory</c> class exactly.
/// </para>
/// </remarks>
public sealed class DummyHistory : IHistory
{
    /// <summary>
    /// Load history entries (always empty).
    /// </summary>
    /// <returns>Empty enumerable.</returns>
    public IEnumerable<string> LoadHistoryStrings()
    {
        return [];
    }

    /// <summary>
    /// Store a string (no-op, discards input).
    /// </summary>
    /// <param name="value">The string to store (ignored).</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <remarks>
    /// While the value is discarded, null validation is performed to maintain
    /// consistent API behavior across all IHistory implementations.
    /// </remarks>
    public void StoreString(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        // Don't remember this.
    }

    /// <summary>
    /// Append a string to history (no-op, discards input).
    /// </summary>
    /// <param name="value">The string to append (ignored).</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <remarks>
    /// While the value is discarded, null validation is performed to maintain
    /// consistent API behavior across all IHistory implementations.
    /// </remarks>
    public void AppendString(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        // Don't remember this.
    }

    /// <summary>
    /// Get all history strings (always empty).
    /// </summary>
    /// <returns>Empty read-only list.</returns>
    public IReadOnlyList<string> GetStrings()
    {
        return [];
    }

    /// <summary>
    /// Load history entries asynchronously (yields nothing).
    /// </summary>
    /// <param name="cancellationToken">Cancellation token (unused, completes immediately).</param>
    /// <returns>Empty async enumerable.</returns>
    public async IAsyncEnumerable<string> LoadAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        yield break;
    }
}
