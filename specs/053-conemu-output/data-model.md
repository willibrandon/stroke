# Data Model: ConEmu Output

**Feature**: 053-conemu-output
**Date**: 2026-02-02

## Entities

### ConEmuOutput

A Windows-specific hybrid output class that delegates operations to either Win32Output or Vt100Output based on operation type.

| Field | Type | Access | Description |
|-------|------|--------|-------------|
| `Win32Output` | `Win32Output` | Public readonly | Underlying Win32 console output (handles sizing, mouse, scroll) |
| `Vt100Output` | `Vt100Output` | Public readonly | Underlying VT100 output (handles all rendering operations) |

**Relationships**:
- **Composes**: Win32Output (1:1)
- **Composes**: Vt100Output (1:1)
- **Implements**: IOutput interface

**Validation Rules**:
- `stdout` parameter must not be null
- Platform must be Windows (enforced by `[SupportedOSPlatform("windows")]`)
- Console screen buffer must be available (NoConsoleScreenBufferError if not)

**State Transitions**: None - class is stateless beyond holding references to underlying outputs

**Invariants**:
- Both underlying outputs share the same TextWriter
- Both outputs have the same defaultColorDepth (when specified)

## Entity Diagram

```text
┌────────────────────────────────────────────────────────────┐
│                      ConEmuOutput                          │
│                    (IOutput implementation)                │
├────────────────────────────────────────────────────────────┤
│ + Win32Output : Win32Output {readonly}                     │
│ + Vt100Output : Vt100Output {readonly}                     │
│ + RespondsToCpr : bool => false                            │
├────────────────────────────────────────────────────────────┤
│ Constructor(stdout, defaultColorDepth?)                    │
├────────────────────────────────────────────────────────────┤
│ ┌─────────────────────┐    ┌─────────────────────────────┐ │
│ │  Win32 Operations   │    │    VT100 Operations         │ │
│ ├─────────────────────┤    ├─────────────────────────────┤ │
│ │ GetSize()           │    │ Write()                     │ │
│ │ GetRowsBelow...()   │    │ WriteRaw()                  │ │
│ │ EnableMouseSupport()│    │ Flush()                     │ │
│ │ DisableMouseSupport │    │ CursorGoto/Up/Down/...      │ │
│ │ ScrollBuffer...()   │    │ EraseScreen/EndOfLine/Down  │ │
│ │ EnableBracketedP... │    │ Enter/QuitAlternateScreen   │ │
│ │ DisableBracketedP...│    │ Hide/ShowCursor             │ │
│ └─────────────────────┘    │ Set/ResetCursorShape        │ │
│                            │ SetAttributes/Reset...      │ │
│                            │ SetTitle/ClearTitle         │ │
│                            │ Bell, AskForCpr, etc.       │ │
│                            └─────────────────────────────┘ │
└────────────────────────────────────────────────────────────┘
           │                           │
           ▼                           ▼
┌─────────────────────┐    ┌─────────────────────────────┐
│    Win32Output      │    │       Vt100Output           │
│   (kernel32.dll)    │    │   (ANSI escape sequences)   │
│   [SupportedOS      │    │                             │
│    Platform         │    │                             │
│    ("windows")]     │    │                             │
└─────────────────────┘    └─────────────────────────────┘
           │                           │
           └───────────┬───────────────┘
                       ▼
              ┌─────────────────┐
              │    TextWriter   │
              │    (shared)     │
              └─────────────────┘
```

## Dependencies

### Existing Types Used

| Type | Namespace | Role |
|------|-----------|------|
| `IOutput` | `Stroke.Output` | Interface to implement |
| `Win32Output` | `Stroke.Output.Windows` | Delegate for console operations |
| `Vt100Output` | `Stroke.Output` | Delegate for rendering operations |
| `Size` | `Stroke.Core.Primitives` | Terminal size representation |
| `ColorDepth` | `Stroke.Output` | Color depth enum |
| `NoConsoleScreenBufferError` | `Stroke.Output` | Exception type |
| `PlatformUtils.IsConEmuAnsi` | `Stroke.Core` | ConEmu detection (already exists) |

### New Types Created

| Type | Namespace | Purpose |
|------|-----------|---------|
| `ConEmuOutput` | `Stroke.Output.Windows` | The hybrid output class |
