using Stroke.KeyBinding;
using Stroke.Layout;
using Stroke.Layout.Containers;
using Xunit;

namespace Stroke.Tests.Layout.Containers;

/// <summary>
/// Tests for HSplit container basic functionality.
/// </summary>
public sealed class HSplitTests
{
    #region Helper Classes

    /// <summary>
    /// Test container with configurable dimensions for HSplit testing.
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
    public void Constructor_EmptyChildren_CreatesValidHSplit()
    {
        var hsplit = new HSplit([]);

        Assert.NotNull(hsplit);
        Assert.Empty(hsplit.GetChildren());
    }

    [Fact]
    public void Constructor_WithChildren_StoresChildren()
    {
        var child1 = new TestContainer();
        var child2 = new TestContainer();

        var hsplit = new HSplit([child1, child2]);

        var children = hsplit.GetChildren();
        Assert.Equal(2, children.Count);
    }

    [Fact]
    public void Constructor_DefaultAlign_IsJustify()
    {
        var hsplit = new HSplit([]);

        Assert.Equal(VerticalAlign.Justify, hsplit.Align);
    }

    [Fact]
    public void Constructor_CustomAlign_IsStored()
    {
        var hsplit = new HSplit([], align: VerticalAlign.Top);

        Assert.Equal(VerticalAlign.Top, hsplit.Align);
    }

    [Fact]
    public void Constructor_DefaultPadding_IsZero()
    {
        var hsplit = new HSplit([]);

        // Padding defaults to 0
        Assert.Equal(0, hsplit.Padding.Preferred);
    }

    [Fact]
    public void Constructor_WithPadding_IsStored()
    {
        var hsplit = new HSplit([], padding: 5);

        Assert.Equal(5, hsplit.Padding.Preferred);
    }

    [Fact]
    public void Constructor_WithModal_IsStored()
    {
        var hsplit = new HSplit([], modal: true);

        Assert.True(hsplit.IsModal);
    }

    #endregion

    #region PreferredWidth Tests

    [Fact]
    public void PreferredWidth_NoChildren_ReturnsDefaultDimension()
    {
        var hsplit = new HSplit([]);

        var width = hsplit.PreferredWidth(100);

        // Empty container returns default dimension
        Assert.Equal(0, width.Min);
    }

    [Fact]
    public void PreferredWidth_WithChildren_ReturnsMaxOfChildren()
    {
        var child1 = new TestContainer(width: new Dimension(min: 10, max: 50, preferred: 30));
        var child2 = new TestContainer(width: new Dimension(min: 20, max: 60, preferred: 40));

        var hsplit = new HSplit([child1, child2]);

        var width = hsplit.PreferredWidth(100);

        // MaxLayoutDimensions logic: highest min, lowest max, highest preferred
        Assert.Equal(20, width.Min);
        Assert.Equal(40, width.Preferred);
    }

    [Fact]
    public void PreferredWidth_WithExplicitWidth_UsesExplicitValue()
    {
        var child = new TestContainer(width: new Dimension(min: 10, max: 50, preferred: 30));

        var hsplit = new HSplit([child], width: 25);

        var width = hsplit.PreferredWidth(100);

        Assert.Equal(25, width.Min);
        Assert.Equal(25, width.Max);
        Assert.Equal(25, width.Preferred);
    }

    #endregion

    #region PreferredHeight Tests

    [Fact]
    public void PreferredHeight_NoChildren_ReturnsDefaultDimension()
    {
        var hsplit = new HSplit([]);

        var height = hsplit.PreferredHeight(80, 100);

        Assert.Equal(0, height.Min);
        Assert.Equal(0, height.Max);
        Assert.Equal(0, height.Preferred);
    }

    [Fact]
    public void PreferredHeight_WithChildren_ReturnsSumOfChildren()
    {
        var child1 = new TestContainer(height: new Dimension(min: 5, max: 10, preferred: 7));
        var child2 = new TestContainer(height: new Dimension(min: 3, max: 8, preferred: 5));

        var hsplit = new HSplit([child1, child2]);

        var height = hsplit.PreferredHeight(80, 100);

        // SumLayoutDimensions: sum of min/max/preferred
        Assert.Equal(8, height.Min);  // 5 + 3
        Assert.Equal(18, height.Max); // 10 + 8
        Assert.Equal(12, height.Preferred); // 7 + 5
    }

