using Stroke.Application;
using Stroke.Application.Bindings;
using Stroke.Core;
using Stroke.Filters;
using Stroke.FormattedText;
using Stroke.Input.Pipe;
using Stroke.KeyBinding;
using Stroke.Layout;
using Stroke.Layout.Containers;
using Stroke.Layout.Controls;
using Stroke.Output;
using Stroke.Rendering;
using Xunit;
using AppContext = Stroke.Application.AppContext;
using Buffer = Stroke.Core.Buffer;
using Keys = Stroke.Input.Keys;
using StrokeLayout = Stroke.Layout.Layout;

namespace Stroke.Tests.Application.Bindings;

/// <summary>
/// Tests for <see cref="FocusFunctions"/> and <see cref="CprBindings"/>.
/// </summary>
public sealed class FocusCprBindingsTests : IDisposable
{
    private readonly SimplePipeInput _input;
    private readonly DummyOutput _output;

    public FocusCprBindingsTests()
    {
        _input = new SimplePipeInput();
        _output = new DummyOutput();
    }

    public void Dispose()
    {
        _input.Dispose();
    }

    #region Helpers

    /// <summary>
    /// Creates a focusable window using a FormattedTextControl with focusable=true.
    /// </summary>
    private static Window CreateFocusableWindow()
    {
        var control = new FormattedTextControl(
            Array.Empty<StyleAndTextTuple>(),
            focusable: new FilterOrBool(true));
        return new Window(content: control);
    }

    /// <summary>
    /// Creates a focus test environment with the specified number of focusable windows.
    /// Returns the windows, the application, and a disposable scope.
    /// </summary>
    private (Window[] Windows, Application<object> App, IDisposable Scope)
        CreateFocusEnvironment(int windowCount)
    {
        var windows = new Window[windowCount];
        for (var i = 0; i < windowCount; i++)
            windows[i] = CreateFocusableWindow();

        var container = new HSplit(windows);
        var layout = new StrokeLayout(new AnyContainer(container));
        layout.SetVisibleWindows([.. windows]);
        layout.UpdateParentsRelations();

        var app = new Application<object>(
            input: _input, output: _output, layout: layout);
        var scope = AppContext.SetApp(app.UnsafeCast);

        return (windows, app, scope);
    }

    /// <summary>
    /// Creates a KeyPressEvent for testing focus handler functions.
    /// </summary>
    private static KeyPressEvent CreateEvent(object app)
    {
        return new KeyPressEvent(
            keyProcessorRef: null,
            arg: null,
            keySequence: [new KeyPress(Keys.Any)],
            previousKeySequence: [],
            isRepeat: false,
            app: app);
    }

    /// <summary>
    /// Creates a KeyPressEvent with CPR response data.
    /// </summary>
    private static KeyPressEvent CreateCprEvent(object app, string data)
    {
        return new KeyPressEvent(
            keyProcessorRef: null,
            arg: null,
            keySequence: [new KeyPress(Keys.CPRResponse, data)],
            previousKeySequence: [],
            isRepeat: false,
            app: app);
    }

    /// <summary>
    /// Finds the CPR response binding in the loaded bindings and invokes its handler.
    /// This is required because the CPR handler is a private method referenced as a delegate
    /// inside <see cref="CprBindings.LoadCprBindings"/> and can only be reached through the
    /// binding system.
    /// </summary>
    private static void InvokeCprHandler(KeyBindings kb, KeyPressEvent evt)
    {
        var cprKey = new KeyOrChar(Keys.CPRResponse);
        var binding = kb.Bindings
            .First(b => b.Keys.Contains(cprKey));
        binding.Handler(evt);
    }

    #endregion

    // ═══════════════════════════════════════════════════════════════════════
    // US1: Focus Navigation Between Windows
    // ═══════════════════════════════════════════════════════════════════════

    #region US1 — Focus Navigation (T003, T004)

