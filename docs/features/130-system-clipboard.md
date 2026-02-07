# Feature 130: System Clipboard

## Overview

Implement cross-platform system clipboard integration that enables Stroke applications to read from and write to the operating system's clipboard. This is a faithful port of Python Prompt Toolkit's `PyperclipClipboard` class, which uses the `pyperclip` library to synchronize with the OS clipboard on Windows, macOS, and Linux.

The system clipboard is essential for the `system-clipboard-integration.py` example (Ctrl-Y to paste from OS clipboard, Ctrl-W to cut to OS clipboard) and for any REPL or shell application where users expect clipboard interop with other applications.

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/clipboard/pyperclip.py`
**Dependency:** `pyperclip` library (https://github.com/asweigart/pyperclip)

### Python Implementation

```python
class PyperclipClipboard(Clipboard):
    def __init__(self) -> None:
        self._data: ClipboardData | None = None

    def set_data(self, data: ClipboardData) -> None:
        self._data = data
        pyperclip.copy(data.text)

    def get_data(self) -> ClipboardData:
        text = pyperclip.paste()

        # When the clipboard data is equal to what we copied last time, reuse
        # the `ClipboardData` instance. That way we're sure to keep the same
        # `SelectionType`.
        if self._data and self._data.text == text:
            return self._data

        # Pyperclip returned something else. Create a new `ClipboardData`
        # instance.
        else:
            return ClipboardData(
                text=text,
                type=SelectionType.LINES if "\n" in text else SelectionType.CHARACTERS,
            )
```

### Pyperclip Platform Mechanisms

Pyperclip auto-detects the platform and uses these clipboard access methods:

| Platform | Mechanism | Copy | Paste |
|----------|-----------|------|-------|
| Windows | Win32 API via ctypes | `OpenClipboard` → `EmptyClipboard` → `SetClipboardData(CF_UNICODETEXT)` → `CloseClipboard` | `OpenClipboard` → `GetClipboardData(CF_UNICODETEXT)` → `CloseClipboard` |
| macOS | Process-based | `pbcopy` (stdin pipe) | `pbpaste` (stdout capture) |
| Linux (Wayland) | Process-based | `wl-copy` (stdin pipe) | `wl-paste -n -t text` (stdout) |
| Linux (X11) | Process-based | `xclip -selection c` (stdin pipe) | `xclip -selection c -o` (stdout) |
| Linux (X11 alt) | Process-based | `xsel -b -i` (stdin pipe) | `xsel -b -o` (stdout) |
| WSL | Process-based | `clip.exe` (stdin, UTF-16LE) | `powershell.exe Get-Clipboard` (stdout) |

## Public API

### SystemClipboard Class

```csharp
namespace Stroke.Clipboard;

