# Research: ConEmu Output

**Feature**: 053-conemu-output
**Date**: 2026-02-02

## Research Summary

This feature has minimal unknowns due to clear Python source reference and existing infrastructure in Stroke.

## 1. Delegation Pattern

**Decision**: Use explicit method delegation (not `__getattr__` dynamic dispatch)

**Rationale**:
- C# does not have `__getattr__` equivalent that works at runtime
- Explicit interface implementation provides compile-time safety
- Each IOutput method explicitly calls either `_win32Output` or `_vt100Output`
- This is more idiomatic C# and aligns with how other Stroke output classes work

**Alternatives Considered**:
- Reflection-based dispatch: Rejected due to performance overhead and lack of compile-time safety
- Source generators: Over-engineered for 30 methods
- Manual switch on method name: Same as explicit delegation but less clear

## 2. Vt100Output GetSize Callback

**Decision**: Pass `() => Size.Empty` (or `() => new Size(0, 0)`) as the `getSize` parameter to Vt100Output

**Rationale**:
- Python source uses `lambda: Size(0, 0)` for the same purpose
- ConEmuOutput delegates `GetSize()` to Win32Output anyway
- The Vt100Output's `getSize` callback is never actually used when accessed through ConEmuOutput

**Python Reference**:
```python
self.vt100_output = Vt100_Output(
    stdout, lambda: Size(0, 0), default_color_depth=default_color_depth
)
```

## 3. Thread Safety

**Decision**: ConEmuOutput itself has no mutable state; thread safety is delegated

**Rationale**:
- Win32Output is already thread-safe (uses `Lock`)
- Vt100Output is already thread-safe (uses `Lock`)
- ConEmuOutput only holds readonly references to these outputs
- No additional synchronization needed in ConEmuOutput

**Constitution XI Compliance**: ✅ Satisfied through delegation

## 4. Constructor Parameters

**Decision**: Constructor takes `TextWriter stdout` and optional `ColorDepth? defaultColorDepth`

**Rationale**: Matches Python Prompt Toolkit constructor signature exactly

**Python Reference**:
```python
def __init__(
    self, stdout: TextIO, default_color_depth: ColorDepth | None = None
) -> None:
```

## 5. Public Properties for Underlying Outputs

**Decision**: Expose `Win32Output` and `Vt100Output` as public readonly properties

**Rationale**:
- FR-011 requires exposing underlying outputs
- Python class has `win32_output` and `vt100_output` as public instance attributes
- Enables advanced users to access underlying implementations directly

## 6. Operation Delegation Mapping

**Decision**: Follow Python source exactly for delegation rules

| Delegate to Win32Output | Delegate to Vt100Output |
|------------------------|------------------------|
| `GetSize` | `Write` |
| `GetRowsBelowCursorPosition` | `WriteRaw` |
| `EnableMouseSupport` | `Flush` |
| `DisableMouseSupport` | `EraseScreen` |
| `ScrollBufferToPrompt` | `EraseEndOfLine` |
| `EnableBracketedPaste` | `EraseDown` |
| `DisableBracketedPaste` | `EnterAlternateScreen` |
| | `QuitAlternateScreen` |
| | `CursorGoto` |
| | `CursorUp/Down/Forward/Backward` |
| | `HideCursor/ShowCursor` |
| | `SetCursorShape/ResetCursorShape` |
| | `ResetAttributes/SetAttributes` |
| | `DisableAutowrap/EnableAutowrap` |
| | `SetTitle/ClearTitle` |
| | `Bell` |
| | `AskForCpr` |
| | `ResetCursorKeyMode` |
| | `Fileno` |
| | `GetDefaultColorDepth` |

**Python Reference**:
```python
if name in (
    "get_size",
    "get_rows_below_cursor_position",
    "enable_mouse_support",
    "disable_mouse_support",
    "scroll_buffer_to_prompt",
    "get_win32_screen_buffer_info",
    "enable_bracketed_paste",
    "disable_bracketed_paste",
):
    return getattr(self.win32_output, name)
else:
    return getattr(self.vt100_output, name)
```

**Note**: `get_win32_screen_buffer_info` is Win32Output-specific and not part of IOutput interface. Expose as separate public method if needed.

## 7. RespondsToCpr Property

**Decision**: Always return `false`

**Rationale**:
- Python source explicitly returns `False` with comment "We don't need this on Windows"
- Cursor Position Report is handled differently on Windows console
- Consistent with Win32Output behavior

## 8. Platform Attribute

**Decision**: Use `[SupportedOSPlatform("windows")]` on class

**Rationale**:
- FR-010 requires platform safety annotation
- Matches existing Win32Output pattern
- Provides compile-time and IDE warnings when used on non-Windows

## 9. IsConEmuAnsi Property

**Decision**: Property already exists in `PlatformUtils.IsConEmuAnsi`

**Rationale**: No work needed; the property was added as part of feature 050-event-loop-utils

**Existing Implementation** (PlatformUtils.cs:69-70):
```csharp
public static bool IsConEmuAnsi =>
    IsWindows && Environment.GetEnvironmentVariable("ConEmuANSI") == "ON";
```

## 10. Stdout and Encoding Properties

**Decision**: Delegate `Stdout` and `Encoding` to Vt100Output

**Rationale**:
- Both outputs share the same TextWriter
- Vt100Output uses "utf-8" encoding which is correct for escape sequences
- Win32Output uses "utf-16" but that's internal to console API

## Unresolved Items

None. All technical decisions are resolved.
