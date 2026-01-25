using System.Runtime.CompilerServices;
using Stroke.Core;

namespace Stroke.Completion;

/// <summary>
/// Wrapper around a completer that removes duplicate completions.
/// </summary>
/// <remarks>
/// <para>
/// Only the first unique completions are kept. Completions are considered to be
/// a duplicate if they result in the same document text when they would be applied.
/// </para>
/// <para>
/// Completions that don't change the document at all are also removed.
/// </para>
/// <para>
/// This class maintains a HashSet during enumeration but is stateless between calls,
/// making it thread-safe per Constitution XI.
/// </para>
/// </remarks>
public sealed class DeduplicateCompleter : CompleterBase
{
    private readonly ICompleter _completer;

    /// <summary>
    /// Creates a deduplicating completer wrapping the specified completer.
    /// </summary>
    /// <param name="completer">The completer to wrap.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="completer"/> is null.</exception>
    public DeduplicateCompleter(ICompleter completer)
    {
        ArgumentNullException.ThrowIfNull(completer);
        _completer = completer;
    }

    /// <summary>
    /// Gets deduplicated completions from the wrapped completer.
    /// </summary>
    /// <param name="document">The current document.</param>
    /// <param name="completeEvent">Event describing how completion was triggered.</param>
    /// <returns>Completions with duplicates removed.</returns>
    public override IEnumerable<Completion> GetCompletions(Document document, CompleteEvent completeEvent)
    {
        // Keep track of the document strings we'd get after applying any completion.
        var foundSoFar = new HashSet<string>();

        foreach (var completion in _completer.GetCompletions(document, completeEvent))
        {
            var textIfApplied = GetTextIfApplied(document, completion);

            // Don't include completions that don't have any effect at all.
            if (textIfApplied == document.Text)
            {
                continue;
            }

            // Skip if we've seen this result before
            if (foundSoFar.Contains(textIfApplied))
            {
                continue;
            }

            foundSoFar.Add(textIfApplied);
            yield return completion;
        }
    }

    /// <summary>
    /// Gets deduplicated completions asynchronously from the wrapped completer.
    /// </summary>
    /// <param name="document">The current document.</param>
    /// <param name="completeEvent">Event describing how completion was triggered.</param>
    /// <param name="cancellationToken">Token to cancel the async operation.</param>
    /// <returns>Async enumerable of completions with duplicates removed.</returns>
    public override async IAsyncEnumerable<Completion> GetCompletionsAsync(
        Document document,
        CompleteEvent completeEvent,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var foundSoFar = new HashSet<string>();

        await foreach (var completion in _completer.GetCompletionsAsync(document, completeEvent, cancellationToken)
            .ConfigureAwait(false))
        {
            var textIfApplied = GetTextIfApplied(document, completion);

            if (textIfApplied == document.Text)
            {
                continue;
            }

            if (foundSoFar.Contains(textIfApplied))
            {
                continue;
            }

            foundSoFar.Add(textIfApplied);
            yield return completion;
        }
    }

    /// <summary>
    /// Computes what the document text would be if the completion were applied.
    /// </summary>
    private static string GetTextIfApplied(Document document, Completion completion)
    {
        // Text before the completion insertion point + completion text + text after cursor
        var beforeInsertion = document.Text[..(document.CursorPosition + completion.StartPosition)];
        var afterCursor = document.Text[document.CursorPosition..];
        return beforeInsertion + completion.Text + afterCursor;
    }

    /// <inheritdoc/>
    public override string ToString() => $"DeduplicateCompleter({_completer})";
}
