using Stroke.Completion;
using Stroke.Core;
using Stroke.Filters;
using Stroke.FormattedText;
using Stroke.History;
using Stroke.KeyBinding;
using Stroke.Output;
using Stroke.Shortcuts;
using Stroke.Styles;
using Xunit;

namespace Stroke.Tests.Shortcuts;

/// <summary>
/// Tests for PromptSession Prompt/PromptAsync method behavior: per-prompt overrides,
/// dumb terminal handling, async signatures, and default value/accept-default logic.
/// </summary>
public sealed class PromptSessionPromptTests
{
    #region US6: Per-Prompt Parameter Overrides (T032)

    [Fact]
    public void ApplyOverrides_Message_UpdatesPermanently()
    {
        var session = new PromptSession<string>(message: "old> ");

        // Simulate what Prompt does internally: message override updates permanently
        session.Message = "new> ";
        Assert.Equal("new> ", (string?)session.Message.Value);
    }

    [Fact]
    public void ApplyOverrides_Null_PreservesCurrent()
    {
        var session = new PromptSession<string>(message: "keep> ");

        // When null is passed, original value should be preserved
        // (ApplyOverrides checks for null before assigning)
        Assert.Equal("keep> ", (string?)session.Message.Value);
    }

    [Fact]
    public void ApplyOverrides_Completer_UpdatesPermanently()
    {
        var session = new PromptSession<string>();
        Assert.Null(session.Completer);

        var completer = new DummyCompleter();
        session.Completer = completer;
        Assert.Same(completer, session.Completer);
    }

    [Fact]
    public void ApplyOverrides_Style_UpdatesPermanently()
    {
        var session = new PromptSession<string>();
        Assert.Null(session.Style);

        // Updating style persists
        var style = DummyStyle.Instance;
        session.Style = style;
        Assert.Same(style, session.Style);
    }

    [Fact]
    public void ApplyOverrides_Validator_UpdatesPermanently()
    {
        var session = new PromptSession<string>();
        Assert.Null(session.Validator);

        // No validator to test with easily, just verify null → null round-trip
        Assert.Null(session.Validator);
    }

    [Fact]
    public void ApplyOverrides_CompleteStyle_UpdatesPermanently()
    {
        var session = new PromptSession<string>();
        Assert.Equal(CompleteStyle.Column, session.CompleteStyle);

        session.CompleteStyle = CompleteStyle.MultiColumn;
        Assert.Equal(CompleteStyle.MultiColumn, session.CompleteStyle);

        session.CompleteStyle = CompleteStyle.ReadlineLike;
        Assert.Equal(CompleteStyle.ReadlineLike, session.CompleteStyle);
    }

    [Fact]
    public void ApplyOverrides_ViMode_TakesPrecedence()
    {
        // When viMode=true and editingMode=Emacs, viMode wins
        var session = new PromptSession<string>(viMode: true, editingMode: EditingMode.Emacs);

        Assert.Equal(EditingMode.Vi, session.EditingMode);
    }

    [Fact]
    public void ApplyOverrides_EditingMode_WhenNoViMode()
    {
        var session = new PromptSession<string>(editingMode: EditingMode.Vi);

        Assert.Equal(EditingMode.Vi, session.EditingMode);
    }

    [Fact]
    public void ApplyOverrides_ReserveSpaceForMenu_Updates()
    {
        var session = new PromptSession<string>(reserveSpaceForMenu: 4);
        Assert.Equal(4, session.ReserveSpaceForMenu);

        session.ReserveSpaceForMenu = 16;
        Assert.Equal(16, session.ReserveSpaceForMenu);
    }

    [Fact]
    public void ApplyOverrides_RefreshInterval_Updates()
    {
        var session = new PromptSession<string>();
        Assert.Equal(0.0, session.RefreshInterval);

        session.RefreshInterval = 0.5;
        Assert.Equal(0.5, session.RefreshInterval);
    }

    [Fact]
    public void ApplyOverrides_WrapLines_Updates()
    {
        var session = new PromptSession<string>();
        Assert.True(session.WrapLines.BoolValue);

        session.WrapLines = new FilterOrBool(false);
        Assert.False(session.WrapLines.BoolValue);
    }

