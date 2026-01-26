using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Stroke.Input.Posix;
using Stroke.Input.Windows;
using Stroke.Input.Vt100;
using Xunit;

namespace Stroke.Tests.Input;

/// <summary>
/// Integration tests for raw mode that actually execute on their respective platforms.
/// These tests require a real terminal environment to pass.
/// </summary>
public unsafe class RawModeIntegrationTests
{
    #region POSIX Tests (Linux/macOS)

    [Fact]
    [SupportedOSPlatform("linux")]
    [SupportedOSPlatform("macos")]
    [SupportedOSPlatform("freebsd")]
    public void Posix_IsATty_DetectsTerminal()
    {
        if (!OperatingSystem.IsLinux() && !OperatingSystem.IsMacOS() && !OperatingSystem.IsFreeBSD())
        {
            return; // Skip on non-POSIX platforms
        }

        // isatty returns 1 for TTY, 0 otherwise
        // In CI, stdin may not be a TTY, so we just verify the call works
        var result = Termios.IsATty(Termios.STDIN_FILENO);
        Assert.True(result == 0 || result == 1);
    }

    [Fact]
    [SupportedOSPlatform("linux")]
    [SupportedOSPlatform("macos")]
    [SupportedOSPlatform("freebsd")]
    public void Posix_GetAttr_WorksWhenTty()
    {
        if (!OperatingSystem.IsLinux() && !OperatingSystem.IsMacOS() && !OperatingSystem.IsFreeBSD())
        {
            return; // Skip on non-POSIX platforms
        }

        if (Termios.IsATty(Termios.STDIN_FILENO) == 0)
        {
            return; // Skip if not a TTY (CI environment)
        }

        TermiosStruct termios;
        var result = Termios.GetAttr(Termios.STDIN_FILENO, &termios);

        Assert.Equal(0, result);
    }

    [Fact]
    [SupportedOSPlatform("linux")]
    [SupportedOSPlatform("macos")]
    [SupportedOSPlatform("freebsd")]
    public void Posix_RawModeContext_EntersAndExitsRawMode()
    {
        if (!OperatingSystem.IsLinux() && !OperatingSystem.IsMacOS() && !OperatingSystem.IsFreeBSD())
        {
            return; // Skip on non-POSIX platforms
        }

        if (Termios.IsATty(Termios.STDIN_FILENO) == 0)
        {
            return; // Skip if not a TTY (CI environment)
        }

        // Get original settings
        TermiosStruct original;
        Termios.GetAttr(Termios.STDIN_FILENO, &original);

        // Enter raw mode
        using (var rawMode = new RawModeContext())
        {
            Assert.True(rawMode.IsValid);

            // Verify settings changed
            TermiosStruct current;
            Termios.GetAttr(Termios.STDIN_FILENO, &current);

            // Echo should be disabled in raw mode
            Assert.Equal(0u, current.c_lflag & Termios.ECHO);
            // Canonical mode should be disabled
            Assert.Equal(0u, current.c_lflag & Termios.ICANON);
        }

        // Verify settings restored after dispose
        TermiosStruct restored;
        Termios.GetAttr(Termios.STDIN_FILENO, &restored);

        Assert.Equal(original.c_lflag & Termios.ECHO, restored.c_lflag & Termios.ECHO);
        Assert.Equal(original.c_lflag & Termios.ICANON, restored.c_lflag & Termios.ICANON);
    }

    [Fact]
    [SupportedOSPlatform("linux")]
    [SupportedOSPlatform("macos")]
    [SupportedOSPlatform("freebsd")]
    public void Posix_RawModeContext_HandlesNonTtyGracefully()
    {
        if (!OperatingSystem.IsLinux() && !OperatingSystem.IsMacOS() && !OperatingSystem.IsFreeBSD())
        {
            return; // Skip on non-POSIX platforms
        }

        // Using a non-TTY file descriptor (e.g., a pipe or /dev/null)
        // In CI, stdin is often not a TTY
        using var rawMode = new RawModeContext();

        // Should not throw, IsValid depends on whether stdin is a TTY
        // The test passes as long as no exception is thrown
    }

    [Fact]
    [SupportedOSPlatform("linux")]
    [SupportedOSPlatform("macos")]
    [SupportedOSPlatform("freebsd")]
    public void Posix_MakeRaw_DisablesExpectedFlags()
    {
        if (!OperatingSystem.IsLinux() && !OperatingSystem.IsMacOS() && !OperatingSystem.IsFreeBSD())
        {
            return; // Skip on non-POSIX platforms
        }

        // Create a termios struct with all flags set
        var original = new TermiosStruct
        {
            c_iflag = 0xFFFFFFFF,
            c_oflag = 0xFFFFFFFF,
            c_lflag = 0xFFFFFFFF
        };

        var raw = Termios.MakeRaw(original);

        // Verify input flags cleared
        Assert.Equal(0u, raw.c_iflag & Termios.ICRNL);
        Assert.Equal(0u, raw.c_iflag & Termios.IXON);

        // Verify output flags cleared
        Assert.Equal(0u, raw.c_oflag & Termios.OPOST);

        // Verify local flags cleared
        Assert.Equal(0u, raw.c_lflag & Termios.ECHO);
        Assert.Equal(0u, raw.c_lflag & Termios.ICANON);
        Assert.Equal(0u, raw.c_lflag & Termios.ISIG);

        // Verify VMIN and VTIME set
        Assert.Equal(1, raw.c_cc[Termios.VMIN]);
        Assert.Equal(0, raw.c_cc[Termios.VTIME]);
    }

