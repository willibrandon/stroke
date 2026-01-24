using Stroke.History;
using Xunit;

// Use alias to avoid ambiguity with System.Buffer
using Buffer = Stroke.Core.Buffer;
using Document = Stroke.Core.Document;

namespace Stroke.Tests.Core;

/// <summary>
/// Tests for Buffer history navigation operations (T060-T063).
/// </summary>
public class BufferHistoryTests
{
    #region LoadHistoryIfNotYetLoaded Tests

    [Fact]
    public void LoadHistoryIfNotYetLoaded_LoadsHistoryEntries()
    {
        // Arrange
        var history = new InMemoryHistory();
        history.AppendString("first");
        history.AppendString("second");
        history.AppendString("third");
        var buffer = new Buffer(history: history, document: new Document("current"));

        // Act
        buffer.LoadHistoryIfNotYetLoaded();

        // Assert - should have history entries plus current
        buffer.HistoryBackward();
        Assert.Equal("third", buffer.Text);
        buffer.HistoryBackward();
        Assert.Equal("second", buffer.Text);
        buffer.HistoryBackward();
        Assert.Equal("first", buffer.Text);
    }

    [Fact]
    public void LoadHistoryIfNotYetLoaded_OnlyLoadsOnce()
    {
        // Arrange
        var history = new InMemoryHistory();
        history.AppendString("one");
        var buffer = new Buffer(history: history, document: new Document("current"));

        // Act - call multiple times
        buffer.LoadHistoryIfNotYetLoaded();
        buffer.LoadHistoryIfNotYetLoaded();
        buffer.LoadHistoryIfNotYetLoaded();

        // Assert - should still have only one history entry
        buffer.HistoryBackward();
        Assert.Equal("one", buffer.Text);
        buffer.HistoryBackward(); // Should stay at "one" (can't go further back)
        Assert.Equal("one", buffer.Text);
    }

    #endregion

    #region HistoryBackward Tests

    [Fact]
    public void HistoryBackward_MovesToPreviousEntry()
    {
        // Arrange
        var history = new InMemoryHistory();
        history.AppendString("prev");
        var buffer = new Buffer(history: history, document: new Document("current"));
        buffer.LoadHistoryIfNotYetLoaded();

        // Act
        buffer.HistoryBackward();

        // Assert
        Assert.Equal("prev", buffer.Text);
    }

    [Fact]
    public void HistoryBackward_WithCount_MovesMultipleEntries()
    {
        // Arrange
        var history = new InMemoryHistory();
        history.AppendString("first");
        history.AppendString("second");
        history.AppendString("third");
        var buffer = new Buffer(history: history, document: new Document("current"));
        buffer.LoadHistoryIfNotYetLoaded();

        // Act
        buffer.HistoryBackward(2);

        // Assert - should be at "second" (skipped "third")
        Assert.Equal("second", buffer.Text);
    }

    [Fact]
    public void HistoryBackward_AtStart_StaysAtStart()
    {
        // Arrange
        var history = new InMemoryHistory();
        history.AppendString("only");
        var buffer = new Buffer(history: history, document: new Document("current"));
        buffer.LoadHistoryIfNotYetLoaded();
        buffer.HistoryBackward(); // Now at "only"

        // Act - try to go back further
        buffer.HistoryBackward();

        // Assert - should stay at "only"
        Assert.Equal("only", buffer.Text);
    }

    [Fact]
    public void HistoryBackward_MovesCursorToEnd()
    {
        // Arrange
        var history = new InMemoryHistory();
        history.AppendString("hello world");
        var buffer = new Buffer(history: history, document: new Document("current", cursorPosition: 0));
        buffer.LoadHistoryIfNotYetLoaded();

        // Act
        buffer.HistoryBackward();

        // Assert - cursor should be at end of text
        Assert.Equal("hello world".Length, buffer.CursorPosition);
    }

    [Fact]
    public void HistoryBackward_EmptyHistory_DoesNothing()
    {
        // Arrange - use fresh InMemoryHistory to avoid shared state contamination
        var buffer = new Buffer(history: new InMemoryHistory(), document: new Document("current", cursorPosition: 3));

        // Act
        buffer.HistoryBackward();

        // Assert
        Assert.Equal("current", buffer.Text);
        Assert.Equal(3, buffer.CursorPosition);
    }