/// <summary>
/// Clipboard implementation that synchronizes with the operating system's clipboard.
/// </summary>
/// <remarks>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>PyperclipClipboard</c>, which
/// uses the <c>pyperclip</c> library for cross-platform clipboard access.
/// </para>
/// <para>
/// Platform support:
/// <list type="bullet">
/// <item><description>Windows: Win32 API (OpenClipboard/SetClipboardData/GetClipboardData)</description></item>
/// <item><description>macOS: pbcopy/pbpaste</description></item>
/// <item><description>Linux (Wayland): wl-copy/wl-paste</description></item>
/// <item><description>Linux (X11): xclip or xsel</description></item>
/// <item><description>WSL: clip.exe/powershell.exe</description></item>
/// </list>
/// </para>
/// <para>
/// <b>SelectionType Preservation:</b> When <see cref="GetData"/> reads text from the OS
/// clipboard that matches the last text written via <see cref="SetData"/>, the original
/// <see cref="ClipboardData"/> (including its <see cref="SelectionType"/>) is returned.
/// When the OS clipboard contains text from an external source, a new <see cref="ClipboardData"/>
/// is created with <see cref="SelectionType.Lines"/> if the text contains newlines,
/// otherwise <see cref="SelectionType.Characters"/>.
/// </para>
/// <para>
/// <b>Thread Safety:</b> This class is thread-safe. All public methods are synchronized
/// using <see cref="System.Threading.Lock"/>. Individual operations are atomic.
/// </para>
/// </remarks>
public sealed class SystemClipboard : IClipboard
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SystemClipboard"/> class.
    /// Auto-detects the platform and selects the appropriate clipboard provider.
    /// </summary>
    /// <exception cref="PlatformNotSupportedException">
    /// Thrown when no clipboard mechanism is available on the current platform.
    /// On Linux, this means no clipboard tool (xclip, xsel, wl-copy) was found.
    /// </exception>
    public SystemClipboard();

    /// <summary>
    /// Initializes a new instance with an explicit clipboard provider.
    /// For testing or custom clipboard backends.
    /// </summary>
    /// <param name="provider">The clipboard provider to use.</param>
    internal SystemClipboard(IClipboardProvider provider);

    /// <summary>
    /// Set data on the clipboard. Writes the text to the OS clipboard.
    /// </summary>
    /// <param name="data">The clipboard data to store.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="data"/> is null.</exception>
    public void SetData(ClipboardData data);

    /// <summary>
    /// Set plain text on the clipboard with <see cref="SelectionType.Characters"/> type.
    /// </summary>
    /// <param name="text">The text to store.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="text"/> is null.</exception>
    public void SetText(string text);

    /// <summary>
    /// Return the clipboard data from the OS clipboard.
    /// </summary>
    /// <returns>
    /// If the OS clipboard text matches the last <see cref="SetData"/> call, returns the
    /// original <see cref="ClipboardData"/> (preserving <see cref="SelectionType"/>).
    /// Otherwise returns a new <see cref="ClipboardData"/> with type inferred from content
    /// (Lines if text contains newlines, Characters otherwise).
    /// </returns>
    public ClipboardData GetData();

    /// <summary>
    /// No-op. The OS clipboard does not support kill ring rotation.
    /// </summary>
    public void Rotate();
}
```

### IClipboardProvider Interface (Internal)

```csharp
namespace Stroke.Clipboard;

/// <summary>
/// Internal interface for platform-specific clipboard text access.
/// </summary>
/// <remarks>
/// Implementations handle the raw text I/O with the OS clipboard.
/// <see cref="SystemClipboard"/> wraps a provider and adds <see cref="ClipboardData"/>
/// semantics (SelectionType preservation).
/// </remarks>
internal interface IClipboardProvider
{
    /// <summary>
    /// Write text to the OS clipboard.
    /// </summary>
    /// <param name="text">The text to copy to the clipboard.</param>
    void SetText(string text);

    /// <summary>
    /// Read text from the OS clipboard.
    /// </summary>
    /// <returns>The current clipboard text, or empty string if clipboard is empty.</returns>
    string GetText();
}
```

### ClipboardProviderDetector (Internal Static)

```csharp
namespace Stroke.Clipboard;

/// <summary>
/// Detects and creates the appropriate <see cref="IClipboardProvider"/> for the
/// current platform.
/// </summary>
internal static class ClipboardProviderDetector
{
    /// <summary>
    /// Detect the platform and return the appropriate clipboard provider.
    /// </summary>
    /// <returns>A clipboard provider for the current platform.</returns>
    /// <exception cref="PlatformNotSupportedException">
    /// Thrown when no clipboard mechanism is available.
    /// </exception>
    public static IClipboardProvider Detect();
}
```

### WindowsClipboardProvider (Internal)

```csharp
namespace Stroke.Clipboard;

/// <summary>
/// Windows clipboard provider using Win32 API P/Invoke.
/// Uses OpenClipboard/SetClipboardData/GetClipboardData with CF_UNICODETEXT.
/// </summary>
[SupportedOSPlatform("windows")]
internal sealed class WindowsClipboardProvider : IClipboardProvider
{
    /// <summary>CF_UNICODETEXT clipboard format.</summary>
    private const uint CF_UNICODETEXT = 13;

