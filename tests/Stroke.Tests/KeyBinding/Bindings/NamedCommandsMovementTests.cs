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
/// Tests for the 10 movement named commands.
/// </summary>
public sealed class NamedCommandsMovementTests
{
    private static KeyPressEvent CreateEvent(
        Buffer buffer,
        string? arg = null,
        string? data = null,
        bool isRepeat = false,
        IApplication? app = null)
    {
        var keySequence = data is not null
            ? [new KeyPress(new KeyOrChar(data[0]), data)]
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
    public void BeginningOfBuffer_SetsCursorToZero()
    {
        var buffer = new Buffer(document: new Document("hello world", cursorPosition: 5));
        var binding = NamedCommands.GetByName("beginning-of-buffer");
        binding.Call(CreateEvent(buffer));
        Assert.Equal(0, buffer.CursorPosition);
    }

    [Fact]
    public void EndOfBuffer_SetsCursorToEnd()
    {
        var buffer = new Buffer(document: new Document("hello world", cursorPosition: 0));
        var binding = NamedCommands.GetByName("end-of-buffer");
        binding.Call(CreateEvent(buffer));
        Assert.Equal(11, buffer.CursorPosition);
    }

    [Fact]
    public void BeginningOfLine_MovesToStartOfCurrentLine()
    {
        // Multi-line: cursor is on line 2 at position "hello\nwor" = 9
        var buffer = new Buffer(document: new Document("hello\nworld", cursorPosition: 9));
        var binding = NamedCommands.GetByName("beginning-of-line");
        binding.Call(CreateEvent(buffer));
        Assert.Equal(6, buffer.CursorPosition); // Start of "world" line
    }

    [Fact]
    public void EndOfLine_MovesToEndOfCurrentLine()
    {
        var buffer = new Buffer(document: new Document("hello\nworld", cursorPosition: 6));
        var binding = NamedCommands.GetByName("end-of-line");
        binding.Call(CreateEvent(buffer));
        Assert.Equal(11, buffer.CursorPosition);
    }

    [Fact]
    public void ForwardChar_MovesCursorRightByOne()
    {
        var buffer = new Buffer(document: new Document("hello", cursorPosition: 0));
        var binding = NamedCommands.GetByName("forward-char");
        binding.Call(CreateEvent(buffer));
        Assert.Equal(1, buffer.CursorPosition);
    }

    [Fact]
    public void ForwardChar_WithArg3_MovesCursorRightBy3()
    {
        var buffer = new Buffer(document: new Document("hello world", cursorPosition: 0));
        var binding = NamedCommands.GetByName("forward-char");
        binding.Call(CreateEvent(buffer, arg: "3"));
        Assert.Equal(3, buffer.CursorPosition);
    }

    [Fact]
    public void BackwardChar_MovesCursorLeftByOne()
    {
        var buffer = new Buffer(document: new Document("hello", cursorPosition: 3));
        var binding = NamedCommands.GetByName("backward-char");
        binding.Call(CreateEvent(buffer));
        Assert.Equal(2, buffer.CursorPosition);
    }

    [Fact]
    public void ForwardWord_MovesToEndOfNextWord()
    {
        // "hello world" with cursor at 5 (space) → should move to 11 (end)
        var buffer = new Buffer(document: new Document("hello world", cursorPosition: 5));
        var binding = NamedCommands.GetByName("forward-word");
        binding.Call(CreateEvent(buffer));
        Assert.Equal(11, buffer.CursorPosition);
    }

    [Fact]
    public void BackwardWord_MovesToStartOfPreviousWord()
    {
        var buffer = new Buffer(document: new Document("hello world", cursorPosition: 8));
        var binding = NamedCommands.GetByName("backward-word");
        binding.Call(CreateEvent(buffer));
        Assert.Equal(6, buffer.CursorPosition);
    }

    [Fact]
    public void ForwardChar_AtEndOfBuffer_IsNoOp()
    {
        var buffer = new Buffer(document: new Document("hello", cursorPosition: 5));
        var binding = NamedCommands.GetByName("forward-char");
        binding.Call(CreateEvent(buffer));
        Assert.Equal(5, buffer.CursorPosition);
    }

    [Fact]
    public void BackwardChar_AtPositionZero_IsNoOp()
    {
        var buffer = new Buffer(document: new Document("hello", cursorPosition: 0));
        var binding = NamedCommands.GetByName("backward-char");
        binding.Call(CreateEvent(buffer));
        Assert.Equal(0, buffer.CursorPosition);
    }

    [Fact]
    public void BackwardWord_AtPositionZero_IsNoOp()
    {
        var buffer = new Buffer(document: new Document("hello", cursorPosition: 0));
        var binding = NamedCommands.GetByName("backward-word");
        binding.Call(CreateEvent(buffer));
        Assert.Equal(0, buffer.CursorPosition);
    }

    [Fact]
    public void ForwardWord_OnWhitespaceOnlyText_IsNoOp()
    {
        var buffer = new Buffer(document: new Document("   ", cursorPosition: 0));
        var binding = NamedCommands.GetByName("forward-word");
        binding.Call(CreateEvent(buffer));
        // FindNextWordEnding returns null for whitespace-only → no movement
        Assert.Equal(0, buffer.CursorPosition);
    }

    [Fact]
    public void BackwardWord_OnWhitespaceOnlyText_IsNoOp()
    {
        var buffer = new Buffer(document: new Document("   ", cursorPosition: 3));
        var binding = NamedCommands.GetByName("backward-word");
        binding.Call(CreateEvent(buffer));
        Assert.Equal(3, buffer.CursorPosition);
    }

    [Fact]
    public void ClearScreen_CallsRendererClear()
    {
        using var input = new SimplePipeInput();
        var output = new DummyOutput();
        var app = new Stroke.Application.Application<object>(input: input, output: output);
        using var scope = AppContext.SetApp(app);

        var buffer = new Buffer(document: new Document("hello", cursorPosition: 0));
        var binding = NamedCommands.GetByName("clear-screen");

        // Should not throw — calls renderer.Clear()
        binding.Call(CreateEvent(buffer, app: app));
    }

    [Fact]
    public void RedrawCurrentLine_IsNoOp()
    {
        var buffer = new Buffer(document: new Document("hello", cursorPosition: 0));
        var binding = NamedCommands.GetByName("redraw-current-line");

        // No-op: should not throw and should not change anything
        binding.Call(CreateEvent(buffer));
        Assert.Equal(0, buffer.CursorPosition);
    }
}
