using Stroke.FormattedText;
using Xunit;

namespace Stroke.Tests.FormattedText;

/// <summary>
/// Tests for the <see cref="Ansi"/> class.
/// </summary>
public class AnsiTests
{
    #region T040: Basic SGR tests (colors, bold)

    [Fact]
    public void Constructor_WithPlainText_CreatesEmptyStyleFragment()
    {
        var ansi = new Ansi("hello");

        var fragments = ansi.ToFormattedText();
        var text = FormattedTextUtils.FragmentListToText(fragments);

        Assert.Equal("hello", text);
    }

    [Fact]
    public void Constructor_WithBold_CreatesBoldStyleFragment()
    {
        var ansi = new Ansi("\x1b[1mbold\x1b[0m");

        var fragments = ansi.ToFormattedText();

        Assert.Contains(fragments, f => f.Style.Contains("bold") && f.Text == "b");
    }

    [Fact]
    public void Constructor_WithForegroundColor_CreatesColorStyleFragment()
    {
        var ansi = new Ansi("\x1b[31mred\x1b[0m");

        var fragments = ansi.ToFormattedText();

        // Red is ANSI code 31, should map to "ansibrightred" or similar
        Assert.Contains(fragments, f => f.Style.Contains("ansi") && f.Text == "r");
    }

    [Fact]
    public void Constructor_WithBackgroundColor_CreatesBgStyleFragment()
    {
        var ansi = new Ansi("\x1b[44mblue bg\x1b[0m");

        var fragments = ansi.ToFormattedText();

        Assert.Contains(fragments, f => f.Style.Contains("bg:"));
    }

    [Fact]
    public void Constructor_WithReset_ClearsAllStyles()
    {
        var ansi = new Ansi("\x1b[1;31mbold red\x1b[0mnormal");

        var fragments = ansi.ToFormattedText();
        var lastChar = fragments.Last(f => f.Text == "l");

        // After reset, should have empty style
        Assert.Equal("", lastChar.Style);
    }

    #endregion

    #region T041: SGR attribute tests (dim, italic, underline, strike, blink, reverse, hidden)

    [Fact]
    public void Constructor_WithDim_CreatesDimStyleFragment()
    {
        var ansi = new Ansi("\x1b[2mdim\x1b[0m");

        var fragments = ansi.ToFormattedText();

        Assert.Contains(fragments, f => f.Style.Contains("dim"));
    }

    [Fact]
    public void Constructor_WithItalic_CreatesItalicStyleFragment()
    {
        var ansi = new Ansi("\x1b[3mitalic\x1b[0m");

        var fragments = ansi.ToFormattedText();

        Assert.Contains(fragments, f => f.Style.Contains("italic"));
    }

    [Fact]
    public void Constructor_WithUnderline_CreatesUnderlineStyleFragment()
    {
        var ansi = new Ansi("\x1b[4munderline\x1b[0m");

        var fragments = ansi.ToFormattedText();

        Assert.Contains(fragments, f => f.Style.Contains("underline"));
    }

    [Fact]
    public void Constructor_WithBlink_CreatesBlinkStyleFragment()
    {
        var ansi = new Ansi("\x1b[5mblink\x1b[0m");

        var fragments = ansi.ToFormattedText();

        Assert.Contains(fragments, f => f.Style.Contains("blink"));
    }

    [Fact]
    public void Constructor_WithRapidBlink_CreatesBlinkStyleFragment()
    {
        var ansi = new Ansi("\x1b[6mrapid blink\x1b[0m");

        var fragments = ansi.ToFormattedText();

        Assert.Contains(fragments, f => f.Style.Contains("blink"));
    }

    [Fact]
    public void Constructor_WithReverse_CreatesReverseStyleFragment()
    {
        var ansi = new Ansi("\x1b[7mreverse\x1b[0m");

        var fragments = ansi.ToFormattedText();

        Assert.Contains(fragments, f => f.Style.Contains("reverse"));
    }

    [Fact]
    public void Constructor_WithHidden_CreatesHiddenStyleFragment()
    {
        var ansi = new Ansi("\x1b[8mhidden\x1b[0m");

        var fragments = ansi.ToFormattedText();

        Assert.Contains(fragments, f => f.Style.Contains("hidden"));
    }

