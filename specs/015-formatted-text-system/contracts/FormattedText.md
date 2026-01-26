# Contract: FormattedText

**Namespace**: `Stroke.FormattedText`

## Class Definition

```csharp
/// <summary>
/// A list of styled text fragments representing formatted text.
/// </summary>
/// <remarks>
/// <para>
/// This class provides an immutable collection of <see cref="StyleAndTextTuple"/> fragments.
/// It implements <see cref="IReadOnlyList{T}"/> for easy iteration and indexing.
/// </para>
/// <para>
/// This is the canonical representation of formatted text in Stroke.
/// </para>
/// </remarks>
public sealed class FormattedText : IReadOnlyList<StyleAndTextTuple>, IFormattedText, IEquatable<FormattedText>
{
    /// <summary>
    /// Gets the singleton empty <see cref="FormattedText"/> instance.
    /// </summary>
    public static FormattedText Empty { get; }

    /// <summary>
    /// Creates a new <see cref="FormattedText"/> from an enumerable of fragments.
    /// </summary>
    /// <param name="fragments">The styled text fragments.</param>
    public FormattedText(IEnumerable<StyleAndTextTuple> fragments);

    /// <summary>
    /// Creates a new <see cref="FormattedText"/> from an array of fragments.
    /// </summary>
    /// <param name="fragments">The styled text fragments.</param>
    public FormattedText(params StyleAndTextTuple[] fragments);

    /// <summary>
    /// Gets the number of fragments in this formatted text.
    /// </summary>
    public int Count { get; }

    /// <summary>
    /// Gets the fragment at the specified index.
    /// </summary>
    public StyleAndTextTuple this[int index] { get; }

    /// <summary>
    /// Returns an enumerator that iterates through the fragments.
    /// </summary>
    public IEnumerator<StyleAndTextTuple> GetEnumerator();

    /// <summary>
    /// Returns this instance (FormattedText is its own formatted text representation).
    /// </summary>
    IReadOnlyList<StyleAndTextTuple> IFormattedText.ToFormattedText();

    /// <summary>
    /// Implicitly converts a string to a <see cref="FormattedText"/> with no styling.
    /// </summary>
    public static implicit operator FormattedText(string? text);

    /// <summary>
    /// Determines whether this formatted text equals another.
    /// </summary>
    public bool Equals(FormattedText? other);

    public override bool Equals(object? obj);
    public override int GetHashCode();
}
```

## Usage Examples

```csharp
// Empty formatted text
var empty = FormattedText.Empty;

// From fragments
var formatted = new FormattedText(
    ("class:bold", "Hello"),
    ("", " "),
    ("class:italic", "World"));

// From string (implicit conversion)
FormattedText plain = "Plain text";

// Iteration
foreach (var fragment in formatted)
{
    Console.WriteLine($"[{fragment.Style}] {fragment.Text}");
}

// IFormattedText usage
IFormattedText ft = formatted;
var fragments = ft.ToFormattedText(); // Returns same instance
```