    /// <summary>GMEM_MOVEABLE memory allocation flag.</summary>
    private const uint GMEM_MOVEABLE = 0x0002;

    public void SetText(string text);
    public string GetText();
}
```

### MacOsClipboardProvider (Internal)

```csharp
namespace Stroke.Clipboard;

/// <summary>
/// macOS clipboard provider using pbcopy/pbpaste processes.
/// </summary>
[SupportedOSPlatform("macos")]
internal sealed class MacOsClipboardProvider : IClipboardProvider
{
    public void SetText(string text);
    public string GetText();
}
```

### LinuxClipboardProvider (Internal)

```csharp
namespace Stroke.Clipboard;

/// <summary>
/// Linux clipboard provider with auto-detection of clipboard tools.
/// Detection order: wl-copy/wl-paste (Wayland), xclip (X11), xsel (X11).
/// </summary>
[SupportedOSPlatform("linux")]
internal sealed class LinuxClipboardProvider : IClipboardProvider
{
    /// <summary>
    /// Creates a Linux clipboard provider, auto-detecting the available tool.
    /// </summary>
    /// <exception cref="PlatformNotSupportedException">
    /// Thrown when no clipboard tool (wl-copy, xclip, xsel) is found on PATH.
    /// </exception>
    public LinuxClipboardProvider();

    public void SetText(string text);
    public string GetText();
}
```

### WslClipboardProvider (Internal)

```csharp
namespace Stroke.Clipboard;

/// <summary>
/// WSL (Windows Subsystem for Linux) clipboard provider using clip.exe and powershell.exe.
/// </summary>
[SupportedOSPlatform("linux")]
internal sealed class WslClipboardProvider : IClipboardProvider
{
    public void SetText(string text);
    public string GetText();
}
```

## Project Structure

```
src/Stroke/
└── Clipboard/
    ├── ClipboardData.cs           # (existing)
    ├── IClipboard.cs              # (existing)
    ├── DummyClipboard.cs          # (existing)
    ├── InMemoryClipboard.cs       # (existing)
    ├── DynamicClipboard.cs        # (existing)
    ├── SystemClipboard.cs         # NEW — public IClipboard implementation
    ├── IClipboardProvider.cs      # NEW — internal provider interface
    ├── ClipboardProviderDetector.cs # NEW — platform detection
    ├── WindowsClipboardProvider.cs  # NEW — Win32 P/Invoke
    ├── MacOsClipboardProvider.cs    # NEW — pbcopy/pbpaste
    ├── LinuxClipboardProvider.cs    # NEW — xclip/xsel/wl-clipboard
    └── WslClipboardProvider.cs      # NEW — clip.exe/powershell.exe
tests/Stroke.Tests/
└── Clipboard/
    ├── ClipboardDataTests.cs      # (existing)
    ├── DummyClipboardTests.cs     # (existing)
    ├── InMemoryClipboardTests.cs  # (existing)
    ├── DynamicClipboardTests.cs   # (existing)
    ├── SystemClipboardTests.cs    # NEW — core behavior tests
    ├── ClipboardProviderDetectorTests.cs # NEW — detection logic tests
    ├── MacOsClipboardProviderTests.cs    # NEW — macOS-specific (platform-gated)
    ├── LinuxClipboardProviderTests.cs    # NEW — Linux-specific (platform-gated)
    └── WindowsClipboardProviderTests.cs  # NEW — Windows-specific (platform-gated)
```

## Implementation Notes

### SystemClipboard Core Logic

The core class delegates text I/O to a platform-specific provider while preserving `ClipboardData` semantics:

```csharp
public sealed class SystemClipboard : IClipboard
{
    private readonly IClipboardProvider _provider;
    private readonly Lock _lock = new();
    private ClipboardData? _lastData;

    public SystemClipboard()
    {
        _provider = ClipboardProviderDetector.Detect();
    }

