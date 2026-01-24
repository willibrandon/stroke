using Stroke.Core;
using Xunit;

namespace Stroke.Tests.Core;

/// <summary>
/// Tests for PasteMode enum (T005 - US3).
/// </summary>
public class PasteModeTests
{
    #region Enum Value Tests

    [Fact]
    public void Emacs_IsDefinedEnumValue()
    {
        // Assert
        Assert.True(Enum.IsDefined(typeof(PasteMode), PasteMode.Emacs));
    }

    [Fact]
    public void ViAfter_IsDefinedEnumValue()
    {
        // Assert
        Assert.True(Enum.IsDefined(typeof(PasteMode), PasteMode.ViAfter));
    }

    [Fact]
    public void ViBefore_IsDefinedEnumValue()
    {
        // Assert
        Assert.True(Enum.IsDefined(typeof(PasteMode), PasteMode.ViBefore));
    }

    [Fact]
    public void Enum_HasExactlyThreeValues()
    {
        // Act
        var values = Enum.GetValues<PasteMode>();

        // Assert
        Assert.Equal(3, values.Length);
    }

    #endregion

    #region Default Value Tests (FR-010)

    [Fact]
    public void DefaultValue_IsEmacs()
    {
        // Arrange
        PasteMode defaultValue = default;

        // Assert - default enum value should be Emacs (first value, 0)
        Assert.Equal(PasteMode.Emacs, defaultValue);
    }

    [Fact]
    public void Emacs_HasImplicitValueZero()
    {
        // Assert - no explicit underlying values per FR-010
        Assert.Equal(0, (int)PasteMode.Emacs);
    }

    [Fact]
    public void ViAfter_HasImplicitValueOne()
    {
        // Assert - no explicit underlying values per FR-010
        Assert.Equal(1, (int)PasteMode.ViAfter);
    }

    [Fact]
    public void ViBefore_HasImplicitValueTwo()
    {
        // Assert - no explicit underlying values per FR-010
        Assert.Equal(2, (int)PasteMode.ViBefore);
    }

    #endregion

    #region ToString Tests

    [Fact]
    public void Emacs_ToString_ReturnsEmacs()
    {
        // Act
        var result = PasteMode.Emacs.ToString();

        // Assert
        Assert.Equal("Emacs", result);
    }

    [Fact]
    public void ViAfter_ToString_ReturnsViAfter()
    {
        // Act
        var result = PasteMode.ViAfter.ToString();

        // Assert
        Assert.Equal("ViAfter", result);
    }

    [Fact]
    public void ViBefore_ToString_ReturnsViBefore()
    {
        // Act
        var result = PasteMode.ViBefore.ToString();

        // Assert
        Assert.Equal("ViBefore", result);
    }

    #endregion

    #region Casting Tests

    [Theory]
    [InlineData(0, PasteMode.Emacs)]
    [InlineData(1, PasteMode.ViAfter)]
    [InlineData(2, PasteMode.ViBefore)]
    public void CastFromInt_ReturnsCorrectEnumValue(int intValue, PasteMode expected)
    {
        // Act
        var result = (PasteMode)intValue;

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(PasteMode.Emacs, 0)]
    [InlineData(PasteMode.ViAfter, 1)]
    [InlineData(PasteMode.ViBefore, 2)]
    public void CastToInt_ReturnsCorrectIntValue(PasteMode enumValue, int expected)
    {
        // Act
        var result = (int)enumValue;

        // Assert
        Assert.Equal(expected, result);
    }

    #endregion
}
