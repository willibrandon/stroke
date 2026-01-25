// Copyright (c) 2025 Brandon Pugh. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Stroke.Input;
using Xunit;

namespace Stroke.Tests.Input;

/// <summary>
/// Tests for the <see cref="KeysExtensions"/> class.
/// </summary>
public class KeysExtensionsTests
{
    // ==========================================
    // T009: ToKeyString - Escape Keys
    // ==========================================

    [Fact]
    public void ToKeyString_Escape_ReturnsEscape()
    {
        Assert.Equal("escape", Keys.Escape.ToKeyString());
    }

    [Fact]
    public void ToKeyString_ShiftEscape_ReturnsSEscape()
    {
        Assert.Equal("s-escape", Keys.ShiftEscape.ToKeyString());
    }

    // ==========================================
    // T010: ToKeyString - Control Characters
    // ==========================================

    [Fact]
    public void ToKeyString_ControlAt_ReturnsCAt()
    {
        Assert.Equal("c-@", Keys.ControlAt.ToKeyString());
    }

    [Fact]
    public void ToKeyString_ControlA_ReturnsCA()
    {
        Assert.Equal("c-a", Keys.ControlA.ToKeyString());
    }

    [Fact]
    public void ToKeyString_ControlZ_ReturnsCZ()
    {
        Assert.Equal("c-z", Keys.ControlZ.ToKeyString());
    }

    [Fact]
    public void ToKeyString_ControlBackslash_ReturnsCBackslash()
    {
        Assert.Equal("c-\\", Keys.ControlBackslash.ToKeyString());
    }

    [Fact]
    public void ToKeyString_ControlSquareClose_ReturnsCSquareBracket()
    {
        Assert.Equal("c-]", Keys.ControlSquareClose.ToKeyString());
    }

    [Fact]
    public void ToKeyString_ControlCircumflex_ReturnsCCircumflex()
    {
        Assert.Equal("c-^", Keys.ControlCircumflex.ToKeyString());
    }

    [Fact]
    public void ToKeyString_ControlUnderscore_ReturnsCUnderscore()
    {
        Assert.Equal("c-_", Keys.ControlUnderscore.ToKeyString());
    }

    [Theory]
    [InlineData(Keys.ControlA, "c-a")]
    [InlineData(Keys.ControlB, "c-b")]
    [InlineData(Keys.ControlC, "c-c")]
    [InlineData(Keys.ControlD, "c-d")]
    [InlineData(Keys.ControlE, "c-e")]
    [InlineData(Keys.ControlF, "c-f")]
    [InlineData(Keys.ControlG, "c-g")]
    [InlineData(Keys.ControlH, "c-h")]
    [InlineData(Keys.ControlI, "c-i")]
    [InlineData(Keys.ControlJ, "c-j")]
    [InlineData(Keys.ControlK, "c-k")]
    [InlineData(Keys.ControlL, "c-l")]
    [InlineData(Keys.ControlM, "c-m")]
    [InlineData(Keys.ControlN, "c-n")]
    [InlineData(Keys.ControlO, "c-o")]
    [InlineData(Keys.ControlP, "c-p")]
    [InlineData(Keys.ControlQ, "c-q")]
    [InlineData(Keys.ControlR, "c-r")]
    [InlineData(Keys.ControlS, "c-s")]
    [InlineData(Keys.ControlT, "c-t")]
    [InlineData(Keys.ControlU, "c-u")]
    [InlineData(Keys.ControlV, "c-v")]
    [InlineData(Keys.ControlW, "c-w")]
    [InlineData(Keys.ControlX, "c-x")]
    [InlineData(Keys.ControlY, "c-y")]
    [InlineData(Keys.ControlZ, "c-z")]
    public void ToKeyString_ControlLetters_ReturnsCorrectString(Keys key, string expected)
    {
        Assert.Equal(expected, key.ToKeyString());
    }

    // ==========================================
    // T011: ToKeyString - Control + Numbers and ControlShift + Numbers
    // ==========================================

