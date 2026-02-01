using Stroke.Core;
using Stroke.Input.Pipe;
using Stroke.KeyBinding;
using Stroke.KeyBinding.Bindings;
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
/// Tests for Emacs movement handler behaviors: verifies that invoking
/// movement binding handlers on a real buffer produces correct cursor
/// position changes.
/// </summary>
public sealed class EmacsMovementHandlerTests : IDisposable
{
    private readonly SimplePipeInput _input;
    private readonly DummyOutput _output;

    public EmacsMovementHandlerTests()
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

    /// <summary>
    /// Creates a KeyPressEvent targeting the given buffer and app with the specified key sequence.
    /// </summary>
    private static KeyPressEvent CreateEvent(
        Buffer buffer,
        Stroke.Application.Application<object> app,
        params KeyPress[] keySequence)
    {
        return new KeyPressEvent(
            keyProcessorRef: null,
            arg: null,
            keySequence: keySequence,
            previousKeySequence: [],
            isRepeat: false,
            app: app,
            currentBuffer: buffer);
    }

    /// <summary>
    /// Finds the first binding matching the key sequence from the Emacs bindings.
    /// </summary>
    private static Binding? FindBinding(IKeyBindingsBase kb, params KeyOrChar[] keys)
    {
        var bindings = kb.GetBindingsForKeys(keys);
        return bindings.Count > 0 ? bindings[0] : null;
    }

    #endregion

    #region Ctrl-A: Beginning of Line

    [Fact]
    public void CtrlA_MovesCursorToBeginningOfLine()
    {
        var (buffer, app, scope) = CreateEnvironment("hello world", cursorPosition: 5);
        using (scope)
        {
            var kb = EmacsBindingsType.LoadEmacsBindings();
            var binding = FindBinding(kb, new KeyOrChar(Keys.ControlA));
            Assert.NotNull(binding);

            var @event = CreateEvent(buffer, app, new KeyPress(Keys.ControlA));
            binding.Handler(@event);

            Assert.Equal(0, buffer.CursorPosition);
        }
    }

    #endregion

    #region Ctrl-E: End of Line

    [Fact]
    public void CtrlE_MovesCursorToEndOfLine()
    {
        var (buffer, app, scope) = CreateEnvironment("hello world", cursorPosition: 0);
        using (scope)
        {
            var kb = EmacsBindingsType.LoadEmacsBindings();
            var binding = FindBinding(kb, new KeyOrChar(Keys.ControlE));
            Assert.NotNull(binding);

            var @event = CreateEvent(buffer, app, new KeyPress(Keys.ControlE));
            binding.Handler(@event);

            Assert.Equal(11, buffer.CursorPosition);
        }
    }

    #endregion

    #region Ctrl-F: Forward Char

    [Fact]
    public void CtrlF_MovesForwardOneChar()
    {
        var (buffer, app, scope) = CreateEnvironment("hello", cursorPosition: 0);
        using (scope)
        {
            var kb = EmacsBindingsType.LoadEmacsBindings();
            var binding = FindBinding(kb, new KeyOrChar(Keys.ControlF));
            Assert.NotNull(binding);

            var @event = CreateEvent(buffer, app, new KeyPress(Keys.ControlF));
            binding.Handler(@event);

            Assert.Equal(1, buffer.CursorPosition);
        }
    }

    #endregion

    #region Ctrl-B: Backward Char

    [Fact]
    public void CtrlB_MovesBackwardOneChar()
    {
        var (buffer, app, scope) = CreateEnvironment("hello", cursorPosition: 3);
        using (scope)
        {
            var kb = EmacsBindingsType.LoadEmacsBindings();
            var binding = FindBinding(kb, new KeyOrChar(Keys.ControlB));
            Assert.NotNull(binding);

            var @event = CreateEvent(buffer, app, new KeyPress(Keys.ControlB));
            binding.Handler(@event);

            Assert.Equal(2, buffer.CursorPosition);
        }
    }

    #endregion

    #region Meta-f: Forward Word

    [Fact]
    public void MetaF_MovesForwardOneWord()
    {
        var (buffer, app, scope) = CreateEnvironment("hello world", cursorPosition: 0);
        using (scope)
        {
            var kb = EmacsBindingsType.LoadEmacsBindings();
            var binding = FindBinding(kb,
                new KeyOrChar(Keys.Escape), new KeyOrChar('f'));
            Assert.NotNull(binding);

            var @event = CreateEvent(buffer, app,
                new KeyPress(Keys.Escape), new KeyPress('f'));
            binding.Handler(@event);

            // ForwardWord uses FindNextWordEnding, which moves to end of "hello" (pos 5)
            Assert.Equal(5, buffer.CursorPosition);
        }
    }

