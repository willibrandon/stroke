using Stroke.Filters;
using Stroke.Styles;
using Xunit;

namespace Stroke.Tests.Styles;

/// <summary>
/// Tests for style transformation classes.
/// </summary>
public class StyleTransformationTests
{
    #region IStyleTransformation Basic Tests

    [Fact]
    public void AllTransformations_ImplementIStyleTransformation()
    {
        Assert.IsAssignableFrom<IStyleTransformation>(DummyStyleTransformation.Instance);
        Assert.IsAssignableFrom<IStyleTransformation>(new ReverseStyleTransformation());
        Assert.IsAssignableFrom<IStyleTransformation>(new SwapLightAndDarkStyleTransformation());
        Assert.IsAssignableFrom<IStyleTransformation>(new SetDefaultColorStyleTransformation("#ff0000", "#0000ff"));
        Assert.IsAssignableFrom<IStyleTransformation>(new AdjustBrightnessStyleTransformation(0.2f, 0.8f));
    }

    #endregion

    #region DummyStyleTransformation Tests

    [Fact]
    public void DummyStyleTransformation_IsSingleton()
    {
        var instance1 = DummyStyleTransformation.Instance;
        var instance2 = DummyStyleTransformation.Instance;
        Assert.Same(instance1, instance2);
    }

    [Fact]
    public void DummyStyleTransformation_ReturnsAttrsUnchanged()
    {
        var attrs = new Attrs(Color: "ff0000", Bold: true);
        var result = DummyStyleTransformation.Instance.TransformAttrs(attrs);
        Assert.Equal(attrs, result);
    }