    [Theory]
    [InlineData(Keys.Control1, "c-1")]
    [InlineData(Keys.Control2, "c-2")]
    [InlineData(Keys.Control3, "c-3")]
    [InlineData(Keys.Control4, "c-4")]
    [InlineData(Keys.Control5, "c-5")]
    [InlineData(Keys.Control6, "c-6")]
    [InlineData(Keys.Control7, "c-7")]
    [InlineData(Keys.Control8, "c-8")]
    [InlineData(Keys.Control9, "c-9")]
    [InlineData(Keys.Control0, "c-0")]
    public void ToKeyString_ControlNumbers_ReturnsCorrectString(Keys key, string expected)
    {
        Assert.Equal(expected, key.ToKeyString());
    }

    [Theory]
    [InlineData(Keys.ControlShift1, "c-s-1")]
    [InlineData(Keys.ControlShift2, "c-s-2")]
    [InlineData(Keys.ControlShift3, "c-s-3")]
    [InlineData(Keys.ControlShift4, "c-s-4")]
    [InlineData(Keys.ControlShift5, "c-s-5")]
    [InlineData(Keys.ControlShift6, "c-s-6")]
    [InlineData(Keys.ControlShift7, "c-s-7")]
    [InlineData(Keys.ControlShift8, "c-s-8")]
    [InlineData(Keys.ControlShift9, "c-s-9")]
    [InlineData(Keys.ControlShift0, "c-s-0")]
    public void ToKeyString_ControlShiftNumbers_ReturnsCorrectString(Keys key, string expected)
    {
        Assert.Equal(expected, key.ToKeyString());
    }

    // ==========================================
    // T012: ToKeyString - Navigation Keys and Modifier Combinations
    // ==========================================

    [Theory]
    [InlineData(Keys.Left, "left")]
    [InlineData(Keys.Right, "right")]
    [InlineData(Keys.Up, "up")]
    [InlineData(Keys.Down, "down")]
    [InlineData(Keys.Home, "home")]
    [InlineData(Keys.End, "end")]
    [InlineData(Keys.Insert, "insert")]
    [InlineData(Keys.Delete, "delete")]
    [InlineData(Keys.PageUp, "pageup")]
    [InlineData(Keys.PageDown, "pagedown")]
    public void ToKeyString_NavigationKeys_ReturnsCorrectString(Keys key, string expected)
    {
        Assert.Equal(expected, key.ToKeyString());
    }

    [Theory]
    [InlineData(Keys.ControlLeft, "c-left")]
    [InlineData(Keys.ControlRight, "c-right")]
    [InlineData(Keys.ControlUp, "c-up")]
    [InlineData(Keys.ControlDown, "c-down")]
    [InlineData(Keys.ControlHome, "c-home")]
    [InlineData(Keys.ControlEnd, "c-end")]
    [InlineData(Keys.ControlInsert, "c-insert")]
    [InlineData(Keys.ControlDelete, "c-delete")]
    [InlineData(Keys.ControlPageUp, "c-pageup")]
    [InlineData(Keys.ControlPageDown, "c-pagedown")]
    public void ToKeyString_ControlNavigation_ReturnsCorrectString(Keys key, string expected)
    {
        Assert.Equal(expected, key.ToKeyString());
    }

    [Theory]
    [InlineData(Keys.ShiftLeft, "s-left")]
    [InlineData(Keys.ShiftRight, "s-right")]
    [InlineData(Keys.ShiftUp, "s-up")]
    [InlineData(Keys.ShiftDown, "s-down")]
    [InlineData(Keys.ShiftHome, "s-home")]
    [InlineData(Keys.ShiftEnd, "s-end")]
    [InlineData(Keys.ShiftInsert, "s-insert")]
    [InlineData(Keys.ShiftDelete, "s-delete")]
    [InlineData(Keys.ShiftPageUp, "s-pageup")]
    [InlineData(Keys.ShiftPageDown, "s-pagedown")]
    public void ToKeyString_ShiftNavigation_ReturnsCorrectString(Keys key, string expected)
    {
        Assert.Equal(expected, key.ToKeyString());
    }

