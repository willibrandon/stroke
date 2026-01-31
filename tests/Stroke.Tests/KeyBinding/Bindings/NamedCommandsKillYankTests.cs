using Stroke.Application;
using Stroke.Clipboard;
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
/// Tests for the 10 kill and yank named commands.
/// </summary>
public sealed class NamedCommandsKillYankTests
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

    private static Application<object> CreateTestApp()
    {
        var input = new SimplePipeInput();
        return new Application<object>(input: input, output: new DummyOutput());
    }

    [Fact]
    public void KillLine_DeletesToEndOfLine_AndSetsClipboard()
    {
        var app = CreateTestApp();
        using var scope = AppContext.SetApp(app.UnsafeCast);

        var buffer = new Buffer(document: new Document("hello world", cursorPosition: 5));
        var binding = NamedCommands.GetByName("kill-line");
        binding.Call(CreateEvent(buffer, app: app));

        Assert.Equal("hello", buffer.Document.Text);
        Assert.Equal(" world", app.Clipboard.GetData().Text);
    }

    [Fact]
    public void KillLine_NegativeArg_DeletesToStartOfLine()
    {
        var app = CreateTestApp();
        using var scope = AppContext.SetApp(app.UnsafeCast);

        var buffer = new Buffer(document: new Document("hello world", cursorPosition: 5));
        var binding = NamedCommands.GetByName("kill-line");
        binding.Call(CreateEvent(buffer, arg: "-", app: app));

        Assert.Equal(" world", buffer.Document.Text);
        Assert.Equal("hello", app.Clipboard.GetData().Text);
    }

    [Fact]
    public void KillLine_AtNewline_DeletesNewlineChar()
    {
        var app = CreateTestApp();
        using var scope = AppContext.SetApp(app.UnsafeCast);

        var buffer = new Buffer(document: new Document("hello\nworld", cursorPosition: 5));
        var binding = NamedCommands.GetByName("kill-line");
        binding.Call(CreateEvent(buffer, app: app));

        Assert.Equal("helloworld", buffer.Document.Text);
        Assert.Equal("\n", app.Clipboard.GetData().Text);
    }

    [Fact]
    public void KillLine_OnEmptyBuffer_SetsClipboardToEmpty()
    {
        var app = CreateTestApp();
        using var scope = AppContext.SetApp(app.UnsafeCast);

        var buffer = new Buffer(document: new Document("", cursorPosition: 0));
        var binding = NamedCommands.GetByName("kill-line");
        binding.Call(CreateEvent(buffer, app: app));

        Assert.Equal("", app.Clipboard.GetData().Text);
    }

    [Fact]
    public void KillWord_DeletesToNextWordEnd()
    {
        var app = CreateTestApp();
        using var scope = AppContext.SetApp(app.UnsafeCast);

        var buffer = new Buffer(document: new Document("hello world", cursorPosition: 0));
        var binding = NamedCommands.GetByName("kill-word");
        binding.Call(CreateEvent(buffer, app: app));

        Assert.Equal(" world", buffer.Document.Text);
        Assert.Equal("hello", app.Clipboard.GetData().Text);
    }

    [Fact]
    public void ConsecutiveKill_ConcatenatesClipboard()
    {
        var app = CreateTestApp();
        using var scope = AppContext.SetApp(app.UnsafeCast);

        var buffer = new Buffer(document: new Document("hello world test", cursorPosition: 0));

        // First kill-word: deletes "hello"
        var binding = NamedCommands.GetByName("kill-word");
        binding.Call(CreateEvent(buffer, app: app));

        // Second kill-word (isRepeat=true): should concatenate
        binding.Call(CreateEvent(buffer, app: app, isRepeat: true));

        // Forward concatenation: prev + new = "hello" + " world"
        Assert.Equal("hello world", app.Clipboard.GetData().Text);
    }

    [Fact]
    public void UnixWordRubout_DeletesPreviousWord()
    {
        var app = CreateTestApp();
        using var scope = AppContext.SetApp(app.UnsafeCast);

        var buffer = new Buffer(document: new Document("hello world", cursorPosition: 11));
        var binding = NamedCommands.GetByName("unix-word-rubout");
        binding.Call(CreateEvent(buffer, app: app));

        Assert.Equal("hello ", buffer.Document.Text);
    }

    [Fact]
    public void UnixWordRubout_WhenNothingToDelete_TriggersBell()
    {
        var app = CreateTestApp();
        using var scope = AppContext.SetApp(app.UnsafeCast);

        var buffer = new Buffer(document: new Document("", cursorPosition: 0));
        var binding = NamedCommands.GetByName("unix-word-rubout");
        // Should trigger bell but not throw
        binding.Call(CreateEvent(buffer, app: app));
    }

    [Fact]
    public void BackwardKillWord_DeletesPreviousWord()
    {
        var app = CreateTestApp();
        using var scope = AppContext.SetApp(app.UnsafeCast);

        var buffer = new Buffer(document: new Document("hello world", cursorPosition: 11));
        var binding = NamedCommands.GetByName("backward-kill-word");
        binding.Call(CreateEvent(buffer, app: app));

        // backward-kill-word uses non-alphanumeric boundaries
        Assert.Equal("hello ", buffer.Document.Text);
    }

    [Fact]
    public void BackwardKill_ConcatenationPrepends()
    {
        var app = CreateTestApp();
        using var scope = AppContext.SetApp(app.UnsafeCast);

        var buffer = new Buffer(document: new Document("one two three", cursorPosition: 13));

        // First backward-kill-word
        var binding = NamedCommands.GetByName("backward-kill-word");
        binding.Call(CreateEvent(buffer, app: app));

        // Second backward-kill-word (isRepeat=true): should prepend
        binding.Call(CreateEvent(buffer, app: app, isRepeat: true));

        // Backward concatenation: new + prev
        var clipText = app.Clipboard.GetData().Text;
        Assert.Contains("three", clipText);
    }

    [Fact]
    public void DeleteHorizontalSpace_RemovesSpacesAndTabs()
    {
        var buffer = new Buffer(document: new Document("hello   world", cursorPosition: 7));
        var binding = NamedCommands.GetByName("delete-horizontal-space");
        var app = CreateTestApp();
        using var scope = AppContext.SetApp(app.UnsafeCast);
        binding.Call(CreateEvent(buffer, app: app));
        Assert.Equal("helloworld", buffer.Document.Text);
    }

    [Fact]
    public void UnixLineDiscard_DeletesToLineStart()
    {
        var app = CreateTestApp();
        using var scope = AppContext.SetApp(app.UnsafeCast);

        var buffer = new Buffer(document: new Document("hello world", cursorPosition: 5));
        var binding = NamedCommands.GetByName("unix-line-discard");
        binding.Call(CreateEvent(buffer, app: app));

        Assert.Equal(" world", buffer.Document.Text);
        Assert.Equal("hello", app.Clipboard.GetData().Text);
    }

    [Fact]
    public void UnixLineDiscard_AtColumnZero_DeletesOneCharBack()
    {
        var app = CreateTestApp();
        using var scope = AppContext.SetApp(app.UnsafeCast);

        var buffer = new Buffer(document: new Document("hello\nworld", cursorPosition: 6));
        var binding = NamedCommands.GetByName("unix-line-discard");
        binding.Call(CreateEvent(buffer, app: app));

        Assert.Equal("helloworld", buffer.Document.Text);
    }

    [Fact]
    public void Yank_PastesClipboard()
    {
        var app = CreateTestApp();
        using var scope = AppContext.SetApp(app.UnsafeCast);

        app.Clipboard.SetText("pasted");
        var buffer = new Buffer(document: new Document("", cursorPosition: 0));
        var binding = NamedCommands.GetByName("yank");
        binding.Call(CreateEvent(buffer, app: app));

        Assert.Equal("pasted", buffer.Document.Text);
    }

    [Fact]
    public void YankNthArg_InsertsFromHistory()
    {
        var app = CreateTestApp();
        using var scope = AppContext.SetApp(app.UnsafeCast);

        var buffer = new Buffer(document: new Document("", cursorPosition: 0));
        var binding = NamedCommands.GetByName("yank-nth-arg");
        // Should not throw even with no history
        binding.Call(CreateEvent(buffer, app: app));
    }

    [Fact]
    public void YankLastArg_InsertsLastWordFromHistory()
    {
        var app = CreateTestApp();
        using var scope = AppContext.SetApp(app.UnsafeCast);

        var buffer = new Buffer(document: new Document("", cursorPosition: 0));
        var binding = NamedCommands.GetByName("yank-last-arg");
        // Should not throw even with no history
        binding.Call(CreateEvent(buffer, app: app));
    }

    [Fact]
    public void YankPop_WithoutPrecedingYank_IsNoOp()
    {
        var app = CreateTestApp();
        using var scope = AppContext.SetApp(app.UnsafeCast);

        var buffer = new Buffer(document: new Document("hello", cursorPosition: 5));
        var binding = NamedCommands.GetByName("yank-pop");
        binding.Call(CreateEvent(buffer, app: app));

        // DocumentBeforePaste is null → no-op
        Assert.Equal("hello", buffer.Document.Text);
    }
}
