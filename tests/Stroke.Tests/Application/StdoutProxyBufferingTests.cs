using Stroke.Application;
using Stroke.Output;
using Xunit;

using AppContext = Stroke.Application.AppContext;

namespace Stroke.Tests.Application;

/// <summary>
/// Newline-gated buffering tests for <see cref="StdoutProxy"/>.
/// Covers FR-003, FR-008, FR-015, FR-018, FR-023, NFR-001.
/// </summary>
public class StdoutProxyBufferingTests
{
    /// <summary>
    /// Creates a StdoutProxy backed by a PlainTextOutput writing to a StringWriter.
    /// Returns the proxy and writer for assertions.
    /// </summary>
    private static (StdoutProxy proxy, StringWriter writer, AppSession session) CreateProxy(
        TimeSpan? sleep = null)
    {
        var writer = new StringWriter();
        var output = new PlainTextOutput(writer);
        var session = AppContext.CreateAppSession(output: output);
        var proxy = new StdoutProxy(sleepBetweenWrites: sleep ?? TimeSpan.Zero);
        return (proxy, writer, session);
    }

    private static string FlushAndCapture(StdoutProxy proxy, StringWriter writer)
    {
        proxy.Close();
        return writer.ToString();
    }

    [Fact]
    public void Write_WithoutNewline_DoesNotFlushImmediately()
    {
        var (proxy, writer, session) = CreateProxy();
        using var _ = session;

        proxy.Write("partial");

        // Give the flush thread a moment to process
        Thread.Sleep(50);
        Assert.Equal("", writer.ToString());

        proxy.Close();
    }

    [Fact]
    public void Write_WithNewline_FlushesCompleteLine()
    {
        var (proxy, writer, session) = CreateProxy();
        using var _ = session;

        proxy.Write("hello\n");
        var result = FlushAndCapture(proxy, writer);
        Assert.Contains("hello\n", result);
    }

    [Fact]
    public void Write_EmbeddedNewlines_SplitsCorrectly()
    {
        // "line1\nline2\nline3" → "line1\nline2\n" flushed, "line3" buffered
        var (proxy, writer, session) = CreateProxy();
        using var _ = session;

        proxy.Write("line1\nline2\nline3");
        var result = FlushAndCapture(proxy, writer);

        Assert.Contains("line1\nline2\n", result);
        Assert.Contains("line3", result);
    }

    [Fact]
    public void Write_OnlyNewlines_AllFlushed()
    {
        var (proxy, writer, session) = CreateProxy();
        using var _ = session;

        proxy.Write("\n\n\n");
        var result = FlushAndCapture(proxy, writer);
        Assert.Contains("\n\n\n", result);
    }

    [Fact]
    public void Write_CarriageReturn_NotTreatedAsLineTerminator()
    {
        // FR-003: \r is NOT a line terminator
        var (proxy, writer, session) = CreateProxy();
        using var _ = session;

        proxy.Write("before\rafter");

        // Without a \n, nothing should flush
        Thread.Sleep(50);
        Assert.Equal("", writer.ToString());

        var result = FlushAndCapture(proxy, writer);
        Assert.Contains("before\rafter", result);
    }

    [Fact]
    public void Write_WindowsCrLf_BuffersUntilNewline()
    {
        // FR-003: \r\n buffers until the \n
        var (proxy, writer, session) = CreateProxy();
        using var _ = session;

        proxy.Write("text\r\n");
        var result = FlushAndCapture(proxy, writer);
        Assert.Contains("text\r\n", result);
    }

    [Fact]
    public void Write_WhitespaceWithoutNewline_StaysBuffered()
    {
        // FR-003 edge case: whitespace-only without newlines stays buffered
        var (proxy, writer, session) = CreateProxy();
        using var _ = session;

        proxy.Write("   \t  ");

        Thread.Sleep(50);
        Assert.Equal("", writer.ToString());

        var result = FlushAndCapture(proxy, writer);
        Assert.Contains("   \t  ", result);
    }

    [Fact]
    public void Flush_ForcesBufferToQueue()
    {
        // FR-015: Flush forces buffer content to the queue
        var (proxy, writer, session) = CreateProxy();
        using var _ = session;

        proxy.Write("no newline here");
        proxy.Flush();
        var result = FlushAndCapture(proxy, writer);
        Assert.Contains("no newline here", result);
    }

    [Fact]
    public void Write_Null_IsSilentlyIgnored()
    {
        // FR-018
        var (proxy, writer, session) = CreateProxy();
        using var _ = session;

        proxy.Write((string?)null);
        var result = FlushAndCapture(proxy, writer);
        Assert.Equal("", result);
    }

    [Fact]
    public void Write_EmptyString_IsSilentlyIgnored()
    {
        // FR-018
        var (proxy, writer, session) = CreateProxy();
        using var _ = session;

        proxy.Write("");
        var result = FlushAndCapture(proxy, writer);
        Assert.Equal("", result);
    }

    [Fact]
    public void Write_Char_DelegatesToWriteString()
    {
        // FR-023
        var (proxy, writer, session) = CreateProxy();
        using var _ = session;

        proxy.Write('X');
        proxy.Write('\n');
        var result = FlushAndCapture(proxy, writer);
        Assert.Contains("X\n", result);
    }

    [Fact]
    public void Write_Char_ParticipatesInNewlineGatedBuffering()
    {
        // FR-023: char without newline stays buffered
        var (proxy, writer, session) = CreateProxy();
        using var _ = session;

        proxy.Write('A');
        proxy.Write('B');

        Thread.Sleep(50);
        Assert.Equal("", writer.ToString());

        proxy.Write('\n');
        var result = FlushAndCapture(proxy, writer);
        Assert.Contains("AB\n", result);
    }

    [Fact]
    public void Write_DirectOutputWhenNoApp_WorksCorrectly()
    {
        // FR-008: when no app is running, writes directly to IOutput
        var (proxy, writer, session) = CreateProxy();
        using var _ = session;

        // No AppContext.SetApp() called — no app running
        proxy.Write("direct output\n");
        var result = FlushAndCapture(proxy, writer);
        Assert.Contains("direct output", result);
    }

    [Fact]
    public void Write_VeryLargeString_ProcessedWithoutError()
    {
        // NFR-001: no maximum size limit
        var (proxy, writer, session) = CreateProxy();
        using var _ = session;

        var largeString = new string('X', 1_048_576) + "\n"; // 1MB+
        proxy.Write(largeString);
        var result = FlushAndCapture(proxy, writer);
        Assert.Contains(new string('X', 100), result); // Spot check
        Assert.True(result.Length >= 1_048_576);
    }
}
