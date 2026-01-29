# Research: Screen and Character Model

**Feature**: 028-screen-character-model
**Date**: 2026-01-29

## Research Questions

### 1. Window Type Dependency

**Question**: How should Screen reference Window for cursor/menu position tracking when Window doesn't exist yet?

**Finding**: In Python Prompt Toolkit, `Window` is a forward reference via `TYPE_CHECKING` import. The Screen class uses Window as a dictionary key for cursor positions and menu positions.

**Decision**: Use `object` as the key type with a placeholder `IWindow` interface in the same namespace. This allows:
1. Type-safe dictionary keys with proper equality semantics
2. Future Window implementation to implement IWindow
3. No circular dependencies (IWindow is defined in Layout namespace alongside Screen)

**Rationale**: Python uses dynamic typing so Window can be any object. C# requires a type. Using `object` would work but loses type safety. A minimal interface (even marker interface) provides documentation and future extensibility.

**Alternative Considered**: Using generic type parameter `TWindow` - rejected because it complicates the API and Python PTK doesn't parameterize Screen.

### 2. Sparse Storage Pattern

**Question**: How to implement Python's `defaultdict` pattern efficiently in C#?

**Finding**: Python uses nested `defaultdict[int, defaultdict[int, Char]]` for O(1) access with automatic default values.

**Decision**: Use `Dictionary<int, Dictionary<int, Char>>` with custom indexer that creates rows on-demand and returns default Char for missing cells.

**Implementation Pattern**:
```csharp
public Char this[int row, int col]
{
    get
    {
        if (_buffer.TryGetValue(row, out var rowDict) &&
            rowDict.TryGetValue(col, out var ch))
            return ch;
        return _defaultChar;
    }
    set
    {
        if (!_buffer.TryGetValue(row, out var rowDict))
        {
            rowDict = new Dictionary<int, Char>();
            _buffer[row] = rowDict;
        }
        rowDict[col] = value;
    }
}
```

**Rationale**: Matches Python semantics exactly while using idiomatic C# dictionary patterns.

### 3. Char Caching Strategy

**Question**: How to implement character interning with FastDictCache?

**Finding**: Python uses `_CHAR_CACHE: FastDictCache[tuple[str, str], Char]` with size 1,000,000.

**Decision**: Create static `CharCache` using existing `FastDictCache<(string, string), Char>` with tuple key.

**Implementation**:
```csharp
private static readonly FastDictCache<(string Char, string Style), Char> _cache =
    new(key => new Char(key.Char, key.Style, skipMapping: true), 1_000_000);

public static Char Create(string ch, string style) => _cache[(ch, style)];
```

Note: Need `skipMapping` internal parameter to avoid re-mapping when retrieved from cache.

**Rationale**: Faithful port of Python behavior; reuses existing FastDictCache infrastructure.

### 4. Thread Safety Implementation

**Question**: What synchronization is needed for Screen class?

**Finding**: Screen has mutable state:
- `data_buffer` - read/write character data
- `zero_width_escapes` - read/write escape sequences
- `cursor_positions` - read/write per-window cursor
- `menu_positions` - read/write per-window menu pos
- `visible_windows_to_write_positions` - read/write
- `_draw_float_functions` - read/write/iterate
- `width`, `height`, `show_cursor` - read/write scalars

**Decision**: Use single `Lock` for all mutable state per Constitution XI pattern.

**Rationale**:
- Single lock is simpler and avoids deadlock complexity
- Screen operations are fast (dictionary lookups) so contention is minimal
- Multiple granular locks could lead to inconsistent state reads

### 5. Zero-Width Escape Concatenation

**Question**: How to handle multiple escapes at same position?

**Finding**: Python uses `defaultdict(str)` which returns empty string for missing keys. Setting adds/replaces. Spec says "concatenated as string".

**Decision**: Append new escapes to existing string at position:
```csharp
public void AddZeroWidthEscape(int row, int col, string escape)
{
    var existing = GetZeroWidthEscapes(row, col);
    SetZeroWidthEscapes(row, col, existing + escape);
}
```

**Rationale**: Matches spec requirement "Both are stored (concatenated to existing string)".

### 6. Display Mappings Organization

**Question**: Should display_mappings be part of Char or separate class?

**Finding**: Python has `display_mappings` as a class attribute of Char. The spec mentions `CharacterDisplayMappings` utility.

**Decision**:
1. `CharacterDisplayMappings` - static class with `FrozenDictionary<char, string>` for O(1) lookup
2. `Char` - references `CharacterDisplayMappings` for transformation

