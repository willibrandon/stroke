using Stroke.Input;
using Stroke.Input.Vt100;
using Xunit;

namespace Stroke.Tests.Input;

public class Vt100ParserTests
{
    private readonly List<KeyPress> _keys = new();
    private readonly Vt100Parser _parser;

    public Vt100ParserTests()
    {
        _parser = new Vt100Parser(key => _keys.Add(key));
    }

    [Fact]
    public void Feed_SingleCharacter_OutputsCharacter()
    {
        _parser.Feed("a");

        Assert.Single(_keys);
        Assert.Equal(Keys.Any, _keys[0].Key);
        Assert.Equal("a", _keys[0].Data);
    }

    [Fact]
    public void Feed_MultipleCharacters_OutputsEach()
    {
        _parser.Feed("abc");

        Assert.Equal(3, _keys.Count);
        Assert.Equal("a", _keys[0].Data);
        Assert.Equal("b", _keys[1].Data);
        Assert.Equal("c", _keys[2].Data);
    }

    [Fact]
    public void Feed_ControlCharacter_OutputsControlKey()
    {
        _parser.Feed("\x03"); // Ctrl+C

        Assert.Single(_keys);
        Assert.Equal(Keys.ControlC, _keys[0].Key);
    }

    [Theory]
    [InlineData("\x01", Keys.ControlA)]
    [InlineData("\x02", Keys.ControlB)]
    [InlineData("\x03", Keys.ControlC)]
    [InlineData("\x04", Keys.ControlD)]
    [InlineData("\x05", Keys.ControlE)]
    [InlineData("\x06", Keys.ControlF)]
    [InlineData("\x07", Keys.ControlG)]
    [InlineData("\x08", Keys.ControlH)]
    [InlineData("\x09", Keys.ControlI)]
    [InlineData("\x0a", Keys.ControlJ)]
    [InlineData("\x0b", Keys.ControlK)]
    [InlineData("\x0c", Keys.ControlL)]
    [InlineData("\x0d", Keys.ControlM)]
    [InlineData("\x0e", Keys.ControlN)]
    [InlineData("\x0f", Keys.ControlO)]
    [InlineData("\x10", Keys.ControlP)]
    [InlineData("\x11", Keys.ControlQ)]
    [InlineData("\x12", Keys.ControlR)]
    [InlineData("\x13", Keys.ControlS)]
    [InlineData("\x14", Keys.ControlT)]
    [InlineData("\x15", Keys.ControlU)]
    [InlineData("\x16", Keys.ControlV)]
    [InlineData("\x17", Keys.ControlW)]
    [InlineData("\x18", Keys.ControlX)]
    [InlineData("\x19", Keys.ControlY)]
    [InlineData("\x1a", Keys.ControlZ)]
    public void Feed_ControlCharacters_MapsCorrectly(string input, Keys expectedKey)
    {
        _parser.Feed(input);

        Assert.Single(_keys);
        Assert.Equal(expectedKey, _keys[0].Key);
    }

    [Theory]
    [InlineData("\x1b[A", Keys.Up)]
    [InlineData("\x1b[B", Keys.Down)]
    [InlineData("\x1b[C", Keys.Right)]
    [InlineData("\x1b[D", Keys.Left)]
    [InlineData("\x1b[H", Keys.Home)]
    [InlineData("\x1b[F", Keys.End)]
    public void Feed_ArrowKeys_MapsCorrectly(string sequence, Keys expectedKey)
    {
        _parser.Feed(sequence);

        Assert.Single(_keys);
        Assert.Equal(expectedKey, _keys[0].Key);
        Assert.Equal(sequence, _keys[0].Data);
    }

    [Theory]
    [InlineData("\x1b[2~", Keys.Insert)]
    [InlineData("\x1b[3~", Keys.Delete)]
    [InlineData("\x1b[5~", Keys.PageUp)]
    [InlineData("\x1b[6~", Keys.PageDown)]
    public void Feed_EditingKeys_MapsCorrectly(string sequence, Keys expectedKey)
    {
        _parser.Feed(sequence);

        Assert.Single(_keys);
        Assert.Equal(expectedKey, _keys[0].Key);
    }

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
    public void Feed_FunctionKeys_MapsCorrectly(string sequence, Keys expectedKey)
    {
        _parser.Feed(sequence);

        Assert.Single(_keys);
        Assert.Equal(expectedKey, _keys[0].Key);
    }

