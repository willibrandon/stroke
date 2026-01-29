# Feature Specification: Vi Digraphs

**Feature Branch**: `026-vi-digraphs`
**Created**: 2026-01-28
**Status**: Approved
**Input**: User description: "Implement Vi digraphs - a feature that allows inserting special characters by pressing Control+K followed by two characters. This is based on RFC1345 and matches Vim/Neovim behavior."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Insert Special Character via Digraph (Priority: P1)

A user editing text in Vi insert mode needs to insert a special character that is not easily accessible on their keyboard. They press Control+K to initiate digraph input, then type two characters that represent the desired special character. The system inserts the corresponding Unicode character into the document.

**Why this priority**: This is the core functionality of the feature. Without the ability to look up and return digraph characters, the entire feature has no value.

**Independent Test**: Can be fully tested by calling the digraph lookup with known character pairs and verifying the correct Unicode code point is returned, independent of any Vi mode integration.

**Acceptance Scenarios**:

1. **Given** the digraph dictionary is available, **When** a lookup is performed with characters ('E', 'u'), **Then** the Unicode code point 0x20AC (Euro sign) is returned
2. **Given** the digraph dictionary is available, **When** a lookup is performed with characters ('p', '*'), **Then** the Unicode code point 0x03C0 (Greek lowercase pi) is returned
3. **Given** the digraph dictionary is available, **When** a lookup is performed with characters ('<', '-'), **Then** the Unicode code point 0x2190 (Left arrow) is returned
4. **Given** the digraph dictionary is available, **When** a lookup is performed with characters ('h', 'h'), **Then** the Unicode code point 0x2500 (Box drawing horizontal line) is returned

---

### User Story 2 - Handle Invalid Digraph Gracefully (Priority: P2)

A user attempts to enter a digraph sequence that does not exist in the RFC1345 specification. The system gracefully handles this by returning no result, allowing the application layer to decide how to handle the invalid input (e.g., ignore, beep, or insert the literal characters).

**Why this priority**: Error handling is essential for a robust user experience, but the core lookup functionality must work first.

**Independent Test**: Can be tested by calling the digraph lookup with character pairs that are not in the dictionary and verifying null/no result is returned.

**Acceptance Scenarios**:

1. **Given** the digraph dictionary is available, **When** a lookup is performed with characters ('Z', 'Z'), **Then** null is returned indicating no such digraph exists
2. **Given** the digraph dictionary is available, **When** a lookup is performed with characters ('!', '@'), **Then** null is returned indicating no such digraph exists

---

### User Story 3 - Convert Code Point to Character String (Priority: P2)

A user needs to insert a Unicode character that may be in the Basic Multilingual Plane or in supplementary planes (code points above 0xFFFF). The system provides a method to convert the code point to a proper string, handling surrogate pairs correctly for characters outside the BMP.

**Why this priority**: Essential for actual character insertion, but depends on the core lookup working first.

**Independent Test**: Can be tested by requesting the string for known digraphs and verifying correct Unicode strings are produced, including characters that require surrogate pairs.

**Acceptance Scenarios**:

1. **Given** a digraph for Euro sign exists, **When** GetString is called with ('E', 'u'), **Then** the string "€" is returned
2. **Given** a digraph for Greek pi exists, **When** GetString is called with ('p', '*'), **Then** the string "π" is returned
3. **Given** a digraph lookup returns null, **When** GetString is called with invalid characters, **Then** null is returned

---

### User Story 4 - Access Full Digraph Dictionary (Priority: P3)

A developer or advanced user needs to enumerate all available digraphs, perhaps to display a help screen, search for specific characters, or build a custom picker UI. The system exposes the complete digraph mapping as a read-only dictionary.

**Why this priority**: Advanced capability for tooling and help systems, not required for basic digraph insertion.

**Independent Test**: Can be tested by accessing the Map property and verifying it contains the expected number of entries and specific known mappings.

**Acceptance Scenarios**:

1. **Given** the digraph dictionary is available, **When** the Map property is accessed, **Then** a read-only dictionary with all RFC1345 digraph mappings is returned
2. **Given** the digraph dictionary is available, **When** the Map is enumerated, **Then** entries like (('E', 'u'), 0x20AC) are present

---

### Edge Cases

