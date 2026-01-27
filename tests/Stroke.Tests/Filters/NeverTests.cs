using Stroke.Filters;
using Xunit;

namespace Stroke.Tests.Filters;

/// <summary>
/// Tests for the <see cref="Never"/> singleton filter class.
/// </summary>
public sealed class NeverTests
{
    #region Singleton Tests

    [Fact]
    public void Instance_ReturnsSameInstance()
    {
        var instance1 = Never.Instance;
        var instance2 = Never.Instance;

        Assert.Same(instance1, instance2);
    }

    [Fact]
    public void Instance_IsNotNull()
    {
        Assert.NotNull(Never.Instance);
    }

    #endregion

    #region Invoke Tests

    [Fact]
    public void Invoke_ReturnsFalse()
    {
        Assert.False(Never.Instance.Invoke());
    }

    [Fact]
    public void Invoke_CalledMultipleTimes_AlwaysReturnsFalse()
    {
        for (int i = 0; i < 100; i++)
        {
            Assert.False(Never.Instance.Invoke());
        }
    }

    #endregion

    #region ToString Tests

    [Fact]
    public void ToString_ReturnsNever()
    {
        Assert.Equal("Never", Never.Instance.ToString());
    }

    #endregion

    #region Algebraic Property Tests (And)

    [Fact]
    public void And_WithNullOther_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => Never.Instance.And(null!));
    }

    [Fact]
    public void And_WithCondition_ReturnsNever()
    {
        var condition = new Condition(() => true);

        var result = Never.Instance.And(condition);

        Assert.Same(Never.Instance, result);
    }

    [Fact]
    public void And_WithAlways_ReturnsNever()
    {
        var result = Never.Instance.And(Always.Instance);

        Assert.Same(Never.Instance, result);
    }

    [Fact]
    public void And_WithNever_ReturnsNever()
    {
        var result = Never.Instance.And(Never.Instance);

        Assert.Same(Never.Instance, result);
    }

    #endregion

    #region Algebraic Property Tests (Or)

    [Fact]
    public void Or_WithNullOther_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => Never.Instance.Or(null!));
    }

    [Fact]
    public void Or_WithCondition_ReturnsCondition()
    {
        var condition = new Condition(() => true);

        var result = Never.Instance.Or(condition);

        Assert.Same(condition, result);
    }

    [Fact]
    public void Or_WithAlways_ReturnsAlways()
    {
        var result = Never.Instance.Or(Always.Instance);

        Assert.Same(Always.Instance, result);
    }

    [Fact]
    public void Or_WithNever_ReturnsNever()
    {
        var result = Never.Instance.Or(Never.Instance);

        Assert.Same(Never.Instance, result);
    }

    #endregion

    #region Algebraic Property Tests (Invert)

    [Fact]
    public void Invert_ReturnsAlways()
    {
        var result = Never.Instance.Invert();

        Assert.Same(Always.Instance, result);
    }

    [Fact]
    public void Invert_CalledMultipleTimes_ReturnsSameAlwaysInstance()
    {
        var result1 = Never.Instance.Invert();
        var result2 = Never.Instance.Invert();

        Assert.Same(result1, result2);
        Assert.Same(Always.Instance, result1);
    }

    #endregion

    #region Operator Tests

    [Fact]
    public void AndOperator_WithCondition_ReturnsNever()
    {
        var condition = new Condition(() => true);

        var result = Never.Instance & condition;

        Assert.Same(Never.Instance, result);
    }

    [Fact]
    public void OrOperator_WithCondition_ReturnsCondition()
    {
        var condition = new Condition(() => true);

        var result = Never.Instance | condition;

        Assert.Same(condition, result);
    }

    [Fact]
    public void NotOperator_ReturnsAlways()
    {
        var result = ~Never.Instance;

        Assert.Same(Always.Instance, result);
    }

    #endregion
}
