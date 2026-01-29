using Stroke.Layout;
using Xunit;
using SChar = Stroke.Layout.Char;

namespace Stroke.Tests.Layout;

/// <summary>
/// Tests for Screen indexer and dimension tracking (User Story 1).
/// </summary>
public class ScreenTests
{
    [Fact]
    public void Constructor_Default_ZeroDimensions()
    {
        var screen = new Screen();
        Assert.Equal(0, screen.Width);
        Assert.Equal(0, screen.Height);
    }

    [Fact]
    public void Constructor_WithDimensions_SetsInitialValues()
    {
        var screen = new Screen(initialWidth: 80, initialHeight: 24);
        Assert.Equal(80, screen.Width);
        Assert.Equal(24, screen.Height);
    }

    [Fact]
    public void Constructor_NegativeWidth_ClampedToZero()
    {
        var screen = new Screen(initialWidth: -10, initialHeight: 24);
        Assert.Equal(0, screen.Width);
        Assert.Equal(24, screen.Height);
    }

    [Fact]
    public void Constructor_NegativeHeight_ClampedToZero()
    {
        var screen = new Screen(initialWidth: 80, initialHeight: -10);
        Assert.Equal(80, screen.Width);
        Assert.Equal(0, screen.Height);
    }

    [Fact]
    public void Constructor_NullDefaultChar_UsesSpaceWithTransparent()
    {
        var screen = new Screen(defaultChar: null);
        Assert.Equal(" ", screen.DefaultChar.Character);
        Assert.Equal(SChar.Transparent, screen.DefaultChar.Style);
    }

    [Fact]
    public void Constructor_CustomDefaultChar_UsesProvided()
    {
        var customChar = SChar.Create(".", "class:background");
        var screen = new Screen(defaultChar: customChar);
        Assert.Same(customChar, screen.DefaultChar);
    }

    [Fact]
    public void Indexer_UnsetPosition_ReturnsDefaultChar()
    {
        var screen = new Screen();
        var ch = screen[5, 10];
        Assert.Equal(screen.DefaultChar, ch);
    }

    [Fact]
    public void Indexer_SetAndGet_ReturnsStoredChar()
    {
        var screen = new Screen();
        var ch = SChar.Create("A", "class:keyword");
        screen[5, 10] = ch;
        Assert.Equal(ch, screen[5, 10]);
    }

    [Fact]
    public void Indexer_MultiplePositions_IndependentStorage()
    {
        var screen = new Screen();
        var ch1 = SChar.Create("A", "style1");
        var ch2 = SChar.Create("B", "style2");

        screen[0, 0] = ch1;
        screen[1, 1] = ch2;

        Assert.Equal(ch1, screen[0, 0]);
        Assert.Equal(ch2, screen[1, 1]);
        Assert.Equal(screen.DefaultChar, screen[0, 1]);
    }

    [Fact]
    public void Indexer_OverwritePosition_ReturnsNewValue()
    {
        var screen = new Screen();
        var ch1 = SChar.Create("A", "style1");
        var ch2 = SChar.Create("B", "style2");

        screen[5, 10] = ch1;
        screen[5, 10] = ch2;

        Assert.Equal(ch2, screen[5, 10]);
    }

    [Fact]
    public void Indexer_NegativeCoords_StoresAndRetrieves()
    {
        var screen = new Screen();
        var ch = SChar.Create("X", "class:test");

        screen[-5, -10] = ch;

        Assert.Equal(ch, screen[-5, -10]);
    }

    [Fact]
    public void Indexer_NegativeCoords_DoesNotExpandDimensions()
    {
        var screen = new Screen(initialWidth: 80, initialHeight: 24);
        var ch = SChar.Create("X", "class:test");

        screen[-5, -10] = ch;

        // Dimensions should not be affected by negative coords
        Assert.Equal(80, screen.Width);
        Assert.Equal(24, screen.Height);
    }

