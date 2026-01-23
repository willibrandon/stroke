# Feature 01: Document (Immutable Text Model)

## Overview

Implement the immutable `Document` class that represents text with cursor position and selection state. This is the core text representation used throughout Stroke.

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/document.py`

## Public API

### Document Class

```csharp
namespace Stroke.Core;

/// <summary>
/// Immutable class around text and cursor position, containing methods for querying this data.
/// This class is usually instantiated by a Buffer object and accessed as the Document property.
/// </summary>
public sealed class Document : IEquatable<Document>
{
    // Constructor
    public Document(string text = "", int? cursorPosition = null, SelectionState? selection = null);

    // Properties - Text
    public string Text { get; }
    public int CursorPosition { get; }
    public SelectionState? Selection { get; }

    // Properties - Character Access
    public string CurrentChar { get; }
    public string CharBeforeCursor { get; }

    // Properties - Text Sections
    public string TextBeforeCursor { get; }
    public string TextAfterCursor { get; }
    public string CurrentLineBeforeCursor { get; }
    public string CurrentLineAfterCursor { get; }
    public string CurrentLine { get; }
    public string LeadingWhitespaceInCurrentLine { get; }

    // Properties - Lines
    public IReadOnlyList<string> Lines { get; }
    public IReadOnlyList<string> LinesFromCurrent { get; }
    public int LineCount { get; }

    // Properties - Cursor Position
    public int CursorPositionRow { get; }
    public int CursorPositionCol { get; }
    public bool OnFirstLine { get; }
    public bool OnLastLine { get; }
    public bool IsCursorAtTheEnd { get; }
    public bool IsCursorAtTheEndOfLine { get; }

    // Methods - Position Translation
    public (int Row, int Col) TranslateIndexToPosition(int index);
    public int TranslateRowColToIndex(int row, int col);

    // Methods - Search
    public bool HasMatchAtCurrentPosition(string sub);
    public int? Find(string sub, bool inCurrentLine = false, bool includeCurrentPosition = false, bool ignoreCase = false, int count = 1);
    public IReadOnlyList<int> FindAll(string sub, bool ignoreCase = false);
    public int? FindBackwards(string sub, bool inCurrentLine = false, bool ignoreCase = false, int count = 1);

    // Methods - Word Operations
    public string GetWordBeforeCursor(bool WORD = false, Regex? pattern = null);
    public int? FindStartOfPreviousWord(int count = 1, bool WORD = false, Regex? pattern = null);
    public (int Start, int End) FindBoundariesOfCurrentWord(bool WORD = false, bool includeLeadingWhitespace = false, bool includeTrailingWhitespace = false);
    public string GetWordUnderCursor(bool WORD = false);
    public int? FindNextWordBeginning(int count = 1, bool WORD = false);
    public int? FindNextWordEnding(bool includeCurrentPosition = false, int count = 1, bool WORD = false);
    public int? FindPreviousWordBeginning(int count = 1, bool WORD = false);
    public int? FindPreviousWordEnding(int count = 1, bool WORD = false);

    // Methods - Line Operations
    public int? FindNextMatchingLine(Func<string, bool> matchFunc, int count = 1);
    public int? FindPreviousMatchingLine(Func<string, bool> matchFunc, int count = 1);

    // Methods - Cursor Movement
    public int GetCursorLeftPosition(int count = 1);
    public int GetCursorRightPosition(int count = 1);
    public int GetCursorUpPosition(int count = 1, int? preferredColumn = null);
    public int GetCursorDownPosition(int count = 1, int? preferredColumn = null);

    // Methods - Bracket Matching
    public int? FindEnclosingBracketRight(char leftCh, char rightCh, int? endPos = null);
    public int? FindEnclosingBracketLeft(char leftCh, char rightCh, int? startPos = null);
    public int FindMatchingBracketPosition(int? startPos = null, int? endPos = null);

    // Methods - Document Navigation
    public int GetStartOfDocumentPosition();
    public int GetEndOfDocumentPosition();
    public int GetStartOfLinePosition(bool afterWhitespace = false);
    public int GetEndOfLinePosition();
    public int LastNonBlankOfCurrentLinePosition();
    public int GetColumnCursorPosition(int column);

    // Methods - Selection
    public (int From, int To) SelectionRange();
    public IEnumerable<(int From, int To)> SelectionRanges();
    public (int From, int To)? SelectionRangeAtLine(int row);
    public (Document Document, ClipboardData Data) CutSelection();

    // Methods - Clipboard
    public Document PasteClipboardData(ClipboardData data, PasteMode pasteMode = PasteMode.Emacs, int count = 1);

    // Methods - Paragraph
    public int EmptyLineCountAtTheEnd();
    public int StartOfParagraph(int count = 1, bool before = false);
    public int EndOfParagraph(int count = 1, bool after = false);

    // Methods - Modifiers (return new Document)
    public Document InsertAfter(string text);
    public Document InsertBefore(string text);

    // IEquatable
    public bool Equals(Document? other);
    public override bool Equals(object? obj);
    public override int GetHashCode();
    public override string ToString();
}
```

### Internal Cache (Private Implementation)

```csharp
// Internal: Shared cache for Document instances with same text
internal sealed class DocumentCache
{
    internal IReadOnlyList<string>? Lines { get; set; }
    internal IReadOnlyList<int>? LineIndexes { get; set; }
}
```

## Project Structure

```
src/Stroke/
└── Core/
    └── Document.cs
tests/Stroke.Tests/
└── Core/
    └── DocumentTests.cs
```

## Implementation Notes

### Flyweight Pattern

The Python implementation uses a `WeakValueDictionary` to share `_DocumentCache` instances between `Document` objects with the same text. Port this using `ConditionalWeakTable<string, DocumentCache>`.

### Word Regex Patterns

Port the following regex patterns exactly:
- `_FIND_WORD_RE` → `FindWordRegex`
- `_FIND_CURRENT_WORD_RE` → `FindCurrentWordRegex`
- `_FIND_CURRENT_WORD_INCLUDE_TRAILING_WHITESPACE_RE` → `FindCurrentWordIncludeTrailingWhitespaceRegex`
- `_FIND_BIG_WORD_RE` → `FindBigWordRegex`
- `_FIND_CURRENT_BIG_WORD_RE` → `FindCurrentBigWordRegex`
- `_FIND_CURRENT_BIG_WORD_INCLUDE_TRAILING_WHITESPACE_RE` → `FindCurrentBigWordIncludeTrailingWhitespaceRegex`

### WORD vs word

The `WORD` parameter (uppercase) follows Vi terminology:
- `word` = alphanumeric characters and underscores
- `WORD` = any non-whitespace characters

## Dependencies

- `Stroke.Core.Primitives` (Point, Size from Feature 00)
- `Stroke.Core.Selection` (SelectionState, SelectionType, PasteMode from Feature 02)
- `Stroke.Core.Clipboard` (ClipboardData from Feature 03)

## Implementation Tasks

1. Implement `Document` class with all properties and methods
2. Implement `DocumentCache` with flyweight pattern
3. Implement all word-finding regex patterns
4. Write comprehensive unit tests

## Acceptance Criteria

- [ ] All 50+ Document methods match Python Prompt Toolkit semantics
- [ ] Flyweight caching works correctly for shared text
- [ ] Lazy property computation for Lines and LineIndexes
- [ ] All word operations work correctly with both word and WORD modes
- [ ] Selection operations work correctly with all SelectionType values
- [ ] Unit tests achieve 80% coverage
