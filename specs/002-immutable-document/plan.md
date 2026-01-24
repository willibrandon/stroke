# Implementation Plan: Immutable Document Text Model

**Branch**: `002-immutable-document` | **Date**: 2026-01-23 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/002-immutable-document/spec.md`

## Summary

Implement the immutable `Document` class that represents text content with cursor position and optional selection state. This is the core text representation used throughout Stroke, providing 50+ methods for text queries, navigation, search, selection handling, and clipboard operations. The implementation uses flyweight caching to share line data between Document instances with identical text content.

## Technical Context

**Language/Version**: C# 13 / .NET 10
**Primary Dependencies**: None (Core layer has zero external dependencies per Constitution III)
**Storage**: N/A (in-memory immutable data structure)
**Testing**: xUnit (no mocks per Constitution VIII)
**Target Platform**: Cross-platform (.NET 10 - Linux, macOS, Windows 10+)
**Project Type**: Single library project (existing `src/Stroke/`)
**Performance Goals**: Lazy line array computation; flyweight caching for identical text
**Constraints**: Document MUST be immutable; all mutation operations return new instances
**Scale/Scope**: 50+ public APIs matching Python Prompt Toolkit exactly

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Faithful Port (100% API Fidelity) | ✅ PASS | Document API fully mapped in `docs/api-mapping.md` lines 591-725 |
| II. Immutability by Default | ✅ PASS | Document is sealed, immutable with flyweight caching |
| III. Layered Architecture | ✅ PASS | Document belongs in Stroke.Core (layer 1), no external dependencies |
| IV. Cross-Platform Terminal Compatibility | ✅ PASS | Document is platform-agnostic text model |
| V. Complete Editing Mode Parity | ✅ PASS | Document provides word/WORD navigation for Vi/Emacs modes |
| VI. Performance-Conscious Design | ✅ PASS | Flyweight caching, lazy evaluation, no global state |
| VII. Full Scope Commitment | ✅ PASS | All 28 functional requirements will be implemented |
| VIII. Real-World Testing | ✅ PASS | Tests use xUnit, no mocks, target 80% coverage |
| IX. Adherence to Planning Documents | ✅ PASS | Following api-mapping.md and test-mapping.md exactly |

**Gate Status**: PASSED - Proceed to Phase 0

## Project Structure

### Documentation (this feature)

```text
specs/002-immutable-document/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output (N/A - no external API)
└── tasks.md             # Phase 2 output (/speckit.tasks command)
```

### Source Code (repository root)

```text
src/Stroke/
├── Core/
│   ├── Primitives/
│   │   ├── Point.cs           # (existing)
│   │   └── Size.cs            # (existing)
│   ├── Document.cs            # NEW: Main Document class
│   ├── DocumentCache.cs       # NEW: Flyweight cache for line arrays
│   ├── SelectionState.cs      # NEW: Selection tracking (dependency)
│   ├── SelectionType.cs       # NEW: CHARACTERS, LINES, BLOCK enum
│   └── ClipboardData.cs       # NEW: Clipboard content (dependency)
├── Stroke.csproj
└── ...

tests/Stroke.Tests/
├── Core/
│   ├── DocumentTests.cs       # NEW: 12+ tests per test-mapping.md
│   ├── SelectionStateTests.cs # NEW: Selection unit tests
│   └── ClipboardDataTests.cs  # NEW: Clipboard unit tests
└── Stroke.Tests.csproj
```

**Structure Decision**: Single library project with namespace-based organization. Document and related types go in `Stroke.Core` namespace under `src/Stroke/Core/` directory.

## Complexity Tracking

> No violations - structure follows existing project patterns.

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| N/A | N/A | N/A |

---

## Phase 0: Research

### Research Tasks Completed

No NEEDS CLARIFICATION items in Technical Context. The Python source (`/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/document.py`) and `docs/api-mapping.md` provide definitive implementation guidance.

**See**: [research.md](research.md) for detailed findings.

---

## Phase 1: Design & Contracts

### 1.1 Data Model

**See**: [data-model.md](data-model.md) for complete entity definitions.

Key entities from spec:
- **Document**: Immutable text container with cursor and selection
- **DocumentCache**: ConditionalWeakTable-based flyweight for line arrays
- **SelectionState**: Tracks selection origin and type
- **SelectionType**: Enum (Characters, Lines, Block)
- **ClipboardData**: Cut/copy payload with selection type

### 1.2 API Contracts

No external API contracts needed - Document is an internal data structure with public methods defined by Python Prompt Toolkit compatibility. The API is documented in `docs/api-mapping.md` lines 591-725.

### 1.3 Quickstart

**See**: [quickstart.md](quickstart.md) for developer onboarding guide.

### 1.4 Post-Design Constitution Check

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Faithful Port (100% API Fidelity) | ✅ PASS | All 20 properties and 30+ methods match api-mapping.md |
| II. Immutability by Default | ✅ PASS | Document sealed, private fields, ImmutableArray for Lines |
| III. Layered Architecture | ✅ PASS | Only Stroke.Core types, no higher-layer dependencies |
| IV. Cross-Platform Terminal Compatibility | ✅ PASS | Pure C# implementation, no platform-specific code |
| V. Complete Editing Mode Parity | ✅ PASS | word/WORD patterns match Python exactly |
| VI. Performance-Conscious Design | ✅ PASS | ConditionalWeakTable flyweight, lazy line parsing |
| VII. Full Scope Commitment | ✅ PASS | All 28 FRs covered in data model |
| VIII. Real-World Testing | ✅ PASS | 12+ test methods defined, xUnit only |
| IX. Adherence to Planning Documents | ✅ PASS | Data model follows api-mapping.md and test-mapping.md |

**Post-Design Gate Status**: PASSED - Ready for `/speckit.tasks`

---

## Phase 2: Implementation Tasks

*Generated by `/speckit.tasks` command - NOT created by `/speckit.plan`*

---

## References

- Python source: `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/document.py`
- API mapping: `docs/api-mapping.md` (lines 591-725)
- Test mapping: `docs/test-mapping.md` (DocumentTests section)
- Constitution: `.specify/memory/constitution.md`
