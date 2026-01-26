using Stroke.Core.Primitives;
using Stroke.Input;
using Stroke.Layout;
using Xunit;

namespace Stroke.Tests.Layout;

/// <summary>
/// Tests for <see cref="MouseHandlers"/> class.
/// </summary>
public class MouseHandlersTests
{
    // T022: Basic tests - set handler, get handler, returns same handler
    [Fact]
    public void SetMouseHandlerForRange_AndGetHandler_ReturnsSameHandler()
    {
        var handlers = new MouseHandlers();
        Func<MouseEvent, NotImplementedOrNone> testHandler = _ => NotImplementedOrNone.None;

        handlers.SetMouseHandlerForRange(0, 10, 0, 10, testHandler);

        var retrieved = handlers.GetHandler(5, 5);
        Assert.NotNull(retrieved);
        Assert.Same(testHandler, retrieved);
    }

    [Fact]
    public void SetMouseHandlerForRange_AllPositionsInRange_ReturnHandler()
    {
        var handlers = new MouseHandlers();
        NotImplementedOrNone TestHandler(MouseEvent e) => NotImplementedOrNone.None;

        handlers.SetMouseHandlerForRange(0, 3, 0, 3, TestHandler);

        // Check all 9 positions (0,0) to (2,2)
        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                var retrieved = handlers.GetHandler(x, y);
                Assert.NotNull(retrieved);
            }
        }
    }

    [Fact]
    public void SetMouseHandlerForRange_PositionAtMaxBound_ReturnsNull()
    {
        var handlers = new MouseHandlers();
        NotImplementedOrNone TestHandler(MouseEvent e) => NotImplementedOrNone.None;

        handlers.SetMouseHandlerForRange(0, 10, 0, 10, TestHandler);

        // Max bounds are exclusive
        Assert.Null(handlers.GetHandler(10, 5));
        Assert.Null(handlers.GetHandler(5, 10));
        Assert.Null(handlers.GetHandler(10, 10));
    }

    // T023: GetHandler returning null when no handler registered
    [Fact]
    public void GetHandler_NoHandlerRegistered_ReturnsNull()
    {
        var handlers = new MouseHandlers();

        var result = handlers.GetHandler(5, 5);

        Assert.Null(result);
    }

    [Fact]
    public void GetHandler_EmptyHandlers_ReturnsNull()
    {
        var handlers = new MouseHandlers();

        Assert.Null(handlers.GetHandler(0, 0));
        Assert.Null(handlers.GetHandler(100, 100));
    }

    // T024: Clear removes all handlers
    [Fact]
    public void Clear_RemovesAllHandlers()
    {
        var handlers = new MouseHandlers();
        NotImplementedOrNone TestHandler(MouseEvent e) => NotImplementedOrNone.None;

        handlers.SetMouseHandlerForRange(0, 10, 0, 10, TestHandler);
        Assert.NotNull(handlers.GetHandler(5, 5));

        handlers.Clear();

        Assert.Null(handlers.GetHandler(5, 5));
        Assert.Null(handlers.GetHandler(0, 0));
        Assert.Null(handlers.GetHandler(9, 9));
    }

    [Fact]
    public void Clear_CanBeCalledMultipleTimes()
    {
        var handlers = new MouseHandlers();
        NotImplementedOrNone TestHandler(MouseEvent e) => NotImplementedOrNone.None;

        handlers.SetMouseHandlerForRange(0, 10, 0, 10, TestHandler);
        handlers.Clear();
        handlers.Clear(); // Second clear should not throw

        Assert.Null(handlers.GetHandler(5, 5));
    }

    // T025: Overlapping regions - newer handler replaces previous
    [Fact]
    public void SetMouseHandlerForRange_OverlappingRegions_NewerHandlerWins()
    {
        var handlers = new MouseHandlers();
        Func<MouseEvent, NotImplementedOrNone> handler1 = _ => NotImplementedOrNone.NotImplemented;
        Func<MouseEvent, NotImplementedOrNone> handler2 = _ => NotImplementedOrNone.None;

        // First handler covers (0,0) to (9,9)
        handlers.SetMouseHandlerForRange(0, 10, 0, 10, handler1);

        // Second handler covers (5,5) to (14,14) - overlaps with first
        handlers.SetMouseHandlerForRange(5, 15, 5, 15, handler2);

        // Non-overlapping part of first handler
        var handler1Only = handlers.GetHandler(2, 2);
        Assert.Same(handler1, handler1Only);

        // Overlapping region - should have second handler
        var overlapHandler = handlers.GetHandler(7, 7);
        Assert.Same(handler2, overlapHandler);

        // Second handler only region
        var handler2Only = handlers.GetHandler(12, 12);
        Assert.Same(handler2, handler2Only);
    }

    // T026: Zero-width/zero-height region (no positions affected)
    [Fact]
    public void SetMouseHandlerForRange_ZeroWidthRegion_NoPositionsAffected()
    {
        var handlers = new MouseHandlers();
        NotImplementedOrNone TestHandler(MouseEvent e) => NotImplementedOrNone.None;

        // xMin == xMax means zero width
        handlers.SetMouseHandlerForRange(5, 5, 0, 10, TestHandler);

        Assert.Null(handlers.GetHandler(5, 5));
    }

    [Fact]
    public void SetMouseHandlerForRange_ZeroHeightRegion_NoPositionsAffected()
    {
        var handlers = new MouseHandlers();
        NotImplementedOrNone TestHandler(MouseEvent e) => NotImplementedOrNone.None;

        // yMin == yMax means zero height
        handlers.SetMouseHandlerForRange(0, 10, 5, 5, TestHandler);

        Assert.Null(handlers.GetHandler(5, 5));
    }

    [Fact]
    public void SetMouseHandlerForRange_NegativeSize_NoPositionsAffected()
    {
        var handlers = new MouseHandlers();
        NotImplementedOrNone TestHandler(MouseEvent e) => NotImplementedOrNone.None;

        // xMin > xMax is invalid
        handlers.SetMouseHandlerForRange(10, 5, 0, 10, TestHandler);
        Assert.Null(handlers.GetHandler(7, 5));

        // yMin > yMax is invalid
        handlers.SetMouseHandlerForRange(0, 10, 10, 5, TestHandler);
        Assert.Null(handlers.GetHandler(5, 7));
    }

    // T027: Out-of-bounds coordinates (returns null, no exception)
    [Fact]
    public void GetHandler_LargePositiveCoordinates_ReturnsNull()
    {
        var handlers = new MouseHandlers();
        NotImplementedOrNone TestHandler(MouseEvent e) => NotImplementedOrNone.None;

        handlers.SetMouseHandlerForRange(0, 10, 0, 10, TestHandler);

        // Far outside registered region
        Assert.Null(handlers.GetHandler(1000, 1000));
        Assert.Null(handlers.GetHandler(int.MaxValue, int.MaxValue));
    }

    // T028: Negative coordinates (returns null, no exception)
    [Fact]
    public void GetHandler_NegativeCoordinates_ReturnsNull()
    {
        var handlers = new MouseHandlers();
        NotImplementedOrNone TestHandler(MouseEvent e) => NotImplementedOrNone.None;

        handlers.SetMouseHandlerForRange(0, 10, 0, 10, TestHandler);

        Assert.Null(handlers.GetHandler(-1, 5));
        Assert.Null(handlers.GetHandler(5, -1));
        Assert.Null(handlers.GetHandler(-1, -1));
        Assert.Null(handlers.GetHandler(int.MinValue, int.MinValue));
    }

    [Fact]
    public void SetMouseHandlerForRange_NegativeCoordinates_CanRegister()
    {
        var handlers = new MouseHandlers();
        NotImplementedOrNone TestHandler(MouseEvent e) => NotImplementedOrNone.None;

        // Negative coordinates can be registered (terminal might use them)
        handlers.SetMouseHandlerForRange(-5, 0, -5, 0, TestHandler);

        Assert.NotNull(handlers.GetHandler(-3, -3));
        Assert.NotNull(handlers.GetHandler(-1, -1));
        Assert.Null(handlers.GetHandler(0, 0)); // Max bound is exclusive
    }

    // T029: Position (0,0) handler works normally
    [Fact]
    public void GetHandler_AtOrigin_WorksNormally()
    {
        var handlers = new MouseHandlers();
        Func<MouseEvent, NotImplementedOrNone> testHandler = _ => NotImplementedOrNone.None;

        handlers.SetMouseHandlerForRange(0, 10, 0, 10, testHandler);

        var handler = handlers.GetHandler(0, 0);
        Assert.NotNull(handler);
        Assert.Same(testHandler, handler);
    }

    [Fact]
    public void SetMouseHandlerForRange_IncludesOrigin_CanRetrieve()
    {
        var handlers = new MouseHandlers();
        NotImplementedOrNone TestHandler(MouseEvent e) => NotImplementedOrNone.None;

        handlers.SetMouseHandlerForRange(0, 1, 0, 1, TestHandler);

        // Only (0,0) should be set
        Assert.NotNull(handlers.GetHandler(0, 0));
        Assert.Null(handlers.GetHandler(0, 1));
        Assert.Null(handlers.GetHandler(1, 0));
        Assert.Null(handlers.GetHandler(1, 1));
    }

    // T030: ArgumentNullException when handler is null
    [Fact]
    public void SetMouseHandlerForRange_NullHandler_ThrowsArgumentNullException()
    {
        var handlers = new MouseHandlers();

        var exception = Assert.Throws<ArgumentNullException>(() =>
            handlers.SetMouseHandlerForRange(0, 10, 0, 10, null!));

        Assert.Equal("handler", exception.ParamName);
    }

    // T041: Integration test - retrieve handler, invoke with MouseEvent, check return value
    [Fact]
    public void MouseHandlers_IntegrationTest_InvokeHandlerWithMouseEvent()
    {
        var handlers = new MouseHandlers();
        MouseEvent? receivedEvent = null;

        NotImplementedOrNone TestHandler(MouseEvent e)
        {
            receivedEvent = e;
            return NotImplementedOrNone.None;
        }

        handlers.SetMouseHandlerForRange(0, 80, 0, 24, TestHandler);

        var mouseEvent = new MouseEvent(
            new Point(10, 5),
            MouseEventType.MouseDown,
            MouseButton.Left,
            MouseModifiers.None);

        var handler = handlers.GetHandler(10, 5);
        Assert.NotNull(handler);

        var result = handler(mouseEvent);

        Assert.Same(NotImplementedOrNone.None, result);
        Assert.NotNull(receivedEvent);
        Assert.Equal(mouseEvent, receivedEvent.Value);
    }

    [Fact]
    public void MouseHandlers_IntegrationTest_HandlerReturnsNotImplemented()
    {
        var handlers = new MouseHandlers();

        NotImplementedOrNone TestHandler(MouseEvent e) => NotImplementedOrNone.NotImplemented;

        handlers.SetMouseHandlerForRange(0, 80, 0, 24, TestHandler);

        var mouseEvent = new MouseEvent(
            new Point(10, 5),
            MouseEventType.MouseDown,
            MouseButton.Left,
            MouseModifiers.None);

        var handler = handlers.GetHandler(10, 5);
        Assert.NotNull(handler);

        var result = handler(mouseEvent);

        Assert.Same(NotImplementedOrNone.NotImplemented, result);
    }

    // Additional edge case tests
    [Fact]
    public void SetMouseHandlerForRange_SingleCell_WorksCorrectly()
    {
        var handlers = new MouseHandlers();
        NotImplementedOrNone TestHandler(MouseEvent e) => NotImplementedOrNone.None;

        handlers.SetMouseHandlerForRange(5, 6, 3, 4, TestHandler);

        Assert.NotNull(handlers.GetHandler(5, 3));
        Assert.Null(handlers.GetHandler(4, 3));
        Assert.Null(handlers.GetHandler(6, 3));
        Assert.Null(handlers.GetHandler(5, 2));
        Assert.Null(handlers.GetHandler(5, 4));
    }

    [Fact]
    public void SetMouseHandlerForRange_MultipleNonOverlappingRegions()
    {
        var handlers = new MouseHandlers();
        Func<MouseEvent, NotImplementedOrNone> handler1 = _ => NotImplementedOrNone.NotImplemented;
        Func<MouseEvent, NotImplementedOrNone> handler2 = _ => NotImplementedOrNone.None;

        handlers.SetMouseHandlerForRange(0, 10, 0, 10, handler1);
        handlers.SetMouseHandlerForRange(20, 30, 20, 30, handler2);

        Assert.Same(handler1, handlers.GetHandler(5, 5));
        Assert.Same(handler2, handlers.GetHandler(25, 25));
        Assert.Null(handlers.GetHandler(15, 15));
    }

    [Fact]
    public void Clear_AfterSetHandler_CanSetNewHandlers()
    {
        var handlers = new MouseHandlers();
        Func<MouseEvent, NotImplementedOrNone> handler1 = _ => NotImplementedOrNone.NotImplemented;
        Func<MouseEvent, NotImplementedOrNone> handler2 = _ => NotImplementedOrNone.None;

        handlers.SetMouseHandlerForRange(0, 10, 0, 10, handler1);
        handlers.Clear();
        handlers.SetMouseHandlerForRange(0, 10, 0, 10, handler2);

        var retrieved = handlers.GetHandler(5, 5);
        Assert.Same(handler2, retrieved);
    }

    // T031: Concurrent stress test (10+ threads, 1000+ operations) per Constitution XI
    [Fact]
    public void MouseHandlers_ConcurrentAccess_IsThreadSafe()
    {
        var handlers = new MouseHandlers();
        const int threadCount = 12;
        const int operationsPerThread = 100;
        var exceptions = new List<Exception>();
        var exceptionLock = new object();

        var threads = new Thread[threadCount];

        for (int i = 0; i < threadCount; i++)
        {
            int threadIndex = i;
            threads[i] = new Thread(() =>
            {
                try
                {
                    var random = new Random(threadIndex * 1000);
                    for (int op = 0; op < operationsPerThread; op++)
                    {
                        NotImplementedOrNone Handler(MouseEvent e) => NotImplementedOrNone.None;

                        int operation = random.Next(4);
                        switch (operation)
                        {
                            case 0: // SetMouseHandlerForRange
                                int xMin = random.Next(0, 50);
                                int xMax = xMin + random.Next(1, 10);
                                int yMin = random.Next(0, 50);
                                int yMax = yMin + random.Next(1, 10);
                                handlers.SetMouseHandlerForRange(xMin, xMax, yMin, yMax, Handler);
                                break;
                            case 1: // GetHandler
                                handlers.GetHandler(random.Next(0, 100), random.Next(0, 100));
                                break;
                            case 2: // Clear
                                handlers.Clear();
                                break;
                            case 3: // SetMouseHandlerForRange then GetHandler
                                handlers.SetMouseHandlerForRange(threadIndex * 10, threadIndex * 10 + 5, 0, 5, Handler);
                                handlers.GetHandler(threadIndex * 10 + 2, 2);
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    lock (exceptionLock)
                    {
                        exceptions.Add(ex);
                    }
                }
            });
        }

        // Start all threads
        foreach (var thread in threads)
        {
            thread.Start();
        }

        // Wait for all threads to complete
        foreach (var thread in threads)
        {
            thread.Join();
        }

        // Verify no exceptions occurred
        Assert.Empty(exceptions);
    }

    [Fact]
    public void MouseHandlers_ConcurrentReads_WhileWriting()
    {
        var handlers = new MouseHandlers();
        NotImplementedOrNone TestHandler(MouseEvent e) => NotImplementedOrNone.None;

        const int iterations = 500;
        var exceptions = new List<Exception>();
        var exceptionLock = new object();

        var writerThread = new Thread(() =>
        {
            try
            {
                for (int i = 0; i < iterations; i++)
                {
                    handlers.SetMouseHandlerForRange(0, 10, 0, 10, TestHandler);
                    handlers.Clear();
                }
            }
            catch (Exception ex)
            {
                lock (exceptionLock)
                {
                    exceptions.Add(ex);
                }
            }
        });

        var readerThreads = new Thread[5];
        for (int i = 0; i < readerThreads.Length; i++)
        {
            readerThreads[i] = new Thread(() =>
            {
                try
                {
                    for (int j = 0; j < iterations; j++)
                    {
                        // These should never throw, just return handler or null
                        handlers.GetHandler(5, 5);
                        handlers.GetHandler(0, 0);
                        handlers.GetHandler(9, 9);
                    }
                }
                catch (Exception ex)
                {
                    lock (exceptionLock)
                    {
                        exceptions.Add(ex);
                    }
                }
            });
        }

        writerThread.Start();
        foreach (var reader in readerThreads)
        {
            reader.Start();
        }

        writerThread.Join();
        foreach (var reader in readerThreads)
        {
            reader.Join();
        }

        Assert.Empty(exceptions);
    }

    // Exception propagation test (FR-014)
    [Fact]
    public void MouseHandlers_HandlerThrowsException_PropagatesToCaller()
    {
        var handlers = new MouseHandlers();
        var expectedException = new InvalidOperationException("Test exception");

        NotImplementedOrNone ThrowingHandler(MouseEvent e)
        {
            throw expectedException;
        }

        handlers.SetMouseHandlerForRange(0, 10, 0, 10, ThrowingHandler);

        var handler = handlers.GetHandler(5, 5);
        Assert.NotNull(handler);

        var mouseEvent = new MouseEvent(
            new Point(5, 5),
            MouseEventType.MouseDown,
            MouseButton.Left,
            MouseModifiers.None);

        var thrown = Assert.Throws<InvalidOperationException>(() => handler(mouseEvent));
        Assert.Same(expectedException, thrown);
    }
}
