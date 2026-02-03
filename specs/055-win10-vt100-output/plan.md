# Implementation Plan: Windows 10 VT100 Output

**Branch**: `055-win10-vt100-output` | **Date**: 2026-02-03 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/055-win10-vt100-output/spec.md`

## Summary

Implement `Windows10Output`, a Windows-specific IOutput implementation that enables VT100 escape sequences on Windows 10+ by temporarily setting the `ENABLE_VIRTUAL_TERMINAL_PROCESSING` console mode flag during flush operations. The class combines `Win32Output` (console operations) and `Vt100Output` (rendering) in a hybrid proxy pattern, matching the existing `ConEmuOutput` design.

## Technical Context

**Language/Version**: C# 13 / .NET 10+
**Primary Dependencies**: `Stroke.Output` (IOutput, Vt100Output), `Stroke.Output.Windows` (Win32Output), `Stroke.Input.Windows` (ConsoleApi, Win32Types), `Stroke.Core` (PlatformUtils, Size), `System.Runtime.Versioning`
**Storage**: N/A (in-memory only)
**Testing**: xUnit (no mocks, no FluentAssertions per Constitution VIII)
**Target Platform**: Windows 10 build 10586+ (November 2015)
**Project Type**: Single library project
**Performance Goals**: Minimal overhead during flush (save/set/restore console mode)
**Constraints**: Per-instance thread safety via `Lock`, console mode restoration guaranteed via finally block
**Scale/Scope**: 2 new classes (~300 LOC total), following existing ConEmuOutput pattern

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Faithful Port (100% API Fidelity) | ✅ Pass | Python `Windows10_Output` class faithfully ported with matching semantics |
| II. Immutability by Default | ✅ Pass | `Windows10Output` is stateful (holds console handle, locks), uses `sealed` class |
| III. Layered Architecture | ✅ Pass | Lives in `Stroke.Output.Windows` (Rendering layer), no upward dependencies |
| IV. Cross-Platform Terminal Compatibility | ✅ Pass | Windows-specific output with VT100 escape sequences, true color support |
| V. Complete Editing Mode Parity | N/A | Output class, not input/keybinding |
| VI. Performance-Conscious Design | ✅ Pass | Console mode switch is minimal overhead per flush |
| VII. Full Scope Commitment | ✅ Pass | All requirements will be implemented as specified |
| VIII. Real-World Testing | ✅ Pass | Tests use real Windows console APIs, no mocks |
| IX. Adherence to Planning Documents | ✅ Pass | Following `api-mapping.md` for `prompt_toolkit.output.windows10` |
| X. Source Code File Size Limits | ✅ Pass | Estimated ~200 LOC for Windows10Output, ~50 LOC for WindowsVt100Support |
| XI. Thread Safety by Default | ✅ Pass | Per-instance `Lock` for flush serialization per spec clarification |

## Project Structure

### Documentation (this feature)

```text
specs/055-win10-vt100-output/
├── spec.md              # Feature specification
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
│   └── Windows10Output.md
└── checklists/
    └── comprehensive.md  # Specification validation
```

### Source Code (repository root)

```text
src/Stroke/Output/Windows/
├── Win32Output.cs           # Existing - legacy Windows console
├── ConEmuOutput.cs          # Existing - ConEmu hybrid (reference pattern)
├── Windows10Output.cs       # NEW - Windows 10 VT100 hybrid
├── WindowsVt100Support.cs   # NEW - VT100 detection utility
├── ColorLookupTable.cs      # Existing
├── ForegroundColor.cs       # Existing
└── BackgroundColor.cs       # Existing