    internal SystemClipboard(IClipboardProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);
        _provider = provider;
    }

    public void SetData(ClipboardData data)
    {
        ArgumentNullException.ThrowIfNull(data);

        using (_lock.EnterScope())
        {
            _lastData = data;
            _provider.SetText(data.Text);
        }
    }

    public void SetText(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        SetData(new ClipboardData(text));
    }

    public ClipboardData GetData()
    {
        using (_lock.EnterScope())
        {
            var text = _provider.GetText();

            // Preserve SelectionType when text matches our last write.
            // Matches Python's PyperclipClipboard.get_data() exactly.
            if (_lastData is not null && _lastData.Text == text)
                return _lastData;

            // External text: infer SelectionType from content.
            return new ClipboardData(
                text,
                text.Contains('\n') ? SelectionType.Lines : SelectionType.Characters);
        }
    }

    public void Rotate()
    {
        // No-op: OS clipboard has no kill ring concept.
    }
}
```

### Platform Detection Logic

```csharp
internal static class ClipboardProviderDetector
{
    public static IClipboardProvider Detect()
    {
        // 1. Windows
        if (OperatingSystem.IsWindows())
            return new WindowsClipboardProvider();

        // 2. macOS
        if (OperatingSystem.IsMacOS())
            return new MacOsClipboardProvider();

        // 3. Linux — check WSL first, then Wayland, then X11
        if (OperatingSystem.IsLinux())
        {
            if (IsWsl())
                return new WslClipboardProvider();

            return new LinuxClipboardProvider();
        }

        throw new PlatformNotSupportedException(
            "No clipboard mechanism available on this platform.");
    }

    private static bool IsWsl()
    {
        // WSL sets /proc/version containing "microsoft" or "Microsoft"
        try
        {
            var version = File.ReadAllText("/proc/version");
            return version.Contains("microsoft", StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }
}
```

### Windows Provider — Win32 P/Invoke

The Windows provider uses direct P/Invoke for maximum performance and reliability, matching what .NET developers expect on Windows:

```csharp
[SupportedOSPlatform("windows")]
internal sealed class WindowsClipboardProvider : IClipboardProvider
{
    private const uint CF_UNICODETEXT = 13;
    private const uint GMEM_MOVEABLE = 0x0002;

    public void SetText(string text)
    {
        if (!OpenClipboard(IntPtr.Zero))
            throw new InvalidOperationException("Failed to open clipboard.");
        try
        {
            EmptyClipboard();

            var bytes = Encoding.Unicode.GetBytes(text + '\0');
            var hGlobal = GlobalAlloc(GMEM_MOVEABLE, (UIntPtr)bytes.Length);
            if (hGlobal == IntPtr.Zero)
                throw new OutOfMemoryException("Failed to allocate clipboard memory.");

            var ptr = GlobalLock(hGlobal);
            try
            {
                Marshal.Copy(bytes, 0, ptr, bytes.Length);
            }
            finally
            {
                GlobalUnlock(hGlobal);
            }

            if (SetClipboardData(CF_UNICODETEXT, hGlobal) == IntPtr.Zero)
            {
                GlobalFree(hGlobal);
                throw new InvalidOperationException("Failed to set clipboard data.");
            }
            // On success, the system owns hGlobal — do NOT free it.
        }
        finally
        {
            CloseClipboard();
        }
    }

    public string GetText()
    {
        if (!OpenClipboard(IntPtr.Zero))
            return string.Empty;
        try
        {
            var hData = GetClipboardData(CF_UNICODETEXT);
            if (hData == IntPtr.Zero)
                return string.Empty;

            var ptr = GlobalLock(hData);
            if (ptr == IntPtr.Zero)
                return string.Empty;
            try
            {
                return Marshal.PtrToStringUni(ptr) ?? string.Empty;
            }
            finally
            {
                GlobalUnlock(hData);
            }
        }
        finally
        {
            CloseClipboard();
        }
    }

    // P/Invoke declarations
    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool OpenClipboard(IntPtr hWndNewOwner);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool CloseClipboard();

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool EmptyClipboard();

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SetClipboardData(uint uFormat, IntPtr hMem);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr GetClipboardData(uint uFormat);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GlobalAlloc(uint uFlags, UIntPtr dwBytes);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GlobalFree(IntPtr hMem);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GlobalLock(IntPtr hMem);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GlobalUnlock(IntPtr hMem);
}
```

### macOS Provider — pbcopy/pbpaste

```csharp
[SupportedOSPlatform("macos")]
internal sealed class MacOsClipboardProvider : IClipboardProvider
{
    public void SetText(string text)
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "pbcopy",
                RedirectStandardInput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            }
        };
        process.Start();
        process.StandardInput.Write(text);
        process.StandardInput.Close();
        process.WaitForExit(5000);
    }

    public string GetText()
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "pbpaste",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            }
        };
        process.Start();
        var text = process.StandardOutput.ReadToEnd();
        process.WaitForExit(5000);
        return text;
    }
}
```

### Linux Provider — Auto-detection

The Linux provider detects the display server and available clipboard tools:

```csharp
[SupportedOSPlatform("linux")]
internal sealed class LinuxClipboardProvider : IClipboardProvider
{
    private readonly string _copyCommand;
    private readonly string[] _copyArgs;
    private readonly string _pasteCommand;
    private readonly string[] _pasteArgs;

