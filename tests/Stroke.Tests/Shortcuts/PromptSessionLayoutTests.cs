using Stroke.Completion;
using Stroke.Filters;
using Stroke.FormattedText;
using Stroke.History;
using Stroke.Shortcuts;
using Xunit;

namespace Stroke.Tests.Shortcuts;

/// <summary>
/// Tests for PromptSession layout construction: completion menu visibility by CompleteStyle,
/// completeWhileTyping Condition logic, reserveSpaceForMenu, SplitMultilinePrompt,
/// continuation text, showFrame, and PasswordProcessor wiring.
/// </summary>
public sealed class PromptSessionLayoutTests
{
    #region US4: Completion Display — CompleteStyle Visibility (T027)

    [Fact]
    public void Layout_Column_IsDefault()
    {
        var session = new PromptSession<string>();

        Assert.Equal(CompleteStyle.Column, session.CompleteStyle);
        Assert.NotNull(session.Layout);
    }

    [Fact]
    public void Layout_MultiColumn_SetsCompleteStyle()
    {
        var session = new PromptSession<string>(
            completeStyle: CompleteStyle.MultiColumn);

        Assert.Equal(CompleteStyle.MultiColumn, session.CompleteStyle);
        Assert.NotNull(session.Layout);
    }

    [Fact]
    public void Layout_ReadlineLike_SetsCompleteStyle()
    {
        var session = new PromptSession<string>(
            completeStyle: CompleteStyle.ReadlineLike);

        Assert.Equal(CompleteStyle.ReadlineLike, session.CompleteStyle);
        Assert.NotNull(session.Layout);
    }

    [Fact]
    public void Layout_WithCompleter_LayoutStillCreated()
    {
        var session = new PromptSession<string>(
            completer: new DummyCompleter());

        Assert.NotNull(session.Layout);
        Assert.NotNull(session.DefaultBuffer);
    }

    [Fact]
    public void CompleteWhileTyping_Default_IsTrue()
    {
        var session = new PromptSession<string>();

        Assert.True(session.CompleteWhileTyping.HasValue);
        Assert.True(session.CompleteWhileTyping.BoolValue);
    }

    [Fact]
    public void CompleteWhileTyping_DisabledByHistorySearch()
    {
        // When enableHistorySearch is true, completeWhileTyping condition
        // in the buffer should be false even if completeWhileTyping is true
        var session = new PromptSession<string>(enableHistorySearch: true);

        // The buffer's CompleteWhileTyping filter should evaluate to false
        // because the condition is: completeWhileTyping AND NOT enableHistorySearch
        Assert.False(session.DefaultBuffer.CompleteWhileTypingFilter());
    }

    [Fact]
    public void CompleteWhileTyping_DisabledByReadlineLike()
    {
        var session = new PromptSession<string>(
            completeStyle: CompleteStyle.ReadlineLike);

        // ReadlineLike style suppresses complete-while-typing
        Assert.False(session.DefaultBuffer.CompleteWhileTypingFilter());
    }

    [Fact]
    public void CompleteWhileTyping_EnabledByDefault_WhenColumnStyle()
    {
        var session = new PromptSession<string>(
            completeStyle: CompleteStyle.Column);

        Assert.True(session.DefaultBuffer.CompleteWhileTypingFilter());
    }

    [Fact]
    public void ReserveSpaceForMenu_Default_Is8()
    {
        var session = new PromptSession<string>();

        Assert.Equal(8, session.ReserveSpaceForMenu);
    }

    [Fact]
    public void ReserveSpaceForMenu_CustomValue_Accepted()
    {
        var session = new PromptSession<string>(reserveSpaceForMenu: 0);

        Assert.Equal(0, session.ReserveSpaceForMenu);
    }

    [Fact]
    public void CompleteInThread_False_UsesDirectCompleter()
    {
        var completer = new DummyCompleter();
        var session = new PromptSession<string>(
            completer: completer,
            completeInThread: false);

        // Verify the session was created successfully with the completer
        Assert.Same(completer, session.Completer);
        Assert.False(session.CompleteInThread);
    }

    [Fact]
    public void CompleteInThread_True_SetsFlag()
    {
        var session = new PromptSession<string>(completeInThread: true);

        Assert.True(session.CompleteInThread);
    }

    #endregion

    #region US7: Multiline — SplitMultilinePrompt (T034)

    [Fact]
    public void SplitMultilinePrompt_NoNewline_HasBeforeIsFalse()
    {
        var fragments = new StyleAndTextTuple[]
        {
            new("class:prompt", "> ")
        };
        var (hasBefore, _, _) =
            PromptSession<string>.SplitMultilinePrompt(() => fragments);

        Assert.False(hasBefore());
    }

    [Fact]
    public void SplitMultilinePrompt_WithNewline_HasBeforeIsTrue()
    {
        var fragments = new StyleAndTextTuple[]
        {
            new("class:prompt", "Line1\nLine2\n> ")
        };
        var (hasBefore, _, _) =
            PromptSession<string>.SplitMultilinePrompt(() => fragments);

        Assert.True(hasBefore());
    }