    #endregion

    #region HistoryForward Tests

    [Fact]
    public void HistoryForward_MovesToNextEntry()
    {
        // Arrange
        var history = new InMemoryHistory();
        history.AppendString("first");
        history.AppendString("second");
        var buffer = new Buffer(history: history, document: new Document("current"));
        buffer.LoadHistoryIfNotYetLoaded();
        buffer.HistoryBackward(2); // Go to "first"

        // Act
        buffer.HistoryForward();

        // Assert
        Assert.Equal("second", buffer.Text);
    }

    [Fact]
    public void HistoryForward_WithCount_MovesMultipleEntries()
    {
        // Arrange
        var history = new InMemoryHistory();
        history.AppendString("first");
        history.AppendString("second");
        history.AppendString("third");
        var buffer = new Buffer(history: history, document: new Document("current"));
        buffer.LoadHistoryIfNotYetLoaded();
        buffer.HistoryBackward(3); // Go to "first"

        // Act
        buffer.HistoryForward(2);

        // Assert - should be at "third"
        Assert.Equal("third", buffer.Text);
    }

    [Fact]
    public void HistoryForward_AtEnd_StaysAtEnd()
    {
        // Arrange
        var history = new InMemoryHistory();
        history.AppendString("prev");
        var buffer = new Buffer(history: history, document: new Document("current"));
        buffer.LoadHistoryIfNotYetLoaded();

        // Act - try to go forward when already at end
        buffer.HistoryForward();

        // Assert - should stay at "current"
        Assert.Equal("current", buffer.Text);
    }

    [Fact]
    public void HistoryForward_MovesCursorToEndOfFirstLine()
    {
        // Arrange
        var history = new InMemoryHistory();
        history.AppendString("first");
        history.AppendString("second line\nmore text");
        var buffer = new Buffer(history: history, document: new Document("current"));
        buffer.LoadHistoryIfNotYetLoaded();
        buffer.HistoryBackward(2); // Go to "first"

        // Act
        buffer.HistoryForward();

        // Assert - cursor at end of first line ("second line")
        Assert.Equal("second line".Length, buffer.CursorPosition);
    }

    #endregion

    #region GoToHistory Tests

    [Fact]
    public void GoToHistory_GoesToSpecificIndex()
    {
        // Arrange
        var history = new InMemoryHistory();
        history.AppendString("zero");
        history.AppendString("one");
        history.AppendString("two");
        var buffer = new Buffer(history: history, document: new Document("current"));
        buffer.LoadHistoryIfNotYetLoaded();

        // Act - go to index 1 (should be "one")
        buffer.GoToHistory(1);

        // Assert
        Assert.Equal("one", buffer.Text);
    }

    [Fact]
    public void GoToHistory_MovesCursorToEnd()
    {
        // Arrange
        var history = new InMemoryHistory();
        history.AppendString("hello world");
        var buffer = new Buffer(history: history, document: new Document("current", cursorPosition: 0));
        buffer.LoadHistoryIfNotYetLoaded();

        // Act
        buffer.GoToHistory(0);

        // Assert
        Assert.Equal("hello world".Length, buffer.CursorPosition);
    }

    [Fact]
    public void GoToHistory_InvalidIndex_DoesNothing()
    {
        // Arrange
        var history = new InMemoryHistory();
        history.AppendString("only");
        var buffer = new Buffer(history: history, document: new Document("current"));
        buffer.LoadHistoryIfNotYetLoaded();

        // Act - try invalid indices
        buffer.GoToHistory(-1);
        Assert.Equal("current", buffer.Text);

        buffer.GoToHistory(100);
        Assert.Equal("current", buffer.Text);
    }

    #endregion

    #region History Search Tests

