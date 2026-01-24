using Stroke.Core;

namespace Stroke.Completion;

/// <summary>
/// A completer that returns no completions.
/// </summary>
/// <remarks>
/// Used as the default completer when none is specified.
/// </remarks>
public sealed class DummyCompleter : ICompleter
{
    /// <summary>
    /// Gets the singleton instance of the dummy completer.
    /// </summary>
    public static DummyCompleter Instance { get; } = new();

    private DummyCompleter() { }

    /// <inheritdoc />
    public IEnumerable<Completion> GetCompletions(Document document, CompleteEvent completeEvent)
    {
        return [];
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<Completion> GetCompletionsAsync(Document document, CompleteEvent completeEvent)
    {
        await Task.CompletedTask;
        yield break;
    }
}
