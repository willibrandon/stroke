using Stroke.Core;
using Xunit;

// Use alias to avoid ambiguity with System.Buffer
using Buffer = Stroke.Core.Buffer;
using Document = Stroke.Core.Document;

namespace Stroke.Tests.Core;

/// <summary>
/// Tests for BufferOperations static class (T134-T139).
/// </summary>
public class BufferOperationsTests
{
    #region Indent Tests

    [Fact]
    public void Indent_SingleLine_AddsIndent()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello", cursorPosition: 0));

        // Act
        BufferOperations.Indent(buffer, fromRow: 0, toRow: 1);

        // Assert - 4 spaces added
        Assert.Equal("    hello", buffer.Text);
    }

    [Fact]
    public void Indent_MultipleLines_IndentsAllLines()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("line1\nline2\nline3", cursorPosition: 0));

        // Act - indent all three lines
        BufferOperations.Indent(buffer, fromRow: 0, toRow: 3);

        // Assert
        Assert.Equal("    line1\n    line2\n    line3", buffer.Text);
    }

    [Fact]
    public void Indent_PartialRange_OnlyIndentsSpecifiedLines()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("line1\nline2\nline3", cursorPosition: 0));

        // Act - only indent line2
        BufferOperations.Indent(buffer, fromRow: 1, toRow: 2);

        // Assert
        Assert.Equal("line1\n    line2\nline3", buffer.Text);
    }

    [Fact]
    public void Indent_WithCount_AddsMultipleIndentLevels()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello", cursorPosition: 0));

        // Act - indent by 2 levels (8 spaces)
        BufferOperations.Indent(buffer, fromRow: 0, toRow: 1, count: 2);

        // Assert
        Assert.Equal("        hello", buffer.Text);
    }

    [Fact]
    public void Indent_CountZero_NoChange()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello", cursorPosition: 0));

        // Act
        BufferOperations.Indent(buffer, fromRow: 0, toRow: 1, count: 0);

        // Assert
        Assert.Equal("hello", buffer.Text);
    }

    [Fact]
    public void Indent_EmptyRange_NoChange()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello", cursorPosition: 0));

        // Act - empty range (fromRow == toRow)
        BufferOperations.Indent(buffer, fromRow: 0, toRow: 0);

        // Assert
        Assert.Equal("hello", buffer.Text);
    }

    [Fact]
    public void Indent_PreservesCursorColumn()
    {
        // Arrange - cursor at column 2 of line1
        var buffer = new Buffer(document: new Document("hello\nworld", cursorPosition: 2));

        // Act
        BufferOperations.Indent(buffer, fromRow: 0, toRow: 2);

        // Assert - cursor should be at column 6 (original 2 + 4 spaces indent)
        Assert.Equal(6, buffer.CursorPosition);
    }

    [Fact]
    public void Indent_NullBuffer_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => BufferOperations.Indent(null!, 0, 1));
    }

    #endregion

    #region Unindent Tests

    [Fact]
    public void Unindent_SingleLine_RemovesIndent()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("    hello", cursorPosition: 0));

        // Act
        BufferOperations.Unindent(buffer, fromRow: 0, toRow: 1);

        // Assert
        Assert.Equal("hello", buffer.Text);
    }

    [Fact]
    public void Unindent_MultipleLines_UnindentsAllLines()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("    line1\n    line2\n    line3", cursorPosition: 0));

        // Act
        BufferOperations.Unindent(buffer, fromRow: 0, toRow: 3);

        // Assert
        Assert.Equal("line1\nline2\nline3", buffer.Text);
    }

    [Fact]
    public void Unindent_PartialIndent_StripsAllLeadingWhitespace()
    {
        // Arrange - line has only 2 spaces
        var buffer = new Buffer(document: new Document("  hello", cursorPosition: 0));

        // Act
        BufferOperations.Unindent(buffer, fromRow: 0, toRow: 1);

        // Assert - all leading whitespace removed
        Assert.Equal("hello", buffer.Text);
    }

    [Fact]
    public void Unindent_WithCount_RemovesMultipleIndentLevels()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("        hello", cursorPosition: 0));

        // Act - remove 2 levels (8 spaces)
        BufferOperations.Unindent(buffer, fromRow: 0, toRow: 1, count: 2);

        // Assert
        Assert.Equal("hello", buffer.Text);
    }

    [Fact]
    public void Unindent_NoIndent_NoChange()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello", cursorPosition: 0));

        // Act
        BufferOperations.Unindent(buffer, fromRow: 0, toRow: 1);

        // Assert
        Assert.Equal("hello", buffer.Text);
    }

    [Fact]
    public void Unindent_CountZero_NoChange()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("    hello", cursorPosition: 0));

        // Act
        BufferOperations.Unindent(buffer, fromRow: 0, toRow: 1, count: 0);

        // Assert
        Assert.Equal("    hello", buffer.Text);
    }

    [Fact]
    public void Unindent_NullBuffer_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => BufferOperations.Unindent(null!, 0, 1));
    }

    #endregion

    #region ReshapeText Tests

    [Fact]
    public void ReshapeText_SingleLine_WrapsAtWidth()
    {
        // Arrange - long line
        var buffer = new Buffer(document: new Document("hello world this is a long line that should be wrapped"));

        // Act - reshape with 20 char width
        BufferOperations.ReshapeText(buffer, fromRow: 0, toRow: 0, textWidth: 20);

        // Assert - text should be wrapped
        var lines = buffer.Text.Split('\n');
        Assert.True(lines.Length > 1);
        foreach (var line in lines)
        {
            Assert.True(line.Length <= 20, $"Line '{line}' exceeds width 20");
        }
    }

    [Fact]
    public void ReshapeText_PreservesIndentation()
    {
        // Arrange - indented line
        var buffer = new Buffer(document: new Document("    hello world this is indented text"));

        // Act - reshape with 30 char width
        BufferOperations.ReshapeText(buffer, fromRow: 0, toRow: 0, textWidth: 30);

        // Assert - all lines should start with indentation
        var lines = buffer.Text.Split('\n');
        foreach (var line in lines)
        {
            Assert.StartsWith("    ", line);
        }
    }

    [Fact]
    public void ReshapeText_MultipleLines_JoinsAndReshapes()
    {
        // Arrange - multiple short lines
        var buffer = new Buffer(document: new Document("hello\nworld\nfoo\nbar"));

        // Act - reshape all lines with wide width
        BufferOperations.ReshapeText(buffer, fromRow: 0, toRow: 3, textWidth: 80);

        // Assert - should join into fewer lines
        Assert.Equal("hello world foo bar", buffer.Text);
    }

    [Fact]
    public void ReshapeText_EmptyText_NoChange()
    {
        // Arrange
        var buffer = new Buffer(document: new Document(""));

        // Act
        BufferOperations.ReshapeText(buffer, fromRow: 0, toRow: 0);

        // Assert
        Assert.Equal("", buffer.Text);
    }

    [Fact]
    public void ReshapeText_WhitespaceOnly_NoChange()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("   "));

        // Act
        BufferOperations.ReshapeText(buffer, fromRow: 0, toRow: 0);

        // Assert - whitespace only, no words to reshape
        Assert.Equal("   ", buffer.Text);
    }

    [Fact]
    public void ReshapeText_PartialRange_OnlyReshapesSpecifiedLines()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("line1\nhello world foo bar baz qux\nline3"));

        // Act - only reshape line2
        BufferOperations.ReshapeText(buffer, fromRow: 1, toRow: 1, textWidth: 20);

        // Assert - line1 and line3 unchanged
        var lines = buffer.Text.Split('\n');
        Assert.Equal("line1", lines[0]);
        Assert.Equal("line3", lines[^1]);
    }

    [Fact]
    public void ReshapeText_UsesBufferTextWidth()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello world this is a test"));
        buffer.TextWidth = 15;

        // Act - no explicit width, should use buffer's TextWidth
        BufferOperations.ReshapeText(buffer, fromRow: 0, toRow: 0);

        // Assert - wrapped at ~15 chars
        var lines = buffer.Text.Split('\n');
        Assert.True(lines.Length > 1);
    }

    [Fact]
    public void ReshapeText_NullBuffer_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => BufferOperations.ReshapeText(null!, 0, 0));
    }

    [Fact]
    public void ReshapeText_InvalidRowRange_ClampsToValidRange()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello world"));

        // Act - invalid row range
        BufferOperations.ReshapeText(buffer, fromRow: -5, toRow: 100, textWidth: 80);

        // Assert - should handle gracefully (clamps to valid range)
        Assert.Equal("hello world", buffer.Text);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void IndentThenUnindent_ReturnsToOriginal()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("line1\nline2\nline3", cursorPosition: 0));

        // Act
        BufferOperations.Indent(buffer, fromRow: 0, toRow: 3);
        BufferOperations.Unindent(buffer, fromRow: 0, toRow: 3);

        // Assert
        Assert.Equal("line1\nline2\nline3", buffer.Text);
    }

    [Fact]
    public void ReshapeText_LongWordsSurvive()
    {
        // Arrange - word longer than width
        var buffer = new Buffer(document: new Document("supercalifragilisticexpialidocious"));

        // Act - reshape with narrow width
        BufferOperations.ReshapeText(buffer, fromRow: 0, toRow: 0, textWidth: 10);

        // Assert - long word should still be present (can't be broken)
        Assert.Contains("supercalifragilisticexpialidocious", buffer.Text);
    }

    #endregion
}
