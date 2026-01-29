# Feature Specification: Screen and Character Model

**Feature Branch**: `028-screen-character-model`
**Created**: 2026-01-29
**Status**: Draft
**Input**: User description: "Feature 28: Screen and Character Model - Implement the screen buffer system that stores styled characters in a 2D grid, supporting cursor positions, menu positions, and zero-width escape sequences."

---

## Key Definitions

The following terms have precise meanings in this specification:

| Term | Definition |
|------|------------|
| **Sparse 2D buffer** | A `Dictionary<int, Dictionary<int, Char>>` structure where outer keys are row indices, inner keys are column indices. Only cells that have been explicitly written consume memory. Unset cells return a default value without creating entries. |
| **Defaultdict-like behavior** | When the Screen indexer getter is called for an unset position, it returns `DefaultChar` without creating an entry in the underlying dictionary. The indexer setter creates entries as needed. |
| **Valid coordinate** | Any `int` value from `int.MinValue` to `int.MaxValue`. Negative coordinates are valid (for partially-visible floats). There is no artificial bound on screen size. |
| **Thread-safe** | Individual operations (single indexer access, single method call) are atomic via `Lock` synchronization. Compound operations (read-modify-write across multiple calls) require external synchronization by the caller. |
| **Style string** | A space-separated list of style classes (e.g., `"class:keyword class:bold"`). Classes are applied left-to-right. The format follows Python Prompt Toolkit's style notation. |
| **Common Char instances** | Characters likely to be reused frequently: ASCII printable characters (0x20-0x7E) with common styles (empty, `Transparent`, `class:default`). The cache stores up to 1,000,000 entries using FastDictCache. |
| **Display width** | The number of terminal columns a character occupies when rendered: 0 for combining characters, 1 for most characters, 2 for wide/CJK characters per UAX #11 (Unicode East Asian Width). Multi-character display strings (e.g., "^A") sum individual character widths. |
| **Control character** | Any character with code point 0x00-0x1F (C0 controls), 0x7F (DEL), or 0x80-0x9F (C1 controls). |
| **Ascending z-index order** | Draw functions are executed from lowest to highest z-index. For equal z-index values, execution order is the order in which they were queued (FIFO). |
| **Transparent** | The literal string `"[Transparent]"` used as the default style, indicating no explicit styling is applied. |

---

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Store and Retrieve Characters (Priority: P1)

A terminal UI application needs to store styled characters at specific positions in a 2D screen buffer and retrieve them for rendering. This is the fundamental operation that enables all terminal display functionality.

**Why this priority**: This is the core functionality without which no terminal content can be displayed. Every other feature depends on the ability to store and retrieve characters.

**Independent Test**: Can be fully tested by creating a screen, storing characters at various positions, and verifying retrieval returns the correct character and style. Delivers the ability to build up screen content.

**Acceptance Scenarios**:

