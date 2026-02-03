# Research: Windows 10 VT100 Output

**Feature**: 055-win10-vt100-output
**Date**: 2026-02-03

## Research Tasks

### 1. Console Mode Constants

**Question**: Verify ENABLE_PROCESSED_INPUT (0x0001) and ENABLE_VIRTUAL_TERMINAL_PROCESSING (0x0004) values.

**Finding**: Constants are correctly defined in multiple locations:

| Constant | Value | Location |
|----------|-------|----------|
| `ENABLE_PROCESSED_INPUT` | 0x0001 | `ConsoleApi.cs:17` - Console input mode flag |
| `ENABLE_VIRTUAL_TERMINAL_PROCESSING` | 0x0004 | `ConsoleApi.cs:21`, `PlatformUtils.cs:213` |

**Note**: The Python reference uses both constants together when enabling VT100:
```python
DWORD(ENABLE_PROCESSED_INPUT | ENABLE_VIRTUAL_TERMINAL_PROCESSING)  # 0x0005
```

**Decision**: Use existing constants from `Stroke.Input.Windows.ConsoleApi`.

---

### 2. Existing VT100 Detection

**Question**: Review PlatformUtils.IsWindowsVt100Supported implementation.

**Finding**: Full implementation exists in `PlatformUtils.cs:148-194`:

```csharp
public static bool IsWindowsVt100Supported => IsWindows && CheckWindowsVt100Support();

private static bool CheckWindowsVt100Support()
{
    // 1. Get stdout handle
    var handle = Vt100Detection.GetStdHandle(Vt100Detection.STD_OUTPUT_HANDLE);

    // 2. Save original mode
    if (!Vt100Detection.GetConsoleMode(handle, out var originalMode))
        return false;

    try
    {
        // 3. Try to enable VT100
        var newMode = originalMode | Vt100Detection.ENABLE_VIRTUAL_TERMINAL_PROCESSING;
        return Vt100Detection.SetConsoleMode(handle, newMode);
    }
    finally
    {
        // 4. Restore original mode
        Vt100Detection.SetConsoleMode(handle, originalMode);
    }
}
```

**Decision**: `WindowsVt100Support.IsVt100Enabled()` will delegate to `PlatformUtils.IsWindowsVt100Supported` for consistency.

---

### 3. ConEmuOutput Pattern

**Question**: Study proxy composition design for Win32Output + Vt100Output.

**Finding**: `ConEmuOutput` (251 LOC) provides the exact pattern to follow:

```csharp
public sealed class ConEmuOutput : IOutput
{
    private readonly Win32Output _win32Output;
    private readonly Vt100Output _vt100Output;

    public ConEmuOutput(TextWriter stdout, ColorDepth? defaultColorDepth = null)
    {
        _win32Output = new Win32Output(stdout, defaultColorDepth: defaultColorDepth);
        _vt100Output = Vt100Output.FromPty(stdout, defaultColorDepth: defaultColorDepth);
    }

    // Rendering → Vt100Output
    public void Write(string data) => _vt100Output.Write(data);
    public void Flush() => _vt100Output.Flush();  // ← Windows10Output overrides this

    // Console ops → Win32Output
    public Size GetSize() => _win32Output.GetSize();
    public void EnableMouseSupport() => _win32Output.EnableMouseSupport();
}
```

**Key Difference for Windows10Output**: The `Flush()` method needs custom implementation:
1. Acquire per-instance lock
2. Save current console mode
3. Enable VT100 processing
4. Delegate to `_vt100Output.Flush()`
5. Restore original console mode (in finally block)

**Decision**: Copy ConEmuOutput structure, override `Flush()` with VT100 mode switching.

---

### 4. Python Reference API Inventory

**Question**: Verify all Python `Windows10_Output` APIs are identified.

**Finding**: Python `windows10.py` (134 LOC) contains:

**Class: `Windows10_Output`**

| Python API | C# Equivalent | Delegation |
|------------|---------------|------------|
| `__init__(stdout, default_color_depth)` | Constructor | Creates Win32Output + Vt100Output |
| `flush()` | `Flush()` | Custom VT100 mode switching |
| `responds_to_cpr` | `RespondsToCpr` | Returns `false` |
| `__getattr__(name)` | Individual method implementations | Dynamic delegation pattern |
| `get_default_color_depth()` | `GetDefaultColorDepth()` | Returns TrueColor by default |

**Python Delegation Rules** (from `__getattr__`):
```python
if name in ("get_size", "get_rows_below_cursor_position",
            "scroll_buffer_to_prompt", "get_win32_screen_buffer_info"):
    return getattr(self.win32_output, name)
else:
    return getattr(self.vt100_output, name)
```

**Function: `is_win_vt100_enabled()`**

| Python API | C# Equivalent | Location |
|------------|---------------|----------|
| `is_win_vt100_enabled()` | `WindowsVt100Support.IsVt100Enabled()` | New static class |

**Decision**: All Python APIs identified. C# will use explicit method implementations (no `__getattr__` equivalent).

---

### 5. GetWin32ScreenBufferInfo Requirement

**Question**: Spec mentions `GetWin32ScreenBufferInfo` - verify this exists.

**Finding**: Not currently in IOutput interface. Checking Win32Output...

`Win32Output` does not expose `GetWin32ScreenBufferInfo` as a public method. The Python `get_win32_screen_buffer_info` is used internally.

**Options**:
1. Add to IOutput interface (breaking change, affects all implementations)
2. Expose on Windows10Output directly (doesn't satisfy IOutput contract)
3. Mark as implementation detail (matches Python behavior)

**Decision**: The Python `get_win32_screen_buffer_info` is used internally by Win32Output for sizing/cursor operations. Since it's not part of the public `Output` base class in Python (only used via `__getattr__`), we don't need to add it to IOutput. If needed, callers can cast to `Windows10Output` and access `Win32Output` property.

---

## Alternatives Considered

### Console Handle Storage

**Option A**: Store handle in constructor (like Python)
```csharp
private readonly nint _hconsole;

public Windows10Output(...)
{
    _hconsole = ConsoleApi.GetStdHandle(ConsoleApi.STD_OUTPUT_HANDLE);
}
```

**Option B**: Get handle fresh each flush
```csharp
public void Flush()
{
    var handle = ConsoleApi.GetStdHandle(ConsoleApi.STD_OUTPUT_HANDLE);
    // ...
}
```

**Decision**: Option A (matches Python) - store handle in constructor for efficiency.

### Thread Safety Scope

**Option A**: Per-instance lock (as specified)
**Option B**: Global lock for all Windows10Output instances
**Option C**: No lock (delegate to underlying outputs)

**Decision**: Option A per spec clarification. Each instance has its own lock to serialize its flush operations without blocking other instances.

---

## Summary

All research tasks complete. No NEEDS CLARIFICATION items remain.

| Item | Resolution |
|------|------------|
| Console mode constants | Use existing `ConsoleApi` constants |
| VT100 detection | Delegate to `PlatformUtils.IsWindowsVt100Supported` |
| Proxy pattern | Follow `ConEmuOutput` design |
| Python API coverage | All APIs mapped to C# equivalents |
| GetWin32ScreenBufferInfo | Internal detail, expose Win32Output property |
| Console handle | Store in constructor |
| Thread safety | Per-instance Lock |