    [Fact]
    public void HistoryBackward_WithSearchEnabled_FiltersbyPrefix()
    {
        // Arrange
        var history = new InMemoryHistory();
        history.AppendString("abc first");
        history.AppendString("xyz middle");
        history.AppendString("abc second");
        history.AppendString("abc third");
        var buffer = new Buffer(
            history: history,
            document: new Document("abc", cursorPosition: 3), // cursor at end of "abc"
            enableHistorySearch: () => true);
        buffer.LoadHistoryIfNotYetLoaded();

        // Act - search backward
        buffer.HistoryBackward();

        // Assert - should find "abc third" (skipping "xyz middle")
        Assert.Equal("abc third", buffer.Text);
    }

    [Fact]
    public void HistoryBackward_WithSearchEnabled_SkipsNonMatching()
    {
        // Arrange
        var history = new InMemoryHistory();
        history.AppendString("foo one");
        history.AppendString("bar two");
        history.AppendString("foo three");
        var buffer = new Buffer(
            history: history,
            document: new Document("foo", cursorPosition: 3),
            enableHistorySearch: () => true);
        buffer.LoadHistoryIfNotYetLoaded();

        // Act - search backward twice
        buffer.HistoryBackward();
        Assert.Equal("foo three", buffer.Text);

        buffer.HistoryBackward();
        Assert.Equal("foo one", buffer.Text); // Skips "bar two"
    }

    [Fact]
    public void HistoryForward_WithSearchEnabled_FiltersbyPrefix()
    {
        // Arrange
        var history = new InMemoryHistory();
        history.AppendString("test one");
        history.AppendString("other");
        history.AppendString("test two");
        var buffer = new Buffer(
            history: history,
            document: new Document("test", cursorPosition: 4),
            enableHistorySearch: () => true);
        buffer.LoadHistoryIfNotYetLoaded();
        buffer.HistoryBackward(2); // Go to "test one"

        // Act
        buffer.HistoryForward();

        // Assert - should find "test two" (skipping "other")
        Assert.Equal("test two", buffer.Text);
    }

    [Fact]
    public void HistorySearch_DisabledByDefault_MatchesAll()
    {
        // Arrange
        var history = new InMemoryHistory();
        history.AppendString("abc");
        history.AppendString("xyz");
        var buffer = new Buffer(
            history: history,
            document: new Document("abc")); // enableHistorySearch defaults to false
        buffer.LoadHistoryIfNotYetLoaded();

        // Act
        buffer.HistoryBackward();

        // Assert - should match "xyz" since search is disabled
        Assert.Equal("xyz", buffer.Text);
    }

    #endregion

    #region AutoUp/AutoDown History Integration Tests

    [Fact]
    public void AutoUp_OnFirstLine_NavigatesHistory()
    {
        // Arrange
        var history = new InMemoryHistory();
        history.AppendString("previous");
        var buffer = new Buffer(history: history, document: new Document("current"));
        buffer.LoadHistoryIfNotYetLoaded();

        // Act
        buffer.AutoUp();

        // Assert
        Assert.Equal("previous", buffer.Text);
    }

    [Fact]
    public void AutoDown_OnLastLine_NavigatesHistory()
    {
        // Arrange
        var history = new InMemoryHistory();
        history.AppendString("first");
        history.AppendString("second");
        var buffer = new Buffer(history: history, document: new Document("current"));
        buffer.LoadHistoryIfNotYetLoaded();
        buffer.HistoryBackward(2); // Go to "first"

        // Act
        buffer.AutoDown();

        // Assert
        Assert.Equal("second", buffer.Text);
    }

    [Fact]
    public void AutoUp_WithGoToStartOfLine_MovesCursorToStart()
    {
        // Arrange
        var history = new InMemoryHistory();
        history.AppendString("previous command");
        var buffer = new Buffer(history: history, document: new Document("current", cursorPosition: 7));
        buffer.LoadHistoryIfNotYetLoaded();

        // Act
        buffer.AutoUp(goToStartOfLineIfHistoryChanges: true);

        // Assert - cursor should be at start of line (based on Document.GetStartOfLinePosition)
        Assert.Equal(0, buffer.CursorPosition);
    }

    #endregion

    #region Working Index Tests

