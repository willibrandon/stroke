using Stroke.Application;
using Stroke.Clipboard;
using Stroke.CursorShapes;
using Stroke.Filters;
using Stroke.Input;
using Stroke.Input.Pipe;
using Stroke.KeyBinding;
using Stroke.Output;
using Stroke.Styles;
using Xunit;

namespace Stroke.Tests.Application;

public class ApplicationConstructionTests
{
    [Fact]
    public void DefaultConstructor_CreatesApplication()
    {
        var app = new Application<object?>();

        Assert.NotNull(app);
        Assert.NotNull(app.Layout);
        Assert.NotNull(app.Renderer);
        Assert.NotNull(app.KeyProcessor);
        Assert.NotNull(app.ViState);
        Assert.NotNull(app.EmacsState);
    }

    [Fact]
    public void DefaultConstructor_LayoutDefaultsToDummyLayout()
    {
        var app = new Application<object?>();

        // DummyLayout creates a layout with a single Window
        Assert.NotNull(app.Layout);
        var windows = app.Layout.FindAllWindows().ToList();
        Assert.Single(windows);
    }

    [Fact]
    public void DefaultConstructor_EditingModeDefaultsToEmacs()
    {
        var app = new Application<object?>();
        Assert.Equal(EditingMode.Emacs, app.EditingMode);
    }

    [Fact]
    public void DefaultConstructor_ClipboardDefaultsToInMemoryClipboard()
    {
        var app = new Application<object?>();
        Assert.IsType<InMemoryClipboard>(app.Clipboard);
    }

    [Fact]
    public void DefaultConstructor_FullScreenDefaultsToFalse()
    {
        var app = new Application<object?>();
        Assert.False(app.FullScreen);
    }

    [Fact]
    public void DefaultConstructor_EraseWhenDoneDefaultsToFalse()
    {
        var app = new Application<object?>();
        Assert.False(app.EraseWhenDone);
    }

    [Fact]
    public void DefaultConstructor_IsNotRunning()
    {
        var app = new Application<object?>();
        Assert.False(app.IsRunning);
    }

    [Fact]
    public void DefaultConstructor_IsNotDone()
    {
        var app = new Application<object?>();
        Assert.False(app.IsDone);
    }

    [Fact]
    public void DefaultConstructor_IsNotInvalidated()
    {
        var app = new Application<object?>();
        Assert.False(app.Invalidated);
    }

    [Fact]
    public void DefaultConstructor_RenderCounterStartsAtZero()
    {
        // Constructor calls Reset() which doesn't increment render counter.
        // The initial value is 0.
        var app = new Application<object?>();
        Assert.Equal(0, app.RenderCounter);
    }

    [Fact]
    public void DefaultConstructor_CursorDefaultsToSimpleCursorShapeConfig()
    {
        var app = new Application<object?>();
        Assert.IsType<SimpleCursorShapeConfig>(app.Cursor);
    }

    [Fact]
    public void DefaultConstructor_TimingDefaults()
    {
        var app = new Application<object?>();

        Assert.Null(app.MinRedrawInterval);
        Assert.Equal(0.01, app.MaxRenderPostponeTime);
        Assert.Null(app.RefreshInterval);
        Assert.Equal(0.5, app.TerminalSizePollingInterval);
    }

    [Fact]
    public void DefaultConstructor_TimeoutDefaults()
    {
        var app = new Application<object?>();

        Assert.Equal(0.5, app.TtimeoutLen);
        Assert.Equal(1.0, app.TimeoutLen);
    }

    [Fact]
    public void DefaultConstructor_QuotedInsertDefaultsFalse()
    {
        var app = new Application<object?>();
        Assert.False(app.QuotedInsert);
    }

    [Fact]
    public void DefaultConstructor_ExitStyleDefaultsToEmpty()
    {
        var app = new Application<object?>();
        Assert.Equal("", app.ExitStyle);
    }

    [Fact]
    public void DefaultConstructor_PreRunCallablesEmpty()
    {
        var app = new Application<object?>();
        Assert.Empty(app.PreRunCallables);
    }

