using Stroke.Application;
using Stroke.Core;
using Stroke.Core.Primitives;
using Stroke.FormattedText;
using Stroke.Input;
using Stroke.Input.Pipe;
using Stroke.KeyBinding;
using Stroke.Layout.Containers;
using Stroke.Layout.Controls;
using Stroke.Layout.Menus;
using Stroke.Output;
using Xunit;

using AppContext = Stroke.Application.AppContext;
using Buffer = Stroke.Core.Buffer;
using CompletionItem = Stroke.Completion.Completion;
using NotImplementedOrNone = Stroke.KeyBinding.NotImplementedOrNone;
using Point = Stroke.Core.Primitives.Point;
using StrokeLayout = Stroke.Layout.Layout;

namespace Stroke.Tests.Layout.Menus;

/// <summary>
/// Tests for CompletionsMenuControl (US1: single-column completion rendering,
/// US2: meta column, US4: mouse handler).
/// </summary>
public sealed class CompletionsMenuControlTests
{
    private static (Application<object?> app, IDisposable scope, Buffer buffer) CreateAppWithCompletions(
        IReadOnlyList<CompletionItem>? completions = null,
        int? selectIndex = null)
    {
        var buffer = new Buffer(document: new Document("hel", cursorPosition: 3));
        var control = new BufferControl(buffer: buffer);
        var window = new Window(content: control);
        var layout = new StrokeLayout(new AnyContainer(window));
        var input = new SimplePipeInput();
        var output = new DummyOutput();
        var app = new Application<object?>(layout: layout, input: input, output: output);
        var scope = AppContext.SetApp(app.UnsafeCast);

        if (completions is not null)
        {
            buffer.SetCompletions(completions);
            if (selectIndex is not null)
            {
                buffer.GoToCompletion(selectIndex.Value);
            }
        }

        return (app, scope, buffer);
    }

    private static List<CompletionItem> CreateSimpleCompletions(int count = 5)
    {
        var completions = new List<CompletionItem>();
        for (int i = 0; i < count; i++)
        {
            completions.Add(new CompletionItem($"item{i}", startPosition: -3));
        }
        return completions;
    }

    #region IsFocusable Tests

    [Fact]
    public void IsFocusable_ReturnsFalse()
    {
        var control = new CompletionsMenuControl();
        Assert.False(control.IsFocusable);
    }

    #endregion

    #region PreferredWidth Tests

    [Fact]
    public void PreferredWidth_NoCompletions_ReturnsZero()
    {
        var (_, scope, _) = CreateAppWithCompletions();
        using (scope)
        {
            var control = new CompletionsMenuControl();
            var width = control.PreferredWidth(100);
            Assert.Equal(0, width);
        }
    }

    [Fact]
    public void PreferredWidth_WithCompletions_ReturnsPositive()
    {
        var completions = CreateSimpleCompletions();
        var (_, scope, _) = CreateAppWithCompletions(completions);
        using (scope)
        {
            var control = new CompletionsMenuControl();
            var width = control.PreferredWidth(100);
            Assert.NotNull(width);
            Assert.True(width > 0);
        }
    }

    [Fact]
    public void PreferredWidth_RespectsMinWidth()
    {
        var completions = new List<CompletionItem> { new("a", startPosition: -3) };
        var (_, scope, _) = CreateAppWithCompletions(completions);
        using (scope)
        {
            var control = new CompletionsMenuControl();
            var width = control.PreferredWidth(100);
            Assert.True(width >= CompletionsMenuControl.MinWidth);
        }
    }

    #endregion

    #region PreferredHeight Tests

    [Fact]
    public void PreferredHeight_NoCompletions_ReturnsZero()
    {
        var (_, scope, _) = CreateAppWithCompletions();
        using (scope)
        {
            var control = new CompletionsMenuControl();
            var height = control.PreferredHeight(50, 20, false, null);
            Assert.Equal(0, height);
        }
    }

    [Fact]
    public void PreferredHeight_WithCompletions_ReturnsCount()
    {
        var completions = CreateSimpleCompletions(7);
        var (_, scope, _) = CreateAppWithCompletions(completions);
        using (scope)
        {
            var control = new CompletionsMenuControl();
            var height = control.PreferredHeight(50, 20, false, null);
            Assert.Equal(7, height);
        }
    }

    #endregion

    #region CreateContent Tests

    [Fact]
    public void CreateContent_NoCompletions_ReturnsEmptyContent()
    {
        var (_, scope, _) = CreateAppWithCompletions();
        using (scope)
        {
            var control = new CompletionsMenuControl();
            var content = control.CreateContent(50, 10);
            Assert.Equal(0, content.LineCount);
        }
    }

    [Fact]
    public void CreateContent_WithCompletions_HasCorrectLineCount()
    {
        var completions = CreateSimpleCompletions(5);
        var (_, scope, _) = CreateAppWithCompletions(completions);
        using (scope)
        {
            var control = new CompletionsMenuControl();
            var content = control.CreateContent(50, 10);
            Assert.Equal(5, content.LineCount);
        }
    }

    [Fact]
    public void CreateContent_WithSelectedCompletion_HasCursorPosition()
    {
        var completions = CreateSimpleCompletions(5);
        var (_, scope, _) = CreateAppWithCompletions(completions, selectIndex: 2);
        using (scope)
        {
            var control = new CompletionsMenuControl();
            var content = control.CreateContent(50, 10);
            Assert.NotNull(content.CursorPosition);
            Assert.Equal(2, content.CursorPosition.Value.Y);
        }
    }

