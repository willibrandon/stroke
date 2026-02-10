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
/// Tests for the 7 miscellaneous named commands.
/// </summary>
public sealed class NamedCommandsMiscTests
{
    private static KeyPressEvent CreateEvent(
        Buffer buffer,
        string? arg = null,
        string? data = null,
        bool isRepeat = false,
        IApplication? app = null)
    {
        var keySequence = data is not null
            ? new List<KeyPress> { new(new KeyOrChar(data[0]), data) }
            : new List<KeyPress> { new(Keys.Any) };

        return new KeyPressEvent(
            keyProcessorRef: null,
            arg: arg,
            keySequence: keySequence,
            previousKeySequence: [],
            isRepeat: isRepeat,
            app: app,
            currentBuffer: buffer);
    }

    [Fact]
    public void Undo_CallsBufferUndo()
    {
        var buffer = new Buffer(document: new Document("hello", cursorPosition: 5));

        // Save initial state, then modify
        buffer.SaveToUndoStack();
        buffer.InsertText(" world");
        Assert.Equal("hello world", buffer.Document.Text);

        var binding = NamedCommands.GetByName("undo");
        binding.Call(CreateEvent(buffer));

        Assert.Equal("hello", buffer.Document.Text);
    }

    [Fact]
    public void InsertComment_DefaultArg_PrependsHash()
    {
        string? capturedText = null;
        var buffer = new Buffer(
            document: new Document("hello\nworld", cursorPosition: 0),
            acceptHandler: b => { capturedText = b.Document.Text; return ValueTask.FromResult(true); });
        var binding = NamedCommands.GetByName("insert-comment");
        binding.Call(CreateEvent(buffer));

        Assert.Equal("#hello\n#world", capturedText);
    }

    [Fact]
    public void InsertComment_NonOneArg_RemovesLeadingHash()
    {
        string? capturedText = null;
        var buffer = new Buffer(
            document: new Document("#hello\n#world", cursorPosition: 0),
            acceptHandler: b => { capturedText = b.Document.Text; return ValueTask.FromResult(true); });
        var binding = NamedCommands.GetByName("insert-comment");
        binding.Call(CreateEvent(buffer, arg: "2"));

        Assert.Equal("hello\nworld", capturedText);
    }

    [Fact]
    public void InsertComment_NonOneArg_LeavesLinesWithoutHash()
    {
        string? capturedText = null;
        var buffer = new Buffer(
            document: new Document("#hello\nworld", cursorPosition: 0),
            acceptHandler: b => { capturedText = b.Document.Text; return ValueTask.FromResult(true); });
        var binding = NamedCommands.GetByName("insert-comment");
        binding.Call(CreateEvent(buffer, arg: "2"));

        Assert.Equal("hello\nworld", capturedText);
    }

    [Fact]
    public void InsertComment_SingleLine_PrependsHash()
    {
        string? capturedText = null;
        var buffer = new Buffer(
            document: new Document("echo test", cursorPosition: 0),
            acceptHandler: b => { capturedText = b.Document.Text; return ValueTask.FromResult(true); });
        var binding = NamedCommands.GetByName("insert-comment");
        binding.Call(CreateEvent(buffer));

        Assert.Equal("#echo test", capturedText);
    }

    [Fact]
    public void InsertComment_TrailingNewline_DoesNotAddExtraHash()
    {
        // Python: "a\nb\n".splitlines() == ['a', 'b'] (no trailing empty)
        // So insert_comment produces "#a\n#b", not "#a\n#b\n#"
        string? capturedText = null;
        var buffer = new Buffer(
            document: new Document("a\nb\n", cursorPosition: 0),
            acceptHandler: b => { capturedText = b.Document.Text; return ValueTask.FromResult(true); });
        var binding = NamedCommands.GetByName("insert-comment");
        binding.Call(CreateEvent(buffer));

        Assert.Equal("#a\n#b", capturedText);
    }

    [Fact]
    public void ViEditingMode_SwitchesToVi()
    {
        using var input = new SimplePipeInput();
        var app = new Application<object>(input: input, output: new DummyOutput());
        using var scope = AppContext.SetApp(app);

        app.EditingMode = EditingMode.Emacs;
        var buffer = new Buffer();
        var binding = NamedCommands.GetByName("vi-editing-mode");
        binding.Call(CreateEvent(buffer, app: app));

        Assert.Equal(EditingMode.Vi, app.EditingMode);
    }

    [Fact]
    public void EmacsEditingMode_SwitchesToEmacs()
    {
        using var input = new SimplePipeInput();
        var app = new Application<object>(input: input, output: new DummyOutput());
        using var scope = AppContext.SetApp(app);

        app.EditingMode = EditingMode.Vi;
        var buffer = new Buffer();
        var binding = NamedCommands.GetByName("emacs-editing-mode");
        binding.Call(CreateEvent(buffer, app: app));

        Assert.Equal(EditingMode.Emacs, app.EditingMode);
    }

    [Fact]
    public void PrefixMeta_FeedsEscapeKey()
    {
        using var input = new SimplePipeInput();
        var app = new Application<object>(input: input, output: new DummyOutput());
        using var scope = AppContext.SetApp(app);

        var buffer = new Buffer();
        var binding = NamedCommands.GetByName("prefix-meta");

        // Should not throw — feeds Escape key into key processor
        binding.Call(CreateEvent(buffer, app: app));
    }

    [Fact]
    public void OperateAndGetNext_AcceptsInputAndQueuesNextHistory()
    {
        using var input = new SimplePipeInput();
        var app = new Application<object>(input: input, output: new DummyOutput());
        using var scope = AppContext.SetApp(app);

        var buffer = new Buffer(document: new Document("current", cursorPosition: 0));
        var binding = NamedCommands.GetByName("operate-and-get-next");

        // Should not throw
        binding.Call(CreateEvent(buffer, app: app));

        // Should have added a PreRunCallable
        Assert.NotEmpty(app.PreRunCallables);
    }

    [Fact]
    public void EditAndExecuteCommand_IsRegistered()
    {
        var binding = NamedCommands.GetByName("edit-and-execute-command");
        Assert.NotNull(binding);
    }

    [Fact]
    public void Undo_IsRegistered()
    {
        var binding = NamedCommands.GetByName("undo");
        Assert.NotNull(binding);
    }

    [Fact]
    public void InsertComment_IsRegistered()
    {
        var binding = NamedCommands.GetByName("insert-comment");
        Assert.NotNull(binding);
    }

    [Fact]
    public void PrefixMeta_IsRegistered()
    {
        var binding = NamedCommands.GetByName("prefix-meta");
        Assert.NotNull(binding);
    }
}
