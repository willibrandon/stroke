namespace Stroke.Tests.Core;

using Stroke.Core;
using Xunit;

/// <summary>
/// Tests for <see cref="ConversionUtils"/> class.
/// </summary>
public class ConversionUtilsTests
{
    #region ToStr Tests

    [Fact]
    public void ToStr_String_ReturnsString()
    {
        Assert.Equal("hello", ConversionUtils.ToStr("hello"));
        Assert.Equal("world", ConversionUtils.ToStr("world"));
    }

    [Fact]
    public void ToStr_NullString_ReturnsEmptyString()
    {
        Assert.Equal("", ConversionUtils.ToStr((string?)null));
    }

    [Fact]
    public void ToStr_FuncString_InvokesAndReturnsResult()
    {
        Func<string?> getter = () => "from function";
        Assert.Equal("from function", ConversionUtils.ToStr(getter));
    }

    [Fact]
    public void ToStr_NullFuncString_ReturnsEmptyString()
    {
        Func<string?>? getter = null;
        Assert.Equal("", ConversionUtils.ToStr(getter));
    }

    [Fact]
    public void ToStr_FuncReturningNull_ReturnsEmptyString()
    {
        Func<string?> getter = () => null;
        Assert.Equal("", ConversionUtils.ToStr(getter));
    }

    [Fact]
    public void ToStr_NestedFunc_RecursivelyUnwraps()
    {
        Func<Func<string?>?> nested = () => () => "nested value";
        Assert.Equal("nested value", ConversionUtils.ToStr(nested));
    }

    [Fact]
    public void ToStr_Object_CallsToString()
    {
        var obj = new TestObject("test value");
        Assert.Equal("test value", ConversionUtils.ToStr((object)obj));
    }

    [Fact]
    public void ToStr_ObjectWithNullToString_ReturnsEmptyString()
    {
        var obj = new NullToStringObject();
        Assert.Equal("", ConversionUtils.ToStr((object)obj));
    }

    [Fact]
    public void ToStr_NullObject_ReturnsEmptyString()
    {
        Assert.Equal("", ConversionUtils.ToStr((object?)null));
    }

    [Fact]
    public void ToStr_ObjectThatIsFunc_UnwrapsFunc()
    {
        object obj = (Func<string?>)(() => "unwrapped");
        Assert.Equal("unwrapped", ConversionUtils.ToStr(obj));
    }

    #endregion

    #region ToInt Tests

    [Fact]
    public void ToInt_Int_ReturnsInt()
    {
        Assert.Equal(42, ConversionUtils.ToInt(42));
        Assert.Equal(-10, ConversionUtils.ToInt(-10));
        Assert.Equal(0, ConversionUtils.ToInt(0));
    }

    [Fact]
    public void ToInt_FuncInt_InvokesAndReturnsResult()
    {
        Func<int> getter = () => 123;
        Assert.Equal(123, ConversionUtils.ToInt(getter));
    }

    [Fact]
    public void ToInt_NullFuncInt_ReturnsZero()
    {
        Func<int>? getter = null;
        Assert.Equal(0, ConversionUtils.ToInt(getter));
    }

    [Fact]
    public void ToInt_Object_UsesConvertToInt32()
    {
        Assert.Equal(42, ConversionUtils.ToInt((object)42));
        Assert.Equal(43, ConversionUtils.ToInt((object)42.7)); // rounds (Convert.ToInt32 uses banker's rounding)
        Assert.Equal(42, ConversionUtils.ToInt((object)42.4)); // rounds down
        Assert.Equal(42, ConversionUtils.ToInt((object)"42"));
    }

    [Fact]
    public void ToInt_ObjectConversionFails_ReturnsZero()
    {
        Assert.Equal(0, ConversionUtils.ToInt((object)"not a number"));
        Assert.Equal(0, ConversionUtils.ToInt((object)new object()));
    }

    [Fact]
    public void ToInt_NullObject_ReturnsZero()
    {
        Assert.Equal(0, ConversionUtils.ToInt((object?)null));
    }

    #endregion

    #region ToFloat Tests

    [Fact]
    public void ToFloat_Double_ReturnsDouble()
    {
        Assert.Equal(3.14, ConversionUtils.ToFloat(3.14));
        Assert.Equal(-2.5, ConversionUtils.ToFloat(-2.5));
        Assert.Equal(0.0, ConversionUtils.ToFloat(0.0));
    }

    [Fact]
    public void ToFloat_FuncDouble_InvokesAndReturnsResult()
    {
        Func<double> getter = () => 2.718;
        Assert.Equal(2.718, ConversionUtils.ToFloat(getter));
    }

    [Fact]
    public void ToFloat_NullFuncDouble_ReturnsZero()
    {
        Func<double>? getter = null;
        Assert.Equal(0.0, ConversionUtils.ToFloat(getter));
    }

    [Fact]
    public void ToFloat_AnyFloat_ExtractsValue()
    {
        AnyFloat concrete = 5.5;
        Assert.Equal(5.5, ConversionUtils.ToFloat(concrete));

        Func<double> getter = () => 7.7;
        AnyFloat callable = getter;
        Assert.Equal(7.7, ConversionUtils.ToFloat(callable));
    }

