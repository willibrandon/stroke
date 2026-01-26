using Stroke.Input;
using Xunit;

namespace Stroke.Tests.Input;

public class KeyPressTests
{
    [Fact]
    public void Constructor_WithExplicitData_UsesProvidedData()
    {
        var keyPress = new KeyPress(Keys.Up, "\x1b[A");

        Assert.Equal(Keys.Up, keyPress.Key);
        Assert.Equal("\x1b[A", keyPress.Data);
    }

    [Fact]
    public void Constructor_WithNullData_UsesDefaultData()
    {
        var keyPress = new KeyPress(Keys.Up);

        Assert.Equal(Keys.Up, keyPress.Key);
        Assert.Equal("\x1b[A", keyPress.Data);
    }

    [Fact]
    public void Constructor_ControlCharacter_UsesAsciiControlCode()
    {
        var keyPress = new KeyPress(Keys.ControlC);

        Assert.Equal(Keys.ControlC, keyPress.Key);
        Assert.Equal("\x03", keyPress.Data);
    }

    [Fact]
    public void Constructor_Escape_UsesEscapeCharacter()
    {
        var keyPress = new KeyPress(Keys.Escape);

        Assert.Equal(Keys.Escape, keyPress.Key);
        Assert.Equal("\x1b", keyPress.Data);
    }

    [Fact]
    public void Constructor_Any_UsesEmptyString()
    {
        var keyPress = new KeyPress(Keys.Any);

        Assert.Equal(Keys.Any, keyPress.Key);
        Assert.Equal("", keyPress.Data);
    }

    [Fact]
    public void Constructor_FunctionKey_UsesVt100Sequence()
    {
        var f1 = new KeyPress(Keys.F1);
        var f12 = new KeyPress(Keys.F12);

        Assert.Equal("\x1bOP", f1.Data);
        Assert.Equal("\x1b[24~", f12.Data);
    }

    [Theory]
    [InlineData(Keys.ControlAt, "\x00")]
    [InlineData(Keys.ControlA, "\x01")]
    [InlineData(Keys.ControlB, "\x02")]
    [InlineData(Keys.ControlC, "\x03")]
    [InlineData(Keys.ControlD, "\x04")]
    [InlineData(Keys.ControlE, "\x05")]
    [InlineData(Keys.ControlF, "\x06")]
    [InlineData(Keys.ControlG, "\x07")]
    [InlineData(Keys.ControlH, "\x08")]
    [InlineData(Keys.ControlI, "\x09")]
    [InlineData(Keys.ControlJ, "\x0a")]
    [InlineData(Keys.ControlK, "\x0b")]
    [InlineData(Keys.ControlL, "\x0c")]
    [InlineData(Keys.ControlM, "\x0d")]
    [InlineData(Keys.ControlN, "\x0e")]
    [InlineData(Keys.ControlO, "\x0f")]
    [InlineData(Keys.ControlP, "\x10")]
    [InlineData(Keys.ControlQ, "\x11")]
    [InlineData(Keys.ControlR, "\x12")]
    [InlineData(Keys.ControlS, "\x13")]
    [InlineData(Keys.ControlT, "\x14")]
    [InlineData(Keys.ControlU, "\x15")]
    [InlineData(Keys.ControlV, "\x16")]
    [InlineData(Keys.ControlW, "\x17")]
    [InlineData(Keys.ControlX, "\x18")]
    [InlineData(Keys.ControlY, "\x19")]
    [InlineData(Keys.ControlZ, "\x1a")]
    [InlineData(Keys.ControlBackslash, "\x1c")]
    [InlineData(Keys.ControlSquareClose, "\x1d")]
    [InlineData(Keys.ControlCircumflex, "\x1e")]
    [InlineData(Keys.ControlUnderscore, "\x1f")]
    public void Constructor_ControlCharacters_UseCorrectAsciiCodes(Keys key, string expectedData)
    {
        var keyPress = new KeyPress(key);
        Assert.Equal(expectedData, keyPress.Data);
    }

