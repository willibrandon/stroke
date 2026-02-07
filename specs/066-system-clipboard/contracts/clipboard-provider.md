# Contract: Clipboard Providers

**Feature**: 066-system-clipboard
**Date**: 2026-02-07

## Internal API

### IClipboardProvider Interface

```csharp
namespace Stroke.Clipboard;

/// <summary>
/// Internal interface for platform-specific clipboard text I/O.
/// </summary>
/// <remarks>
/// Implementations handle the mechanics of reading from and writing to the
/// operating system's clipboard. Each platform has a dedicated provider.
/// Implementations should be stateless and thread-safe by design.
/// </remarks>
internal interface IClipboardProvider
{
    /// <summary>
    /// Write text to the operating system clipboard.
    /// </summary>
    /// <param name="text">The text to write.</param>
    void SetText(string text);

    /// <summary>
    /// Read text from the operating system clipboard.
    /// </summary>
    /// <returns>The clipboard text, or an empty string if the clipboard is empty or unreadable.</returns>
    string GetText();
}
```

### WindowsClipboardProvider

```csharp
namespace Stroke.Clipboard;

/// <summary>
/// Clipboard provider for Windows using Win32 API (P/Invoke).
/// </summary>
/// <remarks>
/// <para>
/// Uses <c>OpenClipboard</c>, <c>GetClipboardData</c>, <c>SetClipboardData</c>,
/// and related Win32 functions with <c>CF_UNICODETEXT</c> format.
/// </para>
/// <para>
/// <b>Thread Safety:</b> This class is stateless and inherently thread-safe.
/// The Win32 clipboard API serializes access via <c>OpenClipboard</c>/<c>CloseClipboard</c>.
/// </para>
/// </remarks>
[SupportedOSPlatform("windows")]
internal sealed class WindowsClipboardProvider : IClipboardProvider
{
    public void SetText(string text);
    public string GetText();
}
```

**SetText behavior**:
1. Call `OpenClipboard(IntPtr.Zero)` — fails silently if clipboard is locked
2. Call `EmptyClipboard()`
3. Allocate global memory with `GlobalAlloc(GMEM_MOVEABLE, ...)`
4. Copy text bytes (UTF-16) into locked global memory
5. Call `SetClipboardData(CF_UNICODETEXT, hGlobal)`
6. Call `CloseClipboard()` in finally block

**GetText behavior**:
1. Call `OpenClipboard(IntPtr.Zero)` — returns empty string if clipboard is locked
2. Call `GetClipboardData(CF_UNICODETEXT)` — returns empty string if no text data
3. Lock memory and marshal UTF-16 string
4. Call `CloseClipboard()` in finally block

### MacOsClipboardProvider

```csharp
namespace Stroke.Clipboard;

/// <summary>
/// Clipboard provider for macOS using pbcopy/pbpaste.
/// </summary>
/// <remarks>
/// <b>Thread Safety:</b> This class is stateless and inherently thread-safe.
/// </remarks>
[SupportedOSPlatform("macos")]
internal sealed class MacOsClipboardProvider : IClipboardProvider
{
    public void SetText(string text);
    public string GetText();
}
```

**SetText**: Launches `pbcopy`, writes text to stdin, waits up to 5 seconds.
**GetText**: Launches `pbpaste`, reads stdout, waits up to 5 seconds.

### LinuxClipboardProvider

```csharp
namespace Stroke.Clipboard;

/// <summary>
/// Clipboard provider for Linux using wl-copy/wl-paste, xclip, or xsel.
/// </summary>
/// <remarks>
/// <para>
/// The specific tool and arguments are determined at construction time by
/// <see cref="ClipboardProviderDetector"/>. The provider is stateless after construction.
/// </para>
/// <para>
/// <b>Thread Safety:</b> This class is stateless and inherently thread-safe.
/// </para>
/// </remarks>
internal sealed class LinuxClipboardProvider : IClipboardProvider
{
    /// <summary>
    /// Initializes a new instance with the specified clipboard tool configuration.
    /// </summary>
    /// <param name="copyCommand">The command name for clipboard write (e.g., "xclip").</param>
    /// <param name="copyArgs">Arguments for the copy command (e.g., ["-selection", "clipboard"]).</param>
    /// <param name="pasteCommand">The command name for clipboard read (e.g., "xclip").</param>
    /// <param name="pasteArgs">Arguments for the paste command (e.g., ["-selection", "clipboard", "-o"]).</param>
    public LinuxClipboardProvider(
        string copyCommand,
        IReadOnlyList<string> copyArgs,
        string pasteCommand,
        IReadOnlyList<string> pasteArgs);

    public void SetText(string text);
    public string GetText();
}
```

