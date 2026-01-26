# Data Model: Formatted Text System

**Feature**: 015-formatted-text-system
**Date**: 2026-01-25

## Entities

### StyleAndTextTuple

**Description**: A single styled text fragment represented as a (style, text) pair with optional mouse handler.

**Fields**:
| Field | Type | Description | Validation |
|-------|------|-------------|------------|
| Style | `string` | Style class name (e.g., "class:b", "fg:red bg:blue", "bold") | Not null (use empty string for unstyled) |
| Text | `string` | The text content | Not null |
| MouseHandler | `Func<MouseEvent, NotImplementedOrNone>?` | Optional callback for mouse events | May be null |

**State Transitions**: None (immutable value type)

**Relationships**:
- Contained in `FormattedText` collections
- Produced by `Html`, `Ansi`, `PygmentsTokens` parsing

### FormattedText

**Description**: An immutable list of `StyleAndTextTuple` fragments representing formatted text.

**Fields**:
| Field | Type | Description | Validation |
|-------|------|-------------|------------|
| _fragments | `ImmutableArray<StyleAndTextTuple>` | Internal storage | Initialized on construction |

**State Transitions**: None (immutable)

**Relationships**:
- Implements `IFormattedText`
- Implements `IReadOnlyList<StyleAndTextTuple>`
- Returned by `Html.ToFormattedText()`, `Ansi.ToFormattedText()`, etc.

### Html

**Description**: Parses HTML-like markup into formatted text.

**Fields**:
| Field | Type | Description | Validation |
|-------|------|-------------|------------|
| Value | `string` | The original HTML input | Not null |
| _formattedText | `FormattedText` | Cached parse result | Computed on construction |

**Parsing Rules**:
- `<style fg="..." bg="...">` → style with "fg:..." and/or "bg:..."
- `<b>`, `<i>`, `<u>`, `<s>` → style with "class:b", "class:i", "class:u", "class:s"
- `<anyname>` → style with "class:anyname"
- Nested elements accumulate: `<a><b>text</b></a>` → "class:a,b"
- Text nodes produce fragments with current accumulated style
- fg/bg stacks push/pop with element scope

**State Transitions**: None (immutable)

**Relationships**:
- Implements `IFormattedText`
- Uses `HtmlFormatter` for safe interpolation

### Ansi

**Description**: Parses ANSI escape sequences into formatted text.

**Fields**:
| Field | Type | Description | Validation |
|-------|------|-------------|------------|
| Value | `string` | The original ANSI input | Not null |
| _formattedText | `List<StyleAndTextTuple>` | Parse result | Built during parsing |

**Parser State** (during construction):
| Field | Type | Description |
|-------|------|-------------|
| _color | `string?` | Current foreground color |
| _bgcolor | `string?` | Current background color |
| _bold | `bool` | Bold attribute |
| _dim | `bool` | Dim attribute |
| _italic | `bool` | Italic attribute |
| _underline | `bool` | Underline attribute |
| _blink | `bool` | Blink attribute |
| _reverse | `bool` | Reverse video attribute |
| _hidden | `bool` | Hidden attribute |
| _strike | `bool` | Strikethrough attribute |

**SGR Code Handling**:
| Code | Effect |
|------|--------|
| 0 | Reset all attributes |
| 1 | Set bold |
| 2 | Set dim |
| 3 | Set italic |
| 4 | Set underline |
| 5, 6 | Set blink |
| 7 | Set reverse |
| 8 | Set hidden |
| 9 | Set strike |
| 22 | Reset bold and dim |
| 23 | Reset italic |
| 24 | Reset underline |
| 25 | Reset blink |
| 27 | Reset reverse |
| 28 | Reset hidden |
| 29 | Reset strike |
| 30-37 | Set foreground (basic) |
| 38;5;N | Set foreground (256-color) |
| 38;2;R;G;B | Set foreground (true color) |
| 39 | Reset foreground to default |
| 40-47 | Set background (basic) |
| 48;5;N | Set background (256-color) |
| 48;2;R;G;B | Set background (true color) |
| 49 | Reset background to default |
| 90-97 | Set foreground (bright) |
| 100-107 | Set background (bright) |

**Special Sequences**:
| Sequence | Effect |
|----------|--------|
| `\x1b[Nm` | SGR (Select Graphic Rendition) |
| `\x1b[NC` | Cursor forward N spaces |
| `\001...\002` | ZeroWidthEscape fragment |
| `\x9b` | CSI (equivalent to `\x1b[`) |

**State Transitions**: None (immutable after construction)

**Relationships**:
- Implements `IFormattedText`
- Uses `AnsiFormatter` for safe interpolation
- Uses `AnsiColors` for code-to-name mapping

### Template

