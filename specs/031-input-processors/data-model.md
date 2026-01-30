# Data Model: Input Processors

**Feature**: 031-input-processors
**Date**: 2026-01-29

## Entity Diagram

```
IProcessor (interface)
    │
    ├── DummyProcessor
    ├── PasswordProcessor
    ├── HighlightSearchProcessor
    │       └── HighlightIncrementalSearchProcessor (inherits)
    ├── HighlightSelectionProcessor
    ├── HighlightMatchingBracketProcessor
    ├── DisplayMultipleCursors
    ├── BeforeInput
    │       └── ShowArg (inherits)
    ├── AfterInput
    ├── AppendAutoSuggestion
    ├── ShowLeadingWhiteSpaceProcessor
    ├── ShowTrailingWhiteSpaceProcessor
    ├── TabsProcessor
    ├── ReverseSearchProcessor
    ├── ConditionalProcessor (wraps IProcessor + IFilter)
    ├── DynamicProcessor (wraps Func<IProcessor?>)
    └── _MergedProcessor (internal, wraps List<IProcessor>)

TransformationInput ──→ IProcessor ──→ Transformation
     │                                       │
     ├── BufferControl                       ├── Fragments (List<StyleAndTextTuple>)
     ├── Document                            ├── SourceToDisplay (Func<int, int>)
     ├── LineNumber (int)                    └── DisplayToSource (Func<int, int>)
     ├── SourceToDisplay (Func<int, int>)
     ├── Fragments (IReadOnlyList<StyleAndTextTuple>)
     ├── Width (int)
     ├── Height (int)
     └── GetLine (Func<int, IReadOnlyList<StyleAndTextTuple>>?)

ExplodedList : Collection<StyleAndTextTuple>
     └── Exploded = true (auto-explodes on mutation)
```

## Entities

### IProcessor

| Field | Type | Description |
|-------|------|-------------|
| `ApplyTransformation` | Method: `TransformationInput → Transformation` | Transform fragments for one line |

**Validation**: None (interface contract).
**State transitions**: Stateless interface.
**Relationships**: Consumed by `BufferControl` rendering pipeline. Composed by `_MergedProcessor` and `ConditionalProcessor`.

---

### TransformationInput

| Field | Type | Description |
|-------|------|-------------|
| `BufferControl` | `BufferControl` | The control being rendered |
| `Document` | `Document` | Current document state |
| `LineNumber` | `int` | Line number (0-indexed) being transformed |
| `SourceToDisplay` | `Func<int, int>` | Position mapping from prior processors |
| `Fragments` | `IReadOnlyList<StyleAndTextTuple>` | Input fragments to transform |
| `Width` | `int` | Available viewport width |
| `Height` | `int` | Available viewport height |
| `GetLine` | `Func<int, IReadOnlyList<StyleAndTextTuple>>?` | Access other lines' fragments |

**Validation**: `BufferControl` and `Document` must not be null. `LineNumber` must be ≥ 0.
**State transitions**: Immutable — created once per processor invocation.
**Relationships**: Created by rendering pipeline or `_MergedProcessor`. Passed to `IProcessor.ApplyTransformation`.

**Unpack method**: Returns tuple of `(BufferControl, Document, int, Func<int,int>, IReadOnlyList<StyleAndTextTuple>, int, int)` for pattern-matched destructuring.

---

### Transformation

| Field | Type | Description |
|-------|------|-------------|
| `Fragments` | `IReadOnlyList<StyleAndTextTuple>` | Transformed output fragments |
| `SourceToDisplay` | `Func<int, int>` | Forward position mapping (defaults to identity) |
| `DisplayToSource` | `Func<int, int>` | Reverse position mapping (defaults to identity) |

**Validation**: `Fragments` must not be null.
**State transitions**: Immutable — created once per processor result.
**Relationships**: Returned by `IProcessor.ApplyTransformation`. Consumed by rendering pipeline or next processor in chain.

---

### Delegate Types

| Name | Signature | Description |
|------|-----------|-------------|
| `SourceToDisplay` | `Func<int, int>` | Maps source column → display column |
| `DisplayToSource` | `Func<int, int>` | Maps display column → source column |

These are type aliases (not distinct delegate types) since C# `Func<int, int>` is sufficient.

---

### DummyProcessor

| Field | Type | Description |
|-------|------|-------------|
| (none) | — | Stateless; returns fragments unchanged |

---

### PasswordProcessor

| Field | Type | Description |
|-------|------|-------------|
| `Char` | `string` | Mask character (default `"*"`) |

---

### HighlightSearchProcessor

| Field | Type | Description |
|-------|------|-------------|
| `ClassName` | `string` (protected) | Style class name (default `"search"`) |
| `ClassNameCurrent` | `string` (protected) | Current match style class (default `"search.current"`) |

**Virtual method**: `GetSearchText(BufferControl) → string` — overridden by `HighlightIncrementalSearchProcessor`.

---

### HighlightIncrementalSearchProcessor (extends HighlightSearchProcessor)

| Field | Type | Description |
|-------|------|-------------|
| `ClassName` | `string` (override) | `"incsearch"` |
| `ClassNameCurrent` | `string` (override) | `"incsearch.current"` |

**Override**: `GetSearchText` reads from `BufferControl.SearchBuffer.Text` instead of `BufferControl.SearchState.Text`.

---

### HighlightSelectionProcessor

| Field | Type | Description |
|-------|------|-------------|
| (none) | — | Stateless; uses `Document.SelectionRangeAtLine` |

---

