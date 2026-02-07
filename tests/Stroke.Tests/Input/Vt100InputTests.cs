using Stroke.Input;
using Stroke.Input.Pipe;
using Xunit;

namespace Stroke.Tests.Input;

/// <summary>
/// Tests for VT100 input handling via PipeInput.
/// These tests verify keyboard input parsing works correctly
/// using PipeInput as a test harness (per Constitution VIII - no mocks).
/// </summary>
public class Vt100InputTests
{
    #region T023: Basic Keyboard Input

    [Fact]
    public void ReadKeys_SingleCharacter_ReturnsKeyPress()
    {
        using var pipe = InputFactory.CreatePipe();
        pipe.SendText("a");

        var keys = pipe.ReadKeys();

        Assert.Single(keys);
        Assert.Equal(Keys.Any, keys[0].Key);
        Assert.Equal("a", keys[0].Data);
    }

    [Fact]
    public void ReadKeys_MultipleCharacters_ReturnsAllKeyPresses()
    {
        using var pipe = InputFactory.CreatePipe();
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
    public void ReadKeys_ControlCharacter_ReturnsControlKey()
    {
        using var pipe = InputFactory.CreatePipe();
        pipe.SendText("\x03"); // Ctrl+C

        var keys = pipe.ReadKeys();

        Assert.Single(keys);
        Assert.Equal(Keys.ControlC, keys[0].Key);
    }

    [Theory]
    [InlineData("\x01", Keys.ControlA)]
    [InlineData("\x02", Keys.ControlB)]
    [InlineData("\x03", Keys.ControlC)]
    [InlineData("\x04", Keys.ControlD)]
    [InlineData("\r", Keys.ControlM)] // Enter
    [InlineData("\n", Keys.ControlJ)] // Linefeed
    [InlineData("\t", Keys.ControlI)] // Tab
    public void ReadKeys_ControlCharacters_MapCorrectly(string input, Keys expected)
    {
        using var pipe = InputFactory.CreatePipe();
        pipe.SendText(input);

        var keys = pipe.ReadKeys();

        Assert.Single(keys);
        Assert.Equal(expected, keys[0].Key);
    }

    [Fact]
    public void ReadKeys_Tab_ReturnsControlI()
    {
        using var pipe = InputFactory.CreatePipe();
        pipe.SendText("\t");

        var keys = pipe.ReadKeys();

        Assert.Single(keys);
        Assert.Equal(Keys.ControlI, keys[0].Key);
    }

    [Fact]
    public void ReadKeys_Enter_ReturnsControlM()
    {
        using var pipe = InputFactory.CreatePipe();
        pipe.SendText("\r");

        var keys = pipe.ReadKeys();

        Assert.Single(keys);
        Assert.Equal(Keys.ControlM, keys[0].Key);
    }

    [Fact]
    public void ReadKeys_UnicodeCharacter_ReturnsWithCorrectData()
    {
        using var pipe = InputFactory.CreatePipe();
        pipe.SendText("日");

        var keys = pipe.ReadKeys();

        Assert.Single(keys);
        Assert.Equal(Keys.Any, keys[0].Key);
        Assert.Equal("日", keys[0].Data);
    }

    [Fact]
    public void ReadKeys_MixedAsciiAndUnicode_ParsesCorrectly()
    {
        using var pipe = InputFactory.CreatePipe();
        pipe.SendText("a日b");

        var keys = pipe.ReadKeys();

        Assert.Equal(3, keys.Count);
        Assert.Equal("a", keys[0].Data);
        Assert.Equal("日", keys[1].Data);
        Assert.Equal("b", keys[2].Data);
    }

    #endregion

    #region T024: Function Key Input

    [Theory]
    [InlineData("\x1bOP", Keys.F1)]
    [InlineData("\x1bOQ", Keys.F2)]
    [InlineData("\x1bOR", Keys.F3)]
    [InlineData("\x1bOS", Keys.F4)]
    [InlineData("\x1b[15~", Keys.F5)]
    [InlineData("\x1b[17~", Keys.F6)]
    [InlineData("\x1b[18~", Keys.F7)]
    [InlineData("\x1b[19~", Keys.F8)]
    [InlineData("\x1b[20~", Keys.F9)]
    [InlineData("\x1b[21~", Keys.F10)]
    [InlineData("\x1b[23~", Keys.F11)]
    [InlineData("\x1b[24~", Keys.F12)]
    public void ReadKeys_FunctionKeys_MapCorrectly(string sequence, Keys expected)
    {
        using var pipe = InputFactory.CreatePipe();
        pipe.SendText(sequence);

        var keys = pipe.ReadKeys();

        Assert.Single(keys);
        Assert.Equal(expected, keys[0].Key);
    }

    [Theory]
    [InlineData("\x1b[11~", Keys.F1)] // Alternative F1
    [InlineData("\x1b[12~", Keys.F2)] // Alternative F2
    public void ReadKeys_AlternativeFunctionKeySequences_MapCorrectly(string sequence, Keys expected)
    {
        using var pipe = InputFactory.CreatePipe();
        pipe.SendText(sequence);

        var keys = pipe.ReadKeys();

        Assert.Single(keys);
        Assert.Equal(expected, keys[0].Key);
    }

    [Fact]
    public void ReadKeys_ConsecutiveFunctionKeys_ParsesAll()
    {
        using var pipe = InputFactory.CreatePipe();
        pipe.SendText("\x1bOP\x1bOQ\x1bOR"); // F1, F2, F3

        var keys = pipe.ReadKeys();

        Assert.Equal(3, keys.Count);
        Assert.Equal(Keys.F1, keys[0].Key);
        Assert.Equal(Keys.F2, keys[1].Key);
        Assert.Equal(Keys.F3, keys[2].Key);
    }

    #endregion

    #region T025: Arrow Key Input

    [Theory]
    [InlineData("\x1b[A", Keys.Up)]
    [InlineData("\x1b[B", Keys.Down)]
    [InlineData("\x1b[C", Keys.Right)]
    [InlineData("\x1b[D", Keys.Left)]
    public void ReadKeys_ArrowKeys_MapCorrectly(string sequence, Keys expected)
    {
        using var pipe = InputFactory.CreatePipe();
        pipe.SendText(sequence);

        var keys = pipe.ReadKeys();

        Assert.Single(keys);
        Assert.Equal(expected, keys[0].Key);
    }

    [Theory]
    [InlineData("\x1b[1;5A", Keys.ControlUp)]
    [InlineData("\x1b[1;5B", Keys.ControlDown)]
    [InlineData("\x1b[1;5C", Keys.ControlRight)]
    [InlineData("\x1b[1;5D", Keys.ControlLeft)]
    public void ReadKeys_ControlArrowKeys_MapCorrectly(string sequence, Keys expected)
    {
        using var pipe = InputFactory.CreatePipe();
        pipe.SendText(sequence);

        var keys = pipe.ReadKeys();

        Assert.Single(keys);
        Assert.Equal(expected, keys[0].Key);
    }

    [Theory]
    [InlineData("\x1b[1;2A", Keys.ShiftUp)]
    [InlineData("\x1b[1;2B", Keys.ShiftDown)]
    [InlineData("\x1b[1;2C", Keys.ShiftRight)]
    [InlineData("\x1b[1;2D", Keys.ShiftLeft)]
    public void ReadKeys_ShiftArrowKeys_MapCorrectly(string sequence, Keys expected)
    {
        using var pipe = InputFactory.CreatePipe();
        pipe.SendText(sequence);

        var keys = pipe.ReadKeys();

        Assert.Single(keys);
        Assert.Equal(expected, keys[0].Key);
    }

    [Theory]
    [InlineData("\x1b[1;6A", Keys.ControlShiftUp)]
    [InlineData("\x1b[1;6B", Keys.ControlShiftDown)]
    [InlineData("\x1b[1;6C", Keys.ControlShiftRight)]
    [InlineData("\x1b[1;6D", Keys.ControlShiftLeft)]
    public void ReadKeys_ControlShiftArrowKeys_MapCorrectly(string sequence, Keys expected)
    {
        using var pipe = InputFactory.CreatePipe();
        pipe.SendText(sequence);

        var keys = pipe.ReadKeys();

        Assert.Single(keys);
        Assert.Equal(expected, keys[0].Key);
    }

    [Fact]
    public void ReadKeys_ConsecutiveArrowKeys_ParsesAll()
    {
        using var pipe = InputFactory.CreatePipe();
        pipe.SendText("\x1b[A\x1b[B\x1b[C\x1b[D"); // Up, Down, Right, Left

        var keys = pipe.ReadKeys();

        Assert.Equal(4, keys.Count);
        Assert.Equal(Keys.Up, keys[0].Key);
        Assert.Equal(Keys.Down, keys[1].Key);
        Assert.Equal(Keys.Right, keys[2].Key);
        Assert.Equal(Keys.Left, keys[3].Key);
    }

    #endregion

    #region Navigation Keys

    [Theory]
    [InlineData("\x1b[H", Keys.Home)]
    [InlineData("\x1b[F", Keys.End)]
    [InlineData("\x1b[2~", Keys.Insert)]
    [InlineData("\x1b[3~", Keys.Delete)]
    [InlineData("\x1b[5~", Keys.PageUp)]
    [InlineData("\x1b[6~", Keys.PageDown)]
    public void ReadKeys_NavigationKeys_MapCorrectly(string sequence, Keys expected)
    {
        using var pipe = InputFactory.CreatePipe();
        pipe.SendText(sequence);

        var keys = pipe.ReadKeys();

        Assert.Single(keys);
        Assert.Equal(expected, keys[0].Key);
    }

    [Theory]
    [InlineData("\x1b[1~", Keys.Home)] // Alternative Home
    [InlineData("\x1b[4~", Keys.End)]  // Alternative End
    [InlineData("\x1bOH", Keys.Home)]  // SS3 Home
    [InlineData("\x1bOF", Keys.End)]   // SS3 End
    public void ReadKeys_AlternativeNavigationSequences_MapCorrectly(string sequence, Keys expected)
    {
        using var pipe = InputFactory.CreatePipe();
        pipe.SendText(sequence);

        var keys = pipe.ReadKeys();

        Assert.Single(keys);
        Assert.Equal(expected, keys[0].Key);
    }

    #endregion

    #region Special Keys

    [Fact]
    public void ReadKeys_BackTab_MapsCorrectly()
    {
        using var pipe = InputFactory.CreatePipe();
        pipe.SendText("\x1b[Z");

        var keys = pipe.ReadKeys();

        Assert.Single(keys);
        Assert.Equal(Keys.BackTab, keys[0].Key);
    }

    [Fact]
    public void ReadKeys_BracketedPasteStart_EntersPasteModeWithoutEmittingKey()
    {
        using var pipe = InputFactory.CreatePipe();
        pipe.SendText("\x1b[200~");

        var keys = pipe.ReadKeys();

        // Start marker enters paste mode silently — no key emitted.
        Assert.Empty(keys);
    }

    [Fact]
    public void ReadKeys_BracketedPasteEnd_IgnoredWhenNotInPasteMode()
    {
        using var pipe = InputFactory.CreatePipe();
        pipe.SendText("\x1b[201~");

        var keys = pipe.ReadKeys();

        // Stray end marker without a start — ignored silently.
        Assert.Empty(keys);
    }

    #endregion

    #region Mixed Input

    [Fact]
    public void ReadKeys_MixedCharactersAndEscapeSequences_ParsesCorrectly()
    {
        using var pipe = InputFactory.CreatePipe();
        pipe.SendText("a\x1b[Ab\x1b[Bc");

        var keys = pipe.ReadKeys();

        Assert.Equal(5, keys.Count);
        Assert.Equal("a", keys[0].Data);
        Assert.Equal(Keys.Up, keys[1].Key);
        Assert.Equal("b", keys[2].Data);
        Assert.Equal(Keys.Down, keys[3].Key);
        Assert.Equal("c", keys[4].Data);
    }

    [Fact]
    public void ReadKeys_TextFollowedByControlChar_ParsesCorrectly()
    {
        using var pipe = InputFactory.CreatePipe();
        pipe.SendText("hello\r"); // text + Enter

        var keys = pipe.ReadKeys();

        Assert.Equal(6, keys.Count);
        Assert.Equal("h", keys[0].Data);
        Assert.Equal("e", keys[1].Data);
        Assert.Equal("l", keys[2].Data);
        Assert.Equal("l", keys[3].Data);
        Assert.Equal("o", keys[4].Data);
        Assert.Equal(Keys.ControlM, keys[5].Key);
    }

    #endregion
}
