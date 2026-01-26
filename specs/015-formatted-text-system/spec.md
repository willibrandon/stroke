# Feature Specification: Formatted Text System

**Feature Branch**: `015-formatted-text-system`
**Created**: 2026-01-25
**Updated**: 2026-01-25
**Status**: Draft
**Input**: User description: "Implement the formatted text system for representing styled text throughout the application"

## Python API Mapping *(reference)*

This feature ports Python Prompt Toolkit's `prompt_toolkit.formatted_text` module. The following table maps Python APIs to their C# equivalents:

| Python API | C# Equivalent | Notes |
|------------|---------------|-------|
| `__pt_formatted_text__` protocol | `IFormattedText` interface | Duck typing → explicit interface |
| `FormattedText` class | `FormattedText` class | Immutable wrapper for fragment list |
| `HTML` class | `Html` class | PascalCase naming convention |
| `ANSI` class | `Ansi` class | PascalCase naming convention |
| `Template` class | `Template` class | Same name |
| `PygmentsTokens` class | `PygmentsTokens` class | Converts token lists |
| `to_formatted_text()` | `FormattedTextUtils.ToFormattedText()` | Static method |
| `is_formatted_text()` | `FormattedTextUtils.IsFormattedText()` | Static method |
| `merge_formatted_text()` | `FormattedTextUtils.Merge()` | Static method |
| `fragment_list_len()` | `FormattedTextUtils.FragmentListLen()` | Static method |
| `fragment_list_width()` | `FormattedTextUtils.FragmentListWidth()` | Static method |
| `fragment_list_to_text()` | `FormattedTextUtils.FragmentListToText()` | Static method |
| `split_lines()` | `FormattedTextUtils.SplitLines()` | Static method |
| `to_plain_text()` | `FormattedTextUtils.ToPlainText()` | Static method |
| `AnyFormattedText` type alias | `AnyFormattedText` struct | Union type via implicit conversions |
| `StyleAndTextTuple` | `StyleAndTextTuple` record struct | (style, text, handler?) tuple |
| `MagicFormattedText` protocol | N/A - use `IFormattedText` | Python duck typing not applicable in C# |
| `HTML.__mod__()` operator | `Html.Format()` method | `%` operator → method call |
| `ANSI.__mod__()` operator | `Ansi.Format()` method | `%` operator → method call |

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Plain Text to Styled Text Conversion (Priority: P1)

Developers need to convert plain strings and various formatted text representations into a unified styled text format that can be rendered in the terminal.

**Why this priority**: This is the foundational capability - without the ability to convert different input types to formatted text, no other formatted text features can work. All higher-level APIs depend on this conversion.

**Independent Test**: Can be fully tested by creating formatted text from strings, converting various input types, and verifying the output fragment structure. Delivers the core value of text representation.

**Acceptance Scenarios**:

1. **Given** a plain string "Hello", **When** converted to formatted text, **Then** produces a single fragment with empty style and the text "Hello"
2. **Given** a list of (style, text) tuples, **When** converted to formatted text, **Then** the tuples are preserved as FormattedText
3. **Given** an object implementing IFormattedText, **When** converted to formatted text, **Then** the ToFormattedText() method is called and result returned
4. **Given** a callable that returns formatted text, **When** converted to formatted text, **Then** the callable is invoked and result converted
5. **Given** null, **When** converted to formatted text, **Then** an empty FormattedText is returned
6. **Given** a non-convertible object without autoConvert, **When** conversion attempted, **Then** an appropriate error is raised
7. **Given** a non-convertible object with autoConvert=true, **When** converted, **Then** the object is converted to string first

---

### User Story 2 - HTML-Like Markup Parsing (Priority: P1)

Developers need to define styled text using familiar HTML-like syntax with support for semantic elements, inline styles, and nested formatting.

**Why this priority**: HTML markup is the primary developer-facing API for creating styled text. It provides a readable, maintainable way to express formatting without dealing with raw style tuples.

**Independent Test**: Can be fully tested by parsing various HTML strings and verifying the resulting style and text fragments. Delivers rich text authoring capability.

**Acceptance Scenarios**:

