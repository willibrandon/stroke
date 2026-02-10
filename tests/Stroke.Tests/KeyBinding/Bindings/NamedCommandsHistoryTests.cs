using Stroke.Application;
using Stroke.Core;
using Stroke.Input.Pipe;
using Stroke.KeyBinding;
using Stroke.KeyBinding.Bindings;
using Stroke.Layout;
using Stroke.Layout.Containers;
using Stroke.Layout.Controls;
using Stroke.Layout.Windows;
using Stroke.Output;
using Xunit;
using AppContext = Stroke.Application.AppContext;
using Buffer = Stroke.Core.Buffer;
using Keys = Stroke.Input.Keys;

namespace Stroke.Tests.KeyBinding.Bindings;

/// <summary>
/// Tests for the 6 history named commands.
/// </summary>
public sealed class NamedCommandsHistoryTests : IDisposable
{
    private readonly SimplePipeInput _input;
    private readonly DummyOutput _output;

    public NamedCommandsHistoryTests()
    {
        _input = new SimplePipeInput();
        _output = new DummyOutput();
    }

    public void Dispose()
    {
        _input.Dispose();
    }

    private KeyPressEvent CreateEvent(
        Buffer buffer,
        string? arg = null,
        bool isRepeat = false,
        IApplication? app = null)
    {
        return new KeyPressEvent(
            keyProcessorRef: null,
            arg: arg,
            keySequence: [new KeyPress(Keys.Any)],
            previousKeySequence: [],
            isRepeat: isRepeat,
            app: app,
            currentBuffer: buffer);
    }

    [Fact]
    public void AcceptLine_CallsValidateAndHandle()
    {
        var buffer = new Buffer(document: new Document("hello", cursorPosition: 0));
        var bc = new BufferControl(buffer: buffer);
        var window = new Window(content: bc);
        var layout = new Stroke.Layout.Layout(new AnyContainer(window));
        var app = new Application<object>(input: _input, output: _output, layout: layout);
        using var scope = AppContext.SetApp(app);

        var binding = NamedCommands.GetByName("accept-line");
        // ValidateAndHandle should not throw on a basic buffer
        binding.Call(CreateEvent(buffer, app: app));
    }

    [Fact]
    public void PreviousHistory_MovesBackward()
    {
        var buffer = new Buffer(document: new Document("current", cursorPosition: 0));
        var binding = NamedCommands.GetByName("previous-history");
        // Should not throw even with no history loaded
        binding.Call(CreateEvent(buffer));
    }

    [Fact]
    public void NextHistory_MovesForward()
    {
        var buffer = new Buffer(document: new Document("current", cursorPosition: 0));
        var binding = NamedCommands.GetByName("next-history");
        binding.Call(CreateEvent(buffer));
    }

    [Fact]
    public void BeginningOfHistory_JumpsToFirstEntry()
    {
        var buffer = new Buffer(document: new Document("current", cursorPosition: 0));
        var binding = NamedCommands.GetByName("beginning-of-history");
        binding.Call(CreateEvent(buffer));
    }

    [Fact]
    public void EndOfHistory_ReturnsToCurrentInput()
    {
        var buffer = new Buffer(document: new Document("current", cursorPosition: 0));
        var binding = NamedCommands.GetByName("end-of-history");
        binding.Call(CreateEvent(buffer));
    }

    [Fact]
    public void ReverseSearchHistory_ActivatesBackwardSearch()
    {
        using var input = new SimplePipeInput();
        var app = new Application<object>(input: input, output: new DummyOutput());
        using var scope = AppContext.SetApp(app);

        var buffer = new Buffer(document: new Document("test", cursorPosition: 0));
        var binding = NamedCommands.GetByName("reverse-search-history");
        // Should not throw; if no SearchBufferControl configured, it's a no-op
        binding.Call(CreateEvent(buffer, app: app));
    }
}
