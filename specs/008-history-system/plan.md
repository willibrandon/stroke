# Implementation Plan: History System

**Branch**: `008-history-system` | **Date**: 2026-01-24 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/008-history-system/spec.md`

## Summary

Implement a complete history system for storing and retrieving command history from buffers, porting Python Prompt Toolkit's `prompt_toolkit.history` module. The system provides four implementations: `InMemoryHistory` (session-only), `DummyHistory` (no-op), `FileHistory` (persistent disk storage), and `ThreadedHistory` (background loading wrapper). All implementations follow the `IHistory` interface with thread-safe operations per Constitution XI.

## Technical Context

**Language/Version**: C# 13 / .NET 10
**Primary Dependencies**: None (Stroke.History is part of Stroke.Core layer)
**Storage**: File system for `FileHistory`, in-memory for others
**Testing**: xUnit with no mocks (per Constitution VIII)
**Target Platform**: Linux, macOS, Windows 10+
**Project Type**: Single project (existing `src/Stroke/` structure)
**Performance Goals**: ThreadedHistory must make first item available within 100ms of LoadAsync call (SC-003)
**Constraints**: Thread-safe operations, byte-for-byte file format compatibility with Python Prompt Toolkit
**Scale/Scope**: 4 history implementations, ~15 public API members, 80% test coverage target

## Constitution Check

*GATE: Passed before Phase 0 research. Re-checked after Phase 1 design: ✅ ALL PASS*

| Principle | Pre-Phase 0 | Post-Phase 1 | Notes |
|-----------|-------------|--------------|-------|
| I. Faithful Port (100% API Fidelity) | ✅ PASS | ✅ PASS | All 5 classes from `prompt_toolkit.history` mapped: History→HistoryBase, ThreadedHistory, DummyHistory, FileHistory, InMemoryHistory |
| II. Immutability by Default | ✅ PASS | ✅ PASS | History entries are strings (immutable); only mutable state is internal lists |
| III. Layered Architecture | ✅ PASS | ✅ PASS | `Stroke.History` namespace within Stroke.Core layer, zero external dependencies |
| IV. Cross-Platform Compatibility | ✅ PASS | ✅ PASS | FileHistory uses standard file I/O; UTF-8 encoding with replacement |
| V. Complete Editing Mode Parity | N/A | N/A | Not applicable to history system |
| VI. Performance-Conscious Design | ✅ PASS | ✅ PASS | ThreadedHistory provides background loading; lazy evaluation in base class |
| VII. Full Scope Commitment | ✅ PASS | ✅ PASS | All 4 implementations specified with complete APIs |
| VIII. Real-World Testing | ✅ PASS | ✅ PASS | Will use xUnit, real file system for FileHistory tests |
| IX. Adherence to Planning Documents | ✅ PASS | ✅ PASS | Verified against `docs/api-mapping.md` lines 925-959 |
| X. Source Code File Size Limits | ✅ PASS | ✅ PASS | Each implementation in separate file; ThreadedHistory ~150 LOC max |
| XI. Thread Safety by Default | ✅ PASS | ✅ PASS | All mutable implementations use `System.Threading.Lock` |

**Post-Phase 1 Verification**: Re-read Python source (`prompt_toolkit/history.py`). All 5 public classes (`History`, `ThreadedHistory`, `DummyHistory`, `FileHistory`, `InMemoryHistory`) and their public APIs are accounted for in data-model.md.

## Project Structure

### Documentation (this feature)

```text
specs/008-history-system/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output (N/A - no REST/GraphQL APIs)
└── tasks.md             # Phase 2 output (/speckit.tasks command)
```

### Source Code (repository root)

```text
src/Stroke/History/
├── IHistory.cs          # Interface (exists, needs update)
├── HistoryBase.cs       # Abstract base class with caching (NEW)
├── InMemoryHistory.cs   # Session-only storage (exists, needs update)
├── DummyHistory.cs      # No-op implementation (NEW)
├── FileHistory.cs       # Persistent file storage (NEW)
└── ThreadedHistory.cs   # Background loading wrapper (NEW)

tests/Stroke.Tests/History/
├── InMemoryHistoryTests.cs     # (NEW)
├── DummyHistoryTests.cs        # (NEW)
├── FileHistoryTests.cs         # (NEW)
├── ThreadedHistoryTests.cs     # (NEW)
└── HistoryIntegrationTests.cs  # (NEW)
```

**Structure Decision**: Single project structure extending existing `src/Stroke/History/` directory. Tests in `tests/Stroke.Tests/History/` following existing test project pattern.

## Complexity Tracking

> No violations detected. Implementation follows existing patterns.

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| (none) | N/A | N/A |
