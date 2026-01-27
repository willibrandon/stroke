using Stroke.Styles;
using Xunit;

namespace Stroke.Tests.Styles;

/// <summary>
/// Tests for the DynamicStyle class.
/// </summary>
public class DynamicStyleTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_ThrowsForNullGetStyle()
    {
        Assert.Throws<ArgumentNullException>(() => new DynamicStyle(null!));
    }

    [Fact]
    public void Constructor_AcceptsValidCallable()
    {
        var dynamicStyle = new DynamicStyle(() => new Style([]));
        Assert.NotNull(dynamicStyle);
    }

    #endregion

    #region GetAttrsForStyleStr Tests

    [Fact]
    public void GetAttrsForStyleStr_DelegatesToUnderlyingStyle()
    {
        var underlyingStyle = new Style([("title", "bold #ff0000")]);
        var dynamicStyle = new DynamicStyle(() => underlyingStyle);

        var result = dynamicStyle.GetAttrsForStyleStr("class:title");

        Assert.True(result.Bold);
        Assert.Equal("ff0000", result.Color);
    }

    [Fact]
    public void GetAttrsForStyleStr_UsesDummyStyleWhenNull()
    {
        var dynamicStyle = new DynamicStyle(() => null);

        var result = dynamicStyle.GetAttrsForStyleStr("class:title");

        Assert.Equal(DefaultAttrs.Default, result);
    }

    [Fact]
    public void GetAttrsForStyleStr_ReflectsDynamicChanges()
    {
        IStyle? currentStyle = new Style([("title", "bold")]);
        var dynamicStyle = new DynamicStyle(() => currentStyle);

        var result1 = dynamicStyle.GetAttrsForStyleStr("class:title");
        Assert.True(result1.Bold);

        currentStyle = new Style([("title", "italic")]);
        var result2 = dynamicStyle.GetAttrsForStyleStr("class:title");
        Assert.True(result2.Italic);
        Assert.False(result2.Bold);
    }

    [Fact]
    public void GetAttrsForStyleStr_PassesDefaultParameter()
    {
        var dynamicStyle = new DynamicStyle(() => null);
        var customDefault = new Attrs(Color: "ff0000");

        var result = dynamicStyle.GetAttrsForStyleStr("", customDefault);

        Assert.Equal("ff0000", result.Color);
    }

    #endregion

    #region StyleRules Property Tests

    [Fact]
    public void StyleRules_DelegatesToUnderlyingStyle()
    {
        var underlyingStyle = new Style([("title", "bold"), ("error", "red")]);
        var dynamicStyle = new DynamicStyle(() => underlyingStyle);

        var rules = dynamicStyle.StyleRules;

        Assert.Equal(2, rules.Count);
        Assert.Equal(("title", "bold"), rules[0]);
        Assert.Equal(("error", "red"), rules[1]);
    }

    [Fact]
    public void StyleRules_ReturnsEmptyForNull()
    {
        var dynamicStyle = new DynamicStyle(() => null);

        var rules = dynamicStyle.StyleRules;

        Assert.Empty(rules);
    }

    [Fact]
    public void StyleRules_ReflectsDynamicChanges()
    {
        IStyle? currentStyle = new Style([("title", "bold")]);
        var dynamicStyle = new DynamicStyle(() => currentStyle);

        Assert.Single(dynamicStyle.StyleRules);

        currentStyle = new Style([("a", "bold"), ("b", "italic")]);
        Assert.Equal(2, dynamicStyle.StyleRules.Count);
    }

    #endregion

    #region InvalidationHash Property Tests

    [Fact]
    public void InvalidationHash_DelegatesToUnderlyingStyle()
    {
        var underlyingStyle = new Style([("title", "bold")]);
        var dynamicStyle = new DynamicStyle(() => underlyingStyle);

        Assert.Equal(underlyingStyle.InvalidationHash, dynamicStyle.InvalidationHash);
    }

    [Fact]
    public void InvalidationHash_ReturnsDummyHashForNull()
    {
        var dynamicStyle = new DynamicStyle(() => null);

        Assert.Equal(DummyStyle.Instance.InvalidationHash, dynamicStyle.InvalidationHash);
    }

    [Fact]
    public void InvalidationHash_ReflectsDynamicChanges()
    {
        var style1 = new Style([("title", "bold")]);
        var style2 = new Style([("error", "red")]);
        IStyle? currentStyle = style1;
        var dynamicStyle = new DynamicStyle(() => currentStyle);

        var hash1 = dynamicStyle.InvalidationHash;
        currentStyle = style2;
        var hash2 = dynamicStyle.InvalidationHash;

        Assert.NotEqual(hash1, hash2);
    }

    #endregion

    #region IStyle Implementation Tests

    [Fact]
    public void ImplementsIStyle()
    {
        var dynamicStyle = new DynamicStyle(() => null);
        Assert.IsAssignableFrom<IStyle>(dynamicStyle);
    }

    #endregion

    #region Thread Safety Tests

    [Fact]
    public async Task DynamicStyle_IsThreadSafe()
    {
        var style1 = new Style([("title", "bold #ff0000")]);
        var style2 = new Style([("title", "italic #00ff00")]);
        var toggle = false;
        var dynamicStyle = new DynamicStyle(() => toggle ? style2 : style1);

        var exceptions = new List<Exception>();
        var tasks = new Task[10];

        for (int i = 0; i < tasks.Length; i++)
        {
            tasks[i] = Task.Run(() =>
            {
                try
                {
                    for (int j = 0; j < 100; j++)
                    {
                        var attrs = dynamicStyle.GetAttrsForStyleStr("class:title");
                        // Should have either style1 or style2's attributes
                        Assert.True(attrs.Bold == true || attrs.Italic == true);

                        var rules = dynamicStyle.StyleRules;
                        Assert.Single(rules);

                        var hash = dynamicStyle.InvalidationHash;
                        Assert.NotNull(hash);

                        // Toggle style periodically
                        if (j % 10 == 0)
                        {
                            toggle = !toggle;
                        }
                    }
                }
                catch (Exception ex)
                {
                    lock (exceptions)
                    {
                        exceptions.Add(ex);
                    }
                }
            }, TestContext.Current.CancellationToken);
        }

        await Task.WhenAll(tasks);
        Assert.Empty(exceptions);
    }

    #endregion

    #region Python Parity Tests

    [Fact]
    public void DynamicStyle_MatchesPythonBehavior()
    {
        // Python: DynamicStyle(lambda: Style([('title', 'bold')]))
        var dynamicStyle = new DynamicStyle(() => new Style([("title", "bold")]));
        var result = dynamicStyle.GetAttrsForStyleStr("class:title");

        Assert.True(result.Bold);
    }

    [Fact]
    public void DynamicStyle_MatchesPythonBehavior_NullReturnsDefaults()
    {
        // Python: DynamicStyle(lambda: None)
        var dynamicStyle = new DynamicStyle(() => null);
        var result = dynamicStyle.GetAttrsForStyleStr("class:title");

        Assert.Equal(DefaultAttrs.Default, result);
    }

    #endregion
}
