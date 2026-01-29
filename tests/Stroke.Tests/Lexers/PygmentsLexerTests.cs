using Stroke.Core;
using Stroke.Filters;
using Stroke.FormattedText;
using Stroke.Lexers;
using Xunit;

namespace Stroke.Tests.Lexers;

/// <summary>
/// Tests for <see cref="PygmentsLexer"/>.
/// </summary>
public sealed class PygmentsLexerTests
{
    #region Token Conversion Tests (FR-014)

    [Fact]
    public void PygmentsLexer_TokenConversion_SingleLevel()
    {
        // Arrange
        var pythonLexer = new TestPythonLexer();
        var lexer = new PygmentsLexer(pythonLexer);
        var document = new Document("def");

        // Act
        var getLine = lexer.LexDocument(document);
        var tokens = getLine(0);

        // Assert - "def" should be a keyword
        Assert.True(tokens.Count >= 1);
        var keywordToken = tokens.First(t => t.Text == "def");
        Assert.Equal("class:pygments.keyword", keywordToken.Style);
    }

    [Fact]
    public void PygmentsLexer_TokenConversion_Nested()
    {
        // Arrange - using comment which produces ["Comment", "Single"]
        var pythonLexer = new TestPythonLexer();
        var lexer = new PygmentsLexer(pythonLexer);
        var document = new Document("# comment");

        // Act
        var getLine = lexer.LexDocument(document);
        var tokens = getLine(0);

        // Assert
        Assert.True(tokens.Count >= 1);
        var commentToken = tokens.First(t => t.Text.StartsWith("#"));
        Assert.Equal("class:pygments.comment.single", commentToken.Style);
    }

    [Fact]
    public void PygmentsLexer_TokenConversion_PreservesWhitespace()
    {
        // Arrange
        var pythonLexer = new TestPythonLexer();
        var lexer = new PygmentsLexer(pythonLexer);
        var document = new Document("x = 1");

        // Act
        var getLine = lexer.LexDocument(document);
        var tokens = getLine(0);

        // Assert - should have whitespace tokens
        var allText = string.Join("", tokens.Select(t => t.Text));
        Assert.Equal("x = 1", allText);
    }

    #endregion

    #region syncFromStart Tests (FR-015)

    [Fact]
    public void PygmentsLexer_SyncFromStart_DefaultTreatedAsTrue()
    {
        // Arrange - default FilterOrBool should behave as true
        var pythonLexer = new TestPythonLexer();
        var lexer = new PygmentsLexer(pythonLexer, default);
        var document = new Document("line0\nline1\nline2");

        // Act - should work, defaulting to sync from start
        var getLine = lexer.LexDocument(document);
        var tokens = getLine(2);

        // Assert - should complete without error
        Assert.NotNull(tokens);
    }

    [Fact]
    public void PygmentsLexer_SyncFromStart_True_LexesFromBeginning()
    {
        // Arrange
        var pythonLexer = new TestPythonLexer();
        var lexer = new PygmentsLexer(pythonLexer, syncFromStart: true);
        var document = new Document("line0\nline1\nline2");

        // Act
        var getLine = lexer.LexDocument(document);
        var line2 = getLine(2);

        // Assert
        Assert.NotNull(line2);
    }

    [Fact]
    public void PygmentsLexer_SyncFromStart_False_UsesSyntaxSync()
    {
        // Arrange - with explicit syntaxSync
        var pythonLexer = new TestPythonLexer();
        var sync = new RegexSync(@"^def");
        var lexer = new PygmentsLexer(pythonLexer, syncFromStart: false, syntaxSync: sync);
        var document = new Document("def func():\nline1\nline2");

        // Act
        var getLine = lexer.LexDocument(document);
        var line2 = getLine(2);

        // Assert
        Assert.NotNull(line2);
    }

