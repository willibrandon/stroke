# Research: Immutable Document Text Model

**Feature**: 002-immutable-document
**Date**: 2026-01-23
**Source**: Python Prompt Toolkit `prompt_toolkit/document.py` (1182 lines)

## Overview

No Technical Context items required explicit clarification. The Python source at `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/document.py` provides complete implementation guidance, and `docs/api-mapping.md` provides exact API mappings.

## Key Research Findings

### 1. Document Class Architecture

**Decision**: Sealed class with private backing fields
**Rationale**: Matches Python implementation's immutability guarantee via `__slots__`
**Alternatives considered**: Record type (rejected - too many computed properties)

The Document class uses:
- `__slots__ = ("_text", "_cursor_position", "_selection", "_cache")` for memory efficiency
- Private fields with read-only public properties
- No setters - all mutations return new Document instances

### 2. Flyweight Pattern for Line Caching

**Decision**: `ConditionalWeakTable<string, DocumentCache>` for C# implementation
**Rationale**: C# equivalent of Python's `weakref.WeakValueDictionary`
**Alternatives considered**:
- Regular `Dictionary<string, DocumentCache>` (rejected - memory leak on large texts)
- `WeakReference<DocumentCache>` (rejected - ConditionalWeakTable is cleaner)

Python implementation:
```python
_text_to_document_cache: dict[str, _DocumentCache] = cast(
    Dict[str, "_DocumentCache"],
    weakref.WeakValueDictionary(),
)
```

C# equivalent:
```csharp
private static readonly ConditionalWeakTable<string, DocumentCache> _textToCache = new();
```

**Important - String Key Behavior**:

In C#, `ConditionalWeakTable<TKey, TValue>` uses reference equality for keys. For strings:
- String literals and interned strings share references, so they will correctly share caches
- Dynamically created strings with identical content may have different references

**Resolution**: This is acceptable because:
1. Most Document text comes from user input or file reads, which typically creates distinct string instances
2. The flyweight optimization is a performance enhancement, not a correctness requirement
3. If two strings happen to share the same reference (interning), they correctly share the cache
4. If they don't share references, each gets its own cache - still functionally correct, just uses more memory

**Alternative considered**: Using a `Dictionary<string, WeakReference<DocumentCache>>` with string value equality, but this:
- Requires periodic cleanup of dead weak references
- More complex implementation
- Not necessary since the Python behavior also uses object identity for the weak dict

**Decision**: Use `ConditionalWeakTable<string, DocumentCache>` directly. Document this behavior in XML comments.

### 3. Word Navigation Regex Patterns

**Decision**: Port exact regex patterns from Python
**Rationale**: Vi/Emacs mode compatibility requires identical word boundary detection

Python patterns:
```python
# "word" - alphanumeric groups OR punctuation groups (excluding whitespace)
_FIND_WORD_RE = re.compile(r"([a-zA-Z0-9_]+|[^a-zA-Z0-9_\s]+)")
_FIND_CURRENT_WORD_RE = re.compile(r"^([a-zA-Z0-9_]+|[^a-zA-Z0-9_\s]+)")
_FIND_CURRENT_WORD_INCLUDE_TRAILING_WHITESPACE_RE = re.compile(
    r"^(([a-zA-Z0-9_]+|[^a-zA-Z0-9_\s]+)\s*)"
)

# "WORD" - any non-whitespace sequence
_FIND_BIG_WORD_RE = re.compile(r"([^\s]+)")
_FIND_CURRENT_BIG_WORD_RE = re.compile(r"^([^\s]+)")
_FIND_CURRENT_BIG_WORD_INCLUDE_TRAILING_WHITESPACE_RE = re.compile(r"^([^\s]+\s*)")
```

C# equivalents:
```csharp
private static readonly Regex FindWordRegex = new(@"([a-zA-Z0-9_]+|[^a-zA-Z0-9_\s]+)");
private static readonly Regex FindCurrentWordRegex = new(@"^([a-zA-Z0-9_]+|[^a-zA-Z0-9_\s]+)");
// etc.
```

### 4. Line Index Computation

**Decision**: Lazy computation with cumulative sum algorithm
**Rationale**: Matches Python implementation for O(n) line start index lookup

Python implementation:
```python
def _line_start_indexes(self) -> list[int]:
    if self._cache.line_indexes is None:
        line_lengths = map(len, self.lines)
        indexes = [0]
        pos = 0
        for line_length in line_lengths:
            pos += line_length + 1
            indexes.append(pos)
        if len(indexes) > 1:
            indexes.pop()
        self._cache.line_indexes = indexes
    return self._cache.line_indexes
```

**Key insight**: Uses `bisect.bisect_right` for O(log n) row/column lookup:
```python
def _find_line_start_index(self, index: int) -> tuple[int, int]:
    indexes = self._line_start_indexes
    pos = bisect.bisect_right(indexes, index) - 1
    return pos, indexes[pos]
```

### 5. Selection and Clipboard Dependencies