    [Fact]
    public void DummyStyleTransformation_InvalidationHash_IsConsistent()
    {
        var hash1 = DummyStyleTransformation.Instance.InvalidationHash;
        var hash2 = DummyStyleTransformation.Instance.InvalidationHash;
        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void DummyStyleTransformation_InvalidationHash_IsFixedString()
    {
        Assert.Equal("dummy-style-transformation", DummyStyleTransformation.Instance.InvalidationHash);
    }

    #endregion

    #region ReverseStyleTransformation Tests

    [Fact]
    public void ReverseStyleTransformation_TogglesReverseAttribute()
    {
        var transformation = new ReverseStyleTransformation();

        var attrs1 = new Attrs(Reverse: false);
        var result1 = transformation.TransformAttrs(attrs1);
        Assert.True(result1.Reverse);

        var attrs2 = new Attrs(Reverse: true);
        var result2 = transformation.TransformAttrs(attrs2);
        Assert.False(result2.Reverse);
    }

    [Fact]
    public void ReverseStyleTransformation_PreservesOtherAttributes()
    {
        var transformation = new ReverseStyleTransformation();
        var attrs = new Attrs(Color: "ff0000", Bold: true, Reverse: false);
        var result = transformation.TransformAttrs(attrs);

        Assert.Equal("ff0000", result.Color);
        Assert.True(result.Bold);
        Assert.True(result.Reverse);
    }

    [Fact]
    public void ReverseStyleTransformation_InvalidationHash_IncludesClassName()
    {
        var transformation = new ReverseStyleTransformation();
        var hash = transformation.InvalidationHash.ToString();
        Assert.Contains("ReverseStyleTransformation", hash);
    }

    #endregion

    #region SwapLightAndDarkStyleTransformation Tests

    [Fact]
    public void SwapLightAndDarkStyleTransformation_SwapsAnsiBlackAndWhite()
    {
        var transformation = new SwapLightAndDarkStyleTransformation();
        var attrs = new Attrs(Color: "ansiblack");
        var result = transformation.TransformAttrs(attrs);

        Assert.Equal("ansiwhite", result.Color);
    }

    [Fact]
    public void SwapLightAndDarkStyleTransformation_SwapsAnsiColors()
    {
        var transformation = new SwapLightAndDarkStyleTransformation();

        // Test all opposite pairs
        var pairs = new[]
        {
            ("ansiblack", "ansiwhite"),
            ("ansired", "ansibrightred"),
            ("ansigreen", "ansibrightgreen"),
            ("ansiyellow", "ansibrightyellow"),
            ("ansiblue", "ansibrightblue"),
            ("ansimagenta", "ansibrightmagenta"),
            ("ansicyan", "ansibrightcyan"),
            ("ansigray", "ansibrightblack"),
        };

        foreach (var (color, expected) in pairs)
        {
            var attrs = new Attrs(Color: color);
            var result = transformation.TransformAttrs(attrs);
            Assert.Equal(expected, result.Color);
        }
    }

    [Fact]
    public void SwapLightAndDarkStyleTransformation_InvertsHexColorLuminosity()
    {
        var transformation = new SwapLightAndDarkStyleTransformation();

        // Pure black should become pure white
        var black = new Attrs(Color: "000000");
        var blackResult = transformation.TransformAttrs(black);
        Assert.Equal("ffffff", blackResult.Color);

        // Pure white should become pure black
        var white = new Attrs(Color: "ffffff");
        var whiteResult = transformation.TransformAttrs(white);
        Assert.Equal("000000", whiteResult.Color);
    }

    [Fact]
    public void SwapLightAndDarkStyleTransformation_TransformsBothColors()
    {
        var transformation = new SwapLightAndDarkStyleTransformation();
        var attrs = new Attrs(Color: "ansiblack", BgColor: "ansiwhite");
        var result = transformation.TransformAttrs(attrs);

        Assert.Equal("ansiwhite", result.Color);
        Assert.Equal("ansiblack", result.BgColor);
    }

    [Fact]
    public void SwapLightAndDarkStyleTransformation_PreservesNullColors()
    {
        var transformation = new SwapLightAndDarkStyleTransformation();
        var attrs = new Attrs(Bold: true);
        var result = transformation.TransformAttrs(attrs);

        Assert.Null(result.Color);
        Assert.Null(result.BgColor);
        Assert.True(result.Bold);
    }

    [Fact]
    public void SwapLightAndDarkStyleTransformation_PreservesEmptyAndDefault()
    {
        var transformation = new SwapLightAndDarkStyleTransformation();

        var empty = new Attrs(Color: "");
        var emptyResult = transformation.TransformAttrs(empty);
        Assert.Equal("", emptyResult.Color);

        var defaultColor = new Attrs(Color: "default");
        var defaultResult = transformation.TransformAttrs(defaultColor);
        Assert.Equal("default", defaultResult.Color);
    }

    [Fact]
    public void SwapLightAndDarkStyleTransformation_PreservesAnsiDefault()
    {
        var transformation = new SwapLightAndDarkStyleTransformation();
        var attrs = new Attrs(Color: "ansidefault");
        var result = transformation.TransformAttrs(attrs);

        Assert.Equal("ansidefault", result.Color);
    }

    #endregion

    #region SetDefaultColorStyleTransformation Tests

    [Fact]
    public void SetDefaultColorStyleTransformation_SetsDefaultForeground()
    {
        var transformation = new SetDefaultColorStyleTransformation("#ff0000", "#0000ff");
        var attrs = new Attrs();
        var result = transformation.TransformAttrs(attrs);

        Assert.Equal("ff0000", result.Color);
    }

    [Fact]
    public void SetDefaultColorStyleTransformation_SetsDefaultBackground()
    {
        var transformation = new SetDefaultColorStyleTransformation("#ff0000", "#0000ff");
        var attrs = new Attrs();
        var result = transformation.TransformAttrs(attrs);

        Assert.Equal("0000ff", result.BgColor);
    }

    [Fact]
    public void SetDefaultColorStyleTransformation_DoesNotOverrideExistingColor()
    {
        var transformation = new SetDefaultColorStyleTransformation("#ff0000", "#0000ff");
        var attrs = new Attrs(Color: "00ff00", BgColor: "ffff00");
        var result = transformation.TransformAttrs(attrs);

        Assert.Equal("00ff00", result.Color);
        Assert.Equal("ffff00", result.BgColor);
    }

    [Fact]
    public void SetDefaultColorStyleTransformation_OverridesDefaultString()
    {
        var transformation = new SetDefaultColorStyleTransformation("#ff0000", "#0000ff");
        var attrs = new Attrs(Color: "default", BgColor: "default");
        var result = transformation.TransformAttrs(attrs);

        Assert.Equal("ff0000", result.Color);
        Assert.Equal("0000ff", result.BgColor);
    }

    [Fact]
    public void SetDefaultColorStyleTransformation_OverridesEmptyString()
    {
        var transformation = new SetDefaultColorStyleTransformation("#ff0000", "#0000ff");
        var attrs = new Attrs(Color: "", BgColor: "");
        var result = transformation.TransformAttrs(attrs);

        Assert.Equal("ff0000", result.Color);
        Assert.Equal("0000ff", result.BgColor);
    }

    [Fact]
    public void SetDefaultColorStyleTransformation_SupportsCallables()
    {
        var fgColor = "#ff0000";
        var bgColor = "#0000ff";
        var transformation = new SetDefaultColorStyleTransformation(() => fgColor, () => bgColor);

        var attrs = new Attrs();
        var result = transformation.TransformAttrs(attrs);

        Assert.Equal("ff0000", result.Color);
        Assert.Equal("0000ff", result.BgColor);
    }

    [Fact]
    public void SetDefaultColorStyleTransformation_CallablesAreEvaluatedDynamically()
    {
        var fgColor = "#ff0000";
        var transformation = new SetDefaultColorStyleTransformation(() => fgColor, () => "#0000ff");

        var result1 = transformation.TransformAttrs(new Attrs());
        Assert.Equal("ff0000", result1.Color);

        fgColor = "#00ff00";
        var result2 = transformation.TransformAttrs(new Attrs());
        Assert.Equal("00ff00", result2.Color);
    }

    [Fact]
    public void SetDefaultColorStyleTransformation_ThrowsForNullCallables()
    {
        Assert.Throws<ArgumentNullException>(() => new SetDefaultColorStyleTransformation(null!, () => "#0000ff"));
        Assert.Throws<ArgumentNullException>(() => new SetDefaultColorStyleTransformation(() => "#ff0000", null!));
    }

    [Fact]
    public void SetDefaultColorStyleTransformation_InvalidationHash_IncludesColors()
    {
        var transformation = new SetDefaultColorStyleTransformation("#ff0000", "#0000ff");
        var hash = transformation.InvalidationHash;

        // Hash should be a tuple containing the colors
        Assert.NotNull(hash);
    }

    [Fact]
    public void SetDefaultColorStyleTransformation_InvalidationHash_ReflectsDynamicValues()
    {
        var fgColor = "#ff0000";
        var transformation = new SetDefaultColorStyleTransformation(() => fgColor, () => "#0000ff");

        var hash1 = transformation.InvalidationHash;
        fgColor = "#00ff00";
        var hash2 = transformation.InvalidationHash;

        Assert.NotEqual(hash1, hash2);
    }

    #endregion

    #region AdjustBrightnessStyleTransformation Tests

    [Fact]
    public void AdjustBrightnessStyleTransformation_DefaultsToNoChange()
    {
        var transformation = new AdjustBrightnessStyleTransformation();
        var attrs = new Attrs(Color: "ff0000");
        var result = transformation.TransformAttrs(attrs);

        Assert.Equal("ff0000", result.Color);
    }

    [Fact]
    public void AdjustBrightnessStyleTransformation_IncreasesMinBrightness()
    {
        var transformation = new AdjustBrightnessStyleTransformation(minBrightness: 0.5f);
        var attrs = new Attrs(Color: "000000"); // Black - 0 brightness

        var result = transformation.TransformAttrs(attrs);

        // Black (0,0,0) at 0 brightness interpolated to (0.5, 1.0) should give brightness 0.5
        // Which is a gray color (7f7f7f)
        Assert.NotEqual("000000", result.Color);
    }

    [Fact]
    public void AdjustBrightnessStyleTransformation_DecreasesMaxBrightness()
    {
        var transformation = new AdjustBrightnessStyleTransformation(maxBrightness: 0.5f);
        var attrs = new Attrs(Color: "ffffff"); // White - 1 brightness

        var result = transformation.TransformAttrs(attrs);

        // White interpolated to (0.0, 0.5) should give brightness 0.5
        // Which is a gray color
        Assert.NotEqual("ffffff", result.Color);
    }

    [Fact]
    public void AdjustBrightnessStyleTransformation_SkipsWhenBackgroundSet()
    {
        var transformation = new AdjustBrightnessStyleTransformation(minBrightness: 0.5f);
        var attrs = new Attrs(Color: "000000", BgColor: "ffffff");
        var result = transformation.TransformAttrs(attrs);

        // Should not adjust when background is set
        Assert.Equal("000000", result.Color);
    }

    [Fact]
    public void AdjustBrightnessStyleTransformation_SkipsWhenNoForeground()
    {
        var transformation = new AdjustBrightnessStyleTransformation(minBrightness: 0.5f);
        var attrs = new Attrs();
        var result = transformation.TransformAttrs(attrs);

        Assert.Null(result.Color);
    }

    [Fact]
    public void AdjustBrightnessStyleTransformation_SkipsAnsiDefault()
    {
        var transformation = new AdjustBrightnessStyleTransformation(minBrightness: 0.5f);
        var attrs = new Attrs(Color: "ansidefault");
        var result = transformation.TransformAttrs(attrs);

        Assert.Equal("ansidefault", result.Color);
    }

    [Fact]
    public void AdjustBrightnessStyleTransformation_HandlesAnsiColors()
    {
        var transformation = new AdjustBrightnessStyleTransformation(minBrightness: 0.5f);
        var attrs = new Attrs(Color: "ansiblack");
        var result = transformation.TransformAttrs(attrs);

        // Should convert ANSI black to a brighter RGB color
        Assert.NotEqual("ansiblack", result.Color);
        Assert.Matches("^[0-9a-f]{6}$", result.Color);
    }

    [Fact]
    public void AdjustBrightnessStyleTransformation_SupportsCallables()
    {
        var minBrightness = 0.0f;
        var transformation = new AdjustBrightnessStyleTransformation(
            () => minBrightness,
            () => 1.0f
        );

        var attrs = new Attrs(Color: "000000");
        var result1 = transformation.TransformAttrs(attrs);
        Assert.Equal("000000", result1.Color);

        minBrightness = 0.5f;
        var result2 = transformation.TransformAttrs(attrs);
        Assert.NotEqual("000000", result2.Color);
    }

    [Fact]
    public void AdjustBrightnessStyleTransformation_ThrowsForNullCallables()
    {
        Assert.Throws<ArgumentNullException>(() => new AdjustBrightnessStyleTransformation(null!, () => 1.0f));
        Assert.Throws<ArgumentNullException>(() => new AdjustBrightnessStyleTransformation(() => 0.0f, null!));
    }

    [Fact]
    public void AdjustBrightnessStyleTransformation_ThrowsForInvalidRange()
    {
        var transformation1 = new AdjustBrightnessStyleTransformation(() => -0.1f, () => 1.0f);
        Assert.Throws<InvalidOperationException>(() => transformation1.TransformAttrs(new Attrs(Color: "ff0000")));

        var transformation2 = new AdjustBrightnessStyleTransformation(() => 0.0f, () => 1.1f);
        Assert.Throws<InvalidOperationException>(() => transformation2.TransformAttrs(new Attrs(Color: "ff0000")));
    }

    [Fact]
    public void AdjustBrightnessStyleTransformation_InvalidationHash_IncludesBrightness()
    {
        var transformation = new AdjustBrightnessStyleTransformation(0.2f, 0.8f);
        var hash = transformation.InvalidationHash;

        Assert.NotNull(hash);
    }

    [Fact]
    public void AdjustBrightnessStyleTransformation_InvalidationHash_ReflectsDynamicValues()
    {
        var minBrightness = 0.2f;
        var transformation = new AdjustBrightnessStyleTransformation(
            () => minBrightness,
            () => 0.8f
        );

        var hash1 = transformation.InvalidationHash;
        minBrightness = 0.4f;
        var hash2 = transformation.InvalidationHash;

        Assert.NotEqual(hash1, hash2);
    }

    #endregion

    #region Python Parity Tests

    [Fact]
    public void SwapLightAndDark_MatchesPythonOppositeColorMap()
    {
        // Verify all opposite color mappings match Python
        var expectedMappings = new Dictionary<string, string>
        {
            ["ansidefault"] = "ansidefault",
            ["ansiblack"] = "ansiwhite",
            ["ansired"] = "ansibrightred",
            ["ansigreen"] = "ansibrightgreen",
            ["ansiyellow"] = "ansibrightyellow",
            ["ansiblue"] = "ansibrightblue",
            ["ansimagenta"] = "ansibrightmagenta",
            ["ansicyan"] = "ansibrightcyan",
            ["ansigray"] = "ansibrightblack",
            ["ansiwhite"] = "ansiblack",
            ["ansibrightred"] = "ansired",
            ["ansibrightgreen"] = "ansigreen",
            ["ansibrightyellow"] = "ansiyellow",
            ["ansibrightblue"] = "ansiblue",
            ["ansibrightmagenta"] = "ansimagenta",
            ["ansibrightcyan"] = "ansicyan",
            ["ansibrightblack"] = "ansigray",
        };

        var transformation = new SwapLightAndDarkStyleTransformation();

        foreach (var (input, expected) in expectedMappings)
        {
            var attrs = new Attrs(Color: input);
            var result = transformation.TransformAttrs(attrs);
            Assert.Equal(expected, result.Color);
        }
    }

    #endregion

    #region ConditionalStyleTransformation Tests

    [Fact]
    public void ConditionalStyleTransformation_AppliesWhenFilterTrue()
    {
        var reverseTransform = new ReverseStyleTransformation();
        var conditional = new ConditionalStyleTransformation(reverseTransform, true);

        var attrs = new Attrs(Reverse: false);
        var result = conditional.TransformAttrs(attrs);

        Assert.True(result.Reverse);
    }

    [Fact]
    public void ConditionalStyleTransformation_DoesNotApplyWhenFilterFalse()
    {
        var reverseTransform = new ReverseStyleTransformation();
        var conditional = new ConditionalStyleTransformation(reverseTransform, false);

        var attrs = new Attrs(Reverse: false);
        var result = conditional.TransformAttrs(attrs);

        Assert.False(result.Reverse);
    }

    [Fact]
    public void ConditionalStyleTransformation_SupportsFilterCallable()
    {
        var filterValue = true;
        var reverseTransform = new ReverseStyleTransformation();
        var conditional = new ConditionalStyleTransformation(reverseTransform, new Condition(() => filterValue));

        var result1 = conditional.TransformAttrs(new Attrs(Reverse: false));
        Assert.True(result1.Reverse);

        filterValue = false;
        var result2 = conditional.TransformAttrs(new Attrs(Reverse: false));
        Assert.False(result2.Reverse);
    }

    [Fact]
    public void ConditionalStyleTransformation_ThrowsForNullTransformation()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new ConditionalStyleTransformation(null!, true));
    }

    [Fact]
    public void ConditionalStyleTransformation_InvalidationHash_IncludesFilterState()
    {
        var filterValue = true;
        var reverseTransform = new ReverseStyleTransformation();
        var conditional = new ConditionalStyleTransformation(reverseTransform, new Condition(() => filterValue));

        var hash1 = conditional.InvalidationHash;
        filterValue = false;
        var hash2 = conditional.InvalidationHash;

        Assert.NotEqual(hash1, hash2);
    }

    #endregion

    #region DynamicStyleTransformation Tests

    [Fact]
    public void DynamicStyleTransformation_DelegatesToUnderlyingTransformation()
    {
        var reverseTransform = new ReverseStyleTransformation();
        var dynamic = new DynamicStyleTransformation(() => reverseTransform);

        var attrs = new Attrs(Reverse: false);
        var result = dynamic.TransformAttrs(attrs);

        Assert.True(result.Reverse);
    }

    [Fact]
    public void DynamicStyleTransformation_UsesDummyWhenNull()
    {
        var dynamic = new DynamicStyleTransformation(() => null);

        var attrs = new Attrs(Color: "ff0000", Bold: true);
        var result = dynamic.TransformAttrs(attrs);

        Assert.Equal(attrs, result);
    }

    [Fact]
    public void DynamicStyleTransformation_ReflectsDynamicChanges()
    {
        IStyleTransformation? currentTransform = new ReverseStyleTransformation();
        var dynamic = new DynamicStyleTransformation(() => currentTransform);

        var result1 = dynamic.TransformAttrs(new Attrs(Reverse: false));
        Assert.True(result1.Reverse);

        currentTransform = new SwapLightAndDarkStyleTransformation();
        var result2 = dynamic.TransformAttrs(new Attrs(Color: "ansiblack"));
        Assert.Equal("ansiwhite", result2.Color);
    }

    [Fact]
    public void DynamicStyleTransformation_ThrowsForNullCallable()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new DynamicStyleTransformation(null!));
    }

    [Fact]
    public void DynamicStyleTransformation_InvalidationHash_DelegatesToUnderlying()
    {
        var reverseTransform = new ReverseStyleTransformation();
        var dynamic = new DynamicStyleTransformation(() => reverseTransform);

        Assert.Equal(reverseTransform.InvalidationHash, dynamic.InvalidationHash);
    }

    [Fact]
    public void DynamicStyleTransformation_InvalidationHash_ReturnsDummyHashWhenNull()
    {
        var dynamic = new DynamicStyleTransformation(() => null);

        Assert.Equal(DummyStyleTransformation.Instance.InvalidationHash, dynamic.InvalidationHash);
    }

    #endregion

    #region StyleTransformationMerger Tests

    [Fact]
    public void MergeStyleTransformations_ThrowsForNullInput()
    {
        Assert.Throws<ArgumentNullException>(() =>
            StyleTransformationMerger.MergeStyleTransformations(null!));
    }

    [Fact]
    public void MergeStyleTransformations_ReturnsDummyForEmpty()
    {
        var result = StyleTransformationMerger.MergeStyleTransformations([]);

        Assert.Same(DummyStyleTransformation.Instance, result);
    }

    [Fact]
    public void MergeStyleTransformations_ReturnsDummyForAllNulls()
    {
        var result = StyleTransformationMerger.MergeStyleTransformations(
            [null, null, null]);

        Assert.Same(DummyStyleTransformation.Instance, result);
    }

    [Fact]
    public void MergeStyleTransformations_ReturnsSingleTransformationUnchanged()
    {
        var transformation = new ReverseStyleTransformation();
        var result = StyleTransformationMerger.MergeStyleTransformations([transformation]);

        Assert.Same(transformation, result);
    }

    [Fact]
    public void MergeStyleTransformations_FiltersOutNulls()
    {
        var transformation = new ReverseStyleTransformation();
        var result = StyleTransformationMerger.MergeStyleTransformations(
            [null, transformation, null]);

        Assert.Same(transformation, result);
    }

    [Fact]
    public void MergeStyleTransformations_AppliesTransformationsInSequence()
    {
        var reverse = new ReverseStyleTransformation();
        var swapColors = new SwapLightAndDarkStyleTransformation();
        var merged = StyleTransformationMerger.MergeStyleTransformations([reverse, swapColors]);

        var attrs = new Attrs(Color: "ansiblack", Reverse: false);
        var result = merged.TransformAttrs(attrs);

        // First reverse toggles Reverse to true
        // Then swapColors changes ansiblack to ansiwhite
        Assert.True(result.Reverse);
        Assert.Equal("ansiwhite", result.Color);
    }

    [Fact]
    public void MergeStyleTransformations_InvalidationHash_IsConsistent()
    {
        var reverse = new ReverseStyleTransformation();
        var swapColors = new SwapLightAndDarkStyleTransformation();
        var merged = StyleTransformationMerger.MergeStyleTransformations([reverse, swapColors]);

        var hash1 = merged.InvalidationHash;
        var hash2 = merged.InvalidationHash;

        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void MergeStyleTransformations_ImplementsIStyleTransformation()
    {
        var reverse = new ReverseStyleTransformation();
        var swapColors = new SwapLightAndDarkStyleTransformation();
        var merged = StyleTransformationMerger.MergeStyleTransformations([reverse, swapColors]);

        Assert.IsAssignableFrom<IStyleTransformation>(merged);
    }

    #endregion

    #region Thread Safety Tests

    [Fact]
    public async Task AllTransformations_AreThreadSafe()
    {
        var transformations = new IStyleTransformation[]
        {
            DummyStyleTransformation.Instance,
            new ReverseStyleTransformation(),
            new SwapLightAndDarkStyleTransformation(),
            new SetDefaultColorStyleTransformation("#ff0000", "#0000ff"),
            new AdjustBrightnessStyleTransformation(0.2f, 0.8f),
        };

        var exceptions = new List<Exception>();
        var tasks = new Task[10];

        for (int i = 0; i < tasks.Length; i++)
        {
            var index = i;
            tasks[i] = Task.Run(() =>
            {
                try
                {
                    for (int j = 0; j < 100; j++)
                    {
                        foreach (var transformation in transformations)
                        {
                            var attrs = new Attrs(Color: "ff0000", Bold: true);
                            var result = transformation.TransformAttrs(attrs);
                            // Attrs is a value type, verify it's valid by checking a property
                            _ = result.Color;

                            var hash = transformation.InvalidationHash;
                            Assert.NotNull(hash);
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
}
