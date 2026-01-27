using Stroke.Styles;
using Xunit;

namespace Stroke.Tests.Styles;

/// <summary>
/// Tests for the DefaultStyles class.
/// </summary>
public class DefaultStylesTests
{
    #region PromptToolkitStyle Tests

    [Fact]
    public void PromptToolkitStyle_ContainsExpectedRuleCount()
    {
        // Python Prompt Toolkit has 68 rules in PROMPT_TOOLKIT_STYLE
        Assert.True(DefaultStyles.PromptToolkitStyle.Count >= 68);
    }

    [Fact]
    public void PromptToolkitStyle_ContainsSearchRules()
    {
        var rules = DefaultStyles.PromptToolkitStyle.ToDictionary(r => r.Name, r => r.Style);

        Assert.True(rules.ContainsKey("search"));
        Assert.Equal("bg:ansibrightyellow ansiblack", rules["search"]);
        Assert.True(rules.ContainsKey("search.current"));
    }

    [Fact]
    public void PromptToolkitStyle_ContainsSelectedRule()
    {
        var rules = DefaultStyles.PromptToolkitStyle.ToDictionary(r => r.Name, r => r.Style);

        Assert.True(rules.ContainsKey("selected"));
        Assert.Equal("reverse", rules["selected"]);
    }

    [Fact]
    public void PromptToolkitStyle_ContainsCursorRules()
    {
        var rules = DefaultStyles.PromptToolkitStyle.ToDictionary(r => r.Name, r => r.Style);

        Assert.True(rules.ContainsKey("cursor-column"));
        Assert.True(rules.ContainsKey("cursor-line"));
    }

    [Fact]
    public void PromptToolkitStyle_ContainsLineNumberRules()
    {
        var rules = DefaultStyles.PromptToolkitStyle.ToDictionary(r => r.Name, r => r.Style);

        Assert.True(rules.ContainsKey("line-number"));
        Assert.Equal("#888888", rules["line-number"]);
        Assert.True(rules.ContainsKey("line-number.current"));
        Assert.Equal("bold", rules["line-number.current"]);
    }

    [Fact]
    public void PromptToolkitStyle_ContainsPromptRules()
    {
        var rules = DefaultStyles.PromptToolkitStyle.ToDictionary(r => r.Name, r => r.Style);

        Assert.True(rules.ContainsKey("prompt"));
        Assert.True(rules.ContainsKey("prompt.arg"));
        Assert.True(rules.ContainsKey("prompt.search"));
    }

    [Fact]
    public void PromptToolkitStyle_ContainsToolbarRules()
    {
        var rules = DefaultStyles.PromptToolkitStyle.ToDictionary(r => r.Name, r => r.Style);

        Assert.True(rules.ContainsKey("search-toolbar"));
        Assert.True(rules.ContainsKey("system-toolbar"));
        Assert.True(rules.ContainsKey("arg-toolbar"));
        Assert.True(rules.ContainsKey("validation-toolbar"));
    }

    [Fact]
    public void PromptToolkitStyle_ContainsCompletionMenuRules()
    {
        var rules = DefaultStyles.PromptToolkitStyle.ToDictionary(r => r.Name, r => r.Style);

        Assert.True(rules.ContainsKey("completion-menu"));
        Assert.True(rules.ContainsKey("completion-menu.completion"));
        Assert.True(rules.ContainsKey("completion-menu.completion.current"));
    }

    [Fact]
    public void PromptToolkitStyle_ContainsFuzzyMatchRules()
    {
        var rules = DefaultStyles.PromptToolkitStyle.ToDictionary(r => r.Name, r => r.Style);

        Assert.True(rules.ContainsKey("completion-menu.completion fuzzymatch.outside"));
        Assert.True(rules.ContainsKey("completion-menu.completion fuzzymatch.inside"));
    }

    [Fact]
    public void PromptToolkitStyle_ContainsScrollbarRules()
    {
        var rules = DefaultStyles.PromptToolkitStyle.ToDictionary(r => r.Name, r => r.Style);

        Assert.True(rules.ContainsKey("scrollbar.background"));
        Assert.True(rules.ContainsKey("scrollbar.button"));
        Assert.True(rules.ContainsKey("scrollbar.arrow"));
    }

    [Fact]
    public void PromptToolkitStyle_ContainsAutoSuggestionRule()
    {
        var rules = DefaultStyles.PromptToolkitStyle.ToDictionary(r => r.Name, r => r.Style);

        Assert.True(rules.ContainsKey("auto-suggestion"));
        Assert.Equal("#666666", rules["auto-suggestion"]);
    }

