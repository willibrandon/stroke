# Feature 22: Screen and Character Model

## Overview

Implement the screen buffer system that stores styled characters in a 2D grid, supporting cursor positions, menu positions, and zero-width escape sequences.

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/layout/screen.py`

## Public API

### Char Class

```csharp
namespace Stroke.Rendering;

/// <summary>
/// Represents a single character in the screen buffer.
/// Immutable: modifying requires creating a new instance.
/// </summary>
public sealed class Char : IEquatable<Char>
{
    /// <summary>
    /// The null character (empty cell).
    /// </summary>
    public static readonly Char Null = new(" ", Attrs.Empty);

    /// <summary>
    /// Creates a character with style.
    /// </summary>
    /// <param name="char">The character string (may be multi-byte for wide chars).</param>
    /// <param name="style">The style attributes.</param>
    public Char(string @char = " ", string style = "");

    /// <summary>
    /// The character string.
    /// </summary>
    public string Character { get; }

    /// <summary>
    /// The style string.
    /// </summary>
    public string Style { get; }

    /// <summary>
    /// The width of this character (1 for normal, 2 for wide/CJK).
    /// </summary>
    public int Width { get; }

    public override bool Equals(object? obj);
    public bool Equals(Char? other);
    public override int GetHashCode();
    public override string ToString();

    public static bool operator ==(Char? left, Char? right);
    public static bool operator !=(Char? left, Char? right);
}
```

### CharacterDisplayMappings

```csharp
namespace Stroke.Rendering;

/// <summary>
/// Display mappings for control characters.
/// </summary>
public static class CharacterDisplayMappings
{
    /// <summary>
    /// Get the display representation of a control character.
    /// Returns null if the character is not a control character.
    /// </summary>
    /// <param name="char">The character to check.</param>
    /// <returns>The display string (e.g., "^A" for 0x01), or null.</returns>
    public static string? GetDisplayMapping(char @char);

    /// <summary>
    /// Mapping of control characters to their display representations.
    /// </summary>
    public static IReadOnlyDictionary<char, string> Mappings { get; }
}
```

### WritePosition Struct

```csharp
namespace Stroke.Rendering;

/// <summary>
/// Position and dimensions for writing to the screen.
/// </summary>
public readonly struct WritePosition : IEquatable<WritePosition>
{
    /// <summary>
    /// Creates a write position.
    /// </summary>
    /// <param name="xpos">The column position.</param>
    /// <param name="ypos">The row position.</param>
    /// <param name="width">The width of the region.</param>
    /// <param name="height">The height of the region.</param>
    public WritePosition(int xpos, int ypos, int width, int height);

    /// <summary>
    /// The column position.
    /// </summary>
    public int XPos { get; }

    /// <summary>
    /// The row position.
    /// </summary>
    public int YPos { get; }

    /// <summary>
    /// The width of the region.
    /// </summary>
    public int Width { get; }

    /// <summary>
    /// The height of the region.
    /// </summary>
    public int Height { get; }

    public override bool Equals(object? obj);
    public bool Equals(WritePosition other);
    public override int GetHashCode();
    public override string ToString();

    public static bool operator ==(WritePosition left, WritePosition right);
    public static bool operator !=(WritePosition left, WritePosition right);
}
```

### Screen Class

```csharp
namespace Stroke.Rendering;

/// <summary>
/// Two-dimensional buffer of Char instances.
/// Sparse storage: only non-empty cells are stored.
/// </summary>
public sealed class Screen
{
    /// <summary>
    /// Creates a new screen.
    /// </summary>
    /// <param name="defaultChar">The default character for empty cells.</param>
    /// <param name="initialWidth">Initial width hint.</param>
    /// <param name="initialHeight">Initial height hint.</param>
    public Screen(Char? defaultChar = null, int initialWidth = 0, int initialHeight = 0);

    /// <summary>
    /// The data buffer. Maps (y, x) to Char.
    /// Access via DataBuffer[y][x].
    /// </summary>
    public IDictionary<int, IDictionary<int, Char>> DataBuffer { get; }

    /// <summary>
    /// Cursor positions. Maps window to (row, column).
    /// </summary>
    public IDictionary<IWindow, Point> CursorPositions { get; }

    /// <summary>
    /// Menu positions. Maps window to (row, column).
    /// </summary>
    public IDictionary<IWindow, Point> MenuPositions { get; }

    /// <summary>
    /// True if the cursor is shown.
    /// </summary>
    public bool ShowCursor { get; set; }

    /// <summary>
    /// Zero-width escape sequences (e.g., for OSC window titles).
    /// Maps (y, x) to list of escape strings.
    /// </summary>
    public IDictionary<int, IDictionary<int, IList<string>>> ZeroWidthEscapes { get; }

    /// <summary>
    /// The width of the screen content.
    /// </summary>
    public int Width { get; }

