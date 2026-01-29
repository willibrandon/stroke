using Stroke.Layout;
using Xunit;

namespace Stroke.Tests.Layout;

/// <summary>
/// Tests for Screen visible windows tracking (Phase 10).
/// </summary>
public class ScreenWindowTests
{
    [Fact]
    public void VisibleWindows_Initially_Empty()
    {
        var screen = new Screen();

        Assert.Empty(screen.VisibleWindows);
    }

    [Fact]
    public void VisibleWindowsToWritePositions_AddWindow_AppearsInVisibleWindows()
    {
        var screen = new Screen();
        var window = new TestWindow("main");
        var position = new WritePosition(0, 0, 80, 24);

        screen.VisibleWindowsToWritePositions[window] = position;

        Assert.Single(screen.VisibleWindows);
        Assert.Contains(window, screen.VisibleWindows);
    }

    [Fact]
    public void VisibleWindowsToWritePositions_MultipleWindows_AllAppear()
    {
        var screen = new Screen();
        var window1 = new TestWindow("window1");
        var window2 = new TestWindow("window2");
        var window3 = new TestWindow("window3");

        screen.VisibleWindowsToWritePositions[window1] = new WritePosition(0, 0, 40, 24);
        screen.VisibleWindowsToWritePositions[window2] = new WritePosition(40, 0, 40, 24);
        screen.VisibleWindowsToWritePositions[window3] = new WritePosition(0, 24, 80, 5);

        Assert.Equal(3, screen.VisibleWindows.Count);
        Assert.Contains(window1, screen.VisibleWindows);
        Assert.Contains(window2, screen.VisibleWindows);
        Assert.Contains(window3, screen.VisibleWindows);
    }

    [Fact]
    public void VisibleWindowsToWritePositions_RemoveWindow_DisappearsFromVisibleWindows()
    {
        var screen = new Screen();
        var window = new TestWindow("main");

        screen.VisibleWindowsToWritePositions[window] = new WritePosition(0, 0, 80, 24);
        screen.VisibleWindowsToWritePositions.Remove(window);

        Assert.Empty(screen.VisibleWindows);
    }

    [Fact]
    public void VisibleWindows_ReturnsSnapshot()
    {
        var screen = new Screen();
        var window1 = new TestWindow("window1");
        var window2 = new TestWindow("window2");

        screen.VisibleWindowsToWritePositions[window1] = new WritePosition(0, 0, 80, 24);
        var snapshot = screen.VisibleWindows;

        // Add another window after getting snapshot
        screen.VisibleWindowsToWritePositions[window2] = new WritePosition(0, 0, 80, 24);

        // Snapshot should not be affected (depending on implementation)
        // Note: The current implementation returns a new list each time
        Assert.Single(snapshot);
        Assert.Equal(2, screen.VisibleWindows.Count);
    }

    [Fact]
    public void VisibleWindowsToWritePositions_UpdatePosition_ReflectsChange()
    {
        var screen = new Screen();
        var window = new TestWindow("main");

        screen.VisibleWindowsToWritePositions[window] = new WritePosition(0, 0, 40, 24);
        screen.VisibleWindowsToWritePositions[window] = new WritePosition(0, 0, 80, 24);

        Assert.Equal(80, screen.VisibleWindowsToWritePositions[window].Width);
    }

    [Fact]
    public void VisibleWindowsToWritePositions_SameWindowReaddedAfterRemoval()
    {
        var screen = new Screen();
        var window = new TestWindow("main");

        screen.VisibleWindowsToWritePositions[window] = new WritePosition(0, 0, 40, 24);
        screen.VisibleWindowsToWritePositions.Remove(window);
        screen.VisibleWindowsToWritePositions[window] = new WritePosition(10, 10, 60, 20);

        Assert.Single(screen.VisibleWindows);
        Assert.Equal(new WritePosition(10, 10, 60, 20), screen.VisibleWindowsToWritePositions[window]);
    }

    [Fact]
    public void Clear_ClearsVisibleWindows()
    {
        var screen = new Screen();
        var window = new TestWindow("main");
        screen.VisibleWindowsToWritePositions[window] = new WritePosition(0, 0, 80, 24);

        screen.Clear();

        Assert.Empty(screen.VisibleWindows);
        Assert.Empty(screen.VisibleWindowsToWritePositions);
    }
}
