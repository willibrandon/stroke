using Stroke.Core;
using Xunit;

namespace Stroke.Tests.Core;

/// <summary>
/// Tests for Document bracket matching methods (User Story 6).
/// Covers FindMatchingBracketPosition, FindEnclosingBracketLeft, FindEnclosingBracketRight.
/// </summary>
public class DocumentBracketTests
{
    #region FindMatchingBracketPosition Tests

    // T108: Test FindMatchingBracketPosition for each bracket type (), [], {}, <>
    [Theory]
    [InlineData("(hello)", 0, 6)]   // Opening paren at 0, matching at 6 (offset = 6)
    [InlineData("(hello)", 6, -6)]  // Closing paren at 6, matching at 0 (offset = -6)
    [InlineData("[hello]", 0, 6)]   // Square brackets
    [InlineData("{hello}", 0, 6)]   // Curly braces
    [InlineData("<hello>", 0, 6)]   // Angle brackets
    public void FindMatchingBracketPosition_BasicBrackets_ReturnsCorrectOffset(string text, int cursorPos, int expectedOffset)
    {
        // Arrange
        var doc = new Document(text, cursorPosition: cursorPos);

        // Act
        var result = doc.FindMatchingBracketPosition();

        // Assert
        Assert.Equal(expectedOffset, result);
    }

    // T109: Test FindMatchingBracketPosition with nested brackets
    [Fact]
    public void FindMatchingBracketPosition_NestedBrackets_FindsCorrectMatch()
    {
        // Arrange - cursor at outer opening paren at index 0
        // Text: "((inner))" - indices: 0:'(' 1:'(' 2:'i' ... 7:')' 8:')'
        var doc = new Document("((inner))", cursorPosition: 0);

        // Act
        var result = doc.FindMatchingBracketPosition();

        // Assert - outer closing paren at index 8, offset = 8 - 0 = 8
        Assert.Equal(8, result);
    }

    [Fact]
    public void FindMatchingBracketPosition_NestedBrackets_InnerOpening()
    {
        // Arrange - cursor at inner opening paren at index 1
        var doc = new Document("((inner))", cursorPosition: 1);

        // Act
        var result = doc.FindMatchingBracketPosition();

        // Assert - inner closing paren at index 7, offset = 7 - 1 = 6
        Assert.Equal(6, result);
    }

    [Fact]
    public void FindMatchingBracketPosition_MixedBrackets_FindsCorrectType()
    {
        // Arrange - cursor at opening square bracket at index 1
        // Text: "([{<>}])" - indices: 0:'(' 1:'[' 2:'{' 3:'<' 4:'>' 5:'}' 6:']' 7:')'
        var doc = new Document("([{<>}])", cursorPosition: 1);

        // Act
        var result = doc.FindMatchingBracketPosition();

        // Assert - matching ] at index 6, offset = 6 - 1 = 5
        Assert.Equal(5, result);
    }

    [Fact]
    public void FindMatchingBracketPosition_CursorNotOnBracket_ReturnsZero()
    {
        // Arrange - cursor on letter 'h' at index 1
        var doc = new Document("(hello)", cursorPosition: 1);

        // Act
        var result = doc.FindMatchingBracketPosition();

        // Assert - not on a bracket, returns 0
        Assert.Equal(0, result);
    }

    #endregion

    #region FindEnclosingBracketLeft Tests

    // T110: Test FindEnclosingBracketLeft
    [Fact]
    public void FindEnclosingBracketLeft_FindsOpeningBracket()
    {
        // Arrange - cursor inside parentheses at index 3
        // Text: "(hello)" - indices: 0:'(' 1:'h' 2:'e' 3:'l' 4:'l' 5:'o' 6:')'
        var doc = new Document("(hello)", cursorPosition: 3);

        // Act
        var result = doc.FindEnclosingBracketLeft('(', ')');

        // Assert - opening bracket at index 0, offset = 0 - 3 = -3
        Assert.Equal(-3, result);
    }

