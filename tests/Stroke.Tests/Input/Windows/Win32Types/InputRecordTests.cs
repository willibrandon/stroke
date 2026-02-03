using System.Runtime.InteropServices;
using Stroke.Input.Windows.Win32Types;
using Xunit;

namespace Stroke.Tests.Input.Windows.Win32Types;

/// <summary>
/// Tests for the <see cref="InputRecord"/> struct.
/// </summary>
public sealed class InputRecordTests
{
    [Fact]
    public void Size_Is20Bytes()
    {
        // INPUT_RECORD layout:
        // EventType (WORD): 2 bytes at offset 0
        // Padding: 2 bytes (alignment)
        // Union: 16 bytes at offset 4 (largest member is KEY_EVENT_RECORD or MOUSE_EVENT_RECORD)
        // Total: 20 bytes
        Assert.Equal(20, Marshal.SizeOf<InputRecord>());
    }

    [Fact]
    public void EventType_AtOffset0()
    {
        var offset = Marshal.OffsetOf<InputRecord>(nameof(InputRecord.EventType));
        Assert.Equal(0, offset.ToInt32());
    }

    [Fact]
    public void KeyEvent_AtOffset4()
    {
        var offset = Marshal.OffsetOf<InputRecord>(nameof(InputRecord.KeyEvent));
        Assert.Equal(4, offset.ToInt32());
    }

    [Fact]
    public void MouseEvent_AtOffset4()
    {
        var offset = Marshal.OffsetOf<InputRecord>(nameof(InputRecord.MouseEvent));
        Assert.Equal(4, offset.ToInt32());
    }

    [Fact]
    public void WindowBufferSizeEvent_AtOffset4()
    {
        var offset = Marshal.OffsetOf<InputRecord>(nameof(InputRecord.WindowBufferSizeEvent));
        Assert.Equal(4, offset.ToInt32());
    }

    [Fact]
    public void MenuEvent_AtOffset4()
    {
        var offset = Marshal.OffsetOf<InputRecord>(nameof(InputRecord.MenuEvent));
        Assert.Equal(4, offset.ToInt32());
    }

    [Fact]
    public void FocusEvent_AtOffset4()
    {
        var offset = Marshal.OffsetOf<InputRecord>(nameof(InputRecord.FocusEvent));
        Assert.Equal(4, offset.ToInt32());
    }

    [Fact]
    public void DefaultValue_HasExpectedDefaults()
    {
        var record = default(InputRecord);

        Assert.Equal((EventType)0, record.EventType);
    }

    [Fact]
    public void AllUnionFieldsOverlap_AtSameOffset()
    {
        // Verify all union fields are at offset 4 (overlapping)
        var keyOffset = Marshal.OffsetOf<InputRecord>(nameof(InputRecord.KeyEvent)).ToInt32();
        var mouseOffset = Marshal.OffsetOf<InputRecord>(nameof(InputRecord.MouseEvent)).ToInt32();
        var bufferSizeOffset = Marshal.OffsetOf<InputRecord>(nameof(InputRecord.WindowBufferSizeEvent)).ToInt32();
        var menuOffset = Marshal.OffsetOf<InputRecord>(nameof(InputRecord.MenuEvent)).ToInt32();
        var focusOffset = Marshal.OffsetOf<InputRecord>(nameof(InputRecord.FocusEvent)).ToInt32();

        Assert.Equal(4, keyOffset);
        Assert.Equal(4, mouseOffset);
        Assert.Equal(4, bufferSizeOffset);
        Assert.Equal(4, menuOffset);
        Assert.Equal(4, focusOffset);
    }
}
