using Stroke.FormattedText;
using Xunit;

namespace Stroke.Tests.FormattedText;

/// <summary>
/// Tests for <see cref="AnyFormattedText"/>.
/// </summary>
public sealed class AnyFormattedTextTests
{
    [Fact]
    public void Empty_IsDefault()
    {
        var empty = AnyFormattedText.Empty;

        Assert.True(empty.IsEmpty);
        Assert.Null(empty.Value);
    }

    [Fact]
    public void ImplicitConversion_FromString_Works()
    {
        AnyFormattedText aft = "hello";

        Assert.Equal("hello", aft.Value);
        Assert.False(aft.IsEmpty);
    }

    [Fact]
    public void ImplicitConversion_FromNullString_IsEmpty()
    {
        string? nullStr = null;
        AnyFormattedText aft = nullStr;

        Assert.True(aft.IsEmpty);
    }

    [Fact]
    public void ImplicitConversion_FromEmptyString_IsEmpty()
    {
        AnyFormattedText aft = "";

        Assert.True(aft.IsEmpty);
    }

    [Fact]
    public void ImplicitConversion_FromFormattedText_Works()
    {
        var ft = new Stroke.FormattedText.FormattedText([
            new StyleAndTextTuple("bold", "hello")
        ]);

        AnyFormattedText aft = ft;

        Assert.Same(ft, aft.Value);
        Assert.False(aft.IsEmpty);
    }

    [Fact]
    public void ImplicitConversion_FromNullFormattedText_IsEmpty()
    {
        Stroke.FormattedText.FormattedText? nullFt = null;
        AnyFormattedText aft = nullFt;

        Assert.True(aft.IsEmpty);
    }

    [Fact]
    public void ImplicitConversion_FromEmptyFormattedText_IsEmpty()
    {
        AnyFormattedText aft = Stroke.FormattedText.FormattedText.Empty;

        Assert.True(aft.IsEmpty);
    }

    [Fact]
    public void ImplicitConversion_FromFunc_Works()
    {
        Func<AnyFormattedText> func = () => "lazy value";

        AnyFormattedText aft = func;

        Assert.Same(func, aft.Value);
        Assert.False(aft.IsEmpty);
    }

    [Fact]
    public void ImplicitConversion_FromNullFunc_IsEmpty()
    {
        Func<AnyFormattedText>? nullFunc = null;
        AnyFormattedText aft = nullFunc;

        Assert.True(aft.IsEmpty);
    }

    [Fact]
    public void ToFormattedText_FromString_ReturnsUnstyledFragment()
    {
        AnyFormattedText aft = "hello";

        var ft = aft.ToFormattedText();

        var fragment = Assert.Single(ft);
        Assert.Equal("", fragment.Style);
        Assert.Equal("hello", fragment.Text);
    }

    [Fact]
    public void ToFormattedText_FromString_WithStyle_AppliesStyle()
    {
        AnyFormattedText aft = "hello";

        var ft = aft.ToFormattedText("bold");

        var fragment = Assert.Single(ft);
        Assert.Equal("bold", fragment.Style);
        Assert.Equal("hello", fragment.Text);
    }

    [Fact]
    public void ToFormattedText_FromFormattedText_ReturnsOriginal()
    {
        var original = new Stroke.FormattedText.FormattedText([
            new StyleAndTextTuple("italic", "world")
        ]);
        AnyFormattedText aft = original;

        var ft = aft.ToFormattedText();

        Assert.Same(original, ft);
    }

    [Fact]
    public void ToFormattedText_FromFunc_InvokesFunc()
    {
        var called = false;
        Func<AnyFormattedText> func = () =>
        {
            called = true;
            return "lazy";
        };
        AnyFormattedText aft = func;

        var ft = aft.ToFormattedText();

        Assert.True(called);
        Assert.Equal("lazy", ft[0].Text);
    }

    [Fact]
    public void ToFormattedText_FromEmpty_ReturnsEmptyFormattedText()
    {
        var aft = AnyFormattedText.Empty;

        var ft = aft.ToFormattedText();

        Assert.Same(Stroke.FormattedText.FormattedText.Empty, ft);
    }

    [Fact]
    public void ToPlainText_FromString_ReturnsString()
    {
        AnyFormattedText aft = "hello world";

        var text = aft.ToPlainText();

        Assert.Equal("hello world", text);
    }

    [Fact]
    public void ToPlainText_FromFormattedText_JoinsFragments()
    {
        var ft = new Stroke.FormattedText.FormattedText([
            new StyleAndTextTuple("bold", "hello"),
            new StyleAndTextTuple("", " "),
            new StyleAndTextTuple("italic", "world")
        ]);
        AnyFormattedText aft = ft;

        var text = aft.ToPlainText();

        Assert.Equal("hello world", text);
    }

    [Fact]
    public void ToPlainText_FromEmpty_ReturnsEmptyString()
    {
        var aft = AnyFormattedText.Empty;

        var text = aft.ToPlainText();

        Assert.Equal("", text);
    }