**Rationale**:
- Separation of concerns (mappings vs character representation)
- `FrozenDictionary` is perfect for static immutable lookup table
- Easier testing of mappings in isolation

### 7. Char Type Design

**Question**: Should Char be a `record`, `readonly struct`, or `sealed class`?

**Finding**: Python Char uses `__slots__` for memory efficiency and is treated as immutable.

**Decision**: Use `sealed class` with value equality:
```csharp
public sealed class Char : IEquatable<Char>
{
    public string Character { get; }
    public string Style { get; }
    public int Width { get; }
    // Value equality via IEquatable<Char>
}
```

**Rationale**:
- `record class` would work but adds unnecessary `with` expression support
- `readonly struct` risks boxing when used as dictionary values
- `sealed class` matches Python's reference semantics exactly
- FastDictCache stores references, not copies

### 8. WritePosition Equality

**Question**: Does WritePosition need value equality for spec FR-020?

**Finding**: Spec says "WritePosition MUST implement value equality based on all four properties."

**Decision**: Use `record struct`:
```csharp
public readonly record struct WritePosition(int XPos, int YPos, int Width, int Height)
```

**Rationale**:
- `record struct` provides automatic value equality
- Small (4 ints = 16 bytes) so value type is efficient
- Immutable with `readonly`

## Dependencies Verified

| Dependency | Status | Notes |
|------------|--------|-------|
| Stroke.Core.Point | ✅ Available | `src/Stroke/Core/Primitives/Point.cs` |
| Stroke.Core.FastDictCache | ✅ Available | `src/Stroke/Core/FastDictCache.cs` |
| Stroke.Core.UnicodeWidth | ✅ Available | `src/Stroke/Core/UnicodeWidth.cs` |
| Wcwidth NuGet | ✅ Available | Referenced in Stroke.csproj |

## API Mapping Verification

| Python API | C# API | Notes |
|------------|--------|-------|
| `Char.__init__(char, style)` | `Char(string ch, string style)` | Constructor |
| `Char.char` | `Char.Character` | Property (PascalCase) |
| `Char.style` | `Char.Style` | Property |
| `Char.width` | `Char.Width` | Computed via UnicodeWidth |
| `Char.display_mappings` | `CharacterDisplayMappings.Mappings` | Static utility |
| `Char.__eq__` | `Char.Equals(Char)` | IEquatable |
| `_CHAR_CACHE` | `Char.Cache` | Internal static |
| `Transparent` | `Char.Transparent` | Constant |
| `Screen.__init__` | `Screen()` | Constructor |
| `Screen.data_buffer` | `Screen.DataBuffer` | Indexer access |
| `Screen.zero_width_escapes` | `Screen.ZeroWidthEscapes` | Dictionary-like |
| `Screen.cursor_positions` | `Screen.CursorPositions` | Dictionary |
| `Screen.menu_positions` | `Screen.MenuPositions` | Dictionary |
| `Screen.show_cursor` | `Screen.ShowCursor` | Property |
| `Screen.width` | `Screen.Width` | Property |
| `Screen.height` | `Screen.Height` | Property |
| `Screen.visible_windows_to_write_positions` | `Screen.VisibleWindowsToWritePositions` | Dictionary |
| `Screen.visible_windows` | `Screen.VisibleWindows` | Property (list) |
| `Screen.set_cursor_position` | `Screen.SetCursorPosition` | Method |
| `Screen.set_menu_position` | `Screen.SetMenuPosition` | Method |
| `Screen.get_cursor_position` | `Screen.GetCursorPosition` | Method |
| `Screen.get_menu_position` | `Screen.GetMenuPosition` | Method |
| `Screen.draw_with_z_index` | `Screen.DrawWithZIndex` | Method |
| `Screen.draw_all_floats` | `Screen.DrawAllFloats` | Method |
| `Screen.append_style_to_content` | `Screen.AppendStyleToContent` | Method |
| `Screen.fill_area` | `Screen.FillArea` | Method |
| `WritePosition.__init__` | `WritePosition(xpos, ypos, width, height)` | Constructor |
| `WritePosition.xpos` | `WritePosition.XPos` | Property |
| `WritePosition.ypos` | `WritePosition.YPos` | Property |
| `WritePosition.width` | `WritePosition.Width` | Property |
| `WritePosition.height` | `WritePosition.Height` | Property |

## Unresolved Items

None - all technical decisions made.