1. **Given** HTML `<b>bold</b>`, **When** parsed, **Then** produces fragment with style "class:b" and text "bold"
2. **Given** HTML `<i>italic</i>`, **When** parsed, **Then** produces fragment with style "class:i" and text "italic"
3. **Given** HTML `<u>underline</u>`, **When** parsed, **Then** produces fragment with style "class:u" and text "underline"
4. **Given** HTML `<s>strike</s>`, **When** parsed, **Then** produces fragment with style "class:s" and text "strike"
5. **Given** HTML `<style fg="red">text</style>`, **When** parsed, **Then** produces fragment with style "fg:red" and text "text"
6. **Given** HTML `<style bg="blue">text</style>`, **When** parsed, **Then** produces fragment with style "bg:blue" and text "text"
7. **Given** HTML `<style fg="red" bg="blue">text</style>`, **When** parsed, **Then** produces fragment with style "fg:red bg:blue"
8. **Given** HTML `<username>john</username>`, **When** parsed, **Then** produces fragment with style "class:username" and text "john"
9. **Given** HTML `<outer><inner>text</inner></outer>`, **When** parsed, **Then** produces fragment with style "class:outer,inner"
10. **Given** HTML with `&lt;`, `&gt;`, `&amp;`, `&quot;`, `&apos;` entities, **When** parsed, **Then** entities are properly decoded
11. **Given** HTML.Format() with user input, **When** formatted, **Then** special characters in arguments are escaped
12. **Given** HTML with numeric entity `&#60;`, **When** parsed, **Then** decodes to `<` character
13. **Given** HTML with hex entity `&#x3C;`, **When** parsed, **Then** decodes to `<` character
14. **Given** HTML with self-closing tag `<br/>`, **When** parsed, **Then** produces empty fragment for that element
15. **Given** HTML with empty element `<b></b>`, **When** parsed, **Then** produces no fragment (empty text)
16. **Given** HTML with `<style color="red">`, **When** parsed, **Then** `color` is treated as alias for `fg`
17. **Given** HTML with whitespace between elements `<b>A</b> <i>B</i>`, **When** parsed, **Then** whitespace is preserved as unstyled fragment
18. **Given** malformed XML in HTML, **When** parsed, **Then** throws `FormatException` with descriptive message
19. **Given** HTML with invalid color value `<style fg="notacolor">`, **When** parsed, **Then** passes value through (validation at render time)
20. **Given** HTML with spaces in attribute `<style fg=" red ">`, **When** parsed, **Then** whitespace is trimmed from color value

---

### User Story 3 - ANSI Escape Sequence Parsing (Priority: P1)

Developers need to convert text containing ANSI escape sequences (from external tools, legacy systems, or user input) into the formatted text representation.

**Why this priority**: ANSI parsing enables integration with existing terminal content and external command output. It's essential for interoperability with the broader terminal ecosystem.

**Independent Test**: Can be fully tested by parsing ANSI-escaped strings and verifying the resulting style fragments. Delivers compatibility with existing terminal content.

**Acceptance Scenarios**:

1. **Given** ANSI `\x1b[31mred`, **When** parsed, **Then** produces fragment with foreground color and text "red"
2. **Given** ANSI `\x1b[1mbold`, **When** parsed, **Then** produces fragment with bold style
3. **Given** ANSI `\x1b[4munderline`, **When** parsed, **Then** produces fragment with underline style
4. **Given** ANSI `\x1b[0m`, **When** parsed, **Then** resets all style attributes
5. **Given** ANSI with 256-color code `\x1b[38;5;196m`, **When** parsed, **Then** produces appropriate color style
6. **Given** ANSI with true color `\x1b[38;2;255;128;0m`, **When** parsed, **Then** produces hex color style
7. **Given** text between \001 and \002, **When** parsed, **Then** produces ZeroWidthEscape fragment
8. **Given** cursor forward escape `\x1b[5C`, **When** parsed, **Then** produces 5 space characters with current style
9. **Given** ANSI.Format() with user input, **When** formatted, **Then** escape sequences in arguments are neutralized
10. **Given** ANSI with malformed sequence `\x1b[m`, **When** parsed, **Then** treats as reset (SGR 0)
11. **Given** ANSI with `\x1b[;m`, **When** parsed, **Then** treats empty parameters as 0 (reset)
12. **Given** ANSI with single-byte CSI `\x9b31m`, **When** parsed, **Then** handles same as `\x1b[31m`
13. **Given** ANSI with cursor forward `\x1b[0C`, **When** parsed, **Then** produces 0 spaces (no-op)
14. **Given** ANSI with 256-color index 256 (out of bounds), **When** parsed, **Then** clamps to valid range 0-255
15. **Given** ANSI with RGB value 300 (out of bounds), **When** parsed, **Then** clamps to valid range 0-255
16. **Given** ANSI with unknown CSI sequence, **When** parsed, **Then** sequence is discarded, text continues normally
17. **Given** ANSI with SGR disable codes (22-29), **When** parsed, **Then** corresponding attribute is disabled
18. **Given** interleaved `\001`/`\002` and ANSI sequences, **When** parsed, **Then** both are processed correctly

