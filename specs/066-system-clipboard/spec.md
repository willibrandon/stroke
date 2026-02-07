# Feature Specification: System Clipboard

**Feature Branch**: `066-system-clipboard`
**Created**: 2026-02-07
**Status**: Draft
**Input**: User description: "Cross-platform system clipboard integration that enables Stroke applications to read from and write to the operating system's clipboard, faithfully porting Python Prompt Toolkit's PyperclipClipboard class."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Copy and Paste Between Stroke and External Applications (Priority: P1)

A developer building a REPL or database shell with Stroke wants their users to paste text copied from a web browser, text editor, or other application into the Stroke prompt, and copy text from the Stroke prompt to paste elsewhere. The user selects text in their browser, copies it (Cmd-C/Ctrl-C), switches to the terminal running the Stroke application, and presses Ctrl-Y (Emacs yank) to paste the text at the cursor. Similarly, the user selects text in the Stroke prompt with Ctrl-W (Emacs kill-word) and pastes it into another application.

**Why this priority**: This is the core value proposition. Without bidirectional clipboard interop, users are stuck manually retyping text between applications, which is the primary pain point this feature solves.

**Independent Test**: Can be fully tested by launching a Stroke prompt with system clipboard enabled, copying text externally, pasting it into the prompt with Ctrl-Y, and verifying the text appears. Delivers immediate clipboard interop value.

**Acceptance Scenarios**:

1. **Given** a Stroke prompt is running with system clipboard enabled and the user has copied "hello world" from an external application, **When** the user presses Ctrl-Y, **Then** "hello world" appears at the cursor position in the Stroke prompt.
2. **Given** a Stroke prompt is running with system clipboard enabled and the user types "some text" then selects it with Ctrl-W, **When** the user switches to an external application and pastes, **Then** "some text" appears in the external application.
3. **Given** a Stroke prompt is running with system clipboard enabled, **When** the user copies multi-line text from an external application and presses Ctrl-Y, **Then** all lines of text appear correctly in the Stroke prompt.

---

### User Story 2 - Opt-In Clipboard Activation (Priority: P2)

A developer integrating Stroke into their application wants to enable OS clipboard support by passing a single configuration option. The default clipboard behavior (in-memory only) remains unchanged for applications that don't need system clipboard integration. The developer creates a `SystemClipboard` instance and passes it to `PromptSession` or `Prompt.RunPrompt`.

**Why this priority**: Developers need a clear, simple API to enable system clipboard. Without this, even if the clipboard backend works, developers can't access it.

**Independent Test**: Can be tested by constructing a `SystemClipboard` instance and verifying it implements the clipboard interface, writes to the OS clipboard, and reads back correctly.

**Acceptance Scenarios**:

1. **Given** a developer creates a `SystemClipboard` instance, **When** they pass it to a prompt session, **Then** all clipboard operations (cut, copy, paste) use the OS clipboard.
2. **Given** a developer creates a prompt session without specifying a clipboard, **When** the user performs clipboard operations, **Then** the default in-memory clipboard is used (no OS clipboard interaction).
3. **Given** a developer creates a `SystemClipboard` on a supported platform, **When** they call the set-data method with text, **Then** the text is available in the OS clipboard for other applications to paste.

---

### User Story 3 - Cross-Platform Clipboard Detection (Priority: P3)

A developer deploying their Stroke-based application on Windows, macOS, or Linux wants the system clipboard to work automatically without platform-specific configuration. The clipboard integration detects the platform and selects the appropriate clipboard mechanism (Win32 API, pbcopy/pbpaste, wl-copy/xclip/xsel) at construction time.

**Why this priority**: Cross-platform support is essential for a terminal toolkit, but the detection logic is infrastructure that enables the P1 and P2 stories.

**Independent Test**: Can be tested on each platform by constructing a `SystemClipboard` and verifying a round-trip (write text, read it back).

**Acceptance Scenarios**:

1. **Given** the application is running on macOS, **When** a `SystemClipboard` is created, **Then** clipboard operations use the macOS clipboard (pbcopy/pbpaste).
2. **Given** the application is running on Windows, **When** a `SystemClipboard` is created, **Then** clipboard operations use the Windows clipboard (Win32 API).
3. **Given** the application is running on Linux with a Wayland display server, **When** a `SystemClipboard` is created and wl-copy/wl-paste are installed, **Then** clipboard operations use the Wayland clipboard.
4. **Given** the application is running on Linux with X11, **When** a `SystemClipboard` is created and xclip is installed, **Then** clipboard operations use the X11 clipboard.
5. **Given** the application is running under WSL, **When** a `SystemClipboard` is created and `clip.exe`/`powershell.exe` are accessible, **Then** clipboard operations use the WSL clipboard interop (clip.exe for write, powershell.exe for read).
6. **Given** the application is running on Linux with no clipboard tools installed, **When** a `SystemClipboard` is created, **Then** a `ClipboardProviderNotAvailableException` is thrown with the message: "No clipboard tool found. Install one of: xclip, xsel, wl-clipboard".

