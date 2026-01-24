namespace Stroke.AutoSuggest;

/// <summary>
/// Represents a suggestion returned by an auto-suggest implementation.
/// </summary>
/// <remarks>
/// Thread-safe: Immutable record type with no mutable state.
/// </remarks>
public sealed record Suggestion
{
    /// <summary>
    /// Gets the suggested text to insert after the cursor.
    /// </summary>
    public string Text { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Suggestion"/> class.
    /// </summary>
    /// <param name="text">The suggested text. Cannot be null; empty string is valid.</param>
    /// <exception cref="ArgumentNullException">Thrown when text is null.</exception>
    public Suggestion(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        Text = text;
    }

    /// <summary>
    /// Returns a string representation for debugging.
    /// </summary>
    /// <returns>Format: <c>Suggestion({Text})</c> matching Python's <c>__repr__</c>.</returns>
    public override string ToString() => $"Suggestion({Text})";
}
