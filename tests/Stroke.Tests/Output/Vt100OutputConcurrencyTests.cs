using Stroke.CursorShapes;
using Stroke.Output;
using Stroke.Styles;
using Xunit;

namespace Stroke.Tests.Output;

/// <summary>
/// Concurrency tests for <see cref="Vt100Output"/> thread safety.
/// </summary>
public sealed class Vt100OutputConcurrencyTests
{
    #region Concurrent Write Tests

    [Fact]
    public void Write_ConcurrentCalls_NoExceptions()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);
        const int threadCount = 100;
        const int iterationsPerThread = 100;

        var threads = new Thread[threadCount];
        var exceptions = new List<Exception>();
        var exceptionLock = new object();

        for (int i = 0; i < threadCount; i++)
        {
            int threadId = i;
            threads[i] = new Thread(() =>
            {
                try
                {
                    for (int j = 0; j < iterationsPerThread; j++)
                    {
                        output.Write($"Thread{threadId}-{j}");
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

        foreach (var thread in threads) thread.Start();
        foreach (var thread in threads) thread.Join();

        Assert.Empty(exceptions);
    }

    [Fact]
    public void WriteRaw_ConcurrentCalls_NoExceptions()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);
        const int threadCount = 100;
        const int iterationsPerThread = 100;

        var threads = new Thread[threadCount];
        var exceptions = new List<Exception>();
        var exceptionLock = new object();

        for (int i = 0; i < threadCount; i++)
        {
            int threadId = i;
            threads[i] = new Thread(() =>
            {
                try
                {
                    for (int j = 0; j < iterationsPerThread; j++)
                    {
                        output.WriteRaw($"\x1b[{threadId};{j}H");
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

        foreach (var thread in threads) thread.Start();
        foreach (var thread in threads) thread.Join();

        Assert.Empty(exceptions);
    }

    #endregion

    #region Concurrent Write and Flush Tests

    [Fact]
    public void WriteAndFlush_Interleaved_NoExceptions()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);
        const int threadCount = 50;
        const int iterationsPerThread = 100;

        var threads = new Thread[threadCount * 2]; // Half writers, half flushers
        var exceptions = new List<Exception>();
        var exceptionLock = new object();

        // Writer threads
        for (int i = 0; i < threadCount; i++)
        {
            int threadId = i;
            threads[i] = new Thread(() =>
            {
                try
                {
                    for (int j = 0; j < iterationsPerThread; j++)
                    {
                        output.Write($"Write{threadId}-{j}");
                        Thread.Yield();
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

        // Flusher threads
        for (int i = 0; i < threadCount; i++)
        {
            threads[threadCount + i] = new Thread(() =>
            {
                try
                {
                    for (int j = 0; j < iterationsPerThread; j++)
                    {
                        output.Flush();
                        Thread.Yield();
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

        foreach (var thread in threads) thread.Start();
        foreach (var thread in threads) thread.Join();

        Assert.Empty(exceptions);
    }

    [Fact]
    public void WriteFlushSequence_ConcurrentThreads_NoExceptions()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);
        const int threadCount = 10;
        const int iterationsPerThread = 50;

        var threads = new Thread[threadCount];
        var exceptions = new List<Exception>();
        var exceptionLock = new object();

        for (int i = 0; i < threadCount; i++)
        {
            int threadId = i;
            threads[i] = new Thread(() =>
            {
                try
                {
                    for (int j = 0; j < iterationsPerThread; j++)
                    {
                        string content = $"[T{threadId}I{j}]";
                        output.Write(content);
                        output.Flush();
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

        foreach (var thread in threads) thread.Start();
        foreach (var thread in threads) thread.Join();
        output.Flush();

        Assert.Empty(exceptions);
        // Verify something was written
        Assert.NotEmpty(writer.ToString());
    }

    #endregion

    #region Concurrent Cursor Operations

    [Fact]
    public void HideCursor_ShowCursor_ConcurrentCalls_NoExceptions()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);
        const int threadCount = 50;
        const int iterationsPerThread = 100;

        var threads = new Thread[threadCount];
        var exceptions = new List<Exception>();
        var exceptionLock = new object();

        for (int i = 0; i < threadCount; i++)
        {
            int threadId = i;
            threads[i] = new Thread(() =>
            {
                try
                {
                    for (int j = 0; j < iterationsPerThread; j++)
                    {
                        if ((threadId + j) % 2 == 0)
                        {
                            output.HideCursor();
                        }
                        else
                        {
                            output.ShowCursor();
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

        foreach (var thread in threads) thread.Start();
        foreach (var thread in threads) thread.Join();

        Assert.Empty(exceptions);
    }

    [Fact]
    public void CursorMovement_ConcurrentCalls_NoExceptions()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);
        const int threadCount = 50;
        const int iterationsPerThread = 100;

        var threads = new Thread[threadCount];
        var exceptions = new List<Exception>();
        var exceptionLock = new object();

        for (int i = 0; i < threadCount; i++)
        {
            int threadId = i;
            threads[i] = new Thread(() =>
            {
                try
                {
                    var rand = new Random(threadId);
                    for (int j = 0; j < iterationsPerThread; j++)
                    {
                        var op = rand.Next(5);
                        switch (op)
                        {
                            case 0:
                                output.CursorGoto(rand.Next(24), rand.Next(80));
                                break;
                            case 1:
                                output.CursorUp(rand.Next(1, 10));
                                break;
                            case 2:
                                output.CursorDown(rand.Next(1, 10));
                                break;
                            case 3:
                                output.CursorForward(rand.Next(1, 10));
                                break;
                            case 4:
                                output.CursorBackward(rand.Next(1, 10));
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

        foreach (var thread in threads) thread.Start();
        foreach (var thread in threads) thread.Join();

        Assert.Empty(exceptions);
    }

    #endregion

    #region Concurrent SetCursorShape

    [Fact]
    public void SetCursorShape_ConcurrentCalls_NoExceptions()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);
        const int threadCount = 50;
        const int iterationsPerThread = 100;

        var shapes = Enum.GetValues<CursorShape>();
        var threads = new Thread[threadCount];
        var exceptions = new List<Exception>();
        var exceptionLock = new object();

        for (int i = 0; i < threadCount; i++)
        {
            int threadId = i;
            threads[i] = new Thread(() =>
            {
                try
                {
                    var rand = new Random(threadId);
                    for (int j = 0; j < iterationsPerThread; j++)
                    {
                        var shape = shapes[rand.Next(shapes.Length)];
                        output.SetCursorShape(shape);
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

        foreach (var thread in threads) thread.Start();
        foreach (var thread in threads) thread.Join();

        Assert.Empty(exceptions);
    }

    #endregion

    #region Concurrent Color Operations

    [Fact]
    public void SetAttributes_ConcurrentCalls_NoExceptions()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);
        const int threadCount = 50;
        const int iterationsPerThread = 100;

        var threads = new Thread[threadCount];
        var exceptions = new List<Exception>();
        var exceptionLock = new object();

        for (int i = 0; i < threadCount; i++)
        {
            int threadId = i;
            threads[i] = new Thread(() =>
            {
                try
                {
                    var rand = new Random(threadId);
                    for (int j = 0; j < iterationsPerThread; j++)
                    {
                        var attrs = new Attrs(
                            Color: $"{rand.Next(256):X2}{rand.Next(256):X2}{rand.Next(256):X2}",
                            BgColor: $"{rand.Next(256):X2}{rand.Next(256):X2}{rand.Next(256):X2}",
                            Bold: rand.Next(2) == 1,
                            Italic: rand.Next(2) == 1);
                        output.SetAttributes(attrs, ColorDepth.Depth24Bit);
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

        foreach (var thread in threads) thread.Start();
        foreach (var thread in threads) thread.Join();

        Assert.Empty(exceptions);
    }

    #endregion

    #region Stress Tests

    [Fact]
    public void MixedOperations_HighConcurrency_NoExceptions()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);
        const int threadCount = 100;
        const int iterationsPerThread = 50;

        var threads = new Thread[threadCount];
        var exceptions = new List<Exception>();
        var exceptionLock = new object();

        for (int i = 0; i < threadCount; i++)
        {
            int threadId = i;
            threads[i] = new Thread(() =>
            {
                try
                {
                    var rand = new Random(threadId);
                    for (int j = 0; j < iterationsPerThread; j++)
                    {
                        var op = rand.Next(10);
                        switch (op)
                        {
                            case 0:
                                output.Write($"T{threadId}");
                                break;
                            case 1:
                                output.WriteRaw($"\x1b[{rand.Next(24)};{rand.Next(80)}H");
                                break;
                            case 2:
                                output.Flush();
                                break;
                            case 3:
                                output.CursorGoto(rand.Next(24), rand.Next(80));
                                break;
                            case 4:
                                output.HideCursor();
                                break;
                            case 5:
                                output.ShowCursor();
                                break;
                            case 6:
                                output.EraseScreen();
                                break;
                            case 7:
                                output.EraseEndOfLine();
                                break;
                            case 8:
                                output.SetAttributes(DefaultAttrs.Default, ColorDepth.Depth24Bit);
                                break;
                            case 9:
                                output.ResetAttributes();
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

        foreach (var thread in threads) thread.Start();
        foreach (var thread in threads) thread.Join();

        Assert.Empty(exceptions);
    }

    #endregion
}