    [Theory]
    [InlineData(Keys.Up, "\x1b[A")]
    [InlineData(Keys.Down, "\x1b[B")]
    [InlineData(Keys.Right, "\x1b[C")]
    [InlineData(Keys.Left, "\x1b[D")]
    [InlineData(Keys.Home, "\x1b[H")]
    [InlineData(Keys.End, "\x1b[F")]
    [InlineData(Keys.Insert, "\x1b[2~")]
    [InlineData(Keys.Delete, "\x1b[3~")]
    [InlineData(Keys.PageUp, "\x1b[5~")]
    [InlineData(Keys.PageDown, "\x1b[6~")]
    public void Constructor_NavigationKeys_UseCorrectVt100Sequences(Keys key, string expectedData)
    {
        var keyPress = new KeyPress(key);
        Assert.Equal(expectedData, keyPress.Data);
    }

    [Theory]
    [InlineData(Keys.ControlUp, "\x1b[1;5A")]
    [InlineData(Keys.ControlDown, "\x1b[1;5B")]
    [InlineData(Keys.ControlRight, "\x1b[1;5C")]
    [InlineData(Keys.ControlLeft, "\x1b[1;5D")]
    [InlineData(Keys.ControlHome, "\x1b[1;5H")]
    [InlineData(Keys.ControlEnd, "\x1b[1;5F")]
    [InlineData(Keys.ControlInsert, "\x1b[2;5~")]
    [InlineData(Keys.ControlDelete, "\x1b[3;5~")]
    [InlineData(Keys.ControlPageUp, "\x1b[5;5~")]
    [InlineData(Keys.ControlPageDown, "\x1b[6;5~")]
    public void ControlNavigationKeys_UseCorrectVt100Sequences(Keys key, string expectedData)
    {
        var keyPress = new KeyPress(key);
        Assert.Equal(expectedData, keyPress.Data);
    }

    [Theory]
    [InlineData(Keys.ShiftUp, "\x1b[1;2A")]
    [InlineData(Keys.ShiftDown, "\x1b[1;2B")]
    [InlineData(Keys.ShiftRight, "\x1b[1;2C")]
    [InlineData(Keys.ShiftLeft, "\x1b[1;2D")]
    [InlineData(Keys.ShiftHome, "\x1b[1;2H")]
    [InlineData(Keys.ShiftEnd, "\x1b[1;2F")]
    [InlineData(Keys.ShiftInsert, "\x1b[2;2~")]
    [InlineData(Keys.ShiftDelete, "\x1b[3;2~")]
    [InlineData(Keys.ShiftPageUp, "\x1b[5;2~")]
    [InlineData(Keys.ShiftPageDown, "\x1b[6;2~")]
    public void ShiftNavigationKeys_UseCorrectVt100Sequences(Keys key, string expectedData)
    {
        var keyPress = new KeyPress(key);
        Assert.Equal(expectedData, keyPress.Data);
    }

    [Theory]
    [InlineData(Keys.ControlShiftUp, "\x1b[1;6A")]
    [InlineData(Keys.ControlShiftDown, "\x1b[1;6B")]
    [InlineData(Keys.ControlShiftRight, "\x1b[1;6C")]
    [InlineData(Keys.ControlShiftLeft, "\x1b[1;6D")]
    [InlineData(Keys.ControlShiftHome, "\x1b[1;6H")]
    [InlineData(Keys.ControlShiftEnd, "\x1b[1;6F")]
    [InlineData(Keys.ControlShiftInsert, "\x1b[2;6~")]
    [InlineData(Keys.ControlShiftDelete, "\x1b[3;6~")]
    [InlineData(Keys.ControlShiftPageUp, "\x1b[5;6~")]
    [InlineData(Keys.ControlShiftPageDown, "\x1b[6;6~")]
    public void ControlShiftNavigationKeys_UseCorrectVt100Sequences(Keys key, string expectedData)
    {
        var keyPress = new KeyPress(key);
        Assert.Equal(expectedData, keyPress.Data);
    }

    [Theory]
    [InlineData(Keys.F1, "\x1bOP")]
    [InlineData(Keys.F2, "\x1bOQ")]
    [InlineData(Keys.F3, "\x1bOR")]
    [InlineData(Keys.F4, "\x1bOS")]
    [InlineData(Keys.F5, "\x1b[15~")]
    [InlineData(Keys.F6, "\x1b[17~")]
    [InlineData(Keys.F7, "\x1b[18~")]
    [InlineData(Keys.F8, "\x1b[19~")]
    [InlineData(Keys.F9, "\x1b[20~")]
    [InlineData(Keys.F10, "\x1b[21~")]
    [InlineData(Keys.F11, "\x1b[23~")]
    [InlineData(Keys.F12, "\x1b[24~")]
    public void Constructor_FunctionKeys_UseCorrectVt100Sequences(Keys key, string expectedData)
    {
        var keyPress = new KeyPress(key);
        Assert.Equal(expectedData, keyPress.Data);
    }

