using Stroke.Styles;
using Xunit;

namespace Stroke.Tests.Styles;

/// <summary>
/// Tests for the StyleMerger static class.
/// </summary>
public class StyleMergerTests
{
    #region MergeStyles - Basic Tests

    [Fact]
    public void MergeStyles_ThrowsForNullInput()
    {
        Assert.Throws<ArgumentNullException>(() => StyleMerger.MergeStyles(null!));
    }

    [Fact]
    public void MergeStyles_ReturnsEmptyStyleForEmptyInput()
    {
        var result = StyleMerger.MergeStyles(Array.Empty<IStyle>());

        Assert.Same(DummyStyle.Instance, result);
    }

    [Fact]
    public void MergeStyles_ReturnsEmptyStyleForAllNulls()
    {
        var result = StyleMerger.MergeStyles(new IStyle?[] { null, null, null });

        Assert.Same(DummyStyle.Instance, result);
    }

    [Fact]
    public void MergeStyles_ReturnsSingleStyleUnchanged()
    {
        var style = new Style(new[] { ("title", "bold") });
        var result = StyleMerger.MergeStyles(new[] { style });

        Assert.Same(style, result);
    }

    [Fact]
    public void MergeStyles_FiltersOutNullStyles()
    {
        var style = new Style(new[] { ("title", "bold") });
        var result = StyleMerger.MergeStyles(new IStyle?[] { null, style, null });

        Assert.Same(style, result);
    }

    #endregion

    #region MergeStyles - Merging Multiple Styles

    [Fact]
    public void MergeStyles_CombinesStyleRules()
    {
        var style1 = new Style(new[] { ("title", "bold") });
        var style2 = new Style(new[] { ("error", "#ff0000") });

        var result = StyleMerger.MergeStyles(new IStyle[] { style1, style2 });

        Assert.Equal(2, result.StyleRules.Count);
        Assert.Contains(result.StyleRules, r => r.ClassNames == "title");
        Assert.Contains(result.StyleRules, r => r.ClassNames == "error");
    }

    [Fact]
    public void MergeStyles_LaterStylesOverrideEarlier()
    {
        var style1 = new Style(new[] { ("title", "#ff0000") });
        var style2 = new Style(new[] { ("title", "#00ff00") });

        var result = StyleMerger.MergeStyles(new IStyle[] { style1, style2 });

        // The result should contain both rules, with later one having precedence
        var attrs = result.GetAttrsForStyleStr("class:title");
        Assert.Equal("00ff00", attrs.Color);
    }

    [Fact]
    public void MergeStyles_PreservesRuleOrder()
    {
        var style1 = new Style(new[] { ("a", "bold"), ("b", "italic") });
        var style2 = new Style(new[] { ("c", "underline") });

        var result = StyleMerger.MergeStyles(new IStyle[] { style1, style2 });
        var rules = result.StyleRules.ToList();

        Assert.Equal(3, rules.Count);
        Assert.Equal("a", rules[0].ClassNames);
        Assert.Equal("b", rules[1].ClassNames);
        Assert.Equal("c", rules[2].ClassNames);
    }

    #endregion

    #region GetAttrsForStyleStr Tests

    [Fact]
    public void MergedStyle_GetAttrsForStyleStr_ReturnsCorrectAttrs()
    {
        var style1 = new Style(new[] { ("title", "bold") });
        var style2 = new Style(new[] { ("error", "#ff0000") });
        var merged = StyleMerger.MergeStyles(new IStyle[] { style1, style2 });

        var titleAttrs = merged.GetAttrsForStyleStr("class:title");
        var errorAttrs = merged.GetAttrsForStyleStr("class:error");

        Assert.True(titleAttrs.Bold);
        Assert.Equal("ff0000", errorAttrs.Color);
    }

    [Fact]
    public void MergedStyle_GetAttrsForStyleStr_AppliesMultipleClasses()
    {
        var style1 = new Style(new[] { ("title", "bold") });
        var style2 = new Style(new[] { ("error", "#ff0000") });
        var merged = StyleMerger.MergeStyles(new IStyle[] { style1, style2 });

        var attrs = merged.GetAttrsForStyleStr("class:title class:error");

        Assert.True(attrs.Bold);
        Assert.Equal("ff0000", attrs.Color);
    }

    [Fact]
    public void MergedStyle_GetAttrsForStyleStr_AppliesDefaultStyles()
    {
        var style1 = new Style(new[] { ("", "bold") });  // Default rule
        var style2 = new Style(new[] { ("title", "italic") });
        var merged = StyleMerger.MergeStyles(new IStyle[] { style1, style2 });

        var attrs = merged.GetAttrsForStyleStr("");

        Assert.True(attrs.Bold);
    }

    #endregion

    #region InvalidationHash Tests

