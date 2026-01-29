# Pre-Implementation Checklist: Vi Digraphs

**Purpose**: Author self-review to validate requirements completeness, clarity, and coverage before implementation
**Created**: 2026-01-28
**Feature**: [spec.md](../spec.md)
**Focus**: Comprehensive (Data Completeness, API Contract, Unicode/Edge Cases)
**Depth**: Lightweight (~15 items)

## Data Completeness

- [x] CHK001 - Is the authoritative source for digraph mappings explicitly identified? [Completeness, Spec §FR-001, FR-006] ✅ Added "Data Sources" section identifying Python Prompt Toolkit's digraphs.py as authoritative source
- [x] CHK002 - Is the expected entry count (1,300+) specified as a measurable target? [Measurability, Spec §SC-001] ✅ Already present
- [x] CHK003 - Are specific digraph categories enumerated (Greek, Cyrillic, box drawing, etc.) to validate coverage? [Completeness, Spec §SC-002] ✅ Added "Digraph Categories" table with 16 categories and examples

## API Contract Clarity

- [x] CHK004 - Is the return type for invalid lookups clearly specified (null vs exception)? [Clarity, Spec §FR-002, SC-005] ✅ Already present
- [x] CHK005 - Is the distinction between `Lookup` (returns code point) and `GetString` (returns string) clearly defined? [Clarity, Spec §FR-002, FR-004] ✅ Updated FR-002/FR-004 with explicit method names and return types
- [x] CHK006 - Is the `Map` property's exposed type explicitly specified (IReadOnlyDictionary vs concrete type)? [Clarity, Spec §FR-005] ✅ Updated FR-005 with exact type: `IReadOnlyDictionary<(char Char1, char Char2), int>`
- [x] CHK007 - Are the parameter names and types for the lookup methods documented? [Completeness, Gap] ✅ Updated FR-002/FR-004 with `(char char1, char char2)` parameters

## Unicode/Edge Case Coverage

- [x] CHK008 - Are requirements for control character handling (0x00-0x1F) explicitly stated? [Coverage, Spec Edge Cases] ✅ Already present
- [x] CHK009 - Are surrogate pair requirements for supplementary plane characters (>0xFFFF) specified? [Coverage, Spec §FR-004] ✅ Already present
- [x] CHK010 - Is case sensitivity behavior documented with examples? [Clarity, Spec §FR-007, Edge Cases] ✅ Already present
- [x] CHK011 - Is the behavior for reversed character order (e.g., 'uE' vs 'Eu') specified? [Coverage, Spec Edge Cases] ✅ Already present

## Thread Safety & Performance

- [x] CHK012 - Is the thread safety mechanism (immutability) explicitly documented? [Clarity, Spec §FR-008, Assumptions] ✅ Already present
- [x] CHK013 - Is the performance requirement (O(1) constant-time lookup) specified? [Measurability, Spec §SC-003] ✅ Already present

## Scope Boundaries

- [x] CHK014 - Are out-of-scope items (Ctrl+K binding, ViState integration, UI) explicitly listed? [Completeness, Spec Out of Scope] ✅ Already present
- [x] CHK015 - Is the assumption that callers handle input collection documented? [Completeness, Spec Assumptions] ✅ Already present

## Notes

- Lightweight checklist for author self-review before implementation
- All items focus on requirements quality, not implementation verification
- Total: 15 items across 5 categories

## Review Results

**Reviewed**: 2026-01-28
**Status**: ✅ All 15 items pass

### Changes Made
5 items required spec updates:
1. **CHK001**: Added "Data Sources" section with authoritative source
2. **CHK003**: Added "Digraph Categories" table with 16 Unicode block categories
3. **CHK005**: Added explicit method names to FR-002 and FR-004
4. **CHK006**: Added exact `IReadOnlyDictionary<(char, char), int>` type to FR-005
5. **CHK007**: Added parameter signatures `(char char1, char char2)` to FRs
