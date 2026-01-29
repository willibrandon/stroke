using System.Collections.Frozen;
using System.Text.RegularExpressions;

namespace Stroke.Contrib.RegularLanguages;

/// <summary>
/// Result of matching input against a compiled grammar.
/// This class is immutable and thread-safe.
/// </summary>
public sealed class Match
{
    private const string InvalidTrailingInput = "invalid_trailing";

    private readonly List<(Regex Pattern, System.Text.RegularExpressions.Match ReMatch)> _reMatches;
    private readonly FrozenDictionary<string, string> _groupNamesToVarNames;
    private readonly FrozenDictionary<string, Func<string, string>> _unescapeFuncs;

    /// <summary>
    /// Create a new Match.
    /// </summary>
    internal Match(
        string input,
        List<(Regex Pattern, System.Text.RegularExpressions.Match ReMatch)> reMatches,
        FrozenDictionary<string, string> groupNamesToVarNames,
        FrozenDictionary<string, Func<string, string>> unescapeFuncs)
    {
        Input = input;
        _reMatches = reMatches;
        _groupNamesToVarNames = groupNamesToVarNames;
        _unescapeFuncs = unescapeFuncs;
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

        foreach (var (pattern, reMatch) in _reMatches)
        {
            foreach (var groupName in pattern.GetGroupNames())
            {
                // Skip numeric groups and the trailing input group
                if (groupName == "0" || groupName == InvalidTrailingInput)
                {
                    continue;
                }

                if (!_groupNamesToVarNames.TryGetValue(groupName, out var varName))
                {
                    continue;
                }

                var group = reMatch.Groups[groupName];
                if (group.Success && group.Index >= 0)
                {
                    var rawValue = Input.Substring(group.Index, group.Length);
                    var value = Unescape(varName, rawValue);
                    tuples.Add((varName, value, group.Index, group.Index + group.Length));
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

        foreach (var (pattern, reMatch) in _reMatches)
        {
            var group = reMatch.Groups[InvalidTrailingInput];
            if (group.Success && group.Index >= 0)
            {
                slices.Add((group.Index, group.Index + group.Length));
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
        foreach (var (pattern, reMatch) in _reMatches)
        {
            foreach (var groupName in pattern.GetGroupNames())
            {
                // Skip numeric groups and the trailing input group
                if (groupName == "0" || groupName == InvalidTrailingInput)
                {
                    continue;
                }

                if (!_groupNamesToVarNames.TryGetValue(groupName, out var varName))
                {
                    continue;
                }

                var group = reMatch.Groups[groupName];
                if (group.Success && group.Index >= 0)
                {
                    // If this part goes until the end of the input string
                    if (group.Index + group.Length == Input.Length)
                    {
                        var rawValue = Input.Substring(group.Index, group.Length);
                        var value = Unescape(varName, rawValue);
                        yield return new MatchVariable(varName, value, group.Index, group.Index + group.Length);
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
        foreach (var (pattern, reMatch) in _reMatches)
        {
            foreach (var groupName in pattern.GetGroupNames())
            {
                // Skip numeric groups and the trailing input group
                if (groupName == "0" || groupName == InvalidTrailingInput)
                {
                    continue;
                }

                if (!_groupNamesToVarNames.TryGetValue(groupName, out var varName))
                {
                    continue;
                }

                var group = reMatch.Groups[groupName];
                if (group.Success && group.Index >= 0)
                {
                    var start = group.Index;
                    var stop = group.Index + group.Length;

                    // Check if cursor is within this variable's range
                    if (cursorPosition >= start && cursorPosition < stop)
                    {
                        var rawValue = Input.Substring(start, group.Length);
                        var value = Unescape(varName, rawValue);
                        return new MatchVariable(varName, value, start, stop);
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
