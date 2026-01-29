using Stroke.Core;
using Stroke.FormattedText;
using Stroke.Lexers;
using Xunit;

namespace Stroke.Tests.Lexers;

/// <summary>
/// Concurrency tests for <see cref="DynamicLexer"/>.
/// </summary>
public sealed class DynamicLexerConcurrencyTests
{
    [Fact]
    public async Task Concurrent_CallbackInvocation_Safe()
    {
        // Arrange
        var invocationCount = 0;
        var innerLexer = new SimpleLexer("class:concurrent");
        var dynamicLexer = new DynamicLexer(() =>
        {
            Interlocked.Increment(ref invocationCount);
            return innerLexer;
        });
        var document = new Document("test document content");
        var exceptions = new List<Exception>();
        var results = new List<IReadOnlyList<StyleAndTextTuple>>();
        var lockObj = new object();

        // Act - 10 threads, 100 calls each
        var tasks = Enumerable.Range(0, 10).Select(_ => Task.Run(() =>
        {
            for (int i = 0; i < 100; i++)
            {
                try
                {
                    var getLine = dynamicLexer.LexDocument(document);
                    var line = getLine(0);
                    lock (lockObj)
                    {
                        results.Add(line);
                    }
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
        Assert.Equal(1000, results.Count);
        Assert.All(results, r =>
        {
            Assert.Single(r);
            Assert.Equal("class:concurrent", r[0].Style);
        });
        Assert.True(invocationCount >= 1000, $"Callback should have been invoked at least 1000 times, but was invoked {invocationCount} times");
    }

    [Fact]
    public async Task Concurrent_ReturnedFunction_ThreadSafe()
    {
        // Arrange
        var innerLexer = new SimpleLexer("class:test");
        var dynamicLexer = new DynamicLexer(() => innerLexer);
        var document = new Document("line0\nline1\nline2\nline3\nline4");
        var getLine = dynamicLexer.LexDocument(document);
        var results = new IReadOnlyList<StyleAndTextTuple>[500];
        var exceptions = new Exception?[500];

        // Act - concurrent access to the same returned function
        var tasks = Enumerable.Range(0, 500).Select(i => Task.Run(() =>
        {
            try
            {
                results[i] = getLine(i % 5);
            }
            catch (Exception ex)
            {
                exceptions[i] = ex;
            }
        })).ToArray();

        await Task.WhenAll(tasks);

        // Assert
        for (int i = 0; i < 500; i++)
        {
            Assert.Null(exceptions[i]);
            Assert.Single(results[i]);
            Assert.Equal("class:test", results[i][0].Style);
            Assert.Equal($"line{i % 5}", results[i][0].Text);
        }
    }

    [Fact]
    public async Task Concurrent_LexerSwitching_Safe()
    {
        // Arrange - callback returns different lexers based on a volatile flag
        var useSecondLexer = false;
        var lexer1 = new SimpleLexer("class:first");
        var lexer2 = new SimpleLexer("class:second");
        var dynamicLexer = new DynamicLexer(() => Volatile.Read(ref useSecondLexer) ? lexer2 : lexer1);
        var document = new Document("test");
        var exceptions = new List<Exception>();
        var lockObj = new object();
        var readerComplete = false;

        // Act - switch lexers while concurrent calls are happening
        var readerTask = Task.Factory.StartNew(() =>
        {
            for (int i = 0; i < 100; i++)
            {
                try
                {
                    var getLine = dynamicLexer.LexDocument(document);
                    var line = getLine(0);
                    // Result should be from either lexer1 or lexer2
                    if (line[0].Style != "class:first" && line[0].Style != "class:second")
                    {
                        throw new InvalidOperationException($"Unexpected style: {line[0].Style}");
                    }
                }
                catch (Exception ex)
                {
                    lock (lockObj) { exceptions.Add(ex); }
                }
            }
            Volatile.Write(ref readerComplete, true);
        }, TaskCreationOptions.LongRunning);

        var switcherTask = Task.Factory.StartNew(() =>
        {
            for (int i = 0; i < 50 && !Volatile.Read(ref readerComplete); i++)
            {
                Volatile.Write(ref useSecondLexer, !Volatile.Read(ref useSecondLexer));
                Thread.SpinWait(1000);
            }
        }, TaskCreationOptions.LongRunning);

        await Task.WhenAll(readerTask, switcherTask);

        // Assert
        Assert.Empty(exceptions);
    }

    [Fact]
    public async Task Concurrent_InvalidationHash_Safe()
    {
        // Arrange
        var innerLexer = new SimpleLexer("class:hash");
        var dynamicLexer = new DynamicLexer(() => innerLexer);
        var hashes = new object[100];
        var exceptions = new Exception?[100];

        // Act
        var tasks = Enumerable.Range(0, 100).Select(i => Task.Run(() =>
        {
            try
            {
                hashes[i] = dynamicLexer.InvalidationHash();
            }
            catch (Exception ex)
            {
                exceptions[i] = ex;
            }
        })).ToArray();

        await Task.WhenAll(tasks);

        // Assert
        Assert.All(exceptions, ex => Assert.Null(ex));
        Assert.All(hashes, h => Assert.Same(innerLexer, h)); // All hashes should be the same instance
    }

    [Fact]
    public async Task Concurrent_NullCallback_FallbackThreadSafe()
    {
        // Arrange - callback always returns null
        var dynamicLexer = new DynamicLexer(() => null);
        var document = new Document("fallback test");
        var results = new IReadOnlyList<StyleAndTextTuple>[100];
        var exceptions = new Exception?[100];

        // Act
        var tasks = Enumerable.Range(0, 100).Select(i => Task.Run(() =>
        {
            try
            {
                var getLine = dynamicLexer.LexDocument(document);
                results[i] = getLine(0);
            }
            catch (Exception ex)
            {
                exceptions[i] = ex;
            }
        })).ToArray();

        await Task.WhenAll(tasks);

        // Assert
        for (int i = 0; i < 100; i++)
        {
            Assert.Null(exceptions[i]);
            Assert.Single(results[i]);
            Assert.Equal("", results[i][0].Style); // Fallback SimpleLexer
            Assert.Equal("fallback test", results[i][0].Text);
        }
    }
}