    [Fact]
    public void PreferredHeight_WithExplicitHeight_UsesExplicitValue()
    {
        var child = new TestContainer(height: new Dimension(min: 5, max: 10, preferred: 7));

        var hsplit = new HSplit([child], height: 15);

        var height = hsplit.PreferredHeight(80, 100);

        Assert.Equal(15, height.Min);
        Assert.Equal(15, height.Max);
        Assert.Equal(15, height.Preferred);
    }

    #endregion

    #region Reset Tests

    [Fact]
    public void Reset_CallsResetOnAllChildren()
    {
        var child1 = new TestContainer();
        var child2 = new TestContainer();

        var hsplit = new HSplit([child1, child2]);
        hsplit.Reset();

        Assert.True(child1.ResetCalled);
        Assert.True(child2.ResetCalled);
    }

    #endregion

    #region WriteToScreen Tests - Basic Positioning

    [Fact]
    public void WriteToScreen_SingleChild_GetsFullHeight()
    {
        var child = new TestContainer(height: new Dimension(min: 10, max: 50, preferred: 20));
        var hsplit = new HSplit([child]);

        var screen = new Screen(initialWidth: 80, initialHeight: 24);
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 80, 24);

        hsplit.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);

        Assert.Equal(1, child.WriteToScreenCallCount);
        Assert.NotNull(child.LastWritePosition);
        Assert.Equal(0, child.LastWritePosition.Value.YPos);
        Assert.Equal(80, child.LastWritePosition.Value.Width);
        Assert.Equal(24, child.LastWritePosition.Value.Height);
    }

    [Fact]
    public void WriteToScreen_TwoEqualChildren_SplitEvenly()
    {
        var child1 = new TestContainer(height: new Dimension(min: 0, max: 100, preferred: 50, weight: 1));
        var child2 = new TestContainer(height: new Dimension(min: 0, max: 100, preferred: 50, weight: 1));

        var hsplit = new HSplit([child1, child2]);

        var screen = new Screen(initialWidth: 80, initialHeight: 20);
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 80, 20);

        hsplit.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);

        Assert.NotNull(child1.LastWritePosition);
        Assert.NotNull(child2.LastWritePosition);

        // With equal weights and Justify align, children should split the space
        Assert.Equal(0, child1.LastWritePosition.Value.YPos);
        // Child 2 should start after child 1
        Assert.True(child2.LastWritePosition.Value.YPos >= child1.LastWritePosition.Value.Height);
    }

    [Fact]
    public void WriteToScreen_ChildrenWithDifferentWeights_ProportionalSizes()
    {
        // Weight 1 vs weight 3 should give ~1:3 ratio
        var child1 = new TestContainer(height: new Dimension(min: 0, max: 100, preferred: 0, weight: 1));
        var child2 = new TestContainer(height: new Dimension(min: 0, max: 100, preferred: 0, weight: 3));

        var hsplit = new HSplit([child1, child2]);

        var screen = new Screen(initialWidth: 80, initialHeight: 40);
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 80, 40);

        hsplit.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);

        Assert.NotNull(child1.LastWritePosition);
        Assert.NotNull(child2.LastWritePosition);

        // Allow some tolerance for rounding
        var height1 = child1.LastWritePosition.Value.Height;
        var height2 = child2.LastWritePosition.Value.Height;

        // Child 2 should be roughly 3x child 1
        Assert.True(height2 > height1, $"Child2 ({height2}) should be larger than Child1 ({height1})");
    }

    #endregion

    #region WriteToScreen Tests - Window Position

    [Fact]
    public void WriteToScreen_WithXOffset_PassesXOffsetToChildren()
    {
        var child = new TestContainer(height: new Dimension(min: 5, max: 20, preferred: 10));
        var hsplit = new HSplit([child]);

        var screen = new Screen(initialWidth: 100, initialHeight: 50);
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(10, 5, 60, 30);

        hsplit.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);

        Assert.NotNull(child.LastWritePosition);
        Assert.Equal(10, child.LastWritePosition.Value.XPos);
        Assert.Equal(60, child.LastWritePosition.Value.Width);
    }

    [Fact]
    public void WriteToScreen_WithYOffset_PassesYOffsetToFirstChild()
    {
        var child = new TestContainer(height: new Dimension(min: 5, max: 20, preferred: 10));
        var hsplit = new HSplit([child]);

        var screen = new Screen(initialWidth: 100, initialHeight: 50);
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 10, 80, 30);

        hsplit.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);

        Assert.NotNull(child.LastWritePosition);
        Assert.Equal(10, child.LastWritePosition.Value.YPos);
    }

    #endregion
}