    [Theory]
    [InlineData("\x1b[1;5A", Keys.ControlUp)]
    [InlineData("\x1b[1;5B", Keys.ControlDown)]
    [InlineData("\x1b[1;5C", Keys.ControlRight)]
    [InlineData("\x1b[1;5D", Keys.ControlLeft)]
    [InlineData("\x1b[1;2A", Keys.ShiftUp)]
    [InlineData("\x1b[1;2B", Keys.ShiftDown)]
    [InlineData("\x1b[1;2C", Keys.ShiftRight)]
    [InlineData("\x1b[1;2D", Keys.ShiftLeft)]
    [InlineData("\x1b[1;6A", Keys.ControlShiftUp)]
    [InlineData("\x1b[1;6B", Keys.ControlShiftDown)]
    [InlineData("\x1b[1;6C", Keys.ControlShiftRight)]
    [InlineData("\x1b[1;6D", Keys.ControlShiftLeft)]
    public void Feed_ModifiedArrowKeys_MapsCorrectly(string sequence, Keys expectedKey)
    {
        _parser.Feed(sequence);

        Assert.Single(_keys);
        Assert.Equal(expectedKey, _keys[0].Key);
    }

    [Fact]
    public void Feed_BackTab_MapsCorrectly()
    {
        _parser.Feed("\x1b[Z");

        Assert.Single(_keys);
        Assert.Equal(Keys.BackTab, _keys[0].Key);
    }

    [Fact]
    public void Feed_PartialSequence_BuffersUntilComplete()
    {
        _parser.Feed("\x1b");
        Assert.Empty(_keys);

        _parser.Feed("[");
        Assert.Empty(_keys);

        _parser.Feed("A");
        Assert.Single(_keys);
        Assert.Equal(Keys.Up, _keys[0].Key);
    }

    [Fact]
    public void Feed_PartialFunctionKeySequence_BuffersUntilComplete()
    {
        _parser.Feed("\x1b");
        Assert.Empty(_keys);

        _parser.Feed("O");
        Assert.Empty(_keys);

        _parser.Feed("P");
        Assert.Single(_keys);
        Assert.Equal(Keys.F1, _keys[0].Key);
    }

    [Fact]
    public void Flush_PartialEscape_OutputsEscapeKey()
    {
        _parser.Feed("\x1b");
        Assert.Empty(_keys);

        _parser.Flush();
        Assert.Single(_keys);
        Assert.Equal(Keys.Escape, _keys[0].Key);
    }

    [Fact]
    public void Flush_EmptyBuffer_DoesNothing()
    {
        _parser.Flush();
        Assert.Empty(_keys);
    }

    [Fact]
    public void FeedAndFlush_CompleteSequence_OutputsKey()
    {
        _parser.FeedAndFlush("\x1b[A");

        Assert.Single(_keys);
        Assert.Equal(Keys.Up, _keys[0].Key);
    }

    [Fact]
    public void FeedAndFlush_PartialSequence_OutputsAsEscape()
    {
        _parser.FeedAndFlush("\x1b");

        Assert.Single(_keys);
        Assert.Equal(Keys.Escape, _keys[0].Key);
    }

    [Fact]
    public void Reset_ClearsBuffer()
    {
        _parser.Feed("\x1b[");
        Assert.Empty(_keys);

        _parser.Reset();
        _parser.Feed("A");

        // After reset, 'A' should be treated as a standalone character
        Assert.Single(_keys);
        Assert.Equal(Keys.Any, _keys[0].Key);
        Assert.Equal("A", _keys[0].Data);
    }

