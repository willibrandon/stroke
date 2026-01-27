using Stroke.Filters;
using Xunit;

namespace Stroke.Tests.Filters;

/// <summary>
/// Tests for the caching behavior of <see cref="Filter.And"/>, <see cref="Filter.Or"/>,
/// and <see cref="Filter.Invert"/> methods.
/// </summary>
public sealed class FilterCachingTests
{
    #region And Caching Tests

    [Fact]
    public void And_SameFilterTwice_ReturnsCachedInstance()
    {
        var filter1 = new Condition(() => true);
        var filter2 = new Condition(() => false);

        var result1 = filter1.And(filter2);
        var result2 = filter1.And(filter2);

        Assert.Same(result1, result2);
    }

    [Fact]
    public void And_DifferentSecondFilters_ReturnsDifferentInstances()
    {
        var filter1 = new Condition(() => true);
        var filter2 = new Condition(() => false);
        var filter3 = new Condition(() => true);

        var result1 = filter1.And(filter2);
        var result2 = filter1.And(filter3);

        Assert.NotSame(result1, result2);
    }

    [Fact]
    public void And_CachingIsPerInstance()
    {
        var filter1 = new Condition(() => true);
        var filter2 = new Condition(() => true);
        var filterOther = new Condition(() => false);

        // filter1 & filterOther
        var result1 = filter1.And(filterOther);

        // filter2 & filterOther (different base filter)
        var result2 = filter2.And(filterOther);

        Assert.NotSame(result1, result2);
    }

    [Fact]
    public void And_WithAlways_NotCached_ReturnsThis()
    {
        var filter = new Condition(() => true);

        var result1 = filter.And(Always.Instance);
        var result2 = filter.And(Always.Instance);

        // Always optimization returns 'this', not a cached AndList
        Assert.Same(filter, result1);
        Assert.Same(filter, result2);
    }

    [Fact]
    public void And_WithNever_NotCached_ReturnsNever()
    {
        var filter = new Condition(() => true);

        var result1 = filter.And(Never.Instance);
        var result2 = filter.And(Never.Instance);

        // Never optimization returns Never, not a cached AndList
        Assert.Same(Never.Instance, result1);
        Assert.Same(Never.Instance, result2);
    }

    #endregion

    #region Or Caching Tests

    [Fact]
    public void Or_SameFilterTwice_ReturnsCachedInstance()
    {
        var filter1 = new Condition(() => false);
        var filter2 = new Condition(() => true);

        var result1 = filter1.Or(filter2);
        var result2 = filter1.Or(filter2);

        Assert.Same(result1, result2);
    }

    [Fact]
    public void Or_DifferentSecondFilters_ReturnsDifferentInstances()
    {
        var filter1 = new Condition(() => false);
        var filter2 = new Condition(() => true);
        var filter3 = new Condition(() => false);

        var result1 = filter1.Or(filter2);
        var result2 = filter1.Or(filter3);

        Assert.NotSame(result1, result2);
    }

    [Fact]
    public void Or_CachingIsPerInstance()
    {
        var filter1 = new Condition(() => false);
        var filter2 = new Condition(() => false);
        var filterOther = new Condition(() => true);

        // filter1 | filterOther
        var result1 = filter1.Or(filterOther);

        // filter2 | filterOther (different base filter)
        var result2 = filter2.Or(filterOther);

        Assert.NotSame(result1, result2);
    }

    [Fact]
    public void Or_WithAlways_NotCached_ReturnsAlways()
    {
        var filter = new Condition(() => false);

        var result1 = filter.Or(Always.Instance);
        var result2 = filter.Or(Always.Instance);

        // Always optimization returns Always, not a cached OrList
        Assert.Same(Always.Instance, result1);
        Assert.Same(Always.Instance, result2);
    }

    [Fact]
    public void Or_WithNever_NotCached_ReturnsThis()
    {
        var filter = new Condition(() => true);

        var result1 = filter.Or(Never.Instance);
        var result2 = filter.Or(Never.Instance);

        // Never optimization returns 'this', not a cached OrList
        Assert.Same(filter, result1);
        Assert.Same(filter, result2);
    }

    #endregion

    #region Invert Caching Tests

    [Fact]
    public void Invert_CalledTwice_ReturnsCachedInstance()
    {
        var filter = new Condition(() => true);

        var result1 = filter.Invert();
        var result2 = filter.Invert();

        Assert.Same(result1, result2);
    }

    [Fact]
    public void Invert_DifferentFilters_ReturnsDifferentInstances()
    {
        var filter1 = new Condition(() => true);
        var filter2 = new Condition(() => false);

        var result1 = filter1.Invert();
        var result2 = filter2.Invert();

        Assert.NotSame(result1, result2);
    }

    [Fact]
    public void Invert_Always_ReturnsNever()
    {
        var result1 = Always.Instance.Invert();
        var result2 = Always.Instance.Invert();

        Assert.Same(Never.Instance, result1);
        Assert.Same(result1, result2);
    }

    [Fact]
    public void Invert_Never_ReturnsAlways()
    {
        var result1 = Never.Instance.Invert();
        var result2 = Never.Instance.Invert();

        Assert.Same(Always.Instance, result1);
        Assert.Same(result1, result2);
    }

    #endregion

    #region Combined Operations Caching Tests

