using Stroke.Core;
using Stroke.Lexers;
using Xunit;

namespace Stroke.Tests.Lexers;

/// <summary>
/// Tests for <see cref="ISyntaxSync"/>, <see cref="SyncFromStart"/>, and <see cref="RegexSync"/>.
/// </summary>
public sealed class SyntaxSyncTests
{
    #region ISyntaxSync Contract Tests (FR-008)

    [Fact]
    public void ISyntaxSync_GetSyncStartPosition_ReturnsTuple()
    {
        // Arrange
        ISyntaxSync sync = SyncFromStart.Instance;
        var document = new Document("test");

        // Act
        var position = sync.GetSyncStartPosition(document, 0);

        // Assert
        Assert.Equal((0, 0), position);
    }

    #endregion

    #region SyncFromStart Tests (FR-009)

    [Fact]
    public void SyncFromStart_AlwaysReturnsZeroZero()
    {
        // Arrange
        var sync = SyncFromStart.Instance;
        var document = new Document("line0\nline1\nline2\nline3\nline4");

        // Act & Assert
        Assert.Equal((0, 0), sync.GetSyncStartPosition(document, 0));
        Assert.Equal((0, 0), sync.GetSyncStartPosition(document, 1));
        Assert.Equal((0, 0), sync.GetSyncStartPosition(document, 100));
        Assert.Equal((0, 0), sync.GetSyncStartPosition(document, 1000));
    }

    [Fact]
    public void SyncFromStart_Instance_ReturnsSingleton()
    {
        // Act
        var instance1 = SyncFromStart.Instance;
        var instance2 = SyncFromStart.Instance;

        // Assert
        Assert.Same(instance1, instance2);
    }