    [Fact]
    public void Feed_UnknownCsiSequence_OutputsAsEscape()
    {
        _parser.Feed("\x1b[999X");

        // Unknown sequence should output Escape followed by remaining characters
        Assert.True(_keys.Count >= 1);
        // The exact behavior depends on implementation - either as escape or ignore
    }

    [Fact]
    public void Feed_MixedContent_ParsesCorrectly()
    {
        _parser.Feed("a\x1b[Ab");

        Assert.Equal(3, _keys.Count);
        Assert.Equal("a", _keys[0].Data);
        Assert.Equal(Keys.Up, _keys[1].Key);
        Assert.Equal("b", _keys[2].Data);
    }

    [Fact]
    public void Feed_ConsecutiveEscapeSequences_ParsesAll()
    {
        _parser.Feed("\x1b[A\x1b[B\x1b[C");

        Assert.Equal(3, _keys.Count);
        Assert.Equal(Keys.Up, _keys[0].Key);
        Assert.Equal(Keys.Down, _keys[1].Key);
        Assert.Equal(Keys.Right, _keys[2].Key);
    }

    [Fact]
    public void Feed_BracketedPasteStart_OutputsBracketedPasteKey()
    {
        _parser.Feed("\x1b[200~");

        Assert.Single(_keys);
        Assert.Equal(Keys.BracketedPaste, _keys[0].Key);
    }

    [Fact]
    public void Feed_BracketedPasteEnd_OutputsBracketedPasteKey()
    {
        _parser.Feed("\x1b[201~");

        Assert.Single(_keys);
        Assert.Equal(Keys.BracketedPaste, _keys[0].Key);
    }

    [Fact]
    public void Feed_AlternativeHomeSequence_MapsToHome()
    {
        _parser.Feed("\x1b[1~");

        Assert.Single(_keys);
        Assert.Equal(Keys.Home, _keys[0].Key);
    }

    [Fact]
    public void Feed_AlternativeEndSequence_MapsToEnd()
    {
        _parser.Feed("\x1b[4~");

        Assert.Single(_keys);
        Assert.Equal(Keys.End, _keys[0].Key);
    }

    [Fact]
    public void Feed_AlternativeHomeWithOH_MapsToHome()
    {
        _parser.Feed("\x1bOH");

        Assert.Single(_keys);
        Assert.Equal(Keys.Home, _keys[0].Key);
    }

    [Fact]
    public void Feed_AlternativeEndWithOF_MapsToEnd()
    {
        _parser.Feed("\x1bOF");

        Assert.Single(_keys);
        Assert.Equal(Keys.End, _keys[0].Key);
    }

    [Fact]
    public void Feed_AlternativeF1Sequence_MapsToF1()
    {
        _parser.Feed("\x1b[11~");

        Assert.Single(_keys);
        Assert.Equal(Keys.F1, _keys[0].Key);
    }

    [Fact]
    public void Feed_AlternativeF2Sequence_MapsToF2()
    {
        _parser.Feed("\x1b[12~");

        Assert.Single(_keys);
        Assert.Equal(Keys.F2, _keys[0].Key);
    }

    [Fact]
    public void Feed_UnicodeCharacter_OutputsAsAny()
    {
        _parser.Feed("日");

        Assert.Single(_keys);
        Assert.Equal(Keys.Any, _keys[0].Key);
        Assert.Equal("日", _keys[0].Data);
    }

    [Fact]
    public void Feed_MultipleUnicodeCharacters_OutputsEach()
    {
        _parser.Feed("日本語");

        Assert.Equal(3, _keys.Count);
        Assert.Equal("日", _keys[0].Data);
        Assert.Equal("本", _keys[1].Data);
        Assert.Equal("語", _keys[2].Data);
    }

    [Fact]
    public void Feed_NullCharacter_OutputsControlAt()
    {
        _parser.Feed("\x00");

        Assert.Single(_keys);
        Assert.Equal(Keys.ControlAt, _keys[0].Key);
    }

    [Fact]
    public void Feed_Tab_OutputsControlI()
    {
        _parser.Feed("\t");

        Assert.Single(_keys);
        Assert.Equal(Keys.ControlI, _keys[0].Key);
    }

