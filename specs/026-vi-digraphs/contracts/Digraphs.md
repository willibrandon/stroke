# API Contract: Digraphs Static Class

**Namespace**: `Stroke.KeyBinding`
**Type**: Static class
**Source**: Port of `prompt_toolkit.key_binding.digraphs.DIGRAPHS`

## Class Definition

```csharp
namespace Stroke.KeyBinding;

/// <summary>
/// Provides Vi digraph mappings for inserting special characters.
/// </summary>
/// <remarks>
/// <para>
/// Digraphs are two-character sequences that map to Unicode code points,
/// following the RFC1345 standard as implemented in Vim/Neovim.
/// </para>
/// <para>
/// This class is thread-safe. The underlying dictionary is immutable
/// and populated at static initialization.
/// </para>
/// <para>
/// Equivalent to Python Prompt Toolkit's <c>DIGRAPHS</c> dictionary from
/// <c>prompt_toolkit.key_binding.digraphs</c>.
/// </para>
/// </remarks>
public static class Digraphs
{
    // Implementation details...
}
```

## Properties

### Map

```csharp
/// <summary>
/// Gets the complete digraph dictionary for enumeration and lookup.
/// </summary>
/// <value>
/// A read-only dictionary mapping character pairs to Unicode code points.
/// Contains 1,300+ entries from RFC1345.
/// </value>
/// <remarks>
/// The returned dictionary is immutable and safe for concurrent access.
/// </remarks>
/// <example>
/// <code>
/// // Enumerate all digraphs
/// foreach (var kvp in Digraphs.Map)
/// {
///     var (char1, char2) = kvp.Key;
///     var codePoint = kvp.Value;
///     Console.WriteLine($"{char1}{char2} → U+{codePoint:X4}");
/// }
///
/// // Get count
/// int total = Digraphs.Map.Count; // ~1300
/// </code>
/// </example>
public static IReadOnlyDictionary<(char Char1, char Char2), int> Map { get; }
```

## Methods

### Lookup

```csharp
/// <summary>
/// Looks up a digraph and returns its Unicode code point.
/// </summary>
/// <param name="char1">The first character of the digraph.</param>
/// <param name="char2">The second character of the digraph.</param>
/// <returns>
/// The Unicode code point if the digraph exists; otherwise, <c>null</c>.
/// </returns>
/// <remarks>
/// <para>
/// Digraphs are case-sensitive. For example, <c>('a', '*')</c> maps to
/// Greek lowercase alpha (α), while <c>('A', '*')</c> maps to uppercase Alpha (Α).
/// </para>
/// <para>
/// Only the canonical ordering from RFC1345 is recognized.
/// For example, <c>('E', 'u')</c> returns the Euro sign, but <c>('u', 'E')</c>
/// returns <c>null</c>.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Look up Euro sign
/// int? euro = Digraphs.Lookup('E', 'u');
/// // euro == 0x20AC
///
/// // Look up Greek pi
/// int? pi = Digraphs.Lookup('p', '*');
/// // pi == 0x03C0
///
/// // Invalid digraph
/// int? invalid = Digraphs.Lookup('Z', 'Z');
/// // invalid == null
/// </code>
/// </example>
public static int? Lookup(char char1, char char2);
```

### GetString

```csharp
/// <summary>
/// Gets the string representation of a digraph.
/// </summary>
/// <param name="char1">The first character of the digraph.</param>
/// <param name="char2">The second character of the digraph.</param>
/// <returns>
/// A string containing the Unicode character if the digraph exists;
/// otherwise, <c>null</c>.
/// </returns>
/// <remarks>
/// <para>
/// This method correctly handles code points above U+FFFF that require
/// surrogate pairs in UTF-16. The returned string will contain either
/// one or two UTF-16 code units as appropriate.
/// </para>
/// <para>
/// For direct code point access, use <see cref="Lookup"/> instead.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Get Euro sign string
/// string? euro = Digraphs.GetString('E', 'u');
/// // euro == "€"
///
/// // Get Greek pi string
/// string? pi = Digraphs.GetString('p', '*');
/// // pi == "π"
///
/// // Invalid digraph
/// string? invalid = Digraphs.GetString('Z', 'Z');
/// // invalid == null
/// </code>
/// </example>
public static string? GetString(char char1, char char2);
```

## Usage Examples

### Basic Lookup

```csharp
using Stroke.KeyBinding;

// Look up common digraphs
var euro = Digraphs.Lookup('E', 'u');      // 0x20AC (€)
var pi = Digraphs.Lookup('p', '*');        // 0x03C0 (π)
var leftArrow = Digraphs.Lookup('<', '-'); // 0x2190 (←)
var boxH = Digraphs.Lookup('h', 'h');      // 0x2500 (─)

// Get string for insertion
string? euroStr = Digraphs.GetString('E', 'u'); // "€"
```

### Integration with ViState

```csharp
using Stroke.KeyBinding;

public void HandleDigraphCompletion(ViState state, char secondChar)
{
    if (state.WaitingForDigraph && state.DigraphSymbol1 is { Length: 1 } first)
    {
        var result = Digraphs.GetString(first[0], secondChar);
        if (result != null)
        {
            InsertText(result);
        }

        // Reset state
        state.WaitingForDigraph = false;
        state.DigraphSymbol1 = null;
    }
}
```

### Enumerate All Digraphs

```csharp
using Stroke.KeyBinding;

// Find all Greek letter digraphs
var greekLetters = Digraphs.Map
    .Where(kvp => kvp.Value >= 0x0370 && kvp.Value <= 0x03FF)
    .Select(kvp => (kvp.Key, Character: char.ConvertFromUtf32(kvp.Value)));

foreach (var (key, character) in greekLetters)
{
    Console.WriteLine($"Ctrl+K {key.Char1}{key.Char2} → {character}");
}
```

## Implementation Notes

1. **Static Initialization**: The dictionary is populated from hard-coded data at first access. No file I/O or external dependencies.

2. **FrozenDictionary**: Uses `FrozenDictionary<(char, char), int>` for optimal read performance and guaranteed immutability.

3. **Case Sensitivity**: All lookups are case-sensitive per RFC1345. The dictionary keys preserve exact casing from the source.

4. **Null Returns**: Invalid digraphs return `null` rather than throwing exceptions, allowing callers to handle missing mappings gracefully.

5. **Thread Safety**: The class is inherently thread-safe due to immutability. No locks or synchronization required.

## Acceptance Criteria Mapping

| Requirement | Contract Element |
|-------------|------------------|
| FR-001: Static dictionary | `Map` property |
| FR-002: Lookup method returning null | `Lookup` method |
| FR-003: Character for BMP | `GetString` method |
| FR-004: String for all code points | `GetString` method |
| FR-005: Read-only collection | `Map` property |
| FR-006: All Python mappings | Data in dictionary |
| FR-007: Case-sensitive | Dictionary key semantics |
| FR-008: Thread-safe | Immutable implementation |
