using Stroke.Clipboard;
using Stroke.Filters;
using Stroke.History;
using Stroke.KeyBinding;
using Stroke.Shortcuts;
using Xunit;

namespace Stroke.Tests.Shortcuts;

/// <summary>
/// Tests for the <see cref="PromptSession{TResult}"/> constructor, property defaults,
/// DynCond resolution, FilterOrBool HasValue defaults, and exception type validation.
/// </summary>
public sealed class PromptSessionTests
{
    #region Constructor Default Tests

    [Fact]
    public void Constructor_Default_CreatesSessionSuccessfully()
    {
        var session = new PromptSession<string>();

        Assert.NotNull(session);
    }

    [Fact]
    public void Constructor_Default_HistoryIsInMemoryHistory()
    {
        var session = new PromptSession<string>();

        Assert.IsType<InMemoryHistory>(session.History);
    }

    [Fact]
    public void Constructor_Default_ClipboardIsInMemoryClipboard()
    {
        var session = new PromptSession<string>();

        Assert.IsType<InMemoryClipboard>(session.Clipboard);
    }

    [Fact]
    public void Constructor_Default_DefaultBufferCreated()
    {
        var session = new PromptSession<string>();

        Assert.NotNull(session.DefaultBuffer);
        Assert.Equal("DEFAULT_BUFFER", session.DefaultBuffer.Name);
    }

    [Fact]
    public void Constructor_Default_SearchBufferCreated()
    {
        var session = new PromptSession<string>();

        Assert.NotNull(session.SearchBuffer);
        Assert.Equal("SEARCH_BUFFER", session.SearchBuffer.Name);
    }

    [Fact]
    public void Constructor_Default_LayoutCreated()
    {
        var session = new PromptSession<string>();

        Assert.NotNull(session.Layout);
    }

    [Fact]
    public void Constructor_Default_AppCreated()
    {
        var session = new PromptSession<string>();

        Assert.NotNull(session.App);
    }

    [Fact]
    public void Constructor_Default_EditingModeIsEmacs()
    {
        var session = new PromptSession<string>();

        Assert.Equal(EditingMode.Emacs, session.EditingMode);
    }

    #endregion

    #region FilterOrBool HasValue Sentinel Tests

    [Fact]
    public void Constructor_Default_WrapLinesDefaultsToTrue()
    {
        var session = new PromptSession<string>();

        Assert.True(session.WrapLines.HasValue);
        Assert.True(session.WrapLines.IsBool);
        Assert.True(session.WrapLines.BoolValue);
    }

    [Fact]
    public void Constructor_Default_CompleteWhileTypingDefaultsToTrue()
    {
        var session = new PromptSession<string>();

        Assert.True(session.CompleteWhileTyping.HasValue);
        Assert.True(session.CompleteWhileTyping.IsBool);
        Assert.True(session.CompleteWhileTyping.BoolValue);
    }

    [Fact]
    public void Constructor_Default_ValidateWhileTypingDefaultsToTrue()
    {
        var session = new PromptSession<string>();

        Assert.True(session.ValidateWhileTyping.HasValue);
        Assert.True(session.ValidateWhileTyping.IsBool);
        Assert.True(session.ValidateWhileTyping.BoolValue);
    }

    [Fact]
    public void Constructor_Default_IncludeDefaultPygmentsStyleDefaultsToTrue()
    {
        var session = new PromptSession<string>();

        Assert.True(session.IncludeDefaultPygmentsStyle.HasValue);
        Assert.True(session.IncludeDefaultPygmentsStyle.IsBool);
        Assert.True(session.IncludeDefaultPygmentsStyle.BoolValue);
    }

    [Fact]
    public void Constructor_ExplicitFalse_WrapLinesRespected()
    {
        var session = new PromptSession<string>(wrapLines: false);

        Assert.True(session.WrapLines.HasValue);
        Assert.True(session.WrapLines.IsBool);
        Assert.False(session.WrapLines.BoolValue);
    }

    [Fact]
    public void Constructor_Default_MultilineDefaultsToFalsy()
    {
        var session = new PromptSession<string>();

        // Default FilterOrBool for multiline should still have value (false)
        // or be falsy
        Assert.False(FilterUtils.ToFilter(session.Multiline).Invoke());
    }

    [Fact]
    public void Constructor_Default_IsPasswordDefaultsToFalsy()
    {
        var session = new PromptSession<string>();

        Assert.False(FilterUtils.ToFilter(session.IsPassword).Invoke());
    }

    #endregion

    #region viMode Precedence Tests (Edge Case 1)

    [Fact]
    public void Constructor_ViMode_SetsEditingModeToVi()
    {
        var session = new PromptSession<string>(viMode: true);

        Assert.Equal(EditingMode.Vi, session.EditingMode);
    }

    [Fact]
    public void Constructor_ViMode_OverridesExplicitEmacsMode()
    {
        var session = new PromptSession<string>(
            viMode: true,
            editingMode: EditingMode.Emacs);

        Assert.Equal(EditingMode.Vi, session.EditingMode);
    }

    [Fact]
    public void Constructor_NoViMode_ExplicitEditingModeRespected()
    {
        var session = new PromptSession<string>(editingMode: EditingMode.Vi);

        Assert.Equal(EditingMode.Vi, session.EditingMode);
    }

    #endregion

    #region Exception Type Validation (FR-037)

    [Fact]
    public void Constructor_Default_InterruptExceptionIsKeyboardInterruptException()
    {
        var session = new PromptSession<string>();

        Assert.Equal(typeof(KeyboardInterruptException), session.InterruptException);
    }

