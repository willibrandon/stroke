using Stroke.Layout;
using Xunit;
using SChar = Stroke.Layout.Char;

namespace Stroke.Tests.Layout;

/// <summary>
/// Tests for Screen sparse storage memory efficiency (Phase 11).
/// </summary>
public class ScreenMemoryTests
{
    [Fact]
    public void SparseStorage_100CellsOnLargeScreen_ApproximatelyExpectedEntries()
    {
        var screen = new Screen();

        // Write 100 cells scattered across a notional 10000x10000 screen
        var positions = new List<(int row, int col)>();
        for (int i = 0; i < 100; i++)
        {
            int row = i * 100;  // 0, 100, 200, ..., 9900
            int col = i * 100;  // 0, 100, 200, ..., 9900
            screen[row, col] = SChar.Create($"{i}", "");
            positions.Add((row, col));
        }

        // Verify all cells are retrievable
        for (int i = 0; i < 100; i++)
        {
            var (row, col) = positions[i];
            Assert.Equal($"{i}", screen[row, col].Character);
        }

        // Verify unset cells return default (no entries created for reads)
        // Written positions are (0,0), (100,100), (200,200), ..., (9900,9900)
        // So (50,50) and (150,150) should be unset
        Assert.Equal(screen.DefaultChar, screen[50, 50]);
        Assert.Equal(screen.DefaultChar, screen[150, 150]);

        // The dimensions should reflect the maximum accessed position + 1
        Assert.Equal(9901, screen.Width);
        Assert.Equal(9901, screen.Height);
    }

    [Fact]
    public void SparseStorage_DifferentRowsShareNothing()
    {
        var screen = new Screen();

        screen[0, 0] = SChar.Create("A", "");
        screen[1000, 0] = SChar.Create("B", "");

        // Each row should be independent
        Assert.Equal("A", screen[0, 0].Character);
        Assert.Equal("B", screen[1000, 0].Character);
        Assert.Equal(screen.DefaultChar, screen[500, 0]);
    }

    [Fact]
    public void SparseStorage_OnlyWrittenCellsConsumeMemory()
    {
        var screen = new Screen();

        // Write to extreme positions
        screen[0, 0] = SChar.Create("corner", "");
        screen[int.MaxValue - 1, 0] = SChar.Create("far", "");

        // These should work without allocating O(int.MaxValue) memory
        Assert.Equal("corner", screen[0, 0].Character);
        Assert.Equal("far", screen[int.MaxValue - 1, 0].Character);

        // Unset middle positions return default
        Assert.Equal(screen.DefaultChar, screen[1000000, 0]);
    }

    [Fact]
    public void SparseStorage_NegativeCoordinates_NoWastedMemory()
    {
        var screen = new Screen();

        // Write at negative coordinates
        screen[-1000, -1000] = SChar.Create("neg", "");
        screen[1000, 1000] = SChar.Create("pos", "");

        // Both should be stored independently
        Assert.Equal("neg", screen[-1000, -1000].Character);
        Assert.Equal("pos", screen[1000, 1000].Character);

        // Should not affect dimensions (only positive coords expand)
        Assert.Equal(1001, screen.Width);
        Assert.Equal(1001, screen.Height);
    }

    [Fact]
    public void SparseStorage_Clear_ReleasesMemory()
    {
        var screen = new Screen();

        // Fill with data
        for (int i = 0; i < 100; i++)
        {
            screen[i, i] = SChar.Create($"{i}", "");
        }

        screen.Clear();

        // All data should be cleared
        Assert.Equal(screen.DefaultChar, screen[0, 0]);
        Assert.Equal(screen.DefaultChar, screen[99, 99]);
    }

    [Fact]
    public void SparseStorage_ReadsDoNotCreateEntries()
    {
        var screen = new Screen(initialWidth: 80, initialHeight: 24);

        // Read from many positions
        for (int i = 0; i < 1000; i++)
        {
            _ = screen[i, i];
        }

        // Dimensions should remain at initial values (reads don't expand)
        Assert.Equal(80, screen.Width);
        Assert.Equal(24, screen.Height);

        // Writing a single cell should only affect that one
        screen[500, 500] = SChar.Create("X", "");
        Assert.Equal(501, screen.Width);
        Assert.Equal(501, screen.Height);
    }
}
