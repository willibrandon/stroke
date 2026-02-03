# Data Model: Windows 10 VT100 Output

**Feature**: 055-win10-vt100-output
**Date**: 2026-02-03

## Entities

This feature introduces two new types, both stateless or internally-managed state.

### Windows10Output

| Field | Type | Description | Mutability |
|-------|------|-------------|------------|
| `_win32Output` | `Win32Output` | Underlying Win32 console output | Immutable (readonly) |
| `_vt100Output` | `Vt100Output` | Underlying VT100 terminal output | Immutable (readonly) |
| `_hconsole` | `nint` | Console output handle | Immutable (readonly) |
| `_defaultColorDepth` | `ColorDepth?` | Optional color depth override | Immutable (readonly) |
| `_lock` | `Lock` | Per-instance synchronization | Immutable (readonly) |

**State Transitions**: None - the class holds references to mutable underlying outputs but does not transition through states itself.

**Validation Rules**:
- `stdout` parameter cannot be null
- Must be running on Windows platform
- Must have valid console handle

### WindowsVt100Support

Stateless static utility class. No fields or state.

## Relationships

```text
Windows10Output
    ├── has-a Win32Output (for console operations)
    ├── has-a Vt100Output (for rendering)
    └── uses ConsoleApi P/Invoke (for mode switching)

WindowsVt100Support
    └── delegates to PlatformUtils.IsWindowsVt100Supported
```

## Console Mode Data

While not a persistent data model, the console mode is a transient value:

| Constant | Value | Purpose |
|----------|-------|---------|
| `ENABLE_PROCESSED_INPUT` | 0x0001 | Process Ctrl+C and other control sequences |
| `ENABLE_VIRTUAL_TERMINAL_PROCESSING` | 0x0004 | Process VT100 escape sequences |

**Combined Mode**: `0x0005` (both flags enabled during flush)

## Dependencies

### Existing Types Used

| Type | Namespace | Purpose |
|------|-----------|---------|
| `IOutput` | `Stroke.Output` | Interface contract |
| `Vt100Output` | `Stroke.Output` | VT100 rendering |
| `Win32Output` | `Stroke.Output.Windows` | Console operations |
| `ColorDepth` | `Stroke.Output` | Color capability enum |
| `ConsoleApi` | `Stroke.Input.Windows` | P/Invoke declarations |
| `Size` | `Stroke.Core.Primitives` | Terminal dimensions |
| `Attrs` | `Stroke.Styles` | Text attributes |
| `CursorShape` | `Stroke.CursorShapes` | Cursor shape enum |
| `PlatformUtils` | `Stroke.Core` | VT100 detection |
| `NoConsoleScreenBufferError` | `Stroke.Output.Windows` | Exception type |

### Layer Dependencies

```text
Stroke.Output.Windows.Windows10Output
    │
    ├── Stroke.Output (IOutput, Vt100Output, ColorDepth)
    ├── Stroke.Output.Windows (Win32Output, NoConsoleScreenBufferError)
    ├── Stroke.Input.Windows (ConsoleApi) *cross-layer but permitted*
    ├── Stroke.Core (PlatformUtils, Size)
    ├── Stroke.Core.Primitives (Size)
    ├── Stroke.Styles (Attrs)
    └── Stroke.CursorShapes (CursorShape)
```

**Note**: The dependency on `Stroke.Input.Windows.ConsoleApi` crosses from Rendering to Input layer. This is permitted because:
1. ConsoleApi contains platform P/Invoke declarations (infrastructure)
2. Both Win32Output and ConEmuOutput already depend on it
3. The layering prohibition is against higher layers depending on lower layers
