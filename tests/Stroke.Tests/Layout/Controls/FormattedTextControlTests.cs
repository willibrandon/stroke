using Stroke.Core;
using Stroke.Core.Primitives;
using Stroke.Filters;
using Stroke.FormattedText;
using Stroke.Layout.Controls;
using Xunit;

namespace Stroke.Tests.Layout.Controls;

/// <summary>
/// Tests for FormattedTextControl.
/// </summary>
public sealed class FormattedTextControlTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_WithString_CreatesControl()
    {
        var control = new FormattedTextControl("Hello World");

        Assert.NotNull(control);
        Assert.False(control.IsFocusable);
    }

    [Fact]
    public void Constructor_WithFragments_StoresFragments()
    {
        var fragments = new[]
        {
            new StyleAndTextTuple("class:test", "Hello "),
            new StyleAndTextTuple("class:other", "World")
        };

        var control = new FormattedTextControl(fragments);

        var content = control.CreateContent(80, 24);
        Assert.Equal(1, content.LineCount);
    }

    [Fact]
    public void Constructor_WithStyle_AppliesStyle()
    {
        var control = new FormattedTextControl("Test", style: "class:custom");

        var content = control.CreateContent(80, 24);
        var line = content.GetLine(0);

        // Style should be applied
        Assert.NotEmpty(line);
        Assert.Contains("class:custom", line[0].Style);
    }

    [Fact]
    public void Constructor_WithFocusable_SetsFocusable()
    {
        var control = new FormattedTextControl("Test", focusable: new FilterOrBool(true));

        Assert.True(control.IsFocusable);
    }

    [Fact]
    public void Constructor_WithCallable_EvaluatesLazily()
    {
        int callCount = 0;
        var control = new FormattedTextControl(() =>
        {
            callCount++;
            return [new StyleAndTextTuple("", "Dynamic")];
        });

        Assert.Equal(0, callCount);

        control.CreateContent(80, 24);
        Assert.True(callCount > 0);
    }

    #endregion

    #region CreateContent Tests

    [Fact]
    public void CreateContent_SingleLine_ReturnsCorrectLineCount()
    {
        var control = new FormattedTextControl("Hello");

        var content = control.CreateContent(80, 24);

        Assert.Equal(1, content.LineCount);
    }

    [Fact]
    public void CreateContent_MultiLine_ReturnsCorrectLineCount()
    {
        var control = new FormattedTextControl("Line 1\nLine 2\nLine 3");

        var content = control.CreateContent(80, 24);

        Assert.Equal(3, content.LineCount);
    }

    [Fact]
    public void CreateContent_EmptyText_ReturnsOneLine()
    {
        var control = new FormattedTextControl("");

        var content = control.CreateContent(80, 24);

        Assert.Equal(1, content.LineCount);
    }

    [Fact]
    public void CreateContent_PreservesFragmentBoundaries()
    {
        var fragments = new[]
        {
            new StyleAndTextTuple("class:a", "Hello "),
            new StyleAndTextTuple("class:b", "World")
        };
        var control = new FormattedTextControl(fragments);

        var content = control.CreateContent(80, 24);
        var line = content.GetLine(0);

        Assert.Equal(2, line.Count);
        Assert.Equal("Hello ", line[0].Text);
        Assert.Equal("World", line[1].Text);
    }

    [Fact]
    public void CreateContent_WithNewlineInFragment_SplitsCorrectly()
    {
        var fragments = new[]
        {
            new StyleAndTextTuple("class:test", "Line 1\nLine 2")
        };
        var control = new FormattedTextControl(fragments);

        var content = control.CreateContent(80, 24);

        Assert.Equal(2, content.LineCount);
        Assert.Equal("Line 1", content.GetLine(0)[0].Text);
        Assert.Equal("Line 2", content.GetLine(1)[0].Text);
    }

    #endregion

    #region PreferredWidth Tests

    [Fact]
    public void PreferredWidth_SingleLine_ReturnsLineLength()
    {
        var control = new FormattedTextControl("Hello World");

        var width = control.PreferredWidth(100);

        Assert.Equal(11, width); // "Hello World" = 11 chars
    }

    [Fact]
    public void PreferredWidth_MultiLine_ReturnsLongestLine()
    {
        var control = new FormattedTextControl("Short\nMuch longer line\nMedium");

        var width = control.PreferredWidth(100);

        Assert.Equal(16, width); // "Much longer line" = 16 chars
    }

    [Fact]
    public void PreferredWidth_Unicode_AccountsForWidth()
    {
        var control = new FormattedTextControl("Hello 世界");

        var width = control.PreferredWidth(100);

        // "Hello " = 6, "世界" = 4 (2 chars × 2 width each)
        Assert.Equal(10, width);
    }

    #endregion

    #region PreferredHeight Tests

    [Fact]
    public void PreferredHeight_NoWrap_ReturnsLineCount()
    {
        var control = new FormattedTextControl("Line 1\nLine 2\nLine 3");

        var height = control.PreferredHeight(80, 100, wrapLines: false, getLinePrefix: null);

        Assert.Equal(3, height);
    }

    [Fact]
    public void PreferredHeight_WithWrap_AccountsForWrapping()
    {
        var control = new FormattedTextControl("This is a very long line that should wrap");

        var height = control.PreferredHeight(10, 100, wrapLines: true, getLinePrefix: null);

        // Line should wrap multiple times at width 10
        Assert.True(height > 1);
    }

    #endregion

    #region ShowCursor Tests

    [Fact]
    public void ShowCursor_Default_IsTrue()
    {
        var control = new FormattedTextControl("Test");

        Assert.True(control.ShowCursor);
    }

    [Fact]
    public void ShowCursor_WhenFalse_ContentReflects()
    {
        var control = new FormattedTextControl("Test", showCursor: false);

        var content = control.CreateContent(80, 24);

        Assert.False(content.ShowCursor);
    }

    #endregion

    #region Cursor Position Tests

    [Fact]
    public void CursorPosition_WithCallback_UsesCallback()
    {
        var control = new FormattedTextControl(
            "Test",
            getCursorPosition: () => new Point(5, 0));

        var content = control.CreateContent(80, 24);

        Assert.NotNull(content.CursorPosition);
        Assert.Equal(5, content.CursorPosition.Value.X);
        Assert.Equal(0, content.CursorPosition.Value.Y);
    }

    [Fact]
    public void CursorPosition_WithSetCursorPosition_FindsMarker()
    {
        var fragments = new[]
        {
            new StyleAndTextTuple("", "Hello "),
            new StyleAndTextTuple("[SetCursorPosition]", ""),
            new StyleAndTextTuple("", "World")
        };
        var control = new FormattedTextControl(fragments);

        var content = control.CreateContent(80, 24);

        Assert.NotNull(content.CursorPosition);
        Assert.Equal(6, content.CursorPosition.Value.X); // After "Hello "
    }

    #endregion

    #region Menu Position Tests

    [Fact]
    public void MenuPosition_WithSetMenuPosition_FindsMarker()
    {
        var fragments = new[]
        {
            new StyleAndTextTuple("", "Menu: "),
            new StyleAndTextTuple("[SetMenuPosition]", ""),
            new StyleAndTextTuple("", "Items")
        };
        var control = new FormattedTextControl(fragments);

        var content = control.CreateContent(80, 24);

        Assert.NotNull(content.MenuPosition);
        Assert.Equal(6, content.MenuPosition.Value.X); // After "Menu: "
    }

    [Fact]
    public void MenuPosition_NoMarker_ReturnsNull()
    {
        var control = new FormattedTextControl("No menu marker here");

        var content = control.CreateContent(80, 24);

        Assert.Null(content.MenuPosition);
    }

    #endregion

    #region Reset Tests

    [Fact]
    public void Reset_DoesNotThrow()
    {
        var control = new FormattedTextControl("Test");

        // Should not throw
        control.Reset();
    }

    #endregion

    #region KeyBindings Tests

    [Fact]
    public void GetKeyBindings_NoBindings_ReturnsNull()
    {
        var control = new FormattedTextControl("Test");

        Assert.Null(control.GetKeyBindings());
    }

    #endregion

    #region Thread Safety Tests

    [Fact]
    public async Task CreateContent_ConcurrentCalls_NoExceptions()
    {
        var control = new FormattedTextControl("Test content\nLine 2\nLine 3");
        var ct = TestContext.Current.CancellationToken;

        var tasks = Enumerable.Range(0, 10)
            .Select(_ => Task.Run(() =>
            {
                for (int i = 0; i < 100; i++)
                {
                    var content = control.CreateContent(80, 24);
                    Assert.NotNull(content);
                }
            }, ct))
            .ToArray();

        await Task.WhenAll(tasks);
    }

    #endregion
}
