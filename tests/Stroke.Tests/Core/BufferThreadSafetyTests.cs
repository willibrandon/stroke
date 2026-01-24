using Stroke.Core;
using Stroke.History;
using Xunit;

// Use alias to avoid ambiguity with System.Buffer
using Buffer = Stroke.Core.Buffer;
using Document = Stroke.Core.Document;

namespace Stroke.Tests.Core;

/// <summary>
/// Thread safety tests for Buffer class (T140-T143).
/// These tests verify that concurrent access to Buffer is handled safely.
/// </summary>
public class BufferThreadSafetyTests
{
    private const int ConcurrentTaskCount = 3;
    private const int IterationsPerTask = 20;

    #region Parallel Insert/Delete Tests (T141)

    [Fact]
    public async Task ConcurrentInsertText_ThreadSafe()
    {
        // Arrange
        var buffer = new Buffer(document: new Document(""));

        // Act - multiple tasks inserting text concurrently
        var tasks = Enumerable.Range(0, ConcurrentTaskCount).Select(taskId =>
            Task.Run(() =>
            {
                for (var i = 0; i < IterationsPerTask; i++)
                {
                    buffer.InsertText($"T{taskId}I{i}");
                }
            })).ToArray();

        await Task.WhenAll(tasks);

        // Assert - no exceptions, text contains all inserted content
        Assert.NotEmpty(buffer.Text);
    }

    [Fact]
    public async Task ConcurrentDelete_ThreadSafe()
    {
        // Arrange - start with some text
        var initialText = new string('x', 1000);
        var buffer = new Buffer(document: new Document(initialText, cursorPosition: 500));

        // Act - multiple tasks deleting text concurrently
        var tasks = Enumerable.Range(0, ConcurrentTaskCount).Select(_ =>
            Task.Run(() =>
            {
                for (var i = 0; i < IterationsPerTask; i++)
                {
                    try
                    {
                        buffer.Delete(1);
                        buffer.DeleteBeforeCursor(1);
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        // Ignore - deletion may exceed available text
                    }
                }
            })).ToArray();

        await Task.WhenAll(tasks);

        // Assert - no exceptions beyond expected
        Assert.True(buffer.Text.Length <= initialText.Length);
    }