    [Fact]
    public void SplitMultilinePrompt_Before_ReturnsTextBeforeLastNewline()
    {
        // Provide pre-exploded single-char fragments to match the ExplodeTextFragments behavior
        var fragments = new StyleAndTextTuple[]
        {
            new("class:prompt", "abc\n> ")
        };
        var (_, before, _) =
            PromptSession<string>.SplitMultilinePrompt(() => fragments);

        var result = before();
        // The "before" portion is everything before the last \n
        var text = string.Join("", result.Select(f => f.Text));
        Assert.Contains("abc", text);
        Assert.DoesNotContain("> ", text);
    }

    [Fact]
    public void SplitMultilinePrompt_FirstInputLine_ReturnsLastLine()
    {
        var fragments = new StyleAndTextTuple[]
        {
            new("class:prompt", "abc\n> ")
        };
        var (_, _, firstInput) =
            PromptSession<string>.SplitMultilinePrompt(() => fragments);

        var result = firstInput();
        var text = string.Join("", result.Select(f => f.Text));
        Assert.Equal("> ", text);
    }

    [Fact]
    public void SplitMultilinePrompt_NewlineOnly_HasBeforeIsTrue()
    {
        // Edge Case 8: newline-only prompt
        var fragments = new StyleAndTextTuple[]
        {
            new("class:prompt", "\n\n")
        };
        var (hasBefore, _, _) =
            PromptSession<string>.SplitMultilinePrompt(() => fragments);

        Assert.True(hasBefore());
    }

    [Fact]
    public void SplitMultilinePrompt_SingleNewline_FirstInputLineIsEmpty()
    {
        var fragments = new StyleAndTextTuple[]
        {
            new("class:prompt", "abc\n")
        };
        var (_, _, firstInput) =
            PromptSession<string>.SplitMultilinePrompt(() => fragments);

        var result = firstInput();
        // After the last newline, there's nothing
        var text = string.Join("", result.Select(f => f.Text));
        Assert.Equal("", text);
    }

    [Fact]
    public void Multiline_Default_IsFalsy()
    {
        var session = new PromptSession<string>();

        Assert.False(FilterUtils.ToFilter(session.Multiline).Invoke());
    }

    [Fact]
    public void Multiline_SetTrue_IsTrue()
    {
        var session = new PromptSession<string>(multiline: true);

        Assert.True(FilterUtils.ToFilter(session.Multiline).Invoke());
    }

    [Fact]
    public void ShowFrame_Default_IsFalsy()
    {
        var session = new PromptSession<string>();

        Assert.False(FilterUtils.ToFilter(session.ShowFrame).Invoke());
    }

    [Fact]
    public void ShowFrame_SetTrue_IsTrue()
    {
        var session = new PromptSession<string>(showFrame: true);

        Assert.True(FilterUtils.ToFilter(session.ShowFrame).Invoke());
    }

    [Fact]
    public void PromptContinuation_Default_IsNull()
    {
        var session = new PromptSession<string>();

        Assert.Null(session.PromptContinuation);
    }

    [Fact]
    public void PromptContinuation_Callable_Accepted()
    {
        PromptContinuationCallable cont = (width, line, wrap) => ". ";
        var session = new PromptSession<string>();
        session.PromptContinuation = cont;

        Assert.Same(cont, session.PromptContinuation);
    }

    [Fact]
    public void PromptContinuation_String_Accepted()
    {
        var session = new PromptSession<string>();
        session.PromptContinuation = "... ";

        Assert.Equal("... ", session.PromptContinuation);
    }

    #endregion

    #region US8: Password — PasswordProcessor Wiring (T036)

    [Fact]
    public void IsPassword_Default_IsFalsy()
    {
        var session = new PromptSession<string>();

        Assert.False(FilterUtils.ToFilter(session.IsPassword).Invoke());
    }

    [Fact]
    public void IsPassword_SetTrue_IsTrue()
    {
        var session = new PromptSession<string>(isPassword: true);

        Assert.True(FilterUtils.ToFilter(session.IsPassword).Invoke());
    }

    [Fact]
    public void IsPassword_LayoutCreated_WithPasswordMode()
    {
        // Even with isPassword=true, the layout should be created successfully
        // The PasswordProcessor is wired via ConditionalProcessor(PasswordProcessor, DynCond(isPassword))
        var session = new PromptSession<string>(isPassword: true);

        Assert.NotNull(session.Layout);
        Assert.NotNull(session.App);
    }

    [Fact]
    public void IsPassword_DynCond_ReactiveToChange()
    {
        var session = new PromptSession<string>();

        // Initially falsy
        Assert.False(FilterUtils.ToFilter(session.IsPassword).Invoke());

        // Change to true
        session.IsPassword = new FilterOrBool(true);
        Assert.True(FilterUtils.ToFilter(session.IsPassword).Invoke());

        // Change back
        session.IsPassword = new FilterOrBool(false);
        Assert.False(FilterUtils.ToFilter(session.IsPassword).Invoke());
    }

    #endregion

    #region Test Helpers

    /// <summary>
    /// Minimal completer for testing layout creation with a completer configured.
    /// </summary>
    private sealed class DummyCompleter : ICompleter
    {
        public IEnumerable<Stroke.Completion.Completion> GetCompletions(
            Stroke.Core.Document document, CompleteEvent completeEvent)
        {
            yield break;
        }

        public async IAsyncEnumerable<Stroke.Completion.Completion> GetCompletionsAsync(
            Stroke.Core.Document document,
            CompleteEvent completeEvent,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
            yield break;
        }
    }

    #endregion
}