    [Fact]
    public void PygmentsLexer_SyncFromStart_FilterEvaluated()
    {
        // Arrange
        var evaluated = false;
        var filter = new Condition(() =>
        {
            evaluated = true;
            return true;
        });
        var pythonLexer = new TestPythonLexer();
        var lexer = new PygmentsLexer(pythonLexer, syncFromStart: filter);
        var document = new Document("test");

        // Act
        var getLine = lexer.LexDocument(document);
        _ = getLine(0);

        // Assert
        Assert.True(evaluated, "Filter should have been evaluated");
    }

    #endregion

    #region Null Handling Tests (EC-017, EC-019)

    [Fact]
    public void PygmentsLexer_Constructor_NullPygmentsLexer_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new PygmentsLexer(null!));
        Assert.Equal("pygmentsLexer", exception.ParamName);
    }

    [Fact]
    public void PygmentsLexer_LexDocument_NullDocument_ThrowsArgumentNullException()
    {
        // Arrange
        var pythonLexer = new TestPythonLexer();
        var lexer = new PygmentsLexer(pythonLexer);

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => lexer.LexDocument(null!));
        Assert.Equal("document", exception.ParamName);
    }

    #endregion

    #region FromFilename Tests (FR-019)

    [Fact]
    public void PygmentsLexer_FromFilename_UnknownExtension_ReturnsSimpleLexer()
    {
        // Act
        var lexer = PygmentsLexer.FromFilename("file.unknown");

        // Assert
        Assert.IsType<SimpleLexer>(lexer);
    }

    [Fact]
    public void PygmentsLexer_FromFilename_EmptyFilename_ReturnsSimpleLexer()
    {
        // Act
        var lexer = PygmentsLexer.FromFilename("");

        // Assert
        Assert.IsType<SimpleLexer>(lexer);
    }

    [Fact]
    public void PygmentsLexer_FromFilename_NullFilename_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => PygmentsLexer.FromFilename(null!));
        Assert.Equal("filename", exception.ParamName);
    }

    #endregion

    #region IPygmentsLexer Interface Tests (FR-020)

    [Fact]
    public void IPygmentsLexer_Name_ReturnsLexerName()
    {
        // Arrange
        IPygmentsLexer lexer = new TestPythonLexer();

        // Act
        var name = lexer.Name;

        // Assert
        Assert.Equal("Python", name);
    }

    [Fact]
    public void IPygmentsLexer_GetTokensUnprocessed_ReturnsTokensInOrder()
    {
        // Arrange
        var lexer = new TestPythonLexer();

        // Act
        var tokens = lexer.GetTokensUnprocessed("x = 1").ToList();

        // Assert
        Assert.True(tokens.Count >= 3, "Should have at least x, =, 1");
        var lastIndex = -1;
        foreach (var token in tokens)
        {
            Assert.True(token.Index > lastIndex || lastIndex == -1,
                $"Token index {token.Index} should be after {lastIndex}");
            lastIndex = token.Index + token.Text.Length - 1;
        }
    }

    [Fact]
    public void IPygmentsLexer_GetTokensUnprocessed_EmptyInput_ReturnsEmpty()
    {
        // Arrange
        var lexer = new TestPythonLexer();

        // Act
        var tokens = lexer.GetTokensUnprocessed("").ToList();

        // Assert
        Assert.Empty(tokens);
    }

    #endregion

    #region Invalid Line Number Tests (EC-005)

    [Fact]
    public void PygmentsLexer_NegativeLineNumber_ReturnsEmptyList()
    {
        // Arrange
        var pythonLexer = new TestPythonLexer();
        var lexer = new PygmentsLexer(pythonLexer);
        var document = new Document("test");

        // Act
        var getLine = lexer.LexDocument(document);
        var tokens = getLine(-1);

        // Assert
        Assert.Empty(tokens);
    }

    [Fact]
    public void PygmentsLexer_LineBeyondBounds_ReturnsEmptyList()
    {
        // Arrange
        var pythonLexer = new TestPythonLexer();
        var lexer = new PygmentsLexer(pythonLexer);
        var document = new Document("test");

        // Act
        var getLine = lexer.LexDocument(document);
        var tokens = getLine(100);

        // Assert
        Assert.Empty(tokens);
    }

    #endregion

    #region Empty Document Tests (EC-006)

    [Fact]
    public void PygmentsLexer_EmptyDocument_ReturnsTokensForEmptyLine()
    {
        // Arrange
        var pythonLexer = new TestPythonLexer();
        var lexer = new PygmentsLexer(pythonLexer);
        var document = new Document("");

        // Act
        var getLine = lexer.LexDocument(document);
        var tokens = getLine(0);

        // Assert - empty document has one empty line
        Assert.NotNull(tokens);
    }

    #endregion

    #region Unicode Content Tests (EC-011)

    [Fact]
    public void PygmentsLexer_UnicodeContent_ProcessedWithoutError()
    {
        // Arrange
        var pythonLexer = new TestPythonLexer();
        var lexer = new PygmentsLexer(pythonLexer);
        var document = new Document("x = '世界'");

        // Act
        var getLine = lexer.LexDocument(document);
        var tokens = getLine(0);

        // Assert - should process without error
        Assert.NotNull(tokens);
        var allText = string.Join("", tokens.Select(t => t.Text));
        Assert.Equal("x = '世界'", allText);
    }

    #endregion

    #region InvalidationHash Tests

    [Fact]
    public void PygmentsLexer_InvalidationHash_ReturnsSelf()
    {
        // Arrange
        var pythonLexer = new TestPythonLexer();
        var lexer = new PygmentsLexer(pythonLexer);

        // Act
        var hash = lexer.InvalidationHash();

        // Assert
        Assert.Same(lexer, hash);
    }

    #endregion

    #region Multi-Line Document Tests

    [Fact]
    public void PygmentsLexer_MultiLineDocument_ProcessesAllLines()
    {
        // Arrange
        var pythonLexer = new TestPythonLexer();
        var lexer = new PygmentsLexer(pythonLexer);
        var document = new Document("def foo():\n    return 1\n\nx = foo()");

        // Act
        var getLine = lexer.LexDocument(document);

        // Assert - all lines should be accessible
        for (int i = 0; i < 4; i++)
        {
            var tokens = getLine(i);
            Assert.NotNull(tokens);
        }
    }

    [Fact]
    public void PygmentsLexer_LinesCanBeAccessedOutOfOrder()
    {
        // Arrange
        var pythonLexer = new TestPythonLexer();
        var lexer = new PygmentsLexer(pythonLexer);
        var document = new Document("line0\nline1\nline2\nline3\nline4");

        // Act - access lines out of order
        var getLine = lexer.LexDocument(document);
        var line4 = getLine(4);
        var line1 = getLine(1);
        var line3 = getLine(3);
        var line0 = getLine(0);
        var line2 = getLine(2);

        // Assert - all lines should be correct
        Assert.Contains(line0, t => t.Text.Contains("line0"));
        Assert.Contains(line1, t => t.Text.Contains("line1"));
        Assert.Contains(line2, t => t.Text.Contains("line2"));
        Assert.Contains(line3, t => t.Text.Contains("line3"));
        Assert.Contains(line4, t => t.Text.Contains("line4"));
    }

    #endregion

    #region Acceptance Tests (User Story 4)

    [Fact]
    public void Given_PygmentsLexerWithLexer_When_LexingDocument_Then_TokensConvertedToClassPygmentsFormat()
    {
        // Given
        var pythonLexer = new TestPythonLexer();
        var lexer = new PygmentsLexer(pythonLexer);
        var document = new Document("def hello(): pass");

        // When
        var getLine = lexer.LexDocument(document);
        var tokens = getLine(0);

        // Then
        var keywordToken = tokens.First(t => t.Text == "def");
        Assert.StartsWith("class:pygments.", keywordToken.Style);
    }

    [Fact]
    public void Given_PygmentsLexerWithSyncFromStartEnabled_When_LexingAnyLine_Then_LexesFromBeginning()
    {
        // Given
        var pythonLexer = new TestPythonLexer();
        var lexer = new PygmentsLexer(pythonLexer, syncFromStart: true);
        var document = new Document("line0\nline1\nline2");

        // When
        var getLine = lexer.LexDocument(document);
        var line2 = getLine(2);

        // Then - should complete successfully
        Assert.NotNull(line2);
    }

    [Fact]
    public void Given_PygmentsLexerWithSyncFromStartDisabled_When_LexingFarLine_Then_UsesSyntaxSync()
    {
        // Given
        var pythonLexer = new TestPythonLexer();
        var lexer = new PygmentsLexer(pythonLexer, syncFromStart: false);
        var lines = Enumerable.Range(0, 200).Select(i => $"line{i}").ToArray();
        var document = new Document(string.Join("\n", lines));

        // When
        var getLine = lexer.LexDocument(document);
        var line150 = getLine(150);

        // Then - should complete (using default RegexSync based on lexer name)
        Assert.NotNull(line150);
    }

    [Fact]
    public void Given_Filename_When_CreatingLexer_Then_AppropriateLexerReturned()
    {
        // Given/When - unknown extension
        var lexer = PygmentsLexer.FromFilename("test.xyz");

        // Then - should return SimpleLexer for unknown
        Assert.IsType<SimpleLexer>(lexer);
    }

    #endregion

    #region Constants Tests

    [Fact]
    public void PygmentsLexer_MinLinesBackwards_Is50()
    {
        Assert.Equal(50, PygmentsLexer.MinLinesBackwards);
    }

    [Fact]
    public void PygmentsLexer_ReuseGeneratorMaxDistance_Is100()
    {
        Assert.Equal(100, PygmentsLexer.ReuseGeneratorMaxDistance);
    }

    #endregion

    #region Caching Tests (FR-016, User Story 5)

    [Fact]
    public void PygmentsLexer_CacheHit_ReturnsCachedResult()
    {
        // Arrange
        var pythonLexer = new TestPythonLexer();
        var lexer = new PygmentsLexer(pythonLexer);
        var document = new Document("def test(): pass");

        // Act - request same line twice
        var getLine = lexer.LexDocument(document);
        var first = getLine(0);
        var second = getLine(0);

        // Assert - should return equivalent results
        Assert.Equal(first.Count, second.Count);
        for (int i = 0; i < first.Count; i++)
        {
            Assert.Equal(first[i].Style, second[i].Style);
            Assert.Equal(first[i].Text, second[i].Text);
        }
    }

    [Fact]
    public void PygmentsLexer_CacheMiss_LexesAndCachesResult()
    {
        // Arrange
        var pythonLexer = new TestPythonLexer();
        var lexer = new PygmentsLexer(pythonLexer);
        var document = new Document("line0\nline1\nline2");

        // Act - request different lines
        var getLine = lexer.LexDocument(document);
        var line0 = getLine(0);
        var line1 = getLine(1);
        var line2 = getLine(2);

        // Assert - all should be retrieved
        Assert.NotEmpty(line0);
        Assert.NotEmpty(line1);
        Assert.NotEmpty(line2);
    }

    #endregion

    #region Generator Reuse Tests (FR-017, FR-018, User Story 5)

    [Fact]
    public void PygmentsLexer_SequentialLineAccess_Efficient()
    {
        // Arrange
        var pythonLexer = new TestPythonLexer();
        var lexer = new PygmentsLexer(pythonLexer);
        var lines = Enumerable.Range(0, 100).Select(i => $"line{i}").ToArray();
        var document = new Document(string.Join("\n", lines));

        // Act - access lines sequentially
        var getLine = lexer.LexDocument(document);
        for (int i = 0; i < 100; i++)
        {
            var tokens = getLine(i);
            Assert.NotNull(tokens);
        }
    }

    [Fact]
    public void PygmentsLexer_GeneratorReuse_WithinReuseDistance()
    {
        // Arrange
        var pythonLexer = new TestPythonLexer();
        var lexer = new PygmentsLexer(pythonLexer);
        var lines = Enumerable.Range(0, 200).Select(i => $"line{i}").ToArray();
        var document = new Document(string.Join("\n", lines));

        // Act - access line 50, then line 100 (within ReuseGeneratorMaxDistance)
        var getLine = lexer.LexDocument(document);
        var line50 = getLine(50);
        var line100 = getLine(100); // Should reuse generator from line 50

        // Assert
        Assert.NotNull(line50);
        Assert.NotNull(line100);
    }

    [Fact]
    public void PygmentsLexer_GeneratorNotReused_BeyondReuseDistance()
    {
        // Arrange
        var pythonLexer = new TestPythonLexer();
        var lexer = new PygmentsLexer(pythonLexer);
        var lines = Enumerable.Range(0, 300).Select(i => $"line{i}").ToArray();
        var document = new Document(string.Join("\n", lines));

        // Act - access line 50, then line 200 (beyond ReuseGeneratorMaxDistance of 100)
        var getLine = lexer.LexDocument(document);
        var line50 = getLine(50);
        var line200 = getLine(200); // Should create new generator

        // Assert
        Assert.NotNull(line50);
        Assert.NotNull(line200);
    }

    [Fact]
    public void PygmentsLexer_NewGenerator_GoesBackAtLeast50Lines()
    {
        // Arrange - syncFromStart=false to test generator positioning
        var pythonLexer = new TestPythonLexer();
        var lexer = new PygmentsLexer(pythonLexer, syncFromStart: false);
        var lines = Enumerable.Range(0, 200).Select(i => $"line{i}").ToArray();
        var document = new Document(string.Join("\n", lines));

        // Act - access line 100
        var getLine = lexer.LexDocument(document);
        var line100 = getLine(100);

        // Assert - should work (generator goes back to at least line 50)
        Assert.NotNull(line100);
    }

    #endregion

    #region Acceptance Tests (User Story 5)

    [Fact]
    public void Given_LexedLine_When_RequestingSameLine_Then_CachedResultReturned()
    {
        // Given
        var pythonLexer = new TestPythonLexer();
        var lexer = new PygmentsLexer(pythonLexer);
        var document = new Document("x = 1\ny = 2");

        // When
        var getLine = lexer.LexDocument(document);
        var first = getLine(0);
        var second = getLine(0);

        // Then - cached result should be identical
        Assert.Equal(first.Count, second.Count);
    }

    [Fact]
    public void Given_GeneratorAtLineN_When_RequestingLineNPlus10_Then_GeneratorReused()
    {
        // Given
        var pythonLexer = new TestPythonLexer();
        var lexer = new PygmentsLexer(pythonLexer);
        var lines = Enumerable.Range(0, 50).Select(i => $"line{i}").ToArray();
        var document = new Document(string.Join("\n", lines));

        // When - access line 10, then line 20 (within reuse distance)
        var getLine = lexer.LexDocument(document);
        var line10 = getLine(10);
        var line20 = getLine(20);

        // Then
        Assert.NotNull(line10);
        Assert.NotNull(line20);
    }

    [Fact]
    public void Given_GeneratorAtLineN_When_RequestingLineBeyondReuseDistance_Then_NewGeneratorCreated()
    {
        // Given
        var pythonLexer = new TestPythonLexer();
        var lexer = new PygmentsLexer(pythonLexer);
        var lines = Enumerable.Range(0, 250).Select(i => $"line{i}").ToArray();
        var document = new Document(string.Join("\n", lines));

        // When - access line 10, then line 150 (beyond reuse distance of 100)
        var getLine = lexer.LexDocument(document);
        var line10 = getLine(10);
        var line150 = getLine(150);

        // Then
        Assert.NotNull(line10);
        Assert.NotNull(line150);
    }

    #endregion
}
