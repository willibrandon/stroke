using Stroke.Filters;
using Xunit;

namespace Stroke.Tests.Filters;

/// <summary>
/// Tests for the <see cref="Filter"/> abstract base class behavior.
/// </summary>
public sealed class FilterTests
{
    /// <summary>
    /// A concrete test filter that returns a configurable value.
    /// </summary>
    private sealed class TestFilter : Filter
    {
        private readonly bool _value;

        public TestFilter(bool value)
        {
            _value = value;
        }

        public override bool Invoke()
        {
            return _value;
        }

        public override string ToString() => $"TestFilter({_value})";
    }

    #region And() Tests

    [Fact]
    public void And_WithNullOther_ThrowsArgumentNullException()
    {
        var filter = new TestFilter(true);

        Assert.Throws<ArgumentNullException>(() => filter.And(null!));
    }

    [Fact]
    public void And_WithAlways_ReturnsThis()
    {
        var filter = new TestFilter(true);

        var result = filter.And(Always.Instance);

        Assert.Same(filter, result);
    }

    [Fact]
    public void And_WithNever_ReturnsNever()
    {
        var filter = new TestFilter(true);

        var result = filter.And(Never.Instance);

        Assert.Same(Never.Instance, result);
    }

    [Fact]
    public void And_WithOtherFilter_ReturnsCombinedFilter()
    {
        var filter1 = new TestFilter(true);
        var filter2 = new TestFilter(false);

        var result = filter1.And(filter2);

        Assert.NotSame(filter1, result);
        Assert.NotSame(filter2, result);
    }

    [Fact]
    public void And_WithSameFilterTwice_ReturnsCachedResult()
    {
        var filter1 = new TestFilter(true);
        var filter2 = new TestFilter(false);

        var result1 = filter1.And(filter2);
        var result2 = filter1.And(filter2);

        Assert.Same(result1, result2);
    }

    #endregion

    #region Or() Tests

    [Fact]
    public void Or_WithNullOther_ThrowsArgumentNullException()
    {
        var filter = new TestFilter(true);

        Assert.Throws<ArgumentNullException>(() => filter.Or(null!));
    }

    [Fact]
    public void Or_WithAlways_ReturnsAlways()
    {
        var filter = new TestFilter(false);

        var result = filter.Or(Always.Instance);

        Assert.Same(Always.Instance, result);
    }

    [Fact]
    public void Or_WithNever_ReturnsThis()
    {
        var filter = new TestFilter(true);

        var result = filter.Or(Never.Instance);

        Assert.Same(filter, result);
    }

    [Fact]
    public void Or_WithOtherFilter_ReturnsCombinedFilter()
    {
        var filter1 = new TestFilter(true);
        var filter2 = new TestFilter(false);

        var result = filter1.Or(filter2);

        Assert.NotSame(filter1, result);
        Assert.NotSame(filter2, result);
    }

    [Fact]
    public void Or_WithSameFilterTwice_ReturnsCachedResult()
    {
        var filter1 = new TestFilter(true);
        var filter2 = new TestFilter(false);

        var result1 = filter1.Or(filter2);
        var result2 = filter1.Or(filter2);

        Assert.Same(result1, result2);
    }

    #endregion

    #region Invert() Tests

    [Fact]
    public void Invert_ReturnsInvertedFilter()
    {
        var filter = new TestFilter(true);

        var result = filter.Invert();

        Assert.NotSame(filter, result);
        Assert.False(result.Invoke());
    }

    [Fact]
    public void Invert_CalledTwice_ReturnsCachedResult()
    {
        var filter = new TestFilter(true);

        var result1 = filter.Invert();
        var result2 = filter.Invert();

        Assert.Same(result1, result2);
    }

    #endregion

    #region Operator Tests

    [Fact]
    public void AndOperator_DelegatesToAndMethod()
    {
        var filter1 = new TestFilter(true);
        var filter2 = new TestFilter(true);

        var result = filter1 & filter2;

        Assert.True(result.Invoke());
    }

    [Fact]
    public void OrOperator_DelegatesToOrMethod()
    {
        var filter1 = new TestFilter(false);
        var filter2 = new TestFilter(true);

        var result = filter1 | filter2;

        Assert.True(result.Invoke());
    }

    [Fact]
    public void NotOperator_DelegatesToInvertMethod()
    {
        var filter = new TestFilter(true);

        var result = ~filter;

        Assert.False(result.Invoke());
    }

    [Fact]
    public void Operators_WithNull_ThrowArgumentNullException()
    {
        Filter? nullFilter = null;
        var filter = new TestFilter(true);

        Assert.Throws<ArgumentNullException>(() => nullFilter! & filter);
        Assert.Throws<ArgumentNullException>(() => nullFilter! | filter);
        Assert.Throws<ArgumentNullException>(() => ~nullFilter!);
    }

    #endregion

    #region Protected Constructor Tests

    [Fact]
    public void Constructor_InitializesEmptyCaches()
    {
        // Verify caches start empty by checking that And/Or create new results
        var filter1 = new TestFilter(true);
        var filter2 = new TestFilter(false);
        var filter3 = new TestFilter(true);

        // First call should create new result
        var andResult1 = filter1.And(filter2);
        var orResult1 = filter1.Or(filter3);

        // Different filters should create different results
        Assert.NotSame(andResult1, filter1.And(filter3));
        Assert.NotSame(orResult1, filter1.Or(filter2));
    }

    #endregion
}
