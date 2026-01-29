using Xunit;
using SChar = Stroke.Layout.Char;

namespace Stroke.Tests.Layout;

/// <summary>
/// Tests for Char control character transformation (User Story 3).
/// </summary>
public class CharControlCharacterTests
{
    [Fact]
    public void C0Controls_AllTransformToCaretNotation()
    {
        // Test all C0 controls (0x00-0x1F)
        for (int i = 0; i <= 0x1F; i++)
        {
            var ch = new SChar(((char)i).ToString(), "");
            char expectedChar = (char)('@' + i);
            string expectedDisplay = $"^{expectedChar}";

            Assert.Equal(expectedDisplay, ch.Character);
            Assert.Equal("class:control-character", ch.Style);
        }
    }

    [Fact]
    public void DEL_TransformsToCaretQuestion()
    {
        var ch = new SChar("\x7F", "");

        Assert.Equal("^?", ch.Character);
        Assert.Equal("class:control-character", ch.Style);
    }

    [Fact]
    public void C1Controls_AllTransformToHexNotation()
    {
        // Test all C1 controls (0x80-0x9F)
        for (int i = 0x80; i <= 0x9F; i++)
        {
            var ch = new SChar(((char)i).ToString(), "");
            string expectedDisplay = $"<{i:x2}>";

            Assert.Equal(expectedDisplay, ch.Character);
            Assert.Equal("class:control-character", ch.Style);
        }
    }

    [Fact]
    public void NBSP_TransformsToSpace()
    {
        var ch = new SChar("\xA0", "");

        Assert.Equal(" ", ch.Character);
        Assert.Equal("class:nbsp", ch.Style);
    }

    [Fact]
    public void C0Control_WithExistingStyle_PrependsControlClass()
    {
        var ch = new SChar("\x01", "class:highlight");

        Assert.Equal("^A", ch.Character);
        Assert.Equal("class:control-character class:highlight", ch.Style);
    }

    [Fact]
    public void C1Control_WithExistingStyle_PrependsControlClass()
    {
        var ch = new SChar("\x80", "class:highlight class:underline");

        Assert.Equal("<80>", ch.Character);
        Assert.Equal("class:control-character class:highlight class:underline", ch.Style);
    }

    [Fact]
    public void NBSP_WithExistingStyle_PrependsNbspClass()
    {
        var ch = new SChar("\xA0", "class:highlight");

        Assert.Equal(" ", ch.Character);
        Assert.Equal("class:nbsp class:highlight", ch.Style);
    }

    [Fact]
    public void DEL_WithExistingStyle_PrependsControlClass()
    {
        var ch = new SChar("\x7F", "class:bold");

        Assert.Equal("^?", ch.Character);
        Assert.Equal("class:control-character class:bold", ch.Style);
    }

    [Theory]
    [InlineData('\x00', "^@", 2)]  // Width of "^@"
    [InlineData('\x01', "^A", 2)]  // Width of "^A"
    [InlineData('\x1B', "^[", 2)]  // Width of "^["
    [InlineData('\x7F', "^?", 2)]  // Width of "^?"
    public void CaretNotation_HasCorrectWidth(char input, string expectedDisplay, int expectedWidth)
    {
        var ch = new SChar(input.ToString(), "");

        Assert.Equal(expectedDisplay, ch.Character);
        Assert.Equal(expectedWidth, ch.Width);
    }

    [Theory]
    [InlineData('\x80', "<80>", 4)]  // Width of "<80>"
    [InlineData('\x8F', "<8f>", 4)]  // Width of "<8f>"
    [InlineData('\x9F', "<9f>", 4)]  // Width of "<9f>"
    public void HexNotation_HasCorrectWidth(char input, string expectedDisplay, int expectedWidth)
    {
        var ch = new SChar(input.ToString(), "");

        Assert.Equal(expectedDisplay, ch.Character);
        Assert.Equal(expectedWidth, ch.Width);
    }

    [Fact]
    public void NBSP_TransformedSpace_HasWidthOne()
    {
        var ch = new SChar("\xA0", "");

        Assert.Equal(" ", ch.Character);
        Assert.Equal(1, ch.Width);
    }

    [Fact]
    public void PrintableASCII_NoTransformation()
    {
        // Space (0x20) through tilde (0x7E)
        for (int i = 0x20; i <= 0x7E; i++)
        {
            var expected = ((char)i).ToString();
            var ch = new SChar(expected, "");

            Assert.Equal(expected, ch.Character);
            Assert.Equal("", ch.Style);
        }
    }

    [Fact]
    public void CharactersAfterNBSP_NoTransformation()
    {
        // 0xA1 and beyond should not be transformed
        var ch = new SChar("\xA1", "");

        Assert.Equal("\xA1", ch.Character);
        Assert.Equal("", ch.Style);
    }

    [Fact]
    public void MultiCharString_WithControlChars_NoTransformation()
    {
        // Multi-character strings containing control chars are NOT transformed
        // (transformation only applies to single-char strings)
        var ch = new SChar("\x01\x02", "");

        Assert.Equal("\x01\x02", ch.Character);
        Assert.Equal("", ch.Style);
    }

    [Fact]
    public void EmptyString_NoTransformation()
    {
        var ch = new SChar("", "");

        Assert.Equal("", ch.Character);
        Assert.Equal("", ch.Style);
        Assert.Equal(0, ch.Width);
    }

    [Fact]
    public void CharCreate_ControlChar_TransformsCorrectly()
    {
        // Verify factory method also applies transformations
        var ch = SChar.Create("\x01", "");

        Assert.Equal("^A", ch.Character);
        Assert.Equal("class:control-character", ch.Style);
    }

    [Fact]
    public void CharCreate_NBSP_TransformsCorrectly()
    {
        var ch = SChar.Create("\xA0", "class:test");

        Assert.Equal(" ", ch.Character);
        Assert.Equal("class:nbsp class:test", ch.Style);
    }
}
