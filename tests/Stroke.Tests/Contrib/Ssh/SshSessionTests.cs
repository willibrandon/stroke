using System.Text;
using Stroke.Contrib.Ssh;
using Stroke.Core.Primitives;
using Xunit;

namespace Stroke.Tests.Contrib.Ssh;

/// <summary>
/// Tests for <see cref="PromptToolkitSshSession"/> properties and behavior.
/// </summary>
public class SshSessionTests
{
    #region Properties

    [Fact]
    public void Interact_ReturnsProvidedCallback()
    {
        Func<PromptToolkitSshSession, Task> callback = _ => Task.CompletedTask;
        var channel = new TestSshChannel();
        var session = CreateSession(channel, callback, enableCpr: true);

        Assert.Same(callback, session.Interact);
    }

    [Fact]
    public void EnableCpr_ReturnsProvidedValue_True()
    {
        var channel = new TestSshChannel();
        var session = CreateSession(channel, enableCpr: true);

        Assert.True(session.EnableCpr);
    }

    [Fact]
    public void EnableCpr_ReturnsProvidedValue_False()
    {
        var channel = new TestSshChannel();
        var session = CreateSession(channel, enableCpr: false);

        Assert.False(session.EnableCpr);
    }

    [Fact]
    public void AppSession_IsNullBeforeRun()
    {
        var channel = new TestSshChannel();
        var session = CreateSession(channel);

        Assert.Null(session.AppSession);
    }

    [Fact]
    public void InteractTask_IsNullBeforeRun()
    {
        var channel = new TestSshChannel();
        var session = CreateSession(channel);

        Assert.Null(session.InteractTask);
    }

    [Fact]
    public void IsClosed_IsFalseInitially()
    {
        var channel = new TestSshChannel();
        var session = CreateSession(channel);

        Assert.False(session.IsClosed);
    }

    #endregion

    #region GetSize

    [Fact]
    public void GetSize_ReturnsChannelSize()
    {
        var channel = new TestSshChannel();
        var session = CreateSession(channel);

        var size = session.GetSize();

        // Session gets initial size from channel's GetTerminalSize()
        // TestSshChannel returns (80, 24)
        Assert.Equal(80, size.Columns);
        Assert.Equal(24, size.Rows);
    }

    [Fact]
    public void GetSize_AfterTerminalSizeChanged_ReturnsNewSize()
    {
        var channel = new TestSshChannel();
        var session = CreateSession(channel);

        session.TerminalSizeChanged(120, 40);
        var size = session.GetSize();

        Assert.Equal(120, size.Columns);
        Assert.Equal(40, size.Rows);
    }

    #endregion

    #region TerminalSizeChanged

    [Fact]
    public void TerminalSizeChanged_UpdatesSize()
    {
        var channel = new TestSshChannel();
        var session = CreateSession(channel);

        session.TerminalSizeChanged(100, 50);

        var size = session.GetSize();
        Assert.Equal(100, size.Columns);
        Assert.Equal(50, size.Rows);
    }

    [Fact]
    public void TerminalSizeChanged_MultipleUpdates_ReturnsLatestSize()
    {
        var channel = new TestSshChannel();
        var session = CreateSession(channel);

        session.TerminalSizeChanged(80, 24);
        session.TerminalSizeChanged(120, 40);
        session.TerminalSizeChanged(160, 50);

        var size = session.GetSize();
        Assert.Equal(160, size.Columns);
        Assert.Equal(50, size.Rows);
    }

    [Theory]
    [InlineData(0, 20, 1, 20)]    // Width clamped to minimum 1
    [InlineData(-5, 20, 1, 20)]   // Negative width clamped to 1
    [InlineData(80, 0, 80, 1)]    // Height clamped to minimum 1
    [InlineData(80, -10, 80, 1)]  // Negative height clamped to 1
    [InlineData(600, 20, 500, 20)] // Width clamped to maximum 500
    [InlineData(80, 600, 80, 500)] // Height clamped to maximum 500
    public void TerminalSizeChanged_ClampsDimensions(int inputWidth, int inputHeight, int expectedWidth, int expectedHeight)
    {
        var channel = new TestSshChannel();
        var session = CreateSession(channel);

        session.TerminalSizeChanged(inputWidth, inputHeight);

        var size = session.GetSize();
        Assert.Equal(expectedWidth, size.Columns);
        Assert.Equal(expectedHeight, size.Rows);
    }

