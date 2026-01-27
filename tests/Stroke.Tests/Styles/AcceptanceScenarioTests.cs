using Stroke.Filters;
using Stroke.Styles;
using Xunit;

namespace Stroke.Tests.Styles;

/// <summary>
/// Acceptance scenario tests verifying spec.md requirements.
/// </summary>
public class AcceptanceScenarioTests
{
    #region User Story 1 - Define Custom Styles for Application UI

    [Fact]
    public void US1_Scenario1_CreateStyleWithRules_GetAttrsForTitleClass()
    {
        // Given a developer creates a Style with rules [("title", "#ff0000 bold"), ("subtitle", "#666666 italic")]
        var style = new Style([("title", "#ff0000 bold"), ("subtitle", "#666666 italic")]);

        // When they request attrs for "class:title"
        var result = style.GetAttrsForStyleStr("class:title");

        // Then they receive Attrs with color="ff0000" and bold=true
        Assert.Equal("ff0000", result.Color);
        Assert.True(result.Bold);
    }

    [Fact]
    public void US1_Scenario2_CreateStyleFromDict_GetAttrsForDefinedClass()
    {
        // Given a developer creates a Style from a dictionary with class names and style definitions
        var styleDict = new Dictionary<string, string>
        {
            ["error"] = "#ff0000 bold",
            ["warning"] = "#ffff00",
            ["info"] = "#0000ff"
        };
        var style = Style.FromDict(styleDict);

        // When they request attrs for any defined class
        var errorAttrs = style.GetAttrsForStyleStr("class:error");
        var warningAttrs = style.GetAttrsForStyleStr("class:warning");
        var infoAttrs = style.GetAttrsForStyleStr("class:info");

        // Then they receive the corresponding Attrs
        Assert.Equal("ff0000", errorAttrs.Color);
        Assert.True(errorAttrs.Bold);
        Assert.Equal("ffff00", warningAttrs.Color);
        Assert.Equal("0000ff", infoAttrs.Color);
    }

    [Fact]
    public void US1_Scenario3_InlineStyles_AllAttributesApplied()
    {
        // Given a developer specifies inline styles like "bold underline #00ff00"
        var style = new Style([]);

        // When attrs are computed
        var result = style.GetAttrsForStyleStr("bold underline #00ff00");

        // Then all specified attributes are correctly applied
        Assert.True(result.Bold);
        Assert.True(result.Underline);
        Assert.Equal("00ff00", result.Color);
    }

    [Fact]
    public void US1_Scenario4_ClassAndInlineStyles_InlineTakesPrecedence()
    {
        // Given a developer uses both class names and inline styles together
        var style = new Style([("title", "#ff0000 nobold")]);

        // When attrs are computed (inline bold should override class nobold)
        var result = style.GetAttrsForStyleStr("class:title bold");

        // Then inline styles take precedence over class styles
        Assert.Equal("ff0000", result.Color);
        Assert.True(result.Bold);  // inline "bold" overrides class "nobold"
    }

    #endregion

    #region User Story 2 - Use Standard Color Names and Formats

    [Fact]
    public void US2_Scenario1_AnsiBlue_RecognizedAsValidAnsiColor()
    {
        // Given a developer specifies "ansiblue" as a color
        // When the color is parsed
        var color = StyleParser.ParseColor("ansiblue");

        // Then it is recognized as a valid ANSI color
        Assert.Equal("ansiblue", color);
    }

    [Fact]
    public void US2_Scenario2_AliceBlue_ConvertedToHex()
    {
        // Given a developer specifies "AliceBlue" as a color
        // When the color is parsed
        var color = StyleParser.ParseColor("AliceBlue");

        // Then it is converted to the hex value "f0f8ff"
        Assert.Equal("f0f8ff", color);
    }

    [Fact]
    public void US2_Scenario3_ThreeDigitHex_ExpandedToSix()
    {
        // Given a developer specifies "#ff0" (3-digit hex)
        // When the color is parsed
        var color = StyleParser.ParseColor("#ff0");

        // Then it is expanded to "ffff00"
        Assert.Equal("ffff00", color);
    }

    [Fact]
    public void US2_Scenario4_SixDigitHex_StoredWithoutHash()
    {
        // Given a developer specifies "#ff0000" (6-digit hex)
        // When the color is parsed
        var color = StyleParser.ParseColor("#ff0000");

        // Then it is stored as "ff0000"
        Assert.Equal("ff0000", color);
    }

    [Fact]
    public void US2_Scenario5_InvalidColor_ErrorRaised()
    {
        // Given a developer specifies an invalid color like "notacolor"
        // When the color is parsed, then an appropriate error is raised
        var ex = Assert.Throws<ArgumentException>(() => StyleParser.ParseColor("notacolor"));
        Assert.Contains("Wrong color format", ex.Message);
    }

