using Stroke.Core;
using Stroke.Input.Pipe;
using Stroke.KeyBinding;
using Stroke.Layout;
using Stroke.Layout.Containers;
using Stroke.Layout.Controls;
using Stroke.Output;
using Xunit;
using AppContext = Stroke.Application.AppContext;
using Buffer = Stroke.Core.Buffer;
using EmacsBindingsType = Stroke.Application.Bindings.EmacsBindings;
using Keys = Stroke.Input.Keys;

namespace Stroke.Tests.Application.Bindings.EmacsBindings;

/// <summary>
/// Tests for Emacs selection handler behaviors: Ctrl-@ (start selection),
/// Ctrl-G (cancel/dismiss), Ctrl-W (cut selection), Meta-w (copy selection).
/// Verifies handler side effects on buffer and clipboard state.
/// </summary>
public sealed class EmacsSelectionHandlerTests : IDisposable
{
    private readonly SimplePipeInput _input;
    private readonly DummyOutput _output;

    public EmacsSelectionHandlerTests()
    {
        _input = new SimplePipeInput();
        _output = new DummyOutput();
    }

    public void Dispose()
    {
        _input.Dispose();
    }

    #region Test Environment Setup

    private (Buffer Buffer, Stroke.Application.Application<object> App, IDisposable Scope)
        CreateEnvironment(
            string text = "",
            int cursorPosition = 0,
            bool multiline = false)
    {
        var buffer = new Buffer(
            document: new Document(text, cursorPosition: cursorPosition),
            multiline: multiline ? () => true : () => false);
        var bufferControl = new BufferControl(buffer: buffer);
        var window = new Window(content: bufferControl);
        var layout = new Stroke.Layout.Layout(new AnyContainer(window));
        var app = new Stroke.Application.Application<object>(
            input: _input, output: _output, layout: layout);
        var scope = AppContext.SetApp(app.UnsafeCast);

        return (buffer, app, scope);
    }

    private static KeyPressEvent CreateEvent(
        Buffer buffer,
        object app,
        Keys key = Keys.Any,
        string? data = null)
    {
        var keyPress = data != null
            ? new KeyPress(key, data)
            : new KeyPress(key);

        return new KeyPressEvent(
            keyProcessorRef: null,
            arg: null,
            keySequence: [keyPress],
            previousKeySequence: [],
            isRepeat: false,
            app: app,
            currentBuffer: buffer);
    }

    /// <summary>
    /// Finds a binding by key sequence from the loaded Emacs bindings.
    /// </summary>
    private static IReadOnlyList<Binding> GetBindings(
        IKeyBindingsBase kb, params KeyOrChar[] keys)
    {
        return kb.GetBindingsForKeys(keys);
    }

    #endregion

    #region Ctrl-@ Start Selection

    [Fact]
    public void CtrlAt_StartsCharacterSelection_OnNonEmptyBuffer()
    {
        var (buffer, app, scope) = CreateEnvironment("hello world", 5);
        using (scope)
        {
            app.EditingMode = EditingMode.Emacs;
            var kb = EmacsBindingsType.LoadEmacsBindings();

            var bindings = GetBindings(kb, new KeyOrChar(Keys.ControlAt));
            Assert.NotEmpty(bindings);

            var @event = CreateEvent(buffer, app, Keys.ControlAt);
            bindings[0].Handler(@event);

            Assert.NotNull(buffer.SelectionState);
            Assert.Equal(SelectionType.Characters, buffer.SelectionState.Type);
        }
    }

    [Fact]
    public void CtrlAt_DoesNothing_OnEmptyBuffer()
    {
        var (buffer, app, scope) = CreateEnvironment("", 0);
        using (scope)
        {
            app.EditingMode = EditingMode.Emacs;
            var kb = EmacsBindingsType.LoadEmacsBindings();

            var bindings = GetBindings(kb, new KeyOrChar(Keys.ControlAt));
            Assert.NotEmpty(bindings);

            var @event = CreateEvent(buffer, app, Keys.ControlAt);
            bindings[0].Handler(@event);

            Assert.Null(buffer.SelectionState);
        }
    }

