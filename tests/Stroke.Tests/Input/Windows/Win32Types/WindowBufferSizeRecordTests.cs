using System.Runtime.InteropServices;
using Stroke.Input.Windows.Win32Types;
using Xunit;

namespace Stroke.Tests.Input.Windows.Win32Types;

/// <summary>
/// Tests for the <see cref="WindowBufferSizeRecord"/> struct.
/// </summary>
public sealed class WindowBufferSizeRecordTests
{
    [Fact]
    public void Size_Is4Bytes()
    {
        // WINDOW_BUFFER_SIZE_RECORD contains one COORD (4 bytes)
        Assert.Equal(4, Marshal.SizeOf<WindowBufferSizeRecord>());
    }

    [Fact]
    public void DefaultValue_IsZero()
    {
        var record = default(WindowBufferSizeRecord);

        Assert.Equal(0, record.Size.X);
        Assert.Equal(0, record.Size.Y);
    }
}
