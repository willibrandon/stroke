using Stroke.Application;
using Stroke.Core;
using Stroke.Input.Pipe;
using Stroke.KeyBinding;
using Stroke.KeyBinding.Bindings;
using Stroke.Output;
using Xunit;
using AppContext = Stroke.Application.AppContext;
using Buffer = Stroke.Core.Buffer;
using Keys = Stroke.Input.Keys;

namespace Stroke.Tests.KeyBinding.Bindings;

/// <summary>
/// Tests for the 3 completion named commands.
/// </summary>
public sealed class NamedCommandsCompletionTests
{
    private static KeyPressEvent CreateEvent(
        Buffer buffer,
        string? arg = null,
        IApplication? app = null)
    {
        return new KeyPressEvent(
            keyProcessorRef: null,
            arg: arg,
            keySequence: [new KeyPress(Keys.Any)],
            previousKeySequence: [],
            isRepeat: false,
            app: app,
            currentBuffer: buffer);
    }

    [Fact]
    public void Complete_WithNoCompleter_IsNoOp()
    {
        using var input = new SimplePipeInput();
        var app = new Application<object>(input: input, output: new DummyOutput());
        using var scope = AppContext.SetApp(app);

        var buffer = new Buffer(document: new Document("hello", cursorPosition: 5));
        var binding = NamedCommands.GetByName("complete");
        // No completer configured — should be safe no-op
        binding.Call(CreateEvent(buffer, app: app));
    }

    [Fact]
    public void MenuComplete_WithNoCompleter_IsNoOp()
    {
        using var input = new SimplePipeInput();
        var app = new Application<object>(input: input, output: new DummyOutput());
        using var scope = AppContext.SetApp(app);

        var buffer = new Buffer(document: new Document("hello", cursorPosition: 5));
        var binding = NamedCommands.GetByName("menu-complete");
        binding.Call(CreateEvent(buffer, app: app));
    }

    [Fact]
    public void MenuCompleteBackward_WithNoCompletions_IsNoOp()
    {
        var buffer = new Buffer(document: new Document("hello", cursorPosition: 5));
        var binding = NamedCommands.GetByName("menu-complete-backward");
        binding.Call(CreateEvent(buffer));
    }

    [Fact]
    public void Complete_IsRegistered()
    {
        var binding = NamedCommands.GetByName("complete");
        Assert.NotNull(binding);
    }

    [Fact]
    public void MenuComplete_IsRegistered()
    {
        var binding = NamedCommands.GetByName("menu-complete");
        Assert.NotNull(binding);
    }

    [Fact]
    public void MenuCompleteBackward_IsRegistered()
    {
        var binding = NamedCommands.GetByName("menu-complete-backward");
        Assert.NotNull(binding);
    }
}
