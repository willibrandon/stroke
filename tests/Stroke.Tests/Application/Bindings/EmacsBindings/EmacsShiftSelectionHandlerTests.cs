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
/// Tests for the shift-selection handler behavior in EmacsBindings.ShiftSelection:
/// verifies that ShiftStartSelection, ShiftExtendSelection, ShiftReplaceSelection,
/// ShiftDelete, and ShiftCancelMove produce the correct buffer state changes.
/// </summary>
public sealed class EmacsShiftSelectionHandlerTests : IDisposable
{
    private readonly SimplePipeInput _input;
    private readonly DummyOutput _output;

    public EmacsShiftSelectionHandlerTests()
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
        app.EditingMode = Stroke.KeyBinding.EditingMode.Emacs;
        var scope = AppContext.SetApp(app.UnsafeCast);
        return (buffer, app, scope);
    }

    private static KeyPressEvent CreateEvent(
        Stroke.Application.Application<object> app,
        Buffer buffer,
        Keys key)
    {
        return new KeyPressEvent(
            keyProcessorRef: new WeakReference<object>(app.KeyProcessor),
            arg: null,
            keySequence: [new KeyPress(new KeyOrChar(key))],
            previousKeySequence: [],
            isRepeat: false,
            app: app,
            currentBuffer: buffer);
    }

    #endregion

    #region ShiftStartSelection

    [Fact]
    public void ShiftStartSelection_OnNonEmptyBuffer_StartsSelectionAndMovesCursor()
    {
        var (buffer, app, scope) = CreateEnvironment(text: "hello", cursorPosition: 0);
        using (scope)
        {
            var kb = EmacsBindingsType.LoadEmacsShiftSelectionBindings();
            var bindings = kb.GetBindingsForKeys([new KeyOrChar(Keys.ShiftRight)]);

            // Use Last() to get the most specific binding (fewest wildcards).
            // Bindings are sorted descending by AnyCount.
            var startBinding = bindings.Last();

            var evt = CreateEvent(app, buffer, Keys.ShiftRight);
            startBinding.Handler(evt);

            // Cursor should have moved right from 0 to 1
            Assert.Equal(1, buffer.CursorPosition);
            // Selection should be active in shift mode
            Assert.NotNull(buffer.SelectionState);
            Assert.True(buffer.SelectionState.ShiftMode);
            Assert.Equal(0, buffer.SelectionState.OriginalCursorPosition);
        }
    }

    [Fact]
    public void ShiftStartSelection_OnEmptyBuffer_DoesNotStartSelection()
    {
        var (buffer, app, scope) = CreateEnvironment(text: "", cursorPosition: 0);
        using (scope)
        {
            var kb = EmacsBindingsType.LoadEmacsShiftSelectionBindings();
            var bindings = kb.GetBindingsForKeys([new KeyOrChar(Keys.ShiftRight)]);
            // Use [^1] to get the most specific binding (fewest wildcards).
            var startBinding = bindings[^1];

            var evt = CreateEvent(app, buffer, Keys.ShiftRight);
            startBinding.Handler(evt);

            // Empty buffer guard: no selection started
            Assert.Null(buffer.SelectionState);
            Assert.Equal(0, buffer.CursorPosition);
        }
    }

    [Fact]
    public void ShiftStartSelection_AtEndOfText_CancelsSelection()
    {
        var (buffer, app, scope) = CreateEnvironment(text: "hello", cursorPosition: 5);
        using (scope)
        {
            var kb = EmacsBindingsType.LoadEmacsShiftSelectionBindings();
            var bindings = kb.GetBindingsForKeys([new KeyOrChar(Keys.ShiftRight)]);
            // Use [^1] to get the most specific binding (fewest wildcards).
            var startBinding = bindings[^1];

            var evt = CreateEvent(app, buffer, Keys.ShiftRight);
            startBinding.Handler(evt);

            // At end, ShiftRight cannot move further, so selection is cancelled
            Assert.Null(buffer.SelectionState);
            Assert.Equal(5, buffer.CursorPosition);
        }
    }

    #endregion

    #region ShiftExtendSelection

    [Fact]
    public void ShiftExtendSelection_ExtendsExistingShiftSelection()
    {
        var (buffer, app, scope) = CreateEnvironment(text: "hello", cursorPosition: 0);
        using (scope)
        {
            // Manually enter shift-selection state at position 0, cursor at 1
            buffer.StartSelection(selectionType: SelectionType.Characters);
            buffer.SelectionState!.EnterShiftMode();
            buffer.CursorPosition = 1;

            var kb = EmacsBindingsType.LoadEmacsShiftSelectionBindings();
            var bindings = kb.GetBindingsForKeys([new KeyOrChar(Keys.ShiftRight)]);

            // With descending sort by AnyCount, extend-selection is at [^2] (second from end).
            var extendBinding = bindings[^2];

            var evt = CreateEvent(app, buffer, Keys.ShiftRight);
            extendBinding.Handler(evt);

            // Cursor should have moved from 1 to 2, selection still active
            Assert.Equal(2, buffer.CursorPosition);
            Assert.NotNull(buffer.SelectionState);
            Assert.True(buffer.SelectionState.ShiftMode);
        }
    }

    #endregion

    #region ShiftReplaceSelection

    [Fact]
    public void ShiftReplaceSelection_CutsSelectionAndInserts()
    {
        var (buffer, app, scope) = CreateEnvironment(text: "hello", cursorPosition: 0);
        using (scope)
        {
            // Set up shift selection covering "hel" (position 0..3)
            buffer.StartSelection(selectionType: SelectionType.Characters);
            buffer.SelectionState!.EnterShiftMode();
            buffer.CursorPosition = 3;

            var kb = EmacsBindingsType.LoadEmacsShiftSelectionBindings();
            var bindings = kb.GetBindingsForKeys([new KeyOrChar(Keys.Any)]);
            // Use [^1] to get the most specific binding (fewest wildcards).
            var replaceBinding = bindings[^1];

            // Create event for typing 'X' (Keys.Any with data 'X')
            var evt = new KeyPressEvent(
                keyProcessorRef: new WeakReference<object>(app.KeyProcessor),
                arg: null,
                keySequence: [new KeyPress(new KeyOrChar('X'))],
                previousKeySequence: [],
                isRepeat: false,
                app: app,
                currentBuffer: buffer);

            replaceBinding.Handler(evt);

            // "hel" should be replaced with "X", yielding "Xlo"
            Assert.Equal("Xlo", buffer.Text);
            // Selection should be cleared after cut
            Assert.Null(buffer.SelectionState);
        }
    }

    #endregion

    #region ShiftDelete

    [Fact]
    public void ShiftDelete_CutsSelection()
    {
        var (buffer, app, scope) = CreateEnvironment(text: "hello", cursorPosition: 0);
        using (scope)
        {
            // Set up shift selection covering "hel" (position 0..3)
            buffer.StartSelection(selectionType: SelectionType.Characters);
            buffer.SelectionState!.EnterShiftMode();
            buffer.CursorPosition = 3;

            var kb = EmacsBindingsType.LoadEmacsShiftSelectionBindings();
            var bindings = kb.GetBindingsForKeys([new KeyOrChar(Keys.ControlH)]);
            // Use [^1] to get the most specific binding (fewest wildcards).
            var deleteBinding = bindings[^1];

            var evt = CreateEvent(app, buffer, Keys.ControlH);
            deleteBinding.Handler(evt);

            // "hel" should be deleted, leaving "lo"
            Assert.Equal("lo", buffer.Text);
            Assert.Null(buffer.SelectionState);
        }
    }

    #endregion

    #region ShiftCancelMove

    [Fact]
    public void ShiftCancelMove_ExitsSelectionAndFeedsKey()
    {
        var (buffer, app, scope) = CreateEnvironment(text: "hello", cursorPosition: 0);
        using (scope)
        {
            // Set up shift selection covering "hel" (position 0..3)
            buffer.StartSelection(selectionType: SelectionType.Characters);
            buffer.SelectionState!.EnterShiftMode();
            buffer.CursorPosition = 3;

            var kb = EmacsBindingsType.LoadEmacsShiftSelectionBindings();
            var bindings = kb.GetBindingsForKeys([new KeyOrChar(Keys.Right)]);
            // Use [^1] to get the most specific binding (fewest wildcards).
            var cancelBinding = bindings[^1];

            var evt = CreateEvent(app, buffer, Keys.Right);
            cancelBinding.Handler(evt);

            // Selection should be cleared
            Assert.Null(buffer.SelectionState);
            // The key should be re-fed into the KeyProcessor's input queue
            Assert.Contains(
                app.KeyProcessor.InputQueue,
                kp => kp.Key == new KeyOrChar(Keys.Right));
        }
    }

    #endregion
}