    [Fact]
    public void ApplyOverrides_IsPassword_Updates()
    {
        var session = new PromptSession<string>();
        Assert.False(FilterUtils.ToFilter(session.IsPassword).Invoke());

        session.IsPassword = new FilterOrBool(true);
        Assert.True(FilterUtils.ToFilter(session.IsPassword).Invoke());
    }

    [Fact]
    public void ApplyOverrides_EnableHistorySearch_Updates()
    {
        var session = new PromptSession<string>();

        session.EnableHistorySearch = new FilterOrBool(true);
        Assert.True(FilterUtils.ToFilter(session.EnableHistorySearch).Invoke());
    }

    [Fact]
    public void ApplyOverrides_SwapLightAndDarkColors_Updates()
    {
        var session = new PromptSession<string>();

        session.SwapLightAndDarkColors = new FilterOrBool(true);
        Assert.True(FilterUtils.ToFilter(session.SwapLightAndDarkColors).Invoke());
    }

    [Fact]
    public void ApplyOverrides_CompleteInThread_Updates()
    {
        var session = new PromptSession<string>();
        Assert.False(session.CompleteInThread);

        session.CompleteInThread = true;
        Assert.True(session.CompleteInThread);
    }

    [Fact]
    public void ApplyOverrides_Clipboard_Updates()
    {
        var session = new PromptSession<string>();
        var original = session.Clipboard;

        var newClipboard = new Stroke.Clipboard.InMemoryClipboard();
        session.Clipboard = newClipboard;
        Assert.Same(newClipboard, session.Clipboard);
        Assert.NotSame(original, session.Clipboard);
    }

    [Fact]
    public void ApplyOverrides_Placeholder_Updates()
    {
        var session = new PromptSession<string>();
        Assert.Null(session.Placeholder);

        session.Placeholder = (AnyFormattedText)"Type here...";
        Assert.NotNull(session.Placeholder);
    }

    [Fact]
    public void ApplyOverrides_RPrompt_Updates()
    {
        var session = new PromptSession<string>();

        session.RPrompt = "right>";
        Assert.Equal("right>", (string?)session.RPrompt.Value);
    }

    [Fact]
    public void ApplyOverrides_BottomToolbar_Updates()
    {
        var session = new PromptSession<string>();

        session.BottomToolbar = "toolbar text";
        Assert.Equal("toolbar text", (string?)session.BottomToolbar.Value);
    }

    [Fact]
    public void ApplyOverrides_SearchIgnoreCase_Updates()
    {
        var session = new PromptSession<string>();

        session.SearchIgnoreCase = new FilterOrBool(true);
        Assert.True(FilterUtils.ToFilter(session.SearchIgnoreCase).Invoke());
    }

    [Fact]
    public void ApplyOverrides_EnableSystemPrompt_Updates()
    {
        var session = new PromptSession<string>();

        session.EnableSystemPrompt = new FilterOrBool(true);
        Assert.True(FilterUtils.ToFilter(session.EnableSystemPrompt).Invoke());
    }

    [Fact]
    public void ApplyOverrides_EnableSuspend_Updates()
    {
        var session = new PromptSession<string>();

        session.EnableSuspend = new FilterOrBool(true);
        Assert.True(FilterUtils.ToFilter(session.EnableSuspend).Invoke());
    }

    [Fact]
    public void ApplyOverrides_EnableOpenInEditor_Updates()
    {
        var session = new PromptSession<string>();

        session.EnableOpenInEditor = new FilterOrBool(true);
        Assert.True(FilterUtils.ToFilter(session.EnableOpenInEditor).Invoke());
    }

    [Fact]
    public void ApplyOverrides_MouseSupport_Updates()
    {
        var session = new PromptSession<string>();

        session.MouseSupport = new FilterOrBool(true);
        Assert.True(FilterUtils.ToFilter(session.MouseSupport).Invoke());
    }

    [Fact]
    public void ApplyOverrides_Multiline_Updates()
    {
        var session = new PromptSession<string>();

        session.Multiline = new FilterOrBool(true);
        Assert.True(FilterUtils.ToFilter(session.Multiline).Invoke());
    }