    [Fact]
    public void FocusNext_ThreeWindows_MovesFromAToB()
    {
        var (windows, app, scope) = CreateFocusEnvironment(3);
        using (scope)
        {
            Assert.Same(windows[0], app.Layout.CurrentWindow);

            var evt = CreateEvent(app);
            FocusFunctions.FocusNext(evt);

            Assert.Same(windows[1], app.Layout.CurrentWindow);
        }
    }

    [Fact]
    public void FocusNext_ThreeWindows_WrapsFromCToA()
    {
        var (windows, app, scope) = CreateFocusEnvironment(3);
        using (scope)
        {
            // Move focus to window C
            app.Layout.CurrentWindow = windows[2];
            Assert.Same(windows[2], app.Layout.CurrentWindow);

            var evt = CreateEvent(app);
            FocusFunctions.FocusNext(evt);

            Assert.Same(windows[0], app.Layout.CurrentWindow);
        }
    }

    [Fact]
    public void FocusPrevious_ThreeWindows_WrapsFromAToC()
    {
        var (windows, app, scope) = CreateFocusEnvironment(3);
        using (scope)
        {
            Assert.Same(windows[0], app.Layout.CurrentWindow);

            var evt = CreateEvent(app);
            FocusFunctions.FocusPrevious(evt);

            Assert.Same(windows[2], app.Layout.CurrentWindow);
        }
    }

    [Fact]
    public void FocusPrevious_ThreeWindows_MovesFromBToA()
    {
        var (windows, app, scope) = CreateFocusEnvironment(3);
        using (scope)
        {
            // Move focus to window B
            app.Layout.CurrentWindow = windows[1];
            Assert.Same(windows[1], app.Layout.CurrentWindow);

            var evt = CreateEvent(app);
            FocusFunctions.FocusPrevious(evt);

            Assert.Same(windows[0], app.Layout.CurrentWindow);
        }
    }

    #endregion

    // ═══════════════════════════════════════════════════════════════════════
    // US2: CPR Response Handling
    // ═══════════════════════════════════════════════════════════════════════

    #region US2 — CPR Response (T008, T009)

    [Fact]
    public void CprHandler_ParsesRow35Col1_ReportsRow35()
    {
        // DummyOutput.GetSize() returns Size(40, 80) → 40 rows
        // Before CPR: _minAvailableHeight = 0 → RowsAboveLayout throws HeightIsUnknownException
        // After CPR with row=35: _minAvailableHeight = 40 - 35 + 1 = 6 → RowsAboveLayout = 40 - 6 = 34
        var (_, app, scope) = CreateFocusEnvironment(1);
        using (scope)
        {
            Assert.Throws<HeightIsUnknownException>(
                () => _ = app.Renderer.RowsAboveLayout);

            var kb = CprBindings.LoadCprBindings();
            var evt = CreateCprEvent(app, "\x1b[35;1R");
            InvokeCprHandler(kb, evt);

            Assert.Equal(34, app.Renderer.RowsAboveLayout);
        }
    }

    [Fact]
    public void CprHandler_ParsesRow1Col80_ReportsRow1()
    {
        // DummyOutput.GetSize() returns Size(40, 80) → 40 rows
        // After CPR with row=1: _minAvailableHeight = 40 - 1 + 1 = 40 → RowsAboveLayout = 40 - 40 = 0
        var (_, app, scope) = CreateFocusEnvironment(1);
        using (scope)
        {
            Assert.Throws<HeightIsUnknownException>(
                () => _ = app.Renderer.RowsAboveLayout);

            var kb = CprBindings.LoadCprBindings();
            var evt = CreateCprEvent(app, "\x1b[1;80R");
            InvokeCprHandler(kb, evt);

            Assert.Equal(0, app.Renderer.RowsAboveLayout);
        }
    }

