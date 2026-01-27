# Contract: Ansi % Operator

**Feature**: 020-ansi-formatted-text
**Date**: 2026-01-26

## API Signature

```csharp
namespace Stroke.FormattedText;

public sealed class Ansi : IFormattedText
{
    // ... existing members ...

    /// <summary>
    /// Formats ANSI text with %s-style substitution (single value).
    /// </summary>
    /// <param name="ansi">The Ansi template.</param>
    /// <param name="value">The value to substitute.</param>
    /// <returns>A new Ansi with the value escaped and substituted.</returns>
    /// <remarks>
    /// <para>
    /// ANSI escape sequences (\x1b) and backspaces (\b) in the value are replaced with '?'.
    /// This prevents style injection attacks.
    /// </para>
    /// <para>
    /// Equivalent to Python Prompt Toolkit's <c>ANSI.__mod__</c> method.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var greeting = new Ansi("\x1b[1mHello %s!\x1b[0m") % "World";
    /// // Result: Bold "Hello World!"
    /// </code>
    /// </example>
    public static Ansi operator %(Ansi ansi, object value);

    /// <summary>
    /// Formats ANSI text with %s-style substitution (multiple values).
    /// </summary>
    /// <param name="ansi">The Ansi template.</param>
    /// <param name="values">The values to substitute.</param>
    /// <returns>A new Ansi with all values escaped and substituted.</returns>
    /// <remarks>
    /// <para>
    /// ANSI escape sequences (\x1b) and backspaces (\b) in values are replaced with '?'.
    /// </para>
    /// <para>
    /// If there are more placeholders than values, extra placeholders remain as literal %s.
    /// If there are more values than placeholders, extra values are ignored.
    /// </para>
    /// <para>
    /// Equivalent to Python Prompt Toolkit's <c>ANSI.__mod__</c> method with tuple argument.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var message = new Ansi("%s said: %s") % new object[] { "Alice", "Hello" };
    /// // Result: "Alice said: Hello"
    /// </code>
    /// </example>
    public static Ansi operator %(Ansi ansi, object[] values);
}
```

## Behavioral Specification

### Single Value Substitution

| Input | Template | Output |
|-------|----------|--------|
| `"World"` | `"Hello %s"` | `"Hello World"` |
| `"<script>"` | `"%s"` | `"<script>"` (no HTML escaping for Ansi) |
| `"\x1b[31m"` | `"%s"` | `"?"` (escape char neutralized) |
| `"\b"` | `"%s"` | `"?"` (backspace neutralized) |
| `"\x1b\b"` | `"%s"` | `"??"` (both neutralized) |
| `null` | `"%s"` | `""` (empty string) |
| `123` | `"%s"` | `"123"` (ToString called) |

### Multiple Value Substitution

| Input Array | Template | Output |
|-------------|----------|--------|
| `["A", "B"]` | `"%s and %s"` | `"A and B"` |
| `["A"]` | `"%s and %s"` | `"A and %s"` |
| `["A", "B", "C"]` | `"%s"` | `"A"` (extra ignored) |
| `[]` | `"%s"` | `"%s"` (no substitution) |
| `null` | `"%s"` | `ArgumentNullException` |

### Style Preservation

The `%` operator substitutes text but preserves ANSI styling from the template:

```
Template: "\x1b[1mHello %s!\x1b[0m" (bold "Hello " + placeholder + bold "!")
Value: "World"
Result: "\x1b[1mHello World!\x1b[0m" (bold preserved)
```

### Immutability

Both operators return a **new** `Ansi` instance. The original template is never modified.

## Error Handling

| Condition | Behavior |
|-----------|----------|
| `ansi` parameter is `null` | `ArgumentNullException` (C# operator semantics) |
| `values` array parameter is `null` | `ArgumentNullException` |
| Element within `values` array is `null` | Converted to empty string (graceful handling) |

## Security

### Escaped Characters

| Character | Code Point | Escaped To | Purpose |
|-----------|------------|------------|---------|
| ESC | `\x1b` (0x1B) | `?` | Prevents ANSI escape sequence injection |
| BS | `\b` (0x08) | `?` | Prevents backspace-based overwrite attacks |

### Characters NOT Escaped

The following are **intentionally NOT escaped** for Python parity:
- `\x9b` (CSI, 8-bit) - Not escaped by Python PTK
- `\x07` (BEL) - Not escaped by Python PTK
- Other C0/C1 control characters - Not escaped by Python PTK

## Dependencies

- `AnsiFormatter.FormatPercent(string format, params object[] args)`: Performs the actual substitution with escaping
- `AnsiFormatter.Escape(object? text)`: Escapes ANSI control characters

## Python Parity

Maps to Python Prompt Toolkit's `ANSI.__mod__` (lines 268-276 in `ansi.py`):

```python
def __mod__(self, value: object) -> ANSI:
    if not isinstance(value, tuple):
        value = (value,)
    value = tuple(ansi_escape(i) for i in value)
    return ANSI(self.value % value)
```

The C# implementation achieves the same behavior through two operator overloads.
