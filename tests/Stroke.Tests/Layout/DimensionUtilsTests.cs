using Stroke.Layout;
using Xunit;

namespace Stroke.Tests.Layout;

/// <summary>
/// Unit tests for the DimensionUtils static class.
/// </summary>
public class DimensionUtilsTests
{
    #region T029: SumLayoutDimensions Basic Tests

    [Fact]
    public void SumLayoutDimensions_TwoDimensions_SumsMinMaxPreferred()
    {
        var dimensions = new List<Dimension>
        {
            new Dimension(min: 5, max: 10, preferred: 7),
            new Dimension(min: 3, max: 8, preferred: 5)
        };

        var result = DimensionUtils.SumLayoutDimensions(dimensions);

        Assert.Equal(8, result.Min);
        Assert.Equal(18, result.Max);
        Assert.Equal(12, result.Preferred);
    }

    [Fact]
    public void SumLayoutDimensions_SingleDimension_ReturnsSameValues()
    {
        var dimensions = new List<Dimension>
        {
            new Dimension(min: 10, max: 50, preferred: 30)
        };

        var result = DimensionUtils.SumLayoutDimensions(dimensions);

        Assert.Equal(10, result.Min);
        Assert.Equal(50, result.Max);
        Assert.Equal(30, result.Preferred);
    }

    [Fact]
    public void SumLayoutDimensions_ThreeDimensions_SumsAll()
    {
        var dimensions = new List<Dimension>
        {
            new Dimension(min: 10, max: 20, preferred: 15),
            new Dimension(min: 5, max: 10, preferred: 8),
            new Dimension(min: 3, max: 7, preferred: 5)
        };

        var result = DimensionUtils.SumLayoutDimensions(dimensions);

        Assert.Equal(18, result.Min);
        Assert.Equal(37, result.Max);
        Assert.Equal(28, result.Preferred);
    }

    #endregion

    #region T030: SumLayoutDimensions Empty List Test

    [Fact]
    public void SumLayoutDimensions_EmptyList_ReturnsZeroDimension()
    {
        var dimensions = new List<Dimension>();

        var result = DimensionUtils.SumLayoutDimensions(dimensions);

        Assert.Equal(0, result.Min);
        Assert.Equal(0, result.Max);
        Assert.Equal(0, result.Preferred);
    }

    #endregion

    #region T031: SumLayoutDimensions Null List Test

