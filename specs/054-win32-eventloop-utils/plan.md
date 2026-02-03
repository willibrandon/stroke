# Implementation Plan: Win32 Event Loop Utilities

**Branch**: `054-win32-eventloop-utils` | **Date**: 2026-02-03 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/054-win32-eventloop-utils/spec.md`

## Summary

Port Win32-specific event loop utilities from Python Prompt Toolkit's `eventloop/win32.py` module: `wait_for_handles` (synchronous multiplexed waiting) and `create_win32_event` (manual-reset event creation). Add async wrapper `WaitForHandlesAsync` with 100ms polling and cancellation token support. All P/Invoke infrastructure already exists in `ConsoleApi.cs`; this feature wraps those primitives with idiomatic C# APIs.

## Technical Context

**Language/Version**: C# 13 / .NET 10+
**Primary Dependencies**: Stroke.Input.Windows (ConsoleApi, Win32Types) — all P/Invoke already ported
**Storage**: N/A (in-memory kernel handles only)
**Testing**: xUnit (no mocks per Constitution VIII), Windows-only conditional tests
**Target Platform**: Windows 10+ (platform-gated via `[SupportedOSPlatform("windows")]`)
**Project Type**: Single project (Stroke)
**Performance Goals**: Timeout accuracy within 10% of specified duration (SC-002)
**Constraints**: Max 64 handles per WaitForMultipleObjects call (Windows limit)
**Scale/Scope**: Single static utility class (~150 LOC estimated)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Faithful Port | ✅ PASS | Direct 1:1 port of `wait_for_handles`, `create_win32_event` from Python PTK `eventloop/win32.py` |
| II. Immutability by Default | ✅ PASS | Static utility class, no mutable state |
| III. Layered Architecture | ✅ PASS | Lives in `Stroke.EventLoop` alongside existing `EventLoopUtils.cs`; depends only on `Stroke.Input.Windows` (lower layer) |
| IV. Cross-Platform Compatibility | ✅ PASS | Windows-only by design; platform-gated with `[SupportedOSPlatform("windows")]` |
| V. Complete Editing Mode Parity | N/A | Not applicable to event loop utilities |
| VI. Performance-Conscious Design | ✅ PASS | Direct P/Invoke, no allocations in hot path, 100ms polling for async |
| VII. Full Scope Commitment | ✅ PASS | All 13 functional requirements will be implemented |
| VIII. Real-World Testing | ✅ PASS | Tests will use actual Windows events, not mocks |
| IX. Adherence to Planning Documents | ✅ PASS | API matches `api-mapping.md` pattern for eventloop module |
| X. Source Code File Size | ✅ PASS | Estimated ~150 LOC, well under 1,000 limit |
| XI. Thread Safety by Default | ✅ PASS | Static stateless class; underlying Win32 APIs are thread-safe |

## Project Structure

### Documentation (this feature)

```text
specs/054-win32-eventloop-utils/
├── spec.md              # Feature specification
├── plan.md              # This file
├── research.md          # Phase 0: No unknowns (infrastructure complete)
├── data-model.md        # Phase 1: Key entities
├── quickstart.md        # Phase 1: Usage examples
├── contracts/           # Phase 1: API contracts (markdown only)
│   └── win32-eventloop-utils.md
└── tasks.md             # Phase 2 output (not created by /speckit.plan)
```

### Source Code (repository root)

```text
src/Stroke/
├── EventLoop/
│   ├── EventLoopUtils.cs        # Existing cross-platform utilities
│   └── Win32EventLoopUtils.cs   # NEW: Windows-specific utilities (this feature)
└── Input/Windows/
    ├── ConsoleApi.cs            # P/Invoke (already has WaitForMultipleObjects, CreateEvent, etc.)
    └── Win32Types/              # Structs (SecurityAttributes already ported)

tests/Stroke.Tests/
└── EventLoop/
    ├── EventLoopUtilsTests.cs   # Existing tests
    └── Win32EventLoopUtilsTests.cs  # NEW: Windows-specific tests
```

**Structure Decision**: Single project with namespace separation. Win32-specific utilities in `Stroke.EventLoop` namespace (matching Python PTK's `prompt_toolkit.eventloop.win32` → `Stroke.EventLoop`). Platform-gated at class level with `[SupportedOSPlatform("windows")]`.

## Complexity Tracking

> No violations — complexity tracking not required.

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| (none) | — | — |
