using Stroke.Application;
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
/// Tests for EmacsBindings handler behaviors: ToggleStartEnd, StartOfWord,
/// StartNextWord, Cancel, and placeholder no-ops (PrevSentence, EndOfSentence,
/// SwapCharacters).
/// </summary>
public sealed class EmacsCompletionHandlerTests : IDisposable
{
    private readonly SimplePipeInput _input;
    private readonly DummyOutput _output;

    public EmacsCompletionHandlerTests()
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
        var scope = AppContext.SetApp(app);

        return (buffer, app, scope);
    }

    private static KeyPressEvent CreateEvent(
        Buffer buffer,
        IApplication app,
        IReadOnlyList<KeyPress> keySequence)
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
    /// Finds an inline handler binding for the given multi-key sequence.
    /// </summary>
    private static Binding? FindInlineBinding(
        IKeyBindingsBase kb, params KeyOrChar[] keys)
    {
        var bindings = kb.GetBindingsForKeys(keys);
        return bindings.FirstOrDefault();
    }

    #endregion

    #region ToggleStartEnd (Ctrl-X Ctrl-X)

    [Fact]
    public void ToggleStartEnd_CursorAtEnd_MovesToStart()
    {
        var (buffer, app, scope) = CreateEnvironment("  hello  ", cursorPosition: 9);
        using (scope)
        {
            app.EditingMode = EditingMode.Emacs;
            var kb = EmacsBindingsType.LoadEmacsBindings();

            var binding = FindInlineBinding(kb,
                new KeyOrChar(Keys.ControlX), new KeyOrChar(Keys.ControlX));
            Assert.NotNull(binding);

            var @event = CreateEvent(buffer, app,
                [new KeyPress(Keys.ControlX), new KeyPress(Keys.ControlX)]);

            binding.Handler(@event);

            // Cursor should move to start of line (position 0)
            Assert.Equal(0, buffer.CursorPosition);
        }
    }

    [Fact]
    public void ToggleStartEnd_CursorAtStart_MovesToEnd()
    {
        var (buffer, app, scope) = CreateEnvironment("  hello  ", cursorPosition: 0);
        using (scope)
        {
            app.EditingMode = EditingMode.Emacs;
            var kb = EmacsBindingsType.LoadEmacsBindings();

            var binding = FindInlineBinding(kb,
                new KeyOrChar(Keys.ControlX), new KeyOrChar(Keys.ControlX));
            Assert.NotNull(binding);

            // First call: cursor at position 0 is not at end of line, so goes to end
            var @event = CreateEvent(buffer, app,
                [new KeyPress(Keys.ControlX), new KeyPress(Keys.ControlX)]);

            binding.Handler(@event);

            // Cursor should move to end of line (position 9)
            Assert.Equal(9, buffer.CursorPosition);
        }
    }

    #endregion

    #region StartOfWord (Escape + Left)

    [Fact]
    public void StartOfWord_MovesCursorToPreviousWordBeginning()
    {
        // Cursor at position 11 ("world t|est") - between 'world' and 'test'
        var (buffer, app, scope) = CreateEnvironment("hello world test", cursorPosition: 11);
        using (scope)
        {
            app.EditingMode = EditingMode.Emacs;
            var kb = EmacsBindingsType.LoadEmacsBindings();

            var binding = FindInlineBinding(kb,
                new KeyOrChar(Keys.Escape), new KeyOrChar(Keys.Left));
            Assert.NotNull(binding);

            var @event = CreateEvent(buffer, app,
                [new KeyPress(Keys.Escape), new KeyPress(Keys.Left)]);

            binding.Handler(@event);

            // Cursor should move to the start of "world" (position 6)
            Assert.Equal(6, buffer.CursorPosition);
        }
    }

    #endregion

    #region StartNextWord (Escape + Right)

    [Fact]
    public void StartNextWord_MovesCursorToNextWordBeginning()
    {
        // Cursor at position 5 (space after "hello")
        var (buffer, app, scope) = CreateEnvironment("hello world test", cursorPosition: 5);
        using (scope)
        {
            app.EditingMode = EditingMode.Emacs;
            var kb = EmacsBindingsType.LoadEmacsBindings();

            var binding = FindInlineBinding(kb,
                new KeyOrChar(Keys.Escape), new KeyOrChar(Keys.Right));
            Assert.NotNull(binding);

            var @event = CreateEvent(buffer, app,
                [new KeyPress(Keys.Escape), new KeyPress(Keys.Right)]);

            binding.Handler(@event);

            // Cursor should move forward to start of next word
            Assert.True(buffer.CursorPosition > 5,
                $"Expected cursor to move past position 5, got {buffer.CursorPosition}");
        }
    }

    #endregion

    #region Cancel (Ctrl-G without selection)

    [Fact]
    public void Cancel_DismissesCompletionAndValidation()
    {
        var (buffer, app, scope) = CreateEnvironment("hello", cursorPosition: 5);
        using (scope)
        {
            app.EditingMode = EditingMode.Emacs;
            var kb = EmacsBindingsType.LoadEmacsBindings();

            // Find the Ctrl-G binding without selection (Cancel handler)
            var bindings = kb.GetBindingsForKeys([new KeyOrChar(Keys.ControlG)]);
            // The Cancel binding has HasSelection.Invert() filter
            // Find the one that works without selection
            var cancelBinding = bindings.FirstOrDefault(b =>
                b.Filter is not Stroke.Filters.Always);
            Assert.NotNull(cancelBinding);

            var @event = CreateEvent(buffer, app,
                [new KeyPress(Keys.ControlG)]);

            // Should not throw; calls DismissCompletion and DismissValidation
            cancelBinding.Handler(@event);

            // After cancel, complete state should be null
            Assert.Null(buffer.CompleteState);
        }
    }

    #endregion

    #region Placeholder No-ops

    [Fact]
    public void PrevSentence_IsNoOp_BufferUnchanged()
    {
        var (buffer, app, scope) = CreateEnvironment("hello world", cursorPosition: 5);
        using (scope)
        {
            app.EditingMode = EditingMode.Emacs;
            var kb = EmacsBindingsType.LoadEmacsBindings();

            var binding = FindInlineBinding(kb,
                new KeyOrChar(Keys.Escape), new KeyOrChar('a'));
            Assert.NotNull(binding);

            var @event = CreateEvent(buffer, app,
                [new KeyPress(Keys.Escape), new KeyPress('a')]);

            binding.Handler(@event);

            Assert.Equal("hello world", buffer.Text);
            Assert.Equal(5, buffer.CursorPosition);
        }
    }

    [Fact]
    public void EndOfSentence_IsNoOp_BufferUnchanged()
    {
        var (buffer, app, scope) = CreateEnvironment("hello world", cursorPosition: 5);
        using (scope)
        {
            app.EditingMode = EditingMode.Emacs;
            var kb = EmacsBindingsType.LoadEmacsBindings();

            var binding = FindInlineBinding(kb,
                new KeyOrChar(Keys.Escape), new KeyOrChar('e'));
            Assert.NotNull(binding);

            var @event = CreateEvent(buffer, app,
                [new KeyPress(Keys.Escape), new KeyPress('e')]);

            binding.Handler(@event);

            Assert.Equal("hello world", buffer.Text);
            Assert.Equal(5, buffer.CursorPosition);
        }
    }

    [Fact]
    public void SwapCharacters_IsNoOp_BufferUnchanged()
    {
        var (buffer, app, scope) = CreateEnvironment("hello world", cursorPosition: 5);
        using (scope)
        {
            app.EditingMode = EditingMode.Emacs;
            var kb = EmacsBindingsType.LoadEmacsBindings();

            var binding = FindInlineBinding(kb,
                new KeyOrChar(Keys.Escape), new KeyOrChar('t'));
            Assert.NotNull(binding);

            var @event = CreateEvent(buffer, app,
                [new KeyPress(Keys.Escape), new KeyPress('t')]);

            binding.Handler(@event);

            Assert.Equal("hello world", buffer.Text);
            Assert.Equal(5, buffer.CursorPosition);
        }
    }

    #endregion
}
