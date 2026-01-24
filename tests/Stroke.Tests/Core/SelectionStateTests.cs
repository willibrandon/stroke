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
}