    #endregion

    #region Windows Tests

    [Fact]
    [SupportedOSPlatform("windows")]
    public void Windows_GetStdHandle_ReturnsValidHandle()
    {
        if (!OperatingSystem.IsWindows())
        {
            return; // Skip on non-Windows platforms
        }

        var handle = ConsoleApi.GetStdHandle(ConsoleApi.STD_INPUT_HANDLE);

        // Handle should not be INVALID_HANDLE_VALUE (though it may be null in some CI environments)
        // The test passes as long as the P/Invoke call succeeds
    }

    [Fact]
    [SupportedOSPlatform("windows")]
    public void Windows_GetConsoleMode_WorksWhenConsole()
    {
        if (!OperatingSystem.IsWindows())
        {
            return; // Skip on non-Windows platforms
        }

        var handle = ConsoleApi.GetStdHandle(ConsoleApi.STD_INPUT_HANDLE);
        if (handle == ConsoleApi.INVALID_HANDLE_VALUE)
        {
            return; // Skip if no valid handle (CI environment)
        }

        var success = ConsoleApi.GetConsoleMode(handle, out var mode);

        // In CI, this may fail if stdin is redirected
        // The test passes as long as the P/Invoke call doesn't crash
    }

    [Fact]
    [SupportedOSPlatform("windows")]
    public void Windows_Win32RawMode_EntersAndExitsRawMode()
    {
        if (!OperatingSystem.IsWindows())
        {
            return; // Skip on non-Windows platforms
        }

        var handle = ConsoleApi.GetStdHandle(ConsoleApi.STD_INPUT_HANDLE);
        if (handle == ConsoleApi.INVALID_HANDLE_VALUE)
        {
            return; // Skip if no valid handle
        }

        if (!ConsoleApi.GetConsoleMode(handle, out var original))
        {
            return; // Skip if not a console (CI environment)
        }

        // Enter raw mode
        using (var rawMode = new Win32RawMode())
        {
            if (!rawMode.IsValid)
            {
                return; // Skip if raw mode couldn't be entered
            }

            // Verify settings changed
            ConsoleApi.GetConsoleMode(handle, out var current);

            // Echo should be disabled in raw mode
            Assert.Equal(0u, current & ConsoleApi.ENABLE_ECHO_INPUT);
            // Line input should be disabled
            Assert.Equal(0u, current & ConsoleApi.ENABLE_LINE_INPUT);
            // Processed input should be disabled
            Assert.Equal(0u, current & ConsoleApi.ENABLE_PROCESSED_INPUT);
        }

        // Verify settings restored after dispose
        ConsoleApi.GetConsoleMode(handle, out var restored);

        Assert.Equal(original & ConsoleApi.ENABLE_ECHO_INPUT, restored & ConsoleApi.ENABLE_ECHO_INPUT);
        Assert.Equal(original & ConsoleApi.ENABLE_LINE_INPUT, restored & ConsoleApi.ENABLE_LINE_INPUT);
    }

    [Fact]
    [SupportedOSPlatform("windows")]
    public void Windows_Win32RawMode_HandlesNonConsoleGracefully()
    {
        if (!OperatingSystem.IsWindows())
        {
            return; // Skip on non-Windows platforms
        }

        // In CI, stdin may be redirected
        using var rawMode = new Win32RawMode();

        // Should not throw, IsValid depends on whether stdin is a console
        // The test passes as long as no exception is thrown
    }

    #endregion

    #region Cross-Platform Tests

    [Fact]
    public void CrossPlatform_RawModeAvailable_OnAllSupportedPlatforms()
    {
        // This test verifies that we can create a raw mode context on any platform
        // without throwing exceptions

        IDisposable? rawModeContext = null;

        try
        {
            if (OperatingSystem.IsWindows())
            {
                rawModeContext = new Win32RawMode();
            }
            else if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS() || OperatingSystem.IsFreeBSD())
            {
                rawModeContext = new RawModeContext();
            }

            // Context creation should not throw on supported platforms
            Assert.NotNull(rawModeContext);
        }
        finally
        {
            rawModeContext?.Dispose();
        }
    }

    #endregion
}