    [Fact]
    public void CombinedOperations_IndependentCaches()
    {
        var filter1 = new Condition(() => true);
        var filter2 = new Condition(() => false);

        // And and Or use different caches
        var andResult = filter1.And(filter2);
        var orResult = filter1.Or(filter2);

        Assert.NotSame(andResult, orResult);
    }

    [Fact]
    public void CombinedOperations_CachesPersistAcrossMultipleCalls()
    {
        var filter1 = new Condition(() => true);
        var filter2 = new Condition(() => false);
        var filter3 = new Condition(() => true);

        // Create multiple combinations
        var and12 = filter1.And(filter2);
        var and13 = filter1.And(filter3);
        var or12 = filter1.Or(filter2);

        // Verify all are cached independently
        Assert.Same(and12, filter1.And(filter2));
        Assert.Same(and13, filter1.And(filter3));
        Assert.Same(or12, filter1.Or(filter2));
    }

    [Fact]
    public void ChainedOperations_EachLevelCachesIndependently()
    {
        var condition1 = new Condition(() => true);
        var condition2 = new Condition(() => false);
        var condition3 = new Condition(() => true);

        // (condition1 & condition2) | condition3
        var andResult = condition1.And(condition2);
        var orResult = ((Filter)andResult).Or(condition3);

        // Verify the And result is cached
        Assert.Same(andResult, condition1.And(condition2));

        // The Or result should also be cached on the AndList
        var orResult2 = ((Filter)andResult).Or(condition3);
        Assert.Same(orResult, orResult2);
    }

    #endregion

    #region Operator Caching Tests

    [Fact]
    public void AndOperator_UsesSameCacheAsMethod()
    {
        var filter1 = new Condition(() => true);
        var filter2 = new Condition(() => false);

        var methodResult = filter1.And(filter2);
        var operatorResult = filter1 & filter2;

        Assert.Same(methodResult, operatorResult);
    }

    [Fact]
    public void OrOperator_UsesSameCacheAsMethod()
    {
        var filter1 = new Condition(() => false);
        var filter2 = new Condition(() => true);

        var methodResult = filter1.Or(filter2);
        var operatorResult = filter1 | filter2;

        Assert.Same(methodResult, operatorResult);
    }

    [Fact]
    public void NotOperator_UsesSameCacheAsMethod()
    {
        var filter = new Condition(() => true);

        var methodResult = filter.Invert();
        var operatorResult = ~filter;

        Assert.Same(methodResult, operatorResult);
    }

    #endregion

    #region Comprehensive Caching Tests (US6)

    [Fact]
    public void Caching_ManyAndOperations_AllCached()
    {
        var baseFilter = new Condition(() => true);
        var filters = Enumerable.Range(0, 10).Select(i => new Condition(() => true)).ToArray();

        // Create combinations first time
        var results = filters.Select(f => baseFilter.And(f)).ToArray();

        // Verify all are cached
        for (int i = 0; i < filters.Length; i++)
        {
            Assert.Same(results[i], baseFilter.And(filters[i]));
        }
    }

    [Fact]
    public void Caching_ManyOrOperations_AllCached()
    {
        var baseFilter = new Condition(() => false);
        var filters = Enumerable.Range(0, 10).Select(i => new Condition(() => true)).ToArray();

        // Create combinations first time
        var results = filters.Select(f => baseFilter.Or(f)).ToArray();

        // Verify all are cached
        for (int i = 0; i < filters.Length; i++)
        {
            Assert.Same(results[i], baseFilter.Or(filters[i]));
        }
    }

    [Fact]
    public void Caching_InvertMultipleTimes_AlwaysReturnsCached()
    {
        var condition = new Condition(() => true);

        var first = condition.Invert();
        for (int i = 0; i < 100; i++)
        {
            Assert.Same(first, condition.Invert());
        }
    }

    [Fact]
    public void Caching_ComplexChain_EachCombinationCached()
    {
        var a = new Condition(() => true);
        var b = new Condition(() => false);
        var c = new Condition(() => true);
        var d = new Condition(() => false);

        // Build: ((a & b) | c) & d
        var ab = a.And(b);
        var abc = ((Filter)ab).Or(c);
        var abcd = ((Filter)abc).And(d);

        // Verify each step is cached
        Assert.Same(ab, a.And(b));
        Assert.Same(abc, ((Filter)ab).Or(c));
        Assert.Same(abcd, ((Filter)abc).And(d));
    }

    [Fact]
    public void Caching_AndOrSameFilters_UseDifferentCaches()
    {
        var filter1 = new Condition(() => true);
        var filter2 = new Condition(() => false);

        // a & b
        var andResult = filter1.And(filter2);

        // a | b
        var orResult = filter1.Or(filter2);

        // They should be different (different operations)
        Assert.NotSame(andResult, orResult);

        // But both should be cached
        Assert.Same(andResult, filter1.And(filter2));
        Assert.Same(orResult, filter1.Or(filter2));
    }

    [Fact]
    public void Caching_InvertOfCombinedFilter_IsCached()
    {
        var a = new Condition(() => true);
        var b = new Condition(() => false);

        var combined = a.And(b);
        var inverted1 = ((Filter)combined).Invert();
        var inverted2 = ((Filter)combined).Invert();

        Assert.Same(inverted1, inverted2);
    }

    #endregion
}
