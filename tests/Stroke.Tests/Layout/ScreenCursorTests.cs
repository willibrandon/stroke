using Stroke.Core.Primitives;
using Stroke.Layout;
using Xunit;

namespace Stroke.Tests.Layout;

/// <summary>
/// Tests for Screen cursor and menu position tracking (User Story 2).
/// </summary>
public class ScreenCursorTests
{
    [Fact]
    public void SetCursorPosition_ValidWindow_StoresPosition()
    {
        var screen = new Screen();
        var window = new TestWindow("main");
        var position = new Point(10, 5);

        screen.SetCursorPosition(window, position);

        Assert.Equal(position, screen.GetCursorPosition(window));
    }

    [Fact]
    public void SetCursorPosition_NullWindow_ThrowsArgumentNullException()
    {
        var screen = new Screen();

        Assert.Throws<ArgumentNullException>(() =>
            screen.SetCursorPosition(null!, new Point(0, 0)));
    }

    [Fact]
    public void GetCursorPosition_UnsetWindow_ReturnsPointZero()
    {
        var screen = new Screen();
        var window = new TestWindow("main");

        Assert.Equal(Point.Zero, screen.GetCursorPosition(window));
    }

    [Fact]
    public void GetCursorPosition_NullWindow_ThrowsArgumentNullException()
    {
        var screen = new Screen();

        Assert.Throws<ArgumentNullException>(() =>
            screen.GetCursorPosition(null!));
    }

    [Fact]
    public void SetCursorPosition_MultipleWindows_IndependentPositions()
    {
        var screen = new Screen();
        var window1 = new TestWindow("window1");
        var window2 = new TestWindow("window2");
        var window3 = new TestWindow("window3");

        screen.SetCursorPosition(window1, new Point(1, 1));
        screen.SetCursorPosition(window2, new Point(2, 2));
        screen.SetCursorPosition(window3, new Point(3, 3));

        Assert.Equal(new Point(1, 1), screen.GetCursorPosition(window1));
        Assert.Equal(new Point(2, 2), screen.GetCursorPosition(window2));
        Assert.Equal(new Point(3, 3), screen.GetCursorPosition(window3));
    }

    [Fact]
    public void SetCursorPosition_Overwrite_UpdatesPosition()
    {
        var screen = new Screen();
        var window = new TestWindow("main");

        screen.SetCursorPosition(window, new Point(10, 5));
        screen.SetCursorPosition(window, new Point(20, 15));

        Assert.Equal(new Point(20, 15), screen.GetCursorPosition(window));
    }

    [Fact]
    public void SetMenuPosition_ValidWindow_StoresPosition()
    {
        var screen = new Screen();
        var window = new TestWindow("main");
        var position = new Point(10, 7);

        screen.SetMenuPosition(window, position);

        Assert.Equal(position, screen.GetMenuPosition(window));
    }

    [Fact]
    public void SetMenuPosition_NullWindow_ThrowsArgumentNullException()
    {
        var screen = new Screen();

        Assert.Throws<ArgumentNullException>(() =>
            screen.SetMenuPosition(null!, new Point(0, 0)));
    }

    [Fact]
    public void GetMenuPosition_UnsetMenuButSetCursor_ReturnsCursorPosition()
    {
        var screen = new Screen();
        var window = new TestWindow("main");

        screen.SetCursorPosition(window, new Point(10, 5));

        // Menu position falls back to cursor position
        Assert.Equal(new Point(10, 5), screen.GetMenuPosition(window));
    }

    [Fact]
    public void GetMenuPosition_BothUnset_ReturnsPointZero()
    {
        var screen = new Screen();
        var window = new TestWindow("main");

        Assert.Equal(Point.Zero, screen.GetMenuPosition(window));
    }

    [Fact]
    public void GetMenuPosition_NullWindow_ThrowsArgumentNullException()
    {
        var screen = new Screen();

        Assert.Throws<ArgumentNullException>(() =>
            screen.GetMenuPosition(null!));
    }

    [Fact]
    public void GetMenuPosition_MenuSetCursorSet_ReturnsMenuPosition()
    {
        var screen = new Screen();
        var window = new TestWindow("main");

        screen.SetCursorPosition(window, new Point(10, 5));
        screen.SetMenuPosition(window, new Point(10, 7));

        // Menu position takes precedence
        Assert.Equal(new Point(10, 7), screen.GetMenuPosition(window));
    }

    [Fact]
    public void SetMenuPosition_MultipleWindows_IndependentPositions()
    {
        var screen = new Screen();
        var window1 = new TestWindow("window1");
        var window2 = new TestWindow("window2");

        screen.SetMenuPosition(window1, new Point(1, 1));
        screen.SetMenuPosition(window2, new Point(2, 2));

        Assert.Equal(new Point(1, 1), screen.GetMenuPosition(window1));
        Assert.Equal(new Point(2, 2), screen.GetMenuPosition(window2));
    }

    [Fact]
    public void CursorPosition_NegativeCoords_Valid()
    {
        var screen = new Screen();
        var window = new TestWindow("main");
        var position = new Point(-5, -10);

        screen.SetCursorPosition(window, position);

        Assert.Equal(position, screen.GetCursorPosition(window));
    }

    [Fact]
    public void MenuPosition_NegativeCoords_Valid()
    {
        var screen = new Screen();
        var window = new TestWindow("main");
        var position = new Point(-5, -10);

        screen.SetMenuPosition(window, position);

        Assert.Equal(position, screen.GetMenuPosition(window));
    }
}
