# Data Model: HTML Formatted Text

**Feature**: 019-html-formatted-text
**Date**: 2026-01-26

## Overview

The HTML Formatted Text feature introduces no new persistent entities. It operates on in-memory data structures, transforming HTML-like markup strings into styled text fragments.

## Core Types

### Html (Immutable Class)

**Purpose**: Parses HTML-like markup and produces styled text fragments.

**Namespace**: `Stroke.FormattedText`

| Field | Type | Description |
|-------|------|-------------|
| `Value` | `string` | Original HTML input (readonly) |
| `_fragments` | `ImmutableArray<StyleAndTextTuple>` | Parsed styled fragments (private) |

**State Transitions**: None (immutable - state set at construction, never changes)

**Thread Safety**: Inherently thread-safe (immutable)

**Relationships**:
- Produces → `StyleAndTextTuple` (1:many)
- Implements → `IFormattedText`

### StyleAndTextTuple (Existing Record Struct)

**Purpose**: A single styled text fragment.

**Namespace**: `Stroke.FormattedText`

**Defined In**: Feature 015 (FormattedText System)

| Field | Type | Description |
|-------|------|-------------|
| `Style` | `string` | Style string (e.g., `"class:b fg:red"`) |
| `Text` | `string` | Text content |
| `MouseHandler` | `Func<MouseEvent, NotImplementedOrNone>?` | Optional mouse handler |

**Validation**: None (value type, any string combination is valid)

### HtmlFormatter (Internal Static Class)

**Purpose**: Provides HTML escaping and string formatting utilities.

**Namespace**: `Stroke.FormattedText` (internal)

| Method | Purpose |
|--------|---------|
| `Escape(object?)` | Escapes HTML special characters |
| `Format(string, object[])` | Positional placeholder substitution |
| `Format(string, IDictionary<string, object>)` | Named placeholder substitution |
| `FormatPercent(string, object[])` | `%s` style substitution |

## Data Flow

```
┌─────────────────┐     ┌───────────────┐     ┌────────────────────────┐
│  Input String   │ ──► │   Html.ctor   │ ──► │  ImmutableArray<       │
│  "<b>Hello</b>" │     │  (XML Parse)  │     │  StyleAndTextTuple>    │
└─────────────────┘     └───────────────┘     └────────────────────────┘
                                │
                                ▼
                        ┌───────────────┐
                        │   Stacks:     │
                        │   nameStack   │
                        │   fgStack     │
                        │   bgStack     │
                        └───────────────┘
```

## Style String Format

The style string follows a specific format for rendering layer consumption:

```
[class:<names>] [fg:<color>] [bg:<color>]
```

### Components

| Component | Format | Example |
|-----------|--------|---------|
| Class names | `class:name1,name2,...` | `class:b,i` |
| Foreground | `fg:color` | `fg:ansired` |
| Background | `bg:color` | `bg:#00ff00` |

### Examples

| Markup | Style String |
|--------|--------------|
| `<b>text</b>` | `class:b` |
| `<i>text</i>` | `class:i` |
| `<b><i>text</i></b>` | `class:b,i` |
| `<style fg="red">text</style>` | `fg:red` |
| `<style bg="blue">text</style>` | `bg:blue` |
| `<error>text</error>` | `class:error` |
| `<b><style fg="red">text</style></b>` | `class:b fg:red` |
| Plain text | `` (empty) |

## Validation Rules

### Construction-Time Validation

| Rule | Error Type | Message |
|------|------------|---------|
| Null input | `ArgumentNullException` | Standard parameter null message |
| Malformed XML | `FormatException` | "Invalid HTML markup: {details}" |
| Space in `fg` | `FormatException` | "\"fg\" attribute contains a space." |
| Space in `bg` | `FormatException` | "\"bg\" attribute contains a space." |
| Space in `color` | `FormatException` | "\"color\" attribute contains a space." |

### Escape Character Mapping

| Character | Entity | Numeric | Hex |
|-----------|--------|---------|-----|
| `&` | `&amp;` | `&#38;` | `&#x26;` |
| `<` | `&lt;` | `&#60;` | `&#x3C;` |
| `>` | `&gt;` | `&#62;` | `&#x3E;` |
| `"` | `&quot;` | `&#34;` | `&#x22;` |

## Entity Relationship

```
┌─────────────────────────────────┐
│             Html                │
│  (implements IFormattedText)    │
├─────────────────────────────────┤
│ + Value: string                 │
│ - _fragments: ImmutableArray<>  │
├─────────────────────────────────┤
│ + ToFormattedText()             │
│ + Format(params object[])       │
│ + Format(IDictionary<,>)        │
│ + static Escape(object?)        │
│ + operator %(Html, object)      │ ← MISSING
│ + operator %(Html, object[])    │ ← MISSING
└─────────────────────────────────┘
           │
           │ produces
           ▼
┌─────────────────────────────────┐
│      StyleAndTextTuple          │
│       (record struct)           │
├─────────────────────────────────┤
│ + Style: string                 │
│ + Text: string                  │
│ + MouseHandler: Func<>?         │
└─────────────────────────────────┘
           │
           │ implements
           ▼
┌─────────────────────────────────┐
│       IFormattedText            │
│        (interface)              │
├─────────────────────────────────┤
│ + ToFormattedText(): IReadOnly  │
│   List<StyleAndTextTuple>       │
└─────────────────────────────────┘
```

## Storage

**Persistence**: None - all data is in-memory only

**Caching**: The `Html` instance caches parsed fragments in `_fragments` field, computed once at construction time.

**Memory Characteristics**:
- `Html` instance lifetime: typically short-lived (created, used, garbage collected)
- Fragment storage: `ImmutableArray<T>` backed by contiguous array
- String interning: No explicit interning; relies on CLR string pooling for literals