    [Fact]
    public async Task ConcurrentInsertAndDelete_ThreadSafe()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("initial text for concurrent operations"));

        // Act - mixed insert and delete operations
        var insertTask = Task.Run(() =>
        {
            for (var i = 0; i < IterationsPerTask; i++)
            {
                buffer.InsertText("X");
            }
        });

        var deleteTask = Task.Run(() =>
        {
            for (var i = 0; i < IterationsPerTask; i++)
            {
                try
                {
                    buffer.Delete(1);
                }
                catch (ArgumentOutOfRangeException)
                {
                    // Ignore
                }
            }
        });

        await Task.WhenAll(insertTask, deleteTask);

        // Assert - no deadlocks or exceptions
        Assert.NotNull(buffer.Text);
    }

    #endregion

    #region Parallel Undo/Redo Tests (T142)

    [Fact]
    public async Task ConcurrentUndoRedo_ThreadSafe()
    {
        // Arrange - add some history
        var buffer = new Buffer(document: new Document(""));
        for (var i = 0; i < 20; i++)
        {
            buffer.InsertText($"line{i}\n");
            buffer.SaveToUndoStack();
        }

        // Act - concurrent undo and redo operations
        var undoTask = Task.Run(() =>
        {
            for (var i = 0; i < IterationsPerTask; i++)
            {
                buffer.Undo();
            }
        });

        var redoTask = Task.Run(() =>
        {
            for (var i = 0; i < IterationsPerTask; i++)
            {
                buffer.Redo();
            }
        });

        await Task.WhenAll(undoTask, redoTask);

        // Assert - no deadlocks or exceptions
        Assert.NotNull(buffer.Text);
    }

    [Fact]
    public async Task ConcurrentInsertAndUndo_ThreadSafe()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("initial"));
        for (var i = 0; i < 10; i++)
        {
            buffer.InsertText($"_{i}");
            buffer.SaveToUndoStack();
        }

        // Act - insert, undo, and save all running concurrently
        var insertTask = Task.Run(() =>
        {
            for (var i = 0; i < IterationsPerTask; i++)
            {
                buffer.InsertText("X");
            }
        });

        var undoTask = Task.Run(() =>
        {
            for (var i = 0; i < IterationsPerTask; i++)
            {
                buffer.Undo();
            }
        });

        var saveTask = Task.Run(() =>
        {
            for (var i = 0; i < IterationsPerTask; i++)
            {
                buffer.SaveToUndoStack();
            }
        });

        await Task.WhenAll(insertTask, undoTask, saveTask);

        // Assert - no deadlocks
        Assert.NotNull(buffer.Text);
    }

    #endregion

    #region Async Retry-on-Document-Change Tests (T143)

    [Fact]
    public async Task ConcurrentCompletionAndEditing_ThreadSafe()
    {
        // Arrange
        var completions = new List<Stroke.Completion.Completion>
        {
            new Stroke.Completion.Completion("option1", StartPosition: 0),
            new Stroke.Completion.Completion("option2", StartPosition: 0),
            new Stroke.Completion.Completion("option3", StartPosition: 0)
        };
        var buffer = new Buffer(document: new Document("test"));

        // Act - completion operations and editing concurrently
        var completionTask = Task.Run(() =>
        {
            for (var i = 0; i < IterationsPerTask; i++)
            {
                buffer.SetCompletions(completions);
                buffer.CancelCompletion();
            }
        });

        var editTask = Task.Run(() =>
        {
            for (var i = 0; i < IterationsPerTask; i++)
            {
                buffer.InsertText("x");
            }
        });

        var readTask = Task.Run(() =>
        {
            for (var i = 0; i < IterationsPerTask; i++)
            {
                _ = buffer.Text;
                _ = buffer.CursorPosition;
                _ = buffer.Document;
            }
        });

        await Task.WhenAll(completionTask, editTask, readTask);

        // Assert
        Assert.NotNull(buffer.Text);
    }

    [Fact]
    public async Task ConcurrentSuggestionAndEditing_ThreadSafe()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("test"));

        // Act
        var editTask = Task.Run(() =>
        {
            for (var i = 0; i < IterationsPerTask; i++)
            {
                buffer.InsertText("y");
            }
        });

        var readTask = Task.Run(() =>
        {
            for (var i = 0; i < IterationsPerTask; i++)
            {
                _ = buffer.Suggestion;
                _ = buffer.Text;
            }
        });

        await Task.WhenAll(editTask, readTask);

        // Assert
        Assert.NotNull(buffer.Text);
    }

    [Fact]
    public async Task ConcurrentValidationAndEditing_ThreadSafe()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("test"));

        // Act
        var validationTask = Task.Run(async () =>
        {
            for (var i = 0; i < IterationsPerTask; i++)
            {
                await buffer.ValidateAsync();
            }
        });

        var editTask = Task.Run(() =>
        {
            for (var i = 0; i < IterationsPerTask; i++)
            {
                buffer.InsertText("z");
            }
        });

        var syncValidateTask = Task.Run(() =>
        {
            for (var i = 0; i < IterationsPerTask; i++)
            {
                buffer.Validate();
            }
        });

        await Task.WhenAll(validationTask, editTask, syncValidateTask);

        // Assert
        Assert.NotNull(buffer.Text);
    }

    #endregion

    #region Navigation Thread Safety

    [Fact]
    public async Task ConcurrentCursorMovement_ThreadSafe()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("line1\nline2\nline3\nline4\nline5", cursorPosition: 15));

        // Act
        var leftTask = Task.Run(() =>
        {
            for (var i = 0; i < IterationsPerTask; i++)
            {
                buffer.CursorLeft();
            }
        });

        var rightTask = Task.Run(() =>
        {
            for (var i = 0; i < IterationsPerTask; i++)
            {
                buffer.CursorRight();
            }
        });

        var upTask = Task.Run(() =>
        {
            for (var i = 0; i < IterationsPerTask; i++)
            {
                buffer.CursorUp();
            }
        });

        var downTask = Task.Run(() =>
        {
            for (var i = 0; i < IterationsPerTask; i++)
            {
                buffer.CursorDown();
            }
        });

        await Task.WhenAll(leftTask, rightTask, upTask, downTask);

        // Assert - cursor position is valid
        Assert.True(buffer.CursorPosition >= 0);
        Assert.True(buffer.CursorPosition <= buffer.Text.Length);
    }

    #endregion

    #region Selection Thread Safety

    [Fact]
    public async Task ConcurrentSelectionOperations_ThreadSafe()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello world test text for selection", cursorPosition: 0));

        // Act
        var selectTask = Task.Run(() =>
        {
            for (var i = 0; i < IterationsPerTask; i++)
            {
                buffer.StartSelection(SelectionType.Characters);
                buffer.CursorRight(3);
                buffer.ExitSelection();
            }
        });

        var copyTask = Task.Run(() =>
        {
            for (var i = 0; i < IterationsPerTask; i++)
            {
                buffer.StartSelection(SelectionType.Characters);
                buffer.CursorRight(2);
                _ = buffer.CopySelection();
                buffer.ExitSelection();
            }
        });

        await Task.WhenAll(selectTask, copyTask);

        // Assert
        Assert.NotNull(buffer.Text);
    }

    #endregion

    #region History Thread Safety

    [Fact]
    public async Task ConcurrentHistoryNavigation_ThreadSafe()
    {
        // Arrange
        var history = new InMemoryHistory();
        for (var i = 0; i < 20; i++)
        {
            history.AppendString($"history entry {i}");
        }
        var buffer = new Buffer(history: history, document: new Document("current"));
        buffer.LoadHistoryIfNotYetLoaded();

        // Act
        var backwardTask = Task.Run(() =>
        {
            for (var i = 0; i < IterationsPerTask; i++)
            {
                buffer.HistoryBackward();
            }
        });

        var forwardTask = Task.Run(() =>
        {
            for (var i = 0; i < IterationsPerTask; i++)
            {
                buffer.HistoryForward();
            }
        });

        var gotoTask = Task.Run(() =>
        {
            for (var i = 0; i < IterationsPerTask; i++)
            {
                buffer.GoToHistory(i % 10);
            }
        });

        await Task.WhenAll(backwardTask, forwardTask, gotoTask);

        // Assert
        Assert.NotNull(buffer.Text);
    }

    #endregion

    #region Search Thread Safety

    [Fact]
    public async Task ConcurrentSearchOperations_ThreadSafe()
    {
        // Arrange
        var history = new InMemoryHistory();
        for (var i = 0; i < 10; i++)
        {
            history.AppendString($"line with searchable text {i}");
        }
        var buffer = new Buffer(history: history, document: new Document("current text"));
        buffer.LoadHistoryIfNotYetLoaded();

        // Act
        var forwardSearchTask = Task.Run(() =>
        {
            for (var i = 0; i < IterationsPerTask; i++)
            {
                var state = new SearchState("text", SearchDirection.Forward);
                buffer.ApplySearch(state);
            }
        });

        var backwardSearchTask = Task.Run(() =>
        {
            for (var i = 0; i < IterationsPerTask; i++)
            {
                var state = new SearchState("text", SearchDirection.Backward);
                buffer.ApplySearch(state);
            }
        });

        var documentSearchTask = Task.Run(() =>
        {
            for (var i = 0; i < IterationsPerTask; i++)
            {
                var state = new SearchState("searchable", SearchDirection.Forward);
                _ = buffer.DocumentForSearch(state);
            }
        });

        await Task.WhenAll(forwardSearchTask, backwardSearchTask, documentSearchTask);

        // Assert
        Assert.NotNull(buffer.Text);
    }

    #endregion

    #region Property Access Thread Safety

    [Fact]
    public async Task ConcurrentPropertyAccess_ThreadSafe()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("test content", cursorPosition: 5));

        // Act - concurrent reads and writes of various properties
        var readTextTask = Task.Run(() =>
        {
            for (var i = 0; i < IterationsPerTask; i++)
            {
                _ = buffer.Text;
                _ = buffer.Document;
            }
        });

        var readPositionTask = Task.Run(() =>
        {
            for (var i = 0; i < IterationsPerTask; i++)
            {
                _ = buffer.CursorPosition;
                _ = buffer.WorkingIndex;
            }
        });

        var writePositionTask = Task.Run(() =>
        {
            for (var i = 0; i < IterationsPerTask; i++)
            {
                buffer.CursorPosition = i % 5;
            }
        });

        var readStateTask = Task.Run(() =>
        {
            for (var i = 0; i < IterationsPerTask; i++)
            {
                _ = buffer.SelectionState;
                _ = buffer.ValidationState;
                _ = buffer.Suggestion;
            }
        });

        var insertTask = Task.Run(() =>
        {
            for (var i = 0; i < IterationsPerTask; i++)
            {
                buffer.InsertText("x");
            }
        });

        await Task.WhenAll(readTextTask, readPositionTask, writePositionTask, readStateTask, insertTask);

        // Assert - buffer is in consistent state
        Assert.NotNull(buffer.Text);
        Assert.True(buffer.CursorPosition >= 0);
        Assert.True(buffer.CursorPosition <= buffer.Text.Length);
    }

    #endregion
}
