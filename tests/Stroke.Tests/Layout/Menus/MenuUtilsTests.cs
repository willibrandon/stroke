using Stroke.FormattedText;
using Stroke.Layout.Menus;
using Xunit;

using CompletionItem = Stroke.Completion.Completion;
using FText = Stroke.FormattedText.FormattedText;

namespace Stroke.Tests.Layout.Menus;

/// <summary>
/// Tests for MenuUtils static utility class (GetMenuItemFragments, TrimFormattedText).
/// </summary>
public sealed class MenuUtilsTests
{
    #region GetMenuItemFragments Tests

    [Fact]
    public void GetMenuItemFragments_SimpleCompletion_ReturnsStyledFragments()
    {
        var completion = new CompletionItem("hello");
        var fragments = MenuUtils.GetMenuItemFragments(completion, isCurrentCompletion: false, width: 10);

        Assert.NotEmpty(fragments);
        // All fragments should have the completion-menu.completion style class
        foreach (var f in fragments)
        {
            Assert.Contains("class:completion-menu.completion", f.Style);
        }
    }

    [Fact]
    public void GetMenuItemFragments_CurrentCompletion_UsesCurrentStyle()
    {
        var completion = new CompletionItem("hello");
        var fragments = MenuUtils.GetMenuItemFragments(completion, isCurrentCompletion: true, width: 10);

        Assert.NotEmpty(fragments);
        foreach (var f in fragments)
        {
            Assert.Contains("class:completion-menu.completion.current", f.Style);
        }
    }

    [Fact]
    public void GetMenuItemFragments_WithCustomStyle_IncludesCompletionStyle()
    {
        var completion = new CompletionItem("hello", style: "bold");
        var fragments = MenuUtils.GetMenuItemFragments(completion, isCurrentCompletion: false, width: 10);

        // Should include both the menu class and the completion's custom style
        var allStyles = string.Join(" ", fragments.Select(f => f.Style));
        Assert.Contains("bold", allStyles);
    }

    [Fact]
    public void GetMenuItemFragments_CurrentWithSelectedStyle_IncludesBothStyles()
    {
        var completion = new CompletionItem("hello", style: "fg:red", selectedStyle: "bg:blue");
        var fragments = MenuUtils.GetMenuItemFragments(completion, isCurrentCompletion: true, width: 15);

        var allStyles = string.Join(" ", fragments.Select(f => f.Style));
        Assert.Contains("fg:red", allStyles);
        Assert.Contains("bg:blue", allStyles);
    }

    [Fact]
    public void GetMenuItemFragments_LeadingSpace_AlwaysPresent()
    {
        var completion = new CompletionItem("test");
        var fragments = MenuUtils.GetMenuItemFragments(completion, isCurrentCompletion: false, width: 10);

        // The raw (unstyled) text of the first fragment should start with a space
        var allText = string.Concat(fragments.Select(f => f.Text));
        Assert.StartsWith(" ", allText);
    }

    [Fact]
    public void GetMenuItemFragments_SpaceAfterTrue_AdjustsTrimWidth()
    {
        var completion = new CompletionItem("hello");
        var withSpace = MenuUtils.GetMenuItemFragments(completion, isCurrentCompletion: false, width: 10, spaceAfter: true);
        var withoutSpace = MenuUtils.GetMenuItemFragments(completion, isCurrentCompletion: false, width: 10, spaceAfter: false);

        // Both should produce valid output
        Assert.NotEmpty(withSpace);
        Assert.NotEmpty(withoutSpace);
    }

    [Fact]
    public void GetMenuItemFragments_LongCompletion_IsTrimmed()
    {
        var completion = new CompletionItem("averylongcompletiontext");
        var fragments = MenuUtils.GetMenuItemFragments(completion, isCurrentCompletion: false, width: 8);

        // Should contain "..." ellipsis
        var allText = string.Concat(fragments.Select(f => f.Text));
        Assert.Contains("...", allText);
    }

    [Fact]
    public void GetMenuItemFragments_WithDisplayProperty_UsesDisplay()
    {
        AnyFormattedText display = "custom display";
        var completion = new CompletionItem("text", display: display);
        var fragments = MenuUtils.GetMenuItemFragments(completion, isCurrentCompletion: false, width: 20);

        var allText = string.Concat(fragments.Select(f => f.Text));
        Assert.Contains("custom display", allText);
    }

