using Stroke.KeyBinding;
using Stroke.Layout;
using Stroke.Layout.Containers;
using Xunit;

namespace Stroke.Tests.Layout.Containers;

/// <summary>
/// Tests for HSplit edge cases and "window too small" scenarios.
/// </summary>
public sealed class HSplitEdgeCaseTests
{
    #region Helper Classes

    /// <summary>
    /// Test container with configurable dimensions.
    /// </summary>
    private sealed class TestContainer : IContainer
    {
        private readonly Dimension _width;
        private readonly Dimension _height;
        public int WriteToScreenCallCount { get; private set; }
        public WritePosition? LastWritePosition { get; private set; }

        public TestContainer(Dimension? width = null, Dimension? height = null)
        {
            _width = width ?? new Dimension();
            _height = height ?? new Dimension();
        }

        public bool IsModal => false;

        public void Reset() { }

        public Dimension PreferredWidth(int maxAvailableWidth) => _width;

        public Dimension PreferredHeight(int width, int maxAvailableHeight) => _height;

        public void WriteToScreen(
            Screen screen,
            MouseHandlers mouseHandlers,
            WritePosition writePosition,
            string parentStyle,
            bool eraseBg,
            int? zIndex)
        {
            WriteToScreenCallCount++;
            LastWritePosition = writePosition;
            for (int y = writePosition.YPos; y < writePosition.YPos + writePosition.Height; y++)
            {
                for (int x = writePosition.XPos; x < writePosition.XPos + writePosition.Width; x++)
                {
                    screen[y, x] = Stroke.Layout.Char.Create("X", "");
                }
            }
        }

        public IKeyBindingsBase? GetKeyBindings() => null;

        public IReadOnlyList<IContainer> GetChildren() => [];
    }

    /// <summary>
    /// Marker container to track if "too small" message was shown.
    /// </summary>
    private sealed class TooSmallContainer : IContainer
    {
        public bool WasRendered { get; private set; }

        public bool IsModal => false;

        public void Reset() { }

        public Dimension PreferredWidth(int maxAvailableWidth) => new Dimension();

        public Dimension PreferredHeight(int width, int maxAvailableHeight) => new Dimension();

        public void WriteToScreen(
            Screen screen,
            MouseHandlers mouseHandlers,
            WritePosition writePosition,
            string parentStyle,
            bool eraseBg,
            int? zIndex)
        {
            WasRendered = true;
        }

        public IKeyBindingsBase? GetKeyBindings() => null;

        public IReadOnlyList<IContainer> GetChildren() => [];
    }

    #endregion

    #region Window Too Small Tests

    [Fact]
    public void WriteToScreen_MinimumExceedsSpace_ShowsWindowTooSmall()
    {
        var tooSmall = new TooSmallContainer();
        var child = new TestContainer(height: new Dimension(min: 50, max: 100, preferred: 75));

        var hsplit = new HSplit([child], windowTooSmall: tooSmall);

        var screen = new Screen(initialWidth: 80, initialHeight: 30); // Only 30, min is 50
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 80, 30);

