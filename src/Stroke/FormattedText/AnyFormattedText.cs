namespace Stroke.FormattedText;

/// <summary>
/// A union type that can hold a string, <see cref="FormattedText"/>, or a function returning formatted text.
/// </summary>
/// <remarks>
/// <para>
/// This struct provides implicit conversions from common types used for formatted text,
/// allowing flexible API usage. It can be converted to canonical <see cref="FormattedText"/>
/// using <see cref="ToFormattedText"/> or to plain text using <see cref="ToPlainText"/>.
/// </para>
/// </remarks>
public readonly struct AnyFormattedText : IEquatable<AnyFormattedText>
{
    /// <summary>
    /// Gets the default empty instance.
    /// </summary>
    public static AnyFormattedText Empty { get; } = default;

    /// <summary>
    /// Gets the underlying value (string, FormattedText, or Func&lt;AnyFormattedText&gt;).
    /// </summary>
    public object? Value { get; }

    /// <summary>
    /// Gets a value indicating whether this instance is empty.
    /// </summary>
    /// <value>
    /// true if the value is null, an empty string, or an empty FormattedText; otherwise, false.
    /// </value>
    public bool IsEmpty => Value switch
    {
        null => true,
        string s => string.IsNullOrEmpty(s),
        FormattedText { Count: 0 } => true,
        _ => false
    };

    private AnyFormattedText(object? value) => Value = value;

    /// <summary>
    /// Implicitly converts a string to <see cref="AnyFormattedText"/>.
    /// </summary>
    /// <param name="text">The text to convert.</param>
    /// <returns>An AnyFormattedText containing the string.</returns>
    public static implicit operator AnyFormattedText(string? text) => new(text);

    /// <summary>
    /// Implicitly converts a <see cref="FormattedText"/> to <see cref="AnyFormattedText"/>.
    /// </summary>
    /// <param name="text">The formatted text to convert.</param>
    /// <returns>An AnyFormattedText containing the formatted text.</returns>
    public static implicit operator AnyFormattedText(FormattedText? text) => new(text);

    /// <summary>
    /// Implicitly converts a function to <see cref="AnyFormattedText"/>.
    /// </summary>
    /// <param name="func">A function that returns formatted text lazily.</param>
    /// <returns>An AnyFormattedText containing the function.</returns>
    public static implicit operator AnyFormattedText(Func<AnyFormattedText>? func) => new(func);

    /// <summary>
    /// Implicitly converts an <see cref="Html"/> instance to <see cref="AnyFormattedText"/>.
    /// </summary>
    /// <param name="html">The HTML markup to convert.</param>
    /// <returns>An AnyFormattedText containing the HTML instance.</returns>
    public static implicit operator AnyFormattedText(Html? html) => new(html);

    /// <summary>
    /// Implicitly converts an <see cref="Ansi"/> instance to <see cref="AnyFormattedText"/>.
    /// </summary>
    /// <param name="ansi">The ANSI-escaped text to convert.</param>
    /// <returns>An AnyFormattedText containing the ANSI instance.</returns>
    public static implicit operator AnyFormattedText(Ansi? ansi) => new(ansi);

    /// <summary>
    /// Implicitly converts a <see cref="PygmentsTokens"/> instance to <see cref="AnyFormattedText"/>.
    /// </summary>
    /// <param name="tokens">The Pygments tokens to convert.</param>
    /// <returns>An AnyFormattedText containing the PygmentsTokens instance.</returns>
    public static implicit operator AnyFormattedText(PygmentsTokens? tokens) => new(tokens);

    /// <summary>
    /// Converts this value to canonical <see cref="FormattedText"/>.
    /// </summary>
    /// <param name="style">Optional style to apply to string values.</param>
    /// <returns>The formatted text representation of this value.</returns>
    public FormattedText ToFormattedText(string style = "") =>
        FormattedTextUtils.ToFormattedText(this, style);

    /// <summary>
    /// Extracts the plain text content from this value.
    /// </summary>
    /// <returns>The plain text without styling information.</returns>
    public string ToPlainText() =>
        FormattedTextUtils.ToPlainText(this);

    /// <summary>
    /// Determines whether this instance equals another <see cref="AnyFormattedText"/>.
    /// </summary>
    /// <param name="other">The other instance to compare.</param>
    /// <returns>true if the underlying values are equal; otherwise, false.</returns>
    public bool Equals(AnyFormattedText other) => Equals(Value, other.Value);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is AnyFormattedText other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => Value?.GetHashCode() ?? 0;

    /// <summary>
    /// Determines whether two <see cref="AnyFormattedText"/> instances are equal.
    /// </summary>
    public static bool operator ==(AnyFormattedText left, AnyFormattedText right) => left.Equals(right);

    /// <summary>
    /// Determines whether two <see cref="AnyFormattedText"/> instances are not equal.
    /// </summary>
    public static bool operator !=(AnyFormattedText left, AnyFormattedText right) => !left.Equals(right);
}