---

### User Story 4 - Fragment List Utilities (Priority: P2)

Developers need utility functions to measure, extract, and manipulate formatted text fragments for layout and rendering calculations.

**Why this priority**: These utilities are required for layout calculations, text truncation, and rendering. They build upon P1 capabilities to enable proper terminal output.

**Independent Test**: Can be fully tested by creating fragment lists and measuring/extracting their properties. Delivers the tooling needed for layout and rendering.

**Acceptance Scenarios**:

1. **Given** fragment list with total 10 characters, **When** FragmentListLen called, **Then** returns 10
2. **Given** fragment list containing ZeroWidthEscape fragments, **When** FragmentListLen called, **Then** ZeroWidthEscape text is excluded from count
3. **Given** fragment list with ASCII characters, **When** FragmentListWidth called, **Then** returns character count
4. **Given** fragment list with CJK characters (width 2), **When** FragmentListWidth called, **Then** returns doubled width for CJK
5. **Given** fragment list with mixed content, **When** FragmentListToText called, **Then** returns concatenated plain text
6. **Given** fragment list with ZeroWidthEscape, **When** FragmentListToText called, **Then** ZeroWidthEscape text is excluded
7. **Given** fragment list containing newlines, **When** SplitLines called, **Then** yields one list per line preserving styles
8. **Given** fragment ending with newline, **When** SplitLines called, **Then** yields empty final line to distinguish "line\n" from "line"
9. **Given** any formatted text type, **When** ToPlainText called, **Then** returns plain string without styles
10. **Given** fragment with mouse handler spanning newline, **When** SplitLines called, **Then** mouse handler is preserved on both resulting lines
11. **Given** consecutive newlines `\n\n`, **When** SplitLines called, **Then** yields empty line between non-empty lines
12. **Given** CR+LF line endings `\r\n`, **When** SplitLines called, **Then** treats as single newline (splits correctly)
13. **Given** fragment with control character (e.g., `\x00`), **When** FragmentListWidth called, **Then** control character contributes width 0 or -1 per Unicode rules
14. **Given** fragment with combining character (e.g., accent), **When** FragmentListWidth called, **Then** combining character contributes width 0
15. **Given** empty fragment list, **When** FragmentListLen called, **Then** returns 0
16. **Given** empty fragment list, **When** FragmentListWidth called, **Then** returns 0
17. **Given** empty fragment list, **When** FragmentListToText called, **Then** returns empty string
18. **Given** empty fragment list, **When** SplitLines called, **Then** yields single empty line

---

### User Story 5 - Template Interpolation (Priority: P2)

Developers need to create formatted text templates with placeholders that can be filled with dynamic values while preserving formatting.

**Why this priority**: Templates enable separation of formatting from data, making it easier to maintain styled prompts and messages. Depends on P1 conversion capabilities.

**Independent Test**: Can be fully tested by creating templates and formatting them with values. Delivers reusable formatted text patterns.

**Acceptance Scenarios**:

1. **Given** template "Hello {}", **When** formatted with "World", **Then** produces formatted text with "Hello World"
2. **Given** template "User: {}", **When** formatted with HTML("<b>admin</b>"), **Then** HTML formatting is preserved
3. **Given** template with multiple {}, **When** formatted with corresponding values, **Then** each placeholder is replaced in order
4. **Given** template.Format() return value, **When** invoked as callable, **Then** returns the formatted result (lazy evaluation)
5. **Given** template "{0}" with positional syntax, **When** parsed, **Then** throws `FormatException` (positional syntax not supported)
6. **Given** template with escaped braces "{{literal}}", **When** formatted, **Then** produces literal "{literal}" in output
7. **Given** template with more placeholders than values, **When** formatted, **Then** throws `ArgumentException`
8. **Given** template with fewer placeholders than values, **When** formatted, **Then** throws `ArgumentException`
9. **Given** empty template "", **When** formatted with no arguments, **Then** produces empty FormattedText

