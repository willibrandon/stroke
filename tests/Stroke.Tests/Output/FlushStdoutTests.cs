using Stroke.Output;
using Xunit;

namespace Stroke.Tests.Output;

/// <summary>
/// Tests for <see cref="FlushStdout"/> helper class.
/// </summary>
public sealed class FlushStdoutTests
{
    #region Write Tests

    [Fact]
    public void Write_WritesAndFlushes()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        FlushStdout.Write(output, "Hello");

        Assert.Contains("Hello", writer.ToString());
    }

    [Fact]
    public void Write_NullOutput_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => FlushStdout.Write(null!, "text"));
    }

    [Fact]
    public void Write_NullText_ThrowsArgumentNullException()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        Assert.Throws<ArgumentNullException>(() => FlushStdout.Write(output, null!));
    }

    [Fact]
    public void Write_EmptyText_WritesNothing()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        FlushStdout.Write(output, "");

        Assert.Empty(writer.ToString());
    }

    [Fact]
    public void Write_EscapesEscapeCharacters()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        FlushStdout.Write(output, "Test\x1bText");

        var result = writer.ToString();
        // Escape character should be replaced with ?
        Assert.Contains("Test?Text", result);
        // Verify no raw escape characters remain (char code 27)
        Assert.False(result.Contains((char)27), "Result should not contain escape character");
    }

    #endregion

    #region WriteRaw Tests

    [Fact]
    public void WriteRaw_WritesAndFlushes()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        FlushStdout.WriteRaw(output, "\x1b[2J");

        Assert.Contains("\x1b[2J", writer.ToString());
    }

    [Fact]
    public void WriteRaw_NullOutput_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => FlushStdout.WriteRaw(null!, "text"));
    }

    [Fact]
    public void WriteRaw_NullText_ThrowsArgumentNullException()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        Assert.Throws<ArgumentNullException>(() => FlushStdout.WriteRaw(output, null!));
    }

    [Fact]
    public void WriteRaw_PreservesEscapeCharacters()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        FlushStdout.WriteRaw(output, "\x1b[H\x1b[2J");

        var result = writer.ToString();
        Assert.Contains("\x1b[H", result);
        Assert.Contains("\x1b[2J", result);
    }

    #endregion

    #region WriteLine Tests

    [Fact]
    public void WriteLine_WithText_WritesTextAndNewline()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        FlushStdout.WriteLine(output, "Hello");

        var result = writer.ToString();
        Assert.Contains("Hello\n", result);
    }

    [Fact]
    public void WriteLine_NullOutput_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => FlushStdout.WriteLine(null!, "text"));
    }

    [Fact]
    public void WriteLine_NullText_ThrowsArgumentNullException()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        Assert.Throws<ArgumentNullException>(() => FlushStdout.WriteLine(output, null!));
    }

    [Fact]
    public void WriteLine_Empty_WritesOnlyNewline()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        FlushStdout.WriteLine(output);

        var result = writer.ToString();
        Assert.Contains("\n", result);
    }

    [Fact]
    public void WriteLine_NullOutputForEmpty_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => FlushStdout.WriteLine(null!));
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void MultipleWrites_AllFlushed()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        FlushStdout.Write(output, "A");
        FlushStdout.Write(output, "B");
        FlushStdout.Write(output, "C");

        var result = writer.ToString();
        Assert.Contains("A", result);
        Assert.Contains("B", result);
        Assert.Contains("C", result);
    }

    [Fact]
    public void MixedWriteAndWriteRaw()
    {
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        FlushStdout.WriteRaw(output, "\x1b[H");
        FlushStdout.Write(output, "Hello");
        FlushStdout.WriteRaw(output, "\x1b[K");

        var result = writer.ToString();
        Assert.Contains("\x1b[H", result);
        Assert.Contains("Hello", result);
        Assert.Contains("\x1b[K", result);
    }

    [Fact]
    public void WorksWithDummyOutput()
    {
        var output = new DummyOutput();

        // Should not throw
        FlushStdout.Write(output, "Hello");
        FlushStdout.WriteRaw(output, "\x1b[H");
        FlushStdout.WriteLine(output, "World");
        FlushStdout.WriteLine(output);
    }

    [Fact]
    public void WorksWithPlainTextOutput()
    {
        var writer = new StringWriter();
        var output = new PlainTextOutput(writer);

        FlushStdout.Write(output, "Hello");
        FlushStdout.WriteLine(output, "World");

        var result = writer.ToString();
        Assert.Contains("Hello", result);
        Assert.Contains("World\n", result);
    }

    #endregion
}
