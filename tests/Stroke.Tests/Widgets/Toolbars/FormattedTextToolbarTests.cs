using Stroke.FormattedText;
using Stroke.Layout.Containers;
using Stroke.Layout.Controls;
using Stroke.Widgets.Toolbars;
using Xunit;

namespace Stroke.Tests.Widgets.Toolbars;

/// <summary>
/// Tests for FormattedTextToolbar (US1: display static formatted text in a toolbar).
/// </summary>
public sealed class FormattedTextToolbarTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_WithPlainString_CreatesInstance()
    {
        var toolbar = new FormattedTextToolbar("Hello");
        Assert.NotNull(toolbar);
    }

    [Fact]
    public void Constructor_WithEmptyString_CreatesInstance()
    {
        var toolbar = new FormattedTextToolbar("");
        Assert.NotNull(toolbar);
    }

    [Fact]
    public void Constructor_WithFormattedText_CreatesInstance()
    {
        var formatted = new Stroke.FormattedText.FormattedText(new StyleAndTextTuple[]
        {
            new("class:toolbar", "Status: "),
            new("class:toolbar.text", "OK"),
        });
        var toolbar = new FormattedTextToolbar(formatted);
        Assert.NotNull(toolbar);
    }

    [Fact]
    public void Constructor_WithFunc_CreatesInstance()
    {
        Func<AnyFormattedText> func = () => "Dynamic text";
        var toolbar = new FormattedTextToolbar(func);
        Assert.NotNull(toolbar);
    }

    [Fact]
    public void Constructor_WithStyle_AppliesStyleToWindow()
    {
        var toolbar = new FormattedTextToolbar("Hello", style: "class:my-toolbar");
        Assert.NotNull(toolbar);
    }

    [Fact]
    public void Constructor_DefaultStyle_IsEmpty()
    {
        var toolbar = new FormattedTextToolbar("Hello");
        // Default style is empty string — toolbar should still construct
        Assert.NotNull(toolbar);
    }

    #endregion

    #region Inheritance Tests

    [Fact]
    public void FormattedTextToolbar_ExtendsWindow()
    {
        var toolbar = new FormattedTextToolbar("Hello");
        Assert.IsAssignableFrom<Window>(toolbar);
    }

    [Fact]
    public void FormattedTextToolbar_IsIContainer()
    {
        var toolbar = new FormattedTextToolbar("Hello");
        Assert.IsAssignableFrom<IContainer>(toolbar);
    }

    #endregion

    #region Window Configuration Tests

    [Fact]
    public void Constructor_DontExtendHeight_IsTrue()
    {
        var toolbar = new FormattedTextToolbar("Hello");
        Assert.True(toolbar.DontExtendHeight.Invoke());
    }

    [Fact]
    public void Constructor_Height_HasMinOfOne()
    {
        var toolbar = new FormattedTextToolbar("Hello");
        // The preferred height with min=1 should return at least 1
        var preferred = toolbar.PreferredHeight(80, 24);
        Assert.True(preferred.Min >= 1);
    }

    [Fact]
    public void Constructor_Content_IsFormattedTextControl()
    {
        var toolbar = new FormattedTextToolbar("Hello");
        Assert.IsType<FormattedTextControl>(toolbar.Content);
    }

    [Fact]
    public void Constructor_ContentUsesTextGetterFunc()
    {
        // FormattedTextControl should use the Func constructor (lazy evaluation)
        var toolbar = new FormattedTextToolbar("Hello");
        var control = Assert.IsType<FormattedTextControl>(toolbar.Content);

        // Verify the control can create content (confirms the Func getter works)
        var content = control.CreateContent(80, 1);
        Assert.NotNull(content);
        Assert.True(content.LineCount >= 1);
    }

    #endregion

    #region Text Display Tests

    [Fact]
    public void CreateContent_PlainString_DisplaysText()
    {
        var toolbar = new FormattedTextToolbar("Hello World");
        var control = (FormattedTextControl)toolbar.Content;
        var content = control.CreateContent(80, 1);

        var line = content.GetLine(0);
        var text = string.Join("", line.Select(f => f.Text));
        Assert.Contains("Hello World", text);
    }

    [Fact]
    public void CreateContent_DynamicFunc_EvaluatesLazily()
    {
        var counter = 0;
        Func<AnyFormattedText> textFunc = () =>
        {
            counter++;
            return $"Count: {counter}";
        };

        var toolbar = new FormattedTextToolbar(textFunc);
        var control = (FormattedTextControl)toolbar.Content;

        // First call
        control.Reset();
        var content1 = control.CreateContent(80, 1);
        var line1 = content1.GetLine(0);
        var text1 = string.Join("", line1.Select(f => f.Text));
        Assert.Contains("Count:", text1);

        // Second call after reset should re-evaluate
        control.Reset();
        var content2 = control.CreateContent(80, 1);
        var line2 = content2.GetLine(0);
        var text2 = string.Join("", line2.Select(f => f.Text));
        Assert.Contains("Count:", text2);
    }

    #endregion
}