1. **Given** an empty screen, **When** a character with style is stored at position (5, 10), **Then** retrieving the character at (5, 10) returns that exact character and style
2. **Given** an empty screen, **When** retrieving a character at a position where nothing was stored, **Then** the default character (space with Transparent style) is returned without creating a dictionary entry
3. **Given** a screen with stored characters, **When** a wide character (CJK, e.g., "中") is stored at position (row=5, col=10), **Then** the Char reports Width=2 (the Screen does NOT automatically manage the following cell - that is the caller's responsibility during layout)
4. **Given** a screen, **When** a character is stored at a negative coordinate (e.g., row=-5, col=-10), **Then** the character is stored and retrievable at that position
5. **Given** a screen, **When** accessing coordinates at Int32.MaxValue, **Then** the operation succeeds without exception (limited only by available memory)

---

### User Story 2 - Track Cursor and Menu Positions (Priority: P2)

A terminal application with multiple windows needs to track cursor positions and menu positions independently for each window. The cursor position shows where text input appears, while the menu position shows where completion menus should anchor.

**Why this priority**: Cursor tracking is essential for interactive applications, but basic character storage must work first. Multiple window support is common in complex terminal UIs.

**Independent Test**: Can be tested by creating a screen, setting cursor positions for mock window references, and verifying the correct position is returned for each window.

**Acceptance Scenarios**:

1. **Given** a screen and two window references, **When** different cursor positions are set for each window, **Then** retrieving cursor position for each window returns the correct position
2. **Given** a screen and a window, **When** no cursor position has been set, **Then** retrieving cursor position returns Point.Zero (0, 0)
3. **Given** a screen with only cursor position set (no menu position), **When** menu position is requested, **Then** the cursor position is returned as fallback; if neither is set, Point.Zero is returned
4. **Given** a null window reference, **When** SetCursorPosition or SetMenuPosition is called, **Then** an ArgumentNullException is thrown
5. **Given** a null window reference, **When** GetCursorPosition or GetMenuPosition is called, **Then** an ArgumentNullException is thrown
6. **Given** a screen with a cursor position set for a window, **When** the same window is removed from the cursor positions (via direct dictionary access), **Then** subsequent GetCursorPosition returns Point.Zero

---

### User Story 3 - Display Control Characters (Priority: P2)

A text editor needs to visually display control characters (like Ctrl+A, Escape, Tab) in a readable format using caret notation (^A, ^[, ^I). This helps users understand what non-printable characters exist in their text.

**Why this priority**: Control character display is important for text editing applications but not required for basic display functionality.

**Independent Test**: Can be tested by creating Char instances with control character input and verifying they are converted to the correct caret notation display string.

**Acceptance Scenarios**:

1. **Given** a control character (0x01), **When** a Char is created with it, **Then** the displayed character is "^A" and style has "class:control-character " prepended (note: space-separated, prepended before any provided style)
2. **Given** the escape character (0x1B), **When** a Char is created with it, **Then** the displayed character is "^[" with "class:control-character " prepended to style
3. **Given** a normal printable character, **When** a Char is created with it, **Then** the character is stored unchanged without the control-character class
4. **Given** a DEL character (0x7F), **When** a Char is created with it, **Then** the displayed character is "^?" with "class:control-character " prepended
5. **Given** a C1 control character (e.g., 0x80), **When** a Char is created with it, **Then** the displayed character is "<80>" with "class:control-character " prepended
6. **Given** a non-breaking space (0xA0), **When** a Char is created with it, **Then** the displayed character is " " (regular space) with "class:nbsp " prepended

---

### User Story 4 - Attach Zero-Width Escape Sequences (Priority: P3)

A terminal application needs to attach invisible escape sequences (like OSC hyperlinks or terminal title changes) at specific screen positions without affecting the visible character layout. These sequences are output during rendering but don't consume screen columns.

**Why this priority**: Zero-width escapes are an advanced feature for hyperlinks and terminal extensions, not required for basic rendering.

**Independent Test**: Can be tested by attaching escape sequences at positions and verifying they are stored separately from the character data and can be retrieved.

**Acceptance Scenarios**:

1. **Given** a screen with character content, **When** a zero-width escape sequence is added at position (5, 10), **Then** the escape is stored without affecting the character at that position
2. **Given** multiple escape sequences at the same position, **When** escapes are retrieved, **Then** all sequences are returned in order added (concatenated as single string, no delimiter)
3. **Given** a position with no escape sequences, **When** escapes are retrieved, **Then** an empty string is returned
4. **Given** an empty string passed to AddZeroWidthEscape, **When** the method is called, **Then** no change is made (empty strings are ignored)
5. **Given** a null string passed to AddZeroWidthEscape, **When** the method is called, **Then** an ArgumentNullException is thrown

---

### User Story 5 - Draw Floating Content with Z-Index (Priority: P3)

A terminal application with floating windows (like completion menus, dialogs, tooltips) needs to defer drawing of overlay content until all base content is laid out, then draw floats in z-index order so higher-index content appears on top.

**Why this priority**: Floating content is advanced UI functionality that builds upon the basic screen buffer system.

**Independent Test**: Can be tested by queueing multiple draw functions with different z-indices and verifying they execute in ascending z-index order.

**Acceptance Scenarios**:

1. **Given** draw functions queued with z-indices 5, 2, 8, **When** DrawAllFloats is called, **Then** functions execute in order: 2, 5, 8
2. **Given** multiple draw functions with the same z-index (e.g., two functions both at z=5), **When** DrawAllFloats is called, **Then** they execute in FIFO order (the order they were queued)
3. **Given** a draw function that queues another draw function during execution, **When** DrawAllFloats is called, **Then** the newly queued function is also processed in the same loop iteration (iterative, not recursive)
4. **Given** no queued draw functions, **When** DrawAllFloats is called, **Then** the method completes without error and the queue remains empty
5. **Given** a draw function that throws an exception, **When** DrawAllFloats is called, **Then** the exception propagates to the caller; remaining functions in the queue are NOT executed; the queue is cleared before throwing
6. **Given** DrawAllFloats was already called, **When** DrawAllFloats is called again with no new queued functions, **Then** it completes immediately with no effect

---

### User Story 6 - Fill Screen Regions (Priority: P3)

A layout engine needs to fill rectangular regions of the screen with style attributes. This enables efficient background fills and style application to areas.

**Why this priority**: Region filling is an optimization for layout, not strictly required when individual cell writes work.

**Independent Test**: Can be tested by filling a region with a style and verifying all cells in the region have the style applied.

**Acceptance Scenarios**:

1. **Given** a screen with existing content and a rectangular region (x=5, y=3, width=10, height=5), **When** FillArea is called with a style and after=false (default), **Then** all cells in the region have the style prepended to their existing style (format: `"newStyle existingStyle"`)
2. **Given** a screen with existing content and a rectangular region, **When** FillArea is called with after=true, **Then** all cells in the region have the style appended to their existing style (format: `"existingStyle newStyle"`)
3. **Given** an empty or whitespace-only style string, **When** FillArea is called, **Then** no changes are made to the screen content
4. **Given** a region with width=0 or height=0, **When** FillArea is called, **Then** no changes are made (the region contains zero cells)
5. **Given** a WritePosition with negative width or height (via direct construction), **When** used with FillArea, **Then** it is treated as zero cells (no iteration occurs for negative ranges)
6. **Given** an empty screen (no cells written), **When** AppendStyleToContent is called, **Then** no changes are made and no exception is thrown
7. **Given** a screen with content, **When** AppendStyleToContent is called with a style, **Then** every existing cell has the style appended (format: `"existingStyle appendedStyle"`)
8. **Given** AppendStyleToContent with an empty or whitespace-only style, **When** called, **Then** no changes are made

---

### User Story 7 - Reset Screen State (Priority: P3)

A layout system needs to clear and reuse a Screen instance between rendering cycles to avoid allocation overhead from creating new Screen objects.

**Why this priority**: Screen reuse is a performance optimization, not required for basic functionality.

**Independent Test**: Can be tested by populating a screen with content and positions, calling Clear(), and verifying all state is reset.

**Acceptance Scenarios**:

1. **Given** a screen with characters stored, cursor positions, menu positions, zero-width escapes, and queued draw functions, **When** Clear() is called, **Then** all content is removed: data buffer is empty, cursor/menu positions are cleared, zero-width escapes are cleared, draw queue is cleared, visible windows are cleared
2. **Given** a screen with Width=100 and Height=50 after writes, **When** Clear() is called, **Then** Width and Height are reset to the constructor's initialWidth and initialHeight values
3. **Given** a screen with ShowCursor=false and a custom DefaultChar, **When** Clear() is called, **Then** ShowCursor and DefaultChar are preserved (not reset)

---

### Edge Cases

#### Character Creation (Char)
- **Null character string**: Char.Create(null, style) throws ArgumentNullException.
- **Null style string**: Char.Create(char, null) throws ArgumentNullException.
- **Empty character string**: Stored as-is with Width=0. This is valid for placeholder cells.
- **Empty style string**: Valid; results in no styling applied (inherits from parent/default).
- **Surrogate pairs (emoji)**: Characters like "😀" (U+1F600) are stored as the full surrogate pair string. Width is calculated by UnicodeWidth (typically 2 for emoji).
- **Multi-character display strings**: Display strings like "^A" have Width = sum of individual character widths (e.g., "^A" → 1+1=2).

#### Character Display (Char.ToString)
- **Char.ToString()**: Returns a debug-friendly format: `Char('{Character}', '{Style}')`. For example, `Char('A', 'class:keyword')`.

#### Screen Indexer
- **Negative row/col**: Valid coordinates. Characters can be stored at negative positions for partially-visible content.
- **Int32.MaxValue coordinates**: Valid; limited only by available memory for dictionary entries.
- **Concurrent read/write to same cell**: Each individual operation is atomic. Two threads writing to the same cell results in one winning (last-write-wins). Two threads where one reads and one writes may see either the old or new value depending on timing.

#### Screen Dimensions
- **Screen constructor with negative dimensions**: Negative initialWidth or initialHeight are clamped to 0.
- **Dimension auto-expansion**: When setting a cell at (row, col), Width is updated to max(Width, col+1) and Height is updated to max(Height, row+1). Reading does NOT expand dimensions.

#### Wide Characters
- **Wide character at rightmost column**: The character is stored normally. Clipping is a rendering concern, not a storage concern. The Screen stores the character; the renderer decides how to handle overflow.

#### WritePosition
- **Negative x/y coordinates**: Valid for partially-visible floats positioned off-screen.
- **Zero width/height**: Valid; represents an empty region. FillArea on such regions is a no-op.
- **Negative width/height**: The WritePosition record struct does not validate; callers iterating from x to x+width will simply not iterate if width is negative (empty range).

#### Zero-Width Escapes
- **Same escape added twice**: Both are stored (concatenated to existing string). De-duplication is the caller's responsibility.
- **Empty escape string**: Ignored (no change to stored escapes).

#### VisibleWindows
- **No windows drawn**: VisibleWindows returns an empty list.
- **Window removed after being drawn**: If a window is removed from VisibleWindowsToWritePositions, it no longer appears in VisibleWindows.
- **Same window re-added after removal**: Treated as a new entry; previous position is gone.

#### Thread Safety
- **Concurrent DrawAllFloats calls**: The Lock ensures only one executes at a time. The second caller blocks until the first completes.
- **Modifications during iteration**: All public methods acquire the lock, so external code cannot modify while internal iteration occurs.

## Requirements *(mandatory)*

### Functional Requirements

#### Char Type
- **FR-001**: System MUST provide a Char sealed class that stores a character string (Character property) and style string (Style property) as immutable values. Arguments MUST NOT be null (ArgumentNullException if null).
- **FR-002**: System MUST calculate and expose the display width of each Char via a Width property. Width is computed as the sum of UnicodeWidth.GetWidth() for each character in the display string. Normal ASCII = 1, wide/CJK = 2, combining = 0. Multi-character display strings (e.g., "^A") sum to 2.
- **FR-003**: System MUST convert C0 control characters (0x00-0x1F) to caret notation: char 0x00 → "^@", 0x01 → "^A", ..., 0x1F → "^_". Style MUST have "class:control-character " prepended.
- **FR-004**: System MUST convert DEL (0x7F) to "^?" with "class:control-character " prepended to style.
- **FR-005**: System MUST convert C1 control characters (0x80-0x9F) to hex format: "<80>", "<81>", ..., "<9F>". Style MUST have "class:control-character " prepended.
- **FR-006**: System MUST handle non-breaking space (0xA0) with "class:nbsp " prepended to style. The displayed character is a regular space (" ").
- **FR-007**: Char MUST implement IEquatable<Char> with equality based on Character AND Style strings (case-sensitive). Width is NOT part of equality (it's derived from Character).
- **FR-008**: Char MUST provide a ToString() method returning `Char('{Character}', '{Style}')` format for debugging.

#### Char Factory and Caching
- **FR-009**: System MUST provide a static Char.Create(string character, string style) factory method that returns cached instances for common characters.
- **FR-010**: System MUST provide a Transparent constant with value `"[Transparent]"` for default character styling.
- **FR-011**: The Char cache MUST use FastDictCache<(string, string), Char> with a maximum capacity of 1,000,000 entries (matching Python PTK's _CHAR_CACHE).

#### CharacterDisplayMappings
- **FR-012**: System MUST provide a CharacterDisplayMappings static class with a FrozenDictionary<char, string> Mappings property containing exactly 66 entries:
  - 32 entries for C0 controls (0x00-0x1F → "^@" through "^_")
  - 1 entry for DEL (0x7F → "^?")
  - 32 entries for C1 controls (0x80-0x9F → "<80>" through "<9F>")
  - 1 entry for NBSP (0xA0 → " " space)
  - Total: 32 + 1 + 32 + 1 = 66 mappings.
- **FR-013**: CharacterDisplayMappings MUST provide TryGetDisplay(char, out string) returning true if a mapping exists.
- **FR-014**: CharacterDisplayMappings MUST provide GetDisplayOrDefault(char) returning the mapping or the character as-is.
- **FR-015**: CharacterDisplayMappings MUST be immutable and thread-safe (static readonly FrozenDictionary).

#### WritePosition
- **FR-016**: System MUST provide WritePosition as a readonly record struct with properties: XPos (int), YPos (int), Width (int), Height (int).
- **FR-017**: WritePosition MUST implement value equality based on all four properties (provided by record struct).
- **FR-018**: WritePosition MUST allow negative XPos/YPos values (for partially-visible floats). Width and Height MUST be validated >= 0 at construction (ArgumentOutOfRangeException if negative).

#### Screen Buffer
- **FR-019**: System MUST provide a Screen sealed class storing Char instances in a Dictionary<int, Dictionary<int, Char>> (row → column → Char).
- **FR-020**: Screen MUST provide an indexer this[int row, int col] for get/set access. Getter returns DefaultChar for unset positions without creating entries. Setter creates dictionary entries as needed.
- **FR-021**: Screen MUST accept an optional defaultChar constructor parameter. If null or omitted, DefaultChar is a space (" ") with Transparent style.
- **FR-022**: Screen MUST track Width and Height properties. Initial values come from constructor (initialWidth, initialHeight, both defaulting to 0, clamped to non-negative). Setting a cell at (row, col) expands dimensions: Width = max(Width, col+1), Height = max(Height, row+1). Reading does NOT expand dimensions.

#### Cursor and Menu Positions
- **FR-023**: Screen MUST maintain cursor positions in a Dictionary<IWindow, Point>. SetCursorPosition(IWindow, Point) and GetCursorPosition(IWindow) MUST throw ArgumentNullException for null window.
- **FR-024**: GetCursorPosition MUST return Point.Zero if no position is set for the window.
- **FR-025**: Screen MUST maintain menu positions in a Dictionary<IWindow, Point>. SetMenuPosition(IWindow, Point) and GetMenuPosition(IWindow) MUST throw ArgumentNullException for null window.
- **FR-026**: GetMenuPosition MUST return: (1) the menu position if set, (2) else the cursor position if set, (3) else Point.Zero.

#### Zero-Width Escapes
- **FR-027**: Screen MUST store zero-width escape sequences in a Dictionary<(int row, int col), string>. Multiple escapes at the same position are concatenated (no delimiter).
- **FR-028**: AddZeroWidthEscape(int row, int col, string escape) MUST throw ArgumentNullException for null escape. Empty string is ignored (no-op).
- **FR-029**: GetZeroWidthEscapes(int row, int col) MUST return the stored string or empty string if none.

#### Z-Index Drawing
- **FR-030**: Screen MUST provide DrawWithZIndex(int zIndex, Action drawFunc) to queue draw functions.
- **FR-031**: Screen MUST provide DrawAllFloats() that executes queued functions in ascending z-index order. Equal z-indices execute in FIFO order.
- **FR-032**: DrawAllFloats MUST process functions iteratively: if a function queues more functions, they are processed in the same pass.
- **FR-033**: If a draw function throws, DrawAllFloats MUST clear the queue and re-throw. Remaining functions are not executed.

#### Style Operations
- **FR-034**: Screen MUST provide FillArea(WritePosition, string style, bool after = false). If style is empty or whitespace-only (per string.IsNullOrWhiteSpace), no-op. For each cell in region: creates/updates with combined style. after=false prepends, after=true appends.
- **FR-035**: Screen MUST provide AppendStyleToContent(string styleStr) that appends the style to all existing cells. If styleStr is empty or whitespace-only (per string.IsNullOrWhiteSpace), no-op.

#### Visible Windows
- **FR-036**: Screen MUST expose VisibleWindowsToWritePositions as IDictionary<IWindow, WritePosition> for tracking drawn windows.
- **FR-037**: Screen MUST expose VisibleWindows as IReadOnlyList<IWindow> returning a snapshot of keys from VisibleWindowsToWritePositions.
- **FR-038**: Screen MUST provide ShowCursor bool property (default true).

#### Screen Reset
- **FR-040**: Screen MUST provide a Clear() method that resets the screen to initial state: clears data buffer, zero-width escapes, cursor positions, menu positions, draw queue, and visible windows. Width and Height are reset to initial constructor values. DefaultChar and ShowCursor are preserved.

#### IWindow Interface
- **FR-041**: System MUST provide IWindow marker interface in Stroke.Layout namespace. No members required. Implementations must provide proper equality semantics for dictionary key usage.

### Key Entities

- **Char**: Sealed class representing a single styled character cell. Properties: Character (string), Style (string), Width (int, computed). Immutable with IEquatable<Char> value equality on Character+Style. Automatically converts control characters to caret/hex notation with "class:control-character " prepended to style.
- **CharacterDisplayMappings**: Static class with FrozenDictionary<char, string> providing 66 mappings: C0 controls (32) + DEL (1) + C1 controls (32) + NBSP (1). Thread-safe via immutability.
- **WritePosition**: Readonly record struct with XPos, YPos, Width, Height (all int). Value equality via record semantics. XPos/YPos may be negative; Width/Height are conventionally non-negative but not validated.
- **Screen**: Sealed class providing sparse 2D buffer via Dictionary<int, Dictionary<int, Char>>. Thread-safe via Lock synchronization. Tracks cursor/menu positions per IWindow, zero-width escapes per position, z-index draw queue, and visible windows.
- **IWindow**: Marker interface for window types used as dictionary keys. Implementations must provide proper Equals/GetHashCode for dictionary usage.

### Non-Functional Requirements

- **NFR-001**: Thread Safety - Screen MUST be thread-safe per Constitution XI. All mutable operations MUST use Lock synchronization. Individual operations are atomic; compound operations require external synchronization.
- **NFR-002**: Thread Safety - CharacterDisplayMappings MUST be thread-safe via immutability (static readonly FrozenDictionary).
- **NFR-003**: Thread Safety - Char instances MUST be immutable and inherently thread-safe.
- **NFR-004**: Thread Safety - FastDictCache used for Char interning is thread-safe per Feature 006.
- **NFR-005**: Memory Efficiency - Sparse storage MUST ensure memory usage scales with O(n) where n = number of written cells, not O(width × height).
- **NFR-006**: Performance - Screen indexer access MUST be O(1) average case (dictionary lookup).
- **NFR-007**: Performance - DrawAllFloats with N queued functions MUST complete in O(N log N) time (sort) + O(N) execution.
- **NFR-008**: Performance - Char.Create for cached entries MUST be O(1) average case (dictionary lookup).

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Character storage and retrieval operations succeed for coordinates in the range [Int32.MinValue, Int32.MaxValue]. Verification: unit tests store/retrieve at (0,0), (-1000,-1000), (1000000, 1000000) without exception.
- **SC-002**: CharacterDisplayMappings.Mappings.Count equals 66 entries covering: C0 controls (32: 0x00-0x1F), DEL (1: 0x7F), C1 controls (32: 0x80-0x9F), NBSP (1: 0xA0). Verification: unit test asserts exact count and samples key mappings.
- **SC-003**: Wide character width calculation uses UnicodeWidth.GetWidth() per UAX #11 (Unicode Standard Annex #11: East Asian Width). Verification: unit tests verify width=2 for sample CJK characters (e.g., '中', '日', '韓'), width=1 for ASCII, width=0 for combining marks.
- **SC-004**: Z-index draw functions execute in ascending order; equal z-indices execute in FIFO order. Verification: unit test queues functions with z=[5,2,8,5] and verifies execution order [2,5,5,8] with first 5 before second 5.
- **SC-005**: Cursor and menu positions are tracked independently per window. Verification: unit test creates 3 IWindow instances, sets different positions for each, verifies retrieval returns correct values.
- **SC-006**: Sparse storage uses Dictionary<int, Dictionary<int, Char>>. A screen with 100 cells written uses O(100) dictionary entries, not O(width×height). Verification: unit test writes 100 cells at scattered positions on a notional 10000×10000 screen and verifies dictionary entry count is approximately 100.
- **SC-007**: Unit tests achieve ≥80% line coverage for Char.cs, CharacterDisplayMappings.cs, WritePosition.cs, Screen.cs, IWindow.cs. Coverage is measured by `dotnet test --collect:"XPlat Code Coverage"`.
- **SC-008**: Thread safety is verified by concurrent access tests. Verification: unit test spawns 10 threads that simultaneously read/write to Screen; test passes if no exceptions occur and data integrity is maintained (reads return valid Char instances).

## Assumptions

- **A-001**: Window type does not exist yet. IWindow marker interface is created as a forward reference. **Validation**: IWindow interface defined in this feature; future Window class will implement it.
- **A-002**: Point type from Stroke.Core.Primitives is available. **Validation**: Confirmed - Point record struct exists at src/Stroke/Core/Primitives/Point.cs with X, Y properties and Point.Zero static member.
- **A-003**: Unicode character width calculation uses UnicodeWidth from Feature 024. **Validation**: Confirmed - UnicodeWidth.GetWidth(char) and GetWidth(string) exist at src/Stroke/Core/UnicodeWidth.cs with LRU caching.
- **A-004**: Char interning uses FastDictCache from Feature 006. **Validation**: Confirmed - FastDictCache<TKey, TValue> exists at src/Stroke/Core/FastDictCache.cs with thread-safe operation and tuple key support.
- **A-005**: Negative x/y coordinates in WritePosition are valid for partially-visible floats. **Validation**: Matches Python PTK behavior where floats can be positioned off-screen.
- **A-006**: Screen is in Stroke.Layout namespace per api-mapping.md (layout.screen → Stroke.Layout). **Validation**: Consistent with Constitution III layered architecture.
- **A-007**: Zero-width escapes are stored as concatenated strings, not lists. **Justification**: Python PTK uses string concatenation (`+=`). Lists would add allocation overhead for typical single-escape case. Concatenation matches Python behavior exactly.

---

## Python PTK Fidelity

This section documents how the C# implementation maps to Python Prompt Toolkit's `layout/screen.py`.

### Public APIs Ported

| Python API | C# API | Notes |
|------------|--------|-------|
| `Char.__init__(char, style)` | `new Char(char, style)` | Constructor |
| `Char.char` | `Char.Character` | Property |
| `Char.style` | `Char.Style` | Property |
| `Char.width` | `Char.Width` | Computed property |
| `Char.__eq__`, `Char.__ne__` | `IEquatable<Char>` | Equality |
| `Char.__hash__` | `GetHashCode()` | Hash code |
| `Char.__repr__` | `ToString()` | Debug string |
| `Char.display_mappings` (class var) | `CharacterDisplayMappings.Mappings` | Static class |
| `_CHAR_CACHE` (module level) | `Char._cache` (static field) | 1M entry limit |
| `Char._get_or_create(char, style)` | `Char.Create(char, style)` | Factory |
| `Transparent` (module constant) | `Char.Transparent` | Constant |
| `Screen.__init__(default_char, ...)` | `new Screen(defaultChar, ...)` | Constructor |
| `Screen.data_buffer` | Private `_dataBuffer` | Sparse dict |
| `Screen.__getitem__` | `this[row, col]` getter | Indexer |
| `Screen.__setitem__` | `this[row, col]` setter | Indexer |
| `Screen.zero_width_escapes` | Private `_zeroWidthEscapes` | Dict storage |
| `Screen.cursor_positions` | Via Get/SetCursorPosition | Dict storage |
| `Screen.menu_positions` | Via Get/SetMenuPosition | Dict storage |
| `Screen.show_cursor` | `ShowCursor` | Bool property |
| `Screen.visible_windows_to_write_positions` | `VisibleWindowsToWritePositions` | Dict property |
| `Screen.width`, `Screen.height` | `Width`, `Height` | Int properties |
| `Screen.draw_with_z_index` | `DrawWithZIndex` | Method |
| `Screen.draw_all_floats` | `DrawAllFloats` | Method |
| `Screen.append_style_to_content` | `AppendStyleToContent` | Method |
| `Screen.fill_area` | `FillArea` | Method |
| `WritePosition` (NamedTuple) | `WritePosition` (record struct) | Type |

### Performance Optimizations Ported

1. **Char equality**: Python uses `__slots__` and identity check optimization. C# uses sealed class and ReferenceEquals short-circuit in Equals.
2. **Char caching**: Python uses module-level `_CHAR_CACHE` dict with 1M limit. C# uses static FastDictCache<(string, string), Char> with same limit.
3. **Sparse storage**: Both use nested dictionaries for O(1) cell access.

### APIs Intentionally Omitted

| Python API | Reason |
|------------|--------|
| `Screen.get_cursor_position(window)` | Ported as `GetCursorPosition(IWindow)` - not omitted |
| `Screen.get_menu_position(window)` | Ported as `GetMenuPosition(IWindow)` - not omitted |

**None intentionally omitted** - All public APIs from Python PTK's screen.py are ported.

### Documented Deviations

1. **Thread Safety**: Python PTK is single-threaded. C# implementation uses Lock per Constitution XI.
2. **IWindow Interface**: Python uses `Window` class directly. C# uses IWindow marker interface for forward reference.
3. **display_mappings Location**: Python has it as Char class variable. C# extracts to CharacterDisplayMappings static class for single-responsibility.
4. **Visible Windows Access**: Python exposes dict directly. C# provides IDictionary interface with internal locking.
