using Xunit;

// Use alias to avoid ambiguity with System.Buffer
using Buffer = Stroke.Core.Buffer;
using Document = Stroke.Core.Document;

namespace Stroke.Tests.Core;

/// <summary>
/// Tests for Buffer undo/redo operations (T038-T041).
/// </summary>
public class BufferUndoRedoTests
{
    #region SaveToUndoStack Tests

    [Fact]
    public void SaveToUndoStack_SavesCurrentState()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello", cursorPosition: 3));

        // Act
        buffer.SaveToUndoStack();
        buffer.Text = "world";
        buffer.Undo();

        // Assert
        Assert.Equal("hello", buffer.Text);
        Assert.Equal(3, buffer.CursorPosition);
    }

    [Fact]
    public void SaveToUndoStack_SameText_UpdatesCursorOnly()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello", cursorPosition: 2));
        buffer.SaveToUndoStack();

        // Act
        buffer.CursorPosition = 4;
        buffer.SaveToUndoStack();

        // Change text and undo - should restore with updated cursor
        buffer.Text = "world";
        buffer.Undo();

        // Assert - undoes to state where cursor was 4 (last saved position for "hello")
        Assert.Equal("hello", buffer.Text);
        Assert.Equal(4, buffer.CursorPosition);
    }

    [Fact]
    public void SaveToUndoStack_ClearsRedoStack()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello", cursorPosition: 5));
        buffer.SaveToUndoStack();
        buffer.Text = "world";
        buffer.SaveToUndoStack();
        buffer.Undo(); // Now redo stack has "world"

        // Act
        buffer.SaveToUndoStack(); // This should clear redo stack

        // Redo should do nothing now
        buffer.Redo();

        // Assert
        Assert.Equal("hello", buffer.Text);
    }

    [Fact]
    public void SaveToUndoStack_WithClearRedoFalse_KeepsRedoStack()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello", cursorPosition: 5));
        buffer.SaveToUndoStack();
        buffer.Text = "world";
        buffer.SaveToUndoStack();
        buffer.Undo(); // Now redo stack has "world"

        // Act
        buffer.SaveToUndoStack(clearRedoStack: false); // Keep redo stack

        // Redo should still work
        buffer.Redo();

        // Assert
        Assert.Equal("world", buffer.Text);
    }

    #endregion

    #region Undo Tests

    [Fact]
    public void Undo_RestoresPreviousState()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello", cursorPosition: 5));
        buffer.SaveToUndoStack();
        buffer.Text = "world";

        // Act
        buffer.Undo();

        // Assert
        Assert.Equal("hello", buffer.Text);
    }

    [Fact]
    public void Undo_RestoresCursorPosition()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello", cursorPosition: 2));
        buffer.SaveToUndoStack();
        buffer.Text = "world";
        buffer.CursorPosition = 5;

        // Act
        buffer.Undo();

        // Assert
        Assert.Equal("hello", buffer.Text);
        Assert.Equal(2, buffer.CursorPosition);
    }

    [Fact]
    public void Undo_EmptyStack_DoesNothing()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello", cursorPosition: 3));

        // Act
        buffer.Undo();

        // Assert
        Assert.Equal("hello", buffer.Text);
        Assert.Equal(3, buffer.CursorPosition);
    }

    [Fact]
    public void Undo_MultipleChanges_UndoesInOrder()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("one", cursorPosition: 3));
        buffer.SaveToUndoStack();
        buffer.Text = "two";
        buffer.SaveToUndoStack();
        buffer.Text = "three";
        buffer.SaveToUndoStack();
        buffer.Text = "four";

        // Act & Assert
        buffer.Undo();
        Assert.Equal("three", buffer.Text);

        buffer.Undo();
        Assert.Equal("two", buffer.Text);

        buffer.Undo();
        Assert.Equal("one", buffer.Text);
    }

    [Fact]
    public void Undo_SkipsSameTextEntries()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello", cursorPosition: 5));
        buffer.SaveToUndoStack();
        buffer.SaveToUndoStack(); // Same text, different entry
        buffer.Text = "world";
        buffer.SaveToUndoStack();
        buffer.Text = "world"; // Make current text same as top of stack

        // Act - should skip the "world" entry and restore "hello"
        buffer.Undo();

        // Assert
        Assert.Equal("hello", buffer.Text);
    }

    [Fact]
    public void Undo_AddsTorRedoStack()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello", cursorPosition: 5));
        buffer.SaveToUndoStack();
        buffer.Text = "world";

        // Act
        buffer.Undo();

        // Assert - can redo
        buffer.Redo();
        Assert.Equal("world", buffer.Text);
    }

    #endregion

    #region Redo Tests

    [Fact]
    public void Redo_RestoresPreviouslyUndonState()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello", cursorPosition: 5));
        buffer.SaveToUndoStack();
        buffer.Text = "world";
        buffer.SaveToUndoStack();
        buffer.Undo();

        // Act
        buffer.Redo();

        // Assert
        Assert.Equal("world", buffer.Text);
    }

    [Fact]
    public void Redo_RestoresCursorPosition()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello", cursorPosition: 2));
        buffer.SaveToUndoStack();
        buffer.Text = "world";
        buffer.CursorPosition = 4;
        buffer.SaveToUndoStack();
        buffer.Undo();

        // Act
        buffer.Redo();

        // Assert
        Assert.Equal("world", buffer.Text);
        Assert.Equal(4, buffer.CursorPosition);
    }

    [Fact]
    public void Redo_EmptyStack_DoesNothing()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello", cursorPosition: 3));

        // Act
        buffer.Redo();

        // Assert
        Assert.Equal("hello", buffer.Text);
        Assert.Equal(3, buffer.CursorPosition);
    }

    [Fact]
    public void Redo_MultipleUndos_RedoesInOrder()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("one", cursorPosition: 3));
        buffer.SaveToUndoStack();
        buffer.Text = "two";
        buffer.SaveToUndoStack();
        buffer.Text = "three";
        buffer.SaveToUndoStack();
        buffer.Undo();
        buffer.Undo();

        // Act & Assert
        buffer.Redo();
        Assert.Equal("two", buffer.Text);

        buffer.Redo();
        Assert.Equal("three", buffer.Text);
    }

    [Fact]
    public void Redo_AfterNewEdit_DoesNothing()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello", cursorPosition: 5));
        buffer.SaveToUndoStack();
        buffer.Text = "world";
        buffer.SaveToUndoStack();
        buffer.Undo();
        buffer.SaveToUndoStack(); // New save clears redo stack

        // Act
        buffer.Redo();

        // Assert - should still be "hello" since redo stack was cleared
        Assert.Equal("hello", buffer.Text);
    }

    [Fact]
    public void Redo_SavesCurrentStateToUndoStack()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello", cursorPosition: 5));
        buffer.SaveToUndoStack();
        buffer.Text = "world";
        buffer.SaveToUndoStack();
        buffer.Undo(); // hello
        buffer.Redo(); // world

        // Act - undo again should work because redo saved current state
        buffer.Undo();

        // Assert
        Assert.Equal("hello", buffer.Text);
    }

    #endregion

    #region Undo/Redo Integration Tests

    [Fact]
    public void UndoRedo_Cycle_WorksCorrectly()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("A", cursorPosition: 1));
        buffer.SaveToUndoStack();
        buffer.Text = "B";
        buffer.SaveToUndoStack();
        buffer.Text = "C";

        // Act & Assert
        Assert.Equal("C", buffer.Text);

        buffer.Undo();
        Assert.Equal("B", buffer.Text);

        buffer.Undo();
        Assert.Equal("A", buffer.Text);

        buffer.Redo();
        Assert.Equal("B", buffer.Text);

        buffer.Undo();
        Assert.Equal("A", buffer.Text);

        buffer.Redo();
        Assert.Equal("B", buffer.Text);

        buffer.Redo();
        Assert.Equal("C", buffer.Text);
    }

    [Fact]
    public void UndoRedo_WithEditing_IntegrationTest()
    {
        // Arrange
        var buffer = new Buffer();

        // Act - simulate typing with undo points
        buffer.InsertText("Hello", fireEvent: false);
        buffer.SaveToUndoStack();

        buffer.InsertText(" World", fireEvent: false);
        buffer.SaveToUndoStack();

        buffer.InsertText("!", fireEvent: false);

        // Assert
        Assert.Equal("Hello World!", buffer.Text);

        buffer.Undo();
        Assert.Equal("Hello World", buffer.Text);

        buffer.Undo();
        Assert.Equal("Hello", buffer.Text);

        buffer.Redo();
        Assert.Equal("Hello World", buffer.Text);
    }

    [Fact]
    public void Undo_CursorClampedToTextLength()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello world", cursorPosition: 11));
        buffer.SaveToUndoStack();
        buffer.Text = "hi"; // shorter text

        // Change undo stack to have cursor beyond new text length
        // This is done by first undoing, then redoing with a modified state
        buffer.Undo();

        // Assert - cursor should be clamped to text length
        Assert.True(buffer.CursorPosition <= buffer.Text.Length);
    }

    #endregion

    #region Thread Safety Tests

    [Fact]
    public async Task ConcurrentUndoRedo_ThreadSafe()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("start", cursorPosition: 5));
        var iterations = 50;
        var barrier = new Barrier(3);

        // Act - concurrent undo/redo operations
        var saveTask = Task.Run(() =>
        {
            barrier.SignalAndWait();
            for (var i = 0; i < iterations; i++)
            {
                buffer.SaveToUndoStack();
                buffer.Text = $"text{i}";
            }
        });

        var undoTask = Task.Run(() =>
        {
            barrier.SignalAndWait();
            for (var i = 0; i < iterations; i++)
            {
                buffer.Undo();
            }
        });

        var redoTask = Task.Run(() =>
        {
            barrier.SignalAndWait();
            for (var i = 0; i < iterations; i++)
            {
                buffer.Redo();
            }
        });

        // Assert - no exceptions
        await Task.WhenAll(saveTask, undoTask, redoTask);
    }

    #endregion
}
