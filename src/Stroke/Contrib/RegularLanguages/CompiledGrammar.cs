using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Text;
using System.Text.RegularExpressions;

namespace Stroke.Contrib.RegularLanguages;

/// <summary>
/// A compiled grammar that can match input strings and extract variables.
/// This class is thread-safe; all operations can be called concurrently.
/// </summary>
/// <remarks>
/// <para>
/// Use <see cref="Grammar.Compile"/> to create instances.
/// </para>
/// <para>
/// All regex patterns are pre-compiled at construction time with
/// <see cref="RegexOptions.Compiled"/> for maximum matching performance.
/// </para>
/// </remarks>
public sealed class CompiledGrammar
{
    private const string InvalidTrailingInput = "invalid_trailing";

    private readonly Regex _fullPattern;
    private readonly ImmutableArray<Regex> _prefixPatterns;
    private readonly ImmutableArray<Regex> _prefixWithTrailingPatterns;
    private readonly FrozenDictionary<string, string> _groupNamesToVarNames;
    private readonly FrozenDictionary<string, Func<string, string>> _escapeFuncs;
    private readonly FrozenDictionary<string, Func<string, string>> _unescapeFuncs;

    /// <summary>
    /// Create a CompiledGrammar from a parse tree.
    /// </summary>
    internal CompiledGrammar(
        Node rootNode,
        IDictionary<string, Func<string, string>>? escapeFuncs,
        IDictionary<string, Func<string, string>>? unescapeFuncs)
    {
        _escapeFuncs = (escapeFuncs ?? new Dictionary<string, Func<string, string>>())
            .ToFrozenDictionary();
        _unescapeFuncs = (unescapeFuncs ?? new Dictionary<string, Func<string, string>>())
            .ToFrozenDictionary();

        // Dictionary that will map the regex names to variable names
        var groupNamesToVarNames = new Dictionary<string, string>();
        var counter = 0;

        string CreateGroupFunc(Variable node)
        {
            var name = $"n{counter++}";
            groupNamesToVarNames[name] = node.VarName;
            return name;
        }

        // Compile regex strings
        var fullPatternStr = $"^{Transform(rootNode, CreateGroupFunc)}$";
        var prefixPatternStrs = TransformPrefix(rootNode, CreateGroupFunc).ToList();

        // Compile the regexes
        const RegexOptions options = RegexOptions.Singleline | RegexOptions.Compiled;
        _fullPattern = new Regex(fullPatternStr, options);
        _prefixPatterns = prefixPatternStrs.Select(p => new Regex(p, options)).ToImmutableArray();

        // Compile patterns that also accept trailing input
        _prefixWithTrailingPatterns = prefixPatternStrs
            .Select(p => new Regex(
                $"(?:{p.TrimEnd('$').TrimStart('^')})(?<{InvalidTrailingInput}>.*?)$",
                options))
            .ToImmutableArray();

        _groupNamesToVarNames = groupNamesToVarNames.ToFrozenDictionary();
    }

    /// <summary>
    /// Match the complete string against the grammar.
    /// Returns null if the input doesn't match the grammar exactly.
    /// </summary>
    /// <param name="input">The input string to match.</param>
    /// <returns>
    /// A <see cref="Match"/> instance if the input matches the grammar exactly,
    /// or null if there is no match.
    /// </returns>
    public Match? Match(string input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var reMatch = _fullPattern.Match(input);
        if (reMatch.Success)
        {
            return new Match(
                input,
                [(_fullPattern, reMatch)],
                _groupNamesToVarNames,
                _unescapeFuncs);
        }

        return null;
    }