    public LinuxClipboardProvider()
    {
        var waylandDisplay = Environment.GetEnvironmentVariable("WAYLAND_DISPLAY");
        var x11Display = Environment.GetEnvironmentVariable("DISPLAY");

        // 1. Wayland: wl-copy / wl-paste
        if (!string.IsNullOrEmpty(waylandDisplay) && CanFind("wl-copy") && CanFind("wl-paste"))
        {
            _copyCommand = "wl-copy";
            _copyArgs = [];
            _pasteCommand = "wl-paste";
            _pasteArgs = ["-n", "-t", "text"];
            return;
        }

        // 2. X11: xclip
        if (!string.IsNullOrEmpty(x11Display) && CanFind("xclip"))
        {
            _copyCommand = "xclip";
            _copyArgs = ["-selection", "clipboard"];
            _pasteCommand = "xclip";
            _pasteArgs = ["-selection", "clipboard", "-o"];
            return;
        }

        // 3. X11: xsel
        if (!string.IsNullOrEmpty(x11Display) && CanFind("xsel"))
        {
            _copyCommand = "xsel";
            _copyArgs = ["-b", "-i"];
            _pasteCommand = "xsel";
            _pasteArgs = ["-b", "-o"];
            return;
        }

        throw new PlatformNotSupportedException(
            "No clipboard tool found. Install one of: wl-clipboard (Wayland), "
            + "xclip (X11), or xsel (X11).");
    }

    public void SetText(string text)
    {
        RunWithStdin(_copyCommand, _copyArgs, text);
    }

    public string GetText()
    {
        return RunWithStdout(_pasteCommand, _pasteArgs);
    }

    private static bool CanFind(string command)
    {
        // Use 'which' to check if command exists on PATH
        try
        {
            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = "which",
                ArgumentList = { command },
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            });
            process?.WaitForExit(2000);
            return process?.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    private static void RunWithStdin(string command, string[] args, string input)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = command,
            RedirectStandardInput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        foreach (var arg in args)
            startInfo.ArgumentList.Add(arg);