    [Theory]
    [InlineData(Keys.ControlShiftLeft, "c-s-left")]
    [InlineData(Keys.ControlShiftRight, "c-s-right")]
    [InlineData(Keys.ControlShiftUp, "c-s-up")]
    [InlineData(Keys.ControlShiftDown, "c-s-down")]
    [InlineData(Keys.ControlShiftHome, "c-s-home")]
    [InlineData(Keys.ControlShiftEnd, "c-s-end")]
    [InlineData(Keys.ControlShiftInsert, "c-s-insert")]
    [InlineData(Keys.ControlShiftDelete, "c-s-delete")]
    [InlineData(Keys.ControlShiftPageUp, "c-s-pageup")]
    [InlineData(Keys.ControlShiftPageDown, "c-s-pagedown")]
    public void ToKeyString_ControlShiftNavigation_ReturnsCorrectString(Keys key, string expected)
    {
        Assert.Equal(expected, key.ToKeyString());
    }

    [Fact]
    public void ToKeyString_BackTab_ReturnsSTab()
    {
        Assert.Equal("s-tab", Keys.BackTab.ToKeyString());
    }

    // ==========================================
    // T013: ToKeyString - Function Keys and Control + Function Keys
    // ==========================================

    [Theory]
    [InlineData(Keys.F1, "f1")]
    [InlineData(Keys.F2, "f2")]
    [InlineData(Keys.F3, "f3")]
    [InlineData(Keys.F4, "f4")]
    [InlineData(Keys.F5, "f5")]
    [InlineData(Keys.F6, "f6")]
    [InlineData(Keys.F7, "f7")]
    [InlineData(Keys.F8, "f8")]
    [InlineData(Keys.F9, "f9")]
    [InlineData(Keys.F10, "f10")]
    [InlineData(Keys.F11, "f11")]
    [InlineData(Keys.F12, "f12")]
    [InlineData(Keys.F13, "f13")]
    [InlineData(Keys.F14, "f14")]
    [InlineData(Keys.F15, "f15")]
    [InlineData(Keys.F16, "f16")]
    [InlineData(Keys.F17, "f17")]
    [InlineData(Keys.F18, "f18")]
    [InlineData(Keys.F19, "f19")]
    [InlineData(Keys.F20, "f20")]
    [InlineData(Keys.F21, "f21")]
    [InlineData(Keys.F22, "f22")]
    [InlineData(Keys.F23, "f23")]
    [InlineData(Keys.F24, "f24")]
    public void ToKeyString_FunctionKeys_ReturnsCorrectString(Keys key, string expected)
    {
        Assert.Equal(expected, key.ToKeyString());
    }

    [Theory]
    [InlineData(Keys.ControlF1, "c-f1")]
    [InlineData(Keys.ControlF2, "c-f2")]
    [InlineData(Keys.ControlF3, "c-f3")]
    [InlineData(Keys.ControlF4, "c-f4")]
    [InlineData(Keys.ControlF5, "c-f5")]
    [InlineData(Keys.ControlF6, "c-f6")]
    [InlineData(Keys.ControlF7, "c-f7")]
    [InlineData(Keys.ControlF8, "c-f8")]
    [InlineData(Keys.ControlF9, "c-f9")]
    [InlineData(Keys.ControlF10, "c-f10")]
    [InlineData(Keys.ControlF11, "c-f11")]
    [InlineData(Keys.ControlF12, "c-f12")]
    [InlineData(Keys.ControlF13, "c-f13")]
    [InlineData(Keys.ControlF14, "c-f14")]
    [InlineData(Keys.ControlF15, "c-f15")]
    [InlineData(Keys.ControlF16, "c-f16")]
    [InlineData(Keys.ControlF17, "c-f17")]
    [InlineData(Keys.ControlF18, "c-f18")]
    [InlineData(Keys.ControlF19, "c-f19")]
    [InlineData(Keys.ControlF20, "c-f20")]
    [InlineData(Keys.ControlF21, "c-f21")]
    [InlineData(Keys.ControlF22, "c-f22")]
    [InlineData(Keys.ControlF23, "c-f23")]
    [InlineData(Keys.ControlF24, "c-f24")]
    public void ToKeyString_ControlFunctionKeys_ReturnsCorrectString(Keys key, string expected)
    {
        Assert.Equal(expected, key.ToKeyString());
    }