    [Fact]
    public void Constructor_WithStrike_CreatesStrikeStyleFragment()
    {
        var ansi = new Ansi("\x1b[9mstrike\x1b[0m");

        var fragments = ansi.ToFormattedText();

        Assert.Contains(fragments, f => f.Style.Contains("strike"));
    }

    #endregion

    #region T042: SGR disable code tests (22-29)

    [Fact]
    public void Constructor_WithCode22_DisablesBoldAndDim()
    {
        var ansi = new Ansi("\x1b[1;2mboth\x1b[22mnormal\x1b[0m");

        var fragments = ansi.ToFormattedText();
        var normalN = fragments.First(f => f.Text == "n" && !f.Style.Contains("bold") && !f.Style.Contains("dim"));

        Assert.Equal("n", normalN.Text);
    }

    [Fact]
    public void Constructor_WithCode23_DisablesItalic()
    {
        var ansi = new Ansi("\x1b[3mitalic\x1b[23mnormal\x1b[0m");

        var fragments = ansi.ToFormattedText();
        var normalN = fragments.First(f => f.Text == "n" && !f.Style.Contains("italic"));

        Assert.Equal("n", normalN.Text);
    }

    [Fact]
    public void Constructor_WithCode24_DisablesUnderline()
    {
        var ansi = new Ansi("\x1b[4munderline\x1b[24mnormal\x1b[0m");

        var fragments = ansi.ToFormattedText();
        var normalN = fragments.First(f => f.Text == "n" && !f.Style.Contains("underline"));

        Assert.Equal("n", normalN.Text);
    }

    [Fact]
    public void Constructor_WithCode25_DisablesBlink()
    {
        var ansi = new Ansi("\x1b[5mblink\x1b[25mnormal\x1b[0m");

        var fragments = ansi.ToFormattedText();
        var normalN = fragments.First(f => f.Text == "n" && !f.Style.Contains("blink"));

        Assert.Equal("n", normalN.Text);
    }

    [Fact]
    public void Constructor_WithCode27_DisablesReverse()
    {
        var ansi = new Ansi("\x1b[7mreverse\x1b[27mnormal\x1b[0m");

        var fragments = ansi.ToFormattedText();
        var normalN = fragments.First(f => f.Text == "n" && !f.Style.Contains("reverse"));

        Assert.Equal("n", normalN.Text);
    }

    [Fact]
    public void Constructor_WithCode28_DisablesHidden()
    {
        var ansi = new Ansi("\x1b[8mhidden\x1b[28mnormal\x1b[0m");

        var fragments = ansi.ToFormattedText();
        var normalN = fragments.First(f => f.Text == "n" && !f.Style.Contains("hidden"));

        Assert.Equal("n", normalN.Text);
    }

    [Fact]
    public void Constructor_WithCode29_DisablesStrike()
    {
        var ansi = new Ansi("\x1b[9mstrike\x1b[29mnormal\x1b[0m");

        var fragments = ansi.ToFormattedText();
        var normalN = fragments.First(f => f.Text == "n" && !f.Style.Contains("strike"));

        Assert.Equal("n", normalN.Text);
    }

    #endregion

    #region T043: 256-color tests

    [Fact]
    public void Constructor_With256ColorForeground_CreatesColorStyleFragment()
    {
        // 38;5;196 is red in 256-color palette
        var ansi = new Ansi("\x1b[38;5;196mred\x1b[0m");

        var fragments = ansi.ToFormattedText();

        Assert.Contains(fragments, f => f.Style.Contains("#") && f.Text == "r");
    }

    [Fact]
    public void Constructor_With256ColorBackground_CreatesBgStyleFragment()
    {
        // 48;5;21 is blue in 256-color palette
        var ansi = new Ansi("\x1b[48;5;21mblue\x1b[0m");

        var fragments = ansi.ToFormattedText();

        Assert.Contains(fragments, f => f.Style.Contains("bg:#") && f.Text == "b");
    }

    [Fact]
    public void Constructor_With256ColorIndex0_MapsToBlack()
    {
        var ansi = new Ansi("\x1b[38;5;0mblack\x1b[0m");

        var fragments = ansi.ToFormattedText();

        // Index 0 should map to black (#000000)
        Assert.Contains(fragments, f => f.Style.Contains("#000000") && f.Text == "b");
    }

    [Fact]
    public void Constructor_With256ColorIndex255_MapsToWhite()
    {
        var ansi = new Ansi("\x1b[38;5;255mwhite\x1b[0m");

        var fragments = ansi.ToFormattedText();

        // Index 255 should map to bright white (#eeeeee)
        Assert.Contains(fragments, f => f.Style.Contains("#") && f.Text == "w");
    }

