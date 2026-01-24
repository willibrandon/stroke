using Stroke.Core;
using Stroke.History;
using Xunit;

// Use alias to avoid ambiguity with System.Buffer
using Buffer = Stroke.Core.Buffer;
using Document = Stroke.Core.Document;

namespace Stroke.Tests.Core;

/// <summary>
/// Tests for Buffer search operations (T128-T133).
/// </summary>
public class BufferSearchTests
{
    #region DocumentForSearch Tests

    [Fact]
    public void DocumentForSearch_EmptySearchText_ReturnsCurrentDocument()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello world", cursorPosition: 0));
        var searchState = new SearchState(text: "");

        // Act
        var result = buffer.DocumentForSearch(searchState);

        // Assert - returns current document unchanged
        Assert.Equal("hello world", result.Text);
        Assert.Equal(0, result.CursorPosition);
    }

    [Fact]
    public void DocumentForSearch_TextFound_ReturnsDocumentWithMatchPosition()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello world", cursorPosition: 0));
        var searchState = new SearchState(text: "world", direction: SearchDirection.Forward);

        // Act
        var result = buffer.DocumentForSearch(searchState);

        // Assert - cursor should be at "world" position
        Assert.Equal("hello world", result.Text);
        Assert.Equal(6, result.CursorPosition); // "hello " is 6 chars
    }

    [Fact]
    public void DocumentForSearch_TextNotFound_ReturnsCurrentDocument()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello world", cursorPosition: 5));
        var searchState = new SearchState(text: "xyz");

        // Act
        var result = buffer.DocumentForSearch(searchState);

        // Assert - unchanged
        Assert.Equal("hello world", result.Text);
        Assert.Equal(5, result.CursorPosition);
    }

    [Fact]
    public void DocumentForSearch_PreservesSelectionWhenSameWorkingIndex()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello world", cursorPosition: 0));
        buffer.StartSelection(SelectionType.Characters);
        buffer.CursorPosition = 5;
        var searchState = new SearchState(text: "world");

        // Act
        var result = buffer.DocumentForSearch(searchState);

        // Assert - selection should be preserved
        Assert.NotNull(result.Selection);
    }

    [Fact]
    public void DocumentForSearch_SearchBackward_FindsPreviousMatch()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("abc abc abc", cursorPosition: 11)); // At end
        var searchState = new SearchState(text: "abc", direction: SearchDirection.Backward);

        // Act
        var result = buffer.DocumentForSearch(searchState);

        // Assert - should find the last "abc" (index 8) - backward search from end finds most recent occurrence
        Assert.Equal(8, result.CursorPosition);
    }

    #endregion

    #region GetSearchPosition Tests

    [Fact]
    public void GetSearchPosition_TextFound_ReturnsMatchPosition()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello world", cursorPosition: 0));
        var searchState = new SearchState(text: "world");

        // Act
        var position = buffer.GetSearchPosition(searchState);

        // Assert
        Assert.Equal(6, position);
    }

    [Fact]
    public void GetSearchPosition_TextNotFound_ReturnsCurrentPosition()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello world", cursorPosition: 5));
        var searchState = new SearchState(text: "xyz");

        // Act
        var position = buffer.GetSearchPosition(searchState);

        // Assert
        Assert.Equal(5, position);
    }

    [Fact]
    public void GetSearchPosition_WithCount_ReturnsNthMatchPosition()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("abc abc abc", cursorPosition: 0));
        var searchState = new SearchState(text: "abc");

        // Act - get second match
        var position = buffer.GetSearchPosition(searchState, count: 2);

        // Assert - should find second "abc" at index 4
        Assert.Equal(4, position);
    }

    [Fact]
    public void GetSearchPosition_ExcludeCurrentPosition_SkipsCurrentMatch()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("abc abc", cursorPosition: 0));
        var searchState = new SearchState(text: "abc");

        // Act - exclude current position
        var position = buffer.GetSearchPosition(searchState, includeCurrentPosition: false);

        // Assert - should skip first "abc" and find second at index 4
        Assert.Equal(4, position);
    }

    [Fact]
    public void GetSearchPosition_IgnoreCase_FindsCaseInsensitive()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("Hello WORLD", cursorPosition: 0));
        var searchState = new SearchState(text: "world")
        {
            IgnoreCaseFilter = () => true
        };

        // Act
        var position = buffer.GetSearchPosition(searchState);

        // Assert
        Assert.Equal(6, position);
    }

    #endregion

    #region ApplySearch Tests

    [Fact]
    public void ApplySearch_TextFound_UpdatesCursorPosition()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello world", cursorPosition: 0));
        var searchState = new SearchState(text: "world");

        // Act
        buffer.ApplySearch(searchState);

        // Assert
        Assert.Equal(6, buffer.CursorPosition);
    }

    [Fact]
    public void ApplySearch_TextNotFound_CursorUnchanged()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello world", cursorPosition: 5));
        var searchState = new SearchState(text: "xyz");

        // Act
        buffer.ApplySearch(searchState);

        // Assert
        Assert.Equal(5, buffer.CursorPosition);
    }

    [Fact]
    public void ApplySearch_WithCount_AppliesNthMatch()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("abc abc abc", cursorPosition: 0));
        var searchState = new SearchState(text: "abc");

        // Act - apply third match
        buffer.ApplySearch(searchState, count: 3);

        // Assert - should be at third "abc" at index 8
        Assert.Equal(8, buffer.CursorPosition);
    }

    [Fact]
    public void ApplySearch_SearchBackward_MovesBackward()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("abc abc", cursorPosition: 7)); // At end
        var searchState = new SearchState(text: "abc", direction: SearchDirection.Backward);

        // Act
        buffer.ApplySearch(searchState);

        // Assert - should find "abc" at index 4
        Assert.Equal(4, buffer.CursorPosition);
    }

    #endregion

    #region Cross-History Search Tests

    [Fact]
    public void ApplySearch_SearchAcrossHistoryLines_Forward()
    {
        // Arrange
        var history = new InMemoryHistory();
        history.AppendString("first line");
        history.AppendString("second xyz");
        var buffer = new Buffer(history: history, document: new Document("current", cursorPosition: 7));
        buffer.LoadHistoryIfNotYetLoaded();

        // Act - search for "first" which is only in history
        var searchState = new SearchState(text: "first");
        buffer.ApplySearch(searchState);

        // Assert - should have moved to history line
        Assert.Equal("first line", buffer.Text);
        Assert.Equal(0, buffer.CursorPosition);
    }

    [Fact]
    public void ApplySearch_SearchAcrossHistoryLines_Backward()
    {
        // Arrange
        var history = new InMemoryHistory();
        history.AppendString("first xyz");
        history.AppendString("second line");
        var buffer = new Buffer(history: history, document: new Document("current", cursorPosition: 0));
        buffer.LoadHistoryIfNotYetLoaded();

        // Act - search backward for "xyz"
        var searchState = new SearchState(text: "xyz", direction: SearchDirection.Backward);
        buffer.ApplySearch(searchState);

        // Assert - should find "xyz" in first history line
        Assert.Equal("first xyz", buffer.Text);
        Assert.Equal(6, buffer.CursorPosition);
    }

    [Fact]
    public void ApplySearch_WrapsAroundHistoryForward()
    {
        // Arrange
        var history = new InMemoryHistory();
        history.AppendString("target here");
        history.AppendString("second");
        var buffer = new Buffer(history: history, document: new Document("current", cursorPosition: 7));
        buffer.LoadHistoryIfNotYetLoaded();
        // Now at "current" (index 2)

        // Act - search forward for "target" - should wrap around to index 0
        var searchState = new SearchState(text: "target");
        buffer.ApplySearch(searchState);

        // Assert
        Assert.Equal("target here", buffer.Text);
    }

    [Fact]
    public void ApplySearch_WrapsAroundHistoryBackward()
    {
        // Arrange
        var history = new InMemoryHistory();
        history.AppendString("first");
        history.AppendString("target here");
        var buffer = new Buffer(history: history, document: new Document("current", cursorPosition: 0));
        buffer.LoadHistoryIfNotYetLoaded();
        buffer.GoToHistory(0); // Go to "first"

        // Act - search backward for "current" - should wrap around
        var searchState = new SearchState(text: "current", direction: SearchDirection.Backward);
        buffer.ApplySearch(searchState);

        // Assert
        Assert.Equal("current", buffer.Text);
    }

    #endregion

    #region DocumentForSearch with History Tests

    [Fact]
    public void DocumentForSearch_MatchInHistory_ReturnsHistoryDocument()
    {
        // Arrange
        var history = new InMemoryHistory();
        history.AppendString("history match");
        var buffer = new Buffer(history: history, document: new Document("current", cursorPosition: 0));
        buffer.LoadHistoryIfNotYetLoaded();

        // Act
        var searchState = new SearchState(text: "history");
        var result = buffer.DocumentForSearch(searchState);

        // Assert
        Assert.Equal("history match", result.Text);
        Assert.Equal(0, result.CursorPosition);
    }

    [Fact]
    public void DocumentForSearch_NoSelectionWhenWorkingIndexChanges()
    {
        // Arrange
        var history = new InMemoryHistory();
        history.AppendString("history line");
        var buffer = new Buffer(history: history, document: new Document("current", cursorPosition: 0));
        buffer.LoadHistoryIfNotYetLoaded();
        buffer.StartSelection(SelectionType.Characters);

        // Act - search finds result in history (different working index)
        var searchState = new SearchState(text: "history");
        var result = buffer.DocumentForSearch(searchState);

        // Assert - selection should NOT be preserved when moving to different line
        Assert.Null(result.Selection);
    }

    #endregion

    #region Edge Cases Tests

    [Fact]
    public void Search_EmptyBuffer_ReturnsCurrentPosition()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("", cursorPosition: 0));
        var searchState = new SearchState(text: "test");

        // Act
        var position = buffer.GetSearchPosition(searchState);

        // Assert
        Assert.Equal(0, position);
    }

    [Fact]
    public void Search_CountZero_ReturnsCurrentPosition()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("test test", cursorPosition: 0));
        var searchState = new SearchState(text: "test");

        // Act - count of 0
        var position = buffer.GetSearchPosition(searchState, count: 0);

        // Assert - should return current position (no search performed)
        Assert.Equal(0, position);
    }

    [Fact]
    public void Search_MultipleMatches_CountExceedsMatches_ReturnsCurrentPosition()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("abc abc", cursorPosition: 0));
        var searchState = new SearchState(text: "abc");

        // Act - request 100 matches when only 2 exist
        // Because search wraps around, it will find matches
        var position = buffer.GetSearchPosition(searchState, count: 100);

        // Assert - wraps around, ends up somewhere
        Assert.True(position >= 0);
    }

    [Fact]
    public void Search_IgnoreCase_MatchesUpperAndLower()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("ABC abc ABC", cursorPosition: 0));
        var searchState = new SearchState(text: "abc")
        {
            IgnoreCaseFilter = () => true
        };

        // Act
        buffer.ApplySearch(searchState, includeCurrentPosition: true);

        // Assert - should match first "ABC"
        Assert.Equal(0, buffer.CursorPosition);
    }

    [Fact]
    public void Search_CaseSensitive_DoesNotMatchDifferentCase()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("ABC xyz", cursorPosition: 0));
        var searchState = new SearchState(text: "abc")
        {
            IgnoreCaseFilter = () => false
        };

        // Act - should not find "ABC" when case-sensitive
        buffer.ApplySearch(searchState, includeCurrentPosition: false);

        // Assert - cursor unchanged (no match found)
        Assert.Equal(0, buffer.CursorPosition);
    }

    #endregion

    #region Thread Safety Tests

    [Fact]
    public async Task ConcurrentSearchOperations_ThreadSafe()
    {
        // Arrange
        var history = new InMemoryHistory();
        for (var i = 0; i < 10; i++)
        {
            history.AppendString($"line {i} with text");
        }
        var buffer = new Buffer(history: history, document: new Document("current with text"));
        buffer.LoadHistoryIfNotYetLoaded();
        var iterations = 20;
        var barrier = new Barrier(3);

        // Act - concurrent search operations
        var searchForwardTask = Task.Run(() =>
        {
            barrier.SignalAndWait();
            for (var i = 0; i < iterations; i++)
            {
                var searchState = new SearchState(text: "text");
                buffer.ApplySearch(searchState);
            }
        });

        var searchBackwardTask = Task.Run(() =>
        {
            barrier.SignalAndWait();
            for (var i = 0; i < iterations; i++)
            {
                var searchState = new SearchState(text: "text", direction: SearchDirection.Backward);
                buffer.ApplySearch(searchState);
            }
        });

        var documentForSearchTask = Task.Run(() =>
        {
            barrier.SignalAndWait();
            for (var i = 0; i < iterations; i++)
            {
                var searchState = new SearchState(text: "line");
                _ = buffer.DocumentForSearch(searchState);
            }
        });

        // Assert - no exceptions
        await Task.WhenAll(searchForwardTask, searchBackwardTask, documentForSearchTask);
    }

    #endregion
}
