using Stroke.Layout;
using Xunit;
using SChar = Stroke.Layout.Char;

namespace Stroke.Tests.Layout;

/// <summary>
/// Tests for Screen FillArea and AppendStyleToContent (User Story 6).
/// </summary>
public class ScreenFillTests
{
    [Fact]
    public void FillArea_EmptyRegion_SetsStyleOnCells()
    {
        var screen = new Screen();
        var region = new WritePosition(0, 0, 3, 2);

        screen.FillArea(region, "class:background");

        // All cells in region should have the style (prepended to [transparent])
        for (int row = 0; row < 2; row++)
        {
            for (int col = 0; col < 3; col++)
            {
                Assert.Equal("class:background [transparent]", screen[row, col].Style);
            }
        }
    }

    [Fact]
    public void FillArea_WithExistingContent_PrependsStyle()
    {
        var screen = new Screen();
        screen[0, 0] = SChar.Create("A", "class:text");

        var region = new WritePosition(0, 0, 1, 1);
        screen.FillArea(region, "class:highlight");

        Assert.Equal("class:highlight class:text", screen[0, 0].Style);
    }

    [Fact]
    public void FillArea_AfterTrue_AppendsStyle()
    {
        var screen = new Screen();
        screen[0, 0] = SChar.Create("A", "class:text");

        var region = new WritePosition(0, 0, 1, 1);
        screen.FillArea(region, "class:highlight", after: true);

        Assert.Equal("class:text class:highlight", screen[0, 0].Style);
    }

    [Fact]
    public void FillArea_EmptyStyle_NoOp()
    {
        var screen = new Screen();
        screen[0, 0] = SChar.Create("A", "class:original");

        var region = new WritePosition(0, 0, 1, 1);
        screen.FillArea(region, "");

        Assert.Equal("class:original", screen[0, 0].Style);
    }

    [Fact]
    public void FillArea_WhitespaceStyle_NoOp()
    {
        var screen = new Screen();
        screen[0, 0] = SChar.Create("A", "class:original");

        var region = new WritePosition(0, 0, 1, 1);
        screen.FillArea(region, "   ");

        Assert.Equal("class:original", screen[0, 0].Style);
    }

    [Fact]
    public void FillArea_ZeroWidth_NoOp()
    {
        var screen = new Screen();
        var region = new WritePosition(0, 0, 0, 5);

        screen.FillArea(region, "class:test");

        // No cells should be affected
        Assert.Equal(screen.DefaultChar, screen[0, 0]);
    }

    [Fact]
    public void FillArea_ZeroHeight_NoOp()
    {
        var screen = new Screen();
        var region = new WritePosition(0, 0, 5, 0);

        screen.FillArea(region, "class:test");

        // No cells should be affected
        Assert.Equal(screen.DefaultChar, screen[0, 0]);
    }

    [Fact]
    public void FillArea_NegativePosition_AffectsCorrectCells()
    {
        var screen = new Screen();
        // Region starts at (-1, -1), so only (0, 0), (0, 1), (1, 0), (1, 1) are visible
        // But the fill will include (-1, -1), (-1, 0), (-1, 1), (0, -1), (0, 0), (0, 1), (1, -1), (1, 0), (1, 1)
        var region = new WritePosition(-1, -1, 3, 3);

        screen.FillArea(region, "class:fill");

        Assert.Equal("class:fill [transparent]", screen[-1, -1].Style);
        Assert.Equal("class:fill [transparent]", screen[0, 0].Style);
        Assert.Equal("class:fill [transparent]", screen[1, 1].Style);
    }

    [Fact]
    public void FillArea_PreservesCharacter()
    {
        var screen = new Screen();
        screen[0, 0] = SChar.Create("A", "class:text");

        var region = new WritePosition(0, 0, 1, 1);
        screen.FillArea(region, "class:highlight");

        Assert.Equal("A", screen[0, 0].Character);
    }

    [Fact]
    public void FillArea_EmptyCell_DefaultCharWithStyle()
    {
        var screen = new Screen();
        var region = new WritePosition(0, 0, 1, 1);

        screen.FillArea(region, "class:fill");

        Assert.Equal(" ", screen[0, 0].Character); // Default char
        Assert.Equal("class:fill [transparent]", screen[0, 0].Style); // Style prepended to [transparent]
    }

    [Fact]
    public void AppendStyleToContent_ExistingCells_AppendsStyle()
    {
        var screen = new Screen();
        screen[0, 0] = SChar.Create("A", "class:text");
        screen[1, 1] = SChar.Create("B", "class:keyword");

        screen.AppendStyleToContent("class:dim");

        Assert.Equal("class:text class:dim", screen[0, 0].Style);
        Assert.Equal("class:keyword class:dim", screen[1, 1].Style);
    }

    [Fact]
    public void AppendStyleToContent_EmptyStyle_NoOp()
    {
        var screen = new Screen();
        screen[0, 0] = SChar.Create("A", "class:original");

        screen.AppendStyleToContent("");

        Assert.Equal("class:original", screen[0, 0].Style);
    }

    [Fact]
    public void AppendStyleToContent_WhitespaceStyle_NoOp()
    {
        var screen = new Screen();
        screen[0, 0] = SChar.Create("A", "class:original");

        screen.AppendStyleToContent("   ");

        Assert.Equal("class:original", screen[0, 0].Style);
    }

    [Fact]
    public void AppendStyleToContent_EmptyScreen_NoException()
    {
        var screen = new Screen();

        // Should complete without exception
        screen.AppendStyleToContent("class:dim");
    }

    [Fact]
    public void AppendStyleToContent_CellWithEmptyStyle_JustAppendsNew()
    {
        var screen = new Screen();
        screen[0, 0] = SChar.Create("A", "");

        screen.AppendStyleToContent("class:dim");

        Assert.Equal("class:dim", screen[0, 0].Style);
    }

    [Fact]
    public void AppendStyleToContent_DoesNotAffectUnsetCells()
    {
        var screen = new Screen();
        screen[5, 5] = SChar.Create("X", "class:test");

        screen.AppendStyleToContent("class:dim");

        // Only the set cell should be affected
        Assert.Equal("class:test class:dim", screen[5, 5].Style);
        Assert.Equal(screen.DefaultChar, screen[0, 0]);
    }
}
