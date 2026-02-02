using Stroke.Application;
using Stroke.Output;
using Xunit;

using AppContext = Stroke.Application.AppContext;

namespace Stroke.Tests.Application;

/// <summary>
/// Raw vs non-raw VT100 routing tests for <see cref="StdoutProxy"/>.
/// Covers FR-006, FR-014.
/// </summary>
public class StdoutProxyRawModeTests
{
    [Fact]
    public void RawFalse_EscapesVt100Sequences()
    {
        // FR-006: raw=false routes through IOutput.Write() which escapes 0x1B → '?'
        var writer = new StringWriter();
        // Use Vt100Output which actually escapes ESC bytes in Write()
        var output = Vt100Output.FromPty(writer, enableCpr: false);
        using var session = AppContext.CreateAppSession(output: output);
        using var proxy = new StdoutProxy(sleepBetweenWrites: TimeSpan.Zero, raw: false);

        proxy.Write("\x1b[31mRed\x1b[0m\n");
        proxy.Close();

        var text = writer.ToString();
        // Vt100Output.Write() replaces 0x1B with '?' in the user data.
        Assert.Contains("?[31mRed?[0m", text);
    }

    [Fact]
    public void RawTrue_PassesThroughVt100Sequences()
    {
        // FR-006: raw=true routes through IOutput.WriteRaw() (no escaping)
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer, enableCpr: false);
        using var session = AppContext.CreateAppSession(output: output);
        using var proxy = new StdoutProxy(sleepBetweenWrites: TimeSpan.Zero, raw: true);

        proxy.Write("\x1b[31mRed\x1b[0m\n");
        proxy.Close();

        var text = writer.ToString();
        // WriteRaw passes through ESC bytes unmodified
        Assert.Contains("\x1b[31mRed\x1b[0m", text);
    }

    [Fact]
    public void RawMode_IsImmutableAfterConstruction()
    {
        using var session = AppContext.CreateAppSession(output: new DummyOutput());
        using var proxyRaw = new StdoutProxy(raw: true);
        using var proxyDefault = new StdoutProxy(raw: false);

        Assert.True(proxyRaw.Raw);
        Assert.False(proxyDefault.Raw);
    }

    [Fact]
    public void EnableAutowrap_CalledBeforeEachWrite()
    {
        // FR-014: EnableAutowrap() called before each write in both modes.
        // Vt100Output.EnableAutowrap() writes "\x1b[?7h" to the buffer.
        // In raw mode, this will appear in the output.
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer, enableCpr: false);
        using var session = AppContext.CreateAppSession(output: output);
        using var proxy = new StdoutProxy(sleepBetweenWrites: TimeSpan.Zero, raw: true);

        proxy.Write("test\n");
        proxy.Close();

        var text = writer.ToString();
        // The EnableAutowrap sequence should be present
        Assert.Contains("\x1b[?7h", text);
    }
}