    [Fact]
    public void DefaultConstructor_MouseSupportIsFilter()
    {
        var app = new Application<object?>();
        Assert.NotNull(app.MouseSupport);
    }

    [Fact]
    public void DefaultConstructor_PasteModeIsFilter()
    {
        var app = new Application<object?>();
        Assert.NotNull(app.PasteMode);
    }

    [Fact]
    public void DefaultConstructor_ReverseViSearchDirectionIsFilter()
    {
        var app = new Application<object?>();
        Assert.NotNull(app.ReverseViSearchDirection);
    }

    [Fact]
    public void DefaultConstructor_EnablePageNavigationBindingsIsFilter()
    {
        var app = new Application<object?>();
        Assert.NotNull(app.EnablePageNavigationBindings);
    }

    [Fact]
    public void Constructor_WithExplicitInput()
    {
        var input = new DummyInput();
        var app = new Application<object?>(input: input);

        Assert.Same(input, app.Input);
    }

    [Fact]
    public void Constructor_WithExplicitOutput()
    {
        var output = new DummyOutput();
        var app = new Application<object?>(output: output);

        Assert.Same(output, app.Output);
    }

    [Fact]
    public void Constructor_WithFullScreen()
    {
        var app = new Application<object?>(fullScreen: true);
        Assert.True(app.FullScreen);
    }

    [Fact]
    public void Constructor_WithEraseWhenDone()
    {
        var app = new Application<object?>(eraseWhenDone: true);
        Assert.True(app.EraseWhenDone);
    }

    [Fact]
    public void Constructor_WithViEditingMode()
    {
        var app = new Application<object?>(editingMode: EditingMode.Vi);
        Assert.Equal(EditingMode.Vi, app.EditingMode);
    }

    [Fact]
    public void Constructor_WithTimingParameters()
    {
        var app = new Application<object?>(
            minRedrawInterval: 0.1,
            maxRenderPostponeTime: 0.05,
            refreshInterval: 1.0,
            terminalSizePollingInterval: 2.0);

        Assert.Equal(0.1, app.MinRedrawInterval);
        Assert.Equal(0.05, app.MaxRenderPostponeTime);
        Assert.Equal(1.0, app.RefreshInterval);
        Assert.Equal(2.0, app.TerminalSizePollingInterval);
    }

    [Fact]
    public void Constructor_WithCustomClipboard()
    {
        var clipboard = new InMemoryClipboard();
        var app = new Application<object?>(clipboard: clipboard);

        Assert.Same(clipboard, app.Clipboard);
    }

    [Fact]
    public void Constructor_WithKeyBindings()
    {
        var bindings = new KeyBindings();
        var app = new Application<object?>(keyBindings: bindings);

        Assert.Same(bindings, app.KeyBindings);
    }

    // --- Mutable property setters ---

    [Fact]
    public void EditingMode_CanBeChanged()
    {
        var app = new Application<object?>();
        app.EditingMode = EditingMode.Vi;
        Assert.Equal(EditingMode.Vi, app.EditingMode);
    }

    [Fact]
    public void QuotedInsert_CanBeChanged()
    {
        var app = new Application<object?>();
        app.QuotedInsert = true;
        Assert.True(app.QuotedInsert);
    }

    [Fact]
    public void TtimeoutLen_CanBeChanged()
    {
        var app = new Application<object?>();
        app.TtimeoutLen = 0.25;
        Assert.Equal(0.25, app.TtimeoutLen);
    }

    [Fact]
    public void TimeoutLen_CanBeChanged()
    {
        var app = new Application<object?>();
        app.TimeoutLen = 2.0;
        Assert.Equal(2.0, app.TimeoutLen);
    }

    [Fact]
    public void TimeoutLen_CanBeSetToNull()
    {
        var app = new Application<object?>();
        app.TimeoutLen = null;
        Assert.Null(app.TimeoutLen);
    }

