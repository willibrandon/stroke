# Feature Specification: HTML Formatted Text

**Feature Branch**: `019-html-formatted-text`
**Created**: 2026-01-26
**Status**: Draft
**Input**: User description: "Implement HTML-like markup parsing for styled formatted text, allowing users to write styled content using familiar XML-like syntax."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Parse Basic HTML Markup (Priority: P1)

A developer wants to create styled terminal output using familiar HTML-like syntax. They write markup like `<b>Hello</b> <i>World</i>` and expect it to be converted into styled text fragments that can be rendered with bold and italic formatting.

**Why this priority**: This is the core functionality - without basic parsing, no other features work. It enables the fundamental use case of creating styled text with familiar syntax.

**Independent Test**: Can be fully tested by creating an Html instance with simple markup and verifying the output fragments contain correct style classes.

**Acceptance Scenarios**:

1. **Given** an Html instance with `<b>Bold</b>`, **When** the formatted text is retrieved, **Then** the output contains a fragment with style `class:b` and text `Bold`
2. **Given** an Html instance with `<i>Italic</i>`, **When** the formatted text is retrieved, **Then** the output contains a fragment with style `class:i` and text `Italic`
3. **Given** an Html instance with `<u>Underline</u>`, **When** the formatted text is retrieved, **Then** the output contains a fragment with style `class:u` and text `Underline`
4. **Given** an Html instance with `<s>Strike</s>`, **When** the formatted text is retrieved, **Then** the output contains a fragment with style `class:s` and text `Strike`

---

### User Story 2 - Apply Foreground and Background Colors (Priority: P1)

A developer wants to colorize their terminal output by specifying foreground and background colors. They use the `<style>` element with `fg` and `bg` attributes to apply colors to text segments.

**Why this priority**: Color support is essential for practical terminal UI styling and is a primary use case alongside basic formatting.

**Independent Test**: Can be fully tested by creating Html instances with color attributes and verifying the style strings contain correct `fg:` and `bg:` specifications.

**Acceptance Scenarios**:

1. **Given** an Html instance with `<style fg="ansired">Error</style>`, **When** the formatted text is retrieved, **Then** the output contains a fragment with style `fg:ansired` and text `Error`
2. **Given** an Html instance with `<style bg="ansiblue">Highlight</style>`, **When** the formatted text is retrieved, **Then** the output contains a fragment with style `bg:ansiblue` and text `Highlight`
3. **Given** an Html instance with `<style fg="ansired" bg="ansiwhite">Alert</style>`, **When** the formatted text is retrieved, **Then** the output contains a fragment with style containing both `fg:ansired` and `bg:ansiwhite`
4. **Given** an Html instance with `<style color="ansigreen">OK</style>`, **When** the formatted text is retrieved, **Then** the `color` attribute is treated as an alias for `fg`, producing `fg:ansigreen`

---

### User Story 3 - Create Custom Style Classes (Priority: P2)

A developer wants to define semantic markup for their application. They use custom element names like `<username>john</username>` which become style classes that can be themed via a style sheet.

**Why this priority**: Custom style classes enable theming and semantic markup, which are important for building maintainable UIs, but basic formatting and colors are needed first.

**Independent Test**: Can be fully tested by creating Html with custom element names and verifying the output contains `class:` prefixed style names.

**Acceptance Scenarios**:

1. **Given** an Html instance with `<username>john</username>`, **When** the formatted text is retrieved, **Then** the output contains a fragment with style `class:username` and text `john`
2. **Given** an Html instance with `<error>Failed</error>`, **When** the formatted text is retrieved, **Then** the output contains a fragment with style `class:error` and text `Failed`
3. **Given** an Html instance with `<custom fg="ansired">Text</custom>`, **When** the formatted text is retrieved, **Then** the output contains a fragment with style `class:custom fg:ansired` and text `Text`

---

### User Story 4 - Nest Elements with Combined Styles (Priority: P2)

A developer wants to combine multiple style attributes by nesting elements. For example, `<b><i>Bold Italic</i></b>` should produce text that is both bold and italic.

**Why this priority**: Nesting enables rich styling combinations, but requires basic element parsing to work first.

