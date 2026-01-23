# Feature 109: Layout Utilities

## Overview

Implement layout utility functions including `ExplodeTextFragments` - a function that converts formatted text fragments into character-by-character fragments for precise rendering and cursor positioning.

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/layout/utils.py`

## Public API

### ExplodeTextFragments

```csharp
namespace Stroke.Layout;

/// <summary>
/// Layout utility functions.
/// </summary>
public static class LayoutUtils
{
    /// <summary>
    /// Turn a list of (style, text) tuples into another list where each string
    /// is exactly one character.
    /// </summary>
    /// <param name="fragments">List of (style, text) tuples.</param>
    /// <returns>Exploded list with one character per fragment.</returns>
    /// <remarks>
    /// It is safe to call this function multiple times. Calling this on a list
    /// that is already exploded is a no-op.
    /// </remarks>
    /// <example>
    /// var fragments = new[] { ("bold", "Hello"), ("", " World") };
    /// var exploded = LayoutUtils.ExplodeTextFragments(fragments);
    /// // Result: [("bold","H"), ("bold","e"), ("bold","l"), ("bold","l"), ("bold","o"),
    /// //          ("", " "), ("", "W"), ("", "o"), ("", "r"), ("", "l"), ("", "d")]
    /// </example>
    public static ExplodedList ExplodeTextFragments(
        IEnumerable<StyleAndTextTuple> fragments);
}
```

### ExplodedList

```csharp
namespace Stroke.Layout;

/// <summary>
/// A list of style/text tuples that has been exploded (one character per tuple).
/// When items are added, they are automatically exploded as well.
/// </summary>
public sealed class ExplodedList : IList<StyleAndTextTuple>
{
    /// <summary>
    /// Indicates this list has been exploded.
    /// </summary>
    public bool Exploded => true;

    /// <summary>
    /// Create an empty exploded list.
    /// </summary>
    public ExplodedList();

    /// <summary>
    /// Create an exploded list from existing fragments.
    /// </summary>
    /// <param name="fragments">Fragments to add (will be exploded).</param>
    public ExplodedList(IEnumerable<StyleAndTextTuple> fragments);

    /// <inheritdoc/>
    public void Add(StyleAndTextTuple item);

    /// <inheritdoc/>
    public void AddRange(IEnumerable<StyleAndTextTuple> items);

    /// <inheritdoc/>
    public StyleAndTextTuple this[int index] { get; set; }

    /// <inheritdoc/>
    public int Count { get; }

    // ... other IList<T> members
}
```

### StyleAndTextTuple

```csharp
namespace Stroke.Layout;

/// <summary>
/// A tuple of style string and text content.
/// </summary>
/// <param name="Style">The style string.</param>
/// <param name="Text">The text content.</param>
public readonly record struct StyleAndTextTuple(string Style, string Text)
{
    /// <summary>
    /// Create from tuple.
    /// </summary>
    public static implicit operator StyleAndTextTuple((string Style, string Text) tuple)
        => new(tuple.Style, tuple.Text);

    /// <summary>
    /// Create a tuple with an optional handler.
    /// </summary>
    public StyleAndTextTuple(string style, string text, MouseHandler? handler)
        : this(style, text)
    {
        Handler = handler;
    }

    /// <summary>
    /// Optional mouse handler for this fragment.
    /// </summary>
    public MouseHandler? Handler { get; init; }
}
```

## Project Structure

```
src/Stroke/
└── Layout/
    ├── LayoutUtils.cs
    ├── ExplodedList.cs
    └── StyleAndTextTuple.cs
tests/Stroke.Tests/
└── Layout/
    └── LayoutUtilsTests.cs
```

## Implementation Notes

### ExplodeTextFragments Implementation

```csharp
public static class LayoutUtils
{
    public static ExplodedList ExplodeTextFragments(
        IEnumerable<StyleAndTextTuple> fragments)
    {
        // If already exploded, return as-is
        if (fragments is ExplodedList exploded)
            return exploded;

        var result = new List<StyleAndTextTuple>();

        foreach (var (style, text, handler) in fragments)
        {
            foreach (char c in text)
            {
                result.Add(new StyleAndTextTuple(style, c.ToString(), handler));
            }
        }

        return new ExplodedList(result, skipExplode: true);
    }
}
```

### ExplodedList Implementation

```csharp
public sealed class ExplodedList : IList<StyleAndTextTuple>
{
    private readonly List<StyleAndTextTuple> _items = new();

    public bool Exploded => true;

    public ExplodedList() { }

    internal ExplodedList(List<StyleAndTextTuple> items, bool skipExplode)
    {
        _items = items;
    }

    public ExplodedList(IEnumerable<StyleAndTextTuple> fragments)
    {
        AddRange(fragments);
    }

    public void Add(StyleAndTextTuple item)
    {
        // Explode on add
        foreach (char c in item.Text)
        {
            _items.Add(new StyleAndTextTuple(item.Style, c.ToString(), item.Handler));
        }
    }

    public void AddRange(IEnumerable<StyleAndTextTuple> items)
    {
        foreach (var item in items)
            Add(item);
    }

    public StyleAndTextTuple this[int index]
    {
        get => _items[index];
        set
        {
            // Explode on set
            if (value.Text.Length == 1)
            {
                _items[index] = value;
            }
            else
            {
                // Replace single item with exploded items
                _items.RemoveAt(index);
                int insertIndex = index;
                foreach (char c in value.Text)
                {
                    _items.Insert(insertIndex++,
                        new StyleAndTextTuple(value.Style, c.ToString(), value.Handler));
                }
            }
        }
    }

    public int Count => _items.Count;

    public void Insert(int index, StyleAndTextTuple item)
        => throw new NotImplementedException("TODO: Implement with explosion");

    public bool IsReadOnly => false;
    public void Clear() => _items.Clear();
    public bool Contains(StyleAndTextTuple item) => _items.Contains(item);
    public void CopyTo(StyleAndTextTuple[] array, int arrayIndex)
        => _items.CopyTo(array, arrayIndex);
    public int IndexOf(StyleAndTextTuple item) => _items.IndexOf(item);
    public bool Remove(StyleAndTextTuple item) => _items.Remove(item);
    public void RemoveAt(int index) => _items.RemoveAt(index);
    public IEnumerator<StyleAndTextTuple> GetEnumerator() => _items.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
```

### Usage Example

```csharp
// Original fragments with multi-character strings
var fragments = new StyleAndTextTuple[]
{
    ("class:keyword", "def"),
    ("", " "),
    ("class:function", "hello"),
    ("", "()")
};

// Explode to one character per fragment
var exploded = LayoutUtils.ExplodeTextFragments(fragments);

// Now each fragment is a single character
// This allows precise cursor positioning and character-by-character rendering
foreach (var (style, ch, _) in exploded)
{
    Console.WriteLine($"'{ch}' with style '{style}'");
}
```

## Dependencies

- Feature 13: Formatted Text

## Implementation Tasks

1. Define StyleAndTextTuple record struct
2. Implement ExplodedList with auto-explosion
3. Implement ExplodeTextFragments static method
4. Handle idempotent explosion (already exploded lists)
5. Write unit tests

## Acceptance Criteria

- [ ] ExplodeTextFragments splits multi-char strings to single chars
- [ ] ExplodedList maintains explosion on add/set
- [ ] Already-exploded lists are returned as-is
- [ ] Style and handler are preserved during explosion
- [ ] Unit tests achieve 80% coverage
