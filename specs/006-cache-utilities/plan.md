# Implementation Plan: Cache Utilities

**Branch**: `006-cache-utilities` | **Date**: 2026-01-23 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/006-cache-utilities/spec.md`

## Summary

Implement caching utilities for performance optimization throughout Stroke: `SimpleCache<TKey, TValue>` for custom key/value caching, `FastDictCache<TKey, TValue>` for high-frequency lookups with auto-populating dictionary semantics, and `Memoization` static class for function result caching. All implementations use FIFO eviction and port Python Prompt Toolkit's `cache.py` semantics faithfully to C#.

## Technical Context

**Language/Version**: C# 13 / .NET 10
**Primary Dependencies**: None (Stroke.Core layer - zero external dependencies per Constitution III)
**Storage**: N/A (in-memory only)
**Testing**: xUnit (no mocks, no FluentAssertions per Constitution VIII)
**Target Platform**: Linux, macOS, Windows 10+ (cross-platform)
**Project Type**: Single project (library)
**Performance Goals**: Dictionary lookup performance for cache hits; FastDictCache must perform comparably to standard Dictionary for cache hits
**Constraints**: Thread-safe implementation required per Constitution XI; uses `System.Threading.Lock` with `EnterScope()` pattern
**Scale/Scope**: Small feature - 3 classes, ~200-300 LOC implementation, ~300-400 LOC tests

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Faithful Port (100% API Fidelity) | ✅ PASS | Porting `cache.py` exactly; SimpleCache, FastDictCache, memoized decorator all mapped |
| II. Immutability by Default | ✅ PASS | Caches are mutable containers by nature; no core data structures affected |
| III. Layered Architecture | ✅ PASS | Cache utilities go in Stroke.Core (no external dependencies) |
| IV. Cross-Platform Terminal Compatibility | ✅ N/A | Not terminal-specific |
| V. Complete Editing Mode Parity | ✅ N/A | Not editing-mode specific |
| VI. Performance-Conscious Design | ✅ PASS | FastDictCache specifically designed for performance-critical caching |
| VII. Full Scope Commitment | ✅ PASS | All 3 user stories, 16 functional requirements will be implemented |
| VIII. Real-World Testing | ✅ PASS | xUnit with real implementations; no mocks |
| IX. Adherence to Planning Documents | ✅ PASS | api-mapping.md consulted; cache → Stroke.Core namespace |
| X. Source Code File Size Limits | ✅ PASS | Each class will be <200 LOC; well under 1,000 LOC limit |
| XI. Thread Safety by Default | ✅ PASS | Cache classes use `System.Threading.Lock` per Constitution XI; deviation from Python PTK per I.3 |

## Project Structure

### Documentation (this feature)

```text
specs/006-cache-utilities/
├── plan.md              # This file
├── spec.md              # Feature specification (already created)
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output (N/A - no external APIs)
├── tasks.md             # Phase 2 output (created by /speckit.tasks)
└── checklists/
    └── requirements.md  # Quality checklist (already created)
```

### Source Code (repository root)

```text
src/Stroke/
└── Core/
    ├── SimpleCache.cs      # SimpleCache<TKey, TValue> implementation
    ├── FastDictCache.cs    # FastDictCache<TKey, TValue> implementation
    └── Memoization.cs      # Memoization static class

tests/Stroke.Tests/
└── Core/
    ├── SimpleCacheTests.cs      # SimpleCache unit tests
    ├── FastDictCacheTests.cs    # FastDictCache unit tests
    └── MemoizationTests.cs      # Memoization unit tests
```

**Structure Decision**: Single project structure. All cache utilities are part of Stroke.Core layer (per api-mapping.md: `prompt_toolkit.cache` → `Stroke.Core`). Tests follow existing pattern in `tests/Stroke.Tests/Core/`.

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| Thread safety deviation from Python PTK | Constitution Principle XI requires thread safety; Principle I.3 explicitly permits this deviation | Single-threaded version would violate Constitution XI |

## Post-Design Constitution Re-Check

*Verified after Phase 1 design completion (2026-01-23)*

| Principle | Status | Verification |
|-----------|--------|--------------|
| I. Faithful Port | ✅ PASS | data-model.md matches Python PTK cache.py exactly |
| II. Immutability | ✅ PASS | Caches are containers; no immutable cores affected |
| III. Layered Architecture | ✅ PASS | All code in Stroke.Core, zero dependencies |
| VII. Full Scope | ✅ PASS | All 3 classes, all 16 requirements covered in data-model |
| VIII. Testing | ✅ PASS | Test files planned with real implementations |
| IX. Planning Docs | ✅ PASS | Namespace placement matches api-mapping.md |
| X. File Size | ✅ PASS | Each file <200 LOC estimated |
| XI. Thread Safety | ✅ PASS | All cache classes thread-safe per Constitution XI |

**Gate Status**: PASSED - Ready for `/speckit.tasks`
