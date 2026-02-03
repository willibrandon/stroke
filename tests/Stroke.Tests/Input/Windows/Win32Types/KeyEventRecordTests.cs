using System.Runtime.InteropServices;
using Stroke.Input.Windows.Win32Types;
using Xunit;

namespace Stroke.Tests.Input.Windows.Win32Types;

/// <summary>
/// Tests for the <see cref="KeyEventRecord"/> struct.
/// </summary>
public sealed class KeyEventRecordTests
{
    [Fact]
    public void Size_Is16Bytes()
    {
        // KEY_EVENT_RECORD layout:
        // KeyDown (BOOL): 4 bytes
        // RepeatCount (WORD): 2 bytes
        // VirtualKeyCode (WORD): 2 bytes
        // VirtualScanCode (WORD): 2 bytes
        // UnicodeChar (WCHAR): 2 bytes
        // ControlKeyState (DWORD): 4 bytes
        // Total: 16 bytes
        Assert.Equal(16, Marshal.SizeOf<KeyEventRecord>());
    }

    [Fact]
    public void DefaultValue_HasExpectedDefaults()
    {
        var record = default(KeyEventRecord);

        Assert.Equal(0, record.KeyDown);
        Assert.Equal(0, record.RepeatCount);
        Assert.Equal(0, record.VirtualKeyCode);
        Assert.Equal(0, record.VirtualScanCode);
        Assert.Equal('\0', record.UnicodeChar);
        Assert.Equal(ControlKeyState.None, record.ControlKeyState);
        Assert.False(record.IsKeyDown);
    }

    [Fact]
    public void IsKeyDown_ReturnsCorrectValue()
    {
        // Default is not pressed
        var record = default(KeyEventRecord);
        Assert.False(record.IsKeyDown);
    }
}