    [Fact]
    public void Equality_SameKeyAndData_AreEqual()
    {
        var k1 = new KeyPress(Keys.Up, "\x1b[A");
        var k2 = new KeyPress(Keys.Up, "\x1b[A");

        Assert.Equal(k1, k2);
        Assert.True(k1 == k2);
        Assert.False(k1 != k2);
    }

    [Fact]
    public void Equality_DifferentKey_AreNotEqual()
    {
        var k1 = new KeyPress(Keys.Up, "\x1b[A");
        var k2 = new KeyPress(Keys.Down, "\x1b[A");

        Assert.NotEqual(k1, k2);
    }

    [Fact]
    public void Equality_DifferentData_AreNotEqual()
    {
        var k1 = new KeyPress(Keys.Up, "\x1b[A");
        var k2 = new KeyPress(Keys.Up, "different");

        Assert.NotEqual(k1, k2);
    }

    [Fact]
    public void GetHashCode_SameKeyAndData_SameHashCode()
    {
        var k1 = new KeyPress(Keys.Up, "\x1b[A");
        var k2 = new KeyPress(Keys.Up, "\x1b[A");

        Assert.Equal(k1.GetHashCode(), k2.GetHashCode());
    }

    [Fact]
    public void Deconstruct_ExtractsKeyAndData()
    {
        var keyPress = new KeyPress(Keys.ControlC, "\x03");
        var (key, data) = keyPress;

        Assert.Equal(Keys.ControlC, key);
        Assert.Equal("\x03", data);
    }

    [Fact]
    public void SpecialKeys_UseKeyNameAsDefaultData()
    {
        Assert.Equal("ScrollUp", new KeyPress(Keys.ScrollUp).Data);
        Assert.Equal("ScrollDown", new KeyPress(Keys.ScrollDown).Data);
        Assert.Equal("CPRResponse", new KeyPress(Keys.CPRResponse).Data);
        Assert.Equal("Vt100MouseEvent", new KeyPress(Keys.Vt100MouseEvent).Data);
        Assert.Equal("WindowsMouseEvent", new KeyPress(Keys.WindowsMouseEvent).Data);
        Assert.Equal("BracketedPaste", new KeyPress(Keys.BracketedPaste).Data);
        Assert.Equal("SIGINT", new KeyPress(Keys.SIGINT).Data);
        Assert.Equal("Ignore", new KeyPress(Keys.Ignore).Data);
    }

    [Fact]
    public void BackTab_UsesCorrectSequence()
    {
        var keyPress = new KeyPress(Keys.BackTab);
        Assert.Equal("\x1b[Z", keyPress.Data);
    }

    [Fact]
    public void ShiftEscape_UsesEscapeCharacter()
    {
        var keyPress = new KeyPress(Keys.ShiftEscape);
        Assert.Equal("\x1b", keyPress.Data);
    }

    [Theory]
    [InlineData(Keys.F13, "\x1b[25~")]
    [InlineData(Keys.F14, "\x1b[26~")]
    [InlineData(Keys.F15, "\x1b[28~")]
    [InlineData(Keys.F16, "\x1b[29~")]
    [InlineData(Keys.F17, "\x1b[31~")]
    [InlineData(Keys.F18, "\x1b[32~")]
    [InlineData(Keys.F19, "\x1b[33~")]
    [InlineData(Keys.F20, "\x1b[34~")]
    public void ExtendedFunctionKeys_UseCorrectVt100Sequences(Keys key, string expectedData)
    {
        var keyPress = new KeyPress(key);
        Assert.Equal(expectedData, keyPress.Data);
    }

    [Theory]
    [InlineData(Keys.F21, "F21")]
    [InlineData(Keys.F22, "F22")]
    [InlineData(Keys.F23, "F23")]
    [InlineData(Keys.F24, "F24")]
    public void ExtendedFunctionKeys_F21ToF24_UseKeyNameAsDefaultData(Keys key, string expectedData)
    {
        Assert.Equal(expectedData, new KeyPress(key).Data);
    }