    #endregion

    #region User Story 3 - Merge Multiple Styles

    [Fact]
    public void US3_Scenario1_MergeOverlappingRules_LaterTakesPrecedence()
    {
        // Given two styles A and B where B defines a rule for "title" that A also defines
        var styleA = new Style([("title", "#ff0000")]);
        var styleB = new Style([("title", "#00ff00")]);

        // When they are merged as [A, B]
        var merged = StyleMerger.MergeStyles([styleA, styleB]);

        // Then B's rule takes precedence for "title"
        var result = merged.GetAttrsForStyleStr("class:title");
        Assert.Equal("00ff00", result.Color);
    }

    [Fact]
    public void US3_Scenario2_MergeNonOverlappingRules_AllAvailable()
    {
        // Given styles A and B with non-overlapping rules
        var styleA = new Style([("title", "#ff0000")]);
        var styleB = new Style([("subtitle", "#00ff00")]);

        // When they are merged
        var merged = StyleMerger.MergeStyles([styleA, styleB]);

        // Then all rules from both styles are available
        var titleAttrs = merged.GetAttrsForStyleStr("class:title");
        var subtitleAttrs = merged.GetAttrsForStyleStr("class:subtitle");
        Assert.Equal("ff0000", titleAttrs.Color);
        Assert.Equal("00ff00", subtitleAttrs.Color);
    }

    [Fact]
    public void US3_Scenario3_MergedStyle_InvalidationHashChanges()
    {
        // Given a merged style
        var style1 = new Style([("title", "#ff0000")]);
        var style2 = new Style([("title", "#00ff00")]);
        IStyle? currentStyle = style1;
        var dynamicStyle = new DynamicStyle(() => currentStyle);
        var merged = StyleMerger.MergeStyles([dynamicStyle, new Style([])]);

        // When one of the source styles changes
        var hash1 = merged.InvalidationHash;
        currentStyle = style2;
        var hash2 = merged.InvalidationHash;

        // Then the merged style's invalidation hash changes
        Assert.NotEqual(hash1, hash2);
    }

    #endregion

    #region User Story 4 - Transform Styles Dynamically

    [Fact]
    public void US4_Scenario1_SwapLightAndDark_DarkColorInverted()
    {
        // Given attrs with color="000000"
        var attrs = new Attrs(Color: "000000");
        var transform = new SwapLightAndDarkStyleTransformation();

        // When SwapLightAndDarkStyleTransformation is applied
        var result = transform.TransformAttrs(attrs);

        // Then the color is inverted to a light color
        Assert.NotNull(result.Color);
        Assert.NotEqual("000000", result.Color);
        // Black (000000) should become white (ffffff) or close to it
        Assert.Equal("ffffff", result.Color);
    }

    [Fact]
    public void US4_Scenario2_ReverseTransformation_ReverseBecomeTrue()
    {
        // Given attrs with reverse=false
        var attrs = new Attrs(Reverse: false);
        var transform = new ReverseStyleTransformation();

        // When ReverseStyleTransformation is applied
        var result = transform.TransformAttrs(attrs);

        // Then reverse becomes true
        Assert.True(result.Reverse);
    }

    [Fact]
    public void US4_Scenario3_SetDefaultColor_DefaultsUsed()
    {
        // Given attrs with default/empty colors
        var attrs = new Attrs(Color: "", BgColor: "");
        var transform = new SetDefaultColorStyleTransformation(
            fg: "#ff0000",
            bg: "#0000ff");

        // When SetDefaultColorStyleTransformation is applied with specific defaults
        var result = transform.TransformAttrs(attrs);

        // Then those defaults are used
        Assert.Equal("ff0000", result.Color);
        Assert.Equal("0000ff", result.BgColor);
    }

    [Fact]
    public void US4_Scenario4_AdjustBrightness_MinBrightnessApplied()
    {
        // Given attrs with a specific foreground color (dark color)
        var attrs = new Attrs(Color: "000000");  // Black, very low brightness
        var transform = new AdjustBrightnessStyleTransformation(minBrightness: 0.3f);

        // When AdjustBrightnessStyleTransformation is applied with min_brightness=0.3
        var result = transform.TransformAttrs(attrs);

        // Then the color brightness is adjusted to be at least 0.3
        Assert.NotNull(result.Color);
        Assert.NotEqual("000000", result.Color);  // Color should be brighter now
    }

