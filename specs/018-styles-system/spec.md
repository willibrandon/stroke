# Feature Specification: Styles System

**Feature Branch**: `018-styles-system`
**Created**: 2026-01-26
**Status**: Draft
**Input**: User description: "Implement the styling system for defining and applying visual styles to formatted text."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Define Custom Styles for Application UI (Priority: P1)

As a terminal application developer, I need to define custom styles for my application's UI components so that I can create a visually consistent and branded user experience. I want to specify colors (foreground and background), text attributes (bold, italic, underline), and organize styles by class names for easy maintenance and reuse.

**Why this priority**: This is the foundational capability that enables all styling in the framework. Without the ability to define styles, no visual customization is possible. This unlocks all other styling features.

**Independent Test**: Can be fully tested by creating a Style object with multiple rules, retrieving Attrs for various style strings, and verifying the correct attributes are returned.

**Acceptance Scenarios**:

1. **Given** a developer creates a Style with rules `[("title", "#ff0000 bold"), ("subtitle", "#666666 italic")]`, **When** they request attrs for `"class:title"`, **Then** they receive Attrs with color="ff0000" and bold=true
2. **Given** a developer creates a Style from a dictionary with class names and style definitions, **When** they request attrs for any defined class, **Then** they receive the corresponding Attrs
3. **Given** a developer specifies inline styles like `"bold underline #00ff00"`, **When** attrs are computed, **Then** all specified attributes are correctly applied
4. **Given** a developer uses both class names and inline styles together, **When** attrs are computed, **Then** inline styles take precedence over class styles

---

### User Story 2 - Use Standard Color Names and Formats (Priority: P1)

As a terminal application developer, I need to specify colors using multiple formats including ANSI color names, HTML/CSS named colors, and hex codes so that I can choose the most convenient format for my use case while ensuring cross-platform compatibility.

**Why this priority**: Color specification is essential for any styling system. Supporting multiple formats makes the API accessible to developers with different backgrounds and ensures compatibility with existing color schemes.

**Independent Test**: Can be fully tested by parsing various color formats (ANSI names, named colors, hex codes) and verifying correct normalization and validation.

**Acceptance Scenarios**:

1. **Given** a developer specifies `"ansiblue"` as a color, **When** the color is parsed, **Then** it is recognized as a valid ANSI color
2. **Given** a developer specifies `"AliceBlue"` as a color, **When** the color is parsed, **Then** it is converted to the hex value "f0f8ff"
3. **Given** a developer specifies `"#ff0"` (3-digit hex), **When** the color is parsed, **Then** it is expanded to "ffff00"
4. **Given** a developer specifies `"#ff0000"` (6-digit hex), **When** the color is parsed, **Then** it is stored as "ff0000"
5. **Given** a developer specifies an invalid color like `"notacolor"`, **When** the color is parsed, **Then** an appropriate error is raised

---

### User Story 3 - Merge Multiple Styles (Priority: P2)

As a terminal application developer, I need to merge multiple Style objects together so that I can compose styles from different sources (application defaults, user preferences, component-specific styles) while maintaining correct precedence rules.

**Why this priority**: Style composition is critical for building modular applications where styles come from multiple sources. This enables separation of concerns and customization.

**Independent Test**: Can be fully tested by creating multiple Style objects with overlapping and distinct rules, merging them, and verifying that later styles override earlier ones while non-overlapping rules are preserved.

**Acceptance Scenarios**:

1. **Given** two styles A and B where B defines a rule for "title" that A also defines, **When** they are merged as [A, B], **Then** B's rule takes precedence for "title"
2. **Given** styles A and B with non-overlapping rules, **When** they are merged, **Then** all rules from both styles are available
3. **Given** a merged style, **When** one of the source styles changes, **Then** the merged style's invalidation hash changes

---

### User Story 4 - Transform Styles Dynamically (Priority: P2)

As a terminal application developer, I need to transform style attributes after they are computed so that I can implement features like dark mode, high contrast mode, or color adjustments without redefining all my styles.

**Why this priority**: Style transformations enable runtime customization and accessibility features. They allow a single style definition to work across multiple visual themes.

**Independent Test**: Can be fully tested by applying transformations to Attrs and verifying the output attributes match expected values for each transformation type.

**Acceptance Scenarios**:

