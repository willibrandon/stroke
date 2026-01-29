using Stroke.Core.Primitives;
using Stroke.Layout;
using Xunit;
using SChar = Stroke.Layout.Char;

namespace Stroke.Tests.Layout;

/// <summary>
/// Tests for Screen thread safety (Phase 11).
/// </summary>
public class ScreenThreadSafetyTests
{
    private const int ConcurrentTaskCount = 10;
    private const int IterationsPerTask = 100;

    [Fact]
    public async Task ConcurrentReads_NoException()
    {
        var screen = new Screen();
        screen[5, 5] = SChar.Create("X", "");

        var ct = TestContext.Current.CancellationToken;
        var tasks = Enumerable.Range(0, ConcurrentTaskCount).Select(_ =>
            Task.Run(() =>
            {
                for (int j = 0; j < IterationsPerTask; j++)
                {
                    SChar ch = screen[5, 5];
                }
            }, ct)).ToArray();

        await Task.WhenAll(tasks);
    }

    [Fact]
    public async Task ConcurrentWrites_NoException()
    {
        var screen = new Screen();

        var ct = TestContext.Current.CancellationToken;
        var tasks = Enumerable.Range(0, ConcurrentTaskCount).Select(threadId =>
            Task.Run(() =>
            {
                for (int j = 0; j < IterationsPerTask; j++)
                {
                    screen[threadId, j] = SChar.Create($"{threadId}", "");
                }
            }, ct)).ToArray();

        await Task.WhenAll(tasks);
    }

    [Fact]
    public async Task ConcurrentReadWrite_NoException()
    {
        var screen = new Screen();
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(
            TestContext.Current.CancellationToken);
        cts.CancelAfter(TimeSpan.FromSeconds(2));

        // Writers
        var writers = Enumerable.Range(0, 5).Select(writerId =>
            Task.Run(async () =>
            {
                while (!cts.Token.IsCancellationRequested)
                {
                    for (int j = 0; j < IterationsPerTask && !cts.Token.IsCancellationRequested; j++)
                    {
                        screen[writerId, j] = SChar.Create($"{j}", "");
                    }
                    await Task.Yield();
                }
            }, cts.Token)).ToList();

        // Readers
        var readers = Enumerable.Range(0, 5).Select(_ =>
            Task.Run(async () =>
            {
                while (!cts.Token.IsCancellationRequested)
                {
                    for (int j = 0; j < IterationsPerTask && !cts.Token.IsCancellationRequested; j++)
                    {
                        SChar ch = screen[j % 5, j];
                    }
                    await Task.Yield();
                }
            }, cts.Token)).ToList();

        try
        {
            await Task.WhenAll(writers.Concat(readers));
        }
        catch (OperationCanceledException)
        {
            // Expected when timeout occurs
        }
    }

    [Fact]
    public async Task ConcurrentCursorPositions_NoException()
    {
        var screen = new Screen();
        var windows = Enumerable.Range(0, ConcurrentTaskCount)
            .Select(i => new TestWindow($"window{i}"))
            .ToArray();

        var ct = TestContext.Current.CancellationToken;
        var tasks = windows.Select(window =>
            Task.Run(() =>
            {
                for (int j = 0; j < IterationsPerTask; j++)
                {
                    screen.SetCursorPosition(window, new Point(j, j));
                    _ = screen.GetCursorPosition(window);
                }
            }, ct)).ToArray();

        await Task.WhenAll(tasks);
    }

    [Fact]
    public async Task ConcurrentZeroWidthEscapes_NoException()
    {
        var screen = new Screen();

        var ct = TestContext.Current.CancellationToken;
        var tasks = Enumerable.Range(0, ConcurrentTaskCount).Select(threadId =>
            Task.Run(() =>
            {
                for (int j = 0; j < IterationsPerTask; j++)
                {
                    screen.AddZeroWidthEscape(threadId, j, $"escape{threadId}_{j}");
                    _ = screen.GetZeroWidthEscapes(threadId, j);
                }
            }, ct)).ToArray();

        await Task.WhenAll(tasks);
    }

    [Fact]
    public async Task ConcurrentDrawAllFloats_OnlyOneExecutesAtATime()
    {
        var screen = new Screen();
        var concurrentCalls = 0;
        var maxConcurrent = 0;
        var lockObj = new object();

        for (int i = 0; i < 10; i++)
        {
            screen.DrawWithZIndex(i, () =>
            {
                lock (lockObj)
                {
                    concurrentCalls++;
                    maxConcurrent = Math.Max(maxConcurrent, concurrentCalls);
                }
                Thread.Sleep(10);
                lock (lockObj)
                {
                    concurrentCalls--;
                }
            });
        }

        var ct = TestContext.Current.CancellationToken;
        var tasks = Enumerable.Range(0, 5).Select(_ =>
            Task.Run(() => screen.DrawAllFloats(), ct)).ToArray();

        await Task.WhenAll(tasks);

        // Due to Lock, only one DrawAllFloats should execute at a time
        // and draw functions should execute one at a time
        Assert.True(maxConcurrent <= 1, $"Max concurrent was {maxConcurrent}, expected <= 1");
    }

    [Fact]
    public async Task ConcurrentFillArea_NoException()
    {
        var screen = new Screen();

        var ct = TestContext.Current.CancellationToken;
        var tasks = Enumerable.Range(0, ConcurrentTaskCount).Select(i =>
            Task.Run(() =>
            {
                int row = i * 10;
                var region = new WritePosition(0, row, 80, 5);
                for (int j = 0; j < IterationsPerTask; j++)
                {
                    screen.FillArea(region, $"class:style{j}");
                }
            }, ct)).ToArray();

        await Task.WhenAll(tasks);
    }

    [Fact]
    public async Task ConcurrentClear_NoException()
    {
        var screen = new Screen(initialWidth: 80, initialHeight: 24);

        var ct = TestContext.Current.CancellationToken;
        var tasks = Enumerable.Range(0, ConcurrentTaskCount).Select(_ =>
            Task.Run(() =>
            {
                for (int j = 0; j < IterationsPerTask; j++)
                {
                    screen[j % 24, j % 80] = SChar.Create("X", "");
                    if (j % 10 == 0)
                    {
                        screen.Clear();
                    }
                }
            }, ct)).ToArray();

        await Task.WhenAll(tasks);
    }

    [Fact]
    public async Task Reads_ReturnValidCharInstances()
    {
        var screen = new Screen();
        screen[0, 0] = SChar.Create("A", "class:test");

        var ct = TestContext.Current.CancellationToken;
        var tasks = Enumerable.Range(0, ConcurrentTaskCount).Select(_ =>
            Task.Run(() =>
            {
                for (int j = 0; j < IterationsPerTask; j++)
                {
                    var ch = screen[0, 0];
                    // Verify we got a valid Char instance
                    Assert.NotNull(ch);
                    Assert.NotNull(ch.Character);
                    Assert.NotNull(ch.Style);
                }
            }, ct)).ToArray();

        await Task.WhenAll(tasks);
    }
}
