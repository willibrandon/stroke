using Stroke.Styles;
using Xunit;

namespace Stroke.Tests.Styles;

/// <summary>
/// Tests for the Attrs record struct and DefaultAttrs static class.
/// </summary>
public class AttrsTests
{
    #region Attrs Record Struct Tests

    [Fact]
    public void Attrs_DefaultConstructor_AllNullValues()
    {
        var attrs = new Attrs();

        Assert.Null(attrs.Color);
        Assert.Null(attrs.BgColor);
        Assert.Null(attrs.Bold);
        Assert.Null(attrs.Underline);
        Assert.Null(attrs.Strike);
        Assert.Null(attrs.Italic);
        Assert.Null(attrs.Blink);
        Assert.Null(attrs.Reverse);
        Assert.Null(attrs.Hidden);
        Assert.Null(attrs.Dim);
    }

    [Fact]
    public void Attrs_PartialConstruction_SpecifiedValuesSet()
    {
        var attrs = new Attrs(Color: "ff0000", Bold: true);

        Assert.Equal("ff0000", attrs.Color);
        Assert.True(attrs.Bold);
        Assert.Null(attrs.BgColor);
        Assert.Null(attrs.Underline);
    }

    [Fact]
    public void Attrs_FullConstruction_AllValuesSet()
    {
        var attrs = new Attrs(
            Color: "ff0000",
            BgColor: "00ff00",
            Bold: true,
            Underline: true,
            Strike: true,
            Italic: true,
            Blink: true,
            Reverse: true,
            Hidden: true,
            Dim: true);

        Assert.Equal("ff0000", attrs.Color);
        Assert.Equal("00ff00", attrs.BgColor);
        Assert.True(attrs.Bold);
        Assert.True(attrs.Underline);
        Assert.True(attrs.Strike);
        Assert.True(attrs.Italic);
        Assert.True(attrs.Blink);
        Assert.True(attrs.Reverse);
        Assert.True(attrs.Hidden);
        Assert.True(attrs.Dim);
    }

    [Fact]
    public void Attrs_WithExpression_CreatesNewInstance()
    {
        var original = new Attrs(Color: "ff0000", Bold: true);
        var modified = original with { Color = "00ff00" };

        Assert.Equal("ff0000", original.Color);
        Assert.Equal("00ff00", modified.Color);
        Assert.True(modified.Bold);
    }

    [Fact]
    public void Attrs_Equality_EqualInstances()
    {
        var attrs1 = new Attrs(Color: "ff0000", Bold: true);
        var attrs2 = new Attrs(Color: "ff0000", Bold: true);

        Assert.Equal(attrs1, attrs2);
        Assert.True(attrs1 == attrs2);
        Assert.False(attrs1 != attrs2);
    }

    [Fact]
    public void Attrs_Equality_DifferentInstances()
    {
        var attrs1 = new Attrs(Color: "ff0000", Bold: true);
        var attrs2 = new Attrs(Color: "00ff00", Bold: true);

        Assert.NotEqual(attrs1, attrs2);
        Assert.False(attrs1 == attrs2);
        Assert.True(attrs1 != attrs2);
    }

    [Fact]
    public void Attrs_GetHashCode_EqualInstancesHaveSameHash()
    {
        var attrs1 = new Attrs(Color: "ff0000", Bold: true);
        var attrs2 = new Attrs(Color: "ff0000", Bold: true);

        Assert.Equal(attrs1.GetHashCode(), attrs2.GetHashCode());
    }

    [Fact]
    public void Attrs_ToString_ContainsFieldValues()
    {
        var attrs = new Attrs(Color: "ff0000", Bold: true);
        var str = attrs.ToString();

        Assert.Contains("ff0000", str);
        Assert.Contains("True", str);
    }

    [Fact]
    public void Attrs_AnsiColorName_PreservedAsIs()
    {
        var attrs = new Attrs(Color: "ansiblue", BgColor: "ansired");

        Assert.Equal("ansiblue", attrs.Color);
        Assert.Equal("ansired", attrs.BgColor);
    }

    [Fact]
    public void Attrs_EmptyString_ValidDefaultColor()
    {
        var attrs = new Attrs(Color: "", BgColor: "");

        Assert.Equal("", attrs.Color);
        Assert.Equal("", attrs.BgColor);
    }

    #endregion

    #region DefaultAttrs.Default Tests

    [Fact]
    public void DefaultAttrs_Default_HasEmptyColors()
    {
        Assert.Equal("", DefaultAttrs.Default.Color);
        Assert.Equal("", DefaultAttrs.Default.BgColor);
    }