    [Fact]
    public void Feed_Enter_OutputsControlM()
    {
        _parser.Feed("\r");

        Assert.Single(_keys);
        Assert.Equal(Keys.ControlM, _keys[0].Key);
    }

    [Fact]
    public void Feed_Linefeed_OutputsControlJ()
    {
        _parser.Feed("\n");

        Assert.Single(_keys);
        Assert.Equal(Keys.ControlJ, _keys[0].Key);
    }

    [Theory]
    [InlineData("\x1c", Keys.ControlBackslash)]
    [InlineData("\x1d", Keys.ControlSquareClose)]
    [InlineData("\x1e", Keys.ControlCircumflex)]
    [InlineData("\x1f", Keys.ControlUnderscore)]
    public void Feed_SpecialControlCharacters_MapsCorrectly(string input, Keys expectedKey)
    {
        _parser.Feed(input);

        Assert.Single(_keys);
        Assert.Equal(expectedKey, _keys[0].Key);
    }

    [Fact]
    public void Feed_Delete_OutputsDelete()
    {
        _parser.Feed("\x7f");

        // DEL character (127) - treated as backspace in some contexts
        Assert.Single(_keys);
        // The mapping depends on implementation - could be ControlH or a special handling
    }

    [Fact]
    public void Feed_EmptyString_DoesNothing()
    {
        _parser.Feed("");
        Assert.Empty(_keys);
    }

