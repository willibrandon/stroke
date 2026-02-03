# Implementation Plan: ConEmu Output

**Branch**: `053-conemu-output` | **Date**: 2026-02-02 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/053-conemu-output/spec.md`

## Summary

ConEmuOutput is a Windows-specific hybrid output class that proxies operations to Win32Output (for console sizing, mouse, scrolling, bracketed paste) and Vt100Output (for rendering). This enables 256-color support in ConEmu/Cmder terminals while maintaining proper Windows console integration.

## Technical Context

**Language/Version**: C# 13 / .NET 10+
**Primary Dependencies**: Stroke.Output (IOutput, Win32Output, Vt100Output), Stroke.Core (PlatformUtils, Size), System.Runtime.Versioning
**Storage**: N/A (in-memory only)
**Testing**: xUnit (no mocks per Constitution VIII)
**Target Platform**: Windows only (`[SupportedOSPlatform("windows")]`)
**Project Type**: Single library (Stroke)
**Performance Goals**: ConEmu detection in under 1ms; rendering at least as fast as pure VT100 output
**Constraints**: Must implement full IOutput interface; must reuse existing Win32Output and Vt100Output
**Scale/Scope**: Single class (~200 LOC) plus tests (~400 LOC)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Faithful Port | ✅ PASS | Direct port of `prompt_toolkit.output.conemu.ConEmuOutput` |
| II. Immutability | ✅ PASS | Class holds references to immutable/sealed outputs; no additional mutable state |
| III. Layered Architecture | ✅ PASS | Stroke.Output layer; depends only on Core and other Output types |
| IV. Cross-Platform | ✅ PASS | Windows-specific with proper platform attribute |
| V. Editing Mode Parity | N/A | Not related to editing modes |
| VI. Performance-Conscious | ✅ PASS | Delegates to existing optimized implementations |
| VII. Full Scope | ✅ PASS | All FR-001 through FR-012 will be implemented |
| VIII. Real-World Testing | ✅ PASS | Tests use real Win32Output and Vt100Output instances |
| IX. Planning Documents | ✅ PASS | Python source reference followed exactly |
| X. File Size Limits | ✅ PASS | Single class ~200 LOC, tests ~400 LOC |
| XI. Thread Safety | ✅ PASS | Delegates to thread-safe Win32Output and Vt100Output |

## Project Structure

### Documentation (this feature)

```text
specs/053-conemu-output/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
│   └── ConEmuOutput.md
└── tasks.md             # Phase 2 output (via /speckit.tasks)
```

### Source Code (repository root)

```text
src/Stroke/
├── Output/
│   └── Windows/
│       ├── Win32Output.cs        # Existing - delegate target for console ops
│       ├── ColorLookupTable.cs   # Existing
│       └── ConEmuOutput.cs       # NEW - hybrid output class
└── Core/
    └── PlatformUtils.cs          # Existing - already has IsConEmuAnsi

tests/Stroke.Tests/
└── Output/
    └── Windows/
        └── ConEmuOutputTests.cs  # NEW - unit tests
```

**Structure Decision**: ConEmuOutput placed in `Stroke.Output.Windows` namespace alongside Win32Output, following the Python module structure (`prompt_toolkit.output.conemu`).

## Complexity Tracking

> No violations requiring justification.

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| (none) | | |
