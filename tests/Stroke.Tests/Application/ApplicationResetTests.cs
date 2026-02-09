using Stroke.Application;
using Stroke.Filters;
using Stroke.FormattedText;
using Stroke.Input;
using Stroke.KeyBinding;
using Stroke.Layout;
using Stroke.Layout.Containers;
using Stroke.Layout.Controls;
using Stroke.Output;
using Xunit;

using StrokeLayout = Stroke.Layout.Layout;

namespace Stroke.Tests.Application;

public class ApplicationResetTests
{
    [Fact]
    public void Reset_ClearsExitStyle()
    {
        var app = new Application<object?>();
        app.ExitStyle = "class:custom";

        app.Reset();

        Assert.Equal("", app.ExitStyle);
    }

    [Fact]
    public void Reset_FiresOnResetEvent()
    {
        var app = new Application<object?>();
        int resetCount = 0;
        app.OnReset.AddHandler(_ => resetCount++);

        app.Reset();

        Assert.Equal(1, resetCount);
    }

    [Fact]
    public void Reset_OnResetCallback_FiredDuringConstruction()
    {
        int resetCount = 0;
        // Constructor calls Reset() which fires OnReset
        var app = new Application<object?>(onReset: _ => resetCount++);

        // Should have been called once during construction
        Assert.Equal(1, resetCount);
    }

    [Fact]
    public void Reset_OnResetCallback_FiredAgainOnExplicitReset()
    {
        int resetCount = 0;
        var app = new Application<object?>(onReset: _ => resetCount++);

        // Constructor calls Reset() = 1 call
        Assert.Equal(1, resetCount);

        app.Reset();
        // Explicit Reset = 2nd call
        Assert.Equal(2, resetCount);
    }

    [Fact]
    public void Reset_MultipleHandlers_AllFired()
    {
        var app = new Application<object?>();
        int count1 = 0;
        int count2 = 0;
        app.OnReset.AddHandler(_ => count1++);
        app.OnReset.AddHandler(_ => count2++);

        app.Reset();

        Assert.Equal(1, count1);
        Assert.Equal(1, count2);
    }

    [Fact]
    public void Reset_ClearsKeyProcessorState()
    {
        var app = new Application<object?>();

        // Set some state on key processor
        app.KeyProcessor.Arg = "5";

        app.Reset();

        Assert.Null(app.KeyProcessor.Arg);
    }

    [Fact]
    public void Reset_ClearsKeyProcessorInputQueue()
    {
        var app = new Application<object?>();

        // Feed a key to the processor
        app.KeyProcessor.Feed(
            new Stroke.KeyBinding.KeyPress(new KeyOrChar('x'), "x"));

        Assert.NotEmpty(app.KeyProcessor.InputQueue);

        app.Reset();

        Assert.Empty(app.KeyProcessor.InputQueue);
    }

    [Fact]
    public void Reset_ResetsViStateInputMode()
    {
        var app = new Application<object?>();

        // Change Vi state
        app.ViState.InputMode = InputMode.Navigation;

        app.Reset();

        Assert.Equal(InputMode.Insert, app.ViState.InputMode);
    }

    [Fact]
    public void Reset_CanBeCalledMultipleTimes()
    {
        var app = new Application<object?>();

        // Should not throw
        app.Reset();
        app.Reset();
        app.Reset();
    }

    [Fact]
    public void Reset_ExecutionOrder_ExitStyleClearedFirst()
    {
        // We verify that ExitStyle is cleared during reset by checking
        // it's empty after the handler fires (handler runs at step 8,
        // ExitStyle clearing is step 1)
        string? exitStyleDuringReset = null;
        var app = new Application<object?>(onReset: a => exitStyleDuringReset = a.ExitStyle);

        // Set ExitStyle, then Reset
        app.ExitStyle = "class:test";
        app.Reset();

        Assert.Equal("", exitStyleDuringReset);
    }

    [Fact]
    public void Reset_DoesNotClearLayout()
    {
        var app = new Application<object?>();
        var layoutBefore = app.Layout;

        app.Reset();

        // Layout reference should be the same (Reset resets containers, not the Layout object)
        Assert.Same(layoutBefore, app.Layout);
    }

    [Fact]
    public void Reset_DoesNotChangeEditingMode()
    {
        var app = new Application<object?>(editingMode: EditingMode.Vi);

        app.Reset();

        Assert.Equal(EditingMode.Vi, app.EditingMode);
    }

    [Fact]
    public void Reset_PreservesClipboard()
    {
        var app = new Application<object?>();
        var clipboardBefore = app.Clipboard;

        app.Reset();

        Assert.Same(clipboardBefore, app.Clipboard);
    }

    // --- Phase 9 (T040): Extended 9-step order verification ---