    #endregion

    #region Meta-b: Backward Word

    [Fact]
    public void MetaB_MovesBackwardOneWord()
    {
        var (buffer, app, scope) = CreateEnvironment("hello world", cursorPosition: 8);
        using (scope)
        {
            var kb = EmacsBindingsType.LoadEmacsBindings();
            var binding = FindBinding(kb,
                new KeyOrChar(Keys.Escape), new KeyOrChar('b'));
            Assert.NotNull(binding);

            var @event = CreateEvent(buffer, app,
                new KeyPress(Keys.Escape), new KeyPress('b'));
            binding.Handler(@event);

            // BackwardWord uses FindPreviousWordBeginning, which moves to start of "world" (pos 6)
            Assert.Equal(6, buffer.CursorPosition);
        }
    }

    #endregion

    #region Ctrl-Home: Beginning of Buffer

    [Fact]
    public void CtrlHome_MovesCursorToBeginningOfBuffer()
    {
        var (buffer, app, scope) = CreateEnvironment("hello\nworld", cursorPosition: 8, multiline: true);
        using (scope)
        {
            var kb = EmacsBindingsType.LoadEmacsBindings();
            var binding = FindBinding(kb, new KeyOrChar(Keys.ControlHome));
            Assert.NotNull(binding);

            var @event = CreateEvent(buffer, app, new KeyPress(Keys.ControlHome));
            binding.Handler(@event);

            Assert.Equal(0, buffer.CursorPosition);
        }
    }

    #endregion

    #region Ctrl-End: End of Buffer

    [Fact]
    public void CtrlEnd_MovesCursorToEndOfBuffer()
    {
        var (buffer, app, scope) = CreateEnvironment("hello\nworld", cursorPosition: 0, multiline: true);
        using (scope)
        {
            var kb = EmacsBindingsType.LoadEmacsBindings();
            var binding = FindBinding(kb, new KeyOrChar(Keys.ControlEnd));
            Assert.NotNull(binding);

            var @event = CreateEvent(buffer, app, new KeyPress(Keys.ControlEnd));
            binding.Handler(@event);

            Assert.Equal(11, buffer.CursorPosition);
        }
    }

    #endregion

    #region Ctrl-N: AutoDown

    [Fact]
    public void CtrlN_MovesDownInMultilineBuffer()
    {
        // "hello\nworld" with cursor at position 2 (line 0, col 2) -> should move to line 1
        var (buffer, app, scope) = CreateEnvironment("hello\nworld", cursorPosition: 2, multiline: true);
        using (scope)
        {
            var kb = EmacsBindingsType.LoadEmacsBindings();
            var binding = FindBinding(kb, new KeyOrChar(Keys.ControlN));
            Assert.NotNull(binding);

            int originalPosition = buffer.CursorPosition;
            var @event = CreateEvent(buffer, app, new KeyPress(Keys.ControlN));
            binding.Handler(@event);

            // AutoDown() calls CursorDown when not on last line
            // Cursor should now be on the second line
            Assert.True(buffer.CursorPosition > originalPosition,
                "Ctrl-N should move cursor down in multiline buffer");
            Assert.Equal(8, buffer.CursorPosition); // "hello\n" = 6 chars, then col 2 = pos 8
        }
    }

    #endregion

    #region Ctrl-P: AutoUp

    [Fact]
    public void CtrlP_MovesUpInMultilineBuffer()
    {
        // "hello\nworld" with cursor at position 8 (line 1, col 2) -> should move to line 0
        var (buffer, app, scope) = CreateEnvironment("hello\nworld", cursorPosition: 8, multiline: true);
        using (scope)
        {
            var kb = EmacsBindingsType.LoadEmacsBindings();
            var binding = FindBinding(kb, new KeyOrChar(Keys.ControlP));
            Assert.NotNull(binding);

            int originalPosition = buffer.CursorPosition;
            var @event = CreateEvent(buffer, app, new KeyPress(Keys.ControlP));
            binding.Handler(@event);

            // AutoUp(count: event.Arg) calls CursorUp when not on first line
            // Cursor should now be on the first line
            Assert.True(buffer.CursorPosition < originalPosition,
                "Ctrl-P should move cursor up in multiline buffer");
            Assert.Equal(2, buffer.CursorPosition); // line 0, col 2
        }
    }

    #endregion
}
