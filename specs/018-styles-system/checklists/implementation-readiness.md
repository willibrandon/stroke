# Implementation Readiness Checklist: Styles System

**Purpose**: Validate that specification, contracts, and data model are complete, clear, and ready for implementation
**Created**: 2026-01-26
**Feature**: [spec.md](../spec.md) | [plan.md](../plan.md)

## Requirement Completeness

- [x] CHK001 - Are all 17 ANSI color names explicitly enumerated in requirements? [Completeness, Spec §FR-003]
  - **VERIFIED**: data-model.md §AnsiColorNames lists all 17: ansidefault, ansiblack, ansired, ansigreen, ansiyellow, ansiblue, ansimagenta, ansicyan, ansigray, ansibrightblack, ansibrightred, ansibrightgreen, ansibrightyellow, ansibrightblue, ansibrightmagenta, ansibrightcyan, ansiwhite.
- [x] CHK002 - Are all 10 ANSI color aliases documented with their canonical mappings? [Completeness, Spec §FR-003]
  - **VERIFIED**: data-model.md §AnsiColorNames lists all 10 aliases with mappings (ansidarkgray→ansibrightblack, ansiteal→ansicyan, etc.).
- [x] CHK003 - Is the full list of 140 HTML/CSS named colors specified or referenced? [Completeness, Spec §FR-004]
  - **VERIFIED**: contracts/core.md §NamedColors references "140 named HTML/CSS colors". Full list in Python source named_colors.py (149 entries including Gray/Grey variants).
- [x] CHK004 - Are all 10 style attributes (color, bgcolor, bold, etc.) defined in Attrs? [Completeness, Spec §FR-001]
  - **VERIFIED**: contracts/core.md §Attrs defines all 10: Color, BgColor, Bold, Underline, Strike, Italic, Blink, Reverse, Hidden, Dim. Matches Python base.py:22-32.
- [x] CHK005 - Are all 8 style transformation types specified with their behavior? [Completeness, Spec §FR-013 to FR-019]
  - **VERIFIED**: data-model.md §IStyleTransformation lists all 8: Dummy, Reverse, SwapLightAndDark, SetDefaultColor, AdjustBrightness, Conditional, Dynamic, _Merged. Contracts define each.
- [x] CHK006 - Are default UI style rules documented with their exact class names and style definitions? [Completeness, Spec §FR-021]
  - **VERIFIED**: data-model.md §DefaultStyles documents composition: 68 PROMPT_TOOLKIT_STYLE + 157 COLORS_STYLE + 19 WIDGETS_STYLE = 244 rules. Full list in Python defaults.py.
- [x] CHK007 - Are Pygments token rules documented for syntax highlighting? [Completeness, Spec §FR-021]
  - **VERIFIED**: data-model.md §DefaultStyles documents 34 Pygments rules. Full list in Python defaults.py PYGMENTS_DEFAULT_STYLE dict (lines 172-214).
- [x] CHK008 - Is the style string grammar fully documented (attributes, colors, classes, special)? [Completeness, Spec §FR-022]
  - **VERIFIED**: research.md §4 documents complete grammar: attribute tokens (bold/nobold/etc.), color formats (hex/ansi/named/prefixed), class references (class:name), special (noinherit), ignored (roman/sans/mono/border).

## Requirement Clarity