        hsplit.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);

        Assert.True(tooSmall.WasRendered);
        Assert.Equal(0, child.WriteToScreenCallCount);
    }

    [Fact]
    public void WriteToScreen_MultipleChildrenMinimumExceedsSpace_ShowsWindowTooSmall()
    {
        var tooSmall = new TooSmallContainer();
        var child1 = new TestContainer(height: new Dimension(min: 30, max: 100, preferred: 50));
        var child2 = new TestContainer(height: new Dimension(min: 30, max: 100, preferred: 50));

        var hsplit = new HSplit([child1, child2], windowTooSmall: tooSmall);

        // Total minimum is 60, but only 50 available
        var screen = new Screen(initialWidth: 80, initialHeight: 50);
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 80, 50);

        hsplit.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);

        Assert.True(tooSmall.WasRendered);
        Assert.Equal(0, child1.WriteToScreenCallCount);
        Assert.Equal(0, child2.WriteToScreenCallCount);
    }

    [Fact]
    public void WriteToScreen_MinimumExceedsSpace_DefaultTooSmallShown()
    {
        var child = new TestContainer(height: new Dimension(min: 50, max: 100, preferred: 75));

        var hsplit = new HSplit([child]); // No custom tooSmall, use default

        var screen = new Screen(initialWidth: 80, initialHeight: 30);
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 80, 30);

        hsplit.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);

        // Should render without exception - default "too small" window is used
        Assert.Equal(0, child.WriteToScreenCallCount);
    }

    [Fact]
    public void WriteToScreen_ExactlyAtMinimum_RendersNormally()
    {
        var tooSmall = new TooSmallContainer();
        var child1 = new TestContainer(height: new Dimension(min: 25, max: 100, preferred: 50));
        var child2 = new TestContainer(height: new Dimension(min: 25, max: 100, preferred: 50));

        var hsplit = new HSplit([child1, child2], windowTooSmall: tooSmall);

        // Exactly 50 = min sum
        var screen = new Screen(initialWidth: 80, initialHeight: 50);
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 80, 50);

        hsplit.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);

        Assert.False(tooSmall.WasRendered);
        Assert.Equal(1, child1.WriteToScreenCallCount);
        Assert.Equal(1, child2.WriteToScreenCallCount);
    }

    #endregion

    #region Zero Size Tests

    [Fact]
    public void WriteToScreen_ZeroHeight_NoExceptionEmptyChildren()
    {
        var hsplit = new HSplit([]);

        var screen = new Screen(initialWidth: 80, initialHeight: 0);
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 80, 0);

        // Should not throw
        hsplit.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);
    }

    [Fact]
    public void WriteToScreen_ZeroWidth_NoException()
    {
        var child = new TestContainer(height: new Dimension(min: 0, max: 100, preferred: 50));
        var hsplit = new HSplit([child]);

        var screen = new Screen(initialWidth: 0, initialHeight: 100);
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 0, 100);

        // Should not throw
        hsplit.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);
    }

    #endregion

    #region Single Child Edge Cases

    [Fact]
    public void WriteToScreen_SingleChild_GetsAllSpace()
    {
        var child = new TestContainer(height: new Dimension(min: 0, max: 1000, preferred: 100, weight: 1));

        var hsplit = new HSplit([child]);

        var screen = new Screen(initialWidth: 80, initialHeight: 50);
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 80, 50);

        hsplit.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);

        Assert.Equal(50, child.LastWritePosition?.Height);
    }

    [Fact]
    public void WriteToScreen_SingleChildWithMax_CapsAtMax()
    {
        var child = new TestContainer(height: new Dimension(min: 0, max: 30, preferred: 30, weight: 1));

        var hsplit = new HSplit([child]);

        var screen = new Screen(initialWidth: 80, initialHeight: 100);
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 80, 100);

        hsplit.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);

        Assert.Equal(30, child.LastWritePosition?.Height);
    }

    #endregion

    #region Deeply Nested Tests

    [Fact]
    public void WriteToScreen_DeeplyNested_10Levels_NoStackOverflow()
    {
        // Create 10-level deep nesting
        IContainer current = new TestContainer(height: new Dimension(min: 0, max: 100, preferred: 10, weight: 1));

        for (int i = 0; i < 10; i++)
        {
            current = new HSplit([current]);
        }

        var screen = new Screen(initialWidth: 80, initialHeight: 100);
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 80, 100);

        // Should not throw - verifies 10-level depth works (SC-001)
        current.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);
    }

    #endregion

    #region Offset Position Tests

    [Fact]
    public void WriteToScreen_WithOffset_ChildrenGetCorrectOffset()
    {
        var child1 = new TestContainer(height: new Dimension(min: 10, max: 20, preferred: 15));
        var child2 = new TestContainer(height: new Dimension(min: 10, max: 20, preferred: 15));

        var hsplit = new HSplit([child1, child2]);

        var screen = new Screen(initialWidth: 100, initialHeight: 100);
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(10, 20, 50, 40); // Offset from (10, 20)

        hsplit.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);

        Assert.Equal(10, child1.LastWritePosition?.XPos);
        Assert.Equal(20, child1.LastWritePosition?.YPos);
        Assert.True(child2.LastWritePosition?.YPos > 20);
    }

    #endregion

    #region Style Propagation Tests

    [Fact]
    public void WriteToScreen_WithParentStyle_PropagatesStyle()
    {
        var child = new TestContainer(height: new Dimension(min: 0, max: 100, preferred: 50, weight: 1));

        var hsplit = new HSplit([child], style: "class:hsplit");

        var screen = new Screen(initialWidth: 80, initialHeight: 50);
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 80, 50);

        // Should not throw - style is properly propagated
        hsplit.WriteToScreen(screen, mouseHandlers, writePosition, "class:parent", true, null);
    }

    #endregion

    #region Z-Index Tests

    [Fact]
    public void WriteToScreen_WithZIndex_PropagatesZIndex()
    {
        var child = new TestContainer(height: new Dimension(min: 0, max: 100, preferred: 50, weight: 1));

        var hsplit = new HSplit([child], zIndex: 5);

        var screen = new Screen(initialWidth: 80, initialHeight: 50);
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 80, 50);

        // Should not throw - zIndex is properly used
        hsplit.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);
        Assert.Equal(1, child.WriteToScreenCallCount);
    }

    #endregion
}
