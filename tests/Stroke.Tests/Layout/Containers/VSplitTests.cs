using Stroke.KeyBinding;
using Stroke.Layout;
using Stroke.Layout.Containers;
using Xunit;

namespace Stroke.Tests.Layout.Containers;

/// <summary>
/// Tests for VSplit container basic functionality.
/// </summary>
public sealed class VSplitTests
{
    #region Helper Classes

    /// <summary>
    /// Test container with configurable dimensions for VSplit testing.
    /// </summary>
    private sealed class TestContainer : IContainer
    {
        private readonly Dimension _width;
        private readonly Dimension _height;
        public int WriteToScreenCallCount { get; private set; }
        public WritePosition? LastWritePosition { get; private set; }
        public bool ResetCalled { get; private set; }

        public TestContainer(Dimension? width = null, Dimension? height = null)
        {
            _width = width ?? new Dimension();
            _height = height ?? new Dimension();
        }

        public bool IsModal => false;

        public void Reset() => ResetCalled = true;

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
            // Fill the region with a character to verify positioning
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

    #endregion

    #region Constructor Tests

    [Fact]
    public void Constructor_EmptyChildren_CreatesValidVSplit()
    {
        var vsplit = new VSplit([]);

        Assert.NotNull(vsplit);
        Assert.Empty(vsplit.GetChildren());
    }

    [Fact]
    public void Constructor_WithChildren_StoresChildren()
    {
        var child1 = new TestContainer();
        var child2 = new TestContainer();

        var vsplit = new VSplit([child1, child2]);

        var children = vsplit.GetChildren();
        Assert.Equal(2, children.Count);
    }

    [Fact]
    public void Constructor_DefaultAlign_IsJustify()
    {
        var vsplit = new VSplit([]);

        Assert.Equal(HorizontalAlign.Justify, vsplit.Align);
    }

    [Fact]
    public void Constructor_CustomAlign_IsStored()
    {
        var vsplit = new VSplit([], align: HorizontalAlign.Left);

        Assert.Equal(HorizontalAlign.Left, vsplit.Align);
    }

    #endregion

    #region PreferredWidth Tests

    [Fact]
    public void PreferredWidth_NoChildren_ReturnsZeroDimension()
    {
        var vsplit = new VSplit([]);

        var width = vsplit.PreferredWidth(100);

        Assert.Equal(0, width.Min);
        Assert.Equal(0, width.Max);
        Assert.Equal(0, width.Preferred);
    }

    [Fact]
    public void PreferredWidth_WithChildren_ReturnsSumOfChildren()
    {
        var child1 = new TestContainer(width: new Dimension(min: 10, max: 20, preferred: 15));
        var child2 = new TestContainer(width: new Dimension(min: 5, max: 10, preferred: 8));

        var vsplit = new VSplit([child1, child2]);

        var width = vsplit.PreferredWidth(100);

        // SumLayoutDimensions: sum of min/max/preferred
        Assert.Equal(15, width.Min);  // 10 + 5
        Assert.Equal(30, width.Max);  // 20 + 10
        Assert.Equal(23, width.Preferred); // 15 + 8
    }

    #endregion

    #region PreferredHeight Tests

    [Fact]
    public void PreferredHeight_NoChildren_ReturnsDefaultDimension()
    {
        var vsplit = new VSplit([]);

        var height = vsplit.PreferredHeight(80, 100);

        Assert.Equal(0, height.Min);
    }

    [Fact]
    public void PreferredHeight_WithChildren_ReturnsMaxOfChildren()
    {
        var child1 = new TestContainer(height: new Dimension(min: 5, max: 50, preferred: 20));
        var child2 = new TestContainer(height: new Dimension(min: 10, max: 30, preferred: 25));

        var vsplit = new VSplit([child1, child2]);

        var height = vsplit.PreferredHeight(80, 100);

        // MaxLayoutDimensions logic
        Assert.Equal(10, height.Min); // highest min
        Assert.Equal(25, height.Preferred); // highest preferred
    }

