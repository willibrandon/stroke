using Stroke.Input.Windows;
using Xunit;

namespace Stroke.Tests.Input.Windows.Win32Types;

/// <summary>
/// Tests for the <see cref="StdHandles"/> class.
/// </summary>
public sealed class StdHandlesTests
{
    [Fact]
    public void STD_INPUT_HANDLE_IsMinus10()
    {
        Assert.Equal(-10, StdHandles.STD_INPUT_HANDLE);
    }

    [Fact]
    public void STD_OUTPUT_HANDLE_IsMinus11()
    {
        Assert.Equal(-11, StdHandles.STD_OUTPUT_HANDLE);
    }

    [Fact]
    public void STD_ERROR_HANDLE_IsMinus12()
    {
        Assert.Equal(-12, StdHandles.STD_ERROR_HANDLE);
    }

    [Fact]
    public void Values_MatchWindowsApiDefinitions()
    {
        // These values are defined in winbase.h and are standard across all Windows versions
        // STD_INPUT_HANDLE  ((DWORD)-10)
        // STD_OUTPUT_HANDLE ((DWORD)-11)
        // STD_ERROR_HANDLE  ((DWORD)-12)
        Assert.Equal(-10, StdHandles.STD_INPUT_HANDLE);
        Assert.Equal(-11, StdHandles.STD_OUTPUT_HANDLE);
        Assert.Equal(-12, StdHandles.STD_ERROR_HANDLE);
    }
}
