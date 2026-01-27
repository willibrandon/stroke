using Stroke.Filters;
using Xunit;

namespace Stroke.Tests.Filters;

/// <summary>
/// Tests for the <see cref="Always"/> singleton filter class.
/// </summary>
public sealed class AlwaysTests
{
    #region Singleton Tests

    [Fact]
    public void Instance_ReturnsSameInstance()
    {
        var instance1 = Always.Instance;
        var instance2 = Always.Instance;

        Assert.Same(instance1, instance2);
    }

    [Fact]
    public void Instance_IsNotNull()
    {
        Assert.NotNull(Always.Instance);
    }

    #endregion

    #region Invoke Tests

    [Fact]
    public void Invoke_ReturnsTrue()
    {
        Assert.True(Always.Instance.Invoke());
    }

    [Fact]
    public void Invoke_CalledMultipleTimes_AlwaysReturnsTrue()
    {
        for (int i = 0; i < 100; i++)
        {
            Assert.True(Always.Instance.Invoke());
        }
    }

    #endregion

    #region ToString Tests

    [Fact]
    public void ToString_ReturnsAlways()
    {
        Assert.Equal("Always", Always.Instance.ToString());
    }

    #endregion

    #region Algebraic Property Tests (And)

    [Fact]
    public void And_WithNullOther_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => Always.Instance.And(null!));
    }

    [Fact]
    public void And_WithCondition_ReturnsCondition()
    {
        var condition = new Condition(() => true);

        var result = Always.Instance.And(condition);

        Assert.Same(condition, result);
    }

    [Fact]
    public void And_WithNever_ReturnsNever()
    {
        var result = Always.Instance.And(Never.Instance);

        Assert.Same(Never.Instance, result);
    }

    [Fact]
    public void And_WithAlways_ReturnsAlways()
    {
        var result = Always.Instance.And(Always.Instance);

        Assert.Same(Always.Instance, result);
    }

    #endregion

    #region Algebraic Property Tests (Or)

    [Fact]
    public void Or_WithNullOther_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => Always.Instance.Or(null!));
    }

    [Fact]
    public void Or_WithCondition_ReturnsAlways()
    {
        var condition = new Condition(() => false);

        var result = Always.Instance.Or(condition);

        Assert.Same(Always.Instance, result);
    }

    [Fact]
    public void Or_WithNever_ReturnsAlways()
    {
        var result = Always.Instance.Or(Never.Instance);

        Assert.Same(Always.Instance, result);
    }

    [Fact]
    public void Or_WithAlways_ReturnsAlways()
    {
        var result = Always.Instance.Or(Always.Instance);

        Assert.Same(Always.Instance, result);
    }

    #endregion

    #region Algebraic Property Tests (Invert)

    [Fact]
    public void Invert_ReturnsNever()
    {
        var result = Always.Instance.Invert();

        Assert.Same(Never.Instance, result);
    }

    [Fact]
    public void Invert_CalledMultipleTimes_ReturnsSameNeverInstance()
    {
        var result1 = Always.Instance.Invert();
        var result2 = Always.Instance.Invert();

        Assert.Same(result1, result2);
        Assert.Same(Never.Instance, result1);
    }

    #endregion

    #region Operator Tests

    [Fact]
    public void AndOperator_WithCondition_ReturnsCondition()
    {
        var condition = new Condition(() => true);

        var result = Always.Instance & condition;

        Assert.Same(condition, result);
    }

    [Fact]
    public void OrOperator_WithCondition_ReturnsAlways()
    {
        var condition = new Condition(() => false);

        var result = Always.Instance | condition;

        Assert.Same(Always.Instance, result);
    }

    [Fact]
    public void NotOperator_ReturnsNever()
    {
        var result = ~Always.Instance;

        Assert.Same(Never.Instance, result);
    }

    #endregion
}