    #endregion

    #region Reset Tests

    [Fact]
    public void Reset_CallsResetOnAllChildren()
    {
        var child1 = new TestContainer();
        var child2 = new TestContainer();

        var vsplit = new VSplit([child1, child2]);
        vsplit.Reset();

        Assert.True(child1.ResetCalled);
        Assert.True(child2.ResetCalled);
    }

    #endregion

    #region WriteToScreen Tests - Basic Positioning

    [Fact]
    public void WriteToScreen_SingleChild_GetsFullWidth()
    {
        var child = new TestContainer(width: new Dimension(min: 10, max: 100, preferred: 40));
        var vsplit = new VSplit([child]);

        var screen = new Screen(initialWidth: 80, initialHeight: 24);
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 80, 24);

        vsplit.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);

        Assert.Equal(1, child.WriteToScreenCallCount);
        Assert.NotNull(child.LastWritePosition);
        Assert.Equal(0, child.LastWritePosition.Value.XPos);
        Assert.Equal(24, child.LastWritePosition.Value.Height);
        Assert.Equal(80, child.LastWritePosition.Value.Width);
    }

    [Fact]
    public void WriteToScreen_TwoEqualChildren_SplitEvenly()
    {
        var child1 = new TestContainer(width: new Dimension(min: 0, max: 100, preferred: 50, weight: 1));
        var child2 = new TestContainer(width: new Dimension(min: 0, max: 100, preferred: 50, weight: 1));

        var vsplit = new VSplit([child1, child2]);

        var screen = new Screen(initialWidth: 80, initialHeight: 24);
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 80, 24);

        vsplit.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);

        Assert.NotNull(child1.LastWritePosition);
        Assert.NotNull(child2.LastWritePosition);

        // With equal weights and Justify align, children should split the space
        Assert.Equal(0, child1.LastWritePosition.Value.XPos);
        // Child 2 should start after child 1
        Assert.True(child2.LastWritePosition.Value.XPos >= child1.LastWritePosition.Value.Width);
    }

    [Fact]
    public void WriteToScreen_ChildrenWithDifferentWeights_ProportionalSizes()
    {
        // Weight 1 vs weight 3 should give ~1:3 ratio
        var child1 = new TestContainer(width: new Dimension(min: 0, max: 100, preferred: 0, weight: 1));
        var child2 = new TestContainer(width: new Dimension(min: 0, max: 100, preferred: 0, weight: 3));

        var vsplit = new VSplit([child1, child2]);

        var screen = new Screen(initialWidth: 80, initialHeight: 24);
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 80, 24);

        vsplit.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);

        Assert.NotNull(child1.LastWritePosition);
        Assert.NotNull(child2.LastWritePosition);

        var width1 = child1.LastWritePosition.Value.Width;
        var width2 = child2.LastWritePosition.Value.Width;

        // Child 2 should be roughly 3x child 1
        Assert.True(width2 > width1, $"Child2 ({width2}) should be larger than Child1 ({width1})");
    }

    #endregion

    #region WriteToScreen Tests - Padding

    [Fact]
    public void WriteToScreen_WithPadding_AddsPaddingBetweenChildren()
    {
        var child1 = new TestContainer(width: new Dimension(min: 10, max: 30, preferred: 20));
        var child2 = new TestContainer(width: new Dimension(min: 10, max: 30, preferred: 20));

        var vsplit = new VSplit([child1, child2], padding: 2, paddingChar: '|');

        var screen = new Screen(initialWidth: 80, initialHeight: 24);
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 80, 24);

        vsplit.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);

        Assert.NotNull(child1.LastWritePosition);
        Assert.NotNull(child2.LastWritePosition);

        // Child 2 should start after child 1 + padding
        var expectedChild2Start = child1.LastWritePosition.Value.XPos + child1.LastWritePosition.Value.Width + 2;
        Assert.Equal(expectedChild2Start, child2.LastWritePosition.Value.XPos);
    }

    #endregion
}
