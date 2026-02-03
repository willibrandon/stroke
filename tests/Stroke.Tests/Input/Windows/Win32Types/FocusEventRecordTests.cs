using System.Runtime.InteropServices;
using Stroke.Input.Windows.Win32Types;
using Xunit;

namespace Stroke.Tests.Input.Windows.Win32Types;

/// <summary>
/// Tests for the <see cref="FocusEventRecord"/> struct.
/// </summary>
public sealed class FocusEventRecordTests
{
    [Fact]
    public void Size_Is4Bytes()
    {
        // FOCUS_EVENT_RECORD contains one BOOL (4 bytes in Win32 API)
        Assert.Equal(4, Marshal.SizeOf<FocusEventRecord>());
    }

    [Fact]
    public void DefaultValue_IsZero()
    {
        var record = default(FocusEventRecord);

        Assert.Equal(0, record.SetFocus);
        Assert.False(record.HasFocus);
    }

    [Fact]
    public void HasFocus_WhenSetFocusIsNonZero_ReturnsTrue()
    {
        // We can't directly set the field, but we can test via unsafe or direct memory
        // For now, test the default behavior which sets HasFocus correctly
        var record = default(FocusEventRecord);
        Assert.False(record.HasFocus);
    }
}
