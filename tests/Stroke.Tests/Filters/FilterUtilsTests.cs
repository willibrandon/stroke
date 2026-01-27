using Stroke.Filters;
using Xunit;

namespace Stroke.Tests.Filters;

/// <summary>
/// Tests for the <see cref="FilterUtils"/> static class.
/// </summary>
public sealed class FilterUtilsTests
{
    #region ToFilter Tests

    [Fact]
    public void ToFilter_WithTrue_ReturnsAlways()
    {
        var result = FilterUtils.ToFilter(true);

        Assert.Same(Always.Instance, result);
    }

    [Fact]
    public void ToFilter_WithFalse_ReturnsNever()
    {
        var result = FilterUtils.ToFilter(false);

        Assert.Same(Never.Instance, result);
    }

    [Fact]
    public void ToFilter_WithCondition_ReturnsSameInstance()
    {
        var condition = new Condition(() => true);

        var result = FilterUtils.ToFilter(condition);

        Assert.Same(condition, result);
    }

    [Fact]
    public void ToFilter_WithAlways_ReturnsAlways()
    {
        var result = FilterUtils.ToFilter(Always.Instance);

        Assert.Same(Always.Instance, result);
    }

    [Fact]
    public void ToFilter_WithNever_ReturnsNever()
    {
        var result = FilterUtils.ToFilter(Never.Instance);

        Assert.Same(Never.Instance, result);
    }

    [Fact]
    public void ToFilter_WithInvertedFilter_ReturnsSameInstance()
    {
        var condition = new Condition(() => true);
        var inverted = condition.Invert();

        var result = FilterUtils.ToFilter(new FilterOrBool(inverted));

        Assert.Same(inverted, result);
    }

    [Fact]
    public void ToFilter_DoesNotInvokeFilter()
    {
        var callCount = 0;
        var condition = new Condition(() =>
        {
            callCount++;
            return true;
        });

        FilterUtils.ToFilter(condition);

        Assert.Equal(0, callCount);
    }

    #endregion

    #region IsTrue Tests

    [Fact]
    public void IsTrue_WithTrue_ReturnsTrue()
    {
        var result = FilterUtils.IsTrue(true);

        Assert.True(result);
    }

    [Fact]
    public void IsTrue_WithFalse_ReturnsFalse()
    {
        var result = FilterUtils.IsTrue(false);

        Assert.False(result);
    }

    [Fact]
    public void IsTrue_WithAlways_ReturnsTrue()
    {
        var result = FilterUtils.IsTrue(Always.Instance);

        Assert.True(result);
    }

    [Fact]
    public void IsTrue_WithNever_ReturnsFalse()
    {
        var result = FilterUtils.IsTrue(Never.Instance);

        Assert.False(result);
    }

    [Fact]
    public void IsTrue_WithConditionReturningTrue_ReturnsTrue()
    {
        var condition = new Condition(() => true);

        var result = FilterUtils.IsTrue(condition);

        Assert.True(result);
    }

    [Fact]
    public void IsTrue_WithConditionReturningFalse_ReturnsFalse()
    {
        var condition = new Condition(() => false);

        var result = FilterUtils.IsTrue(condition);

        Assert.False(result);
    }

    [Fact]
    public void IsTrue_InvokesFilter()
    {
        var callCount = 0;
        var condition = new Condition(() =>
        {
            callCount++;
            return true;
        });

        FilterUtils.IsTrue(condition);

        Assert.Equal(1, callCount);
    }

    [Fact]
    public void IsTrue_PropagatesException()
    {
        var condition = new Condition(() => throw new InvalidOperationException("Test exception"));

        var ex = Assert.Throws<InvalidOperationException>(() => FilterUtils.IsTrue(condition));
        Assert.Equal("Test exception", ex.Message);
    }

    [Fact]
    public void IsTrue_WithInvertedCondition_ReturnsCorrectValue()
    {
        var condition = new Condition(() => true);
        var inverted = condition.Invert();

        var result = FilterUtils.IsTrue(new FilterOrBool(inverted));

        Assert.False(result);
    }

    #endregion

    #region Equivalence Tests

    [Fact]
    public void IsTrue_EquivalentToToFilterThenInvoke_WithTrue()
    {
        FilterOrBool value = true;

        var toFilterResult = FilterUtils.ToFilter(value).Invoke();
        var isTrueResult = FilterUtils.IsTrue(value);

        Assert.Equal(toFilterResult, isTrueResult);
    }

    [Fact]
    public void IsTrue_EquivalentToToFilterThenInvoke_WithFalse()
    {
        FilterOrBool value = false;

        var toFilterResult = FilterUtils.ToFilter(value).Invoke();
        var isTrueResult = FilterUtils.IsTrue(value);

        Assert.Equal(toFilterResult, isTrueResult);
    }

    [Fact]
    public void IsTrue_EquivalentToToFilterThenInvoke_WithCondition()
    {
        var condition = new Condition(() => true);
        FilterOrBool value = condition;

        var toFilterResult = FilterUtils.ToFilter(value).Invoke();
        var isTrueResult = FilterUtils.IsTrue(value);

        Assert.Equal(toFilterResult, isTrueResult);
    }

    #endregion

    #region API Usage Pattern Tests

    [Fact]
    public void TypicalUsage_AcceptBoolOrFilter()
    {
        // Simulates typical API usage where a method accepts FilterOrBool

        // Static enable
        Assert.True(SimulateFeatureCheck(true));

        // Static disable
        Assert.False(SimulateFeatureCheck(false));

        // Dynamic based on condition
        var isEnabled = true;
        Assert.True(SimulateFeatureCheck(new Condition(() => isEnabled)));

        isEnabled = false;
        Assert.False(SimulateFeatureCheck(new Condition(() => isEnabled)));
    }

    private static bool SimulateFeatureCheck(FilterOrBool condition)
    {
        return FilterUtils.IsTrue(condition);
    }

    [Fact]
    public void TypicalUsage_StoreFilterForLaterEvaluation()
    {
        // Simulates storing a filter for later evaluation

        var isActive = false;
        var filter = FilterUtils.ToFilter(new Condition(() => isActive));

        Assert.False(filter.Invoke());

        isActive = true;
        Assert.True(filter.Invoke());
    }

    #endregion

    #region Default FilterOrBool Tests

    [Fact]
    public void ToFilter_WithDefaultFilterOrBool_ReturnsNever()
    {
        var defaultValue = default(FilterOrBool);

        var result = FilterUtils.ToFilter(defaultValue);

        Assert.Same(Never.Instance, result);
    }

    [Fact]
    public void IsTrue_WithDefaultFilterOrBool_ReturnsFalse()
    {
        var defaultValue = default(FilterOrBool);

        var result = FilterUtils.IsTrue(defaultValue);

        Assert.False(result);
    }

    #endregion

    #region Combined Filter Tests

    [Fact]
    public void ToFilter_WithAndCombination_ReturnsAndList()
    {
        var condition1 = new Condition(() => true);
        var condition2 = new Condition(() => false);
        var combined = condition1.And(condition2);

        var result = FilterUtils.ToFilter(new FilterOrBool(combined));

        Assert.Same(combined, result);
    }

    [Fact]
    public void ToFilter_WithOrCombination_ReturnsOrList()
    {
        var condition1 = new Condition(() => false);
        var condition2 = new Condition(() => true);
        var combined = condition1.Or(condition2);

        var result = FilterUtils.ToFilter(new FilterOrBool(combined));

        Assert.Same(combined, result);
    }

    #endregion
}