    [Fact]
    public void SumLayoutDimensions_NullList_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            DimensionUtils.SumLayoutDimensions(null!));
    }

    #endregion

    #region T032: MaxLayoutDimensions Basic Tests

    [Fact]
    public void MaxLayoutDimensions_TwoDimensions_TakesHighestMin()
    {
        var dimensions = new List<Dimension>
        {
            new Dimension(min: 5, max: 50, preferred: 20),
            new Dimension(min: 10, max: 40, preferred: 25)
        };

        var result = DimensionUtils.MaxLayoutDimensions(dimensions);

        Assert.Equal(10, result.Min); // Highest min
    }

    [Fact]
    public void MaxLayoutDimensions_TwoDimensions_TakesHighestPreferred()
    {
        var dimensions = new List<Dimension>
        {
            new Dimension(min: 5, max: 50, preferred: 20),
            new Dimension(min: 10, max: 40, preferred: 25)
        };

        var result = DimensionUtils.MaxLayoutDimensions(dimensions);

        Assert.Equal(25, result.Preferred); // Highest preferred
    }

    [Fact]
    public void MaxLayoutDimensions_SingleDimension_ReturnsSameValues()
    {
        var dimensions = new List<Dimension>
        {
            new Dimension(min: 10, max: 50, preferred: 30)
        };

        var result = DimensionUtils.MaxLayoutDimensions(dimensions);

        Assert.Equal(10, result.Min);
        Assert.Equal(50, result.Max);
        Assert.Equal(30, result.Preferred);
    }

    #endregion

    #region T033: MaxLayoutDimensions Empty List Test

    [Fact]
    public void MaxLayoutDimensions_EmptyList_ReturnsZeroDimension()
    {
        var dimensions = new List<Dimension>();

        var result = DimensionUtils.MaxLayoutDimensions(dimensions);

        Assert.Equal(0, result.Min);
        Assert.Equal(0, result.Max);
        Assert.Equal(0, result.Preferred);
    }

    #endregion

    #region T034: MaxLayoutDimensions All-Zero List Test

    [Fact]
    public void MaxLayoutDimensions_AllZeroDimensions_ReturnsZeroDimension()
    {
        var dimensions = new List<Dimension>
        {
            Dimension.Zero(),
            Dimension.Zero()
        };

        var result = DimensionUtils.MaxLayoutDimensions(dimensions);

        Assert.Equal(0, result.Min);
        Assert.Equal(0, result.Max);
        Assert.Equal(0, result.Preferred);
    }

    #endregion

    #region T035: MaxLayoutDimensions Zero-Filtering Tests

    [Fact]
    public void MaxLayoutDimensions_MixedWithZero_FiltersOutZero()
    {
        var dimensions = new List<Dimension>
        {
            Dimension.Zero(),
            new Dimension(min: 10, max: 50, preferred: 30)
        };

        var result = DimensionUtils.MaxLayoutDimensions(dimensions);

        // Should filter out zero dimension and use only the non-zero one
        Assert.Equal(10, result.Min);
        Assert.Equal(50, result.Max);
        Assert.Equal(30, result.Preferred);
    }

    [Fact]
    public void MaxLayoutDimensions_ZeroPreferredNonZeroMax_IsFiltered()
    {
        // Python: [d for d in dimensions if d.preferred != 0 and d.max != 0]
        // Dimensions with preferred=0 are filtered out even if max>0
        var dimensions = new List<Dimension>
        {
            new Dimension(min: 0, max: 10, preferred: 0),  // preferred=0, filtered OUT
            new Dimension(min: 5, max: 20, preferred: 15)  // kept
        };

        var result = DimensionUtils.MaxLayoutDimensions(dimensions);

        // First dimension filtered out, only second remains
        Assert.Equal(5, result.Min);
        Assert.Equal(20, result.Max);
        Assert.Equal(15, result.Preferred);
    }

    #endregion

    #region T036: MaxLayoutDimensions Non-Overlapping Ranges Test

    [Fact]
    public void MaxLayoutDimensions_NonOverlappingRanges_AdjustsMaxToMin()
    {
        // Range 1: [1, 5]
        // Range 2: [8, 9]
        // These don't overlap. Highest min is 8, lowest max is 5.
        // Since min > max, we adjust max = min
        var dimensions = new List<Dimension>
        {
            new Dimension(min: 1, max: 5, preferred: 3),
            new Dimension(min: 8, max: 9, preferred: 8)
        };

        var result = DimensionUtils.MaxLayoutDimensions(dimensions);

        Assert.Equal(8, result.Min);  // Highest min
        // Max should be adjusted to be >= min
        Assert.True(result.Max >= result.Min);
    }

    #endregion

    #region T037: MaxLayoutDimensions Null List Test

    [Fact]
    public void MaxLayoutDimensions_NullList_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            DimensionUtils.MaxLayoutDimensions(null!));
    }

    #endregion

    #region T041: ToDimension Null Input Test

    [Fact]
    public void ToDimension_NullInput_ReturnsDefaultDimension()
    {
        var result = DimensionUtils.ToDimension(null);

        Assert.Equal(0, result.Min);
        Assert.Equal(Dimension.MaxDimensionValue, result.Max);
    }

    #endregion

    #region T042: ToDimension Int Input Test

    [Fact]
    public void ToDimension_IntInput_ReturnsExactDimension()
    {
        var result = DimensionUtils.ToDimension(25);

        Assert.Equal(25, result.Min);
        Assert.Equal(25, result.Max);
        Assert.Equal(25, result.Preferred);
    }

    [Fact]
    public void ToDimension_ZeroInt_ReturnsZeroDimension()
    {
        var result = DimensionUtils.ToDimension(0);

        Assert.Equal(0, result.Min);
        Assert.Equal(0, result.Max);
        Assert.Equal(0, result.Preferred);
    }

    #endregion

    #region T043: ToDimension Dimension Passthrough Test

    [Fact]
    public void ToDimension_DimensionInput_ReturnsSameInstance()
    {
        var input = new Dimension(min: 10, max: 50, preferred: 30);

        var result = DimensionUtils.ToDimension(input);

        Assert.Same(input, result);
    }

    #endregion

    #region T044: ToDimension Func<object?> Callable Tests

    [Fact]
    public void ToDimension_FuncReturningDimension_CallsAndReturnsDimension()
    {
        var dimension = new Dimension(min: 15, max: 45, preferred: 25);
        Func<object?> callable = () => dimension;

        var result = DimensionUtils.ToDimension(callable);

        Assert.Same(dimension, result);
    }

    [Fact]
    public void ToDimension_FuncReturningInt_CallsAndReturnsExactDimension()
    {
        Func<object?> callable = () => 42;

        var result = DimensionUtils.ToDimension(callable);

        Assert.Equal(42, result.Min);
        Assert.Equal(42, result.Max);
        Assert.Equal(42, result.Preferred);
    }

    [Fact]
    public void ToDimension_FuncReturningNull_CallsAndReturnsDefaultDimension()
    {
        Func<object?> callable = () => null;

        var result = DimensionUtils.ToDimension(callable);

        Assert.Equal(0, result.Min);
        Assert.Equal(Dimension.MaxDimensionValue, result.Max);
    }

    #endregion

    #region T045: ToDimension Nested Callable Test

    [Fact]
    public void ToDimension_NestedCallables_ResolvesRecursively()
    {
        var innerDimension = new Dimension(min: 20, max: 60, preferred: 40);
        Func<object?> innerCallable = () => innerDimension;
        Func<object?> outerCallable = () => innerCallable;

        var result = DimensionUtils.ToDimension(outerCallable);

        Assert.Same(innerDimension, result);
    }

    [Fact]
    public void ToDimension_DeeplyNestedCallables_ResolvesRecursively()
    {
        Func<object?> level3 = () => 100;
        Func<object?> level2 = () => level3;
        Func<object?> level1 = () => level2;

        var result = DimensionUtils.ToDimension(level1);

        Assert.Equal(100, result.Min);
        Assert.Equal(100, result.Max);
        Assert.Equal(100, result.Preferred);
    }

    #endregion

    #region T046: ToDimension Unsupported Type Test

    [Fact]
    public void ToDimension_UnsupportedType_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            DimensionUtils.ToDimension("string value"));

        Assert.Equal("Not an integer or Dimension object.", ex.Message);
    }

    [Fact]
    public void ToDimension_DoubleType_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            DimensionUtils.ToDimension(3.14));

        Assert.Equal("Not an integer or Dimension object.", ex.Message);
    }

    #endregion

    #region T047: IsDimension Tests for Supported Types

    [Fact]
    public void IsDimension_NullValue_ReturnsTrue()
    {
        Assert.True(DimensionUtils.IsDimension(null));
    }

    [Fact]
    public void IsDimension_IntValue_ReturnsTrue()
    {
        Assert.True(DimensionUtils.IsDimension(42));
    }

    [Fact]
    public void IsDimension_DimensionValue_ReturnsTrue()
    {
        Assert.True(DimensionUtils.IsDimension(new Dimension()));
    }

    [Fact]
    public void IsDimension_FuncValue_ReturnsTrue()
    {
        Func<object?> callable = () => 10;
        Assert.True(DimensionUtils.IsDimension(callable));
    }

    [Fact]
    public void IsDimension_CallableWithoutInvoking_ReturnsTrue()
    {
        bool wasCalled = false;
        Func<object?> callable = () =>
        {
            wasCalled = true;
            return 10;
        };

        var result = DimensionUtils.IsDimension(callable);

        Assert.True(result);
        Assert.False(wasCalled); // Should not invoke the callable
    }

    #endregion

    #region T048: IsDimension Tests for Unsupported Types

    [Fact]
    public void IsDimension_StringValue_ReturnsFalse()
    {
        Assert.False(DimensionUtils.IsDimension("string"));
    }

    [Fact]
    public void IsDimension_DoubleValue_ReturnsFalse()
    {
        Assert.False(DimensionUtils.IsDimension(3.14));
    }

    [Fact]
    public void IsDimension_ObjectValue_ReturnsFalse()
    {
        Assert.False(DimensionUtils.IsDimension(new object()));
    }

    [Fact]
    public void IsDimension_ListValue_ReturnsFalse()
    {
        Assert.False(DimensionUtils.IsDimension(new List<int>()));
    }

    #endregion

    #region T053: D.Create Tests

    [Fact]
    public void D_Create_NoArguments_ReturnsDefaultDimension()
    {
        var result = D.Create();

        Assert.Equal(0, result.Min);
        Assert.Equal(Dimension.MaxDimensionValue, result.Max);
        Assert.Equal(0, result.Preferred);
        Assert.Equal(1, result.Weight);
    }

    [Fact]
    public void D_Create_WithArguments_ReturnsConfiguredDimension()
    {
        var result = D.Create(min: 10, max: 50, weight: 2, preferred: 30);

        Assert.Equal(10, result.Min);
        Assert.Equal(50, result.Max);
        Assert.Equal(30, result.Preferred);
        Assert.Equal(2, result.Weight);
    }

    #endregion

    #region T054: D.Exact Tests

    [Fact]
    public void D_Exact_ReturnsExactDimension()
    {
        var result = D.Exact(25);

        Assert.Equal(25, result.Min);
        Assert.Equal(25, result.Max);
        Assert.Equal(25, result.Preferred);
    }

    #endregion

    #region T055: D.Zero Tests

    [Fact]
    public void D_Zero_ReturnsZeroDimension()
    {
        var result = D.Zero();

        Assert.Equal(0, result.Min);
        Assert.Equal(0, result.Max);
        Assert.Equal(0, result.Preferred);
    }

    #endregion

    #region T061: Edge Case Tests

    [Fact]
    public void MaxLayoutDimensions_AllIdenticalDimensions_ReturnsSameConstraints()
    {
        var dimensions = new List<Dimension>
        {
            new Dimension(min: 10, max: 50, preferred: 30),
            new Dimension(min: 10, max: 50, preferred: 30),
            new Dimension(min: 10, max: 50, preferred: 30)
        };

        var result = DimensionUtils.MaxLayoutDimensions(dimensions);

        Assert.Equal(10, result.Min);
        Assert.Equal(50, result.Max);
        Assert.Equal(30, result.Preferred);
    }

    [Fact]
    public void SumLayoutDimensions_WithZeroDimensions_IncludesZeroInSum()
    {
        var dimensions = new List<Dimension>
        {
            new Dimension(min: 10, max: 20, preferred: 15),
            Dimension.Zero()
        };

        var result = DimensionUtils.SumLayoutDimensions(dimensions);

        Assert.Equal(10, result.Min);
        Assert.Equal(20, result.Max);
        Assert.Equal(15, result.Preferred);
    }

    #endregion

    #region T063: Quickstart Examples Validation

    [Fact]
    public void Quickstart_DefaultDimension_CorrectValues()
    {
        var d1 = new Dimension();
        Assert.Equal(0, d1.Min);
        Assert.Equal(1000000000, d1.Max);
        Assert.Equal(0, d1.Preferred);
        Assert.Equal(1, d1.Weight);
    }

    [Fact]
    public void Quickstart_SpecificConstraints_CorrectValues()
    {
        var d2 = new Dimension(min: 10, max: 50, preferred: 30);
        Assert.Equal(10, d2.Min);
        Assert.Equal(50, d2.Max);
        Assert.Equal(30, d2.Preferred);
    }

    [Fact]
    public void Quickstart_ExactSize_CorrectValues()
    {
        var d3 = Dimension.Exact(20);
        Assert.Equal(20, d3.Min);
        Assert.Equal(20, d3.Max);
        Assert.Equal(20, d3.Preferred);
    }

    [Fact]
    public void Quickstart_ZeroSize_CorrectValues()
    {
        var d4 = Dimension.Zero();
        Assert.Equal(0, d4.Min);
        Assert.Equal(0, d4.Max);
        Assert.Equal(0, d4.Preferred);
    }

    [Fact]
    public void Quickstart_DAliasCreate_CorrectValues()
    {
        var d5 = D.Create(min: 5, max: 100, weight: 2);
        Assert.Equal(5, d5.Min);
        Assert.Equal(100, d5.Max);
        Assert.Equal(2, d5.Weight);
    }

    [Fact]
    public void Quickstart_SpecifiedProperties_CorrectBooleans()
    {
        var d = new Dimension(min: 10, preferred: 20);
        Assert.True(d.MinSpecified);
        Assert.False(d.MaxSpecified);
        Assert.True(d.PreferredSpecified);
        Assert.False(d.WeightSpecified);
    }

    [Fact]
    public void Quickstart_SumDimensions_CorrectValues()
    {
        var dimensions = new List<Dimension>
        {
            new Dimension(min: 10, max: 30, preferred: 20),
            new Dimension(min: 5, max: 15, preferred: 10)
        };

        var total = DimensionUtils.SumLayoutDimensions(dimensions);
        Assert.Equal(15, total.Min);
        Assert.Equal(45, total.Max);
        Assert.Equal(30, total.Preferred);
    }

    [Fact]
    public void Quickstart_PreferredClamping_ClampsToMax()
    {
        var d = new Dimension(min: 10, max: 20, preferred: 100);
        Assert.Equal(20, d.Preferred);
    }

    [Fact]
    public void Quickstart_DebugOutput_ShowsSpecifiedOnly()
    {
        var d = new Dimension(min: 10, max: 50, weight: 2);
        Assert.Equal("Dimension(min=10, max=50, weight=2)", d.ToString());
    }

    [Fact]
    public void Quickstart_DynamicDimension_ResolvesCallable()
    {
        Func<object?> exactSize = () => 42;
        var d = DimensionUtils.ToDimension(exactSize);
        Assert.Equal(42, d.Min);
        Assert.Equal(42, d.Max);
        Assert.Equal(42, d.Preferred);
    }

    #endregion
}
