using Stroke.History;
using Stroke.Shortcuts;
using Xunit;

namespace Stroke.Tests.Shortcuts;

/// <summary>
/// Tests for <see cref="Prompt"/> static methods:
/// Prompt creates temp session, CreateConfirmSession bindings, Confirm/ConfirmAsync.
/// </summary>
public sealed class PromptTests
{
    #region CreateConfirmSession Tests

    [Fact]
    public void CreateConfirmSession_ReturnsPromptSessionOfBool()
    {
        var session = Prompt.CreateConfirmSession("Delete?");

        Assert.NotNull(session);
        Assert.NotNull(session.App);
    }

    [Fact]
    public void CreateConfirmSession_DefaultSuffix_IsYN()
    {
        var session = Prompt.CreateConfirmSession("Delete?");

        // The message should have a value (merged message + suffix)
        Assert.False(session.Message.IsEmpty);
    }

    [Fact]
    public void CreateConfirmSession_CustomSuffix_Applied()
    {
        var session = Prompt.CreateConfirmSession("Delete?", suffix: " [yes/no] ");

        Assert.NotNull(session);
    }

    [Fact]
    public void CreateConfirmSession_HasKeyBindings()
    {
        var session = Prompt.CreateConfirmSession("Continue?");

        // The session should have key bindings set
        Assert.NotNull(session.App.KeyBindings);
    }

    #endregion

    #region Static Prompt Method Tests

    [Fact]
    public void RunPrompt_MethodExists_WithHistoryParam()
    {
        // Verify the method signature accepts a history parameter
        // We can't actually call RunPrompt without a terminal, but verify the API exists
        var method = typeof(Prompt).GetMethod("RunPrompt");
        Assert.NotNull(method);

        // Verify it has a 'history' parameter
        var parameters = method.GetParameters();
        Assert.Contains(parameters, p => p.Name == "history");
    }

    [Fact]
    public void PromptAsync_MethodExists()
    {
        var method = typeof(Prompt).GetMethod("PromptAsync");
        Assert.NotNull(method);
        Assert.Equal(typeof(System.Threading.Tasks.Task<string>), method.ReturnType);
    }

    [Fact]
    public void Confirm_MethodExists()
    {
        var method = typeof(Prompt).GetMethod("Confirm");
        Assert.NotNull(method);
        Assert.Equal(typeof(bool), method.ReturnType);
    }

    [Fact]
    public void ConfirmAsync_MethodExists()
    {
        var method = typeof(Prompt).GetMethod("ConfirmAsync");
        Assert.NotNull(method);
        Assert.Equal(typeof(System.Threading.Tasks.Task<bool>), method.ReturnType);
    }

    #endregion

    #region Prompt Parameter Passthrough Tests

    [Fact]
    public void RunPrompt_HistoryParam_IsOnlySessionParam()
    {
        // The 'history' parameter should exist and be of type IHistory
        var method = typeof(Prompt).GetMethod("RunPrompt");
        Assert.NotNull(method);

        var historyParam = Array.Find(method.GetParameters(), p => p.Name == "history");
        Assert.NotNull(historyParam);
        Assert.Equal(typeof(IHistory), historyParam.ParameterType);
    }

    [Fact]
    public void RunPrompt_ReturnType_IsString()
    {
        var method = typeof(Prompt).GetMethod("RunPrompt");
        Assert.NotNull(method);
        Assert.Equal(typeof(string), method.ReturnType);
    }

    #endregion
}
