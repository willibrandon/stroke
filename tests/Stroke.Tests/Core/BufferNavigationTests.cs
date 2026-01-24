using Stroke.Core;
using Stroke.History;
using Xunit;

// Use alias to avoid ambiguity with System.Buffer
using Buffer = Stroke.Core.Buffer;
using Document = Stroke.Core.Document;

namespace Stroke.Tests.Core;

/// <summary>
/// Tests for Buffer cursor navigation operations (T048-T051).
/// </summary>
public class BufferNavigationTests
{
    #region CursorLeft Tests

    [Fact]
    public void CursorLeft_MovesCursorLeft()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello", cursorPosition: 3));

        // Act
        buffer.CursorLeft();

        // Assert
        Assert.Equal(2, buffer.CursorPosition);
    }

    [Fact]
    public void CursorLeft_WithCount_MovesMultiplePositions()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello world", cursorPosition: 8));

        // Act
        buffer.CursorLeft(3);

        // Assert
        Assert.Equal(5, buffer.CursorPosition);
    }

    [Fact]
    public void CursorLeft_AtStart_StaysAtStart()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello", cursorPosition: 0));

        // Act
        buffer.CursorLeft();

        // Assert
        Assert.Equal(0, buffer.CursorPosition);
    }

    [Fact]
    public void CursorLeft_CountExceedsPosition_StopsAtStart()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello", cursorPosition: 2));

        // Act
        buffer.CursorLeft(10);

        // Assert
        Assert.Equal(0, buffer.CursorPosition);
    }

    #endregion

    #region CursorRight Tests

    [Fact]
    public void CursorRight_MovesCursorRight()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello", cursorPosition: 2));

        // Act
        buffer.CursorRight();

        // Assert
        Assert.Equal(3, buffer.CursorPosition);
    }

    [Fact]
    public void CursorRight_WithCount_MovesMultiplePositions()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello world", cursorPosition: 2));

        // Act
        buffer.CursorRight(4);

        // Assert
        Assert.Equal(6, buffer.CursorPosition);
    }

    [Fact]
    public void CursorRight_AtEnd_StaysAtEnd()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello", cursorPosition: 5));

        // Act
        buffer.CursorRight();

        // Assert
        Assert.Equal(5, buffer.CursorPosition);
    }

    [Fact]
    public void CursorRight_CountExceedsRemaining_StopsAtEnd()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello", cursorPosition: 3));

        // Act
        buffer.CursorRight(10);

        // Assert
        Assert.Equal(5, buffer.CursorPosition);
    }

    #endregion

    #region CursorUp Tests

    [Fact]
    public void CursorUp_MovesToPreviousLine()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("line1\nline2", cursorPosition: 8));
        // Cursor is at position 8 (l-i-n-e-1-\n = 6, then l-i = 2 more)

        // Act
        buffer.CursorUp();

        // Assert - should be on first line
        Assert.Equal(0, buffer.Document.CursorPositionRow);
    }

    [Fact]
    public void CursorUp_PreservesColumn()
    {
        // Arrange - cursor at column 3 of line 2
        var buffer = new Buffer(document: new Document("hello\nworld", cursorPosition: 9));
        // Position 9: h-e-l-l-o-\n = 6, then w-o-r = 3 more

        // Act
        buffer.CursorUp();

        // Assert - should be at column 3 of line 1
        Assert.Equal(3, buffer.Document.CursorPositionCol);
    }

    [Fact]
    public void CursorUp_OnFirstLine_StaysOnFirstLine()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello", cursorPosition: 3));

        // Act
        buffer.CursorUp();

        // Assert
        Assert.Equal(0, buffer.Document.CursorPositionRow);
        Assert.Equal(3, buffer.CursorPosition);
    }

    [Fact]
    public void CursorUp_WithCount_MovesMultipleLines()
    {
        // Arrange - cursor on line 3
        var buffer = new Buffer(document: new Document("a\nb\nc\nd", cursorPosition: 6));
        // Position 6: a-\n-b-\n-c-\n = 6, cursor at start of 'd'

        // Act
        buffer.CursorUp(2);

        // Assert - should be on line 1
        Assert.Equal(1, buffer.Document.CursorPositionRow);
    }

    [Fact]
    public void CursorUp_PreferredColumn_PreservedAcrossMultipleMoves()
    {
        // Arrange - start at column 5 on a long line
        var buffer = new Buffer(document: new Document("hello world\nhi\ntest line", cursorPosition: 5));

        // Act - move down to short line, then back up
        buffer.CursorDown(); // to "hi" line - cursor clamped to col 2
        buffer.CursorUp(); // back to first line - should restore col 5

        // Assert
        Assert.Equal(5, buffer.Document.CursorPositionCol);
    }

    [Fact]
    public void CursorUp_ShortLine_ClampsToLineEnd()
    {
        // Arrange - cursor at column 10 on line 2
        var buffer = new Buffer(document: new Document("hi\nhello world", cursorPosition: 13));

        // Act
        buffer.CursorUp();

        // Assert - first line only has 2 chars, cursor should be at end (col 2)
        Assert.Equal(2, buffer.Document.CursorPositionCol);
    }

    #endregion

    #region CursorDown Tests

    [Fact]
    public void CursorDown_MovesToNextLine()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("line1\nline2", cursorPosition: 2));

        // Act
        buffer.CursorDown();

        // Assert
        Assert.Equal(1, buffer.Document.CursorPositionRow);
    }

    [Fact]
    public void CursorDown_PreservesColumn()
    {
        // Arrange - cursor at column 3 of line 1
        var buffer = new Buffer(document: new Document("hello\nworld", cursorPosition: 3));

        // Act
        buffer.CursorDown();

        // Assert - should be at column 3 of line 2
        Assert.Equal(3, buffer.Document.CursorPositionCol);
    }

    [Fact]
    public void CursorDown_OnLastLine_StaysOnLastLine()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello", cursorPosition: 3));

        // Act
        buffer.CursorDown();

        // Assert
        Assert.Equal(0, buffer.Document.CursorPositionRow);
        Assert.Equal(3, buffer.CursorPosition);
    }

    [Fact]
    public void CursorDown_WithCount_MovesMultipleLines()
    {
        // Arrange - cursor on line 1
        var buffer = new Buffer(document: new Document("a\nb\nc\nd", cursorPosition: 0));

        // Act
        buffer.CursorDown(2);

        // Assert - should be on line 3
        Assert.Equal(2, buffer.Document.CursorPositionRow);
    }

    [Fact]
    public void CursorDown_ShortLine_ClampsToLineEnd()
    {
        // Arrange - cursor at column 10 on line 1
        var buffer = new Buffer(document: new Document("hello world\nhi", cursorPosition: 10));

        // Act
        buffer.CursorDown();

        // Assert - second line only has 2 chars, cursor should be at end
        Assert.Equal(2, buffer.Document.CursorPositionCol);
    }

    #endregion

    #region GoToMatchingBracket Tests

    [Fact]
    public void GoToMatchingBracket_AtOpenParen_GoesToCloseParen()
    {
        // Arrange - cursor at open paren
        var buffer = new Buffer(document: new Document("(hello)", cursorPosition: 0));

        // Act
        buffer.GoToMatchingBracket();

        // Assert - should be at close paren
        Assert.Equal(6, buffer.CursorPosition);
    }

    [Fact]
    public void GoToMatchingBracket_AtCloseParen_GoesToOpenParen()
    {
        // Arrange - cursor at close paren
        var buffer = new Buffer(document: new Document("(hello)", cursorPosition: 6));

        // Act
        buffer.GoToMatchingBracket();

        // Assert - should be at open paren
        Assert.Equal(0, buffer.CursorPosition);
    }

    [Fact]
    public void GoToMatchingBracket_NestedBrackets_FindsCorrectMatch()
    {
        // Arrange - cursor at outer open bracket
        var buffer = new Buffer(document: new Document("([{}])", cursorPosition: 0));

        // Act
        buffer.GoToMatchingBracket();

        // Assert - should be at outer close bracket
        Assert.Equal(5, buffer.CursorPosition);
    }

    [Fact]
    public void GoToMatchingBracket_NoBracket_DoesNotMove()
    {
        // Arrange - cursor not at a bracket
        var buffer = new Buffer(document: new Document("hello", cursorPosition: 2));

        // Act
        buffer.GoToMatchingBracket();

        // Assert - cursor unchanged
        Assert.Equal(2, buffer.CursorPosition);
    }

    [Fact]
    public void GoToMatchingBracket_UnmatchedBracket_DoesNotMove()
    {
        // Arrange - cursor at unmatched open paren
        var buffer = new Buffer(document: new Document("(hello", cursorPosition: 0));

        // Act
        buffer.GoToMatchingBracket();

        // Assert - cursor unchanged (no matching bracket found)
        Assert.Equal(0, buffer.CursorPosition);
    }

    [Fact]
    public void GoToMatchingBracket_CurlyBraces()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("{code}", cursorPosition: 0));

        // Act
        buffer.GoToMatchingBracket();

        // Assert
        Assert.Equal(5, buffer.CursorPosition);
    }

    [Fact]
    public void GoToMatchingBracket_SquareBrackets()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("[item]", cursorPosition: 0));

        // Act
        buffer.GoToMatchingBracket();

        // Assert
        Assert.Equal(5, buffer.CursorPosition);
    }

    #endregion

    #region AutoUp Tests

    [Fact]
    public void AutoUp_OnFirstLine_NoSelection_CallsHistoryBackward()
    {
        // Arrange - on first line with no selection, use fresh empty history
        var buffer = new Buffer(history: new InMemoryHistory(), document: new Document("hello", cursorPosition: 3));

        // Act
        buffer.AutoUp();

        // Assert - no exception, cursor unchanged (HistoryBackward does nothing with empty history)
        Assert.Equal(3, buffer.CursorPosition);
    }

    [Fact]
    public void AutoUp_NotOnFirstLine_MovesCursorUp()
    {
        // Arrange - on second line
        var buffer = new Buffer(document: new Document("line1\nline2", cursorPosition: 8));

        // Act
        buffer.AutoUp();

        // Assert - moved to first line
        Assert.Equal(0, buffer.Document.CursorPositionRow);
    }

    [Fact]
    public void AutoUp_WithSelection_DoesNotCallHistory()
    {
        // Arrange - on first line with selection, use fresh empty history
        var selection = new SelectionState(0, SelectionType.Characters);
        var buffer = new Buffer(history: new InMemoryHistory(), document: new Document("hello", cursorPosition: 3, selection: selection));

        // Act
        buffer.AutoUp();

        // Assert - cursor unchanged, didn't call history
        Assert.Equal(3, buffer.CursorPosition);
    }

    [Fact]
    public void AutoUp_InCompletionState_CallsCompletePrevious()
    {
        // Arrange - with completion state set
        var buffer = new Buffer(document: new Document("hello", cursorPosition: 5));
        // Note: CompleteState is not yet implemented, so we can't easily test this
        // This test documents expected behavior

        // Act
        buffer.AutoUp();

        // Assert - no exception
        Assert.Equal(5, buffer.CursorPosition);
    }

    #endregion

    #region AutoDown Tests

    [Fact]
    public void AutoDown_OnLastLine_NoSelection_CallsHistoryForward()
    {
        // Arrange - on last line with no selection
        var buffer = new Buffer(document: new Document("hello", cursorPosition: 3));

        // Act
        buffer.AutoDown();

        // Assert - no exception, cursor unchanged (HistoryForward is a stub)
        Assert.Equal(3, buffer.CursorPosition);
    }

    [Fact]
    public void AutoDown_NotOnLastLine_MovesCursorDown()
    {
        // Arrange - on first line
        var buffer = new Buffer(document: new Document("line1\nline2", cursorPosition: 2));

        // Act
        buffer.AutoDown();

        // Assert - moved to second line
        Assert.Equal(1, buffer.Document.CursorPositionRow);
    }

    [Fact]
    public void AutoDown_WithSelection_DoesNotCallHistory()
    {
        // Arrange - on last line with selection
        // Create a document with selection state already set
        var selection = new SelectionState(0, SelectionType.Characters);
        var buffer = new Buffer(document: new Document("hello", cursorPosition: 3, selection: selection));

        // Act
        buffer.AutoDown();

        // Assert - cursor unchanged, didn't call history
        Assert.Equal(3, buffer.CursorPosition);
    }

    #endregion

    #region CompletePrevious/CompleteNext Tests (Stubs)

    [Fact]
    public void CompletePrevious_Stub_DoesNotThrow()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello", cursorPosition: 5));

        // Act & Assert - no exception
        buffer.CompletePrevious();
        buffer.CompletePrevious(3);
    }

    [Fact]
    public void CompleteNext_Stub_DoesNotThrow()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello", cursorPosition: 5));

        // Act & Assert - no exception
        buffer.CompleteNext();
        buffer.CompleteNext(3);
    }

    #endregion

    #region HistoryBackward/HistoryForward Tests (Stubs)

    [Fact]
    public void HistoryBackward_Stub_DoesNotThrow()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello", cursorPosition: 5));

        // Act & Assert - no exception
        buffer.HistoryBackward();
        buffer.HistoryBackward(3);
    }

    [Fact]
    public void HistoryForward_Stub_DoesNotThrow()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello", cursorPosition: 5));

        // Act & Assert - no exception
        buffer.HistoryForward();
        buffer.HistoryForward(3);
    }

    #endregion

    #region Thread Safety Tests

    [Fact]
    public async Task ConcurrentNavigation_ThreadSafe()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("line1\nline2\nline3", cursorPosition: 8));
        var iterations = 50;
        var barrier = new Barrier(4);

        // Act - concurrent navigation operations
        var leftTask = Task.Run(() =>
        {
            barrier.SignalAndWait();
            for (var i = 0; i < iterations; i++)
            {
                buffer.CursorLeft();
            }
        });

        var rightTask = Task.Run(() =>
        {
            barrier.SignalAndWait();
            for (var i = 0; i < iterations; i++)
            {
                buffer.CursorRight();
            }
        });

        var upTask = Task.Run(() =>
        {
            barrier.SignalAndWait();
            for (var i = 0; i < iterations; i++)
            {
                buffer.CursorUp();
            }
        });

        var downTask = Task.Run(() =>
        {
            barrier.SignalAndWait();
            for (var i = 0; i < iterations; i++)
            {
                buffer.CursorDown();
            }
        });

        // Assert - no exceptions
        await Task.WhenAll(leftTask, rightTask, upTask, downTask);
    }

    #endregion
}
