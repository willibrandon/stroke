using Stroke.FormattedText;
using Xunit;

namespace Stroke.Tests.FormattedText;

/// <summary>
/// Tests for the <see cref="Html"/> class.
/// </summary>
public class HtmlTests
{
    #region T023: Basic element tests (b, i, u, s)

    [Fact]
    public void Constructor_WithPlainText_CreatesEmptyStyleFragment()
    {
        var html = new Html("hello");

        var fragments = html.ToFormattedText();

        Assert.Single(fragments);
        Assert.Equal("", fragments[0].Style);
        Assert.Equal("hello", fragments[0].Text);
    }

    [Fact]
    public void Constructor_WithBoldElement_CreatesBoldClassFragment()
    {
        var html = new Html("<b>bold</b>");

        var fragments = html.ToFormattedText();

        Assert.Single(fragments);
        Assert.Equal("class:b", fragments[0].Style);
        Assert.Equal("bold", fragments[0].Text);
    }

    [Fact]
    public void Constructor_WithItalicElement_CreatesItalicClassFragment()
    {
        var html = new Html("<i>italic</i>");

        var fragments = html.ToFormattedText();

        Assert.Single(fragments);
        Assert.Equal("class:i", fragments[0].Style);
        Assert.Equal("italic", fragments[0].Text);
    }

    [Fact]
    public void Constructor_WithUnderlineElement_CreatesUnderlineClassFragment()
    {
        var html = new Html("<u>underline</u>");

        var fragments = html.ToFormattedText();

        Assert.Single(fragments);
        Assert.Equal("class:u", fragments[0].Style);
        Assert.Equal("underline", fragments[0].Text);
    }

    [Fact]
    public void Constructor_WithStrikethroughElement_CreatesStrikethroughClassFragment()
    {
        var html = new Html("<s>strike</s>");

        var fragments = html.ToFormattedText();

        Assert.Single(fragments);
        Assert.Equal("class:s", fragments[0].Style);
        Assert.Equal("strike", fragments[0].Text);
    }

    #endregion

    #region T024: Style element tests (fg, bg, color alias)

    [Fact]
    public void Constructor_WithStyleFgAttribute_CreatesFgStyleFragment()
    {
        var html = new Html("<style fg='red'>colored</style>");

        var fragments = html.ToFormattedText();

        Assert.Single(fragments);
        Assert.Equal("fg:red", fragments[0].Style);
        Assert.Equal("colored", fragments[0].Text);
    }

    [Fact]
    public void Constructor_WithStyleBgAttribute_CreatesBgStyleFragment()
    {
        var html = new Html("<style bg='blue'>highlighted</style>");

        var fragments = html.ToFormattedText();

        Assert.Single(fragments);
        Assert.Equal("bg:blue", fragments[0].Style);
        Assert.Equal("highlighted", fragments[0].Text);
    }

    [Fact]
    public void Constructor_WithStyleColorAttribute_TreatsAsAliasFg()
    {
        var html = new Html("<style color='green'>colored</style>");

        var fragments = html.ToFormattedText();

        Assert.Single(fragments);
        Assert.Equal("fg:green", fragments[0].Style);
        Assert.Equal("colored", fragments[0].Text);
    }

    [Fact]
    public void Constructor_WithStyleFgAndBg_CreatesCombinedStyleFragment()
    {
        var html = new Html("<style fg='white' bg='black'>text</style>");

        var fragments = html.ToFormattedText();

        Assert.Single(fragments);
        Assert.Contains("fg:white", fragments[0].Style);
        Assert.Contains("bg:black", fragments[0].Style);
        Assert.Equal("text", fragments[0].Text);
    }

    [Fact]
    public void Constructor_WithHexColor_PreservesColorFormat()
    {
        var html = new Html("<style fg='#ff0000'>red</style>");

        var fragments = html.ToFormattedText();

        Assert.Single(fragments);
        Assert.Equal("fg:#ff0000", fragments[0].Style);
    }

    #endregion

    #region T025: Custom element to class tests

    [Fact]
    public void Constructor_WithCustomElement_CreatesClassFragment()
    {
        var html = new Html("<myclass>custom</myclass>");

        var fragments = html.ToFormattedText();

        Assert.Single(fragments);
        Assert.Equal("class:myclass", fragments[0].Style);
        Assert.Equal("custom", fragments[0].Text);
    }