    [Fact]
    public void ToFloat_Object_UsesConvertToDouble()
    {
        Assert.Equal(3.14, ConversionUtils.ToFloat((object)3.14));
        Assert.Equal(42.0, ConversionUtils.ToFloat((object)42));
        Assert.Equal(3.14, ConversionUtils.ToFloat((object)"3.14"));
    }

    [Fact]
    public void ToFloat_ObjectConversionFails_ReturnsZero()
    {
        Assert.Equal(0.0, ConversionUtils.ToFloat((object)"not a number"));
        Assert.Equal(0.0, ConversionUtils.ToFloat((object)new object()));
    }

    [Fact]
    public void ToFloat_NullObject_ReturnsZero()
    {
        Assert.Equal(0.0, ConversionUtils.ToFloat((object?)null));
    }

    #endregion

    [Fact]
    public void ToStr_NestedFunc_InnerReturnsNull_ReturnsEmptyString()
    {
        Func<Func<string?>?> nested = () => null;
        Assert.Equal("", ConversionUtils.ToStr(nested));
    }

    [Fact]
    public void ToStr_NullNestedFunc_ReturnsEmptyString()
    {
        Func<Func<string?>?>? nested = null;
        Assert.Equal("", ConversionUtils.ToStr(nested));
    }

    [Fact]
    public void ToInt_ObjectThatIsFuncInt_InvokesFunc()
    {
        object obj = (Func<int>)(() => 99);
        Assert.Equal(99, ConversionUtils.ToInt(obj));
    }

    #region AnyFloat Tests

    [Fact]
    public void AnyFloat_GetHashCode_DefaultValue_ReturnsZero()
    {
        AnyFloat value = default;
        Assert.Equal(0, value.GetHashCode());
    }

    [Fact]
    public void AnyFloat_GetHashCode_ConcreteValue_ReturnsDoubleHashCode()
    {
        AnyFloat value = 3.14;
        Assert.Equal(3.14.GetHashCode(), value.GetHashCode());
    }

    [Fact]
    public void AnyFloat_GetHashCode_Getter_ReturnsGetterHashCode()
    {
        Func<double> getter = () => 1.0;
        AnyFloat value = getter;
        Assert.Equal(getter.GetHashCode(), value.GetHashCode());
    }

    [Fact]
    public void AnyFloat_Equals_BothDefault_AreEqual()
    {
        AnyFloat a = default;
        AnyFloat b = default;
        Assert.True(a.Equals(b));
    }

    [Fact]
    public void AnyFloat_Equals_OneDefault_NotEqual()
    {
        AnyFloat a = default;
        AnyFloat b = 1.0;
        Assert.False(a.Equals(b));
    }

    [Fact]
    public void AnyFloat_Equals_BothSameConcreteValue_AreEqual()
    {
        AnyFloat a = 5.5;
        AnyFloat b = 5.5;
        Assert.True(a.Equals(b));
    }

    [Fact]
    public void AnyFloat_Equals_DifferentConcreteValues_NotEqual()
    {
        AnyFloat a = 5.5;
        AnyFloat b = 6.6;
        Assert.False(a.Equals(b));
    }

    [Fact]
    public void AnyFloat_Equals_BothSameGetter_AreEqual()
    {
        Func<double> getter = () => 1.0;
        AnyFloat a = getter;
        AnyFloat b = getter;
        Assert.True(a.Equals(b));
    }

    [Fact]
    public void AnyFloat_Equals_DifferentGetters_NotEqual()
    {
        AnyFloat a = (Func<double>)(() => 1.0);
        AnyFloat b = (Func<double>)(() => 1.0);
        Assert.False(a.Equals(b));
    }

    [Fact]
    public void AnyFloat_Equals_ConcreteVsGetter_NotEqual()
    {
        AnyFloat a = 1.0;
        AnyFloat b = (Func<double>)(() => 1.0);
        Assert.False(a.Equals(b));
    }

    [Fact]
    public void AnyFloat_EqualsObject_BoxedAnyFloat_ReturnsTrue()
    {
        AnyFloat a = 5.5;
        object b = (AnyFloat)5.5;
        Assert.True(a.Equals(b));
    }

    [Fact]
    public void AnyFloat_EqualsObject_NonAnyFloat_ReturnsFalse()
    {
        AnyFloat a = 5.5;
        Assert.False(a.Equals("not an AnyFloat"));
    }

    [Fact]
    public void AnyFloat_EqualityOperator_Equal_ReturnsTrue()
    {
        AnyFloat a = 5.5;
        AnyFloat b = 5.5;
        Assert.True(a == b);
    }

    [Fact]
    public void AnyFloat_InequalityOperator_Different_ReturnsTrue()
    {
        AnyFloat a = 5.5;
        AnyFloat b = 6.6;
        Assert.True(a != b);
    }

    [Fact]
    public void AnyFloat_ExplicitCast_ReturnsValue()
    {
        AnyFloat f = 7.7;
        double d = (double)f;
        Assert.Equal(7.7, d);
    }

    [Fact]
    public void AnyFloat_Value_FromGetter_InvokesGetter()
    {
        Func<double> getter = () => 42.0;
        AnyFloat value = getter;
        Assert.Equal(42.0, value.Value);
    }

    #endregion

    #region Test Helpers

    private sealed class TestObject(string value)
    {
        public override string? ToString() => value;
    }

    private sealed class NullToStringObject
    {
        public override string? ToString() => null;
    }

    #endregion
}
