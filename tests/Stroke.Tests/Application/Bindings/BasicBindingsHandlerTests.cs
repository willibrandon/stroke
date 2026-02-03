using Stroke.Application.Bindings;
using Stroke.Core;
using Stroke.Filters;
using Stroke.Input.Pipe;
using Stroke.KeyBinding;
using Stroke.Layout;
using Stroke.Layout.Containers;
using Stroke.Layout.Controls;
using Stroke.Output;
using Xunit;
using AppContext = Stroke.Application.AppContext;
using Buffer = Stroke.Core.Buffer;
using Keys = Stroke.Input.Keys;

namespace Stroke.Tests.Application.Bindings;

/// <summary>
/// Tests for BasicBindings inline handler behaviors: Enter multiline (US4),
/// Ctrl+J re-dispatch (US4), auto up/down (US4), delete selection (US7),
/// Ctrl+Z literal insert (US5), bracketed paste (US5), and quoted insert (US6).
/// </summary>
public sealed class BasicBindingsHandlerTests : IDisposable
{
    private readonly SimplePipeInput _input;
    private readonly DummyOutput _output;
    private readonly KeyBindings _kb;

    public BasicBindingsHandlerTests()
    {
        _input = new SimplePipeInput();
        _output = new DummyOutput();
        _kb = BasicBindings.LoadBasicBindings();
    }

    public void Dispose()
    {
        _input.Dispose();
    }

    #region Test Environment Setup

    /// <summary>
    /// Creates a test environment with a buffer, layout, and application.
    /// </summary>
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
        var scope = AppContext.SetApp(app.UnsafeCast);

