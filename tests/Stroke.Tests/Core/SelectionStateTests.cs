using Stroke.Core;
using Xunit;

namespace Stroke.Tests.Core;

/// <summary>
/// Tests for SelectionState class (Phase 11 - T141).
/// </summary>
public class SelectionStateTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_DefaultValues_SetsCharactersTypeAndPositionZero()
    {
        // Act
        var state = new SelectionState();

        // Assert
        Assert.Equal(0, state.OriginalCursorPosition);
        Assert.Equal(SelectionType.Characters, state.Type);
        Assert.False(state.ShiftMode);
    }

    [Fact]
    public void Constructor_WithPosition_SetsPosition()
    {
        // Act
        var state = new SelectionState(originalCursorPosition: 42);

        // Assert
        Assert.Equal(42, state.OriginalCursorPosition);
        Assert.Equal(SelectionType.Characters, state.Type);
    }

    [Fact]
    public void Constructor_WithType_SetsType()
    {
        // Act
        var state = new SelectionState(type: SelectionType.Lines);

        // Assert
        Assert.Equal(0, state.OriginalCursorPosition);
        Assert.Equal(SelectionType.Lines, state.Type);
    }

    [Fact]
    public void Constructor_WithBothParameters_SetsBoth()
    {
        // Act
        var state = new SelectionState(originalCursorPosition: 100, type: SelectionType.Block);

        // Assert
        Assert.Equal(100, state.OriginalCursorPosition);
        Assert.Equal(SelectionType.Block, state.Type);
    }

    #endregion

    #region Property Tests

    [Fact]
    public void OriginalCursorPosition_IsReadOnly()
    {
        // Arrange
        var state = new SelectionState(originalCursorPosition: 10);

        // Assert - property should be immutable (getter only)
        Assert.Equal(10, state.OriginalCursorPosition);
    }

    [Fact]
    public void Type_IsReadOnly()
    {
        // Arrange
        var state = new SelectionState(type: SelectionType.Lines);

        // Assert - property should be immutable (getter only)
        Assert.Equal(SelectionType.Lines, state.Type);
    }

    #endregion

    #region ShiftMode Tests

    [Fact]
    public void ShiftMode_InitiallyFalse()
    {
        // Arrange
        var state = new SelectionState();

        // Assert
        Assert.False(state.ShiftMode);
    }

    [Fact]
    public void EnterShiftMode_SetsShiftModeTrue()
    {
        // Arrange
        var state = new SelectionState();

        // Act
        state.EnterShiftMode();

        // Assert
        Assert.True(state.ShiftMode);
    }

    [Fact]
    public void EnterShiftMode_CalledMultipleTimes_RemainsTrue()
    {
        // Arrange
        var state = new SelectionState();

        // Act
        state.EnterShiftMode();
        state.EnterShiftMode();
        state.EnterShiftMode();

        // Assert
        Assert.True(state.ShiftMode);
    }

    #endregion

    #region Boundary Tests (T004 - US1)

    [Fact]
    public void Constructor_WithNegativePosition_AcceptsValue()
    {
        // Arrange & Act
        var state = new SelectionState(originalCursorPosition: -1);

        // Assert - negative positions are valid per spec (matching Python behavior)
        Assert.Equal(-1, state.OriginalCursorPosition);
    }

    [Fact]
    public void Constructor_WithIntMinValue_AcceptsValue()
    {
        // Arrange & Act
        var state = new SelectionState(originalCursorPosition: int.MinValue);

        // Assert - boundary value is valid
        Assert.Equal(int.MinValue, state.OriginalCursorPosition);
    }

    [Fact]
    public void Constructor_WithIntMaxValue_AcceptsValue()
    {
        // Arrange & Act
        var state = new SelectionState(originalCursorPosition: int.MaxValue);

        // Assert - boundary value is valid
        Assert.Equal(int.MaxValue, state.OriginalCursorPosition);
    }

    [Fact]
    public void Constructor_WithLargeNegativePosition_AcceptsValue()
    {
        // Arrange & Act
        var state = new SelectionState(originalCursorPosition: -1000000);

        // Assert
        Assert.Equal(-1000000, state.OriginalCursorPosition);
    }

    #endregion

    #region SelectionType Enum Tests

    [Theory]
    [InlineData(SelectionType.Characters)]
    [InlineData(SelectionType.Lines)]
    [InlineData(SelectionType.Block)]
    public void Constructor_AllSelectionTypes_Supported(SelectionType type)
    {
        // Act
        var state = new SelectionState(type: type);

        // Assert
        Assert.Equal(type, state.Type);
    }

    #endregion

    #region ToString Tests (T007 - US5)

    [Fact]
    public void ToString_WithDefaultValues_ReturnsExpectedFormat()
    {
        // Arrange
        var state = new SelectionState();

        // Act
        var result = state.ToString();

        // Assert
        Assert.Equal("SelectionState(OriginalCursorPosition=0, Type=Characters)", result);
    }

    [Fact]
    public void ToString_WithPosition_IncludesPosition()
    {
        // Arrange
        var state = new SelectionState(originalCursorPosition: 42);

        // Act
        var result = state.ToString();

        // Assert
        Assert.Equal("SelectionState(OriginalCursorPosition=42, Type=Characters)", result);
    }

    [Fact]
    public void ToString_WithLinesType_IncludesType()
    {
        // Arrange
        var state = new SelectionState(originalCursorPosition: 10, type: SelectionType.Lines);

        // Act
        var result = state.ToString();

        // Assert - matches FR-007 format
        Assert.Equal("SelectionState(OriginalCursorPosition=10, Type=Lines)", result);
    }

    [Fact]
    public void ToString_WithBlockType_IncludesType()
    {
        // Arrange
        var state = new SelectionState(originalCursorPosition: 5, type: SelectionType.Block);

        // Act
        var result = state.ToString();

        // Assert
        Assert.Equal("SelectionState(OriginalCursorPosition=5, Type=Block)", result);
    }

    [Fact]
    public void ToString_DoesNotIncludeShiftMode()
    {
        // Arrange
        var state = new SelectionState(originalCursorPosition: 100, type: SelectionType.Lines);
        state.EnterShiftMode(); // ShiftMode is true

        // Act
        var result = state.ToString();

        // Assert - ShiftMode is not included per spec (matching Python's __repr__)
        Assert.Equal("SelectionState(OriginalCursorPosition=100, Type=Lines)", result);
        Assert.DoesNotContain("ShiftMode", result);
    }

    [Fact]
    public void ToString_WithNegativePosition_IncludesNegativePosition()
    {
        // Arrange
        var state = new SelectionState(originalCursorPosition: -50);

        // Act
        var result = state.ToString();

        // Assert
        Assert.Equal("SelectionState(OriginalCursorPosition=-50, Type=Characters)", result);
    }

    #endregion

    #region Sealed Class Tests (T010 - US6)

    [Fact]
    public void SelectionState_IsSealed()
    {
        // Act
        var type = typeof(SelectionState);

        // Assert - FR-009 requires sealed class
        Assert.True(type.IsSealed);
    }

    #endregion
}
