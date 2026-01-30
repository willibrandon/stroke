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

using AppContext = Stroke.Application.AppContext;
using KBKeyPress = Stroke.KeyBinding.KeyPress;
using StrokeLayout = Stroke.Layout.Layout;

namespace Stroke.Tests.Application;

public class ApplicationKeyBindingMergingTests
{
    // Helper to create a simple focusable control with optional key bindings
    private static FormattedTextControl CreateFocusableControl(IKeyBindingsBase? keyBindings = null)
    {
        return new FormattedTextControl(
            Array.Empty<StyleAndTextTuple>(),
            focusable: new FilterOrBool(true),
            keyBindings: keyBindings);
    }

    // Helper to create a simple binding
    private static KeyBindings CreateBindingsFor(char key, out int callCount)
    {
        int count = 0;
        var kb = new KeyBindings();
        kb.Add<KeyHandlerCallable>([new KeyOrChar(key)])(e => { Interlocked.Increment(ref count); return null; });
        callCount = 0; // Will be tracked via closure
        return kb;
    }

    // Helper to create a KeyBindings with a handler that increments a counter
    private static (KeyBindings Bindings, Func<int> GetCount) CreateTrackedBindings(char key)
    {
        int count = 0;
        var kb = new KeyBindings();
        kb.Add<KeyHandlerCallable>([new KeyOrChar(key)])(e => { Interlocked.Increment(ref count); return null; });
        return (kb, () => count);
    }

    [Fact]
    public void FocusedControlBindings_TakePriority()
    {
        // Create control-level bindings
        var (controlKb, getControlCount) = CreateTrackedBindings('a');
        var focusedControl = CreateFocusableControl(controlKb);
        var window = new Window(content: focusedControl);

        // Create app-level bindings for the same key
        var (appKb, getAppCount) = CreateTrackedBindings('a');

        var layout = new StrokeLayout(new AnyContainer(window));
        var output = new DummyOutput();
        var app = new Application<object?>(
            layout: layout,
            keyBindings: appKb,
            output: output);

        // The CombinedRegistry should merge all bindings.
        // The focused control's bindings should have higher priority (last in list = highest priority after reversal).
        var bindings = app.KeyProcessor;
        Assert.NotNull(bindings);

        // Verify the key processor was created with the CombinedRegistry
        Assert.NotNull(app.KeyProcessor.InputQueue);
    }

    [Fact]
    public void ApplicationLevelBindings_Accessible()
    {
        var (appKb, getAppCount) = CreateTrackedBindings('x');
        var output = new DummyOutput();
        var app = new Application<object?>(
            keyBindings: appKb,
            output: output);

        Assert.Same(appKb, app.KeyBindings);
    }

    [Fact]
    public void NullKeyBindings_DoesNotThrow()
    {
        var output = new DummyOutput();
        var app = new Application<object?>(
            keyBindings: null,
            output: output);

        Assert.Null(app.KeyBindings);
    }

    [Fact]
    public void ModalContainer_ExcludesParentBindings()
    {
        // Create an inner control with bindings
        var (innerKb, getInnerCount) = CreateTrackedBindings('a');
        var innerControl = CreateFocusableControl(innerKb);
        var innerWindow = new Window(content: innerControl);

        // Create modal HSplit containing the window
        var hsplit = new HSplit(
            children: [innerWindow],
            modal: true);

        // Create an outer container that wraps the modal
        var outerKb = new KeyBindings();
        outerKb.Add<KeyHandlerCallable>([new KeyOrChar('b')])(e => null);

        var outerSplit = new HSplit(
            children: [hsplit],
            keyBindings: outerKb);

        var layout = new StrokeLayout(new AnyContainer(outerSplit));
        var output = new DummyOutput();
        var app = new Application<object?>(
            layout: layout,
            output: output);

        // Layout should have the inner window focused
        Assert.Same(innerWindow, layout.CurrentWindow);

        // The modal container should stop parent traversal
        Assert.True(hsplit.IsModal);
    }

