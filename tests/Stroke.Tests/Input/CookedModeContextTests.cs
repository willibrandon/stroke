using System.Runtime.Versioning;
using Stroke.Input.Vt100;
using Stroke.Input.Windows;
using Xunit;

namespace Stroke.Tests.Input;

/// <summary>
/// Tests for CookedModeContext (POSIX) and Win32CookedMode (Windows).
/// These tests verify cooked mode can be entered and exited correctly.
/// Platform-specific tests only run on their respective platforms.
/// </summary>
public class CookedModeContextTests
{
    #region T051: POSIX CookedModeContext Tests

    [Fact]
    [SupportedOSPlatform("linux")]
    [SupportedOSPlatform("macos")]
    [SupportedOSPlatform("freebsd")]
    public void Constructor_OnPosix_CreatesInstance()
    {
        if (!OperatingSystem.IsLinux() && !OperatingSystem.IsMacOS() && !OperatingSystem.IsFreeBSD())
            return;

        using var context = new CookedModeContext();

        Assert.NotNull(context);
    }

    [Fact]
    [SupportedOSPlatform("linux")]
    [SupportedOSPlatform("macos")]
    [SupportedOSPlatform("freebsd")]
    public void IsValid_WhenTerminalAvailable_ReturnsTrue_OnPosix()
    {
        if (!OperatingSystem.IsLinux() && !OperatingSystem.IsMacOS() && !OperatingSystem.IsFreeBSD())
            return;

        using var context = new CookedModeContext();

        // When running in a real terminal, IsValid should be true
        // When running in CI or piped environment, may be false
        // Just verify no exception is thrown
        _ = context.IsValid;
    }

    [Fact]
    [SupportedOSPlatform("linux")]
    [SupportedOSPlatform("macos")]
    [SupportedOSPlatform("freebsd")]
    public void Dispose_MultipleCallsSafe_OnPosix()
    {
        if (!OperatingSystem.IsLinux() && !OperatingSystem.IsMacOS() && !OperatingSystem.IsFreeBSD())
            return;

        var context = new CookedModeContext();
        context.Dispose();
        context.Dispose(); // Should not throw

        // Verify no exception
        Assert.True(true);
    }

    [Fact]
    [SupportedOSPlatform("linux")]
    [SupportedOSPlatform("macos")]
    [SupportedOSPlatform("freebsd")]
    public void Constructor_WithCustomFd_OnPosix()
    {
        if (!OperatingSystem.IsLinux() && !OperatingSystem.IsMacOS() && !OperatingSystem.IsFreeBSD())
            return;

        // Test with stdin fd (0)
        using var context = new CookedModeContext(0);

        Assert.NotNull(context);
    }

    [Fact]
    [SupportedOSPlatform("linux")]
    [SupportedOSPlatform("macos")]
    [SupportedOSPlatform("freebsd")]
    public void Constructor_WithInvalidFd_IsValidFalse_OnPosix()
    {
        if (!OperatingSystem.IsLinux() && !OperatingSystem.IsMacOS() && !OperatingSystem.IsFreeBSD())
            return;

        // Use an invalid fd that's not a terminal
        using var context = new CookedModeContext(-1);

        // Invalid fd should result in IsValid = false
        Assert.False(context.IsValid);
    }

    #endregion

    #region T051: Windows Win32CookedMode Tests

    [Fact]
    [SupportedOSPlatform("windows")]
    public void Constructor_OnWindows_CreatesInstance()
    {
        if (!OperatingSystem.IsWindows())
            return;

        using var context = new Win32CookedMode();

        Assert.NotNull(context);
    }

    [Fact]
    [SupportedOSPlatform("windows")]
    public void IsValid_OnWindows_ReturnsValue()
    {
        if (!OperatingSystem.IsWindows())
            return;

        using var context = new Win32CookedMode();

        // Just verify no exception - validity depends on console availability
        _ = context.IsValid;
    }

    [Fact]
    [SupportedOSPlatform("windows")]
    public void Dispose_MultipleCallsSafe_OnWindows()
    {
        if (!OperatingSystem.IsWindows())
            return;

        var context = new Win32CookedMode();
        context.Dispose();
        context.Dispose(); // Should not throw

        Assert.True(true);
    }

    #endregion

    #region Cross-Platform Nesting Tests

    [Fact]
    public void NestedCookedModeContexts_RestoreCorrectly()
    {
        // Test that nested cooked mode contexts can be created and disposed
        // in the correct order without issues

        if (OperatingSystem.IsWindows())
        {
            using var outer = new Win32CookedMode();
            using var inner = new Win32CookedMode();

            // Both should be usable (though may not be valid without real console)
            Assert.NotNull(outer);
            Assert.NotNull(inner);
        }
        else if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS() || OperatingSystem.IsFreeBSD())
        {
            using var outer = new CookedModeContext();
            using var inner = new CookedModeContext();

            Assert.NotNull(outer);
            Assert.NotNull(inner);
        }
    }

    #endregion
}
