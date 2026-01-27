using Stroke.Layout;
using Xunit;

namespace Stroke.Tests.Layout;

/// <summary>
/// Unit tests for the Dimension class.
/// </summary>
public class DimensionTests
{
    #region T007: Constructor Default Value Tests

    [Fact]
    public void Constructor_NoArguments_UsesDefaultValues()
    {
        var d = new Dimension();

        Assert.Equal(0, d.Min);
        Assert.Equal(Dimension.MaxDimensionValue, d.Max);
        Assert.Equal(0, d.Preferred); // Defaults to min
        Assert.Equal(Dimension.DefaultWeight, d.Weight);
    }

    [Fact]
    public void Constructor_NoArguments_MaxDimensionValueIs1Billion()
    {
        Assert.Equal(1_000_000_000, Dimension.MaxDimensionValue);
    }

    [Fact]
    public void Constructor_NoArguments_DefaultWeightIs1()
    {
        Assert.Equal(1, Dimension.DefaultWeight);
    }

    [Fact]
    public void Constructor_OnlyMinSpecified_PreferredDefaultsToMin()
    {
        var d = new Dimension(min: 10);

        Assert.Equal(10, d.Min);
        Assert.Equal(10, d.Preferred); // Defaults to min
    }

    #endregion

    #region T008: Constructor Explicit Value Tests

    [Fact]
    public void Constructor_AllValuesSpecified_UsesProvidedValues()
    {
        var d = new Dimension(min: 5, max: 100, weight: 3, preferred: 50);

        Assert.Equal(5, d.Min);
        Assert.Equal(100, d.Max);
        Assert.Equal(50, d.Preferred);
        Assert.Equal(3, d.Weight);
    }

    [Fact]
    public void Constructor_MinOnly_OtherValuesUseDefaults()
    {
        var d = new Dimension(min: 20);

        Assert.Equal(20, d.Min);
        Assert.Equal(Dimension.MaxDimensionValue, d.Max);
        Assert.Equal(20, d.Preferred); // Defaults to min
        Assert.Equal(1, d.Weight);
    }

    [Fact]
    public void Constructor_MaxOnly_OtherValuesUseDefaults()
    {
        var d = new Dimension(max: 100);

        Assert.Equal(0, d.Min);
        Assert.Equal(100, d.Max);
        Assert.Equal(0, d.Preferred); // Defaults to min (0)
        Assert.Equal(1, d.Weight);
    }

    [Fact]
    public void Constructor_MinAndMax_PreferredDefaultsToMin()
    {
        var d = new Dimension(min: 10, max: 50);

        Assert.Equal(10, d.Min);
        Assert.Equal(50, d.Max);
        Assert.Equal(10, d.Preferred); // Defaults to min
    }

    #endregion

    #region T009: Validation Tests (Negative Values)

