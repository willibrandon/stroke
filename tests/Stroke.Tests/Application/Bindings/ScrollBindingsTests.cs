using Stroke.Application;
using Stroke.Application.Bindings;
using Stroke.Core;
using Stroke.Core.Primitives;
using Stroke.Input.Pipe;
using Stroke.KeyBinding;
using Stroke.Layout;
using Stroke.Layout.Containers;
using Stroke.Layout.Controls;
using Stroke.Output;
using Xunit;
using AppContext = Stroke.Application.AppContext;
using Buffer = Stroke.Core.Buffer;
using Keys = Stroke.Input.Keys;

namespace Stroke.Tests.Application.Bindings;

/// <summary>
/// Tests for the 8 scroll functions in <see cref="ScrollBindings"/>.
/// </summary>
public sealed class ScrollBindingsTests : IDisposable
{
    private readonly SimplePipeInput _input;
    private readonly DummyOutput _output;

    public ScrollBindingsTests()
    {
        _input = new SimplePipeInput();
        _output = new DummyOutput();
    }

    public void Dispose()
    {
        _input.Dispose();
    }

    /// <summary>
    /// Creates a test environment with a buffer containing the specified text,
    /// a window rendered at the given dimensions, and an application wired together.
    /// Returns the buffer, window, app, and disposable scope.
    /// </summary>
    private (Buffer Buffer, Window Window, Stroke.Application.Application<object> App, IDisposable Scope)
        CreateScrollEnvironment(string text, int cursorPosition, int windowWidth, int windowHeight)
    {
        var buffer = new Buffer(document: new Document(text, cursorPosition: cursorPosition));
        var bufferControl = new BufferControl(buffer: buffer);
        var window = new Window(content: bufferControl);
        var layout = new Stroke.Layout.Layout(new AnyContainer(window));
        var app = new Stroke.Application.Application<object>(
            input: _input, output: _output, layout: layout);
        var scope = AppContext.SetApp(app);

        // Render to populate RenderInfo
        var screen = new Screen();
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, windowWidth, windowHeight);
        window.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);

        return (buffer, window, app, scope);
    }

    private static KeyPressEvent CreateEvent(Buffer buffer, IApplication? app = null)
    {
        return new KeyPressEvent(
            keyProcessorRef: null,
            arg: null,
            keySequence: [new Stroke.KeyBinding.KeyPress(Keys.Any)],
            previousKeySequence: [],
            isRepeat: false,
            app: app,
            currentBuffer: buffer);
    }

    /// <summary>
    /// Creates a multiline text with the specified number of lines, each with the given content.
    /// </summary>
    private static string CreateLines(int count, string lineContent = "line")
    {
        return string.Join("\n", Enumerable.Range(0, count).Select(i => $"{lineContent} {i}"));
    }

    #region US1: ScrollPageDown Tests

    [Fact]
    public void ScrollPageDown_UniformLines_SetsVerticalScrollAndCursor()
    {
        // 100 lines, cursor at start, 20-row window
        var text = CreateLines(100);
        var (buffer, window, app, scope) = CreateScrollEnvironment(text, 0, 80, 20);
        using (scope)
        {
            Assert.NotNull(window.RenderInfo);
            var lastVisible = window.RenderInfo!.LastVisibleLine();

            ScrollBindings.ScrollPageDown(CreateEvent(buffer, app));

            // VerticalScroll should be set to at least lastVisible
            Assert.True(window.VerticalScroll >= lastVisible);
            // Cursor should have moved forward
            Assert.True(buffer.CursorPosition > 0);
        }
    }

    [Fact]
    public void ScrollPageDown_AtLastPage_StillMakesForwardProgress()
    {
        // 25 lines, cursor near end, 20-row window
        var text = CreateLines(25);
        // Position cursor at line 20
        var cursorPos = new Document(text).TranslateRowColToIndex(20, 0);
        var (buffer, window, app, scope) = CreateScrollEnvironment(text, cursorPos, 80, 20);
        using (scope)
        {
            var initialScroll = window.VerticalScroll;

            ScrollBindings.ScrollPageDown(CreateEvent(buffer, app));

            // Should still make forward progress (VerticalScroll >= initial + 1)
            Assert.True(window.VerticalScroll >= initialScroll + 1);
        }
    }

    [Fact]
    public void ScrollPageDown_NullRenderInfo_IsNoOp()
    {
        // Window with no WriteToScreen call → no RenderInfo
        var buffer = new Buffer(document: new Document(CreateLines(50)));
        var bufferControl = new BufferControl(buffer: buffer);
        var window = new Window(content: bufferControl);
        var layout = new Stroke.Layout.Layout(new AnyContainer(window));
        var app = new Stroke.Application.Application<object>(
            input: _input, output: _output, layout: layout);
        using var scope = AppContext.SetApp(app);

        Assert.Null(window.RenderInfo);

        var initialCursor = buffer.CursorPosition;
        ScrollBindings.ScrollPageDown(CreateEvent(buffer, app));

        // No-op: cursor and scroll unchanged
        Assert.Equal(initialCursor, buffer.CursorPosition);
        Assert.Equal(0, window.VerticalScroll);
    }

    #endregion

    #region US1: ScrollPageUp Tests

    [Fact]
    public void ScrollPageUp_CursorOnLine40_MovesToFirstVisibleOrAbove()
    {
        // 100 lines, cursor on line 40, 20-row window
        var text = CreateLines(100);
        var cursorPos = new Document(text).TranslateRowColToIndex(40, 0);
        var (buffer, window, app, scope) = CreateScrollEnvironment(text, cursorPos, 80, 20);
        using (scope)
        {
            var initialRow = buffer.Document.CursorPositionRow;

            ScrollBindings.ScrollPageUp(CreateEvent(buffer, app));

            // Cursor should have moved up
            Assert.True(buffer.Document.CursorPositionRow < initialRow);
            // VerticalScroll should be 0
            Assert.Equal(0, window.VerticalScroll);
        }
    }

    [Fact]
    public void ScrollPageUp_VerticalScrollAlready0_CursorStillRepositions()
    {
        // 100 lines, cursor on line 5, scroll already at 0
        var text = CreateLines(100);
        var cursorPos = new Document(text).TranslateRowColToIndex(5, 0);
        var (buffer, window, app, scope) = CreateScrollEnvironment(text, cursorPos, 80, 20);
        using (scope)
        {
            Assert.Equal(0, window.VerticalScroll);

            ScrollBindings.ScrollPageUp(CreateEvent(buffer, app));

            // Cursor should have moved up (at least 1 line)
            Assert.True(buffer.Document.CursorPositionRow < 5);
            Assert.Equal(0, window.VerticalScroll);
        }
    }

    [Fact]
    public void ScrollPageUp_CursorAtLine1_MovesToLine0()
    {
        var text = CreateLines(100);
        var cursorPos = new Document(text).TranslateRowColToIndex(1, 0);
        var (buffer, window, app, scope) = CreateScrollEnvironment(text, cursorPos, 80, 20);
        using (scope)
        {
            ScrollBindings.ScrollPageUp(CreateEvent(buffer, app));

            Assert.Equal(0, buffer.Document.CursorPositionRow);
        }
    }

    #endregion

    #region US2: ScrollForward Tests

    [Fact]
    public void ScrollForward_UniformLines_MovesCursorDownByWindowHeight()
    {
        // 100 uniform lines, 20-row window, cursor at line 0
        var text = CreateLines(100);
        var (buffer, window, app, scope) = CreateScrollEnvironment(text, 0, 80, 20);
        using (scope)
        {
            ScrollBindings.ScrollForward(CreateEvent(buffer, app));

            // With uniform single-row lines, cursor should move down ~19 lines
            // (accumulates heights until reaching windowHeight)
            Assert.True(buffer.Document.CursorPositionRow >= 15,
                $"Expected cursor to move down significantly, but row is {buffer.Document.CursorPositionRow}");
        }
    }

    [Fact]
    public void ScrollForward_CursorNearEndOfBuffer_StopsAtLastLine()
    {
        // 25 lines, cursor on line 22, 20-row window
        var text = CreateLines(25);
        var cursorPos = new Document(text).TranslateRowColToIndex(22, 0);
        var (buffer, window, app, scope) = CreateScrollEnvironment(text, cursorPos, 80, 20);
        using (scope)
        {
            ScrollBindings.ScrollForward(CreateEvent(buffer, app));

            // Cursor should be at or near the last line
            Assert.True(buffer.Document.CursorPositionRow >= 22);
            Assert.True(buffer.Document.CursorPositionRow <= 24);
        }
    }

    [Fact]
    public void ScrollForward_SingleLineBuffer_CursorStaysAtLine0()
    {
        var text = "single line";
        var (buffer, window, app, scope) = CreateScrollEnvironment(text, 0, 80, 20);
        using (scope)
        {
            ScrollBindings.ScrollForward(CreateEvent(buffer, app));

            Assert.Equal(0, buffer.Document.CursorPositionRow);
        }
    }

    [Fact]
    public void ScrollForward_NullRenderInfo_IsNoOp()
    {
        var buffer = new Buffer(document: new Document(CreateLines(50)));
        var bufferControl = new BufferControl(buffer: buffer);
        var window = new Window(content: bufferControl);
        var layout = new Stroke.Layout.Layout(new AnyContainer(window));
        var app = new Stroke.Application.Application<object>(
            input: _input, output: _output, layout: layout);
        using var scope = AppContext.SetApp(app);

        var initialCursor = buffer.CursorPosition;
        ScrollBindings.ScrollForward(CreateEvent(buffer, app));

        Assert.Equal(initialCursor, buffer.CursorPosition);
    }

    #endregion

    #region US2: ScrollBackward Tests

    [Fact]
    public void ScrollBackward_UniformLines_MovesCursorUpByWindowHeight()
    {
        // 100 lines, cursor on line 30, 20-row window
        var text = CreateLines(100);
        var cursorPos = new Document(text).TranslateRowColToIndex(30, 0);
        var (buffer, window, app, scope) = CreateScrollEnvironment(text, cursorPos, 80, 20);
        using (scope)
        {
            ScrollBindings.ScrollBackward(CreateEvent(buffer, app));

            // Cursor should have moved up significantly
            Assert.True(buffer.Document.CursorPositionRow <= 15,
                $"Expected cursor to move up significantly, but row is {buffer.Document.CursorPositionRow}");
        }
    }

    [Fact]
    public void ScrollBackward_CursorNearBeginning_StopsAtLine0()
    {
        // 100 lines, cursor on line 3, 20-row window
        var text = CreateLines(100);
        var cursorPos = new Document(text).TranslateRowColToIndex(3, 0);
        var (buffer, window, app, scope) = CreateScrollEnvironment(text, cursorPos, 80, 20);
        using (scope)
        {
            ScrollBindings.ScrollBackward(CreateEvent(buffer, app));

            Assert.Equal(0, buffer.Document.CursorPositionRow);
        }
    }

    [Fact]
    public void ScrollBackward_CursorAtLine0_StaysAtLine0()
    {
        var text = CreateLines(100);
        var (buffer, window, app, scope) = CreateScrollEnvironment(text, 0, 80, 20);
        using (scope)
        {
            ScrollBindings.ScrollBackward(CreateEvent(buffer, app));

            Assert.Equal(0, buffer.Document.CursorPositionRow);
        }
    }

    #endregion

    #region US3: ScrollHalfPageDown / ScrollHalfPageUp Tests

    [Fact]
    public void ScrollHalfPageDown_UniformLines_MovesCursorByHalfWindowHeight()
    {
        // 100 lines, 20-row window → half = 10 lines
        var text = CreateLines(100);
        var (buffer, window, app, scope) = CreateScrollEnvironment(text, 0, 80, 20);
        using (scope)
        {
            ScrollBindings.ScrollHalfPageDown(CreateEvent(buffer, app));

            // With uniform single-row lines, cursor should move ~9 lines (height accumulation)
            Assert.True(buffer.Document.CursorPositionRow >= 5,
                $"Expected cursor to move down ~10 lines, but row is {buffer.Document.CursorPositionRow}");
            Assert.True(buffer.Document.CursorPositionRow <= 15);
        }
    }

    [Fact]
    public void ScrollHalfPageUp_UniformLines_MovesCursorUpByHalfWindowHeight()
    {
        // 100 lines, cursor at line 20, 20-row window → half = 10 lines
        var text = CreateLines(100);
        var cursorPos = new Document(text).TranslateRowColToIndex(20, 0);
        var (buffer, window, app, scope) = CreateScrollEnvironment(text, cursorPos, 80, 20);
        using (scope)
        {
            ScrollBindings.ScrollHalfPageUp(CreateEvent(buffer, app));

            // Should move up about 10 lines
            Assert.True(buffer.Document.CursorPositionRow <= 15);
            Assert.True(buffer.Document.CursorPositionRow >= 5);
        }
    }

    [Fact]
    public void ScrollHalfPageDown_OddWindowHeight_UsesIntegerDivision()
    {
        // 100 lines, 21-row window → half = 21/2 = 10 (integer division)
        var text = CreateLines(100);
        var (buffer, window, app, scope) = CreateScrollEnvironment(text, 0, 80, 21);
        using (scope)
        {
            ScrollBindings.ScrollHalfPageDown(CreateEvent(buffer, app));

            // With 21-row window, half = 10
            Assert.True(buffer.Document.CursorPositionRow >= 5,
                $"Expected cursor to move ~10 lines with 21-row window, but row is {buffer.Document.CursorPositionRow}");
        }
    }

    [Fact]
    public void ScrollHalfPageDown_NearEndOfBuffer_ClampsToLastLine()
    {
        // 15 lines, cursor at line 12, 20-row window → half = 10
        var text = CreateLines(15);
        var cursorPos = new Document(text).TranslateRowColToIndex(12, 0);
        var (buffer, window, app, scope) = CreateScrollEnvironment(text, cursorPos, 80, 20);
        using (scope)
        {
            ScrollBindings.ScrollHalfPageDown(CreateEvent(buffer, app));

            Assert.True(buffer.Document.CursorPositionRow <= 14);
        }
    }

    #endregion

    #region US4: ScrollOneLineDown Tests

    [Fact]
    public void ScrollOneLineDown_CursorInMiddle_IncrementsScrollOnly()
    {
        // 100 lines, cursor in middle of viewport
        var text = CreateLines(100);
        var cursorPos = new Document(text).TranslateRowColToIndex(10, 0);
        var (buffer, window, app, scope) = CreateScrollEnvironment(text, cursorPos, 80, 20);
        using (scope)
        {
            var initialCursorRow = buffer.Document.CursorPositionRow;
            var initialScroll = window.VerticalScroll;

            // The render info's cursor position should not be at the scroll offset boundary
            // for the scroll to only increment VerticalScroll
            ScrollBindings.ScrollOneLineDown(CreateEvent(buffer, app));

            // VerticalScroll should increment by 1 (if not at max)
            if (window.RenderInfo!.ContentHeight > window.RenderInfo.WindowHeight)
            {
                Assert.Equal(initialScroll + 1, window.VerticalScroll);
            }
        }
    }

    [Fact]
    public void ScrollOneLineDown_AtMaxScroll_IsNoOp()
    {
        // 5 lines in a 20-row window → already fully visible, no scroll needed
        var text = CreateLines(5);
        var (buffer, window, app, scope) = CreateScrollEnvironment(text, 0, 80, 20);
        using (scope)
        {
            var initialScroll = window.VerticalScroll;
            var initialCursor = buffer.CursorPosition;

            ScrollBindings.ScrollOneLineDown(CreateEvent(buffer, app));

            // Content fits in window, so scroll should not change
            Assert.Equal(initialScroll, window.VerticalScroll);
        }
    }

    [Fact]
    public void ScrollOneLineDown_NullRenderInfo_IsNoOp()
    {
        var buffer = new Buffer(document: new Document(CreateLines(50)));
        var bufferControl = new BufferControl(buffer: buffer);
        var window = new Window(content: bufferControl);
        var layout = new Stroke.Layout.Layout(new AnyContainer(window));
        var app = new Stroke.Application.Application<object>(
            input: _input, output: _output, layout: layout);
        using var scope = AppContext.SetApp(app);

        var initialScroll = window.VerticalScroll;
        ScrollBindings.ScrollOneLineDown(CreateEvent(buffer, app));

        Assert.Equal(initialScroll, window.VerticalScroll);
    }

    #endregion

    #region US4: ScrollOneLineUp Tests

    [Fact]
    public void ScrollOneLineUp_VerticalScrollGreaterThan0_DecrementsScroll()
    {
        // 100 lines, cursor in middle, scrolled down
        var text = CreateLines(100);
        var cursorPos = new Document(text).TranslateRowColToIndex(25, 0);
        var (buffer, window, app, scope) = CreateScrollEnvironment(text, cursorPos, 80, 20);
        using (scope)
        {
            var initialScroll = window.VerticalScroll;

            // Only test if we're actually scrolled
            if (initialScroll > 0)
            {
                ScrollBindings.ScrollOneLineUp(CreateEvent(buffer, app));
                Assert.Equal(initialScroll - 1, window.VerticalScroll);
            }
        }
    }

    [Fact]
    public void ScrollOneLineUp_VerticalScrollAt0_IsNoOp()
    {
        // 100 lines, cursor at top, scroll at 0
        var text = CreateLines(100);
        var (buffer, window, app, scope) = CreateScrollEnvironment(text, 0, 80, 20);
        using (scope)
        {
            Assert.Equal(0, window.VerticalScroll);

            var initialCursor = buffer.CursorPosition;
            ScrollBindings.ScrollOneLineUp(CreateEvent(buffer, app));

            Assert.Equal(0, window.VerticalScroll);
        }
    }

    [Fact]
    public void ScrollOneLineUp_NullRenderInfo_IsNoOp()
    {
        var buffer = new Buffer(document: new Document(CreateLines(50)));
        var bufferControl = new BufferControl(buffer: buffer);
        var window = new Window(content: bufferControl);
        var layout = new Stroke.Layout.Layout(new AnyContainer(window));
        var app = new Stroke.Application.Application<object>(
            input: _input, output: _output, layout: layout);
        using var scope = AppContext.SetApp(app);

        var initialScroll = window.VerticalScroll;
        ScrollBindings.ScrollOneLineUp(CreateEvent(buffer, app));

        Assert.Equal(initialScroll, window.VerticalScroll);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void EC001_FewerLinesThanWindowHeight_GracefulClamp()
    {
        // 5 lines in 20-row window
        var text = CreateLines(5);
        var (buffer, window, app, scope) = CreateScrollEnvironment(text, 0, 80, 20);
        using (scope)
        {
            // ScrollForward should not crash, cursor should clamp gracefully
            ScrollBindings.ScrollForward(CreateEvent(buffer, app));
            Assert.True(buffer.Document.CursorPositionRow <= 4);

            // ScrollPageDown should work without error
            ScrollBindings.ScrollPageDown(CreateEvent(buffer, app));
        }
    }

    [Fact]
    public void EC002_CursorAtFirstLine_ScrollBackward_StaysAtLine0()
    {
        var text = CreateLines(50);
        var (buffer, window, app, scope) = CreateScrollEnvironment(text, 0, 80, 20);
        using (scope)
        {
            ScrollBindings.ScrollBackward(CreateEvent(buffer, app));
            Assert.Equal(0, buffer.Document.CursorPositionRow);
        }
    }

    [Fact]
    public void EC003_CursorAtLastLine_ScrollForward_StopsAtLastLine()
    {
        var text = CreateLines(50);
        var lastLinePos = new Document(text).TranslateRowColToIndex(49, 0);
        var (buffer, window, app, scope) = CreateScrollEnvironment(text, lastLinePos, 80, 20);
        using (scope)
        {
            ScrollBindings.ScrollForward(CreateEvent(buffer, app));
            Assert.True(buffer.Document.CursorPositionRow <= 49);
        }
    }

    [Fact]
    public void EC005_SingleLineBuffer_AllScrollsAreNoOp()
    {
        var text = "single line only";
        var (buffer, window, app, scope) = CreateScrollEnvironment(text, 0, 80, 20);
        using (scope)
        {
            var initialCursor = buffer.CursorPosition;

            ScrollBindings.ScrollForward(CreateEvent(buffer, app));
            Assert.Equal(0, buffer.Document.CursorPositionRow);

            buffer.CursorPosition = initialCursor;
            ScrollBindings.ScrollBackward(CreateEvent(buffer, app));
            Assert.Equal(0, buffer.Document.CursorPositionRow);

            buffer.CursorPosition = initialCursor;
            ScrollBindings.ScrollHalfPageDown(CreateEvent(buffer, app));
            Assert.Equal(0, buffer.Document.CursorPositionRow);

            buffer.CursorPosition = initialCursor;
            ScrollBindings.ScrollHalfPageUp(CreateEvent(buffer, app));
            Assert.Equal(0, buffer.Document.CursorPositionRow);
        }
    }

    [Fact]
    public void EC007_EmptyBuffer_AllScrollsAreNoOp()
    {
        var text = "";
        var (buffer, window, app, scope) = CreateScrollEnvironment(text, 0, 80, 20);
        using (scope)
        {
            // None of these should throw
            ScrollBindings.ScrollForward(CreateEvent(buffer, app));
            ScrollBindings.ScrollBackward(CreateEvent(buffer, app));
            ScrollBindings.ScrollHalfPageDown(CreateEvent(buffer, app));
            ScrollBindings.ScrollHalfPageUp(CreateEvent(buffer, app));
            ScrollBindings.ScrollOneLineDown(CreateEvent(buffer, app));
            ScrollBindings.ScrollOneLineUp(CreateEvent(buffer, app));
            ScrollBindings.ScrollPageDown(CreateEvent(buffer, app));
            ScrollBindings.ScrollPageUp(CreateEvent(buffer, app));

            Assert.Equal(0, buffer.CursorPosition);
        }
    }

    [Fact]
    public void EC008_ScrollPageDown_AtLastPage_MakesForwardProgress()
    {
        // 25 lines, already scrolled to near end
        var text = CreateLines(25);
        var cursorPos = new Document(text).TranslateRowColToIndex(22, 0);
        var (buffer, window, app, scope) = CreateScrollEnvironment(text, cursorPos, 80, 20);
        using (scope)
        {
            var initialScroll = window.VerticalScroll;

            ScrollBindings.ScrollPageDown(CreateEvent(buffer, app));

            // Math.Max(lastVisible, scroll + 1) ensures forward progress
            Assert.True(window.VerticalScroll >= initialScroll + 1);
        }
    }

    [Fact]
    public void EC009_ScrollPageUp_ScrollAlready0_CursorStillRepositions()
    {
        var text = CreateLines(50);
        var cursorPos = new Document(text).TranslateRowColToIndex(10, 0);
        var (buffer, window, app, scope) = CreateScrollEnvironment(text, cursorPos, 80, 20);
        using (scope)
        {
            Assert.Equal(0, window.VerticalScroll);

            ScrollBindings.ScrollPageUp(CreateEvent(buffer, app));

            // Cursor should reposition even when scroll is already 0
            Assert.True(buffer.Document.CursorPositionRow < 10);
            Assert.Equal(0, window.VerticalScroll);
        }
    }

    [Fact]
    public void AllScrollFunctions_ReturnNull()
    {
        var text = CreateLines(50);
        var (buffer, window, app, scope) = CreateScrollEnvironment(text, 0, 80, 20);
        using (scope)
        {
            Assert.Null(ScrollBindings.ScrollForward(CreateEvent(buffer, app)));
            Assert.Null(ScrollBindings.ScrollBackward(CreateEvent(buffer, app)));
            Assert.Null(ScrollBindings.ScrollHalfPageDown(CreateEvent(buffer, app)));
            Assert.Null(ScrollBindings.ScrollHalfPageUp(CreateEvent(buffer, app)));
            Assert.Null(ScrollBindings.ScrollOneLineDown(CreateEvent(buffer, app)));
            Assert.Null(ScrollBindings.ScrollOneLineUp(CreateEvent(buffer, app)));
            Assert.Null(ScrollBindings.ScrollPageDown(CreateEvent(buffer, app)));
            Assert.Null(ScrollBindings.ScrollPageUp(CreateEvent(buffer, app)));
        }
    }

    #endregion
}
