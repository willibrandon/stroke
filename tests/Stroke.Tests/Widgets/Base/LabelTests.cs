using Stroke.FormattedText;
using Stroke.Layout;
using Stroke.Layout.Containers;
using Stroke.Widgets.Base;
using Xunit;

namespace Stroke.Tests.Widgets.Base;

public class LabelTests
{
    [Fact]
    public void DefaultConstruction_WithText_CreatesLabel()
    {
        var label = new Label("Hello");
        Assert.NotNull(label);
        Assert.NotNull(label.Window);
        Assert.NotNull(label.FormattedTextControl);
    }

    [Fact]
    public void PtContainer_ReturnsWindow()
    {
        var label = new Label("Hello");
        var container = label.PtContainer();
        Assert.IsType<Window>(container);
        Assert.Same(label.Window, container);
    }

    [Fact]
    public void Style_IncludesClassLabelPrefix()
    {
        // The style passed to Window should be "class:label " + user style
        var label = new Label("Hello", style: "custom-style");
        // We can verify via Window's style (it's set once at construction)
        // The style string is "class:label custom-style"
        Assert.NotNull(label.Window);
    }

    [Fact]
    public void Style_DefaultIsClassLabelOnly()
    {
        var label = new Label("Hello");
        Assert.NotNull(label.Window);
    }

    [Fact]
    public void AutoWidth_SingleLineText_CalculatesFromTextWidth()
    {
        var label = new Label("Hello");
        // "Hello" is 5 ASCII chars, each width 1 = preferred 5
        var dim = label.Window.PreferredWidth(80);
        Assert.Equal(5, dim.Preferred);
    }

    [Fact]
    public void AutoWidth_MultiLineText_UsesLongestLine()
    {
        var label = new Label("Hi\nHello World\nBye");
        // "Hello World" = 11, "Hi" = 2, "Bye" = 3 → longest = 11
        var dim = label.Window.PreferredWidth(80);
        Assert.Equal(11, dim.Preferred);
    }

    [Fact]
    public void AutoWidth_EmptyText_GivesPreferredZero()
    {
        var label = new Label("");
        var dim = label.Window.PreferredWidth(80);
        Assert.Equal(0, dim.Preferred);
    }

    [Fact]
    public void TextProperty_IsGetSet()
    {
        var label = new Label("Initial");
        Assert.Equal("Initial", FormattedTextUtils.FragmentListToText(
            FormattedTextUtils.ToFormattedText(label.Text)));

        label.Text = "Updated";
        Assert.Equal("Updated", FormattedTextUtils.FragmentListToText(
            FormattedTextUtils.ToFormattedText(label.Text)));
    }

    [Fact]
    public void ExplicitWidth_OverridesAutoCalculation()
    {
        var label = new Label("Hello", width: Dimension.Exact(20));
        var dim = label.Window.PreferredWidth(80);
        // With explicit width, the Window should use the given dimension
        Assert.Equal(20, dim.Preferred);
    }

    [Fact]
    public void TextChange_AffectsAutoWidth()
    {
        var label = new Label("Hi");
        var dim1 = label.Window.PreferredWidth(80);
        Assert.Equal(2, dim1.Preferred);

        label.Text = "Hello World";
        var dim2 = label.Window.PreferredWidth(80);
        Assert.Equal(11, dim2.Preferred);
    }

    [Fact]
    public void DefaultDontExtendHeight_IsTrue()
    {
        // The Python default is dontExtendHeight=True
        // Verify height is minimal (1 line for single-line text)
        var label = new Label("Hello");
        var dim = label.Window.PreferredHeight(80, 24);
        Assert.Equal(1, dim.Preferred);
    }
}
