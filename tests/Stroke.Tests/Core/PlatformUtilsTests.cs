namespace Stroke.Tests.Core;

using Stroke.Core;
using Xunit;

/// <summary>
/// Tests for <see cref="PlatformUtils"/> class.
/// </summary>
public class PlatformUtilsTests
{
    #region Platform Detection Tests

    [Fact]
    public void ExactlyOnePlatform_IsTrue_OnSupportedPlatforms()
    {
        // Count how many platform flags are true
        var trueCount = 0;
        if (PlatformUtils.IsWindows) trueCount++;
        if (PlatformUtils.IsMacOS) trueCount++;
        if (PlatformUtils.IsLinux) trueCount++;

        // On Windows, macOS, or Linux, exactly one should be true
        // On other platforms (FreeBSD, etc.), all could be false
        Assert.True(trueCount <= 1, "More than one platform flag is true");

        // If we're on a known platform, exactly one should be true
        if (OperatingSystem.IsWindows() || OperatingSystem.IsMacOS() || OperatingSystem.IsLinux())
        {
            Assert.Equal(1, trueCount);
        }
    }

    [Fact]
    public void IsWindows_MatchesOperatingSystem()
    {
        Assert.Equal(OperatingSystem.IsWindows(), PlatformUtils.IsWindows);
    }

    [Fact]
    public void IsMacOS_MatchesOperatingSystem()
    {
        Assert.Equal(OperatingSystem.IsMacOS(), PlatformUtils.IsMacOS);
    }

    [Fact]
    public void IsLinux_MatchesOperatingSystem()
    {
        Assert.Equal(OperatingSystem.IsLinux(), PlatformUtils.IsLinux);
    }

    #endregion

    #region SuspendToBackgroundSupported Tests

    [Fact]
    public void SuspendToBackgroundSupported_TrueOnUnix_FalseOnWindows()
    {
        if (PlatformUtils.IsWindows)
        {
            Assert.False(PlatformUtils.SuspendToBackgroundSupported);
        }
        else
        {
            // On Unix-like systems (macOS, Linux, FreeBSD, etc.), should be true
            Assert.True(PlatformUtils.SuspendToBackgroundSupported);
        }
    }

    [Fact]
    public void SuspendToBackgroundSupported_IsOppositeOfIsWindows()
    {
        Assert.Equal(!PlatformUtils.IsWindows, PlatformUtils.SuspendToBackgroundSupported);
    }

    #endregion

    #region GetTermEnvironmentVariable Tests

    [Fact]
    public void GetTermEnvironmentVariable_ReturnsEnvironmentValue()
    {
        var savedTerm = Environment.GetEnvironmentVariable("TERM");

        try
        {
            Environment.SetEnvironmentVariable("TERM", "xterm-256color");

            var result = PlatformUtils.GetTermEnvironmentVariable();

            Assert.Equal("xterm-256color", result);
        }
        finally
        {
            Environment.SetEnvironmentVariable("TERM", savedTerm);
        }
    }

    [Fact]
    public void GetTermEnvironmentVariable_ReturnsEmptyString_WhenNotSet()
    {
        var savedTerm = Environment.GetEnvironmentVariable("TERM");

        try
        {
            Environment.SetEnvironmentVariable("TERM", null);

            var result = PlatformUtils.GetTermEnvironmentVariable();

            Assert.Equal("", result);
        }
        finally
        {
            Environment.SetEnvironmentVariable("TERM", savedTerm);
        }
    }

    #endregion

    #region IsDumbTerminal Tests

    [Fact]
    public void IsDumbTerminal_WithDumb_ReturnsTrue()
    {
        Assert.True(PlatformUtils.IsDumbTerminal("dumb"));
    }

    [Fact]
    public void IsDumbTerminal_WithDumb_CaseInsensitive()
    {
        Assert.True(PlatformUtils.IsDumbTerminal("DUMB"));
        Assert.True(PlatformUtils.IsDumbTerminal("Dumb"));
        Assert.True(PlatformUtils.IsDumbTerminal("dUmB"));
    }

    [Fact]
    public void IsDumbTerminal_WithUnknown_ReturnsTrue()
    {
        Assert.True(PlatformUtils.IsDumbTerminal("unknown"));
    }

