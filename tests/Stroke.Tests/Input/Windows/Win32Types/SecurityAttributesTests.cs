using System.Runtime.InteropServices;
using Stroke.Input.Windows.Win32Types;
using Xunit;

namespace Stroke.Tests.Input.Windows.Win32Types;

/// <summary>
/// Tests for the <see cref="SecurityAttributes"/> struct.
/// </summary>
public sealed class SecurityAttributesTests
{
    [Fact]
    public void Size_MatchesExpected()
    {
        // SECURITY_ATTRIBUTES size depends on pointer size:
        // x86: 4 + 4 + 4 = 12 bytes
        // x64: 4 + 8 + 4 = 16 bytes (but with alignment padding = 24 bytes)
        var size = Marshal.SizeOf<SecurityAttributes>();

        // Size should be either 12 (x86) or 24 (x64 with alignment)
        Assert.True(size == 12 || size == 24,
            $"Expected size 12 (x86) or 24 (x64), got {size}");
    }

    [Fact]
    public void Create_SetsLengthCorrectly()
    {
        var sa = SecurityAttributes.Create();

        Assert.Equal((uint)Marshal.SizeOf<SecurityAttributes>(), sa.Length);
    }

    [Fact]
    public void Create_DefaultsSecurityDescriptorToZero()
    {
        var sa = SecurityAttributes.Create();

        Assert.Equal(nint.Zero, sa.SecurityDescriptor);
    }

    [Fact]
    public void Create_DefaultsInheritHandleToFalse()
    {
        var sa = SecurityAttributes.Create();

        Assert.Equal(0, sa.InheritHandle);
    }

    [Fact]
    public void FieldAssignment_Works()
    {
        var sa = SecurityAttributes.Create();
        sa.InheritHandle = 1; // true

        Assert.Equal(1, sa.InheritHandle);
    }

    [Fact]
    public void DefaultValue_HasZeroLength()
    {
        var sa = default(SecurityAttributes);

        Assert.Equal(0u, sa.Length);
        Assert.Equal(nint.Zero, sa.SecurityDescriptor);
        Assert.Equal(0, sa.InheritHandle);
    }
}