**Independent Test**: Can be fully tested by creating Html with nested elements and verifying the output fragments contain combined style classes.

**Acceptance Scenarios**:

1. **Given** an Html instance with `<b><i>BoldItalic</i></b>`, **When** the formatted text is retrieved, **Then** the output contains a fragment with style `class:b,i` and text `BoldItalic`
2. **Given** an Html instance with `<outer><inner>Text</inner></outer>`, **When** the formatted text is retrieved, **Then** the output contains a fragment with style `class:outer,inner` and text `Text`
3. **Given** an Html instance with `<b><style fg="ansired">ColoredBold</style></b>`, **When** the formatted text is retrieved, **Then** the output contains a fragment with style `class:b fg:ansired` and text `ColoredBold`
4. **Given** an Html instance with `<style fg="red"><style fg="blue">Text</style></style>`, **When** the formatted text is retrieved, **Then** the innermost `fg` attribute takes precedence, producing `fg:blue`

---

### User Story 5 - Format Strings with Safe Escaping (Priority: P3)

A developer wants to safely interpolate user-provided data into HTML markup without risking markup injection. They use the Format method which automatically escapes special characters.

**Why this priority**: Safe string formatting is important for security and convenience, but the core parsing must work first.

**Independent Test**: Can be fully tested by calling Format with values containing special characters and verifying they are escaped in the output.

**Acceptance Scenarios**:

1. **Given** an Html instance with `<b>{0}</b>` and Format called with `"<script>"`, **When** the formatted text is retrieved, **Then** the text contains the escaped string `&lt;script&gt;` not literal `<script>`
2. **Given** an Html instance with `Hello {name}` and Format called with a dictionary containing `{"name": "John & Jane"}`, **When** the formatted text is retrieved, **Then** the text contains `John &amp; Jane`
3. **Given** an Html instance with `<b>%s</b>` and modulo operator used with `"<test>"`, **When** the formatted text is retrieved, **Then** the text contains the escaped string

---

### User Story 6 - HTML Escape Utility (Priority: P3)

A developer wants to manually escape text for inclusion in HTML markup without going through the Format method. They use the HtmlEscape utility function.

**Why this priority**: Utility function for manual escaping is helpful but not critical for core functionality.

**Independent Test**: Can be fully tested by calling HtmlEscape with various inputs and verifying correct escaping of special characters.

**Acceptance Scenarios**:

1. **Given** the HtmlEscape function called with `"<tag>"`, **Then** the result is `"&lt;tag&gt;"`
2. **Given** the HtmlEscape function called with `"A & B"`, **Then** the result is `"A &amp; B"`
3. **Given** the HtmlEscape function called with `"\"quoted\""`, **Then** the result is `"&quot;quoted&quot;"`
4. **Given** the HtmlEscape function called with `123` (an integer), **Then** the result is `"123"` (converted to string first)

---

### Edge Cases

**Input Validation**:
- Null input → throws `ArgumentNullException`
- Empty string `""` → returns empty `IReadOnlyList<StyleAndTextTuple>`
- Whitespace-only input `"   "` → returns single fragment with empty style and whitespace text
- Malformed XML → throws `FormatException` with message "Invalid HTML markup: {details}"
- Space in `fg`/`bg`/`color` attribute → throws `FormatException` with specific message

**XML Constructs**:
- Self-closing elements `<br/>` → valid XML, produces no text content
- XML comments `<!-- comment -->` → ignored (standard XML parsing)
- CDATA sections `<![CDATA[text]]>` → text extracted without markup interpretation
- XML processing instructions `<?xml?>` → handled by XML parser, ignored for styling
- Duplicate attributes on same element → behavior undefined by XML spec; parser may throw or use last value

**Nesting & Depth**:
- Deeply nested elements (10+ levels) → supported; style classes accumulate
- Empty text nodes between elements → preserved as empty fragments
- Mixed content `<b>Hello <i>World</i></b>` → produces multiple fragments: `("class:b", "Hello ")`, `("class:b,i", "World")`

