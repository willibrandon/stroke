# Data Model: Styles System

**Feature**: 018-styles-system
**Date**: 2026-01-26
**Status**: Complete

## Entities

### Attrs

**Description**: Immutable value type representing 10 style attributes for terminal text formatting.

**Fields**:
| Field | Type | Default | Description |
|-------|------|---------|-------------|
| Color | `string?` | `null` | Foreground color (hex "rrggbb" or ANSI name "ansiblue") |
| BgColor | `string?` | `null` | Background color (hex "rrggbb" or ANSI name "ansired") |
| Bold | `bool?` | `null` | Bold text |
| Underline | `bool?` | `null` | Underlined text |
| Strike | `bool?` | `null` | Strikethrough text |
| Italic | `bool?` | `null` | Italic text |
| Blink | `bool?` | `null` | Blinking text |
| Reverse | `bool?` | `null` | Reversed foreground/background |
| Hidden | `bool?` | `null` | Hidden text |
| Dim | `bool?` | `null` | Dim/faint text |

**Invariants**:
- Nullable fields allow inheritance from parent styles
- Non-null color fields are either empty string (default), lowercase 6-digit hex, or ANSI name

**Relationships**:
- Used by `IStyle.GetAttrsForStyleStr()` as return type
- Used by `IStyleTransformation.TransformAttrs()` as input and output

---

### Style

**Description**: Immutable style definition containing ordered rules that map class name combinations to attributes.

**Fields**:
| Field | Type | Description |
|-------|------|-------------|
| StyleRules | `IReadOnlyList<(string ClassNames, string StyleDef)>` | Original rules in order |
| _classNamesAndAttrs | `IReadOnlyList<(FrozenSet<string>, Attrs)>` | Parsed rules for matching |
| _cache | `SimpleCache<string, Attrs>` | Computed attrs cache |

**Invariants**:
- Rules are immutable after construction
- Class names in rules are normalized to lowercase
- Later rules override earlier rules for same class

**Relationships**:
- Implements `IStyle`
- Created from list of tuples or dictionary via `FromDict`
- Can be merged via `StyleMerger.MergeStyles()`

---

### Priority

**Description**: Enum controlling rule precedence when creating Style from dictionary.

**Values**:
| Value | Description |
|-------|-------------|
| DictKeyOrder | Rules applied in dictionary iteration order (default) |
| MostPrecise | Rules with more specific class names get higher priority |

---

### Color

**Description**: Logical concept (not a separate type) - colors are represented as strings.

