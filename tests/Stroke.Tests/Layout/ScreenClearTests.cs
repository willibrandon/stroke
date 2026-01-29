using Stroke.Core.Primitives;
using Stroke.Layout;
using Xunit;
using SChar = Stroke.Layout.Char;

namespace Stroke.Tests.Layout;

/// <summary>
/// Tests for Screen.Clear() method (User Story 7).
/// </summary>
public class ScreenClearTests
{
    [Fact]
    public void Clear_ResetsDataBuffer()
    {
        var screen = new Screen();
        screen[0, 0] = SChar.Create("A", "class:test");
        screen[5, 5] = SChar.Create("B", "class:test");

        screen.Clear();

        Assert.Equal(screen.DefaultChar, screen[0, 0]);
        Assert.Equal(screen.DefaultChar, screen[5, 5]);
    }

    [Fact]
    public void Clear_ResetsZeroWidthEscapes()
    {
        var screen = new Screen();
        screen.AddZeroWidthEscape(0, 0, "escape1");
        screen.AddZeroWidthEscape(5, 5, "escape2");

        screen.Clear();

        Assert.Equal(string.Empty, screen.GetZeroWidthEscapes(0, 0));
        Assert.Equal(string.Empty, screen.GetZeroWidthEscapes(5, 5));
    }

    [Fact]
    public void Clear_ResetsCursorPositions()
    {
        var screen = new Screen();
        var window = new TestWindow("main");
        screen.SetCursorPosition(window, new Point(10, 5));

        screen.Clear();

        Assert.Equal(Point.Zero, screen.GetCursorPosition(window));
    }

    [Fact]
    public void Clear_ResetsMenuPositions()
    {
        var screen = new Screen();
        var window = new TestWindow("main");
        screen.SetMenuPosition(window, new Point(10, 7));
        screen.SetCursorPosition(window, new Point(10, 5)); // Fallback

        screen.Clear();

        // Both cleared, should return Point.Zero
        Assert.Equal(Point.Zero, screen.GetMenuPosition(window));
    }

    [Fact]
    public void Clear_ResetsVisibleWindows()
    {
        var screen = new Screen();
        var window = new TestWindow("main");
        screen.VisibleWindowsToWritePositions[window] = new WritePosition(0, 0, 80, 24);

        screen.Clear();

        Assert.Empty(screen.VisibleWindows);
    }

    [Fact]
    public void Clear_ResetsDrawQueue()
    {
        var screen = new Screen();
        var executed = false;
        screen.DrawWithZIndex(5, () => executed = true);

        screen.Clear();
        screen.DrawAllFloats();

        Assert.False(executed);
    }

    [Fact]
    public void Clear_ResetsDimensionsToInitial()
    {
        var screen = new Screen(initialWidth: 80, initialHeight: 24);
        screen[100, 100] = SChar.Create("X", "");

        // Dimensions expanded
        Assert.Equal(101, screen.Width);
        Assert.Equal(101, screen.Height);

        screen.Clear();

        // Dimensions reset to initial
        Assert.Equal(80, screen.Width);
        Assert.Equal(24, screen.Height);
    }

    [Fact]
    public void Clear_ResetsDimensionsToZero_WhenNoInitial()
    {
        var screen = new Screen(); // No initial dimensions
        screen[100, 100] = SChar.Create("X", "");

        screen.Clear();

        Assert.Equal(0, screen.Width);
        Assert.Equal(0, screen.Height);
    }

    [Fact]
    public void Clear_PreservesDefaultChar()
    {
        var customDefault = SChar.Create(".", "class:background");
        var screen = new Screen(defaultChar: customDefault);
        screen[0, 0] = SChar.Create("X", "");

        screen.Clear();

        Assert.Same(customDefault, screen.DefaultChar);
        Assert.Equal(customDefault, screen[0, 0]); // Unset positions return default
    }

    [Fact]
    public void Clear_PreservesShowCursor()
    {
        var screen = new Screen();
        screen.ShowCursor = false;
        screen[0, 0] = SChar.Create("X", "");

        screen.Clear();

        Assert.False(screen.ShowCursor);
    }

    [Fact]
    public void Clear_AllowsReuse()
    {
        var screen = new Screen(initialWidth: 80, initialHeight: 24);
        var window = new TestWindow("main");

        // First use
        screen[0, 0] = SChar.Create("A", "");
        screen.SetCursorPosition(window, new Point(10, 5));

        screen.Clear();

        // Second use
        screen[0, 0] = SChar.Create("B", "");
        screen.SetCursorPosition(window, new Point(20, 15));

        Assert.Equal("B", screen[0, 0].Character);
        Assert.Equal(new Point(20, 15), screen.GetCursorPosition(window));
    }
}