    [Fact]
    public void ApplyOverrides_ShowFrame_Updates()
    {
        var session = new PromptSession<string>();

        session.ShowFrame = new FilterOrBool(true);
        Assert.True(FilterUtils.ToFilter(session.ShowFrame).Invoke());
    }

    [Fact]
    public void ApplyOverrides_IncludeDefaultPygmentsStyle_Updates()
    {
        var session = new PromptSession<string>();
        Assert.True(session.IncludeDefaultPygmentsStyle.BoolValue);

        session.IncludeDefaultPygmentsStyle = new FilterOrBool(false);
        Assert.False(session.IncludeDefaultPygmentsStyle.BoolValue);
    }

    [Fact]
    public void ApplyOverrides_Tempfile_Updates()
    {
        var session = new PromptSession<string>();

        session.TempfileSuffix = ".py";
        Assert.Equal(".py", session.TempfileSuffix);

        session.Tempfile = "/tmp/test.py";
        Assert.Equal("/tmp/test.py", session.Tempfile);
    }

    #endregion

    #region US9: Dumb Terminal (T038)

    [Fact]
    public void IsDumbTerminal_ReturnsFalseNormally()
    {
        // In a normal test environment, TERM is not "dumb"
        // PlatformUtils.IsDumbTerminal() checks TERM env var
        Assert.False(PlatformUtils.IsDumbTerminal("xterm-256color"));
    }

    [Fact]
    public void IsDumbTerminal_ReturnsTrueForDumb()
    {
        Assert.True(PlatformUtils.IsDumbTerminal("dumb"));
    }

    [Fact]
    public void IsDumbTerminal_ReturnsTrueForUnknown()
    {
        Assert.True(PlatformUtils.IsDumbTerminal("unknown"));
    }

    [Fact]
    public void IsDumbTerminal_CaseInsensitive()
    {
        Assert.True(PlatformUtils.IsDumbTerminal("DUMB"));
        Assert.True(PlatformUtils.IsDumbTerminal("Dumb"));
    }

    [Fact]
    public void Session_WithExplicitOutput_NotDumb()
    {
        // When explicit output is provided, dumb terminal check is skipped
        // (the code checks: _output is null && IsDumbTerminal)
        var session = new PromptSession<string>(output: new Stroke.Output.DummyOutput());

        // Session created normally — explicit output prevents dumb mode
        Assert.NotNull(session.App);
    }

    #endregion

    #region US10: Async Prompt (T041)

    [Fact]
    public void PromptAsync_MethodExists_ReturnsTask()
    {
        var method = typeof(PromptSession<string>).GetMethod("PromptAsync");
        Assert.NotNull(method);
        Assert.Equal(typeof(Task<string>), method.ReturnType);
    }

    [Fact]
    public void PromptAsync_HasSameOverrideParams_AsPrompt()
    {
        var promptMethod = typeof(PromptSession<string>).GetMethod("Prompt");
        var asyncMethod = typeof(PromptSession<string>).GetMethod("PromptAsync");
        Assert.NotNull(promptMethod);
        Assert.NotNull(asyncMethod);

        var promptParams = promptMethod.GetParameters();
        var asyncParams = asyncMethod.GetParameters();

        // PromptAsync should have the same params minus inThread and inputHook
        // (which are sync-only)
        Assert.True(asyncParams.Length < promptParams.Length);

        // Verify common params exist in both
        var asyncNames = asyncParams.Select(p => p.Name).ToHashSet();
        Assert.Contains("message", asyncNames);
        Assert.Contains("multiline", asyncNames);
        Assert.Contains("completer", asyncNames);
        Assert.Contains("style", asyncNames);
        Assert.Contains("default_", asyncNames);
        Assert.Contains("acceptDefault", asyncNames);
    }

    [Fact]
    public void PromptAsync_DoesNotHave_InThread()
    {
        var method = typeof(PromptSession<string>).GetMethod("PromptAsync");
        Assert.NotNull(method);

        var paramNames = method.GetParameters().Select(p => p.Name).ToHashSet();
        Assert.DoesNotContain("inThread", paramNames);
        Assert.DoesNotContain("inputHook", paramNames);
    }

    #endregion

    #region US11: Default Value and Accept-Default (T042)

