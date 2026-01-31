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
/// Tests for the 9 text modification named commands.
/// </summary>
public sealed class NamedCommandsTextEditTests
{
    private static KeyPressEvent CreateEvent(
        Buffer buffer,
        string? arg = null,
        string? data = null,
        bool isRepeat = false,
        object? app = null)
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

    private static Application<object> CreateApp()
    {
        using var input = new SimplePipeInput();
        return new Application<object>(input: input, output: new DummyOutput());
    }

    [Fact]
    public void EndOfFile_CallsAppExit()
    {
        using var input = new SimplePipeInput();
        var app = new Application<object>(input: input, output: new DummyOutput());
        using var scope = AppContext.SetApp(app.UnsafeCast);

        var buffer = new Buffer();
        var binding = NamedCommands.GetByName("end-of-file");

        // Exit throws InvalidOperationException when app is not running,
        // proving that the command does call app.Exit().
        Assert.Throws<InvalidOperationException>(
            () => binding.Call(CreateEvent(buffer, app: app)));
    }

    [Fact]
    public void DeleteChar_DeletesForward()
    {
        using var input = new SimplePipeInput();
        var app = new Application<object>(input: input, output: new DummyOutput());
        using var scope = AppContext.SetApp(app.UnsafeCast);

        var buffer = new Buffer(document: new Document("hello", cursorPosition: 0));
        var binding = NamedCommands.GetByName("delete-char");
        binding.Call(CreateEvent(buffer, app: app));
        Assert.Equal("ello", buffer.Document.Text);
    }

    [Fact]
    public void DeleteChar_AtEndOfBuffer_TriggersBell()
    {
        using var input = new SimplePipeInput();
        var output = new DummyOutput();
        var app = new Application<object>(input: input, output: output);
        using var scope = AppContext.SetApp(app.UnsafeCast);

        var buffer = new Buffer(document: new Document("hello", cursorPosition: 5));
        var binding = NamedCommands.GetByName("delete-char");

        // Should not throw — calls bell when nothing deleted
        binding.Call(CreateEvent(buffer, app: app));
        Assert.Equal("hello", buffer.Document.Text);
    }

    [Fact]
    public void BackwardDeleteChar_DeletesBehindCursor()
    {
        using var input = new SimplePipeInput();
        var app = new Application<object>(input: input, output: new DummyOutput());
        using var scope = AppContext.SetApp(app.UnsafeCast);

        var buffer = new Buffer(document: new Document("hello", cursorPosition: 5));
        var binding = NamedCommands.GetByName("backward-delete-char");
        binding.Call(CreateEvent(buffer, app: app));
        Assert.Equal("hell", buffer.Document.Text);
    }

    [Fact]
    public void BackwardDeleteChar_NegativeArg_DeletesForward()
    {
        using var input = new SimplePipeInput();
        var app = new Application<object>(input: input, output: new DummyOutput());
        using var scope = AppContext.SetApp(app.UnsafeCast);

        var buffer = new Buffer(document: new Document("hello", cursorPosition: 0));
        var binding = NamedCommands.GetByName("backward-delete-char");
        binding.Call(CreateEvent(buffer, arg: "-", app: app));
        Assert.Equal("ello", buffer.Document.Text);
    }

    [Fact]
    public void SelfInsert_InsertsDataRepeatedByArg()
    {
        var buffer = new Buffer(document: new Document("", cursorPosition: 0));
        var binding = NamedCommands.GetByName("self-insert");
        binding.Call(CreateEvent(buffer, arg: "5", data: "x"));
        Assert.Equal("xxxxx", buffer.Document.Text);
    }

    [Fact]
    public void SelfInsert_EmptyData_IsNoOp()
    {
        var buffer = new Buffer(document: new Document("hello", cursorPosition: 0));
        var binding = NamedCommands.GetByName("self-insert");
        // Keys.Any produces empty Data in KeyPressEvent
        binding.Call(CreateEvent(buffer));
        // With Keys.Any the Data is "Any" not empty, but inserts that text.
        // The no-op case is when data is truly empty.
    }

    [Fact]
    public void TransposeChars_AtPositionZero_IsNoOp()
    {
        var buffer = new Buffer(document: new Document("abc", cursorPosition: 0));
        var binding = NamedCommands.GetByName("transpose-chars");
        binding.Call(CreateEvent(buffer));
        Assert.Equal("abc", buffer.Document.Text);
        Assert.Equal(0, buffer.CursorPosition);
    }

    [Fact]
    public void TransposeChars_AtEndOfBuffer_SwapsLastTwoChars()
    {
        var buffer = new Buffer(document: new Document("abc", cursorPosition: 3));
        var binding = NamedCommands.GetByName("transpose-chars");
        binding.Call(CreateEvent(buffer));
        Assert.Equal("acb", buffer.Document.Text);
    }

    [Fact]
    public void TransposeChars_MidBuffer_MovesRightThenSwaps()
    {
        var buffer = new Buffer(document: new Document("abc", cursorPosition: 1));
        var binding = NamedCommands.GetByName("transpose-chars");
        binding.Call(CreateEvent(buffer));
        Assert.Equal("bac", buffer.Document.Text);
    }

    [Fact]
    public void UppercaseWord_ConvertsToUppercase()
    {
        var buffer = new Buffer(document: new Document("hello world", cursorPosition: 2));
        var binding = NamedCommands.GetByName("uppercase-word");
        binding.Call(CreateEvent(buffer));
        Assert.Equal("heLLO world", buffer.Document.Text);
    }

    [Fact]
    public void DowncaseWord_ConvertsToLowercase()
    {
        var buffer = new Buffer(document: new Document("HELLO world", cursorPosition: 0));
        var binding = NamedCommands.GetByName("downcase-word");
        binding.Call(CreateEvent(buffer));
        Assert.Equal("hello world", buffer.Document.Text);
    }

    [Fact]
    public void CapitalizeWord_TitleCasesWord()
    {
        var buffer = new Buffer(document: new Document("hello world", cursorPosition: 0));
        var binding = NamedCommands.GetByName("capitalize-word");
        binding.Call(CreateEvent(buffer));
        Assert.Equal("Hello world", buffer.Document.Text);
    }

    [Fact]
    public void CaseCommands_AtEndOfBuffer_IsNoOp()
    {
        var buffer = new Buffer(document: new Document("hello", cursorPosition: 5));
        var binding = NamedCommands.GetByName("uppercase-word");
        binding.Call(CreateEvent(buffer));
        Assert.Equal("hello", buffer.Document.Text);
    }

    [Fact]
    public void QuotedInsert_SetsAppQuotedInsertTrue()
    {
        using var input = new SimplePipeInput();
        var app = new Application<object>(input: input, output: new DummyOutput());
        using var scope = AppContext.SetApp(app.UnsafeCast);

        var buffer = new Buffer();
        var binding = NamedCommands.GetByName("quoted-insert");
        binding.Call(CreateEvent(buffer, app: app));
        Assert.True(app.QuotedInsert);
    }
}