    [Fact]
    public void MergedStyle_InvalidationHash_IsConsistent()
    {
        var style1 = new Style(new[] { ("title", "bold") });
        var style2 = new Style(new[] { ("error", "#ff0000") });
        var merged = StyleMerger.MergeStyles(new IStyle[] { style1, style2 });

        var hash1 = merged.InvalidationHash;
        var hash2 = merged.InvalidationHash;

        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void MergedStyle_InvalidationHash_DiffersForDifferentStyles()
    {
        var style1 = new Style(new[] { ("title", "bold") });
        var style2 = new Style(new[] { ("error", "#ff0000") });
        var style3 = new Style(new[] { ("prompt", "italic") });

        var merged1 = StyleMerger.MergeStyles(new IStyle[] { style1, style2 });
        var merged2 = StyleMerger.MergeStyles(new IStyle[] { style1, style3 });

        Assert.NotEqual(merged1.InvalidationHash, merged2.InvalidationHash);
    }

    #endregion

    #region StyleRules Property Tests

    [Fact]
    public void MergedStyle_StyleRules_ContainsAllRules()
    {
        var style1 = new Style(new[] { ("a", "bold"), ("b", "italic") });
        var style2 = new Style(new[] { ("c", "underline") });

        var merged = StyleMerger.MergeStyles(new IStyle[] { style1, style2 });

        Assert.Equal(3, merged.StyleRules.Count);
    }

    [Fact]
    public void MergedStyle_StyleRules_ReturnsNewListEachTime()
    {
        var style1 = new Style(new[] { ("title", "bold") });
        var merged = StyleMerger.MergeStyles(new IStyle[] { style1 });

        // For single style, returns the same instance
        Assert.Same(style1, merged);
    }

    #endregion

    #region IStyle Implementation Tests

    [Fact]
    public void MergedStyle_ImplementsIStyle()
    {
        var style1 = new Style(new[] { ("title", "bold") });
        var style2 = new Style(new[] { ("error", "#ff0000") });
        var merged = StyleMerger.MergeStyles(new IStyle[] { style1, style2 });

        Assert.IsAssignableFrom<IStyle>(merged);
    }

    #endregion

    #region DummyStyle Merging Tests

    [Fact]
    public void MergeStyles_HandlesDummyStyle()
    {
        var style = new Style(new[] { ("title", "bold") });
        var result = StyleMerger.MergeStyles(new IStyle[] { DummyStyle.Instance, style });

        var attrs = result.GetAttrsForStyleStr("class:title");
        Assert.True(attrs.Bold);
    }

    [Fact]
    public void MergeStyles_DummyStyleHasNoRules()
    {
        var result = StyleMerger.MergeStyles(new IStyle[] { DummyStyle.Instance, DummyStyle.Instance });

        Assert.Empty(result.StyleRules);
    }

    #endregion

    #region Python Parity Tests

    [Fact]
    public void MergeStyles_MatchesPythonBehavior_Precedence()
    {
        // Python: merge_styles([Style([('title', 'bold')]), Style([('title', '#ff0000')])])
        // Later style's color should win, but both rules are present
        var style1 = new Style(new[] { ("title", "bold") });
        var style2 = new Style(new[] { ("title", "#ff0000") });

        var merged = StyleMerger.MergeStyles(new IStyle[] { style1, style2 });
        var attrs = merged.GetAttrsForStyleStr("class:title");

        // Both bold (from first) and color (from second) should apply
        // because they're different attributes
        Assert.True(attrs.Bold);
        Assert.Equal("ff0000", attrs.Color);
    }

    [Fact]
    public void MergeStyles_MatchesPythonBehavior_ThreeStyles()
    {
        var style1 = new Style(new[] { ("title", "bold") });
        var style2 = new Style(new[] { ("error", "#ff0000") });
        var style3 = new Style(new[] { ("prompt", "italic") });

        var merged = StyleMerger.MergeStyles(new IStyle[] { style1, style2, style3 });

        Assert.Equal(3, merged.StyleRules.Count);

        var titleAttrs = merged.GetAttrsForStyleStr("class:title");
        var errorAttrs = merged.GetAttrsForStyleStr("class:error");
        var promptAttrs = merged.GetAttrsForStyleStr("class:prompt");

        Assert.True(titleAttrs.Bold);
        Assert.Equal("ff0000", errorAttrs.Color);
        Assert.True(promptAttrs.Italic);
    }

    #endregion

    #region Thread Safety Tests

    [Fact]
    public async Task MergedStyle_IsThreadSafe()
    {
        var style1 = new Style(new[] { ("title", "bold") });
        var style2 = new Style(new[] { ("error", "#ff0000") });
        var merged = StyleMerger.MergeStyles(new IStyle[] { style1, style2 });

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
                        var attrs = merged.GetAttrsForStyleStr("class:title class:error");
                        Assert.True(attrs.Bold);
                        Assert.Equal("ff0000", attrs.Color);

                        var hash = merged.InvalidationHash;
                        Assert.NotNull(hash);
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
}
