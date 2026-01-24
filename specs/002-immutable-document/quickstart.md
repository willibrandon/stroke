# Quickstart: Immutable Document Text Model

**Feature**: 002-immutable-document
**Date**: 2026-01-23

## Overview

The `Document` class is the core immutable text representation in Stroke. It stores text content with a cursor position and optional selection state, providing 50+ methods for text queries, navigation, search, and clipboard operations.

## Key Concepts

### Immutability

Document is **immutable** - all operations that modify content return a **new** Document instance:

```csharp
var doc = new Document("hello world", cursorPosition: 5);

// This returns a NEW document, original is unchanged
var newDoc = doc.InsertAfter("X");

Console.WriteLine(doc.Text);     // "hello world" (unchanged)
Console.WriteLine(newDoc.Text);  // "helloX world" (new instance)
```

### Cursor Position

Cursor position is an index into the text (0-based). It can be at any position from 0 to text.Length (inclusive - the position after the last character is valid for insertion):

```csharp
// Cursor at position 5 (after "hello")
var doc = new Document("hello world", cursorPosition: 5);

Console.WriteLine(doc.TextBeforeCursor);  // "hello"
Console.WriteLine(doc.TextAfterCursor);   // " world"
Console.WriteLine(doc.CurrentChar);       // ' '

// Default: cursor at end of text
var doc2 = new Document("hello");  // cursor at position 5
Console.WriteLine(doc2.IsCursorAtTheEnd);  // true
```

### Line-Based Access

Documents split text into lines (separated by `\n`):

```csharp
var doc = new Document("line1\nline2\nline3", cursorPosition: 8);

Console.WriteLine(doc.LineCount);          // 3
Console.WriteLine(doc.CursorPositionRow);  // 1 (0-based)
Console.WriteLine(doc.CursorPositionCol);  // 2 (0-based)
Console.WriteLine(doc.CurrentLine);        // "line2"
Console.WriteLine(doc.OnFirstLine);        // false
Console.WriteLine(doc.OnLastLine);         // false
```

## Common Operations

### Text Queries

```csharp
var doc = new Document("  hello world", cursorPosition: 7);

// Text sections
doc.TextBeforeCursor;               // "  hello"
doc.TextAfterCursor;                // " world"
doc.CurrentLineBeforeCursor;        // "  hello"
doc.CurrentLineAfterCursor;         // " world"
doc.CurrentLine;                    // "  hello world"
doc.LeadingWhitespaceInCurrentLine; // "  "

// Characters
doc.CurrentChar;                    // ' '
doc.CharBeforeCursor;               // 'o'
```

### Search

```csharp
var doc = new Document("foo bar foo baz", cursorPosition: 0);

// Find forward (returns offset from cursor, or null)
int? pos = doc.Find("foo");           // null (cursor already at first "foo")
int? pos2 = doc.Find("foo", count: 2); // 8 (second "foo")

// Find with options
doc.Find("FOO", ignoreCase: true);    // works
doc.Find("bar", inCurrentLine: true); // search only in current line

// Find backward
doc.FindBackwards("foo");             // returns negative offset

// Find all (returns absolute positions)
var all = doc.FindAll("foo");         // [0, 8]
```

### Word Navigation

Document distinguishes between "word" (alphanumeric sequences) and "WORD" (non-whitespace sequences):

```csharp
var doc = new Document("hello-world test", cursorPosition: 0);

// word mode (default) - treats punctuation as word boundary
doc.FindNextWordBeginning();          // 5 (position of '-')

// WORD mode - only whitespace is boundary
doc.FindNextWordBeginning(WORD: true); // 12 (position of 't')

// Get word at/before cursor
doc.GetWordUnderCursor();             // "hello"
doc.GetWordBeforeCursor();            // ""
```

### Cursor Movement

Movement methods return **offsets** (relative positions), not new Documents:

```csharp
var doc = new Document("line1\nline2", cursorPosition: 7);

// Horizontal movement
int left = doc.GetCursorLeftPosition(2);   // -2
int right = doc.GetCursorRightPosition(3); // 3

// Vertical movement (respects line boundaries)
int up = doc.GetCursorUpPosition();        // offset to same column on line1
int down = doc.GetCursorDownPosition();    // offset (or 0 if on last line)

// Document boundaries
int start = doc.GetStartOfDocumentPosition(); // offset to position 0
int end = doc.GetEndOfDocumentPosition();     // offset to text.Length
```

### Selection Handling

```csharp
var doc = new Document(
    "hello world",
    cursorPosition: 11,
    selection: new SelectionState(originalCursorPosition: 6, type: SelectionType.Characters)
);

// Get selection range
var (start, end) = doc.SelectionRange();  // (6, 11)

// For line/block selections, get per-line ranges
foreach (var range in doc.SelectionRanges())
{
    Console.WriteLine($"{range.Start} - {range.End}");
}

// Cut selection (returns new Document + clipboard data)
var (newDoc, clipboardData) = doc.CutSelection();
Console.WriteLine(newDoc.Text);       // "hello "
Console.WriteLine(clipboardData.Text); // "world"
```

### Clipboard Operations

```csharp
var doc = new Document("hello", cursorPosition: 5);
var data = new ClipboardData("X", SelectionType.Characters);

// Paste modes
var emacs = doc.PasteClipboardData(data, PasteMode.Emacs);
// Result: "helloX" with cursor at 6

var viBefore = doc.PasteClipboardData(data, PasteMode.ViBefore);
// Result: "hellXo" with cursor at 4

var viAfter = doc.PasteClipboardData(data, PasteMode.ViAfter);
// Result: "helloX" with cursor at 5

// Paste multiple times
var multi = doc.PasteClipboardData(data, PasteMode.Emacs, count: 3);
// Result: "helloXXX"
```

### Bracket Matching

```csharp
var doc = new Document("(foo (bar) baz)", cursorPosition: 0);

// Find matching bracket
int? match = doc.FindMatchingBracketPosition(); // 14 (closing ')')

// Find enclosing brackets
var doc2 = new Document("code (inner) more", cursorPosition: 8);
int? left = doc2.FindEnclosingBracketLeft();   // offset to '('
int? right = doc2.FindEnclosingBracketRight(); // offset to ')'
```

## Performance Notes

### Flyweight Pattern

Documents with identical text share cached line data:

```csharp
var doc1 = new Document("same text");
var doc2 = new Document("same text", cursorPosition: 2);

// Both documents share the same cached Lines array
// Memory efficient for undo/redo stacks
```

### Lazy Computation

Line arrays and line start indexes are computed lazily:

```csharp
var doc = new Document("large text...");

// This does NOT parse lines
int pos = doc.CursorPosition;

// This triggers line parsing (cached afterward)
int lineCount = doc.LineCount;
```

## File Locations

| File | Purpose |
|------|---------|
| `src/Stroke/Core/Document.cs` | Main Document class |
| `src/Stroke/Core/DocumentCache.cs` | Internal flyweight cache |
| `src/Stroke/Core/SelectionState.cs` | Selection tracking |
| `src/Stroke/Core/SelectionType.cs` | Selection type enum |
| `src/Stroke/Core/PasteMode.cs` | Paste mode enum |
| `src/Stroke/Core/ClipboardData.cs` | Clipboard content |
| `tests/Stroke.Tests/Core/DocumentTests.cs` | Unit tests |

## Reference

- Python source: `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/document.py`
- API mapping: `docs/api-mapping.md` (lines 591-725)
- Test mapping: `docs/test-mapping.md` (DocumentTests section)