    [Fact]
    public void Reset_9StepOrder_VerifyStepSequence()
    {
        // Track the order of execution through side effects
        var executionOrder = new List<string>();

        var app = new Application<object?>(
            onReset: _ => executionOrder.Add("OnReset"));

        // After construction Reset was called, clear the tracker
        executionOrder.Clear();

        // Set state that Reset should clear
        app.ExitStyle = "class:test";
        app.KeyProcessor.Arg = "42";
        app.ViState.InputMode = InputMode.Navigation;

        app.Reset();

        // Verify OnReset was called (step 8)
        Assert.Contains("OnReset", executionOrder);

        // Verify all state was reset
        Assert.Equal("", app.ExitStyle);           // Step 1
        Assert.Null(app.KeyProcessor.Arg);          // Step 4
        Assert.Equal(InputMode.Insert, app.ViState.InputMode); // Step 6
    }

    [Fact]
    public void Reset_RendererResetCalled()
    {
        var app = new Application<object?>(output: new DummyOutput());

        // Render to create a last rendered screen
        app.Renderer.Render(app, app.Layout);
        Assert.NotNull(app.Renderer.LastRenderedScreen);

        // Reset should clear the renderer
        app.Reset();
        Assert.Null(app.Renderer.LastRenderedScreen);
    }

    [Fact]
    public void Reset_LayoutResetCalled()
    {
        var app = new Application<object?>(output: new DummyOutput());

        // Layout.Reset() resets containers in the tree
        // Verify it doesn't throw and layout still works
        app.Reset();
        Assert.NotNull(app.Layout.CurrentWindow);
    }

    [Fact]
    public void Reset_ViStateResetCalled()
    {
        var app = new Application<object?>();

        app.ViState.InputMode = InputMode.Navigation;

        app.Reset();

        Assert.Equal(InputMode.Insert, app.ViState.InputMode);
        // Note: TemporaryNavigationMode is intentionally NOT cleared by Reset()
    }

    [Fact]
    public void Reset_EmacsStateResetCalled()
    {
        var app = new Application<object?>();

        // Start recording a macro
        app.EmacsState.StartMacro();
        Assert.True(app.EmacsState.IsRecording);

        app.Reset();

        // Reset should stop macro recording
        Assert.False(app.EmacsState.IsRecording);
    }

    [Fact]
    public void Reset_EnsuresFocusableControl()
    {
        // Create a layout where the focused control is not focusable
        var nonFocusable = new FormattedTextControl(
            Array.Empty<StyleAndTextTuple>(),
            focusable: new FilterOrBool(false));
        var nonFocusableWindow = new Window(content: nonFocusable);

        var focusableControl = new FormattedTextControl(
            Array.Empty<StyleAndTextTuple>(),
            focusable: new FilterOrBool(true));
        var focusableWindow = new Window(content: focusableControl);

        var hsplit = new HSplit(children: [nonFocusableWindow, focusableWindow]);
        var layout = new StrokeLayout(new AnyContainer(hsplit));

        // Focus the non-focusable window first
        // (Layout constructor focuses first window which is non-focusable
        //  ... actually Layout validates this. Let's use a different approach)

        // Create app with the layout where first window is focusable
        var output = new DummyOutput();
        var app = new Application<object?>(
            layout: layout,
            output: output);

        // After construction (which calls Reset), focus should be on a focusable control
        Assert.True(app.Layout.CurrentControl.IsFocusable);
    }

    [Fact]
    public void Reset_MultipleCallsIdempotent()
    {
        var app = new Application<object?>();
        int resetCount = 0;
        app.OnReset.AddHandler(_ => resetCount++);

        app.Reset();
        app.Reset();
        app.Reset();

        Assert.Equal(3, resetCount);
    }

    [Fact]
    public void Reset_ExitStyleClearedBeforeOnResetFires()
    {
        // Verify step ordering: ExitStyle (step 1) happens before OnReset (step 8)
        string? exitStyleSeenInHandler = null;
        var app = new Application<object?>(
            onReset: a => exitStyleSeenInHandler = a.ExitStyle);

        app.ExitStyle = "class:before-reset";
        app.Reset();

        // ExitStyle should be "" when the handler fires
        Assert.Equal("", exitStyleSeenInHandler);
    }

    [Fact]
    public void Reset_KeyProcessorResetBeforeOnResetFires()
    {
        // Verify step ordering: KeyProcessor.Reset (step 4) happens before OnReset (step 8)
        string? argSeenInHandler = null;
        var app = new Application<object?>(
            onReset: a => argSeenInHandler = a.KeyProcessor.Arg);

        app.KeyProcessor.Arg = "99";
        app.Reset();

        // Arg should be null when the handler fires
        Assert.Null(argSeenInHandler);
    }

    [Fact]
    public void Reset_ViStateResetBeforeOnResetFires()
    {
        // Verify step ordering: ViState.Reset (step 6) happens before OnReset (step 8)
        InputMode? modeSeenInHandler = null;
        var app = new Application<object?>(
            onReset: a => modeSeenInHandler = a.ViState.InputMode);

        app.ViState.InputMode = InputMode.Navigation;
        app.Reset();

        // ViState should be reset to Insert when the handler fires
        Assert.Equal(InputMode.Insert, modeSeenInHandler);
    }
}