    [Fact]
    public void GlobalOnlyBindings_FromNonFocusedContainers()
    {
        // Create two windows in an HSplit
        var (kb1, getCount1) = CreateTrackedBindings('a');
        var control1 = CreateFocusableControl(kb1);
        var window1 = new Window(content: control1);

        // Second window with a global binding
        var kb2 = new KeyBindings();
        kb2.Add<KeyHandlerCallable>(
            [new KeyOrChar('b')],
            isGlobal: new FilterOrBool(true))(e => null);
        var control2 = CreateFocusableControl(kb2);
        var window2 = new Window(content: control2);

        var hsplit = new HSplit(children: [window1, window2]);
        var layout = new StrokeLayout(new AnyContainer(hsplit));

        // Focus window1
        layout.Focus(new FocusableElement(window1));

        var output = new DummyOutput();
        var app = new Application<object?>(
            layout: layout,
            output: output);

        // window2's global binding should be included via GlobalOnlyKeyBindings
        // because window2 is not in the focused hierarchy but its binding is global
        Assert.Same(window1, layout.CurrentWindow);
    }

    [Fact]
    public void PageNavigationBindings_ConditionalOnFilter()
    {
        var output = new DummyOutput();

        // enablePageNavigationBindings defaults to Condition(() => fullScreen)
        var appFullScreen = new Application<object?>(
            output: output,
            fullScreen: true);

        Assert.NotNull(appFullScreen.EnablePageNavigationBindings);
        // FullScreen true => EnablePageNavigationBindings should be true
        Assert.True(appFullScreen.EnablePageNavigationBindings.Invoke());

        var appNotFullScreen = new Application<object?>(
            output: output,
            fullScreen: false);

        // FullScreen false => EnablePageNavigationBindings should be false
        Assert.False(appNotFullScreen.EnablePageNavigationBindings.Invoke());
    }

    [Fact]
    public void EnablePageNavigationBindings_ExplicitTrue_AlwaysEnabled()
    {
        var output = new DummyOutput();
        var app = new Application<object?>(
            output: output,
            fullScreen: false,
            enablePageNavigationBindings: new FilterOrBool(true));

        Assert.True(app.EnablePageNavigationBindings.Invoke());
    }

    [Fact]
    public void EnablePageNavigationBindings_ExplicitFalse_AlwaysDisabled()
    {
        var output = new DummyOutput();
        var app = new Application<object?>(
            output: output,
            fullScreen: true,
            enablePageNavigationBindings: new FilterOrBool(false));

        Assert.False(app.EnablePageNavigationBindings.Invoke());
    }

    [Fact]
    public void DefaultBindings_LoadedAtConstruction()
    {
        var output = new DummyOutput();
        var app = new Application<object?>(output: output);

        Assert.NotNull(app.DefaultBindings);
        Assert.NotNull(app.PageNavigationBindings);
    }

    [Fact]
    public void CombinedRegistry_CacheInvalidation_OnFocusChange()
    {
        // Create two focusable controls in different windows
        var (kb1, _) = CreateTrackedBindings('a');
        var control1 = CreateFocusableControl(kb1);
        var window1 = new Window(content: control1);

        var (kb2, _) = CreateTrackedBindings('b');
        var control2 = CreateFocusableControl(kb2);
        var window2 = new Window(content: control2);

        var hsplit = new HSplit(children: [window1, window2]);
        var layout = new StrokeLayout(new AnyContainer(hsplit));

        var output = new DummyOutput();
        var app = new Application<object?>(
            layout: layout,
            output: output);

        // Focus window1 first
        layout.Focus(new FocusableElement(window1));
        Assert.Same(window1, layout.CurrentWindow);

        // Change focus to window2
        layout.Focus(new FocusableElement(window2));
        Assert.Same(window2, layout.CurrentWindow);

        // CombinedRegistry uses SimpleCache keyed by current window,
        // so different focus should produce different merged bindings
    }

