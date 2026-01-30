using Stroke.Core.Primitives;
using Stroke.Filters;
using Stroke.FormattedText;
using Stroke.Layout;
using Stroke.Layout.Containers;
using Stroke.Layout.Controls;
using Stroke.Layout.Windows;
using Xunit;

namespace Stroke.Tests.Layout.Windows;

/// <summary>
/// Tests for Window cursor line and cursor column highlighting.
/// </summary>
public sealed class WindowCursorHighlightTests
{
    #region Cursorline Tests

    [Fact]
    public void Cursorline_WhenEnabled_AppliesCursorLineStyle()
    {
        var control = new FormattedTextControl("Line 1\nLine 2\nLine 3",
            getCursorPosition: () => new Point(0, 1));
        var window = new Window(
            content: control,
            cursorline: new FilterOrBool(true));

        var screen = new Screen();
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 20, 3);

        window.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);

        // The cursor line (row 1) should have cursor-line style
        var cell = screen[1, 0];
        Assert.Contains("class:cursor-line", cell.Style);
    }

    [Fact]
    public void Cursorline_WhenDisabled_NoCursorLineStyle()
    {
        var control = new FormattedTextControl("Line 1\nLine 2\nLine 3",
            getCursorPosition: () => new Point(0, 1));
        var window = new Window(
            content: control,
            cursorline: new FilterOrBool(false));

        var screen = new Screen();
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 20, 3);

        window.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);

        // No cursor-line style should be present
        var cell = screen[1, 0];
        Assert.DoesNotContain("class:cursor-line", cell.Style);
    }

    [Fact]
    public void Cursorline_Default_IsDisabled()
    {
        var control = new FormattedTextControl("Line 1\nLine 2\nLine 3",
            getCursorPosition: () => new Point(0, 1));
        var window = new Window(content: control);

        var screen = new Screen();
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 20, 3);

        window.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);

        // Default: no cursor-line style
        var cell = screen[1, 0];
        Assert.DoesNotContain("class:cursor-line", cell.Style);
    }

    #endregion

    #region Cursorcolumn Tests

    [Fact]
    public void Cursorcolumn_WhenEnabled_AppliesCursorColumnStyle()
    {
        var control = new FormattedTextControl("Line 1\nLine 2\nLine 3",
            getCursorPosition: () => new Point(3, 1));
        var window = new Window(
            content: control,
            cursorcolumn: new FilterOrBool(true));

        var screen = new Screen();
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 20, 3);

        window.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);

        // Column 3 should have cursor-column style on all rows
        var cell = screen[0, 3];
        Assert.Contains("class:cursor-column", cell.Style);
    }

    [Fact]
    public void Cursorcolumn_WhenDisabled_NoCursorColumnStyle()
    {
        var control = new FormattedTextControl("Line 1\nLine 2\nLine 3",
            getCursorPosition: () => new Point(3, 1));
        var window = new Window(
            content: control,
            cursorcolumn: new FilterOrBool(false));

        var screen = new Screen();
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 20, 3);

        window.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);

        // No cursor-column style
        var cell = screen[0, 3];
        Assert.DoesNotContain("class:cursor-column", cell.Style);
    }

    #endregion

    #region ColorColumn Tests

    [Fact]
    public void ColorColumns_WhenSpecified_AppliesColorColumnStyle()
    {
        var control = new FormattedTextControl("A long line of text");
        var colorColumns = new[] { new ColorColumn(10) };
        var window = new Window(
            content: control,
            colorcolumns: colorColumns);

        var screen = new Screen();
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 20, 1);

        window.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);

        // Column 10 should have color-column style
        var cell = screen[0, 10];
        Assert.Contains("class:color-column", cell.Style);
    }

    [Fact]
    public void ColorColumns_WithCustomStyle_AppliesCustomStyle()
    {
        var control = new FormattedTextControl("A long line of text");
        var colorColumns = new[] { new ColorColumn(10, style: "class:custom-column") };
        var window = new Window(
            content: control,
            colorcolumns: colorColumns);

        var screen = new Screen();
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 20, 1);

        window.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);

        // Column 10 should have custom style
        var cell = screen[0, 10];
        Assert.Contains("class:custom-column", cell.Style);
    }

    [Fact]
    public void ColorColumns_BeyondWidth_Ignored()
    {
        var control = new FormattedTextControl("Short");
        var colorColumns = new[] { new ColorColumn(100) };
        var window = new Window(
            content: control,
            colorcolumns: colorColumns);

        var screen = new Screen();
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 20, 1);

        // Should not throw
        window.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);
    }

    [Fact]
    public void ColorColumns_MultipleColumns_AppliesAllStyles()
    {
        var control = new FormattedTextControl("A very long line of text that is very wide");
        var colorColumns = new[]
        {
            new ColorColumn(5),
            new ColorColumn(15),
            new ColorColumn(25)
        };
        var window = new Window(
            content: control,
            colorcolumns: colorColumns);

        var screen = new Screen();
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 40, 1);

        window.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);

        // All specified columns should have color-column style
        Assert.Contains("class:color-column", screen[0, 5].Style);
        Assert.Contains("class:color-column", screen[0, 15].Style);
        Assert.Contains("class:color-column", screen[0, 25].Style);
    }

    #endregion

    #region Combined Highlighting Tests

    [Fact]
    public void CursorlineAndCursorcolumn_BothEnabled_BothStylesApplied()
    {
        var control = new FormattedTextControl("Line 1\nLine 2\nLine 3",
            getCursorPosition: () => new Point(3, 1));
        var window = new Window(
            content: control,
            cursorline: new FilterOrBool(true),
            cursorcolumn: new FilterOrBool(true));

        var screen = new Screen();
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 20, 3);

        window.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);

        // The intersection cell should have both styles
        var cell = screen[1, 3];
        Assert.Contains("class:cursor-line", cell.Style);
        Assert.Contains("class:cursor-column", cell.Style);
    }

    #endregion
}
