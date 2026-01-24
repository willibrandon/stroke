using Stroke.Core;

namespace Stroke.AutoSuggest;

/// <summary>
/// Wrapper that runs auto suggestion in a background thread.
/// </summary>
/// <remarks>
/// Thread-safe: Stores only readonly reference; execution is thread-safe via Task.Run.
///
/// Python uses <c>run_in_executor_with_context</c> to offload to thread pool while preserving
/// execution context. .NET's <c>Task.Run</c> already captures execution context, providing
/// equivalent semantics.
///
/// <c>ConfigureAwait(false)</c> is used because auto-suggest results don't need to return
/// to a specific synchronization context (no UI thread affinity required at this layer).
/// </remarks>
public sealed class ThreadedAutoSuggest : IAutoSuggest
{
    private readonly IAutoSuggest _autoSuggest;

    /// <summary>
    /// Creates a threaded auto suggest.
    /// </summary>
    /// <param name="autoSuggest">The underlying auto suggest to run in background.</param>
    /// <exception cref="ArgumentNullException">Thrown if autoSuggest is null.</exception>
    public ThreadedAutoSuggest(IAutoSuggest autoSuggest)
    {
        ArgumentNullException.ThrowIfNull(autoSuggest);
        _autoSuggest = autoSuggest;
    }

    /// <inheritdoc />
    /// <remarks>
    /// Synchronous call delegates directly to wrapped provider on current thread.
    /// No threading occurs for sync calls (matches Python behavior).
    /// </remarks>
    public Suggestion? GetSuggestion(IBuffer buffer, Document document)
        => _autoSuggest.GetSuggestion(buffer, document);

    /// <inheritdoc />
    /// <remarks>
    /// Executes wrapped provider's sync method on a dedicated background thread.
    /// Uses <c>TaskCreationOptions.LongRunning</c> to guarantee execution on a new thread,
    /// matching Python's behavior where <c>run_in_executor</c> always uses a thread
    /// different from the event loop thread.
    /// If wrapped provider throws, exception is captured and re-thrown when awaited.
    /// </remarks>
    public async ValueTask<Suggestion?> GetSuggestionAsync(IBuffer buffer, Document document)
    {
        return await Task.Factory.StartNew(
            () => _autoSuggest.GetSuggestion(buffer, document),
            CancellationToken.None,
            TaskCreationOptions.LongRunning,
            TaskScheduler.Default).ConfigureAwait(false);
    }
}
