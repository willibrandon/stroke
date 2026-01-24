using Stroke.Core;
using Xunit;

namespace Stroke.Tests.Core;

/// <summary>
/// Tests for SelectionType enum (T003 - US2).
/// </summary>
public class SelectionTypeTests
{
    #region Enum Value Tests

    [Fact]
    public void Characters_IsDefinedEnumValue()
    {
        // Assert
        Assert.True(Enum.IsDefined(typeof(SelectionType), SelectionType.Characters));
    }

    [Fact]
    public void Lines_IsDefinedEnumValue()
    {
        // Assert
        Assert.True(Enum.IsDefined(typeof(SelectionType), SelectionType.Lines));
    }

    [Fact]
    public void Block_IsDefinedEnumValue()
    {
        // Assert
        Assert.True(Enum.IsDefined(typeof(SelectionType), SelectionType.Block));
    }

    [Fact]
    public void Enum_HasExactlyThreeValues()
    {
        // Act
        var values = Enum.GetValues<SelectionType>();

        // Assert
        Assert.Equal(3, values.Length);
    }

    #endregion

    #region Default Value Tests (FR-010)

    [Fact]
    public void DefaultValue_IsCharacters()
    {
        // Arrange
        SelectionType defaultValue = default;

        // Assert - default enum value should be Characters (first value, 0)
        Assert.Equal(SelectionType.Characters, defaultValue);
    }

    [Fact]
    public void Characters_HasImplicitValueZero()
    {
        // Assert - no explicit underlying values per FR-010
        Assert.Equal(0, (int)SelectionType.Characters);
    }

    [Fact]
    public void Lines_HasImplicitValueOne()
    {
        // Assert - no explicit underlying values per FR-010
        Assert.Equal(1, (int)SelectionType.Lines);
    }

    [Fact]
    public void Block_HasImplicitValueTwo()
    {
        // Assert - no explicit underlying values per FR-010
        Assert.Equal(2, (int)SelectionType.Block);
    }

    #endregion

    #region ToString Tests

    [Fact]
    public void Characters_ToString_ReturnsCharacters()
    {
        // Act
        var result = SelectionType.Characters.ToString();

        // Assert
        Assert.Equal("Characters", result);
    }

    [Fact]
    public void Lines_ToString_ReturnsLines()
    {
        // Act
        var result = SelectionType.Lines.ToString();

        // Assert
        Assert.Equal("Lines", result);
    }

    [Fact]
    public void Block_ToString_ReturnsBlock()
    {
        // Act
        var result = SelectionType.Block.ToString();

        // Assert
        Assert.Equal("Block", result);
    }

    #endregion

    #region Casting Tests

    [Theory]
    [InlineData(0, SelectionType.Characters)]
    [InlineData(1, SelectionType.Lines)]
    [InlineData(2, SelectionType.Block)]
    public void CastFromInt_ReturnsCorrectEnumValue(int intValue, SelectionType expected)
    {
        // Act
        var result = (SelectionType)intValue;

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(SelectionType.Characters, 0)]
    [InlineData(SelectionType.Lines, 1)]
    [InlineData(SelectionType.Block, 2)]
    public void CastToInt_ReturnsCorrectIntValue(SelectionType enumValue, int expected)
    {
        // Act
        var result = (int)enumValue;

        // Assert
        Assert.Equal(expected, result);
    }

    #endregion
}
