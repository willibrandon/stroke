using Stroke.Application;
using Stroke.Core;
using Stroke.Input.Pipe;
using Stroke.Layout.Containers;
using Stroke.Layout.Controls;
using Stroke.Layout.Menus;
using Stroke.Output;
using Xunit;

using AppContext = Stroke.Application.AppContext;
using Buffer = Stroke.Core.Buffer;
using CompletionItem = Stroke.Completion.Completion;
using StrokeLayout = Stroke.Layout.Layout;

namespace Stroke.Tests.Layout.Menus;

/// <summary>
/// Thread safety tests for menu controls with mutable state.
/// MultiColumnCompletionMenuControl is the primary target since it
/// uses Lock for its mutable render state.
/// </summary>
public sealed class MenuThreadSafetyTests
{
    private static (Application<object?> app, IDisposable scope, Buffer buffer) CreateAppWithCompletions(
        int count = 20)
    {
        var buffer = new Buffer(document: new Document("hel", cursorPosition: 3));
        var control = new BufferControl(buffer: buffer);
        var window = new Window(content: control);
        var layout = new StrokeLayout(new AnyContainer(window));
        var input = new SimplePipeInput();
        var output = new DummyOutput();
        var app = new Application<object?>(layout: layout, input: input, output: output);
        var scope = AppContext.SetApp(app);

        var completions = new List<CompletionItem>();
        for (int i = 0; i < count; i++)
        {
            completions.Add(new CompletionItem($"item{i:D2}", startPosition: -3));
        }
        buffer.SetCompletions(completions);
        buffer.GoToCompletion(0);

        return (app, scope, buffer);
    }

    [Fact]
    public void MultiColumnControl_ConcurrentCreateContent_NoExceptions()
    {
        var (_, scope, _) = CreateAppWithCompletions(30);
        using (scope)
        {
            var control = new MultiColumnCompletionMenuControl();

            var exceptions = new List<Exception>();
            var threads = new Thread[4];
            for (int t = 0; t < threads.Length; t++)
            {
                threads[t] = new Thread(() =>
                {
                    try
                    {
                        for (int i = 0; i < 50; i++)
                        {
                            control.CreateContent(80, 5);
                        }
                    }
                    catch (Exception ex)
                    {
                        lock (exceptions)
                        {
                            exceptions.Add(ex);
                        }
                    }
                });
            }

            foreach (var t in threads)
                t.Start();
            foreach (var t in threads)
                t.Join();

            Assert.Empty(exceptions);
        }
    }

    [Fact]
    public void MultiColumnControl_ConcurrentPreferredWidth_NoExceptions()
    {
        var (_, scope, _) = CreateAppWithCompletions(20);
        using (scope)
        {
            var control = new MultiColumnCompletionMenuControl();

            var exceptions = new List<Exception>();
            var threads = new Thread[4];
            for (int t = 0; t < threads.Length; t++)
            {
                threads[t] = new Thread(() =>
                {
                    try
                    {
                        for (int i = 0; i < 50; i++)
                        {
                            control.PreferredWidth(200);
                        }
                    }
                    catch (Exception ex)
                    {
                        lock (exceptions)
                        {
                            exceptions.Add(ex);
                        }
                    }
                });
            }

            foreach (var t in threads)
                t.Start();
            foreach (var t in threads)
                t.Join();

            Assert.Empty(exceptions);
        }
    }

    [Fact]
    public void MultiColumnControl_ConcurrentReset_NoExceptions()
    {
        var control = new MultiColumnCompletionMenuControl();

        var exceptions = new List<Exception>();
        var threads = new Thread[4];
        for (int t = 0; t < threads.Length; t++)
        {
            threads[t] = new Thread(() =>
            {
                try
                {
                    for (int i = 0; i < 100; i++)
                    {
                        control.Reset();
                    }
                }
                catch (Exception ex)
                {
                    lock (exceptions)
                    {
                        exceptions.Add(ex);
                    }
                }
            });
        }

        foreach (var t in threads)
            t.Start();
        foreach (var t in threads)
            t.Join();

        Assert.Empty(exceptions);
    }

    [Fact]
    public void MultiColumnControl_ConcurrentCreateContentAndReset_NoExceptions()
    {
        var (_, scope, _) = CreateAppWithCompletions(30);
        using (scope)
        {
            var control = new MultiColumnCompletionMenuControl();

            var exceptions = new List<Exception>();
            var threads = new Thread[4];

            // Two threads doing CreateContent, two doing Reset
            for (int t = 0; t < 2; t++)
            {
                threads[t] = new Thread(() =>
                {
                    try
                    {
                        for (int i = 0; i < 50; i++)
                        {
                            control.CreateContent(80, 5);
                        }
                    }
                    catch (Exception ex)
                    {
                        lock (exceptions)
                        {
                            exceptions.Add(ex);
                        }
                    }
                });
            }
            for (int t = 2; t < 4; t++)
            {
                threads[t] = new Thread(() =>
                {
                    try
                    {
                        for (int i = 0; i < 50; i++)
                        {
                            control.Reset();
                        }
                    }
                    catch (Exception ex)
                    {
                        lock (exceptions)
                        {
                            exceptions.Add(ex);
                        }
                    }
                });
            }

            foreach (var t in threads)
                t.Start();
            foreach (var t in threads)
                t.Join();

            Assert.Empty(exceptions);
        }
    }
}
