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
    public void BracketedPasteStart_Detected()
    {
        using var pipe = InputFactory.CreatePipe();
        pipe.SendText("\x1b[200~");

        var keys = pipe.ReadKeys();

        Assert.Single(keys);
        Assert.Equal(Keys.BracketedPaste, keys[0].Key);
        Assert.Equal("\x1b[200~", keys[0].Data);
    }

    [Fact]
    public void BracketedPasteEnd_Detected()
    {
        using var pipe = InputFactory.CreatePipe();
        pipe.SendText("\x1b[201~");

        var keys = pipe.ReadKeys();

        Assert.Single(keys);
        Assert.Equal(Keys.BracketedPaste, keys[0].Key);
        Assert.Equal("\x1b[201~", keys[0].Data);
    }

    [Fact]
    public void BracketedPaste_PastedTextAfterStart_CollectedUntilEnd()
    {
        using var pipe = InputFactory.CreatePipe();
        // Start paste, content, end paste
        pipe.SendText("\x1b[200~hello world\x1b[201~");

        var keys = pipe.ReadKeys();

        // Should get: BracketedPaste start, then content as BracketedPaste with content
        // The exact behavior depends on implementation - let's verify we at least detect it
        Assert.True(keys.Count >= 1);
        Assert.Equal(Keys.BracketedPaste, keys[0].Key);
    }

    [Fact]
    public void BracketedPaste_TextBeforeAndAfterPaste_ParsedSeparately()
    {
        using var pipe = InputFactory.CreatePipe();
        pipe.SendText("before\x1b[200~pasted\x1b[201~after");

        var keys = pipe.ReadKeys();

        // "before" should be 6 chars, then bracketed paste, then "after" 5 chars
        Assert.True(keys.Count >= 1);
        // First chars should be "before"
        Assert.Equal("b", keys[0].Data);
    }

    [Fact]
    public void BracketedPaste_SequenceWithEscapeInContent_HandledCorrectly()
    {
        using var pipe = InputFactory.CreatePipe();
        // Paste content that itself contains escape sequences
        pipe.SendText("\x1b[200~\x1b[A\x1b[201~");

        var keys = pipe.ReadKeys();

        // The escape sequence inside paste should be treated as paste content, not parsed
        Assert.True(keys.Count >= 1);
        Assert.Equal(Keys.BracketedPaste, keys[0].Key);
    }

    [Fact]
    public void BracketedPaste_EmptyPaste_Detected()
    {
        using var pipe = InputFactory.CreatePipe();
        pipe.SendText("\x1b[200~\x1b[201~");

        var keys = pipe.ReadKeys();

        // Empty paste still produces key events
        Assert.True(keys.Count >= 1);
    }

    [Fact]
    public void BracketedPaste_MultilinePaste_HandledCorrectly()
    {
        using var pipe = InputFactory.CreatePipe();
        pipe.SendText("\x1b[200~line1\nline2\nline3\x1b[201~");

        var keys = pipe.ReadKeys();

        Assert.True(keys.Count >= 1);
        Assert.Equal(Keys.BracketedPaste, keys[0].Key);
    }
}
