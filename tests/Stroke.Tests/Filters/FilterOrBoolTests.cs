using Stroke.Filters;
using Xunit;

namespace Stroke.Tests.Filters;

/// <summary>
/// Tests for the <see cref="FilterOrBool"/> readonly struct.
/// </summary>
public sealed class FilterOrBoolTests
{
    #region Constructor Tests (Bool)

    [Fact]
    public void Constructor_WithTrue_CreatesCorrectInstance()
    {
        var fob = new FilterOrBool(true);

        Assert.True(fob.IsBool);
        Assert.False(fob.IsFilter);
        Assert.True(fob.BoolValue);
    }

    [Fact]
    public void Constructor_WithFalse_CreatesCorrectInstance()
    {
        var fob = new FilterOrBool(false);

        Assert.True(fob.IsBool);
        Assert.False(fob.IsFilter);
        Assert.False(fob.BoolValue);
    }

    #endregion

    #region Constructor Tests (Filter)

    [Fact]
    public void Constructor_WithFilter_CreatesCorrectInstance()
    {
        var condition = new Condition(() => true);

        var fob = new FilterOrBool(condition);

        Assert.True(fob.IsFilter);
        Assert.False(fob.IsBool);
        Assert.Same(condition, fob.FilterValue);
    }

    [Fact]
    public void Constructor_WithNullFilter_TreatsAsNever()
    {
        var fob = new FilterOrBool((IFilter?)null);

        Assert.True(fob.IsFilter);
        Assert.False(fob.IsBool);
        Assert.Same(Never.Instance, fob.FilterValue);
    }

    [Fact]
    public void Constructor_WithAlways_StoresAlways()
    {
        var fob = new FilterOrBool(Always.Instance);

        Assert.True(fob.IsFilter);
        Assert.Same(Always.Instance, fob.FilterValue);
    }

    [Fact]
    public void Constructor_WithNever_StoresNever()
    {
        var fob = new FilterOrBool(Never.Instance);

        Assert.True(fob.IsFilter);
        Assert.Same(Never.Instance, fob.FilterValue);
    }

    #endregion

    #region Property Access Tests

    [Fact]
    public void BoolValue_WhenIsFilter_ThrowsInvalidOperationException()
    {
        var fob = new FilterOrBool(Always.Instance);

        Assert.Throws<InvalidOperationException>(() => fob.BoolValue);
    }

    [Fact]
    public void FilterValue_WhenIsBool_ThrowsInvalidOperationException()
    {
        var fob = new FilterOrBool(true);

        Assert.Throws<InvalidOperationException>(() => fob.FilterValue);
    }

    #endregion

    #region Implicit Conversion Tests

    [Fact]
    public void ImplicitConversion_FromTrue_Works()
    {
        FilterOrBool fob = true;

        Assert.True(fob.IsBool);
        Assert.True(fob.BoolValue);
    }

    [Fact]
    public void ImplicitConversion_FromFalse_Works()
    {
        FilterOrBool fob = false;

        Assert.True(fob.IsBool);
        Assert.False(fob.BoolValue);
    }

    [Fact]
    public void ImplicitConversion_FromCondition_Works()
    {
        var condition = new Condition(() => true);

        FilterOrBool fob = condition;

        Assert.True(fob.IsFilter);
        Assert.Same(condition, fob.FilterValue);
    }

    [Fact]
    public void ImplicitConversion_FromAlways_Works()
    {
        FilterOrBool fob = Always.Instance;

        Assert.True(fob.IsFilter);
        Assert.Same(Always.Instance, fob.FilterValue);
    }

    [Fact]
    public void ImplicitConversion_FromNever_Works()
    {
        FilterOrBool fob = Never.Instance;

        Assert.True(fob.IsFilter);
        Assert.Same(Never.Instance, fob.FilterValue);
    }

    #endregion

    #region Equality Tests

    [Fact]
    public void Equals_BoolTrue_WithBoolTrue_ReturnsTrue()
    {
        var fob1 = new FilterOrBool(true);
        var fob2 = new FilterOrBool(true);

        Assert.True(fob1.Equals(fob2));
        Assert.True(fob1 == fob2);
        Assert.False(fob1 != fob2);
    }

    [Fact]
    public void Equals_BoolFalse_WithBoolFalse_ReturnsTrue()
    {
        var fob1 = new FilterOrBool(false);
        var fob2 = new FilterOrBool(false);

        Assert.True(fob1.Equals(fob2));
    }

