# Research: Formatted Text System

**Feature**: 015-formatted-text-system
**Date**: 2026-01-25

## Research Tasks

### 1. XML Parsing Strategy for HTML Class

**Context**: Python uses `xml.dom.minidom` to parse HTML-like markup. Need equivalent for C#.

**Decision**: Use `System.Xml.Linq.XDocument` with permissive parsing

**Rationale**:
- `XDocument` is part of .NET BCL (no external dependencies)
- Provides DOM-like traversal similar to `minidom`
- Supports attribute access via `XElement.Attribute()`
- Child node iteration via `XElement.Nodes()` and `XElement.Elements()`
- Text content via `XText` nodes

**Alternatives Considered**:
- `System.Xml.XmlDocument`: Older API, more verbose
- `HtmlAgilityPack`: External dependency, violates Constitution III
- Manual parsing: Error-prone, unnecessary complexity

**Implementation Notes**:
- Wrap input in `<html-root>` element (like Python)
- Handle `TEXT_NODE` equivalent via `node is XText`
- Handle element nodes via `node is XElement`
- Extract fg/bg attributes from `<style>` elements

### 2. ANSI Parser State Machine Design

**Context**: Python uses a coroutine-based parser. Need C# equivalent.

**Decision**: Implement as explicit state machine with enum states

**Rationale**:
- C# iterators (`yield return`) don't support `yield` receiving values
- Explicit state machine is idiomatic C#
- Clear state transitions for maintainability
- Easy to unit test individual states

**States**:
```
Normal → (receive \x1b) → EscapeStart → (receive [) → CsiStart → (accumulate params) → CsiParam → (receive m/C) → Normal
Normal → (receive \001) → ZeroWidth → (receive \002) → Normal
Normal → (receive \x9b) → CsiStart
```

**Alternatives Considered**:
- Async streams: Overcomplicated for character-by-character parsing
- Rx.NET: External dependency
- Manual yield pattern with state object: Less readable

### 3. ANSI Color Code Mapping

**Context**: ANSI parser needs mappings from SGR codes to color names.

**Decision**: Define static dictionaries `FgAnsiColors` and `BgAnsiColors` matching Python's `FG_ANSI_COLORS` and `BG_ANSI_COLORS`

**Foreground Colors** (code → name):
| Code | Name |
|------|------|
| 30 | ansiblack |
| 31 | ansired |
| 32 | ansigreen |
| 33 | ansiyellow |
| 34 | ansiblue |
| 35 | ansimagenta |
| 36 | ansicyan |
| 37 | ansigray |
| 39 | ansidefault |
| 90 | ansibrightblack |
| 91 | ansibrightred |
| 92 | ansibrightgreen |
| 93 | ansibrightyellow |
| 94 | ansibrightblue |
| 95 | ansibrightmagenta |
| 96 | ansibrightcyan |
| 97 | ansiwhite |

**Background Colors** (code → name):
| Code | Name |
|------|------|
| 40 | ansiblack |
| 41 | ansired |
| 42 | ansigreen |
| 43 | ansiyellow |
| 44 | ansiblue |
| 45 | ansimagenta |
| 46 | ansicyan |
| 47 | ansigray |
| 49 | ansidefault |
| 100 | ansibrightblack |
| 101 | ansibrightred |
| 102 | ansibrightgreen |
| 103 | ansibrightyellow |
| 104 | ansibrightblue |
| 105 | ansibrightmagenta |
| 106 | ansibrightcyan |
| 107 | ansiwhite |

**256-Color Mapping**: Build lookup table of (r, g, b) tuples for colors 0-255:
- 0-15: Basic ANSI colors
- 16-231: 6x6x6 color cube (r = (i/36)%6, g = (i/6)%6, b = i%6) with values [0x00, 0x5F, 0x87, 0xAF, 0xD7, 0xFF]
- 232-255: Grayscale (8 + i*10 for i in 0..23)

### 4. Character Width Calculation

**Context**: `fragment_list_width` needs to account for wide (CJK) characters.

**Decision**: Use `Wcwidth` NuGet package per `docs/dependencies-plan.md`

**Rationale**:
- Already specified in project dependencies plan
- MIT licensed
- Matches Python's `wcwidth` behavior
- Handles all Unicode width categories

**Implementation**:
```csharp
public static int FragmentListWidth(IEnumerable<StyleAndTextTuple> fragments)
{
    const string ZeroWidthEscape = "[ZeroWidthEscape]";
    return fragments
        .Where(f => !f.Style.Contains(ZeroWidthEscape))
        .SelectMany(f => f.Text)
        .Sum(c => Wcwidth.UnicodeWidth.GetWidth(c));
}
```

**Note**: May need to handle return value of -1 (control characters) as 0.

### 5. IFormattedText Interface Design

**Context**: Python uses duck typing with `__pt_formatted_text__` magic method.

**Decision**: Define `IFormattedText` interface with single method