    #endregion

    #region Ctrl-G Cancel

    [Fact]
    public void CtrlG_DismissesCompletionAndValidation_WhenNoSelection()
    {
        var (buffer, app, scope) = CreateEnvironment("hello", 5);
        using (scope)
        {
            app.EditingMode = EditingMode.Emacs;
            var kb = EmacsBindingsType.LoadEmacsBindings();

            // Get the Ctrl-G binding without selection (first one, with ~HasSelection filter)
            var bindings = GetBindings(kb, new KeyOrChar(Keys.ControlG));
            Assert.True(bindings.Count >= 2);

            // No selection is active, so find the binding that works without selection
            // The Cancel handler (no selection) dismisses completion + validation
            var @event = CreateEvent(buffer, app, Keys.ControlG);

            // Invoke the cancel (no-selection) handler — it should not throw
            // and should call DismissCompletion + DismissValidation
            bindings[0].Handler(@event);

            // After cancel, completion and validation state should be cleared
            Assert.Null(buffer.CompleteState);
        }
    }

    [Fact]
    public void CtrlG_ExitsSelection_WhenSelectionIsActive()
    {
        var (buffer, app, scope) = CreateEnvironment("hello world", 0);
        using (scope)
        {
            app.EditingMode = EditingMode.Emacs;

            // Start a selection first
            buffer.StartSelection();
            buffer.CursorPosition = 5;
            Assert.NotNull(buffer.SelectionState);

            var kb = EmacsBindingsType.LoadEmacsBindings();
            var bindings = GetBindings(kb, new KeyOrChar(Keys.ControlG));
            Assert.True(bindings.Count >= 2);

            // Find the CancelSelection handler (the one with HasSelection filter)
            // It's the second Ctrl-G binding
            var @event = CreateEvent(buffer, app, Keys.ControlG);
            bindings[1].Handler(@event);

            Assert.Null(buffer.SelectionState);
        }
    }

    #endregion

    #region Ctrl-W Cut Selection

    [Fact]
    public void CtrlW_CutsSelectionToClipboard()
    {
        var (buffer, app, scope) = CreateEnvironment("hello world", 0);
        using (scope)
        {
            app.EditingMode = EditingMode.Emacs;

            // Create selection covering "hello"
            buffer.StartSelection();
            buffer.CursorPosition = 5;

            var kb = EmacsBindingsType.LoadEmacsBindings();
            var bindings = GetBindings(kb, new KeyOrChar(Keys.ControlW));
            Assert.NotEmpty(bindings);

            var @event = CreateEvent(buffer, app, Keys.ControlW);
            bindings[0].Handler(@event);

            // Text should have "hello" removed
            Assert.Equal(" world", buffer.Text);
            // Clipboard should contain the cut text
            Assert.Equal("hello", app.Clipboard.GetData().Text);
        }
    }

    #endregion

    #region Meta-w Copy Selection

    [Fact]
    public void MetaW_CopiesSelectionToClipboard_WithoutRemovingText()
    {
        var (buffer, app, scope) = CreateEnvironment("hello world", 0);
        using (scope)
        {
            app.EditingMode = EditingMode.Emacs;

            // Create selection covering "hello"
            buffer.StartSelection();
            buffer.CursorPosition = 5;

            var kb = EmacsBindingsType.LoadEmacsBindings();
            var bindings = GetBindings(
                kb, new KeyOrChar(Keys.Escape), new KeyOrChar('w'));
            Assert.NotEmpty(bindings);

            var @event = CreateEvent(buffer, app, Keys.Escape);
            bindings[0].Handler(@event);

            // Text should remain unchanged
            Assert.Equal("hello world", buffer.Text);
            // Clipboard should contain the copied text
            Assert.Equal("hello", app.Clipboard.GetData().Text);
        }
    }

    #endregion
}
