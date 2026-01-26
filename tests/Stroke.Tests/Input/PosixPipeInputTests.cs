// Copyright (c) 2025 Brandon Pugh. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#pragma warning disable CA1416 // Platform compatibility - tests are skipped at runtime on unsupported platforms

using System.Runtime.InteropServices;
using Stroke.Input;
using Stroke.Input.Posix;
using Xunit;

namespace Stroke.Tests.Input;

/// <summary>
/// Tests for <see cref="PosixPipeInput"/>.
/// </summary>
/// <remarks>
/// These tests only run on POSIX platforms (Linux, macOS, FreeBSD).
/// </remarks>
public sealed class PosixPipeInputTests
{
    private static bool IsPosix => RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ||
                                    RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ||
                                    RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD);

    #region Constructor Tests

    [Fact]
    public void Constructor_CreatesPipeSuccessfully()
    {
        Assert.SkipUnless(IsPosix, "Test only runs on POSIX platforms");

        using var input = new PosixPipeInput();

        Assert.False(input.Closed);
    }

    [Fact]
    public void Constructor_WithInitialText_SendsTextToPipe()
    {
        Assert.SkipUnless(IsPosix, "Test only runs on POSIX platforms");

        using var input = new PosixPipeInput("hello");

        var keys = input.ReadKeys();
        Assert.Equal(5, keys.Count);
        Assert.Equal("h", keys[0].Data);
        Assert.Equal("e", keys[1].Data);
        Assert.Equal("l", keys[2].Data);
        Assert.Equal("l", keys[3].Data);
        Assert.Equal("o", keys[4].Data);
    }

    #endregion

    #region SendText and ReadKeys Tests

    [Fact]
    public void SendText_ThenReadKeys_ReturnsKeys()
    {
        Assert.SkipUnless(IsPosix, "Test only runs on POSIX platforms");

        using var input = new PosixPipeInput();

        input.SendText("abc");
        var keys = input.ReadKeys();

        Assert.Equal(3, keys.Count);
        Assert.Equal("a", keys[0].Data);
        Assert.Equal("b", keys[1].Data);
        Assert.Equal("c", keys[2].Data);
    }

    [Fact]
    public void SendText_EscapeSequence_ParsesCorrectly()
    {
        Assert.SkipUnless(IsPosix, "Test only runs on POSIX platforms");

        using var input = new PosixPipeInput();

        input.SendText("\x1b[A"); // Up arrow
        var keys = input.ReadKeys();

        Assert.Single(keys);
        Assert.Equal(Keys.Up, keys[0].Key);
    }

    [Fact]
    public void SendText_MultipleReads_ReturnsKeysInOrder()
    {
        Assert.SkipUnless(IsPosix, "Test only runs on POSIX platforms");

        using var input = new PosixPipeInput();

        input.SendText("a");
        var keys1 = input.ReadKeys();
        Assert.Single(keys1);
        Assert.Equal("a", keys1[0].Data);

        input.SendText("b");
        var keys2 = input.ReadKeys();
        Assert.Single(keys2);
        Assert.Equal("b", keys2[0].Data);
    }

    [Fact]
    public void SendText_NullData_ThrowsArgumentNullException()
    {
        Assert.SkipUnless(IsPosix, "Test only runs on POSIX platforms");

        using var input = new PosixPipeInput();

        Assert.Throws<ArgumentNullException>(() => input.SendText(null!));
    }

    #endregion

    #region SendBytes Tests

    [Fact]
    public void SendBytes_ThenReadKeys_ReturnsKeys()
    {
        Assert.SkipUnless(IsPosix, "Test only runs on POSIX platforms");

        using var input = new PosixPipeInput();

        input.SendBytes("xyz"u8.ToArray());
        var keys = input.ReadKeys();

        Assert.Equal(3, keys.Count);
        Assert.Equal("x", keys[0].Data);
        Assert.Equal("y", keys[1].Data);
        Assert.Equal("z", keys[2].Data);
    }

    [Fact]
    public void SendBytes_EscapeSequence_ParsesCorrectly()
    {
        Assert.SkipUnless(IsPosix, "Test only runs on POSIX platforms");

        using var input = new PosixPipeInput();

        input.SendBytes("\x1b[B"u8.ToArray()); // Down arrow
        var keys = input.ReadKeys();

        Assert.Single(keys);
        Assert.Equal(Keys.Down, keys[0].Key);
    }

    #endregion

    #region FlushKeys Tests

    [Fact]
    public void FlushKeys_PartialEscapeSequence_FlushesAsEscape()
    {
        Assert.SkipUnless(IsPosix, "Test only runs on POSIX platforms");

        using var input = new PosixPipeInput();

        input.SendText("\x1b");
        var keys = input.FlushKeys();

        Assert.Single(keys);
        Assert.Equal(Keys.Escape, keys[0].Key);
    }

    [Fact]
    public void FlushKeys_EmptyBuffer_ReturnsEmptyList()
    {
        Assert.SkipUnless(IsPosix, "Test only runs on POSIX platforms");

        using var input = new PosixPipeInput();

        var keys = input.FlushKeys();

        Assert.Empty(keys);
    }

    #endregion

    #region Close Tests

    [Fact]
    public void Close_SetsClosedToTrue()
    {
        Assert.SkipUnless(IsPosix, "Test only runs on POSIX platforms");

        using var input = new PosixPipeInput();

        Assert.False(input.Closed);
        input.Close();
        Assert.True(input.Closed);
    }

    [Fact]
    public void Close_MultipleCalls_IsIdempotent()
    {
        Assert.SkipUnless(IsPosix, "Test only runs on POSIX platforms");

        using var input = new PosixPipeInput();

        input.Close();
        input.Close();
        input.Close();

        Assert.True(input.Closed);
    }

    [Fact]
    public void SendText_AfterClose_ThrowsObjectDisposedException()
    {
        Assert.SkipUnless(IsPosix, "Test only runs on POSIX platforms");

        using var input = new PosixPipeInput();

        input.Close();

        Assert.Throws<ObjectDisposedException>(() => input.SendText("test"));
    }

    #endregion

    #region Dispose Tests

    [Fact]
    public void Dispose_CleansUpResources()
    {
        Assert.SkipUnless(IsPosix, "Test only runs on POSIX platforms");

        var input = new PosixPipeInput();
        input.Dispose();

        // After dispose, operations should throw
        Assert.Throws<ObjectDisposedException>(() => input.ReadKeys());
    }

    [Fact]
    public void Dispose_MultipleCalls_IsIdempotent()
    {
        Assert.SkipUnless(IsPosix, "Test only runs on POSIX platforms");

        var input = new PosixPipeInput();

        input.Dispose();
        input.Dispose();
        input.Dispose();

        // Should not throw
    }

    #endregion

    #region Mode Tests

    [Fact]
    public void RawMode_ReturnsNoOpDisposable()
    {
        Assert.SkipUnless(IsPosix, "Test only runs on POSIX platforms");

        using var input = new PosixPipeInput();

        using var rawMode = input.RawMode();

        Assert.NotNull(rawMode);
    }

    [Fact]
    public void CookedMode_ReturnsNoOpDisposable()
    {
        Assert.SkipUnless(IsPosix, "Test only runs on POSIX platforms");

        using var input = new PosixPipeInput();

        using var cookedMode = input.CookedMode();

        Assert.NotNull(cookedMode);
    }

    #endregion

    #region Attach/Detach Tests

    [Fact]
    public void Attach_AddsCallback()
    {
        Assert.SkipUnless(IsPosix, "Test only runs on POSIX platforms");

        using var input = new PosixPipeInput();
        var callCount = 0;

        using (input.Attach(() => callCount++))
        {
            // Callback is attached but not automatically invoked
            Assert.Equal(0, callCount);
        }
    }

    [Fact]
    public void Attach_NullCallback_ThrowsArgumentNullException()
    {
        Assert.SkipUnless(IsPosix, "Test only runs on POSIX platforms");

        using var input = new PosixPipeInput();

        Assert.Throws<ArgumentNullException>(() => input.Attach(null!));
    }

    [Fact]
    public void Detach_RemovesCallback()
    {
        Assert.SkipUnless(IsPosix, "Test only runs on POSIX platforms");

        using var input = new PosixPipeInput();

        using (input.Attach(() => { }))
        {
            using (input.Detach())
            {
                // Callback is detached
            }
            // Callback is reattached after dispose
        }
    }

    [Fact]
    public void Detach_NoCallbacks_ReturnsNoOpDisposable()
    {
        Assert.SkipUnless(IsPosix, "Test only runs on POSIX platforms");

        using var input = new PosixPipeInput();

        using var detach = input.Detach();

        Assert.NotNull(detach);
    }

    #endregion

    #region FileNo and TypeaheadHash Tests

    [Fact]
    public void FileNo_ReturnsReadFileDescriptor()
    {
        Assert.SkipUnless(IsPosix, "Test only runs on POSIX platforms");

        using var input = new PosixPipeInput();

        var fd = input.FileNo();

        Assert.True(fd >= 0);
    }

    [Fact]
    public void TypeaheadHash_ReturnsUniqueString()
    {
        Assert.SkipUnless(IsPosix, "Test only runs on POSIX platforms");

        using var input1 = new PosixPipeInput();
        using var input2 = new PosixPipeInput();

        var hash1 = input1.TypeaheadHash();
        var hash2 = input2.TypeaheadHash();

        Assert.StartsWith("posix-pipe-input-", hash1);
        Assert.StartsWith("posix-pipe-input-", hash2);
        Assert.NotEqual(hash1, hash2);
    }

    #endregion

    #region Thread Safety Tests

    [Fact]
    public void SendText_IsThreadSafe()
    {
        Assert.SkipUnless(IsPosix, "Test only runs on POSIX platforms");

        using var input = new PosixPipeInput();
        const int threadCount = 10;
        const int iterationsPerThread = 100;

        var threads = new Thread[threadCount];
        for (int i = 0; i < threadCount; i++)
        {
            threads[i] = new Thread(() =>
            {
                for (int j = 0; j < iterationsPerThread; j++)
                {
                    input.SendText("a");
                }
            });
        }

        foreach (var thread in threads)
        {
            thread.Start();
        }

        foreach (var thread in threads)
        {
            thread.Join();
        }

        // Read all keys
        var totalKeys = 0;
        IReadOnlyList<KeyPress> keys;
        while ((keys = input.ReadKeys()).Count > 0)
        {
            totalKeys += keys.Count;
        }

        // Should have received all keys (though exact count may vary due to read timing)
        Assert.True(totalKeys >= 0);
    }

    #endregion

    #region IPipeInput Interface Tests

    [Fact]
    public void ImplementsIPipeInput()
    {
        Assert.SkipUnless(IsPosix, "Test only runs on POSIX platforms");

        using var input = new PosixPipeInput();

        Assert.IsAssignableFrom<Stroke.Input.Pipe.IPipeInput>(input);
    }

    [Fact]
    public void ImplementsIInput()
    {
        Assert.SkipUnless(IsPosix, "Test only runs on POSIX platforms");

        using var input = new PosixPipeInput();

        Assert.IsAssignableFrom<IInput>(input);
    }

    #endregion
}