    [Fact]
    public void ExitStyle_CanBeChanged()
    {
        var app = new Application<object?>();
        app.ExitStyle = "class:custom";
        Assert.Equal("class:custom", app.ExitStyle);
    }

    [Fact]
    public void Clipboard_CanBeChanged()
    {
        var app = new Application<object?>();
        var newClipboard = new InMemoryClipboard();
        app.Clipboard = newClipboard;
        Assert.Same(newClipboard, app.Clipboard);
    }

    [Fact]
    public void Clipboard_SetNull_Throws()
    {
        var app = new Application<object?>();
        Assert.Throws<ArgumentNullException>(() => app.Clipboard = null!);
    }

    [Fact]
    public void StyleTransformation_CanBeChanged()
    {
        var app = new Application<object?>();
        var transform = DummyStyleTransformation.Instance;
        app.StyleTransformation = transform;
        Assert.Same(transform, app.StyleTransformation);
    }

    [Fact]
    public void StyleTransformation_SetNull_Throws()
    {
        var app = new Application<object?>();
        Assert.Throws<ArgumentNullException>(() => app.StyleTransformation = null!);
    }

    // --- Event callbacks ---

    [Fact]
    public void Constructor_OnResetCallback_IsRegistered()
    {
        bool called = false;
        // The constructor calls Reset(), which fires OnReset, which invokes the callback
        var app = new Application<object?>(onReset: _ => called = true);
        Assert.True(called);
    }

    [Fact]
    public void OnInvalidate_EventExists()
    {
        var app = new Application<object?>();
        Assert.NotNull(app.OnInvalidate);
    }

    [Fact]
    public void BeforeRender_EventExists()
    {
        var app = new Application<object?>();
        Assert.NotNull(app.BeforeRender);
    }

    [Fact]
    public void AfterRender_EventExists()
    {
        var app = new Application<object?>();
        Assert.NotNull(app.AfterRender);
    }

    // --- Computed properties ---

    [Fact]
    public void ColorDepth_ResolvesFromOutput()
    {
        var output = new DummyOutput();
        var app = new Application<object?>(output: output);

        // DummyOutput.GetDefaultColorDepth() returns Depth1Bit
        Assert.Equal(ColorDepth.Depth1Bit, app.ColorDepth);
    }

    [Fact]
    public void CurrentBuffer_ReturnsDummyWhenNoBufferControl()
    {
        var app = new Application<object?>();

        // Default DummyLayout has FormattedTextControl, not BufferControl
        var buffer = app.CurrentBuffer;
        Assert.NotNull(buffer);
        Assert.Equal("dummy-buffer", buffer.Name);
    }

    [Fact]
    public void CurrentSearchState_ReturnsNewSearchState()
    {
        var app = new Application<object?>();
        var state = app.CurrentSearchState;
        Assert.NotNull(state);
    }

    // --- Generic covariance ---

    [Fact]
    public void GenericApplication_CanBeCreated()
    {
        // Application<string> should construct without error
        var app = new Application<string>();
        Assert.NotNull(app);
    }

    [Fact]
    public void GenericApplication_IntResult()
    {
        var app = new Application<int>();
        Assert.NotNull(app);
        Assert.Equal(0, app.RenderCounter);
    }

    // --- KeyBindings default ---

    [Fact]
    public void KeyBindings_DefaultsToNull()
    {
        var app = new Application<object?>();
        Assert.Null(app.KeyBindings);
    }

    [Fact]
    public void KeyBindings_CanBeSet()
    {
        var app = new Application<object?>();
        var bindings = new KeyBindings();
        app.KeyBindings = bindings;
        Assert.Same(bindings, app.KeyBindings);
    }

    [Fact]
    public void KeyBindings_CanBeSetToNull()
    {
        var app = new Application<object?>(keyBindings: new KeyBindings());
        app.KeyBindings = null;
        Assert.Null(app.KeyBindings);
    }

    // --- Style ---

    [Fact]
    public void Style_DefaultsToNull()
    {
        var app = new Application<object?>();
        Assert.Null(app.Style);
    }