    [Fact]
    public void Constructor_NegativeMin_ThrowsArgumentOutOfRangeException()
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => new Dimension(min: -1));
        Assert.Equal("min", ex.ParamName);
    }

    [Fact]
    public void Constructor_NegativeMax_ThrowsArgumentOutOfRangeException()
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => new Dimension(max: -1));
        Assert.Equal("max", ex.ParamName);
    }

    [Fact]
    public void Constructor_NegativePreferred_ThrowsArgumentOutOfRangeException()
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => new Dimension(preferred: -1));
        Assert.Equal("preferred", ex.ParamName);
    }

    [Fact]
    public void Constructor_NegativeWeight_ThrowsArgumentOutOfRangeException()
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => new Dimension(weight: -1));
        Assert.Equal("weight", ex.ParamName);
    }

    [Fact]
    public void Constructor_ZeroValues_AreValid()
    {
        var d = new Dimension(min: 0, max: 0, weight: 0, preferred: 0);

        Assert.Equal(0, d.Min);
        Assert.Equal(0, d.Max);
        Assert.Equal(0, d.Preferred);
        Assert.Equal(0, d.Weight);
    }

    #endregion

    #region T010: Validation Tests (max < min)

    [Fact]
    public void Constructor_MaxLessThanMin_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() => new Dimension(min: 50, max: 10));
        Assert.Equal("Invalid Dimension: max < min.", ex.Message);
    }

    [Fact]
    public void Constructor_MaxEqualsMin_IsValid()
    {
        var d = new Dimension(min: 25, max: 25);

        Assert.Equal(25, d.Min);
        Assert.Equal(25, d.Max);
        Assert.Equal(25, d.Preferred); // Clamped to range
    }

    #endregion

    #region T011: Preferred Clamping Tests

    [Fact]
    public void Constructor_PreferredBelowMin_ClampsToMin()
    {
        var d = new Dimension(min: 20, max: 50, preferred: 5);

        Assert.Equal(20, d.Preferred);
    }

    [Fact]
    public void Constructor_PreferredAboveMax_ClampsToMax()
    {
        var d = new Dimension(min: 10, max: 50, preferred: 100);

        Assert.Equal(50, d.Preferred);
    }

    [Fact]
    public void Constructor_PreferredWithinRange_NotClamped()
    {
        var d = new Dimension(min: 10, max: 50, preferred: 30);

        Assert.Equal(30, d.Preferred);
    }

    [Fact]
    public void Constructor_PreferredEqualsMin_NotClamped()
    {
        var d = new Dimension(min: 10, max: 50, preferred: 10);

        Assert.Equal(10, d.Preferred);
    }

    [Fact]
    public void Constructor_PreferredEqualsMax_NotClamped()
    {
        var d = new Dimension(min: 10, max: 50, preferred: 50);

        Assert.Equal(50, d.Preferred);
    }

    #endregion

    #region T012: *Specified Property Tests

    [Fact]
    public void Constructor_NoArguments_AllSpecifiedAreFalse()
    {
        var d = new Dimension();

        Assert.False(d.MinSpecified);
        Assert.False(d.MaxSpecified);
        Assert.False(d.PreferredSpecified);
        Assert.False(d.WeightSpecified);
    }

    [Fact]
    public void Constructor_MinSpecified_OnlyMinSpecifiedIsTrue()
    {
        var d = new Dimension(min: 10);

        Assert.True(d.MinSpecified);
        Assert.False(d.MaxSpecified);
        Assert.False(d.PreferredSpecified);
        Assert.False(d.WeightSpecified);
    }

    [Fact]
    public void Constructor_MaxSpecified_OnlyMaxSpecifiedIsTrue()
    {
        var d = new Dimension(max: 100);

        Assert.False(d.MinSpecified);
        Assert.True(d.MaxSpecified);
        Assert.False(d.PreferredSpecified);
        Assert.False(d.WeightSpecified);
    }

    [Fact]
    public void Constructor_PreferredSpecified_OnlyPreferredSpecifiedIsTrue()
    {
        var d = new Dimension(preferred: 50);

        Assert.False(d.MinSpecified);
        Assert.False(d.MaxSpecified);
        Assert.True(d.PreferredSpecified);
        Assert.False(d.WeightSpecified);
    }

    [Fact]
    public void Constructor_WeightSpecified_OnlyWeightSpecifiedIsTrue()
    {
        var d = new Dimension(weight: 2);

        Assert.False(d.MinSpecified);
        Assert.False(d.MaxSpecified);
        Assert.False(d.PreferredSpecified);
        Assert.True(d.WeightSpecified);
    }

    [Fact]
    public void Constructor_AllSpecified_AllSpecifiedAreTrue()
    {
        var d = new Dimension(min: 5, max: 100, weight: 2, preferred: 50);

        Assert.True(d.MinSpecified);
        Assert.True(d.MaxSpecified);
        Assert.True(d.PreferredSpecified);
        Assert.True(d.WeightSpecified);
    }

    #endregion

    #region T013: ToString Tests

    [Fact]
    public void ToString_NoArgumentsSpecified_ReturnsEmptyDimension()
    {
        var d = new Dimension();
        Assert.Equal("Dimension()", d.ToString());
    }

    [Fact]
    public void ToString_OnlyMinSpecified_ShowsOnlyMin()
    {
        var d = new Dimension(min: 5);
        Assert.Equal("Dimension(min=5)", d.ToString());
    }

    [Fact]
    public void ToString_OnlyMaxSpecified_ShowsOnlyMax()
    {
        var d = new Dimension(max: 100);
        Assert.Equal("Dimension(max=100)", d.ToString());
    }

    [Fact]
    public void ToString_OnlyPreferredSpecified_ShowsOnlyPreferred()
    {
        var d = new Dimension(preferred: 50);
        Assert.Equal("Dimension(preferred=50)", d.ToString());
    }

    [Fact]
    public void ToString_OnlyWeightSpecified_ShowsOnlyWeight()
    {
        var d = new Dimension(weight: 2);
        Assert.Equal("Dimension(weight=2)", d.ToString());
    }

    [Fact]
    public void ToString_MinAndMaxSpecified_ShowsBoth()
    {
        var d = new Dimension(min: 5, max: 10);
        Assert.Equal("Dimension(min=5, max=10)", d.ToString());
    }

    [Fact]
    public void ToString_AllSpecified_ShowsAllInOrder()
    {
        var d = new Dimension(min: 5, max: 10, preferred: 7, weight: 2);
        Assert.Equal("Dimension(min=5, max=10, preferred=7, weight=2)", d.ToString());
    }

    [Fact]
    public void ToString_MinMaxWeight_ShowsInOrder()
    {
        var d = new Dimension(min: 10, max: 50, weight: 2);
        Assert.Equal("Dimension(min=10, max=50, weight=2)", d.ToString());
    }

    #endregion

    #region T014: Weight Property Tests

    [Fact]
    public void Constructor_DefaultWeight_Is1()
    {
        var d = new Dimension();
        Assert.Equal(1, d.Weight);
    }

    [Fact]
    public void Constructor_ExplicitWeight_IsStored()
    {
        var d = new Dimension(weight: 5);
        Assert.Equal(5, d.Weight);
    }

    [Fact]
    public void Constructor_ZeroWeight_IsValid()
    {
        var d = new Dimension(weight: 0);
        Assert.Equal(0, d.Weight);
    }

    [Fact]
    public void Constructor_LargeWeight_IsStored()
    {
        var d = new Dimension(weight: 1000);
        Assert.Equal(1000, d.Weight);
    }

    #endregion

    #region T023: Exact() Factory Method Tests

    [Fact]
    public void Exact_CreatesFixedSizeDimension()
    {
        var d = Dimension.Exact(20);

        Assert.Equal(20, d.Min);
        Assert.Equal(20, d.Max);
        Assert.Equal(20, d.Preferred);
    }

    [Fact]
    public void Exact_ZeroAmount_CreatesZeroDimension()
    {
        var d = Dimension.Exact(0);

        Assert.Equal(0, d.Min);
        Assert.Equal(0, d.Max);
        Assert.Equal(0, d.Preferred);
    }

    [Fact]
    public void Exact_LargeAmount_CreatesLargeDimension()
    {
        var d = Dimension.Exact(500);

        Assert.Equal(500, d.Min);
        Assert.Equal(500, d.Max);
        Assert.Equal(500, d.Preferred);
    }

    #endregion

    #region T024: Zero() Factory Method Tests

    [Fact]
    public void Zero_CreatesZeroSizeDimension()
    {
        var d = Dimension.Zero();

        Assert.Equal(0, d.Min);
        Assert.Equal(0, d.Max);
        Assert.Equal(0, d.Preferred);
    }

    [Fact]
    public void Zero_MultipleCallsReturnEquivalentDimensions()
    {
        var d1 = Dimension.Zero();
        var d2 = Dimension.Zero();

        Assert.Equal(d1.Min, d2.Min);
        Assert.Equal(d1.Max, d2.Max);
        Assert.Equal(d1.Preferred, d2.Preferred);
    }

    #endregion

    #region T025: Exact() Negative Amount Validation Tests

    [Fact]
    public void Exact_NegativeAmount_ThrowsArgumentOutOfRangeException()
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => Dimension.Exact(-1));
        Assert.Equal("amount", ex.ParamName);
    }

    [Fact]
    public void Exact_NegativeLargeAmount_ThrowsArgumentOutOfRangeException()
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => Dimension.Exact(-100));
        Assert.Equal("amount", ex.ParamName);
    }

    #endregion
}
