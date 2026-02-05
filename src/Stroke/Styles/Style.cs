using System.Collections.Frozen;
using System.Text.RegularExpressions;

namespace Stroke.Styles;

/// <summary>
/// Comparer for FrozenSet using set equality semantics.
/// </summary>
internal sealed class FrozenSetComparer<T> : IEqualityComparer<FrozenSet<T>>
{
    public static readonly FrozenSetComparer<T> Default = new();

    public bool Equals(FrozenSet<T>? x, FrozenSet<T>? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x is null || y is null) return false;
        return x.SetEquals(y);
    }

    public int GetHashCode(FrozenSet<T> obj)
    {
        // Hash code must be the same for equal sets regardless of order
        var hash = 0;
        foreach (var item in obj)
        {
            hash ^= item?.GetHashCode() ?? 0;
        }
        return hash;
    }
}

/// <summary>
/// Create a Style instance from a list of style rules.
/// </summary>
/// <remarks>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>Style</c> class
/// from <c>prompt_toolkit.styles.style</c>.
/// </para>
/// <para>
/// The style rules is a list of (classnames, style) tuples.
/// The classnames are whitespace-separated class names and the style string
/// is like a Pygments style definition with additions (reverse, blink).
/// </para>
/// <para>
/// Later rules always override previous rules.
/// </para>
/// <example>
/// <code>
/// var style = new Style(new[]
/// {
///     ("title", "#ff0000 bold underline"),
///     ("something-else", "reverse"),
///     ("class1 class2", "reverse"),
/// });
/// </code>
/// </example>
/// <para>
/// This type is thread-safe. The style is immutable after construction;
/// cached computations use thread-safe caching.
/// </para>
/// </remarks>
public sealed class Style : IStyle
{
    private static readonly Regex ClassNamesRegex = new(@"^[a-z0-9.\s_-]*$", RegexOptions.Compiled);

    private readonly IReadOnlyList<(string ClassNames, string StyleDef)> _styleRules;
    private readonly IReadOnlyList<(FrozenSet<string> ClassNames, Attrs Attrs)> _classNamesAndAttrs;

    /// <summary>
    /// Creates a style from a list of rules.
    /// </summary>
    /// <param name="styleRules">List of (classnames, style definition) tuples.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="styleRules"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when any class name contains invalid characters.</exception>
    public Style(IEnumerable<(string ClassNames, string StyleDef)> styleRules)
    {
        ArgumentNullException.ThrowIfNull(styleRules);

        var rules = styleRules.ToList();
        var classNamesAndAttrs = new List<(FrozenSet<string> ClassNames, Attrs Attrs)>();

        // Loop through the rules in the order they were defined.
        // Rules that are defined later get priority.
        foreach (var (classNames, styleDef) in rules)
        {
            if (!ClassNamesRegex.IsMatch(classNames))
            {
                throw new ArgumentException($"Invalid class names: '{classNames}'", nameof(styleRules));
            }

            // The order of the class names doesn't matter.
            // (But the order of rules does matter.)
            var classNamesSet = classNames.ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries).ToFrozenSet();
            var attrs = ParseStyleStr(styleDef);

            classNamesAndAttrs.Add((classNamesSet, attrs));
        }