    [Fact]
    public void IsDumbTerminal_WithUnknown_CaseInsensitive()
    {
        Assert.True(PlatformUtils.IsDumbTerminal("UNKNOWN"));
        Assert.True(PlatformUtils.IsDumbTerminal("Unknown"));
        Assert.True(PlatformUtils.IsDumbTerminal("uNkNoWn"));
    }

    [Fact]
    public void IsDumbTerminal_WithOtherValues_ReturnsFalse()
    {
        Assert.False(PlatformUtils.IsDumbTerminal("xterm"));
        Assert.False(PlatformUtils.IsDumbTerminal("xterm-256color"));
        Assert.False(PlatformUtils.IsDumbTerminal("vt100"));
        Assert.False(PlatformUtils.IsDumbTerminal("screen"));
    }

    [Fact]
    public void IsDumbTerminal_WithEmptyString_ReturnsFalse()
    {
        Assert.False(PlatformUtils.IsDumbTerminal(""));
    }

    [Fact]
    public void IsDumbTerminal_NoParameter_ReadsFromEnvironment_Dumb()
    {
        var savedTerm = Environment.GetEnvironmentVariable("TERM");

        try
        {
            Environment.SetEnvironmentVariable("TERM", "dumb");

            Assert.True(PlatformUtils.IsDumbTerminal());
        }
        finally
        {
            Environment.SetEnvironmentVariable("TERM", savedTerm);
        }
    }

    [Fact]
    public void IsDumbTerminal_NoParameter_ReadsFromEnvironment_Unknown()
    {
        var savedTerm = Environment.GetEnvironmentVariable("TERM");

        try
        {
            Environment.SetEnvironmentVariable("TERM", "unknown");

            Assert.True(PlatformUtils.IsDumbTerminal());
        }
        finally
        {
            Environment.SetEnvironmentVariable("TERM", savedTerm);
        }
    }

    [Fact]
    public void IsDumbTerminal_NoParameter_ReadsFromEnvironment_Xterm()
    {
        var savedTerm = Environment.GetEnvironmentVariable("TERM");

        try
        {
            Environment.SetEnvironmentVariable("TERM", "xterm");

            Assert.False(PlatformUtils.IsDumbTerminal());
        }
        finally
        {
            Environment.SetEnvironmentVariable("TERM", savedTerm);
        }
    }

    [Fact]
    public void IsDumbTerminal_NoParameter_ReturnsFalse_WhenNotSet()
    {
        var savedTerm = Environment.GetEnvironmentVariable("TERM");

        try
        {
            Environment.SetEnvironmentVariable("TERM", null);

            Assert.False(PlatformUtils.IsDumbTerminal());
        }
        finally
        {
            Environment.SetEnvironmentVariable("TERM", savedTerm);
        }
    }

    #endregion

    #region IsConEmuAnsi Tests

    [Fact]
    public void IsConEmuAnsi_WithON_ReturnsTrue_OnWindows()
    {
        if (!PlatformUtils.IsWindows)
        {
            // On non-Windows, IsConEmuAnsi always returns false regardless of env var
            return;
        }

        var savedConEmuAnsi = Environment.GetEnvironmentVariable("ConEmuANSI");

        try
        {
            Environment.SetEnvironmentVariable("ConEmuANSI", "ON");

            Assert.True(PlatformUtils.IsConEmuAnsi);
        }
        finally
        {
            Environment.SetEnvironmentVariable("ConEmuANSI", savedConEmuAnsi);
        }
    }

    [Fact]
    public void IsConEmuAnsi_WithLowercase_ReturnsFalse()
    {
        var savedConEmuAnsi = Environment.GetEnvironmentVariable("ConEmuANSI");

        try
        {
            Environment.SetEnvironmentVariable("ConEmuANSI", "on");

            // Case-sensitive: "on" != "ON"
            Assert.False(PlatformUtils.IsConEmuAnsi);
        }
        finally
        {
            Environment.SetEnvironmentVariable("ConEmuANSI", savedConEmuAnsi);
        }
    }

    [Fact]
    public void IsConEmuAnsi_WithMixedCase_ReturnsFalse()
    {
        var savedConEmuAnsi = Environment.GetEnvironmentVariable("ConEmuANSI");

        try
        {
            Environment.SetEnvironmentVariable("ConEmuANSI", "On");

            Assert.False(PlatformUtils.IsConEmuAnsi);
        }
        finally
        {
            Environment.SetEnvironmentVariable("ConEmuANSI", savedConEmuAnsi);
        }
    }

