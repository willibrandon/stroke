using Stroke.Core;
using Stroke.FormattedText;

namespace Stroke.Layout.Processors;

/// <summary>
/// Render tabs as visible, column-aligned sequences.
/// </summary>
/// <remarks>
/// <para>
/// Port of Python Prompt Toolkit's <c>TabsProcessor</c> class from
/// <c>prompt_toolkit.layout.processors</c>.
/// </para>
/// <para>
/// Constructor parameter types use <c>object</c> to accept both <c>int</c>/<c>string</c>
/// and <c>Func&lt;int&gt;</c>/<c>Func&lt;string&gt;</c>, matching Python's duck typing.
/// <c>ConversionUtils.ToInt</c> and <c>ConversionUtils.ToStr</c> handle resolution.
/// </para>
/// </remarks>
public sealed class TabsProcessor : IProcessor
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TabsProcessor"/> class.
    /// </summary>
    /// <param name="tabstop">Tab stop width. int or Func&lt;int&gt;. Default 4.</param>
    /// <param name="char1">First tab character. string or Func&lt;string&gt;. Default "|".</param>
    /// <param name="char2">Remaining tab characters. string or Func&lt;string&gt;. Default "\u2508".</param>
    /// <param name="style">Style for tab characters. Default "class:tab".</param>
    public TabsProcessor(
        object? tabstop = null,
        object? char1 = null,
        object? char2 = null,
        string style = "class:tab")
    {
        TabStop = tabstop ?? 4;
        Char1 = char1 ?? "|";
        Char2 = char2 ?? "\u2508";
        Style = style;
    }

    /// <summary>Tab stop width (int or callable).</summary>
    public object TabStop { get; }

    /// <summary>First tab character (string or callable).</summary>
    public object Char1 { get; }

    /// <summary>Second tab character (string or callable).</summary>
    public object Char2 { get; }

    /// <summary>Style for tab characters.</summary>
    public string Style { get; }

    /// <inheritdoc/>
    public Transformation ApplyTransformation(TransformationInput ti)
    {
        var tabstop = ConversionUtils.ToInt(TabStop);
        var style = Style;

        // Create separator for tabs.
        var separator1 = ConversionUtils.ToStr(Char1);
        var separator2 = ConversionUtils.ToStr(Char2);

        // Transform fragments.
        var fragments = LayoutUtils.ExplodeTextFragments(ti.Fragments);

        var positionMappings = new Dictionary<int, int>();
        var resultFragments = new List<StyleAndTextTuple>();
        var pos = 0;

        for (int i = 0; i < fragments.Count; i++)
        {
            positionMappings[i] = pos;

            if (fragments[i].Text == "\t")
            {
                // Calculate how many characters we have to insert.
                var count = tabstop - (pos % tabstop);
                if (count == 0)
                    count = tabstop;

                // Insert tab.
                resultFragments.Add(new StyleAndTextTuple(style, separator1));
                if (count > 1)
                    resultFragments.Add(new StyleAndTextTuple(style, string.Concat(Enumerable.Repeat(separator2, count - 1))));
                pos += count;
            }
            else
            {
                resultFragments.Add(fragments[i]);
                pos += 1;
            }
        }

        positionMappings[fragments.Count] = pos;
        // Add pos+1 to mapping, because the cursor can be right after the
        // line as well.
        positionMappings[fragments.Count + 1] = pos + 1;

        int SourceToDisplay(int fromPosition)
        {
            if (positionMappings.TryGetValue(fromPosition, out var mapped))
                return mapped;
            return fromPosition;
        }

        int DisplayToSource(int displayPos)
        {
            // Build reverse mapping
            var reversed = new Dictionary<int, int>();
            foreach (var (k, v) in positionMappings)
            {
                // First key wins (matching Python's dict comprehension behavior)
                reversed.TryAdd(v, k);
            }

            while (displayPos >= 0)
            {
                if (reversed.TryGetValue(displayPos, out var result))
                    return result;
                displayPos--;
            }
            return 0;
        }

        return new Transformation(
            resultFragments,
            sourceToDisplay: SourceToDisplay,
            displayToSource: DisplayToSource);
    }
}