    [Fact]
    public void Equals_BoolTrue_WithBoolFalse_ReturnsFalse()
    {
        var fob1 = new FilterOrBool(true);
        var fob2 = new FilterOrBool(false);

        Assert.False(fob1.Equals(fob2));
        Assert.False(fob1 == fob2);
        Assert.True(fob1 != fob2);
    }

    [Fact]
    public void Equals_SameFilter_ReturnsTrue()
    {
        var condition = new Condition(() => true);
        var fob1 = new FilterOrBool(condition);
        var fob2 = new FilterOrBool(condition);

        Assert.True(fob1.Equals(fob2));
    }

    [Fact]
    public void Equals_DifferentFilters_ReturnsFalse()
    {
        var condition1 = new Condition(() => true);
        var condition2 = new Condition(() => true);
        var fob1 = new FilterOrBool(condition1);
        var fob2 = new FilterOrBool(condition2);

        // Different instances, even with same logic
        Assert.False(fob1.Equals(fob2));
    }

    [Fact]
    public void Equals_Bool_WithFilter_ReturnsFalse()
    {
        var fob1 = new FilterOrBool(true);
        var fob2 = new FilterOrBool(Always.Instance);

        Assert.False(fob1.Equals(fob2));
    }

    [Fact]
    public void Equals_WithObject_HandlesTypeCorrectly()
    {
        var fob = new FilterOrBool(true);

        Assert.True(fob.Equals((object)new FilterOrBool(true)));
        Assert.False(fob.Equals((object)new FilterOrBool(false)));
        Assert.False(fob.Equals((object?)"not a FilterOrBool"));
        Assert.False(fob.Equals((object?)null));
    }

    #endregion

    #region GetHashCode Tests

    [Fact]
    public void GetHashCode_SameBoolValues_ReturnsSameHash()
    {
        var fob1 = new FilterOrBool(true);
        var fob2 = new FilterOrBool(true);

        Assert.Equal(fob1.GetHashCode(), fob2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_SameFilter_ReturnsSameHash()
    {
        var condition = new Condition(() => true);
        var fob1 = new FilterOrBool(condition);
        var fob2 = new FilterOrBool(condition);

        Assert.Equal(fob1.GetHashCode(), fob2.GetHashCode());
    }

    #endregion

    #region ToString Tests

    [Fact]
    public void ToString_WithTrue_ReturnsTrue()
    {
        var fob = new FilterOrBool(true);

        Assert.Equal("true", fob.ToString());
    }

    [Fact]
    public void ToString_WithFalse_ReturnsFalse()
    {
        var fob = new FilterOrBool(false);

        Assert.Equal("false", fob.ToString());
    }

    [Fact]
    public void ToString_WithCondition_ReturnsConditionString()
    {
        var condition = new Condition(() => true);
        var fob = new FilterOrBool(condition);

        Assert.Equal("Condition", fob.ToString());
    }

    [Fact]
    public void ToString_WithAlways_ReturnsAlways()
    {
        var fob = new FilterOrBool(Always.Instance);

        Assert.Equal("Always", fob.ToString());
    }

    [Fact]
    public void ToString_WithNever_ReturnsNever()
    {
        var fob = new FilterOrBool(Never.Instance);

        Assert.Equal("Never", fob.ToString());
    }

    #endregion

    #region Default Value Tests

    [Fact]
    public void Default_IsBoolFalse()
    {
        var fob = default(FilterOrBool);

        Assert.True(fob.IsBool);
        Assert.False(fob.IsFilter);
        Assert.False(fob.BoolValue);
    }

    #endregion

    #region Method Parameter Usage Tests

    [Fact]
    public void CanBeUsedAsMethodParameter_WithDefaultTrue()
    {
        var result = MethodWithFilterOrBool(true);
        Assert.True(result);
    }

    [Fact]
    public void CanBeUsedAsMethodParameter_WithDefaultFalse()
    {
        var result = MethodWithFilterOrBool(false);
        Assert.False(result);
    }

    [Fact]
    public void CanBeUsedAsMethodParameter_WithCondition()
    {
        var condition = new Condition(() => true);
        var result = MethodWithFilterOrBool(condition);
        Assert.True(result);
    }

    private static bool MethodWithFilterOrBool(FilterOrBool condition)
    {
        return FilterUtils.IsTrue(condition);
    }

    #endregion
}