    // ==========================================
    // T014: ToKeyString - Special Keys with Angle Bracket Notation
    // ==========================================

    [Fact]
    public void ToKeyString_Any_ReturnsAngleBracketAny()
    {
        Assert.Equal("<any>", Keys.Any.ToKeyString());
    }

    [Fact]
    public void ToKeyString_ScrollUp_ReturnsAngleBracketScrollUp()
    {
        Assert.Equal("<scroll-up>", Keys.ScrollUp.ToKeyString());
    }

    [Fact]
    public void ToKeyString_ScrollDown_ReturnsAngleBracketScrollDown()
    {
        Assert.Equal("<scroll-down>", Keys.ScrollDown.ToKeyString());
    }

    [Fact]
    public void ToKeyString_CPRResponse_ReturnsAngleBracketCursorPositionResponse()
    {
        Assert.Equal("<cursor-position-response>", Keys.CPRResponse.ToKeyString());
    }

    [Fact]
    public void ToKeyString_Vt100MouseEvent_ReturnsAngleBracketVt100MouseEvent()
    {
        Assert.Equal("<vt100-mouse-event>", Keys.Vt100MouseEvent.ToKeyString());
    }

    [Fact]
    public void ToKeyString_WindowsMouseEvent_ReturnsAngleBracketWindowsMouseEvent()
    {
        Assert.Equal("<windows-mouse-event>", Keys.WindowsMouseEvent.ToKeyString());
    }

    [Fact]
    public void ToKeyString_BracketedPaste_ReturnsAngleBracketBracketedPaste()
    {
        Assert.Equal("<bracketed-paste>", Keys.BracketedPaste.ToKeyString());
    }

    [Fact]
    public void ToKeyString_SIGINT_ReturnsAngleBracketSigint()
    {
        Assert.Equal("<sigint>", Keys.SIGINT.ToKeyString());
    }

    [Fact]
    public void ToKeyString_Ignore_ReturnsAngleBracketIgnore()
    {
        Assert.Equal("<ignore>", Keys.Ignore.ToKeyString());
    }

    // ==========================================
    // T015: ToKeyString - Throws for Invalid Enum Value
    // ==========================================

    [Fact]
    public void ToKeyString_InvalidEnumValue_ThrowsArgumentOutOfRangeException()
    {
        var invalidKey = (Keys)9999;
        Assert.Throws<ArgumentOutOfRangeException>(() => invalidKey.ToKeyString());
    }

    // ==========================================
    // T020-T025: ParseKey Tests (User Story 2)
    // ==========================================

    // T020: Parse canonical strings
    [Theory]
    [InlineData("escape", Keys.Escape)]
    [InlineData("s-escape", Keys.ShiftEscape)]
    [InlineData("c-@", Keys.ControlAt)]
    [InlineData("c-a", Keys.ControlA)]
    [InlineData("c-z", Keys.ControlZ)]
    [InlineData("c-\\", Keys.ControlBackslash)]
    [InlineData("c-]", Keys.ControlSquareClose)]
    [InlineData("c-^", Keys.ControlCircumflex)]
    [InlineData("c-_", Keys.ControlUnderscore)]
    [InlineData("c-1", Keys.Control1)]
    [InlineData("c-0", Keys.Control0)]
    [InlineData("c-s-1", Keys.ControlShift1)]
    [InlineData("c-s-0", Keys.ControlShift0)]
    [InlineData("left", Keys.Left)]
    [InlineData("right", Keys.Right)]
    [InlineData("up", Keys.Up)]
    [InlineData("down", Keys.Down)]
    [InlineData("home", Keys.Home)]
    [InlineData("end", Keys.End)]
    [InlineData("insert", Keys.Insert)]
    [InlineData("delete", Keys.Delete)]
    [InlineData("pageup", Keys.PageUp)]
    [InlineData("pagedown", Keys.PageDown)]
    [InlineData("c-left", Keys.ControlLeft)]
    [InlineData("c-pagedown", Keys.ControlPageDown)]
    [InlineData("s-left", Keys.ShiftLeft)]
    [InlineData("s-pagedown", Keys.ShiftPageDown)]
    [InlineData("c-s-left", Keys.ControlShiftLeft)]
    [InlineData("c-s-pagedown", Keys.ControlShiftPageDown)]
    [InlineData("s-tab", Keys.BackTab)]
    [InlineData("f1", Keys.F1)]
    [InlineData("f24", Keys.F24)]
    [InlineData("c-f1", Keys.ControlF1)]
    [InlineData("c-f24", Keys.ControlF24)]
    [InlineData("<any>", Keys.Any)]
    [InlineData("<scroll-up>", Keys.ScrollUp)]
    [InlineData("<scroll-down>", Keys.ScrollDown)]
    [InlineData("<cursor-position-response>", Keys.CPRResponse)]
    [InlineData("<vt100-mouse-event>", Keys.Vt100MouseEvent)]
    [InlineData("<windows-mouse-event>", Keys.WindowsMouseEvent)]
    [InlineData("<bracketed-paste>", Keys.BracketedPaste)]
    [InlineData("<sigint>", Keys.SIGINT)]
    [InlineData("<ignore>", Keys.Ignore)]
    public void ParseKey_CanonicalStrings_ReturnsCorrectKey(string keyString, Keys expected)
    {
        var result = KeysExtensions.ParseKey(keyString);
        Assert.Equal(expected, result);
    }