    [Fact]
    public void Constructor_Default_EofExceptionIsEOFException()
    {
        var session = new PromptSession<string>();

        Assert.Equal(typeof(EOFException), session.EofException);
    }

    [Fact]
    public void Constructor_CustomInterruptException_IsAccepted()
    {
        var session = new PromptSession<string>(
            interruptException: typeof(OperationCanceledException));

        Assert.Equal(typeof(OperationCanceledException), session.InterruptException);
    }

    [Fact]
    public void Constructor_AbstractExceptionType_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            new PromptSession<string>(
                interruptException: typeof(AbstractTestException)));
    }

    [Fact]
    public void Constructor_NonExceptionType_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            new PromptSession<string>(
                interruptException: typeof(string)));
    }

    #endregion

    #region Test Helpers

    private abstract class AbstractTestException : Exception
    {
        protected AbstractTestException() { }
    }

    #endregion

    #region Mutable Property Get/Set Tests

    [Fact]
    public void Message_SetAndGet_RoundTrips()
    {
        var session = new PromptSession<string>();

        session.Message = "new prompt> ";
        Assert.Equal("new prompt> ", (string?)session.Message.Value);
    }

    [Fact]
    public void Completer_Default_IsNull()
    {
        var session = new PromptSession<string>();

        Assert.Null(session.Completer);
    }

    [Fact]
    public void Validator_SetAndGet_RoundTrips()
    {
        var session = new PromptSession<string>();

        Assert.Null(session.Validator);
    }

    [Fact]
    public void CompleteStyle_Default_IsColumn()
    {
        var session = new PromptSession<string>();

        Assert.Equal(CompleteStyle.Column, session.CompleteStyle);
    }

    [Fact]
    public void CompleteStyle_SetAndGet_RoundTrips()
    {
        var session = new PromptSession<string>();

        session.CompleteStyle = CompleteStyle.MultiColumn;
        Assert.Equal(CompleteStyle.MultiColumn, session.CompleteStyle);
    }

    [Fact]
    public void ReserveSpaceForMenu_Default_Is8()
    {
        var session = new PromptSession<string>();

        Assert.Equal(8, session.ReserveSpaceForMenu);
    }

    [Fact]
    public void RefreshInterval_Default_IsZero()
    {
        var session = new PromptSession<string>();

        Assert.Equal(0.0, session.RefreshInterval);
    }

    [Fact]
    public void CompleteInThread_Default_IsFalse()
    {
        var session = new PromptSession<string>();

        Assert.False(session.CompleteInThread);
    }

    [Fact]
    public void Lexer_Default_IsNull()
    {
        var session = new PromptSession<string>();

        Assert.Null(session.Lexer);
    }

    [Fact]
    public void AutoSuggest_Default_IsNull()
    {
        var session = new PromptSession<string>();

        Assert.Null(session.AutoSuggest);
    }

    [Fact]
    public void Style_Default_IsNull()
    {
        var session = new PromptSession<string>();

        Assert.Null(session.Style);
    }

    [Fact]
    public void KeyBindings_Default_IsNull()
    {
        var session = new PromptSession<string>();

        Assert.Null(session.KeyBindings);
    }

    [Fact]
    public void Placeholder_Default_IsNull()
    {
        var session = new PromptSession<string>();

        Assert.Null(session.Placeholder);
    }

    [Fact]
    public void InputProcessors_Default_IsNull()
    {
        var session = new PromptSession<string>();

        Assert.Null(session.InputProcessors);
    }

    #endregion

    #region Constructor Parameter Passthrough

    [Fact]
    public void Constructor_WithMessage_SetsMessage()
    {
        var session = new PromptSession<string>(message: "sql> ");

        Assert.Equal("sql> ", (string?)session.Message.Value);
    }

    [Fact]
    public void Constructor_WithHistory_UsesProvided()
    {
        var history = new InMemoryHistory();
        var session = new PromptSession<string>(history: history);

        Assert.Same(history, session.History);
    }

    [Fact]
    public void Constructor_WithClipboard_UsesProvided()
    {
        var clipboard = new InMemoryClipboard();
        var session = new PromptSession<string>(clipboard: clipboard);

        Assert.Same(clipboard, session.Clipboard);
    }

    [Fact]
    public void Constructor_WithCompleteStyle_SetsValue()
    {
        var session = new PromptSession<string>(completeStyle: CompleteStyle.ReadlineLike);

        Assert.Equal(CompleteStyle.ReadlineLike, session.CompleteStyle);
    }

    [Fact]
    public void Constructor_WithReserveSpaceForMenu_SetsValue()
    {
        var session = new PromptSession<string>(reserveSpaceForMenu: 16);

        Assert.Equal(16, session.ReserveSpaceForMenu);
    }

    #endregion

    #region Computed Delegation Properties

    [Fact]
    public void EditingMode_SetAndGet_DelegatesToApp()
    {
        var session = new PromptSession<string>();

        session.EditingMode = EditingMode.Vi;
        Assert.Equal(EditingMode.Vi, session.EditingMode);
        Assert.Equal(EditingMode.Vi, session.App.EditingMode);
    }

    [Fact]
    public void Input_DelegatesToApp()
    {
        var session = new PromptSession<string>();

        Assert.Same(session.App.Input, session.Input);
    }

    [Fact]
    public void Output_DelegatesToApp()
    {
        var session = new PromptSession<string>();

        Assert.Same(session.App.Output, session.Output);
    }

    #endregion
}
