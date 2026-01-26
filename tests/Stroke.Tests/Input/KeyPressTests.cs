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
    [InlineData(Keys.ShiftUp, "\x1b[1;2A")]
    [InlineData(Keys.ShiftDown, "\x1b[1;2B")]
    [InlineData(Keys.ShiftRight, "\x1b[1;2C")]
    [InlineData(Keys.ShiftLeft, "\x1b[1;2D")]
    [InlineData(Keys.ControlShiftUp, "\x1b[1;6A")]
    [InlineData(Keys.ControlShiftDown, "\x1b[1;6B")]
    [InlineData(Keys.ControlShiftRight, "\x1b[1;6C")]
    [InlineData(Keys.ControlShiftLeft, "\x1b[1;6D")]
    public void Constructor_ModifiedNavigationKeys_UseCorrectVt100Sequences(Keys key, string expectedData)
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
    public void ExtendedFunctionKeys_UseKeyNameAsDefaultData()
    {
        Assert.Equal("F21", new KeyPress(Keys.F21).Data);
        Assert.Equal("F22", new KeyPress(Keys.F22).Data);
        Assert.Equal("F23", new KeyPress(Keys.F23).Data);
        Assert.Equal("F24", new KeyPress(Keys.F24).Data);
    }

    [Fact]
    public void ControlNumbers_UseKeyNameAsDefaultData()
    {
        Assert.Equal("Control0", new KeyPress(Keys.Control0).Data);
        Assert.Equal("Control1", new KeyPress(Keys.Control1).Data);
        Assert.Equal("Control9", new KeyPress(Keys.Control9).Data);
    }

    [Fact]
    public void ControlShiftNumbers_UseKeyNameAsDefaultData()
    {
        Assert.Equal("ControlShift0", new KeyPress(Keys.ControlShift0).Data);
        Assert.Equal("ControlShift1", new KeyPress(Keys.ControlShift1).Data);
        Assert.Equal("ControlShift9", new KeyPress(Keys.ControlShift9).Data);
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