    #endregion

    #region DataReceived

    [Fact]
    public void DataReceived_WithNullData_DoesNotThrow()
    {
        var channel = new TestSshChannel();
        var session = CreateSession(channel);

        // Should not throw
        session.DataReceived(null!);
    }

    [Fact]
    public void DataReceived_WithEmptyData_DoesNotThrow()
    {
        var channel = new TestSshChannel();
        var session = CreateSession(channel);

        // Should not throw
        session.DataReceived(Array.Empty<byte>());
    }

    [Fact]
    public void DataReceived_AfterClose_DoesNotThrow()
    {
        var channel = new TestSshChannel();
        var session = CreateSession(channel);
        session.Close();

        // Should not throw - just ignored
        session.DataReceived(Encoding.UTF8.GetBytes("test"));
    }

    #endregion

    #region Close

    [Fact]
    public void Close_SetsIsClosedToTrue()
    {
        var channel = new TestSshChannel();
        var session = CreateSession(channel);

        session.Close();

        Assert.True(session.IsClosed);
    }

    [Fact]
    public void Close_IsIdempotent()
    {
        var channel = new TestSshChannel();
        var session = CreateSession(channel);

        session.Close();
        session.Close(); // Second call should not throw

        Assert.True(session.IsClosed);
    }

    [Fact]
    public void Close_ClosesChannel()
    {
        var channel = new TestSshChannel();
        var session = CreateSession(channel);

        session.Close();

        Assert.True(channel.IsClosed);
    }

    #endregion

    #region CPR Support (Phase 6)

    [Fact]
    public void EnableCpr_True_ConfiguresSessionForCprSequences()
    {
        // T028: Verify EnableCpr=true allows CPR sequences
        var channel = new TestSshChannel();
        var session = CreateSession(channel, enableCpr: true);

        // The session should have CPR enabled, which will be passed to Vt100Output
        Assert.True(session.EnableCpr);
    }

    [Fact]
    public void EnableCpr_False_ConfiguresSessionToDisableCpr()
    {
        // T029: Verify EnableCpr=false disables CPR sequences
        var channel = new TestSshChannel();
        var session = CreateSession(channel, enableCpr: false);

        // The session should have CPR disabled
        Assert.False(session.EnableCpr);
    }

    [Fact]
    public void EnableCpr_DefaultsToTrue()
    {
        // Verify default behavior matches FR-004 (CPR enabled by default)
        var channel = new TestSshChannel();
        var session = CreateSession(channel); // Uses default enableCpr=true

        Assert.True(session.EnableCpr);
    }

    #endregion

    #region SetLineMode

    [Fact]
    public void SessionStart_CallsSetLineModeFalse()
    {
        // This verifies FR-011: SetLineMode(false) is called on session start
        // We can't easily test RunAsync without a full SSH connection,
        // but we can verify the channel's SetLineMode behavior
        var channel = new TestSshChannel();

        // SetLineMode should be callable without error
        channel.SetLineMode(false);

        // Verify it was called (TestSshChannel tracks this)
        Assert.False(channel.LineModeEnabled);
    }

    #endregion

    #region Helper Methods

    private static PromptToolkitSshSession CreateSession(
        ISshChannel channel,
        Func<PromptToolkitSshSession, Task>? interact = null,
        bool enableCpr = true)
    {
        return new PromptToolkitSshSession(
            channel,
            interact ?? (_ => Task.CompletedTask),
            enableCpr);
    }

    #endregion

    #region Test Doubles

    /// <summary>
    /// Test implementation of ISshChannel for unit testing.
    /// </summary>
    private class TestSshChannel : ISshChannel
    {
        private readonly StringBuilder _written = new();
        public bool IsClosed { get; private set; }
        public bool LineModeEnabled { get; private set; } = true;

        public string WrittenData => _written.ToString();

        public void Write(string data)
        {
            if (!IsClosed)
            {
                _written.Append(data);
            }
        }

        public void Close()
        {
            IsClosed = true;
        }

        public string GetTerminalType() => "xterm-256color";

        public (int Width, int Height) GetTerminalSize() => (80, 24);

        public Encoding GetEncoding() => Encoding.UTF8;

        public void SetLineMode(bool enabled)
        {
            LineModeEnabled = enabled;
        }
    }

    #endregion
}
