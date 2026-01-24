using Xunit;

// Use alias to avoid ambiguity with System.Buffer
using Buffer = Stroke.Core.Buffer;
using Document = Stroke.Core.Document;

namespace Stroke.Tests.Core;

/// <summary>
/// Tests for Buffer editing operations (T028-T031).
/// </summary>
public class BufferEditingTests
{
    #region InsertText Tests

    [Fact]
    public void InsertText_AtBeginning_InsertsText()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello", cursorPosition: 0));

        // Act
        buffer.InsertText("Hi ");

        // Assert
        Assert.Equal("Hi hello", buffer.Text);
        Assert.Equal(3, buffer.CursorPosition);
    }

    [Fact]
    public void InsertText_AtMiddle_InsertsText()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello", cursorPosition: 2));

        // Act
        buffer.InsertText("LL");

        // Assert
        Assert.Equal("heLLllo", buffer.Text);
        Assert.Equal(4, buffer.CursorPosition);
    }

    [Fact]
    public void InsertText_AtEnd_InsertsText()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello", cursorPosition: 5));

        // Act
        buffer.InsertText(" world");

        // Assert
        Assert.Equal("hello world", buffer.Text);
        Assert.Equal(11, buffer.CursorPosition);
    }

    [Fact]
    public void InsertText_EmptyBuffer_InsertsText()
    {
        // Arrange
        var buffer = new Buffer();

        // Act
        buffer.InsertText("hello");

        // Assert
        Assert.Equal("hello", buffer.Text);
        Assert.Equal(5, buffer.CursorPosition);
    }

    [Fact]
    public void InsertText_EmptyString_DoesNothing()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello", cursorPosition: 2));

        // Act
        buffer.InsertText("");

        // Assert
        Assert.Equal("hello", buffer.Text);
        Assert.Equal(2, buffer.CursorPosition);
    }

    [Fact]
    public void InsertText_NullString_DoesNothing()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello", cursorPosition: 2));

        // Act
        buffer.InsertText(null!);

        // Assert
        Assert.Equal("hello", buffer.Text);
        Assert.Equal(2, buffer.CursorPosition);
    }

    [Fact]
    public void InsertText_WithMoveCursorFalse_DoesNotMoveCursor()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello", cursorPosition: 2));

        // Act
        buffer.InsertText("XX", moveCursor: false);

        // Assert
        Assert.Equal("heXXllo", buffer.Text);
        Assert.Equal(2, buffer.CursorPosition);
    }

    [Fact]
    public void InsertText_OverwriteMode_OverwritesCharacters()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello", cursorPosition: 1));

        // Act
        buffer.InsertText("XX", overwrite: true);

        // Assert
        Assert.Equal("hXXlo", buffer.Text);
        Assert.Equal(3, buffer.CursorPosition);
    }

    [Fact]
    public void InsertText_OverwriteAtEnd_InsertsInsteadOfOverwrite()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello", cursorPosition: 5));

        // Act
        buffer.InsertText("XX", overwrite: true);

        // Assert
        Assert.Equal("helloXX", buffer.Text);
        Assert.Equal(7, buffer.CursorPosition);
    }

    [Fact]
    public void InsertText_OverwriteWithNewline_StopsAtNewline()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("ab\ncd", cursorPosition: 1));

        // Act
        buffer.InsertText("XXX", overwrite: true);

        // Assert - should not overwrite past the newline
        Assert.Equal("aXXX\ncd", buffer.Text);
        Assert.Equal(4, buffer.CursorPosition);
    }

    [Fact]
    public void InsertText_FiresOnTextInsertEvent()
    {
        // Arrange
        var buffer = new Buffer();
        var wasFired = false;
        var signal = new ManualResetEventSlim(false);
        buffer.OnTextInsert += _ => { wasFired = true; signal.Set(); };

        // Act
        buffer.InsertText("test");
        signal.Wait(TimeSpan.FromSeconds(5));

        // Assert
        Assert.True(wasFired);
    }

    [Fact]
    public void InsertText_FireEventFalse_DoesNotFireEvent()
    {
        // Arrange
        var buffer = new Buffer();
        var wasFired = false;
        buffer.OnTextInsert += _ => wasFired = true;

        // Act
        buffer.InsertText("test", fireEvent: false);
        Thread.Sleep(50);

        // Assert
        Assert.False(wasFired);
    }

    [Fact]
    public void InsertText_ReadOnlyBuffer_ThrowsException()
    {
        // Arrange
        var buffer = new Buffer(readOnly: () => true);

        // Act & Assert
        Assert.Throws<Stroke.Core.EditReadOnlyBufferException>(() => buffer.InsertText("test"));
    }

    #endregion

    #region Delete Tests

    [Fact]
    public void Delete_AtBeginning_DeletesCharacter()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello", cursorPosition: 0));

        // Act
        var deleted = buffer.Delete();

        // Assert
        Assert.Equal("h", deleted);
        Assert.Equal("ello", buffer.Text);
        Assert.Equal(0, buffer.CursorPosition);
    }

    [Fact]
    public void Delete_AtMiddle_DeletesCharacter()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello", cursorPosition: 2));

        // Act
        var deleted = buffer.Delete();

        // Assert
        Assert.Equal("l", deleted);
        Assert.Equal("helo", buffer.Text);
        Assert.Equal(2, buffer.CursorPosition);
    }

    [Fact]
    public void Delete_AtEnd_ReturnsEmptyString()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello", cursorPosition: 5));

        // Act
        var deleted = buffer.Delete();

        // Assert
        Assert.Equal("", deleted);
        Assert.Equal("hello", buffer.Text);
        Assert.Equal(5, buffer.CursorPosition);
    }

    [Fact]
    public void Delete_MultipleCharacters_DeletesCount()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello world", cursorPosition: 2));

        // Act
        var deleted = buffer.Delete(5);

        // Assert
        Assert.Equal("llo w", deleted);
        Assert.Equal("heorld", buffer.Text);
        Assert.Equal(2, buffer.CursorPosition);
    }

    [Fact]
    public void Delete_MoreThanAvailable_DeletesOnlyAvailable()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello", cursorPosition: 3));

        // Act
        var deleted = buffer.Delete(100);

        // Assert
        Assert.Equal("lo", deleted);
        Assert.Equal("hel", buffer.Text);
        Assert.Equal(3, buffer.CursorPosition);
    }

    [Fact]
    public void Delete_ZeroCount_DeletesNothing()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello", cursorPosition: 2));

        // Act
        var deleted = buffer.Delete(0);

        // Assert
        Assert.Equal("", deleted);
        Assert.Equal("hello", buffer.Text);
    }

    [Fact]
    public void Delete_NegativeCount_ThrowsException()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello", cursorPosition: 2));

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => buffer.Delete(-1));
    }

    [Fact]
    public void Delete_ReadOnlyBuffer_ThrowsException()
    {
        // Arrange
        var buffer = new Buffer(readOnly: () => true, document: new Document("hello", cursorPosition: 2));

        // Act & Assert
        Assert.Throws<Stroke.Core.EditReadOnlyBufferException>(() => buffer.Delete());
    }

    #endregion

    #region DeleteBeforeCursor Tests

    [Fact]
    public void DeleteBeforeCursor_AtMiddle_DeletesCharacter()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello", cursorPosition: 2));

        // Act
        var deleted = buffer.DeleteBeforeCursor();

        // Assert
        Assert.Equal("e", deleted);
        Assert.Equal("hllo", buffer.Text);
        Assert.Equal(1, buffer.CursorPosition);
    }

    [Fact]
    public void DeleteBeforeCursor_AtEnd_DeletesLastCharacter()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello", cursorPosition: 5));

        // Act
        var deleted = buffer.DeleteBeforeCursor();

        // Assert
        Assert.Equal("o", deleted);
        Assert.Equal("hell", buffer.Text);
        Assert.Equal(4, buffer.CursorPosition);
    }

    [Fact]
    public void DeleteBeforeCursor_AtBeginning_ReturnsEmptyString()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello", cursorPosition: 0));

        // Act
        var deleted = buffer.DeleteBeforeCursor();

        // Assert
        Assert.Equal("", deleted);
        Assert.Equal("hello", buffer.Text);
        Assert.Equal(0, buffer.CursorPosition);
    }

    [Fact]
    public void DeleteBeforeCursor_MultipleCharacters_DeletesCount()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello world", cursorPosition: 6));

        // Act
        var deleted = buffer.DeleteBeforeCursor(3);

        // Assert
        Assert.Equal("lo ", deleted);
        Assert.Equal("helworld", buffer.Text);
        Assert.Equal(3, buffer.CursorPosition);
    }

    [Fact]
    public void DeleteBeforeCursor_MoreThanAvailable_DeletesOnlyAvailable()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello", cursorPosition: 2));

        // Act
        var deleted = buffer.DeleteBeforeCursor(100);

        // Assert
        Assert.Equal("he", deleted);
        Assert.Equal("llo", buffer.Text);
        Assert.Equal(0, buffer.CursorPosition);
    }

    [Fact]
    public void DeleteBeforeCursor_ZeroCount_DeletesNothing()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello", cursorPosition: 2));

        // Act
        var deleted = buffer.DeleteBeforeCursor(0);

        // Assert
        Assert.Equal("", deleted);
        Assert.Equal("hello", buffer.Text);
        Assert.Equal(2, buffer.CursorPosition);
    }

    [Fact]
    public void DeleteBeforeCursor_NegativeCount_ThrowsException()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello", cursorPosition: 2));

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => buffer.DeleteBeforeCursor(-1));
    }

    [Fact]
    public void DeleteBeforeCursor_ReadOnlyBuffer_ThrowsException()
    {
        // Arrange
        var buffer = new Buffer(readOnly: () => true, document: new Document("hello", cursorPosition: 2));

        // Act & Assert
        Assert.Throws<Stroke.Core.EditReadOnlyBufferException>(() => buffer.DeleteBeforeCursor());
    }

    #endregion

    #region Newline Tests

    [Fact]
    public void Newline_InsertsLineEnding()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello", cursorPosition: 2));

        // Act
        buffer.Newline(copyMargin: false);

        // Assert
        Assert.Equal("he\nllo", buffer.Text);
        Assert.Equal(3, buffer.CursorPosition);
    }

    [Fact]
    public void Newline_WithCopyMargin_CopiesLeadingWhitespace()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("    hello", cursorPosition: 7));

        // Act
        buffer.Newline(copyMargin: true);

        // Assert
        Assert.Equal("    hel\n    lo", buffer.Text);
    }

    [Fact]
    public void Newline_AtBeginning_InsertsLineEndingAtStart()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello", cursorPosition: 0));

        // Act
        buffer.Newline(copyMargin: false);

        // Assert
        Assert.Equal("\nhello", buffer.Text);
        Assert.Equal(1, buffer.CursorPosition);
    }

    [Fact]
    public void Newline_AtEnd_InsertsLineEndingAtEnd()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello", cursorPosition: 5));

        // Act
        buffer.Newline(copyMargin: false);

        // Assert
        Assert.Equal("hello\n", buffer.Text);
        Assert.Equal(6, buffer.CursorPosition);
    }

    #endregion

    #region InsertLineAbove Tests

    [Fact]
    public void InsertLineAbove_InsertsNewLineAbove()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello\nworld", cursorPosition: 8));

        // Act
        buffer.InsertLineAbove(copyMargin: false);

        // Assert
        Assert.Equal("hello\n\nworld", buffer.Text);
    }

    [Fact]
    public void InsertLineAbove_WithCopyMargin_CopiesLeadingWhitespace()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("    hello", cursorPosition: 7));

        // Act
        buffer.InsertLineAbove(copyMargin: true);

        // Assert
        Assert.Equal("    \n    hello", buffer.Text);
    }

    [Fact]
    public void InsertLineAbove_OnFirstLine_InsertsAtStart()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello", cursorPosition: 3));

        // Act
        buffer.InsertLineAbove(copyMargin: false);

        // Assert
        Assert.Equal("\nhello", buffer.Text);
    }

    #endregion

    #region InsertLineBelow Tests

    [Fact]
    public void InsertLineBelow_InsertsNewLineBelow()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello\nworld", cursorPosition: 2));

        // Act
        buffer.InsertLineBelow(copyMargin: false);

        // Assert
        Assert.Equal("hello\n\nworld", buffer.Text);
    }

    [Fact]
    public void InsertLineBelow_WithCopyMargin_CopiesLeadingWhitespace()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("    hello", cursorPosition: 7));

        // Act
        buffer.InsertLineBelow(copyMargin: true);

        // Assert
        Assert.Equal("    hello\n    ", buffer.Text);
    }

    [Fact]
    public void InsertLineBelow_OnLastLine_InsertsAtEnd()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello", cursorPosition: 3));

        // Act
        buffer.InsertLineBelow(copyMargin: false);

        // Assert
        Assert.Equal("hello\n", buffer.Text);
    }

    #endregion

    #region JoinNextLine Tests

    [Fact]
    public void JoinNextLine_JoinsWithSpace()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello\nworld", cursorPosition: 2));

        // Act
        buffer.JoinNextLine();

        // Assert
        Assert.Equal("hello world", buffer.Text);
    }

    [Fact]
    public void JoinNextLine_WithCustomSeparator_UsesCustomSeparator()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello\nworld", cursorPosition: 2));

        // Act
        buffer.JoinNextLine(separator: ", ");

        // Assert
        Assert.Equal("hello, world", buffer.Text);
    }

    [Fact]
    public void JoinNextLine_RemovesLeadingSpaces()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello\n   world", cursorPosition: 2));

        // Act
        buffer.JoinNextLine();

        // Assert
        Assert.Equal("hello world", buffer.Text);
    }

    [Fact]
    public void JoinNextLine_OnLastLine_DoesNothing()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello", cursorPosition: 2));

        // Act
        buffer.JoinNextLine();

        // Assert
        Assert.Equal("hello", buffer.Text);
    }

    #endregion

    #region SwapCharactersBeforeCursor Tests

    [Fact]
    public void SwapCharactersBeforeCursor_SwapsLastTwoChars()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello", cursorPosition: 3));

        // Act
        buffer.SwapCharactersBeforeCursor();

        // Assert
        Assert.Equal("hlelo", buffer.Text);
        Assert.Equal(3, buffer.CursorPosition);
    }

    [Fact]
    public void SwapCharactersBeforeCursor_AtPosition2_SwapsFirstTwoChars()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello", cursorPosition: 2));

        // Act
        buffer.SwapCharactersBeforeCursor();

        // Assert
        Assert.Equal("ehllo", buffer.Text);
    }

    [Fact]
    public void SwapCharactersBeforeCursor_AtPosition1_DoesNothing()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello", cursorPosition: 1));

        // Act
        buffer.SwapCharactersBeforeCursor();

        // Assert
        Assert.Equal("hello", buffer.Text);
    }

    [Fact]
    public void SwapCharactersBeforeCursor_AtPosition0_DoesNothing()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello", cursorPosition: 0));

        // Act
        buffer.SwapCharactersBeforeCursor();

        // Assert
        Assert.Equal("hello", buffer.Text);
    }

    [Fact]
    public void SwapCharactersBeforeCursor_ReadOnlyBuffer_ThrowsException()
    {
        // Arrange
        var buffer = new Buffer(readOnly: () => true, document: new Document("hello", cursorPosition: 3));

        // Act & Assert
        Assert.Throws<Stroke.Core.EditReadOnlyBufferException>(() => buffer.SwapCharactersBeforeCursor());
    }

    #endregion

    #region TransformLines Tests

    [Fact]
    public void TransformLines_UppercasesSpecifiedLines()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("line0\nline1\nline2\nline3\nline4"));

        // Act
        var result = buffer.TransformLines(new[] { 1, 3 }, s => s.ToUpper());

        // Assert
        Assert.Equal("line0\nLINE1\nline2\nLINE3\nline4", result);
    }

    [Fact]
    public void TransformLines_SkipsInvalidIndices()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("line0\nline1"));

        // Act - include invalid indices
        var result = buffer.TransformLines(new[] { 0, 5, -1, 10 }, s => s.ToUpper());

        // Assert - only valid index 0 transformed
        Assert.Equal("LINE0\nline1", result);
    }

    [Fact]
    public void TransformLines_EmptyIndices_ReturnsOriginal()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("line0\nline1"));

        // Act
        var result = buffer.TransformLines(Array.Empty<int>(), s => s.ToUpper());

        // Assert
        Assert.Equal("line0\nline1", result);
    }

    [Fact]
    public void TransformLines_Range_TransformsMultipleLines()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("a\nb\nc\nd\ne"));

        // Act
        var result = buffer.TransformLines(Enumerable.Range(1, 3), s => s.ToUpper());

        // Assert
        Assert.Equal("a\nB\nC\nD\ne", result);
    }

    [Fact]
    public void TransformLines_NullIndices_ThrowsException()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("test"));

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => buffer.TransformLines(null!, s => s.ToUpper()));
    }

    [Fact]
    public void TransformLines_NullCallback_ThrowsException()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("test"));

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => buffer.TransformLines(new[] { 0 }, null!));
    }

    #endregion

    #region TransformCurrentLine Tests

    [Fact]
    public void TransformCurrentLine_UppercasesCurrentLine()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("line0\nline1\nline2", cursorPosition: 8));
        // Cursor is on "line1"

        // Act
        buffer.TransformCurrentLine(s => s.ToUpper());

        // Assert
        Assert.Equal("line0\nLINE1\nline2", buffer.Text);
    }

    [Fact]
    public void TransformCurrentLine_FirstLine()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello\nworld", cursorPosition: 2));

        // Act
        buffer.TransformCurrentLine(s => s.ToUpper());

        // Assert
        Assert.Equal("HELLO\nworld", buffer.Text);
    }

    [Fact]
    public void TransformCurrentLine_LastLine()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello\nworld", cursorPosition: 8));

        // Act
        buffer.TransformCurrentLine(s => s.ToUpper());

        // Assert
        Assert.Equal("hello\nWORLD", buffer.Text);
    }

    [Fact]
    public void TransformCurrentLine_ReadOnly_ThrowsException()
    {
        // Arrange
        var buffer = new Buffer(readOnly: () => true, document: new Document("test"));

        // Act & Assert
        Assert.Throws<Stroke.Core.EditReadOnlyBufferException>(() =>
            buffer.TransformCurrentLine(s => s.ToUpper()));
    }

    [Fact]
    public void TransformCurrentLine_NullCallback_ThrowsException()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("test"));

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => buffer.TransformCurrentLine(null!));
    }

    #endregion

    #region TransformRegion Tests

    [Fact]
    public void TransformRegion_UppercasesRegion()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello world"));

        // Act
        buffer.TransformRegion(0, 5, s => s.ToUpper());

        // Assert
        Assert.Equal("HELLO world", buffer.Text);
    }

    [Fact]
    public void TransformRegion_MiddleOfText()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello beautiful world"));

        // Act
        buffer.TransformRegion(6, 15, s => s.ToUpper());

        // Assert
        Assert.Equal("hello BEAUTIFUL world", buffer.Text);
    }

    [Fact]
    public void TransformRegion_SpanningNewlines()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("line1\nline2\nline3"));

        // Act - transform "e1\nli" (positions 3-8)
        buffer.TransformRegion(3, 8, s => s.ToUpper());

        // Assert
        Assert.Equal("linE1\nLIne2\nline3", buffer.Text);
    }

    [Fact]
    public void TransformRegion_FromGreaterThanTo_ThrowsException()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello"));

        // Act & Assert
        Assert.Throws<ArgumentException>(() => buffer.TransformRegion(5, 2, s => s.ToUpper()));
    }

    [Fact]
    public void TransformRegion_FromEqualsTo_ThrowsException()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello"));

        // Act & Assert
        Assert.Throws<ArgumentException>(() => buffer.TransformRegion(3, 3, s => s.ToUpper()));
    }

    [Fact]
    public void TransformRegion_ReadOnly_ThrowsException()
    {
        // Arrange
        var buffer = new Buffer(readOnly: () => true, document: new Document("test"));

        // Act & Assert
        Assert.Throws<Stroke.Core.EditReadOnlyBufferException>(() =>
            buffer.TransformRegion(0, 2, s => s.ToUpper()));
    }

    [Fact]
    public void TransformRegion_NullCallback_ThrowsException()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("test"));

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => buffer.TransformRegion(0, 2, null!));
    }

    [Fact]
    public void TransformRegion_BeyondTextLength_ClampsToEnd()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hi"));

        // Act - request beyond text length, should clamp
        buffer.TransformRegion(0, 100, s => s.ToUpper());

        // Assert
        Assert.Equal("HI", buffer.Text);
    }

    #endregion

    #region Thread Safety Tests

    [Fact]
    public async Task ConcurrentInsertText_ThreadSafe()
    {
        // Arrange
        var buffer = new Buffer();
        var iterations = 100;
        var barrier = new Barrier(2);

        // Act - concurrent inserts
        var task1 = Task.Run(() =>
        {
            barrier.SignalAndWait();
            for (var i = 0; i < iterations; i++)
            {
                buffer.InsertText("a", fireEvent: false);
            }
        });

        var task2 = Task.Run(() =>
        {
            barrier.SignalAndWait();
            for (var i = 0; i < iterations; i++)
            {
                buffer.InsertText("b", fireEvent: false);
            }
        });

        // Assert - no exceptions and text has correct total length
        await Task.WhenAll(task1, task2);
        Assert.Equal(200, buffer.Text.Length);
    }

    #endregion
}