        using var process = new Process { StartInfo = startInfo };
        process.Start();
        process.StandardInput.Write(input);
        process.StandardInput.Close();
        process.WaitForExit(5000);
    }

    private static string RunWithStdout(string command, string[] args)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = command,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        foreach (var arg in args)
            startInfo.ArgumentList.Add(arg);

        using var process = new Process { StartInfo = startInfo };
        process.Start();
        var text = process.StandardOutput.ReadToEnd();
        process.WaitForExit(5000);
        return text;
    }
}
```

### WSL Provider — clip.exe/powershell.exe

```csharp
[SupportedOSPlatform("linux")]
internal sealed class WslClipboardProvider : IClipboardProvider
{
    public void SetText(string text)
    {
        // WSL uses clip.exe with UTF-16LE encoding (matching pyperclip)
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "clip.exe",
                RedirectStandardInput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardInputEncoding = Encoding.Unicode,
            }
        };
        process.Start();
        process.StandardInput.Write(text);
        process.StandardInput.Close();
        process.WaitForExit(5000);
    }

    public string GetText()
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                ArgumentList = { "-noprofile", "-command", "Get-Clipboard -Raw" },
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            }
        };
        process.Start();
        var text = process.StandardOutput.ReadToEnd();
        process.WaitForExit(5000);

        // PowerShell appends \r\n — trim trailing newline
        return text.TrimEnd('\r', '\n');
    }
}
```

### Error Handling Strategy

All providers follow the same error handling philosophy:

1. **SetText failures**: Swallow process/P/Invoke errors silently. The clipboard is a best-effort mechanism — a paste that doesn't work is better than a crash. This matches pyperclip's behavior in the process-based providers.
2. **GetText failures**: Return `string.Empty` on any error. This causes `GetData()` to return an empty `ClipboardData`, which is safe for all consumers.
3. **Process timeouts**: All subprocess operations have a 5-second timeout. If a clipboard tool hangs (e.g., `xclip` with no X server), the operation completes with empty/no-op rather than blocking indefinitely.
4. **Detection failures**: `ClipboardProviderDetector.Detect()` throws `PlatformNotSupportedException` with a helpful message listing which tools to install. The `SystemClipboard()` constructor propagates this exception.

### Process Execution Safety

All process-based providers (macOS, Linux, WSL) follow safe subprocess patterns:

- **`ArgumentList`** instead of `Arguments` to prevent shell injection
- **`UseShellExecute = false`** for direct process execution
- **`CreateNoWindow = true`** to prevent terminal flash on Windows/WSL
- **`RedirectStandardInput/Output`** for piped I/O
- **Explicit `Close()`** on StandardInput before WaitForExit
- **5-second timeout** on WaitForExit to prevent hangs

### Integration with Existing Clipboard System

`SystemClipboard` implements `IClipboard` and is a drop-in replacement anywhere `InMemoryClipboard` or `DummyClipboard` is used:

```csharp
// In the SystemClipboard example:
var answer = Prompt.RunPrompt(
    "Give me some input: ",
    clipboard: new SystemClipboard());