**Text Content**:
- Unicode text → fully supported (UTF-8/UTF-16 via .NET strings)
- Unicode in element names → supported if valid XML name characters
- Very long text (>1MB) → supported up to available memory
- Very long attribute values → supported up to available memory

**Color Attributes**:
- Nested color attributes → innermost takes precedence (stack LIFO)
- Both `fg` and `color` on same element → `fg` takes precedence
- Empty color value `fg=""` → produces `fg:` in style string (may be invalid for renderer)

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST parse HTML-like markup into styled text fragments
- **FR-002**: System MUST recognize `<b>`, `<i>`, `<u>`, `<s>` elements as predefined style classes
- **FR-003**: System MUST treat custom element names as style class names (e.g., `<username>` becomes `class:username`)
- **FR-004**: System MUST support `fg` attribute for foreground color specification
- **FR-005**: System MUST support `bg` attribute for background color specification
- **FR-006**: System MUST support `color` attribute as an alias for `fg`
- **FR-007**: System MUST combine styles from nested elements using comma-separated class names in nesting order (outermost first)
- **FR-008**: System MUST use stack-based color resolution where innermost color takes precedence
- **FR-009**: System MUST throw `FormatException` with message "Invalid HTML markup: {details}" when markup is not well-formed XML
- **FR-010**: System MUST throw `FormatException` with message `"\"fg\" attribute contains a space."` (or `bg`/`color`) when color attributes contain spaces
- **FR-011**: System MUST provide a Format method that escapes special characters in interpolated values
- **FR-012**: System MUST provide an `Html.Escape(object?)` static method for manual text escaping
- **FR-013**: System MUST escape exactly these four characters: `&` → `&amp;`, `<` → `&lt;`, `>` → `&gt;`, `"` → `&quot;`
- **FR-014**: System MUST convert non-string values to strings via `ToString()` before escaping; null values become empty string
- **FR-015**: System MUST preserve the original markup string in the `Value` property for inspection
- **FR-016**: System MUST support the `%` operator for `%s`-style string interpolation with automatic escaping
- **FR-017**: System MUST exclude `html-root`, `#document`, and `style` elements from the class name stack

### Color Format Requirements

- **FR-018**: Color attribute values MUST be passed through to the style string without validation
- **FR-019**: System MUST support ANSI color names (e.g., `ansired`, `ansiblue`, `ansiwhite`)
- **FR-020**: System MUST support hex color codes (e.g., `#ff0000`, `#00ff00`)
- **FR-021**: System MUST support CSS named colors (e.g., `red`, `blue`, `forestgreen`)
- **FR-022**: When both `fg` and `color` attributes are present on the same element, `fg` takes precedence

### Character Encoding Requirements

- **FR-023**: System MUST decode standard XML entities (`&amp;`, `&lt;`, `&gt;`, `&quot;`, `&apos;`) in text content
- **FR-024**: System MUST decode numeric character references (`&#60;`, `&#x3C;`) via standard XML parsing
- **FR-025**: System MUST preserve whitespace exactly as specified in the input markup
- **FR-026**: Single quote (`'`) is intentionally NOT escaped by `Html.Escape()` to match Python Prompt Toolkit behavior

### API Contract Requirements

- **FR-027**: `Html` class MUST be `sealed` and implement `IFormattedText`
- **FR-028**: `ToFormattedText()` MUST return `IReadOnlyList<StyleAndTextTuple>`
- **FR-029**: Constructor MUST throw `ArgumentNullException` when value is null
- **FR-030**: `Format(params object[] args)` MUST substitute `{0}`, `{1}`, etc. placeholders with escaped values
- **FR-031**: `Format(IDictionary<string, object> args)` MUST substitute `{name}` placeholders with escaped values
- **FR-032**: `Format()` with missing placeholders MUST leave the placeholder text unchanged
- **FR-033**: `%` operator with insufficient arguments MUST leave remaining `%s` placeholders unchanged
- **FR-034**: `Format()` dictionary with null values MUST treat them as empty strings

### Key Entities

