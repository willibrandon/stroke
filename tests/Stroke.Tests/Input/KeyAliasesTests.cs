using Stroke.Input;
using Xunit;

namespace Stroke.Tests.Input;

/// <summary>
/// Tests for the <see cref="KeyAliases"/> static class.
/// </summary>
public class KeyAliasesTests
{
    // T030: Tab equals ControlI
    [Fact]
    public void Tab_EqualsControlI()
    {
        Assert.Equal(Keys.ControlI, KeyAliases.Tab);
    }

    // T031: Enter equals ControlM
    [Fact]
    public void Enter_EqualsControlM()
    {
        Assert.Equal(Keys.ControlM, KeyAliases.Enter);
    }

    // T032: Backspace equals ControlH
    [Fact]
    public void Backspace_EqualsControlH()
    {
        Assert.Equal(Keys.ControlH, KeyAliases.Backspace);
    }

    // T033: ControlSpace equals ControlAt
    [Fact]
    public void ControlSpace_EqualsControlAt()
    {
        Assert.Equal(Keys.ControlAt, KeyAliases.ControlSpace);
    }

    // T034: Backwards-compatibility aliases
    [Fact]
    public void ShiftControlLeft_EqualsControlShiftLeft()
    {
        Assert.Equal(Keys.ControlShiftLeft, KeyAliases.ShiftControlLeft);
    }

    [Fact]
    public void ShiftControlRight_EqualsControlShiftRight()
    {
        Assert.Equal(Keys.ControlShiftRight, KeyAliases.ShiftControlRight);
    }

    [Fact]
    public void ShiftControlHome_EqualsControlShiftHome()
    {
        Assert.Equal(Keys.ControlShiftHome, KeyAliases.ShiftControlHome);
    }

    [Fact]
    public void ShiftControlEnd_EqualsControlShiftEnd()
    {
        Assert.Equal(Keys.ControlShiftEnd, KeyAliases.ShiftControlEnd);
    }

    // Verify aliases produce correct key strings
    [Fact]
    public void Tab_ToKeyString_ReturnsCi()
    {
        Assert.Equal("c-i", KeyAliases.Tab.ToKeyString());
    }

    [Fact]
    public void Enter_ToKeyString_ReturnsCm()
    {
        Assert.Equal("c-m", KeyAliases.Enter.ToKeyString());
    }

    [Fact]
    public void Backspace_ToKeyString_ReturnsCh()
    {
        Assert.Equal("c-h", KeyAliases.Backspace.ToKeyString());
    }

    [Fact]
    public void ControlSpace_ToKeyString_ReturnsCAt()
    {
        Assert.Equal("c-@", KeyAliases.ControlSpace.ToKeyString());
    }
}