    [Fact]
    public void PromptToolkitStyle_ContainsHtmlElementRules()
    {
        var rules = DefaultStyles.PromptToolkitStyle.ToDictionary(r => r.Name, r => r.Style);

        Assert.Equal("italic", rules["i"]);
        Assert.Equal("underline", rules["u"]);
        Assert.Equal("strike", rules["s"]);
        Assert.Equal("bold", rules["b"]);
        Assert.Equal("italic", rules["em"]);
        Assert.Equal("bold", rules["strong"]);
        Assert.Equal("strike", rules["del"]);
        Assert.Equal("hidden", rules["hidden"]);
    }

    [Fact]
    public void PromptToolkitStyle_ContainsStyleNameRules()
    {
        var rules = DefaultStyles.PromptToolkitStyle.ToDictionary(r => r.Name, r => r.Style);

        Assert.Equal("italic", rules["italic"]);
        Assert.Equal("underline", rules["underline"]);
        Assert.Equal("strike", rules["strike"]);
        Assert.Equal("bold", rules["bold"]);
        Assert.Equal("reverse", rules["reverse"]);
        Assert.Equal("noitalic", rules["noitalic"]);
        Assert.Equal("nounderline", rules["nounderline"]);
        Assert.Equal("nostrike", rules["nostrike"]);
        Assert.Equal("nobold", rules["nobold"]);
        Assert.Equal("noreverse", rules["noreverse"]);
    }

    [Fact]
    public void PromptToolkitStyle_ContainsBottomToolbarRule()
    {
        var rules = DefaultStyles.PromptToolkitStyle.ToDictionary(r => r.Name, r => r.Style);

        Assert.True(rules.ContainsKey("bottom-toolbar"));
        Assert.Equal("reverse", rules["bottom-toolbar"]);
    }

    #endregion

    #region ColorsStyle Tests

    [Fact]
    public void ColorsStyle_ContainsAnsiColors()
    {
        var rules = DefaultStyles.ColorsStyle.ToDictionary(r => r.Name, r => r.Style);

        // Check some ANSI color names
        Assert.True(rules.ContainsKey("ansiblack"));
        Assert.Equal("fg:ansiblack", rules["ansiblack"]);
        Assert.True(rules.ContainsKey("ansiwhite"));
        Assert.True(rules.ContainsKey("ansired"));
        Assert.True(rules.ContainsKey("ansiblue"));
    }

    [Fact]
    public void ColorsStyle_ContainsNamedColors()
    {
        var rules = DefaultStyles.ColorsStyle.ToDictionary(r => r.Name, r => r.Style);

        // Check some HTML/CSS named colors (lowercase)
        Assert.True(rules.ContainsKey("aliceblue"));
        Assert.True(rules.ContainsKey("red"));
        Assert.True(rules.ContainsKey("blue"));
        Assert.True(rules.ContainsKey("green"));
    }

    [Fact]
    public void ColorsStyle_ContainsExpectedRuleCount()
    {
        // 17 ANSI colors + 140 named colors = 157 rules
        Assert.True(DefaultStyles.ColorsStyle.Count >= 157);
    }

    [Fact]
    public void ColorsStyle_UsesCorrectForegroundFormat()
    {
        var rules = DefaultStyles.ColorsStyle.ToDictionary(r => r.Name, r => r.Style);

        // All color rules should use "fg:" format
        foreach (var rule in DefaultStyles.ColorsStyle)
        {
            Assert.StartsWith("fg:", rule.Style);
        }
    }

    #endregion

    #region WidgetsStyle Tests

    [Fact]
    public void WidgetsStyle_ContainsExpectedRuleCount()
    {
        // Python Prompt Toolkit has 19 rules in WIDGETS_STYLE
        Assert.True(DefaultStyles.WidgetsStyle.Count >= 19);
    }

    [Fact]
    public void WidgetsStyle_ContainsDialogRules()
    {
        var rules = DefaultStyles.WidgetsStyle.ToDictionary(r => r.Name, r => r.Style);

        Assert.True(rules.ContainsKey("dialog"));
        Assert.Equal("bg:#4444ff", rules["dialog"]);
        Assert.True(rules.ContainsKey("dialog.body"));
        Assert.True(rules.ContainsKey("dialog frame.label"));
    }

    [Fact]
    public void WidgetsStyle_ContainsButtonRules()
    {
        var rules = DefaultStyles.WidgetsStyle.ToDictionary(r => r.Name, r => r.Style);

        Assert.True(rules.ContainsKey("button"));
        Assert.True(rules.ContainsKey("button.arrow"));
        Assert.True(rules.ContainsKey("button.focused"));
    }