    [Fact]
    public void CprHandler_ParsesRow100Col40_NoException()
    {
        // DummyOutput.GetSize() returns Size(40, 80) → 40 rows
        // row=100 → _minAvailableHeight = 40 - 100 + 1 = -59 (negative, matching Python behavior)
        // Just assert no exception is thrown during handler execution
        var (_, app, scope) = CreateFocusEnvironment(1);
        using (scope)
        {
            var kb = CprBindings.LoadCprBindings();
            var evt = CreateCprEvent(app, "\x1b[100;40R");

            var exception = Record.Exception(() => InvokeCprHandler(kb, evt));
            Assert.Null(exception);
        }
    }

    [Fact]
    public void LoadCprBindings_SaveBeforeReturnsFalse()
    {
        var kb = CprBindings.LoadCprBindings();
        var cprKey = new KeyOrChar(Keys.CPRResponse);
        var binding = kb.Bindings
            .First(b => b.Keys.Contains(cprKey));

        // SaveBefore should return false for any event
        var evt = new KeyPressEvent(
            keyProcessorRef: null,
            arg: null,
            keySequence: [new KeyPress(Keys.CPRResponse)],
            previousKeySequence: [],
            isRepeat: false);

        Assert.False(binding.SaveBefore(evt));
    }

    #endregion

    // ═══════════════════════════════════════════════════════════════════════
    // US3: Focus Navigation with Single Window
    // ═══════════════════════════════════════════════════════════════════════

    #region US3 — Single Window (T011)

    [Fact]
    public void FocusNext_SingleWindow_FocusRemainsOnSameWindow()
    {
        var (windows, app, scope) = CreateFocusEnvironment(1);
        using (scope)
        {
            Assert.Same(windows[0], app.Layout.CurrentWindow);

            var evt = CreateEvent(app);
            FocusFunctions.FocusNext(evt);

            Assert.Same(windows[0], app.Layout.CurrentWindow);
        }
    }

    [Fact]
    public void FocusPrevious_SingleWindow_FocusRemainsOnSameWindow()
    {
        var (windows, app, scope) = CreateFocusEnvironment(1);
        using (scope)
        {
            Assert.Same(windows[0], app.Layout.CurrentWindow);

            var evt = CreateEvent(app);
            FocusFunctions.FocusPrevious(evt);

            Assert.Same(windows[0], app.Layout.CurrentWindow);
        }
    }

    #endregion

    // ═══════════════════════════════════════════════════════════════════════
    // US4: Focus Navigation with No Focusable Windows
    // ═══════════════════════════════════════════════════════════════════════

    #region US4 — No Focusable Windows (T012)

    [Fact]
    public void FocusNext_NoFocusableWindows_NoException()
    {
        // Create a layout with a non-focusable window (DummyControl, IsFocusable=false)
        var window = new Window(); // default content is DummyControl
        var container = new HSplit([window]);
        var layout = new StrokeLayout(new AnyContainer(container));
        layout.SetVisibleWindows([window]);
        layout.UpdateParentsRelations();

        var app = new Application<object>(
            input: _input, output: _output, layout: layout);
        using var scope = AppContext.SetApp(app.UnsafeCast);

        var evt = CreateEvent(app);
        var exception = Record.Exception(() => FocusFunctions.FocusNext(evt));
        Assert.Null(exception);
    }

    [Fact]
    public void FocusPrevious_NoFocusableWindows_NoException()
    {
        // Create a layout with a non-focusable window (DummyControl, IsFocusable=false)
        var window = new Window(); // default content is DummyControl
        var container = new HSplit([window]);
        var layout = new StrokeLayout(new AnyContainer(container));
        layout.SetVisibleWindows([window]);
        layout.UpdateParentsRelations();

        var app = new Application<object>(
            input: _input, output: _output, layout: layout);
        using var scope = AppContext.SetApp(app.UnsafeCast);

        var evt = CreateEvent(app);
        var exception = Record.Exception(() => FocusFunctions.FocusPrevious(evt));
        Assert.Null(exception);
    }

    #endregion
}
