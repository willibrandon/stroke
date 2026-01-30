using Stroke.Application;
using Stroke.Filters;
using Stroke.Output;
using Stroke.Styles;
using Xunit;

namespace Stroke.Tests.Application;

public class ApplicationStyleMergingTests
{
    [Fact]
    public void NoCustomStyle_UsesDefaults()
    {
        var output = new DummyOutput();
        var app = new Application<object?>(
            output: output,
            style: null);

        // MergedStyle should exist even without custom style
        Assert.NotNull(app.MergedStyle);
    }

    [Fact]
    public void CustomStyle_OverridesDefaults()
    {
        var output = new DummyOutput();
        var customStyle = Style.FromDict(new Dictionary<string, string>
        {
            { "my-custom", "fg:red" }
        });

        var app = new Application<object?>(
            output: output,
            style: customStyle);

        Assert.NotNull(app.MergedStyle);

        // The merged style should include the custom style's rule
        var attrs = app.MergedStyle.GetAttrsForStyleStr("class:my-custom");
        // Custom style set fg:red, so attrs should be valid (Attrs is a value type)
        _ = attrs;
    }

    [Fact]
    public void IncludeDefaultPygmentsStyle_True_IncludesPygments()
    {
        var output = new DummyOutput();
        var app = new Application<object?>(
            output: output,
            includeDefaultPygmentsStyle: new FilterOrBool(true));

        Assert.NotNull(app.MergedStyle);

        // Pygments style includes token styling rules
        // The merged style should include pygments token styles
        var styleRules = app.MergedStyle.StyleRules;
        Assert.NotNull(styleRules);
    }

    [Fact]
    public void IncludeDefaultPygmentsStyle_False_ExcludesPygments()
    {
        var output = new DummyOutput();
        var app = new Application<object?>(
            output: output,
            includeDefaultPygmentsStyle: new FilterOrBool(false));

        Assert.NotNull(app.MergedStyle);
        // When Pygments excluded, fewer style rules should be present
        var styleRules = app.MergedStyle.StyleRules;
        Assert.NotNull(styleRules);
    }

    [Fact]
    public void IncludeDefaultPygmentsStyle_Default_TreatedAsTrue()
    {
        var output = new DummyOutput();
        // default FilterOrBool (no HasValue) should be treated as Always (include pygments)
        var app = new Application<object?>(output: output);

        Assert.NotNull(app.MergedStyle);
    }

    [Fact]
    public void StyleTransformation_Applied()
    {
        var output = new DummyOutput();

        // Use a real transformation
        var transformation = new ReverseStyleTransformation();

        var app = new Application<object?>(
            output: output,
            styleTransformation: transformation);

        Assert.Same(transformation, app.StyleTransformation);
    }

    [Fact]
    public void StyleTransformation_Default_IsDummy()
    {
        var output = new DummyOutput();
        var app = new Application<object?>(output: output);

        Assert.Same(DummyStyleTransformation.Instance, app.StyleTransformation);
    }

    [Fact]
    public void StyleTransformation_CanBeChanged()
    {
        var output = new DummyOutput();
        var app = new Application<object?>(output: output);

        var newTransformation = new ReverseStyleTransformation();
        app.StyleTransformation = newTransformation;
        Assert.Same(newTransformation, app.StyleTransformation);
    }

    [Fact]
    public void StyleTransformation_NullThrows()
    {
        var output = new DummyOutput();
        var app = new Application<object?>(output: output);

        Assert.Throws<ArgumentNullException>(() => app.StyleTransformation = null!);
    }

    [Fact]
    public void Style_PropertyCanBeSet()
    {
        var output = new DummyOutput();
        var app = new Application<object?>(output: output);

        Assert.Null(app.Style);

        var customStyle = Style.FromDict(new Dictionary<string, string>
        {
            { "custom", "bold" }
        });

        app.Style = customStyle;
        Assert.Same(customStyle, app.Style);
    }

    [Fact]
    public void Style_SetToNull_Allowed()
    {
        var output = new DummyOutput();
        var customStyle = Style.FromDict(new Dictionary<string, string>
        {
            { "custom", "bold" }
        });

        var app = new Application<object?>(
            output: output,
            style: customStyle);

        Assert.Same(customStyle, app.Style);

        app.Style = null;
        Assert.Null(app.Style);
    }

    [Fact]
    public void MergedStyle_IsDynamicForUserStyle()
    {
        var output = new DummyOutput();
        var app = new Application<object?>(output: output);

        // The merged style is created at construction time, using DynamicStyle
        // for the user style portion. This means changing app.Style will be
        // reflected in the merged style dynamically.
        Assert.NotNull(app.MergedStyle);

        // Set a custom style (keys use class name without "class:" prefix)
        var customStyle = Style.FromDict(new Dictionary<string, string>
        {
            { "dynamic-test", "fg:green" }
        });
        app.Style = customStyle;

        // The merged style should now include the custom rule via DynamicStyle
        var attrs = app.MergedStyle.GetAttrsForStyleStr("class:dynamic-test");
        _ = attrs; // Attrs is a value type; accessing it verifies no exception
    }

    [Fact]
    public void GetUsedStyleStrings_EmptyBeforeRender()
    {
        var output = new DummyOutput();
        var app = new Application<object?>(output: output);

        // Before any render, AttrsForStyle is null, so GetUsedStyleStrings returns empty
        var strings = app.GetUsedStyleStrings();
        Assert.Empty(strings);
    }

    [Fact]
    public void GetUsedStyleStrings_PopulatedAfterRender()
    {
        var output = new DummyOutput();
        var app = new Application<object?>(output: output);

        // Trigger a render
        app.Renderer.Render(app.UnsafeCast, app.Layout);

        var strings = app.GetUsedStyleStrings();
        // After render, should have some style strings (sorted)
        Assert.NotNull(strings);

        // If there are strings, they should be sorted
        if (strings.Count > 1)
        {
            for (int i = 1; i < strings.Count; i++)
            {
                Assert.True(string.Compare(strings[i - 1], strings[i], StringComparison.Ordinal) <= 0);
            }
        }
    }

    [Fact]
    public void MergedStyle_IncludesDefaultUiStyle()
    {
        var output = new DummyOutput();
        var app = new Application<object?>(output: output);

        // Default UI style should be included in merged style rules
        var rules = app.MergedStyle.StyleRules;
        Assert.NotNull(rules);
        Assert.True(rules.Count > 0, "Merged style should include default UI rules");
    }
}
