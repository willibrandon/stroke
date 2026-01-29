# Quickstart: Vi Digraphs

**Feature**: 026-vi-digraphs
**Date**: 2026-01-28

## What It Does

The `Digraphs` class provides a lookup table for Vi digraph insertion. Digraphs let users type special characters by pressing `Ctrl+K` followed by two characters. For example, `Ctrl+K E u` inserts the Euro sign (€).

## Quick Example

```csharp
using Stroke.KeyBinding;

// Look up a digraph
int? codePoint = Digraphs.Lookup('E', 'u');  // 0x20AC (Euro sign)

// Get the actual character
string? character = Digraphs.GetString('E', 'u');  // "€"

// Check if a digraph exists
bool exists = Digraphs.Lookup('Z', 'Z') != null;  // false
```

## Common Digraphs

| Keys | Character | Description |
|------|-----------|-------------|
| `E u` | € | Euro sign |
| `p *` | π | Greek lowercase pi |
| `< -` | ← | Left arrow |
| `- >` | → | Right arrow |
| `h h` | ─ | Horizontal box drawing |
| `v v` | │ | Vertical box drawing |
| `O K` | ✓ | Check mark |
| `! =` | ≠ | Not equal |
| `= <` | ≤ | Less than or equal |
| `> =` | ≥ | Greater than or equal |
| `D G` | ° | Degree sign |
| `C o` | © | Copyright |
| `+ -` | ± | Plus-minus |
| `1 2` | ½ | One half |
| `* *` | • | Bullet |

## API Summary

| Member | Purpose |
|--------|---------|
| `Digraphs.Map` | Full dictionary of all 1,300+ digraphs |
| `Digraphs.Lookup(c1, c2)` | Get Unicode code point (or null) |
| `Digraphs.GetString(c1, c2)` | Get character string (or null) |

## Integration Example

```csharp
// In a Vi mode key handler
if (viState.WaitingForDigraph && viState.DigraphSymbol1 is { Length: 1 } first)
{
    var result = Digraphs.GetString(first[0], pressedChar);
    if (result != null)
    {
        buffer.InsertText(result);
    }
    viState.WaitingForDigraph = false;
    viState.DigraphSymbol1 = null;
}
```

## Notes

- **Case-sensitive**: `a*` and `A*` map to different Greek letters (α vs Α)
- **Order matters**: `Eu` works, but `uE` returns null
- **Thread-safe**: Safe to call from any thread
- **No exceptions**: Invalid digraphs return null, never throw