    [Fact]
    public void WidgetsStyle_ContainsMenuRules()
    {
        var rules = DefaultStyles.WidgetsStyle.ToDictionary(r => r.Name, r => r.Style);

        Assert.True(rules.ContainsKey("menu-bar"));
        Assert.True(rules.ContainsKey("menu-bar.selected-item"));
        Assert.True(rules.ContainsKey("menu"));
        Assert.True(rules.ContainsKey("menu.border"));
    }

    [Fact]
    public void WidgetsStyle_ContainsProgressBarRules()
    {
        var rules = DefaultStyles.WidgetsStyle.ToDictionary(r => r.Name, r => r.Style);

        Assert.True(rules.ContainsKey("progress-bar"));
        Assert.True(rules.ContainsKey("progress-bar.used"));
    }

    [Fact]
    public void WidgetsStyle_ContainsShadowRules()
    {
        var rules = DefaultStyles.WidgetsStyle.ToDictionary(r => r.Name, r => r.Style);

        Assert.True(rules.ContainsKey("dialog shadow"));
        Assert.True(rules.ContainsKey("dialog.body shadow"));
    }

    #endregion

    #region PygmentsDefaultStyle Tests

    [Fact]
    public void PygmentsDefaultStyle_ContainsExpectedRuleCount()
    {
        // Python Prompt Toolkit has 34 rules in PYGMENTS_DEFAULT_STYLE
        Assert.True(DefaultStyles.PygmentsDefaultStyle.Count >= 34);
    }

    [Fact]
    public void PygmentsDefaultStyle_ContainsCommentRules()
    {
        Assert.True(DefaultStyles.PygmentsDefaultStyle.ContainsKey("pygments.comment"));
        Assert.Equal("italic #408080", DefaultStyles.PygmentsDefaultStyle["pygments.comment"]);
        Assert.True(DefaultStyles.PygmentsDefaultStyle.ContainsKey("pygments.comment.preproc"));
    }

    [Fact]
    public void PygmentsDefaultStyle_ContainsKeywordRules()
    {
        Assert.True(DefaultStyles.PygmentsDefaultStyle.ContainsKey("pygments.keyword"));
        Assert.Equal("bold #008000", DefaultStyles.PygmentsDefaultStyle["pygments.keyword"]);
        Assert.True(DefaultStyles.PygmentsDefaultStyle.ContainsKey("pygments.keyword.pseudo"));
        Assert.True(DefaultStyles.PygmentsDefaultStyle.ContainsKey("pygments.keyword.type"));
    }

    [Fact]
    public void PygmentsDefaultStyle_ContainsNameRules()
    {
        Assert.True(DefaultStyles.PygmentsDefaultStyle.ContainsKey("pygments.name.builtin"));
        Assert.True(DefaultStyles.PygmentsDefaultStyle.ContainsKey("pygments.name.function"));
        Assert.True(DefaultStyles.PygmentsDefaultStyle.ContainsKey("pygments.name.class"));
        Assert.True(DefaultStyles.PygmentsDefaultStyle.ContainsKey("pygments.name.exception"));
    }

    [Fact]
    public void PygmentsDefaultStyle_ContainsStringRules()
    {
        Assert.True(DefaultStyles.PygmentsDefaultStyle.ContainsKey("pygments.literal.string"));
        Assert.True(DefaultStyles.PygmentsDefaultStyle.ContainsKey("pygments.literal.string.doc"));
        Assert.True(DefaultStyles.PygmentsDefaultStyle.ContainsKey("pygments.literal.string.escape"));
    }

    [Fact]
    public void PygmentsDefaultStyle_ContainsGenericRules()
    {
        Assert.True(DefaultStyles.PygmentsDefaultStyle.ContainsKey("pygments.generic.heading"));
        Assert.True(DefaultStyles.PygmentsDefaultStyle.ContainsKey("pygments.generic.deleted"));
        Assert.True(DefaultStyles.PygmentsDefaultStyle.ContainsKey("pygments.generic.inserted"));
        Assert.True(DefaultStyles.PygmentsDefaultStyle.ContainsKey("pygments.generic.error"));
    }

    [Fact]
    public void PygmentsDefaultStyle_ContainsErrorRule()
    {
        Assert.True(DefaultStyles.PygmentsDefaultStyle.ContainsKey("pygments.error"));
        Assert.Equal("border:#ff0000", DefaultStyles.PygmentsDefaultStyle["pygments.error"]);
    }

    #endregion

    #region DefaultUiStyle Tests

    [Fact]
    public void DefaultUiStyle_IsNotNull()
    {
        Assert.NotNull(DefaultStyles.DefaultUiStyle);
    }