    [Fact]
    public void Constructor_WithAnyElementName_UsesElementNameAsClass()
    {
        var html = new Html("<error>error text</error>");

        var fragments = html.ToFormattedText();

        Assert.Single(fragments);
        Assert.Equal("class:error", fragments[0].Style);
    }

    #endregion

    #region T026: Nested element tests

    [Fact]
    public void Constructor_WithNestedElements_AccumulatesClasses()
    {
        var html = new Html("<b><i>bold italic</i></b>");

        var fragments = html.ToFormattedText();

        Assert.Single(fragments);
        Assert.Equal("class:b,i", fragments[0].Style);
        Assert.Equal("bold italic", fragments[0].Text);
    }

    [Fact]
    public void Constructor_WithMixedContentAndNesting_CreatesMultipleFragments()
    {
        var html = new Html("before <b>bold <i>both</i> bold</b> after");

        var fragments = html.ToFormattedText();

        Assert.Equal(5, fragments.Count);
        Assert.Equal("", fragments[0].Style);
        Assert.Equal("before ", fragments[0].Text);
        Assert.Equal("class:b", fragments[1].Style);
        Assert.Equal("bold ", fragments[1].Text);
        Assert.Equal("class:b,i", fragments[2].Style);
        Assert.Equal("both", fragments[2].Text);
        Assert.Equal("class:b", fragments[3].Style);
        Assert.Equal(" bold", fragments[3].Text);
        Assert.Equal("", fragments[4].Style);
        Assert.Equal(" after", fragments[4].Text);
    }

    [Fact]
    public void Constructor_WithNestedFgColors_UsesInnermostColor()
    {
        var html = new Html("<style fg='red'>outer <style fg='blue'>inner</style> outer</style>");

        var fragments = html.ToFormattedText();

        Assert.Equal(3, fragments.Count);
        Assert.Equal("fg:red", fragments[0].Style);
        Assert.Equal("fg:blue", fragments[1].Style);
        Assert.Equal("fg:red", fragments[2].Style);
    }

    #endregion

    #region T027: Entity decoding tests

    [Fact]
    public void Constructor_WithLtEntity_DecodesCorrectly()
    {
        var html = new Html("a &lt; b");

        var fragments = html.ToFormattedText();
        var text = FormattedTextUtils.FragmentListToText(fragments);

        Assert.Equal("a < b", text);
    }

    [Fact]
    public void Constructor_WithGtEntity_DecodesCorrectly()
    {
        var html = new Html("a &gt; b");

        var fragments = html.ToFormattedText();
        var text = FormattedTextUtils.FragmentListToText(fragments);

        Assert.Equal("a > b", text);
    }

    [Fact]
    public void Constructor_WithAmpEntity_DecodesCorrectly()
    {
        var html = new Html("a &amp; b");

        var fragments = html.ToFormattedText();
        var text = FormattedTextUtils.FragmentListToText(fragments);

        Assert.Equal("a & b", text);
    }

    [Fact]
    public void Constructor_WithNumericEntity_DecodesCorrectly()
    {
        var html = new Html("&#60;");

        var fragments = html.ToFormattedText();
        var text = FormattedTextUtils.FragmentListToText(fragments);

        Assert.Equal("<", text);
    }

    [Fact]
    public void Constructor_WithHexEntity_DecodesCorrectly()
    {
        var html = new Html("&#x3C;");

        var fragments = html.ToFormattedText();
        var text = FormattedTextUtils.FragmentListToText(fragments);

        Assert.Equal("<", text);
    }

    [Fact]
    public void Constructor_WithQuotEntity_DecodesCorrectly()
    {
        var html = new Html("say &quot;hello&quot;");

        var fragments = html.ToFormattedText();
        var text = FormattedTextUtils.FragmentListToText(fragments);

        Assert.Equal("say \"hello\"", text);
    }

    #endregion

    #region T028: Html.Format() safe interpolation tests

    [Fact]
    public void Format_WithPlainString_EscapesSpecialCharacters()
    {
        var html = new Html("Hello {0}!").Format("<world>");

        var fragments = html.ToFormattedText();
        var text = FormattedTextUtils.FragmentListToText(fragments);

        Assert.Equal("Hello <world>!", text);
    }

