using Stroke.CursorShapes;
using Xunit;

namespace Stroke.Tests.CursorShapes;

/// <summary>
/// Tests for <see cref="CursorShape"/> enum and <see cref="CursorShapeExtensions"/>.
/// </summary>
public sealed class CursorShapeTests
{
    #region Enum Values

    [Fact]
    public void CursorShape_NeverChange_HasValue0()
    {
        Assert.Equal(0, (int)CursorShape.NeverChange);
    }

    [Fact]
    public void CursorShape_Block_HasValue1()
    {
        Assert.Equal(1, (int)CursorShape.Block);
    }

    [Fact]
    public void CursorShape_Beam_HasValue2()
    {
        Assert.Equal(2, (int)CursorShape.Beam);
    }

    [Fact]
    public void CursorShape_Underline_HasValue3()
    {
        Assert.Equal(3, (int)CursorShape.Underline);
    }

    [Fact]
    public void CursorShape_BlinkingBlock_HasValue4()
    {
        Assert.Equal(4, (int)CursorShape.BlinkingBlock);
    }

    [Fact]
    public void CursorShape_BlinkingBeam_HasValue5()
    {
        Assert.Equal(5, (int)CursorShape.BlinkingBeam);
    }

    [Fact]
    public void CursorShape_BlinkingUnderline_HasValue6()
    {
        Assert.Equal(6, (int)CursorShape.BlinkingUnderline);
    }

    #endregion

    #region DECSCUSR Code Mappings

    [Fact]
    public void GetDecscusrCode_NeverChange_ReturnsNull()
    {
        var result = CursorShape.NeverChange.GetDecscusrCode();

        Assert.Null(result);
    }

    [Fact]
    public void GetDecscusrCode_Block_Returns2()
    {
        var result = CursorShape.Block.GetDecscusrCode();

        Assert.Equal(2, result);
    }

    [Fact]
    public void GetDecscusrCode_Beam_Returns6()
    {
        var result = CursorShape.Beam.GetDecscusrCode();

        Assert.Equal(6, result);
    }

    [Fact]
    public void GetDecscusrCode_Underline_Returns4()
    {
        var result = CursorShape.Underline.GetDecscusrCode();

        Assert.Equal(4, result);
    }

    [Fact]
    public void GetDecscusrCode_BlinkingBlock_Returns1()
    {
        var result = CursorShape.BlinkingBlock.GetDecscusrCode();

        Assert.Equal(1, result);
    }

    [Fact]
    public void GetDecscusrCode_BlinkingBeam_Returns5()
    {
        var result = CursorShape.BlinkingBeam.GetDecscusrCode();

        Assert.Equal(5, result);
    }

    [Fact]
    public void GetDecscusrCode_BlinkingUnderline_Returns3()
    {
        var result = CursorShape.BlinkingUnderline.GetDecscusrCode();

        Assert.Equal(3, result);
    }

    #endregion

    #region Escape Sequences

    [Fact]
    public void GetEscapeSequence_NeverChange_ReturnsNull()
    {
        var result = CursorShape.NeverChange.GetEscapeSequence();

        Assert.Null(result);
    }

    [Fact]
    public void GetEscapeSequence_Block_ReturnsCorrectSequence()
    {
        var result = CursorShape.Block.GetEscapeSequence();

        Assert.Equal("\x1b[2 q", result);
    }

    [Fact]
    public void GetEscapeSequence_Beam_ReturnsCorrectSequence()
    {
        var result = CursorShape.Beam.GetEscapeSequence();

        Assert.Equal("\x1b[6 q", result);
    }

    [Fact]
    public void GetEscapeSequence_Underline_ReturnsCorrectSequence()
    {
        var result = CursorShape.Underline.GetEscapeSequence();

        Assert.Equal("\x1b[4 q", result);
    }

    [Fact]
    public void GetEscapeSequence_BlinkingBlock_ReturnsCorrectSequence()
    {
        var result = CursorShape.BlinkingBlock.GetEscapeSequence();

        Assert.Equal("\x1b[1 q", result);
    }

    [Fact]
    public void GetEscapeSequence_BlinkingBeam_ReturnsCorrectSequence()
    {
        var result = CursorShape.BlinkingBeam.GetEscapeSequence();

        Assert.Equal("\x1b[5 q", result);
    }

    [Fact]
    public void GetEscapeSequence_BlinkingUnderline_ReturnsCorrectSequence()
    {
        var result = CursorShape.BlinkingUnderline.GetEscapeSequence();

        Assert.Equal("\x1b[3 q", result);
    }

    #endregion

    #region DECSCUSR Code Correctness

    /// <summary>
    /// Verifies that the DECSCUSR codes match the VT100 specification.
    /// </summary>
    [Theory]
    [InlineData(CursorShape.BlinkingBlock, 1)]    // Blinking block
    [InlineData(CursorShape.Block, 2)]            // Steady block
    [InlineData(CursorShape.BlinkingUnderline, 3)] // Blinking underline
    [InlineData(CursorShape.Underline, 4)]        // Steady underline
    [InlineData(CursorShape.BlinkingBeam, 5)]     // Blinking bar (beam)
    [InlineData(CursorShape.Beam, 6)]             // Steady bar (beam)
    public void GetDecscusrCode_MatchesVt100Spec(CursorShape shape, int expectedCode)
    {
        var result = shape.GetDecscusrCode();

        Assert.Equal(expectedCode, result);
    }

    #endregion
}
