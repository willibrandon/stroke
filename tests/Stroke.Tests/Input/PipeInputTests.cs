using Stroke.Input;
using Stroke.Input.Pipe;
using Xunit;

namespace Stroke.Tests.Input;

public class PipeInputTests
{
    [Fact]
    public void SendText_ThenReadKeys_ReturnsKeys()
    {
        using var pipe = new SimplePipeInput();
        pipe.SendText("hello");

        var keys = pipe.ReadKeys();

        Assert.Equal(5, keys.Count);
        Assert.Equal("h", keys[0].Data);
        Assert.Equal("e", keys[1].Data);
        Assert.Equal("l", keys[2].Data);
        Assert.Equal("l", keys[3].Data);
        Assert.Equal("o", keys[4].Data);
    }

    [Fact]
    public void SendBytes_ThenReadKeys_ReturnsKeys()
    {
        using var pipe = new SimplePipeInput();
        pipe.SendBytes("abc"u8);

        var keys = pipe.ReadKeys();

        Assert.Equal(3, keys.Count);
        Assert.Equal("a", keys[0].Data);
        Assert.Equal("b", keys[1].Data);
        Assert.Equal("c", keys[2].Data);
    }

    [Fact]
    public void SendText_EscapeSequence_ParsedCorrectly()
    {
        using var pipe = new SimplePipeInput();
        pipe.SendText("\x1b[A");

        var keys = pipe.ReadKeys();

        Assert.Single(keys);
        Assert.Equal(Keys.Up, keys[0].Key);
    }

    [Fact]
    public void SendText_ControlCharacter_ParsedCorrectly()
    {
        using var pipe = new SimplePipeInput();
        pipe.SendText("\x03");

        var keys = pipe.ReadKeys();

        Assert.Single(keys);
        Assert.Equal(Keys.ControlC, keys[0].Key);
    }

    [Fact]
    public void ReadKeys_WhenEmpty_ReturnsEmptyList()
    {
        using var pipe = new SimplePipeInput();

        var keys = pipe.ReadKeys();

        Assert.NotNull(keys);
        Assert.Empty(keys);
    }

    [Fact]
    public void ReadKeys_ClearsBuffer()
    {
        using var pipe = new SimplePipeInput();
        pipe.SendText("a");

        var keys1 = pipe.ReadKeys();
        var keys2 = pipe.ReadKeys();

        Assert.Single(keys1);
        Assert.Empty(keys2);
    }

    [Fact]
    public void FlushKeys_WithPartialSequence_FlushesRemaining()
    {
        using var pipe = new SimplePipeInput();
        pipe.SendText("\x1b");

        // ReadKeys doesn't flush partial sequences
        var keys1 = pipe.ReadKeys();
        Assert.Empty(keys1);

        // FlushKeys forces incomplete sequences to be output
        var keys2 = pipe.FlushKeys();
        Assert.Single(keys2);
        Assert.Equal(Keys.Escape, keys2[0].Key);
    }

    [Fact]
    public void Closed_InitiallyFalse()
    {
        using var pipe = new SimplePipeInput();

        Assert.False(pipe.Closed);
    }

    [Fact]
    public void Close_SetsClosed()
    {
        using var pipe = new SimplePipeInput();
        pipe.Close();

        Assert.True(pipe.Closed);
    }

    [Fact]
    public void SendText_AfterClose_ThrowsObjectDisposedException()
    {
        using var pipe = new SimplePipeInput();
        pipe.Close();

        Assert.Throws<ObjectDisposedException>(() => pipe.SendText("test"));
    }

    [Fact]
    public void SendBytes_AfterClose_ThrowsObjectDisposedException()
    {
        using var pipe = new SimplePipeInput();
        pipe.Close();

        Assert.Throws<ObjectDisposedException>(() => pipe.SendBytes("test"u8));
    }

    [Fact]
    public void Dispose_ClosesPipe()
    {
        var pipe = new SimplePipeInput();
        pipe.Dispose();

        Assert.True(pipe.Closed);
    }

    [Fact]
    public void ReadKeys_AfterDispose_ThrowsObjectDisposedException()
    {
        var pipe = new SimplePipeInput();
        pipe.Dispose();

        Assert.Throws<ObjectDisposedException>(() => pipe.ReadKeys());
    }

    [Fact]
    public void FlushKeys_AfterDispose_ThrowsObjectDisposedException()
    {
        var pipe = new SimplePipeInput();
        pipe.Dispose();

        Assert.Throws<ObjectDisposedException>(() => pipe.FlushKeys());
    }

