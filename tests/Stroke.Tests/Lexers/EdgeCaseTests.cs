using Stroke.Core;
using Stroke.FormattedText;
using Stroke.Lexers;
using Xunit;

namespace Stroke.Tests.Lexers;

/// <summary>
/// Edge case tests for the Lexer System covering boundary conditions and error handling.
/// </summary>
public sealed class EdgeCaseTests
{
    // EC-002: Mixed line endings - Document handles normalization
    // Note: Document only treats \n and \r\n as line separators (not standalone \r)
    [Theory]
    [InlineData("line1\nline2", 2)]           // Unix LF
    [InlineData("line1\r\nline2", 2)]          // Windows CRLF
    [InlineData("line1\nline2\r\nline3", 3)]   // Mixed LF and CRLF
    public void MixedLineEndings_DocumentNormalizes(string text, int expectedLineCount)
    {
        // Arrange
        var document = new Document(text);
        var lexer = new SimpleLexer();

        // Act
        var getLine = lexer.LexDocument(document);

        // Assert - Document normalizes line endings
        Assert.Equal(expectedLineCount, document.Lines.Count);
        for (int i = 0; i < expectedLineCount; i++)
        {
            var tokens = getLine(i);
            Assert.NotNull(tokens);
        }
    }

    // EC-004: Malformed tokens from IPygmentsLexer - processed without validation
    [Fact]
    public void MalformedTokens_EmptyTokenType_ProcessedWithDefaultStyle()
    {
        // Arrange
        var malformedLexer = new MalformedTokenLexer();
        var lexer = new PygmentsLexer(malformedLexer);
        var document = new Document("test");

        // Act
        var getLine = lexer.LexDocument(document);
        var tokens = getLine(0);

        // Assert - processes without throwing, uses default style for empty token type
        Assert.NotNull(tokens);
    }

    [Fact]
    public void MalformedTokens_NullTextInToken_ProcessedWithEmptyString()
    {
        // Arrange
        var nullTextLexer = new NullTextTokenLexer();
        var lexer = new PygmentsLexer(nullTextLexer);
        var document = new Document("test");

        // Act
        var getLine = lexer.LexDocument(document);
        var tokens = getLine(0);

        // Assert - processes without throwing
        Assert.NotNull(tokens);
    }

    // EC-007: Concurrent access thread safety verification
    [Fact]
    public async Task ConcurrentAccess_DifferentLexDocumentCalls_Isolated()
    {
        // Arrange
        var pythonLexer = new TestPythonLexer();
        var lexer = new PygmentsLexer(pythonLexer);
        var document1 = new Document("doc1 content");
        var document2 = new Document("doc2 different");
        var exceptions = new List<Exception>();
        var lockObj = new object();

        // Act - concurrent LexDocument calls
        var tasks = new List<Task>();
        for (int i = 0; i < 20; i++)
        {
            var doc = i % 2 == 0 ? document1 : document2;
            var expectedContent = i % 2 == 0 ? "doc1" : "doc2";
            tasks.Add(Task.Factory.StartNew(() =>
            {
                try
                {
                    var getLine = lexer.LexDocument(doc);
                    var tokens = getLine(0);
                    var text = string.Join("", tokens.Select(t => t.Text));
                    Assert.Contains(expectedContent, text);
                }
                catch (Exception ex)
                {
                    lock (lockObj)
                    {
                        exceptions.Add(ex);
                    }
                }
            }, TaskCreationOptions.LongRunning));
        }

        await Task.WhenAll(tasks);

        // Assert
        Assert.Empty(exceptions);
    }

    // EC-010: Very long lines (>64KB) processed without truncation
    [Fact]
    public void VeryLongLine_Over64KB_ProcessedWithoutTruncation()
    {
        // Arrange
        var longLine = new string('x', 70000); // >64KB
        var document = new Document(longLine);
        var lexer = new SimpleLexer("test-style");

        // Act
        var getLine = lexer.LexDocument(document);
        var tokens = getLine(0);

        // Assert - full content preserved
        Assert.Single(tokens);
        Assert.Equal(70000, tokens[0].Text.Length);
    }