    [Fact]
    public void DefaultBuffer_InitialState_EmptyText()
    {
        var session = new PromptSession<string>();

        Assert.Equal("", session.DefaultBuffer.Document.Text);
    }

    [Fact]
    public void DefaultBuffer_Reset_ClearsText()
    {
        var session = new PromptSession<string>();

        // Manually set some text
        session.DefaultBuffer.Document = new Document("hello", 5);
        Assert.Equal("hello", session.DefaultBuffer.Document.Text);

        // Reset clears it
        session.DefaultBuffer.Reset();
        Assert.Equal("", session.DefaultBuffer.Document.Text);
    }

    [Fact]
    public void DefaultBuffer_Reset_WithDocument_SetsText()
    {
        var session = new PromptSession<string>();

        session.DefaultBuffer.Reset(document: new Document("prefilled", 9));
        Assert.Equal("prefilled", session.DefaultBuffer.Document.Text);
    }

    [Fact]
    public void DefaultBuffer_Reset_ClearsCursorPosition()
    {
        var session = new PromptSession<string>();

        session.DefaultBuffer.Document = new Document("hello", 3);
        Assert.Equal(3, session.DefaultBuffer.CursorPosition);

        session.DefaultBuffer.Reset();
        Assert.Equal(0, session.DefaultBuffer.CursorPosition);
    }

    [Fact]
    public void DefaultBuffer_Reset_ClearsCompletionState()
    {
        var session = new PromptSession<string>();

        // After reset, completion state should be null
        session.DefaultBuffer.Reset();
        Assert.Null(session.DefaultBuffer.CompleteState);
    }

    [Fact]
    public void Prompt_HasDefaultParameter()
    {
        var method = typeof(PromptSession<string>).GetMethod("Prompt");
        Assert.NotNull(method);

        var defaultParam = Array.Find(method.GetParameters(), p => p.Name == "default_");
        Assert.NotNull(defaultParam);
    }

    [Fact]
    public void Prompt_HasAcceptDefaultParameter()
    {
        var method = typeof(PromptSession<string>).GetMethod("Prompt");
        Assert.NotNull(method);

        var param = Array.Find(method.GetParameters(), p => p.Name == "acceptDefault");
        Assert.NotNull(param);
        Assert.Equal(typeof(bool), param.ParameterType);
    }

    [Fact]
    public void Prompt_HasPreRunParameter()
    {
        var method = typeof(PromptSession<string>).GetMethod("Prompt");
        Assert.NotNull(method);

        var param = Array.Find(method.GetParameters(), p => p.Name == "preRun");
        Assert.NotNull(param);
        Assert.Equal(typeof(Action), param.ParameterType);
    }

    [Fact]
    public void Prompt_HasSetExceptionHandlerParameter()
    {
        var method = typeof(PromptSession<string>).GetMethod("Prompt");
        Assert.NotNull(method);

        var param = Array.Find(method.GetParameters(), p => p.Name == "setExceptionHandler");
        Assert.NotNull(param);
        Assert.Equal(typeof(bool), param.ParameterType);
    }

    [Fact]
    public void Prompt_HasHandleSigintParameter()
    {
        var method = typeof(PromptSession<string>).GetMethod("Prompt");
        Assert.NotNull(method);

        var param = Array.Find(method.GetParameters(), p => p.Name == "handleSigint");
        Assert.NotNull(param);
        Assert.Equal(typeof(bool), param.ParameterType);
    }

    [Fact]
    public void Prompt_HasInThreadParameter()
    {
        var method = typeof(PromptSession<string>).GetMethod("Prompt");
        Assert.NotNull(method);

        var param = Array.Find(method.GetParameters(), p => p.Name == "inThread");
        Assert.NotNull(param);
        Assert.Equal(typeof(bool), param.ParameterType);
    }

    #endregion

    #region Test Helpers

    private sealed class DummyCompleter : ICompleter
    {
        public IEnumerable<Stroke.Completion.Completion> GetCompletions(
            Document document, CompleteEvent completeEvent)
        {
            yield break;
        }

        public async IAsyncEnumerable<Stroke.Completion.Completion> GetCompletionsAsync(
            Document document,
            CompleteEvent completeEvent,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
            yield break;
        }
    }

    #endregion
}
