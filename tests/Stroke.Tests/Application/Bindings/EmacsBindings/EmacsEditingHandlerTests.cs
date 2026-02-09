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
/// Tests for Emacs editing handler behavior: verifies that key binding handlers
/// produce correct buffer mutations (undo, uppercase, lowercase, capitalize,
/// escape no-op) and that Vi mode does not activate Emacs-only bindings.
/// </summary>
public sealed class EmacsEditingHandlerTests : IDisposable
{
    private readonly SimplePipeInput _input;
    private readonly DummyOutput _output;

    public EmacsEditingHandlerTests()
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
        app.EditingMode = EditingMode.Emacs;
        var scope = AppContext.SetApp(app);

        return (buffer, app, scope);
    }

    /// <summary>
    /// Creates a KeyPressEvent for invoking a handler directly.
    /// </summary>
    private static KeyPressEvent MakeEvent(
        Stroke.Application.Application<object> app,
        Buffer buffer,
        Keys key)
    {
        return new KeyPressEvent(
            keyProcessorRef: null,
            arg: null,
            keySequence: [new KeyPress(key)],
            previousKeySequence: [],
            isRepeat: false,
            app: app,
            currentBuffer: buffer);
    }

    /// <summary>
    /// Find a binding whose handler matches a named command.
    /// </summary>
    private static Binding? FindNamedCommandBinding(
        IKeyBindingsBase kb, string commandName, params KeyOrChar[] keys)
    {
        var bindings = kb.GetBindingsForKeys(keys);
        var expectedHandler = NamedCommands.GetByName(commandName).Handler;
        return bindings.FirstOrDefault(b => b.Handler == expectedHandler);
    }

    #endregion

    #region Undo Handler

    [Fact]
    public void CtrlUnderscore_UndoesLastEdit()
    {
        var (buffer, app, scope) = CreateEnvironment("hello world", cursorPosition: 5);
        using (scope)
        {
            // Save state, insert text, then undo
            buffer.SaveToUndoStack();
            buffer.InsertText(" there");
            Assert.Equal("hello there world", buffer.Text);

            // Find and invoke the undo binding handler
            var kb = EmacsBindingsType.LoadEmacsBindings();
            var binding = FindNamedCommandBinding(
                kb, "undo", new KeyOrChar(Keys.ControlUnderscore));
            Assert.NotNull(binding);

            var evt = MakeEvent(app, buffer, Keys.ControlUnderscore);
            binding.Handler(evt);

            Assert.Equal("hello world", buffer.Text);
        }
    }

    #endregion

    #region Word Case Handlers

    [Fact]
    public void MetaU_UppercasesWord()
    {
        // Cursor at start, "hello world" -> uppercase "hello" -> "HELLO world"
        var (buffer, app, scope) = CreateEnvironment("hello world", cursorPosition: 0);
        using (scope)
        {
            var kb = EmacsBindingsType.LoadEmacsBindings();
            var binding = FindNamedCommandBinding(
                kb, "uppercase-word",
                new KeyOrChar(Keys.Escape), new KeyOrChar('u'));
            Assert.NotNull(binding);

            var evt = new KeyPressEvent(
                keyProcessorRef: null,
                arg: null,
                keySequence: [new KeyPress(Keys.Escape), new KeyPress('u')],
                previousKeySequence: [],
                isRepeat: false,
                app: app,
                currentBuffer: buffer);

            binding.Handler(evt);

            Assert.Equal("HELLO world", buffer.Text);
            Assert.Equal(5, buffer.CursorPosition);
        }
    }

    [Fact]
    public void MetaL_LowercasesWord()
    {
        // Cursor at start, "HELLO WORLD" -> lowercase "HELLO" -> "hello WORLD"
        var (buffer, app, scope) = CreateEnvironment("HELLO WORLD", cursorPosition: 0);
        using (scope)
        {
            var kb = EmacsBindingsType.LoadEmacsBindings();
            var binding = FindNamedCommandBinding(
                kb, "downcase-word",
                new KeyOrChar(Keys.Escape), new KeyOrChar('l'));
            Assert.NotNull(binding);

            var evt = new KeyPressEvent(
                keyProcessorRef: null,
                arg: null,
                keySequence: [new KeyPress(Keys.Escape), new KeyPress('l')],
                previousKeySequence: [],
                isRepeat: false,
                app: app,
                currentBuffer: buffer);

            binding.Handler(evt);

            Assert.Equal("hello WORLD", buffer.Text);
            Assert.Equal(5, buffer.CursorPosition);
        }
    }

    [Fact]
    public void MetaC_CapitalizesWord()
    {
        // Cursor at start, "hello world" -> capitalize "hello" -> "Hello world"
        var (buffer, app, scope) = CreateEnvironment("hello world", cursorPosition: 0);
        using (scope)
        {
            var kb = EmacsBindingsType.LoadEmacsBindings();
            var binding = FindNamedCommandBinding(
                kb, "capitalize-word",
                new KeyOrChar(Keys.Escape), new KeyOrChar('c'));
            Assert.NotNull(binding);

            var evt = new KeyPressEvent(
                keyProcessorRef: null,
                arg: null,
                keySequence: [new KeyPress(Keys.Escape), new KeyPress('c')],
                previousKeySequence: [],
                isRepeat: false,
                app: app,
                currentBuffer: buffer);

            binding.Handler(evt);

            Assert.Equal("Hello world", buffer.Text);
            Assert.Equal(5, buffer.CursorPosition);
        }
    }

    #endregion

    #region Escape No-Op Handler

    [Fact]
    public void Escape_IsConsumedSilently()
    {
        var (buffer, app, scope) = CreateEnvironment("hello world", cursorPosition: 5);
        using (scope)
        {
            var kb = EmacsBindingsType.LoadEmacsBindings();

            // Escape is the first binding (no-op handler)
            var allBindings = kb.Bindings;
            var escapeBinding = allBindings[0];
            Assert.Equal(new KeyOrChar(Keys.Escape), escapeBinding.Keys[0]);
            Assert.Single(escapeBinding.Keys);

            var evt = MakeEvent(app, buffer, Keys.Escape);
            var result = escapeBinding.Handler(evt);

            // Handler returns null (consumed, not "not implemented")
            Assert.Null(result);
            // Buffer unchanged
            Assert.Equal("hello world", buffer.Text);
            Assert.Equal(5, buffer.CursorPosition);
        }
    }

    #endregion

    #region Ctrl-N / Ctrl-P Auto Navigation

    [Fact]
    public void CtrlN_MovesDown_InMultilineBuffer()
    {
        var (buffer, app, scope) = CreateEnvironment(
            "line1\nline2\nline3", cursorPosition: 0, multiline: true);
        using (scope)
        {
            var kb = EmacsBindingsType.LoadEmacsBindings();
            var bindings = kb.GetBindingsForKeys([new KeyOrChar(Keys.ControlN)]);
            Assert.NotEmpty(bindings);

            var evt = MakeEvent(app, buffer, Keys.ControlN);
            bindings[0].Handler(evt);

            // Cursor should move to second line
            Assert.Equal(1, buffer.Document.CursorPositionRow);
        }
    }

    #endregion

    #region Vi Mode Does Not Activate Emacs Bindings

    [Fact]
    public void ViMode_DoesNotActivateEmacsBindings()
    {
        var (buffer, app, scope) = CreateEnvironment("hello world", cursorPosition: 0);
        using (scope)
        {
            // Switch to Vi mode
            app.EditingMode = EditingMode.Vi;

            var kb = EmacsBindingsType.LoadEmacsBindings();

            // GetBindingsForKeys returns bindings matched by key sequence (ignores filters).
            // The ConditionalKeyBindings composes EmacsMode into each binding's filter,
            // so with Vi mode active, all filters must evaluate to false.
            var undoBindings = kb.GetBindingsForKeys(
                [new KeyOrChar(Keys.ControlUnderscore)]);
            Assert.All(undoBindings, b => Assert.False(b.Filter.Invoke()));

            var upperBindings = kb.GetBindingsForKeys(
                [new KeyOrChar(Keys.Escape), new KeyOrChar('u')]);
            Assert.All(upperBindings, b => Assert.False(b.Filter.Invoke()));

            var lowerBindings = kb.GetBindingsForKeys(
                [new KeyOrChar(Keys.Escape), new KeyOrChar('l')]);
            Assert.All(lowerBindings, b => Assert.False(b.Filter.Invoke()));
        }
    }

    #endregion

    #region Word Case at End of Buffer

    [Fact]
    public void MetaU_AtEndOfBuffer_NoChange()
    {
        // Cursor at end of text, no word after cursor to uppercase
        var (buffer, app, scope) = CreateEnvironment("hello", cursorPosition: 5);
        using (scope)
        {
            var kb = EmacsBindingsType.LoadEmacsBindings();
            var binding = FindNamedCommandBinding(
                kb, "uppercase-word",
                new KeyOrChar(Keys.Escape), new KeyOrChar('u'));
            Assert.NotNull(binding);

            var evt = new KeyPressEvent(
                keyProcessorRef: null,
                arg: null,
                keySequence: [new KeyPress(Keys.Escape), new KeyPress('u')],
                previousKeySequence: [],
                isRepeat: false,
                app: app,
                currentBuffer: buffer);

            binding.Handler(evt);

            // No change since cursor is at end
            Assert.Equal("hello", buffer.Text);
            Assert.Equal(5, buffer.CursorPosition);
        }
    }

    #endregion
}