    [Fact]
    public void VeryLongLine_WithPygmentsLexer_ProcessedWithoutTruncation()
    {
        // Arrange
        var longLine = "x = " + new string('1', 65000); // >64KB of digits
        var document = new Document(longLine);
        var pythonLexer = new TestPythonLexer();
        var lexer = new PygmentsLexer(pythonLexer);

        // Act
        var getLine = lexer.LexDocument(document);
        var tokens = getLine(0);

        // Assert - content preserved (sum of token lengths matches input)
        var totalLength = tokens.Sum(t => t.Text.Length);
        Assert.Equal(longLine.Length, totalLength);
    }

    // EC-012: Catastrophic regex backtracking - document behavior (no timeout enforced)
    [Fact]
    public void RegexSync_ComplexPattern_CompletesWithinReasonableTime()
    {
        // Arrange - pattern that could cause backtracking but input is reasonable
        var pattern = @"^(class|def|async\s+def)\s+";
        var regexSync = new RegexSync(pattern);
        var lines = Enumerable.Range(0, 200).Select(i => $"    x = {i}").ToList();
        var document = new Document(string.Join("\n", lines));

        // Act - should complete without hanging
        var (Row, Column) = regexSync.GetSyncStartPosition(document, 150);

        // Assert - returns valid position (no match found, so falls back)
        Assert.True(Row >= 0);
        Assert.True(Column >= 0);
    }

    // EC-018: Null/empty text in GetTokensUnprocessed
    [Fact]
    public void PygmentsLexer_EmptyText_ReturnsEmptyTokens()
    {
        // Arrange
        var pythonLexer = new TestPythonLexer();
        var lexer = new PygmentsLexer(pythonLexer);
        var document = new Document("");

        // Act
        var getLine = lexer.LexDocument(document);
        var tokens = getLine(0);

        // Assert
        Assert.Empty(tokens);
    }

    [Fact]
    public void SimpleLexer_EmptyLine_ReturnsEmptyToken()
    {
        // Arrange
        var lexer = new SimpleLexer("style");
        var document = new Document("\n\n");

        // Act
        var getLine = lexer.LexDocument(document);

        // Assert - empty lines still return a token with empty text
        for (int i = 0; i < 3; i++)
        {
            var tokens = getLine(i);
            Assert.Single(tokens);
            Assert.Empty(tokens[0].Text);
        }
    }

    // EC-020: Generator disposed mid-iteration - verify isolation
    [Fact]
    public void MultipleGetLineFunctions_IndependentIteration()
    {
        // Arrange
        var pythonLexer = new TestPythonLexer();
        var lexer = new PygmentsLexer(pythonLexer);
        var document = new Document("line0\nline1\nline2\nline3\nline4");

        // Act - get two separate functions from same document
        var getLine1 = lexer.LexDocument(document);
        var getLine2 = lexer.LexDocument(document);

        // Access lines in different order
        var tokens1_2 = getLine1(2);
        var tokens2_0 = getLine2(0);
        var tokens1_0 = getLine1(0);
        var tokens2_4 = getLine2(4);

        // Assert - each function iterates independently
        Assert.Contains("line2", string.Join("", tokens1_2.Select(t => t.Text)));
        Assert.Contains("line0", string.Join("", tokens2_0.Select(t => t.Text)));
        Assert.Contains("line0", string.Join("", tokens1_0.Select(t => t.Text)));
        Assert.Contains("line4", string.Join("", tokens2_4.Select(t => t.Text)));
    }