### WslClipboardProvider

```csharp
namespace Stroke.Clipboard;

/// <summary>
/// Clipboard provider for Windows Subsystem for Linux using clip.exe/powershell.exe.
/// </summary>
/// <remarks>
/// <para>
/// Write uses <c>clip.exe</c> (stdin). Read uses <c>powershell.exe -NoProfile -Command Get-Clipboard</c>
/// (stdout, with trailing CRLF stripped).
/// </para>
/// <para>
/// <b>Thread Safety:</b> This class is stateless and inherently thread-safe.
/// </para>
/// </remarks>
internal sealed class WslClipboardProvider : IClipboardProvider
{
    public void SetText(string text);
    public string GetText();
}
```

### ClipboardProviderDetector

```csharp
namespace Stroke.Clipboard;

/// <summary>
/// Detects the current platform and instantiates the appropriate clipboard provider.
/// </summary>
/// <remarks>
/// <para>
/// Detection order: Windows → macOS → WSL → Linux Wayland → Linux X11 (xclip → xsel).
/// </para>
/// <para>
/// <b>Thread Safety:</b> This class is stateless (static methods only) and inherently thread-safe.
/// </para>
/// </remarks>
internal static class ClipboardProviderDetector
{
    /// <summary>
    /// Detect the current platform and return the appropriate clipboard provider.
    /// </summary>
    /// <returns>A platform-appropriate <see cref="IClipboardProvider"/>.</returns>
    /// <exception cref="ClipboardProviderNotAvailableException">
    /// Thrown when no clipboard mechanism is available. The exception message includes
    /// platform-specific installation guidance.
    /// </exception>
    public static IClipboardProvider Detect();
}
```

**Detection algorithm**:
1. If `OperatingSystem.IsWindows()` → return `new WindowsClipboardProvider()`
2. If `OperatingSystem.IsMacOS()` → return `new MacOsClipboardProvider()`
3. If `PlatformUtils.IsLinux`:
   a. If `PlatformUtils.IsWsl` → verify `clip.exe` and `powershell.exe` accessible → return `new WslClipboardProvider()`; if not accessible, throw with WSL-specific guidance
   b. If `WAYLAND_DISPLAY` env var is set AND `wl-copy`/`wl-paste` found → return `new LinuxClipboardProvider(wayland config)`
   c. If `xclip` found → return `new LinuxClipboardProvider(xclip config)`
   d. If `xsel` found → return `new LinuxClipboardProvider(xsel config)`
4. Throw `ClipboardProviderNotAvailableException` with installation guidance

**Tool detection**: Uses `Process.Start("which", toolName)` (Unix) or `Process.Start("where", toolName)` (Windows) with 2-second timeout.

### StringClipboardProvider

```csharp
namespace Stroke.Clipboard;

/// <summary>
/// Clipboard provider backed by a string field for testing.
/// </summary>
/// <remarks>
/// <para>
/// This is a real <see cref="IClipboardProvider"/> implementation, not a mock.
/// It stores clipboard text in a simple string field, enabling testing of
/// <see cref="SystemClipboard"/> behavior without OS clipboard access.
/// </para>
/// <para>
/// <b>Thread Safety:</b> This class is thread-safe. The backing string is
/// synchronized using <see cref="System.Threading.Lock"/>.
/// </para>
/// </remarks>
internal sealed class StringClipboardProvider : IClipboardProvider
{
    /// <summary>
    /// Initializes a new instance with optional initial text.
    /// </summary>
    /// <param name="initialText">Initial clipboard text (default: empty string).</param>
    public StringClipboardProvider(string initialText = "");

    public void SetText(string text);
    public string GetText();
}
```

## Requirement Traceability

| Requirement | Contract Element |
|-------------|-----------------|
| FR-002 | Four platform providers: Windows, macOS, Linux, WSL |
| FR-003 | `ClipboardProviderDetector.Detect()` auto-detects platform |
| FR-009 | Process-based providers use 5-second timeout |
| FR-011 | `ClipboardProviderNotAvailableException` with installation guidance |
| FR-012 | All process providers use `ProcessStartInfo.ArgumentList` |
| FR-013 | Detector checks Wayland before X11, xclip before xsel |
| FR-014 | Detector checks WSL before native Linux |
