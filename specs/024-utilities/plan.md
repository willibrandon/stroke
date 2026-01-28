# Implementation Plan: Utilities

**Branch**: `024-utilities` | **Date**: 2026-01-28 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/024-utilities/spec.md`

## Summary

Implement utility functions and classes for Stroke: Event<TSender> for pub/sub, Unicode width calculation with caching, platform detection utilities, weight-based item distribution, and lazy value conversion helpers. All utilities port Python Prompt Toolkit's `utils.py` module with 100% API fidelity.

## Technical Context

**Language/Version**: C# 13 / .NET 10
**Primary Dependencies**: Wcwidth NuGet package (v4.0.1, MIT) for character width calculation
**Storage**: N/A (in-memory caches only)
**Testing**: xUnit (no mocks, no FluentAssertions per Constitution VIII)
**Target Platform**: Linux, macOS, Windows 10+
**Project Type**: Single project (Stroke library)
**Performance Goals**: Unicode width lookup < 50ns (cached), Event fire O(n) handlers
**Constraints**: Thread safety required per Constitution XI
**Scale/Scope**: 6 utility classes, 33 functional requirements, target 80% coverage

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Faithful Port | ✅ PASS | All APIs from `utils.py` mapped in api-mapping.md |
| II. Immutability by Default | ✅ PASS | Event handlers stored in list (mutable by design); caches use thread-safe patterns |
| III. Layered Architecture | ✅ PASS | Utilities go in Stroke.Core (layer 1, zero external deps except Wcwidth) |
| IV. Cross-Platform | ✅ PASS | Platform detection via RuntimeInformation; Unix/Windows specific logic |
| V. Editing Mode Parity | N/A | Not applicable to utilities |
| VI. Performance-Conscious | ✅ PASS | String width caching with LRU eviction (64 char threshold, 16 long strings max) |
| VII. Full Scope | ✅ PASS | All 33 FRs from spec will be implemented |
| VIII. Real-World Testing | ✅ PASS | xUnit only, no mocks |
| IX. Planning Documents | ✅ PASS | api-mapping.md section for utils.py consulted |
| X. File Size Limits | ✅ PASS | Plan for 6 separate files well under 1000 LOC each |
| XI. Thread Safety | ✅ PASS | UnicodeWidth cache is thread-safe; Event follows standard .NET event semantics (not thread-safe, matching developer expectations for += / -= syntax) |

## Project Structure

### Documentation (this feature)

```text
specs/024-utilities/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
└── tasks.md             # Phase 2 output (/speckit.tasks command)
```

### Source Code (repository root)

```text
src/Stroke/
├── Core/
│   ├── Event.cs                    # Event<TSender> class
│   ├── DummyContext.cs             # DummyContext no-op disposable
│   ├── UnicodeWidth.cs             # Unicode width calculation + caching
│   ├── PlatformUtils.cs            # Platform detection utilities
│   ├── ConversionUtils.cs          # ToStr/ToInt/ToFloat helpers
│   └── CollectionUtils.cs          # TakeUsingWeights generator

tests/Stroke.Tests/
├── Core/
│   ├── EventTests.cs               # Event class tests
│   ├── DummyContextTests.cs        # DummyContext tests
│   ├── UnicodeWidthTests.cs        # Unicode width tests
│   ├── PlatformUtilsTests.cs       # Platform detection tests
│   ├── ConversionUtilsTests.cs     # Value conversion tests
│   └── CollectionUtilsTests.cs     # Weight distribution tests
```

**Structure Decision**: All utilities go in `Stroke.Core` namespace per api-mapping.md and Constitution III (Core layer has zero external dependencies except Wcwidth which is MIT-licensed).

## Complexity Tracking

> No violations. Event follows standard .NET event semantics (not thread-safe) which is the expected behavior for .NET developers using += / -= syntax.

---

## Post-Design Constitution Re-Check

*Re-evaluated after Phase 1 design completion.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Faithful Port | ✅ PASS | Contracts match api-mapping.md exactly; all Python APIs accounted for |
| II. Immutability | ✅ PASS | AnyFloat is readonly struct; DummyContext is singleton; caches internal |
| III. Layered Architecture | ✅ PASS | All utilities in Stroke.Core; only dependency is Wcwidth (MIT) |
| IV. Cross-Platform | ✅ PASS | PlatformUtils covers Windows/macOS/Linux detection |
| V. Editing Mode Parity | N/A | Not applicable |
| VI. Performance | ✅ PASS | Caching strategy documented in contracts |
| VII. Full Scope | ✅ PASS | All 33 FRs mapped to contract methods |
| VIII. Testing | ✅ PASS | No mocks in test strategy |
| IX. Planning Documents | ✅ PASS | api-mapping.md consulted for all contracts |
| X. File Size | ✅ PASS | 6 files planned, each < 200 LOC estimated |
| XI. Thread Safety | ✅ PASS | UnicodeWidth cache thread-safe; Event follows .NET event conventions (not thread-safe) |

**Gate Status**: ✅ PASSED - Ready for Phase 2 (/speckit.tasks)

---

## Generated Artifacts

| Artifact | Path | Status |
|----------|------|--------|
| Implementation Plan | `specs/024-utilities/plan.md` | ✅ Complete |
| Research | `specs/024-utilities/research.md` | ✅ Complete |
| Data Model | `specs/024-utilities/data-model.md` | ✅ Complete |
| Quickstart | `specs/024-utilities/quickstart.md` | ✅ Complete |
| Event Contract | `specs/024-utilities/contracts/Event.md` | ✅ Complete |
| UnicodeWidth Contract | `specs/024-utilities/contracts/UnicodeWidth.md` | ✅ Complete |
| PlatformUtils Contract | `specs/024-utilities/contracts/PlatformUtils.md` | ✅ Complete |
| ConversionUtils Contract | `specs/024-utilities/contracts/ConversionUtils.md` | ✅ Complete |
| CollectionUtils Contract | `specs/024-utilities/contracts/CollectionUtils.md` | ✅ Complete |
| DummyContext Contract | `specs/024-utilities/contracts/DummyContext.md` | ✅ Complete |