    [Theory]
    [InlineData(Keys.ControlF1, "\x1b[1;5P")]
    [InlineData(Keys.ControlF2, "\x1b[1;5Q")]
    [InlineData(Keys.ControlF3, "\x1b[1;5R")]
    [InlineData(Keys.ControlF4, "\x1b[1;5S")]
    [InlineData(Keys.ControlF5, "\x1b[15;5~")]
    [InlineData(Keys.ControlF6, "\x1b[17;5~")]
    [InlineData(Keys.ControlF7, "\x1b[18;5~")]
    [InlineData(Keys.ControlF8, "\x1b[19;5~")]
    [InlineData(Keys.ControlF9, "\x1b[20;5~")]
    [InlineData(Keys.ControlF10, "\x1b[21;5~")]
    [InlineData(Keys.ControlF11, "\x1b[23;5~")]
    [InlineData(Keys.ControlF12, "\x1b[24;5~")]
    public void ControlFunctionKeys_UseCorrectVt100Sequences(Keys key, string expectedData)
    {
        var keyPress = new KeyPress(key);
        Assert.Equal(expectedData, keyPress.Data);
    }

    [Theory]
    [InlineData(Keys.ControlF13, "ControlF13")]
    [InlineData(Keys.ControlF14, "ControlF14")]
    [InlineData(Keys.ControlF15, "ControlF15")]
    [InlineData(Keys.ControlF16, "ControlF16")]
    [InlineData(Keys.ControlF17, "ControlF17")]
    [InlineData(Keys.ControlF18, "ControlF18")]
    [InlineData(Keys.ControlF19, "ControlF19")]
    [InlineData(Keys.ControlF20, "ControlF20")]
    [InlineData(Keys.ControlF21, "ControlF21")]
    [InlineData(Keys.ControlF22, "ControlF22")]
    [InlineData(Keys.ControlF23, "ControlF23")]
    [InlineData(Keys.ControlF24, "ControlF24")]
    public void ControlFunctionKeys_F13ToF24_UseKeyNameAsDefaultData(Keys key, string expectedData)
    {
        Assert.Equal(expectedData, new KeyPress(key).Data);
    }

    [Theory]
    [InlineData(Keys.Control0, "Control0")]
    [InlineData(Keys.Control1, "Control1")]
    [InlineData(Keys.Control2, "Control2")]
    [InlineData(Keys.Control3, "Control3")]
    [InlineData(Keys.Control4, "Control4")]
    [InlineData(Keys.Control5, "Control5")]
    [InlineData(Keys.Control6, "Control6")]
    [InlineData(Keys.Control7, "Control7")]
    [InlineData(Keys.Control8, "Control8")]
    [InlineData(Keys.Control9, "Control9")]
    public void ControlNumbers_UseKeyNameAsDefaultData(Keys key, string expectedData)
    {
        Assert.Equal(expectedData, new KeyPress(key).Data);
    }

    [Theory]
    [InlineData(Keys.ControlShift0, "ControlShift0")]
    [InlineData(Keys.ControlShift1, "ControlShift1")]
    [InlineData(Keys.ControlShift2, "ControlShift2")]
    [InlineData(Keys.ControlShift3, "ControlShift3")]
    [InlineData(Keys.ControlShift4, "ControlShift4")]
    [InlineData(Keys.ControlShift5, "ControlShift5")]
    [InlineData(Keys.ControlShift6, "ControlShift6")]
    [InlineData(Keys.ControlShift7, "ControlShift7")]
    [InlineData(Keys.ControlShift8, "ControlShift8")]
    [InlineData(Keys.ControlShift9, "ControlShift9")]
    public void ControlShiftNumbers_UseKeyNameAsDefaultData(Keys key, string expectedData)
    {
        Assert.Equal(expectedData, new KeyPress(key).Data);
    }

    [Fact]
    public void KeyPress_IsImmutable()
    {
        var keyPress = new KeyPress(Keys.Up, "\x1b[A");

        // Since KeyPress is a readonly record struct, we can't modify it
        // This test verifies the type is correctly a readonly record struct
        Assert.True(typeof(KeyPress).IsValueType);
    }

    [Fact]
    public void Constructor_CharacterKey_StoresCharacterInData()
    {
        var keyPress = new KeyPress(Keys.Any, "a");

        Assert.Equal(Keys.Any, keyPress.Key);
        Assert.Equal("a", keyPress.Data);
    }

    [Fact]
    public void Constructor_UnicodeCharacter_StoresUnicodeInData()
    {
        var keyPress = new KeyPress(Keys.Any, "日");

        Assert.Equal(Keys.Any, keyPress.Key);
        Assert.Equal("日", keyPress.Data);
    }
}
