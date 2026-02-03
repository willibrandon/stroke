using System.Runtime.InteropServices;
using Stroke.Input.Windows.Win32Types;
using Xunit;

namespace Stroke.Tests.Input.Windows.Win32Types;

/// <summary>
/// Tests for the <see cref="MouseEventRecord"/> struct.
/// </summary>
public sealed class MouseEventRecordTests
{
    [Fact]
    public void Size_Is16Bytes()
    {
        // MOUSE_EVENT_RECORD layout:
        // MousePosition (COORD): 4 bytes
        // ButtonState (DWORD): 4 bytes
        // ControlKeyState (DWORD): 4 bytes
        // EventFlags (DWORD): 4 bytes
        // Total: 16 bytes
        Assert.Equal(16, Marshal.SizeOf<MouseEventRecord>());
    }

    [Fact]
    public void DefaultValue_HasExpectedDefaults()
    {
        var record = default(MouseEventRecord);

        Assert.Equal(0, record.MousePosition.X);
        Assert.Equal(0, record.MousePosition.Y);
        Assert.Equal(MouseButtonState.None, record.ButtonState);
        Assert.Equal(ControlKeyState.None, record.ControlKeyState);
        Assert.Equal(MouseEventFlags.None, record.EventFlags);
    }
}
