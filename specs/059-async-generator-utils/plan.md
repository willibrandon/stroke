# Implementation Plan: Async Generator Utilities

**Branch**: `059-async-generator-utils` | **Date**: 2026-02-03 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/059-async-generator-utils/spec.md`

## Summary

Port Python Prompt Toolkit's `prompt_toolkit.eventloop.async_generator` module to C#, providing two utilities: `Aclosing<T>` for safe async generator cleanup via async-disposable wrapper, and `GeneratorToAsyncGenerator<T>` for converting synchronous `IEnumerable<T>` to `IAsyncEnumerable<T>` with backpressure support via bounded `BlockingCollection<T>`.

## Technical Context

**Language/Version**: C# 13 / .NET 10+
**Primary Dependencies**: .NET BCL only (`System.Collections.Concurrent`, `System.Threading.Tasks`)
**Storage**: N/A (in-memory buffering only)
**Testing**: xUnit (no mocks per Constitution VIII)
**Target Platform**: Linux, macOS, Windows 10+ (cross-platform)
**Project Type**: Single project (`Stroke` library + `Stroke.Tests`)
**Performance Goals**: Maintain responsiveness for 50k+ completion items per spec SC-002
**Constraints**: Memory bounded by buffer size; producer thread terminates within 2 seconds of disposal (SC-003)
**Scale/Scope**: 2 utility classes, ~250 LOC implementation, ~300 LOC tests

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Faithful Port | ✅ PASS | Direct port of `async_generator.py`; API matches per `docs/api-mapping.md` lines 762-763 |
| II. Immutability | ✅ PASS | `Done` sentinel is stateless; wrapper delegates to underlying enumerator |
| III. Layered Architecture | ✅ PASS | `Stroke.EventLoop` has no dependencies on higher layers |
| IV. Cross-Platform | ✅ PASS | BCL-only; no platform-specific code |
| V. Editing Modes | ✅ N/A | Not applicable to this feature |
| VI. Performance | ✅ PASS | Bounded buffer provides backpressure; sparse memory usage |
| VII. Full Scope | ✅ PASS | All 4 user stories and 16 FRs will be implemented |
| VIII. Real-World Testing | ✅ PASS | Tests use real async enumeration, no mocks |
| IX. Planning Documents | ✅ PASS | API mapped in `docs/api-mapping.md:762-763`; tests in `docs/test-mapping.md:757-773` |
| X. File Size | ✅ PASS | Estimated ~250 LOC; well under 1,000 limit |
| XI. Thread Safety | ✅ PASS | `BlockingCollection` provides thread-safe producer-consumer; volatile flag for cancellation |

**Gate Result**: ✅ PASS - Proceed to Phase 0

### Post-Phase 1 Re-Check

| Principle | Status | Verification |
|-----------|--------|--------------|
| I. Faithful Port | ✅ PASS | Contract signatures match `api-mapping.md:762-763` exactly |
| II. Immutability | ✅ PASS | `IAsyncDisposableValue<T>` is covariant interface; `Done` is stateless |
| III. Layered Architecture | ✅ PASS | No dependencies added; stays within `Stroke.EventLoop` |
| IX. Planning Documents | ✅ PASS | Test file `AsyncGeneratorTests.cs` matches `test-mapping.md:759` |
| X. File Size | ✅ PASS | Single file `AsyncGeneratorUtils.cs` estimated ~200 LOC |
| XI. Thread Safety | ✅ PASS | `BlockingCollection<T>` + `volatile bool` per research.md |

**Post-Design Gate Result**: ✅ PASS - Ready for `/speckit.tasks`

## Project Structure

### Documentation (this feature)

```text
specs/059-async-generator-utils/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
│   └── async-generator-utils.md
└── tasks.md             # Phase 2 output (/speckit.tasks command)
```

### Source Code (repository root)

```text
src/Stroke/
├── EventLoop/
│   ├── EventLoopUtils.cs           # Existing (050-event-loop-utils)
│   ├── Win32EventLoopUtils.cs      # Existing (054-win32-eventloop-utils)
│   └── AsyncGeneratorUtils.cs      # NEW: Aclosing<T>, GeneratorToAsyncGenerator<T>

tests/Stroke.Tests/
├── EventLoop/
│   ├── EventLoopUtilsTests.cs      # Existing
│   └── AsyncGeneratorTests.cs      # NEW: Tests per test-mapping.md
```

**Structure Decision**: Single project structure matches existing Stroke architecture. New code goes in `Stroke.EventLoop` namespace alongside existing `EventLoopUtils`.

## Complexity Tracking

> No violations requiring justification.

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| (none) | — | — |
