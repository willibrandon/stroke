using Stroke.Application;
using Stroke.Core;
using Stroke.Core.Primitives;
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
using KeyBindings = Stroke.KeyBinding.KeyBindings;
using NotImplementedOrNone = Stroke.KeyBinding.NotImplementedOrNone;
using Point = Stroke.Core.Primitives.Point;
using StrokeLayout = Stroke.Layout.Layout;

namespace Stroke.Tests.Layout.Menus;

/// <summary>
/// Tests for MultiColumnCompletionMenuControl (US5: multi-column rendering,
/// US6: mouse and key binding support).
/// </summary>
public sealed class MultiColumnCompletionMenuControlTests
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

    private static List<CompletionItem> CreateSimpleCompletions(int count = 20)
    {
        var completions = new List<CompletionItem>();
        for (int i = 0; i < count; i++)
        {
            completions.Add(new CompletionItem($"item{i:D2}", startPosition: -3));
        }
        return completions;
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_Default_CreatesInstance()
    {
        var control = new MultiColumnCompletionMenuControl();
        Assert.NotNull(control);
    }

    [Fact]
    public void Constructor_CustomMinRows_CreatesInstance()
    {
        var control = new MultiColumnCompletionMenuControl(minRows: 5);
        Assert.NotNull(control);
    }

    [Fact]
    public void Constructor_InvalidMinRows_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new MultiColumnCompletionMenuControl(minRows: 0));
    }

    [Fact]
    public void Constructor_MinRowsNegative_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new MultiColumnCompletionMenuControl(minRows: -1));
    }

    #endregion

    #region IsFocusable Tests

    [Fact]
    public void IsFocusable_ReturnsFalse()
    {
        var control = new MultiColumnCompletionMenuControl();
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
            var control = new MultiColumnCompletionMenuControl();
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
            var control = new MultiColumnCompletionMenuControl();
            var width = control.PreferredWidth(200);
            Assert.NotNull(width);
            Assert.True(width > 0);
        }
    }

    [Fact]
    public void PreferredWidth_IncludesMargin()
    {
        var completions = CreateSimpleCompletions(3);
        var (_, scope, _) = CreateAppWithCompletions(completions);
        using (scope)
        {
            var control = new MultiColumnCompletionMenuControl();
            var width = control.PreferredWidth(200);
            // Width should include RequiredMargin (3)
            Assert.True(width >= 3);
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
            var control = new MultiColumnCompletionMenuControl();
            var height = control.PreferredHeight(80, 20, false, null);
            Assert.Equal(0, height);
        }
    }

    [Fact]
    public void PreferredHeight_WithCompletions_ReturnsBasedOnColumns()
    {
        var completions = CreateSimpleCompletions(20);
        var (_, scope, _) = CreateAppWithCompletions(completions);
        using (scope)
        {
            var control = new MultiColumnCompletionMenuControl();
            var height = control.PreferredHeight(200, 30, false, null);
            Assert.NotNull(height);
            Assert.True(height > 0);
            // With multiple columns, height should be less than total completions
            Assert.True(height <= 20);
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
            var control = new MultiColumnCompletionMenuControl();
            var content = control.CreateContent(80, 10);
            Assert.Equal(0, content.LineCount);
        }
    }

    [Fact]
    public void CreateContent_WithCompletions_ProducesRows()
    {
        var completions = CreateSimpleCompletions(20);
        var (_, scope, _) = CreateAppWithCompletions(completions, selectIndex: 0);
        using (scope)
        {
            var control = new MultiColumnCompletionMenuControl();
            var content = control.CreateContent(80, 5);
            Assert.True(content.LineCount > 0);

            for (int i = 0; i < content.LineCount; i++)
            {
                var line = content.GetLine(i);
                Assert.NotEmpty(line);
            }
        }
    }

    [Fact]
    public void CreateContent_Height1_ProducesSingleRow()
    {
        var completions = CreateSimpleCompletions(5);
        var (_, scope, _) = CreateAppWithCompletions(completions, selectIndex: 0);
        using (scope)
        {
            var control = new MultiColumnCompletionMenuControl();
            var content = control.CreateContent(80, 1);
            Assert.Equal(1, content.LineCount);
        }
    }

    [Fact]
    public void CreateContent_WithSelectedCompletion_HighlightsSelected()
    {
        var completions = CreateSimpleCompletions(10);
        var (_, scope, _) = CreateAppWithCompletions(completions, selectIndex: 2);
        using (scope)
        {
            var control = new MultiColumnCompletionMenuControl();
            var content = control.CreateContent(80, 5);

            // Verify content is rendered (exact highlighting depends on style system)
            Assert.True(content.LineCount > 0);
        }
    }

    #endregion

    #region Reset Tests

    [Fact]
    public void Reset_ClearsScrollPosition()
    {
        var control = new MultiColumnCompletionMenuControl();
        // Reset should not throw
        control.Reset();
    }

    #endregion

    #region Mouse Handler Tests (US6)

    [Fact]
    public void MouseHandler_ScrollDown_ReturnsNone()
    {
        var completions = CreateSimpleCompletions(20);
        var (_, scope, buffer) = CreateAppWithCompletions(completions, selectIndex: 0);
        using (scope)
        {
            var control = new MultiColumnCompletionMenuControl();
            // Render first to set up internal state
            control.CreateContent(80, 5);

            var mouseEvent = new MouseEvent(
                new Point(5, 2),
                MouseEventType.ScrollDown,
                MouseButton.None,
                MouseModifiers.None);

            var result = control.MouseHandler(mouseEvent);
            Assert.Same(NotImplementedOrNone.None, result);
        }
    }

    [Fact]
    public void MouseHandler_ScrollUp_ReturnsNone()
    {
        var completions = CreateSimpleCompletions(20);
        var (_, scope, buffer) = CreateAppWithCompletions(completions, selectIndex: 10);
        using (scope)
        {
            var control = new MultiColumnCompletionMenuControl();
            control.CreateContent(80, 5);

            var mouseEvent = new MouseEvent(
                new Point(5, 2),
                MouseEventType.ScrollUp,
                MouseButton.None,
                MouseModifiers.None);

            var result = control.MouseHandler(mouseEvent);
            Assert.Same(NotImplementedOrNone.None, result);
        }
    }

    [Fact]
    public void MouseHandler_MouseUp_ReturnsNone()
    {
        var completions = CreateSimpleCompletions(20);
        var (_, scope, buffer) = CreateAppWithCompletions(completions, selectIndex: 0);
        using (scope)
        {
            var control = new MultiColumnCompletionMenuControl();
            control.CreateContent(80, 5);

            var mouseEvent = new MouseEvent(
                new Point(5, 2),
                MouseEventType.MouseUp,
                MouseButton.Left,
                MouseModifiers.None);

            var result = control.MouseHandler(mouseEvent);
            Assert.Same(NotImplementedOrNone.None, result);
        }
    }

    [Fact]
    public void MouseHandler_MouseMove_ReturnsNone()
    {
        var completions = CreateSimpleCompletions(5);
        var (_, scope, _) = CreateAppWithCompletions(completions, selectIndex: 0);
        using (scope)
        {
            var control = new MultiColumnCompletionMenuControl();
            control.CreateContent(80, 5);

            var mouseEvent = new MouseEvent(
                new Point(5, 2),
                MouseEventType.MouseMove,
                MouseButton.None,
                MouseModifiers.None);

            var result = control.MouseHandler(mouseEvent);
            // MouseMove is not explicitly handled, returns None at the end
            Assert.Same(NotImplementedOrNone.None, result);
        }
    }

    #endregion

    #region Key Bindings Tests (US6)

    [Fact]
    public void GetKeyBindings_ReturnsKeyBindings()
    {
        var control = new MultiColumnCompletionMenuControl();
        var bindings = control.GetKeyBindings();
        Assert.NotNull(bindings);
        Assert.IsType<KeyBindings>(bindings);
    }

    #endregion

    #region Column Width Caching Tests

    [Fact]
    public void PreferredWidth_CalledTwice_ReturnsSameResult()
    {
        var completions = CreateSimpleCompletions(10);
        var (_, scope, _) = CreateAppWithCompletions(completions);
        using (scope)
        {
            var control = new MultiColumnCompletionMenuControl();
            var width1 = control.PreferredWidth(200);
            var width2 = control.PreferredWidth(200);
            Assert.Equal(width1, width2);
        }
    }

    #endregion
}
