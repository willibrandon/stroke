using System.Text.RegularExpressions;

namespace Stroke.FormattedText;

/// <summary>
/// String template for formatted text interpolation.
/// </summary>
/// <remarks>
/// <para>
/// Allows creating templates with <c>{}</c> placeholders that can be filled
/// with formatted text values (strings, HTML, ANSI, etc.).
/// </para>
/// <para>
/// Unlike <see cref="Html.Format(object[])"/> and <see cref="Ansi.Format(object[])"/>, Template
/// preserves the formatting of the interpolated values rather than escaping them.
/// </para>
/// <para>
/// Equivalent to Python Prompt Toolkit's <c>Template</c> class.
/// </para>
/// </remarks>
public sealed class Template
{
    private static readonly Regex PositionalPlaceholderRegex = new(@"\{[0-9]+\}", RegexOptions.Compiled);

    /// <summary>
    /// Gets the template text with <c>{}</c> placeholders.
    /// </summary>
    public string Text { get; }

    /// <summary>
    /// Creates a new <see cref="Template"/> with the given text.
    /// </summary>
    /// <param name="text">The template text containing <c>{}</c> placeholders.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="text"/> is null.</exception>
    /// <exception cref="FormatException">Thrown when <paramref name="text"/> contains <c>{0}</c> (positional placeholders not supported).</exception>
    public Template(string text)
    {
        Text = text ?? throw new ArgumentNullException(nameof(text));

        // Check for positional placeholders like {0}, {1}, etc.
        if (PositionalPlaceholderRegex.IsMatch(text))
        {
            throw new FormatException("Positional placeholders like {0} are not supported. Use {} instead.");
        }
    }

    /// <summary>
    /// Formats the template with the given values.
    /// </summary>
    /// <param name="values">Values to substitute for each <c>{}</c> placeholder.</param>
    /// <returns>A callable that produces the formatted result when invoked.</returns>
    /// <remarks>
    /// <para>
    /// The number of values must match the number of <c>{}</c> placeholders.
    /// </para>
    /// <para>
    /// Returns a <see cref="Func{AnyFormattedText}"/> for lazy evaluation,
    /// matching Python Prompt Toolkit's behavior.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentException">Thrown when the number of values doesn't match the number of placeholders.</exception>
    public Func<AnyFormattedText> Format(params AnyFormattedText[] values)
    {
        // Capture the values for lazy evaluation
        var capturedText = Text;
        var capturedValues = values;

        return () =>
        {
            // Parse template into parts: literal text and placeholders
            // Handle {{ and }} as escaped braces
            var fragments = new List<StyleAndTextTuple>();
            int valueIndex = 0;
            int placeholderCount = 0;
            var currentText = new System.Text.StringBuilder();
            int i = 0;

            // First pass: count placeholders
            int j = 0;
            while (j < capturedText.Length)
            {
                if (j + 1 < capturedText.Length && capturedText[j] == '{' && capturedText[j + 1] == '{')
                {
                    j += 2; // Skip escaped {{
                }
                else if (j + 1 < capturedText.Length && capturedText[j] == '}' && capturedText[j + 1] == '}')
                {
                    j += 2; // Skip escaped }}
                }
                else if (j + 1 < capturedText.Length && capturedText[j] == '{' && capturedText[j + 1] == '}')
                {
                    placeholderCount++;
                    j += 2;
                }
                else
                {
                    j++;
                }
            }

            // Verify placeholder count matches value count
            if (placeholderCount != capturedValues.Length)
            {
                throw new ArgumentException(
                    $"Template has {placeholderCount} placeholder(s) but {capturedValues.Length} value(s) were provided.",
                    nameof(values));
            }

            // Second pass: build fragments
            while (i < capturedText.Length)
            {
                if (i + 1 < capturedText.Length && capturedText[i] == '{' && capturedText[i + 1] == '{')
                {
                    // Escaped {{ -> literal {
                    currentText.Append('{');
                    i += 2;
                }
                else if (i + 1 < capturedText.Length && capturedText[i] == '}' && capturedText[i + 1] == '}')
                {
                    // Escaped }} -> literal }
                    currentText.Append('}');
                    i += 2;
                }
                else if (i + 1 < capturedText.Length && capturedText[i] == '{' && capturedText[i + 1] == '}')
                {
                    // Placeholder {}
                    // Flush current text
                    if (currentText.Length > 0)
                    {
                        fragments.Add(new StyleAndTextTuple("", currentText.ToString()));
                        currentText.Clear();
                    }

                    // Add the value's fragments
                    if (valueIndex < capturedValues.Length)
                    {
                        var valueFragments = FormattedTextUtils.ToFormattedText(capturedValues[valueIndex++]);
                        fragments.AddRange(valueFragments);
                    }
                    i += 2;
                }
                else
                {
                    currentText.Append(capturedText[i]);
                    i++;
                }
            }

            // Flush remaining text
            if (currentText.Length > 0)
            {
                fragments.Add(new StyleAndTextTuple("", currentText.ToString()));
            }

            return new FormattedText(fragments);
        };
    }

    /// <inheritdoc />
    public override string ToString() => $"Template({Text})";
}
