using Stroke.KeyBinding;
using Stroke.Layout;
using Stroke.Layout.Containers;
using Xunit;

namespace Stroke.Tests.Layout.Containers;

/// <summary>
/// Tests for VSplit width division algorithm.
/// Tests the _divide_widths algorithm which allocates horizontal space among children.
/// </summary>
public sealed class VSplitDivisionTests
{
    #region Helper Classes

    /// <summary>
    /// Test container with configurable dimensions for VSplit division testing.
    /// </summary>
    private sealed class TestContainer : IContainer
    {
        private readonly Dimension _width;
        private readonly Dimension _height;
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
            AllocatedWidth = writePosition.Width;
        }

        public IKeyBindingsBase? GetKeyBindings() => null;

        public IReadOnlyList<IContainer> GetChildren() => [];
    }

    #endregion

    #region Test Vector 1: Equal Weights

    [Fact]
    public void DivideWidths_TwoEqualWeights_SplitsEvenly()
    {
        var child1 = new TestContainer(width: new Dimension(min: 0, max: 100, preferred: 50, weight: 1));
        var child2 = new TestContainer(width: new Dimension(min: 0, max: 100, preferred: 50, weight: 1));

        var vsplit = new VSplit([child1, child2]);

        var screen = new Screen(initialWidth: 100, initialHeight: 24);
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 100, 24);

        vsplit.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);

        Assert.Equal(50, child1.AllocatedWidth);
        Assert.Equal(50, child2.AllocatedWidth);
    }

    [Fact]
    public void DivideWidths_ThreeEqualWeights_SplitsEvenly()
    {
        var child1 = new TestContainer(width: new Dimension(min: 0, max: 100, preferred: 30, weight: 1));
        var child2 = new TestContainer(width: new Dimension(min: 0, max: 100, preferred: 30, weight: 1));
        var child3 = new TestContainer(width: new Dimension(min: 0, max: 100, preferred: 30, weight: 1));

        var vsplit = new VSplit([child1, child2, child3]);

        var screen = new Screen(initialWidth: 90, initialHeight: 24);
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 90, 24);

        vsplit.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);

        Assert.Equal(30, child1.AllocatedWidth);
        Assert.Equal(30, child2.AllocatedWidth);
        Assert.Equal(30, child3.AllocatedWidth);
    }

    #endregion

    #region Test Vector 2: Unequal Weights

    [Fact]
    public void DivideWidths_Weight1vs3_Roughly1to3Ratio()
    {
        var child1 = new TestContainer(width: new Dimension(min: 0, max: 100, preferred: 0, weight: 1));
        var child2 = new TestContainer(width: new Dimension(min: 0, max: 100, preferred: 0, weight: 3));

        var vsplit = new VSplit([child1, child2]);

        var screen = new Screen(initialWidth: 100, initialHeight: 24);
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 100, 24);

        vsplit.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);

        // Expect approximately 25:75 ratio
        Assert.True(child1.AllocatedWidth >= 20 && child1.AllocatedWidth <= 30,
            $"Child1 should be ~25, was {child1.AllocatedWidth}");
        Assert.True(child2.AllocatedWidth >= 70 && child2.AllocatedWidth <= 80,
            $"Child2 should be ~75, was {child2.AllocatedWidth}");
    }

    [Fact]
    public void DivideWidths_Weight2vs8_Roughly1to4Ratio()
    {
        var child1 = new TestContainer(width: new Dimension(min: 0, max: 100, preferred: 0, weight: 2));
        var child2 = new TestContainer(width: new Dimension(min: 0, max: 100, preferred: 0, weight: 8));

        var vsplit = new VSplit([child1, child2]);

        var screen = new Screen(initialWidth: 100, initialHeight: 24);
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 100, 24);

        vsplit.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);

        // Weight ratio 2:8 = 1:4, expect approximately 20:80
        Assert.True(child1.AllocatedWidth >= 15 && child1.AllocatedWidth <= 25,
            $"Child1 should be ~20, was {child1.AllocatedWidth}");
        Assert.True(child2.AllocatedWidth >= 75 && child2.AllocatedWidth <= 85,
            $"Child2 should be ~80, was {child2.AllocatedWidth}");
    }

    #endregion

    #region Test Vector 3: Fixed Size Children

    [Fact]
    public void DivideWidths_FixedSizeChild_GetsExactSize()
    {
        var fixedChild = new TestContainer(width: new Dimension(min: 20, max: 20, preferred: 20));
        var flexChild = new TestContainer(width: new Dimension(min: 0, max: 100, preferred: 50, weight: 1));

        var vsplit = new VSplit([fixedChild, flexChild]);

        var screen = new Screen(initialWidth: 100, initialHeight: 24);
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 100, 24);

        vsplit.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);

        Assert.Equal(20, fixedChild.AllocatedWidth);
        Assert.Equal(80, flexChild.AllocatedWidth);
    }

    [Fact]
    public void DivideWidths_TwoFixedSizeChildren_BothGetExactSize()
    {
        var fixed1 = new TestContainer(width: new Dimension(min: 30, max: 30, preferred: 30));
        var fixed2 = new TestContainer(width: new Dimension(min: 25, max: 25, preferred: 25));

        var vsplit = new VSplit([fixed1, fixed2]);

        var screen = new Screen(initialWidth: 100, initialHeight: 24);
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 100, 24);

        vsplit.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);

        Assert.Equal(30, fixed1.AllocatedWidth);
        Assert.Equal(25, fixed2.AllocatedWidth);
    }

    #endregion

    #region Test Vector 4: Minimum Size Constraints

    [Fact]
    public void DivideWidths_MinimumConstraint_RespectsMinimum()
    {
        var minChild = new TestContainer(width: new Dimension(min: 30, max: 100, preferred: 50, weight: 1));
        var flexChild = new TestContainer(width: new Dimension(min: 0, max: 100, preferred: 50, weight: 1));

        var vsplit = new VSplit([minChild, flexChild]);

        var screen = new Screen(initialWidth: 50, initialHeight: 24);
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 50, 24);

        vsplit.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);

        Assert.True(minChild.AllocatedWidth >= 30,
            $"MinChild should be at least 30, was {minChild.AllocatedWidth}");
    }

    #endregion

    #region Test Vector 5: Maximum Size Constraints

    [Fact]
    public void DivideWidths_MaximumConstraint_RespectsMaximum()
    {
        var maxChild = new TestContainer(width: new Dimension(min: 0, max: 20, preferred: 20, weight: 1));
        var flexChild = new TestContainer(width: new Dimension(min: 0, max: 200, preferred: 100, weight: 1));

        var vsplit = new VSplit([maxChild, flexChild]);

        var screen = new Screen(initialWidth: 100, initialHeight: 24);
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 100, 24);

        vsplit.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);

        Assert.True(maxChild.AllocatedWidth <= 20,
            $"MaxChild should be at most 20, was {maxChild.AllocatedWidth}");
        Assert.True(flexChild.AllocatedWidth >= 80,
            $"FlexChild should get remaining space, was {flexChild.AllocatedWidth}");
    }

    #endregion

    #region Test Vector 6: Preferred Size Allocation

    [Fact]
    public void DivideWidths_PreferredSizeRespected_WhenSpaceAvailable()
    {
        var child1 = new TestContainer(width: new Dimension(min: 0, max: 100, preferred: 40, weight: 1));
        var child2 = new TestContainer(width: new Dimension(min: 0, max: 100, preferred: 60, weight: 1));

        var vsplit = new VSplit([child1, child2]);

        var screen = new Screen(initialWidth: 100, initialHeight: 24);
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 100, 24);

        vsplit.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);

        Assert.True(child1.AllocatedWidth >= 40,
            $"Child1 should be at least 40 (preferred), was {child1.AllocatedWidth}");
        Assert.True(child2.AllocatedWidth >= 60,
            $"Child2 should be at least 60 (preferred), was {child2.AllocatedWidth}");
    }

    #endregion

    #region Test Vector 7: Zero Weight

    [Fact]
    public void DivideWidths_ZeroWeight_GetsMinimumOnly()
    {
        var zeroWeightChild = new TestContainer(width: new Dimension(min: 10, max: 100, preferred: 50, weight: 0));
        var weightedChild = new TestContainer(width: new Dimension(min: 0, max: 100, preferred: 50, weight: 1));

        var vsplit = new VSplit([zeroWeightChild, weightedChild]);

        var screen = new Screen(initialWidth: 100, initialHeight: 24);
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 100, 24);

        vsplit.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);

        Assert.Equal(10, zeroWeightChild.AllocatedWidth);
        Assert.Equal(90, weightedChild.AllocatedWidth);
    }

    #endregion

    #region Test Vector 8: All Zero Weights

    [Fact]
    public void DivideWidths_AllZeroWeights_GetsMinimums()
    {
        var child1 = new TestContainer(width: new Dimension(min: 10, max: 100, preferred: 50, weight: 0));
        var child2 = new TestContainer(width: new Dimension(min: 20, max: 100, preferred: 50, weight: 0));

        var vsplit = new VSplit([child1, child2]);

        var screen = new Screen(initialWidth: 100, initialHeight: 24);
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 100, 24);

        vsplit.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);

        Assert.Equal(10, child1.AllocatedWidth);
        Assert.Equal(20, child2.AllocatedWidth);
    }

    #endregion

    #region Test Vector 9: Excess Space

    [Fact]
    public void DivideWidths_ExcessSpace_DistributedByWeight()
    {
        var child1 = new TestContainer(width: new Dimension(min: 0, max: 30, preferred: 20, weight: 1));
        var child2 = new TestContainer(width: new Dimension(min: 0, max: 40, preferred: 30, weight: 2));

        var vsplit = new VSplit([child1, child2]);

        var screen = new Screen(initialWidth: 100, initialHeight: 24);
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 100, 24);

        vsplit.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);

        // Should fill to max since total max (70) < available (100)
        Assert.Equal(30, child1.AllocatedWidth);
        Assert.Equal(40, child2.AllocatedWidth);
    }

    #endregion

    #region Test Vector 10: Constrained Space

    [Fact]
    public void DivideWidths_ConstrainedSpace_ScalesProportionally()
    {
        var child1 = new TestContainer(width: new Dimension(min: 0, max: 100, preferred: 60, weight: 1));
        var child2 = new TestContainer(width: new Dimension(min: 0, max: 100, preferred: 60, weight: 1));

        var vsplit = new VSplit([child1, child2]);

        // Only 100 columns but preferred sum is 120
        var screen = new Screen(initialWidth: 100, initialHeight: 24);
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 100, 24);

        vsplit.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);

        // Should split evenly since weights are equal
        Assert.Equal(50, child1.AllocatedWidth);
        Assert.Equal(50, child2.AllocatedWidth);
    }

    #endregion

    #region Test Vector 11: Padding Between Children

    [Fact]
    public void DivideWidths_WithPadding_AccountsForPaddingSpace()
    {
        var child1 = new TestContainer(width: new Dimension(min: 0, max: 100, preferred: 50, weight: 1));
        var child2 = new TestContainer(width: new Dimension(min: 0, max: 100, preferred: 50, weight: 1));

        var vsplit = new VSplit([child1, child2], padding: 2);

        var screen = new Screen(initialWidth: 100, initialHeight: 24);
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 100, 24);

        vsplit.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);

        // Total should be 100 with 2 for padding
        var totalAllocated = child1.AllocatedWidth + child2.AllocatedWidth;
        Assert.True(totalAllocated <= 98, $"Children should leave room for padding, got {totalAllocated}");
    }

    #endregion

    #region Test Vector 12: Many Children

    [Fact]
    public void DivideWidths_TenChildren_DistributesEvenly()
    {
        var children = Enumerable.Range(0, 10)
            .Select(_ => new TestContainer(width: new Dimension(min: 0, max: 100, preferred: 10, weight: 1)))
            .ToList();

        var vsplit = new VSplit(children);

        var screen = new Screen(initialWidth: 100, initialHeight: 24);
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 100, 24);

        vsplit.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);

        // Each should get exactly 10
        foreach (var child in children)
        {
            Assert.Equal(10, child.AllocatedWidth);
        }
    }

    #endregion
}
