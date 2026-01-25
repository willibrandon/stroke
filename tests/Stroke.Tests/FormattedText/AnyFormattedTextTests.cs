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
}
