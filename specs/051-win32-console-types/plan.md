# Implementation Plan: Win32 Console Types

**Branch**: `051-win32-console-types` | **Date**: 2026-02-02 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/051-win32-console-types/spec.md`

## Summary

Implement C# struct types and P/Invoke declarations for Windows Console API interop. This is a 1:1 port of Python Prompt Toolkit's `win32_types.py` module, providing the foundational type layer for native console input/output operations on Windows. All structs use explicit memory layouts matching Windows API byte-for-byte, enabling correct marshalling between managed and unmanaged code.

## Technical Context

**Language/Version**: C# 13 / .NET 10
**Primary Dependencies**: .NET BCL only (System.Runtime.InteropServices, System.Runtime.Versioning)
**Storage**: N/A (in-memory struct types only)
**Testing**: xUnit (no mocks per Constitution VIII)
**Target Platform**: Windows 10+ (structs compile on all platforms; P/Invoke callable only on Windows)
**Project Type**: Single project (Stroke library)
**Performance Goals**: Zero-allocation struct marshalling; struct sizes verified at compile time
**Constraints**: Struct layouts must match native Windows sizes exactly; all P/Invoke calls must be Windows-only
**Scale/Scope**: ~18 types (10 structs, 6 enums, 1 static class, ~12 P/Invoke methods)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Faithful Port (100% API Fidelity) | ✅ PASS | All types from `win32_types.py` will be ported 1:1 |
| II. Immutability by Default | ✅ PASS | Structs are value types; readonly where appropriate |
| III. Layered Architecture | ✅ PASS | Types go in `Stroke.Input.Windows` (low layer, no dependencies on higher layers) |
| IV. Cross-Platform Compatibility | ✅ PASS | Types compile everywhere; P/Invoke guarded with `[SupportedOSPlatform("windows")]` |
| V. Complete Editing Mode Parity | N/A | Not applicable to this feature |
| VI. Performance-Conscious Design | ✅ PASS | Structs use explicit layout for zero-copy marshalling |
| VII. Full Scope Commitment | ✅ PASS | All 18 FRs will be implemented; no deferrals |
| VIII. Real-World Testing | ✅ PASS | Tests verify actual struct sizes and P/Invoke calls on Windows |
| IX. Adherence to Planning Documents | ✅ PASS | Following existing pattern in `ConsoleApi.cs` |
| X. Source Code File Size Limits | ✅ PASS | Types split across multiple files; no file > 1,000 LOC |
| XI. Thread Safety by Default | ✅ PASS | Structs are immutable value types; P/Invoke calls are inherently thread-safe |
| XII. Contracts in Markdown Only | ✅ PASS | Contracts defined in `/contracts/*.md` |

**All gates pass. Proceeding to Phase 0.**

## Project Structure

### Documentation (this feature)

```text
specs/051-win32-console-types/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
└── tasks.md             # Phase 2 output (/speckit.tasks)
```

### Source Code (repository root)

```text
src/Stroke/Input/Windows/
├── ConsoleApi.cs         # [EXISTING] P/Invoke wrapper - will be extended
├── Win32Types/           # [NEW] Namespace for Win32 struct types
│   ├── Coord.cs          # COORD struct
│   ├── SmallRect.cs      # SMALL_RECT struct
│   ├── ConsoleScreenBufferInfo.cs  # CONSOLE_SCREEN_BUFFER_INFO struct
│   ├── KeyEventRecord.cs           # KEY_EVENT_RECORD struct
│   ├── MouseEventRecord.cs         # MOUSE_EVENT_RECORD struct
│   ├── WindowBufferSizeRecord.cs   # WINDOW_BUFFER_SIZE_RECORD struct
│   ├── MenuEventRecord.cs          # MENU_EVENT_RECORD struct
│   ├── FocusEventRecord.cs         # FOCUS_EVENT_RECORD struct
│   ├── InputRecord.cs              # INPUT_RECORD union struct
│   ├── CharInfo.cs                 # CHAR_INFO struct
│   ├── SecurityAttributes.cs       # SECURITY_ATTRIBUTES struct
│   ├── EventType.cs                # EventType enum
│   ├── ControlKeyState.cs          # ControlKeyState flags enum
│   ├── MouseEventFlags.cs          # MouseEventFlags flags enum
│   ├── MouseButtonState.cs         # MouseButtonState flags enum
│   ├── ConsoleInputMode.cs         # ConsoleInputMode flags enum
│   └── ConsoleOutputMode.cs        # ConsoleOutputMode flags enum
├── StdHandles.cs         # [NEW] Standard handle constants
└── Win32Input.cs         # [EXISTING] Will consume new types

tests/Stroke.Tests/Input/Windows/
├── Win32Types/           # [NEW] Tests for Win32 types
│   ├── CoordTests.cs
│   ├── SmallRectTests.cs
│   ├── ConsoleScreenBufferInfoTests.cs
│   ├── KeyEventRecordTests.cs
│   ├── MouseEventRecordTests.cs
│   ├── InputRecordTests.cs
│   ├── CharInfoTests.cs
│   ├── EnumTests.cs      # Tests for all enum types
│   └── NativeMethodsTests.cs  # P/Invoke tests (Windows-only)
```

**Structure Decision**: Types placed under `Stroke.Input.Windows.Win32Types` sub-namespace to maintain separation from existing ConsoleApi. This follows the existing pattern of platform-specific code in `Input/Windows/` and keeps files focused and under 1,000 LOC each.

## Complexity Tracking

No violations. All design choices comply with Constitution principles.

---

## Phase Completion Status

### Phase 0: Research ✅ Complete

**Output**: `research.md`

- Analyzed existing Win32 infrastructure in Stroke (ConsoleApi.cs, Win32Input.cs)
- Verified all struct sizes against Microsoft documentation
- Verified all enum/flag values against official Windows API constants
- Determined P/Invoke pattern (LibraryImport, modern .NET 5+)
- Identified 4 new P/Invoke methods needed (GetConsoleScreenBufferInfo, ReadConsoleInput, WriteConsoleOutput, SetConsoleCursorPosition)

### Phase 1: Design ✅ Complete

**Outputs**:
- `data-model.md` - Complete entity definitions with field layouts
- `contracts/structs.md` - C# API contracts for all 11 structs
- `contracts/enums.md` - C# API contracts for all 6 enums
- `contracts/native-methods.md` - P/Invoke declarations and StdHandles
- `quickstart.md` - Usage examples and platform considerations

### Constitution Re-Check (Post Phase 1)

| Principle | Status | Verification |
|-----------|--------|--------------|
| I. Faithful Port | ✅ PASS | All 9 Python structs + CHAR_INFO mapped; deviations documented |
| II. Immutability | ✅ PASS | All structs use `readonly` fields |
| III. Layered Architecture | ✅ PASS | Types in Stroke.Input.Windows (Input layer) |
| IV. Cross-Platform | ✅ PASS | `[SupportedOSPlatform("windows")]` on all P/Invoke |
| VI. Performance | ✅ PASS | Explicit layouts enable zero-copy marshalling |
| VII. Full Scope | ✅ PASS | All 18 FRs addressed in contracts |
| VIII. Real-World Testing | ✅ PASS | Test plan includes struct size verification |
| X. File Size Limits | ✅ PASS | 17 separate files; none will exceed 100 LOC |
| XI. Thread Safety | ✅ PASS | Immutable value types are inherently thread-safe |
| XII. Contracts in Markdown | ✅ PASS | All contracts in `/contracts/*.md` |

**All gates pass. Ready for `/speckit.tasks` to generate implementation tasks.**

---

## Next Steps

Run `/speckit.tasks` to generate the implementation task list based on this plan.