    [Fact]
    public void WorkingIndex_ReflectsCurrentPosition()
    {
        // Arrange
        var history = new InMemoryHistory();
        history.AppendString("first");
        history.AppendString("second");
        var buffer = new Buffer(history: history, document: new Document("current"));

        // Assert - initial index is 0 (pointing to "current")
        Assert.Equal(0, buffer.WorkingIndex);

        // Load history
        buffer.LoadHistoryIfNotYetLoaded();

        // Assert - after loading, index should be 2 (pointing to "current" which is now at end)
        Assert.Equal(2, buffer.WorkingIndex);

        // Navigate back
        buffer.HistoryBackward();
        Assert.Equal(1, buffer.WorkingIndex);

        buffer.HistoryBackward();
        Assert.Equal(0, buffer.WorkingIndex);
    }

    #endregion

    #region Thread Safety Tests

    [Fact]
    public async Task ConcurrentHistoryNavigation_ThreadSafe()
    {
        // Arrange
        var history = new InMemoryHistory();
        for (var i = 0; i < 20; i++)
        {
            history.AppendString($"entry {i}");
        }

        var buffer = new Buffer(history: history, document: new Document("current"));
        var iterations = 30;
        var barrier = new Barrier(3);

        // Act - concurrent history operations
        var backwardTask = Task.Run(() =>
        {
            barrier.SignalAndWait();
            for (var i = 0; i < iterations; i++)
            {
                buffer.HistoryBackward();
            }
        });

        var forwardTask = Task.Run(() =>
        {
            barrier.SignalAndWait();
            for (var i = 0; i < iterations; i++)
            {
                buffer.HistoryForward();
            }
        });

        var gotoTask = Task.Run(() =>
        {
            barrier.SignalAndWait();
            for (var i = 0; i < iterations; i++)
            {
                buffer.GoToHistory(i % 10);
            }
        });

        // Assert - no exceptions
        await Task.WhenAll(backwardTask, forwardTask, gotoTask);
    }

    #endregion

    #region History Reset Tests

    [Fact]
    public void Reset_ClearsHistory()
    {
        // Arrange
        var history = new InMemoryHistory();
        history.AppendString("prev");
        var buffer = new Buffer(history: history, document: new Document("current"));
        buffer.LoadHistoryIfNotYetLoaded();
        buffer.HistoryBackward();
        Assert.Equal("prev", buffer.Text);

        // Act
        buffer.Reset(new Document("new"));

        // Assert - history navigation should start fresh
        Assert.Equal("new", buffer.Text);
        Assert.Equal(0, buffer.WorkingIndex);
    }

    [Fact]
    public void Reset_WithAppendToHistory_SavesCurrentText()
    {
        // Arrange
        var history = new InMemoryHistory();
        var buffer = new Buffer(history: history, document: new Document("to save"));

        // Act
        buffer.Reset(new Document("new"), appendToHistory: true);

        // Assert - "to save" should be in history
        var strings = history.GetStrings();
        Assert.Contains("to save", strings);
    }

    #endregion

    #region YankNthArg Tests

