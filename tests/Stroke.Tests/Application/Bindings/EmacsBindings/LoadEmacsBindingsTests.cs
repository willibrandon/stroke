using Stroke.Core;
using Stroke.Input.Pipe;
using Stroke.KeyBinding;
using Stroke.KeyBinding.Bindings;
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
/// Tests for EmacsBindings binding registration: verifies correct key sequences,
/// named commands, and handler registrations for LoadEmacsBindings (78 bindings)
/// and LoadEmacsShiftSelectionBindings (34 bindings).
/// </summary>
public sealed class LoadEmacsBindingsTests : IDisposable
{
    private readonly SimplePipeInput _input;
    private readonly DummyOutput _output;

    public LoadEmacsBindingsTests()
    {
        _input = new SimplePipeInput();
        _output = new DummyOutput();
    }

    public void Dispose()
    {
        _input.Dispose();
    }

    #region Test Environment Setup

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

        return (buffer, app, scope);
    }

    /// <summary>
    /// Helper to get bindings for a given key sequence from the loader result.
    /// The ConditionalKeyBindings wrapper composes EmacsMode into all filters,
    /// so we look up bindings by keys and match by handler.
    /// </summary>
    private static IReadOnlyList<Binding> GetBindings(
        IKeyBindingsBase kb, params KeyOrChar[] keys)
    {
        return kb.GetBindingsForKeys(keys);
    }

    /// <summary>
    /// Find a binding whose handler matches a named command.
    /// </summary>
    private static Binding? FindNamedCommandBinding(
        IKeyBindingsBase kb, string commandName, params KeyOrChar[] keys)
    {
        var bindings = kb.GetBindingsForKeys(keys);
        var expectedHandler = NamedCommands.GetByName(commandName).Handler;
        return bindings.FirstOrDefault(b => b.Handler == expectedHandler);
    }

    #endregion

    #region Loader Return Type Tests

    [Fact]
    public void LoadEmacsBindings_ReturnsConditionalKeyBindings()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var result = EmacsBindingsType.LoadEmacsBindings();
            Assert.IsType<ConditionalKeyBindings>(result);
        }
    }

    [Fact]
    public void LoadEmacsBindings_Has78Bindings()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = EmacsBindingsType.LoadEmacsBindings();
            Assert.Equal(78, kb.Bindings.Count);
        }
    }

    [Fact]
    public void LoadEmacsShiftSelectionBindings_ReturnsConditionalKeyBindings()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var result = EmacsBindingsType.LoadEmacsShiftSelectionBindings();
            Assert.IsType<ConditionalKeyBindings>(result);
        }
    }

    #endregion

    #region US1: Movement Binding Registration

    [Theory]
    [InlineData(Keys.ControlA, "beginning-of-line")]
    [InlineData(Keys.ControlB, "backward-char")]
    [InlineData(Keys.ControlE, "end-of-line")]
    [InlineData(Keys.ControlF, "forward-char")]
    [InlineData(Keys.ControlLeft, "backward-word")]
    [InlineData(Keys.ControlRight, "forward-word")]
    [InlineData(Keys.ControlHome, "beginning-of-buffer")]
    [InlineData(Keys.ControlEnd, "end-of-buffer")]
    public void RegistersMovementBinding(Keys key, string commandName)
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = EmacsBindingsType.LoadEmacsBindings();
            var match = FindNamedCommandBinding(kb, commandName, new KeyOrChar(key));
            Assert.NotNull(match);
        }
    }

    [Theory]
    [InlineData('b', "backward-word")]
    [InlineData('f', "forward-word")]
    public void RegistersEscapeMovementBinding(char secondKey, string commandName)
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = EmacsBindingsType.LoadEmacsBindings();
            var match = FindNamedCommandBinding(
                kb, commandName,
                new KeyOrChar(Keys.Escape), new KeyOrChar(secondKey));
            Assert.NotNull(match);
        }
    }

    [Fact]
    public void RegistersCtrlN_AsInlineHandler()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = EmacsBindingsType.LoadEmacsBindings();
            var bindings = GetBindings(kb, new KeyOrChar(Keys.ControlN));
            Assert.NotEmpty(bindings);
        }
    }

    [Fact]
    public void RegistersCtrlP_AsInlineHandler()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = EmacsBindingsType.LoadEmacsBindings();
            var bindings = GetBindings(kb, new KeyOrChar(Keys.ControlP));
            Assert.NotEmpty(bindings);
        }
    }

    #endregion

    #region US1: Editing Binding Registration

    [Theory]
    [InlineData('c', "capitalize-word")]
    [InlineData('l', "downcase-word")]
    [InlineData('u', "uppercase-word")]
    public void RegistersEditingBinding(char escapeSecond, string commandName)
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = EmacsBindingsType.LoadEmacsBindings();
            var match = FindNamedCommandBinding(
                kb, commandName,
                new KeyOrChar(Keys.Escape), new KeyOrChar(escapeSecond));
            Assert.NotNull(match);
        }
    }

    [Fact]
    public void RegistersUndo_CtrlUnderscore()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = EmacsBindingsType.LoadEmacsBindings();
            var match = FindNamedCommandBinding(
                kb, "undo", new KeyOrChar(Keys.ControlUnderscore));
            Assert.NotNull(match);
        }
    }

    [Fact]
    public void RegistersUndo_CtrlXCtrlU()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = EmacsBindingsType.LoadEmacsBindings();
            var match = FindNamedCommandBinding(
                kb, "undo",
                new KeyOrChar(Keys.ControlX), new KeyOrChar(Keys.ControlU));
            Assert.NotNull(match);
        }
    }

    [Fact]
    public void RegistersCtrlDelete_KillWord()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = EmacsBindingsType.LoadEmacsBindings();
            var match = FindNamedCommandBinding(
                kb, "kill-word", new KeyOrChar(Keys.ControlDelete));
            Assert.NotNull(match);
        }
    }

    #endregion

    #region US2: Kill Ring Binding Registration

    [Theory]
    [InlineData('d', "kill-word")]
    [InlineData('y', "yank-pop")]
    [InlineData('\\', "delete-horizontal-space")]
    public void RegistersKillRingEscapeBinding(char secondKey, string commandName)
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = EmacsBindingsType.LoadEmacsBindings();
            var match = FindNamedCommandBinding(
                kb, commandName,
                new KeyOrChar(Keys.Escape), new KeyOrChar(secondKey));
            Assert.NotNull(match);
        }
    }

    [Fact]
    public void RegistersCtrlY_Yank()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = EmacsBindingsType.LoadEmacsBindings();
            var match = FindNamedCommandBinding(
                kb, "yank", new KeyOrChar(Keys.ControlY));
            Assert.NotNull(match);
        }
    }

    [Fact]
    public void RegistersCtrlXRY_Yank()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = EmacsBindingsType.LoadEmacsBindings();
            var match = FindNamedCommandBinding(
                kb, "yank",
                new KeyOrChar(Keys.ControlX), new KeyOrChar('r'), new KeyOrChar('y'));
            Assert.NotNull(match);
        }
    }

    [Fact]
    public void RegistersEscapeBackspace_BackwardKillWord()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = EmacsBindingsType.LoadEmacsBindings();
            var match = FindNamedCommandBinding(
                kb, "backward-kill-word",
                new KeyOrChar(Keys.Escape), new KeyOrChar(Keys.ControlH));
            Assert.NotNull(match);
        }
    }

    #endregion

    #region US8: History Binding Registration

    [Theory]
    [InlineData('<', "beginning-of-history")]
    [InlineData('>', "end-of-history")]
    public void RegistersHistoryBinding(char secondKey, string commandName)
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = EmacsBindingsType.LoadEmacsBindings();
            var match = FindNamedCommandBinding(
                kb, commandName,
                new KeyOrChar(Keys.Escape), new KeyOrChar(secondKey));
            Assert.NotNull(match);
        }
    }

    [Theory]
    [InlineData('.', "yank-last-arg")]
    [InlineData('_', "yank-last-arg")]
    [InlineData('#', "insert-comment")]
    public void RegistersHistoryEscapeBinding(char secondKey, string commandName)
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = EmacsBindingsType.LoadEmacsBindings();
            var match = FindNamedCommandBinding(
                kb, commandName,
                new KeyOrChar(Keys.Escape), new KeyOrChar(secondKey));
            Assert.NotNull(match);
        }
    }

    [Fact]
    public void RegistersEscapeCtrlY_YankNthArg()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = EmacsBindingsType.LoadEmacsBindings();
            var match = FindNamedCommandBinding(
                kb, "yank-nth-arg",
                new KeyOrChar(Keys.Escape), new KeyOrChar(Keys.ControlY));
            Assert.NotNull(match);
        }
    }

    [Fact]
    public void RegistersCtrlO_OperateAndGetNext()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = EmacsBindingsType.LoadEmacsBindings();
            var match = FindNamedCommandBinding(
                kb, "operate-and-get-next", new KeyOrChar(Keys.ControlO));
            Assert.NotNull(match);
        }
    }

    #endregion

    #region US8: Accept-Line Registration

    [Fact]
    public void RegistersEnter_AcceptLine()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = EmacsBindingsType.LoadEmacsBindings();
            var match = FindNamedCommandBinding(
                kb, "accept-line", new KeyOrChar(Keys.ControlM));
            Assert.NotNull(match);
        }
    }

    [Fact]
    public void RegistersEscapeEnter_AcceptLine()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = EmacsBindingsType.LoadEmacsBindings();
            var match = FindNamedCommandBinding(
                kb, "accept-line",
                new KeyOrChar(Keys.Escape), new KeyOrChar(Keys.ControlM));
            Assert.NotNull(match);
        }
    }

    #endregion

    #region US3: Selection Binding Registration

    [Fact]
    public void RegistersCtrlAt_StartSelection()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = EmacsBindingsType.LoadEmacsBindings();
            var bindings = GetBindings(kb, new KeyOrChar(Keys.ControlAt));
            Assert.NotEmpty(bindings);
        }
    }

    [Fact]
    public void RegistersCtrlG_BothWithAndWithoutSelection()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = EmacsBindingsType.LoadEmacsBindings();
            var bindings = GetBindings(kb, new KeyOrChar(Keys.ControlG));
            Assert.True(bindings.Count >= 2, "Ctrl-G should have at least 2 bindings (with/without selection)");
        }
    }

    [Fact]
    public void RegistersCtrlW_CutSelection()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = EmacsBindingsType.LoadEmacsBindings();
            var bindings = GetBindings(kb, new KeyOrChar(Keys.ControlW));
            Assert.NotEmpty(bindings);
        }
    }

    [Fact]
    public void RegistersEscapeW_CopySelection()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = EmacsBindingsType.LoadEmacsBindings();
            var bindings = GetBindings(
                kb, new KeyOrChar(Keys.Escape), new KeyOrChar('w'));
            Assert.NotEmpty(bindings);
        }
    }

    [Fact]
    public void RegistersCtrlXRK_CutSelection()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = EmacsBindingsType.LoadEmacsBindings();
            var bindings = GetBindings(
                kb,
                new KeyOrChar(Keys.ControlX), new KeyOrChar('r'), new KeyOrChar('k'));
            Assert.NotEmpty(bindings);
        }
    }

    #endregion

    #region US6: Numeric Argument Registration

    [Theory]
    [InlineData('0')]
    [InlineData('5')]
    [InlineData('9')]
    public void RegistersEscapeDigit(char digit)
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = EmacsBindingsType.LoadEmacsBindings();
            var bindings = GetBindings(
                kb, new KeyOrChar(Keys.Escape), new KeyOrChar(digit));
            Assert.NotEmpty(bindings);
        }
    }

    [Fact]
    public void RegistersEscapeDash_MetaDash()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = EmacsBindingsType.LoadEmacsBindings();
            var bindings = GetBindings(
                kb, new KeyOrChar(Keys.Escape), new KeyOrChar('-'));
            Assert.NotEmpty(bindings);
        }
    }

    #endregion

    #region US6: Character Search Registration

    [Fact]
    public void RegistersCtrlSquareBracket_Any_GotoChar()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = EmacsBindingsType.LoadEmacsBindings();
            var bindings = GetBindings(
                kb, new KeyOrChar(Keys.ControlSquareClose), new KeyOrChar(Keys.Any));
            Assert.NotEmpty(bindings);
        }
    }

    [Fact]
    public void RegistersEscapeCtrlSquareBracket_Any_GotoCharBackwards()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = EmacsBindingsType.LoadEmacsBindings();
            var bindings = GetBindings(
                kb,
                new KeyOrChar(Keys.Escape),
                new KeyOrChar(Keys.ControlSquareClose),
                new KeyOrChar(Keys.Any));
            Assert.NotEmpty(bindings);
        }
    }

    #endregion

    #region US7: Macro Registration

    [Fact]
    public void RegistersCtrlXOpenParen_StartKbdMacro()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = EmacsBindingsType.LoadEmacsBindings();
            var match = FindNamedCommandBinding(
                kb, "start-kbd-macro",
                new KeyOrChar(Keys.ControlX), new KeyOrChar('('));
            Assert.NotNull(match);
        }
    }

    [Fact]
    public void RegistersCtrlXCloseParen_EndKbdMacro()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = EmacsBindingsType.LoadEmacsBindings();
            var match = FindNamedCommandBinding(
                kb, "end-kbd-macro",
                new KeyOrChar(Keys.ControlX), new KeyOrChar(')'));
            Assert.NotNull(match);
        }
    }

    [Fact]
    public void RegistersCtrlXE_CallLastKbdMacro()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = EmacsBindingsType.LoadEmacsBindings();
            var match = FindNamedCommandBinding(
                kb, "call-last-kbd-macro",
                new KeyOrChar(Keys.ControlX), new KeyOrChar('e'));
            Assert.NotNull(match);
        }
    }

    #endregion

    #region US9: Miscellaneous Registration

    [Fact]
    public void RegistersCtrlQ_QuotedInsert()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = EmacsBindingsType.LoadEmacsBindings();
            var match = FindNamedCommandBinding(
                kb, "quoted-insert", new KeyOrChar(Keys.ControlQ));
            Assert.NotNull(match);
        }
    }

    [Fact]
    public void RegistersCtrlXCtrlX_ToggleStartEnd()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = EmacsBindingsType.LoadEmacsBindings();
            var bindings = GetBindings(
                kb, new KeyOrChar(Keys.ControlX), new KeyOrChar(Keys.ControlX));
            Assert.NotEmpty(bindings);
        }
    }

    [Fact]
    public void RegistersEscapeSlash_Complete()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = EmacsBindingsType.LoadEmacsBindings();
            var bindings = GetBindings(
                kb, new KeyOrChar(Keys.Escape), new KeyOrChar('/'));
            Assert.NotEmpty(bindings);
        }
    }

    [Fact]
    public void RegistersEscapeAsterisk_InsertAllCompletions()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = EmacsBindingsType.LoadEmacsBindings();
            var bindings = GetBindings(
                kb, new KeyOrChar(Keys.Escape), new KeyOrChar('*'));
            Assert.NotEmpty(bindings);
        }
    }

    [Theory]
    [InlineData('a')]
    [InlineData('e')]
    public void RegistersPlaceholderBinding(char secondKey)
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = EmacsBindingsType.LoadEmacsBindings();
            var bindings = GetBindings(
                kb, new KeyOrChar(Keys.Escape), new KeyOrChar(secondKey));
            Assert.NotEmpty(bindings);
        }
    }

    [Fact]
    public void RegistersEscapeT_SwapCharacters()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = EmacsBindingsType.LoadEmacsBindings();
            var bindings = GetBindings(
                kb, new KeyOrChar(Keys.Escape), new KeyOrChar('t'));
            Assert.NotEmpty(bindings);
        }
    }

    [Fact]
    public void RegistersEscapeLeft_StartOfWord()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = EmacsBindingsType.LoadEmacsBindings();
            var bindings = GetBindings(
                kb, new KeyOrChar(Keys.Escape), new KeyOrChar(Keys.Left));
            Assert.NotEmpty(bindings);
        }
    }

    [Fact]
    public void RegistersEscapeRight_StartNextWord()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = EmacsBindingsType.LoadEmacsBindings();
            var bindings = GetBindings(
                kb, new KeyOrChar(Keys.Escape), new KeyOrChar(Keys.Right));
            Assert.NotEmpty(bindings);
        }
    }

    [Fact]
    public void RegistersCtrlCGreaterThan_IndentSelection()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = EmacsBindingsType.LoadEmacsBindings();
            var bindings = GetBindings(
                kb, new KeyOrChar(Keys.ControlC), new KeyOrChar('>'));
            Assert.NotEmpty(bindings);
        }
    }

    [Fact]
    public void RegistersCtrlCLessThan_UnindentSelection()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = EmacsBindingsType.LoadEmacsBindings();
            var bindings = GetBindings(
                kb, new KeyOrChar(Keys.ControlC), new KeyOrChar('<'));
            Assert.NotEmpty(bindings);
        }
    }

    #endregion

    #region Escape Binding Order Test

    [Fact]
    public void EscapeIsFirstBinding()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = EmacsBindingsType.LoadEmacsBindings();
            var allBindings = kb.Bindings;

            // First binding should be the Escape no-op
            var firstBinding = allBindings[0];
            Assert.Equal(new KeyOrChar(Keys.Escape), firstBinding.Keys[0]);
            Assert.Single(firstBinding.Keys);
        }
    }

    #endregion
}
