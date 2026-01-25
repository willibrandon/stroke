namespace Stroke.FormattedText;

/// <summary>
/// A single styled text fragment represented as a (style, text) pair.
/// </summary>
/// <remarks>
/// <para>
/// This is an immutable value type that represents a fragment of text with an associated style.
/// The style is a string that can be interpreted by the rendering layer (e.g., "bold", "italic",
/// "class:completion-menu").
/// </para>
/// <para>
/// This type is part of the minimal FormattedText implementation required by the Completion System.
/// A full FormattedText implementation will be provided in Feature 13.
/// </para>
/// </remarks>
/// <param name="Style">The style class name. Use empty string for unstyled text.</param>
/// <param name="Text">The text content.</param>
public readonly record struct StyleAndTextTuple(string Style, string Text)
{
    /// <summary>
    /// Implicitly converts a value tuple (string, string) to a <see cref="StyleAndTextTuple"/>.
    /// </summary>
    /// <param name="tuple">The value tuple containing (style, text).</param>
    /// <returns>A new StyleAndTextTuple with the given style and text.</returns>
    public static implicit operator StyleAndTextTuple((string Style, string Text) tuple) =>
        new(tuple.Style, tuple.Text);
}
