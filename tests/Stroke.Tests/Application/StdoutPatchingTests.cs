using Stroke.Application;
using Stroke.Output;
using Xunit;

using AppContext = Stroke.Application.AppContext;

namespace Stroke.Tests.Application;

/// <summary>
/// Tests for <see cref="StdoutPatching"/> static class.
/// Covers FR-001, FR-017, FR-020, SC-005.
/// </summary>
public class StdoutPatchingTests
{
    [Fact]
    public void PatchStdout_ReplacesConsoleOut()
    {
        // FR-001, FR-017: Console.Out should change
        using var session = AppContext.CreateAppSession(output: new DummyOutput());
        var originalOut = Console.Out;

        using (StdoutPatching.PatchStdout())
        {
            Assert.NotSame(originalOut, Console.Out);
        }
    }

    [Fact]
    public void PatchStdout_ReplacesConsoleError()
    {
        // FR-017: stderr also redirects
        using var session = AppContext.CreateAppSession(output: new DummyOutput());
        var originalErr = Console.Error;

        using (StdoutPatching.PatchStdout())
        {
            Assert.NotSame(originalErr, Console.Error);
        }
    }

    [Fact]
    public void PatchStdout_StdoutAndStderrBothRedirected()
    {
        // FR-017: both stdout and stderr redirect to the proxy.
        // Console.SetOut/SetError wrap in separate SyncTextWriter instances,
        // so we verify both changed (not that they're same instance).
        using var session = AppContext.CreateAppSession(output: new DummyOutput());
        var originalOut = Console.Out;
        var originalErr = Console.Error;

        using (StdoutPatching.PatchStdout())
        {
            Assert.NotSame(originalOut, Console.Out);
            Assert.NotSame(originalErr, Console.Error);
        }
    }

    [Fact]
    public void Dispose_RestoresOriginalStreams()
    {
        // SC-005: disposal restores original streams
        using var session = AppContext.CreateAppSession(output: new DummyOutput());
        var originalOut = Console.Out;
        var originalErr = Console.Error;

        var patch = StdoutPatching.PatchStdout();
        patch.Dispose();

        Assert.Same(originalOut, Console.Out);
        Assert.Same(originalErr, Console.Error);
    }

    [Fact]
    public void Dispose_IsIdempotent()
    {
        using var session = AppContext.CreateAppSession(output: new DummyOutput());
        var originalOut = Console.Out;

        var patch = StdoutPatching.PatchStdout();
        patch.Dispose();
        patch.Dispose(); // Second dispose should not throw

        Assert.Same(originalOut, Console.Out);
    }

    [Fact]
    public void Nesting_InnerDisposeRestoresOuter()
    {
        // FR-020: nesting support
        using var session = AppContext.CreateAppSession(output: new DummyOutput());
        var originalOut = Console.Out;

        using (StdoutPatching.PatchStdout())
        {
            var outerWriter = Console.Out;

            using (StdoutPatching.PatchStdout())
            {
                // Inner patch should be different from outer
                Assert.NotSame(outerWriter, Console.Out);
            }

            // Inner dispose restores the outer writer
            Assert.Same(outerWriter, Console.Out);
        }

        // Outer dispose restores original
        Assert.Same(originalOut, Console.Out);
    }

    [Fact]
    public void PatchStdout_WithRawTrue_CreatesRawProxy()
    {
        // Verify the underlying proxy has Raw=true by writing an escape
        // and checking it passes through.
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer, enableCpr: false);
        using var session = AppContext.CreateAppSession(output: output);

        using (StdoutPatching.PatchStdout(raw: true))
        {
            Console.Write("\x1b[31mRed\x1b[0m\n");
        }

        var text = writer.ToString();
        // In raw mode, ESC bytes pass through WriteRaw unmodified
        Assert.Contains("\x1b[31m", text);
    }

    [Fact]
    public void PatchStdout_WithRawFalse_EscapesSequences()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer, enableCpr: false);
        using var session = AppContext.CreateAppSession(output: output);

        using (StdoutPatching.PatchStdout(raw: false))
        {
            Console.Write("\x1b[31mRed\x1b[0m\n");
        }

        var text = writer.ToString();
        // In non-raw mode, Write() escapes 0x1B → '?'
        Assert.Contains("?[31mRed?[0m", text);
    }

    [Fact]
    public void PatchStdout_WriteThroughConsole_ReachesOutput()
    {
        var writer = new StringWriter();
        var output = new PlainTextOutput(writer);
        using var session = AppContext.CreateAppSession(output: output);

        using (StdoutPatching.PatchStdout())
        {
            Console.Write("hello from console\n");
        }

        Assert.Contains("hello from console", writer.ToString());
    }
}
