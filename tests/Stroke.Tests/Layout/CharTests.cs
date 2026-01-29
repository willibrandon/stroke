using Xunit;
using SChar = Stroke.Layout.Char;

namespace Stroke.Tests.Layout;

public class CharTests
{
    [Fact]
    public void Constructor_DefaultValues_SpaceAndEmptyStyle()
    {
        var ch = new SChar();
        Assert.Equal(" ", ch.Character);
        Assert.Equal("", ch.Style);
        Assert.Equal(1, ch.Width);
    }

    [Fact]
    public void Constructor_WithCharacterAndStyle_SetsProperties()
    {
        var ch = new SChar("A", "class:keyword");
        Assert.Equal("A", ch.Character);
        Assert.Equal("class:keyword", ch.Style);
        Assert.Equal(1, ch.Width);
    }

    [Fact]
    public void Constructor_NullCharacter_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new SChar(null!, "style"));
    }

    [Fact]
    public void Constructor_NullStyle_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new SChar("A", null!));
    }

    [Fact]
    public void Transparent_HasCorrectValue()
    {
        Assert.Equal("[Transparent]", SChar.Transparent);
    }

    [Theory]
    [InlineData("A", 1)]
    [InlineData(" ", 1)]
    [InlineData("", 0)]
    [InlineData("中", 2)]  // CJK wide character
    [InlineData("日", 2)]  // CJK wide character
    [InlineData("^A", 2)]  // Multi-char string (caret notation)
    public void Width_CalculatedCorrectly(string character, int expectedWidth)
    {
        var ch = new SChar(character, "");
        Assert.Equal(expectedWidth, ch.Width);
    }

    [Theory]
    [InlineData('\x00', "^@")]  // NUL
    [InlineData('\x01', "^A")]  // SOH
    [InlineData('\x1B', "^[")]  // ESC
    [InlineData('\x1F', "^_")]  // US
    public void Constructor_C0ControlChar_TransformsToCaretNotation(char input, string expected)
    {
        var ch = new SChar(input.ToString(), "");
        Assert.Equal(expected, ch.Character);
        Assert.Equal("class:control-character", ch.Style);
    }

    [Fact]
    public void Constructor_DEL_TransformsToCaretQuestion()
    {
        var ch = new SChar("\x7F", "");
        Assert.Equal("^?", ch.Character);
        Assert.Equal("class:control-character", ch.Style);
    }

    [Theory]
    [InlineData('\x80', "<80>")]
    [InlineData('\x9F', "<9f>")]
    public void Constructor_C1ControlChar_TransformsToHexNotation(char input, string expected)
    {
        var ch = new SChar(input.ToString(), "");
        Assert.Equal(expected, ch.Character);
        Assert.Equal("class:control-character", ch.Style);
    }

    [Fact]
    public void Constructor_NonBreakingSpace_TransformsToSpace()
    {
        var ch = new SChar("\xA0", "");
        Assert.Equal(" ", ch.Character);
        Assert.Equal("class:nbsp", ch.Style);
    }

    [Fact]
    public void Constructor_ControlCharWithExistingStyle_PrependsClass()
    {
        var ch = new SChar("\x01", "class:highlight");
        Assert.Equal("^A", ch.Character);
        Assert.Equal("class:control-character class:highlight", ch.Style);
    }

    [Fact]
    public void Constructor_NBSPWithExistingStyle_PrependsClass()
    {
        var ch = new SChar("\xA0", "class:highlight");
        Assert.Equal(" ", ch.Character);
        Assert.Equal("class:nbsp class:highlight", ch.Style);
    }

    [Fact]
    public void Constructor_NormalChar_NoTransformation()
    {
        var ch = new SChar("A", "class:keyword");
        Assert.Equal("A", ch.Character);
        Assert.Equal("class:keyword", ch.Style);
    }

    [Fact]
    public void Constructor_MultiCharString_NoTransformation()
    {
        // Multi-char strings are not transformed (only single chars)
        var ch = new SChar("AB", "");
        Assert.Equal("AB", ch.Character);
        Assert.Equal("", ch.Style);
    }

    [Fact]
    public void Create_ReturnsCachedInstance()
    {
        var ch1 = SChar.Create("A", "style");
        var ch2 = SChar.Create("A", "style");
        Assert.Same(ch1, ch2);  // Same reference from cache
    }

    [Fact]
    public void Create_DifferentInputs_DifferentInstances()
    {
        var ch1 = SChar.Create("A", "style1");
        var ch2 = SChar.Create("A", "style2");
        Assert.NotSame(ch1, ch2);
    }

    [Fact]
    public void Create_NullCharacter_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => SChar.Create(null!, "style"));
    }

    [Fact]
    public void Create_NullStyle_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => SChar.Create("A", null!));
    }

    [Fact]
    public void Equals_SameCharacterAndStyle_ReturnsTrue()
    {
        var ch1 = new SChar("A", "style");
        var ch2 = new SChar("A", "style");
        Assert.True(ch1.Equals(ch2));
        Assert.True(ch1 == ch2);
        Assert.False(ch1 != ch2);
    }

    [Fact]
    public void Equals_DifferentCharacter_ReturnsFalse()
    {
        var ch1 = new SChar("A", "style");
        var ch2 = new SChar("B", "style");
        Assert.False(ch1.Equals(ch2));
        Assert.False(ch1 == ch2);
        Assert.True(ch1 != ch2);
    }

    [Fact]
    public void Equals_DifferentStyle_ReturnsFalse()
    {
        var ch1 = new SChar("A", "style1");
        var ch2 = new SChar("A", "style2");
        Assert.False(ch1.Equals(ch2));
    }

    [Fact]
    public void Equals_Null_ReturnsFalse()
    {
        var ch = new SChar("A", "style");
        Assert.False(ch.Equals(null));
        Assert.False(ch == null);
        Assert.True(ch != null);
    }

    [Fact]
    public void Equals_SameReference_ReturnsTrue()
    {
        var ch = new SChar("A", "style");
        Assert.True(ch.Equals(ch));
#pragma warning disable CS1718 // Comparison made to same variable
        Assert.True(ch == ch);
#pragma warning restore CS1718
    }

    [Fact]
    public void Equals_ObjectOverload_WorksCorrectly()
    {
        var ch1 = new SChar("A", "style");
        object ch2 = new SChar("A", "style");
        Assert.True(ch1.Equals(ch2));
    }

    [Fact]
    public void Equals_DifferentType_ReturnsFalse()
    {
        var ch = new SChar("A", "style");
        Assert.False(ch.Equals("not a Char"));
    }

    [Fact]
    public void GetHashCode_EqualInstances_SameHash()
    {
        var ch1 = new SChar("A", "style");
        var ch2 = new SChar("A", "style");
        Assert.Equal(ch1.GetHashCode(), ch2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_DifferentInstances_LikelyDifferentHash()
    {
        var ch1 = new SChar("A", "style1");
        var ch2 = new SChar("B", "style2");
        // Hash codes are likely different (not guaranteed but highly probable)
        Assert.NotEqual(ch1.GetHashCode(), ch2.GetHashCode());
    }

    [Theory]
    [InlineData("A", "style", "Char('A', 'style')")]
    [InlineData(" ", "", "Char(' ', '')")]
    [InlineData("^A", "class:control-character", "Char('^A', 'class:control-character')")]
    public void ToString_ReturnsCorrectFormat(string character, string style, string expected)
    {
        var ch = new SChar(character, style);
        Assert.Equal(expected, ch.ToString());
    }

    [Fact]
    public void OperatorEquals_BothNull_ReturnsTrue()
    {
        SChar? ch1 = null;
        SChar? ch2 = null;
        Assert.True(ch1 == ch2);
    }

    [Fact]
    public void OperatorEquals_LeftNull_ReturnsFalse()
    {
        SChar? ch1 = null;
        var ch2 = new SChar("A", "style");
        Assert.False(ch1 == ch2);
        Assert.True(ch1 != ch2);
    }

    [Fact]
    public void OperatorEquals_RightNull_ReturnsFalse()
    {
        var ch1 = new SChar("A", "style");
        SChar? ch2 = null;
        Assert.False(ch1 == ch2);
        Assert.True(ch1 != ch2);
    }

    [Fact]
    public void Create_ControlChar_TransformsCorrectly()
    {
        // Verify that Create also applies transformations
        var ch = SChar.Create("\x01", "");
        Assert.Equal("^A", ch.Character);
        Assert.Equal("class:control-character", ch.Style);
    }

    [Fact]
    public void Width_ControlCharDisplay_CorrectWidth()
    {
        // Control char transforms to "^A" which should have width 2
        var ch = new SChar("\x01", "");
        Assert.Equal("^A", ch.Character);
        Assert.Equal(2, ch.Width);
    }

    [Fact]
    public void Width_C1Control_CorrectWidth()
    {
        // C1 control transforms to "<80>" which should have width 4
        var ch = new SChar("\x80", "");
        Assert.Equal("<80>", ch.Character);
        Assert.Equal(4, ch.Width);
    }
}