    /// <summary>
    /// Match a prefix of the string against the grammar.
    /// Used for autocompletion on incomplete input.
    /// </summary>
    /// <param name="input">The input string (typically text before cursor).</param>
    /// <returns>
    /// A <see cref="Match"/> instance representing all possible prefix matches,
    /// or null if the input cannot match any prefix of the grammar.
    /// </returns>
    /// <remarks>
    /// If the input contains trailing characters that don't match the grammar,
    /// those are captured separately and can be retrieved via <see cref="Match.TrailingInput"/>.
    /// </remarks>
    public Match? MatchPrefix(string input)
    {
        ArgumentNullException.ThrowIfNull(input);

        // First try to match using prefix patterns
        var matches = new List<(Regex, System.Text.RegularExpressions.Match)>();
        foreach (var pattern in _prefixPatterns)
        {
            var m = pattern.Match(input);
            if (m.Success)
            {
                matches.Add((pattern, m));
            }
        }

        if (matches.Count > 0)
        {
            return new Match(input, matches, _groupNamesToVarNames, _unescapeFuncs);
        }

        // If nothing found, try patterns that accept trailing characters
        foreach (var pattern in _prefixWithTrailingPatterns)
        {
            var m = pattern.Match(input);
            if (m.Success)
            {
                matches.Add((pattern, m));
            }
        }

        if (matches.Count > 0)
        {
            return new Match(input, matches, _groupNamesToVarNames, _unescapeFuncs);
        }

        return null;
    }

    /// <summary>
    /// Escape a value for a variable according to registered escape functions.
    /// Used when inserting completion text back into the input.
    /// </summary>
    /// <param name="varname">Variable name.</param>
    /// <param name="value">Value to escape.</param>
    /// <returns>
    /// The escaped value if an escape function is registered for this variable,
    /// otherwise the original value unchanged.
    /// </returns>
    public string Escape(string varname, string value)
    {
        if (_escapeFuncs.TryGetValue(varname, out var func))
        {
            return func(value);
        }

        return value;
    }

    /// <summary>
    /// Unescape a value for a variable according to registered unescape functions.
    /// Used when extracting variable values for validation or completion.
    /// </summary>
    /// <param name="varname">Variable name.</param>
    /// <param name="value">Value to unescape.</param>
    /// <returns>
    /// The unescaped value if an unescape function is registered for this variable,
    /// otherwise the original value unchanged.
    /// </returns>
    public string Unescape(string varname, string value)
    {
        if (_unescapeFuncs.TryGetValue(varname, out var func))
        {
            return func(value);
        }

        return value;
    }

    #region Pattern Transformation

    /// <summary>
    /// Turn a Node into a regular expression pattern.
    /// </summary>
    private static string Transform(Node node, Func<Variable, string> createGroupFunc)
    {
        return node switch
        {
            AnyNode anyNode => $"(?:{string.Join("|", anyNode.Children.Select(c => Transform(c, createGroupFunc)))})",
            NodeSequence seq => string.Join("", seq.Children.Select(c => Transform(c, createGroupFunc))),
            RegexNode regex => regex.Pattern,
            Lookahead lookahead => lookahead.Negative
                ? $"(?!{Transform(lookahead.ChildNode, createGroupFunc)})"
                : throw new NotSupportedException("Positive lookahead not yet supported."),
            Variable variable => $"(?<{createGroupFunc(variable)}>{Transform(variable.ChildNode, createGroupFunc)})",
            Repeat repeat => TransformRepeat(repeat, createGroupFunc),
            _ => throw new InvalidOperationException($"Unknown node type: {node.GetType()}")
        };
    }

    private static string TransformRepeat(Repeat node, Func<Variable, string> createGroupFunc)
    {
        var childPattern = Transform(node.ChildNode, createGroupFunc);
        string repeatSign;

        if (node.MaxRepeat is null)
        {
            repeatSign = node.MinRepeat == 0 ? "*" : node.MinRepeat == 1 ? "+" : $"{{{node.MinRepeat},}}";
        }
        else
        {
            repeatSign = $"{{{node.MinRepeat},{node.MaxRepeat}}}";
        }

        var greedyMod = node.Greedy ? "" : "?";
        return $"(?:{childPattern}){repeatSign}{greedyMod}";
    }

    /// <summary>
    /// Generate prefix patterns for partial matching.
    /// </summary>
    private static IEnumerable<string> TransformPrefix(Node node, Func<Variable, string> createGroupFunc)
    {
        foreach (var pattern in TransformPrefixInner(node, createGroupFunc))
        {
            yield return $"^(?:{pattern})$";
        }
    }

