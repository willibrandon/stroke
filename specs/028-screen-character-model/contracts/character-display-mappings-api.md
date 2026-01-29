# API Contract: CharacterDisplayMappings

**Namespace**: `Stroke.Layout`
**Type**: `static class`
**Thread Safety**: Immutable (inherently thread-safe)

## Class Definition

```csharp
namespace Stroke.Layout;

/// <summary>
/// Provides mappings from control characters to their display representations.
/// </summary>
/// <remarks>
/// <para>
/// Contains 66 mappings covering:
/// <list type="bullet">
/// <item>C0 control characters 0x00-0x1F (32 entries) → caret notation (^@, ^A, ...)</item>
/// <item>Delete character 0x7F (1 entry) → ^?</item>
/// <item>C1 control characters 0x80-0x9F (32 entries) → hex notation (&lt;80&gt;, ...)</item>
/// <item>Non-breaking space 0xA0 (1 entry) → single space</item>
/// </list>
/// Total: 32 + 1 + 32 + 1 = 66 mappings.
/// </para>
/// <para>
/// Port of Python Prompt Toolkit's <c>Char.display_mappings</c> class attribute
/// from <c>layout/screen.py</c>.
/// </para>
/// </remarks>
public static class CharacterDisplayMappings
```

## Properties

```csharp
/// <summary>
/// Gets the complete mapping of characters to their display representations.
/// </summary>
/// <remarks>
/// Returns a <see cref="FrozenDictionary{TKey, TValue}"/> for O(1) lookup performance.
/// The dictionary is created once at static initialization and is immutable.
/// </remarks>
public static FrozenDictionary<char, string> Mappings { get; }
```

## Static Methods

```csharp
/// <summary>
/// Attempts to get the display representation for a character.
/// </summary>
/// <param name="c">The character to look up.</param>
/// <param name="display">When this method returns, contains the display string if found.</param>
/// <returns><c>true</c> if the character has a display mapping; otherwise, <c>false</c>.</returns>
public static bool TryGetDisplay(char c, out string display)

/// <summary>
/// Gets the display representation for a character, or returns the character itself.
/// </summary>
/// <param name="c">The character to look up.</param>
/// <returns>The display representation if mapped; otherwise, the character as a string.</returns>
public static string GetDisplayOrDefault(char c)

/// <summary>
/// Determines whether a character has a display mapping.
/// </summary>
/// <param name="c">The character to check.</param>
/// <returns><c>true</c> if the character is a control character with a mapping.</returns>
public static bool IsControlCharacter(char c)

/// <summary>
/// Determines whether a character is the non-breaking space (0xA0).
/// </summary>
/// <param name="c">The character to check.</param>
/// <returns><c>true</c> if the character is non-breaking space.</returns>
public static bool IsNonBreakingSpace(char c)
```

## Mapping Table

| Range | Count | Format | Example |
|-------|-------|--------|---------|
| 0x00-0x1F | 32 | Caret notation | 0x01 → "^A" |
| 0x7F | 1 | Caret notation | 0x7F → "^?" |
| 0x80-0x9F | 32 | Hex notation | 0x80 → "&lt;80&gt;" |
| 0xA0 | 1 | Space | 0xA0 → " " |

## Usage Examples

```csharp
// Check if character has mapping
if (CharacterDisplayMappings.TryGetDisplay('\x01', out var display))
{
    // display == "^A"
}

// Get display or original
var result = CharacterDisplayMappings.GetDisplayOrDefault('A');
// result == "A" (no mapping, returns original)

// Check categories
CharacterDisplayMappings.IsControlCharacter('\x1B'); // true (Escape)
CharacterDisplayMappings.IsNonBreakingSpace('\xA0'); // true

// Access full mapping
foreach (var (ch, disp) in CharacterDisplayMappings.Mappings)
{
    Console.WriteLine($"0x{(int)ch:X2} → {disp}");
}
```