    [Fact]
    public void SyncFromStart_NullDocument_ThrowsArgumentNullException()
    {
        // Arrange
        var sync = SyncFromStart.Instance;

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => sync.GetSyncStartPosition(null!, 0));
        Assert.Equal("document", exception.ParamName);
    }

    #endregion

    #region RegexSync Constructor Tests (FR-010)

    [Fact]
    public void RegexSync_Constructor_ValidPattern_Succeeds()
    {
        // Act
        var sync = new RegexSync(@"^\s*(class|def)\s+");

        // Assert
        Assert.NotNull(sync);
    }

    [Fact]
    public void RegexSync_Constructor_NullPattern_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new RegexSync(null!));
        Assert.Equal("pattern", exception.ParamName);
    }

    [Fact]
    public void RegexSync_Constructor_InvalidRegex_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new RegexSync("[invalid"));
        Assert.Contains("pattern", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void RegexSync_Constructor_EmptyPattern_IsValid()
    {
        // Empty pattern matches at position 0 of every line
        // Act
        var sync = new RegexSync("");

        // Assert
        Assert.NotNull(sync);
    }

    #endregion

    #region RegexSync Backwards Scan Tests (FR-010, FR-011, FR-012)

    [Fact]
    public void RegexSync_ScansBackwards_UpToMaxBackwards()
    {
        // Arrange - pattern matches line starting with "def"
        var sync = new RegexSync(@"^def");
        var lines = new string[600];
        for (int i = 0; i < 600; i++)
        {
            lines[i] = $"line{i}";
        }
        lines[100] = "def function1():"; // Only match
        var document = new Document(string.Join("\n", lines));

        // Act - request line 500, scan backwards
        var (Row, Column) = sync.GetSyncStartPosition(document, 500);

        // Assert - should find match at line 100
        Assert.Equal(100, Row);
        Assert.Equal(0, Column);
    }

    [Fact]
    public void RegexSync_ScansBackwards_StopsAtMaxBackwards()
    {
        // Arrange - pattern matches "def", but match is beyond MaxBackwards
        var sync = new RegexSync(@"^def");
        var lines = new string[700];
        for (int i = 0; i < 700; i++)
        {
            lines[i] = $"line{i}";
        }
        lines[50] = "def too_far():"; // Match at line 50, but we're at line 600
        var document = new Document(string.Join("\n", lines));

        // Act - request line 600 (beyond MaxBackwards from match at 50)
        var (Row, Column) = sync.GetSyncStartPosition(document, 600);

        // Assert - match at 50 is more than 500 lines back, so no match found
        // Since line 600 > FromStartIfNoSyncPosFound (100), return requested line
        Assert.Equal(600, Row);
        Assert.Equal(0, Column);
    }

    [Fact]
    public void RegexSync_NoMatchNearStart_ReturnsZeroZero()
    {
        // Arrange - no match in document
        var sync = new RegexSync(@"^NOMATCH$");
        var document = new Document("line0\nline1\nline2\nline3");

        // Act - request line 50 (within FromStartIfNoSyncPosFound = 100)
        var position = sync.GetSyncStartPosition(document, 50);

        // Assert - should return (0, 0)
        Assert.Equal((0, 0), position);
    }

    [Fact]
    public void RegexSync_NoMatchFarFromStart_ReturnsRequestedLine()
    {
        // Arrange - no match in document
        var sync = new RegexSync(@"^NOMATCH$");
        var lines = new string[200];
        for (int i = 0; i < 200; i++)
        {
            lines[i] = $"line{i}";
        }
        var document = new Document(string.Join("\n", lines));

        // Act - request line 150 (beyond FromStartIfNoSyncPosFound = 100)
        var position = sync.GetSyncStartPosition(document, 150);

        // Assert - should return (150, 0)
        Assert.Equal((150, 0), position);
    }

    [Fact]
    public void RegexSync_MatchFound_ReturnsMatchPosition()
    {
        // Arrange
        var sync = new RegexSync(@"^class\s+");
        var document = new Document("# comment\nclass MyClass:\n    pass");

        // Act - request line 2 (the "    pass" line)
        var (Row, Column) = sync.GetSyncStartPosition(document, 2);

        // Assert - should find "class MyClass:" at line 1
        Assert.Equal(1, Row);
        Assert.Equal(0, Column);
    }

    [Fact]
    public void RegexSync_MultipleMatches_ReturnsClosestToRequestedLine()
    {
        // Arrange
        var sync = new RegexSync(@"^def\s+");
        var document = new Document("def func1():\n    pass\ndef func2():\n    pass\ndef func3():\n    pass");

        // Act - request line 3 (first "    pass")
        var (Row, Column) = sync.GetSyncStartPosition(document, 3);

        // Assert - should find "def func2():" at line 2
        Assert.Equal(2, Row);
        Assert.Equal(0, Column);
    }

    [Fact]
    public void RegexSync_NullDocument_ThrowsArgumentNullException()
    {
        // Arrange
        var sync = new RegexSync(@"^test");

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => sync.GetSyncStartPosition(null!, 0));
        Assert.Equal("document", exception.ParamName);
    }

    #endregion

    #region RegexSync Constants Tests

    [Fact]
    public void RegexSync_MaxBackwards_Is500()
    {
        Assert.Equal(500, RegexSync.MaxBackwards);
    }

    [Fact]
    public void RegexSync_FromStartIfNoSyncPosFound_Is100()
    {
        Assert.Equal(100, RegexSync.FromStartIfNoSyncPosFound);
    }

    #endregion

    #region RegexSync ForLanguage Tests (FR-013)

    [Fact]
    public void RegexSync_ForLanguage_Python_ReturnsCorrectPattern()
    {
        // Arrange
        var sync = RegexSync.ForLanguage("Python");
        var document = new Document("# comment\nclass MyClass:\n    pass");

        // Act
        var (Row, Column) = sync.GetSyncStartPosition(document, 2);

        // Assert - should find "class MyClass:" at line 1
        Assert.Equal(1, Row);
    }

    [Fact]
    public void RegexSync_ForLanguage_Python3_ReturnsCorrectPattern()
    {
        // Arrange
        var sync = RegexSync.ForLanguage("Python 3");
        var document = new Document("import os\ndef main():\n    pass");

        // Act
        var (Row, Column) = sync.GetSyncStartPosition(document, 2);

        // Assert - should find "def main():" at line 1
        Assert.Equal(1, Row);
    }

    [Fact]
    public void RegexSync_ForLanguage_HTML_ReturnsCorrectPattern()
    {
        // Arrange
        var sync = RegexSync.ForLanguage("HTML");
        var document = new Document("text\n<div>\ncontent\n</div>");

        // Act
        var position = sync.GetSyncStartPosition(document, 2);

        // Assert - should find "<div>" at line 1
        Assert.Equal(1, position.Row);
    }

    [Fact]
    public void RegexSync_ForLanguage_JavaScript_ReturnsCorrectPattern()
    {
        // Arrange
        var sync = RegexSync.ForLanguage("JavaScript");
        var document = new Document("// comment\nfunction test() {\n  return 1;\n}");

        // Act
        var (Row, Column) = sync.GetSyncStartPosition(document, 2);

        // Assert - should find "function test()" at line 1
        Assert.Equal(1, Row);
    }

    [Fact]
    public void RegexSync_ForLanguage_Unknown_ReturnsDefaultPattern()
    {
        // Arrange
        var sync = RegexSync.ForLanguage("SomeUnknownLanguage");
        var document = new Document("line0\nline1\nline2");

        // Act - default pattern "^" matches every line start
        var (Row, Column) = sync.GetSyncStartPosition(document, 2);

        // Assert - should match line 2 (closest match scanning backwards)
        Assert.Equal(2, Row);
        Assert.Equal(0, Column);
    }

    [Fact]
    public void RegexSync_ForLanguage_CaseInsensitive()
    {
        // Act - should work regardless of case
        var syncLower = RegexSync.ForLanguage("python");
        var syncUpper = RegexSync.ForLanguage("PYTHON");
        var syncMixed = RegexSync.ForLanguage("PyThOn");

        var document = new Document("import os\ndef main():\n    pass");

        // Assert - all should work
        Assert.Equal(1, syncLower.GetSyncStartPosition(document, 2).Row);
        Assert.Equal(1, syncUpper.GetSyncStartPosition(document, 2).Row);
        Assert.Equal(1, syncMixed.GetSyncStartPosition(document, 2).Row);
    }

    #endregion

    #region Edge Cases (EC-015, EC-016)

    [Fact]
    public void RegexSync_EmptyPattern_MatchesAllLines()
    {
        // Arrange - empty pattern matches at position 0 of every line
        var sync = new RegexSync("");
        var document = new Document("line0\nline1\nline2\nline3");

        // Act - request line 3
        var (Row, Column) = sync.GetSyncStartPosition(document, 3);

        // Assert - empty pattern matches every line, so returns line 3 itself
        Assert.Equal(3, Row);
        Assert.Equal(0, Column);
    }

    [Fact]
    public void RegexSync_RequestLineZero_ReturnsZeroZero()
    {
        // Arrange
        var sync = new RegexSync(@"^def");
        var document = new Document("def func():\n    pass");

        // Act
        var position = sync.GetSyncStartPosition(document, 0);

        // Assert - line 0 has match, return (0, 0)
        Assert.Equal((0, 0), position);
    }

    [Fact]
    public void RegexSync_RequestNegativeLine_ReturnsZeroZero()
    {
        // Arrange
        var sync = new RegexSync(@"^def");
        var document = new Document("def func():\n    pass");

        // Act
        var position = sync.GetSyncStartPosition(document, -5);

        // Assert - negative line, should return (0, 0)
        Assert.Equal((0, 0), position);
    }

    [Fact]
    public void RegexSync_RequestBeyondDocument_SearchesFromEnd()
    {
        // Arrange
        var sync = new RegexSync(@"^def");
        var document = new Document("def func1():\n    pass\ndef func2():\n    pass");

        // Act - request line 1000, document has 4 lines (0-3)
        var (Row, Column) = sync.GetSyncStartPosition(document, 1000);

        // Assert - should find closest match before line 1000 (clamped to document end)
        Assert.Equal(2, Row); // "def func2():" is at line 2
    }

    #endregion

    #region Acceptance Tests (User Story 3)

    [Fact]
    public void Given_RegexSyncWithPattern_When_RequestingLine1000_Then_PositionWithin500Lines()
    {
        // Given
        var sync = new RegexSync(@"^class\s+");
        var lines = new string[1200];
        for (int i = 0; i < 1200; i++)
        {
            lines[i] = $"    statement{i}";
        }
        lines[600] = "class MyClass:"; // Match at line 600
        var document = new Document(string.Join("\n", lines));

        // When
        var (Row, Column) = sync.GetSyncStartPosition(document, 1000);

        // Then - position should be at line 600 (within 500 lines of line 1000)
        Assert.True(Row >= 500 && Row <= 1000,
            $"Expected position within 500 lines of line 1000, got {Row}");
    }

    [Fact]
    public void Given_RegexSync_When_NoMatchNearStart_Then_ReturnsZeroZero()
    {
        // Given
        var sync = new RegexSync(@"^NEVER_MATCH$");
        var document = new Document("line0\nline1\nline2");

        // When - request line within FromStartIfNoSyncPosFound (< 100)
        var position = sync.GetSyncStartPosition(document, 50);

        // Then
        Assert.Equal((0, 0), position);
    }

    [Fact]
    public void Given_RegexSync_When_NoMatchFarFromStart_Then_ReturnsRequestedLine()
    {
        // Given
        var sync = new RegexSync(@"^NEVER_MATCH$");
        var lines = new string[200];
        for (int i = 0; i < 200; i++)
        {
            lines[i] = $"line{i}";
        }
        var document = new Document(string.Join("\n", lines));

        // When - request line beyond FromStartIfNoSyncPosFound (>= 100)
        var position = sync.GetSyncStartPosition(document, 150);

        // Then
        Assert.Equal((150, 0), position);
    }

    [Fact]
    public void Given_SyncFromStart_When_RequestingAnyLine_Then_ReturnsZeroZero()
    {
        // Given
        var sync = SyncFromStart.Instance;
        var document = new Document("line0\nline1\nline2\nline3\nline4");

        // When/Then
        Assert.Equal((0, 0), sync.GetSyncStartPosition(document, 0));
        Assert.Equal((0, 0), sync.GetSyncStartPosition(document, 2));
        Assert.Equal((0, 0), sync.GetSyncStartPosition(document, 1000));
    }

    #endregion
}
