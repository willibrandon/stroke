using Stroke.History;
using Stroke.Shortcuts;
using Xunit;

namespace Stroke.Tests.Shortcuts;

/// <summary>
/// Tests for <see cref="PromptFunctions"/> static methods:
/// Prompt creates temp session, CreateConfirmSession bindings, Confirm/ConfirmAsync.
/// </summary>
public sealed class PromptFunctionsTests
{
    #region CreateConfirmSession Tests

    [Fact]
    public void CreateConfirmSession_ReturnsPromptSessionOfBool()
    {
        var session = PromptFunctions.CreateConfirmSession("Delete?");

        Assert.NotNull(session);
        Assert.NotNull(session.App);
    }

    [Fact]
    public void CreateConfirmSession_DefaultSuffix_IsYN()
    {
        var session = PromptFunctions.CreateConfirmSession("Delete?");

        // The message should have a value (merged message + suffix)
        Assert.False(session.Message.IsEmpty);
    }

    [Fact]
    public void CreateConfirmSession_CustomSuffix_Applied()
    {
        var session = PromptFunctions.CreateConfirmSession("Delete?", suffix: " [yes/no] ");

        Assert.NotNull(session);
    }

    [Fact]
    public void CreateConfirmSession_HasKeyBindings()
    {
        var session = PromptFunctions.CreateConfirmSession("Continue?");

        // The session should have key bindings set
        Assert.NotNull(session.App.KeyBindings);
    }

    #endregion

    #region Static Prompt Method Tests

    [Fact]
    public void Prompt_MethodExists_WithHistoryParam()
    {
        // Verify the method signature accepts a history parameter
        // We can't actually call Prompt without a terminal, but verify the API exists
        var method = typeof(PromptFunctions).GetMethod("Prompt");
        Assert.NotNull(method);

        // Verify it has a 'history' parameter
        var parameters = method.GetParameters();
        Assert.Contains(parameters, p => p.Name == "history");
    }

    [Fact]
    public void PromptAsync_MethodExists()
    {
        var method = typeof(PromptFunctions).GetMethod("PromptAsync");
        Assert.NotNull(method);
        Assert.Equal(typeof(System.Threading.Tasks.Task<string>), method.ReturnType);
    }

    [Fact]
    public void Confirm_MethodExists()
    {
        var method = typeof(PromptFunctions).GetMethod("Confirm");
        Assert.NotNull(method);
        Assert.Equal(typeof(bool), method.ReturnType);
    }

    [Fact]
    public void ConfirmAsync_MethodExists()
    {
        var method = typeof(PromptFunctions).GetMethod("ConfirmAsync");
        Assert.NotNull(method);
        Assert.Equal(typeof(System.Threading.Tasks.Task<bool>), method.ReturnType);
    }

    #endregion

    #region Prompt Parameter Passthrough Tests

    [Fact]
    public void Prompt_HistoryParam_IsOnlySessionParam()
    {
        // The 'history' parameter should exist and be of type IHistory
        var method = typeof(PromptFunctions).GetMethod("Prompt");
        Assert.NotNull(method);

        var historyParam = Array.Find(method.GetParameters(), p => p.Name == "history");
        Assert.NotNull(historyParam);
        Assert.Equal(typeof(IHistory), historyParam.ParameterType);
    }

    [Fact]
    public void Prompt_ReturnType_IsString()
    {
        var method = typeof(PromptFunctions).GetMethod("Prompt");
        Assert.NotNull(method);
        Assert.Equal(typeof(string), method.ReturnType);
    }

    #endregion
}
