# Feature 95: Vi Digraphs

## Overview

Implement Vi digraphs - a feature that allows inserting special characters by pressing Control+K followed by two characters. This is based on RFC1345 and matches Vim/Neovim behavior.

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/key_binding/digraphs.py`

## Public API

### Digraphs Dictionary

```csharp
namespace Stroke.KeyBinding;

/// <summary>
/// Vi digraphs for inserting special characters.
/// Press Ctrl+K followed by two characters to insert the corresponding character.
/// Based on RFC1345 and Vim/Neovim implementation.
/// </summary>
public static class Digraphs
{
    /// <summary>
    /// Dictionary mapping two-character sequences to Unicode code points.
    /// The key is a tuple of (char1, char2) and the value is the Unicode code point.
    /// </summary>
    public static IReadOnlyDictionary<(char, char), int> Map { get; }

    /// <summary>
    /// Look up a digraph.
    /// </summary>
    /// <param name="char1">First character.</param>
    /// <param name="char2">Second character.</param>
    /// <returns>Unicode code point, or null if not found.</returns>
    public static int? Lookup(char char1, char char2);

    /// <summary>
    /// Get the character for a digraph.
    /// </summary>
    /// <param name="char1">First character.</param>
    /// <param name="char2">Second character.</param>
    /// <returns>The resulting character, or null if not found.</returns>
    public static char? GetCharacter(char char1, char char2);
}
```

## Project Structure

```
src/Stroke/
└── KeyBinding/
    └── Digraphs.cs
tests/Stroke.Tests/
└── KeyBinding/
    └── DigraphsTests.cs
```

## Implementation Notes

### Common Digraph Examples

```csharp
// Currency symbols
('E', 'u') => 0x20AC,  // € Euro sign
('P', 'd') => 0x00A3,  // £ Pound sign
('Y', 'e') => 0x00A5,  // ¥ Yen sign
('=', 'e') => 0x20AC,  // € Euro sign (alternative)
('=', 'R') => 0x20BD,  // ₽ Rouble sign

// Mathematical symbols
('*', 'P') => 0x220F,  // ∏ Product
('+', 'Z') => 0x2211,  // ∑ Sum
('R', 'T') => 0x221A,  // √ Square root
('0', '0') => 0x221E,  // ∞ Infinity
('!', '=') => 0x2260,  // ≠ Not equal

// Greek letters
('a', '*') => 0x03B1,  // α Alpha
('b', '*') => 0x03B2,  // β Beta
('g', '*') => 0x03B3,  // γ Gamma
('d', '*') => 0x03B4,  // δ Delta
('p', '*') => 0x03C0,  // π Pi

// Accented characters
('e', '\'') => 0x00E9, // é
('n', '~') => 0x00F1,  // ñ
('u', ':') => 0x00FC,  // ü

// Arrows
('<', '-') => 0x2190,  // ← Left arrow
('-', '>') => 0x2192,  // → Right arrow
('-', '!') => 0x2191,  // ↑ Up arrow
('-', 'v') => 0x2193,  // ↓ Down arrow

// Box drawing
('h', 'h') => 0x2500,  // ─ Horizontal line
('v', 'v') => 0x2502,  // │ Vertical line
('d', 'r') => 0x250C,  // ┌ Top-left corner
('d', 'l') => 0x2510,  // ┐ Top-right corner
('u', 'r') => 0x2514,  // └ Bottom-left corner
('u', 'l') => 0x2518,  // ┘ Bottom-right corner
```

### Digraphs Static Class

```csharp
public static class Digraphs
{
    private static readonly Dictionary<(char, char), int> _map = new()
    {
        // Control characters
        [('N', 'U')] = 0x00,
        [('S', 'H')] = 0x01,
        // ... many more entries ...

        // Latin supplement
        [('!', 'I')] = 0xA1,  // ¡
        [('C', 't')] = 0xA2,  // ¢
        [('P', 'd')] = 0xA3,  // £
        // ... etc ...
    };

    public static IReadOnlyDictionary<(char, char), int> Map => _map;

    public static int? Lookup(char char1, char char2)
    {
        return _map.TryGetValue((char1, char2), out var codePoint)
            ? codePoint
            : null;
    }

    public static char? GetCharacter(char char1, char char2)
    {
        var codePoint = Lookup(char1, char2);
        if (codePoint == null)
            return null;

        // Handle surrogate pairs for code points > 0xFFFF
        if (codePoint.Value > 0xFFFF)
        {
            // Return as string instead
            return null;
        }

        return (char)codePoint.Value;
    }

    public static string? GetString(char char1, char char2)
    {
        var codePoint = Lookup(char1, char2);
        if (codePoint == null)
            return null;

        return char.ConvertFromUtf32(codePoint.Value);
    }
}
```

### Vi Mode Integration

```csharp
// In Vi insert mode key bindings
KeyBindings.Add(
    Keys.ControlK,
    filter: ViInsertMode,
    handler: (e) =>
    {
        // Wait for two more keys
        e.App.ViState.WaitingForDigraph = true;
    });

// Handler for digraph completion
internal void HandleDigraphKey(KeyPressEvent e)
{
    var state = e.App.ViState;

    if (state.DigraphFirstKey == null)
    {
        // Store first key
        state.DigraphFirstKey = e.Data;
    }
    else
    {
        // Complete digraph
        var result = Digraphs.GetString(
            state.DigraphFirstKey.Value,
            e.Data);

        if (result != null)
        {
            e.CurrentBuffer.InsertText(result);
        }

        state.WaitingForDigraph = false;
        state.DigraphFirstKey = null;
    }
}
```

### Usage Example

```csharp
// In Vi insert mode, to type €:
// Press Ctrl+K, then 'E', then 'u'

// To type π:
// Press Ctrl+K, then 'p', then '*'

// To type ñ:
// Press Ctrl+K, then 'n', then '~'

// To type →:
// Press Ctrl+K, then '-', then '>'
```

## Dependencies

- Feature 21: Editing modes (Vi mode)
- Feature 19: Key bindings

## Implementation Tasks

1. Create Digraphs static class with dictionary
2. Port all digraph mappings from Python source
3. Implement Lookup method
4. Implement GetCharacter and GetString methods
5. Integrate with Vi insert mode
6. Add ViState properties for digraph input
7. Handle waiting state in key processor
8. Write unit tests

## Acceptance Criteria

- [ ] All RFC1345 digraphs are present
- [ ] Ctrl+K initiates digraph input
- [ ] Two characters produce correct output
- [ ] Invalid digraphs are ignored
- [ ] Unicode supplementary planes handled
- [ ] Works in Vi insert mode
- [ ] Unit tests achieve 80% coverage
