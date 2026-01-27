using Stroke.Filters;
using Xunit;

namespace Stroke.Tests.Filters;

/// <summary>
/// Tests for the <see cref="InvertFilter"/> internal class.
/// </summary>
public sealed class InvertTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullFilter_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new InvertFilter(null!));
    }

    [Fact]
    public void Constructor_WithValidFilter_DoesNotThrow()
    {
        var condition = new Condition(() => true);

        var inverted = new InvertFilter(condition);

        Assert.NotNull(inverted);
    }

    #endregion

    #region InnerFilter Property Tests

    [Fact]
    public void InnerFilter_ReturnsWrappedFilter()
    {
        var condition = new Condition(() => true);

        var inverted = new InvertFilter(condition);

        Assert.Same(condition, inverted.InnerFilter);
    }

    #endregion

    #region Invoke Tests

    [Fact]
    public void Invoke_WhenInnerReturnsTrue_ReturnsFalse()
    {
        var condition = new Condition(() => true);
        var inverted = new InvertFilter(condition);

        Assert.False(inverted.Invoke());
    }

    [Fact]
    public void Invoke_WhenInnerReturnsFalse_ReturnsTrue()
    {
        var condition = new Condition(() => false);
        var inverted = new InvertFilter(condition);

        Assert.True(inverted.Invoke());
    }

    [Fact]
    public void Invoke_PropagatesExceptions()
    {
        var throwingCondition = new Condition(() => throw new InvalidOperationException("Test exception"));
        var inverted = new InvertFilter(throwingCondition);

        var ex = Assert.Throws<InvalidOperationException>(() => inverted.Invoke());
        Assert.Equal("Test exception", ex.Message);
    }

    [Fact]
    public void Invoke_ReEvaluatesOnEachCall()
    {
        var state = true;
        var condition = new Condition(() => state);
        var inverted = new InvertFilter(condition);

        Assert.False(inverted.Invoke()); // ~true = false

        state = false;
        Assert.True(inverted.Invoke()); // ~false = true

        state = true;
        Assert.False(inverted.Invoke()); // ~true = false
    }

    #endregion

    #region Double Negation Tests

    [Fact]
    public void DoubleNegation_ProducesOriginalResult()
    {
        var condition = new Condition(() => true);

        var inverted = condition.Invert();
        var doubleInverted = inverted.Invert();

        Assert.True(doubleInverted.Invoke()); // ~~true = true
    }

    [Fact]
    public void DoubleNegation_WithFalse_ProducesOriginalResult()
    {
        var condition = new Condition(() => false);

        var inverted = condition.Invert();
        var doubleInverted = inverted.Invert();

        Assert.False(doubleInverted.Invoke()); // ~~false = false
    }

    [Fact]
    public void DoubleNegation_CreatesNestedInvertFilter()
    {
        var condition = new Condition(() => true);

        var inverted = (InvertFilter)condition.Invert();
        var doubleInverted = (InvertFilter)inverted.Invert();

        // Verify it's nested (InvertFilter wrapping InvertFilter)
        Assert.IsType<InvertFilter>(doubleInverted.InnerFilter);
    }

    [Fact]
    public void TripleNegation_ProducesInvertedResult()
    {
        var condition = new Condition(() => true);

        var inverted1 = condition.Invert();
        var inverted2 = inverted1.Invert();
        var inverted3 = inverted2.Invert();

        Assert.False(inverted3.Invoke()); // ~~~true = false
    }

    #endregion

    #region ToString Tests

    [Fact]
    public void ToString_PrefixesWithTilde()
    {
        var condition = new Condition(() => true);
        var inverted = new InvertFilter(condition);

        Assert.Equal("~Condition", inverted.ToString());
    }

    [Fact]
    public void ToString_WithAlways_ReturnsExpectedFormat()
    {
        // Note: Always.Invert() returns Never directly, so we have to construct manually
        var inverted = new InvertFilter(Always.Instance);

        Assert.Equal("~Always", inverted.ToString());
    }

    [Fact]
    public void ToString_WithNestedInvert_ShowsNestedTildes()
    {
        var condition = new Condition(() => true);
        var inverted = new InvertFilter(condition);
        var doubleInverted = new InvertFilter(inverted);

        Assert.Equal("~~Condition", doubleInverted.ToString());
    }

    #endregion

    #region Integration with Filter.Invert Tests

    [Fact]
    public void FilterInvert_CreatesInvertFilter()
    {
        var condition = new Condition(() => true);

        var inverted = condition.Invert();

        Assert.IsType<InvertFilter>(inverted);
    }

    [Fact]
    public void FilterInvert_CachesResult()
    {
        var condition = new Condition(() => true);

        var inverted1 = condition.Invert();
        var inverted2 = condition.Invert();

        Assert.Same(inverted1, inverted2);
    }

    [Fact]
    public void NotOperator_CreatesInvertFilter()
    {
        var condition = new Condition(() => true);

        var inverted = ~condition;

        Assert.IsType<InvertFilter>(inverted);
    }

    #endregion

    #region Combination Tests

    [Fact]
    public void InvertedFilter_CanBeCombinedWithAnd()
    {
        var condition1 = new Condition(() => true);
        var condition2 = new Condition(() => true);

        var inverted = condition1.Invert();
        var combined = ((Filter)inverted).And(condition2);

        // ~true & true = false & true = false
        Assert.False(combined.Invoke());
    }

    [Fact]
    public void InvertedFilter_CanBeCombinedWithOr()
    {
        var condition1 = new Condition(() => true);
        var condition2 = new Condition(() => false);

        var inverted = condition1.Invert();
        var combined = ((Filter)inverted).Or(condition2);

        // ~true | false = false | false = false
        Assert.False(combined.Invoke());
    }

    [Fact]
    public void ComplexExpression_WithInvert()
    {
        var condition1 = new Condition(() => true);
        var condition2 = new Condition(() => false);

        // ~(true & false) = ~false = true
        var andResult = condition1.And(condition2);
        var inverted = ((Filter)andResult).Invert();

        Assert.True(inverted.Invoke());
    }

    #endregion
}
