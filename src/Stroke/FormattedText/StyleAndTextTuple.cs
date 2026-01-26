using Stroke.Input;
using Stroke.KeyBinding;

namespace Stroke.FormattedText;

/// <summary>
/// A single styled text fragment represented as a (style, text) pair with optional mouse handler.
/// </summary>
/// <remarks>
/// <para>
/// This is an immutable value type that represents a fragment of text with an associated style.
/// The style is a string that can be interpreted by the rendering layer (e.g., "bold", "italic",
/// "class:completion-menu", "fg:red bg:blue").
/// </para>
/// <para>
/// The optional mouse handler allows interactive elements to respond to mouse events.
/// </para>
/// </remarks>
/// <param name="Style">The style class name. Use empty string for unstyled text.</param>
/// <param name="Text">The text content.</param>
/// <param name="MouseHandler">Optional callback for mouse events.</param>
public readonly record struct StyleAndTextTuple(
    string Style,
    string Text,
    Func<MouseEvent, NotImplementedOrNone>? MouseHandler = null)
{
    /// <summary>
    /// Creates a StyleAndTextTuple without a mouse handler.
    /// </summary>
    /// <param name="style">The style class name.</param>
    /// <param name="text">The text content.</param>
    public StyleAndTextTuple(string style, string text) : this(style, text, null)
    {
    }

    /// <summary>
    /// Implicitly converts a value tuple (string, string) to a <see cref="StyleAndTextTuple"/>.
    /// </summary>
    /// <param name="tuple">The value tuple containing (style, text).</param>
    /// <returns>A new StyleAndTextTuple with the given style and text.</returns>
    public static implicit operator StyleAndTextTuple((string Style, string Text) tuple) =>
        new(tuple.Style, tuple.Text);

    /// <summary>
    /// Implicitly converts a value tuple (string, string, handler) to a <see cref="StyleAndTextTuple"/>.
    /// </summary>
    /// <param name="tuple">The value tuple containing (style, text, handler).</param>
    /// <returns>A new StyleAndTextTuple with the given style, text, and mouse handler.</returns>
    public static implicit operator StyleAndTextTuple(
        (string Style, string Text, Func<MouseEvent, NotImplementedOrNone>? Handler) tuple) =>
        new(tuple.Style, tuple.Text, tuple.Handler);
}
