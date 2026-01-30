using Stroke.KeyBinding;
using Stroke.Layout;
using Stroke.Layout.Containers;
using Xunit;

namespace Stroke.Tests.Layout.Containers;

/// <summary>
/// Tests for HSplit height division algorithm (FR-034).
/// Tests the _divide_heights algorithm which allocates space among children.
/// </summary>
public sealed class HSplitDivisionTests
{
    #region Helper Classes

    /// <summary>
    /// Test container with configurable dimensions for HSplit division testing.
    /// </summary>
    private sealed class TestContainer : IContainer
    {
        private readonly Dimension _width;
        private readonly Dimension _height;
        public int AllocatedHeight { get; private set; }

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
            AllocatedHeight = writePosition.Height;
        }

        public IKeyBindingsBase? GetKeyBindings() => null;

        public IReadOnlyList<IContainer> GetChildren() => [];
    }

    #endregion

    #region Test Vector 1: Equal Weights

    [Fact]
    public void DivideHeights_TwoEqualWeights_SplitsEvenly()
    {
        // Two children with equal weights should split space evenly
        var child1 = new TestContainer(height: new Dimension(min: 0, max: 100, preferred: 50, weight: 1));
        var child2 = new TestContainer(height: new Dimension(min: 0, max: 100, preferred: 50, weight: 1));

        var hsplit = new HSplit([child1, child2]);

        var screen = new Screen(initialWidth: 80, initialHeight: 100);
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 80, 100);

        hsplit.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);

        Assert.Equal(50, child1.AllocatedHeight);
        Assert.Equal(50, child2.AllocatedHeight);
    }

    [Fact]
    public void DivideHeights_ThreeEqualWeights_SplitsEvenly()
    {
        var child1 = new TestContainer(height: new Dimension(min: 0, max: 100, preferred: 30, weight: 1));
        var child2 = new TestContainer(height: new Dimension(min: 0, max: 100, preferred: 30, weight: 1));
        var child3 = new TestContainer(height: new Dimension(min: 0, max: 100, preferred: 30, weight: 1));

        var hsplit = new HSplit([child1, child2, child3]);

        var screen = new Screen(initialWidth: 80, initialHeight: 90);
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 80, 90);

        hsplit.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);

        Assert.Equal(30, child1.AllocatedHeight);
        Assert.Equal(30, child2.AllocatedHeight);
        Assert.Equal(30, child3.AllocatedHeight);
    }

    #endregion

    #region Test Vector 2: Unequal Weights

    [Fact]
    public void DivideHeights_Weight1vs3_Roughly1to3Ratio()
    {
        // Weight 1 vs weight 3 should give approximately 1:3 ratio
        var child1 = new TestContainer(height: new Dimension(min: 0, max: 100, preferred: 0, weight: 1));
        var child2 = new TestContainer(height: new Dimension(min: 0, max: 100, preferred: 0, weight: 3));

        var hsplit = new HSplit([child1, child2]);

        var screen = new Screen(initialWidth: 80, initialHeight: 100);
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 80, 100);

        hsplit.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);

        // Due to integer allocation, expect approximately 25:75 ratio
        Assert.True(child1.AllocatedHeight >= 20 && child1.AllocatedHeight <= 30,
            $"Child1 should be ~25, was {child1.AllocatedHeight}");
        Assert.True(child2.AllocatedHeight >= 70 && child2.AllocatedHeight <= 80,
            $"Child2 should be ~75, was {child2.AllocatedHeight}");
    }

    [Fact]
    public void DivideHeights_Weight2vs8_Roughly1to4Ratio()
    {
        var child1 = new TestContainer(height: new Dimension(min: 0, max: 100, preferred: 0, weight: 2));
        var child2 = new TestContainer(height: new Dimension(min: 0, max: 100, preferred: 0, weight: 8));

        var hsplit = new HSplit([child1, child2]);

        var screen = new Screen(initialWidth: 80, initialHeight: 100);
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 80, 100);

        hsplit.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);

        // Weight ratio 2:8 = 1:4, expect approximately 20:80
        Assert.True(child1.AllocatedHeight >= 15 && child1.AllocatedHeight <= 25,
            $"Child1 should be ~20, was {child1.AllocatedHeight}");
        Assert.True(child2.AllocatedHeight >= 75 && child2.AllocatedHeight <= 85,
            $"Child2 should be ~80, was {child2.AllocatedHeight}");
    }

    #endregion

    #region Test Vector 3: Fixed Size Children

    [Fact]
    public void DivideHeights_FixedSizeChild_GetsExactSize()
    {
        // Child with min=max=preferred=20 should get exactly 20
        var fixedChild = new TestContainer(height: new Dimension(min: 20, max: 20, preferred: 20));
        var flexChild = new TestContainer(height: new Dimension(min: 0, max: 100, preferred: 50, weight: 1));

        var hsplit = new HSplit([fixedChild, flexChild]);

        var screen = new Screen(initialWidth: 80, initialHeight: 100);
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 80, 100);

        hsplit.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);

        Assert.Equal(20, fixedChild.AllocatedHeight);
        Assert.Equal(80, flexChild.AllocatedHeight);
    }

    [Fact]
    public void DivideHeights_TwoFixedSizeChildren_BothGetExactSize()
    {
        var fixed1 = new TestContainer(height: new Dimension(min: 30, max: 30, preferred: 30));
        var fixed2 = new TestContainer(height: new Dimension(min: 25, max: 25, preferred: 25));

        var hsplit = new HSplit([fixed1, fixed2]);

        var screen = new Screen(initialWidth: 80, initialHeight: 100);
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 80, 100);

        hsplit.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);

        Assert.Equal(30, fixed1.AllocatedHeight);
        Assert.Equal(25, fixed2.AllocatedHeight);
    }

    #endregion

    #region Test Vector 4: Minimum Size Constraints

    [Fact]
    public void DivideHeights_MinimumConstraint_RespectsMinimum()
    {
        // Child with min=30 should get at least 30
        var minChild = new TestContainer(height: new Dimension(min: 30, max: 100, preferred: 50, weight: 1));
        var flexChild = new TestContainer(height: new Dimension(min: 0, max: 100, preferred: 50, weight: 1));

        var hsplit = new HSplit([minChild, flexChild]);

        var screen = new Screen(initialWidth: 80, initialHeight: 50);
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 80, 50);

        hsplit.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);

        Assert.True(minChild.AllocatedHeight >= 30,
            $"MinChild should be at least 30, was {minChild.AllocatedHeight}");
    }

    #endregion

    #region Test Vector 5: Maximum Size Constraints

    [Fact]
    public void DivideHeights_MaximumConstraint_RespectsMaximum()
    {
        // Child with max=20 should get at most 20
        var maxChild = new TestContainer(height: new Dimension(min: 0, max: 20, preferred: 20, weight: 1));
        var flexChild = new TestContainer(height: new Dimension(min: 0, max: 200, preferred: 100, weight: 1));

        var hsplit = new HSplit([maxChild, flexChild]);

        var screen = new Screen(initialWidth: 80, initialHeight: 100);
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 80, 100);

        hsplit.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);

        Assert.True(maxChild.AllocatedHeight <= 20,
            $"MaxChild should be at most 20, was {maxChild.AllocatedHeight}");
        Assert.True(flexChild.AllocatedHeight >= 80,
            $"FlexChild should get remaining space, was {flexChild.AllocatedHeight}");
    }

    #endregion

    #region Test Vector 6: Preferred Size Allocation

    [Fact]
    public void DivideHeights_PreferredSizeRespected_WhenSpaceAvailable()
    {
        var child1 = new TestContainer(height: new Dimension(min: 0, max: 100, preferred: 40, weight: 1));
        var child2 = new TestContainer(height: new Dimension(min: 0, max: 100, preferred: 60, weight: 1));

        var hsplit = new HSplit([child1, child2]);

        var screen = new Screen(initialWidth: 80, initialHeight: 100);
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 80, 100);

        hsplit.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);

        // Both should reach their preferred sizes
        Assert.True(child1.AllocatedHeight >= 40,
            $"Child1 should be at least 40 (preferred), was {child1.AllocatedHeight}");
        Assert.True(child2.AllocatedHeight >= 60,
            $"Child2 should be at least 60 (preferred), was {child2.AllocatedHeight}");
    }

    #endregion

    #region Test Vector 7: Zero Weight

    [Fact]
    public void DivideHeights_ZeroWeight_GetsMinimumOnly()
    {
        var zeroWeightChild = new TestContainer(height: new Dimension(min: 10, max: 100, preferred: 50, weight: 0));
        var weightedChild = new TestContainer(height: new Dimension(min: 0, max: 100, preferred: 50, weight: 1));

        var hsplit = new HSplit([zeroWeightChild, weightedChild]);

        var screen = new Screen(initialWidth: 80, initialHeight: 100);
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 80, 100);

        hsplit.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);

        // Zero-weight child should stay at minimum
        Assert.Equal(10, zeroWeightChild.AllocatedHeight);
        // Weighted child gets the rest
        Assert.Equal(90, weightedChild.AllocatedHeight);
    }

    #endregion

    #region Test Vector 8: All Zero Weights

    [Fact]
    public void DivideHeights_AllZeroWeights_GetsMinimums()
    {
        var child1 = new TestContainer(height: new Dimension(min: 10, max: 100, preferred: 50, weight: 0));
        var child2 = new TestContainer(height: new Dimension(min: 20, max: 100, preferred: 50, weight: 0));

        var hsplit = new HSplit([child1, child2]);

        var screen = new Screen(initialWidth: 80, initialHeight: 100);
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 80, 100);

        hsplit.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);

        // Both should get their minimums
        Assert.Equal(10, child1.AllocatedHeight);
        Assert.Equal(20, child2.AllocatedHeight);
    }

    #endregion

    #region Test Vector 9: Excess Space

    [Fact]
    public void DivideHeights_ExcessSpace_DistributedByWeight()
    {
        // When all maxes are less than available space
        var child1 = new TestContainer(height: new Dimension(min: 0, max: 30, preferred: 20, weight: 1));
        var child2 = new TestContainer(height: new Dimension(min: 0, max: 40, preferred: 30, weight: 2));

        var hsplit = new HSplit([child1, child2]);

        var screen = new Screen(initialWidth: 80, initialHeight: 100);
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 80, 100);

        hsplit.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);

        // Should fill to max since total max (70) < available (100)
        Assert.Equal(30, child1.AllocatedHeight);
        Assert.Equal(40, child2.AllocatedHeight);
    }

    #endregion

    #region Test Vector 10: Constrained Space

    [Fact]
    public void DivideHeights_ConstrainedSpace_ScalesProportionally()
    {
        // When preferred exceeds available space
        var child1 = new TestContainer(height: new Dimension(min: 0, max: 100, preferred: 60, weight: 1));
        var child2 = new TestContainer(height: new Dimension(min: 0, max: 100, preferred: 60, weight: 1));

        var hsplit = new HSplit([child1, child2]);

        // Only 100 rows but preferred sum is 120
        var screen = new Screen(initialWidth: 80, initialHeight: 100);
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 80, 100);

        hsplit.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);

        // Should split evenly since weights are equal
        Assert.Equal(50, child1.AllocatedHeight);
        Assert.Equal(50, child2.AllocatedHeight);
    }

    #endregion

    #region Test Vector 11: Padding Between Children

    [Fact]
    public void DivideHeights_WithPadding_AccountsForPaddingSpace()
    {
        var child1 = new TestContainer(height: new Dimension(min: 0, max: 100, preferred: 50, weight: 1));
        var child2 = new TestContainer(height: new Dimension(min: 0, max: 100, preferred: 50, weight: 1));

        var hsplit = new HSplit([child1, child2], padding: 2);

        var screen = new Screen(initialWidth: 80, initialHeight: 100);
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 80, 100);

        hsplit.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);

        // Total should be 100 with 2 for padding
        var totalAllocated = child1.AllocatedHeight + child2.AllocatedHeight;
        Assert.True(totalAllocated <= 98, $"Children should leave room for padding, got {totalAllocated}");
    }

    #endregion

    #region Test Vector 12: Many Children

    [Fact]
    public void DivideHeights_TenChildren_DistributesEvenly()
    {
        var children = Enumerable.Range(0, 10)
            .Select(_ => new TestContainer(height: new Dimension(min: 0, max: 100, preferred: 10, weight: 1)))
            .ToList();

        var hsplit = new HSplit(children);

        var screen = new Screen(initialWidth: 80, initialHeight: 100);
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 80, 100);

        hsplit.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);

        // Each should get exactly 10
        foreach (var child in children)
        {
            Assert.Equal(10, child.AllocatedHeight);
        }
    }

    #endregion
}
