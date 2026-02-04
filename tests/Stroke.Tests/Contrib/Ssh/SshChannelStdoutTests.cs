using System.Text;
using Stroke.Contrib.Ssh;
using Xunit;

namespace Stroke.Tests.Contrib.Ssh;

/// <summary>
/// Tests for <see cref="SshChannelStdout"/> TextWriter wrapper.
/// </summary>
public class SshChannelStdoutTests
{
    #region Write Tests

    [Fact]
    public void Write_String_DelegatesToChannel()
    {
        var channel = new TestSshChannel();
        using var stdout = new SshChannelStdout(channel);

        stdout.Write("Hello");

        Assert.Contains("Hello", channel.WrittenData);
    }

    [Fact]
    public void Write_NullString_DoesNotThrow()
    {
        var channel = new TestSshChannel();
        using var stdout = new SshChannelStdout(channel);

        // Should not throw
        stdout.Write((string?)null);
    }

    [Fact]
    public void Write_EmptyString_DoesNotThrow()
    {
        var channel = new TestSshChannel();
        using var stdout = new SshChannelStdout(channel);

        // Should not throw
        stdout.Write(string.Empty);
    }

    [Fact]
    public void Write_Char_DelegatesToChannel()
    {
        var channel = new TestSshChannel();
        using var stdout = new SshChannelStdout(channel);

        stdout.Write('X');

        Assert.Contains("X", channel.WrittenData);
    }

    [Fact]
    public void Write_CharArray_DelegatesToChannel()
    {
        var channel = new TestSshChannel();
        using var stdout = new SshChannelStdout(channel);

        stdout.Write(new[] { 'A', 'B', 'C' });

        Assert.Contains("ABC", channel.WrittenData);
    }

    #endregion

    #region Line Ending Conversion (FR-006, FR-007)

    [Fact]
    public void Write_ConvertsLfToCrlf()
    {
        var channel = new TestSshChannel();
        using var stdout = new SshChannelStdout(channel);

        stdout.Write("Line1\nLine2\nLine3");

        Assert.Equal("Line1\r\nLine2\r\nLine3", channel.WrittenData);
    }

    [Fact]
    public void Write_PreservesExistingCrlf()
    {
        var channel = new TestSshChannel();
        using var stdout = new SshChannelStdout(channel);

        stdout.Write("Line1\r\nLine2\r\n");

        // Should not double the \r
        Assert.Equal("Line1\r\nLine2\r\n", channel.WrittenData);
    }

    [Fact]
    public void Write_MixedLineEndings_ConvertsOnlyLf()
    {
        var channel = new TestSshChannel();
        using var stdout = new SshChannelStdout(channel);

        stdout.Write("A\r\nB\nC\r\nD\n");

        Assert.Equal("A\r\nB\r\nC\r\nD\r\n", channel.WrittenData);
    }

    [Fact]
    public void Write_OnlyNewline_ConvertsToCrlf()
    {
        var channel = new TestSshChannel();
        using var stdout = new SshChannelStdout(channel);

        stdout.Write("\n");

        Assert.Equal("\r\n", channel.WrittenData);
    }

    [Fact]
    public void Write_MultipleNewlines_ConvertsEach()
    {
        var channel = new TestSshChannel();
        using var stdout = new SshChannelStdout(channel);

        stdout.Write("\n\n\n");

        Assert.Equal("\r\n\r\n\r\n", channel.WrittenData);
    }

    [Fact]
    public void WriteLine_AppendsCrlf()
    {
        var channel = new TestSshChannel();
        using var stdout = new SshChannelStdout(channel);

        stdout.WriteLine("Test");

        Assert.Equal("Test\r\n", channel.WrittenData);
    }

    #endregion

    #region Encoding

    [Fact]
    public void Encoding_ReturnsUtf8()
    {
        var channel = new TestSshChannel();
        using var stdout = new SshChannelStdout(channel);

        Assert.Equal(Encoding.UTF8, stdout.Encoding);
    }

    #endregion

    #region IsAtty

    [Fact]
    public void IsAtty_ReturnsTrue()
    {
        var channel = new TestSshChannel();
        using var stdout = new SshChannelStdout(channel);

        Assert.True(stdout.IsAtty);
    }

    #endregion

    #region Flush

    [Fact]
    public void Flush_DoesNotThrow()
    {
        var channel = new TestSshChannel();
        using var stdout = new SshChannelStdout(channel);

        // Flush should be a no-op but not throw
        stdout.Flush();
    }

    #endregion

    #region Close

    [Fact]
    public void Close_DoesNotCloseChannel()
    {
        // SshChannelStdout.Close() does NOT close the underlying channel.
        // Channel lifecycle is managed by the session, not the stdout wrapper.
        var channel = new TestSshChannel();
        var stdout = new SshChannelStdout(channel);

        stdout.Close();

        // Channel should still be open - session manages channel lifecycle
        Assert.False(channel.IsClosed);
    }

    [Fact]
    public void Write_AfterClose_DoesNotThrow()
    {
        var channel = new TestSshChannel();
        var stdout = new SshChannelStdout(channel);
        stdout.Close();

        // Should not throw - just ignored
        stdout.Write("After close");
    }

    #endregion

    #region Test Doubles

    /// <summary>
    /// Test implementation of ISshChannel for unit testing.
    /// </summary>
    private class TestSshChannel : ISshChannel
    {
        private readonly StringBuilder _written = new();
        public bool IsClosed { get; private set; }

        public string WrittenData => _written.ToString();

        public void Write(string data)
        {
            if (!IsClosed && data != null)
            {
                _written.Append(data);
            }
        }

        public void Close()
        {
            IsClosed = true;
        }

        public string GetTerminalType() => "xterm";

        public (int Width, int Height) GetTerminalSize() => (80, 24);

        public Encoding GetEncoding() => Encoding.UTF8;

        public void SetLineMode(bool enabled) { }
    }

    #endregion
}
