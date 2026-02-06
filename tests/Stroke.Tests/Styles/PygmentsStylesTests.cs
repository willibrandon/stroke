using Stroke.Styles;
using Xunit;

namespace Stroke.Tests.Styles;

/// <summary>
/// Tests for <see cref="PygmentsStyles"/> pre-built style instances.
/// </summary>
public class PygmentsStylesTests
{
    [Fact]
    public void DefaultDark_IsNotNull()
    {
        Assert.NotNull(PygmentsStyles.DefaultDark);
    }

    [Fact]
    public void DefaultLight_IsNotNull()
    {
        Assert.NotNull(PygmentsStyles.DefaultLight);
    }

    [Fact]
    public void DefaultDark_HasKeywordStyle()
    {
        var attrs = PygmentsStyles.DefaultDark.GetAttrsForStyleStr("class:pygments.keyword");
        Assert.NotNull(attrs.Color); // Should have a foreground color
    }

    [Fact]
    public void DefaultDark_HasCommentStyle()
    {
        var attrs = PygmentsStyles.DefaultDark.GetAttrsForStyleStr("class:pygments.comment");
        Assert.NotNull(attrs.Color);
    }

    [Fact]
    public void DefaultDark_HasStringStyle()
    {
        var attrs = PygmentsStyles.DefaultDark.GetAttrsForStyleStr("class:pygments.string");
        Assert.NotNull(attrs.Color);
    }

    [Fact]
    public void DefaultDark_HasNumberStyle()
    {
        var attrs = PygmentsStyles.DefaultDark.GetAttrsForStyleStr("class:pygments.number");
        Assert.NotNull(attrs.Color);
    }

    [Fact]
    public void DefaultDark_HasNameFunctionStyle()
    {
        var attrs = PygmentsStyles.DefaultDark.GetAttrsForStyleStr("class:pygments.name.function");
        Assert.NotNull(attrs.Color);
    }

    [Fact]
    public void DefaultDark_HasNameClassStyle()
    {
        var attrs = PygmentsStyles.DefaultDark.GetAttrsForStyleStr("class:pygments.name.class");
        Assert.NotNull(attrs.Color);
    }

    [Fact]
    public void DefaultLight_HasKeywordStyle()
    {
        var attrs = PygmentsStyles.DefaultLight.GetAttrsForStyleStr("class:pygments.keyword");
        Assert.NotNull(attrs.Color);
    }

    [Fact]
    public void DefaultDark_KeywordColor_IsBlue()
    {
        var attrs = PygmentsStyles.DefaultDark.GetAttrsForStyleStr("class:pygments.keyword");
        Assert.Equal("569cd6", attrs.Color);
    }

    [Fact]
    public void DefaultDark_CommentColor_IsGreen()
    {
        var attrs = PygmentsStyles.DefaultDark.GetAttrsForStyleStr("class:pygments.comment");
        Assert.Equal("6a9955", attrs.Color);
    }

    [Fact]
    public void DefaultDark_StringColor_IsOrange()
    {
        var attrs = PygmentsStyles.DefaultDark.GetAttrsForStyleStr("class:pygments.string");
        Assert.Equal("ce9178", attrs.Color);
    }
}
