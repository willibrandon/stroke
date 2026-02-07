# Research: System Clipboard

**Feature**: 066-system-clipboard
**Date**: 2026-02-07

## Research Tasks

### R1: Python PyperclipClipboard Implementation

**Decision**: Port `PyperclipClipboard` as `SystemClipboard` with inlined platform providers.

**Rationale**: Python's `PyperclipClipboard` is a thin wrapper around the `pyperclip` third-party library. The class itself has minimal logic:
- Caches last-written `ClipboardData` in `_data`
- On `get_data()`: reads OS clipboard via `pyperclip.paste()`, compares text to cache, returns cached data if matching (preserving SelectionType), otherwise infers SelectionType from newline presence
- On `set_data()`: stores data in `_data` cache, writes text to OS clipboard via `pyperclip.copy()`
- `rotate()`: inherited no-op from base class

The `pyperclip` library handles platform detection and provides `copy()`/`paste()` functions. Since no .NET equivalent exists, Stroke inlines this logic via `IClipboardProvider` + `ClipboardProviderDetector`.

**Alternatives considered**:
- **TextCopy NuGet package**: Third-party .NET clipboard library. Rejected because: (a) adds external dependency, (b) doesn't match pyperclip's exact platform detection order, (c) Constitution I requires faithful port of pyperclip's behavior.
- **Shelling out to pyperclip itself**: Not viable — would require Python runtime.

### R2: Win32 Clipboard P/Invoke Strategy

**Decision**: Use Win32 API (OpenClipboard, GetClipboardData, SetClipboardData) via `LibraryImport` P/Invoke in a private nested class within `WindowsClipboardProvider`.

**Rationale**: The Win32 clipboard API is the standard Windows mechanism. Python's `pyperclip` library uses `ctypes` to call these same APIs. The nested class pattern matches the existing `PlatformUtils.Vt100Detection` pattern, keeping P/Invoke declarations scoped to the provider that needs them. This avoids depending on `Stroke.Input.Windows.ConsoleApi` (which would violate Architecture layer III — clipboard is in Core, ConsoleApi is in Input).

**Key APIs needed**:
- `OpenClipboard(nint hWndNewOwner)` — opens clipboard for access
- `CloseClipboard()` — releases clipboard
- `EmptyClipboard()` — clears clipboard before writing
- `GetClipboardData(uint uFormat)` — reads clipboard in specified format
- `SetClipboardData(uint uFormat, nint hMem)` — writes clipboard data
- `GlobalAlloc(uint uFlags, nuint dwBytes)` / `GlobalLock` / `GlobalUnlock` / `GlobalFree` — memory management for clipboard data
- `CF_UNICODETEXT = 13` — Unicode text format constant

**Alternatives considered**:
- **Reuse ConsoleApi.cs**: Would create circular dependency (Clipboard → Input). Rejected.
- **Shared P/Invoke project**: Over-engineering for 8 function declarations. Rejected.

### R3: Process-Based Provider Safety

**Decision**: Use `ProcessStartInfo.ArgumentList` (not `Arguments` string) with `UseShellExecute = false` and `CreateNoWindow = true`. Enforce 5-second timeout via `process.WaitForExitAsync()` with `CancellationTokenSource`.

**Rationale**: Matches the existing Stroke pattern in `Application.Lifecycle.cs` (`RunSystemCommandAsync`) which uses `ArgumentList` for safe argument passing. The `ArgumentList` property bypasses shell interpretation entirely, preventing command injection (FR-012). The 5-second timeout (FR-009) prevents indefinite blocking when clipboard tools hang.

**Implementation pattern**:
```
ProcessStartInfo:
  FileName = tool path (e.g., "pbcopy")
  ArgumentList = arguments (if any, e.g., "-selection clipboard" for xclip)
  UseShellExecute = false
  CreateNoWindow = true
  RedirectStandardInput = true   (for write operations)
  RedirectStandardOutput = true  (for read operations)
  RedirectStandardError = true   (suppress error output)

Timeout:
  using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
  await process.WaitForExitAsync(cts.Token);
```

**Error handling**: Catch all exceptions, return empty string on read failure, swallow on write failure (FR-008).

### R4: Platform Detection Order

**Decision**: Detection order: Windows → macOS → WSL → Linux Wayland → Linux X11 (xclip → xsel).

**Rationale**: Matches `pyperclip`'s detection logic:
1. **Windows** first (native Win32 API, most reliable)
2. **macOS** next (pbcopy/pbpaste always available)
3. **WSL before native Linux** (FR-014) — WSL reports as Linux via `RuntimeInformation` but should use Windows clipboard via `clip.exe`/`powershell.exe`
4. **Linux Wayland** before X11 (FR-013) — Wayland is the modern default; `WAYLAND_DISPLAY` env var indicates Wayland session
5. **Linux X11** last — `xclip` preferred over `xsel` (matches pyperclip preference)

**WSL Detection**: Read `/proc/version` and check for case-insensitive "microsoft" or "WSL". This is the standard WSL detection method used by pyperclip and other cross-platform tools.

### R5: StringClipboardProvider (Test Provider)

**Decision**: Create `StringClipboardProvider` — a real `IClipboardProvider` backed by a `string` field. Not a mock.

**Rationale**: Constitution VIII forbids mocks, fakes, and test doubles. `StringClipboardProvider` is a genuine `IClipboardProvider` implementation that stores text in a string field. It enables testing `SystemClipboard` behavior (selection type preservation, inference, thread safety) without requiring OS clipboard access. It's the same approach used by Python's test suite, which substitutes pyperclip's copy/paste functions with in-memory lambdas.

**Thread safety**: Uses `Lock` for the backing string field, making it suitable for concurrent tests.

### R6: Existing Stroke Patterns

**Decision**: Follow established patterns from the Stroke codebase.

**Findings**:
- **Thread safety**: Use `System.Threading.Lock` with `EnterScope()` pattern (matches `InMemoryClipboard`, `Buffer`, `StdoutProxy`)
- **Platform detection**: Use `RuntimeInformation.IsOSPlatform()` and `OperatingSystem.Is*()` (matches `PlatformUtils`)
- **P/Invoke**: Use `[LibraryImport]` with `EntryPoint` and `SetLastError` in a nested class (matches `PlatformUtils.Vt100Detection`)
- **Error handling**: Silent catch for non-critical failures (matches `Application.RunSystemCommandAsync`, `Buffer.OpenFileInEditor`)
- **Process execution**: Use `ProcessStartInfo.ArgumentList` (matches `Application.Lifecycle.cs`)
- **XML docs**: Required on all public types and members
- **Sealed classes**: All concrete implementations are `sealed`
- **Null validation**: Use `ArgumentNullException.ThrowIfNull()` (matches `InMemoryClipboard`)
