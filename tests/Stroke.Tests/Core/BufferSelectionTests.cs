using Stroke.Clipboard;
using Stroke.Core;
using Xunit;

// Use alias to avoid ambiguity with System.Buffer
using Buffer = Stroke.Core.Buffer;
using Document = Stroke.Core.Document;
using SelectionState = Stroke.Core.SelectionState;
using SelectionType = Stroke.Core.SelectionType;

namespace Stroke.Tests.Core;

/// <summary>
/// Tests for Buffer selection operations (T064-T081).
/// </summary>
public class BufferSelectionTests
{
    #region StartSelection Tests

    [Fact]
    public void StartSelection_SetsSelectionState()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello world", cursorPosition: 5));

        // Act
        buffer.StartSelection();

        // Assert
        Assert.NotNull(buffer.SelectionState);
        Assert.Equal(5, buffer.SelectionState.OriginalCursorPosition);
        Assert.Equal(SelectionType.Characters, buffer.SelectionState.Type);
    }

    [Fact]
    public void StartSelection_WithLineType_SetsLineSelection()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello\nworld", cursorPosition: 3));

        // Act
        buffer.StartSelection(SelectionType.Lines);

        // Assert
        Assert.NotNull(buffer.SelectionState);
        Assert.Equal(SelectionType.Lines, buffer.SelectionState.Type);
    }

    [Fact]
    public void StartSelection_WithBlockType_SetsBlockSelection()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello\nworld", cursorPosition: 3));

        // Act
        buffer.StartSelection(SelectionType.Block);

        // Assert
        Assert.NotNull(buffer.SelectionState);
        Assert.Equal(SelectionType.Block, buffer.SelectionState.Type);
    }

    [Fact]
    public void StartSelection_ReplacesPreviousSelection()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello world", cursorPosition: 2));
        buffer.StartSelection(SelectionType.Lines);

        // Act
        buffer.CursorPosition = 8;
        buffer.StartSelection(SelectionType.Characters);

        // Assert
        Assert.NotNull(buffer.SelectionState);
        Assert.Equal(8, buffer.SelectionState.OriginalCursorPosition);
        Assert.Equal(SelectionType.Characters, buffer.SelectionState.Type);
    }

    #endregion

    #region ExitSelection Tests

    [Fact]
    public void ExitSelection_ClearsSelectionState()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello world", cursorPosition: 5));
        buffer.StartSelection();
        Assert.NotNull(buffer.SelectionState);

        // Act
        buffer.ExitSelection();

        // Assert
        Assert.Null(buffer.SelectionState);
    }

    [Fact]
    public void ExitSelection_WhenNoSelection_DoesNothing()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello world"));

        // Act
        buffer.ExitSelection();

        // Assert
        Assert.Null(buffer.SelectionState);
    }

    #endregion

    #region CopySelection Tests

    [Fact]
    public void CopySelection_ReturnsClipboardData()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello world", cursorPosition: 0));
        buffer.StartSelection();
        buffer.CursorPosition = 5; // Select "hello"

        // Act
        var clipboardData = buffer.CopySelection();

        // Assert
        Assert.NotNull(clipboardData);
        Assert.Equal("hello", clipboardData.Text);
    }

    [Fact]
    public void CopySelection_DoesNotModifyText()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello world", cursorPosition: 0));
        buffer.StartSelection();
        buffer.CursorPosition = 5;

        // Act
        buffer.CopySelection();

        // Assert
        Assert.Equal("hello world", buffer.Text);
    }

    [Fact]
    public void CopySelection_ClearsSelectionState()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello world", cursorPosition: 0));
        buffer.StartSelection();
        buffer.CursorPosition = 5;

        // Act
        buffer.CopySelection();

        // Assert
        Assert.Null(buffer.SelectionState);
    }

    [Fact]
    public void CopySelection_NoSelection_ReturnsEmptyClipboard()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello world", cursorPosition: 5));
        // No selection started

        // Act
        var clipboardData = buffer.CopySelection();

        // Assert
        Assert.Equal("", clipboardData.Text);
    }

    [Fact]
    public void CopySelection_BackwardSelection_Works()
    {
        // Arrange - select backwards
        var buffer = new Buffer(document: new Document("hello world", cursorPosition: 5));
        buffer.StartSelection(); // Start at position 5
        buffer.CursorPosition = 0; // Move cursor back to 0

        // Act
        var clipboardData = buffer.CopySelection();

        // Assert
        Assert.Equal("hello", clipboardData.Text);
    }

    #endregion

    #region CutSelection Tests

    [Fact]
    public void CutSelection_ReturnsClipboardData()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello world", cursorPosition: 0));
        buffer.StartSelection();
        buffer.CursorPosition = 5;

        // Act
        var clipboardData = buffer.CutSelection();

        // Assert
        Assert.Equal("hello", clipboardData.Text);
    }

    [Fact]
    public void CutSelection_RemovesSelectedText()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello world", cursorPosition: 0));
        buffer.StartSelection();
        buffer.CursorPosition = 5;

        // Act
        buffer.CutSelection();

        // Assert
        Assert.Equal(" world", buffer.Text);
    }

    [Fact]
    public void CutSelection_ClearsSelectionState()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello world", cursorPosition: 0));
        buffer.StartSelection();
        buffer.CursorPosition = 5;

        // Act
        buffer.CutSelection();

        // Assert
        Assert.Null(buffer.SelectionState);
    }

    [Fact]
    public void CutSelection_UpdatesCursorPosition()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello world", cursorPosition: 0));
        buffer.StartSelection();
        buffer.CursorPosition = 5;

        // Act
        buffer.CutSelection();

        // Assert - cursor should be at start of selection
        Assert.Equal(0, buffer.CursorPosition);
    }

    [Fact]
    public void CutSelection_MiddleOfText()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello beautiful world", cursorPosition: 6));
        buffer.StartSelection();
        buffer.CursorPosition = 16; // Select "beautiful "

        // Act
        buffer.CutSelection();

        // Assert
        Assert.Equal("hello world", buffer.Text);
        Assert.Equal(6, buffer.CursorPosition);
    }

    #endregion

    #region PasteClipboardData Tests

    [Fact]
    public void PasteClipboardData_EmacsMode_InsertsAtCursor()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello world", cursorPosition: 5));
        var clipboardData = new ClipboardData(" beautiful");

        // Act
        buffer.PasteClipboardData(clipboardData, PasteMode.Emacs);

        // Assert
        Assert.Equal("hello beautiful world", buffer.Text);
    }

    [Fact]
    public void PasteClipboardData_ViBefore_InsertsBeforeCursor()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello world", cursorPosition: 5));
        var clipboardData = new ClipboardData("X");

        // Act
        buffer.PasteClipboardData(clipboardData, PasteMode.ViBefore);

        // Assert
        Assert.Equal("helloX world", buffer.Text);
    }

    [Fact]
    public void PasteClipboardData_ViAfter_InsertsAfterCursor()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello world", cursorPosition: 5));
        var clipboardData = new ClipboardData("X");

        // Act
        buffer.PasteClipboardData(clipboardData, PasteMode.ViAfter);

        // Assert
        Assert.Equal("hello Xworld", buffer.Text);
    }

    [Fact]
    public void PasteClipboardData_WithCount_PastesMultipleTimes()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello", cursorPosition: 5));
        var clipboardData = new ClipboardData("!");

        // Act
        buffer.PasteClipboardData(clipboardData, PasteMode.Emacs, count: 3);

        // Assert
        Assert.Equal("hello!!!", buffer.Text);
    }

    [Fact]
    public void PasteClipboardData_RemembersDocumentBeforePaste()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello", cursorPosition: 5));
        var clipboardData = new ClipboardData(" world");

        // Act
        buffer.PasteClipboardData(clipboardData, PasteMode.Emacs);

        // Assert
        Assert.NotNull(buffer.DocumentBeforePaste);
        Assert.Equal("hello", buffer.DocumentBeforePaste.Text);
        Assert.Equal(5, buffer.DocumentBeforePaste.CursorPosition);
    }

    [Fact]
    public void PasteClipboardData_ThrowsOnNull()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello"));

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => buffer.PasteClipboardData(null!));
    }

    #endregion

    #region Line Selection Tests

    [Fact]
    public void CutSelection_LineSelection_CutsEntireLine()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("line1\nline2\nline3", cursorPosition: 8));
        // Cursor is on "line2"
        buffer.StartSelection(SelectionType.Lines);

        // Act
        var clipboardData = buffer.CutSelection();

        // Assert
        Assert.Contains("line2", clipboardData.Text);
    }

    [Fact]
    public void CopySelection_LineSelection_CopiesEntireLine()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("line1\nline2\nline3", cursorPosition: 8));
        buffer.StartSelection(SelectionType.Lines);

        // Act
        var clipboardData = buffer.CopySelection();

        // Assert
        Assert.Contains("line2", clipboardData.Text);
        Assert.Equal("line1\nline2\nline3", buffer.Text); // Text unchanged
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void CopyPaste_RoundTrip()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello world", cursorPosition: 0));
        buffer.StartSelection();
        buffer.CursorPosition = 5;

        // Act - copy then paste
        var clipboardData = buffer.CopySelection();
        buffer.CursorPosition = 11; // End of text
        buffer.PasteClipboardData(clipboardData);

        // Assert
        Assert.Equal("hello worldhello", buffer.Text);
    }

    [Fact]
    public void CutPaste_RoundTrip()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello world", cursorPosition: 6));
        buffer.StartSelection();
        buffer.CursorPosition = 11; // Select "world"

        // Act - cut then paste elsewhere
        var clipboardData = buffer.CutSelection();
        Assert.Equal("hello ", buffer.Text);

        buffer.CursorPosition = 0;
        buffer.PasteClipboardData(clipboardData);

        // Assert
        Assert.Equal("worldhello ", buffer.Text);
    }

    [Fact]
    public void SelectionThenEdit_ClearsSelection()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello", cursorPosition: 0));
        buffer.StartSelection();
        buffer.CursorPosition = 3;
        Assert.NotNull(buffer.SelectionState);

        // Act - edit clears selection
        buffer.InsertText("X", fireEvent: false);

        // Assert
        Assert.Null(buffer.SelectionState);
    }

    #endregion

    #region Thread Safety Tests

    [Fact]
    public async Task ConcurrentSelectionOperations_ThreadSafe()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello world test", cursorPosition: 5));
        var iterations = 50;
        var barrier = new Barrier(3);

        // Act - concurrent selection operations
        var ct = TestContext.Current.CancellationToken;
        var startTask = Task.Run(() =>
        {
            barrier.SignalAndWait();
            for (var i = 0; i < iterations; i++)
            {
                buffer.StartSelection();
            }
        }, ct);

        var exitTask = Task.Run(() =>
        {
            barrier.SignalAndWait();
            for (var i = 0; i < iterations; i++)
            {
                buffer.ExitSelection();
            }
        }, ct);

        var copyTask = Task.Run(() =>
        {
            barrier.SignalAndWait();
            for (var i = 0; i < iterations; i++)
            {
                buffer.StartSelection();
                buffer.CursorPosition = (i % 10);
                buffer.CopySelection();
            }
        }, ct);

        // Assert - no exceptions
        await Task.WhenAll(startTask, exitTask, copyTask);
    }

    #endregion

    #region MultipleCursorPositions Tests

    [Fact]
    public void MultipleCursorPositions_DefaultsToEmpty()
    {
        // Arrange & Act
        var buffer = new Buffer();

        // Assert
        Assert.Empty(buffer.MultipleCursorPositions);
    }

    [Fact]
    public void MultipleCursorPositions_CanBeSet()
    {
        // Arrange
        var buffer = new Buffer();
        var positions = new List<int> { 0, 5, 10 };

        // Act
        buffer.MultipleCursorPositions = positions;

        // Assert
        Assert.Equal(3, buffer.MultipleCursorPositions.Count);
        Assert.Equal(0, buffer.MultipleCursorPositions[0]);
        Assert.Equal(5, buffer.MultipleCursorPositions[1]);
        Assert.Equal(10, buffer.MultipleCursorPositions[2]);
    }

    [Fact]
    public void MultipleCursorPositions_ReturnsDefensiveCopy()
    {
        // Arrange
        var buffer = new Buffer();
        buffer.MultipleCursorPositions = [1, 2, 3];

        // Act - get the list twice
        var first = buffer.MultipleCursorPositions;
        var second = buffer.MultipleCursorPositions;

        // Assert - different instances (defensive copy)
        Assert.NotSame(first, second);
        Assert.Equal(first, second);
    }

    [Fact]
    public void MultipleCursorPositions_ClearedOnReset()
    {
        // Arrange
        var buffer = new Buffer();
        buffer.MultipleCursorPositions = [1, 2, 3];
        Assert.Equal(3, buffer.MultipleCursorPositions.Count);

        // Act
        buffer.Reset();

        // Assert
        Assert.Empty(buffer.MultipleCursorPositions);
    }

    #endregion
}