tests/Stroke.Tests/Output/Windows/
├── Win32OutputTests.cs      # Existing
├── ConEmuOutputTests.cs     # Existing
├── Windows10OutputTests.cs  # NEW
└── WindowsVt100SupportTests.cs  # NEW
```

**Structure Decision**: Single project structure, following existing `Stroke.Output.Windows` namespace organization. New files placed alongside existing Windows output implementations.

## Complexity Tracking

No violations requiring justification. Design follows established `ConEmuOutput` pattern.

---

## Phase 0: Research

### Research Tasks

1. **Console Mode Constants**: Verify ENABLE_PROCESSED_INPUT (0x0001) and ENABLE_VIRTUAL_TERMINAL_PROCESSING (0x0004) values
2. **Existing VT100 Detection**: Review PlatformUtils.IsWindowsVt100Supported implementation
3. **ConEmuOutput Pattern**: Study proxy composition design for Win32Output + Vt100Output
4. **Python Reference**: Verify all Python `Windows10_Output` APIs are identified

### Findings

See [research.md](research.md) for detailed findings.

---

## Phase 1: Design & Contracts

### Key Entities

| Entity | Purpose | Mutability |
|--------|---------|------------|
| `Windows10Output` | Hybrid IOutput combining Win32Output (console ops) + Vt100Output (rendering) with VT100 mode switching | Mutable (console handle, lock) |
| `WindowsVt100Support` | Static utility for VT100 support detection | Stateless |

### API Contracts

See [contracts/Windows10Output.md](contracts/Windows10Output.md) for full contract.

### Delegation Strategy

Following the established `ConEmuOutput` pattern:

| Operation Category | Delegate To | Rationale |
|-------------------|-------------|-----------|
| Writing (Write, WriteRaw) | Vt100Output | VT100 escape sequences for rendering |
| **Flush** | **Custom** | Enable VT100 mode → Vt100Output.Flush() → Restore mode |
| Screen Control | Vt100Output | VT100 escape sequences |
| Cursor Movement | Vt100Output | VT100 escape sequences |
| Cursor Visibility | Vt100Output | VT100 escape sequences |
| Attributes | Vt100Output | VT100 escape sequences |
| Mouse Support | Win32Output | Windows console API |
| Bracketed Paste | Win32Output | Windows console API |
| Title | Vt100Output | VT100 escape sequences |
| Bell | Vt100Output | VT100 escape sequences |
| GetSize() | Win32Output | Windows console API |
| GetRowsBelowCursorPosition() | Win32Output | Windows console API |
| ScrollBufferToPrompt() | Win32Output | Windows console API |
| GetDefaultColorDepth() | **Custom** | Returns TrueColor by default |

### Thread Safety Design

Per FR-011 and Constitution XI:

```csharp
public sealed class Windows10Output : IOutput
{
    private readonly Lock _lock = new();  // Per-instance lock

    public void Flush()
    {
        using (_lock.EnterScope())  // Serialize flush operations
        {
            // Save console mode
            // Enable VT100
            try
            {
                _vt100Output.Flush();
            }
            finally
            {
                // Restore console mode
            }
        }
    }
}
```

### Console Mode Switching Flow

```text
Flush() called
  │
  ├─ Acquire per-instance lock
  │
  ├─ GetConsoleMode() → save originalMode
  │
  ├─ SetConsoleMode(ENABLE_PROCESSED_INPUT | ENABLE_VIRTUAL_TERMINAL_PROCESSING)
  │
  ├─ try:
  │     └─ _vt100Output.Flush()
  │
  └─ finally:
        └─ SetConsoleMode(originalMode)
```

---

## Implementation Notes

### Differences from ConEmuOutput

| Aspect | ConEmuOutput | Windows10Output |
|--------|--------------|-----------------|
| VT100 mode | Always enabled (ConEmu terminal) | Enabled per-flush operation |
| Console mode switching | None | Save/Set/Restore per flush |
| Thread safety | Delegated to underlying outputs | Per-instance lock for flush serialization |
| Color depth default | Auto-detected | TrueColor (24-bit) |

### P/Invoke Reuse

Windows10Output will reuse existing P/Invoke declarations from:
- `Stroke.Core.PlatformUtils.Vt100Detection` (for detection)
- `Stroke.Input.Windows.ConsoleApi` (for console operations)

The constants `ENABLE_PROCESSED_INPUT` (0x0001) and `ENABLE_VIRTUAL_TERMINAL_PROCESSING` (0x0004) are already defined in `ConsoleApi`.

### WindowsVt100Support Placement

Per the spec, `WindowsVt100Support` is a separate static utility class. However, `PlatformUtils.IsWindowsVt100Supported` already provides this functionality. Options:

1. **Recommended**: Create `WindowsVt100Support.IsVt100Enabled()` as a thin wrapper around `PlatformUtils.IsWindowsVt100Supported` for API parity with Python
2. Alternative: Skip WindowsVt100Support and use PlatformUtils directly

Decision: Option 1 for faithful port (API naming matches Python's `is_win_vt100_enabled()`).