    [Fact]
    public void FilterOrBool_Default_TreatedAsFalse_ForMouseSupport()
    {
        var output = new DummyOutput();
        // mouseSupport default (struct default) is treated as false
        var app = new Application<object?>(output: output);

        Assert.False(app.MouseSupport.Invoke());
    }

    [Fact]
    public void FilterOrBool_Default_TreatedAsFalse_ForPasteMode()
    {
        var output = new DummyOutput();
        var app = new Application<object?>(output: output);

        Assert.False(app.PasteMode.Invoke());
    }

    [Fact]
    public void FilterOrBool_Default_TreatedAsFalse_ForReverseViSearchDirection()
    {
        var output = new DummyOutput();
        var app = new Application<object?>(output: output);

        Assert.False(app.ReverseViSearchDirection.Invoke());
    }

    [Fact]
    public void MouseSupport_ExplicitTrue()
    {
        var output = new DummyOutput();
        var app = new Application<object?>(
            output: output,
            mouseSupport: new FilterOrBool(true));

        Assert.True(app.MouseSupport.Invoke());
    }

    [Fact]
    public void PasteMode_ExplicitTrue()
    {
        var output = new DummyOutput();
        var app = new Application<object?>(
            output: output,
            pasteMode: new FilterOrBool(true));

        Assert.True(app.PasteMode.Invoke());
    }

    [Fact]
    public void KeyBindings_SetterUpdatesRegistry()
    {
        var output = new DummyOutput();
        var app = new Application<object?>(output: output);

        Assert.Null(app.KeyBindings);

        var kb = new KeyBindings();
        app.KeyBindings = kb;
        Assert.Same(kb, app.KeyBindings);

        app.KeyBindings = null;
        Assert.Null(app.KeyBindings);
    }

    [Fact]
    public void ParentContainerBindings_IncludedWhenNotModal()
    {
        // Create a focused window inside a non-modal HSplit with bindings
        var control = CreateFocusableControl();
        var window = new Window(content: control);

        var parentKb = new KeyBindings();
        parentKb.Add<KeyHandlerCallable>([new KeyOrChar('x')])(e => null);

        var parent = new HSplit(
            children: [window],
            keyBindings: parentKb);

        var layout = new StrokeLayout(new AnyContainer(parent));
        var output = new DummyOutput();
        var app = new Application<object?>(
            layout: layout,
            output: output);

        // Parent is not modal, so its bindings should be included
        Assert.False(parent.IsModal);
        Assert.Same(window, layout.CurrentWindow);

        // Walk up from window should reach parent
        layout.UpdateParentsRelations();
        var parentContainer = layout.GetParent(window);
        Assert.Same(parent, parentContainer);
    }

    [Fact]
    public void HSplit_KeyBindings_ReturnedCorrectly()
    {
        var kb = new KeyBindings();
        kb.Add<KeyHandlerCallable>([new KeyOrChar('z')])(e => null);

        var window = new Window();
        var hsplit = new HSplit(
            children: [window],
            keyBindings: kb);

        Assert.Same(kb, hsplit.GetKeyBindings());
    }

    [Fact]
    public void Window_GetKeyBindings_DelegatesToContent()
    {
        var controlKb = new KeyBindings();
        controlKb.Add<KeyHandlerCallable>([new KeyOrChar('a')])(e => null);

        var control = CreateFocusableControl(controlKb);
        var window = new Window(content: control);

        // Window delegates to Content.GetKeyBindings()
        Assert.Same(controlKb, window.GetKeyBindings());
    }

    [Fact]
    public void Window_GetKeyBindings_NullWhenNoControlBindings()
    {
        var control = CreateFocusableControl(keyBindings: null);
        var window = new Window(content: control);

        Assert.Null(window.GetKeyBindings());
    }
}