**Decision**: Create `SelectionState`, `SelectionType`, `PasteMode`, and `ClipboardData` as dependencies
**Rationale**: Document imports these from `prompt_toolkit.selection` and `prompt_toolkit.clipboard`

Python imports:
```python
from .clipboard import ClipboardData
from .filters import vi_mode
from .selection import PasteMode, SelectionState, SelectionType
```

Required types in Stroke.Core:
- `SelectionType` enum: `Characters`, `Lines`, `Block`
- `PasteMode` enum: `Emacs`, `ViBefore`, `ViAfter`
- `SelectionState` class: `OriginalCursorPosition`, `Type`, `ShiftMode`
- `ClipboardData` class: `Text`, `Type`

**Note**: `vi_mode()` filter dependency is for selection boundary behavior - will be addressed in Filters module (documented in spec Assumptions).

### 6. ImmutableLineList Protection

**Decision**: Use `ImmutableArray<string>` for Lines property
**Rationale**: C# immutable collections provide compile-time and runtime protection

Python uses a custom `_ImmutableLineList` class that overrides all mutating methods to throw:
```python
class _ImmutableLineList(List[str]):
    def _error(self, *a, **kw) -> NoReturn:
        raise NotImplementedError("Attempt to modify an immutable list.")
    __setitem__ = _error
    append = _error
    # etc.
```

C# equivalent: `ImmutableArray<string>` - immutable by design.

### 7. Equality and Hashing

**Decision**: Implement value equality based on text, cursor position, and selection
**Rationale**: Matches Python `__eq__` implementation

Python:
```python
def __eq__(self, other: object) -> bool:
    if not isinstance(other, Document):
        return False
    return (
        self.text == other.text
        and self.cursor_position == other.cursor_position
        and self.selection == other.selection
    )
```

C# should also override `GetHashCode()` for proper dictionary/set behavior.

### 8. Bracket Matching

**Decision**: Support `()`, `[]`, `{}`, `<>` pairs with nesting
**Rationale**: Required for Vi `%` motion and Emacs C-M-f/C-M-b

Methods to implement:
- `FindMatchingBracketPosition()` - finds matching bracket from current position
- `FindEnclosingBracketLeft()` - finds opening bracket to the left
- `FindEnclosingBracketRight()` - finds closing bracket to the right

## API Summary (from api-mapping.md)

### Properties (20)
1. `Text` - the document text
2. `CursorPosition` - cursor index
3. `Selection` - optional SelectionState
4. `CurrentChar` - char at cursor
5. `CharBeforeCursor` - char before cursor
6. `TextBeforeCursor` - substring before cursor
7. `TextAfterCursor` - substring after cursor
8. `CurrentLineBeforeCursor` - line portion before cursor
9. `CurrentLineAfterCursor` - line portion after cursor
10. `CurrentLine` - entire current line
11. `Lines` - all lines as ImmutableArray
12. `LineCount` - number of lines
13. `CursorPositionRow` - 0-based row
14. `CursorPositionCol` - 0-based column
15. `IsCursorAtTheEnd` - at document end
16. `IsCursorAtTheEndOfLine` - at line end
17. `LeadingWhitespaceInCurrentLine` - indentation
18. `OnFirstLine` - cursor on first line
19. `OnLastLine` - cursor on last line
20. `EmptyLineCountAtTheEnd` - trailing empty lines

### Methods (30+)
See `docs/api-mapping.md` lines 591-725 for complete method signatures.

## Test Coverage Target

Per `docs/test-mapping.md`, DocumentTests requires 12 test methods:
1. `CurrentChar`
2. `TextBeforeCursor`
3. `TextAfterCursor`
4. `Lines`
5. `LineCount`
6. `CurrentLineBeforeCursor`
7. `CurrentLineAfterCursor`
8. `CurrentLine`
9. `CursorPosition`
10. `TranslateIndexToPosition`
11. `IsCursorAtTheEnd`
12. `GetWordBeforeCursor_WithWhitespaceAndPattern`

Additional tests needed for:
- Selection operations (SelectionRange, SelectionRanges, CutSelection)
- Paste operations (PasteClipboardData)
- Word navigation (WORD vs word distinction)
- Bracket matching
- Paragraph navigation

## Dependencies Graph

```
Document
├── SelectionState (optional)
│   └── SelectionType
├── ClipboardData (for CutSelection/PasteClipboardData)
│   └── SelectionType
├── PasteMode (for PasteClipboardData)
└── DocumentCache (internal, shared via flyweight)
```

## Implementation Order

1. **Enums first**: `SelectionType`, `PasteMode`
2. **Data classes**: `SelectionState`, `ClipboardData`, `DocumentCache`
3. **Document core**: Constructor, properties, equality
4. **Document navigation**: Cursor movement, line operations
5. **Document search**: Find, FindAll, FindBackwards
6. **Document word ops**: Word boundary detection
7. **Document selection**: SelectionRange, SelectionRanges
8. **Document clipboard**: CutSelection, PasteClipboardData
9. **Tests**: Following test-mapping.md exactly
