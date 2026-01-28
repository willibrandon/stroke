namespace Stroke.Tests.Core;

using Stroke.Core;
using Xunit;

/// <summary>
/// Tests for <see cref="AnyFloat"/> struct.
/// </summary>
public class AnyFloatTests
{
    #region Implicit Conversion Tests

    [Fact]
    public void ImplicitConversion_FromDouble_Works()
    {
        AnyFloat value = 3.14;
        Assert.Equal(3.14, value.Value);
    }

    [Fact]
    public void ImplicitConversion_FromFuncDouble_Works()
    {
        Func<double> getter = () => 2.718;
        AnyFloat value = getter;
        Assert.Equal(2.718, value.Value);
    }

    #endregion

    #region Explicit Conversion Tests

    [Fact]
    public void ExplicitConversion_ToDouble_Works()
    {
        AnyFloat value = 3.14;
        double result = (double)value;
        Assert.Equal(3.14, result);
    }

    [Fact]
    public void ExplicitConversion_FromCallable_InvokesCallable()
    {
        Func<double> getter = () => 5.5;
        AnyFloat value = getter;
        double result = (double)value;
        Assert.Equal(5.5, result);
    }

    #endregion

    #region Value Property Tests

    [Fact]
    public void Value_WithConcreteValue_ReturnsValue()
    {
        AnyFloat value = 42.0;
        Assert.Equal(42.0, value.Value);
    }

    [Fact]
    public void Value_WithCallable_InvokesCallable()
    {
        var callCount = 0;
        Func<double> getter = () =>
        {
            callCount++;
            return 99.0;
        };
        AnyFloat value = getter;

        // Access Value multiple times
        var result1 = value.Value;
        var result2 = value.Value;

        Assert.Equal(99.0, result1);
        Assert.Equal(99.0, result2);

        // Callable should be invoked each time (not cached)
        Assert.Equal(2, callCount);
    }

    [Fact]
    public void Value_Default_ReturnsZero()
    {
        var value = default(AnyFloat);
        Assert.Equal(0.0, value.Value);
    }

    #endregion

    #region HasValue Property Tests

    [Fact]
    public void HasValue_WithConcreteValue_ReturnsTrue()
    {
        AnyFloat value = 42.0;
        Assert.True(value.HasValue);
    }

    [Fact]
    public void HasValue_WithCallable_ReturnsTrue()
    {
        Func<double> getter = () => 42.0;
        AnyFloat value = getter;
        Assert.True(value.HasValue);
    }

    [Fact]
    public void HasValue_Default_ReturnsFalse()
    {
        var value = default(AnyFloat);
        Assert.False(value.HasValue);
    }

    [Fact]
    public void HasValue_WithZeroValue_ReturnsTrue()
    {
        // Zero is a valid value, distinct from default
        AnyFloat value = 0.0;
        Assert.True(value.HasValue);
    }

    #endregion

    #region Equality Tests

    [Fact]
    public void Equality_TwoConcreteValues_ComparesByValue()
    {
        AnyFloat a = 3.14;
        AnyFloat b = 3.14;
        AnyFloat c = 2.71;

        Assert.True(a.Equals(b));
        Assert.False(a.Equals(c));
    }

    [Fact]
    public void Equality_TwoCallables_ComparesByReference()
    {
        Func<double> func1 = () => 1.0;
        Func<double> func2 = () => 1.0; // Different delegate instance

        AnyFloat a = func1;
        AnyFloat b = func1; // Same delegate
        AnyFloat c = func2; // Different delegate

        Assert.True(a.Equals(b));  // Same reference
        Assert.False(a.Equals(c)); // Different reference, even though same result
    }

    [Fact]
    public void Equality_ConcreteVsCallable_NotEqual()
    {
        AnyFloat concrete = 5.0;
        Func<double> getter = () => 5.0;
        AnyFloat callable = getter;

        Assert.False(concrete.Equals(callable));
    }

    [Fact]
    public void Equality_WithObject_Works()
    {
        AnyFloat a = 3.14;
        object b = (AnyFloat)3.14;
        object c = "not an AnyFloat";

        Assert.True(a.Equals(b));
        Assert.False(a.Equals(c));
        Assert.False(a.Equals((object?)null));
    }

    #endregion

    #region GetHashCode Tests

    [Fact]
    public void GetHashCode_EqualValues_SameHashCode()
    {
        AnyFloat a = 3.14;
        AnyFloat b = 3.14;

        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void GetHashCode_SameCallable_SameHashCode()
    {
        Func<double> func = () => 1.0;
        AnyFloat a = func;
        AnyFloat b = func;

        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void GetHashCode_Default_DoesNotThrow()
    {
        var value = default(AnyFloat);
        var exception = Record.Exception(() => value.GetHashCode());
        Assert.Null(exception);
    }

    #endregion

    #region Operator Tests

    [Fact]
    public void OperatorEquals_Works()
    {
        AnyFloat a = 3.14;
        AnyFloat b = 3.14;
        AnyFloat c = 2.71;

        Assert.True(a == b);
        Assert.False(a == c);
    }

    [Fact]
    public void OperatorNotEquals_Works()
    {
        AnyFloat a = 3.14;
        AnyFloat b = 3.14;
        AnyFloat c = 2.71;

        Assert.False(a != b);
        Assert.True(a != c);
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public void AnyFloat_WithNegativeValue_Works()
    {
        AnyFloat value = -123.456;
        Assert.Equal(-123.456, value.Value);
        Assert.True(value.HasValue);
    }

    [Fact]
    public void AnyFloat_WithInfinity_Works()
    {
        AnyFloat positive = double.PositiveInfinity;
        AnyFloat negative = double.NegativeInfinity;

        Assert.Equal(double.PositiveInfinity, positive.Value);
        Assert.Equal(double.NegativeInfinity, negative.Value);
    }

    [Fact]
    public void AnyFloat_WithNaN_Works()
    {
        AnyFloat nan = double.NaN;
        Assert.True(double.IsNaN(nan.Value));
    }

    #endregion
}
