# Data Model: ANSI Formatted Text - % Operator

**Feature**: 020-ansi-formatted-text
**Date**: 2026-01-26

## Overview

This feature does not introduce new data models. It adds behavior (operator overloads) to the existing `Ansi` class.

## Existing Data Models Used

### `Ansi` Class

**Location**: `/src/Stroke/FormattedText/Ansi.cs`

**Purpose**: Parses ANSI escape sequences into formatted text fragments.

**Relevant Properties**:
- `Value`: The original ANSI-escaped input string (used by `%` operator)
- `_fragments`: Parsed `ImmutableArray<StyleAndTextTuple>` (not affected by `%` operator)

### `AnsiFormatter` Class

**Location**: `/src/Stroke/FormattedText/AnsiFormatter.cs`

**Purpose**: Internal formatter for safe ANSI string interpolation.

**Relevant Methods**:
- `Escape(object?)`: Replaces `\x1b` and `\b` with `?` (already exists)
- `FormatPercent(string, params object[])`: Substitutes `%s` placeholders with escaped values (already exists)

## Data Flow

```
User: new Ansi("\x1b[1m%s\x1b[0m") % "value"
                    |
                    v
        Ansi.operator%(Ansi ansi, object value)
                    |
                    v
        AnsiFormatter.FormatPercent(ansi.Value, value)
                    |
                    v
        AnsiFormatter.Escape(value)  // "value" -> "value" (no escaping needed)
                    |
                    v
        String replacement: "\x1b[1mvalue\x1b[0m"
                    |
                    v
        new Ansi("\x1b[1mvalue\x1b[0m")  // Returned to user
```

## No New Models Required

The `%` operator is a pure functional transformation:
1. Takes `Ansi.Value` (string)
2. Substitutes placeholders using `AnsiFormatter.FormatPercent()`
3. Creates new `Ansi` instance with the result

No state is modified; no new types are introduced.