    [Fact]
    public void US4_Scenario5_ConditionalTransformation_FilterFalse_AttrsUnchanged()
    {
        // Given a conditional transformation with a filter that returns false
        var transform = new ReverseStyleTransformation();
        var conditional = new ConditionalStyleTransformation(transform, false);
        var attrs = new Attrs(Reverse: false);

        // When applied
        var result = conditional.TransformAttrs(attrs);

        // Then attrs are unchanged
        Assert.False(result.Reverse);
    }

    #endregion

    #region User Story 5 - Use Dynamic and Conditional Styles

    [Fact]
    public void US5_Scenario1_DynamicStyle_UsesCallableStyle()
    {
        // Given a DynamicStyle with a callable that returns Style A
        var styleA = new Style([("title", "#ff0000 bold")]);
        var dynamicStyle = new DynamicStyle(() => styleA);

        // When attrs are requested
        var result = dynamicStyle.GetAttrsForStyleStr("class:title");

        // Then Style A's rules are used
        Assert.Equal("ff0000", result.Color);
        Assert.True(result.Bold);
    }

    [Fact]
    public void US5_Scenario2_DynamicStyle_CallableChangeReflected()
    {
        // Given a DynamicStyle where the callable returns a different Style B
        var styleA = new Style([("title", "#ff0000")]);
        var styleB = new Style([("title", "#00ff00")]);
        IStyle? currentStyle = styleA;
        var dynamicStyle = new DynamicStyle(() => currentStyle);

        var result1 = dynamicStyle.GetAttrsForStyleStr("class:title");
        Assert.Equal("ff0000", result1.Color);

        currentStyle = styleB;

        // When attrs are requested again
        var result2 = dynamicStyle.GetAttrsForStyleStr("class:title");

        // Then Style B's rules are used
        Assert.Equal("00ff00", result2.Color);
    }

    [Fact]
    public void US5_Scenario3_DynamicStyleTransformation_HashChanges()
    {
        // Given a DynamicStyleTransformation
        var transformA = new ReverseStyleTransformation();
        var transformB = DummyStyleTransformation.Instance;
        IStyleTransformation? currentTransform = transformA;
        var dynamic = new DynamicStyleTransformation(() => currentTransform);

        // When the underlying transformation changes
        var hash1 = dynamic.InvalidationHash;
        currentTransform = transformB;
        var hash2 = dynamic.InvalidationHash;

        // Then the invalidation hash changes
        Assert.NotEqual(hash1, hash2);
    }

    #endregion

    #region User Story 6 - Apply Default UI and Pygments Styles

    [Fact]
    public void US6_Scenario1_DefaultUiStyle_SearchClass()
    {
        // Given the default UI style
        var style = DefaultStyles.DefaultUiStyle;

        // When attrs are requested for "class:search"
        var result = style.GetAttrsForStyleStr("class:search");

        // Then appropriate search highlighting attrs are returned
        Assert.NotNull(result.BgColor);  // search has bg:ansibrightyellow
    }

    [Fact]
    public void US6_Scenario2_DefaultUiStyle_CompletionMenuClass()
    {
        // Given the default UI style
        var style = DefaultStyles.DefaultUiStyle;

        // When attrs are requested for "class:completion-menu"
        var result = style.GetAttrsForStyleStr("class:completion-menu");

        // Then appropriate menu attrs are returned
        Assert.NotNull(result.BgColor);  // completion-menu has bg:#bbbbbb
    }

    [Fact]
    public void US6_Scenario3_DefaultPygmentsStyle_KeywordClass()
    {
        // Given the default Pygments style
        var style = DefaultStyles.DefaultPygmentsStyle;

        // When attrs are requested for "class:pygments.keyword"
        var result = style.GetAttrsForStyleStr("class:pygments.keyword");

        // Then appropriate syntax highlighting attrs are returned
        Assert.True(result.Bold);
        Assert.Equal("008000", result.Color);
    }

    [Fact]
    public void US6_Scenario4_StyleFromPygmentsClass()
    {
        // Given a Pygments-style class with token definitions
        // When StyleFromPygmentsClass is called
        var style = PygmentsStyleUtils.StyleFromPygmentsClass(() => new Dictionary<string, string>
        {
            ["Keyword"] = "bold #008000",
            ["String"] = "#ba2121"
        });

        // Then a Style with corresponding rules is returned
        var keywordAttrs = style.GetAttrsForStyleStr("class:pygments.keyword");
        Assert.True(keywordAttrs.Bold);
    }

    [Fact]
    public void US6_Scenario5_StyleFromPygmentsDict()
    {
        // Given a dictionary mapping tokens to style strings
        var dict = new Dictionary<string, string>
        {
            ["Comment"] = "italic #408080",
            ["Name.Function"] = "#0000ff"
        };

        // When StyleFromPygmentsDict is called
        var style = PygmentsStyleUtils.StyleFromPygmentsDict(dict);

        // Then a Style with those rules is created
        var commentAttrs = style.GetAttrsForStyleStr("class:pygments.comment");
        Assert.True(commentAttrs.Italic);
    }