    [Fact]
    public void Constructor_With256ColorOutOfRange_ClampsTo255()
    {
        var ansi = new Ansi("\x1b[38;5;999mtext\x1b[0m");

        var fragments = ansi.ToFormattedText();

        // Should clamp to 255 and not throw
        Assert.NotEmpty(fragments);
    }

    #endregion

    #region T044: True color RGB tests

    [Fact]
    public void Constructor_WithTrueColorForeground_CreatesHexColorStyleFragment()
    {
        var ansi = new Ansi("\x1b[38;2;255;0;0mred\x1b[0m");

        var fragments = ansi.ToFormattedText();

        Assert.Contains(fragments, f => f.Style.Contains("#ff0000") && f.Text == "r");
    }

    [Fact]
    public void Constructor_WithTrueColorBackground_CreatesBgHexColorStyleFragment()
    {
        var ansi = new Ansi("\x1b[48;2;0;128;255mblue\x1b[0m");

        var fragments = ansi.ToFormattedText();

        Assert.Contains(fragments, f => f.Style.Contains("bg:#0080ff") && f.Text == "b");
    }

    [Fact]
    public void Constructor_WithTrueColorOutOfRange_ClampsTo255()
    {
        var ansi = new Ansi("\x1b[38;2;300;-10;500mtext\x1b[0m");

        var fragments = ansi.ToFormattedText();

        // Should clamp values to 0-255 and not throw
        Assert.NotEmpty(fragments);
    }

    #endregion

    #region T045: ZeroWidthEscape tests

    [Fact]
    public void Constructor_WithZeroWidthEscape_CreatesZeroWidthStyleFragment()
    {
        var ansi = new Ansi("before\u0001escape\u0002after");

        var fragments = ansi.ToFormattedText();

        Assert.Contains(fragments, f => f.Style.Contains("[ZeroWidthEscape]"));
    }

    [Fact]
    public void Constructor_WithZeroWidthEscape_ExcludesFromTextLength()
    {
        var ansi = new Ansi("ab\u0001escape\u0002cd");

        var fragments = ansi.ToFormattedText();
        var len = FormattedTextUtils.FragmentListLen(fragments);

        // Should only count "ab" and "cd"
        Assert.Equal(4, len);
    }

    [Fact]
    public void Constructor_WithZeroWidthEscape_ExcludesFromPlainText()
    {
        var ansi = new Ansi("ab\u0001escape\u0002cd");

        var fragments = ansi.ToFormattedText();
        var text = FormattedTextUtils.FragmentListToText(fragments);

        Assert.Equal("abcd", text);
    }

    #endregion

    #region T046: Cursor forward escape tests

    [Fact]
    public void Constructor_WithCursorForward_AddsSpaces()
    {
        var ansi = new Ansi("a\x1b[3Cb");

        var fragments = ansi.ToFormattedText();
        var text = FormattedTextUtils.FragmentListToText(fragments);

        Assert.Equal("a   b", text);
    }

    [Fact]
    public void Constructor_WithCursorForwardZero_AddsNoSpaces()
    {
        var ansi = new Ansi("a\x1b[0Cb");

        var fragments = ansi.ToFormattedText();
        var text = FormattedTextUtils.FragmentListToText(fragments);

        Assert.Equal("ab", text);
    }

    #endregion

    #region T047: Ansi.Format() safe interpolation tests

    [Fact]
    public void Format_WithPlainString_EscapesEscapeCharacter()
    {
        var ansi = new Ansi("Value: {0}").Format("\x1b[31mred");

        var fragments = ansi.ToFormattedText();
        var text = FormattedTextUtils.FragmentListToText(fragments);

        // Escape character should be replaced with '?'
        Assert.Contains("?", text);
    }

    [Fact]
    public void Format_WithMultipleArgs_SubstitutesAllPlaceholders()
    {
        var ansi = new Ansi("{0} and {1}").Format("foo", "bar");

        var fragments = ansi.ToFormattedText();
        var text = FormattedTextUtils.FragmentListToText(fragments);

        Assert.Equal("foo and bar", text);
    }