```csharp
public interface IFormattedText
{
    IReadOnlyList<StyleAndTextTuple> ToFormattedText();
}
```

**Rationale**:
- Explicit interface is idiomatic C#
- Allows any class to be convertible to formatted text
- `FormattedText`, `Html`, `Ansi`, `PygmentsTokens` will implement this

**Implementation Pattern**:
- `Html`: Parse in constructor, cache result, return from `ToFormattedText()`
- `Ansi`: Parse in constructor, cache result, return from `ToFormattedText()`
- `FormattedText`: Return `this` (it implements `IReadOnlyList<StyleAndTextTuple>`)
- `PygmentsTokens`: Transform token list to style tuples

### 6. String Interpolation with Escaping

**Context**: Python uses `string.Formatter` subclass for safe interpolation.

**Decision**: Use C# custom `IFormatProvider` and `ICustomFormatter`

**For HTML**:
- `HtmlFormatter` escapes `<`, `>`, `&`, `"` in interpolated values
- `Html.Format("{0}", value)` applies escaping automatically

**For ANSI**:
- `AnsiFormatter` replaces `\x1b` and `\b` with `?` in interpolated values
- `Ansi.Format("{0}", value)` applies escaping automatically

**Implementation**:
```csharp
public sealed class HtmlFormatter : IFormatProvider, ICustomFormatter
{
    public static HtmlFormatter Instance { get; } = new();

    public object? GetFormat(Type? formatType) =>
        formatType == typeof(ICustomFormatter) ? this : null;

    public string Format(string? format, object? arg, IFormatProvider? formatProvider)
    {
        var formatted = arg is IFormattable f
            ? f.ToString(format, formatProvider)
            : arg?.ToString() ?? "";
        return HtmlEscape(formatted);
    }
}
```

### 7. Existing Implementation Gap Analysis

**Current State** (from code review):
- `StyleAndTextTuple`: ✅ Exists, needs mouse handler overload
- `FormattedText`: ✅ Exists, needs `IFormattedText` implementation
- `AnyFormattedText`: ✅ Exists, needs `IFormattedText` support in conversion
- `FormattedTextUtils.ToFormattedText`: ✅ Exists
- `FormattedTextUtils.ToPlainText`: ✅ Exists
- `FormattedTextUtils.FragmentListToText`: ✅ Exists
- `FormattedTextUtils.FragmentListLen`: ✅ Exists (but doesn't exclude ZeroWidthEscape)

**Missing**:
- `IFormattedText` interface
- `FormattedTextUtils.FragmentListWidth` (with Unicode width)
- `FormattedTextUtils.SplitLines`
- `FormattedTextUtils.IsFormattedText`
- `FormattedTextUtils.Merge`
- `Html` class
- `Ansi` class
- `Template` class
- `PygmentsTokens` class
- `HtmlEscape` function
- `AnsiEscape` function
- ZeroWidthEscape handling in FragmentListLen

### 8. Test Coverage Mapping

**Python Tests** (`test_formatted_text.py`):
| Python Test | C# Test | Priority |
|-------------|---------|----------|
| `test_basic_html` | `HtmlTests.BasicHtml_*` | P1 |
| `test_html_with_fg_bg` | `HtmlTests.HtmlWithFgBg_*` | P1 |
| `test_ansi_formatting` | `AnsiTests.BasicFormatting_*` | P1 |
| `test_ansi_dim` | `AnsiTests.DimFormatting_*` | P1 |
| `test_ansi_256_color` | `AnsiTests.Color256_*` | P1 |
| `test_ansi_true_color` | `AnsiTests.TrueColor_*` | P1 |
| `test_ansi_interpolation` | `AnsiTests.Interpolation_*` | P1 |
| `test_interpolation` | `TemplateTests.Interpolation_*` | P2 |
| `test_html_interpolation` | `HtmlTests.Interpolation_*` | P1 |
| `test_merge_formatted_text` | `FormattedTextUtilsTests.Merge_*` | P2 |
| `test_pygments_tokens` | `PygmentsTokensTests.*` | P2 |
| `test_split_lines` | `FormattedTextUtilsTests.SplitLines_*` | P2 |
| `test_split_lines_2` | `FormattedTextUtilsTests.SplitLines_*` | P2 |
| `test_split_lines_3` | `FormattedTextUtilsTests.SplitLines_*` | P2 |
| `test_split_lines_4` | `FormattedTextUtilsTests.SplitLines_*` | P2 |

## Summary

All technical questions resolved:
1. XML parsing via `System.Xml.Linq.XDocument` (BCL, no external deps)
2. ANSI parser as explicit state machine with enum states
3. ANSI color mappings defined statically matching Python
4. Character width via `Wcwidth` NuGet (per dependencies-plan.md)
5. `IFormattedText` interface for C# equivalent of `__pt_formatted_text__`
6. Custom formatters for HTML/ANSI escaping
7. Gap analysis identifies 10 missing components
8. Test mapping from 15 Python tests to C# test classes
