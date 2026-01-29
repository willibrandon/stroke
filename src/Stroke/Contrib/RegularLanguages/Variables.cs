using System.Collections;
using System.Collections.Immutable;

namespace Stroke.Contrib.RegularLanguages;

/// <summary>
/// Collection of matched variable name-value pairs.
/// This class is immutable and thread-safe.
/// Implements <see cref="IEnumerable{MatchVariable}"/> for iteration.
/// </summary>
public sealed class Variables : IEnumerable<MatchVariable>
{
    private readonly ImmutableArray<(string VarName, string Value, int Start, int Stop)> _tuples;

    /// <summary>
    /// Create a new Variables collection.
    /// </summary>
    /// <param name="tuples">List of (varName, value, start, stop) tuples.</param>
    internal Variables(IEnumerable<(string VarName, string Value, int Start, int Stop)> tuples)
    {
        _tuples = tuples.ToImmutableArray();
    }

    /// <summary>
    /// Get the first value of a variable by name.
    /// </summary>
    /// <param name="key">Variable name.</param>
    /// <returns>The first matched value, or null if the variable was not matched.</returns>
    public string? Get(string key)
    {
        foreach (var (varName, value, _, _) in _tuples)
        {
            if (varName == key)
            {
                return value;
            }
        }

        return null;
    }

    /// <summary>
    /// Get the first value of a variable by name with a default value.
    /// </summary>
    /// <param name="key">Variable name.</param>
    /// <param name="defaultValue">Default value if not found.</param>
    /// <returns>The first matched value, or the default value if not found.</returns>
    public string Get(string key, string defaultValue)
    {
        return Get(key) ?? defaultValue;
    }

    /// <summary>
    /// Get all values for a variable (for repeated matches or ambiguous grammars).
    /// </summary>
    /// <param name="key">Variable name.</param>
    /// <returns>
    /// A list of all matched values for this variable.
    /// Returns an empty list if the variable was not matched.
    /// </returns>
    public IReadOnlyList<string> GetAll(string key)
    {
        var results = new List<string>();
        foreach (var (varName, value, _, _) in _tuples)
        {
            if (varName == key)
            {
                results.Add(value);
            }
        }

        return results;
    }

    /// <summary>
    /// Indexer for getting the first variable value.
    /// Equivalent to <see cref="Get(string)"/>.
    /// </summary>
    /// <param name="key">Variable name.</param>
    /// <returns>The first matched value, or null if not found.</returns>
    public string? this[string key] => Get(key);

    /// <inheritdoc/>
    public IEnumerator<MatchVariable> GetEnumerator()
    {
        foreach (var (varName, value, start, stop) in _tuples)
        {
            yield return new MatchVariable(varName, value, start, stop);
        }
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <inheritdoc/>
    public override string ToString()
    {
        var pairs = _tuples.Select(t => $"{t.VarName}='{t.Value}'");
        return $"Variables({string.Join(", ", pairs)})";
    }
}
