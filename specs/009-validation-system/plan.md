# Implementation Plan: Validation System

**Branch**: `009-validation-system` | **Date**: 2026-01-24 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/009-validation-system/spec.md`

## Summary

Implement the input validation system for Stroke, providing a framework to validate buffer content before acceptance. This is a faithful port of Python Prompt Toolkit's `validation.py` module to C#. The implementation includes:

- `ValidationError` exception with cursor position and message
- `IValidator` interface with `Validate` and `ValidateAsync` methods
- `ValidatorBase` abstract class with `FromCallable` factory method
- Four decorator validators: `DummyValidator`, `ThreadedValidator`, `ConditionalValidator`, `DynamicValidator`

**Key Approach**: Extend existing stub implementations (`ValidationError`, `IValidator`) with full functionality and add all validator implementations in the `Stroke.Validation` namespace.

## Technical Context

**Language/Version**: C# 13 / .NET 10
**Primary Dependencies**: Stroke.Core (Document class)
**Storage**: N/A (stateless validation)
**Testing**: xUnit (no mocks, no FluentAssertions per Constitution VIII)
**Target Platform**: Linux, macOS, Windows 10+
**Project Type**: Single library project (Stroke)
**Performance Goals**: Synchronous validation must not block UI; async validation must support background execution
**Constraints**: Thread-safe implementations per Constitution XI; no external dependencies beyond Stroke.Core
**Scale/Scope**: ~7 types (1 exception, 1 interface, 5 classes)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Faithful Port (100% API Fidelity) | ✅ PASS | All 6 Python validation types mapped to C# equivalents in `docs/api-mapping.md` |
| II. Immutability by Default | ✅ PASS | Validators are stateless; ValidationError is immutable after construction |
| III. Layered Architecture | ✅ PASS | `Stroke.Validation` depends only on `Stroke.Core` (Document) |
| IV. Cross-Platform Compatibility | ✅ PASS | Pure .NET code, no platform-specific dependencies |
| V. Complete Editing Mode Parity | N/A | Not applicable to validation system |
| VI. Performance-Conscious Design | ✅ PASS | ThreadedValidator runs in background thread; no blocking operations |
| VII. Full Scope Commitment | ✅ PASS | All 22 functional requirements will be implemented |
| VIII. Real-World Testing | ✅ PASS | Tests will use xUnit with real implementations only |
| IX. Adherence to Planning Documents | ✅ PASS | Implementation follows `docs/api-mapping.md` exactly |
| X. Source Code File Size Limits | ✅ PASS | Estimated ~400 LOC total across 7 files (well under 1,000 per file) |
| XI. Thread Safety by Default | ✅ PASS | All validators are stateless or use thread-safe patterns |

**Gate Status**: ✅ PASS - All applicable principles satisfied

## Project Structure

### Documentation (this feature)

```text
specs/009-validation-system/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
└── contracts/           # Phase 1 output (empty - no external APIs)
```

### Source Code (repository root)

```text
src/Stroke/
├── Core/
│   └── Document.cs           # Existing - dependency for IValidator
└── Validation/
    ├── ValidationError.cs    # Existing stub - will be updated
    ├── IValidator.cs         # Existing stub - will be updated
    ├── ValidatorBase.cs      # New - abstract base class
    ├── DummyValidator.cs     # New - null-object validator
    ├── ThreadedValidator.cs  # New - background thread wrapper
    ├── ConditionalValidator.cs  # New - conditional wrapper
    └── DynamicValidator.cs   # New - dynamic wrapper

tests/Stroke.Tests/
└── Validation/
    ├── ValidationErrorTests.cs
    ├── ValidatorFromCallableTests.cs
    ├── DummyValidatorTests.cs
    ├── ThreadedValidatorTests.cs
    ├── ConditionalValidatorTests.cs
    └── DynamicValidatorTests.cs
```

**Structure Decision**: Validation types reside in `src/Stroke/Validation/` namespace, following the existing pattern for Clipboard, AutoSuggest, History, and other subsystems. Tests follow the parallel structure in `tests/Stroke.Tests/Validation/`.

## Complexity Tracking

No constitution violations to track. All implementations follow the established patterns.

---

## Phase 0: Research Findings

See [research.md](./research.md) for detailed analysis.

**Summary**:
- Python architecture ported faithfully with C# adaptations
- `ValueTask` chosen for async hot paths
- `Task.Run()` for ThreadedValidator background execution
- `Func<bool>` for filter (Stroke.Filters not yet implemented)
- Default parameters added to ValidationError
- Existing stubs extended rather than replaced

## Phase 1: Design Artifacts

See:
- [data-model.md](./data-model.md) - Entity definitions
- [quickstart.md](./quickstart.md) - Usage examples
- [contracts/](./contracts/) - API contracts (empty - no external APIs)

**Summary**:
- 7 types defined: ValidationError, IValidator, ValidatorBase, DummyValidator, ThreadedValidator, ConditionalValidator, DynamicValidator
- ~270 LOC estimated across all files
- All validators are stateless or immutable (thread-safe)
- Internal `FromCallableValidator` class for factory-created validators

## Post-Design Constitution Re-Check

| Principle | Status | Post-Design Notes |
|-----------|--------|-------------------|
| I. Faithful Port | ✅ PASS | All Python APIs mapped in data-model.md |
| II. Immutability | ✅ PASS | All validators immutable after construction |
| III. Layered Architecture | ✅ PASS | Only Stroke.Core dependency confirmed |
| IV. Cross-Platform | ✅ PASS | No platform-specific code |
| V. Editing Mode Parity | N/A | Not applicable |
| VI. Performance | ✅ PASS | ThreadedValidator design confirmed |
| VII. Full Scope | ✅ PASS | All 22 FRs covered by design |
| VIII. Real-World Testing | ✅ PASS | Test plan uses real implementations |
| IX. Planning Documents | ✅ PASS | Matches api-mapping.md exactly |
| X. File Size Limits | ✅ PASS | ~270 LOC / 7 files = ~39 LOC avg |
| XI. Thread Safety | ✅ PASS | All types are stateless/immutable |

**Post-Design Gate Status**: ✅ PASS - Ready for `/speckit.tasks`