    [Fact]
    public void Equality_SameStringValue_AreEqual()
    {
        AnyFormattedText aft1 = "hello";
        AnyFormattedText aft2 = "hello";

        Assert.True(aft1.Equals(aft2));
        Assert.Equal(aft1.GetHashCode(), aft2.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentStringValue_AreNotEqual()
    {
        AnyFormattedText aft1 = "hello";
        AnyFormattedText aft2 = "world";

        Assert.False(aft1.Equals(aft2));
    }

    [Fact]
    public void Equality_BothEmpty_AreEqual()
    {
        var aft1 = AnyFormattedText.Empty;
        var aft2 = AnyFormattedText.Empty;

        Assert.True(aft1.Equals(aft2));
    }

    [Fact]
    public void Equality_StringAndFormattedText_AreNotEqual()
    {
        AnyFormattedText aft1 = "hello";
        AnyFormattedText aft2 = new Stroke.FormattedText.FormattedText([
            new StyleAndTextTuple("", "hello")
        ]);

        // Different underlying types, even if same plain text
        Assert.False(aft1.Equals(aft2));
    }

    #region T099-T103: User Story 8 - Html/Ansi/PygmentsTokens conversion tests

    [Fact]
    public void ImplicitConversion_FromHtml_Works()
    {
        var html = new Html("<b>bold</b>");

        AnyFormattedText aft = html;

        Assert.Same(html, aft.Value);
        Assert.False(aft.IsEmpty);
    }

    [Fact]
    public void ImplicitConversion_FromNullHtml_IsEmpty()
    {
        Html? nullHtml = null;
        AnyFormattedText aft = nullHtml;

        Assert.True(aft.IsEmpty);
    }

    [Fact]
    public void ImplicitConversion_FromAnsi_Works()
    {
        var ansi = new Ansi("\x1b[1mbold\x1b[0m");

        AnyFormattedText aft = ansi;

        Assert.Same(ansi, aft.Value);
        Assert.False(aft.IsEmpty);
    }

    [Fact]
    public void ImplicitConversion_FromNullAnsi_IsEmpty()
    {
        Ansi? nullAnsi = null;
        AnyFormattedText aft = nullAnsi;

        Assert.True(aft.IsEmpty);
    }

    [Fact]
    public void ImplicitConversion_FromPygmentsTokens_Works()
    {
        var tokens = new PygmentsTokens([("Token.Keyword", "if")]);

        AnyFormattedText aft = tokens;

        Assert.Same(tokens, aft.Value);
        Assert.False(aft.IsEmpty);
    }

    [Fact]
    public void ImplicitConversion_FromNullPygmentsTokens_IsEmpty()
    {
        PygmentsTokens? nullTokens = null;
        AnyFormattedText aft = nullTokens;

        Assert.True(aft.IsEmpty);
    }

    [Fact]
    public void ToFormattedText_FromHtml_ConvertsCorrectly()
    {
        var html = new Html("<b>bold</b>");
        AnyFormattedText aft = html;

        var ft = aft.ToFormattedText();

        Assert.Single(ft);
        Assert.Contains("class:b", ft[0].Style);
        Assert.Equal("bold", ft[0].Text);
    }

    [Fact]
    public void ToFormattedText_FromAnsi_ConvertsCorrectly()
    {
        var ansi = new Ansi("\x1b[1mtest\x1b[0m");
        AnyFormattedText aft = ansi;

        var ft = aft.ToFormattedText();

        // Should have fragments with bold style
        Assert.Contains(ft, f => f.Style.Contains("bold"));
    }

    [Fact]
    public void ToFormattedText_FromPygmentsTokens_ConvertsCorrectly()
    {
        var tokens = new PygmentsTokens([("Token.Keyword", "def")]);
        AnyFormattedText aft = tokens;

        var ft = aft.ToFormattedText();

        Assert.Single(ft);
        Assert.Equal("class:pygments.keyword", ft[0].Style);
        Assert.Equal("def", ft[0].Text);
    }

    [Fact]
    public void ToFormattedText_FromHtml_WithStylePrefix_AppliesPrefix()
    {
        var html = new Html("<b>bold</b>");
        AnyFormattedText aft = html;

        var ft = aft.ToFormattedText("prefix");

        Assert.Single(ft);
        Assert.StartsWith("prefix", ft[0].Style);
    }

    [Fact]
    public void ToPlainText_FromHtml_ReturnsTextOnly()
    {
        var html = new Html("<b>bold</b> text");
        AnyFormattedText aft = html;

        var text = aft.ToPlainText();

        Assert.Equal("bold text", text);
    }

    [Fact]
    public void ToPlainText_FromAnsi_ReturnsTextOnly()
    {
        var ansi = new Ansi("\x1b[1mbold\x1b[0m text");
        AnyFormattedText aft = ansi;

        var text = aft.ToPlainText();

        Assert.Equal("bold text", text);
    }

    [Fact]
    public void ToPlainText_FromPygmentsTokens_ReturnsTextOnly()
    {
        var tokens = new PygmentsTokens([
            ("Token.Keyword", "def"),
            ("Token.Text", " "),
            ("Token.Name", "func")
        ]);
        AnyFormattedText aft = tokens;

        var text = aft.ToPlainText();

        Assert.Equal("def func", text);
    }

    #endregion

    #region Equality operator tests

    [Fact]
    public void OperatorEquals_SameValue_ReturnsTrue()
    {
        AnyFormattedText aft1 = "hello";
        AnyFormattedText aft2 = "hello";

        Assert.True(aft1 == aft2);
    }

    [Fact]
    public void OperatorNotEquals_DifferentValue_ReturnsTrue()
    {
        AnyFormattedText aft1 = "hello";
        AnyFormattedText aft2 = "world";

        Assert.True(aft1 != aft2);
    }

    [Fact]
    public void Equals_WithObject_WorksCorrectly()
    {
        AnyFormattedText aft = "hello";

        Assert.True(aft.Equals((object)aft));
        // When calling Equals("hello"), C# prefers the IEquatable<AnyFormattedText>
        // overload using implicit string -> AnyFormattedText conversion
        Assert.True(aft.Equals("hello"));
        // To test object equality without implicit conversion, must box first
        Assert.False(aft.Equals((object)42));
    }

    #endregion
}
