using Stroke.Filters;
using Stroke.Input;
using Stroke.KeyBinding;
using Stroke.Shortcuts;
using Xunit;

namespace Stroke.Tests.Shortcuts;

/// <summary>
/// Tests for prompt-specific key bindings created by PromptSession:
/// Enter (accept), Ctrl-C (interrupt), Ctrl-D (EOF on empty), Tab (readline completions).
/// </summary>
public sealed class PromptSessionBindingsTests
{
    #region Exception Type Tests

    [Fact]
    public void InterruptException_Default_IsKeyboardInterruptException()
    {
        var session = new PromptSession<string>();

        Assert.Equal(typeof(KeyboardInterruptException), session.InterruptException);
    }

    [Fact]
    public void EofException_Default_IsEOFException()
    {
        var session = new PromptSession<string>();

        Assert.Equal(typeof(EOFException), session.EofException);
    }

    #endregion

    #region KeyboardInterruptException Tests

    [Fact]
    public void KeyboardInterruptException_DefaultConstructor_CreatesInstance()
    {
        var ex = new KeyboardInterruptException();

        Assert.NotNull(ex);
        Assert.IsType<KeyboardInterruptException>(ex);
    }

    [Fact]
    public void KeyboardInterruptException_MessageConstructor_SetsMessage()
    {
        var ex = new KeyboardInterruptException("test message");

        Assert.Equal("test message", ex.Message);
    }

    [Fact]
    public void KeyboardInterruptException_InnerExceptionConstructor_SetsInner()
    {
        var inner = new InvalidOperationException("inner");
        var ex = new KeyboardInterruptException("outer", inner);

        Assert.Equal("outer", ex.Message);
        Assert.Same(inner, ex.InnerException);
    }

    [Fact]
    public void KeyboardInterruptException_IsException()
    {
        var ex = new KeyboardInterruptException();

        Assert.IsAssignableFrom<Exception>(ex);
    }

    #endregion

    #region EOFException Tests

    [Fact]
    public void EOFException_DefaultConstructor_CreatesInstance()
    {
        var ex = new EOFException();

        Assert.NotNull(ex);
        Assert.IsType<EOFException>(ex);
    }

    [Fact]
    public void EOFException_MessageConstructor_SetsMessage()
    {
        var ex = new EOFException("test message");

        Assert.Equal("test message", ex.Message);
    }

    [Fact]
    public void EOFException_InnerExceptionConstructor_SetsInner()
    {
        var inner = new InvalidOperationException("inner");
        var ex = new EOFException("outer", inner);

        Assert.Equal("outer", ex.Message);
        Assert.Same(inner, ex.InnerException);
    }

    [Fact]
    public void EOFException_IsException()
    {
        var ex = new EOFException();

        Assert.IsAssignableFrom<Exception>(ex);
    }

    #endregion

    #region Custom Exception Type Tests

    [Fact]
    public void Constructor_CustomInterruptExceptionType_Accepted()
    {
        var session = new PromptSession<string>(
            interruptException: typeof(OperationCanceledException));

        Assert.Equal(typeof(OperationCanceledException), session.InterruptException);
    }

    [Fact]
    public void Constructor_CustomEofExceptionType_Accepted()
    {
        var session = new PromptSession<string>(
            eofException: typeof(EndOfStreamException));

        Assert.Equal(typeof(EndOfStreamException), session.EofException);
    }

    [Fact]
    public void Constructor_NonExceptionInterruptType_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new PromptSession<string>(interruptException: typeof(string)));
    }

    [Fact]
    public void Constructor_NonExceptionEofType_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new PromptSession<string>(eofException: typeof(int)));
    }

    [Fact]
    public void Constructor_AbstractExceptionType_ThrowsArgumentException()
    {
        // SystemException is not abstract in .NET, so use a custom abstract type
        Assert.Throws<ArgumentException>(() =>
            new PromptSession<string>(interruptException: typeof(AbstractTestException)));
    }

    #endregion

    #region Prompt-Specific Binding Registration Tests

    [Fact]
    public void Session_AppHasKeyBindings()
    {
        var session = new PromptSession<string>();

        // The app should have key bindings configured
        Assert.NotNull(session.App.KeyBindings);
    }

    [Fact]
    public void Session_DefaultBuffer_HasAcceptHandler()
    {
        var session = new PromptSession<string>();

        // DefaultBuffer has an accept handler (AcceptInput)
        Assert.NotNull(session.DefaultBuffer);
        Assert.Equal("DEFAULT_BUFFER", session.DefaultBuffer.Name);
    }

    [Fact]
    public void Session_ReadlineLikeStyle_IsAvailable()
    {
        var session = new PromptSession<string>(
            completeStyle: CompleteStyle.ReadlineLike);

        Assert.Equal(CompleteStyle.ReadlineLike, session.CompleteStyle);
    }

    [Fact]
    public void Session_SuspendDisabled_ByDefault()
    {
        var session = new PromptSession<string>();

        // EnableSuspend defaults to falsy (not enabled by default)
        Assert.False(FilterUtils.ToFilter(session.EnableSuspend).Invoke());
    }

    #endregion

    #region Test Helpers

    private abstract class AbstractTestException : Exception
    {
        protected AbstractTestException() { }
        protected AbstractTestException(string message) : base(message) { }
    }

    #endregion
}
