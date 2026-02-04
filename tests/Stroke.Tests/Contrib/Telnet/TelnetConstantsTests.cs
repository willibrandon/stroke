namespace Stroke.Tests.Contrib.Telnet;

using Stroke.Contrib.Telnet;
using Xunit;

/// <summary>
/// Unit tests for <see cref="TelnetConstants"/>.
/// </summary>
public class TelnetConstantsTests
{
    #region Command Constants (RFC 854)

    [Fact]
    public void IAC_ShouldBe255()
    {
        Assert.Equal(255, TelnetConstants.IAC);
    }

    [Fact]
    public void DO_ShouldBe253()
    {
        Assert.Equal(253, TelnetConstants.DO);
    }

    [Fact]
    public void DONT_ShouldBe254()
    {
        Assert.Equal(254, TelnetConstants.DONT);
    }

    [Fact]
    public void WILL_ShouldBe251()
    {
        Assert.Equal(251, TelnetConstants.WILL);
    }

    [Fact]
    public void WONT_ShouldBe252()
    {
        Assert.Equal(252, TelnetConstants.WONT);
    }

    [Fact]
    public void SB_ShouldBe250()
    {
        Assert.Equal(250, TelnetConstants.SB);
    }

    [Fact]
    public void SE_ShouldBe240()
    {
        Assert.Equal(240, TelnetConstants.SE);
    }

    [Fact]
    public void NOP_ShouldBe0()
    {
        Assert.Equal(0, TelnetConstants.NOP);
    }

    #endregion

    #region Simple Command Constants

    [Fact]
    public void DM_ShouldBe242()
    {
        Assert.Equal(242, TelnetConstants.DM);
    }

    [Fact]
    public void BRK_ShouldBe243()
    {
        Assert.Equal(243, TelnetConstants.BRK);
    }

    [Fact]
    public void IP_ShouldBe244()
    {
        Assert.Equal(244, TelnetConstants.IP);
    }

    [Fact]
    public void AO_ShouldBe245()
    {
        Assert.Equal(245, TelnetConstants.AO);
    }

    [Fact]
    public void AYT_ShouldBe246()
    {
        Assert.Equal(246, TelnetConstants.AYT);
    }

    [Fact]
    public void EC_ShouldBe247()
    {
        Assert.Equal(247, TelnetConstants.EC);
    }

    [Fact]
    public void EL_ShouldBe248()
    {
        Assert.Equal(248, TelnetConstants.EL);
    }

    [Fact]
    public void GA_ShouldBe249()
    {
        Assert.Equal(249, TelnetConstants.GA);
    }

    #endregion

    #region Option Constants

    [Fact]
    public void ECHO_ShouldBe1()
    {
        Assert.Equal(1, TelnetConstants.ECHO);
    }

    [Fact]
    public void SGA_ShouldBe3()
    {
        Assert.Equal(3, TelnetConstants.SGA);
    }

    [Fact]
    public void SUPPRESS_GO_AHEAD_ShouldBe3()
    {
        Assert.Equal(3, TelnetConstants.SUPPRESS_GO_AHEAD);
    }

    [Fact]
    public void SGA_And_SUPPRESS_GO_AHEAD_ShouldBeEqual()
    {
        Assert.Equal(TelnetConstants.SGA, TelnetConstants.SUPPRESS_GO_AHEAD);
    }

    [Fact]
    public void TTYPE_ShouldBe24()
    {
        Assert.Equal(24, TelnetConstants.TTYPE);
    }

    [Fact]
    public void NAWS_ShouldBe31()
    {
        Assert.Equal(31, TelnetConstants.NAWS);
    }

    [Fact]
    public void LINEMODE_ShouldBe34()
    {
        Assert.Equal(34, TelnetConstants.LINEMODE);
    }

    #endregion

    #region Subnegotiation Constants

    [Fact]
    public void IS_ShouldBe0()
    {
        Assert.Equal(0, TelnetConstants.IS);
    }

    [Fact]
    public void SEND_ShouldBe1()
    {
        Assert.Equal(1, TelnetConstants.SEND);
    }

    [Fact]
    public void MODE_ShouldBe1()
    {
        Assert.Equal(1, TelnetConstants.MODE);
    }

    #endregion

    #region ToByte Helper

    [Fact]
    public void ToByte_ShouldReturnSingleElementArray()
    {
        var result = TelnetConstants.ToByte(42);

        Assert.Single(result);
        Assert.Equal(42, result[0]);
    }

    [Fact]
    public void ToByte_WithIAC_ShouldReturn255()
    {
        var result = TelnetConstants.ToByte(TelnetConstants.IAC);

        Assert.Single(result);
        Assert.Equal(255, result[0]);
    }

    [Fact]
    public void ToByte_WithZero_ShouldReturn0()
    {
        var result = TelnetConstants.ToByte(0);

        Assert.Single(result);
        Assert.Equal(0, result[0]);
    }

    #endregion

    #region Python PTK Compatibility

    /// <summary>
    /// Verify constants match Python Prompt Toolkit's protocol.py values.
    /// </summary>
    [Theory]
    [InlineData(nameof(TelnetConstants.NOP), 0)]
    [InlineData(nameof(TelnetConstants.SGA), 3)]
    [InlineData(nameof(TelnetConstants.IAC), 255)]
    [InlineData(nameof(TelnetConstants.DO), 253)]
    [InlineData(nameof(TelnetConstants.DONT), 254)]
    [InlineData(nameof(TelnetConstants.WILL), 251)]
    [InlineData(nameof(TelnetConstants.WONT), 252)]
    [InlineData(nameof(TelnetConstants.SB), 250)]
    [InlineData(nameof(TelnetConstants.SE), 240)]
    [InlineData(nameof(TelnetConstants.LINEMODE), 34)]
    [InlineData(nameof(TelnetConstants.MODE), 1)]
    [InlineData(nameof(TelnetConstants.ECHO), 1)]
    [InlineData(nameof(TelnetConstants.NAWS), 31)]
    [InlineData(nameof(TelnetConstants.SUPPRESS_GO_AHEAD), 3)]
    [InlineData(nameof(TelnetConstants.TTYPE), 24)]
    [InlineData(nameof(TelnetConstants.SEND), 1)]
    [InlineData(nameof(TelnetConstants.IS), 0)]
    public void Constants_ShouldMatchPythonPTK(string constantName, byte expectedValue)
    {
        var field = typeof(TelnetConstants).GetField(constantName);
        Assert.NotNull(field);
        var actualValue = (byte)field!.GetValue(null)!;
        Assert.Equal(expectedValue, actualValue);
    }

    #endregion
}