---

### User Story 6 - Formatted Text Merging (Priority: P2)

Developers need to concatenate multiple pieces of formatted text (of any supported type) into a single unified result.

**Why this priority**: Merging enables composition of complex formatted output from simpler parts. Essential for building prompts and messages from components.

**Independent Test**: Can be fully tested by merging various formatted text items and verifying the combined result. Delivers compositional text building.

**Acceptance Scenarios**:

1. **Given** two FormattedText items, **When** merged, **Then** produces combined fragments in original order (order preserved)
2. **Given** string and HTML items, **When** merged, **Then** both are converted and combined
3. **Given** merge result (callable), **When** invoked, **Then** performs lazy conversion and returns FormattedText
4. **Given** null item in merge list, **When** merged, **Then** null is treated as empty (skipped)
5. **Given** empty string in merge list, **When** merged, **Then** empty string is skipped
6. **Given** three items A, B, C, **When** merged, **Then** result order is A + B + C (left to right)

---

### User Story 7 - Pygments Token Conversion (Priority: P3)

Developers need to convert syntax-highlighted token lists (from code lexers) into the formatted text representation for display in the terminal.

**Why this priority**: This enables integration with syntax highlighting systems. It's a specialized use case that builds on the core formatted text infrastructure.

**Independent Test**: Can be fully tested by creating PygmentsTokens from token lists and verifying the resulting styled fragments.

**Acceptance Scenarios**:

1. **Given** token list `[("Token.Keyword", "def")]`, **When** converted to formatted text, **Then** produces fragment with style `class:pygments.keyword`
2. **Given** token list `[("Token.Name.Function", "foo")]`, **When** converted, **Then** style is `class:pygments.name.function`
3. **Given** empty token list, **When** converted, **Then** produces empty FormattedText
4. **Given** token with empty text `[("Token.Keyword", "")]`, **When** converted, **Then** fragment is skipped

---

### User Story 8 - Flexible API Input (Priority: P2)

API consumers need to pass formatted text to Stroke APIs using various input types without explicit conversion, enabling flexible and convenient usage patterns.

**Why this priority**: This is the primary developer ergonomics feature, allowing APIs to accept strings, HTML, ANSI, or callables interchangeably.

**Independent Test**: Can be fully tested by passing various types to AnyFormattedText and verifying conversion behavior.

**Acceptance Scenarios**:

1. **Given** a plain string assigned to AnyFormattedText, **When** converted, **Then** produces unstyled fragment
2. **Given** an Html instance assigned to AnyFormattedText, **When** converted, **Then** HTML is parsed and styled
3. **Given** a Func<AnyFormattedText> callable, **When** converted multiple times, **Then** callable is invoked each time (lazy)
4. **Given** null assigned to AnyFormattedText, **When** IsEmpty checked, **Then** returns true
5. **Given** AnyFormattedText with style parameter, **When** ToFormattedText("bold") called, **Then** style is prepended to fragments

---

### Edge Cases

#### HTML Parser Edge Cases

| Scenario | Expected Behavior | Rationale |
|----------|-------------------|-----------|
| Malformed XML | Throw `FormatException` with message indicating parse location | Fail fast for authoring errors |
| Self-closing tags `<br/>` | Produce empty fragment (element with no text content) | XML compliance |
| Empty elements `<b></b>` | No fragment produced (zero-length text) | Avoid empty fragments |
| Unknown element `<foo>` | Convert to `class:foo` style | All elements become classes |
| Deeply nested (100+ levels) | No limit; accumulate all classes | Match Python behavior |
| Invalid color in `fg`/`bg` | Pass through value unchanged | Defer validation to renderer |
| Whitespace in color value | Trim leading/trailing whitespace | User convenience |
| Entity `&#60;` (decimal) | Decode to `<` | Standard XML entity handling |
| Entity `&#x3C;` (hex) | Decode to `<` | Standard XML entity handling |
| Unknown entity `&foo;` | Pass through unchanged or throw | XML parser behavior |
| `color` attribute | Treat as alias for `fg` | Match Python HTML class |
| Mixed content `A<b>B</b>C` | Three fragments: unstyled "A", styled "B", unstyled "C" | Preserve all text |

