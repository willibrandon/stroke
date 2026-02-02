using Stroke.Filters;
using Stroke.Shortcuts;
using Xunit;

namespace Stroke.Tests.Shortcuts;

/// <summary>
/// Concurrency tests for PromptSession: verifies Lock-protected property reads/writes
/// from multiple threads, DynCond reads during writes, and no deadlocks under contention.
/// </summary>
public sealed class PromptSessionConcurrencyTests
{
    #region Concurrent Property Read/Write Tests

    [Fact]
    public void ConcurrentReads_Message_NoExceptions()
    {
        var session = new PromptSession<string>(message: "prompt> ");
        var exceptions = new List<Exception>();

        Parallel.For(0, 100, _ =>
        {
            try
            {
                var msg = session.Message;
            }
            catch (Exception ex)
            {
                lock (exceptions)
                {
                    exceptions.Add(ex);
                }
            }
        });

        Assert.Empty(exceptions);
    }

    [Fact]
    public void ConcurrentWrites_Message_NoExceptions()
    {
        var session = new PromptSession<string>();
        var exceptions = new List<Exception>();

        Parallel.For(0, 100, i =>
        {
            try
            {
                session.Message = $"prompt{i}> ";
            }
            catch (Exception ex)
            {
                lock (exceptions)
                {
                    exceptions.Add(ex);
                }
            }
        });

        Assert.Empty(exceptions);
    }

    [Fact]
    public void ConcurrentReadWrite_CompleteStyle_NoExceptions()
    {
        var session = new PromptSession<string>();
        var exceptions = new List<Exception>();
        var styles = new[] { CompleteStyle.Column, CompleteStyle.MultiColumn, CompleteStyle.ReadlineLike };

        Parallel.For(0, 200, i =>
        {
            try
            {
                if (i % 2 == 0)
                {
                    session.CompleteStyle = styles[i % 3];
                }
                else
                {
                    _ = session.CompleteStyle;
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

        Assert.Empty(exceptions);
    }

    [Fact]
    public void ConcurrentReadWrite_FilterOrBool_Properties_NoExceptions()
    {
        var session = new PromptSession<string>();
        var exceptions = new List<Exception>();

        Parallel.For(0, 200, i =>
        {
            try
            {
                if (i % 3 == 0)
                {
                    session.WrapLines = new FilterOrBool(i % 2 == 0);
                }
                else if (i % 3 == 1)
                {
                    _ = session.WrapLines;
                }
                else
                {
                    _ = FilterUtils.ToFilter(session.Multiline).Invoke();
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

        Assert.Empty(exceptions);
    }

    [Fact]
    public void ConcurrentReadWrite_Completer_NoExceptions()
    {
        var session = new PromptSession<string>();
        var exceptions = new List<Exception>();

        Parallel.For(0, 100, i =>
        {
            try
            {
                if (i % 2 == 0)
                {
                    session.Completer = null;
                }
                else
                {
                    _ = session.Completer;
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

        Assert.Empty(exceptions);
    }

    [Fact]
    public void ConcurrentReadWrite_ReserveSpaceForMenu_NoExceptions()
    {
        var session = new PromptSession<string>();
        var exceptions = new List<Exception>();

        Parallel.For(0, 200, i =>
        {
            try
            {
                if (i % 2 == 0)
                {
                    session.ReserveSpaceForMenu = i % 20;
                }
                else
                {
                    _ = session.ReserveSpaceForMenu;
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

        Assert.Empty(exceptions);
    }

    #endregion

    #region DynCond Under Concurrent Write Tests

    [Fact]
    public async Task DynCond_ConcurrentPropertyWrite_WhileReading_NoExceptions()
    {
        var session = new PromptSession<string>();
        var exceptions = new List<Exception>();
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));

        // Writer thread: continuously update IsPassword
        var writer = Task.Run(() =>
        {
            while (!cts.Token.IsCancellationRequested)
            {
                try
                {
                    session.IsPassword = new FilterOrBool(true);
                    session.IsPassword = new FilterOrBool(false);
                }
                catch (Exception ex)
                {
                    lock (exceptions) { exceptions.Add(ex); }
                    break;
                }
            }
        }, TestContext.Current.CancellationToken);

        // Reader thread: continuously read IsPassword via DynCond-style access
        var reader = Task.Run(() =>
        {
            while (!cts.Token.IsCancellationRequested)
            {
                try
                {
                    var _ = FilterUtils.ToFilter(session.IsPassword).Invoke();
                }
                catch (Exception ex)
                {
                    lock (exceptions) { exceptions.Add(ex); }
                    break;
                }
            }
        }, TestContext.Current.CancellationToken);

        await Task.WhenAll(writer, reader);
        Assert.Empty(exceptions);
    }

    [Fact]
    public void ConcurrentReadWrite_MultipleProperties_NoDeadlock()
    {
        var session = new PromptSession<string>();
        var exceptions = new List<Exception>();

        // Access multiple properties concurrently to detect potential deadlocks
        Parallel.For(0, 100, i =>
        {
            try
            {
                // Mix reads and writes across different properties
                switch (i % 6)
                {
                    case 0:
                        session.Message = $"test{i}";
                        break;
                    case 1:
                        _ = session.CompleteStyle;
                        break;
                    case 2:
                        session.WrapLines = new FilterOrBool(i % 2 == 0);
                        break;
                    case 3:
                        _ = session.Completer;
                        break;
                    case 4:
                        session.ReserveSpaceForMenu = i;
                        break;
                    case 5:
                        _ = FilterUtils.ToFilter(session.Multiline).Invoke();
                        break;
                }
            }
            catch (Exception ex)
            {
                lock (exceptions) { exceptions.Add(ex); }
            }
        });

        Assert.Empty(exceptions);
    }

    #endregion
}