    [Fact]
    public void GetMenuItemFragments_Width1_ProducesOutput()
    {
        var completion = new CompletionItem("x");
        var fragments = MenuUtils.GetMenuItemFragments(completion, isCurrentCompletion: false, width: 1);

        Assert.NotEmpty(fragments);
    }

    #endregion

    #region TrimFormattedText Tests

    [Fact]
    public void TrimFormattedText_FitsWithinWidth_ReturnsUnchanged()
    {
        var fragments = new List<StyleAndTextTuple>
        {
            new("", "hello")
        };

        var (result, width) = MenuUtils.TrimFormattedText(fragments, 10);

        Assert.Equal(5, width);
        // Content preserved
        var text = string.Concat(result.Select(f => f.Text));
        Assert.Equal("hello", text);
    }

    [Fact]
    public void TrimFormattedText_ExactFit_ReturnsUnchanged()
    {
        var fragments = new List<StyleAndTextTuple>
        {
            new("", "hello")
        };

        var (result, width) = MenuUtils.TrimFormattedText(fragments, 5);

        Assert.Equal(5, width);
        var text = string.Concat(result.Select(f => f.Text));
        Assert.Equal("hello", text);
    }

    [Fact]
    public void TrimFormattedText_ExceedsWidth_TrimmedWithEllipsis()
    {
        var fragments = new List<StyleAndTextTuple>
        {
            new("", "hello world")
        };

        var (result, width) = MenuUtils.TrimFormattedText(fragments, 8);

        var text = string.Concat(result.Select(f => f.Text));
        Assert.Contains("...", text);
        // Width should be <= maxWidth
        Assert.True(width <= 8);
    }

    [Fact]
    public void TrimFormattedText_ZeroWidth_ReturnsEmpty()
    {
        var fragments = new List<StyleAndTextTuple>
        {
            new("", "hello")
        };

        var (result, width) = MenuUtils.TrimFormattedText(fragments, 0);

        Assert.Empty(result);
        Assert.Equal(0, width);
    }

    [Fact]
    public void TrimFormattedText_NegativeWidth_ReturnsEmpty()
    {
        var fragments = new List<StyleAndTextTuple>
        {
            new("", "hello")
        };

        var (result, width) = MenuUtils.TrimFormattedText(fragments, -5);

        Assert.Empty(result);
        Assert.Equal(0, width);
    }

    [Fact]
    public void TrimFormattedText_EmptyInput_ReturnsEmpty()
    {
        var fragments = new List<StyleAndTextTuple>();

        var (result, width) = MenuUtils.TrimFormattedText(fragments, 10);

        Assert.Equal(0, width);
    }

    [Fact]
    public void TrimFormattedText_MultipleFragments_PreservesStyles()
    {
        var fragments = new List<StyleAndTextTuple>
        {
            new("bold", "he"),
            new("italic", "llo")
        };

        var (result, width) = MenuUtils.TrimFormattedText(fragments, 20);

        Assert.Equal(5, width);
        // Should have the original fragments
        var text = string.Concat(result.Select(f => f.Text));
        Assert.Equal("hello", text);
    }

    [Fact]
    public void TrimFormattedText_TrimAcrossFragments_AppendsEllipsis()
    {
        var fragments = new List<StyleAndTextTuple>
        {
            new("", "helloworld")
        };

        var (result, width) = MenuUtils.TrimFormattedText(fragments, 6);

        var text = string.Concat(result.Select(f => f.Text));
        Assert.Contains("...", text);
    }

    [Fact]
    public void TrimFormattedText_Width3_OnlyEllipsis()
    {
        // With maxWidth=3, remainingWidth=0, so no chars fit, just "..."
        var fragments = new List<StyleAndTextTuple>
        {
            new("", "hello")
        };

        var (result, width) = MenuUtils.TrimFormattedText(fragments, 3);

        var text = string.Concat(result.Select(f => f.Text));
        Assert.Equal("...", text);
    }

    #endregion
}