    [Fact]
    public void FindEnclosingBracketLeft_NestedBrackets_FindsInnermost()
    {
        // Arrange - cursor inside inner parens at index 9
        // Text: "(outer(inner))" - 0:'(' 6:'(' inner... 12:')' 13:')'
        var doc = new Document("(outer(inner))", cursorPosition: 9);

        // Act
        var result = doc.FindEnclosingBracketLeft('(', ')');

        // Assert - should find inner opening at index 6, offset = 6 - 9 = -3
        Assert.Equal(-3, result);
    }

    [Fact]
    public void FindEnclosingBracketLeft_NoEnclosing_ReturnsNull()
    {
        // Arrange - cursor outside any brackets
        var doc = new Document("hello world", cursorPosition: 5);

        // Act
        var result = doc.FindEnclosingBracketLeft('(', ')');

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void FindEnclosingBracketLeft_SkipsNestedPairs()
    {
        // Arrange - cursor after nested parens at index 7
        // Text: "((a)(b)c)" - 0:'(' 1:'(' 2:'a' 3:')' 4:'(' 5:'b' 6:')' 7:'c' 8:')'
        var doc = new Document("((a)(b)c)", cursorPosition: 7);

        // Act
        var result = doc.FindEnclosingBracketLeft('(', ')');

        // Assert - should find outer opening at index 0, offset = 0 - 7 = -7
        Assert.Equal(-7, result);
    }

    [Fact]
    public void FindEnclosingBracketLeft_CursorOnLeftBracket_ReturnsZero()
    {
        // Arrange - cursor on the opening bracket itself
        var doc = new Document("(hello)", cursorPosition: 0);

        // Act
        var result = doc.FindEnclosingBracketLeft('(', ')');

        // Assert - Python returns 0 when cursor is on the left bracket
        Assert.Equal(0, result);
    }

    #endregion

    #region FindEnclosingBracketRight Tests

    // T111: Test FindEnclosingBracketRight
    [Fact]
    public void FindEnclosingBracketRight_FindsClosingBracket()
    {
        // Arrange - cursor inside parentheses at index 3
        var doc = new Document("(hello)", cursorPosition: 3);

        // Act
        var result = doc.FindEnclosingBracketRight('(', ')');

        // Assert - closing bracket at index 6, offset = 6 - 3 = 3
        Assert.Equal(3, result);
    }

    [Fact]
    public void FindEnclosingBracketRight_NestedBrackets_FindsInnermost()
    {
        // Arrange - cursor inside inner parens at index 9
        // Text: "(outer(inner)outer)" - 0:'(' 6:'(' inner 12:')' 18:')'
        var doc = new Document("(outer(inner)outer)", cursorPosition: 9);

        // Act
        var result = doc.FindEnclosingBracketRight('(', ')');

        // Assert - should find inner closing at index 12, offset = 12 - 9 = 3
        Assert.Equal(3, result);
    }

    [Fact]
    public void FindEnclosingBracketRight_NoEnclosing_ReturnsNull()
    {
        // Arrange - cursor outside any brackets
        var doc = new Document("hello world", cursorPosition: 5);

        // Act
        var result = doc.FindEnclosingBracketRight('(', ')');

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void FindEnclosingBracketRight_SkipsNestedPairs()
    {
        // Arrange - cursor before nested parens at index 1
        // Text: "(a(b)(c))" - 0:'(' 1:'a' 2:'(' 3:'b' 4:')' 5:'(' 6:'c' 7:')' 8:')'
        var doc = new Document("(a(b)(c))", cursorPosition: 1);

        // Act
        var result = doc.FindEnclosingBracketRight('(', ')');

        // Assert - should find outer closing at index 8, offset = 8 - 1 = 7
        Assert.Equal(7, result);
    }

    [Fact]
    public void FindEnclosingBracketRight_CursorOnRightBracket_ReturnsZero()
    {
        // Arrange - cursor on the closing bracket itself
        var doc = new Document("(hello)", cursorPosition: 6);

        // Act
        var result = doc.FindEnclosingBracketRight('(', ')');

        // Assert - Python returns 0 when cursor is on the right bracket
        Assert.Equal(0, result);
    }

    #endregion

    #region Edge Cases

    // T112: Test edge cases
    [Fact]
    public void FindMatchingBracketPosition_UnmatchedOpening_ReturnsZero()
    {
        // Arrange - unmatched opening bracket
        var doc = new Document("(hello", cursorPosition: 0);

        // Act
        var result = doc.FindMatchingBracketPosition();

        // Assert - no match found, returns 0
        Assert.Equal(0, result);
    }

    [Fact]
    public void FindMatchingBracketPosition_UnmatchedClosing_ReturnsZero()
    {
        // Arrange - unmatched closing bracket
        var doc = new Document("hello)", cursorPosition: 5);

        // Act
        var result = doc.FindMatchingBracketPosition();

        // Assert - no match found, returns 0
        Assert.Equal(0, result);
    }

    [Fact]
    public void FindMatchingBracketPosition_EmptyDocument_ReturnsZero()
    {
        // Arrange
        var doc = new Document("", cursorPosition: 0);

        // Act
        var result = doc.FindMatchingBracketPosition();

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void FindEnclosingBracketLeft_AtStartOfDocument_ReturnsNull()
    {
        // Arrange - cursor at start, no brackets before
        var doc = new Document("hello)", cursorPosition: 0);

        // Act
        var result = doc.FindEnclosingBracketLeft('(', ')');

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void FindEnclosingBracketRight_AtEndOfDocument_ReturnsNull()
    {
        // Arrange - cursor at end, no brackets after
        var doc = new Document("(hello", cursorPosition: 6);

        // Act
        var result = doc.FindEnclosingBracketRight('(', ')');

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void FindMatchingBracketPosition_DeeplyNested_FindsCorrectMatch()
    {
        // Arrange - deeply nested brackets
        // Text: "(((a)))" - 0:'(' 1:'(' 2:'(' 3:'a' 4:')' 5:')' 6:')'
        var doc = new Document("(((a)))", cursorPosition: 0);

        // Act
        var result = doc.FindMatchingBracketPosition();

        // Assert - outermost closing at index 6, offset = 6 - 0 = 6
        Assert.Equal(6, result);
    }

    [Fact]
    public void FindMatchingBracketPosition_MultipleTypes_IgnoresDifferentTypes()
    {
        // Arrange - cursor on square bracket at index 1, ignore parens
        // Text: "([)]" - 0:'(' 1:'[' 2:')' 3:']'
        // Note: mismatched parens, but we're only looking for [] match
        var doc = new Document("([)]", cursorPosition: 1);

        // Act
        var result = doc.FindMatchingBracketPosition();

        // Assert - matching ] at index 3, offset = 3 - 1 = 2
        Assert.Equal(2, result);
    }

    [Fact]
    public void FindEnclosingBracketLeft_WithStartPos_DoesNotLookPast()
    {
        // Arrange - text has brackets but startPos limits search
        // Text: "(a(b))" - 0:'(' 1:'a' 2:'(' 3:'b' 4:')' 5:')'
        var doc = new Document("(a(b))", cursorPosition: 3);

        // Act - with startPos=2, should find '(' at index 2
        var result = doc.FindEnclosingBracketLeft('(', ')', startPos: 2);

        // Assert - opening at index 2, offset = 2 - 3 = -1
        Assert.Equal(-1, result);
    }

    [Fact]
    public void FindEnclosingBracketRight_WithEndPos_DoesNotLookPast()
    {
        // Arrange
        // Text: "((a))" - 0:'(' 1:'(' 2:'a' 3:')' 4:')'
        var doc = new Document("((a))", cursorPosition: 2);

        // Act - with endPos=4, should find ')' at index 3
        var result = doc.FindEnclosingBracketRight('(', ')', endPos: 4);

        // Assert - closing at index 3, offset = 3 - 2 = 1
        Assert.Equal(1, result);
    }

    #endregion
}
