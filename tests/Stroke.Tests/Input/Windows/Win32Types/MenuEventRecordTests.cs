using System.Runtime.InteropServices;
using Stroke.Input.Windows.Win32Types;
using Xunit;

namespace Stroke.Tests.Input.Windows.Win32Types;

/// <summary>
/// Tests for the <see cref="MenuEventRecord"/> struct.
/// </summary>
public sealed class MenuEventRecordTests
{
    [Fact]
    public void Size_Is4Bytes()
    {
        // MENU_EVENT_RECORD contains one UINT (4 bytes)
        Assert.Equal(4, Marshal.SizeOf<MenuEventRecord>());
    }

    [Fact]
    public void DefaultValue_IsZero()
    {
        var record = default(MenuEventRecord);

        Assert.Equal(0u, record.CommandId);
    }
}