// In PromptSession constructor — could be used as default:
public PromptSession(
    // ...
    IClipboard? clipboard = null,
    // ...)
{
    // User explicitly passes clipboard; no change to defaults needed.
    Clipboard = clipboard ?? new InMemoryClipboard();
}
```

The default clipboard for `PromptSession` remains `InMemoryClipboard` to match Python Prompt Toolkit's default behavior (Python defaults to `InMemoryClipboard`, and `PyperclipClipboard` must be explicitly passed). The `SystemClipboard` is an opt-in feature.

## Dependencies

- Feature 03/79: Clipboard System (`IClipboard`, `ClipboardData`, `SelectionType`)
- .NET BCL: `System.Diagnostics.Process` (macOS, Linux, WSL providers)
- .NET BCL: `System.Runtime.InteropServices` (Windows provider P/Invoke)
- .NET BCL: `System.Runtime.Versioning` (`SupportedOSPlatform` attribute)
- .NET BCL: `System.Text.Encoding` (WSL provider UTF-16LE)
- No external NuGet packages required

## Implementation Tasks

1. Create `IClipboardProvider` internal interface in `IClipboardProvider.cs`
2. Implement `WindowsClipboardProvider` with Win32 P/Invoke in `WindowsClipboardProvider.cs`
3. Implement `MacOsClipboardProvider` with pbcopy/pbpaste in `MacOsClipboardProvider.cs`
4. Implement `LinuxClipboardProvider` with wl-copy/xclip/xsel auto-detection in `LinuxClipboardProvider.cs`
5. Implement `WslClipboardProvider` with clip.exe/powershell.exe in `WslClipboardProvider.cs`
6. Implement `ClipboardProviderDetector` with platform detection in `ClipboardProviderDetector.cs`
7. Implement `SystemClipboard` public class in `SystemClipboard.cs`
8. Update `SystemClipboard.cs` example to use `new SystemClipboard()` instead of `new InMemoryClipboard()`
9. Write `SystemClipboardTests.cs` — core behavior tests (SelectionType preservation, Rotate no-op, SetData/GetData round-trip)
10. Write `ClipboardProviderDetectorTests.cs` — detection logic tests
11. Write platform-specific provider tests (platform-gated with `[SupportedOSPlatform]` skip conditions)
12. Verify SystemClipboard example end-to-end: copy text externally, Ctrl-Y pastes it; Ctrl-W cuts, paste externally

## Testing Strategy

### Unit Tests (SystemClipboardTests.cs)

Tests that verify SystemClipboard behavior using a test provider (a simple in-process implementation of `IClipboardProvider` backed by a string field — this is NOT a mock, it's a real implementation of the internal interface for testing the SystemClipboard wrapper logic):

```csharp
// Internal test provider — real implementation, not a mock
internal sealed class StringClipboardProvider : IClipboardProvider
{
    private string _text = "";
    public void SetText(string text) => _text = text;
    public string GetText() => _text;
}

[Fact]
public void SetData_GetData_RoundTrip()
{
    var clipboard = new SystemClipboard(new StringClipboardProvider());
    clipboard.SetData(new ClipboardData("hello", SelectionType.Lines));
    var data = clipboard.GetData();
    Assert.Equal("hello", data.Text);
    Assert.Equal(SelectionType.Lines, data.Type);
}

[Fact]
public void GetData_ExternalText_WithNewlines_ReturnsLines()
{
    var provider = new StringClipboardProvider();
    var clipboard = new SystemClipboard(provider);
    // Simulate external clipboard change
    provider.SetText("line1\nline2");
    var data = clipboard.GetData();
    Assert.Equal("line1\nline2", data.Text);
    Assert.Equal(SelectionType.Lines, data.Type);
}

[Fact]
public void GetData_ExternalText_WithoutNewlines_ReturnsCharacters()
{
    var provider = new StringClipboardProvider();
    var clipboard = new SystemClipboard(provider);
    provider.SetText("single line");
    var data = clipboard.GetData();
    Assert.Equal(SelectionType.Characters, data.Type);
}

[Fact]
public void GetData_PreservesSelectionType_WhenTextMatches()
{
    var clipboard = new SystemClipboard(new StringClipboardProvider());
    clipboard.SetData(new ClipboardData("text", SelectionType.Block));
    var data = clipboard.GetData();
    Assert.Equal(SelectionType.Block, data.Type);
}

[Fact]
public void Rotate_IsNoOp()
{
    var clipboard = new SystemClipboard(new StringClipboardProvider());
    clipboard.SetData(new ClipboardData("a"));
    clipboard.Rotate();
    Assert.Equal("a", clipboard.GetData().Text);
}

[Fact]
public void SetData_ThrowsOnNull()
{
    var clipboard = new SystemClipboard(new StringClipboardProvider());
    Assert.Throws<ArgumentNullException>(() => clipboard.SetData(null!));
}

[Fact]
public void SetText_ThrowsOnNull()
{
    var clipboard = new SystemClipboard(new StringClipboardProvider());
    Assert.Throws<ArgumentNullException>(() => clipboard.SetText(null!));
}
```

### Platform Integration Tests

Platform-specific tests that actually read/write the OS clipboard. These are gated by platform and require a display server on Linux:

```csharp
[Fact]
[SupportedOSPlatform("macos")]
public void MacOs_SetText_GetText_RoundTrip()
{
    if (!OperatingSystem.IsMacOS()) return;
    var provider = new MacOsClipboardProvider();
    var testText = $"stroke-test-{Guid.NewGuid()}";
    provider.SetText(testText);
    Assert.Equal(testText, provider.GetText());
}

[Fact]
[SupportedOSPlatform("windows")]
public void Windows_SetText_GetText_RoundTrip()
{
    if (!OperatingSystem.IsWindows()) return;
    var provider = new WindowsClipboardProvider();
    var testText = $"stroke-test-{Guid.NewGuid()}";
    provider.SetText(testText);
    Assert.Equal(testText, provider.GetText());
}
```

### End-to-End Verification

Using TUI Driver to verify the SystemClipboard example:

```javascript
// 1. Copy text to OS clipboard first (platform-specific)
// macOS: echo "pasted from OS" | pbcopy

// 2. Launch example
const session = await tui_launch({
  command: "dotnet",
  args: ["run", "--project", "examples/Stroke.Examples.Prompts", "--", "SystemClipboard"],
  cols: 80, rows: 24
});
await tui_wait_for_text({ session_id: session.id, text: "Give me some input:" });

// 3. Press Ctrl-Y to paste from OS clipboard
await tui_press_key({ session_id: session.id, key: "Ctrl+y" });
await tui_wait_for_text({ session_id: session.id, text: "pasted from OS" });

// 4. Press Enter
await tui_press_key({ session_id: session.id, key: "Enter" });
await tui_wait_for_text({ session_id: session.id, text: "You said: pasted from OS" });

await tui_close({ session_id: session.id });
```

## Acceptance Criteria

### Core Behavior
- [ ] `SystemClipboard` implements `IClipboard`
- [ ] `SetData` writes text to OS clipboard and caches `ClipboardData`
- [ ] `GetData` reads text from OS clipboard
- [ ] `GetData` preserves `SelectionType` when text matches last `SetData` call
- [ ] `GetData` infers `SelectionType.Lines` when external text contains newlines
- [ ] `GetData` infers `SelectionType.Characters` when external text has no newlines
- [ ] `Rotate` is a no-op (OS clipboard has no kill ring)
- [ ] `SetData(null)` throws `ArgumentNullException`
- [ ] `SetText(null)` throws `ArgumentNullException`
- [ ] Thread-safe via `System.Threading.Lock`

### Platform Support
- [ ] Windows: Win32 P/Invoke with CF_UNICODETEXT works
- [ ] macOS: pbcopy/pbpaste round-trip works
- [ ] Linux (Wayland): wl-copy/wl-paste works when `WAYLAND_DISPLAY` is set
- [ ] Linux (X11): xclip works when `DISPLAY` is set and xclip is installed
- [ ] Linux (X11): xsel works as fallback when xclip is not available
- [ ] WSL: clip.exe/powershell.exe works
- [ ] `PlatformNotSupportedException` thrown with helpful message when no tool available

### Detection Logic
- [ ] Windows detected via `OperatingSystem.IsWindows()`
- [ ] macOS detected via `OperatingSystem.IsMacOS()`
- [ ] WSL detected via `/proc/version` containing "microsoft" (case-insensitive)
- [ ] Linux Wayland detected via `WAYLAND_DISPLAY` environment variable + `which wl-copy`
- [ ] Linux X11 detected via `DISPLAY` environment variable + `which xclip`/`which xsel`
- [ ] Detection order: Windows → macOS → WSL → Linux Wayland → Linux X11

### Error Handling
- [ ] SetText swallows process/P/Invoke errors (best-effort)
- [ ] GetText returns empty string on any error
- [ ] Process operations timeout after 5 seconds
- [ ] No shell injection possible (ArgumentList, not Arguments)

### Example Integration
- [ ] `SystemClipboard.cs` example uses `new SystemClipboard()` instead of `new InMemoryClipboard()`
- [ ] Ctrl-Y pastes text from OS clipboard
- [ ] Ctrl-W cuts to OS clipboard
- [ ] External applications can paste text cut from Stroke
- [ ] Ctrl-C exits gracefully

### Testing
- [ ] Unit tests for SystemClipboard core behavior (SelectionType preservation, null checks, Rotate)
- [ ] Unit tests for ClipboardProviderDetector logic
- [ ] Platform integration tests for each provider (platform-gated)
- [ ] Unit tests achieve 80% coverage