1. **Given** attrs with color="000000", **When** SwapLightAndDarkStyleTransformation is applied, **Then** the color is inverted to a light color
2. **Given** attrs with reverse=false, **When** ReverseStyleTransformation is applied, **Then** reverse becomes true
3. **Given** attrs with default/empty colors, **When** SetDefaultColorStyleTransformation is applied with specific defaults, **Then** those defaults are used
4. **Given** attrs with a specific foreground color, **When** AdjustBrightnessStyleTransformation is applied with min_brightness=0.3, **Then** the color brightness is adjusted to be at least 0.3
5. **Given** a conditional transformation with a filter that returns false, **When** applied, **Then** attrs are unchanged

---

### User Story 5 - Use Dynamic and Conditional Styles (Priority: P3)

As a terminal application developer, I need to use styles that can change dynamically at runtime and transformations that apply only under certain conditions so that I can create responsive and context-aware UIs.

**Why this priority**: Dynamic styling enables advanced use cases like theme switching and state-dependent styling. While important, the static styling capabilities must work first.

**Independent Test**: Can be fully tested by creating DynamicStyle and ConditionalStyleTransformation instances, toggling conditions, and verifying correct style/transformation behavior.

**Acceptance Scenarios**:

1. **Given** a DynamicStyle with a callable that returns Style A, **When** attrs are requested, **Then** Style A's rules are used
2. **Given** a DynamicStyle where the callable returns a different Style B, **When** attrs are requested again, **Then** Style B's rules are used
3. **Given** a DynamicStyleTransformation, **When** the underlying transformation changes, **Then** the invalidation hash changes

---

### User Story 6 - Apply Default UI and Pygments Styles (Priority: P3)

As a terminal application developer, I need access to pre-built default styles for common UI elements and syntax highlighting so that I can quickly build applications without defining styles from scratch. I also need utilities to create styles from Pygments-compatible token dictionaries and convert tokens to class names for seamless integration with syntax highlighters.

**Why this priority**: Default styles provide a good starting point and ensure consistent behavior across applications. They are conveniences built on top of the core styling system.

**Independent Test**: Can be fully tested by retrieving default styles and verifying they contain expected rules for common classes like "search", "completion-menu", and Pygments token types. Pygments utilities can be tested by creating styles from token dictionaries and verifying class name conversion.

**Acceptance Scenarios**:

1. **Given** the default UI style, **When** attrs are requested for "class:search", **Then** appropriate search highlighting attrs are returned
2. **Given** the default UI style, **When** attrs are requested for "class:completion-menu", **Then** appropriate menu attrs are returned
3. **Given** the default Pygments style, **When** attrs are requested for "class:pygments.keyword", **Then** appropriate syntax highlighting attrs are returned
4. **Given** a Pygments-style class with token definitions, **When** StyleFromPygmentsClass is called, **Then** a Style with corresponding rules is returned
5. **Given** a dictionary mapping tokens to style strings, **When** StyleFromPygmentsDict is called, **Then** a Style with those rules is created
6. **Given** a Pygments token like `Token.Name.Exception`, **When** PygmentsTokenToClassName is called, **Then** it returns "pygments.name.exception"

---

### Edge Cases

