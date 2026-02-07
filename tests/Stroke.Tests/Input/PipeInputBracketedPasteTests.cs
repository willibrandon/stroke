using Stroke.Input;
using Stroke.Input.Pipe;
using Xunit;

namespace Stroke.Tests.Input;

/// <summary>
/// Tests for bracketed paste mode via PipeInput.
/// </summary>
public class PipeInputBracketedPasteTests
{
    [Fact]
    public void BracketedPasteStart_EntersPasteModeWithoutEmittingKey()
    {
        using var pipe = InputFactory.CreatePipe();
        pipe.SendText("\x1b[200~");

        var keys = pipe.ReadKeys();

        // Start marker enters paste mode silently — no key emitted.
        Assert.Empty(keys);
    }

    [Fact]
    public void BracketedPasteEnd_IgnoredWhenNotInPasteMode()
    {
        using var pipe = InputFactory.CreatePipe();
        pipe.SendText("\x1b[201~");

        var keys = pipe.ReadKeys();

        // Stray end marker without a start — ignored silently.
        Assert.Empty(keys);
    }

    [Fact]
    public void BracketedPaste_PastedTextAfterStart_CollectedUntilEnd()
    {
        using var pipe = InputFactory.CreatePipe();
        // Start paste, content, end paste
        pipe.SendText("\x1b[200~hello world\x1b[201~");

        var keys = pipe.ReadKeys();

        // Should get exactly one BracketedPaste key with the pasted content
        Assert.Single(keys);
        Assert.Equal(Keys.BracketedPaste, keys[0].Key);
        Assert.Equal("hello world", keys[0].Data);
    }

    [Fact]
    public void BracketedPaste_TextBeforeAndAfterPaste_ParsedSeparately()
    {
        using var pipe = InputFactory.CreatePipe();
        pipe.SendText("before\x1b[200~pasted\x1b[201~after");

        var keys = pipe.ReadKeys();

        // "before" = 6 char keys, then 1 BracketedPaste, then "after" = 5 char keys
        Assert.Equal(12, keys.Count);
        Assert.Equal("b", keys[0].Data);
        Assert.Equal(Keys.BracketedPaste, keys[6].Key);
        Assert.Equal("pasted", keys[6].Data);
        Assert.Equal("a", keys[7].Data);
    }

    [Fact]
    public void BracketedPaste_SequenceWithEscapeInContent_HandledCorrectly()
    {
        using var pipe = InputFactory.CreatePipe();
        // Paste content that itself contains escape sequences
        pipe.SendText("\x1b[200~\x1b[A\x1b[201~");

        var keys = pipe.ReadKeys();

        // The escape sequence inside paste should be treated as paste content, not parsed
        Assert.Single(keys);
        Assert.Equal(Keys.BracketedPaste, keys[0].Key);
        Assert.Equal("\x1b[A", keys[0].Data);
    }

    [Fact]
    public void BracketedPaste_EmptyPaste_Detected()
    {
        using var pipe = InputFactory.CreatePipe();
        pipe.SendText("\x1b[200~\x1b[201~");

        var keys = pipe.ReadKeys();

        // Empty paste produces one BracketedPaste key with empty content
        Assert.Single(keys);
        Assert.Equal(Keys.BracketedPaste, keys[0].Key);
        Assert.Equal("", keys[0].Data);
    }

    [Fact]
    public void BracketedPaste_MultilinePaste_HandledCorrectly()
    {
        using var pipe = InputFactory.CreatePipe();
        pipe.SendText("\x1b[200~line1\nline2\nline3\x1b[201~");

        var keys = pipe.ReadKeys();

        Assert.Single(keys);
        Assert.Equal(Keys.BracketedPaste, keys[0].Key);
        Assert.Equal("line1\nline2\nline3", keys[0].Data);
    }
}
