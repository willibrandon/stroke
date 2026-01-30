using Stroke.Layout.Windows;
using Xunit;

namespace Stroke.Tests.Layout.Windows;

/// <summary>
/// Tests for ScrollOffsets class.
/// </summary>
public sealed class ScrollOffsetsTests
{
    #region Constructor Tests - Constant Values

    [Fact]
    public void Constructor_DefaultValues_AllZero()
    {
        var offsets = new ScrollOffsets(0, 0, 0, 0);

        Assert.Equal(0, offsets.Top);
        Assert.Equal(0, offsets.Bottom);
        Assert.Equal(0, offsets.Left);
        Assert.Equal(0, offsets.Right);
    }

    [Fact]
    public void Constructor_WithConstantValues_StoresValues()
    {
        var offsets = new ScrollOffsets(top: 3, bottom: 5, left: 2, right: 4);

        Assert.Equal(3, offsets.Top);
        Assert.Equal(5, offsets.Bottom);
        Assert.Equal(2, offsets.Left);
        Assert.Equal(4, offsets.Right);
    }

    [Fact]
    public void Constructor_PartialValues_UsesDefaults()
    {
        var offsets = new ScrollOffsets(top: 3);

        Assert.Equal(3, offsets.Top);
        Assert.Equal(0, offsets.Bottom);
        Assert.Equal(0, offsets.Left);
        Assert.Equal(0, offsets.Right);
    }

    [Fact]
    public void Constructor_NegativeTop_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ScrollOffsets(top: -1));
    }

    [Fact]
    public void Constructor_NegativeBottom_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ScrollOffsets(bottom: -1));
    }

    [Fact]
    public void Constructor_NegativeLeft_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ScrollOffsets(left: -1));
    }

    [Fact]
    public void Constructor_NegativeRight_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ScrollOffsets(right: -1));
    }

    #endregion

    #region Constructor Tests - Dynamic Values

    [Fact]
    public void Constructor_WithFunctions_CallsFunctions()
    {
        int topCallCount = 0;
        int bottomCallCount = 0;

        var offsets = new ScrollOffsets(
            topGetter: () => { topCallCount++; return 3; },
            bottomGetter: () => { bottomCallCount++; return 5; });

        // Access properties multiple times
        _ = offsets.Top;
        _ = offsets.Top;
        _ = offsets.Bottom;

        Assert.Equal(2, topCallCount);
        Assert.Equal(1, bottomCallCount);
    }

    [Fact]
    public void Constructor_WithNullFunctions_UsesDefaults()
    {
        var offsets = new ScrollOffsets(
            topGetter: null,
            bottomGetter: null,
            leftGetter: null,
            rightGetter: null);

        Assert.Equal(0, offsets.Top);
        Assert.Equal(0, offsets.Bottom);
        Assert.Equal(0, offsets.Left);
        Assert.Equal(0, offsets.Right);
    }

    [Fact]
    public void Constructor_MixedFunctionsAndNulls_WorksCorrectly()
    {
        var offsets = new ScrollOffsets(
            topGetter: () => 10,
            bottomGetter: null,
            leftGetter: () => 5,
            rightGetter: null);

        Assert.Equal(10, offsets.Top);
        Assert.Equal(0, offsets.Bottom);
        Assert.Equal(5, offsets.Left);
        Assert.Equal(0, offsets.Right);
    }

    #endregion

    #region Property Tests

    [Fact]
    public void Properties_ReturnFunctionResults()
    {
        int value = 0;
        var offsets = new ScrollOffsets(topGetter: () => ++value);

        Assert.Equal(1, offsets.Top);
        Assert.Equal(2, offsets.Top);
        Assert.Equal(3, offsets.Top);
    }

    [Fact]
    public void TopGetter_ReturnsProvidedFunction()
    {
        Func<int> getter = () => 42;
        var offsets = new ScrollOffsets(topGetter: getter);

        Assert.Same(getter, offsets.TopGetter);
    }

    [Fact]
    public void BottomGetter_ReturnsProvidedFunction()
    {
        Func<int> getter = () => 42;
        var offsets = new ScrollOffsets(bottomGetter: getter);

        Assert.Same(getter, offsets.BottomGetter);
    }

    [Fact]
    public void LeftGetter_ReturnsProvidedFunction()
    {
        Func<int> getter = () => 42;
        var offsets = new ScrollOffsets(leftGetter: getter);

        Assert.Same(getter, offsets.LeftGetter);
    }

    [Fact]
    public void RightGetter_ReturnsProvidedFunction()
    {
        Func<int> getter = () => 42;
        var offsets = new ScrollOffsets(rightGetter: getter);

        Assert.Same(getter, offsets.RightGetter);
    }

    #endregion

    #region ToString Tests

    [Fact]
    public void ToString_ReturnsFormattedString()
    {
        var offsets = new ScrollOffsets(top: 1, bottom: 2, left: 3, right: 4);

        var result = offsets.ToString();

        Assert.Equal("ScrollOffsets(top=1, bottom=2, left=3, right=4)", result);
    }

    [Fact]
    public void ToString_WithDynamicValues_ReturnsCurrentValues()
    {
        int value = 5;
        var offsets = new ScrollOffsets(topGetter: () => value);

        value = 10;
        var result = offsets.ToString();

        Assert.Contains("top=10", result);
    }

    #endregion
}
