using Stroke.Filters;
using Xunit;

namespace Stroke.Tests.Filters;

/// <summary>
/// Tests for the <see cref="Condition"/> filter class.
/// </summary>
public sealed class ConditionTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullFunc_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new Condition(null!));
    }

    [Fact]
    public void Constructor_WithValidFunc_DoesNotThrow()
    {
        var condition = new Condition(() => true);

        Assert.NotNull(condition);
    }

    #endregion

    #region Invoke Tests

    [Fact]
    public void Invoke_WhenFuncReturnsTrue_ReturnsTrue()
    {
        var condition = new Condition(() => true);

        Assert.True(condition.Invoke());
    }

    [Fact]
    public void Invoke_WhenFuncReturnsFalse_ReturnsFalse()
    {
        var condition = new Condition(() => false);

        Assert.False(condition.Invoke());
    }

    [Fact]
    public void Invoke_WhenStateChanges_ReturnsNewValue()
    {
        var state = false;
        var condition = new Condition(() => state);

        Assert.False(condition.Invoke());

        state = true;
        Assert.True(condition.Invoke());

        state = false;
        Assert.False(condition.Invoke());
    }

    [Fact]
    public void Invoke_WhenFuncThrows_PropagatesException()
    {
        var condition = new Condition(() => throw new InvalidOperationException("Test exception"));

        var ex = Assert.Throws<InvalidOperationException>(() => condition.Invoke());
        Assert.Equal("Test exception", ex.Message);
    }

    [Fact]
    public void Invoke_CalledMultipleTimes_EvaluatesEachTime()
    {
        var callCount = 0;
        var condition = new Condition(() =>
        {
            callCount++;
            return true;
        });

        condition.Invoke();
        condition.Invoke();
        condition.Invoke();

        Assert.Equal(3, callCount);
    }

    #endregion

    #region ToString Tests

    [Fact]
    public void ToString_ReturnsCondition()
    {
        var condition = new Condition(() => true);

        Assert.Equal("Condition", condition.ToString());
    }

    #endregion

    #region Combination Tests

    [Fact]
    public void And_WithAnotherCondition_ReturnsCombinedFilter()
    {
        var condition1 = new Condition(() => true);
        var condition2 = new Condition(() => true);

        var result = condition1.And(condition2);

        Assert.NotSame(condition1, result);
        Assert.NotSame(condition2, result);
        Assert.True(result.Invoke());
    }

    [Fact]
    public void Or_WithAnotherCondition_ReturnsCombinedFilter()
    {
        var condition1 = new Condition(() => false);
        var condition2 = new Condition(() => true);

        var result = condition1.Or(condition2);

        Assert.NotSame(condition1, result);
        Assert.NotSame(condition2, result);
        Assert.True(result.Invoke());
    }

    [Fact]
    public void Invert_ReturnsInvertedResult()
    {
        var condition = new Condition(() => true);

        var inverted = condition.Invert();

        Assert.False(inverted.Invoke());
    }

    #endregion

    #region Complex State Tests

    [Fact]
    public void Invoke_WithComplexStateObject_EvaluatesCorrectly()
    {
        var state = new TestState { IsActive = true, IsReadOnly = false };
        var canEdit = new Condition(() => state.IsActive && !state.IsReadOnly);

        Assert.True(canEdit.Invoke());

        state.IsReadOnly = true;
        Assert.False(canEdit.Invoke());

        state.IsReadOnly = false;
        state.IsActive = false;
        Assert.False(canEdit.Invoke());
    }

    private sealed class TestState
    {
        public bool IsActive { get; set; }
        public bool IsReadOnly { get; set; }
    }

    #endregion
}
