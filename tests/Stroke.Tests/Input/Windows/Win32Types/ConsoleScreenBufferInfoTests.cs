using System.Runtime.InteropServices;
using Stroke.Input.Windows.Win32Types;
using Xunit;

namespace Stroke.Tests.Input.Windows.Win32Types;

/// <summary>
/// Tests for the <see cref="ConsoleScreenBufferInfo"/> struct.
/// </summary>
public sealed class ConsoleScreenBufferInfoTests
{
    [Fact]
    public void Size_Is22Bytes()
    {
        // CONSOLE_SCREEN_BUFFER_INFO layout:
        // Size (COORD): 4 bytes
        // CursorPosition (COORD): 4 bytes
        // Attributes (WORD): 2 bytes
        // Window (SMALL_RECT): 8 bytes
        // MaximumWindowSize (COORD): 4 bytes
        // Total: 22 bytes
        Assert.Equal(22, Marshal.SizeOf<ConsoleScreenBufferInfo>());
    }

    [Fact]
    public void DefaultValue_HasExpectedDefaults()
    {
        var info = default(ConsoleScreenBufferInfo);

        Assert.Equal(0, info.Size.X);
        Assert.Equal(0, info.Size.Y);
        Assert.Equal(0, info.CursorPosition.X);
        Assert.Equal(0, info.CursorPosition.Y);
        Assert.Equal(0, info.Attributes);
        Assert.Equal(0, info.Window.Left);
        Assert.Equal(0, info.Window.Top);
        Assert.Equal(0, info.Window.Right);
        Assert.Equal(0, info.Window.Bottom);
        Assert.Equal(0, info.MaximumWindowSize.X);
        Assert.Equal(0, info.MaximumWindowSize.Y);
    }

    [Fact]
    public void FieldOffsets_AreSequential()
    {
        // Verify fields are at expected sequential offsets
        Assert.Equal(0, Marshal.OffsetOf<ConsoleScreenBufferInfo>(nameof(ConsoleScreenBufferInfo.Size)).ToInt32());
        Assert.Equal(4, Marshal.OffsetOf<ConsoleScreenBufferInfo>(nameof(ConsoleScreenBufferInfo.CursorPosition)).ToInt32());
        Assert.Equal(8, Marshal.OffsetOf<ConsoleScreenBufferInfo>(nameof(ConsoleScreenBufferInfo.Attributes)).ToInt32());
        Assert.Equal(10, Marshal.OffsetOf<ConsoleScreenBufferInfo>(nameof(ConsoleScreenBufferInfo.Window)).ToInt32());
        Assert.Equal(18, Marshal.OffsetOf<ConsoleScreenBufferInfo>(nameof(ConsoleScreenBufferInfo.MaximumWindowSize)).ToInt32());
    }

    [Fact]
    public void ToString_ReturnsExpectedFormat()
    {
        var info = default(ConsoleScreenBufferInfo);

        var result = info.ToString();

        // Format matches Python's __repr__: all field values in order
        Assert.Equal("ConsoleScreenBufferInfo(0,0,0,0,0,0,0,0,0,0,0)", result);
    }
}
