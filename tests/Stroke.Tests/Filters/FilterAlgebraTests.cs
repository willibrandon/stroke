using Stroke.Filters;
using Xunit;

namespace Stroke.Tests.Filters;

/// <summary>
/// Comprehensive tests for filter algebraic properties.
/// Tests the boolean algebra rules that filters must satisfy.
/// </summary>
public sealed class FilterAlgebraTests
{
    #region Identity Properties

    /// <summary>
    /// Always is the identity element for AND: Always &amp; x = x
    /// </summary>
    [Fact]
    public void Identity_And_AlwaysAndX_ReturnsX()
    {
        var condition = new Condition(() => true);

        var result = Always.Instance.And(condition);

        Assert.Same(condition, result);
    }

    /// <summary>
    /// Never is the identity element for OR: Never | x = x
    /// </summary>
    [Fact]
    public void Identity_Or_NeverOrX_ReturnsX()
    {
        var condition = new Condition(() => true);

        var result = Never.Instance.Or(condition);

        Assert.Same(condition, result);
    }

    #endregion

    #region Annihilation Properties

    /// <summary>
    /// Never is the annihilator for AND: Never &amp; x = Never
    /// </summary>
    [Fact]
    public void Annihilator_And_NeverAndX_ReturnsNever()
    {
        var condition = new Condition(() => true);

        var result = Never.Instance.And(condition);

        Assert.Same(Never.Instance, result);
    }

    /// <summary>
    /// Always is the annihilator for OR: Always | x = Always
    /// </summary>
    [Fact]
    public void Annihilator_Or_AlwaysOrX_ReturnsAlways()
    {
        var condition = new Condition(() => false);

        var result = Always.Instance.Or(condition);

        Assert.Same(Always.Instance, result);
    }

    /// <summary>
    /// Annihilation from the right side via Filter.And: x &amp; Never = Never
    /// </summary>
    [Fact]
    public void Annihilator_And_XAndNever_ReturnsNever()
    {
        var condition = new Condition(() => true);

        var result = condition.And(Never.Instance);

        Assert.Same(Never.Instance, result);
    }

    /// <summary>
    /// Annihilation from the right side via Filter.Or: x | Always = Always
    /// </summary>
    [Fact]
    public void Annihilator_Or_XOrAlways_ReturnsAlways()
    {
        var condition = new Condition(() => false);

        var result = condition.Or(Always.Instance);

        Assert.Same(Always.Instance, result);
    }

    #endregion

    #region Negation Properties

    /// <summary>
    /// ~Always = Never
    /// </summary>
    [Fact]
    public void Negation_InvertAlways_ReturnsNever()
    {
        var result = Always.Instance.Invert();

        Assert.Same(Never.Instance, result);
    }

    /// <summary>
    /// ~Never = Always
    /// </summary>
    [Fact]
    public void Negation_InvertNever_ReturnsAlways()
    {
        var result = Never.Instance.Invert();

        Assert.Same(Always.Instance, result);
    }

    /// <summary>
    /// Double negation produces same result: ~~x has same value as x
    /// </summary>
    [Fact]
    public void Negation_DoubleNegation_ProducesSameValue()
    {
        var conditionTrue = new Condition(() => true);
        var conditionFalse = new Condition(() => false);

        var doubleNegatedTrue = conditionTrue.Invert().Invert();
        var doubleNegatedFalse = conditionFalse.Invert().Invert();

        Assert.Equal(conditionTrue.Invoke(), doubleNegatedTrue.Invoke());
        Assert.Equal(conditionFalse.Invoke(), doubleNegatedFalse.Invoke());
    }

    #endregion

    #region Self-Operations

    /// <summary>
    /// Always &amp; Always = Always
    /// </summary>
    [Fact]
    public void SelfOperation_AlwaysAndAlways_ReturnsAlways()
    {
        var result = Always.Instance.And(Always.Instance);

        Assert.Same(Always.Instance, result);
    }

