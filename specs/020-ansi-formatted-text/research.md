# Research: ANSI Formatted Text - % Operator

**Feature**: 020-ansi-formatted-text
**Date**: 2026-01-26

## Overview

This feature requires minimal research as all technical decisions are pre-determined by existing infrastructure.

## Findings

### Python Reference Implementation

**Location**: `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/formatted_text/ansi.py`

**Lines 268-276** define the `__mod__` method:

```python
def __mod__(self, value: object) -> ANSI:
    """
    ANSI('<b>%s</b>') % value
    """
    if not isinstance(value, tuple):
        value = (value,)

    value = tuple(ansi_escape(i) for i in value)
    return ANSI(self.value % value)
```

**Key behaviors**:
1. If value is not a tuple, wrap it in a tuple
2. Escape each value using `ansi_escape()` (replaces `\x1b` and `\b` with `?`)
3. Use Python's `%` string formatting to substitute `%s` placeholders
4. Return a new `ANSI` instance with the formatted string

### Existing C# Infrastructure

**`AnsiFormatter.FormatPercent()`** in `/src/Stroke/FormattedText/AnsiFormatter.cs`:
- Already implements `%s` placeholder substitution
- Already escapes ANSI control characters (`\x1b`) and backspaces (`\b`)
- Already used by `Html` class for its `%` operator

**`Html` class `%` operators** in `/src/Stroke/FormattedText/Html.cs` (lines 85-95):
- Provides the exact pattern to follow
- Single value: `public static Html operator %(Html html, object value)`
- Array value: `public static Html operator %(Html html, object[] values)`

### Decision Summary

| Decision | Rationale | Alternatives Considered |
|----------|-----------|------------------------|
| Use `AnsiFormatter.FormatPercent()` | Already implemented, tested, and follows the same escaping logic as Python | Inline implementation would duplicate code |
| Follow `Html` operator pattern | Consistency with existing codebase | N/A - this is the established pattern |
| Two operator overloads (single + array) | Matches Python API where tuple/non-tuple are both valid | Single overload with params array (rejected: would change calling convention) |

## Conclusion

No research gaps or unknowns. Implementation can proceed directly to Phase 1 design and Phase 2 tasks.