---

### User Story 4 - Selection Type Preservation (Priority: P3)

When a user cuts a block of lines in Vi visual-line mode within Stroke, the selection type (lines vs. characters) is preserved for paste operations within the same session. If the user then pastes within Stroke, the text is pasted with the correct line-based semantics. If the clipboard content was changed externally (by another application), the selection type is inferred from the content (lines if newlines present, characters otherwise).

**Why this priority**: Selection type preservation is important for Vi and Emacs editing fidelity but is a refinement of the core clipboard functionality.

**Independent Test**: Can be tested by setting clipboard data with a specific selection type, reading it back, and verifying the type is preserved.

**Acceptance Scenarios**:

1. **Given** text was written to the system clipboard via Stroke with a "lines" selection type, **When** the clipboard text is read back and matches, **Then** the original "lines" selection type is returned.
2. **Given** text was written to the system clipboard by an external application, **When** the clipboard text contains newlines, **Then** the selection type is inferred as "lines".
3. **Given** text was written to the system clipboard by an external application, **When** the clipboard text contains no newlines, **Then** the selection type is inferred as "characters".

---

### Edge Cases

- **Clipboard locked (Windows)**: When the OS clipboard is locked by another application, `OpenClipboard` returns false. The operation fails silently (returns empty on read, swallows on write) per FR-008.
- **Display server not running**: When clipboard tools (xclip, xsel) are installed but the display server is not running, the subprocess will fail at runtime. The operation fails silently per FR-008 (return empty on read, swallow on write).
- **Headless/SSH sessions**: When running in a headless or SSH session with no display server, clipboard provider detection will find no Wayland display (`$WAYLAND_DISPLAY` unset) and no X11 tools usable. Construction throws `ClipboardProviderNotAvailableException` per FR-022.
- **Binary/non-text clipboard**: When the clipboard contains binary data or non-text formats, the system returns an empty string (only CF_UNICODETEXT on Windows, text/plain on others is supported).
- **Large clipboard text**: When the clipboard text is extremely large (e.g., 100MB), process-based providers may be slow but MUST function within the 5-second timeout (FR-009). On timeout, the subprocess is killed and empty/partial results returned.
- **Concurrent thread access**: Operations are serialized via `Lock` per FR-010 to prevent data corruption. Lock is held during provider calls, which may block up to 5 seconds.
- **WSL without interop**: When the WSL environment has no access to `powershell.exe` or `clip.exe`, construction fails with a clear error message per FR-011.
- **Empty string vs null**: Provider `GetText()` MUST return `""` (never `null`) per FR-019. `GetData()` returns `ClipboardData` with empty text and `Characters` type when clipboard is empty.
- **Same text from external app**: When a `SystemClipboard` instance writes text, an external app modifies the clipboard to identical text, then `GetData()` is called — the cached `ClipboardData` is returned (preserving original `SelectionType`). The system cannot distinguish this case from no external modification, per FR-005.
- **Line endings (\r\n vs \n)**: Both `\r\n` and `\n` trigger `SelectionType.Lines` inference since `\r\n` contains `\n` per FR-006. No line ending normalization is performed — text is returned as-is from the OS clipboard.
- **SelectionType.Block from external text**: `Block` is never inferred from external clipboard text per FR-006. Block can only be preserved via the cache when originally set by a Stroke `SetData` call.
- **Broken clipboard tools**: When clipboard tools exist on PATH but are broken (e.g., xclip segfaults), the subprocess will exit with a non-zero exit code or crash. This is handled by FR-008's silent failure semantics (return empty on read, swallow on write). Detection only checks tool existence, not functionality.
- **Unicode edge cases**: Surrogate pairs, combining characters, and zero-width characters are passed through transparently — clipboard operations treat text as opaque strings with no character-level processing. Win32 uses UTF-16 (CF_UNICODETEXT) which handles surrogate pairs natively; process-based providers use UTF-8 encoding.
- **Missing `which` command**: When the `which` command is not available (some minimal Docker images), tool detection will fail (the detection process itself returns non-zero or throws). This is treated as "tool not found" and detection continues to the next option or throws per FR-011.
- **Broken clipboard tools on PATH**: When tools are found during detection but fail at runtime (version incompatibility, library issues), FR-008's runtime failure handling applies.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST provide a clipboard implementation that reads from and writes to the operating system's clipboard
- **FR-002**: System MUST support all five platform variants: (1) Windows (Win32 API), (2) macOS (pbcopy/pbpaste), (3) Linux Wayland (wl-copy/wl-paste), (4) Linux X11 (xclip or xsel), and (5) WSL (clip.exe/powershell.exe)
- **FR-003**: System MUST automatically detect the current platform and select the appropriate clipboard provider (the internal platform-specific component responsible for OS clipboard text I/O) during the `SystemClipboard` constructor — not via lazy initialization. On an unsupported or unknown OS (e.g., FreeBSD), the constructor MUST throw `ClipboardProviderNotAvailableException` with a message indicating the OS is not supported
- **FR-004**: System MUST implement the existing clipboard interface (`IClipboard`) as a drop-in replacement for the in-memory clipboard
- **FR-005**: System MUST preserve the selection type (lines, characters, block) when the clipboard text matches the last text written by the same instance (using text value equality per FR-021). When the clipboard text matches but was written externally with the same content, the cached `ClipboardData` is still returned (the system cannot distinguish this case)
- **FR-006**: System MUST infer selection type from external clipboard content: `SelectionType.Lines` when text contains any newline character (`\n` — which also covers `\r\n` since `\r\n` contains `\n`), `SelectionType.Characters` otherwise. `SelectionType.Block` is NEVER inferred from external text — it can only be preserved via the FR-005 cache when originally set by a Stroke operation. This matches Python's `"\n" in text` check mapped to C#'s `text.Contains('\n')`
- **FR-007**: System MUST remain opt-in — the default clipboard for prompt sessions MUST remain in-memory to match Python Prompt Toolkit's default behavior
- **FR-008**: System MUST handle clipboard operation failures gracefully. "Silently swallowed" for write failures means: no exception is propagated to the caller, no special return value, and no logging (best-effort fire-and-forget). This includes partial failures where some Win32 API calls succeed and others fail (e.g., `GlobalAlloc` succeeds but `SetClipboardData` fails — the provider must clean up allocated memory and swallow the error). For read failures, `GetData()` MUST return an empty `ClipboardData` (empty string, `Characters` type)
- **FR-009**: System MUST enforce a 5-second wall-clock timeout on all subprocess-based clipboard operations. On timeout, the subprocess MUST be killed (`Process.Kill()`). For read timeouts, the provider MUST return an empty string. For write timeouts, the failure is silently swallowed per FR-008
- **FR-010**: System MUST be thread-safe — all public operations on the system clipboard MUST be synchronized using `System.Threading.Lock` with `EnterScope()`. Each public method (`SetData`, `GetData`, `SetText`, `Rotate`) MUST acquire the lock for its entire duration, making compound operations (e.g., SetData's cache-then-write) atomic. The lock protects the `_lastData` cache; providers themselves are stateless and inherently thread-safe. Note: if a provider call blocks (up to 5 seconds per FR-009), the lock is held for that duration — this is an acceptable tradeoff since clipboard operations are infrequent. Thread safety guarantees MUST be documented in XML documentation comments per Constitution XI
- **FR-011**: System MUST throw `ClipboardProviderNotAvailableException` with platform-specific installation guidance when no clipboard provider is available. Concrete messages: (1) Linux with no tools: `"No clipboard tool found. Install one of: xclip, xsel, wl-clipboard"`, (2) WSL with missing interop: `"clip.exe or powershell.exe not accessible in WSL environment"`, (3) Unsupported OS: `"Clipboard is not supported on this platform"`
- **FR-012**: System MUST prevent command injection — process-based providers MUST use `ProcessStartInfo.ArgumentList` (not the `Arguments` string property) to pass arguments to subprocess clipboard tools
- **FR-013**: On Linux, the system MUST detect clipboard tools in priority order: Wayland (wl-copy/wl-paste) before X11 (xclip before xsel)
- **FR-014**: On Linux, the system MUST check for WSL before native Linux clipboard tools, using WSL-specific mechanisms (clip.exe/powershell.exe) when running under Windows Subsystem for Linux. WSL detection MUST read `/proc/version` and check for a case-insensitive match of "microsoft" (covers both WSL1 and WSL2). This detection MUST be exposed as a `PlatformUtils.IsWsl` property
- **FR-015**: System MUST support the kill ring "rotate" operation as a no-op, since OS clipboards have no kill ring concept. This matches Python Prompt Toolkit's inherited no-op from the base `Clipboard` class (not a new Stroke invention)
- **FR-016**: All subprocess-based providers (macOS, Linux, WSL) MUST set `ProcessStartInfo.UseShellExecute = false` to prevent shell interpretation of arguments
- **FR-017**: All subprocess-based providers (macOS, Linux, WSL) MUST set `ProcessStartInfo.CreateNoWindow = true` to prevent visible console windows during clipboard operations
- **FR-018**: The WSL provider MUST use `powershell.exe -NoProfile -Command Get-Clipboard` for reads, with `-NoProfile` preventing execution of user profile scripts for both security and performance. The WSL provider MUST strip trailing CRLF from PowerShell output and handle UTF-16 BOM encoding by reading output as UTF-8 (PowerShell writes UTF-8 to piped stdout)
- **FR-019**: All provider `GetText()` implementations MUST return a `string` (never `null`). An empty or unreadable clipboard MUST return an empty string `""`
- **FR-020**: `SetData` MUST cache the `ClipboardData` in `_lastData` BEFORE attempting the provider write, matching Python's `self._data = data` before `pyperclip.copy(data.text)` ordering
- **FR-021**: `GetData` MUST compare clipboard text using value equality (`text == _lastData.Text`), not reference equality, matching Python's `self._data.text == text` behavior
- **FR-022**: When running in a headless or SSH session with no display server, clipboard provider detection MUST throw `ClipboardProviderNotAvailableException` at construction time (since no Wayland/X11 display is available and no tools will be detected). Process-based operations that encounter a missing display at runtime (e.g., `xclip` failing because `$DISPLAY` was unset after construction) are handled by FR-008's silent failure semantics
- **FR-023**: The Wayland paste command MUST use `wl-paste --no-newline` to prevent wl-paste from appending an extra trailing newline to clipboard content

### Key Entities

- **SystemClipboard** (public, `IClipboard`): The public clipboard implementation that synchronizes with the OS clipboard. Wraps a platform-specific `IClipboardProvider` and adds selection type semantics (preservation and inference). Thread-safe via `Lock`.
- **IClipboardProvider** (internal interface): The internal abstraction for platform-specific clipboard text I/O (`SetText`/`GetText`). Implementations are stateless and inherently thread-safe. Each platform has its own provider: `WindowsClipboardProvider` (Win32 API), `MacOsClipboardProvider` (pbcopy/pbpaste), `LinuxClipboardProvider` (wl-copy/xclip/xsel), `WslClipboardProvider` (clip.exe/powershell.exe), `StringClipboardProvider` (test backing).
- **ClipboardProviderDetector** (internal static class): Detects the current platform and instantiates the appropriate `IClipboardProvider`. Detection order: Windows → macOS → WSL → Linux Wayland → Linux X11 (xclip → xsel). Throws `ClipboardProviderNotAvailableException` if no provider can be created.
- **ClipboardData** (existing, immutable): Existing entity representing clipboard content with text and selection type. Used by `SystemClipboard` to preserve selection semantics across clipboard operations.
- **ClipboardProviderNotAvailableException** (public): Exception thrown at construction time when no clipboard provider is available. Contains platform-specific installation guidance in its message.

### Security Considerations

- **Command injection prevention**: All subprocess providers use `ProcessStartInfo.ArgumentList` (FR-012), `UseShellExecute = false` (FR-016), and `CreateNoWindow = true` (FR-017). These three requirements together prevent shell interpretation of clipboard text as commands.
- **Text sanitization**: Clipboard text is NOT sanitized or validated before writing to the OS clipboard. This matches the behavior of all native clipboard APIs — the clipboard is a transparent data channel. Sanitization is the responsibility of the receiving application when pasting. Stroke does not add, remove, or modify clipboard content.
- **Win32 memory management**: The Windows provider MUST follow the Win32 clipboard ownership protocol: `GlobalAlloc(GMEM_MOVEABLE, ...)` → `GlobalLock` → copy data → `GlobalUnlock` → `SetClipboardData`. After a successful `SetClipboardData`, the system owns the memory (do NOT call `GlobalFree`). On failure before `SetClipboardData`, `GlobalFree` MUST be called to prevent leaks.
- **Win32 clipboard lifecycle**: `CloseClipboard()` MUST be called in a `finally` block to ensure the clipboard is released even when intermediate Win32 API calls (`EmptyClipboard`, `GlobalAlloc`, `SetClipboardData`) fail.
- **PowerShell security**: WSL reads use `powershell.exe -NoProfile` (FR-018) to prevent execution of user profile scripts that could modify output or introduce side effects.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Clipboard text written by `SystemClipboard.SetText()` is readable by the OS clipboard API (verified by reading back via the same provider), and text written to the OS clipboard externally is readable by `SystemClipboard.GetData()`, on all five platform variants defined in FR-002
- **SC-002**: Clipboard round-trip (write then read) returns identical text on all supported platforms, verified by unit tests using `StringClipboardProvider` and manual validation on at least macOS (the development platform)
- **SC-003**: Selection type is correctly preserved when clipboard text matches the last write, and correctly inferred (`Lines` when text contains `\n`, `Characters` otherwise) when clipboard was modified externally — verified by unit tests
- **SC-004**: Clipboard operations complete within 5 seconds. Timeout behavior is verified by a unit test using a `StringClipboardProvider` that simulates a slow provider (thread sleep) and confirming the operation returns within the timeout window
- **SC-005**: The system clipboard can be enabled with a single constructor call (`new SystemClipboard()`) and passed to any prompt session — no additional configuration required
- **SC-006**: Applications that do not opt into system clipboard experience zero observable behavior change: no new code paths are executed, no new exceptions are thrown, and the default `InMemoryClipboard` remains the default. Verified by confirming no existing tests change behavior
- **SC-007**: Line coverage for the `Stroke.Clipboard` namespace's new system clipboard components (`SystemClipboard`, all providers, `ClipboardProviderDetector`, `ClipboardProviderNotAvailableException`) reaches 80% or higher, measured at the file level
- **SC-008**: Concurrent stress tests with 10+ threads and 1000+ operations demonstrate no data corruption, deadlocks, or exceptions from the `SystemClipboard` class (per Constitution XI)

### Assumptions

- macOS systems have `pbcopy` and `pbpaste` available — they have shipped with every macOS release since Mac OS X 10.0 (2001). No minimum macOS version requirement beyond what .NET 10 already requires
- Windows systems have the Win32 clipboard API available (standard on all supported Windows versions; Win32 clipboard API predates Windows 95)
- Linux users are expected to install clipboard tools (xclip, xsel, or wl-clipboard) themselves; the system provides a clear error message with installation guidance if none are found
- WSL environments (both WSL1 and WSL2) have access to `clip.exe` and `powershell.exe` via the Windows interop layer — this is a standard feature of WSL's Windows filesystem interop (`/mnt/c/Windows/System32/` is on PATH by default). Both WSL1 and WSL2 are in scope
- The default clipboard for `PromptSession` remains `InMemoryClipboard` to match Python Prompt Toolkit's default — `SystemClipboard` is always opt-in
- The `which` command is available on Linux systems for clipboard tool detection. On minimal systems lacking `which`, detection falls through to "not found" behavior (FR-011). This is acceptable since such minimal systems (e.g., bare Docker images) typically also lack clipboard tools

### Dependencies

- **PlatformUtils.IsWsl** (new property): A new `bool` property must be added to `Stroke.Core.PlatformUtils` that reads `/proc/version` and checks for case-insensitive "microsoft". This is a cross-file modification to an existing class in the Core layer
- **Existing IClipboard interface**: `SystemClipboard` implements the existing `IClipboard` interface from `Stroke.Clipboard` — no modifications to the interface are needed
- **Existing ClipboardData class**: Used as-is for `SetData`/`GetData` operations — no modifications needed
- **.NET BCL only**: No external NuGet packages required — uses `System.Diagnostics.Process`, `System.Runtime.InteropServices`, `System.Threading.Lock`

### Deviations from Python Prompt Toolkit

| Deviation | Rationale |
|-----------|-----------|
| Inline platform providers instead of delegating to `pyperclip` third-party library | No .NET equivalent of pyperclip exists; inlining providers avoids an unnecessary dependency and gives full control over platform-specific behavior |
| `System.Threading.Lock` synchronization on `SystemClipboard` | Python assumes single-threaded execution; .NET commonly operates in multi-threaded contexts — defensive thread safety per Constitution XI |
| Win32 P/Invoke instead of subprocess-based `clip.exe` on Windows | Direct Win32 API access is more efficient and reliable than spawning a subprocess, and avoids command injection surface |
| `ClipboardProviderNotAvailableException` is a specific exception type | Python's pyperclip raises generic `PyperclipException`; a specific exception type follows .NET conventions for catch filtering |
| `_lastData` comparison uses C# string equality | Maps directly from Python's `self._data.text == text`; C#'s `==` on strings performs value equality (not reference equality), matching Python's behavior |
