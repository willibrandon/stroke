using Stroke.Core;

namespace Stroke.AutoSuggest;

/// <summary>
/// Give suggestions based on the lines in the history.
/// </summary>
/// <remarks>
/// Thread-safe: Stateless implementation with no mutable fields.
///
/// Algorithm:
/// 1. Extract current line (text after last '\n')
/// 2. Skip if empty or whitespace-only
/// 3. Search history entries from most recent to oldest
/// 4. Within each entry, search lines from last to first
/// 5. Return suffix of first case-sensitive prefix match
/// </remarks>
public sealed class AutoSuggestFromHistory : IAutoSuggest
{
    /// <inheritdoc />
    /// <exception cref="ArgumentNullException">Thrown if buffer or document is null.</exception>
    public Suggestion? GetSuggestion(IBuffer buffer, Document document)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        ArgumentNullException.ThrowIfNull(document);

        var history = buffer.History;

        // Consider only the last line for the suggestion (Python: rsplit("\n", 1)[-1])
        var text = document.Text;
        var lastNewlineIndex = text.LastIndexOf('\n');
        var currentLine = lastNewlineIndex >= 0 ? text[(lastNewlineIndex + 1)..] : text;

        // Only create a suggestion when this is not an empty line (Python: text.strip())
        if (string.IsNullOrWhiteSpace(currentLine))
            return null;

        // Find first matching line in history (search from most recent)
        var historyStrings = history.GetStrings();
        for (int i = historyStrings.Count - 1; i >= 0; i--)
        {
            var entry = historyStrings[i];
            // Python: string.splitlines() then reversed()
            var lines = entry.Split('\n');
            for (int j = lines.Length - 1; j >= 0; j--)
            {
                var line = lines[j];
                // Case-sensitive prefix match (Python: line.startswith(text))
                if (line.StartsWith(currentLine, StringComparison.Ordinal))
                {
                    return new Suggestion(line[currentLine.Length..]);
                }
            }
        }

        return null;
    }

    /// <inheritdoc />
    public ValueTask<Suggestion?> GetSuggestionAsync(IBuffer buffer, Document document)
        => ValueTask.FromResult(GetSuggestion(buffer, document));
}
