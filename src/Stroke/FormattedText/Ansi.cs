using System.Collections.Immutable;
using System.Text;

namespace Stroke.FormattedText;

/// <summary>
/// Parses ANSI escape sequences into formatted text.
/// </summary>
/// <remarks>
/// <para>
/// Parses ANSI/VT100 escape sequences and converts them to styled text fragments.
/// Supports SGR (Select Graphic Rendition) codes for colors and text attributes.
/// </para>
/// <para>
/// Text between <c>\001</c> and <c>\002</c> is treated as a zero-width escape
/// (used for terminal control sequences that shouldn't affect display width).
/// </para>
/// <para>
/// Equivalent to Python Prompt Toolkit's <c>ANSI</c> class.
/// </para>
/// </remarks>
public sealed class Ansi : IFormattedText
{
    private const char Esc = '\x1b';
    private const char Csi = '\x9b'; // 8-bit CSI alternative
    private const char ZeroWidthStart = '\x01';
    private const char ZeroWidthEnd = '\x02';
    private const string ZeroWidthEscapeStyle = "[ZeroWidthEscape]";

    private readonly ImmutableArray<StyleAndTextTuple> _fragments;

    // Style state
    private string? _color;
    private string? _bgcolor;
    private bool _bold;
    private bool _dim;
    private bool _underline;
    private bool _strike;
    private bool _italic;
    private bool _blink;
    private bool _reverse;
    private bool _hidden;