    private static IEnumerable<string> TransformPrefixInner(Node node, Func<Variable, string> createGroupFunc)
    {
        switch (node)
        {
            case AnyNode anyNode:
                {
                    var childrenWithVariable = new List<Node>();
                    var childrenWithoutVariable = new List<Node>();

                    foreach (var child in anyNode.Children)
                    {
                        if (ContainsVariable(child))
                        {
                            childrenWithVariable.Add(child);
                        }
                        else
                        {
                            childrenWithoutVariable.Add(child);
                        }
                    }

                    foreach (var child in childrenWithVariable)
                    {
                        foreach (var pattern in TransformPrefixInner(child, createGroupFunc))
                        {
                            yield return pattern;
                        }
                    }

                    if (childrenWithoutVariable.Count > 0)
                    {
                        var patterns = childrenWithoutVariable
                            .SelectMany(c => TransformPrefixInner(c, createGroupFunc));
                        yield return string.Join("|", patterns);
                    }

                    break;
                }

            case NodeSequence seq:
                {
                    var complete = seq.Children.Select(c => Transform(c, createGroupFunc)).ToList();
                    var prefixes = seq.Children.Select(c => TransformPrefixInner(c, createGroupFunc).ToList()).ToList();
                    var variableNodes = seq.Children.Select(ContainsVariable).ToList();

                    // Yield patterns for each child containing a variable
                    for (int i = 0; i < seq.Children.Count; i++)
                    {
                        if (variableNodes[i])
                        {
                            foreach (var cStr in prefixes[i])
                            {
                                yield return string.Join("", complete.Take(i)) + cStr;
                            }
                        }
                    }

                    // Merge non-variable nodes into one pattern
                    if (!variableNodes.All(v => v))
                    {
                        var result = new StringBuilder();

                        // Start with complete patterns
                        for (int i = 0; i < seq.Children.Count; i++)
                        {
                            result.Append("(?:");
                            result.Append(complete[i]);
                        }

                        // Add prefix patterns
                        for (int i = seq.Children.Count - 1; i >= 0; i--)
                        {
                            if (variableNodes[i])
                            {
                                result.Append(")");
                            }
                            else
                            {
                                result.Append("|(?:");
                                // If this yields multiple, we should yield all combinations
                                if (prefixes[i].Count > 0)
                                {
                                    result.Append(prefixes[i][0]);
                                }
                                result.Append("))");
                            }
                        }

                        yield return result.ToString();
                    }

                    break;
                }

            case RegexNode regex:
                yield return $"(?:{regex.Pattern})?";
                break;

            case Lookahead lookahead:
                if (lookahead.Negative)
                {
                    yield return $"(?!{Transform(lookahead.ChildNode, createGroupFunc)})";
                }
                else
                {
                    throw new NotSupportedException("Positive lookahead not yet supported.");
                }
                break;

            case Variable variable:
                foreach (var cStr in TransformPrefixInner(variable.ChildNode, createGroupFunc))
                {
                    yield return $"(?<{createGroupFunc(variable)}>{cStr})";
                }
                break;

            case Repeat repeat:
                {
                    var prefix = Transform(repeat.ChildNode, createGroupFunc);

                    if (repeat.MaxRepeat == 1)
                    {
                        foreach (var pattern in TransformPrefixInner(repeat.ChildNode, createGroupFunc))
                        {
                            yield return pattern;
                        }
                    }
                    else
                    {
                        foreach (var cStr in TransformPrefixInner(repeat.ChildNode, createGroupFunc))
                        {
                            string repeatSign = repeat.MaxRepeat.HasValue
                                ? $"{{,{repeat.MaxRepeat.Value - 1}}}"
                                : "*";
                            var greedyMod = repeat.Greedy ? "" : "?";
                            yield return $"(?:{prefix}){repeatSign}{greedyMod}{cStr}";
                        }
                    }

                    break;
                }

            default:
                throw new InvalidOperationException($"Unknown node type: {node.GetType()}");
        }
    }

    private static bool ContainsVariable(Node node)
    {
        return node switch
        {
            RegexNode => false,
            Variable => true,
            Lookahead lookahead => ContainsVariable(lookahead.ChildNode),
            Repeat repeat => ContainsVariable(repeat.ChildNode),
            NodeSequence seq => seq.Children.Any(ContainsVariable),
            AnyNode anyNode => anyNode.Children.Any(ContainsVariable),
            _ => false
        };
    }

    #endregion
}