    [Fact]
    public void Format_WithMultipleArgs_SubstitutesAllPlaceholders()
    {
        var html = new Html("{0} and {1}").Format("foo", "bar");

        var fragments = html.ToFormattedText();
        var text = FormattedTextUtils.FragmentListToText(fragments);

        Assert.Equal("foo and bar", text);
    }

    [Fact]
    public void Format_WithNamedArgs_SubstitutesCorrectly()
    {
        var html = new Html("Hello {name}!").Format(new Dictionary<string, object> { ["name"] = "World" });

        var fragments = html.ToFormattedText();
        var text = FormattedTextUtils.FragmentListToText(fragments);

        Assert.Equal("Hello World!", text);
    }

    [Fact]
    public void Format_WithSpecialChars_EscapesThem()
    {
        var html = new Html("Value: {0}").Format("1 < 2 && 3 > 2");

        var fragments = html.ToFormattedText();
        var text = FormattedTextUtils.FragmentListToText(fragments);

        Assert.Equal("Value: 1 < 2 && 3 > 2", text);
    }

    [Fact]
    public void Escape_ReturnsEscapedString()
    {
        var escaped = Html.Escape("<b>test</b> & more");

        Assert.Equal("&lt;b&gt;test&lt;/b&gt; &amp; more", escaped);
    }

    #endregion

    #region T029: Malformed XML error handling tests

    [Fact]
    public void Constructor_WithUnclosedTag_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => new Html("<b>unclosed"));
    }

    [Fact]
    public void Constructor_WithMismatchedTags_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => new Html("<b><i>mismatched</b></i>"));
    }

    [Fact]
    public void Constructor_WithInvalidXml_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => new Html("<not valid &"));
    }

    #endregion

    #region T030: Edge case tests

    [Fact]
    public void Constructor_WithSelfClosingTag_ParsesCorrectly()
    {
        // Self-closing tags should be valid but produce no text
        var html = new Html("before<br/>after");

        var fragments = html.ToFormattedText();
        var text = FormattedTextUtils.FragmentListToText(fragments);

        Assert.Equal("beforeafter", text);
    }

    [Fact]
    public void Constructor_WithEmptyElement_ParsesCorrectly()
    {
        var html = new Html("<b></b>text");

        var fragments = html.ToFormattedText();

        Assert.Single(fragments);
        Assert.Equal("text", fragments[0].Text);
    }

    [Fact]
    public void Constructor_WithWhitespaceOnly_PreservesWhitespace()
    {
        var html = new Html("  <b>  </b>  ");

        var fragments = html.ToFormattedText();
        var text = FormattedTextUtils.FragmentListToText(fragments);

        // 2 spaces before <b> + 2 spaces inside <b> + 2 spaces after </b> = 6 spaces
        Assert.Equal("      ", text);
    }

    [Fact]
    public void Constructor_WithNull_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new Html(null!));
    }

    [Fact]
    public void Value_ReturnsOriginalInput()
    {
        var input = "<b>test</b>";
        var html = new Html(input);

        Assert.Equal(input, html.Value);
    }

    [Fact]
    public void ToString_ReturnsDescriptiveString()
    {
        var html = new Html("<b>test</b>");

        Assert.Equal("Html(<b>test</b>)", html.ToString());
    }

    [Fact]
    public void Constructor_WithAttributeContainingSpace_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => new Html("<style fg='red blue'>text</style>"));
    }

    [Fact]
    public void Constructor_WithBgAttributeContainingSpace_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => new Html("<style bg='red blue'>text</style>"));
    }

    #endregion

    #region IFormattedText interface tests

    [Fact]
    public void Html_ImplementsIFormattedText()
    {
        var html = new Html("<b>test</b>");

        Assert.IsAssignableFrom<IFormattedText>(html);
    }

    [Fact]
    public void ToFormattedText_ReturnsSameResultMultipleTimes()
    {
        var html = new Html("<b>test</b>");

        var result1 = html.ToFormattedText();
        var result2 = html.ToFormattedText();

        // ImmutableArray is a value type, so we verify content equality
        Assert.Equal(result1.Count, result2.Count);
        Assert.Equal(result1[0], result2[0]);
    }

    #endregion
}