    [Fact]
    public void DefaultUiStyle_ImplementsIStyle()
    {
        Assert.IsAssignableFrom<IStyle>(DefaultStyles.DefaultUiStyle);
    }

    [Fact]
    public void DefaultUiStyle_ContainsPromptToolkitStyles()
    {
        var result = DefaultStyles.DefaultUiStyle.GetAttrsForStyleStr("class:selected");
        Assert.True(result.Reverse);
    }

    [Fact]
    public void DefaultUiStyle_ContainsColorStyles()
    {
        var result = DefaultStyles.DefaultUiStyle.GetAttrsForStyleStr("class:ansiblue");
        Assert.Equal("ansiblue", result.Color);
    }

    [Fact]
    public void DefaultUiStyle_ContainsWidgetStyles()
    {
        var result = DefaultStyles.DefaultUiStyle.GetAttrsForStyleStr("class:dialog");
        Assert.Equal("4444ff", result.BgColor);
    }

    [Fact]
    public void DefaultUiStyle_IsCached()
    {
        var style1 = DefaultStyles.DefaultUiStyle;
        var style2 = DefaultStyles.DefaultUiStyle;
        Assert.Same(style1, style2);
    }

    #endregion

    #region DefaultPygmentsStyle Tests

    [Fact]
    public void DefaultPygmentsStyle_IsNotNull()
    {
        Assert.NotNull(DefaultStyles.DefaultPygmentsStyle);
    }

    [Fact]
    public void DefaultPygmentsStyle_ImplementsIStyle()
    {
        Assert.IsAssignableFrom<IStyle>(DefaultStyles.DefaultPygmentsStyle);
    }

    [Fact]
    public void DefaultPygmentsStyle_ContainsKeywordStyle()
    {
        var result = DefaultStyles.DefaultPygmentsStyle.GetAttrsForStyleStr("class:pygments.keyword");
        Assert.True(result.Bold);
        Assert.Equal("008000", result.Color);
    }

    [Fact]
    public void DefaultPygmentsStyle_ContainsCommentStyle()
    {
        var result = DefaultStyles.DefaultPygmentsStyle.GetAttrsForStyleStr("class:pygments.comment");
        Assert.True(result.Italic);
        Assert.Equal("408080", result.Color);
    }

    [Fact]
    public void DefaultPygmentsStyle_IsCached()
    {
        var style1 = DefaultStyles.DefaultPygmentsStyle;
        var style2 = DefaultStyles.DefaultPygmentsStyle;
        Assert.Same(style1, style2);
    }

    #endregion

    #region Thread Safety Tests

    [Fact]
    public async Task DefaultStyles_ThreadSafe_ConcurrentAccess()
    {
        var tasks = new Task[10];
        var exceptions = new List<Exception>();

        for (int i = 0; i < tasks.Length; i++)
        {
            tasks[i] = Task.Run(() =>
            {
                try
                {
                    for (int j = 0; j < 100; j++)
                    {
                        // Access all static properties
                        _ = DefaultStyles.PromptToolkitStyle.Count;
                        _ = DefaultStyles.ColorsStyle.Count;
                        _ = DefaultStyles.WidgetsStyle.Count;
                        _ = DefaultStyles.PygmentsDefaultStyle.Count;

                        // Access lazy-initialized styles
                        var ui = DefaultStyles.DefaultUiStyle;
                        var pygments = DefaultStyles.DefaultPygmentsStyle;

                        // Use the styles
                        _ = ui.GetAttrsForStyleStr("class:selected");
                        _ = pygments.GetAttrsForStyleStr("class:pygments.keyword");
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
    public void DefaultStyles_MatchesPythonPromptToolkitStyleCount()
    {
        // Python has exactly 68 rules in PROMPT_TOOLKIT_STYLE
        // We might have exactly 68 or more if additional rules were added
        Assert.InRange(DefaultStyles.PromptToolkitStyle.Count, 68, 100);
    }

    [Fact]
    public void DefaultStyles_MatchesPythonWidgetsStyleCount()
    {
        // Python has exactly 19 rules in WIDGETS_STYLE (counting shadow rules)
        // Note: Python has 19 rules
        Assert.InRange(DefaultStyles.WidgetsStyle.Count, 19, 30);
    }

    [Fact]
    public void DefaultStyles_MatchesPythonPygmentsDefaultStyleCount()
    {
        // Python has exactly 34 rules in PYGMENTS_DEFAULT_STYLE
        Assert.InRange(DefaultStyles.PygmentsDefaultStyle.Count, 34, 50);
    }

    #endregion
}
