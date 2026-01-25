using System.Runtime.CompilerServices;
using Stroke.Core;

namespace Stroke.Completion;

/// <summary>
/// Utility methods for working with completers.
/// </summary>
public static class CompletionUtils
{
    /// <summary>
    /// Combines several completers into one.
    /// </summary>
    /// <param name="completers">The completers to merge.</param>
    /// <param name="deduplicate">If true, wrap the result in a <see cref="DeduplicateCompleter"/>
    /// so that completions that would result in the same text will be deduplicated.</param>
    /// <returns>A merged completer that yields completions from all provided completers.</returns>
    public static ICompleter Merge(IEnumerable<ICompleter> completers, bool deduplicate = false)
    {
        var completerList = completers.ToList();

        if (completerList.Count == 0)
        {
            return DummyCompleter.Instance;
        }

        ICompleter merged = new MergedCompleter(completerList);

        if (deduplicate)
        {
            return new DeduplicateCompleter(merged);
        }

        return merged;
    }

    /// <summary>
    /// Returns the common prefix for all completions.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This finds the longest common suffix that can be safely inserted without
    /// affecting any completion. It's used to auto-complete the common part when
    /// the user presses Tab.
    /// </para>
    /// <para>
    /// If any completion changes text before the cursor differently, returns empty string.
    /// </para>
    /// </remarks>
    /// <param name="document">The current document.</param>
    /// <param name="completions">The list of completions to analyze.</param>
    /// <returns>The common suffix string that can be safely inserted.</returns>
    public static string GetCommonCompleteSuffix(Document document, IEnumerable<Completion> completions)
    {
        var completionList = completions.ToList();

        if (completionList.Count == 0)
        {
            return "";
        }

        // Take only completions that don't change the text before the cursor.
        bool DoesntChangeBeforeCursor(Completion completion)
        {
            // When start_position is 0, there's nothing before the insertion point to check.
            if (completion.StartPosition == 0)
            {
                return true;
            }

            // Get the part of completion text that would replace existing text
            // (the part before the original cursor position).
            // start_position is negative, so -start_position gives us the count of chars replaced.
            var end = completion.Text[..(-completion.StartPosition)];
            return document.TextBeforeCursor.EndsWith(end, StringComparison.Ordinal);
        }

        var filtered = completionList.Where(DoesntChangeBeforeCursor).ToList();

        // When there is at least one completion that changes the text before the
        // cursor, don't return any common part.
        if (filtered.Count != completionList.Count)
        {
            return "";
        }

        // Get the suffix part of each completion (the part after the replaced text).
        static string GetSuffix(Completion completion)
        {
            if (completion.StartPosition == 0)
            {
                return completion.Text;
            }

            // start_position is negative, so -start_position is the index to start from.
            return completion.Text[(-completion.StartPosition)..];
        }

        var suffixes = filtered.Select(GetSuffix).ToList();
        return CommonPrefix(suffixes);
    }

    /// <summary>
    /// Returns the common prefix of a collection of strings.
    /// Similar to Python's os.path.commonprefix.
    /// </summary>
    private static string CommonPrefix(IReadOnlyList<string> strings)
    {
        if (strings.Count == 0)
        {
            return "";
        }

        // Find min and max strings lexicographically.
        // The common prefix of all strings is the common prefix of min and max.
        var s1 = strings.Min()!;
        var s2 = strings.Max()!;

        for (var i = 0; i < s1.Length; i++)
        {
            if (i >= s2.Length || s1[i] != s2[i])
            {
                return s1[..i];
            }
        }

        return s1;
    }

    /// <summary>
    /// Internal completer that combines several completers into one.
    /// </summary>
    private sealed class MergedCompleter : CompleterBase
    {
        private readonly IReadOnlyList<ICompleter> _completers;

        public MergedCompleter(IReadOnlyList<ICompleter> completers)
        {
            _completers = completers;
        }

        /// <summary>
        /// Gets completions from all wrapped completers in order.
        /// </summary>
        public override IEnumerable<Completion> GetCompletions(Document document, CompleteEvent completeEvent)
        {
            foreach (var completer in _completers)
            {
                foreach (var completion in completer.GetCompletions(document, completeEvent))
                {
                    yield return completion;
                }
            }
        }

        /// <summary>
        /// Gets completions asynchronously from all wrapped completers in order.
        /// </summary>
        public override async IAsyncEnumerable<Completion> GetCompletionsAsync(
            Document document,
            CompleteEvent completeEvent,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            foreach (var completer in _completers)
            {
                await foreach (var completion in completer.GetCompletionsAsync(document, completeEvent, cancellationToken)
                    .ConfigureAwait(false))
                {
                    yield return completion;
                }
            }
        }

        /// <inheritdoc/>
        public override string ToString() => $"MergedCompleter([{string.Join(", ", _completers)}])";
    }
}
