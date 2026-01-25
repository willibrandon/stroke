using Xunit;

// Use aliases to avoid naming conflicts
using Buffer = Stroke.Core.Buffer;
using CompletionState = Stroke.Core.CompletionState;
using Document = Stroke.Core.Document;
using CompletionItem = Stroke.Completion.Completion;

namespace Stroke.Tests.Core;

/// <summary>
/// Tests for Buffer completion operations (T082-T095).
/// </summary>
public class BufferCompletionTests
{
    #region SetCompletions Tests

    [Fact]
    public void SetCompletions_CreatesCompletionState()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hel", cursorPosition: 3));
        var completions = new List<CompletionItem>
        {
            new("hello", startPosition: -3),
            new("help", startPosition: -3),
            new("helicopter", startPosition: -3)
        };

        // Act
        var state = buffer.SetCompletions(completions);

        // Assert
        Assert.NotNull(buffer.CompleteState);
        Assert.Equal(3, buffer.CompleteState.Completions.Count);
        Assert.Null(buffer.CompleteState.CompleteIndex); // No selection initially
    }

    [Fact]
    public void SetCompletions_StoresOriginalDocument()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hel", cursorPosition: 3));
        var completions = new List<CompletionItem> { new("hello", startPosition: -3) };

        // Act
        buffer.SetCompletions(completions);

        // Assert
        Assert.Equal("hel", buffer.CompleteState!.OriginalDocument.Text);
        Assert.Equal(3, buffer.CompleteState.OriginalDocument.CursorPosition);
    }

    [Fact]
    public void SetCompletions_EmptyList_CreatesEmptyState()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("test"));
        var completions = new List<CompletionItem>();

        // Act
        buffer.SetCompletions(completions);

        // Assert
        Assert.NotNull(buffer.CompleteState);
        Assert.Empty(buffer.CompleteState.Completions);
    }

    #endregion

    #region CompleteNext Tests

    [Fact]
    public void CompleteNext_SelectsFirstCompletion()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hel", cursorPosition: 3));
        var completions = new List<CompletionItem>
        {
            new("hello", startPosition: -3),
            new("help", startPosition: -3)
        };
        buffer.SetCompletions(completions);

        // Act
        buffer.CompleteNext();

        // Assert
        Assert.Equal(0, buffer.CompleteState!.CompleteIndex);
        Assert.Equal("hello", buffer.Text);
    }

    [Fact]
    public void CompleteNext_AdvancesToNextCompletion()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hel", cursorPosition: 3));
        var completions = new List<CompletionItem>
        {
            new("hello", startPosition: -3),
            new("help", startPosition: -3),
            new("helicopter", startPosition: -3)
        };
        buffer.SetCompletions(completions);
        buffer.CompleteNext(); // Select first

        // Act
        buffer.CompleteNext();

        // Assert
        Assert.Equal(1, buffer.CompleteState!.CompleteIndex);
        Assert.Equal("help", buffer.Text);
    }

    [Fact]
    public void CompleteNext_AtEnd_WrapsToNull()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hel", cursorPosition: 3));
        var completions = new List<CompletionItem>
        {
            new("hello", startPosition: -3),
            new("help", startPosition: -3)
        };
        buffer.SetCompletions(completions);
        buffer.CompleteNext(); // 0
        buffer.CompleteNext(); // 1

        // Act
        buffer.CompleteNext(); // Should wrap to null

        // Assert
        Assert.Null(buffer.CompleteState!.CompleteIndex);
        Assert.Equal("hel", buffer.Text); // Back to original
    }

    [Fact]
    public void CompleteNext_DisableWrapAround_StaysAtEnd()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hel", cursorPosition: 3));
        var completions = new List<CompletionItem>
        {
            new("hello", startPosition: -3),
            new("help", startPosition: -3)
        };
        buffer.SetCompletions(completions);
        buffer.CompleteNext(); // 0
        buffer.CompleteNext(); // 1

        // Act
        buffer.CompleteNext(disableWrapAround: true);

        // Assert
        Assert.Equal(1, buffer.CompleteState!.CompleteIndex); // Stays at 1
        Assert.Equal("help", buffer.Text);
    }

    [Fact]
    public void CompleteNext_WithCount_SkipsCompletions()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hel", cursorPosition: 3));
        var completions = new List<CompletionItem>
        {
            new("a", startPosition: -3),
            new("b", startPosition: -3),
            new("c", startPosition: -3),
            new("d", startPosition: -3)
        };
        buffer.SetCompletions(completions);
        buffer.CompleteNext(); // Select first

        // Act
        buffer.CompleteNext(count: 2);

        // Assert
        Assert.Equal(2, buffer.CompleteState!.CompleteIndex); // Jumped from 0 to 2
    }

    [Fact]
    public void CompleteNext_NoCompletionState_DoesNothing()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("test", cursorPosition: 4));

        // Act & Assert - no exception
        buffer.CompleteNext();
        Assert.Null(buffer.CompleteState);
    }

    #endregion

    #region CompletePrevious Tests

    [Fact]
    public void CompletePrevious_FromNull_SelectsLast()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hel", cursorPosition: 3));
        var completions = new List<CompletionItem>
        {
            new("hello", startPosition: -3),
            new("help", startPosition: -3),
            new("helicopter", startPosition: -3)
        };
        buffer.SetCompletions(completions);

        // Act
        buffer.CompletePrevious();

        // Assert
        Assert.Equal(2, buffer.CompleteState!.CompleteIndex);
        Assert.Equal("helicopter", buffer.Text);
    }

    [Fact]
    public void CompletePrevious_GoesToPreviousCompletion()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hel", cursorPosition: 3));
        var completions = new List<CompletionItem>
        {
            new("hello", startPosition: -3),
            new("help", startPosition: -3),
            new("helicopter", startPosition: -3)
        };
        buffer.SetCompletions(completions);
        buffer.CompleteNext(); // 0
        buffer.CompleteNext(); // 1

        // Act
        buffer.CompletePrevious();

        // Assert
        Assert.Equal(0, buffer.CompleteState!.CompleteIndex);
        Assert.Equal("hello", buffer.Text);
    }

    [Fact]
    public void CompletePrevious_AtStart_WrapsToNull()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hel", cursorPosition: 3));
        var completions = new List<CompletionItem>
        {
            new("hello", startPosition: -3),
            new("help", startPosition: -3)
        };
        buffer.SetCompletions(completions);
        buffer.CompleteNext(); // Select first (0)

        // Act
        buffer.CompletePrevious(); // Should wrap to null

        // Assert
        Assert.Null(buffer.CompleteState!.CompleteIndex);
        Assert.Equal("hel", buffer.Text);
    }

    [Fact]
    public void CompletePrevious_DisableWrapAround_StaysAtStart()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hel", cursorPosition: 3));
        var completions = new List<CompletionItem>
        {
            new("hello", startPosition: -3),
            new("help", startPosition: -3)
        };
        buffer.SetCompletions(completions);
        buffer.CompleteNext(); // 0

        // Act
        buffer.CompletePrevious(disableWrapAround: true);

        // Assert
        Assert.Equal(0, buffer.CompleteState!.CompleteIndex); // Stays at 0
    }

    #endregion

    #region GoToCompletion Tests

    [Fact]
    public void GoToCompletion_SelectsSpecificIndex()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hel", cursorPosition: 3));
        var completions = new List<CompletionItem>
        {
            new("hello", startPosition: -3),
            new("help", startPosition: -3),
            new("helicopter", startPosition: -3)
        };
        buffer.SetCompletions(completions);

        // Act
        buffer.GoToCompletion(2);

        // Assert
        Assert.Equal(2, buffer.CompleteState!.CompleteIndex);
        Assert.Equal("helicopter", buffer.Text);
    }

    [Fact]
    public void GoToCompletion_Null_RestoresOriginal()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hel", cursorPosition: 3));
        var completions = new List<CompletionItem>
        {
            new("hello", startPosition: -3)
        };
        buffer.SetCompletions(completions);
        buffer.CompleteNext(); // Select first
        Assert.Equal("hello", buffer.Text);

        // Act
        buffer.GoToCompletion(null);

        // Assert
        Assert.Null(buffer.CompleteState!.CompleteIndex);
        Assert.Equal("hel", buffer.Text);
    }

    #endregion

    #region CancelCompletion Tests

    [Fact]
    public void CancelCompletion_RestoresOriginalAndClearsState()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hel", cursorPosition: 3));
        var completions = new List<CompletionItem> { new("hello", startPosition: -3) };
        buffer.SetCompletions(completions);
        buffer.CompleteNext();
        Assert.Equal("hello", buffer.Text);

        // Act
        buffer.CancelCompletion();

        // Assert
        Assert.Null(buffer.CompleteState);
        Assert.Equal("hel", buffer.Text);
    }

    [Fact]
    public void CancelCompletion_NoCompletionState_DoesNothing()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("test"));

        // Act & Assert - no exception
        buffer.CancelCompletion();
        Assert.Null(buffer.CompleteState);
    }

    #endregion

    #region ApplyCompletion Tests

    [Fact]
    public void ApplyCompletion_InsertsCompletionText()
    {
        // Arrange - startPosition: -3 means replace the 3 chars before cursor
        var buffer = new Buffer(document: new Document("hel", cursorPosition: 3));
        var completion = new CompletionItem("hello", startPosition: -3);

        // Act
        buffer.ApplyCompletion(completion);

        // Assert
        Assert.Equal("hello", buffer.Text);
        Assert.Equal(5, buffer.CursorPosition);
    }

    [Fact]
    public void ApplyCompletion_ClearsCompletionState()
    {
        // Arrange - startPosition: -3 means replace the 3 chars before cursor
        var buffer = new Buffer(document: new Document("hel", cursorPosition: 3));
        var completions = new List<CompletionItem> { new("hello", startPosition: -3) };
        buffer.SetCompletions(completions);
        buffer.CompleteNext();

        // Act
        buffer.ApplyCompletion(new CompletionItem("helper", startPosition: -3));

        // Assert
        Assert.Null(buffer.CompleteState);
        Assert.Equal("helper", buffer.Text);
    }

    [Fact]
    public void ApplyCompletion_PreservesTextAfterCursor()
    {
        // Arrange - startPosition: -3 means replace the 3 chars before cursor
        var buffer = new Buffer(document: new Document("hel world", cursorPosition: 3));
        var completion = new CompletionItem("hello", startPosition: -3);

        // Act
        buffer.ApplyCompletion(completion);

        // Assert
        Assert.Equal("hello world", buffer.Text);
    }

    [Fact]
    public void ApplyCompletion_WithZeroStartPosition_InsertsAtCursor()
    {
        // Arrange - startPosition: 0 means insert at cursor without deleting
        var buffer = new Buffer(document: new Document("hel", cursorPosition: 3));
        var completion = new CompletionItem("lo", startPosition: 0);

        // Act
        buffer.ApplyCompletion(completion);

        // Assert - "hel" + "lo" = "hello"
        Assert.Equal("hello", buffer.Text);
        Assert.Equal(5, buffer.CursorPosition);
    }

    [Fact]
    public void ApplyCompletion_WithNegativeStartPosition_ReplacesTextBeforeCursor()
    {
        // Arrange - user typed "hel" and completion replaces it with "hello"
        // startPosition: -3 means replace the 3 characters before cursor
        var buffer = new Buffer(document: new Document("hel", cursorPosition: 3));
        var completion = new CompletionItem("hello", startPosition: -3);

        // Act
        buffer.ApplyCompletion(completion);

        // Assert - "hel" replaced with "hello"
        Assert.Equal("hello", buffer.Text);
        Assert.Equal(5, buffer.CursorPosition);
    }

    [Fact]
    public void ApplyCompletion_WithNegativeStartPosition_PreservesTextAfterCursor()
    {
        // Arrange - user typed "hel world" with cursor after "hel"
        var buffer = new Buffer(document: new Document("hel world", cursorPosition: 3));
        var completion = new CompletionItem("hello", startPosition: -3);

        // Act
        buffer.ApplyCompletion(completion);

        // Assert - "hel" replaced with "hello", " world" preserved
        Assert.Equal("hello world", buffer.Text);
        Assert.Equal(5, buffer.CursorPosition);
    }

    [Fact]
    public void ApplyCompletion_WithNegativeStartPosition_PartialReplacement()
    {
        // Arrange - user typed "print(" and completion for function
        // startPosition: -1 means replace just the "(" character
        var buffer = new Buffer(document: new Document("print(", cursorPosition: 6));
        var completion = new CompletionItem("(x)", startPosition: -1);

        // Act
        buffer.ApplyCompletion(completion);

        // Assert
        Assert.Equal("print(x)", buffer.Text);
        Assert.Equal(8, buffer.CursorPosition);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void CompletionNavigation_FullCycle()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("pr", cursorPosition: 2));
        var completions = new List<CompletionItem>
        {
            new("print", startPosition: -2),
            new("private", startPosition: -2),
            new("process", startPosition: -2)
        };
        buffer.SetCompletions(completions);

        // Act & Assert
        buffer.CompleteNext(); // 0 - print
        Assert.Equal("print", buffer.Text);

        buffer.CompleteNext(); // 1 - private
        Assert.Equal("private", buffer.Text);

        buffer.CompletePrevious(); // 0 - print
        Assert.Equal("print", buffer.Text);

        buffer.GoToCompletion(2); // 2 - process
        Assert.Equal("process", buffer.Text);

        buffer.CancelCompletion();
        Assert.Equal("pr", buffer.Text);
        Assert.Null(buffer.CompleteState);
    }

    [Fact]
    public void CompletionWithPartialText_WorksCorrectly()
    {
        // Arrange - user types "sys" and completions are based on that
        var buffer = new Buffer(document: new Document("import sys", cursorPosition: 10));
        // Completions replace "sys" (3 chars before cursor, so startPosition: -3)
        var completions = new List<CompletionItem>
        {
            new("system", startPosition: -3),
            new("syslog", startPosition: -3)
        };
        buffer.SetCompletions(completions);

        // Act
        buffer.CompleteNext();

        // Assert
        Assert.Equal("import system", buffer.Text);
    }

    #endregion

    #region Thread Safety Tests

    [Fact]
    public async Task ConcurrentCompletionOperations_ThreadSafe()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("test", cursorPosition: 4));
        var completions = new List<CompletionItem>
        {
            new("test1", startPosition: -4),
            new("test2", startPosition: -4),
            new("test3", startPosition: -4),
            new("test4", startPosition: -4),
            new("test5", startPosition: -4)
        };
        var iterations = 30;
        var barrier = new Barrier(4);

        // Act - concurrent completion operations
        var setTask = Task.Run(() =>
        {
            barrier.SignalAndWait();
            for (var i = 0; i < iterations; i++)
            {
                buffer.SetCompletions(completions);
            }
        });

        var nextTask = Task.Run(() =>
        {
            barrier.SignalAndWait();
            for (var i = 0; i < iterations; i++)
            {
                buffer.CompleteNext();
            }
        });

        var prevTask = Task.Run(() =>
        {
            barrier.SignalAndWait();
            for (var i = 0; i < iterations; i++)
            {
                buffer.CompletePrevious();
            }
        });

        var cancelTask = Task.Run(() =>
        {
            barrier.SignalAndWait();
            for (var i = 0; i < iterations; i++)
            {
                buffer.CancelCompletion();
            }
        });

        // Assert - no exceptions
        await Task.WhenAll(setTask, nextTask, prevTask, cancelTask);
    }

    [Fact]
    public async Task CompletionState_ConcurrentAccess_ThreadSafe()
    {
        // Arrange
        var completions = new List<CompletionItem>
        {
            new("item1", startPosition: -2),
            new("item2", startPosition: -2),
            new("item3", startPosition: -2),
            new("item4", startPosition: -2),
            new("item5", startPosition: -2)
        };
        var state = new CompletionState(
            originalDocument: new Document("ab", cursorPosition: 2),
            completions: completions);

        var iterations = 100;
        var barrier = new Barrier(4);

        // Act - concurrent CompletionState operations
        var goToIndexTask = Task.Run(() =>
        {
            barrier.SignalAndWait();
            for (var i = 0; i < iterations; i++)
            {
                state.GoToIndex(i % completions.Count);
            }
        });

        var readIndexTask = Task.Run(() =>
        {
            barrier.SignalAndWait();
            for (var i = 0; i < iterations; i++)
            {
                _ = state.CompleteIndex;
            }
        });

        var currentCompletionTask = Task.Run(() =>
        {
            barrier.SignalAndWait();
            for (var i = 0; i < iterations; i++)
            {
                _ = state.CurrentCompletion;
            }
        });

        var newTextTask = Task.Run(() =>
        {
            barrier.SignalAndWait();
            for (var i = 0; i < iterations; i++)
            {
                _ = state.NewTextAndPosition();
            }
        });

        // Assert - no exceptions
        await Task.WhenAll(goToIndexTask, readIndexTask, currentCompletionTask, newTextTask);
    }

    #endregion
}
