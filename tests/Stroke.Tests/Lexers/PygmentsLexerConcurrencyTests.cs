using Stroke.Core;
using Stroke.FormattedText;
using Stroke.Lexers;
using Xunit;

namespace Stroke.Tests.Lexers;

/// <summary>
/// Concurrency tests for <see cref="PygmentsLexer"/>.
/// </summary>
public sealed class PygmentsLexerConcurrencyTests
{
    [Fact]
    public async Task Concurrent_LexDocument_NoExceptions()
    {
        // Arrange
        var pythonLexer = new TestPythonLexer();
        var lexer = new PygmentsLexer(pythonLexer);
        var document = new Document("def test():\n    return 1\n\nx = test()");
        var exceptions = new List<Exception>();
        var lockObj = new object();

        // Act - 10 threads, 100 calls each
        var tasks = Enumerable.Range(0, 10).Select(_ => Task.Run(() =>
        {
            for (int i = 0; i < 100; i++)
            {
                try
                {
                    var getLine = lexer.LexDocument(document);
                    // Access some lines
                    var tokens = getLine(i % 4);
                    _ = tokens.Count; // Force evaluation
                }
                catch (Exception ex)
                {
                    lock (lockObj)
                    {
                        exceptions.Add(ex);
                    }
                }
            }
        })).ToArray();

        await Task.WhenAll(tasks);

        // Assert
        Assert.Empty(exceptions);
    }

    [Fact]
    public async Task Concurrent_LineAccess_ConsistentResults()
    {
        // Arrange
        var pythonLexer = new TestPythonLexer();
        var lexer = new PygmentsLexer(pythonLexer);
        var document = new Document("line0\nline1\nline2\nline3\nline4");
        var getLine = lexer.LexDocument(document);
        var results = new (int lineNo, IReadOnlyList<StyleAndTextTuple> tokens)[1000];
        var exceptions = new Exception?[1000];

        // Act - 1000 concurrent line requests
        var tasks = Enumerable.Range(0, 1000).Select(i => Task.Run(() =>
        {
            try
            {
                var lineNo = i % 5;
                results[i] = (lineNo, getLine(lineNo));
            }
            catch (Exception ex)
            {
                exceptions[i] = ex;
            }
        })).ToArray();

        await Task.WhenAll(tasks);

        // Assert
        for (int i = 0; i < 1000; i++)
        {
            Assert.Null(exceptions[i]);
            Assert.NotNull(results[i].tokens);
            // Verify content matches expected line
            var allText = string.Join("", results[i].tokens.Select(t => t.Text));
            Assert.Contains($"line{results[i].lineNo}", allText);
        }
    }

    [Fact]
    public async Task Concurrent_CacheAccess_NoCorruption()
    {
        // Arrange
        var pythonLexer = new TestPythonLexer();
        var lexer = new PygmentsLexer(pythonLexer);
        var document = new Document("def foo():\n    x = 1\n    return x");
        var getLine = lexer.LexDocument(document);
        var results = new Dictionary<int, List<IReadOnlyList<StyleAndTextTuple>>>();
        var lockObj = new object();

        for (int i = 0; i < 3; i++)
        {
            results[i] = [];
        }

        // Act - multiple threads accessing the same lines
        var tasks = Enumerable.Range(0, 50).Select(_ => Task.Run(() =>
        {
            for (int lineNo = 0; lineNo < 3; lineNo++)
            {
                var tokens = getLine(lineNo);
                lock (lockObj)
                {
                    results[lineNo].Add(tokens);
                }
            }
        })).ToArray();

        await Task.WhenAll(tasks);

        // Assert - all results for the same line should have same token count
        for (int lineNo = 0; lineNo < 3; lineNo++)
        {
            var allResults = results[lineNo];
            Assert.True(allResults.Count == 50, $"Expected 50 results for line {lineNo}");

            var referenceCount = allResults[0].Count;
            Assert.All(allResults, r => Assert.Equal(referenceCount, r.Count));

            // Verify all tokens have same content
            for (int tokenIndex = 0; tokenIndex < referenceCount; tokenIndex++)
            {
                var referenceStyle = allResults[0][tokenIndex].Style;
                var referenceText = allResults[0][tokenIndex].Text;
                Assert.All(allResults, r =>
                {
                    Assert.Equal(referenceStyle, r[tokenIndex].Style);
                    Assert.Equal(referenceText, r[tokenIndex].Text);
                });
            }
        }
    }

    [Fact]
    public async Task Concurrent_MultipleLexDocumentCalls_Isolated()
    {
        // Arrange
        var pythonLexer = new TestPythonLexer();
        var lexer = new PygmentsLexer(pythonLexer);
        var documents = new[]
        {
            new Document("doc1_line0\ndoc1_line1"),
            new Document("doc2_line0\ndoc2_line1"),
            new Document("doc3_line0\ndoc3_line1")
        };
        var exceptions = new List<Exception>();
        var lockObj = new object();

        // Act - concurrent LexDocument calls with different documents
        var tasks = Enumerable.Range(0, 30).Select(i => Task.Run(() =>
        {
            try
            {
                var docIndex = i % 3;
                var getLine = lexer.LexDocument(documents[docIndex]);

                for (int lineNo = 0; lineNo < 2; lineNo++)
                {
                    var tokens = getLine(lineNo);
                    var allText = string.Join("", tokens.Select(t => t.Text));
                    // Verify we got the right document's content
                    Assert.Contains($"doc{docIndex + 1}_line{lineNo}", allText);
                }
            }
            catch (Exception ex)
            {
                lock (lockObj)
                {
                    exceptions.Add(ex);
                }
            }
        })).ToArray();

        await Task.WhenAll(tasks);

        // Assert
        Assert.Empty(exceptions);
    }

    [Fact]
    public async Task Concurrent_RandomLineAccess_NoDeadlock()
    {
        // Arrange
        var pythonLexer = new TestPythonLexer();
        var lexer = new PygmentsLexer(pythonLexer);
        var lines = Enumerable.Range(0, 100).Select(i => $"line{i}").ToArray();
        var document = new Document(string.Join("\n", lines));
        var getLine = lexer.LexDocument(document);
        var random = new Random(42); // Fixed seed for reproducibility
        var exceptions = new List<Exception>();
        var lockObj = new object();
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        // Act - random line access from multiple threads
        var tasks = Enumerable.Range(0, 20).Select(_ => Task.Run(() =>
        {
            var localRandom = new Random(random.Next());
            while (!cts.Token.IsCancellationRequested)
            {
                try
                {
                    var lineNo = localRandom.Next(100);
                    var tokens = getLine(lineNo);
                    Assert.NotNull(tokens);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    lock (lockObj)
                    {
                        exceptions.Add(ex);
                    }
                    break;
                }
            }
        })).ToArray();

        // Wait a bit then cancel
        Thread.Sleep(500);
        cts.Cancel();

        try
        {
            await Task.WhenAll(tasks);
        }
        catch (OperationCanceledException)
        {
            // Expected
        }

        // Assert
        Assert.Empty(exceptions);
    }
}
