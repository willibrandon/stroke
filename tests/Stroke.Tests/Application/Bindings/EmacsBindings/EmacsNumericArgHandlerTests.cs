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
/// Tests for Emacs numeric argument handler behavior and character search handlers.
/// Verifies HandleDigit, MetaDash, DashWhenArg, GotoChar, and GotoCharBackwards
/// produce correct state changes when invoked.
/// </summary>
public sealed class EmacsNumericArgHandlerTests : IDisposable
{
    private readonly SimplePipeInput _input;
    private readonly DummyOutput _output;

    public EmacsNumericArgHandlerTests()
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

        app.EditingMode = EditingMode.Emacs;

        return (buffer, app, scope);
    }

    /// <summary>
    /// Helper to find a binding by key sequence from the loaded Emacs bindings.
    /// </summary>
    private static Binding? FindBinding(
        IKeyBindingsBase kb, params KeyOrChar[] keys)
    {
        var bindings = kb.GetBindingsForKeys(keys);
        return bindings.Count > 0 ? bindings[^1] : null;
    }

    [Fact]
    public void HandleDigit_Meta5_AccumulatesDigitToArgCount()
    {
        var (buffer, app, scope) = CreateEnvironment("hello");
        using (scope)
        {
            var kb = EmacsBindingsType.LoadEmacsBindings();

            // Find the Escape+'5' binding (HandleDigit)
            var binding = FindBinding(
                kb, new KeyOrChar(Keys.Escape), new KeyOrChar('5'));
            Assert.NotNull(binding);

            // Create the event with the key sequence
            var @event = new KeyPressEvent(
                keyProcessorRef: null,
                arg: null,
                keySequence: [new KeyPress(Keys.Escape), new KeyPress(new KeyOrChar('5'), "5")],
                previousKeySequence: [],
                isRepeat: false,
                app: app,
                currentBuffer: buffer);

            // Invoke the handler
            binding.Handler(@event);

            // After HandleDigit, the arg should have been appended
            Assert.True(@event.ArgPresent);
            Assert.Equal(5, @event.Arg);
        }
    }

    [Fact]
    public void MetaDash_SetsNegativePrefixWhenNoArgPresent()
    {
        var (buffer, app, scope) = CreateEnvironment("hello");
        using (scope)
        {
            var kb = EmacsBindingsType.LoadEmacsBindings();

            // Find the Escape+'-' binding (MetaDash) -- filter: ~HasArg
            var binding = FindBinding(
                kb, new KeyOrChar(Keys.Escape), new KeyOrChar('-'));
            Assert.NotNull(binding);

            // Create event with NO existing arg (ArgPresent will be false)
            var @event = new KeyPressEvent(
                keyProcessorRef: null,
                arg: null,
                keySequence: [new KeyPress(Keys.Escape), new KeyPress(new KeyOrChar('-'), "-")],
                previousKeySequence: [],
                isRepeat: false,
                app: app,
                currentBuffer: buffer);

            Assert.False(@event.ArgPresent);

            binding.Handler(@event);

            // MetaDash calls AppendToArgCount("-"), so arg should now be "-"
            Assert.True(@event.ArgPresent);
            Assert.Equal(-1, @event.Arg);
        }
    }

    [Fact]
    public void DashWhenArg_SetsKeyProcessorArgToDash()
    {
        var (buffer, app, scope) = CreateEnvironment("hello");
        using (scope)
        {
            var kb = EmacsBindingsType.LoadEmacsBindings();

            // Find the '-' binding with IsArg filter (DashWhenArg)
            var binding = FindBinding(kb, new KeyOrChar('-'));
            Assert.NotNull(binding);

            // Create a real KeyProcessor so the handler can set Arg on it
            var processor = new KeyProcessor(kb);
            processor.Arg = "-";

            var @event = new KeyPressEvent(
                keyProcessorRef: new WeakReference<object>(processor),
                arg: "-",
                keySequence: [new KeyPress(new KeyOrChar('-'), "-")],
                previousKeySequence: [],
                isRepeat: false,
                app: app,
                currentBuffer: buffer);

            binding.Handler(@event);

            // DashWhenArg sets KeyProcessor.Arg = "-"
            Assert.Equal("-", processor.Arg);
        }
    }

    [Fact]
    public void GotoChar_FindsCharForwardAndMovesCursor()
    {
        // "hello" with cursor at 0 -> search for 'l' forward -> first 'l' is at index 2
        var (buffer, app, scope) = CreateEnvironment("hello", cursorPosition: 0);
        using (scope)
        {
            var kb = EmacsBindingsType.LoadEmacsBindings();

            // Find Ctrl-] + Any binding (GotoChar)
            var binding = FindBinding(
                kb, new KeyOrChar(Keys.ControlSquareClose), new KeyOrChar(Keys.Any));
            Assert.NotNull(binding);

            // Create event with Data="l" (from the last key in sequence)
            var @event = new KeyPressEvent(
                keyProcessorRef: null,
                arg: null,
                keySequence:
                [
                    new KeyPress(Keys.ControlSquareClose),
                    new KeyPress(new KeyOrChar(Keys.Any), "l")
                ],
                previousKeySequence: [],
                isRepeat: false,
                app: app,
                currentBuffer: buffer);

            Assert.Equal(0, buffer.CursorPosition);

            binding.Handler(@event);

            // Document.Find("l", inCurrentLine: true, count: 1) from position 0
            // skips position 0, searches "ello", finds 'l' at index 1 in substring -> offset 2
            Assert.Equal(2, buffer.CursorPosition);
        }
    }

    [Fact]
    public void GotoCharBackwards_FindsCharBackwardAndMovesCursor()
    {
        // "hello world" with cursor at end (11) -> search for 'l' backward
        var (buffer, app, scope) = CreateEnvironment("hello world", cursorPosition: 11);
        using (scope)
        {
            var kb = EmacsBindingsType.LoadEmacsBindings();

            // Find Escape + Ctrl-] + Any binding (GotoCharBackwards)
            var binding = FindBinding(
                kb,
                new KeyOrChar(Keys.Escape),
                new KeyOrChar(Keys.ControlSquareClose),
                new KeyOrChar(Keys.Any));
            Assert.NotNull(binding);

            var @event = new KeyPressEvent(
                keyProcessorRef: null,
                arg: null,
                keySequence:
                [
                    new KeyPress(Keys.Escape),
                    new KeyPress(Keys.ControlSquareClose),
                    new KeyPress(new KeyOrChar(Keys.Any), "l")
                ],
                previousKeySequence: [],
                isRepeat: false,
                app: app,
                currentBuffer: buffer);

            Assert.Equal(11, buffer.CursorPosition);

            binding.Handler(@event);

            // FindBackwards("l", inCurrentLine: true, count: 1) from position 11
            // "hello world" reversed from cursor: "dlrow olle" -> finds 'l' -> moves back
            Assert.True(buffer.CursorPosition < 11);
            // The 'l' closest to cursor from behind in "hello world" is at index 9 ("d" is 10)
            Assert.Equal(9, buffer.CursorPosition);
        }
    }
}