- What happens when an empty style string is passed? Returns default attrs.
- What happens when a class name is not defined in the style? Falls back to default attrs for undefined portions.
- What happens when multiple style attributes conflict (e.g., `"bold nobold"`)? Later attributes override earlier ones.
- What happens when `noinherit` is used? Resets to default attrs before applying other styles.
- What happens when hierarchical class names are used (e.g., `class:a.b.c`)? Expands to match rules for "a", "a.b", and "a.b.c".
- What happens when null styles are passed to merge? They are filtered out.
- What happens when brightness transformation is applied to ANSI colors? ANSI colors are converted to RGB for transformation.
- What happens when colors use aliases (e.g., `ansibrown`)? Aliases are resolved to canonical ANSI names.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST provide an `Attrs` record struct to represent style attributes (color, bgcolor, bold, underline, strike, italic, blink, reverse, hidden, dim)
- **FR-002**: System MUST provide `DefaultAttrs` with `Default` (all values set) and `Empty` (all values null for inheritance) constants
- **FR-003**: System MUST provide `AnsiColorNames` with the 17 standard ANSI color names and backward-compatible aliases
- **FR-004**: System MUST provide `NamedColors` dictionary with 140 HTML/CSS named colors mapped to hex values
- **FR-005**: System MUST provide `IStyle` interface with `GetAttrsForStyleStr`, `StyleRules`, and `InvalidationHash` members
- **FR-006**: System MUST provide `DummyStyle` that returns default attrs without applying any styling
- **FR-007**: System MUST provide `DynamicStyle` that delegates to a callable-provided style at runtime
- **FR-008**: System MUST provide `Style` class that creates styles from a list of (classnames, style definition) rules
- **FR-009**: System MUST provide `Style.FromDict` factory method with `Priority` enum support (DictKeyOrder, MostPrecise)
- **FR-010**: System MUST provide `StyleParser.ParseColor` for validating and normalizing color formats
- **FR-011**: System MUST provide `StyleMerger.MergeStyles` for combining multiple styles with correct precedence
- **FR-012**: System MUST provide `IStyleTransformation` interface with `TransformAttrs` method
- **FR-013**: System MUST provide `DummyStyleTransformation` that returns attrs unchanged
- **FR-014**: System MUST provide `ReverseStyleTransformation` that toggles the reverse attribute
- **FR-015**: System MUST provide `SwapLightAndDarkStyleTransformation` that inverts color luminosity
- **FR-016**: System MUST provide `SetDefaultColorStyleTransformation` for setting fallback foreground/background colors
- **FR-017**: System MUST provide `AdjustBrightnessStyleTransformation` for constraining color brightness to a range
- **FR-018**: System MUST provide `ConditionalStyleTransformation` that applies transformation based on a filter
- **FR-019**: System MUST provide `DynamicStyleTransformation` that delegates to a callable-provided transformation
- **FR-020**: System MUST provide `StyleTransformationMerger.MergeStyleTransformations` for combining transformations
- **FR-021**: System MUST provide `DefaultStyles` with pre-built UI and Pygments styles
- **FR-022**: System MUST support style string parsing with: colors (`#rrggbb`, `#rgb`, color names, `fg:color`, `bg:color`), attributes (`bold`, `nobold`, `italic`, etc.), classes (`class:name`), and `noinherit`
- **FR-023**: System MUST expand hierarchical class names (e.g., `a.b.c` becomes `a`, `a.b`, `a.b.c`)
- **FR-024**: System MUST apply rules in order with later rules overriding earlier ones
- **FR-025**: System MUST cache style computation results for performance
- **FR-026**: System MUST provide `PygmentsStyleUtils.StyleFromPygmentsClass` for creating a Style from a Pygments-style class type
- **FR-027**: System MUST provide `PygmentsStyleUtils.StyleFromPygmentsDict` for creating a Style from a token-to-style dictionary
- **FR-028**: System MUST provide `PygmentsStyleUtils.PygmentsTokenToClassName` for converting Pygments tokens to class names (e.g., `Token.Name.Exception` → `"pygments.name.exception"`)

### Key Entities

- **Attrs**: Value type representing 10 style attributes (foreground color, background color, and 8 boolean text effects). Supports null values for inheritance from parent styles.
- **Style**: Immutable style definition containing ordered rules that map class name combinations to attribute values. Rules are applied in order with later rules taking precedence.
- **StyleTransformation**: Post-processing operation applied to computed Attrs. Used for runtime adjustments like dark mode without redefining styles.
- **Color**: Either an ANSI color name (e.g., "ansiblue"), a 6-digit hex value (e.g., "ff0000"), or empty string for default. Named colors are normalized to hex values.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: All 140 HTML/CSS named colors are correctly mapped to their hex values
- **SC-002**: All 17 ANSI color names and 10 aliases are correctly recognized
- **SC-003**: Style string parsing handles all documented formats without errors
- **SC-004**: Style rule matching correctly applies rules for class combinations
- **SC-005**: Style merging preserves all rules and maintains correct precedence
- **SC-006**: All 8 style transformations correctly modify Attrs as specified
- **SC-007**: Default UI style contains rules for at least 50 standard UI classes
- **SC-008**: Default Pygments style contains rules for at least 30 token types
- **SC-009**: Unit test coverage reaches at least 80% for all style classes
- **SC-010**: Thread-safe implementations allow concurrent access without data corruption

## Assumptions

- Color values are stored as lowercase 6-digit hex strings (without '#' prefix) after parsing, except for ANSI color names which are preserved as-is.
- Style class names are case-insensitive and normalized to lowercase during processing.
- The `InvalidationHash` property is used by renderers to detect when styles have changed and need to be reapplied.
- Style transformations are applied in sequence when merged, with each transformation receiving the output of the previous one.
- The brightness adjustment transformation uses HLS color space for calculations.
- Empty color strings represent "default" colors that let the terminal decide the actual color.

## Dependencies

- **Stroke.Core.Cache** (Feature 06): For caching style computation results
- **Stroke.Filters** (Feature 17): For `FilterOrBool` type used in `ConditionalStyleTransformation`
