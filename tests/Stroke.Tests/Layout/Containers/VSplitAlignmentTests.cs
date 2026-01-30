using Stroke.KeyBinding;
using Stroke.Layout;
using Stroke.Layout.Containers;
using Xunit;

namespace Stroke.Tests.Layout.Containers;

/// <summary>
/// Tests for VSplit horizontal alignment (Left, Center, Right, Justify).
/// </summary>
public sealed class VSplitAlignmentTests
{
    #region Helper Classes

    /// <summary>
    /// Test container with configurable dimensions for alignment testing.
    /// </summary>
    private sealed class TestContainer : IContainer
    {
        private readonly Dimension _width;
        private readonly Dimension _height;
        public int AllocatedXPos { get; private set; }
        public int AllocatedWidth { get; private set; }

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
            AllocatedXPos = writePosition.XPos;
            AllocatedWidth = writePosition.Width;
        }

        public IKeyBindingsBase? GetKeyBindings() => null;

        public IReadOnlyList<IContainer> GetChildren() => [];
    }

    #endregion

    #region Justify Alignment (Default)

    [Fact]
    public void WriteToScreen_JustifyAlignment_TakesFullWidth()
    {
        var child = new TestContainer(width: new Dimension(min: 20, max: 80, preferred: 40, weight: 1));

        var vsplit = new VSplit([child], align: HorizontalAlign.Justify);

        var screen = new Screen(initialWidth: 100, initialHeight: 24);
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 100, 24);

        vsplit.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);

        // Justify should expand to fill available width
        Assert.Equal(0, child.AllocatedXPos);
        Assert.Equal(80, child.AllocatedWidth); // Limited by max
    }

    [Fact]
    public void WriteToScreen_JustifyAlignment_MultipleChildren_TakesFullWidth()
    {
        var child1 = new TestContainer(width: new Dimension(min: 0, max: 100, preferred: 30, weight: 1));
        var child2 = new TestContainer(width: new Dimension(min: 0, max: 100, preferred: 30, weight: 1));

        var vsplit = new VSplit([child1, child2], align: HorizontalAlign.Justify);

        var screen = new Screen(initialWidth: 100, initialHeight: 24);
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 100, 24);

        vsplit.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);

        // Total should use all 100 columns
        var totalWidth = child1.AllocatedWidth + child2.AllocatedWidth;
        Assert.Equal(100, totalWidth);
    }

    #endregion

    #region Left Alignment

    [Fact]
    public void WriteToScreen_LeftAlignment_StartsAtLeftEdge()
    {
        var child = new TestContainer(width: new Dimension(min: 20, max: 40, preferred: 30, weight: 1));

        var vsplit = new VSplit([child], align: HorizontalAlign.Left);

        var screen = new Screen(initialWidth: 100, initialHeight: 24);
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 100, 24);

        vsplit.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);

        // Left alignment - child starts at position 0
        Assert.Equal(0, child.AllocatedXPos);
        // Child uses its max width
        Assert.Equal(40, child.AllocatedWidth);
    }

    [Fact]
    public void WriteToScreen_LeftAlignment_MultipleChildren_AlignLeft()
    {
        var child1 = new TestContainer(width: new Dimension(min: 10, max: 20, preferred: 15, weight: 1));
        var child2 = new TestContainer(width: new Dimension(min: 10, max: 20, preferred: 15, weight: 1));

        var vsplit = new VSplit([child1, child2], align: HorizontalAlign.Left);

        var screen = new Screen(initialWidth: 100, initialHeight: 24);
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 100, 24);

        vsplit.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);

        // Children should be at left with flexible space on right
        Assert.Equal(0, child1.AllocatedXPos);
        Assert.True(child2.AllocatedXPos > 0);
        // Flexible window fills remaining space on right
    }

    #endregion

    #region Right Alignment

    [Fact]
    public void WriteToScreen_RightAlignment_EndsAtRightEdge()
    {
        var child = new TestContainer(width: new Dimension(min: 20, max: 40, preferred: 30, weight: 1));

        var vsplit = new VSplit([child], align: HorizontalAlign.Right);

        var screen = new Screen(initialWidth: 100, initialHeight: 24);
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 100, 24);

        vsplit.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);

        // Right alignment - child ends at position 100
        // With max 40, child should start around position 60
        Assert.True(child.AllocatedXPos >= 50, $"Child should start past middle, was at {child.AllocatedXPos}");
        Assert.Equal(40, child.AllocatedWidth);
    }

    [Fact]
    public void WriteToScreen_RightAlignment_MultipleChildren_AlignRight()
    {
        var child1 = new TestContainer(width: new Dimension(min: 10, max: 20, preferred: 15, weight: 1));
        var child2 = new TestContainer(width: new Dimension(min: 10, max: 20, preferred: 15, weight: 1));

        var vsplit = new VSplit([child1, child2], align: HorizontalAlign.Right);

        var screen = new Screen(initialWidth: 100, initialHeight: 24);
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 100, 24);

        vsplit.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);

        // Children should be at right with flexible space on left
        // Last child should end at or near 100
        Assert.True(child1.AllocatedXPos > 40, $"Child1 should be past middle, was at {child1.AllocatedXPos}");
        Assert.True(child2.AllocatedXPos > child1.AllocatedXPos);
    }

    #endregion

    #region Center Alignment

    [Fact]
    public void WriteToScreen_CenterAlignment_CentersContent()
    {
        var child = new TestContainer(width: new Dimension(min: 20, max: 40, preferred: 30, weight: 1));

        var vsplit = new VSplit([child], align: HorizontalAlign.Center);

        var screen = new Screen(initialWidth: 100, initialHeight: 24);
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 100, 24);

        vsplit.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);

        // Center alignment - child should be roughly in middle
        // With width 40 in 100 wide area, xpos should be around 30
        Assert.True(child.AllocatedXPos >= 20 && child.AllocatedXPos <= 40,
            $"Child should be centered, xpos was {child.AllocatedXPos}");
    }

    [Fact]
    public void WriteToScreen_CenterAlignment_MultipleChildren_CentersGroup()
    {
        var child1 = new TestContainer(width: new Dimension(min: 10, max: 20, preferred: 15, weight: 1));
        var child2 = new TestContainer(width: new Dimension(min: 10, max: 20, preferred: 15, weight: 1));

        var vsplit = new VSplit([child1, child2], align: HorizontalAlign.Center);

        var screen = new Screen(initialWidth: 100, initialHeight: 24);
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 100, 24);

        vsplit.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);

        // Group of ~40 width should be centered in 100 wide area
        // First child should start around 30
        Assert.True(child1.AllocatedXPos >= 15 && child1.AllocatedXPos <= 45,
            $"Children should be centered, first child at {child1.AllocatedXPos}");
    }

    #endregion

    #region Alignment With Offset

    [Fact]
    public void WriteToScreen_LeftAlignment_WithOffset_StartsAtOffset()
    {
        var child = new TestContainer(width: new Dimension(min: 20, max: 40, preferred: 30, weight: 1));

        var vsplit = new VSplit([child], align: HorizontalAlign.Left);

        var screen = new Screen(initialWidth: 200, initialHeight: 24);
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(50, 0, 100, 24); // Start at x=50

        vsplit.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);

        Assert.Equal(50, child.AllocatedXPos);
    }

    [Fact]
    public void WriteToScreen_RightAlignment_WithOffset_EndsAtOffsetPlusWidth()
    {
        var child = new TestContainer(width: new Dimension(min: 20, max: 40, preferred: 30, weight: 1));

        var vsplit = new VSplit([child], align: HorizontalAlign.Right);

        var screen = new Screen(initialWidth: 200, initialHeight: 24);
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(50, 0, 100, 24); // x=50 to x=150

        vsplit.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);

        // Child should end at 150 (50 + 100), so start at 110 (150 - 40)
        Assert.True(child.AllocatedXPos >= 100, $"Child should end at right edge, started at {child.AllocatedXPos}");
    }

    #endregion

    #region Alignment With Padding

    [Fact]
    public void WriteToScreen_LeftAlignment_WithPadding_ChildrenSeparated()
    {
        var child1 = new TestContainer(width: new Dimension(min: 10, max: 20, preferred: 15, weight: 1));
        var child2 = new TestContainer(width: new Dimension(min: 10, max: 20, preferred: 15, weight: 1));

        var vsplit = new VSplit([child1, child2], align: HorizontalAlign.Left, padding: 5);

        var screen = new Screen(initialWidth: 100, initialHeight: 24);
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 100, 24);

        vsplit.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);

        // Child 2 should start after child 1 + padding
        var expectedChild2Start = child1.AllocatedXPos + child1.AllocatedWidth + 5;
        Assert.Equal(expectedChild2Start, child2.AllocatedXPos);
    }

    #endregion

    #region Default Alignment

    [Fact]
    public void Constructor_DefaultAlign_IsJustify()
    {
        var vsplit = new VSplit([]);

        Assert.Equal(HorizontalAlign.Justify, vsplit.Align);
    }

    #endregion
}