    // T021: Case insensitive parsing
    [Theory]
    [InlineData("C-A", Keys.ControlA)]
    [InlineData("c-A", Keys.ControlA)]
    [InlineData("C-a", Keys.ControlA)]
    [InlineData("ESCAPE", Keys.Escape)]
    [InlineData("Escape", Keys.Escape)]
    [InlineData("LEFT", Keys.Left)]
    [InlineData("Left", Keys.Left)]
    [InlineData("F1", Keys.F1)]
    [InlineData("<ANY>", Keys.Any)]
    [InlineData("<Any>", Keys.Any)]
    public void ParseKey_CaseInsensitive_ReturnsCorrectKey(string keyString, Keys expected)
    {
        var result = KeysExtensions.ParseKey(keyString);
        Assert.Equal(expected, result);
    }

    // T022: Invalid string returns null
    [Theory]
    [InlineData("invalid-key")]
    [InlineData("not-a-key")]
    [InlineData("c-invalid")]
    [InlineData("xyz")]
    public void ParseKey_InvalidString_ReturnsNull(string keyString)
    {
        var result = KeysExtensions.ParseKey(keyString);
        Assert.Null(result);
    }

    // T023: Empty string returns null
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void ParseKey_EmptyOrNullString_ReturnsNull(string? keyString)
    {
        var result = KeysExtensions.ParseKey(keyString!);
        Assert.Null(result);
    }

    // T024: Alias strings resolve correctly
    [Theory]
    [InlineData("enter", Keys.ControlM)]
    [InlineData("tab", Keys.ControlI)]
    [InlineData("backspace", Keys.ControlH)]
    [InlineData("c-space", Keys.ControlAt)]
    [InlineData("s-c-left", Keys.ControlShiftLeft)]
    [InlineData("s-c-right", Keys.ControlShiftRight)]
    [InlineData("s-c-home", Keys.ControlShiftHome)]
    [InlineData("s-c-end", Keys.ControlShiftEnd)]
    // Case insensitive aliases
    [InlineData("ENTER", Keys.ControlM)]
    [InlineData("TAB", Keys.ControlI)]
    [InlineData("BACKSPACE", Keys.ControlH)]
    [InlineData("C-SPACE", Keys.ControlAt)]
    [InlineData("S-C-LEFT", Keys.ControlShiftLeft)]
    public void ParseKey_AliasStrings_ReturnsCorrectKey(string keyString, Keys expected)
    {
        var result = KeysExtensions.ParseKey(keyString);
        Assert.Equal(expected, result);
    }

    // T025: Round-trip conversion
    [Fact]
    public void RoundTrip_EnumToStringToEnum_AllKeysSucceed()
    {
        foreach (var key in Enum.GetValues<Keys>())
        {
            var keyString = key.ToKeyString();
            var parsed = KeysExtensions.ParseKey(keyString);

            Assert.NotNull(parsed);
            Assert.Equal(key, parsed.Value);
        }
    }
}