    [Fact]
    public void Indexer_LargeCoords_StoresAndRetrieves()
    {
        var screen = new Screen();
        var ch = SChar.Create("X", "class:test");

        screen[1000000, 1000000] = ch;

        Assert.Equal(ch, screen[1000000, 1000000]);
    }

    [Fact]
    public void Indexer_IntMaxValue_ExpandsDimensions()
    {
        var screen = new Screen();
        var ch = SChar.Create("X", "class:test");

        // Note: This will expand width/height to int.MaxValue, which may be a problem
        // Using smaller large values for practical testing
        screen[1000, 2000] = ch;

        Assert.Equal(2001, screen.Width);  // col + 1
        Assert.Equal(1001, screen.Height); // row + 1
    }

    [Fact]
    public void Indexer_ZeroCoord_ExpandsDimensionsToOne()
    {
        var screen = new Screen();
        var ch = SChar.Create("X", "");

        screen[0, 0] = ch;

        Assert.Equal(1, screen.Width);
        Assert.Equal(1, screen.Height);
    }

    [Fact]
    public void Indexer_ReadUnset_DoesNotCreateEntry()
    {
        var screen = new Screen(initialWidth: 80, initialHeight: 24);

        // Reading unset positions should not change dimensions or create entries
        _ = screen[100, 100];

        // Dimensions should remain at initial values (reading doesn't expand)
        Assert.Equal(80, screen.Width);
        Assert.Equal(24, screen.Height);
    }

    [Fact]
    public void Dimensions_AutoExpandOnWrite()
    {
        var screen = new Screen();

        screen[10, 20] = SChar.Create("A", "");

        Assert.Equal(21, screen.Width);   // col + 1
        Assert.Equal(11, screen.Height);  // row + 1
    }

    [Fact]
    public void Dimensions_OnlyExpandNeverShrink()
    {
        var screen = new Screen();

        screen[10, 20] = SChar.Create("A", "");
        screen[5, 5] = SChar.Create("B", "");

        // Should still be max values
        Assert.Equal(21, screen.Width);
        Assert.Equal(11, screen.Height);
    }

    [Fact]
    public void Width_CanBeSetDirectly()
    {
        var screen = new Screen();
        screen.Width = 100;
        Assert.Equal(100, screen.Width);
    }

    [Fact]
    public void Height_CanBeSetDirectly()
    {
        var screen = new Screen();
        screen.Height = 50;
        Assert.Equal(50, screen.Height);
    }

    [Fact]
    public void ShowCursor_DefaultTrue()
    {
        var screen = new Screen();
        Assert.True(screen.ShowCursor);
    }

    [Fact]
    public void ShowCursor_CanBeSet()
    {
        var screen = new Screen();
        screen.ShowCursor = false;
        Assert.False(screen.ShowCursor);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(100, 100)]
    [InlineData(-100, -100)]
    [InlineData(int.MaxValue - 1, int.MaxValue - 1)]
    public void Indexer_VariousCoords_WorkCorrectly(int row, int col)
    {
        var screen = new Screen();
        var ch = SChar.Create("X", "test");

        screen[row, col] = ch;

        Assert.Equal(ch, screen[row, col]);
    }

    [Fact]
    public void SparseStorage_OnlyStoresWrittenCells()
    {
        var screen = new Screen();

        // Write 10 cells scattered across a large area
        for (int i = 0; i < 10; i++)
        {
            screen[i * 100, i * 100] = SChar.Create("X", "");
        }

        // Reading unset cells returns default
        // Written positions are (0,0), (100,100), ..., (900,900)
        // So (50,50) and (150,150) should be unset
        Assert.Equal(screen.DefaultChar, screen[50, 50]);
        Assert.Equal(screen.DefaultChar, screen[150, 150]);

        // Written cells are stored
        Assert.Equal("X", screen[0, 0].Character);
        Assert.Equal("X", screen[900, 900].Character);
    }
}
