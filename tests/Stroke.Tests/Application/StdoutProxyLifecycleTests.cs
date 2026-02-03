using Stroke.Application;
using Stroke.Output;
using Xunit;

using AppContext = Stroke.Application.AppContext;

namespace Stroke.Tests.Application;

/// <summary>
/// Lifecycle tests for <see cref="StdoutProxy"/>.
/// Covers FR-004, FR-011, FR-019, FR-021, SC-004.
/// </summary>
public class StdoutProxyLifecycleTests
{
    [Fact]
    public void Close_FlushesRemainingBuffer()
    {
        var writer = new StringWriter();
        var output = new PlainTextOutput(writer);
        using var session = AppContext.CreateAppSession(output: output);
        var proxy = new StdoutProxy(sleepBetweenWrites: TimeSpan.Zero);

        proxy.Write("buffered without newline");
        proxy.Close();

        Assert.Contains("buffered without newline", writer.ToString());
    }

    [Fact]
    public void Close_TerminatesFlushThreadWithinOneSecond()
    {
        // SC-004: flush thread terminates within 1 second of Close()
        var writer = new StringWriter();
        var output = new PlainTextOutput(writer);
        using var session = AppContext.CreateAppSession(output: output);
        var proxy = new StdoutProxy(sleepBetweenWrites: TimeSpan.Zero);

        proxy.Write("some text\n");

        var start = DateTime.UtcNow;
        proxy.Close();
        var elapsed = DateTime.UtcNow - start;

        Assert.True(elapsed < TimeSpan.FromSeconds(1),
            $"Close() took {elapsed.TotalMilliseconds}ms, expected <1000ms");
    }

    [Fact]
    public void Close_IsIdempotent()
    {
        // FR-011
        var writer = new StringWriter();
        var output = new PlainTextOutput(writer);
        using var session = AppContext.CreateAppSession(output: output);
        var proxy = new StdoutProxy(sleepBetweenWrites: TimeSpan.Zero);

        proxy.Write("text\n");
        proxy.Close();
        proxy.Close(); // Second close should not throw or hang
        proxy.Close(); // Third close should not throw or hang

        Assert.True(proxy.Closed);
    }

    [Fact]
    public void Write_AfterClose_IsSilentlyIgnored()
    {
        // FR-019
        var writer = new StringWriter();
        var output = new PlainTextOutput(writer);
        using var session = AppContext.CreateAppSession(output: output);
        var proxy = new StdoutProxy(sleepBetweenWrites: TimeSpan.Zero);

        proxy.Close();
        var beforeWrite = writer.ToString();

        proxy.Write("should be ignored\n");
        var afterWrite = writer.ToString();

        Assert.Equal(beforeWrite, afterWrite);
    }

    [Fact]
    public void Flush_AfterClose_IsSilentlyIgnored()
    {
        // FR-019
        var writer = new StringWriter();
        var output = new PlainTextOutput(writer);
        using var session = AppContext.CreateAppSession(output: output);
        var proxy = new StdoutProxy(sleepBetweenWrites: TimeSpan.Zero);

        proxy.Close();

        // Should not throw
        proxy.Flush();
    }

    [Fact]
    public void Dispose_CallsClose()
    {
        var writer = new StringWriter();
        var output = new PlainTextOutput(writer);
        using var session = AppContext.CreateAppSession(output: output);
        var proxy = new StdoutProxy(sleepBetweenWrites: TimeSpan.Zero);

        proxy.Write("buffered");
        proxy.Dispose();

        Assert.True(proxy.Closed);
        Assert.Contains("buffered", writer.ToString());
    }

    [Fact]
    public void Write_ReturnsWithoutBlocking()
    {
        // FR-004: Write() is non-blocking (returns before flush thread processes)
        var writer = new StringWriter();
        var output = new PlainTextOutput(writer);
        using var session = AppContext.CreateAppSession(output: output);
        using var proxy = new StdoutProxy(sleepBetweenWrites: TimeSpan.FromMilliseconds(500));

        var start = DateTime.UtcNow;
        proxy.Write("test\n");
        var elapsed = DateTime.UtcNow - start;

        // Write should return almost immediately (< 50ms),
        // even though the sleep between writes is 500ms
        Assert.True(elapsed < TimeSpan.FromMilliseconds(50),
            $"Write() took {elapsed.TotalMilliseconds}ms, expected <50ms (non-blocking)");
    }

    [Fact]
    public void Fileno_DelegatesToOutput()
    {
        // FR-021: Fileno delegates to _output.Fileno()
        // DummyOutput.Fileno() throws NotImplementedException
        using var session = AppContext.CreateAppSession(output: new DummyOutput());
        using var proxy = new StdoutProxy();

        Assert.Throws<NotImplementedException>(() => proxy.Fileno());
    }

    [Fact]
    public void IsAtty_ReturnsFalseWhenNoStdout()
    {
        // FR-021: IsAtty returns false when output has no stdout
        using var session = AppContext.CreateAppSession(output: new DummyOutput());
        using var proxy = new StdoutProxy();

        // DummyOutput.Stdout returns null
        Assert.False(proxy.IsAtty());
    }

    [Fact]
    public void Fileno_WorksAfterClose()
    {
        // FR-021: Fileno does not require proxy to be open
        using var session = AppContext.CreateAppSession(output: new DummyOutput());
        var proxy = new StdoutProxy();
        proxy.Close();

        Assert.Throws<NotImplementedException>(() => proxy.Fileno());
    }

    [Fact]
    public void IsAtty_WorksAfterClose()
    {
        // FR-021: IsAtty does not require proxy to be open
        using var session = AppContext.CreateAppSession(output: new DummyOutput());
        var proxy = new StdoutProxy();
        proxy.Close();

        // Should not throw, just return false
        Assert.False(proxy.IsAtty());
    }
}
