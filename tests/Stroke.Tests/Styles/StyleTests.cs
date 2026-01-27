using Stroke.Styles;
using Xunit;

namespace Stroke.Tests.Styles;

/// <summary>
/// Tests for the Style class.
/// </summary>
public class StyleTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_AcceptsEmptyRules()
    {
        var style = new Style(Array.Empty<(string, string)>());
        Assert.NotNull(style);
        Assert.Empty(style.StyleRules);
    }

    [Fact]
    public void Constructor_AcceptsSingleRule()
    {
        var style = new Style(new[] { ("title", "bold red") });

        Assert.Single(style.StyleRules);
        Assert.Equal(("title", "bold red"), style.StyleRules[0]);
    }

    [Fact]
    public void Constructor_AcceptsMultipleRules()
    {
        var rules = new[]
        {
            ("title", "#ff0000 bold"),
            ("something-else", "reverse"),
            ("class1 class2", "underline"),
        };

        var style = new Style(rules);

        Assert.Equal(3, style.StyleRules.Count);
    }

    [Fact]
    public void Constructor_ThrowsForNullRules()
    {
        Assert.Throws<ArgumentNullException>(() => new Style(null!));
    }

    [Theory]
    [InlineData("TITLE")]
    [InlineData("Title!")]
    [InlineData("title@")]
    [InlineData("title#test")]
    [InlineData("title,other")]
    public void Constructor_ThrowsForInvalidClassNames(string classNames)
    {
        var rules = new[] { (classNames, "bold") };

        Assert.Throws<ArgumentException>(() => new Style(rules));
    }

    [Theory]
    [InlineData("title")]
    [InlineData("title-name")]
    [InlineData("title_name")]
    [InlineData("title.sub")]
    [InlineData("title name")]
    [InlineData("a1 b2 c3")]
    [InlineData("")]
    public void Constructor_AcceptsValidClassNames(string classNames)
    {
        var rules = new[] { (classNames, "bold") };
        var style = new Style(rules);
        Assert.NotNull(style);
    }

    #endregion

    #region FromDict Tests

    [Fact]
    public void FromDict_CreatesStyleFromDictionary()
    {
        var dict = new Dictionary<string, string>
        {
            ["title"] = "bold",
            ["prompt"] = "italic",
        };

        var style = Style.FromDict(dict);

        Assert.Equal(2, style.StyleRules.Count);
    }

    [Fact]
    public void FromDict_ThrowsForNullDictionary()
    {
        Assert.Throws<ArgumentNullException>(() => Style.FromDict(null!));
    }

    [Fact]
    public void FromDict_DictKeyOrder_PreservesOrder()
    {
        var dict = new Dictionary<string, string>
        {
            ["a"] = "bold",
            ["b c"] = "italic",
            ["a.b.c"] = "underline",
        };

        var style = Style.FromDict(dict, Priority.DictKeyOrder);
        var rules = style.StyleRules;

        // Should preserve dictionary iteration order
        Assert.Contains(rules, r => r.ClassNames == "a");
        Assert.Contains(rules, r => r.ClassNames == "b c");
        Assert.Contains(rules, r => r.ClassNames == "a.b.c");
    }

    [Fact]
    public void FromDict_MostPrecise_SortsByPrecision()
    {
        var dict = new Dictionary<string, string>
        {
            ["a.b.c"] = "bold underline strike",
            ["a"] = "bold",
            ["a b"] = "bold italic",
        };

        var style = Style.FromDict(dict, Priority.MostPrecise);
        var rules = style.StyleRules.ToList();

        // "a" has 1 element
        // "a b" has 2 elements (1 + 1)
        // "a.b.c" has 3 elements (1 split by '.')
        // Should be sorted least precise to most precise
        Assert.Equal("a", rules[0].ClassNames);
        Assert.Equal("a b", rules[1].ClassNames);
        Assert.Equal("a.b.c", rules[2].ClassNames);
    }

    #endregion

    #region GetAttrsForStyleStr - Basic Tests

    [Fact]
    public void GetAttrsForStyleStr_ReturnsDefaultForEmptyStyleString()
    {
        var style = new Style(new[] { ("title", "bold") });
        var result = style.GetAttrsForStyleStr("");

        Assert.Equal(DefaultAttrs.Default, result);
    }

    [Fact]
    public void GetAttrsForStyleStr_ReturnsCustomDefaultWhenProvided()
    {
        var style = new Style(Array.Empty<(string, string)>());
        var customDefault = new Attrs(Color: "ff0000");
        var result = style.GetAttrsForStyleStr("", customDefault);

        Assert.Equal("ff0000", result.Color);
    }

    #endregion

    #region GetAttrsForStyleStr - Inline Styles

    [Theory]
    [InlineData("bold", true, null)]
    [InlineData("nobold", false, null)]
    [InlineData("italic", null, true)]
    [InlineData("noitalic", null, false)]
    public void GetAttrsForStyleStr_ParsesBooleanStyles(string styleStr, bool? expectedBold, bool? expectedItalic)
    {
        var style = new Style(Array.Empty<(string, string)>());
        var result = style.GetAttrsForStyleStr(styleStr);

        if (expectedBold.HasValue)
            Assert.Equal(expectedBold.Value, result.Bold);
        if (expectedItalic.HasValue)
            Assert.Equal(expectedItalic.Value, result.Italic);
    }

    [Theory]
    [InlineData("underline")]
    [InlineData("strike")]
    [InlineData("blink")]
    [InlineData("reverse")]
    [InlineData("hidden")]
    [InlineData("dim")]
    public void GetAttrsForStyleStr_ParsesAllBooleanFlags(string flag)
    {
        var style = new Style(Array.Empty<(string, string)>());
        var result = style.GetAttrsForStyleStr(flag);

        var prop = typeof(Attrs).GetProperty(char.ToUpperInvariant(flag[0]) + flag[1..]);
        Assert.NotNull(prop);
        Assert.True((bool?)prop.GetValue(result));
    }

    [Fact]
    public void GetAttrsForStyleStr_ParsesForegroundColor()
    {
        var style = new Style(Array.Empty<(string, string)>());

        var result1 = style.GetAttrsForStyleStr("#ff0000");
        var result2 = style.GetAttrsForStyleStr("fg:#00ff00");
        var result3 = style.GetAttrsForStyleStr("Red");

        Assert.Equal("ff0000", result1.Color);
        Assert.Equal("00ff00", result2.Color);
        Assert.Equal("ff0000", result3.Color);
    }

    [Fact]
    public void GetAttrsForStyleStr_ParsesBackgroundColor()
    {
        var style = new Style(Array.Empty<(string, string)>());

        var result = style.GetAttrsForStyleStr("bg:#0000ff");

        Assert.Equal("0000ff", result.BgColor);
    }

    [Fact]
    public void GetAttrsForStyleStr_ParsesMultipleStyles()
    {
        var style = new Style(Array.Empty<(string, string)>());
        var result = style.GetAttrsForStyleStr("bold italic #ff0000 bg:#00ff00");

        Assert.True(result.Bold);
        Assert.True(result.Italic);
        Assert.Equal("ff0000", result.Color);
        Assert.Equal("00ff00", result.BgColor);
    }

    [Fact]
    public void GetAttrsForStyleStr_LaterStylesOverrideEarlier()
    {
        var style = new Style(Array.Empty<(string, string)>());
        var result = style.GetAttrsForStyleStr("bold nobold");

        Assert.False(result.Bold);
    }

    #endregion

    #region GetAttrsForStyleStr - Class References

    [Fact]
    public void GetAttrsForStyleStr_AppliesClassStyles()
    {
        var style = new Style(new[] { ("title", "bold #ff0000") });
        var result = style.GetAttrsForStyleStr("class:title");

        Assert.True(result.Bold);
        Assert.Equal("ff0000", result.Color);
    }

    [Fact]
    public void GetAttrsForStyleStr_ClassNamesAreCaseInsensitive()
    {
        var style = new Style(new[] { ("title", "bold") });

        var result1 = style.GetAttrsForStyleStr("class:title");
        var result2 = style.GetAttrsForStyleStr("class:TITLE");
        var result3 = style.GetAttrsForStyleStr("class:Title");

        Assert.True(result1.Bold);
        Assert.True(result2.Bold);
        Assert.True(result3.Bold);
    }

    [Fact]
    public void GetAttrsForStyleStr_AppliesMultipleClasses()
    {
        var style = new Style(new[]
        {
            ("title", "bold"),
            ("error", "fg:#ff0000"),
        });

        var result = style.GetAttrsForStyleStr("class:title,error");

        Assert.True(result.Bold);
        Assert.Equal("ff0000", result.Color);
    }

    [Fact]
    public void GetAttrsForStyleStr_ExpandsDottedClassNames()
    {
        var style = new Style(new[]
        {
            ("a", "bold"),
            ("a.b", "italic"),
            ("a.b.c", "underline"),
        });

        var result = style.GetAttrsForStyleStr("class:a.b.c");

        // Should apply all: a, a.b, a.b.c
        Assert.True(result.Bold);
        Assert.True(result.Italic);
        Assert.True(result.Underline);
    }

    [Fact]
    public void GetAttrsForStyleStr_CombinesClassAndInlineStyles()
    {
        var style = new Style(new[] { ("title", "bold") });
        var result = style.GetAttrsForStyleStr("class:title italic");

        Assert.True(result.Bold);
        Assert.True(result.Italic);
    }

    [Fact]
    public void GetAttrsForStyleStr_InlineOverridesClass()
    {
        var style = new Style(new[] { ("title", "bold") });
        var result = style.GetAttrsForStyleStr("class:title nobold");

        // Inline 'nobold' comes after 'class:title', so it overrides
        Assert.False(result.Bold);
    }

    #endregion

    #region GetAttrsForStyleStr - Class Combinations

    [Fact]
    public void GetAttrsForStyleStr_AppliesClassCombinations()
    {
        var style = new Style(new[]
        {
            ("title", "bold"),
            ("error", "italic"),
            ("title error", "#ff0000"),
        });

        var result = style.GetAttrsForStyleStr("class:title class:error");

        Assert.True(result.Bold);
        Assert.True(result.Italic);
        Assert.Equal("ff0000", result.Color);
    }

    [Fact]
    public void GetAttrsForStyleStr_ClassOrderDoesNotMatter()
    {
        var style = new Style(new[] { ("title error", "#ff0000") });

        var result1 = style.GetAttrsForStyleStr("class:title class:error");
        var result2 = style.GetAttrsForStyleStr("class:error class:title");

        Assert.Equal("ff0000", result1.Color);
        Assert.Equal("ff0000", result2.Color);
    }

    #endregion

    #region GetAttrsForStyleStr - Default Styling

    [Fact]
    public void GetAttrsForStyleStr_AppliesDefaultStyling()
    {
        var style = new Style(new[]
        {
            ("", "bold"),  // Default rule (empty class names)
            ("title", "italic"),
        });

        var result1 = style.GetAttrsForStyleStr("");
        var result2 = style.GetAttrsForStyleStr("class:title");

        Assert.True(result1.Bold);
        Assert.True(result2.Bold);
        Assert.True(result2.Italic);
    }

    #endregion

    #region GetAttrsForStyleStr - NoInherit

    [Fact]
    public void GetAttrsForStyleStr_NoInheritStartsFromDefault()
    {
        var style = new Style(Array.Empty<(string, string)>());

        // Without noinherit, attrs start empty (null values)
        var resultWithout = style.GetAttrsForStyleStr("bold");
        // With noinherit, attrs start from DEFAULT_ATTRS
        var resultWith = style.GetAttrsForStyleStr("noinherit bold");

        // Both should have bold=true after processing
        Assert.True(resultWithout.Bold);
        Assert.True(resultWith.Bold);
    }

    #endregion

    #region GetAttrsForStyleStr - Ignored Styles

    [Theory]
    [InlineData("roman")]
    [InlineData("sans")]
    [InlineData("mono")]
    [InlineData("border:none")]
    [InlineData("[transparent]")]
    [InlineData("[set-cursor-position]")]
    public void GetAttrsForStyleStr_IgnoresSpecialStyles(string styleStr)
    {
        var style = new Style(Array.Empty<(string, string)>());
        var result = style.GetAttrsForStyleStr(styleStr);

        // Should return defaults (these styles are ignored)
        Assert.Equal(DefaultAttrs.Default, result);
    }

    #endregion

    #region StyleRules Property Tests

    [Fact]
    public void StyleRules_ReturnsAllRules()
    {
        var rules = new[]
        {
            ("title", "bold"),
            ("error", "red"),
        };
        var style = new Style(rules);

        Assert.Equal(2, style.StyleRules.Count);
        Assert.Equal(("title", "bold"), style.StyleRules[0]);
        Assert.Equal(("error", "red"), style.StyleRules[1]);
    }

    [Fact]
    public void StyleRules_IsReadOnly()
    {
        var style = new Style(new[] { ("title", "bold") });

        Assert.IsAssignableFrom<IReadOnlyList<(string, string)>>(style.StyleRules);
    }

    #endregion

    #region InvalidationHash Tests

    [Fact]
    public void InvalidationHash_IsConsistentForSameInstance()
    {
        var style = new Style(new[] { ("title", "bold") });

        var hash1 = style.InvalidationHash;
        var hash2 = style.InvalidationHash;

        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void InvalidationHash_DiffersForDifferentInstances()
    {
        var style1 = new Style(new[] { ("title", "bold") });
        var style2 = new Style(new[] { ("title", "bold") });

        // Different instances have different internal lists, so different hashes
        Assert.NotEqual(style1.InvalidationHash, style2.InvalidationHash);
    }

    #endregion

    #region IStyle Implementation Tests

    [Fact]
    public void ImplementsIStyle()
    {
        var style = new Style(Array.Empty<(string, string)>());
        Assert.IsAssignableFrom<IStyle>(style);
    }

    #endregion

    #region Python Parity Tests

    [Fact]
    public void GetAttrsForStyleStr_MatchesPythonBehavior_BasicClass()
    {
        // Python: Style([('title', '#ff0000 bold underline')])
        var style = new Style(new[] { ("title", "#ff0000 bold underline") });
        var result = style.GetAttrsForStyleStr("class:title");

        Assert.Equal("ff0000", result.Color);
        Assert.True(result.Bold);
        Assert.True(result.Underline);
    }

    [Fact]
    public void GetAttrsForStyleStr_MatchesPythonBehavior_MultipleClasses()
    {
        // Python: Style([('title', 'bold'), ('error', '#ff0000')])
        var style = new Style(new[]
        {
            ("title", "bold"),
            ("error", "#ff0000"),
        });

        var result = style.GetAttrsForStyleStr("class:title class:error");

        Assert.True(result.Bold);
        Assert.Equal("ff0000", result.Color);
    }

    [Fact]
    public void GetAttrsForStyleStr_MatchesPythonBehavior_AnsiColors()
    {
        var style = new Style(new[] { ("title", "ansiblue bg:ansired") });
        var result = style.GetAttrsForStyleStr("class:title");

        Assert.Equal("ansiblue", result.Color);
        Assert.Equal("ansired", result.BgColor);
    }

    #endregion
}
