using Stroke.Core;
using Xunit;

namespace Stroke.Tests.Core;

/// <summary>
/// Tests for YankNthArgState class (T126).
/// </summary>
public class YankNthArgStateTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_DefaultValues()
    {
        // Act
        var state = new YankNthArgState();

        // Assert
        Assert.Equal(0, state.HistoryPosition);
        Assert.Equal(-1, state.N);
        Assert.Equal("", state.PreviousInsertedWord);
    }

    [Fact]
    public void Constructor_WithHistoryPosition()
    {
        // Act
        var state = new YankNthArgState(historyPosition: -5);

        // Assert
        Assert.Equal(-5, state.HistoryPosition);
        Assert.Equal(-1, state.N);
        Assert.Equal("", state.PreviousInsertedWord);
    }

    [Fact]
    public void Constructor_WithN()
    {
        // Act
        var state = new YankNthArgState(n: 3);

        // Assert
        Assert.Equal(0, state.HistoryPosition);
        Assert.Equal(3, state.N);
        Assert.Equal("", state.PreviousInsertedWord);
    }

    [Fact]
    public void Constructor_WithPreviousInsertedWord()
    {
        // Act
        var state = new YankNthArgState(previousInsertedWord: "hello");

        // Assert
        Assert.Equal(0, state.HistoryPosition);
        Assert.Equal(-1, state.N);
        Assert.Equal("hello", state.PreviousInsertedWord);
    }

    [Fact]
    public void Constructor_AllParameters()
    {
        // Act
        var state = new YankNthArgState(
            historyPosition: -10,
            n: 5,
            previousInsertedWord: "test");

        // Assert
        Assert.Equal(-10, state.HistoryPosition);
        Assert.Equal(5, state.N);
        Assert.Equal("test", state.PreviousInsertedWord);
    }

    #endregion

    #region Property Setter Tests

    [Fact]
    public void HistoryPosition_CanBeSet()
    {
        // Arrange
        var state = new YankNthArgState();

        // Act
        state.HistoryPosition = -3;

        // Assert
        Assert.Equal(-3, state.HistoryPosition);
    }

    [Fact]
    public void N_CanBeSet()
    {
        // Arrange
        var state = new YankNthArgState();

        // Act
        state.N = 7;

        // Assert
        Assert.Equal(7, state.N);
    }

    [Fact]
    public void PreviousInsertedWord_CanBeSet()
    {
        // Arrange
        var state = new YankNthArgState();

        // Act
        state.PreviousInsertedWord = "world";

        // Assert
        Assert.Equal("world", state.PreviousInsertedWord);
    }

    #endregion

    #region N Value Semantics Tests

    [Fact]
    public void N_MinusOne_MeansLastArgument()
    {
        // Arrange - this is the semantic meaning of -1
        var state = new YankNthArgState(n: -1);

        // Assert - verify the value
        Assert.Equal(-1, state.N);
        // Note: The actual "last argument" behavior is tested in BufferHistoryTests
    }

    [Fact]
    public void N_Zero_MeansCommandName()
    {
        // Arrange - n=0 refers to the command name (first word)
        var state = new YankNthArgState(n: 0);

        // Assert
        Assert.Equal(0, state.N);
    }

    [Fact]
    public void N_One_MeansFirstArgument()
    {
        // Arrange - n=1 refers to the first argument (second word)
        var state = new YankNthArgState(n: 1);

        // Assert
        Assert.Equal(1, state.N);
    }

    #endregion

    #region HistoryPosition Semantics Tests

    [Fact]
    public void HistoryPosition_Zero_MeansNotStarted()
    {
        // Arrange - 0 means we haven't started cycling through history
        var state = new YankNthArgState(historyPosition: 0);

        // Assert
        Assert.Equal(0, state.HistoryPosition);
    }

    [Fact]
    public void HistoryPosition_Negative_IndicatesHistoryDepth()
    {
        // Arrange - negative values indicate how far back in history
        var state = new YankNthArgState(historyPosition: -3);

        // Assert - -3 means 3 entries back in history
        Assert.Equal(-3, state.HistoryPosition);
    }

    #endregion

    #region Mutability Tests

    [Fact]
    public void State_IsMutable()
    {
        // Arrange
        var state = new YankNthArgState(historyPosition: 0, n: 1, previousInsertedWord: "initial");

        // Act - modify all properties
        state.HistoryPosition = -5;
        state.N = 3;
        state.PreviousInsertedWord = "modified";

        // Assert
        Assert.Equal(-5, state.HistoryPosition);
        Assert.Equal(3, state.N);
        Assert.Equal("modified", state.PreviousInsertedWord);
    }

    [Fact]
    public void PreviousInsertedWord_CanBeEmpty()
    {
        // Arrange
        var state = new YankNthArgState(previousInsertedWord: "something");

        // Act
        state.PreviousInsertedWord = "";

        // Assert
        Assert.Equal("", state.PreviousInsertedWord);
    }

    #endregion
}