    /// <summary>
    /// Gets the original ANSI input string.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Creates a new <see cref="Ansi"/> instance by parsing the given string.
    /// </summary>
    /// <param name="value">The ANSI-escaped string to parse.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public Ansi(string value)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));
        _fragments = Parse(value);
    }

    /// <summary>
    /// Returns the parsed formatted text fragments.
    /// </summary>
    public IReadOnlyList<StyleAndTextTuple> ToFormattedText() => _fragments;

    /// <summary>
    /// Creates a new <see cref="Ansi"/> with format arguments escaped.
    /// </summary>
    /// <param name="args">Format arguments (escape sequences will be neutralized).</param>
    /// <returns>A new Ansi instance with substituted values.</returns>
    /// <remarks>
    /// Escape characters (\x1b) and backspaces (\b) in arguments are replaced with '?'.
    /// </remarks>
    public Ansi Format(params object[] args) =>
        new(AnsiFormatter.Format(Value, args));

    /// <summary>
    /// Creates a new <see cref="Ansi"/> with format arguments escaped (named parameters).
    /// </summary>
    /// <param name="args">Named format arguments (escape sequences will be neutralized).</param>
    /// <returns>A new Ansi instance with substituted values.</returns>
    public Ansi Format(IDictionary<string, object> args) =>
        new(AnsiFormatter.Format(Value, args));

    /// <summary>
    /// Neutralizes ANSI escape sequences in a string.
    /// </summary>
    /// <param name="text">The text to escape.</param>
    /// <returns>The text with \x1b and \b replaced with '?'.</returns>
    public static string Escape(object? text) => AnsiFormatter.Escape(text);

    /// <inheritdoc />
    public override string ToString() => $"Ansi({Value})";

    private ImmutableArray<StyleAndTextTuple> Parse(string value)
    {
        var result = ImmutableArray.CreateBuilder<StyleAndTextTuple>();
        var style = string.Empty;
        int i = 0;

        while (i < value.Length)
        {
            char c = value[i];

            // Zero-width escape: \001...\002
            if (c == ZeroWidthStart)
            {
                i++;
                var escaped = new StringBuilder();
                while (i < value.Length && value[i] != ZeroWidthEnd)
                {
                    escaped.Append(value[i]);
                    i++;
                }
                if (i < value.Length && value[i] == ZeroWidthEnd)
                {
                    result.Add(new StyleAndTextTuple(ZeroWidthEscapeStyle, escaped.ToString()));
                    i++;
                }
                continue;
            }

            // CSI sequence: \x1b[ or \x9b
            bool isCsi = false;
            if (c == Esc && i + 1 < value.Length && value[i + 1] == '[')
            {
                isCsi = true;
                i += 2;
            }
            else if (c == Csi)
            {
                isCsi = true;
                i++;
            }

            if (isCsi)
            {
                var parameters = new List<int>();
                var currentNum = new StringBuilder();

                while (i < value.Length)
                {
                    char ch = value[i];

                    if (char.IsDigit(ch))
                    {
                        currentNum.Append(ch);
                        i++;
                    }
                    else
                    {
                        // Parse current number
                        int num = 0;
                        if (currentNum.Length > 0)
                        {
                            if (int.TryParse(currentNum.ToString(), out int parsed))
                                num = Math.Min(parsed, 9999);
                            currentNum.Clear();
                        }
                        parameters.Add(num);

                        if (ch == ';')
                        {
                            i++;
                            continue;
                        }
                        else if (ch == 'm')
                        {
                            // SGR sequence
                            SelectGraphicRendition(parameters);
                            style = CreateStyleString();
                            i++;
                            break;
                        }
                        else if (ch == 'C')
                        {
                            // Cursor forward: add N spaces with current style
                            int count = parameters.Count > 0 ? parameters[0] : 0;
                            for (int j = 0; j < count; j++)
                            {
                                result.Add(new StyleAndTextTuple(style, " "));
                            }
                            i++;
                            break;
                        }
                        else
                        {
                            // Unsupported sequence, discard
                            i++;
                            break;
                        }
                    }
                }
                continue;
            }

            // Regular character
            result.Add(new StyleAndTextTuple(style, c.ToString()));
            i++;
        }

        return result.ToImmutable();
    }

    private void SelectGraphicRendition(List<int> attrs)
    {
        if (attrs.Count == 0)
        {
            ResetAll();
            return;
        }

        // Process attributes in forward order using an index
        // Python uses attrs[::-1] then pop() which processes from the front
        int i = 0;

        while (i < attrs.Count)
        {
            int attr = attrs[i++];

            if (AnsiColors.ForegroundColors.TryGetValue(attr, out var fgColor))
            {
                _color = fgColor;
            }
            else if (AnsiColors.BackgroundColors.TryGetValue(attr, out var bgColor))
            {
                _bgcolor = bgColor;
            }
            else
            {
                switch (attr)
                {
                    case 0:
                        ResetAll();
                        break;
                    case 1:
                        _bold = true;
                        break;
                    case 2:
                        _dim = true;
                        break;
                    case 3:
                        _italic = true;
                        break;
                    case 4:
                        _underline = true;
                        break;
                    case 5:
                    case 6:
                        _blink = true;
                        break;
                    case 7:
                        _reverse = true;
                        break;
                    case 8:
                        _hidden = true;
                        break;
                    case 9:
                        _strike = true;
                        break;
                    case 22:
                        _bold = false;
                        _dim = false;
                        break;
                    case 23:
                        _italic = false;
                        break;
                    case 24:
                        _underline = false;
                        break;
                    case 25:
                        _blink = false;
                        break;
                    case 27:
                        _reverse = false;
                        break;
                    case 28:
                        _hidden = false;
                        break;
                    case 29:
                        _strike = false;
                        break;
                    case 38 when i < attrs.Count:
                        {
                            int subType = attrs[i++];
                            if (subType == 5 && i < attrs.Count)
                            {
                                // 256 color mode
                                int colorIndex = Math.Clamp(attrs[i++], 0, 255);
                                _color = AnsiColors.Get256Color(colorIndex);
                            }
                            else if (subType == 2 && i + 2 < attrs.Count)
                            {
                                // True color mode
                                int r = Math.Clamp(attrs[i++], 0, 255);
                                int g = Math.Clamp(attrs[i++], 0, 255);
                                int b = Math.Clamp(attrs[i++], 0, 255);
                                _color = $"#{r:x2}{g:x2}{b:x2}";
                            }
                        }
                        break;
                    case 48 when i < attrs.Count:
                        {
                            int subType = attrs[i++];
                            if (subType == 5 && i < attrs.Count)
                            {
                                // 256 color mode
                                int colorIndex = Math.Clamp(attrs[i++], 0, 255);
                                _bgcolor = AnsiColors.Get256Color(colorIndex);
                            }
                            else if (subType == 2 && i + 2 < attrs.Count)
                            {
                                // True color mode
                                int r = Math.Clamp(attrs[i++], 0, 255);
                                int g = Math.Clamp(attrs[i++], 0, 255);
                                int b = Math.Clamp(attrs[i++], 0, 255);
                                _bgcolor = $"#{r:x2}{g:x2}{b:x2}";
                            }
                        }
                        break;
                }
            }
        }
    }

    private void ResetAll()
    {
        _color = null;
        _bgcolor = null;
        _bold = false;
        _dim = false;
        _underline = false;
        _strike = false;
        _italic = false;
        _blink = false;
        _reverse = false;
        _hidden = false;
    }

    private string CreateStyleString()
    {
        var parts = new List<string>();

        if (_color != null)
            parts.Add(_color);
        if (_bgcolor != null)
            parts.Add("bg:" + _bgcolor);
        if (_bold)
            parts.Add("bold");
        if (_dim)
            parts.Add("dim");
        if (_underline)
            parts.Add("underline");
        if (_strike)
            parts.Add("strike");
        if (_italic)
            parts.Add("italic");
        if (_blink)
            parts.Add("blink");
        if (_reverse)
            parts.Add("reverse");
        if (_hidden)
            parts.Add("hidden");

        return string.Join(" ", parts);
    }
}
