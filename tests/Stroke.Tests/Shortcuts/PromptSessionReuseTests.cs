using Stroke.History;
using Stroke.Shortcuts;
using Xunit;

namespace Stroke.Tests.Shortcuts;

/// <summary>
/// Tests for session reuse behavior: history persistence, buffer reset between calls,
/// and property persistence across Prompt calls (T022).
/// </summary>
public sealed class PromptSessionReuseTests
{
    #region History Persistence Tests

    [Fact]
    public void Session_SharedHistory_PersistsAcrossCalls()
    {
        var history = new InMemoryHistory();
        var session = new PromptSession<string>(history: history);

        // History is the same instance throughout session lifetime
        Assert.Same(history, session.History);
    }

    [Fact]
    public void Session_DefaultHistory_IsInMemoryHistory()
    {
        var session = new PromptSession<string>();

        // Default history should be a real InMemoryHistory (not null)
        Assert.IsType<InMemoryHistory>(session.History);
    }

    #endregion

    #region Buffer Reset Between Calls Tests

    [Fact]
    public void DefaultBuffer_InitialState_HasEmptyDocument()
    {
        var session = new PromptSession<string>();

        Assert.Equal("", session.DefaultBuffer.Document.Text);
    }

    [Fact]
    public void DefaultBuffer_Name_IsDefaultBuffer()
    {
        var session = new PromptSession<string>();

        Assert.Equal("DEFAULT_BUFFER", session.DefaultBuffer.Name);
    }

    [Fact]
    public void DefaultBuffer_InitialCursorPosition_IsZero()
    {
        var session = new PromptSession<string>();

        Assert.Equal(0, session.DefaultBuffer.CursorPosition);
    }

    [Fact]
    public void DefaultBuffer_InitialCompletionState_IsNull()
    {
        var session = new PromptSession<string>();

        Assert.Null(session.DefaultBuffer.CompleteState);
    }

    #endregion

    #region Property Persistence Tests

    [Fact]
    public void Session_PropertyChange_Persists()
    {
        var session = new PromptSession<string>();

        session.Message = "new> ";
        Assert.Equal("new> ", (string?)session.Message.Value);
    }

    [Fact]
    public void Session_CompleteStyle_Persists()
    {
        var session = new PromptSession<string>();

        session.CompleteStyle = CompleteStyle.MultiColumn;
        Assert.Equal(CompleteStyle.MultiColumn, session.CompleteStyle);
    }

    [Fact]
    public void Session_ReserveSpaceForMenu_Persists()
    {
        var session = new PromptSession<string>(reserveSpaceForMenu: 4);

        Assert.Equal(4, session.ReserveSpaceForMenu);
        session.ReserveSpaceForMenu = 12;
        Assert.Equal(12, session.ReserveSpaceForMenu);
    }

    #endregion
}