    /// <summary>
    /// Always | Always = Always
    /// </summary>
    [Fact]
    public void SelfOperation_AlwaysOrAlways_ReturnsAlways()
    {
        var result = Always.Instance.Or(Always.Instance);

        Assert.Same(Always.Instance, result);
    }

    /// <summary>
    /// Never &amp; Never = Never
    /// </summary>
    [Fact]
    public void SelfOperation_NeverAndNever_ReturnsNever()
    {
        var result = Never.Instance.And(Never.Instance);

        Assert.Same(Never.Instance, result);
    }

    /// <summary>
    /// Never | Never = Never
    /// </summary>
    [Fact]
    public void SelfOperation_NeverOrNever_ReturnsNever()
    {
        var result = Never.Instance.Or(Never.Instance);

        Assert.Same(Never.Instance, result);
    }

    #endregion

    #region Cross-Constant Operations

    /// <summary>
    /// Always &amp; Never = Never
    /// </summary>
    [Fact]
    public void CrossConstant_AlwaysAndNever_ReturnsNever()
    {
        var result = Always.Instance.And(Never.Instance);

        Assert.Same(Never.Instance, result);
    }

    /// <summary>
    /// Never &amp; Always = Never
    /// </summary>
    [Fact]
    public void CrossConstant_NeverAndAlways_ReturnsNever()
    {
        var result = Never.Instance.And(Always.Instance);

        Assert.Same(Never.Instance, result);
    }

    /// <summary>
    /// Always | Never = Always
    /// </summary>
    [Fact]
    public void CrossConstant_AlwaysOrNever_ReturnsAlways()
    {
        var result = Always.Instance.Or(Never.Instance);

        Assert.Same(Always.Instance, result);
    }

    /// <summary>
    /// Never | Always = Always
    /// </summary>
    [Fact]
    public void CrossConstant_NeverOrAlways_ReturnsAlways()
    {
        var result = Never.Instance.Or(Always.Instance);

        Assert.Same(Always.Instance, result);
    }

    #endregion

    #region Idempotent Operations

    /// <summary>
    /// x &amp; x should still work (deduplication returns x)
    /// </summary>
    [Fact]
    public void Idempotent_AndWithSelf_Deduplicates()
    {
        var condition = new Condition(() => true);

        var result = condition.And(condition);

        // Due to deduplication, a & a returns just a
        Assert.Same(condition, result);
    }

    /// <summary>
    /// x | x should still work (deduplication returns x)
    /// </summary>
    [Fact]
    public void Idempotent_OrWithSelf_Deduplicates()
    {
        var condition = new Condition(() => true);

        var result = condition.Or(condition);

        // Due to deduplication, a | a returns just a
        Assert.Same(condition, result);
    }

    #endregion

    #region Evaluation Correctness

    /// <summary>
    /// Verify AND evaluation is correct for all combinations
    /// </summary>
    [Theory]
    [InlineData(true, true, true)]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    [InlineData(false, false, false)]
    public void Evaluation_And_TruthTable(bool a, bool b, bool expected)
    {
        var filterA = new Condition(() => a);
        var filterB = new Condition(() => b);

        var result = filterA.And(filterB);

        Assert.Equal(expected, result.Invoke());
    }

    /// <summary>
    /// Verify OR evaluation is correct for all combinations
    /// </summary>
    [Theory]
    [InlineData(true, true, true)]
    [InlineData(true, false, true)]
    [InlineData(false, true, true)]
    [InlineData(false, false, false)]
    public void Evaluation_Or_TruthTable(bool a, bool b, bool expected)
    {
        var filterA = new Condition(() => a);
        var filterB = new Condition(() => b);

        var result = filterA.Or(filterB);

        Assert.Equal(expected, result.Invoke());
    }

    /// <summary>
    /// Verify NOT evaluation is correct
    /// </summary>
    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void Evaluation_Not_TruthTable(bool input, bool expected)
    {
        var filter = new Condition(() => input);

        var result = filter.Invert();

        Assert.Equal(expected, result.Invoke());
    }

    #endregion

    #region Complex Expressions