        return (buffer, app, scope);
    }

    /// <summary>
    /// Creates a KeyPressEvent with the given parameters.
    /// </summary>
    private static KeyPressEvent CreateEvent(
        Buffer? buffer = null,
        object? app = null,
        Keys key = Keys.Any,
        string? data = null,
        bool isRepeat = false,
        string? arg = null,
        object? keyProcessor = null)
    {
        var keyPress = data != null
            ? new Stroke.KeyBinding.KeyPress(key, data)
            : new Stroke.KeyBinding.KeyPress(key);

        return new KeyPressEvent(
            keyProcessorRef: keyProcessor != null
                ? new WeakReference<object>(keyProcessor)
                : null,
            arg: arg,
            keySequence: [keyPress],
            previousKeySequence: [],
            isRepeat: isRepeat,
            app: app,
            currentBuffer: buffer);
    }

    /// <summary>
    /// Finds an inline handler binding (non-named-command) for a key with the given filter type.
    /// </summary>
    private Binding? FindInlineHandlerBinding(Keys key, bool hasFilter = false, bool eager = false)
    {
        var bindings = _kb.GetBindingsForKeys([new KeyOrChar(key)]);
        var ignoreHandler = _kb.Bindings[0].Handler;

        // Use LastOrDefault to get the most specific binding (fewest wildcards).
        // Bindings are sorted descending by AnyCount, so the last match is most specific.
        return bindings.LastOrDefault(b =>
            b.Handler != ignoreHandler &&
            !IsNamedCommandHandler(b) &&
            (hasFilter ? b.Filter is not Always : b.Filter is Always) &&
            (eager ? b.Eager is not Never : true));
    }

    private static bool IsNamedCommandHandler(Binding binding)
    {
        // Named command handlers are extracted from existing Binding instances
        // Check if the handler matches any known named command
        var commandNames = new[]
        {
            "beginning-of-line", "end-of-line", "backward-char", "forward-char",
            "previous-history", "next-history", "clear-screen",
            "kill-line", "unix-line-discard", "backward-delete-char",
            "delete-char", "transpose-chars", "unix-word-rubout",
            "self-insert", "menu-complete", "menu-complete-backward"
        };

        foreach (var name in commandNames)
        {
            var namedBinding = Stroke.KeyBinding.Bindings.NamedCommands.GetByName(name);
            if (binding.Handler == namedBinding.Handler)
                return true;
        }
        return false;
    }

    #endregion

    #region US4: Enter Multiline Handler (T016)

    [Fact]
    public void EnterMultiline_InsertsNewline()
    {
        var (buffer, app, scope) = CreateEnvironment("hello", 5, multiline: true);
        using (scope)
        {
            app.EditingMode = Stroke.KeyBinding.EditingMode.Emacs;
            var @event = CreateEvent(buffer: buffer, app: app, key: Keys.ControlM, data: "\r");

            // Find the Enter binding
            var binding = FindEnterBinding();
            Assert.NotNull(binding);

            binding.Handler(@event);
            Assert.Contains("\n", buffer.Text);
        }
    }

    [Fact]
    public void EnterMultiline_HasInsertModeAndIsMultilineFilter()
    {
        var binding = FindEnterBinding();
        Assert.NotNull(binding);
        // The filter should not be Always (has InsertMode & IsMultiline)
        Assert.IsNotType<Always>(binding.Filter);
    }

    /// <summary>
    /// Finds the Enter (ControlM) multiline handler binding.
    /// It's the one with a filter (InsertMode & IsMultiline) that is an inline handler.
    /// </summary>
    private Binding? FindEnterBinding()
    {
        var bindings = _kb.GetBindingsForKeys([new KeyOrChar(Keys.ControlM)]);
        var ignoreHandler = _kb.Bindings[0].Handler;

        // Use LastOrDefault to get the most specific binding (fewest wildcards).
        return bindings.LastOrDefault(b =>
            b.Handler != ignoreHandler &&
            !IsNamedCommandHandler(b) &&
            b.Filter is not Always);
    }

    #endregion

    #region US4: Up/Down Auto-Navigation Handlers (T017)

    [Fact]
    public void AutoUp_CallsBufferAutoUp()
    {
        var (buffer, app, scope) = CreateEnvironment("line1\nline2", 11, multiline: true);
        using (scope)
        {
            var @event = CreateEvent(buffer: buffer, app: app, key: Keys.Up);

            // Find the Up inline handler (no filter, not the ignore handler)
            var binding = FindAutoBinding(Keys.Up);
            Assert.NotNull(binding);

            binding.Handler(@event);
            // Cursor should have moved up (to line 1)
            Assert.True(buffer.Document.CursorPositionRow == 0);
        }
    }

    [Fact]
    public void AutoDown_CallsBufferAutoDown()
    {
        var (buffer, app, scope) = CreateEnvironment("line1\nline2", 0, multiline: true);
        using (scope)
        {
            var @event = CreateEvent(buffer: buffer, app: app, key: Keys.Down);

            var binding = FindAutoBinding(Keys.Down);
            Assert.NotNull(binding);

            binding.Handler(@event);
            // Cursor should have moved down (to line 2)
            Assert.True(buffer.Document.CursorPositionRow == 1);
        }
    }

    [Fact]
    public void AutoUp_SingleLineNoHistory_BufferUnchanged()
    {
        var (buffer, app, scope) = CreateEnvironment("hello", 5);
        using (scope)
        {
            var @event = CreateEvent(buffer: buffer, app: app, key: Keys.Up);
            var binding = FindAutoBinding(Keys.Up);
            Assert.NotNull(binding);

            var initialText = buffer.Text;
            binding.Handler(@event);
            // Buffer text remains unchanged
            Assert.Equal(initialText, buffer.Text);
        }
    }

    [Fact]
    public void AutoUp_HasNoFilter()
    {
        var binding = FindAutoBinding(Keys.Up);
        Assert.NotNull(binding);
        Assert.IsType<Always>(binding.Filter);
    }

    [Fact]
    public void AutoDown_HasNoFilter()
    {
        var binding = FindAutoBinding(Keys.Down);
        Assert.NotNull(binding);
        Assert.IsType<Always>(binding.Filter);
    }

    /// <summary>
    /// Finds the auto up/down inline handler (no filter, not the ignore handler).
    /// </summary>
    private Binding? FindAutoBinding(Keys key)
    {
        var bindings = _kb.GetBindingsForKeys([new KeyOrChar(key)]);
        var ignoreHandler = _kb.Bindings[0].Handler;

        // Use LastOrDefault to get the most specific binding (fewest wildcards).
        return bindings.LastOrDefault(b =>
            b.Handler != ignoreHandler &&
            !IsNamedCommandHandler(b) &&
            b.Filter is Always);
    }

    #endregion

    #region US4: Ctrl+J Re-dispatch Handler (T018)

    [Fact]
    public void CtrlJ_HasBinding()
    {
        var bindings = _kb.GetBindingsForKeys([new KeyOrChar(Keys.ControlJ)]);
        var ignoreHandler = _kb.Bindings[0].Handler;
        // Use LastOrDefault to get the most specific binding (fewest wildcards).
        var inlineBinding = bindings.LastOrDefault(b =>
            b.Handler != ignoreHandler && !IsNamedCommandHandler(b));
        Assert.NotNull(inlineBinding);
    }

    [Fact]
    public void CtrlJ_HasNoFilter()
    {
        var bindings = _kb.GetBindingsForKeys([new KeyOrChar(Keys.ControlJ)]);
        var ignoreHandler = _kb.Bindings[0].Handler;
        // Use LastOrDefault to get the most specific binding (fewest wildcards).
        var inlineBinding = bindings.LastOrDefault(b =>
            b.Handler != ignoreHandler && !IsNamedCommandHandler(b));
        Assert.NotNull(inlineBinding);
        Assert.IsType<Always>(inlineBinding.Filter);
    }

    [Fact]
    public void CtrlJ_ReDispatchesAsControlM()
    {
        var (buffer, app, scope) = CreateEnvironment("hello");
        using (scope)
        {
            // Create a KeyProcessor to capture Feed calls
            var kp = app.KeyProcessor;
            var @event = CreateEvent(
                buffer: buffer,
                app: app,
                key: Keys.ControlJ,
                data: "\n",
                keyProcessor: kp);

            var bindings = _kb.GetBindingsForKeys([new KeyOrChar(Keys.ControlJ)]);
            var ignoreHandler = _kb.Bindings[0].Handler;
            // Use LastOrDefault to get the most specific binding (fewest wildcards).
            var binding = bindings.LastOrDefault(b =>
                b.Handler != ignoreHandler && !IsNamedCommandHandler(b));
            Assert.NotNull(binding);

            // Calling the handler should feed ControlM to the key processor
            // We verify it doesn't throw (the Feed call is real)
            binding.Handler(@event);
        }
    }

    #endregion

    #region US5: Bracketed Paste Handler (T021)

    [Fact]
    public void BracketedPaste_NormalizesWindowsLineEndings()
    {
        var (buffer, app, scope) = CreateEnvironment();
        using (scope)
        {
            var @event = CreateEvent(
                buffer: buffer,
                app: app,
                key: Keys.BracketedPaste,
                data: "hello\r\nworld");

            var binding = FindPasteBinding();
            Assert.NotNull(binding);

            binding.Handler(@event);
            Assert.Equal("hello\nworld", buffer.Text);
        }
    }

    [Fact]
    public void BracketedPaste_NormalizesOldMacLineEndings()
    {
        var (buffer, app, scope) = CreateEnvironment();
        using (scope)
        {
            var @event = CreateEvent(
                buffer: buffer,
                app: app,
                key: Keys.BracketedPaste,
                data: "line1\rline2");

            var binding = FindPasteBinding();
            Assert.NotNull(binding);

            binding.Handler(@event);
            Assert.Equal("line1\nline2", buffer.Text);
        }
    }

    [Fact]
    public void BracketedPaste_PreservesUnixLineEndings()
    {
        var (buffer, app, scope) = CreateEnvironment();
        using (scope)
        {
            var @event = CreateEvent(
                buffer: buffer,
                app: app,
                key: Keys.BracketedPaste,
                data: "hello\nworld");

            var binding = FindPasteBinding();
            Assert.NotNull(binding);

            binding.Handler(@event);
            Assert.Equal("hello\nworld", buffer.Text);
        }
    }

    [Fact]
    public void BracketedPaste_EmptyString_IsNoOp()
    {
        var (buffer, app, scope) = CreateEnvironment("existing");
        using (scope)
        {
            var @event = CreateEvent(
                buffer: buffer,
                app: app,
                key: Keys.BracketedPaste,
                data: "");

            var binding = FindPasteBinding();
            Assert.NotNull(binding);

            binding.Handler(@event);
            Assert.Equal("existing", buffer.Text);
        }
    }

    [Fact]
    public void BracketedPaste_PureCRLF_NormalizesToLF()
    {
        var (buffer, app, scope) = CreateEnvironment();
        using (scope)
        {
            var @event = CreateEvent(
                buffer: buffer,
                app: app,
                key: Keys.BracketedPaste,
                data: "\r\n\r\n");

            var binding = FindPasteBinding();
            Assert.NotNull(binding);

            binding.Handler(@event);
            Assert.Equal("\n\n", buffer.Text);
        }
    }

    [Fact]
    public void BracketedPaste_HasNoFilter()
    {
        var binding = FindPasteBinding();
        Assert.NotNull(binding);
        Assert.IsType<Always>(binding.Filter);
    }

    private Binding? FindPasteBinding()
    {
        var bindings = _kb.GetBindingsForKeys([new KeyOrChar(Keys.BracketedPaste)]);
        // Use LastOrDefault to get the most specific binding (fewest wildcards).
        return bindings.LastOrDefault();
    }

    #endregion

    #region US5: Ctrl+Z Literal Insert Handler (T022)

    [Fact]
    public void CtrlZ_InsertsLiteralData()
    {
        var (buffer, app, scope) = CreateEnvironment();
        using (scope)
        {
            var ctrlZData = "\x1a"; // ASCII 26 = Ctrl+Z
            var @event = CreateEvent(
                buffer: buffer,
                app: app,
                key: Keys.ControlZ,
                data: ctrlZData);

            var binding = FindCtrlZInlineBinding();
            Assert.NotNull(binding);

            binding.Handler(@event);
            Assert.Equal(ctrlZData, buffer.Text);
        }
    }

    [Fact]
    public void CtrlZ_InsertsASCII26ControlCharacter()
    {
        var (buffer, app, scope) = CreateEnvironment();
        using (scope)
        {
            var ctrlZData = "\x1a";
            var @event = CreateEvent(
                buffer: buffer,
                app: app,
                key: Keys.ControlZ,
                data: ctrlZData);

            var binding = FindCtrlZInlineBinding();
            Assert.NotNull(binding);

            binding.Handler(@event);
            Assert.Equal(1, buffer.Text.Length);
            Assert.Equal(26, (int)buffer.Text[0]);
        }
    }

    [Fact]
    public void CtrlZ_HasNoFilter()
    {
        var binding = FindCtrlZInlineBinding();
        Assert.NotNull(binding);
        Assert.IsType<Always>(binding.Filter);
    }

    private Binding? FindCtrlZInlineBinding()
    {
        var bindings = _kb.GetBindingsForKeys([new KeyOrChar(Keys.ControlZ)]);
        var ignoreHandler = _kb.Bindings[0].Handler;
        // Use LastOrDefault to get the most specific binding (fewest wildcards).
        return bindings.LastOrDefault(b =>
            b.Handler != ignoreHandler && !IsNamedCommandHandler(b));
    }

    #endregion

    #region US6: Quoted Insert Handler (T024)

    [Fact]
    public void QuotedInsert_HasEagerBinding()
    {
        var allBindings = _kb.Bindings;
        // Last binding should be quoted insert with eager=true
        var lastBinding = allBindings[^1];
        Assert.Equal(new KeyOrChar(Keys.Any), lastBinding.Keys[0]);
        Assert.IsNotType<Never>(lastBinding.Eager);
    }

    [Fact]
    public void QuotedInsert_HasInQuotedInsertFilter()
    {
        var allBindings = _kb.Bindings;
        var lastBinding = allBindings[^1];
        Assert.IsNotType<Always>(lastBinding.Filter);
    }

    [Fact]
    public void QuotedInsert_InsertsLiteralCharacter()
    {
        var (buffer, app, scope) = CreateEnvironment();
        using (scope)
        {
            app.QuotedInsert = true;
            var @event = CreateEvent(
                buffer: buffer,
                app: app,
                key: Keys.Any,
                data: "a");

            var lastBinding = _kb.Bindings[^1];
            lastBinding.Handler(@event);

            Assert.Equal("a", buffer.Text);
        }
    }

    [Fact]
    public void QuotedInsert_DeactivatesQuotedInsertMode()
    {
        var (buffer, app, scope) = CreateEnvironment();
        using (scope)
        {
            app.QuotedInsert = true;
            var @event = CreateEvent(
                buffer: buffer,
                app: app,
                key: Keys.Any,
                data: "x");

            var lastBinding = _kb.Bindings[^1];
            lastBinding.Handler(@event);

            Assert.False(app.QuotedInsert);
        }
    }

    [Fact]
    public void QuotedInsert_InsertsWithoutOverwrite()
    {
        var (buffer, app, scope) = CreateEnvironment("hello", 0);
        using (scope)
        {
            app.QuotedInsert = true;
            var @event = CreateEvent(
                buffer: buffer,
                app: app,
                key: Keys.Any,
                data: "x");

            var lastBinding = _kb.Bindings[^1];
            lastBinding.Handler(@event);

            // Should insert at position 0 without overwriting
            Assert.Equal("xhello", buffer.Text);
        }
    }

    #endregion

    #region US7: Delete Selection Handler (T027)

    [Fact]
    public void DeleteSelection_HasSelectionFilter()
    {
        var bindings = _kb.GetBindingsForKeys([new KeyOrChar(Keys.Delete)]);
        var ignoreHandler = _kb.Bindings[0].Handler;

        // Find the inline handler binding with HasSelection filter
        // Use LastOrDefault to get the most specific binding (fewest wildcards).
        var selectionBinding = bindings.LastOrDefault(b =>
            b.Handler != ignoreHandler &&
            !IsNamedCommandHandler(b) &&
            b.Filter is not Always);

        Assert.NotNull(selectionBinding);
    }

    [Fact]
    public void DeleteSelection_CutsSelectionAndSetsClipboard()
    {
        var (buffer, app, scope) = CreateEnvironment("hello world", 0);
        using (scope)
        {
            // Select "hello"
            buffer.StartSelection();
            buffer.CursorPosition = 5;

            var @event = CreateEvent(buffer: buffer, app: app, key: Keys.Delete);

            var bindings = _kb.GetBindingsForKeys([new KeyOrChar(Keys.Delete)]);
            var ignoreHandler = _kb.Bindings[0].Handler;
            // Use LastOrDefault to get the most specific binding (fewest wildcards).
            var selectionBinding = bindings.LastOrDefault(b =>
                b.Handler != ignoreHandler &&
                !IsNamedCommandHandler(b) &&
                b.Filter is not Always);
            Assert.NotNull(selectionBinding);

            selectionBinding.Handler(@event);

            // The selected text should be cut
            Assert.Equal(" world", buffer.Text);
            // Clipboard should have the data
            var clipData = app.Clipboard.GetData();
            Assert.Equal("hello", clipData.Text);
        }
    }

    #endregion

    #region US7: Ctrl+D Binding (T028 - behavior test)

    [Fact]
    public void CtrlD_DeletesCharacterWhenTextExists()
    {
        var (buffer, app, scope) = CreateEnvironment("hello", 0);
        using (scope)
        {
            app.EditingMode = Stroke.KeyBinding.EditingMode.Emacs;
            // Find the Ctrl+D → delete-char named command binding
            var bindings = _kb.GetBindingsForKeys([new KeyOrChar(Keys.ControlD)]);
            var ignoreHandler = _kb.Bindings[0].Handler;
            // Use LastOrDefault to get the most specific binding (fewest wildcards).
            var deleteCharBinding = bindings.LastOrDefault(b =>
                b.Handler != ignoreHandler &&
                IsNamedCommandHandler(b) &&
                b.Filter is not Always);
            Assert.NotNull(deleteCharBinding);

            var @event = CreateEvent(buffer: buffer, app: app, key: Keys.ControlD);
            deleteCharBinding.Handler(@event);

            Assert.Equal("ello", buffer.Text);
        }
    }

    #endregion
}
