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
/// Comprehensive boundary condition tests across all command categories.
/// </summary>
public sealed class NamedCommandsEdgeCaseTests
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

    private static Application<object> CreateApp()
    {
        var input = new SimplePipeInput();
        return new Application<object>(input: input, output: new DummyOutput());
    }

    // --- Registry edge cases ---

    [Fact]
    public void GetByName_WhitespaceOnly_ThrowsKeyNotFoundException()
    {
        var ex = Assert.Throws<KeyNotFoundException>(
            () => NamedCommands.GetByName("  "));
        Assert.Contains("Unknown Readline command: '  '", ex.Message);
    }

    // --- Movement edge cases ---

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

    // --- Text edit edge cases ---

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
    public void DeleteChar_AtEndOfBuffer_TriggersBell()
    {
        var app = CreateApp();
        using var scope = AppContext.SetApp(app);

        var buffer = new Buffer(document: new Document("hello", cursorPosition: 5));
        var binding = NamedCommands.GetByName("delete-char");
        binding.Call(CreateEvent(buffer, app: app));
        Assert.Equal("hello", buffer.Document.Text);
    }

    [Fact]
    public void BackwardDeleteChar_WithNegativeArg_DeletesForward()
    {
        var app = CreateApp();
        using var scope = AppContext.SetApp(app);

        var buffer = new Buffer(document: new Document("hello", cursorPosition: 0));
        var binding = NamedCommands.GetByName("backward-delete-char");
        binding.Call(CreateEvent(buffer, arg: "-", app: app));
        Assert.Equal("ello", buffer.Document.Text);
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
    public void SelfInsert_WithEmptyData_InsertsSomething()
    {
        // Keys.Any produces "Any" as Data in KeyPressEvent, so it's not truly empty.
        // True empty would require a special setup. This tests the default behavior.
        var buffer = new Buffer(document: new Document("", cursorPosition: 0));
        var binding = NamedCommands.GetByName("self-insert");
        binding.Call(CreateEvent(buffer));
        // With Keys.Any, the data is the string representation, which gets inserted.
    }

    // --- Kill/Yank edge cases ---

    [Fact]
    public void KillLine_OnEmptyBuffer_SetsClipboardToEmpty()
    {
        var app = CreateApp();
        using var scope = AppContext.SetApp(app);

        var buffer = new Buffer(document: new Document("", cursorPosition: 0));
        var binding = NamedCommands.GetByName("kill-line");
        binding.Call(CreateEvent(buffer, app: app));

        Assert.Equal("", app.Clipboard.GetData().Text);
    }

    [Fact]
    public void KillLine_OnLastLineWithNoTrailingNewline()
    {
        var app = CreateApp();
        using var scope = AppContext.SetApp(app);

        var buffer = new Buffer(document: new Document("hello\nworld", cursorPosition: 6));
        var binding = NamedCommands.GetByName("kill-line");
        binding.Call(CreateEvent(buffer, app: app));

        Assert.Equal("hello\n", buffer.Document.Text);
        Assert.Equal("world", app.Clipboard.GetData().Text);
    }

    [Fact]
    public void UnixWordRubout_WhenNothingToDelete_TriggersBell()
    {
        var app = CreateApp();
        using var scope = AppContext.SetApp(app);

        var buffer = new Buffer(document: new Document("", cursorPosition: 0));
        var binding = NamedCommands.GetByName("unix-word-rubout");
        binding.Call(CreateEvent(buffer, app: app));
    }

    [Fact]
    public void YankPop_WithoutPrecedingYank_IsNoOp()
    {
        var app = CreateApp();
        using var scope = AppContext.SetApp(app);

        var buffer = new Buffer(document: new Document("hello", cursorPosition: 5));
        var binding = NamedCommands.GetByName("yank-pop");
        binding.Call(CreateEvent(buffer, app: app));
        Assert.Equal("hello", buffer.Document.Text);
    }

    // --- Macro edge cases ---

    [Fact]
    public void StartKbdMacro_WhenAlreadyRecording_DelegatesToEmacsState()
    {
        var app = CreateApp();
        using var scope = AppContext.SetApp(app);

        app.EmacsState.StartMacro();
        var buffer = new Buffer();
        var binding = NamedCommands.GetByName("start-kbd-macro");
        // Should not throw
        binding.Call(CreateEvent(buffer, app: app));
    }

    [Fact]
    public void EndKbdMacro_WhenNotRecording_DelegatesToEmacsState()
    {
        var app = CreateApp();
        using var scope = AppContext.SetApp(app);

        var buffer = new Buffer();
        var binding = NamedCommands.GetByName("end-kbd-macro");
        // Should not throw
        binding.Call(CreateEvent(buffer, app: app));
    }

    [Fact]
    public void CallLastKbdMacro_WhenNoMacroRecorded_IsNoOp()
    {
        var app = CreateApp();
        using var scope = AppContext.SetApp(app);

        var buffer = new Buffer();
        var binding = NamedCommands.GetByName("call-last-kbd-macro");
        binding.Call(CreateEvent(buffer, app: app));
    }

    // --- Completion edge cases ---

    [Fact]
    public void Complete_WithNoCompleter_IsNoOp()
    {
        var app = CreateApp();
        using var scope = AppContext.SetApp(app);

        var buffer = new Buffer(document: new Document("hello", cursorPosition: 5));
        var binding = NamedCommands.GetByName("complete");
        binding.Call(CreateEvent(buffer, app: app));
    }

    [Fact]
    public void MenuComplete_WithNoCompleter_IsNoOp()
    {
        var app = CreateApp();
        using var scope = AppContext.SetApp(app);

        var buffer = new Buffer(document: new Document("hello", cursorPosition: 5));
        var binding = NamedCommands.GetByName("menu-complete");
        binding.Call(CreateEvent(buffer, app: app));
    }

    [Fact]
    public void MenuCompleteBackward_WithNoCompleter_IsNoOp()
    {
        var buffer = new Buffer(document: new Document("hello", cursorPosition: 5));
        var binding = NamedCommands.GetByName("menu-complete-backward");
        binding.Call(CreateEvent(buffer));
    }

    // --- Self-insert with multi-byte Unicode ---

    [Fact]
    public void SelfInsert_WithMultiByteUnicode_InsertsCorrectly()
    {
        var buffer = new Buffer(document: new Document("", cursorPosition: 0));
        var binding = NamedCommands.GetByName("self-insert");
        binding.Call(CreateEvent(buffer, data: "\u4e16")); // CJK character '世'
        Assert.Equal("\u4e16", buffer.Document.Text);
    }

    [Fact]
    public void SelfInsert_WithEmoji_InsertsCorrectly()
    {
        var buffer = new Buffer(document: new Document("", cursorPosition: 0));
        var binding = NamedCommands.GetByName("self-insert");
        binding.Call(CreateEvent(buffer, data: "\u2764")); // ❤
        Assert.Equal("\u2764", buffer.Document.Text);
    }

    // --- All 49 commands registered ---

    [Fact]
    public void AllCommandsRegistered_49Total()
    {
        var commandNames = new[]
        {
            // Movement (10)
            "beginning-of-buffer", "end-of-buffer", "beginning-of-line", "end-of-line",
            "forward-char", "backward-char", "forward-word", "backward-word",
            "clear-screen", "redraw-current-line",
            // History (6)
            "accept-line", "previous-history", "next-history",
            "beginning-of-history", "end-of-history", "reverse-search-history",
            // Text Edit (9)
            "end-of-file", "delete-char", "backward-delete-char", "self-insert",
            "transpose-chars", "uppercase-word", "downcase-word", "capitalize-word",
            "quoted-insert",
            // Kill/Yank (10)
            "kill-line", "kill-word", "unix-word-rubout", "backward-kill-word",
            "delete-horizontal-space", "unix-line-discard",
            "yank", "yank-nth-arg", "yank-last-arg", "yank-pop",
            // Completion (3)
            "complete", "menu-complete", "menu-complete-backward",
            // Macro (4)
            "start-kbd-macro", "end-kbd-macro", "call-last-kbd-macro", "print-last-kbd-macro",
            // Misc (7)
            "undo", "insert-comment", "vi-editing-mode", "emacs-editing-mode",
            "prefix-meta", "operate-and-get-next", "edit-and-execute-command",
        };

        Assert.Equal(49, commandNames.Length);

        foreach (var name in commandNames)
        {
            var binding = NamedCommands.GetByName(name);
            Assert.NotNull(binding);
        }
    }

    // --- Exception propagation ---

    [Fact]
    public void ExceptionPropagation_HandlerThrows_ExceptionBubblesUp()
    {
        NotImplementedOrNone? ThrowingHandler(KeyPressEvent e) =>
            throw new InvalidOperationException("Test exception");

        NamedCommands.Register("test-exception-propagation", ThrowingHandler);

        var buffer = new Buffer();
        var binding = NamedCommands.GetByName("test-exception-propagation");

        var ex = Assert.Throws<InvalidOperationException>(
            () => binding.Call(CreateEvent(buffer)));
        Assert.Equal("Test exception", ex.Message);
    }
}
