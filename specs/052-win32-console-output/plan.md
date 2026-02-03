# Implementation Plan: Win32 Console Output

**Branch**: `052-win32-console-output` | **Date**: 2026-02-02 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/052-win32-console-output/spec.md`

## Summary

Implement `Win32Output`, a Windows Console API-based `IOutput` implementation for legacy Windows terminals (cmd.exe) that don't support ANSI/VT100 escape sequences. The implementation provides direct cursor control, 16-color palette mapping, alternate screen buffer support, and screen erase operations using kernel32.dll P/Invoke calls.

## Technical Context

**Language/Version**: C# 13 / .NET 10+
**Primary Dependencies**: System.Runtime.InteropServices (P/Invoke), Stroke.Input.Windows.Win32Types, Stroke.Styles, Stroke.Output
**Storage**: N/A (in-memory buffering only)
**Testing**: xUnit (no mocks per Constitution VIII)
**Target Platform**: Windows 10+ (with legacy console fallback for older Windows)
**Project Type**: Single .NET library (Stroke)
**Performance Goals**: Cursor operations within 1ms response time; RGB-to-Win32 color lookup cached
**Constraints**: Win32 console limited to 16-color (4-bit) palette; character-by-character output to avoid rendering artifacts
**Scale/Scope**: Single IOutput implementation (~600-800 LOC split across 6 source files)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

### Pre-Phase 0 Check ✅

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Faithful Port | ✅ PASS | Porting `prompt_toolkit.output.win32` module exactly: Win32Output, ColorLookupTable, NoConsoleScreenBufferError, FOREGROUND_COLOR, BACKGROUND_COLOR |
| II. Immutability by Default | ✅ PASS | ColorLookupTable is effectively immutable after construction (thread-safe cache). Win32Output has mutable state but encapsulates it safely. |
| III. Layered Architecture | ✅ PASS | Win32Output goes in Stroke.Output.Windows namespace, depends only on Core and Styles (lower layers) |
| IV. Cross-Platform Terminal Compatibility | ✅ PASS | This IS the Windows fallback required by the principle |
| V. Complete Editing Mode Parity | N/A | Not editing mode related |
| VI. Performance-Conscious Design | ✅ PASS | RGB-to-Win32 color cache matches Python implementation |
| VII. Full Scope Commitment | ✅ PASS | All 18 functional requirements will be implemented |
| VIII. Real-World Testing | ✅ PASS | Tests will use actual Win32Output on Windows; conditional tests for non-Windows |
| IX. Adherence to Planning Documents | ✅ PASS | Will consult api-mapping.md for Win32Output class structure |
| X. Source Code File Size Limits | ✅ PASS | Split into Win32Output.cs (~400 LOC) and Win32Output.Colors.cs (~200 LOC) |
| XI. Thread Safety by Default | ✅ PASS | Win32Output will use Lock for buffer/state; ColorLookupTable cache will be thread-safe |

### Post-Phase 1 Re-Check ✅

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Faithful Port | ✅ PASS | Contract in `contracts/Win32Output.md` matches Python API exactly |
| II. Immutability by Default | ✅ PASS | ForegroundColor/BackgroundColor are static readonly; ColorLookupTable is effectively immutable |
| III. Layered Architecture | ✅ PASS | Stroke.Output.Windows depends on Stroke.Input.Windows, Stroke.Styles, Stroke.Core (all lower layers) |
| IX. Adherence to Planning Documents | ✅ PASS | Contract uses markdown format per Constitution XII |
| X. Source Code File Size Limits | ✅ PASS | 6 files planned, none exceeding 400 LOC |
| XII. Contracts in Markdown | ✅ PASS | All contracts in `contracts/Win32Output.md` as markdown |

## Project Structure

### Documentation (this feature)

```text
specs/052-win32-console-output/
├── spec.md              # Feature specification
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
│   └── Win32Output.md   # API contract in markdown
└── tasks.md             # Phase 2 output (/speckit.tasks)
```

### Source Code (repository root)

```text
src/Stroke/
├── Output/
│   ├── Windows/
│   │   ├── Win32Output.cs           # Main IOutput implementation
│   │   ├── Win32Output.Colors.cs    # Color handling partial class
│   │   ├── ColorLookupTable.cs      # RGB-to-Win32 color mapper
│   │   └── ForegroundColor.cs       # Win32 foreground color constants
│   │   └── BackgroundColor.cs       # Win32 background color constants
│   └── NoConsoleScreenBufferError.cs  # Exception type (sibling to IOutput)
├── Input/Windows/
│   └── ConsoleApi.cs                # Extended with new P/Invoke methods

tests/Stroke.Tests/
├── Output/Windows/
│   ├── Win32OutputTests.cs          # Core functionality tests
│   ├── Win32OutputColorTests.cs     # Color mapping tests
│   └── ColorLookupTableTests.cs     # Color lookup tests
```

**Structure Decision**: Win32Output placed in `Stroke.Output.Windows` namespace mirroring Python's `prompt_toolkit.output.win32` module. P/Invoke extensions go in existing `Stroke.Input.Windows.ConsoleApi` since they're shared infrastructure.

## Complexity Tracking

> No violations to track. Implementation follows existing patterns.