        _styleRules = rules.AsReadOnly();
        _classNamesAndAttrs = classNamesAndAttrs.AsReadOnly();
    }

    /// <summary>
    /// Create a Style from a dictionary.
    /// </summary>
    /// <param name="styleDict">Dictionary of class names to style definitions.</param>
    /// <param name="priority">Rule priority ordering.</param>
    /// <returns>A new Style instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="styleDict"/> is null.</exception>
    public static Style FromDict(
        IReadOnlyDictionary<string, string> styleDict,
        Priority priority = Priority.DictKeyOrder)
    {
        ArgumentNullException.ThrowIfNull(styleDict);

        var items = styleDict.Select(kvp => (kvp.Key, kvp.Value));

        if (priority == Priority.MostPrecise)
        {
            // Sort by precision: more elements = higher priority (sorted later)
            items = items.OrderBy(item =>
            {
                // Split on '.' and whitespace. Count elements.
                return item.Key.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .Sum(part => part.Split('.').Length);
            });
        }

        return new Style(items);
    }

    /// <inheritdoc/>
    public Attrs GetAttrsForStyleStr(string styleStr, Attrs? @default = null)
    {
        var defaultAttrs = @default ?? DefaultAttrs.Default;
        var listOfAttrs = new List<Attrs> { defaultAttrs };
        var classNames = new HashSet<string>();

        // Apply default styling (rules with empty class names).
        foreach (var (names, attr) in _classNamesAndAttrs)
        {
            if (names.Count == 0)
            {
                listOfAttrs.Add(attr);
            }
        }

        // Go from left to right through the style string. Things on the right
        // take precedence.
        foreach (var part in styleStr.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            // This part represents a class.
            // Do lookup of this class name in the style definition, as well
            // as all class combinations that we have so far.
            if (part.StartsWith("class:", StringComparison.Ordinal))
            {
                // Expand all class names (comma separated list).
                var newClassNames = new List<string>();
                foreach (var p in part[6..].ToLowerInvariant().Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    newClassNames.AddRange(ExpandClassName(p));
                }

                foreach (var newName in newClassNames)
                {
                    // Build a set of all possible class combinations to be applied.
                    var combos = new HashSet<FrozenSet<string>>(FrozenSetComparer<string>.Default)
                    {
                        new[] { newName }.ToFrozenSet()
                    };

                    // Add combinations with existing class names
                    foreach (var count in Enumerable.Range(1, classNames.Count))
                    {
                        foreach (var c2 in GetCombinations(classNames, count))
                        {
                            combos.Add(c2.Append(newName).ToFrozenSet());
                        }
                    }

                    // Apply the styles that match these class names.
                    // Note: We iterate in declaration order (no specificity sorting).
                    // This matches Python Prompt Toolkit behavior where later rules
                    // override earlier rules, allowing custom styles to override defaults.
                    foreach (var (names, attr) in _classNamesAndAttrs)
                    {
                        // Use set equality comparison
                        if (combos.Any(c => c.SetEquals(names)))
                        {
                            listOfAttrs.Add(attr);
                        }
                    }

                    classNames.Add(newName);
                }
            }
            // Process inline style.
            else
            {
                var inlineAttrs = ParseStyleStr(part);
                listOfAttrs.Add(inlineAttrs);
            }
        }

        return MergeAttrs(listOfAttrs);
    }

    /// <inheritdoc/>
    public IReadOnlyList<(string ClassNames, string StyleDef)> StyleRules => _styleRules;

    /// <inheritdoc/>
    public object InvalidationHash => _classNamesAndAttrs.GetHashCode();

    /// <summary>
    /// Split a single class name at the '.' operator, and build a list of classes.
    /// E.g. 'a.b.c' becomes ['a', 'a.b', 'a.b.c']
    /// </summary>
    private static IEnumerable<string> ExpandClassName(string className)
    {
        var parts = className.Split('.');

        for (var i = 1; i <= parts.Length; i++)
        {
            yield return string.Join('.', parts.Take(i)).ToLowerInvariant();
        }
    }

    /// <summary>
    /// Take a style string, e.g. 'bg:red #88ff00 class:title'
    /// and return an Attrs instance.
    /// </summary>
    private static Attrs ParseStyleStr(string styleStr)
    {
        // Start from default Attrs.
        Attrs attrs;
        if (styleStr.Contains("noinherit", StringComparison.Ordinal))
        {
            attrs = DefaultAttrs.Default;
        }
        else
        {
            attrs = DefaultAttrs.Empty;
        }

        // Now update with the given attributes.
        foreach (var part in styleStr.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            attrs = part switch
            {
                "noinherit" => attrs,
                "bold" => attrs with { Bold = true },
                "nobold" => attrs with { Bold = false },
                "italic" => attrs with { Italic = true },
                "noitalic" => attrs with { Italic = false },
                "underline" => attrs with { Underline = true },
                "nounderline" => attrs with { Underline = false },
                "strike" => attrs with { Strike = true },
                "nostrike" => attrs with { Strike = false },
                // prompt_toolkit extensions. Not in Pygments.
                "blink" => attrs with { Blink = true },
                "noblink" => attrs with { Blink = false },
                "reverse" => attrs with { Reverse = true },
                "noreverse" => attrs with { Reverse = false },
                "hidden" => attrs with { Hidden = true },
                "nohidden" => attrs with { Hidden = false },
                "dim" => attrs with { Dim = true },
                "nodim" => attrs with { Dim = false },
                // Pygments properties that we ignore.
                "roman" or "sans" or "mono" => attrs,
                _ when part.StartsWith("border:", StringComparison.Ordinal) => attrs,
                // Ignore pieces in between square brackets. This is internal stuff.
                // Like '[transparent]' or '[set-cursor-position]'.
                _ when part.StartsWith('[') && part.EndsWith(']') => attrs,
                // Colors.
                _ when part.StartsWith("bg:", StringComparison.Ordinal) => attrs with { BgColor = StyleParser.ParseColor(part[3..]) },
                _ when part.StartsWith("fg:", StringComparison.Ordinal) => attrs with { Color = StyleParser.ParseColor(part[3..]) },
                // Default: foreground color
                _ => attrs with { Color = StyleParser.ParseColor(part) }
            };
        }

        return attrs;
    }

    /// <summary>
    /// Take a list of Attrs instances and merge them into one.
    /// Every Attr in the list can override the styling of the previous one. So,
    /// the last one has highest priority.
    /// </summary>
    private static Attrs MergeAttrs(List<Attrs> listOfAttrs)
    {
        // Take first non-null value, starting at the end.
        string? color = null;
        string? bgColor = null;
        bool? bold = null;
        bool? underline = null;
        bool? strike = null;
        bool? italic = null;
        bool? blink = null;
        bool? reverse = null;
        bool? hidden = null;
        bool? dim = null;

        // Iterate in reverse order - later attrs override earlier ones
        for (var i = listOfAttrs.Count - 1; i >= 0; i--)
        {
            var a = listOfAttrs[i];
            color ??= a.Color;
            bgColor ??= a.BgColor;
            bold ??= a.Bold;
            underline ??= a.Underline;
            strike ??= a.Strike;
            italic ??= a.Italic;
            blink ??= a.Blink;
            reverse ??= a.Reverse;
            hidden ??= a.Hidden;
            dim ??= a.Dim;
        }

        return new Attrs(
            Color: color ?? "",
            BgColor: bgColor ?? "",
            Bold: bold ?? false,
            Underline: underline ?? false,
            Strike: strike ?? false,
            Italic: italic ?? false,
            Blink: blink ?? false,
            Reverse: reverse ?? false,
            Hidden: hidden ?? false,
            Dim: dim ?? false);
    }

    /// <summary>
    /// Get all combinations of a given size from a set.
    /// </summary>
    private static IEnumerable<IEnumerable<T>> GetCombinations<T>(IEnumerable<T> source, int count)
    {
        if (count == 0)
        {
            yield return Enumerable.Empty<T>();
            yield break;
        }

        var array = source.ToArray();
        if (array.Length < count)
        {
            yield break;
        }

        for (var i = 0; i <= array.Length - count; i++)
        {
            foreach (var tail in GetCombinations(array.Skip(i + 1), count - 1))
            {
                yield return new[] { array[i] }.Concat(tail);
            }
        }
    }
}