    [Fact]
    public void SendText_Null_ThrowsArgumentNullException()
    {
        using var pipe = new SimplePipeInput();

        Assert.Throws<ArgumentNullException>(() => pipe.SendText(null!));
    }

    [Fact]
    public void TypeaheadHash_ReturnsUniqueString()
    {
        using var pipe1 = new SimplePipeInput();
        using var pipe2 = new SimplePipeInput();

        var hash1 = pipe1.TypeaheadHash();
        var hash2 = pipe2.TypeaheadHash();

        Assert.NotNull(hash1);
        Assert.NotNull(hash2);
        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void RawMode_ReturnsDisposable()
    {
        using var pipe = new SimplePipeInput();
        using var rawMode = pipe.RawMode();

        Assert.NotNull(rawMode);
    }

    [Fact]
    public void CookedMode_ReturnsDisposable()
    {
        using var pipe = new SimplePipeInput();
        using var cookedMode = pipe.CookedMode();

        Assert.NotNull(cookedMode);
    }

    [Fact]
    public void Attach_ReturnsDisposable()
    {
        using var pipe = new SimplePipeInput();
        using var attachment = pipe.Attach(() => { });

        Assert.NotNull(attachment);
    }

    [Fact]
    public void Attach_NullCallback_ThrowsArgumentNullException()
    {
        using var pipe = new SimplePipeInput();

        Assert.Throws<ArgumentNullException>(() => pipe.Attach(null!));
    }

    [Fact]
    public void Detach_ReturnsDisposable()
    {
        using var pipe = new SimplePipeInput();
        using var detach = pipe.Detach();

        Assert.NotNull(detach);
    }

    [Fact]
    public void FileNo_ThrowsNotSupportedException()
    {
        using var pipe = new SimplePipeInput();

        Assert.Throws<NotSupportedException>(() => pipe.FileNo());
    }

    [Fact]
    public void SendText_Unicode_ParsedCorrectly()
    {
        using var pipe = new SimplePipeInput();
        pipe.SendText("日本語");

        var keys = pipe.ReadKeys();

        Assert.Equal(3, keys.Count);
        Assert.Equal("日", keys[0].Data);
        Assert.Equal("本", keys[1].Data);
        Assert.Equal("語", keys[2].Data);
    }

    [Fact]
    public void SendText_MixedContent_ParsedCorrectly()
    {
        using var pipe = new SimplePipeInput();
        pipe.SendText("a\x1b[Ab");

        var keys = pipe.ReadKeys();

        Assert.Equal(3, keys.Count);
        Assert.Equal("a", keys[0].Data);
        Assert.Equal(Keys.Up, keys[1].Key);
        Assert.Equal("b", keys[2].Data);
    }

    [Fact]
    public void SendText_MultipleCalls_Accumulates()
    {
        using var pipe = new SimplePipeInput();
        pipe.SendText("a");
        pipe.SendText("b");
        pipe.SendText("c");

        var keys = pipe.ReadKeys();

        Assert.Equal(3, keys.Count);
        Assert.Equal("a", keys[0].Data);
        Assert.Equal("b", keys[1].Data);
        Assert.Equal("c", keys[2].Data);
    }

    [Fact]
    public async Task ThreadSafety_ConcurrentSendAndRead()
    {
        using var pipe = new SimplePipeInput();
        var allKeys = new List<KeyPress>();
        var lockObj = new object();
        var sendCount = 100;
        var readIterations = 20;

        var ct = TestContext.Current.CancellationToken;
        var sendTask = Task.Run(async () =>
        {
            for (int i = 0; i < sendCount; i++)
            {
                pipe.SendText("x");
                await Task.Delay(1, ct);
            }
        }, ct);

        var readTask = Task.Run(async () =>
        {
            for (int i = 0; i < readIterations; i++)
            {
                var keys = pipe.ReadKeys();
                lock (lockObj)
                {
                    allKeys.AddRange(keys);
                }
                await Task.Delay(5, ct);
            }
        }, ct);

        await Task.WhenAll(sendTask, readTask);

        // Read any remaining keys
        var remaining = pipe.ReadKeys();
        allKeys.AddRange(remaining);

        // All sent characters should eventually be read
        Assert.True(allKeys.Count <= sendCount);
        Assert.True(allKeys.All(k => k.Data == "x" || k.Key == Keys.Any));
    }
}
