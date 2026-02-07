# Data Model: System Clipboard

**Feature**: 066-system-clipboard
**Date**: 2026-02-07

## Entities

### ClipboardData (existing)

Immutable value type representing clipboard content with text and selection type.

| Field | Type | Description |
|-------|------|-------------|
| `Text` | `string` | Clipboard text content (default: `""`) |
| `Type` | `SelectionType` | Selection type: Characters, Lines, or Block (default: `Characters`) |

**Location**: `src/Stroke/Clipboard/ClipboardData.cs` (already implemented)
**Mutability**: Immutable — no synchronization needed
**Relationships**: Produced by `IClipboard.GetData()`, consumed by `IClipboard.SetData()`

### SystemClipboard (new)

Public `IClipboard` implementation that synchronizes with the OS clipboard via a platform-specific provider.

| Field | Type | Visibility | Description |
|-------|------|------------|-------------|
| `_provider` | `IClipboardProvider` | `private readonly` | Platform-specific clipboard I/O |
| `_lastData` | `ClipboardData?` | `private` | Cached last-written data for SelectionType preservation |
| `_lock` | `Lock` | `private readonly` | Thread safety synchronization |

**Mutability**: Mutable (`_lastData` changes on each `SetData` call)
**Thread Safety**: `System.Threading.Lock` with `EnterScope()` on all public methods
**Relationships**:
- Implements `IClipboard`
- Delegates to `IClipboardProvider` for OS clipboard I/O
- Caches `ClipboardData` for selection type preservation

**State Transitions**:
```
SetData(data) → _lastData = data; provider.SetText(data.Text)
GetData()     → text = provider.GetText()
               if text == _lastData?.Text → return _lastData
               else → infer SelectionType from newline presence
Rotate()      → no-op (OS clipboard has no kill ring)
```

### IClipboardProvider (new)

Internal interface for platform-specific clipboard text I/O.

| Method | Return Type | Description |
|--------|-------------|-------------|
| `SetText(string text)` | `void` | Write text to OS clipboard |
| `GetText()` | `string` | Read text from OS clipboard |

**Visibility**: `internal` (not part of public API)
**Mutability**: Implementations are stateless
**Relationships**: Used by `SystemClipboard`; implemented by all platform providers

### WindowsClipboardProvider (new)

Win32 API provider using P/Invoke for clipboard access.

| Component | Description |
|-----------|-------------|
| Nested `ClipboardApi` class | Contains `LibraryImport` declarations for `OpenClipboard`, `CloseClipboard`, `EmptyClipboard`, `GetClipboardData`, `SetClipboardData`, `GlobalAlloc`, `GlobalLock`, `GlobalUnlock`, `GlobalFree` |
| `CF_UNICODETEXT = 13` | Unicode text clipboard format |
| `GMEM_MOVEABLE = 0x0002` | Global memory allocation flag |

**Platform Guard**: `[SupportedOSPlatform("windows")]`
**Mutability**: Stateless
**Error Handling**: Catches all exceptions; returns empty string on read failure, swallows write failure

### MacOsClipboardProvider (new)

Process-based provider using `pbcopy` (write) and `pbpaste` (read).

| Operation | Command | Stdin/Stdout |
|-----------|---------|-------------|
| Write | `pbcopy` | Text written to stdin |
| Read | `pbpaste` | Text read from stdout |

**Mutability**: Stateless
**Error Handling**: 5-second timeout; returns empty string on failure

### LinuxClipboardProvider (new)

Process-based provider with auto-detection of Wayland or X11 tools.

| Tool | Write Command | Read Command | Detection |
|------|--------------|-------------|-----------|
| wl-clipboard | `wl-copy` | `wl-paste --no-newline` | `WAYLAND_DISPLAY` env var set |
| xclip | `xclip -selection clipboard` | `xclip -selection clipboard -o` | `which xclip` succeeds |
| xsel | `xsel --clipboard --input` | `xsel --clipboard --output` | `which xsel` succeeds |

**Constructor**: Accepts tool name and arguments, determined by `ClipboardProviderDetector`
**Mutability**: Stateless (tool path and arguments are readonly)
**Error Handling**: 5-second timeout; returns empty string on failure

### WslClipboardProvider (new)

Process-based provider for Windows Subsystem for Linux.

| Operation | Command | Stdin/Stdout |
|-----------|---------|-------------|
| Write | `clip.exe` | Text written to stdin |
| Read | `powershell.exe -NoProfile -Command Get-Clipboard` | Text read from stdout (trailing CRLF stripped) |

**Mutability**: Stateless
**Error Handling**: 5-second timeout; returns empty string on failure

### ClipboardProviderDetector (new)

Static class that detects the current platform and instantiates the appropriate provider.

| Method | Return Type | Description |
|--------|-------------|-------------|
| `Detect()` | `IClipboardProvider` | Returns platform-appropriate provider; throws if none available |

**Detection Order**:
1. `PlatformUtils.IsWindows` → `WindowsClipboardProvider`
2. `PlatformUtils.IsMacOS` → `MacOsClipboardProvider`
3. `PlatformUtils.IsWsl` → `WslClipboardProvider` (verify `clip.exe`/`powershell.exe` accessible)
4. `WAYLAND_DISPLAY` env var → `LinuxClipboardProvider` with wl-copy/wl-paste (verify tools exist)
5. `which xclip` → `LinuxClipboardProvider` with xclip
6. `which xsel` → `LinuxClipboardProvider` with xsel
7. None found → throw `ClipboardProviderNotAvailableException` with installation guidance

**Mutability**: Stateless (static methods only)

### StringClipboardProvider (new)

Test-only provider backed by a `string` field. Real implementation, not a mock.

| Field | Type | Description |
|-------|------|-------------|
| `_text` | `string` | Backing clipboard text |
| `_lock` | `Lock` | Thread safety |

**Mutability**: Mutable (protected by Lock)
**Usage**: Unit testing `SystemClipboard` behavior without OS clipboard access

## Entity Relationships

```
IClipboard (interface)
├── InMemoryClipboard (existing, kill ring)
├── DummyClipboard (existing, no-op)
├── DynamicClipboard (existing, runtime delegation)
└── SystemClipboard (NEW, OS clipboard)
    └── uses IClipboardProvider (internal)
        ├── WindowsClipboardProvider (Win32 P/Invoke)
        ├── MacOsClipboardProvider (pbcopy/pbpaste)
        ├── LinuxClipboardProvider (wl-copy/xclip/xsel)
        ├── WslClipboardProvider (clip.exe/powershell.exe)
        └── StringClipboardProvider (test backing)

ClipboardProviderDetector (static)
    └── creates IClipboardProvider based on platform
```

## Validation Rules

| Rule | Source | Enforcement |
|------|--------|-------------|
| `SetData(data)` must not accept null | FR-004 (matches IClipboard contract) | `ArgumentNullException.ThrowIfNull(data)` |
| `SetText(text)` must not accept null | FR-004 (matches IClipboard contract) | `ArgumentNullException.ThrowIfNull(text)` |
| Process timeout must be 5 seconds | FR-009 | `CancellationTokenSource(TimeSpan.FromSeconds(5))` |
| Write failures silently swallowed | FR-008 | `try/catch` around provider.SetText |
| Read failures return empty text | FR-008 | `try/catch` returning `""` |
| No command injection | FR-012 | `ProcessStartInfo.ArgumentList` (not `Arguments`) |
