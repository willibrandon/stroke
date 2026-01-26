using System.Collections.Immutable;
using System.Xml;
using System.Xml.Linq;

namespace Stroke.FormattedText;

/// <summary>
/// Parses HTML-like markup into formatted text.
/// </summary>
/// <remarks>
/// <para>
/// Supports a subset of HTML for styling text:
/// <list type="bullet">
///   <item><c>&lt;b&gt;</c> for bold</item>
///   <item><c>&lt;i&gt;</c> for italic</item>
///   <item><c>&lt;u&gt;</c> for underline</item>
///   <item><c>&lt;s&gt;</c> for strikethrough</item>
///   <item><c>&lt;style fg="color" bg="color"&gt;</c> for colors</item>
///   <item>Any other element becomes a CSS class</item>
/// </list>
/// </para>
/// <para>
/// Equivalent to Python Prompt Toolkit's <c>HTML</c> class.
/// </para>
/// </remarks>
public sealed class Html : IFormattedText
{
    private readonly ImmutableArray<StyleAndTextTuple> _fragments;

    /// <summary>
    /// Gets the original HTML input string.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Creates a new <see cref="Html"/> instance by parsing the given markup.
    /// </summary>
    /// <param name="value">The HTML-like markup to parse.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="FormatException">Thrown when the markup is malformed.</exception>
    public Html(string value)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));
        _fragments = Parse(value);
    }

    /// <summary>
    /// Returns the parsed formatted text fragments.
    /// </summary>
    public IReadOnlyList<StyleAndTextTuple> ToFormattedText() => _fragments;

    /// <summary>
    /// Creates a new <see cref="Html"/> with format arguments escaped.
    /// </summary>
    /// <param name="args">Format arguments (will be HTML-escaped).</param>
    /// <returns>A new Html instance with substituted values.</returns>
    /// <remarks>
    /// Uses positional placeholders: <c>{0}</c>, <c>{1}</c>, etc.
    /// Special characters in arguments are escaped: &amp; &lt; &gt; &quot;
    /// </remarks>
    public Html Format(params object[] args) =>
        new(HtmlFormatter.Format(Value, args));

    /// <summary>
    /// Creates a new <see cref="Html"/> with format arguments escaped (named parameters).
    /// </summary>
    /// <param name="args">Named format arguments (will be HTML-escaped).</param>
    /// <returns>A new Html instance with substituted values.</returns>
    public Html Format(IDictionary<string, object> args) =>
        new(HtmlFormatter.Format(Value, args));

    /// <summary>
    /// Escapes special HTML characters in a string.
    /// </summary>
    /// <param name="text">The text to escape.</param>
    /// <returns>The escaped text with &amp;, &lt;, &gt;, and &quot; replaced.</returns>
    public static string Escape(object? text) => HtmlFormatter.Escape(text);

    /// <inheritdoc />
    public override string ToString() => $"Html({Value})";

    private static ImmutableArray<StyleAndTextTuple> Parse(string value)
    {
        // Wrap in a root element for parsing
        var wrappedXml = $"<html-root>{value}</html-root>";

        XDocument document;
        try
        {
            document = XDocument.Parse(wrappedXml, LoadOptions.PreserveWhitespace);
        }
        catch (XmlException ex)
        {
            throw new FormatException($"Invalid HTML markup: {ex.Message}", ex);
        }

        var result = ImmutableArray.CreateBuilder<StyleAndTextTuple>();
        var nameStack = new List<string>();
        var fgStack = new List<string>();
        var bgStack = new List<string>();

        ProcessNode(document.Root!, result, nameStack, fgStack, bgStack);

        return result.ToImmutable();
    }

    private static void ProcessNode(
        XElement element,
        ImmutableArray<StyleAndTextTuple>.Builder result,
        List<string> nameStack,
        List<string> fgStack,
        List<string> bgStack)
    {
        foreach (var node in element.Nodes())
        {
            if (node is XText textNode)
            {
                var style = GetCurrentStyle(nameStack, fgStack, bgStack);
                // Preserve whitespace - only skip completely empty strings
                if (textNode.Value.Length > 0)
                {
                    result.Add(new StyleAndTextTuple(style, textNode.Value));
                }
            }
            else if (node is XElement childElement)
            {
                var elementName = childElement.Name.LocalName;

                // Special elements that don't add to name stack
                bool addToNameStack = elementName != "html-root" && elementName != "style";
                string fg = string.Empty;
                string bg = string.Empty;

                // Process attributes
                foreach (var attr in childElement.Attributes())
                {
                    var attrName = attr.Name.LocalName;
                    var attrValue = attr.Value.Trim();

                    if (attrName == "fg")
                    {
                        if (attrValue.Contains(' '))
                            throw new FormatException("\"fg\" attribute contains a space.");
                        fg = attrValue;
                    }
                    else if (attrName == "bg")
                    {
                        if (attrValue.Contains(' '))
                            throw new FormatException("\"bg\" attribute contains a space.");
                        bg = attrValue;
                    }
                    else if (attrName == "color")
                    {
                        // Alias for fg
                        if (attrValue.Contains(' '))
                            throw new FormatException("\"color\" attribute contains a space.");
                        fg = attrValue;
                    }
                }

                // Push to stacks
                if (addToNameStack)
                    nameStack.Add(elementName);
                if (!string.IsNullOrEmpty(fg))
                    fgStack.Add(fg);
                if (!string.IsNullOrEmpty(bg))
                    bgStack.Add(bg);

                // Process children recursively
                ProcessNode(childElement, result, nameStack, fgStack, bgStack);

                // Pop from stacks
                if (addToNameStack)
                    nameStack.RemoveAt(nameStack.Count - 1);
                if (!string.IsNullOrEmpty(fg))
                    fgStack.RemoveAt(fgStack.Count - 1);
                if (!string.IsNullOrEmpty(bg))
                    bgStack.RemoveAt(bgStack.Count - 1);
            }
        }
    }

    private static string GetCurrentStyle(List<string> nameStack, List<string> fgStack, List<string> bgStack)
    {
        var parts = new List<string>();

        if (nameStack.Count > 0)
            parts.Add("class:" + string.Join(",", nameStack));

        if (fgStack.Count > 0)
            parts.Add("fg:" + fgStack[^1]);

        if (bgStack.Count > 0)
            parts.Add("bg:" + bgStack[^1]);

        return string.Join(" ", parts);
    }
}
