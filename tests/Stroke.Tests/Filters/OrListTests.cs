using Stroke.Filters;
using Xunit;

namespace Stroke.Tests.Filters;

/// <summary>
/// Tests for the <see cref="OrList"/> internal class.
/// </summary>
public sealed class OrListTests
{
    #region Create Factory Tests

    [Fact]
    public void Create_WithSingleFilter_ReturnsThatFilter()
    {
        var condition = new Condition(() => true);

        var result = OrList.Create([condition]);

        Assert.Same(condition, result);
    }

    [Fact]
    public void Create_WithMultipleFilters_ReturnsOrList()
    {
        var condition1 = new Condition(() => true);
        var condition2 = new Condition(() => false);

        var result = OrList.Create([condition1, condition2]);

        Assert.IsType<OrList>(result);
    }

    [Fact]
    public void Create_WithEmptyAfterDedup_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => OrList.Create([]));
    }

    #endregion

    #region Flattening Tests

    [Fact]
    public void Create_WithNestedOrList_FlattensIntoSingleList()
    {
        var condition1 = new Condition(() => false);
        var condition2 = new Condition(() => false);
        var condition3 = new Condition(() => true);

        // Create (condition1 | condition2)
        var nested = (OrList)OrList.Create([condition1, condition2]);

        // Create (nested | condition3) which should flatten to [condition1, condition2, condition3]
        var result = (OrList)OrList.Create([nested, condition3]);

        Assert.Equal(3, result.Filters.Count);
        Assert.Same(condition1, result.Filters[0]);
        Assert.Same(condition2, result.Filters[1]);
        Assert.Same(condition3, result.Filters[2]);
    }

    [Fact]
    public void Create_WithDeeplyNestedOrList_FlattensCompletely()
    {
        var condition1 = new Condition(() => false);
        var condition2 = new Condition(() => false);
        var condition3 = new Condition(() => false);
        var condition4 = new Condition(() => true);

        // Create (condition1 | condition2)
        var nested1 = (OrList)OrList.Create([condition1, condition2]);

        // Create (condition3 | condition4)
        var nested2 = (OrList)OrList.Create([condition3, condition4]);

        // Create (nested1 | nested2) which should flatten to all four
        var result = (OrList)OrList.Create([nested1, nested2]);

        Assert.Equal(4, result.Filters.Count);
    }

    #endregion

    #region Deduplication Tests

    [Fact]
    public void Create_WithDuplicateFilters_RemovesDuplicates()
    {
        var condition = new Condition(() => true);

        // condition | condition should deduplicate to just condition
        var result = OrList.Create([condition, condition]);

        Assert.Same(condition, result);
    }

    [Fact]
    public void Create_WithDuplicatesPreservesOrder()
    {
        var condition1 = new Condition(() => true);
        var condition2 = new Condition(() => true);

        // condition1 | condition2 | condition1 -> [condition1, condition2]
        var result = (OrList)OrList.Create([condition1, condition2, condition1]);

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

        var result = (OrList)OrList.Create([condition1, condition2]);

        Assert.Equal(2, result.Filters.Count);
    }

    #endregion

    #region Invoke Tests

    [Fact]
    public void Invoke_AllFalse_ReturnsFalse()
    {
        var condition1 = new Condition(() => false);
        var condition2 = new Condition(() => false);
        var condition3 = new Condition(() => false);

        var orList = (OrList)OrList.Create([condition1, condition2, condition3]);

        Assert.False(orList.Invoke());
    }

    [Fact]
    public void Invoke_OneTrue_ReturnsTrue()
    {
        var condition1 = new Condition(() => false);
        var condition2 = new Condition(() => true);
        var condition3 = new Condition(() => false);

        var orList = (OrList)OrList.Create([condition1, condition2, condition3]);

        Assert.True(orList.Invoke());
    }

    [Fact]
    public void Invoke_AllTrue_ReturnsTrue()
    {
        var condition1 = new Condition(() => true);
        var condition2 = new Condition(() => true);

        var orList = (OrList)OrList.Create([condition1, condition2]);

        Assert.True(orList.Invoke());
    }

    #endregion

    #region Short-Circuit Evaluation Tests

    [Fact]
    public void Invoke_ShortCircuitsOnFirstTrue()
    {
        var callCount = 0;

        var trueCondition = new Condition(() =>
        {
            callCount++;
            return true;
        });

        var throwingCondition = new Condition(() =>
        {
            callCount++;
            throw new InvalidOperationException("Should not be called");
        });

        var orList = (OrList)OrList.Create([trueCondition, throwingCondition]);

        var result = orList.Invoke();

        Assert.True(result);
        Assert.Equal(1, callCount); // Only first was called
    }

    [Fact]
    public void Invoke_EvaluatesLeftToRight()
    {
        var callOrder = new List<int>();

        var condition1 = new Condition(() => { callOrder.Add(1); return false; });
        var condition2 = new Condition(() => { callOrder.Add(2); return false; });
        var condition3 = new Condition(() => { callOrder.Add(3); return false; });

        var orList = (OrList)OrList.Create([condition1, condition2, condition3]);

        orList.Invoke();

        Assert.Equal([1, 2, 3], callOrder);
    }

    [Fact]
    public void Invoke_PropagatesExceptions()
    {
        var condition1 = new Condition(() => false);
        var throwingCondition = new Condition(() => throw new InvalidOperationException("Test exception"));

        var orList = (OrList)OrList.Create([condition1, throwingCondition]);

        var ex = Assert.Throws<InvalidOperationException>(() => orList.Invoke());
        Assert.Equal("Test exception", ex.Message);
    }

    #endregion

    #region ToString Tests

    [Fact]
    public void ToString_JoinsFiltersWithPipe()
    {
        var condition1 = new Condition(() => false);
        var condition2 = new Condition(() => true);

        var orList = (OrList)OrList.Create([condition1, condition2]);

        Assert.Equal("Condition|Condition", orList.ToString());
    }

    [Fact]
    public void ToString_WithMixedTypes_ShowsAllTypes()
    {
        var condition = new Condition(() => false);

        var orList = (OrList)OrList.Create([condition, Always.Instance]);

        Assert.Equal("Condition|Always", orList.ToString());
    }

    #endregion

    #region Filters Property Tests

    [Fact]
    public void Filters_ReturnsAllContainedFilters()
    {
        var condition1 = new Condition(() => false);
        var condition2 = new Condition(() => false);
        var condition3 = new Condition(() => true);

        var orList = (OrList)OrList.Create([condition1, condition2, condition3]);

        Assert.Equal(3, orList.Filters.Count);
        Assert.Contains(condition1, orList.Filters);
        Assert.Contains(condition2, orList.Filters);
        Assert.Contains(condition3, orList.Filters);
    }

    [Fact]
    public void Filters_PreservesOrder()
    {
        var condition1 = new Condition(() => false);
        var condition2 = new Condition(() => false);
        var condition3 = new Condition(() => true);

        var orList = (OrList)OrList.Create([condition1, condition2, condition3]);

        Assert.Same(condition1, orList.Filters[0]);
        Assert.Same(condition2, orList.Filters[1]);
        Assert.Same(condition3, orList.Filters[2]);
    }

    #endregion
}
