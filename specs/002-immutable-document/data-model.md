# Data Model: Immutable Document Text Model

**Feature**: 002-immutable-document
**Date**: 2026-01-23

## Entity Definitions

### 1. SelectionType (Enum)

**Namespace**: `Stroke.Core`
**Source**: `prompt_toolkit.selection.SelectionType`

Represents the type of text selection.

| Value | Description | Vi Equivalent |
|-------|-------------|---------------|
| `Characters` | Character-by-character selection | Visual mode (`v`) |
| `Lines` | Whole line selection | Visual-Line mode (`V`) |
| `Block` | Rectangular block selection | Visual-Block mode (`Ctrl+v`) |

```csharp
namespace Stroke.Core;

/// <summary>
/// Type of selection.
/// </summary>
public enum SelectionType
{
    /// <summary>Character selection (Visual in Vi).</summary>
    Characters,

    /// <summary>Whole line selection (Visual-Line in Vi).</summary>
    Lines,

    /// <summary>Block/rectangular selection (Visual-Block in Vi).</summary>
    Block
}
```

### 2. PasteMode (Enum)

**Namespace**: `Stroke.Core`
**Source**: `prompt_toolkit.selection.PasteMode`

Determines how clipboard content is pasted relative to cursor.

| Value | Description | Mode |
|-------|-------------|------|
| `Emacs` | Yank at cursor position | Emacs |
| `ViBefore` | Insert before cursor (P command) | Vi |
| `ViAfter` | Insert after cursor (p command) | Vi |

```csharp
namespace Stroke.Core;

/// <summary>
/// Mode for paste operations.
/// </summary>
public enum PasteMode
{
    /// <summary>Yank like Emacs (at cursor position).</summary>
    Emacs,

    /// <summary>Vi paste after cursor ('p').</summary>
    ViAfter,

    /// <summary>Vi paste before cursor ('P').</summary>
    ViBefore
}
```

### 3. SelectionState (Class)

**Namespace**: `Stroke.Core`
**Source**: `prompt_toolkit.selection.SelectionState`

Tracks the state of the current selection.

| Field | Type | Description |
|-------|------|-------------|
| `OriginalCursorPosition` | `int` | Starting position when selection began |
| `Type` | `SelectionType` | Type of selection |
| `ShiftMode` | `bool` | Whether shift key initiated selection |

**Mutability Decision**: SelectionState is **mutable** to match Python Prompt Toolkit exactly (Constitution Principle I). The `EnterShiftMode()` method mutates the instance. This is acceptable because SelectionState is a transient state object, not a core immutable data structure like Document.

```csharp
namespace Stroke.Core;

/// <summary>
/// State of the current selection.
/// </summary>
public sealed class SelectionState
{
    public SelectionState(
        int originalCursorPosition = 0,
        SelectionType type = SelectionType.Characters)
    {
        OriginalCursorPosition = originalCursorPosition;
        Type = type;
        ShiftMode = false;
    }

    public int OriginalCursorPosition { get; }
    public SelectionType Type { get; }
    public bool ShiftMode { get; private set; }

    /// <summary>
    /// Enter shift selection mode.
    /// </summary>
    public void EnterShiftMode() => ShiftMode = true;
}
```

### 4. ClipboardData (Class)

**Namespace**: `Stroke.Core`
**Source**: `prompt_toolkit.clipboard.ClipboardData`

Contains text content for clipboard operations.

| Field | Type | Description |
|-------|------|-------------|
| `Text` | `string` | The clipboard text content |
| `Type` | `SelectionType` | Type of selection that produced this content |

```csharp
namespace Stroke.Core;

/// <summary>
/// Text data on the clipboard.
/// </summary>
public sealed class ClipboardData
{
    public ClipboardData(string text = "", SelectionType type = SelectionType.Characters)
    {
        Text = text;
        Type = type;
    }

    public string Text { get; }
    public SelectionType Type { get; }
}
```

### 5. DocumentCache (Internal Class)

**Namespace**: `Stroke.Core`
**Source**: `prompt_toolkit.document._DocumentCache`

Internal cache for lazily computed line data. Shared between Document instances with identical text content (flyweight pattern).

| Field | Type | Description |
|-------|------|-------------|
| `Lines` | `ImmutableArray<string>?` | Cached line array |
| `LineIndexes` | `int[]?` | Cached line start positions |

```csharp
namespace Stroke.Core;

/// <summary>
/// Internal cache for Document line data.
/// </summary>
internal sealed class DocumentCache
{
    /// <summary>Cached lines array (null until computed).</summary>
    public ImmutableArray<string>? Lines { get; set; }

    /// <summary>Cached line start indexes (null until computed).</summary>
    public int[]? LineIndexes { get; set; }
}
```

