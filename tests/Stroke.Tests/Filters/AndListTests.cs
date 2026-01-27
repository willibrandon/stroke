using Stroke.Filters;
using Xunit;

namespace Stroke.Tests.Filters;

/// <summary>
/// Tests for the <see cref="AndList"/> internal class.
/// </summary>
public sealed class AndListTests
{
    #region Create Factory Tests

    [Fact]
    public void Create_WithSingleFilter_ReturnsThatFilter()
    {
        var condition = new Condition(() => true);

        var result = AndList.Create([condition]);

        Assert.Same(condition, result);
    }

    [Fact]
    public void Create_WithMultipleFilters_ReturnsAndList()
    {
        var condition1 = new Condition(() => true);
        var condition2 = new Condition(() => false);

        var result = AndList.Create([condition1, condition2]);

        Assert.IsType<AndList>(result);
    }

    [Fact]
    public void Create_WithEmptyAfterDedup_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => AndList.Create([]));
    }

    #endregion

    #region Flattening Tests

    [Fact]
    public void Create_WithNestedAndList_FlattensIntoSingleList()
    {
        var condition1 = new Condition(() => true);
        var condition2 = new Condition(() => true);
        var condition3 = new Condition(() => true);

        // Create (condition1 & condition2)
        var nested = (AndList)AndList.Create([condition1, condition2]);

        // Create (nested & condition3) which should flatten to [condition1, condition2, condition3]
        var result = (AndList)AndList.Create([nested, condition3]);

        Assert.Equal(3, result.Filters.Count);
        Assert.Same(condition1, result.Filters[0]);
        Assert.Same(condition2, result.Filters[1]);
        Assert.Same(condition3, result.Filters[2]);
    }

    [Fact]
    public void Create_WithDeeplyNestedAndList_FlattensCompletely()
    {
        var condition1 = new Condition(() => true);
        var condition2 = new Condition(() => true);
        var condition3 = new Condition(() => true);
        var condition4 = new Condition(() => true);

        // Create (condition1 & condition2)
        var nested1 = (AndList)AndList.Create([condition1, condition2]);

        // Create (condition3 & condition4)
        var nested2 = (AndList)AndList.Create([condition3, condition4]);

        // Create (nested1 & nested2) which should flatten to all four
        var result = (AndList)AndList.Create([nested1, nested2]);

        Assert.Equal(4, result.Filters.Count);
    }

    #endregion

    #region Deduplication Tests

    [Fact]
    public void Create_WithDuplicateFilters_RemovesDuplicates()
    {
        var condition = new Condition(() => true);

        // condition & condition should deduplicate to just condition
        var result = AndList.Create([condition, condition]);

        Assert.Same(condition, result);
    }

    [Fact]
    public void Create_WithDuplicatesPreservesOrder()
    {
        var condition1 = new Condition(() => true);
        var condition2 = new Condition(() => true);

        // condition1 & condition2 & condition1 -> [condition1, condition2]
        var result = (AndList)AndList.Create([condition1, condition2, condition1]);

        Assert.Equal(2, result.Filters.Count);
        Assert.Same(condition1, result.Filters[0]);
        Assert.Same(condition2, result.Filters[1]);
    }

    [Fact]
    public void Create_UsesReferenceEquality_NotValueEquality()
    {
        // Two different Condition instances that both return true
        // Should NOT be deduplicated because they're different instances
        var condition1 = new Condition(() => true);
        var condition2 = new Condition(() => true);

        var result = (AndList)AndList.Create([condition1, condition2]);

        Assert.Equal(2, result.Filters.Count);
    }

    #endregion

    #region Invoke Tests

    [Fact]
    public void Invoke_AllTrue_ReturnsTrue()
    {
        var condition1 = new Condition(() => true);
        var condition2 = new Condition(() => true);
        var condition3 = new Condition(() => true);

        var andList = (AndList)AndList.Create([condition1, condition2, condition3]);

        Assert.True(andList.Invoke());
    }

    [Fact]
    public void Invoke_OneFalse_ReturnsFalse()
    {
        var condition1 = new Condition(() => true);
        var condition2 = new Condition(() => false);
        var condition3 = new Condition(() => true);

        var andList = (AndList)AndList.Create([condition1, condition2, condition3]);

        Assert.False(andList.Invoke());
    }

    [Fact]
    public void Invoke_AllFalse_ReturnsFalse()
    {
        var condition1 = new Condition(() => false);
        var condition2 = new Condition(() => false);

        var andList = (AndList)AndList.Create([condition1, condition2]);

        Assert.False(andList.Invoke());
    }

    #endregion

    #region Short-Circuit Evaluation Tests

    [Fact]
    public void Invoke_ShortCircuitsOnFirstFalse()
    {
        var callCount = 0;

        var falseCondition = new Condition(() =>
        {
            callCount++;
            return false;
        });

        var throwingCondition = new Condition(() =>
        {
            callCount++;
            throw new InvalidOperationException("Should not be called");
        });

        var andList = (AndList)AndList.Create([falseCondition, throwingCondition]);

        var result = andList.Invoke();

        Assert.False(result);
        Assert.Equal(1, callCount); // Only first was called
    }

    [Fact]
    public void Invoke_EvaluatesLeftToRight()
    {
        var callOrder = new List<int>();

        var condition1 = new Condition(() => { callOrder.Add(1); return true; });
        var condition2 = new Condition(() => { callOrder.Add(2); return true; });
        var condition3 = new Condition(() => { callOrder.Add(3); return true; });

        var andList = (AndList)AndList.Create([condition1, condition2, condition3]);

        andList.Invoke();

        Assert.Equal([1, 2, 3], callOrder);
    }

    [Fact]
    public void Invoke_PropagatesExceptions()
    {
        var condition1 = new Condition(() => true);
        var throwingCondition = new Condition(() => throw new InvalidOperationException("Test exception"));

        var andList = (AndList)AndList.Create([condition1, throwingCondition]);

        var ex = Assert.Throws<InvalidOperationException>(() => andList.Invoke());
        Assert.Equal("Test exception", ex.Message);
    }

    #endregion

    #region ToString Tests

    [Fact]
    public void ToString_JoinsFiltersWithAmpersand()
    {
        var condition1 = new Condition(() => true);
        var condition2 = new Condition(() => true);

        var andList = (AndList)AndList.Create([condition1, condition2]);

        Assert.Equal("Condition&Condition", andList.ToString());
    }

    [Fact]
    public void ToString_WithMixedTypes_ShowsAllTypes()
    {
        var condition = new Condition(() => true);

        // Create an AndList with Always through the public API
        var combined = condition.And(Always.Instance);

        // Since Always.And returns the other filter, we need to use Filter.And
        // which creates an AndList
        var andList = (AndList)AndList.Create([condition, Never.Instance]);

        Assert.Equal("Condition&Never", andList.ToString());
    }

    #endregion

    #region Filters Property Tests

    [Fact]
    public void Filters_ReturnsAllContainedFilters()
    {
        var condition1 = new Condition(() => true);
        var condition2 = new Condition(() => true);
        var condition3 = new Condition(() => true);

        var andList = (AndList)AndList.Create([condition1, condition2, condition3]);

        Assert.Equal(3, andList.Filters.Count);
        Assert.Contains(condition1, andList.Filters);
        Assert.Contains(condition2, andList.Filters);
        Assert.Contains(condition3, andList.Filters);
    }

    [Fact]
    public void Filters_PreservesOrder()
    {
        var condition1 = new Condition(() => true);
        var condition2 = new Condition(() => true);
        var condition3 = new Condition(() => true);

        var andList = (AndList)AndList.Create([condition1, condition2, condition3]);

        Assert.Same(condition1, andList.Filters[0]);
        Assert.Same(condition2, andList.Filters[1]);
        Assert.Same(condition3, andList.Filters[2]);
    }

    #endregion
}