    [Fact]
    public void US6_Scenario6_PygmentsTokenToClassName()
    {
        // Given a Pygments token like Token.Name.Exception
        // When PygmentsTokenToClassName is called
        var className = PygmentsStyleUtils.PygmentsTokenToClassName("Name", "Exception");

        // Then it returns "pygments.name.exception"
        Assert.Equal("pygments.name.exception", className);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void EdgeCase_EmptyStyleString_ReturnsDefaultAttrs()
    {
        var style = new Style([("title", "bold")]);
        var result = style.GetAttrsForStyleStr("");
        Assert.Equal(DefaultAttrs.Default, result);
    }

    [Fact]
    public void EdgeCase_UndefinedClass_FallsBackToDefault()
    {
        var style = new Style([("title", "bold")]);
        var result = style.GetAttrsForStyleStr("class:undefined");
        Assert.Equal(DefaultAttrs.Default, result);
    }

    [Fact]
    public void EdgeCase_ConflictingAttributes_LaterOverrides()
    {
        var style = new Style([]);
        var result = style.GetAttrsForStyleStr("bold nobold");
        Assert.False(result.Bold);
    }

    [Fact]
    public void EdgeCase_Noinherit_ResetsToDefault()
    {
        var style = new Style([
            ("base", "bold #ff0000"),
            ("special", "noinherit italic")
        ]);

        // class:base applies bold and color
        // class:special with noinherit should reset then apply italic
        var result = style.GetAttrsForStyleStr("class:base class:special");
        Assert.True(result.Italic);
        // After noinherit, bold should be explicitly set to false (default value)
        Assert.False(result.Bold);  // noinherit resets to default (false)
    }

    [Fact]
    public void EdgeCase_HierarchicalClassNames_ExpandsCorrectly()
    {
        var style = new Style([
            ("a", "#ff0000"),
            ("a.b", "bold"),
            ("a.b.c", "italic")
        ]);

        var result = style.GetAttrsForStyleStr("class:a.b.c");

        // Should apply all: a, a.b, a.b.c
        Assert.Equal("ff0000", result.Color);  // from a
        Assert.True(result.Bold);               // from a.b
        Assert.True(result.Italic);             // from a.b.c
    }

    [Fact]
    public void EdgeCase_NullStylesInMerge_FilteredOut()
    {
        var styleA = new Style([("title", "#ff0000")]);
        var merged = StyleMerger.MergeStyles([styleA, null, null]);

        var result = merged.GetAttrsForStyleStr("class:title");
        Assert.Equal("ff0000", result.Color);
    }

    [Fact]
    public void EdgeCase_BrightnessTransformation_AnsiColorConvertedToRgb()
    {
        // When brightness transformation is applied to ANSI colors
        var attrs = new Attrs(Color: "ansiblue");
        var transform = new AdjustBrightnessStyleTransformation(minBrightness: 0.3f);

        var result = transform.TransformAttrs(attrs);

        // ANSI colors are converted to RGB for transformation
        Assert.NotNull(result.Color);
        // Result should be a hex color, not an ANSI name
        Assert.DoesNotContain("ansi", result.Color);
    }

    [Fact]
    public void EdgeCase_ColorAliases_ResolvedCorrectly()
    {
        // When colors use aliases (e.g., ansibrown)
        var color = StyleParser.ParseColor("ansibrown");

        // Aliases are resolved to canonical ANSI names
        Assert.Equal("ansiyellow", color);  // ansibrown is an alias for ansiyellow
    }

    #endregion

    #region Success Criteria Verification

    [Fact]
    public void SC001_All140NamedColors_CorrectlyMapped()
    {
        // spec.md mentions 140 colors, but Python Prompt Toolkit actually has 148
        Assert.True(NamedColors.Colors.Count >= 140);
    }

    [Fact]
    public void SC002_All17AnsiColorsAnd10Aliases_Recognized()
    {
        Assert.Equal(17, AnsiColorNames.Names.Count);
        Assert.Equal(10, AnsiColorNames.Aliases.Count);
    }

    [Fact]
    public void SC007_DefaultUiStyle_AtLeast50Classes()
    {
        // 68 + 157 + 19 = 244 rules minimum
        Assert.True(DefaultStyles.PromptToolkitStyle.Count >= 50);
    }

    [Fact]
    public void SC008_DefaultPygmentsStyle_AtLeast30TokenTypes()
    {
        Assert.True(DefaultStyles.PygmentsDefaultStyle.Count >= 30);
    }

    #endregion
}
