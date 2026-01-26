# Contract: FormattedTextUtils

**Namespace**: `Stroke.FormattedText`

## Class Definition

```csharp
/// <summary>
/// Utility functions for formatted text conversion and manipulation.
/// </summary>
public static class FormattedTextUtils
{
    /// <summary>
    /// Converts an <see cref="AnyFormattedText"/> value to canonical <see cref="FormattedText"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <param name="style">Optional style to prepend to each fragment's existing style.</param>
    /// <param name="autoConvert">If true, non-convertible objects are converted to string first.</param>
    /// <returns>The converted FormattedText.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the value contains an unsupported type and <paramref name="autoConvert"/> is false.
    /// </exception>
    public static FormattedText ToFormattedText(
        AnyFormattedText value,
        string style = "",
        bool autoConvert = false);

    /// <summary>
    /// Checks whether the given value is valid formatted text.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns>
    /// true if the value is a string, list of tuples, <see cref="IFormattedText"/>, or callable;
    /// otherwise, false.
    /// </returns>
    /// <remarks>
    /// For callables, the return type is not validated (only the presence of the callable is checked).
    /// </remarks>
    public static bool IsFormattedText(object? value);

    /// <summary>
    /// Merges multiple formatted text items into a single result.
    /// </summary>
    /// <param name="items">The items to merge.</param>
    /// <returns>A callable that produces the merged FormattedText when invoked.</returns>
    public static Func<AnyFormattedText> Merge(IEnumerable<AnyFormattedText> items);

    /// <summary>
    /// Merges multiple formatted text items into a single result.
    /// </summary>
    /// <param name="items">The items to merge.</param>
    /// <returns>A callable that produces the merged FormattedText when invoked.</returns>
    public static Func<AnyFormattedText> Merge(params AnyFormattedText[] items);

    /// <summary>
    /// Returns the total character count of all fragment text, excluding ZeroWidthEscape fragments.
    /// </summary>
    /// <param name="fragments">The fragments to measure.</param>
    /// <returns>The sum of all fragment text lengths, excluding ZeroWidthEscape.</returns>
    public static int FragmentListLen(IEnumerable<StyleAndTextTuple> fragments);

    /// <summary>
    /// Returns the display width of all fragment text, accounting for wide characters.
    /// </summary>
    /// <param name="fragments">The fragments to measure.</param>
    /// <returns>The total display width, with CJK characters counting as 2.</returns>
    /// <remarks>
    /// ZeroWidthEscape fragments are excluded from the width calculation.
    /// Uses <c>Wcwidth</c> for Unicode character width determination.
    /// </remarks>
    public static int FragmentListWidth(IEnumerable<StyleAndTextTuple> fragments);

    /// <summary>
    /// Concatenates all fragment text into a single string, excluding ZeroWidthEscape fragments.
    /// </summary>
    /// <param name="fragments">The fragments to concatenate.</param>
    /// <returns>A string containing all fragment text joined together.</returns>
    public static string FragmentListToText(IEnumerable<StyleAndTextTuple> fragments);

    /// <summary>
    /// Splits fragment list by newline characters.
    /// </summary>
    /// <param name="fragments">The fragments to split.</param>
    /// <returns>An enumerable of fragment lists, one per line.</returns>
    /// <remarks>
    /// <para>
    /// Always yields at least one line (even for empty input).
    /// </para>
    /// <para>
    /// If input ends with a newline, yields an empty final line
    /// (to distinguish "line\n" from "line").
    /// </para>
    /// </remarks>
    public static IEnumerable<IReadOnlyList<StyleAndTextTuple>> SplitLines(
        IEnumerable<StyleAndTextTuple> fragments);

    /// <summary>
    /// Converts any formatted text to plain text (no styling).
    /// </summary>
    /// <param name="value">The formatted text value.</param>
    /// <returns>The plain text content without style information.</returns>
    public static string ToPlainText(AnyFormattedText value);
}
```

## ZeroWidthEscape Handling

Fragments with `[ZeroWidthEscape]` in their style are special:
- **FragmentListLen**: Excluded from character count
- **FragmentListWidth**: Excluded from width calculation
- **FragmentListToText**: Excluded from text output

This matches Python Prompt Toolkit's behavior where zero-width escapes
are terminal control sequences that don't contribute to visible output.

## Usage Examples

```csharp
// ToFormattedText - from string
FormattedText ft1 = FormattedTextUtils.ToFormattedText("Hello");
// [("", "Hello")]

// ToFormattedText - with style
FormattedText ft2 = FormattedTextUtils.ToFormattedText("Hello", "bold");
// [("bold", "Hello")]

// ToFormattedText - from IFormattedText
FormattedText ft3 = FormattedTextUtils.ToFormattedText(new Html("<b>Hi</b>"));
// [("class:b", "Hi")]

// ToFormattedText - with auto-convert
FormattedText ft4 = FormattedTextUtils.ToFormattedText(42, autoConvert: true);
// [("", "42")]

// IsFormattedText
bool valid1 = FormattedTextUtils.IsFormattedText("Hello"); // true
bool valid2 = FormattedTextUtils.IsFormattedText(new Html("<b>Hi</b>")); // true
bool valid3 = FormattedTextUtils.IsFormattedText(42); // false

// Merge
var merged = FormattedTextUtils.Merge(
    new Html("<b>Hello</b>"),
    " ",
    new Html("<i>World</i>"));
var result = FormattedTextUtils.ToFormattedText(merged());
// [("class:b", "Hello"), ("", " "), ("class:i", "World")]

// FragmentListLen
var fragments = new FormattedText(
    ("", "Hello"),
    ("[ZeroWidthEscape]", "hidden"),
    ("", "World"));
int len = FormattedTextUtils.FragmentListLen(fragments);
// 10 (excludes "hidden")

// FragmentListWidth
var cjk = new FormattedText(("", "日本語")); // 3 chars, 6 display width
int width = FormattedTextUtils.FragmentListWidth(cjk);
// 6

// SplitLines
var multiline = new FormattedText(("class:a", "line1\nline2\nline3"));
var lines = FormattedTextUtils.SplitLines(multiline).ToList();
// [
//   [("class:a", "line1")],
//   [("class:a", "line2")],
//   [("class:a", "line3")]
// ]

// SplitLines - trailing newline
var trailing = new FormattedText(("class:a", "line1\n"));
var trailingLines = FormattedTextUtils.SplitLines(trailing).ToList();
// [
//   [("class:a", "line1")],
//   [("class:a", "")]  // empty final line
// ]

// ToPlainText
string plain = FormattedTextUtils.ToPlainText(new Html("<b>Hello</b> World"));
// "Hello World"
```