- What happens when a digraph maps to a control character (code points 0x00-0x1F)? The system returns the code point; interpretation is left to the caller.
- What happens when both orderings of characters are tried (e.g., ('u', 'E') vs ('E', 'u'))? Only the canonical ordering from RFC1345 is supported; the reverse order returns null.
- How does the system handle case sensitivity? Digraphs are case-sensitive as defined in RFC1345/Vim (e.g., ('a', '*') and ('A', '*') map to different Greek letters).

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST provide a static dictionary mapping two-character tuples to Unicode code points
- **FR-002**: System MUST provide a `Lookup(char char1, char char2)` method that returns the corresponding Unicode code point as `int?`, or `null` if not found
- **FR-003**: System MUST provide a method to get a single character for digraphs that map to code points within the Basic Multilingual Plane (0x0000-0xFFFF)
- **FR-004**: System MUST provide a `GetString(char char1, char char2)` method that returns a `string?` for any valid digraph, correctly handling surrogate pairs for code points above 0xFFFF, or `null` if not found
- **FR-005**: System MUST expose the complete digraph dictionary via a `Map` property of type `IReadOnlyDictionary<(char Char1, char Char2), int>`
- **FR-006**: System MUST include all digraph mappings from the authoritative source (see Data Sources below)
- **FR-007**: Digraph lookups MUST be case-sensitive as per RFC1345 specification
- **FR-008**: System MUST be thread-safe for concurrent lookups

### Data Sources

**Authoritative Source**: Python Prompt Toolkit's `prompt_toolkit/key_binding/digraphs.py`

This file contains the `DIGRAPHS` dictionary with 1,300+ mappings. It is itself derived from:
1. RFC1345 (Character Mnemonics & Character Sets)
2. Neovim's `digraph.c` implementation

For this port, the Python Prompt Toolkit source is the single source of truth per Constitution Principle I (Faithful Port).

### Digraph Categories

The dictionary MUST include mappings from the following Unicode blocks to validate complete coverage:

| Category | Approximate Count | Code Point Range | Example |
|----------|-------------------|------------------|---------|
| Control characters | 32 | 0x00-0x1F | ('N','U') → 0x00 (NUL) |
| ASCII printable | ~30 | 0x20-0x7F | ('S','P') → 0x20 (Space) |
| Latin-1 Supplement | ~100 | 0x80-0xFF | ('c',',') → 0xE7 (ç) |
| Latin Extended-A | ~100 | 0x100-0x17F | ('A','-') → 0x100 (Ā) |
| Latin Extended-B | ~30 | 0x180-0x24F | ('O','9') → 0x1A0 (Ơ) |
| Greek and Coptic | ~80 | 0x370-0x3FF | ('p','*') → 0x3C0 (π) |
| Cyrillic | ~100 | 0x400-0x4FF | ('A','=') → 0x410 (А) |
| Hebrew | ~30 | 0x5D0-0x5EA | ('A','+') → 0x5D0 (א) |
| Arabic | ~60 | 0x600-0x6FF | ('a','+') → 0x627 (ا) |
| Currency symbols | ~10 | 0x20A0-0x20BF | ('E','u') → 0x20AC (€) |
| Mathematical operators | ~50 | 0x2200-0x22FF | ('R','T') → 0x221A (√) |
| Box drawing | ~60 | 0x2500-0x257F | ('h','h') → 0x2500 (─) |
| Hiragana | ~90 | 0x3040-0x309F | ('k','a') → 0x304B (か) |
| Katakana | ~90 | 0x30A0-0x30FF | ('K','a') → 0x30AB (カ) |
| Bopomofo | ~40 | 0x3100-0x312F | ('b','4') → 0x3105 (ㄅ) |
| Latin ligatures | 6 | 0xFB00-0xFB06 | ('f','i') → 0xFB01 (ﬁ) |

### Key Entities

- **Digraph**: A two-character sequence that maps to a single Unicode code point. The mapping is defined by RFC1345 and includes control characters, Latin supplements, Greek letters, Cyrillic letters, Hebrew, Arabic, mathematical symbols, box drawing characters, and more.
- **Code Point**: An integer representing a Unicode character. Values range from 0x00 to 0x10FFFF, though the digraph dictionary primarily covers 0x00 to 0xFB06.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: All 1,300+ digraph mappings from Python Prompt Toolkit are present and return correct code points
- **SC-002**: 100% of RFC1345 common digraphs (currency symbols, Greek letters, mathematical operators, box drawing) are correctly mapped
- **SC-003**: Lookup operations complete in constant time regardless of dictionary size
- **SC-004**: Unit tests achieve at least 80% code coverage
- **SC-005**: Invalid digraph lookups consistently return null without throwing exceptions

## Assumptions

- The digraph dictionary is static and read-only; runtime modification is not required
- Thread safety is achieved through immutable data structures (the dictionary is populated at static initialization and never modified)
- Vi mode integration (Ctrl+K handling, ViState properties) is out of scope for this feature and will be implemented in a separate key bindings feature
- The caller is responsible for collecting the two input characters after Ctrl+K; this feature only provides the lookup mechanism

## Dependencies

- None (this is a self-contained data structure and lookup utility within Stroke.KeyBinding namespace)

## Out of Scope

- Vi mode key binding for Ctrl+K (handled by key bindings system)
- ViState properties for tracking digraph input state (handled by editing modes)
- UI for displaying available digraphs
- User-defined custom digraphs