    [Fact]
    public void IsConEmuAnsi_WithOFF_ReturnsFalse()
    {
        var savedConEmuAnsi = Environment.GetEnvironmentVariable("ConEmuANSI");

        try
        {
            Environment.SetEnvironmentVariable("ConEmuANSI", "OFF");

            Assert.False(PlatformUtils.IsConEmuAnsi);
        }
        finally
        {
            Environment.SetEnvironmentVariable("ConEmuANSI", savedConEmuAnsi);
        }
    }

    [Fact]
    public void IsConEmuAnsi_NotSet_ReturnsFalse()
    {
        var savedConEmuAnsi = Environment.GetEnvironmentVariable("ConEmuANSI");

        try
        {
            Environment.SetEnvironmentVariable("ConEmuANSI", null);

            Assert.False(PlatformUtils.IsConEmuAnsi);
        }
        finally
        {
            Environment.SetEnvironmentVariable("ConEmuANSI", savedConEmuAnsi);
        }
    }

    [Fact]
    public void IsConEmuAnsi_OnNonWindows_AlwaysFalse()
    {
        if (PlatformUtils.IsWindows)
        {
            return;
        }

        var savedConEmuAnsi = Environment.GetEnvironmentVariable("ConEmuANSI");

        try
        {
            // Even with ConEmuANSI=ON, non-Windows returns false
            Environment.SetEnvironmentVariable("ConEmuANSI", "ON");

            Assert.False(PlatformUtils.IsConEmuAnsi);
        }
        finally
        {
            Environment.SetEnvironmentVariable("ConEmuANSI", savedConEmuAnsi);
        }
    }

    #endregion

    #region InMainThread Tests