#### ANSI Parser Edge Cases

| Scenario | Expected Behavior | Rationale |
|----------|-------------------|-----------|
| Unsupported CSI sequence | Discard sequence, continue parsing text | Graceful degradation |
| `\x1b[m` (empty SGR) | Treat as reset (SGR 0) | Common shorthand |
| `\x1b[;m` (empty params) | Treat empty as 0 (reset) | Match terminal behavior |
| `\x9b` single-byte CSI | Handle same as `\x1b[` | 8-bit CSI support |
| 256-color index > 255 | Clamp to 255 | Defensive handling |
| 256-color index < 0 | Clamp to 0 | Defensive handling |
| RGB channel > 255 | Clamp to 255 | Defensive handling |
| RGB channel < 0 | Clamp to 0 | Defensive handling |
| Cursor forward N=0 | No spaces emitted | Zero is no-op |
| SGR parameter > 9999 | Ignore excessive value | Prevent resource exhaustion |
| Interleaved `\001`/`\002` and SGR | Process both independently | ZeroWidthEscape is separate |
| Split across buffer | State machine is character-by-character | Streaming support |
| SGR code 22 | Disable bold AND dim | Per ECMA-48 |
| SGR code 23 | Disable italic | Per ECMA-48 |
| SGR code 24 | Disable underline | Per ECMA-48 |
| SGR code 25 | Disable blink | Per ECMA-48 |
| SGR code 27 | Disable reverse | Per ECMA-48 |
| SGR code 28 | Disable hidden | Per ECMA-48 |
| SGR code 29 | Disable strike | Per ECMA-48 |

#### Fragment Utility Edge Cases

| Scenario | Expected Behavior | Rationale |
|----------|-------------------|-----------|
| Empty fragment list | Len=0, Width=0, ToText="", SplitLines yields 1 empty line | Consistent empty handling |
| ZeroWidthEscape fragment | Excluded from Len, Width, ToText | Terminal control, not visible |
| Consecutive `\n\n` | SplitLines yields empty line between | Match Python split behavior |
| CR+LF `\r\n` | Treat as single newline | Cross-platform compatibility |
| Control character width | Width 0 or -1 per wcwidth | Unicode standard |
| Combining character | Width 0 | Unicode standard |
| Mouse handler on newline split | Preserve handler on both lines | User interaction continuity |

#### Template Edge Cases

| Scenario | Expected Behavior | Rationale |
|----------|-------------------|-----------|
| Positional `{0}` syntax | Throw `FormatException` | Not supported; use `{}` only |
| Escaped braces `{{` `}}` | Literal `{` `}` in output | Standard escape syntax |
| More `{}` than values | Throw `ArgumentException` | Fail fast for programming errors |
| Fewer `{}` than values | Throw `ArgumentException` | Fail fast for programming errors |
| Empty template `""` | Empty FormattedText | Valid edge case |
| Callable returns callable | Evaluate recursively until non-callable | Match Python behavior |

#### Conversion/Merge Edge Cases

| Scenario | Expected Behavior | Rationale |
|----------|-------------------|-----------|
| Null in merge list | Skip (treat as empty) | Convenience |
| Empty string in merge | Skip | Avoid empty fragments |
| Circular callable reference | Stack overflow (no protection) | Caller responsibility |
| Style prefix + existing style | Prefix prepended: `"bold class:a"` → `"italic bold class:a"` | Cascade order |
| autoConvert with object | Call `.ToString()` first | Convenience mode |

## Requirements *(mandatory)*

### Functional Requirements

#### Core Types (FR-001 to FR-006)

- **FR-001**: System MUST provide a `StyleAndTextTuple` record struct that holds:
  - `Style`: string (style specification)
  - `Text`: string (text content)
  - `MouseHandler`: optional callback with signature `Func<MouseEvent, NotImplementedOrNone>?`
