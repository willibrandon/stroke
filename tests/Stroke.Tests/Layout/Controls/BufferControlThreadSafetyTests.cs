using Stroke.Core;
using Stroke.Core.Primitives;
using Stroke.Input;
using Stroke.Layout.Controls;
using Xunit;

using Buffer = Stroke.Core.Buffer;

namespace Stroke.Tests.Layout.Controls;

/// <summary>
/// Tests for BufferControl thread safety.
/// </summary>
public sealed class BufferControlThreadSafetyTests
{
    #region Concurrent CreateContent Tests

    [Fact]
    public async Task CreateContent_ConcurrentCalls_NoExceptions()
    {
        var buffer = new Buffer();
        buffer.Text = "Hello World\nLine 2\nLine 3";
        var control = new BufferControl(buffer: buffer);

        var ct = TestContext.Current.CancellationToken;
        var tasks = Enumerable.Range(0, 10)
            .Select(_ => Task.Run(() =>
            {
                for (int i = 0; i < 100; i++)
                {
                    var content = control.CreateContent(80, 24);
                    Assert.NotNull(content);
                    Assert.True(content.LineCount > 0);
                }
            }, ct))
            .ToArray();

        await Task.WhenAll(tasks);
    }

    [Fact]
    public async Task CreateContent_ConcurrentWithBufferModification_NoExceptions()
    {
        var buffer = new Buffer();
        buffer.Text = "Initial text";
        var control = new BufferControl(buffer: buffer);

        var ct = TestContext.Current.CancellationToken;
        var completed = false;

        // Task that continuously reads content
        var readTask = Task.Run(async () =>
        {
            while (!completed && !ct.IsCancellationRequested)
            {
                var content = control.CreateContent(80, 24);
                Assert.NotNull(content);
                await Task.Yield();
            }
        }, ct);

        // Task that modifies the buffer
        var writeTask = Task.Run(async () =>
        {
            for (int i = 0; i < 50; i++)
            {
                buffer.Text = $"Modified text iteration {i}";
                await Task.Delay(1, ct);
            }
            completed = true;
        }, ct);

        await writeTask;
        await readTask;
    }

    #endregion

    #region Concurrent MouseHandler Tests

    [Fact]
    public async Task MouseHandler_ConcurrentClicks_NoExceptions()
    {
        var buffer = new Buffer();
        buffer.Text = "Hello World with some text";
        var control = new BufferControl(buffer: buffer);

        var ct = TestContext.Current.CancellationToken;
        var tasks = Enumerable.Range(0, 5)
            .Select(t => Task.Run(() =>
            {
                for (int i = 0; i < 50; i++)
                {
                    var mouseEvent = new MouseEvent(
                        new Point(i % 20, 0),
                        MouseEventType.MouseUp,
                        MouseButton.Left,
                        MouseModifiers.None);

                    control.MouseHandler(mouseEvent);
                }
            }, ct))
            .ToArray();

        await Task.WhenAll(tasks);
    }

    [Fact]
    public async Task MouseHandler_ConcurrentDoubleClicks_NoExceptions()
    {
        var buffer = new Buffer();
        buffer.Text = "one two three four five";
        var control = new BufferControl(buffer: buffer);

        var ct = TestContext.Current.CancellationToken;
        var tasks = Enumerable.Range(0, 5)
            .Select(t => Task.Run(() =>
            {
                for (int i = 0; i < 20; i++)
                {
                    // Simulate double-click
                    var position = new Point((t * 4) % 20, 0);

                    var click1 = new MouseEvent(
                        position,
                        MouseEventType.MouseUp,
                        MouseButton.Left,
                        MouseModifiers.None);
                    control.MouseHandler(click1);

                    var click2 = new MouseEvent(
                        position,
                        MouseEventType.MouseUp,
                        MouseButton.Left,
                        MouseModifiers.None);
                    control.MouseHandler(click2);
                }
            }, ct))
            .ToArray();

        await Task.WhenAll(tasks);
    }

    #endregion

    #region Concurrent MoveCursor Tests

    [Fact]
    public async Task MoveCursor_ConcurrentCalls_NoExceptions()
    {
        var buffer = new Buffer();
        buffer.Text = "Line 1\nLine 2\nLine 3\nLine 4\nLine 5";
        var control = new BufferControl(buffer: buffer);

        var ct = TestContext.Current.CancellationToken;
        var tasks = new List<Task>
        {
            Task.Run(() =>
            {
                for (int i = 0; i < 100; i++)
                {
                    control.MoveCursorDown();
                }
            }, ct),
            Task.Run(() =>
            {
                for (int i = 0; i < 100; i++)
                {
                    control.MoveCursorUp();
                }
            }, ct),
            Task.Run(() =>
            {
                for (int i = 0; i < 100; i++)
                {
                    control.MoveCursorDown();
                    control.MoveCursorUp();
                }
            }, ct)
        };

        await Task.WhenAll(tasks);
    }

    #endregion

    #region Mixed Concurrent Operations Tests

    [Fact]
    public async Task AllOperations_ConcurrentExecution_NoExceptions()
    {
        var buffer = new Buffer();
        buffer.Text = "Line 1\nLine 2\nLine 3";
        var control = new BufferControl(buffer: buffer);

        var ct = TestContext.Current.CancellationToken;
        var completed = false;

        var tasks = new List<Task>
        {
            // CreateContent task
            Task.Run(async () =>
            {
                while (!completed && !ct.IsCancellationRequested)
                {
                    control.CreateContent(80, 24);
                    await Task.Yield();
                }
            }, ct),
            // MouseHandler task
            Task.Run(async () =>
            {
                var rng = new Random(42);
                while (!completed && !ct.IsCancellationRequested)
                {
                    var mouseEvent = new MouseEvent(
                        new Point(rng.Next(0, 10), rng.Next(0, 3)),
                        MouseEventType.MouseUp,
                        MouseButton.Left,
                        MouseModifiers.None);
                    control.MouseHandler(mouseEvent);
                    await Task.Yield();
                }
            }, ct),
            // MoveCursor task
            Task.Run(async () =>
            {
                while (!completed && !ct.IsCancellationRequested)
                {
                    control.MoveCursorDown();
                    control.MoveCursorUp();
                    await Task.Yield();
                }
            }, ct),
            // Timer task to stop after 2 seconds
            Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(2), ct);
                completed = true;
            }, ct)
        };

        await Task.WhenAll(tasks);
    }

    #endregion

    #region Cache Thread Safety Tests

    [Fact]
    public async Task FragmentCache_ConcurrentAccess_NoCorruption()
    {
        var buffer = new Buffer();
        var control = new BufferControl(buffer: buffer);

        var texts = Enumerable.Range(0, 10)
            .Select(i => $"Text content {i}\nWith multiple\nlines")
            .ToList();

        var ct = TestContext.Current.CancellationToken;
        var tasks = texts.Select(text => Task.Run(() =>
        {
            for (int i = 0; i < 50; i++)
            {
                buffer.Text = text;
                var content = control.CreateContent(80, 24);

                // Verify content is valid
                for (int line = 0; line < content.LineCount; line++)
                {
                    var fragments = content.GetLine(line);
                    Assert.NotNull(fragments);
                }
            }
        }, ct)).ToArray();

        await Task.WhenAll(tasks);
    }

    #endregion
}