    [Fact]
    public void DefaultAttrs_Default_HasFalseBooleans()
    {
        Assert.False(DefaultAttrs.Default.Bold);
        Assert.False(DefaultAttrs.Default.Underline);
        Assert.False(DefaultAttrs.Default.Strike);
        Assert.False(DefaultAttrs.Default.Italic);
        Assert.False(DefaultAttrs.Default.Blink);
        Assert.False(DefaultAttrs.Default.Reverse);
        Assert.False(DefaultAttrs.Default.Hidden);
        Assert.False(DefaultAttrs.Default.Dim);
    }

    [Fact]
    public void DefaultAttrs_Default_IsSameInstance()
    {
        var default1 = DefaultAttrs.Default;
        var default2 = DefaultAttrs.Default;

        // Record structs compare by value, but the static field should be the same
        Assert.Equal(default1, default2);
    }

    [Fact]
    public void DefaultAttrs_Default_MatchesPythonDefaultAttrs()
    {
        // Verify exact match with Python's DEFAULT_ATTRS
        var expected = new Attrs(
            Color: "",
            BgColor: "",
            Bold: false,
            Underline: false,
            Strike: false,
            Italic: false,
            Blink: false,
            Reverse: false,
            Hidden: false,
            Dim: false);

        Assert.Equal(expected, DefaultAttrs.Default);
    }

    #endregion

    #region DefaultAttrs.Empty Tests

    [Fact]
    public void DefaultAttrs_Empty_HasNullColors()
    {
        Assert.Null(DefaultAttrs.Empty.Color);
        Assert.Null(DefaultAttrs.Empty.BgColor);
    }

    [Fact]
    public void DefaultAttrs_Empty_HasNullBooleans()
    {
        Assert.Null(DefaultAttrs.Empty.Bold);
        Assert.Null(DefaultAttrs.Empty.Underline);
        Assert.Null(DefaultAttrs.Empty.Strike);
        Assert.Null(DefaultAttrs.Empty.Italic);
        Assert.Null(DefaultAttrs.Empty.Blink);
        Assert.Null(DefaultAttrs.Empty.Reverse);
        Assert.Null(DefaultAttrs.Empty.Hidden);
        Assert.Null(DefaultAttrs.Empty.Dim);
    }

    [Fact]
    public void DefaultAttrs_Empty_IsSameInstance()
    {
        var empty1 = DefaultAttrs.Empty;
        var empty2 = DefaultAttrs.Empty;

        Assert.Equal(empty1, empty2);
    }

    [Fact]
    public void DefaultAttrs_Empty_MatchesPythonEmptyAttrs()
    {
        // Verify exact match with Python's _EMPTY_ATTRS
        var expected = new Attrs(
            Color: null,
            BgColor: null,
            Bold: null,
            Underline: null,
            Strike: null,
            Italic: null,
            Blink: null,
            Reverse: null,
            Hidden: null,
            Dim: null);

        Assert.Equal(expected, DefaultAttrs.Empty);
    }

    #endregion

    #region DefaultAttrs Comparison Tests

    [Fact]
    public void DefaultAttrs_DefaultAndEmpty_AreDifferent()
    {
        Assert.NotEqual(DefaultAttrs.Default, DefaultAttrs.Empty);
    }

    [Fact]
    public void DefaultAttrs_Default_IsNotEqualToNewAttrs()
    {
        // new Attrs() has all null values (like Empty)
        Assert.NotEqual(DefaultAttrs.Default, new Attrs());
    }

    [Fact]
    public void DefaultAttrs_Empty_IsEqualToNewAttrs()
    {
        // new Attrs() has all null values (same as Empty)
        Assert.Equal(DefaultAttrs.Empty, new Attrs());
    }

    #endregion

    #region Priority Enum Tests

    [Fact]
    public void Priority_DictKeyOrder_IsZero()
    {
        Assert.Equal(0, (int)Priority.DictKeyOrder);
    }

    [Fact]
    public void Priority_MostPrecise_IsOne()
    {
        Assert.Equal(1, (int)Priority.MostPrecise);
    }

    [Fact]
    public void Priority_HasTwoValues()
    {
        var values = Enum.GetValues<Priority>();
        Assert.Equal(2, values.Length);
    }

    [Fact]
    public void Priority_ToString_ReturnsName()
    {
        Assert.Equal("DictKeyOrder", Priority.DictKeyOrder.ToString());
        Assert.Equal("MostPrecise", Priority.MostPrecise.ToString());
    }

    #endregion
}
