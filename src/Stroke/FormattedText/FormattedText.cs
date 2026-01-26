using System.Collections;
using System.Collections.Immutable;

namespace Stroke.FormattedText;

/// <summary>
/// A list of styled text fragments representing formatted text.
/// </summary>
/// <remarks>
/// <para>
/// This class provides an immutable collection of <see cref="StyleAndTextTuple"/> fragments.
/// It implements <see cref="IReadOnlyList{T}"/> for easy iteration and indexing, and
/// <see cref="IFormattedText"/> for integration with the formatted text system.
/// </para>
/// <para>
/// Equivalent to Python Prompt Toolkit's <c>FormattedText</c> class which extends
/// <c>StyleAndTextTuples</c> (a list of tuples).
/// </para>
/// </remarks>
public sealed class FormattedText : IReadOnlyList<StyleAndTextTuple>, IEquatable<FormattedText>, IFormattedText
{
    /// <summary>
    /// Gets the singleton empty <see cref="FormattedText"/> instance.
    /// </summary>
    public static FormattedText Empty { get; } = new([]);

    private readonly ImmutableArray<StyleAndTextTuple> _fragments;

    /// <summary>
    /// Creates a new <see cref="FormattedText"/> from an enumerable of fragments.
    /// </summary>
    /// <param name="fragments">The styled text fragments.</param>
    public FormattedText(IEnumerable<StyleAndTextTuple> fragments)
    {
        _fragments = [.. fragments];
    }

    /// <summary>
    /// Creates a new <see cref="FormattedText"/> from an array of fragments.
    /// </summary>
    /// <param name="fragments">The styled text fragments.</param>
    public FormattedText(params StyleAndTextTuple[] fragments)
    {
        _fragments = [.. fragments];
    }

    /// <summary>
    /// Gets the number of fragments in this formatted text.
    /// </summary>
    public int Count => _fragments.Length;

    /// <summary>
    /// Gets the fragment at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the fragment to get.</param>
    /// <returns>The fragment at the specified index.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is less than 0 or greater than or equal to <see cref="Count"/>.
    /// </exception>
    public StyleAndTextTuple this[int index] => _fragments[index];

    /// <summary>
    /// Returns an enumerator that iterates through the fragments.
    /// </summary>
    /// <returns>An enumerator for the fragments.</returns>
    public IEnumerator<StyleAndTextTuple> GetEnumerator() =>
        ((IEnumerable<StyleAndTextTuple>)_fragments).GetEnumerator();

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Determines whether this formatted text equals another.
    /// </summary>
    /// <param name="other">The other formatted text to compare.</param>
    /// <returns>true if the formatted texts have the same fragments; otherwise, false.</returns>
    public bool Equals(FormattedText? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (_fragments.Length != other._fragments.Length) return false;

        for (var i = 0; i < _fragments.Length; i++)
        {
            if (_fragments[i] != other._fragments[i])
                return false;
        }

        return true;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is FormattedText other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var fragment in _fragments)
        {
            hash.Add(fragment);
        }
        return hash.ToHashCode();
    }

    /// <summary>
    /// Implicitly converts a string to a <see cref="FormattedText"/> with no styling.
    /// </summary>
    /// <param name="text">The text to convert.</param>
    /// <returns>
    /// A FormattedText containing a single unstyled fragment, or <see cref="Empty"/> if the string is null or empty.
    /// </returns>
    public static implicit operator FormattedText(string? text) =>
        string.IsNullOrEmpty(text) ? Empty : new([new("", text)]);

    /// <inheritdoc />
    IReadOnlyList<StyleAndTextTuple> IFormattedText.ToFormattedText() => this;
}
