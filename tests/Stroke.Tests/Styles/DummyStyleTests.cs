using Stroke.Styles;
using Xunit;

namespace Stroke.Tests.Styles;

/// <summary>
/// Tests for the DummyStyle class.
/// </summary>
public class DummyStyleTests
{
    #region Singleton Tests

    [Fact]
    public void Instance_IsSingleton()
    {
        var instance1 = DummyStyle.Instance;
        var instance2 = DummyStyle.Instance;

        Assert.Same(instance1, instance2);
    }

    [Fact]
    public void Instance_IsNotNull()
    {
        Assert.NotNull(DummyStyle.Instance);
    }

    #endregion

    #region GetAttrsForStyleStr Tests

    [Fact]
    public void GetAttrsForStyleStr_ReturnsDefaultAttrs_WhenNoDefaultProvided()
    {
        var result = DummyStyle.Instance.GetAttrsForStyleStr("bold red");

        Assert.Equal(DefaultAttrs.Default, result);
    }

    [Fact]
    public void GetAttrsForStyleStr_ReturnsProvidedDefault()
    {
        var customDefault = new Attrs(Color: "ff0000", Bold: true);
        var result = DummyStyle.Instance.GetAttrsForStyleStr("anything", customDefault);

        Assert.Equal(customDefault, result);
    }

    [Fact]
    public void GetAttrsForStyleStr_IgnoresStyleString()
    {
        var result1 = DummyStyle.Instance.GetAttrsForStyleStr("bold");
        var result2 = DummyStyle.Instance.GetAttrsForStyleStr("italic underline");
        var result3 = DummyStyle.Instance.GetAttrsForStyleStr("class:title");

        // All should return DefaultAttrs.Default since no custom default is provided
        Assert.Equal(DefaultAttrs.Default, result1);
        Assert.Equal(DefaultAttrs.Default, result2);
        Assert.Equal(DefaultAttrs.Default, result3);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("any random string")]
    public void GetAttrsForStyleStr_AcceptsAnyString(string styleStr)
    {
        var result = DummyStyle.Instance.GetAttrsForStyleStr(styleStr);
        Assert.Equal(DefaultAttrs.Default, result);
    }

    #endregion

    #region StyleRules Tests

    [Fact]
    public void StyleRules_ReturnsEmptyList()
    {
        var rules = DummyStyle.Instance.StyleRules;

        Assert.NotNull(rules);
        Assert.Empty(rules);
    }

    [Fact]
    public void StyleRules_ReturnsSameInstance()
    {
        var rules1 = DummyStyle.Instance.StyleRules;
        var rules2 = DummyStyle.Instance.StyleRules;

        Assert.Same(rules1, rules2);
    }

    #endregion

    #region InvalidationHash Tests

    [Fact]
    public void InvalidationHash_IsConstant()
    {
        var hash1 = DummyStyle.Instance.InvalidationHash;
        var hash2 = DummyStyle.Instance.InvalidationHash;

        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void InvalidationHash_IsNotNull()
    {
        Assert.NotNull(DummyStyle.Instance.InvalidationHash);
    }

    #endregion

    #region IStyle Implementation Tests

    [Fact]
    public void ImplementsIStyle()
    {
        Assert.IsAssignableFrom<IStyle>(DummyStyle.Instance);
    }

    #endregion
}
