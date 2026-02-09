using Stroke.Application;
using Stroke.Core;
using Stroke.Filters;
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
/// Tests for the 4 macro named commands.
/// </summary>
public sealed class NamedCommandsMacroTests
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
    public void StartKbdMacro_BeginsRecording()
    {
        using var input = new SimplePipeInput();
        var app = new Application<object>(input: input, output: new DummyOutput());
        using var scope = AppContext.SetApp(app);

        var buffer = new Buffer();
        var binding = NamedCommands.GetByName("start-kbd-macro");
        binding.Call(CreateEvent(buffer, app: app));

        Assert.True(app.EmacsState.IsRecording);
    }

    [Fact]
    public void EndKbdMacro_StopsRecording()
    {
        using var input = new SimplePipeInput();
        var app = new Application<object>(input: input, output: new DummyOutput());
        using var scope = AppContext.SetApp(app);

        app.EmacsState.StartMacro();
        Assert.True(app.EmacsState.IsRecording);

        var buffer = new Buffer();
        var binding = NamedCommands.GetByName("end-kbd-macro");
        binding.Call(CreateEvent(buffer, app: app));

        Assert.False(app.EmacsState.IsRecording);
    }

    [Fact]
    public void CallLastKbdMacro_WhenNoMacroRecorded_IsNoOp()
    {
        using var input = new SimplePipeInput();
        var app = new Application<object>(input: input, output: new DummyOutput());
        using var scope = AppContext.SetApp(app);

        var buffer = new Buffer();
        var binding = NamedCommands.GetByName("call-last-kbd-macro");
        // No macro recorded — should be no-op
        binding.Call(CreateEvent(buffer, app: app));
    }

    [Fact]
    public void CallLastKbdMacro_BindingHasRecordInMacroFalse()
    {
        var binding = NamedCommands.GetByName("call-last-kbd-macro");
        // RecordInMacro should be false (Never) to prevent infinite recursion
        Assert.False(binding.RecordInMacro.Invoke());
    }

    [Fact]
    public void StartKbdMacro_WhenAlreadyRecording_DelegatesToEmacsState()
    {
        using var input = new SimplePipeInput();
        var app = new Application<object>(input: input, output: new DummyOutput());
        using var scope = AppContext.SetApp(app);

        app.EmacsState.StartMacro();
        var buffer = new Buffer();
        var binding = NamedCommands.GetByName("start-kbd-macro");
        // Should not throw — EmacsState handles this
        binding.Call(CreateEvent(buffer, app: app));
    }

    [Fact]
    public void EndKbdMacro_WhenNotRecording_DelegatesToEmacsState()
    {
        using var input = new SimplePipeInput();
        var app = new Application<object>(input: input, output: new DummyOutput());
        using var scope = AppContext.SetApp(app);

        var buffer = new Buffer();
        var binding = NamedCommands.GetByName("end-kbd-macro");
        // Should not throw — EmacsState handles this
        binding.Call(CreateEvent(buffer, app: app));
    }

    [Fact]
    public void PrintLastKbdMacro_IsRegistered()
    {
        var binding = NamedCommands.GetByName("print-last-kbd-macro");
        Assert.NotNull(binding);
    }
}