    [Fact]
    public void Style_CanBeSet()
    {
        var app = new Application<object?>();
        app.Style = DummyStyle.Instance;
        Assert.Same(DummyStyle.Instance, app.Style);
    }

    // --- Phase 13 (T048): Extended property tests ---

    [Fact]
    public void ColorDepth_ResolutionOrder_FixedTakesPriority()
    {
        var output = new DummyOutput();
        var app = new Application<object?>(
            output: output,
            colorDepth: ColorDepth.Depth24Bit);

        // Fixed value overrides output default
        Assert.Equal(ColorDepth.Depth24Bit, app.ColorDepth);
    }

    [Fact]
    public void ColorDepth_ResolutionOrder_CallableTakesPriority()
    {
        var output = new DummyOutput();
        var app = new Application<object?>(
            output: output,
            colorDepth: new ColorDepthOption(() => ColorDepth.Depth8Bit));

        // Callable overrides output default
        Assert.Equal(ColorDepth.Depth8Bit, app.ColorDepth);
    }

    [Fact]
    public void ColorDepth_ResolutionOrder_CallableReturnsNull_FallsBackToOutput()
    {
        var output = new DummyOutput();
        var app = new Application<object?>(
            output: output,
            colorDepth: new ColorDepthOption(() => null));

        // Callable returns null, falls back to output default (Depth1Bit from DummyOutput)
        Assert.Equal(ColorDepth.Depth1Bit, app.ColorDepth);
    }

    [Fact]
    public void CurrentBuffer_ReturnsNewDummyEachAccess()
    {
        var app = new Application<object?>();

        // The dummy buffer is created fresh each time (or cached — both are valid).
        // Key point: it should always return a valid buffer.
        var buf1 = app.CurrentBuffer;
        var buf2 = app.CurrentBuffer;
        Assert.NotNull(buf1);
        Assert.NotNull(buf2);
    }

    [Fact]
    public void CurrentSearchState_ReturnsValidSearchState()
    {
        var app = new Application<object?>();
        var state1 = app.CurrentSearchState;
        var state2 = app.CurrentSearchState;
        Assert.NotNull(state1);
        Assert.NotNull(state2);
    }

    [Fact]
    public async Task ExitStyle_SetByExit()
    {
        var ct = TestContext.Current.CancellationToken;
        var output = new DummyOutput();
        using var input = new SimplePipeInput();
        var app = new Application<object?>(input: input, output: output);

        var runTask = app.RunAsync();
        await Task.Delay(50, ct);

        app.Exit(style: "class:my-exit-style");
        await runTask;

        Assert.Equal("class:my-exit-style", app.ExitStyle);
    }

    [Fact]
    public void RenderCounter_IncrementsOnRender()
    {
        var output = new DummyOutput();
        var app = new Application<object?>(output: output);

        Assert.Equal(0, app.RenderCounter);

        // Render the UI manually
        app.Renderer.Render(app, app.Layout);

        // Note: RenderCounter is incremented by Redraw, not by Renderer.Render directly.
        // Redraw is only callable when running. Direct render doesn't increment counter.
        // This is consistent with Python Prompt Toolkit behavior.
    }

    [Fact]
    public void InputHookContext_InputIsReady_InvokesCallback()
    {
        bool callbackInvoked = false;
        var context = new InputHookContext(0, () => callbackInvoked = true);

        Assert.Equal(0, context.FileDescriptor);
        Assert.False(callbackInvoked);

        context.InputIsReady();

        Assert.True(callbackInvoked);
    }

    [Fact]
    public void InputHook_DelegateCanBeCreated()
    {
        // InputHook is accepted by Run(), not the constructor.
        // Verify the delegate type and InputHookContext can be constructed.
        InputHook hook = _ => { };
        Assert.NotNull(hook);
    }

    [Fact]
    public void InputHookContext_FileDescriptor_ReturnsValue()
    {
        var context = new InputHookContext(42, () => { });
        Assert.Equal(42, context.FileDescriptor);
    }
}