**Description**: String template with `{}` placeholders for formatted text interpolation.

**Fields**:
| Field | Type | Description | Validation |
|-------|------|-------------|------------|
| Text | `string` | Template text with `{}` placeholders | Not null, must not contain `{0}` |

**State Transitions**: None (immutable)

**Relationships**:
- `Format()` returns `Func<FormattedText>` (lazy evaluation)
- Accepts `AnyFormattedText` values for interpolation

### PygmentsTokens

**Description**: Converts Pygments-style token list to formatted text.

**Fields**:
| Field | Type | Description | Validation |
|-------|------|-------------|------------|
| TokenList | `IReadOnlyList<(TokenType, string)>` | List of (token, text) pairs | Not null |

**Token Conversion**:
- `(Token.A.B, "text")` → `("class:pygments.a.b", "text")`
- `(Token, "text")` → `("class:pygments", "text")`

**State Transitions**: None (immutable)

**Relationships**:
- Implements `IFormattedText`
- TokenType represented as tuple of strings (e.g., `("Keyword", "Reserved")`)

### AnyFormattedText

**Description**: Union type that can hold string, FormattedText, IFormattedText, or callable.

**Fields**:
| Field | Type | Description | Validation |
|-------|------|-------------|------------|
| Value | `object?` | The underlying value | Must be valid convertible type or null |

**Valid Value Types**:
- `null` → Empty FormattedText
- `string` → Single fragment with empty style
- `FormattedText` → Direct use
- `IFormattedText` → Call `ToFormattedText()`
- `Func<AnyFormattedText>` → Recursive evaluation

**Relationships**:
- Implicit conversions from all valid types
- `ToFormattedText()` method for canonical conversion

## Static Data

### AnsiColors

**FgAnsiColors** (Dictionary<int, string>):
```
30 → "ansiblack"
31 → "ansired"
32 → "ansigreen"
33 → "ansiyellow"
34 → "ansiblue"
35 → "ansimagenta"
36 → "ansicyan"
37 → "ansigray"
39 → "ansidefault"
90 → "ansibrightblack"
91 → "ansibrightred"
92 → "ansibrightgreen"
93 → "ansibrightyellow"
94 → "ansibrightblue"
95 → "ansibrightmagenta"
96 → "ansibrightcyan"
97 → "ansiwhite"
```

**BgAnsiColors** (Dictionary<int, string>):
```
40 → "ansiblack"
41 → "ansired"
42 → "ansigreen"
43 → "ansiyellow"
44 → "ansiblue"
45 → "ansimagenta"
46 → "ansicyan"
47 → "ansigray"
49 → "ansidefault"
100 → "ansibrightblack"
101 → "ansibrightred"
102 → "ansibrightgreen"
103 → "ansibrightyellow"
104 → "ansibrightblue"
105 → "ansibrightmagenta"
106 → "ansibrightcyan"
107 → "ansiwhite"
```

**Colors256** (List<(byte r, byte g, byte b)>):
- Indices 0-15: Basic ANSI colors (platform-dependent RGB values)
- Indices 16-231: 6×6×6 color cube
- Indices 232-255: 24-step grayscale

## Entity Relationships Diagram

```
┌─────────────────────┐
│   AnyFormattedText  │
│   (union wrapper)   │
└──────────┬──────────┘
           │ contains one of
           ▼
┌──────────────────────────────────────────────────────┐
│                                                      │
│  ┌────────┐  ┌──────────────┐  ┌──────┐  ┌────────┐  │
│  │ string │  │ FormattedText│  │ Html │  │  Ansi  │  │
│  └────────┘  │ (implements  │  │      │  │        │  │
│              │ IFormattedText│  │      │  │        │  │
│              └───────┬──────┘  └──┬───┘  └───┬────┘  │
│                      │            │          │       │
│                      ▼            ▼          ▼       │
│              ┌──────────────────────────────────┐    │
│              │    IFormattedText interface     │    │
│              │    ToFormattedText() method     │    │
│              └──────────────────────────────────┘    │
│                                                      │
│  ┌────────────┐  ┌───────────────┐                   │
│  │  Template  │  │ PygmentsTokens│ (also implements  │
│  │ (produces  │  │               │  IFormattedText)  │
│  │  callable) │  └───────────────┘                   │
│  └────────────┘                                      │
│                                                      │
└──────────────────────────────────────────────────────┘
           │
           │ all convert to
           ▼
┌─────────────────────────────────────┐
│  IReadOnlyList<StyleAndTextTuple>   │
│  (canonical form)                   │
└─────────────────────────────────────┘
           │
           │ collection of
           ▼
┌─────────────────────────────────────┐
│       StyleAndTextTuple             │
│  (Style, Text, MouseHandler?)       │
└─────────────────────────────────────┘
```
