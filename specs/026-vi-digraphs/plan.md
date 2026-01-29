# Implementation Plan: Vi Digraphs

**Branch**: `026-vi-digraphs` | **Date**: 2026-01-28 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/026-vi-digraphs/spec.md`

## Summary

Implement a static `Digraphs` class in `Stroke.KeyBinding` namespace that provides a complete RFC1345-based digraph dictionary (1,300+ mappings) ported from Python Prompt Toolkit's `digraphs.py`. The class exposes lookup methods to map two-character sequences to Unicode code points and string representations, enabling Vi digraph insertion functionality.

## Technical Context

**Language/Version**: C# 13 / .NET 10
**Primary Dependencies**: None (Stroke.KeyBinding namespace has no external dependencies for this feature)
**Storage**: N/A (static immutable dictionary populated at static initialization)
**Testing**: xUnit with standard assertions (no mocks per Constitution VIII)
**Target Platform**: Cross-platform (.NET 10+)
**Project Type**: Single project library
**Performance Goals**: O(1) constant-time dictionary lookup
**Constraints**: Thread-safe via immutability; all 1,300+ mappings from Python source
**Scale/Scope**: Static class with 1,300+ dictionary entries, ~4 public methods

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Faithful Port | ✅ PASS | Direct port of `prompt_toolkit.key_binding.digraphs.DIGRAPHS` dictionary |
| II. Immutability by Default | ✅ PASS | Static `FrozenDictionary` populated once at initialization; never modified |
| III. Layered Architecture | ✅ PASS | `Stroke.KeyBinding` depends only on Core and Input per architecture |
| IV. Cross-Platform Compatibility | ✅ PASS | Pure data structure; no platform-specific code |
| V. Editing Mode Parity | ✅ PASS | Provides data layer for Vi digraph insertion |
| VI. Performance-Conscious Design | ✅ PASS | O(1) dictionary lookup; static initialization |
| VII. Full Scope Commitment | ✅ PASS | All 1,300+ digraphs from Python source will be ported |
| VIII. Real-World Testing | ✅ PASS | Tests exercise real static dictionary lookups |
| IX. Adherence to Planning Documents | ✅ PASS | Follows `api-mapping.md` namespace mapping: `prompt_toolkit.key_binding` → `Stroke.KeyBinding` |
| X. File Size Limits | ✅ PASS | Single file with dictionary data; may approach limit but is data, not logic |
| XI. Thread Safety | ✅ PASS | Immutable static class; inherently thread-safe |

## Project Structure

### Documentation (this feature)

```text
specs/026-vi-digraphs/
├── spec.md              # Feature specification
├── plan.md              # This file
├── research.md          # Phase 0 research output
├── data-model.md        # Phase 1 data model
├── quickstart.md        # Phase 1 quickstart guide
├── contracts/           # Phase 1 API contracts
│   └── Digraphs.md      # Digraphs static class contract
└── checklists/
    └── requirements.md  # Requirements checklist (complete)
```

### Source Code (repository root)

```text
src/Stroke/KeyBinding/
├── Digraphs.cs          # Digraphs static class with dictionary and lookup methods

tests/Stroke.Tests/KeyBinding/
├── DigraphsTests.cs     # Unit tests for Digraphs class
```

**Structure Decision**: Single source file in existing `Stroke.KeyBinding` namespace. The digraph dictionary is data-heavy but logically cohesive as a single unit. If the file exceeds 1,000 LOC, the dictionary data could be extracted to a partial class, but given it's static data rather than complex logic, a single file is acceptable per Constitution X exceptions for generated/data files.

## Complexity Tracking

> No complexity violations detected. The implementation is straightforward:
> - Single static class with immutable dictionary
> - No external dependencies
> - No architectural complexity
> - No patterns beyond basic dictionary lookup

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| N/A | N/A | N/A |

## Post-Design Constitution Re-check

*Re-evaluated after Phase 1 design artifacts complete.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Faithful Port | ✅ PASS | API matches Python: `DIGRAPHS` → `Digraphs.Map`, lookup via tuple key |
| II. Immutability by Default | ✅ PASS | `FrozenDictionary<(char,char),int>` is immutable |
| III. Layered Architecture | ✅ PASS | No new dependencies added |
| IV. Cross-Platform Compatibility | ✅ PASS | No platform-specific code |
| V. Editing Mode Parity | ✅ PASS | Integrates with existing `ViState.WaitingForDigraph` |
| VI. Performance-Conscious Design | ✅ PASS | O(1) lookup confirmed in research |
| VII. Full Scope Commitment | ✅ PASS | All 1,300+ entries to be ported |
| VIII. Real-World Testing | ✅ PASS | Contract tests real dictionary |
| IX. Adherence to Planning Documents | ✅ PASS | Matches `api-mapping.md` and `test-mapping.md` |
| X. File Size Limits | ✅ PASS | Data file exception documented |
| XI. Thread Safety | ✅ PASS | Immutable = inherently thread-safe |

**Result**: All principles satisfied. Ready for task generation with `/speckit.tasks`.

## Generated Artifacts

| Artifact | Path | Status |
|----------|------|--------|
| Research | [research.md](research.md) | ✅ Complete |
| Data Model | [data-model.md](data-model.md) | ✅ Complete |
| API Contract | [contracts/Digraphs.md](contracts/Digraphs.md) | ✅ Complete |
| Quickstart | [quickstart.md](quickstart.md) | ✅ Complete |