- **FR-002**: System MUST use `IReadOnlyList<StyleAndTextTuple>` as the canonical collection type for styled text fragments
- **FR-003**: System MUST provide an `IFormattedText` interface with `IReadOnlyList<StyleAndTextTuple> ToFormattedText()` method (C# equivalent of Python's `__pt_formatted_text__` protocol)
- **FR-004**: System MUST provide a `FormattedText` sealed class that:
  - Implements `IFormattedText`
  - Stores fragments in `ImmutableArray<StyleAndTextTuple>`
  - Provides static `Empty` singleton property
  - Implements `IEquatable<FormattedText>` with value semantics (two instances are equal if their fragment lists are sequence-equal by Style, Text, and MouseHandler)
- **FR-005**: System MUST provide `FormattedTextUtils` static class with conversion methods (replacing `FormattedTextConverter` concept from Python `to_formatted_text` function)
- **FR-006**: System MUST support applying additional style prefixes during conversion, prepending to existing styles with space separator

#### HTML Parser (FR-007 to FR-012)

- **FR-007**: System MUST provide an `Html` class that parses HTML-like markup into formatted text
- **FR-008**: HTML parser MUST support `<style>` element with attributes:
  - `fg` or `color`: foreground color (trimmed of whitespace)
  - `bg`: background color (trimmed of whitespace)
- **FR-009**: HTML parser MUST support `<i>`, `<b>`, `<u>`, `<s>` elements mapping to `class:i`, `class:b`, `class:u`, `class:s`
- **FR-010**: HTML parser MUST convert any element name to a CSS class (e.g., `<username>` becomes `class:username`)
- **FR-011**: HTML parser MUST support nested elements accumulating class names (e.g., `class:outer,inner`)
- **FR-012**: System MUST provide `Html.Escape()` static method to escape `&`, `<`, `>`, `"`, `'` characters

#### ANSI Parser (FR-013 to FR-021)

- **FR-013**: System MUST provide an `Ansi` class that parses ANSI escape sequences into formatted text
- **FR-014**: ANSI parser MUST support SGR attribute codes:
  - 0: reset all
  - 1: bold, 2: dim, 3: italic, 4: underline
  - 5: slow blink, 6: rapid blink, 7: reverse, 8: hidden, 9: strike
- **FR-015**: ANSI parser MUST support SGR disable codes:
  - 22: disable bold AND dim
  - 23: disable italic
  - 24: disable underline
  - 25: disable blink
  - 27: disable reverse
  - 28: disable hidden
  - 29: disable strike
- **FR-016**: ANSI parser MUST support basic ANSI colors:
  - 30-37: standard foreground colors
  - 40-47: standard background colors
  - 90-97: bright foreground colors
  - 100-107: bright background colors
- **FR-017**: ANSI parser MUST support 256-color mode: `38;5;N` (foreground), `48;5;N` (background) where N is 0-255
- **FR-018**: ANSI parser MUST support true color mode: `38;2;R;G;B` (foreground), `48;2;R;G;B` (background) where R,G,B are 0-255
- **FR-019**: ANSI parser MUST handle `\001...\002` sequences as `[ZeroWidthEscape]` style fragments
- **FR-020**: ANSI parser MUST handle cursor forward escape `\x1b[NC` as N space characters with current style
- **FR-021**: System MUST provide `Ansi.Escape()` static method to neutralize escape sequences (replace `\x1b` and `\x9b` with `?`)

#### Fragment Utilities (FR-022 to FR-026)

- **FR-022**: System MUST provide `FragmentListLen()` to count characters, excluding fragments with `[ZeroWidthEscape]` in style
- **FR-023**: System MUST provide `FragmentListWidth()` to calculate display width:
  - CJK ideographs: width 2
  - Combining characters: width 0
  - Control characters: width 0 or -1 per wcwidth
  - Narrow characters: width 1
  - Uses Wcwidth NuGet package for Unicode width lookup
- **FR-024**: System MUST provide `FragmentListToText()` to concatenate fragment text, excluding `[ZeroWidthEscape]` fragments
- **FR-025**: System MUST provide `SplitLines()` to split fragment list by `\n` characters:
  - Treats `\r\n` as single newline
  - Preserves mouse handlers on split fragments
  - Always yields at least one line (empty list → single empty line)
  - Trailing newline produces empty final line
- **FR-026**: System MUST provide `ToPlainText()` to convert any `AnyFormattedText` to plain string

#### Template (FR-027)

- **FR-027**: System MUST provide `Template` class:
  - Constructor accepts template string with `{}` placeholders
  - `Format(params AnyFormattedText[] values)` replaces placeholders in order
  - `{{` and `}}` escape to literal `{` and `}`
  - Positional syntax `{0}`, `{1}` is NOT supported (throws `FormatException`)
  - Placeholder/value count mismatch throws `ArgumentException`

#### Merge and Conversion (FR-028 to FR-030)

- **FR-028**: System MUST provide `Merge()` to concatenate multiple `AnyFormattedText` items:
  - Preserves item order (left to right)
  - Null items are skipped
  - Empty strings are skipped
  - Returns `Func<AnyFormattedText>` for lazy evaluation
- **FR-029**: `Html` and `Ansi` classes MUST provide `Format()` method for safe string interpolation:
  - `Html.Format(params object?[] args)` escapes HTML special chars in args
  - `Ansi.Format(params object?[] args)` neutralizes escape sequences in args
- **FR-030**: `FormattedTextUtils.IsFormattedText()` MUST return true for:
  - `string`
  - `IReadOnlyList<StyleAndTextTuple>`
  - `IFormattedText`
  - `Func<AnyFormattedText>`
  - Returns false for other types regardless of `autoConvert` parameter

#### PygmentsTokens (FR-031)

- **FR-031**: System MUST provide `PygmentsTokens` class that:
  - Implements `IFormattedText`
  - Constructor accepts `IEnumerable<(string TokenType, string Text)>` (Pygments-style token list)
  - Converts token types to `class:{token_type}` style format
  - Example: `("Token.Keyword", "def")` → `("class:pygments.keyword", "def")`

#### AnyFormattedText Union Type (FR-032 to FR-034)

- **FR-032**: System MUST provide `AnyFormattedText` readonly struct as union type with implicit conversions from:
  - `string?`
  - `FormattedText?`
  - `Html`
  - `Ansi`
  - `PygmentsTokens`
  - `Func<AnyFormattedText>?`
- **FR-033**: `AnyFormattedText.ToFormattedText(string style = "")` MUST convert contained value to `FormattedText`, applying style prefix if provided
- **FR-034**: `AnyFormattedText.IsEmpty` MUST return true for null, empty string, or `FormattedText.Empty`

### Key Entities

| Entity | Type | Description |
|--------|------|-------------|
| `StyleAndTextTuple` | record struct | Single styled fragment: `(Style, Text, MouseHandler?)` |
| `IFormattedText` | interface | Protocol for types convertible to formatted text |
| `FormattedText` | sealed class | Immutable list of fragments with `ImmutableArray` storage |
| `Html` | sealed class | HTML markup parser; caches parsed result |
| `Ansi` | sealed class | ANSI escape parser; lazy parsing with state machine |
| `Template` | sealed class | Template with `{}` placeholders for interpolation |
| `PygmentsTokens` | sealed class | Converts Pygments token lists to formatted text |
| `AnyFormattedText` | readonly struct | Union type accepting any convertible input |
| `FormattedTextUtils` | static class | Utility functions for conversion and manipulation |

### Constants and Special Values

| Constant | Value | Description |
|----------|-------|-------------|
| `ZeroWidthEscape` style | `"[ZeroWidthEscape]"` | Style marker for terminal control sequences |
| `FormattedText.Empty` | singleton | Empty formatted text (zero fragments) |
| `AnyFormattedText.Empty` | default | Default value representing empty formatted text |

### Dependencies

| Dependency | Type | Purpose |
|------------|------|---------|
| `Stroke.Core` (MouseEvent) | Internal | Mouse handler callback parameter type |
| `Wcwidth` NuGet package | External | Unicode character width calculation |
| `System.Xml.Linq` | BCL | HTML/XML parsing (XDocument) |
| `System.Collections.Immutable` | BCL | ImmutableArray for FormattedText storage |

### Thread Safety

All formatted text types MUST be thread-safe per Constitution XI:

| Type | Thread Safety Approach |
|------|------------------------|
| `StyleAndTextTuple` | Immutable record struct (inherently safe) |
| `FormattedText` | Immutable sealed class with ImmutableArray |
| `Html` | Immutable after construction; parse on construction |
| `Ansi` | Immutable after construction; lazy parsing is idempotent |
| `Template` | Immutable after construction |
| `PygmentsTokens` | Immutable after construction |
| `AnyFormattedText` | Immutable readonly struct |
| `FormattedTextUtils` | Stateless static methods |

## Success Criteria *(mandatory)*

### Measurable Outcomes

#### Performance Criteria

| ID | Criterion | Measurement Method |
|----|-----------|-------------------|
| SC-001 | `ToFormattedText()` completes in <1ms for inputs ≤10KB (10,000 characters) | BenchmarkDotNet with 1KB, 5KB, 10KB inputs |
| SC-002 | HTML parsing handles 100KB (100,000 chars) in <100ms | BenchmarkDotNet with 100KB HTML input |
| SC-003 | ANSI parsing throughput ≥10,000 chars/sec on reference hardware | BenchmarkDotNet; reference = GitHub Actions runner |
| SC-008 | Memory allocation for 10KB parse <50KB total allocations | BenchmarkDotNet MemoryDiagnoser |

**Note**: "Performance degradation" for SC-002 is defined as latency increasing by more than 10x compared to 10KB input.

#### Functional Criteria

| ID | Criterion | Verification Method |
|----|-----------|---------------------|
| SC-004 | All 34 functional requirements (FR-001 to FR-034) have passing tests | xUnit test count >= 34 mapping to FRs |
| SC-005 | Line coverage ≥80% across all FormattedText source files | `dotnet test --collect:"XPlat Code Coverage"` |
| SC-006 | FragmentListWidth handles all Unicode categories | Test with: ASCII, CJK, combining, control, zero-width |
| SC-007 | All 18 Python APIs from mapping table have C# implementations | Checklist against Python API Mapping table |

#### Test Mapping to Python

The following Python Prompt Toolkit tests MUST have corresponding C# tests:

| Python Test File | C# Test File | Test Count |
|------------------|--------------|------------|
| `test_formatted_text.py` | `FormattedTextTests.cs` | ~15 tests |
| (HTML tests inline) | `HtmlTests.cs` | ~20 tests |
| (ANSI tests inline) | `AnsiTests.cs` | ~25 tests |
| (Template tests inline) | `TemplateTests.cs` | ~10 tests |
| (PygmentsTokens tests) | `PygmentsTokensTests.cs` | ~5 tests |

## Downstream Integration *(informational)*

This section documents how the Formatted Text System integrates with other Stroke components.

### Consumers

| Consumer | Usage | Contract |
|----------|-------|----------|
| `Stroke.Rendering` (Screen) | Renders styled fragments to terminal | Reads `StyleAndTextTuple` style strings |
| `Stroke.Layout` (Controls) | Displays formatted text content | Accepts `AnyFormattedText` for content |
| `Stroke.Completion` | Shows styled completion items | Uses `AnyFormattedText` for display/meta |
| `Stroke.Shortcuts` (PromptSession) | Displays prompt messages | Accepts `AnyFormattedText` for message |
| `FormattedTextControl` (future) | Dedicated control for formatted text | Accepts `AnyFormattedText`, handles mouse |

### Style String Interpretation

Downstream renderers interpret style strings with these rules:

| Style Format | Meaning |
|--------------|---------|
| `""` | Default/unstyled |
| `"class:name"` | CSS-like class lookup |
| `"class:a,b,c"` | Multiple classes (cascading) |
| `"fg:color"` | Foreground color |
| `"bg:color"` | Background color |
| `"bold"` | Bold attribute |
| `"italic"` | Italic attribute |
| `"underline"` | Underline attribute |
| `"[ZeroWidthEscape]"` | Non-rendered terminal control |

### Design Decisions

| Decision | Rationale |
|----------|-----------|
| HTMLFormatter/AnsiFormatter are internal | Internal helpers for Format() methods |
| AnsiColors is internal static data | Color name → code mapping not needed publicly |
| No FormattedTextControl in this feature | Deferred to Layout system implementation |
| Wcwidth as external dependency | No pure-C# Unicode width library available |

## Out of Scope

The following are explicitly NOT part of this feature:

- **Rendering**: How styled fragments are displayed (handled by Stroke.Rendering)
- **Style resolution**: How class names map to colors/attributes (handled by StyleSheet)
- **FormattedTextControl**: A layout control for formatted text (future Layout feature)
- **Rich text editing**: In-place editing of formatted text
- **Syntax highlighting**: The lexer/tokenizer itself (PygmentsTokens only converts results)