**Valid Formats**:
| Format | Example | Normalized To |
|--------|---------|---------------|
| ANSI name | `"ansiblue"` | `"ansiblue"` (preserved) |
| ANSI alias | `"ansibrown"` | `"ansiyellow"` (resolved) |
| Named color | `"AliceBlue"` | `"f0f8ff"` (hex, no #) |
| 6-digit hex | `"#ff0000"` | `"ff0000"` (no #) |
| 3-digit hex | `"#f00"` | `"ff0000"` (expanded) |
| Default | `""` or `"default"` | `""` |

---

### IStyleTransformation

**Description**: Interface for post-processing operations applied to computed Attrs.

**Operations**:
| Method | Description |
|--------|-------------|
| TransformAttrs(Attrs) | Transform attrs and return new attrs |
| InvalidationHash | Returns object for cache invalidation |

**Implementations**:
| Class | Behavior |
|-------|----------|
| DummyStyleTransformation | Returns attrs unchanged |
| ReverseStyleTransformation | Toggles reverse attribute |
| SwapLightAndDarkStyleTransformation | Inverts color luminosity |
| SetDefaultColorStyleTransformation | Sets fallback fg/bg colors |
| AdjustBrightnessStyleTransformation | Constrains color brightness |
| ConditionalStyleTransformation | Applies based on filter |
| DynamicStyleTransformation | Delegates to callable |
| _MergedStyleTransformation | Applies sequence of transformations |

---

### AnsiColorNames

**Description**: Static class containing ANSI color name constants.

**Data**:
| Property | Type | Count | Description |
|----------|------|-------|-------------|
| Names | `IReadOnlyList<string>` | 17 | Standard ANSI color names |
| Aliases | `IReadOnlyDictionary<string, string>` | 10 | Backward-compatible aliases |

**Standard Names** (17):
```
ansidefault, ansiblack, ansired, ansigreen, ansiyellow, ansiblue,
ansimagenta, ansicyan, ansigray, ansibrightblack, ansibrightred,
ansibrightgreen, ansibrightyellow, ansibrightblue, ansibrightmagenta,
ansibrightcyan, ansiwhite
```

**Aliases** (10):
```
ansidarkgray → ansibrightblack
ansiteal → ansicyan
ansiturquoise → ansibrightcyan
ansibrown → ansiyellow
ansipurple → ansimagenta
ansifuchsia → ansibrightmagenta
ansilightgray → ansigray
ansidarkred → ansired
ansidarkgreen → ansigreen
ansidarkblue → ansiblue
```

---

### NamedColors

**Description**: Static class containing 140 HTML/CSS named colors.

**Data**:
| Property | Type | Count | Description |
|----------|------|-------|-------------|
| Colors | `IReadOnlyDictionary<string, string>` | 140 | Name to hex mapping |

**Sample Entries**:
```
"AliceBlue" → "f0f8ff"
"AntiqueWhite" → "faebd7"
...
"YellowGreen" → "9acd32"
```

---

### DefaultStyles

**Description**: Static class providing pre-built default styles.

**Properties**:
| Property | Type | Description |
|----------|------|-------------|
| DefaultUiStyle | `IStyle` | Merged style for UI elements |
| DefaultPygmentsStyle | `IStyle` | Style for syntax highlighting |

**DefaultUiStyle Composition**:
1. PROMPT_TOOLKIT_STYLE rules (68 rules)
2. COLORS_STYLE rules (157 rules: 17 ANSI + 140 named)
3. WIDGETS_STYLE rules (19 rules)

**DefaultPygmentsStyle**:
- 34 rules for Pygments token types

---

### PygmentsStyleUtils

**Description**: Static utility class for creating styles from Pygments-compatible token dictionaries.

**Methods**:
| Method | Parameters | Returns | Description |
|--------|------------|---------|-------------|
| PygmentsTokenToClassName | `string[] tokenPath` | `string` | Converts token path to class name |
| StyleFromPygmentsDict | `IReadOnlyDictionary<string[], string>` | `Style` | Creates Style from token dictionary |
| StyleFromPygmentsClass&lt;T&gt; | (generic type) | `Style` | Creates Style from Pygments-style class |

**Token Path Conversion Examples**:
| Token Path | Class Name |
|------------|------------|
| `["Keyword"]` | `"pygments.keyword"` |
| `["Name", "Function"]` | `"pygments.name.function"` |
| `["Name", "Exception"]` | `"pygments.name.exception"` |
| `["Comment", "Single"]` | `"pygments.comment.single"` |
| `["String", "Doc"]` | `"pygments.string.doc"` |

**Invariants**:
- Token path elements are lowercased in output
- "pygments" prefix is always added
- Elements are joined with dots

---

## State Transitions

### Style String Parsing State Machine

```
START
  │
  ├─ "noinherit" ──→ RESET_TO_DEFAULT_ATTRS
  │
  ├─ "class:name" ──→ EXPAND_AND_MATCH_CLASSES
  │
  ├─ attribute ──→ UPDATE_ATTR (bold, nobold, italic, etc.)
  │
  ├─ "fg:color" ──→ PARSE_AND_SET_FG_COLOR
  │
  ├─ "bg:color" ──→ PARSE_AND_SET_BG_COLOR
  │
  ├─ color ──→ PARSE_AND_SET_FG_COLOR (implicit fg:)
  │
  ├─ ignored ──→ SKIP (roman, sans, mono, border:*, [*])
  │
  └─ whitespace ──→ CONTINUE
```

### Attrs Merging

When merging a list of Attrs:
```
For each attribute (color, bold, etc.):
  result = first non-null value from end of list
  if all null, use default (empty string for colors, false for booleans)
```

## Validation Rules

### Color Validation
1. Empty string or "default" → valid (default color)
2. ANSI name (in Names list) → valid
3. ANSI alias (in Aliases keys) → valid (resolved to canonical name)
4. Named color (in Colors keys, case-insensitive) → valid (converted to hex)
5. #RRGGBB or RRGGBB (6 hex digits) → valid
6. #RGB (3 hex digits) → valid (expanded to RRGGBB)
7. Anything else → `ArgumentException`

### Class Name Validation
- Pattern: `[a-z0-9.\s_-]*`
- No commas in class names (commas are list separators)
- Normalized to lowercase

### Brightness Validation
- `min_brightness` must be in range [0.0, 1.0]
- `max_brightness` must be in range [0.0, 1.0]
- `min_brightness <= max_brightness` (not enforced, but expected)