    /// <summary>
    /// The height of the screen content.
    /// </summary>
    public int Height { get; }

    /// <summary>
    /// Set scroll margins (top, bottom row).
    /// </summary>
    public (int Top, int Bottom)? ScrollMargins { get; set; }

    /// <summary>
    /// The default character for empty cells.
    /// </summary>
    public Char DefaultChar { get; }

    /// <summary>
    /// Get the character at a position.
    /// </summary>
    /// <param name="row">The row.</param>
    /// <param name="column">The column.</param>
    /// <returns>The character, or DefaultChar if empty.</returns>
    public Char GetChar(int row, int column);

    /// <summary>
    /// Set the character at a position.
    /// </summary>
    /// <param name="row">The row.</param>
    /// <param name="column">The column.</param>
    /// <param name="char">The character to set.</param>
    public void SetChar(int row, int column, Char @char);

    /// <summary>
    /// Write data at a position.
    /// </summary>
    /// <param name="writePosition">The position to write at.</param>
    /// <param name="style">The style string.</param>
    /// <param name="data">The text data.</param>
    /// <param name="setMenuPosition">If true, also set menu position.</param>
    public void WriteData(
        WritePosition writePosition,
        string style,
        string data,
        bool setMenuPosition = false);

    /// <summary>
    /// Fill a region with a character.
    /// </summary>
    /// <param name="writePosition">The region to fill.</param>
    /// <param name="char">The character to fill with.</param>
    public void FillArea(WritePosition writePosition, Char @char);

    /// <summary>
    /// Draw all floats on this screen.
    /// </summary>
    /// <param name="floatContainer">The float container to draw.</param>
    public void DrawAllFloats(IFloatContainer floatContainer);

    /// <summary>
    /// Append another screen to this one at a position.
    /// Used for float composition.
    /// </summary>
    /// <param name="screen">The screen to append.</param>
    /// <param name="writePosition">The position to append at.</param>
    /// <param name="zIndex">The z-index.</param>
    /// <param name="style">Additional style to apply.</param>
    public void AppendStyleToContent(
        Screen screen,
        WritePosition writePosition,
        int? zIndex = null,
        string? style = null);
}
```

## Project Structure

```
src/Stroke/
└── Rendering/
    ├── Char.cs
    ├── CharacterDisplayMappings.cs
    ├── WritePosition.cs
    └── Screen.cs
tests/Stroke.Tests/
└── Rendering/
    ├── CharTests.cs
    ├── CharacterDisplayMappingsTests.cs
    ├── WritePositionTests.cs
    └── ScreenTests.cs
```

## Implementation Notes

### Control Character Display Mappings

Control characters (0x00-0x1F, 0x7F) are displayed as caret notation:
- `0x00` → `^@`
- `0x01` → `^A`
- `0x02` → `^B`
- ...
- `0x1A` → `^Z`
- `0x1B` → `^[` (ESC)
- `0x7F` → `^?` (DEL)

### Wide Character Handling

The `Char.Width` property returns:
- `1` for normal single-width characters
- `2` for wide/CJK characters (East Asian Width: W, F)
- `0` for combining characters

When writing wide characters, the following cell is marked as a continuation (empty placeholder).

### Sparse Storage

The screen uses dictionary-based storage:
```csharp
// DataBuffer[row][column] = Char
Dictionary<int, Dictionary<int, Char>> DataBuffer
```

This is efficient for terminals where most of the screen is empty or default-styled.

### Zero-Width Escapes

Zero-width escape sequences are stored separately and inserted at specific positions without affecting character layout. Used for:
- OSC (Operating System Command) sequences
- Hyperlinks
- Custom escape sequences

### Float Composition

When drawing floats:
1. Each float renders to its own Screen
2. Floats are composed onto the main screen by z-index
3. Higher z-index floats overwrite lower ones

### Character Interning

Common ASCII characters (space, letters, digits) should be interned to reduce memory allocation.

## Dependencies

- `Stroke.Core.Point` (Feature 00) - Point struct
- `Stroke.Core.IWindow` (Feature 30) - Window interface
- `Stroke.Layout.IFloatContainer` (Feature 32) - Float container

## Implementation Tasks

1. Implement `Char` class with equality and interning
2. Implement `CharacterDisplayMappings` static class
3. Implement `WritePosition` struct
4. Implement `Screen` class with sparse storage
5. Implement wide character handling
6. Implement zero-width escape support
7. Implement float composition
8. Write comprehensive unit tests

## Acceptance Criteria

- [ ] Character model matches Python Prompt Toolkit semantics
- [ ] Sparse storage is efficient for large screens
- [ ] Wide character handling is correct
- [ ] Control characters display correctly
- [ ] Float composition works correctly
- [ ] Unit tests achieve 80% coverage