### 6. Document (Class)

**Namespace**: `Stroke.Core`
**Source**: `prompt_toolkit.document.Document`

The core immutable text representation with cursor position and optional selection state.

#### Core Fields

| Field | Type | Description |
|-------|------|-------------|
| `_text` | `string` | The document text |
| `_cursorPosition` | `int` | Cursor position (0 to text.Length inclusive) |
| `_selection` | `SelectionState?` | Optional selection state |
| `_cache` | `DocumentCache` | Shared cache reference |

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Text` | `string` | The document text |
| `CursorPosition` | `int` | Cursor position |
| `Selection` | `SelectionState?` | Selection state |
| `CurrentChar` | `char` | Character at cursor (or '\0') |
| `CharBeforeCursor` | `char` | Character before cursor (or '\0') |
| `TextBeforeCursor` | `string` | Text from start to cursor |
| `TextAfterCursor` | `string` | Text from cursor to end |
| `CurrentLineBeforeCursor` | `string` | Current line portion before cursor |
| `CurrentLineAfterCursor` | `string` | Current line portion after cursor |
| `CurrentLine` | `string` | Entire current line |
| `Lines` | `IReadOnlyList<string>` | All lines |
| `LinesFromCurrent` | `IReadOnlyList<string>` | Lines from current to end |
| `LineCount` | `int` | Number of lines |
| `CursorPositionRow` | `int` | 0-based row |
| `CursorPositionCol` | `int` | 0-based column |
| `IsCursorAtTheEnd` | `bool` | At document end |
| `IsCursorAtTheEndOfLine` | `bool` | At line end |
| `LeadingWhitespaceInCurrentLine` | `string` | Indentation |
| `OnFirstLine` | `bool` | On first line |
| `OnLastLine` | `bool` | On last line |
| `EmptyLineCountAtTheEnd` | `int` | Trailing empty lines |

#### Methods

| Method | Return Type | Description |
|--------|-------------|-------------|
| `GetWordBeforeCursor(WORD, pattern)` | `string` | Word before cursor |
| `GetWordUnderCursor(WORD)` | `string` | Word at cursor |
| `Find(sub, inCurrentLine, includeCurrentPosition, ignoreCase, count)` | `int?` | Find forward |
| `FindAll(sub, ignoreCase)` | `IReadOnlyList<int>` | Find all occurrences |
| `FindBackwards(sub, inCurrentLine, ignoreCase, count)` | `int?` | Find backward |
| `GetCursorLeftPosition(count)` | `int` | Left movement offset |
| `GetCursorRightPosition(count)` | `int` | Right movement offset |
| `GetCursorUpPosition(count, preferredColumn)` | `int` | Up movement offset |
| `GetCursorDownPosition(count, preferredColumn)` | `int` | Down movement offset |
| `GetStartOfLinePosition(afterWhitespace)` | `int` | Start of line offset |
| `GetEndOfLinePosition()` | `int` | End of line offset |
| `GetStartOfDocumentPosition()` | `int` | Start of document offset |
| `GetEndOfDocumentPosition()` | `int` | End of document offset |
| `FindMatchingBracketPosition()` | `int?` | Matching bracket offset |
| `FindEnclosingBracketLeft(...)` | `int?` | Enclosing open bracket |
| `FindEnclosingBracketRight(...)` | `int?` | Enclosing close bracket |
| `TranslateIndexToPosition(index)` | `(int Row, int Col)` | Index to position |
| `TranslateRowColToIndex(row, col)` | `int` | Position to index |
| `GetColumnCursorPosition(column)` | `int` | Column position offset |
| `SelectionRange()` | `(int Start, int End)` | Selection boundaries |
| `SelectionRanges()` | `IEnumerable<(int, int)>` | Per-line ranges |
| `SelectionRangeAtLine(row)` | `(int, int)?` | Range at specific line |
| `CutSelection()` | `(Document, ClipboardData)` | Cut and return new doc |
| `PasteClipboardData(data, pasteMode, count)` | `Document` | Paste and return new doc |
| `InsertAfter(text)` | `Document` | Insert after cursor |
| `InsertBefore(text)` | `Document` | Insert before cursor |
| `FindNextWordBeginning(...)` | `int?` | Next word start |
| `FindNextWordEnding(...)` | `int?` | Next word end |
| `FindPreviousWordBeginning(...)` | `int?` | Previous word start |
| `FindPreviousWordEnding(...)` | `int?` | Previous word end |
| `StartOfParagraph()` | `int` | Paragraph start offset |
| `EndOfParagraph()` | `int` | Paragraph end offset |
| `HasMatchAtCurrentPosition(sub)` | `bool` | True if substring found at cursor |
| `FindStartOfPreviousWord(count, WORD, pattern)` | `int?` | Start of previous word |
| `FindBoundariesOfCurrentWord(WORD, includeLeading, includeTrailing)` | `(int, int)` | Word boundary offsets |

#### Method Signatures Detail

**Word Navigation Methods** (full signatures):
```csharp
int? FindNextWordBeginning(int count = 1, bool WORD = false);
int? FindNextWordEnding(int count = 1, bool WORD = false);
int? FindPreviousWordBeginning(int count = 1, bool WORD = false);
int? FindPreviousWordEnding(int count = 1, bool WORD = false);
int? FindStartOfPreviousWord(int count = 1, bool WORD = false, Regex? pattern = null);
(int Start, int End) FindBoundariesOfCurrentWord(bool WORD = false, bool includeLeadingWhitespace = false, bool includeTrailingWhitespace = false);
string GetWordBeforeCursor(bool WORD = false, Regex? pattern = null);
string GetWordUnderCursor(bool WORD = false);
```

**Search Methods** (full signatures):
```csharp
int? Find(string sub, bool inCurrentLine = false, bool includeCurrentPosition = false, bool ignoreCase = false, int count = 1);
IReadOnlyList<int> FindAll(string sub, bool ignoreCase = false);
int? FindBackwards(string sub, bool inCurrentLine = false, bool ignoreCase = false, int count = 1);
bool HasMatchAtCurrentPosition(string sub);
```

**Cursor Movement Methods** (full signatures):
```csharp
int GetCursorLeftPosition(int count = 1);
int GetCursorRightPosition(int count = 1);
int GetCursorUpPosition(int count = 1, int? preferredColumn = null);
int GetCursorDownPosition(int count = 1, int? preferredColumn = null);
int GetStartOfLinePosition(bool afterWhitespace = false);
int GetEndOfLinePosition();
int GetStartOfDocumentPosition();
int GetEndOfDocumentPosition();
int GetColumnCursorPosition(int column);
```

**Selection/Clipboard Methods** (full signatures):
```csharp
(int Start, int End) SelectionRange();
IEnumerable<(int Start, int End)> SelectionRanges();
(int Start, int End)? SelectionRangeAtLine(int row);
(Document Document, ClipboardData Data) CutSelection();
Document PasteClipboardData(ClipboardData data, PasteMode pasteMode = PasteMode.Emacs, int count = 1);
```

## Relationships

```
                    ┌──────────────────┐
                    │   SelectionType  │
                    │      (enum)      │
                    └────────┬─────────┘
                             │ used by
            ┌────────────────┼────────────────┐
            │                │                │
            ▼                ▼                ▼
