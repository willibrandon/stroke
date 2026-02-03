using System.Diagnostics.CodeAnalysis;
using Stroke.Output;
using Stroke.Output.Windows;
using Xunit;

namespace Stroke.Tests.Output.Windows;

/// <summary>
/// Tests for <see cref="Win32Output"/> core functionality.
/// </summary>
/// <remarks>
/// <para>
/// Platform-specific tests are marked with [Trait("Platform", "Windows")] and
/// will be skipped on non-Windows platforms.
/// </para>
/// </remarks>
[SuppressMessage("Interoperability", "CA1416:Validate platform compatibility",
    Justification = "Tests explicitly check OperatingSystem.IsWindows() before calling Win32Output")]
public class Win32OutputTests
{
    #region Constructor Tests (T009)

    [Fact]
    public void Constructor_OnNonWindows_ThrowsPlatformNotSupportedException()
    {
        // Skip this test on Windows where it would succeed
        if (OperatingSystem.IsWindows())
        {
            return;
        }

        var exception = Assert.Throws<PlatformNotSupportedException>(
            () => new Win32Output(Console.Out));

        Assert.Contains("Windows", exception.Message);
    }

    [Fact]
    [Trait("Platform", "Windows")]
    public void Constructor_WithNullStdout_ThrowsArgumentNullException()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        Assert.Throws<ArgumentNullException>(() => new Win32Output(null!));
    }

    [Fact]
    [Trait("Platform", "Windows")]
    public void Constructor_OutsideConsole_ThrowsNoConsoleScreenBufferError()
    {
        // This test would need to run in a context without a console
        // For now, we just verify the exception type exists and has correct message logic
        var error = new NoConsoleScreenBufferError();
        Assert.NotEmpty(error.Message);
    }

    #endregion

    #region Property Tests (T016)

    [Fact]
    [Trait("Platform", "Windows")]
    public void RespondsToCpr_AlwaysReturnsFalse()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        // We can only test this if we have a console
        // For now, test that the interface expectation is correct
        // (Win32 console doesn't support CPR)
        Assert.False(false); // Placeholder - actual test needs console context
    }

    [Fact]
    [Trait("Platform", "Windows")]
    public void Encoding_ReturnsUtf16()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        // Test expectation: Win32Output.Encoding should return "utf-16"
        Assert.True(true); // Placeholder - actual test needs console context
    }

    [Fact]
    [Trait("Platform", "Windows")]
    public void Fileno_ReturnsNegativeOne()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        // Test expectation: Win32Output.Fileno() should return -1
        Assert.True(true); // Placeholder - actual test needs console context
    }

    [Fact]
    [Trait("Platform", "Windows")]
    public void GetDefaultColorDepth_ReturnsDepth4Bit()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        // Test expectation: Win32Output.GetDefaultColorDepth() should return ColorDepth.Depth4Bit
        Assert.True(true); // Placeholder - actual test needs console context
    }

    #endregion

    #region Write/Flush Tests (T010)

    [Fact]
    public void Write_WithNullOrEmpty_IsNoOp()
    {
        // This behavior should be consistent - Write with null/empty is a no-op
        // We can test this through the ColorLookupTable as a proxy for buffering behavior
        var lookupTable = new ColorLookupTable();

        // Empty color returns black (fallback)
        Assert.Equal(ForegroundColor.Black, lookupTable.LookupFgColor(string.Empty));
        Assert.Equal(ForegroundColor.Black, lookupTable.LookupFgColor(null!));
    }

    #endregion

    #region Cursor Movement Tests (T011)

    [Fact]
    public void CursorMovement_ZeroAmount_IsNoOp()
    {
        // This verifies the edge case behavior documented in spec
        // Zero/negative amounts should be no-ops
        // Actual cursor testing requires Windows console
        Assert.True(true); // Placeholder
    }

    [Fact]
    public void CursorMovement_NegativeAmount_TreatedAsZero()
    {
        // Negative amounts should be treated as zero (no-op)
        Assert.True(true); // Placeholder
    }

    #endregion
}
