using Stroke.Input.Windows.Win32Types;
using Xunit;

namespace Stroke.Tests.Input.Windows.Win32Types;

/// <summary>
/// Tests for all enum types in Win32Types namespace.
/// </summary>
public sealed class EnumTests
{
    #region EventType Tests

    [Theory]
    [InlineData(EventType.KeyEvent, 0x0001)]
    [InlineData(EventType.MouseEvent, 0x0002)]
    [InlineData(EventType.WindowBufferSizeEvent, 0x0004)]
    [InlineData(EventType.MenuEvent, 0x0008)]
    [InlineData(EventType.FocusEvent, 0x0010)]
    public void EventType_HasCorrectValues(EventType value, int expected)
    {
        Assert.Equal(expected, (int)value);
    }

    [Fact]
    public void EventType_BackingType_IsUshort()
    {
        Assert.Equal(typeof(ushort), Enum.GetUnderlyingType(typeof(EventType)));
    }

    #endregion

    #region ControlKeyState Tests

    [Theory]
    [InlineData(ControlKeyState.None, 0x0000)]
    [InlineData(ControlKeyState.RightAltPressed, 0x0001)]
    [InlineData(ControlKeyState.LeftAltPressed, 0x0002)]
    [InlineData(ControlKeyState.RightCtrlPressed, 0x0004)]
    [InlineData(ControlKeyState.LeftCtrlPressed, 0x0008)]
    [InlineData(ControlKeyState.ShiftPressed, 0x0010)]
    [InlineData(ControlKeyState.NumLockOn, 0x0020)]
    [InlineData(ControlKeyState.ScrollLockOn, 0x0040)]
    [InlineData(ControlKeyState.CapsLockOn, 0x0080)]
    [InlineData(ControlKeyState.EnhancedKey, 0x0100)]
    public void ControlKeyState_HasCorrectValues(ControlKeyState value, int expected)
    {
        Assert.Equal(expected, (int)value);
    }

    [Fact]
    public void ControlKeyState_HasFlagsAttribute()
    {
        Assert.True(typeof(ControlKeyState).IsDefined(typeof(FlagsAttribute), false));
    }

    [Fact]
    public void ControlKeyState_CanCombineFlags()
    {
        // LeftCtrlPressed | ShiftPressed = 0x0008 | 0x0010 = 0x0018
        var combined = ControlKeyState.LeftCtrlPressed | ControlKeyState.ShiftPressed;
        Assert.Equal(0x0018u, (uint)combined);
    }

    [Fact]
    public void ControlKeyState_CanTestIndividualFlags()
    {
        var combined = ControlKeyState.LeftCtrlPressed | ControlKeyState.ShiftPressed;

        Assert.True((combined & ControlKeyState.LeftCtrlPressed) != 0);
        Assert.True((combined & ControlKeyState.ShiftPressed) != 0);
        Assert.False((combined & ControlKeyState.LeftAltPressed) != 0);
    }

    #endregion

    #region MouseEventFlags Tests

    [Theory]
    [InlineData(MouseEventFlags.None, 0x0000)]
    [InlineData(MouseEventFlags.MouseMoved, 0x0001)]
    [InlineData(MouseEventFlags.DoubleClick, 0x0002)]
    [InlineData(MouseEventFlags.MouseWheeled, 0x0004)]
    [InlineData(MouseEventFlags.MouseHWheeled, 0x0008)]
    public void MouseEventFlags_HasCorrectValues(MouseEventFlags value, int expected)
    {
        Assert.Equal(expected, (int)value);
    }

    [Fact]
    public void MouseEventFlags_HasFlagsAttribute()
    {
        Assert.True(typeof(MouseEventFlags).IsDefined(typeof(FlagsAttribute), false));
    }

    #endregion

    #region MouseButtonState Tests

    [Theory]
    [InlineData(MouseButtonState.None, 0x0000)]
    [InlineData(MouseButtonState.FromLeft1stButtonPressed, 0x0001)]
    [InlineData(MouseButtonState.RightmostButtonPressed, 0x0002)]
    [InlineData(MouseButtonState.FromLeft2ndButtonPressed, 0x0004)]
    [InlineData(MouseButtonState.FromLeft3rdButtonPressed, 0x0008)]
    [InlineData(MouseButtonState.FromLeft4thButtonPressed, 0x0010)]
    public void MouseButtonState_HasCorrectValues(MouseButtonState value, int expected)
    {
        Assert.Equal(expected, (int)value);
    }

    [Fact]
    public void MouseButtonState_HasFlagsAttribute()
    {
        Assert.True(typeof(MouseButtonState).IsDefined(typeof(FlagsAttribute), false));
    }

    #endregion

    #region ConsoleInputMode Tests

    [Theory]
    [InlineData(ConsoleInputMode.None, 0x0000)]
    [InlineData(ConsoleInputMode.EnableProcessedInput, 0x0001)]
    [InlineData(ConsoleInputMode.EnableLineInput, 0x0002)]
    [InlineData(ConsoleInputMode.EnableEchoInput, 0x0004)]
    [InlineData(ConsoleInputMode.EnableWindowInput, 0x0008)]
    [InlineData(ConsoleInputMode.EnableMouseInput, 0x0010)]
    [InlineData(ConsoleInputMode.EnableInsertMode, 0x0020)]
    [InlineData(ConsoleInputMode.EnableQuickEditMode, 0x0040)]
    [InlineData(ConsoleInputMode.EnableExtendedFlags, 0x0080)]
    [InlineData(ConsoleInputMode.EnableVirtualTerminalInput, 0x0200)]
    public void ConsoleInputMode_HasCorrectValues(ConsoleInputMode value, int expected)
    {
        Assert.Equal(expected, (int)value);
    }

    [Fact]
    public void ConsoleInputMode_HasFlagsAttribute()
    {
        Assert.True(typeof(ConsoleInputMode).IsDefined(typeof(FlagsAttribute), false));
    }

    [Fact]
    public void ConsoleInputMode_CanCombineFlags()
    {
        // EnableProcessedInput | EnableMouseInput = 0x0001 | 0x0010 = 0x0011
        var combined = ConsoleInputMode.EnableProcessedInput | ConsoleInputMode.EnableMouseInput;
        Assert.Equal(0x0011u, (uint)combined);
    }

    #endregion

    #region ConsoleOutputMode Tests

    [Theory]
    [InlineData(ConsoleOutputMode.None, 0x0000)]
    [InlineData(ConsoleOutputMode.EnableProcessedOutput, 0x0001)]
    [InlineData(ConsoleOutputMode.EnableWrapAtEolOutput, 0x0002)]
    [InlineData(ConsoleOutputMode.EnableVirtualTerminalProcessing, 0x0004)]
    [InlineData(ConsoleOutputMode.DisableNewlineAutoReturn, 0x0008)]
    [InlineData(ConsoleOutputMode.EnableLvbGridWorldwide, 0x0010)]
    public void ConsoleOutputMode_HasCorrectValues(ConsoleOutputMode value, int expected)
    {
        Assert.Equal(expected, (int)value);
    }

    [Fact]
    public void ConsoleOutputMode_HasFlagsAttribute()
    {
        Assert.True(typeof(ConsoleOutputMode).IsDefined(typeof(FlagsAttribute), false));
    }

    #endregion
}
