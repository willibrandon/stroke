using System.Collections.Frozen;
using System.Collections.Immutable;

namespace Stroke.Contrib.RegularLanguages;

/// <summary>
/// Result of matching input against a compiled grammar.
/// This class is immutable and thread-safe.
/// </summary>
/// <remarks>
/// All regex group data is eagerly extracted at construction time and stored
/// in immutable collections. This ensures thread safety since
/// <see cref="System.Text.RegularExpressions.Match.Groups"/> uses lazy
/// initialization internally and is not safe for concurrent reads.
/// </remarks>
public sealed class Match
{
    /// <summary>
    /// Eagerly extracted data from a single regex group match.
    /// </summary>
    private readonly record struct GroupData(string VarName, bool Success, int Index, int Length);

    /// <summary>
    /// Eagerly extracted data from a single regex match (all its named groups).
    /// </summary>
    private readonly record struct MatchData(ImmutableArray<GroupData> Groups, int? TrailingStart, int? TrailingStop);

    private readonly ImmutableArray<MatchData> _matchData;
    private readonly FrozenDictionary<string, Func<string, string>> _unescapeFuncs;

    /// <summary>
    /// Create a new Match.
    /// </summary>
    internal Match(
        string input,
        List<(System.Text.RegularExpressions.Regex Pattern, System.Text.RegularExpressions.Match ReMatch)> reMatches,
        FrozenDictionary<string, string> groupNamesToVarNames,
        FrozenDictionary<string, Func<string, string>> unescapeFuncs)
    {
        Input = input;
        _unescapeFuncs = unescapeFuncs;

        // Eagerly extract all regex group data at construction time.
        // This avoids any concurrent access to System.Text.RegularExpressions.Match
        // objects, whose GroupCollection indexer is not thread-safe.
        var matchDataBuilder = ImmutableArray.CreateBuilder<MatchData>(reMatches.Count);

        foreach (var (pattern, reMatch) in reMatches)
        {
            var groupNames = pattern.GetGroupNames();
            var groupDataBuilder = ImmutableArray.CreateBuilder<GroupData>(groupNames.Length);
            int? trailingStart = null;
            int? trailingStop = null;

            foreach (var groupName in groupNames)
            {
                // Skip numeric groups
                if (groupName == "0")
                    continue;

                // Handle trailing input group separately
                if (groupName == "invalid_trailing")
                {
                    var trailingGroup = reMatch.Groups[groupName];
                    if (trailingGroup.Success && trailingGroup.Index >= 0)
                    {
                        trailingStart = trailingGroup.Index;
                        trailingStop = trailingGroup.Index + trailingGroup.Length;
                    }
                    continue;
                }

                if (!groupNamesToVarNames.TryGetValue(groupName, out var varName))
                    continue;

                var group = reMatch.Groups[groupName];
                groupDataBuilder.Add(new GroupData(varName, group.Success, group.Index, group.Length));
            }

            matchDataBuilder.Add(new MatchData(groupDataBuilder.ToImmutable(), trailingStart, trailingStop));
        }

        _matchData = matchDataBuilder.ToImmutable();
    }

    /// <summary>
    /// The original input string that was matched.
    /// </summary>
    public string Input { get; }

    /// <summary>
    /// Get the matched variables as a collection.
    /// </summary>
    /// <returns>A <see cref="Variables"/> instance containing all matched variable bindings.</returns>
    /// <remarks>
    /// For prefix matches, the same variable name may appear multiple times
    /// if the input is ambiguous (could match multiple grammar paths).
    /// </remarks>
    public Variables Variables()
    {
        var tuples = new List<(string VarName, string Value, int Start, int Stop)>();

        foreach (var matchData in _matchData)
        {
            foreach (var group in matchData.Groups)
            {
                if (group.Success && group.Index >= 0)
                {
                    var rawValue = Input.Substring(group.Index, group.Length);
                    var value = Unescape(group.VarName, rawValue);
                    tuples.Add((group.VarName, value, group.Index, group.Index + group.Length));
                }
            }
        }

        return new Variables(tuples);
    }

    /// <summary>
    /// Get trailing input that doesn't match the grammar.
    /// </summary>
    /// <returns>
    /// A <see cref="MatchVariable"/> representing the trailing input,
    /// or null if there is no trailing input.
    /// </returns>
    /// <remarks>
    /// Trailing input is text at the end of the input that doesn't match
    /// the grammar. This is used by the lexer to highlight invalid input.
    /// The VarName will be "&lt;trailing_input&gt;".
    /// </remarks>
    public MatchVariable? TrailingInput()
    {
        var slices = new List<(int Start, int Stop)>();

        foreach (var matchData in _matchData)
        {
            if (matchData.TrailingStart is not null && matchData.TrailingStop is not null)
            {
                slices.Add((matchData.TrailingStart.Value, matchData.TrailingStop.Value));
            }
        }

        if (slices.Count > 0)
        {
            // Take the smallest part (larger match is better)
            var start = slices.Max(s => s.Start);
            var stop = slices.Max(s => s.Stop);
            var value = Input.Substring(start, stop - start);
            return new MatchVariable("<trailing_input>", value, start, stop);
        }

        return null;
    }

    /// <summary>
    /// Get variables whose match ends at the end of the input string.
    /// Used for autocompletion to determine which variables can receive completions.
    /// </summary>
    /// <returns>
    /// An enumerable of <see cref="MatchVariable"/> instances for variables
    /// that end at the cursor position (end of input).
    /// </returns>
    public IEnumerable<MatchVariable> EndNodes()
    {
        foreach (var matchData in _matchData)
        {
            foreach (var group in matchData.Groups)
            {
                if (group.Success && group.Index >= 0)
                {
                    // If this part goes until the end of the input string
                    if (group.Index + group.Length == Input.Length)
                    {
                        var rawValue = Input.Substring(group.Index, group.Length);
                        var value = Unescape(group.VarName, rawValue);
                        yield return new MatchVariable(group.VarName, value, group.Index, group.Index + group.Length);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Get the variable at the specified cursor position.
    /// </summary>
    /// <param name="cursorPosition">0-based cursor position in the input.</param>
    /// <returns>
    /// A <see cref="MatchVariable"/> containing the cursor, or null if the
    /// cursor is not within any matched variable (e.g., in whitespace or literal text).
    /// </returns>
    /// <remarks>
    /// <para>
    /// The cursor is considered to be within a variable if it is at or after the start
    /// and before the stop position (i.e., <c>start &lt;= cursorPosition &lt; stop</c>).
    /// </para>
    /// <para>
    /// If multiple variables overlap at the cursor position, returns the first match found.
    /// </para>
    /// </remarks>
    public MatchVariable? VariableAtPosition(int cursorPosition)
    {
        foreach (var matchData in _matchData)
        {
            foreach (var group in matchData.Groups)
            {
                if (group.Success && group.Index >= 0)
                {
                    var start = group.Index;
                    var stop = group.Index + group.Length;

                    // Check if cursor is within this variable's range
                    if (cursorPosition >= start && cursorPosition < stop)
                    {
                        var rawValue = Input.Substring(start, group.Length);
                        var value = Unescape(group.VarName, rawValue);
                        return new MatchVariable(group.VarName, value, start, stop);
                    }
                }
            }
        }

        return null;
    }

    private string Unescape(string varName, string value)
    {
        if (_unescapeFuncs.TryGetValue(varName, out var func))
        {
            return func(value);
        }

        return value;
    }
}