- **Html**: A `sealed` class that parses HTML-like markup and produces formatted text. Implements `IFormattedText`. Holds the original markup string in `Value` property and cached parsed fragments. Thread-safe due to immutability.
- **StyleAndTextTuple**: A `record struct` containing `Style` (string), `Text` (string), and optional `MouseHandler`. Already exists from Feature 015.
- **IFormattedText**: Interface with `ToFormattedText()` returning `IReadOnlyList<StyleAndTextTuple>`. Already exists from Feature 015.

### Style String Format

The style string follows a precise grammar:

```
style-string = [class-part] [fg-part] [bg-part]
class-part   = "class:" class-list
class-list   = class-name ("," class-name)*
class-name   = element-name  ; excluding "html-root", "#document", "style"
fg-part      = " fg:" color-value
bg-part      = " bg:" color-value
color-value  = <any non-space string>
```

**Examples**:
| Markup | Style String |
|--------|--------------|
| `plain text` | `""` (empty) |
| `<b>text</b>` | `"class:b"` |
| `<b><i>text</i></b>` | `"class:b,i"` |
| `<style fg="red">text</style>` | `"fg:red"` |
| `<b><style fg="red" bg="blue">text</style></b>` | `"class:b fg:red bg:blue"` |

## Security Considerations

### Injection Prevention

The `Html.Escape()` function and `Format()` method protect against markup injection by escaping:
- `&` → `&amp;` (prevents entity injection)
- `<` → `&lt;` (prevents element injection)
- `>` → `&gt;` (prevents element closing injection)
- `"` → `&quot;` (prevents attribute injection)

**Note**: Single quote (`'`) is NOT escaped. This matches Python Prompt Toolkit behavior. Attribute values should use double quotes in templates.

### XML Security

The implementation uses `System.Xml.Linq.XDocument.Parse()` with default settings which provides:
- **XXE Prevention**: External entity resolution is disabled by default in .NET
- **DTD Prevention**: DTD processing is disabled by default in .NET
- **Billion Laughs**: .NET XML parser has built-in limits on entity expansion

No additional security configuration is required for the XML parser.

### Input Validation

- Null input throws `ArgumentNullException` (standard .NET pattern)
- Malformed XML throws `FormatException` (no crash, no undefined behavior)
- Space in color attributes throws `FormatException` (prevents style string corruption)

## Technical Constraints

### Dependencies

- **System.Xml.Linq**: Used for XML parsing (`XDocument.Parse()` with `LoadOptions.PreserveWhitespace`)
- **Feature 015 (FormattedText)**: Depends on `IFormattedText`, `StyleAndTextTuple` types

### Platform Assumptions

- Input strings are valid .NET `string` objects (UTF-16 encoded)
- No file I/O or network access required
- Thread-safe due to immutability (no synchronization required)

### Performance Characteristics

- Parsing occurs once at construction time; result is cached
- `ToFormattedText()` returns cached result (O(1) after construction)
- Memory usage proportional to input size plus fragment count
- No lazy evaluation; entire markup is parsed eagerly

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: All four formatting elements (`<b>`, `<i>`, `<u>`, `<s>`) produce style classes `class:b`, `class:i`, `class:u`, `class:s` respectively
- **SC-002**: `fg="value"` produces `fg:value`, `bg="value"` produces `bg:value`, `color="value"` produces `fg:value`
- **SC-003**: Nested elements `<a><b>text</b></a>` produce `class:a,b` (outermost first)
- **SC-004**: Malformed XML produces `FormatException` with message starting with "Invalid HTML markup:"
- **SC-005**: `Html.Escape()` transforms: `&` → `&amp;`, `<` → `&lt;`, `>` → `&gt;`, `"` → `&quot;`
- **SC-006**: Unit tests achieve at least 80% code coverage
- **SC-007**: All 20 acceptance scenarios from User Stories 1-6 pass verification testing

### Traceability Matrix

| Success Criteria | Related FRs |
|------------------|-------------|
| SC-001 | FR-001, FR-002 |
| SC-002 | FR-004, FR-005, FR-006, FR-018-FR-022 |
| SC-003 | FR-007, FR-008, FR-017 |
| SC-004 | FR-009, FR-010 |
| SC-005 | FR-012, FR-013, FR-014, FR-026 |
| SC-006 | N/A (process metric) |
| SC-007 | FR-001 through FR-034 |
