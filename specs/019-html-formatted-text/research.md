# Research: HTML Formatted Text

**Feature**: 019-html-formatted-text
**Date**: 2026-01-26

## Executive Summary

The HTML Formatted Text feature is already 95% implemented. Research confirms the implementation follows Python Prompt Toolkit's `HTML` class faithfully. The only gap is the `%` operator overload (FR-016), which requires minimal additional work.

## Research Findings

### R1: Python Prompt Toolkit Reference Implementation

**Source**: `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/formatted_text/html.py`

**Decision**: Follow Python Prompt Toolkit's implementation exactly

**Key Patterns Identified**:
1. Uses XML DOM parsing with `xml.dom.minidom`
2. Wraps markup in `<html-root>` element for parsing
3. Uses stack-based style tracking (nameStack, fgStack, bgStack)
4. Excludes `#document`, `html-root`, `style` from class names
5. Inner colors take precedence via stack (LIFO)
6. Provides `format()` method with safe escaping
7. Provides `__mod__` operator for `%s` substitution
8. Provides `html_escape()` utility function

**C# Adaptation**:
- Uses `System.Xml.Linq.XDocument` instead of `xml.dom.minidom`
- Uses `ImmutableArray<StyleAndTextTuple>` instead of list of tuples
- Uses method overloads instead of Python's `*args` and `**kwargs`
- Operator `%` requires explicit overload in C#

**Alternatives Considered**: None - Constitution I requires faithful port

### R2: XML Parsing Strategy

**Decision**: Use `System.Xml.Linq.XDocument.Parse()` with `LoadOptions.PreserveWhitespace`

**Rationale**:
- `System.Xml.Linq` is included in .NET BCL (no external dependencies)
- `LoadOptions.PreserveWhitespace` ensures whitespace preservation matching Python behavior
- Provides LINQ-friendly API for tree traversal
- Throws `XmlException` for malformed XML (converted to `FormatException`)

**Alternatives Considered**:
- `xml.dom.minidom` equivalent (`System.Xml.XmlDocument`) - More verbose API
- HTML parsing libraries (HtmlAgilityPack) - Adds external dependency, overkill for well-formed XML

### R3: Style String Format

**Decision**: Use space-separated format: `class:name1,name2 fg:color bg:color`

**Rationale**: Matches Python Prompt Toolkit's style string format exactly

**Format Specification**:
- `class:` prefix for element name classes (comma-separated if nested)
- `fg:` prefix for foreground color
- `bg:` prefix for background color
- Components separated by spaces
- Empty style is empty string `""`

**Examples**:
- `<b>text</b>` → `"class:b"`
- `<b><i>text</i></b>` → `"class:b,i"`
- `<style fg="red">text</style>` → `"fg:red"`
- `<b><style fg="red" bg="blue">text</style></b>` → `"class:b fg:red bg:blue"`

### R4: HTML Escaping Characters

**Decision**: Escape `&`, `<`, `>`, `"` characters

**Rationale**: Matches Python Prompt Toolkit's `html_escape()` function exactly

**Escape Mappings**:
| Character | Escape Sequence |
|-----------|-----------------|
| `&` | `&amp;` |
| `<` | `&lt;` |
| `>` | `&gt;` |
| `"` | `&quot;` |

**Note**: Single quotes (`'`) are NOT escaped - matches Python behavior

### R5: Operator `%` Implementation

**Decision**: Add operator overloads for both single value and array

**Rationale**: FR-016 requires `%` operator support; Python uses `__mod__`

**Implementation Pattern**:
```csharp
public static Html operator %(Html html, object value)
{
    if (value is not (object[] or Tuple<object>))
        value = new[] { value };
    return new(HtmlFormatter.FormatPercent(html.Value, (object[])value));
}
```

**Consideration**: C# doesn't support Python's automatic tuple unpacking for operators, so we need to handle both single values and arrays explicitly.

## Gap Summary

| ID | Gap | Resolution | Effort |
|----|-----|------------|--------|
| G1 | Missing `%` operator on `Html` class | Add operator overloads | ~15 LOC |
| G2 | Missing tests for `%` operator | Add test cases | ~30 LOC |

## Recommendations

1. **Immediate**: Add `%` operator to `Html.cs` following the pattern in R5
2. **Immediate**: Add test cases for `%` operator in `HtmlTests.cs`
3. **Verification**: Run existing test suite to ensure no regressions
4. **Documentation**: Verify XML doc comments are complete

## References

- Python Prompt Toolkit source: `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/formatted_text/html.py`
- Stroke Html implementation: `/Users/brandon/src/stroke/src/Stroke/FormattedText/Html.cs`
- Stroke HtmlFormatter: `/Users/brandon/src/stroke/src/Stroke/FormattedText/HtmlFormatter.cs`
- Existing tests: `/Users/brandon/src/stroke/tests/Stroke.Tests/FormattedText/HtmlTests.cs`