### HighlightMatchingBracketProcessor

| Field | Type | Description |
|-------|------|-------------|
| `Chars` | `string` | Bracket characters (default `"[](){}<>"`) |
| `MaxCursorDistance` | `int` | Search distance limit (default `1000`) |
| `_positionsCache` | `SimpleCache<object, List<(int Row, int Col)>>` | Render-cycle cache |

**Thread safety**: `_positionsCache` is `SimpleCache` which is already thread-safe.

---

### DisplayMultipleCursors

| Field | Type | Description |
|-------|------|-------------|
| (none) | — | Stateless; uses `AppFilters.ViInsertMultipleMode` and `Buffer.MultipleCursorPositions` |

---

### BeforeInput

| Field | Type | Description |
|-------|------|-------------|
| `Text` | `AnyFormattedText` | Text to prepend (plain or callable) |
| `Style` | `string` | Style to apply (default `""`) |

---

### ShowArg (extends BeforeInput)

| Field | Type | Description |
|-------|------|-------------|
| (none) | — | Passes a callable to `BeforeInput` that reads `KeyProcessor.Arg` |

---

### AfterInput

| Field | Type | Description |
|-------|------|-------------|
| `Text` | `AnyFormattedText` | Text to append (plain or callable) |
| `Style` | `string` | Style to apply (default `""`) |

---

### AppendAutoSuggestion

| Field | Type | Description |
|-------|------|-------------|
| `Style` | `string` | Style for suggestion text (default `"class:auto-suggestion"`) |

---

### ShowLeadingWhiteSpaceProcessor

| Field | Type | Description |
|-------|------|-------------|
| `GetChar` | `Func<string>` | Returns the visible replacement character |
| `Style` | `string` | Style class (default `"class:leading-whitespace"`) |

---

### ShowTrailingWhiteSpaceProcessor

| Field | Type | Description |
|-------|------|-------------|
| `GetChar` | `Func<string>` | Returns the visible replacement character |
| `Style` | `string` | Style class (default `"class:trailing-whitespace"`) |

**Deviation**: Python uses `"class:training-whitespace"` (typo). C# uses `"class:trailing-whitespace"` (fixed).

---

### TabsProcessor

| Field | Type | Description |
|-------|------|-------------|
| `TabStop` | `int` or `Func<int>` | Tab width (default `4`) |
| `Char1` | `string` or `Func<string>` | First tab character (default `"|"`) |
| `Char2` | `string` or `Func<string>` | Remaining tab characters (default `"\u2508"`) |
| `Style` | `string` | Style class (default `"class:tab"`) |

Uses `ConversionUtils.ToInt` and `ConversionUtils.ToStr` for lazy evaluation.

---

### ReverseSearchProcessor

| Field | Type | Description |
|-------|------|-------------|
| `ExcludedInputProcessors` | `List<Type>` (static) | Processor types to filter out: `HighlightSearchProcessor`, `HighlightSelectionProcessor`, `BeforeInput`, `AfterInput` |

**Relationships**: Accesses `Layout.SearchTargetBufferControl`, `BufferControl.InputProcessors`, `BufferControl.Lexer`. Creates temporary `BufferControl` for content rendering.

---

### ConditionalProcessor

| Field | Type | Description |
|-------|------|-------------|
| `Processor` | `IProcessor` | Wrapped processor |
| `Filter` | `IFilter` | Condition for activation |

---

### DynamicProcessor

| Field | Type | Description |
|-------|------|-------------|
| `GetProcessor` | `Func<IProcessor?>` | Factory callable |

---

### _MergedProcessor (internal)

| Field | Type | Description |
|-------|------|-------------|
| `Processors` | `IReadOnlyList<IProcessor>` | Chained processors |

---

### ExplodedList

| Field | Type | Description |
|-------|------|-------------|
| `Exploded` | `bool` | Always `true` — marks as already exploded |

**Base class**: `Collection<StyleAndTextTuple>`
**Behavior**: Auto-explodes fragments on `InsertItem`, `SetItem`, and `AddRange`.

---

### ProcessorUtils (static class)

| Method | Signature | Description |
|--------|-----------|-------------|
| `MergeProcessors` | `(IReadOnlyList<IProcessor>) → IProcessor` | Combines processors into one |

Returns `DummyProcessor` for empty list, the single processor for length-1 list, or `_MergedProcessor` otherwise.

---

## Prerequisite Entity Changes

### BufferControl (existing, modified)

| New Field | Type | Description |
|-----------|------|-------------|
| `InputProcessors` | `IReadOnlyList<IProcessor>?` | Custom processors |
| `IncludeDefaultInputProcessors` | `bool` | Include default processors (default `true`) |
| `DefaultInputProcessors` | `IReadOnlyList<IProcessor>` | Default processor set |
| `SearchBufferControl` | `SearchBufferControl?` | Linked search control (object form or evaluated from factory) |
| `SearchBufferControlFactory` | `Func<SearchBufferControl?>?` | Callable form of search buffer control (constructor parameter; evaluated lazily by `SearchBufferControl` property) |
| `SearchBuffer` | `Buffer?` | Search buffer (from search control) |
| `SearchState` | `SearchState` | Search state (from search control) |

### Layout (existing, modified)

| New Field | Type | Description |
|-----------|------|-------------|
| `SearchTargetBufferControl` | `BufferControl?` | Buffer control being searched |

### AppFilters (existing, modified)

| New Field | Type | Description |
|-----------|------|-------------|
| `ViInsertMultipleMode` | `IFilter` | Vi insert-multiple mode active |