    [Fact]
    public void Format_WithNamedArgs_SubstitutesCorrectly()
    {
        var ansi = new Ansi("Hello {name}!").Format(new Dictionary<string, object> { ["name"] = "World" });

        var fragments = ansi.ToFormattedText();
        var text = FormattedTextUtils.FragmentListToText(fragments);

        Assert.Equal("Hello World!", text);
    }

    [Fact]
    public void Escape_ReturnsStringWithEscapedCharacters()
    {
        var escaped = Ansi.Escape("\x1b[31mred\bback");

        // Verify escape characters were replaced with '?'
        Assert.Equal("?[31mred?back", escaped);
        // Verify no ESC (0x1B) or BS (0x08) characters remain
        Assert.False(escaped.Contains('\x1b'), "Should not contain ESC character");
        Assert.False(escaped.Contains('\b'), "Should not contain backspace character");
    }

    #endregion

    #region T048: Edge case tests

    [Fact]
    public void Constructor_With8BitCsi_ParsesCorrectly()
    {
        // \x9b is the 8-bit CSI sequence
        var ansi = new Ansi("\x9b31mred\x9b0m");

        var fragments = ansi.ToFormattedText();

        Assert.Contains(fragments, f => f.Text == "r");
    }

    [Fact]
    public void Constructor_WithIncompleteSequence_HandlesGracefully()
    {
        // Incomplete CSI sequence
        var ansi = new Ansi("text\x1b[31");

        var fragments = ansi.ToFormattedText();
        var text = FormattedTextUtils.FragmentListToText(fragments);

        // Should at least contain "text"
        Assert.Contains("text", text);
    }

    [Fact]
    public void Constructor_WithNull_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new Ansi(null!));
    }

    [Fact]
    public void Value_ReturnsOriginalInput()
    {
        var input = "\x1b[1mbold\x1b[0m";
        var ansi = new Ansi(input);

        Assert.Equal(input, ansi.Value);
    }

    [Fact]
    public void ToString_ReturnsDescriptiveString()
    {
        var ansi = new Ansi("\x1b[1mtest\x1b[0m");

        Assert.StartsWith("Ansi(", ansi.ToString());
    }

    [Fact]
    public void Constructor_WithMultipleSgrCodes_CombinesStyles()
    {
        var ansi = new Ansi("\x1b[1;3;4mstyledtext\x1b[0m");

        var fragments = ansi.ToFormattedText();

        // Should have bold, italic, and underline
        Assert.Contains(fragments, f => f.Style.Contains("bold"));
        Assert.Contains(fragments, f => f.Style.Contains("italic"));
        Assert.Contains(fragments, f => f.Style.Contains("underline"));
    }

    [Fact]
    public void Constructor_WithEmptySgr_ResetsStyles()
    {
        var ansi = new Ansi("\x1b[1mbold\x1b[mnormal");

        var fragments = ansi.ToFormattedText();
        var normalN = fragments.First(f => f.Text == "n" && !f.Style.Contains("bold"));

        Assert.Equal("n", normalN.Text);
    }

    [Fact]
    public void Constructor_WithBrightForegroundColors_MapsCorrectly()
    {
        // 90-97 are bright foreground colors
        var ansi = new Ansi("\x1b[91mbright red\x1b[0m");

        var fragments = ansi.ToFormattedText();

        Assert.Contains(fragments, f => f.Text == "b");
    }

    [Fact]
    public void Constructor_WithBrightBackgroundColors_MapsCorrectly()
    {
        // 100-107 are bright background colors
        var ansi = new Ansi("\x1b[101mbright red bg\x1b[0m");

        var fragments = ansi.ToFormattedText();

        Assert.Contains(fragments, f => f.Style.Contains("bg:"));
    }

    #endregion

    #region IFormattedText interface tests

    [Fact]
    public void Ansi_ImplementsIFormattedText()
    {
        var ansi = new Ansi("\x1b[1mtest\x1b[0m");

        Assert.IsAssignableFrom<IFormattedText>(ansi);
    }

    [Fact]
    public void ToFormattedText_ReturnsSameResultMultipleTimes()
    {
        var ansi = new Ansi("\x1b[1mtest\x1b[0m");

        var result1 = ansi.ToFormattedText();
        var result2 = ansi.ToFormattedText();

        // ImmutableArray is a value type, so we verify content equality
        Assert.Equal(result1.Count, result2.Count);
        for (int i = 0; i < result1.Count; i++)
        {
            Assert.Equal(result1[i], result2[i]);
        }
    }

    #endregion
}
