namespace Stroke.Tests.Core;

using Stroke.Core;
using Xunit;

/// <summary>
/// Tests for <see cref="Event{TSender}"/> class.
/// </summary>
public class EventTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_WithSenderOnly_StoresSender()
    {
        var sender = new TestSender();
        var evt = new Event<TestSender>(sender);

        Assert.Same(sender, evt.Sender);
    }

    [Fact]
    public void Constructor_WithSenderAndHandler_AddsHandler()
    {
        var sender = new TestSender();
        var callCount = 0;

        var evt = new Event<TestSender>(sender, s => callCount++);
        evt.Fire();

        Assert.Equal(1, callCount);
    }

    [Fact]
    public void Constructor_WithNullHandler_DoesNotThrow()
    {
        var sender = new TestSender();
        var exception = Record.Exception(() => new Event<TestSender>(sender, null));

        Assert.Null(exception);
    }

    #endregion

    #region AddHandler Tests

    [Fact]
    public void AddHandler_AddsHandlerToList()
    {
        var sender = new TestSender();
        var evt = new Event<TestSender>(sender);
        var callCount = 0;

        evt.AddHandler(s => callCount++);
        evt.Fire();

        Assert.Equal(1, callCount);
    }

    [Fact]
    public void AddHandler_WithNullHandler_ThrowsArgumentNullException()
    {
        var sender = new TestSender();
        var evt = new Event<TestSender>(sender);

        Assert.Throws<ArgumentNullException>(() => evt.AddHandler(null!));
    }

    #endregion

    #region RemoveHandler Tests

    [Fact]
    public void RemoveHandler_RemovesHandlerFromList()
    {
        var sender = new TestSender();
        var evt = new Event<TestSender>(sender);
        var callCount = 0;
        Action<TestSender> handler = s => callCount++;

        evt.AddHandler(handler);
        evt.RemoveHandler(handler);
        evt.Fire();

        Assert.Equal(0, callCount);
    }

    [Fact]
    public void RemoveHandler_NonExistentHandler_SilentlyIgnored()
    {
        var sender = new TestSender();
        var evt = new Event<TestSender>(sender);
        Action<TestSender> handler = s => { };

        var exception = Record.Exception(() => evt.RemoveHandler(handler));

        Assert.Null(exception);
    }

    [Fact]
    public void RemoveHandler_NullHandler_SilentlyIgnored()
    {
        var sender = new TestSender();
        var evt = new Event<TestSender>(sender);

        var exception = Record.Exception(() => evt.RemoveHandler(null!));

        Assert.Null(exception);
    }

    [Fact]
    public void RemoveHandler_HandlerAddedMultipleTimes_RemovesFirstOccurrence()
    {
        var sender = new TestSender();
        var evt = new Event<TestSender>(sender);
        var callCount = 0;
        Action<TestSender> handler = s => callCount++;

        evt.AddHandler(handler);
        evt.AddHandler(handler);
        evt.RemoveHandler(handler);
        evt.Fire();

        Assert.Equal(1, callCount); // Still called once (second occurrence remains)
    }

    #endregion

    #region Fire Tests

    [Fact]
    public void Fire_CallsHandlersInOrderAdded()
    {
        var sender = new TestSender();
        var evt = new Event<TestSender>(sender);
        var order = new List<int>();

        evt.AddHandler(s => order.Add(1));
        evt.AddHandler(s => order.Add(2));
        evt.AddHandler(s => order.Add(3));
        evt.Fire();

        Assert.Equal([1, 2, 3], order);
    }

    [Fact]
    public void Fire_PassesSenderToEachHandler()
    {
        var sender = new TestSender();
        var evt = new Event<TestSender>(sender);
        TestSender? receivedSender = null;

        evt.AddHandler(s => receivedSender = s);
        evt.Fire();

        Assert.Same(sender, receivedSender);
    }

    [Fact]
    public void Fire_WithZeroHandlers_CompletesSuccessfully()
    {
        var sender = new TestSender();
        var evt = new Event<TestSender>(sender);

        var exception = Record.Exception(() => evt.Fire());

        Assert.Null(exception);
    }

    [Fact]
    public void Fire_HandlerThrowsException_PropagatesAndStopsFurtherHandlers()
    {
        var sender = new TestSender();
        var evt = new Event<TestSender>(sender);
        var secondHandlerCalled = false;

        evt.AddHandler(s => throw new InvalidOperationException("Test exception"));
        evt.AddHandler(s => secondHandlerCalled = true);

        Assert.Throws<InvalidOperationException>(() => evt.Fire());
        Assert.False(secondHandlerCalled);
    }

    #endregion

    #region Operator Tests

    [Fact]
    public void OperatorPlus_AddsHandler()
    {
        var sender = new TestSender();
        var evt = new Event<TestSender>(sender);
        var callCount = 0;

        evt += s => callCount++;
        evt.Fire();

        Assert.Equal(1, callCount);
    }

    [Fact]
    public void OperatorPlus_ReturnsSameInstance()
    {
        var sender = new TestSender();
        var evt = new Event<TestSender>(sender);

        var result = evt + (s => { });

        Assert.Same(evt, result);
    }

    [Fact]
    public void OperatorPlus_WithNullHandler_ThrowsArgumentNullException()
    {
        var sender = new TestSender();
        var evt = new Event<TestSender>(sender);

        Assert.Throws<ArgumentNullException>(() => evt + null!);
    }

    [Fact]
    public void OperatorMinus_RemovesHandler()
    {
        var sender = new TestSender();
        var evt = new Event<TestSender>(sender);
        var callCount = 0;
        Action<TestSender> handler = s => callCount++;

        evt += handler;
        evt -= handler;
        evt.Fire();

        Assert.Equal(0, callCount);
    }

    [Fact]
    public void OperatorMinus_ReturnsSameInstance()
    {
        var sender = new TestSender();
        var evt = new Event<TestSender>(sender);
        Action<TestSender> handler = s => { };

        evt += handler;
        var result = evt - handler;

        Assert.Same(evt, result);
    }

    #endregion

    #region Same Handler Multiple Times Tests

    [Fact]
    public void SameHandlerAddedTwice_CalledTwice()
    {
        var sender = new TestSender();
        var evt = new Event<TestSender>(sender);
        var callCount = 0;
        Action<TestSender> handler = s => callCount++;

        evt.AddHandler(handler);
        evt.AddHandler(handler);
        evt.Fire();

        Assert.Equal(2, callCount);
    }

    #endregion

    #region Modification During Iteration Tests

    [Fact]
    public void Fire_HandlerRemovesItself_RemovalDeferredToNextFire()
    {
        var sender = new TestSender();
        var evt = new Event<TestSender>(sender);
        var callCount = 0;
        Action<TestSender>? handler = null;
        handler = s =>
        {
            callCount++;
            evt.RemoveHandler(handler!);
        };

        evt.AddHandler(handler);
        evt.Fire(); // Handler runs and removes itself
        Assert.Equal(1, callCount);

        evt.Fire(); // Handler should not run (already removed)
        Assert.Equal(1, callCount);
    }

    [Fact]
    public void Fire_HandlerAddsNewHandler_NewHandlerCalledOnNextFire()
    {
        var sender = new TestSender();
        var evt = new Event<TestSender>(sender);
        var secondHandlerCalled = false;
        var firstCallCount = 0;

        evt.AddHandler(s =>
        {
            firstCallCount++;
            if (firstCallCount == 1)
            {
                evt.AddHandler(s2 => secondHandlerCalled = true);
            }
        });

        evt.Fire(); // First handler runs and adds second handler
        Assert.False(secondHandlerCalled); // Second handler not called during this Fire

        evt.Fire(); // Now second handler should be called
        Assert.True(secondHandlerCalled);
    }

    #endregion

    #region Value Type Sender Tests

    [Fact]
    public void Constructor_WithValueTypeSender_StoresValue()
    {
        var evt = new Event<int>(42);

        Assert.Equal(42, evt.Sender);
    }

    [Fact]
    public void Fire_WithValueTypeSender_PassesValueToHandler()
    {
        var evt = new Event<int>(42);
        var receivedValue = 0;

        evt.AddHandler(s => receivedValue = s);
        evt.Fire();

        Assert.Equal(42, receivedValue);
    }

    #endregion

    #region Test Helper

    private sealed class TestSender
    {
        public string Name { get; set; } = "Test";
    }

    #endregion
}