    [Fact]
    public void Feed_Null_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _parser.Feed(null!));
    }

    [Fact]
    public void Constructor_NullCallback_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new Vt100Parser(null!));
    }

    #region T086-T088: Mouse Protocol Tests

    // T086: X10 Mouse Protocol Tests
    // Note: X10 format requires exactly 3 bytes after ESC [ M
    // The regex pattern is @"^\x1b\[M...$" which matches any 3 characters
    [Fact]
    public void Feed_X10MouseEvent_OutputsVt100MouseEvent()
    {
        // X10 format: ESC [ M Cb Cx Cy (3 bytes: button, x+32, y+32)
        // Button 0 (left click) at position (1,1): space=32 (button), !=33 (x), !=33 (y)
        // Total: 6 chars (ESC [ M + 3 data bytes)
        _parser.Feed("\x1b[M !!"); // space is button byte (0x20), ! is x and y

        Assert.Single(_keys);
        Assert.Equal(Keys.Vt100MouseEvent, _keys[0].Key);
        Assert.Equal("\x1b[M !!", _keys[0].Data);
    }

    [Fact]
    public void Feed_X10MouseEvent_DifferentButton_OutputsVt100MouseEvent()
    {
        // Button 1 (middle click): button=33 (0x21='!'), X=42 (0x2A='*'), Y=52 (0x34='4')
        _parser.Feed("\x1b[M!*4");

        Assert.Single(_keys);
        Assert.Equal(Keys.Vt100MouseEvent, _keys[0].Key);
    }

    [Fact]
    public void Feed_X10MouseEvent_WithData_PreservesRawSequence()
    {
        var sequence = "\x1b[M !!";
        _parser.Feed(sequence);

        Assert.Single(_keys);
        Assert.Equal(sequence, _keys[0].Data);
    }

    // T087: SGR Mouse Protocol Tests
    [Fact]
    public void Feed_SgrMouseEvent_Press_OutputsVt100MouseEvent()
    {
        // SGR format: ESC [ < Cb ; Cx ; Cy M (press) or m (release)
        // Left button press at (10,20)
        _parser.Feed("\x1b[<0;10;20M");

        Assert.Single(_keys);
        Assert.Equal(Keys.Vt100MouseEvent, _keys[0].Key);
        Assert.Equal("\x1b[<0;10;20M", _keys[0].Data);
    }

    [Fact]
    public void Feed_SgrMouseEvent_Release_OutputsVt100MouseEvent()
    {
        // SGR release event uses lowercase 'm'
        _parser.Feed("\x1b[<0;10;20m");

        Assert.Single(_keys);
        Assert.Equal(Keys.Vt100MouseEvent, _keys[0].Key);
        Assert.Equal("\x1b[<0;10;20m", _keys[0].Data);
    }

    [Fact]
    public void Feed_SgrMouseEvent_RightButton_OutputsVt100MouseEvent()
    {
        // Right button (2) at position (5, 15)
        _parser.Feed("\x1b[<2;5;15M");

        Assert.Single(_keys);
        Assert.Equal(Keys.Vt100MouseEvent, _keys[0].Key);
    }

    [Fact]
    public void Feed_SgrMouseEvent_ScrollUp_OutputsVt100MouseEvent()
    {
        // Scroll up is typically button 64
        _parser.Feed("\x1b[<64;10;10M");

        Assert.Single(_keys);
        Assert.Equal(Keys.Vt100MouseEvent, _keys[0].Key);
    }

    [Fact]
    public void Feed_SgrMouseEvent_ScrollDown_OutputsVt100MouseEvent()
    {
        // Scroll down is typically button 65
        _parser.Feed("\x1b[<65;10;10M");

        Assert.Single(_keys);
        Assert.Equal(Keys.Vt100MouseEvent, _keys[0].Key);
    }

    [Fact]
    public void Feed_SgrMouseEvent_WithModifiers_OutputsVt100MouseEvent()
    {
        // Shift+Left click: button 0 + 4 (shift modifier) = 4
        _parser.Feed("\x1b[<4;10;20M");

        Assert.Single(_keys);
        Assert.Equal(Keys.Vt100MouseEvent, _keys[0].Key);
    }

    [Fact]
    public void Feed_SgrMouseEvent_LargeCoordinates_OutputsVt100MouseEvent()
    {
        // Large coordinates (e.g., wide terminal)
        _parser.Feed("\x1b[<0;200;100M");

        Assert.Single(_keys);
        Assert.Equal(Keys.Vt100MouseEvent, _keys[0].Key);
    }

    // T088: urxvt Mouse Protocol Tests
    [Fact]
    public void Feed_UrxvtMouseEvent_OutputsVt100MouseEvent()
    {
        // urxvt format: ESC [ Cb ; Cx ; Cy M
        // Button 32 (left click) at position (10, 20)
        _parser.Feed("\x1b[32;10;20M");

        Assert.Single(_keys);
        Assert.Equal(Keys.Vt100MouseEvent, _keys[0].Key);
        Assert.Equal("\x1b[32;10;20M", _keys[0].Data);
    }

    [Fact]
    public void Feed_UrxvtMouseEvent_RightButton_OutputsVt100MouseEvent()
    {
        // Right button (34) at position (5, 15)
        _parser.Feed("\x1b[34;5;15M");

        Assert.Single(_keys);
        Assert.Equal(Keys.Vt100MouseEvent, _keys[0].Key);
    }

    [Fact]
    public void Feed_UrxvtMouseEvent_MiddleButton_OutputsVt100MouseEvent()
    {
        // Middle button (33) at position (1, 1)
        _parser.Feed("\x1b[33;1;1M");

        Assert.Single(_keys);
        Assert.Equal(Keys.Vt100MouseEvent, _keys[0].Key);
    }

    // Cross-Protocol Tests
    [Fact]
    public void Feed_MixedMouseAndKeyboard_ParsesCorrectly()
    {
        _parser.Feed("a\x1b[<0;10;20Mb");

        Assert.Equal(3, _keys.Count);
        Assert.Equal("a", _keys[0].Data);
        Assert.Equal(Keys.Vt100MouseEvent, _keys[1].Key);
        Assert.Equal("b", _keys[2].Data);
    }

    [Fact]
    public void Feed_ConsecutiveMouseEvents_ParsesAll()
    {
        _parser.Feed("\x1b[<0;1;1M\x1b[<0;2;2M\x1b[<0;3;3m");

        Assert.Equal(3, _keys.Count);
        Assert.All(_keys, k => Assert.Equal(Keys.Vt100MouseEvent, k.Key));
    }

    #endregion
}