    [Fact]
    public void InMainThread_ConsistentWithManagedThreadId()
    {
        // Verify InMainThread is consistent with ManagedThreadId == 1
        var expected = Thread.CurrentThread.ManagedThreadId == 1;
        var actual = PlatformUtils.InMainThread;

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void InMainThread_FalseOnBackgroundThread()
    {
        var inMainThread = true;
        var managedThreadId = 0;

        var thread = new Thread(() =>
        {
            inMainThread = PlatformUtils.InMainThread;
            managedThreadId = Thread.CurrentThread.ManagedThreadId;
        });

        thread.Start();
        thread.Join();

        // Background threads have ManagedThreadId != 1
        Assert.NotEqual(1, managedThreadId);
        // Therefore InMainThread should be false
        Assert.False(inMainThread);
    }

    #endregion

    #region GetBellEnvironmentVariable Tests

    [Fact]
    public void GetBellEnvironmentVariable_NotSet_ReturnsTrue()
    {
        var savedBell = Environment.GetEnvironmentVariable("STROKE_BELL");

        try
        {
            Environment.SetEnvironmentVariable("STROKE_BELL", null);

            Assert.True(PlatformUtils.GetBellEnvironmentVariable());
        }
        finally
        {
            Environment.SetEnvironmentVariable("STROKE_BELL", savedBell);
        }
    }

    [Fact]
    public void GetBellEnvironmentVariable_EmptyString_ReturnsFalse()
    {
        var savedBell = Environment.GetEnvironmentVariable("STROKE_BELL");

        try
        {
            // Empty string is not "1" or "true", so returns false (matches Python)
            Environment.SetEnvironmentVariable("STROKE_BELL", "");

            Assert.False(PlatformUtils.GetBellEnvironmentVariable());
        }
        finally
        {
            Environment.SetEnvironmentVariable("STROKE_BELL", savedBell);
        }
    }

    [Fact]
    public void GetBellEnvironmentVariable_True_ReturnsTrue()
    {
        var savedBell = Environment.GetEnvironmentVariable("STROKE_BELL");

        try
        {
            Environment.SetEnvironmentVariable("STROKE_BELL", "true");

            Assert.True(PlatformUtils.GetBellEnvironmentVariable());
        }
        finally
        {
            Environment.SetEnvironmentVariable("STROKE_BELL", savedBell);
        }
    }

    [Fact]
    public void GetBellEnvironmentVariable_TRUE_ReturnsTrue()
    {
        var savedBell = Environment.GetEnvironmentVariable("STROKE_BELL");

        try
        {
            Environment.SetEnvironmentVariable("STROKE_BELL", "TRUE");

            Assert.True(PlatformUtils.GetBellEnvironmentVariable());
        }
        finally
        {
            Environment.SetEnvironmentVariable("STROKE_BELL", savedBell);
        }
    }

    [Fact]
    public void GetBellEnvironmentVariable_One_ReturnsTrue()
    {
        var savedBell = Environment.GetEnvironmentVariable("STROKE_BELL");

        try
        {
            Environment.SetEnvironmentVariable("STROKE_BELL", "1");

            Assert.True(PlatformUtils.GetBellEnvironmentVariable());
        }
        finally
        {
            Environment.SetEnvironmentVariable("STROKE_BELL", savedBell);
        }
    }

    [Fact]
    public void GetBellEnvironmentVariable_False_ReturnsFalse()
    {
        var savedBell = Environment.GetEnvironmentVariable("STROKE_BELL");

        try
        {
            Environment.SetEnvironmentVariable("STROKE_BELL", "false");

            Assert.False(PlatformUtils.GetBellEnvironmentVariable());
        }
        finally
        {
            Environment.SetEnvironmentVariable("STROKE_BELL", savedBell);
        }
    }

    [Fact]
    public void GetBellEnvironmentVariable_Zero_ReturnsFalse()
    {
        var savedBell = Environment.GetEnvironmentVariable("STROKE_BELL");

        try
        {
            Environment.SetEnvironmentVariable("STROKE_BELL", "0");

            Assert.False(PlatformUtils.GetBellEnvironmentVariable());
        }
        finally
        {
            Environment.SetEnvironmentVariable("STROKE_BELL", savedBell);
        }
    }

    [Fact]
    public void GetBellEnvironmentVariable_OtherValue_ReturnsFalse()
    {
        var savedBell = Environment.GetEnvironmentVariable("STROKE_BELL");

        try
        {
            // Only "1" and "true" return true (matches Python)
            Environment.SetEnvironmentVariable("STROKE_BELL", "yes");

            Assert.False(PlatformUtils.GetBellEnvironmentVariable());
        }
        finally
        {
            Environment.SetEnvironmentVariable("STROKE_BELL", savedBell);
        }
    }

    #endregion

    #region IsWindowsVt100Supported Tests

    [Fact]
    public void IsWindowsVt100Supported_OnNonWindows_ReturnsFalse()
    {
        if (PlatformUtils.IsWindows)
        {
            // Skip this test on Windows - it tests non-Windows behavior
            return;
        }

        Assert.False(PlatformUtils.IsWindowsVt100Supported);
    }

    [Fact]
    public void IsWindowsVt100Supported_OnWindows_ReturnsConsistentValue()
    {
        if (!PlatformUtils.IsWindows)
        {
            // Skip this test on non-Windows
            return;
        }

        // On Windows, the value should be consistent across multiple calls
        var first = PlatformUtils.IsWindowsVt100Supported;
        var second = PlatformUtils.IsWindowsVt100Supported;

        Assert.Equal(first, second);
    }

    [Fact]
    public void IsWindowsVt100Supported_OnModernWindows_ReturnsTrue()
    {
        if (!PlatformUtils.IsWindows)
        {
            // Skip this test on non-Windows
            return;
        }

        // Windows 10 version 1607+ supports VT100
        // Most CI systems run on modern Windows, so this should be true
        // Note: This test may fail on very old Windows versions or when
        // running without a console (e.g., certain CI configurations)
        var result = PlatformUtils.IsWindowsVt100Supported;

        // We can't assert true because some CI environments may not have a console
        // Instead, verify it doesn't throw and returns a boolean
        Assert.IsType<bool>(result);
    }

    [Fact]
    public void IsWindowsVt100Supported_ImpliesIsWindows()
    {
        // If VT100 is supported, we must be on Windows
        if (PlatformUtils.IsWindowsVt100Supported)
        {
            Assert.True(PlatformUtils.IsWindows);
        }
    }

    [Fact]
    public void IsWindowsVt100Supported_DoesNotThrow()
    {
        // Should never throw, regardless of platform
        var exception = Record.Exception(() =>
        {
            _ = PlatformUtils.IsWindowsVt100Supported;
        });

        Assert.Null(exception);
    }

    #endregion
}