┌───────────────────┐ ┌──────────────┐ ┌─────────────────┐
│  SelectionState   │ │ ClipboardData│ │    PasteMode    │
│     (class)       │ │   (class)    │ │     (enum)      │
└────────┬──────────┘ └──────┬───────┘ └────────┬────────┘
         │                   │                  │
         │                   │                  │
         └───────────────────┼──────────────────┘
                             │ used by
                             ▼
                    ┌────────────────────┐
                    │     Document       │
                    │     (class)        │
                    └────────┬───────────┘
                             │ shares
                             ▼
                    ┌────────────────────┐
                    │   DocumentCache    │
                    │ (internal class)   │
                    └────────────────────┘
```

## Validation Rules

### Document

1. `cursorPosition` MUST be >= 0
2. `cursorPosition` MUST be <= text.Length (can be at insertion point)
3. When `cursorPosition` is null, default to text.Length
4. When accessing `Lines`, lazily compute and cache
5. When accessing `LineIndexes`, lazily compute and cache
6. Cache MUST be shared between Document instances with identical text

### SelectionState

1. `originalCursorPosition` MUST be >= 0
2. `type` MUST be a valid SelectionType value
3. `shiftMode` starts as false, can only transition to true

### ClipboardData

1. `text` MUST NOT be null (use empty string)
2. `type` MUST be a valid SelectionType value

## State Transitions

### Document

Document is immutable - no state transitions. Mutation operations return new Document instances:

```
Document(text="hello", cursor=2)
    │
    │ PasteClipboardData(data="X", mode=Emacs)
    ▼
Document(text="heXllo", cursor=3)
```

### SelectionState

```
SelectionState(shiftMode=false)
    │
    │ EnterShiftMode()
    ▼
SelectionState(shiftMode=true)
```

Note: Once in shift mode, there is no method to exit. A new SelectionState must be created.
