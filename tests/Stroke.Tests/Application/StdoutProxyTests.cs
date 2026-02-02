using System.Text;
using Stroke.Application;
using Stroke.Output;
using Xunit;

using AppContext = Stroke.Application.AppContext;

namespace Stroke.Tests.Application;

/// <summary>
/// Core construction and property tests for <see cref="StdoutProxy"/>.
/// Covers FR-005, FR-013, FR-016.
/// </summary>
public class StdoutProxyTests
{
    [Fact]
    public void Constructor_DefaultParameters_SetsDefaults()
    {
        using var session = AppContext.CreateAppSession(output: new DummyOutput());
        using var proxy = new StdoutProxy();

        Assert.Equal(TimeSpan.FromMilliseconds(200), proxy.SleepBetweenWrites);
        Assert.False(proxy.Raw);
        Assert.False(proxy.Closed);
    }

    [Fact]
    public void Constructor_CustomSleepBetweenWrites_IsStored()
    {
        using var session = AppContext.CreateAppSession(output: new DummyOutput());
        using var proxy = new StdoutProxy(sleepBetweenWrites: TimeSpan.FromMilliseconds(500));

        Assert.Equal(TimeSpan.FromMilliseconds(500), proxy.SleepBetweenWrites);
    }

    [Fact]
    public void Constructor_ZeroSleepBetweenWrites_IsAllowed()
    {
        using var session = AppContext.CreateAppSession(output: new DummyOutput());
        using var proxy = new StdoutProxy(sleepBetweenWrites: TimeSpan.Zero);

        Assert.Equal(TimeSpan.Zero, proxy.SleepBetweenWrites);
    }

    [Fact]
    public void Constructor_NegativeSleepBetweenWrites_Throws()
    {
        using var session = AppContext.CreateAppSession(output: new DummyOutput());

        Assert.Throws<ArgumentOutOfRangeException>(
            () => new StdoutProxy(sleepBetweenWrites: TimeSpan.FromMilliseconds(-1)));
    }

    [Fact]
    public void Constructor_RawTrue_IsStored()
    {
        using var session = AppContext.CreateAppSession(output: new DummyOutput());
        using var proxy = new StdoutProxy(raw: true);

        Assert.True(proxy.Raw);
    }

    [Fact]
    public void Encoding_ReturnsUtf8()
    {
        using var session = AppContext.CreateAppSession(output: new DummyOutput());
        using var proxy = new StdoutProxy();

        Assert.Equal(Encoding.UTF8, proxy.Encoding);
    }

    [Fact]
    public void OriginalStdout_ReturnsOutputStdout()
    {
        var writer = new StringWriter();
        var output = new PlainTextOutput(writer);
        using var session = AppContext.CreateAppSession(output: output);
        using var proxy = new StdoutProxy();

        Assert.Same(writer, proxy.OriginalStdout);
    }

    [Fact]
    public void OriginalStdout_WhenDummyOutput_FallsBackToProcessStdout()
    {
        // DummyOutput.Stdout returns null, so OriginalStdout should fall back
        // to the process-level original stdout (equivalent to sys.__stdout__).
        using var session = AppContext.CreateAppSession(output: new DummyOutput());
        using var proxy = new StdoutProxy();

        Assert.NotNull(proxy.OriginalStdout);
    }

    [Fact]
    public void Errors_ReturnsStrict()
    {
        using var session = AppContext.CreateAppSession(output: new DummyOutput());
        using var proxy = new StdoutProxy();

        Assert.Equal("strict", proxy.Errors);
    }

    [Fact]
    public void Constructor_CapturesAppSessionOutput()
    {
        // Verify FR-013: captures AppSession at construction time
        var writer = new StringWriter();
        var output = new PlainTextOutput(writer);
        using var session = AppContext.CreateAppSession(output: output);
        using var proxy = new StdoutProxy();

        // Write through the proxy, it should reach the captured output
        proxy.Write("hello\n");
        proxy.Flush();
        proxy.Close();

        var text = writer.ToString();
        Assert.Contains("hello", text);
    }
}
