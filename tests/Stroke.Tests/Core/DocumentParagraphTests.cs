using Stroke.Core;
using Xunit;

namespace Stroke.Tests.Core;

/// <summary>
/// Tests for Document paragraph navigation methods (User Story 8).
/// Covers StartOfParagraph and EndOfParagraph.
/// </summary>
public class DocumentParagraphTests
{
    #region StartOfParagraph Tests

    // T131: Test StartOfParagraph
    [Fact]
    public void StartOfParagraph_ReturnsOffsetToEmptyLine()
    {
        // Arrange - cursor in second paragraph
        var doc = new Document("paragraph1\n\nparagraph2", cursorPosition: 15);

        // Act
        var result = doc.StartOfParagraph();

        // Assert - returns negative offset to start (empty line or beginning)
        Assert.True(result <= 0);
    }

    [Fact]
    public void StartOfParagraph_FromFirstParagraph_ReturnsToBeginning()
    {
        // Arrange - cursor in first paragraph
        var doc = new Document("paragraph1\n\nparagraph2", cursorPosition: 5);

        // Act
        var result = doc.StartOfParagraph();

        // Assert - goes to beginning of document
        Assert.Equal(-5, result); // -cursor_position
    }

    [Fact]
    public void StartOfParagraph_WithCount_FindsNthParagraph()
    {
        // Arrange - three paragraphs
        var doc = new Document("para1\n\npara2\n\npara3", cursorPosition: 18); // in "para3"

        // Act - count=2 should find second empty line before cursor
        var result = doc.StartOfParagraph(count: 2);

        // Assert - should be at or before the empty line between para1 and para2
        Assert.True(result < 0);
    }

    [Fact]
    public void StartOfParagraph_WithBefore_StopsBeforeEmptyLine()
    {
        // Arrange
        var doc = new Document("para1\n\npara2", cursorPosition: 10); // in "para2"

        // Act
        var withBefore = doc.StartOfParagraph(before: true);
        var withoutBefore = doc.StartOfParagraph(before: false);

        // Assert - before=true stops one position earlier
        Assert.True(withBefore <= withoutBefore);
    }

    #endregion

    #region EndOfParagraph Tests

    // T132: Test EndOfParagraph
    [Fact]
    public void EndOfParagraph_ReturnsOffsetToEmptyLine()
    {
        // Arrange - cursor in first paragraph
        var doc = new Document("paragraph1\n\nparagraph2", cursorPosition: 5);

        // Act
        var result = doc.EndOfParagraph();

        // Assert - returns positive offset to empty line
        Assert.True(result >= 0);
    }

    [Fact]
    public void EndOfParagraph_FromLastParagraph_ReturnsToEnd()
    {
        // Arrange - cursor in last paragraph
        var doc = new Document("paragraph1\n\nparagraph2", cursorPosition: 15);

        // Act
        var result = doc.EndOfParagraph();

        // Assert - goes to end of document
        Assert.Equal(7, result); // length of "graph2" from cursor position 15
    }

    [Fact]
    public void EndOfParagraph_WithCount_FindsNthParagraph()
    {
        // Arrange - three paragraphs
        var doc = new Document("para1\n\npara2\n\npara3", cursorPosition: 2); // in "para1"

        // Act - count=2 should find second empty line after cursor
        var result = doc.EndOfParagraph(count: 2);

        // Assert - should be at or after the empty line between para2 and para3
        Assert.True(result > 0);
    }

    [Fact]
    public void EndOfParagraph_WithAfter_StopsAfterEmptyLine()
    {
        // Arrange
        var doc = new Document("para1\n\npara2", cursorPosition: 2); // in "para1"

        // Act
        var withAfter = doc.EndOfParagraph(after: true);
        var withoutAfter = doc.EndOfParagraph(after: false);

        // Assert - after=true stops one position later
        Assert.True(withAfter >= withoutAfter);
    }

    #endregion

    #region Single Paragraph Tests

    // T133: Test paragraph navigation with single paragraph
    [Fact]
    public void StartOfParagraph_SingleParagraph_ReturnsToBeginning()
    {
        // Arrange - no empty lines
        var doc = new Document("hello world", cursorPosition: 5);

        // Act
        var result = doc.StartOfParagraph();

        // Assert - no empty line found, go to beginning
        Assert.Equal(-5, result);
    }

    [Fact]
    public void EndOfParagraph_SingleParagraph_ReturnsToEnd()
    {
        // Arrange - no empty lines
        var doc = new Document("hello world", cursorPosition: 5);

        // Act
        var result = doc.EndOfParagraph();

        // Assert - no empty line found, go to end
        Assert.Equal(6, result); // " world" = 6 chars
    }

    #endregion

    #region Trailing Empty Lines Tests

    // T134: Test paragraph navigation with trailing empty lines
    [Fact]
    public void EndOfParagraph_WithTrailingEmptyLines_StopsAtFirstEmpty()
    {
        // Arrange - content followed by empty lines
        var doc = new Document("content\n\n\n", cursorPosition: 2);

        // Act
        var result = doc.EndOfParagraph();

        // Assert - should stop at first empty line
        Assert.True(result > 0);
    }

    [Fact]
    public void StartOfParagraph_WithLeadingEmptyLines_StopsAtFirstEmpty()
    {
        // Arrange - empty lines followed by content
        var doc = new Document("\n\ncontent", cursorPosition: 5);

        // Act
        var result = doc.StartOfParagraph();

        // Assert - should find the empty line(s) at start
        Assert.True(result <= 0);
    }

    [Fact]
    public void StartOfParagraph_OnEmptyLine_FindsPreviousEmpty()
    {
        // Arrange - cursor on empty line
        var doc = new Document("para1\n\npara2", cursorPosition: 6); // on the empty line

        // Act
        var result = doc.StartOfParagraph();

        // Assert - should go to beginning since no empty line before
        Assert.Equal(-6, result);
    }

    [Fact]
    public void EndOfParagraph_OnEmptyLine_FindsNextEmpty()
    {
        // Arrange - cursor on empty line between paragraphs
        var doc = new Document("para1\n\npara2\n\npara3", cursorPosition: 6); // on first empty line

        // Act
        var result = doc.EndOfParagraph();

        // Assert - should find second empty line
        Assert.True(result > 0);
    }

    #endregion
}
