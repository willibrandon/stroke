using Stroke.Output;
using Xunit;

namespace Stroke.Tests.Output;

/// <summary>
/// Tests for synchronized output (DEC Mode 2026) behavior in <see cref="Vt100Output"/>.
/// Verifies that BeginSynchronizedOutput/EndSynchronizedOutput control the flag,
/// and that Flush wraps output in Mode 2026 markers when the flag is active.
/// </summary>
public sealed class Vt100OutputSynchronizedOutputTests
{
    #region Begin/End Flag Behavior

    [Fact]
    public void BeginSynchronizedOutput_SetsFlag_FlushWrapsInMarkers()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        output.BeginSynchronizedOutput();
        output.WriteRaw("content");
        output.Flush();

        var result = writer.ToString();
        Assert.StartsWith("\x1b[?2026h", result);
        Assert.EndsWith("\x1b[?2026l", result);
        Assert.Contains("content", result);
    }

    [Fact]
    public void EndSynchronizedOutput_ClearsFlag_FlushEmitsNoMarkers()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        output.BeginSynchronizedOutput();
        output.EndSynchronizedOutput();
        output.WriteRaw("content");
        output.Flush();

        var result = writer.ToString();
        Assert.Equal("content", result);
        Assert.DoesNotContain("\x1b[?2026h", result);
        Assert.DoesNotContain("\x1b[?2026l", result);
    }

    [Fact]
    public void Flush_EmptyBuffer_NoMarkersEvenWhenFlagSet()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        output.BeginSynchronizedOutput();
        output.Flush();

        Assert.Equal("", writer.ToString());
    }

    #endregion

    #region Idempotency

    [Fact]
    public void BeginSynchronizedOutput_MultipleBegins_FlagRemainsTrue()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        output.BeginSynchronizedOutput();
        output.BeginSynchronizedOutput();
        output.BeginSynchronizedOutput();
        output.WriteRaw("content");
        output.Flush();

        var result = writer.ToString();
        Assert.StartsWith("\x1b[?2026h", result);
        Assert.EndsWith("\x1b[?2026l", result);
    }

    [Fact]
    public void EndSynchronizedOutput_MultipleEnds_FlagRemainsFalse()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        output.EndSynchronizedOutput();
        output.EndSynchronizedOutput();
        output.EndSynchronizedOutput();
        output.WriteRaw("content");
        output.Flush();

        var result = writer.ToString();
        Assert.Equal("content", result);
    }

    #endregion

    #region Marker Order and Content

    [Fact]
    public void Flush_MarkersWrapContent_CorrectOrder()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        output.BeginSynchronizedOutput();
        output.WriteRaw("ABC");
        output.Flush();

        var result = writer.ToString();
        Assert.Equal("\x1b[?2026hABC\x1b[?2026l", result);
    }

    [Fact]
    public void Flush_MarkerOverhead_Exactly16Bytes()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        output.BeginSynchronizedOutput();
        output.WriteRaw("X");
        output.Flush();

        var result = writer.ToString();
        // Total = begin(8) + "X"(1) + end(8) = 17
        // Overhead = total - content = 16
        Assert.Equal(17, result.Length);
        Assert.Equal(8, "\x1b[?2026h".Length);
        Assert.Equal(8, "\x1b[?2026l".Length);
    }

    [Fact]
    public void Flush_MultipleBufferedWrites_AllWrappedInSingleMarkerPair()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        output.BeginSynchronizedOutput();
        output.WriteRaw("first");
        output.WriteRaw("second");
        output.WriteRaw("third");
        output.Flush();

        var result = writer.ToString();
        Assert.Equal("\x1b[?2026hfirstsecondthird\x1b[?2026l", result);
    }

    #endregion

    #region Flag Lifecycle

    [Fact]
    public void Flush_WithFlagSet_DoesNotClearFlag()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        output.BeginSynchronizedOutput();
        output.WriteRaw("first");
        output.Flush();

        // Clear the writer to check second flush
        writer.GetStringBuilder().Clear();

        output.WriteRaw("second");
        output.Flush();

        var result = writer.ToString();
        Assert.StartsWith("\x1b[?2026h", result);
        Assert.EndsWith("\x1b[?2026l", result);
        Assert.Contains("second", result);
    }

    [Fact]
    public void Flush_WithoutFlag_NormalBehavior()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        output.WriteRaw("\x1b[H");
        output.WriteRaw("Hello");
        output.Flush();

        Assert.Equal("\x1b[HHello", writer.ToString());
    }

    [Fact]
    public void BeginEnd_Cycle_MarkersOnlyDuringActiveRegion()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        // First cycle: no sync
        output.WriteRaw("A");
        output.Flush();

        writer.GetStringBuilder().Clear();

        // Second cycle: with sync
        output.BeginSynchronizedOutput();
        output.WriteRaw("B");
        output.Flush();
        output.EndSynchronizedOutput();

        var syncResult = writer.ToString();
        Assert.Equal("\x1b[?2026hB\x1b[?2026l", syncResult);

        writer.GetStringBuilder().Clear();

        // Third cycle: no sync again
        output.WriteRaw("C");
        output.Flush();

        Assert.Equal("C", writer.ToString());
    }

    [Fact]
    public void SynchronizedOutput_ExceptionDuringRender_EndStillCalled()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        output.BeginSynchronizedOutput();
        try
        {
            output.WriteRaw("before-throw");
            throw new InvalidOperationException("simulated render failure");
        }
        catch (InvalidOperationException)
        {
            // Expected — flush the partial content to clear the buffer
            output.Flush();
        }
        finally
        {
            output.EndSynchronizedOutput();
        }

        // After exception + finally, flag should be cleared
        writer.GetStringBuilder().Clear();
        output.WriteRaw("after-exception");
        output.Flush();

        var result = writer.ToString();
        Assert.Equal("after-exception", result);
        Assert.DoesNotContain("\x1b[?2026h", result);
    }

    #endregion

    #region Thread Safety

    [Fact]
    public async Task BeginEndSynchronizedOutput_ConcurrentAccess_NoExceptions()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        var ct = TestContext.Current.CancellationToken;
        var tasks = new List<Task>();
        for (int i = 0; i < 100; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                output.BeginSynchronizedOutput();
                output.WriteRaw("x");
                output.Flush();
                output.EndSynchronizedOutput();
            }, ct));
        }

        // Should not throw — flag mutations are protected by _lock
        await Task.WhenAll(tasks);
    }

    #endregion
}
