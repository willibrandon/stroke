# Feature 85: Formatted Text Utilities

## Overview

Implement utility functions for manipulating formatted text including converting to plain text, measuring length/width, and splitting by lines.

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/formatted_text/utils.py`

## Public API

### Utility Functions

```csharp
namespace Stroke.FormattedText;

/// <summary>
/// Utilities for manipulating formatted text fragments.
/// </summary>
public static class FormattedTextUtils
{
    /// <summary>
    /// Convert formatted text to plain text (stripping styles).
    /// </summary>
    /// <param name="value">Any formatted text.</param>
    /// <returns>Plain text string.</returns>
    public static string ToPlainText(IFormattedText value);

    /// <summary>
    /// Get the number of characters in formatted text fragments.
    /// Excludes zero-width escape sequences.
    /// </summary>
    /// <param name="fragments">List of style/text tuples.</param>
    /// <returns>Character count.</returns>
    public static int FragmentListLength(IReadOnlyList<(string Style, string Text)> fragments);

    /// <summary>
    /// Get the display width of formatted text fragments.
    /// Takes double-width characters (CJK) into account.
    /// </summary>
    /// <param name="fragments">List of style/text tuples.</param>
    /// <returns>Display width in columns.</returns>
    public static int FragmentListWidth(IReadOnlyList<(string Style, string Text)> fragments);

    /// <summary>
    /// Concatenate all text parts from fragments.
    /// </summary>
    /// <param name="fragments">List of style/text tuples.</param>
    /// <returns>Concatenated text.</returns>
    public static string FragmentListToText(IReadOnlyList<(string Style, string Text)> fragments);

    /// <summary>
    /// Split formatted text into lines (on newline characters).
    /// </summary>
    /// <param name="fragments">Formatted text fragments.</param>
    /// <returns>Sequence of fragment lists, one per line.</returns>
    public static IEnumerable<IReadOnlyList<(string Style, string Text)>> SplitLines(
        IEnumerable<(string Style, string Text)> fragments);
}
```

### Template

```csharp
namespace Stroke.FormattedText;

/// <summary>
/// Template for string interpolation with formatted text.
/// </summary>
public sealed class Template
{
    /// <summary>
    /// The template text with {} placeholders.
    /// </summary>
    public string Text { get; }

    /// <summary>
    /// Create a template.
    /// </summary>
    /// <param name="text">Template text with {} placeholders.</param>
    public Template(string text);

    /// <summary>
    /// Format the template with the given values.
    /// </summary>
    /// <param name="values">Values to substitute.</param>
    /// <returns>Formatted text result.</returns>
    public IFormattedText Format(params IFormattedText[] values);
}
```

### Merge Function

```csharp
namespace Stroke.FormattedText;

public static class FormattedTextExtensions
{
    /// <summary>
    /// Merge multiple pieces of formatted text together.
    /// </summary>
    /// <param name="items">Items to merge.</param>
    /// <returns>Combined formatted text.</returns>
    public static IFormattedText Merge(IEnumerable<IFormattedText> items);
}
```

## Project Structure

```
src/Stroke/
└── FormattedText/
    ├── FormattedTextUtils.cs
    ├── Template.cs
    └── FormattedTextExtensions.cs
tests/Stroke.Tests/
└── FormattedText/
    └── FormattedTextUtilsTests.cs
```

## Implementation Notes

### ToPlainText Implementation

```csharp
public static string ToPlainText(IFormattedText value)
{
    var fragments = value.GetFormattedText();
    return FragmentListToText(fragments);
}
```

### FragmentListLength Implementation

```csharp
public static int FragmentListLength(
    IReadOnlyList<(string Style, string Text)> fragments)
{
    const string ZeroWidthEscape = "[ZeroWidthEscape]";
    return fragments
        .Where(f => !f.Style.Contains(ZeroWidthEscape))
        .Sum(f => f.Text.Length);
}
```

### FragmentListWidth Implementation

```csharp
public static int FragmentListWidth(
    IReadOnlyList<(string Style, string Text)> fragments)
{
    const string ZeroWidthEscape = "[ZeroWidthEscape]";
    var width = 0;

    foreach (var (style, text) in fragments)
    {
        if (style.Contains(ZeroWidthEscape))
            continue;

        foreach (var c in text)
            width += UnicodeWidth.GetWidth(c);
    }

    return width;
}
```

### SplitLines Implementation

```csharp
public static IEnumerable<IReadOnlyList<(string Style, string Text)>> SplitLines(
    IEnumerable<(string Style, string Text)> fragments)
{
    var line = new List<(string Style, string Text)>();

    foreach (var (style, text) in fragments)
    {
        var parts = text.Split('\n');

        for (var i = 0; i < parts.Length - 1; i++)
        {
            line.Add((style, parts[i]));
            yield return line.ToList();
            line.Clear();
        }

        line.Add((style, parts[^1]));
    }

    // Always yield the last line
    yield return line.ToList();
}
```

### Template Implementation

```csharp
public sealed class Template
{
    public string Text { get; }

    public Template(string text)
    {
        if (text.Contains("{0}"))
            throw new ArgumentException("Use {} instead of {0}");
        Text = text;
    }

    public IFormattedText Format(params IFormattedText[] values)
    {
        var parts = Text.Split("{}");
        if (parts.Length - 1 != values.Length)
            throw new ArgumentException("Value count mismatch");

        var result = new List<(string Style, string Text)>();

        for (var i = 0; i < values.Length; i++)
        {
            result.Add(("", parts[i]));
            result.AddRange(values[i].GetFormattedText());
        }
        result.Add(("", parts[^1]));

        return new FormattedText(result);
    }
}
```

## Dependencies

- Feature 8: FormattedText base types
- Feature 6: UnicodeWidth

## Implementation Tasks

1. Implement `ToPlainText` function
2. Implement `FragmentListLength` function
3. Implement `FragmentListWidth` function
4. Implement `FragmentListToText` function
5. Implement `SplitLines` function
6. Implement `Template` class
7. Implement `Merge` extension method
8. Write unit tests

## Acceptance Criteria

- [ ] ToPlainText strips all styles
- [ ] FragmentListLength counts characters correctly
- [ ] FragmentListWidth handles CJK characters
- [ ] Zero-width escapes are excluded from counts
- [ ] SplitLines yields correct line fragments
- [ ] Template.Format substitutes values correctly
- [ ] Merge combines multiple formatted texts
- [ ] Unit tests achieve 80% coverage
