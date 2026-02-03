using System.Runtime.InteropServices;
using Stroke.Input.Windows.Win32Types;
using Xunit;

namespace Stroke.Tests.Input.Windows.Win32Types;

/// <summary>
/// Tests for the <see cref="CharInfo"/> struct.
/// </summary>
public sealed class CharInfoTests
{
    [Fact]
    public void Size_Is4Bytes()
    {
        // Windows CHAR_INFO is exactly 4 bytes (char + ushort)
        Assert.Equal(4, Marshal.SizeOf<CharInfo>());
    }

    [Fact]
    public void Constructor_SetsFields()
    {
        var charInfo = new CharInfo('A', 0x0F);

        Assert.Equal('A', charInfo.UnicodeChar);
        Assert.Equal(0x0F, charInfo.Attributes);
    }

    [Fact]
    public void DefaultValue_IsZero()
    {
        var charInfo = default(CharInfo);

        Assert.Equal('\0', charInfo.UnicodeChar);
        Assert.Equal(0, charInfo.Attributes);
    }

    [Theory]
    [InlineData('A', 0x0F)]   // White on black
    [InlineData('☺', 0x1F)]   // White on blue (Unicode smiley)
    [InlineData('\0', 0x00)]  // Null character, no attributes
    [InlineData('中', 0x70)]  // CJK character, black on white
    public void Constructor_HandlesVariousInputs(char ch, ushort attr)
    {
        var charInfo = new CharInfo(ch, attr);

        Assert.Equal(ch, charInfo.UnicodeChar);
        Assert.Equal(attr, charInfo.Attributes);
    }

    [Fact]
    public void Equals_SameValues_ReturnsTrue()
    {
        var info1 = new CharInfo('X', 0x0A);
        var info2 = new CharInfo('X', 0x0A);

        Assert.True(info1.Equals(info2));
        Assert.True(info1 == info2);
        Assert.False(info1 != info2);
    }

    [Fact]
    public void Equals_DifferentChar_ReturnsFalse()
    {
        var info1 = new CharInfo('X', 0x0A);
        var info2 = new CharInfo('Y', 0x0A);

        Assert.False(info1.Equals(info2));
        Assert.False(info1 == info2);
        Assert.True(info1 != info2);
    }

    [Fact]
    public void Equals_DifferentAttributes_ReturnsFalse()
    {
        var info1 = new CharInfo('X', 0x0A);
        var info2 = new CharInfo('X', 0x0B);

        Assert.False(info1.Equals(info2));
    }

    [Fact]
    public void Equals_Object_WorksCorrectly()
    {
        var info = new CharInfo('A', 0x0F);
        object boxed = new CharInfo('A', 0x0F);
        object different = new CharInfo('B', 0x0F);

        Assert.True(info.Equals(boxed));
        Assert.False(info.Equals(different));
        Assert.False(info.Equals(null));
        Assert.False(info.Equals("not a charinfo"));
    }

    [Fact]
    public void GetHashCode_EqualValues_SameHash()
    {
        var info1 = new CharInfo('A', 0x0F);
        var info2 = new CharInfo('A', 0x0F);

        Assert.Equal(info1.GetHashCode(), info2.GetHashCode());
    }

    [Fact]
    public void ToString_ReturnsFormattedString()
    {
        var info = new CharInfo('H', 0x1F);

        Assert.Equal("'H' (0x001F)", info.ToString());
    }
}