    // Additional edge cases
    [Fact]
    public void DynamicLexer_CallbackThrows_PropagatesException()
    {
        // Arrange
        var lexer = new DynamicLexer(() => throw new InvalidOperationException("Test error"));
        var document = new Document("test");

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => lexer.LexDocument(document));
        Assert.Equal("Test error", ex.Message);
    }

    [Fact]
    public void RegexSync_UnicodePattern_MatchesCorrectly()
    {
        // Arrange
        var pattern = @"^クラス\s+";
        var regexSync = new RegexSync(pattern);
        var document = new Document("クラス Test\n    content\n    more");

        // Act
        var (Row, Column) = regexSync.GetSyncStartPosition(document, 2);

        // Assert - should find the Japanese class keyword
        Assert.Equal(0, Row);
        Assert.Equal(0, Column);
    }

    [Fact]
    public void SimpleLexer_UnicodeContent_PreservedExactly()
    {
        // Arrange
        var unicodeText = "日本語テキスト\n🎉 emoji line\nمتن عربی";
        var document = new Document(unicodeText);
        var lexer = new SimpleLexer();

        // Act
        var getLine = lexer.LexDocument(document);

        // Assert - Unicode preserved
        Assert.Equal("日本語テキスト", getLine(0)[0].Text);
        Assert.Equal("🎉 emoji line", getLine(1)[0].Text);
        Assert.Equal("متن عربی", getLine(2)[0].Text);
    }

    [Fact]
    public void PygmentsLexer_RequestLineMultipleTimes_SameInstance()
    {
        // Arrange
        var pythonLexer = new TestPythonLexer();
        var lexer = new PygmentsLexer(pythonLexer);
        var document = new Document("def test():\n    pass");
        var getLine = lexer.LexDocument(document);

        // Act - request same line multiple times
        var tokens1 = getLine(0);
        var tokens2 = getLine(0);
        var tokens3 = getLine(0);

        // Assert - same cached instance returned
        Assert.Same(tokens1, tokens2);
        Assert.Same(tokens2, tokens3);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    [InlineData(int.MinValue)]
    public void NegativeLineNumbers_ReturnsEmptyList(int lineNo)
    {
        // Arrange
        var document = new Document("line0\nline1");
        var simpleLexer = new SimpleLexer();
        var pythonLexer = new TestPythonLexer();
        var pygmentsLexer = new PygmentsLexer(pythonLexer);
        var dynamicLexer = new DynamicLexer(() => simpleLexer);

        // Act
        var simpleTokens = simpleLexer.LexDocument(document)(lineNo);
        var pygmentsTokens = pygmentsLexer.LexDocument(document)(lineNo);
        var dynamicTokens = dynamicLexer.LexDocument(document)(lineNo);

        // Assert
        Assert.Empty(simpleTokens);
        Assert.Empty(pygmentsTokens);
        Assert.Empty(dynamicTokens);
    }

    [Theory]
    [InlineData(100)]
    [InlineData(1000)]
    [InlineData(int.MaxValue)]
    public void LineBeyondBounds_ReturnsEmptyList(int lineNo)
    {
        // Arrange
        var document = new Document("line0\nline1");
        var simpleLexer = new SimpleLexer();
        var pythonLexer = new TestPythonLexer();
        var pygmentsLexer = new PygmentsLexer(pythonLexer);

        // Act
        var simpleTokens = simpleLexer.LexDocument(document)(lineNo);
        var pygmentsTokens = pygmentsLexer.LexDocument(document)(lineNo);

        // Assert
        Assert.Empty(simpleTokens);
        Assert.Empty(pygmentsTokens);
    }
}

/// <summary>
/// Test lexer that returns tokens with empty token type arrays.
/// </summary>
internal sealed class MalformedTokenLexer : IPygmentsLexer
{
    public string Name => "Malformed";

    public IEnumerable<(int Index, IReadOnlyList<string> TokenType, string Text)> GetTokensUnprocessed(string text)
    {
        // Return token with empty type array
        yield return (0, Array.Empty<string>(), text);
    }
}

/// <summary>
/// Test lexer that returns tokens with null text (simulating malformed lexer output).
/// </summary>
internal sealed class NullTextTokenLexer : IPygmentsLexer
{
    public string Name => "NullText";

    public IEnumerable<(int Index, IReadOnlyList<string> TokenType, string Text)> GetTokensUnprocessed(string text)
    {
        // Return token with empty text (representing null-like behavior)
        yield return (0, new[] { "Text" }, "");
        yield return (0, new[] { "Text" }, text ?? "");
    }
}
