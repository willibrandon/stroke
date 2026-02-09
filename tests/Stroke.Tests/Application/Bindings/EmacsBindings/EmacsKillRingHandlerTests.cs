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
/// Tests for kill ring handler behavior: verifies that kill-word, backward-kill-word,
/// yank, and delete-horizontal-space commands produce correct buffer mutations
/// when invoked through their registered binding handlers.
/// </summary>
public sealed class EmacsKillRingHandlerTests : IDisposable
{
    private readonly SimplePipeInput _input;
    private readonly DummyOutput _output;

    public EmacsKillRingHandlerTests()
    {
        _input = new SimplePipeInput();
        _output = new DummyOutput();
    }

    public void Dispose()
    {
        _input.Dispose();
    }

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

    private KeyPressEvent CreateEvent(
        Stroke.Application.Application<object> app,
        Buffer buffer,
        Keys[] keySequence,
        string? lastKeyData = null)
    {
        var sequence = new List<KeyPress>();
        for (int i = 0; i < keySequence.Length; i++)
        {
            var data = (i == keySequence.Length - 1 && lastKeyData != null)
                ? lastKeyData
                : null;
            sequence.Add(new KeyPress(keySequence[i], data));
        }

        return new KeyPressEvent(
            keyProcessorRef: null,
            arg: null,
            keySequence: sequence,
            previousKeySequence: [],
            isRepeat: false,
            app: app,
            currentBuffer: buffer);
    }

    /// <summary>
    /// Meta-d (kill-word): from "hello world" cursor at 0, kills "hello" leaving " world".
    /// The killed text is placed on the clipboard.
    /// </summary>
    [Fact]
    public void MetaD_KillWord_DeletesForwardWordAndSetsClipboard()
    {
        var (buffer, app, scope) = CreateEnvironment("hello world", cursorPosition: 0);
        using (scope)
        {
            app.EditingMode = Stroke.KeyBinding.EditingMode.Emacs;
            var kb = EmacsBindingsType.LoadEmacsBindings();
            var bindings = kb.GetBindingsForKeys(
                [new KeyOrChar(Keys.Escape), new KeyOrChar('d')]);
            var binding = bindings.First(
                b => b.Handler == NamedCommands.GetByName("kill-word").Handler);

            var @event = CreateEvent(app, buffer, [Keys.Escape, Keys.Any], "d");
            binding.Handler(@event);

            Assert.Equal(" world", buffer.Text);
            Assert.Equal(0, buffer.CursorPosition);
            Assert.Equal("hello", app.Clipboard.GetData().Text);
        }
    }

    /// <summary>
    /// Ctrl-Delete (kill-word): same behavior as Meta-d, kills forward word.
    /// </summary>
    [Fact]
    public void CtrlDelete_KillWord_DeletesForwardWord()
    {
        var (buffer, app, scope) = CreateEnvironment("hello world", cursorPosition: 0);
        using (scope)
        {
            app.EditingMode = Stroke.KeyBinding.EditingMode.Emacs;
            var kb = EmacsBindingsType.LoadEmacsBindings();
            var bindings = kb.GetBindingsForKeys([new KeyOrChar(Keys.ControlDelete)]);
            var binding = bindings.First(
                b => b.Handler == NamedCommands.GetByName("kill-word").Handler);

            var @event = CreateEvent(app, buffer, [Keys.ControlDelete]);
            binding.Handler(@event);

            Assert.Equal(" world", buffer.Text);
            Assert.Equal("hello", app.Clipboard.GetData().Text);
        }
    }

    /// <summary>
    /// Meta-Backspace (backward-kill-word): from "hello world" cursor at end,
    /// kills "world" leaving "hello ".
    /// </summary>
    [Fact]
    public void EscapeBackspace_BackwardKillWord_DeletesBackwardWord()
    {
        var (buffer, app, scope) = CreateEnvironment("hello world", cursorPosition: 11);
        using (scope)
        {
            app.EditingMode = Stroke.KeyBinding.EditingMode.Emacs;
            var kb = EmacsBindingsType.LoadEmacsBindings();
            var bindings = kb.GetBindingsForKeys(
                [new KeyOrChar(Keys.Escape), new KeyOrChar(Keys.ControlH)]);
            var binding = bindings.First(
                b => b.Handler == NamedCommands.GetByName("backward-kill-word").Handler);

            var @event = CreateEvent(app, buffer, [Keys.Escape, Keys.ControlH]);
            binding.Handler(@event);

            Assert.Equal("hello ", buffer.Text);
            Assert.Equal(6, buffer.CursorPosition);
            Assert.Equal("world", app.Clipboard.GetData().Text);
        }
    }

    /// <summary>
    /// Ctrl-Y (yank): yanks text from clipboard into buffer at cursor position.
    /// </summary>
    [Fact]
    public void CtrlY_Yank_InsertsClipboardTextAtCursor()
    {
        var (buffer, app, scope) = CreateEnvironment("hello ", cursorPosition: 6);
        using (scope)
        {
            app.EditingMode = Stroke.KeyBinding.EditingMode.Emacs;
            app.Clipboard.SetText("world");

            var kb = EmacsBindingsType.LoadEmacsBindings();
            var bindings = kb.GetBindingsForKeys([new KeyOrChar(Keys.ControlY)]);
            var binding = bindings.First(
                b => b.Handler == NamedCommands.GetByName("yank").Handler);

            var @event = CreateEvent(app, buffer, [Keys.ControlY]);
            binding.Handler(@event);

            Assert.Equal("hello world", buffer.Text);
            Assert.Equal(11, buffer.CursorPosition);
        }
    }

    /// <summary>
    /// Meta-\ (delete-horizontal-space): removes horizontal whitespace (spaces/tabs)
    /// around the cursor position.
    /// </summary>
    [Fact]
    public void EscapeBackslash_DeleteHorizontalSpace_RemovesWhitespaceAroundCursor()
    {
        var (buffer, app, scope) = CreateEnvironment("hello   world", cursorPosition: 7);
        using (scope)
        {
            app.EditingMode = Stroke.KeyBinding.EditingMode.Emacs;
            var kb = EmacsBindingsType.LoadEmacsBindings();
            var bindings = kb.GetBindingsForKeys(
                [new KeyOrChar(Keys.Escape), new KeyOrChar('\\')]);
            var binding = bindings.First(
                b => b.Handler == NamedCommands.GetByName("delete-horizontal-space").Handler);

            var @event = CreateEvent(app, buffer, [Keys.Escape, Keys.Any], "\\");
            binding.Handler(@event);

            Assert.Equal("helloworld", buffer.Text);
        }
    }
}