    /// <summary>
    /// Test complex expression: (a &amp; b) | c
    /// </summary>
    [Theory]
    [InlineData(true, true, false, true)]   // (T & T) | F = T | F = T
    [InlineData(true, false, false, false)] // (T & F) | F = F | F = F
    [InlineData(false, true, true, true)]   // (F & T) | T = F | T = T
    [InlineData(false, false, false, false)] // (F & F) | F = F | F = F
    public void ComplexExpression_AndThenOr(bool a, bool b, bool c, bool expected)
    {
        var filterA = new Condition(() => a);
        var filterB = new Condition(() => b);
        var filterC = new Condition(() => c);

        var andResult = filterA.And(filterB);
        var orResult = ((Filter)andResult).Or(filterC);

        Assert.Equal(expected, orResult.Invoke());
    }

    /// <summary>
    /// Test complex expression: ~(a &amp; b)
    /// </summary>
    [Theory]
    [InlineData(true, true, false)]   // ~(T & T) = ~T = F
    [InlineData(true, false, true)]   // ~(T & F) = ~F = T
    [InlineData(false, true, true)]   // ~(F & T) = ~F = T
    [InlineData(false, false, true)]  // ~(F & F) = ~F = T
    public void ComplexExpression_NotAnd(bool a, bool b, bool expected)
    {
        var filterA = new Condition(() => a);
        var filterB = new Condition(() => b);

        var andResult = filterA.And(filterB);
        var notResult = ((Filter)andResult).Invert();

        Assert.Equal(expected, notResult.Invoke());
    }

    /// <summary>
    /// Test De Morgan's law: ~(a &amp; b) = ~a | ~b
    /// </summary>
    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public void DeMorgan_NotAnd_Equals_NotOrNot(bool a, bool b)
    {
        var filterA = new Condition(() => a);
        var filterB = new Condition(() => b);

        // ~(a & b)
        var andResult = filterA.And(filterB);
        var notAnd = ((Filter)andResult).Invert();

        // ~a | ~b
        var notA = filterA.Invert();
        var notB = filterB.Invert();
        var notOrNot = ((Filter)notA).Or(notB);

        Assert.Equal(notAnd.Invoke(), notOrNot.Invoke());
    }

    /// <summary>
    /// Test De Morgan's law: ~(a | b) = ~a &amp; ~b
    /// </summary>
    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public void DeMorgan_NotOr_Equals_NotAndNot(bool a, bool b)
    {
        var filterA = new Condition(() => a);
        var filterB = new Condition(() => b);

        // ~(a | b)
        var orResult = filterA.Or(filterB);
        var notOr = ((Filter)orResult).Invert();

        // ~a & ~b
        var notA = filterA.Invert();
        var notB = filterB.Invert();
        var notAndNot = ((Filter)notA).And(notB);

        Assert.Equal(notOr.Invoke(), notAndNot.Invoke());
    }

    #endregion

    #region Operator Equivalence

    /// <summary>
    /// Verify & operator produces same results as And method
    /// </summary>
    [Fact]
    public void Operator_And_EquivalentToMethod()
    {
        var filterA = new Condition(() => true);
        var filterB = new Condition(() => false);

        var methodResult = filterA.And(filterB);
        var operatorResult = filterA & filterB;

        Assert.Same(methodResult, operatorResult);
    }

    /// <summary>
    /// Verify | operator produces same results as Or method
    /// </summary>
    [Fact]
    public void Operator_Or_EquivalentToMethod()
    {
        var filterA = new Condition(() => true);
        var filterB = new Condition(() => false);

        var methodResult = filterA.Or(filterB);
        var operatorResult = filterA | filterB;

        Assert.Same(methodResult, operatorResult);
    }

    /// <summary>
    /// Verify ~ operator produces same results as Invert method
    /// </summary>
    [Fact]
    public void Operator_Not_EquivalentToMethod()
    {
        var filter = new Condition(() => true);

        var methodResult = filter.Invert();
        var operatorResult = ~filter;

        Assert.Same(methodResult, operatorResult);
    }

    #endregion
}