    [Fact]
    public void YankNthArg_EmptyHistory_DoesNothing()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("current"));

        // Act
        buffer.YankNthArg();

        // Assert - text unchanged
        Assert.Equal("current", buffer.Text);
    }

    [Fact]
    public void YankNthArg_YanksFirstArgByDefault()
    {
        // Arrange
        var history = new InMemoryHistory();
        history.AppendString("echo hello world");
        var buffer = new Buffer(history: history, document: new Document(""));

        // Act
        buffer.YankNthArg();

        // Assert - should yank "hello" (index 1, the first argument)
        Assert.Equal("hello", buffer.Text);
    }

    [Fact]
    public void YankNthArg_WithN_YanksSpecificArg()
    {
        // Arrange
        var history = new InMemoryHistory();
        history.AppendString("echo hello world foo");
        var buffer = new Buffer(history: history, document: new Document(""));

        // Act - yank the command itself (n=0)
        buffer.YankNthArg(0);

        // Assert
        Assert.Equal("echo", buffer.Text);
    }

    [Fact]
    public void YankNthArg_WithN2_YanksSecondArg()
    {
        // Arrange
        var history = new InMemoryHistory();
        history.AppendString("echo hello world foo");
        var buffer = new Buffer(history: history, document: new Document(""));

        // Act - yank the second argument (n=2)
        buffer.YankNthArg(2);

        // Assert
        Assert.Equal("world", buffer.Text);
    }

    [Fact]
    public void YankNthArg_InvalidIndex_InsertsEmpty()
    {
        // Arrange
        var history = new InMemoryHistory();
        history.AppendString("echo hello");
        var buffer = new Buffer(history: history, document: new Document(""));

        // Act - try to yank non-existent argument
        buffer.YankNthArg(10);

        // Assert - empty string inserted
        Assert.Equal("", buffer.Text);
    }

    [Fact]
    public void YankNthArg_RepeatedCalls_CyclesThroughHistory()
    {
        // Arrange
        var history = new InMemoryHistory();
        history.AppendString("first alpha beta");
        history.AppendString("second gamma delta");
        history.AppendString("third epsilon zeta");
        var buffer = new Buffer(history: history, document: new Document(""));

        // Act & Assert - first call gets from most recent
        buffer.YankNthArg();
        Assert.Equal("epsilon", buffer.Text); // first arg of "third epsilon zeta"

        // Second call cycles to previous history entry
        buffer.YankNthArg();
        Assert.Equal("gamma", buffer.Text); // first arg of "second gamma delta"

        // Third call cycles to oldest
        buffer.YankNthArg();
        Assert.Equal("alpha", buffer.Text); // first arg of "first alpha beta"

        // Fourth call wraps around to most recent
        buffer.YankNthArg();
        Assert.Equal("epsilon", buffer.Text); // back to "third epsilon zeta"
    }

    [Fact]
    public void YankNthArg_HandlesQuotedStrings()
    {
        // Arrange
        var history = new InMemoryHistory();
        history.AppendString("echo \"hello world\" foo");
        var buffer = new Buffer(history: history, document: new Document(""));

        // Act - yank the quoted string (first arg)
        buffer.YankNthArg();

        // Assert - should get content without quotes
        Assert.Equal("hello world", buffer.Text);
    }

    [Fact]
    public void YankNthArg_HandlesSingleQuotedStrings()
    {
        // Arrange
        var history = new InMemoryHistory();
        history.AppendString("echo 'hello world' foo");
        var buffer = new Buffer(history: history, document: new Document(""));

        // Act - yank the quoted string (first arg)
        buffer.YankNthArg();

        // Assert - should get content without quotes
        Assert.Equal("hello world", buffer.Text);
    }

    [Fact]
    public void YankNthArg_DeletesPreviousInsertedWord()
    {
        // Arrange
        var history = new InMemoryHistory();
        history.AppendString("first alpha beta");
        history.AppendString("second gamma delta");
        var buffer = new Buffer(history: history, document: new Document("prefix "));
        buffer.CursorPosition = 7; // At end of "prefix "

        // Act - yank first time
        buffer.YankNthArg();
        Assert.Equal("prefix gamma", buffer.Text);

        // Yank again - should replace "gamma" with "alpha"
        buffer.YankNthArg();
        Assert.Equal("prefix alpha", buffer.Text);
    }

    [Fact]
    public void YankNthArg_TracksState()
    {
        // Arrange
        var history = new InMemoryHistory();
        history.AppendString("echo hello world");
        var buffer = new Buffer(history: history, document: new Document(""));

        // Assert - no state initially
        Assert.Null(buffer.YankNthArgState);

        // Act
        buffer.YankNthArg();

        // Assert - state is set
        Assert.NotNull(buffer.YankNthArgState);
        Assert.Equal(-1, buffer.YankNthArgState.HistoryPosition);
        Assert.Equal("hello", buffer.YankNthArgState.PreviousInsertedWord);
    }

    #endregion

    #region YankLastArg Tests

    [Fact]
    public void YankLastArg_EmptyHistory_DoesNothing()
    {
        // Arrange - use fresh InMemoryHistory to avoid shared state contamination
        var buffer = new Buffer(history: new InMemoryHistory(), document: new Document("current"));

        // Act
        buffer.YankLastArg();

        // Assert - text unchanged
        Assert.Equal("current", buffer.Text);
    }

    [Fact]
    public void YankLastArg_YanksLastArgByDefault()
    {
        // Arrange
        var history = new InMemoryHistory();
        history.AppendString("echo hello world");
        var buffer = new Buffer(history: history, document: new Document(""));

        // Act
        buffer.YankLastArg();

        // Assert - should yank "world" (last word)
        Assert.Equal("world", buffer.Text);
    }

    [Fact]
    public void YankLastArg_WithN_YanksSpecificArg()
    {
        // Arrange
        var history = new InMemoryHistory();
        history.AppendString("echo hello world foo");
        var buffer = new Buffer(history: history, document: new Document(""));

        // Act - yank the first argument (n=1)
        buffer.YankLastArg(1);

        // Assert
        Assert.Equal("hello", buffer.Text);
    }

    [Fact]
    public void YankLastArg_SingleWord_YanksThatWord()
    {
        // Arrange
        var history = new InMemoryHistory();
        history.AppendString("single");
        var buffer = new Buffer(history: history, document: new Document(""));

        // Act
        buffer.YankLastArg();

        // Assert
        Assert.Equal("single", buffer.Text);
    }

    [Fact]
    public void YankLastArg_RepeatedCalls_CyclesThroughHistory()
    {
        // Arrange
        var history = new InMemoryHistory();
        history.AppendString("first alpha beta");
        history.AppendString("second gamma delta");
        var buffer = new Buffer(history: history, document: new Document(""));

        // Act & Assert - first call gets last arg from most recent
        buffer.YankLastArg();
        Assert.Equal("delta", buffer.Text);

        // Second call cycles to previous history entry
        buffer.YankLastArg();
        Assert.Equal("beta", buffer.Text);
    }

    [Fact]
    public void YankLastArg_EmptyLine_InsertsEmpty()
    {
        // Arrange
        var history = new InMemoryHistory();
        history.AppendString("   "); // whitespace only
        var buffer = new Buffer(history: history, document: new Document(""));

        // Act
        buffer.YankLastArg();

        // Assert - empty string (no words)
        Assert.Equal("", buffer.Text);
    }

    [Fact]
    public void YankLastArg_PreservesExistingText()
    {
        // Arrange
        var history = new InMemoryHistory();
        history.AppendString("echo hello world");
        var buffer = new Buffer(history: history, document: new Document("prefix "));
        buffer.CursorPosition = 7; // At end

        // Act
        buffer.YankLastArg();

        // Assert
        Assert.Equal("prefix world", buffer.Text);
    }

    #endregion

    #region SplitIntoWords Tests (via YankNthArg behavior)

    [Fact]
    public void YankNthArg_MultipleWhitespace_TreatedAsSingleSeparator()
    {
        // Arrange
        var history = new InMemoryHistory();
        history.AppendString("echo    hello    world");
        var buffer = new Buffer(history: history, document: new Document(""));

        // Act
        buffer.YankNthArg(2);

        // Assert - whitespace collapsed, still gets "world"
        Assert.Equal("world", buffer.Text);
    }

    [Fact]
    public void YankNthArg_TabsAndSpaces_BothSeparate()
    {
        // Arrange
        var history = new InMemoryHistory();
        history.AppendString("echo\thello\tworld");
        var buffer = new Buffer(history: history, document: new Document(""));

        // Act
        buffer.YankNthArg(1);

        // Assert
        Assert.Equal("hello", buffer.Text);
    }

    [Fact]
    public void YankNthArg_UnmatchedQuote_TreatsRestAsOneWord()
    {
        // Arrange
        var history = new InMemoryHistory();
        history.AppendString("echo \"hello world");
        var buffer = new Buffer(history: history, document: new Document(""));

        // Act - yank the quoted portion
        buffer.YankNthArg(1);

        // Assert - unmatched quote means everything after quote is one word
        Assert.Equal("hello world", buffer.Text);
    }

    #endregion
}