- [x] CHK009 - Is the color normalization behavior explicitly defined (lowercase, no # prefix)? [Clarity, Spec Assumptions]
  - **VERIFIED**: spec.md Assumptions (line 176): "Color values are stored as lowercase 6-digit hex strings (without '#' prefix) after parsing, except for ANSI color names which are preserved as-is."
- [x] CHK010 - Is class name case-sensitivity behavior specified (normalized to lowercase)? [Clarity, Spec Assumptions]
  - **VERIFIED**: spec.md Assumptions (line 177): "Style class names are case-insensitive and normalized to lowercase during processing."
- [x] CHK011 - Is the rule precedence algorithm clearly defined ("later rules override earlier")? [Clarity, Spec §FR-024]
  - **VERIFIED**: FR-024: "System MUST apply rules in order with later rules overriding earlier ones." Also documented in data-model.md Attrs Merging section.
- [x] CHK012 - Is the hierarchical class expansion algorithm specified (a.b.c → a, a.b, a.b.c)? [Clarity, Spec §FR-023]
  - **VERIFIED**: FR-023: "System MUST expand hierarchical class names (e.g., `a.b.c` becomes `a`, `a.b`, `a.b.c`)." Algorithm in research.md §5.
- [x] CHK013 - Is the meaning of `noinherit` in style strings precisely defined? [Clarity, Edge Cases]
  - **VERIFIED**: spec.md Edge Cases: "What happens when `noinherit` is used? Resets to default attrs before applying other styles." Also in data-model.md State Machine.
- [x] CHK014 - Is the Priority enum (DictKeyOrder vs MostPrecise) behavior clearly documented? [Clarity, Spec §FR-009]
  - **VERIFIED**: data-model.md Priority section documents both values. contracts/core.md has detailed XML docs. DictKeyOrder=iteration order, MostPrecise=more elements get priority.
- [x] CHK015 - Is the brightness interpolation formula for AdjustBrightnessStyleTransformation specified? [Clarity, research.md]
  - **VERIFIED**: Added to research.md §1a. Formula: `new_brightness = minBrightness + (maxBrightness - minBrightness) * original_brightness`. Linear interpolation maps lightness into [min, max] range.
- [x] CHK016 - Is the color luminosity inversion algorithm for SwapLightAndDarkStyleTransformation defined? [Clarity, research.md]
  - **VERIFIED**: Added to research.md §1b. Algorithm: ANSI colors use lookup table, hex colors invert lightness in HLS space (`l = 1 - l`).

## Requirement Consistency

- [x] CHK017 - Are Attrs field names consistent between spec (Color/BgColor) and Python source (color/bgcolor)? [Consistency]
  - **VERIFIED**: Python uses lowercase (color, bgcolor), C# contracts use PascalCase (Color, BgColor). Standard C# naming convention (snake_case → PascalCase) per Constitution I.
- [x] CHK018 - Is DefaultAttrs.Default consistent with Python's DEFAULT_ATTRS values? [Consistency, Spec §FR-002]
  - **VERIFIED**: Python DEFAULT_ATTRS (base.py:49-60): color="", bgcolor="", bold=False, etc. contracts/core.md DefaultAttrs.Default (lines 66-76) has identical values.
- [x] CHK019 - Are IStyle interface members consistent with Python BaseStyle abstract methods? [Consistency, Spec §FR-005]
  - **VERIFIED**: Python BaseStyle has get_attrs_for_style_str, style_rules, invalidation_hash. C# IStyle has GetAttrsForStyleStr, StyleRules, InvalidationHash. All members mapped with PascalCase.
- [x] CHK020 - Are IStyleTransformation interface members consistent with Python StyleTransformation? [Consistency, Spec §FR-012]
  - **VERIFIED**: Python StyleTransformation has transform_attrs, invalidation_hash. C# IStyleTransformation has TransformAttrs, InvalidationHash. All members mapped.
- [x] CHK021 - Is the InvalidationHash return type (`object`) consistent across all style types? [Consistency]
  - **VERIFIED**: Python returns `Hashable`. C# uses `object` which is equivalent (all C# objects support GetHashCode). Consistent across IStyle, IStyleTransformation, and all implementations.

## Acceptance Criteria Quality

- [x] CHK022 - Is SC-001 (140 named colors mapped correctly) measurable and testable? [Acceptance Criteria, Spec §SC-001]
  - **VERIFIED**: Testable by comparing NamedColors.Colors against Python named_colors.py. Python has 149 entries (includes Gray/Grey variants). spec.md says 140 (excluding variants). Test can verify exact mapping.
- [x] CHK023 - Is SC-002 (17 ANSI names + 10 aliases recognized) measurable? [Acceptance Criteria, Spec §SC-002]
  - **VERIFIED**: Python base.py has exactly 17 ANSI_COLOR_NAMES (lines 68-88) and 10 ANSI_COLOR_NAMES_ALIASES (lines 96-107). Testable by enumerating AnsiColorNames.Names and Aliases.
- [x] CHK024 - Is SC-007 (≥50 UI style rules) quantified with a verifiable count? [Acceptance Criteria, Spec §SC-007]
  - **VERIFIED**: data-model.md documents 68 PROMPT_TOOLKIT_STYLE + 157 COLORS_STYLE + 19 WIDGETS_STYLE = 244 total rules. Exceeds ≥50 threshold. Testable via DefaultUiStyle.StyleRules.Count.
- [x] CHK025 - Is SC-008 (≥30 Pygments token rules) quantified with a verifiable count? [Acceptance Criteria, Spec §SC-008]
  - **VERIFIED**: data-model.md documents 34 Pygments rules from defaults.py. Exceeds ≥30 threshold. Testable via DefaultPygmentsStyle.StyleRules.Count.
- [x] CHK026 - Is SC-009 (80% test coverage) measurable with specific tooling? [Acceptance Criteria, Spec §SC-009]
  - **VERIFIED**: Constitution VIII mandates 80% coverage. Measurable via `dotnet test --collect:"XPlat Code Coverage"` with Coverlet. Standard .NET coverage tooling.
- [x] CHK027 - Is SC-010 (thread-safe without corruption) testable with specific scenarios? [Acceptance Criteria, Spec §SC-010]
  - **VERIFIED**: Constitution XI mandates thread safety. Testable via concurrent access tests (multiple threads calling GetAttrsForStyleStr, TransformAttrs simultaneously). Similar to existing thread-safety tests in other features.

## Scenario Coverage

- [x] CHK028 - Are all 6 user stories mapped to functional requirements? [Coverage, Spec §User Scenarios]
  - **VERIFIED**: US1→FR-001/002/008/022-025, US2→FR-003/004/010, US3→FR-011, US4→FR-012-020, US5→FR-007/018/019, US6→FR-021. All 6 user stories have corresponding FRs.
- [x] CHK029 - Is the empty style string scenario addressed in requirements? [Coverage, Edge Cases]
  - **VERIFIED**: spec.md Edge Cases: "What happens when an empty style string is passed? Returns default attrs." Covered by FR-005 (GetAttrsForStyleStr returns Attrs).
- [x] CHK030 - Is the undefined class name fallback behavior specified? [Coverage, Edge Cases]
  - **VERIFIED**: spec.md Edge Cases: "What happens when a class name is not defined in the style? Falls back to default attrs for undefined portions."
- [x] CHK031 - Is the conflicting attributes scenario (bold nobold) behavior defined? [Coverage, Edge Cases]
  - **VERIFIED**: spec.md Edge Cases: "What happens when multiple style attributes conflict (e.g., `bold nobold`)? Later attributes override earlier ones." Consistent with FR-024.
- [x] CHK032 - Is the ANSI color brightness transformation behavior specified? [Coverage, Edge Cases]
  - **VERIFIED**: spec.md Edge Cases: "What happens when brightness transformation is applied to ANSI colors? ANSI colors are converted to RGB for transformation." research.md §2 documents AnsiColorsToRgb lookup.
- [x] CHK033 - Is the merged style invalidation hash change behavior specified? [Coverage, User Story 3]
  - **VERIFIED**: US3 Scenario 3: "Given a merged style, When one of the source styles changes, Then the merged style's invalidation hash changes." Python _MergedStyle.invalidation_hash (style.py:406-407) returns tuple of source hashes.

## Edge Case Coverage

- [x] CHK034 - Is the behavior for invalid color formats (e.g., "notacolor") defined? [Edge Case, User Story 2 Scenario 5]
  - **VERIFIED**: US2 Scenario 5: "an appropriate error is raised". contracts/styles.md StyleParser.ParseColor: "Thrown when the color format is invalid" → ArgumentException. Python parse_color raises ValueError (style.py:76).
- [x] CHK035 - Is the behavior for null styles passed to MergeStyles defined? [Edge Case, Spec Edge Cases]
  - **VERIFIED**: spec.md Edge Cases: "What happens when null styles are passed to merge? They are filtered out." contracts/styles.md StyleMerger.MergeStyles: "Null entries are filtered out." Python merge_styles (style.py:361) does same.
- [x] CHK036 - Is the behavior for 3-digit hex expansion (#f00 → ff0000) specified? [Edge Case, User Story 2 Scenario 3]
  - **VERIFIED**: US2 Scenario 3: "#ff0 (3-digit hex)...expanded to ffff00". data-model.md Color Validation: "#RGB (3 hex digits) → valid (expanded to RRGGBB)". Python parse_color (style.py:69-70): `col[0] * 2 + col[1] * 2 + col[2] * 2`.
- [x] CHK037 - Is the behavior when callable returns null for DynamicStyle specified? [Edge Case, contracts/styles.md]
  - **VERIFIED**: Python uses `get_style() or self._dummy` (base.py:179). Falls back to DummyStyle instance when callable returns null. Contracts already specify `Func<IStyle?>` signature.
- [x] CHK038 - Is the behavior when callable returns null for DynamicStyleTransformation specified? [Edge Case, contracts/transformations.md]
  - **VERIFIED**: Python uses `get_style_transformation() or DummyStyleTransformation()` (style_transformation.py:260). Falls back to DummyStyleTransformation when callable returns null. Contracts specify `Func<IStyleTransformation?>` signature.
- [x] CHK039 - Is the behavior for brightness values outside 0.0-1.0 range defined? [Edge Case, Gap]
  - **VERIFIED**: Added to research.md §1a. Values outside [0.0, 1.0] throw `ArgumentOutOfRangeException` in `TransformAttrs`. Matches Python's assertions.
- [x] CHK040 - Is the behavior for empty class name rules (global defaults) defined? [Edge Case, research.md §6]
  - **VERIFIED**: research.md §6 Algorithm step 2: "Apply rules matching empty class set (global defaults)". Python Style.get_attrs_for_style_str (style.py:283-285): applies rules with empty `names` set first.

## API Faithfulness (Python Port)

- [x] CHK041 - Is every public class from prompt_toolkit.styles.base mapped? [API Mapping, docs/api-mapping.md]
  - **VERIFIED**: Python __all__ (base.py:10-18): Attrs→Attrs, DEFAULT_ATTRS→DefaultAttrs.Default, ANSI_COLOR_NAMES→AnsiColorNames.Names, ANSI_COLOR_NAMES_ALIASES→AnsiColorNames.Aliases, BaseStyle→IStyle, DummyStyle→DummyStyle, DynamicStyle→DynamicStyle. All mapped.
- [x] CHK042 - Is every public class from prompt_toolkit.styles.style mapped? [API Mapping, docs/api-mapping.md]
  - **VERIFIED**: Python __all__ (style.py:23-28): Style→Style, parse_color→StyleParser.ParseColor, Priority→Priority, merge_styles→StyleMerger.MergeStyles. All mapped.
- [x] CHK043 - Is every public class from prompt_toolkit.styles.style_transformation mapped? [API Mapping, docs/api-mapping.md]
  - **VERIFIED**: Python __all__ (style_transformation.py:26-36): All 8 transformations + merge_style_transformations mapped. StyleTransformation→IStyleTransformation, SwapLightAndDark→SwapLightAndDarkStyleTransformation, etc.
- [x] CHK044 - Is every public function (parse_color, merge_styles, etc.) mapped? [API Mapping, docs/api-mapping.md]
  - **VERIFIED**: parse_color→StyleParser.ParseColor, merge_styles→StyleMerger.MergeStyles, merge_style_transformations→StyleTransformationMerger.MergeStyleTransformations. All mapped in research.md API Mapping table.
- [x] CHK045 - Are internal types (_MergedStyle, _MergedStyleTransformation) appropriately handled? [API Mapping]
  - **VERIFIED**: Python _MergedStyle (style.py:365) and _MergedStyleTransformation (style_transformation.py:291) are internal (underscore prefix). C# implementations will be internal classes. plan.md lists MergedStyleTransformation.cs as internal.
- [x] CHK046 - Is the OPPOSITE_ANSI_COLOR_NAMES mapping documented for SwapLightAndDark? [API Mapping, research.md]
  - **VERIFIED**: research.md §1b documents OppositeAnsiColorNames lookup. contracts/transformations.md defines OppositeAnsiColorNames internal class (lines 404-419). Python dict at style_transformation.py:316-334.
- [x] CHK047 - Is the CLASS_NAMES_RE regex pattern documented for validation? [API Mapping, research.md §4]
  - **VERIFIED**: Python CLASS_NAMES_RE (style.py:182): `r"^[a-z0-9.\s_-]*$"`. data-model.md Class Name Validation documents pattern: `[a-z0-9.\s_-]*`. Used for style rule class name validation.

## Dependencies & Assumptions

- [x] CHK048 - Is the Stroke.Core.SimpleCache dependency (Feature 06) verified as available? [Dependency, Spec §Dependencies]
  - **VERIFIED**: `src/Stroke/Core/SimpleCache.cs` exists.
- [x] CHK049 - Is the Stroke.Filters.FilterOrBool dependency (Feature 17) verified as available? [Dependency, Spec §Dependencies]
  - **VERIFIED**: `src/Stroke/Filters/FilterOrBool.cs` exists.
- [x] CHK050 - Is the FilterUtils.ToFilter() utility documented for ConditionalStyleTransformation? [Dependency, research.md §7]
  - **VERIFIED**: `src/Stroke/Filters/FilterUtils.cs` exists. Research.md §7 documents the integration pattern.
- [x] CHK051 - Is the assumption about color storage format (lowercase hex, no #) documented? [Assumption, Spec §Assumptions]
  - **VERIFIED**: spec.md Assumptions (line 176): "Color values are stored as lowercase 6-digit hex strings (without '#' prefix) after parsing, except for ANSI color names which are preserved as-is."
- [x] CHK052 - Is the assumption about HLS color space for brightness calculations documented? [Assumption, Spec §Assumptions]
  - **VERIFIED**: spec.md Assumptions (line 180): "The brightness adjustment transformation uses HLS color space for calculations." research.md §1 provides full HLS algorithm.

## Thread Safety Requirements

- [x] CHK053 - Are thread safety guarantees documented for all mutable types? [Constitution XI, plan.md]
  - **VERIFIED**: research.md §9 documents: Immutable types (inherently safe): Attrs, Style, DummyStyle, Priority, AnsiColorNames, NamedColors. Stateless types (inherently safe): DummyStyleTransformation, ReverseStyleTransformation. No mutable types identified - all styles immutable after construction.
- [x] CHK054 - Is the immutability of Attrs, Style, DummyStyle explicitly stated? [Thread Safety, contracts/]
  - **VERIFIED**: contracts/core.md Attrs: "readonly record struct" (immutable). contracts/styles.md DummyStyle: "This type is thread-safe. It is stateless and immutable." contracts/styles.md Style: "This type is thread-safe. The style is immutable after construction."
- [x] CHK055 - Are callable thread safety responsibilities documented for Dynamic* types? [Thread Safety, contracts/transformations.md]
  - **VERIFIED**: contracts/styles.md DynamicStyle: "The underlying callable may be invoked from multiple threads; thread safety of the callable is the caller's responsibility." Same pattern in DynamicStyleTransformation, SetDefaultColor, AdjustBrightness, Conditional contracts.
- [x] CHK056 - Is cache thread safety addressed for Style computation caching? [Thread Safety, research.md §3]
  - **VERIFIED**: research.md §3 documents using SimpleCache from Stroke.Core (Feature 06) with thread-safe LRU semantics. contracts/styles.md Style: "cached computations use thread-safe caching."

## Ambiguities & Conflicts

- [x] CHK057 - Is the Priority enum naming (DictKeyOrder vs DICT_KEY_ORDER) deviation from Python documented? [Ambiguity]
  - **VERIFIED**: Standard C# naming convention (snake_case → PascalCase) per Constitution I. No additional documentation needed.
- [x] CHK058 - Is the api-mapping.md Priority enum (5 values) vs spec Priority enum (2 values) discrepancy resolved? [Conflict, docs/api-mapping.md vs Spec §FR-009]
  - **RESOLVED**: api-mapping.md was incorrect (5 values don't exist in Python). Fixed to match Python's actual 2 values: `Priority.DICT_KEY_ORDER` → `Priority.DictKeyOrder`, `Priority.MOST_PRECISE` → `Priority.MostPrecise`.
- [x] CHK059 - Is the OneOf<string, Func<string>> pattern for SetDefaultColorStyleTransformation justified? [Clarification Needed]
  - **RESOLVED**: Contracts updated to use constructor overloading instead of OneOf. Two constructors: `(string fg, string bg)` for static values, `(Func<string> fg, Func<string> bg)` for dynamic. See research.md §11 and contracts/transformations.md.
- [x] CHK060 - Is the OneOf<float, Func<float>> pattern for AdjustBrightnessStyleTransformation justified? [Clarification Needed]
  - **RESOLVED**: Contracts updated to use constructor overloading instead of OneOf. Two constructors: `(float minBrightness, float maxBrightness)` for static values, `(Func<float> minBrightness, Func<float> maxBrightness)` for dynamic. See research.md §11 and contracts/transformations.md.

## Notes

- This checklist validates requirements quality, NOT implementation correctness
- Focus: API faithfulness for Python port + implementation readiness
- All items should be verified against spec.md, plan.md, research.md, and contracts/
- Items marked [Gap] indicate potentially missing requirements to be added to spec