    [Fact]
    public void CreateContent_NoSelection_CursorAtZero()
    {
        var completions = CreateSimpleCompletions(5);
        var (_, scope, _) = CreateAppWithCompletions(completions);
        using (scope)
        {
            var control = new CompletionsMenuControl();
            var content = control.CreateContent(50, 10);
            Assert.NotNull(content.CursorPosition);
            Assert.Equal(0, content.CursorPosition.Value.Y);
        }
    }

    [Fact]
    public void CreateContent_EachLine_HasContent()
    {
        var completions = CreateSimpleCompletions(3);
        var (_, scope, _) = CreateAppWithCompletions(completions);
        using (scope)
        {
            var control = new CompletionsMenuControl();
            var content = control.CreateContent(50, 10);

            for (int i = 0; i < content.LineCount; i++)
            {
                var line = content.GetLine(i);
                Assert.NotEmpty(line);
                var text = string.Concat(line.Select(f => f.Text));
                Assert.NotEmpty(text.Trim());
            }
        }
    }

    #endregion

    #region Meta Column Tests (US2)

    [Fact]
    public void CreateContent_WithMeta_IncludesMetaColumn()
    {
        AnyFormattedText meta = "some meta";
        var completions = new List<CompletionItem>
        {
            new("hello", startPosition: -3, displayMeta: meta),
            new("world", startPosition: -3, displayMeta: meta)
        };
        var (_, scope, _) = CreateAppWithCompletions(completions);
        using (scope)
        {
            var control = new CompletionsMenuControl();
            var content = control.CreateContent(80, 10);

            // Each line should contain meta text
            for (int i = 0; i < content.LineCount; i++)
            {
                var line = content.GetLine(i);
                var allText = string.Concat(line.Select(f => f.Text));
                Assert.Contains("some meta", allText);
            }
        }
    }

    [Fact]
    public void PreferredWidth_WithMeta_IncludesMetaWidth()
    {
        AnyFormattedText meta = "metadata";
        var completions = new List<CompletionItem>
        {
            new("hello", startPosition: -3, displayMeta: meta)
        };
        var (_, scope, _) = CreateAppWithCompletions(completions);
        using (scope)
        {
            var control = new CompletionsMenuControl();
            var widthWithMeta = control.PreferredWidth(100);

            Assert.NotNull(widthWithMeta);
            Assert.True(widthWithMeta > CompletionsMenuControl.MinWidth);
        }
    }

    [Fact]
    public void CreateContent_NoMeta_OmitsMetaColumn()
    {
        var completions = CreateSimpleCompletions(3);
        var (_, scope, _) = CreateAppWithCompletions(completions);
        using (scope)
        {
            var control = new CompletionsMenuControl();
            var content = control.CreateContent(50, 10);

            // Lines should only contain the completion text + padding, no meta styles
            for (int i = 0; i < content.LineCount; i++)
            {
                var line = content.GetLine(i);
                var styles = line.Select(f => f.Style).ToList();
                Assert.DoesNotContain(styles, s => s.Contains("meta"));
            }
        }
    }

    #endregion

    #region Mouse Handler Tests (US4)

    [Fact]
    public void MouseHandler_ScrollDown_NavigatesNext()
    {
        var completions = CreateSimpleCompletions(10);
        var (_, scope, buffer) = CreateAppWithCompletions(completions, selectIndex: 0);
        using (scope)
        {
            var control = new CompletionsMenuControl();
            var mouseEvent = new MouseEvent(
                new Point(0, 0),
                MouseEventType.ScrollDown,
                MouseButton.None,
                MouseModifiers.None);

            var result = control.MouseHandler(mouseEvent);

            // Should handle the event (return None, not NotImplemented)
            Assert.Same(NotImplementedOrNone.None, result);
        }
    }

    [Fact]
    public void MouseHandler_ScrollUp_NavigatesPrevious()
    {
        var completions = CreateSimpleCompletions(10);
        var (_, scope, buffer) = CreateAppWithCompletions(completions, selectIndex: 5);
        using (scope)
        {
            var control = new CompletionsMenuControl();
            var mouseEvent = new MouseEvent(
                new Point(0, 0),
                MouseEventType.ScrollUp,
                MouseButton.None,
                MouseModifiers.None);

            var result = control.MouseHandler(mouseEvent);

            Assert.Same(NotImplementedOrNone.None, result);
        }
    }

    [Fact]
    public void MouseHandler_MouseUp_SelectsAndCloses()
    {
        var completions = CreateSimpleCompletions(5);
        var (_, scope, buffer) = CreateAppWithCompletions(completions);
        using (scope)
        {
            var control = new CompletionsMenuControl();
            var mouseEvent = new MouseEvent(
                new Point(0, 2),
                MouseEventType.MouseUp,
                MouseButton.Left,
                MouseModifiers.None);

            var result = control.MouseHandler(mouseEvent);

            // Should handle the event
            Assert.Same(NotImplementedOrNone.None, result);
            // Completion state should be cleared after click
            Assert.Null(buffer.CompleteState);
        }
    }

    [Fact]
    public void MouseHandler_UnhandledEvent_ReturnsNotImplemented()
    {
        var completions = CreateSimpleCompletions(5);
        var (_, scope, _) = CreateAppWithCompletions(completions);
        using (scope)
        {
            var control = new CompletionsMenuControl();
            var mouseEvent = new MouseEvent(
                new Point(0, 0),
                MouseEventType.MouseMove,
                MouseButton.None,
                MouseModifiers.None);

            var result = control.MouseHandler(mouseEvent);

            Assert.Same(NotImplementedOrNone.NotImplemented, result);
        }
    }

    #endregion
}
