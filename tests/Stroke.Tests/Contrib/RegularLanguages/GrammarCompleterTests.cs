namespace Stroke.Tests.Contrib.RegularLanguages;

using Stroke.Contrib.RegularLanguages;
using Stroke.Core;
using Xunit;
using CompletionNs = Stroke.Completion;

/// <summary>
/// Tests for GrammarCompleter.
/// </summary>
public class GrammarCompleterTests
{
    [Fact]
    public void Constructor_NullGrammar_ThrowsArgumentNullException()
    {
        var completers = new Dictionary<string, CompletionNs.ICompleter>();

        Assert.Throws<ArgumentNullException>(() =>
            new GrammarCompleter(null!, completers));
    }

    [Fact]
    public void Constructor_NullCompleters_ThrowsArgumentNullException()
    {
        var grammar = Grammar.Compile(@"(?P<cmd>\w+)");

        Assert.Throws<ArgumentNullException>(() =>
            new GrammarCompleter(grammar, null!));
    }

    [Fact]
    public void GetCompletions_NoMatch_ReturnsEmpty()
    {
        var grammar = Grammar.Compile(@"cd\s(?P<dir>\w+)");
        var completers = new Dictionary<string, CompletionNs.ICompleter>
        {
            ["dir"] = new WordCompleter(["home", "tmp", "var"])
        };
        var completer = new GrammarCompleter(grammar, completers);
        var document = new Document("xyz", 3);

        var completions = completer.GetCompletions(document, new CompletionNs.CompleteEvent()).ToList();

        // With trailing input handling, empty prefix matches - but no variable at cursor
        Assert.Empty(completions);
    }

    [Fact]
    public void GetCompletions_MatchWithNoCompleterForVariable_ReturnsEmpty()
    {
        var grammar = Grammar.Compile(@"(?P<cmd>\w+)");
        var completers = new Dictionary<string, CompletionNs.ICompleter>();
        var completer = new GrammarCompleter(grammar, completers);
        var document = new Document("hello", 5);

        var completions = completer.GetCompletions(document, new CompletionNs.CompleteEvent()).ToList();

        Assert.Empty(completions);
    }

    [Fact]
    public void GetCompletions_DelegatestoVariableCompleter()
    {
        var grammar = Grammar.Compile(@"cd\s(?P<dir>\w*)");
        var completers = new Dictionary<string, CompletionNs.ICompleter>
        {
            ["dir"] = new WordCompleter(["home", "tmp", "var"])
        };
        var completer = new GrammarCompleter(grammar, completers);
        var document = new Document("cd ", 3);

        var completions = completer.GetCompletions(document, new CompletionNs.CompleteEvent()).ToList();

        Assert.Equal(3, completions.Count);
        Assert.Contains(completions, c => c.Text == "home");
        Assert.Contains(completions, c => c.Text == "tmp");
        Assert.Contains(completions, c => c.Text == "var");
    }

    [Fact]
    public void GetCompletions_WithPartialInput_FiltersCompletions()
    {
        var grammar = Grammar.Compile(@"cd\s(?P<dir>\w*)");
        var completers = new Dictionary<string, CompletionNs.ICompleter>
        {
            ["dir"] = new WordCompleter(["home", "tmp", "var"])
        };
        var completer = new GrammarCompleter(grammar, completers);
        var document = new Document("cd h", 4);

        var completions = completer.GetCompletions(document, new CompletionNs.CompleteEvent()).ToList();

        Assert.Single(completions);
        Assert.Equal("home", completions[0].Text);
    }

    [Fact]
    public void GetCompletions_AdjustsStartPosition()
    {
        var grammar = Grammar.Compile(@"cd\s(?P<dir>\w*)");
        var completers = new Dictionary<string, CompletionNs.ICompleter>
        {
            ["dir"] = new WordCompleter(["home", "tmp", "var"])
        };
        var completer = new GrammarCompleter(grammar, completers);
        var document = new Document("cd ho", 5);

        var completions = completer.GetCompletions(document, new CompletionNs.CompleteEvent()).ToList();

        Assert.Single(completions);
        // StartPosition should be relative to input, pointing to "ho" start
        Assert.Equal(-2, completions[0].StartPosition);
    }

    [Fact]
    public void GetCompletions_DeduplicatesByTextAndStartPosition()
    {
        // Grammar with ambiguous paths
        var grammar = Grammar.Compile(@"(a(?P<x>\w*)|a(?P<y>\w*))");
        var completers = new Dictionary<string, CompletionNs.ICompleter>
        {
            ["x"] = new WordCompleter(["bc"]),
            ["y"] = new WordCompleter(["bc"])
        };
        var completer = new GrammarCompleter(grammar, completers);
        var document = new Document("a", 1);

        var completions = completer.GetCompletions(document, new CompletionNs.CompleteEvent()).ToList();

        // Should deduplicate - both paths yield "bc" at same position
        Assert.Single(completions);
        Assert.Equal("bc", completions[0].Text);
    }

    [Fact]
    public void GetCompletions_WithEscapeFunction_AppliesEscape()
    {
        var escapeFuncs = new Dictionary<string, Func<string, string>>
        {
            ["path"] = s => $"\"{s}\""
        };
        var grammar = Grammar.Compile(@"(?P<path>.+)", escapeFuncs);
        var completers = new Dictionary<string, CompletionNs.ICompleter>
        {
            ["path"] = new WordCompleter(["test"])
        };
        var completer = new GrammarCompleter(grammar, completers);
        var document = new Document("", 0);

        var completions = completer.GetCompletions(document, new CompletionNs.CompleteEvent()).ToList();

        Assert.Single(completions);
        Assert.Equal("\"test\"", completions[0].Text);
    }

    [Fact]
    public void GetCompletions_WithUnescapeFunction_AppliesUnescape()
    {
        var unescapeFuncs = new Dictionary<string, Func<string, string>>
        {
            ["path"] = s => s.Replace(@"\ ", " ")
        };
        var escapeFuncs = new Dictionary<string, Func<string, string>>
        {
            ["path"] = s => s.Replace(" ", @"\ ")
        };
        var grammar = Grammar.Compile(@"(?P<path>.+)", escapeFuncs, unescapeFuncs);
        var completers = new Dictionary<string, CompletionNs.ICompleter>
        {
            ["path"] = new WordCompleter(["my file"])
        };
        var completer = new GrammarCompleter(grammar, completers);
        var document = new Document("", 0);

        var completions = completer.GetCompletions(document, new CompletionNs.CompleteEvent()).ToList();

        Assert.Single(completions);
        Assert.Equal(@"my\ file", completions[0].Text);
    }

    [Fact]
    public void GetCompletions_PreservesDisplayAndMeta()
    {
        var grammar = Grammar.Compile(@"(?P<cmd>\w*)");
        var completers = new Dictionary<string, CompletionNs.ICompleter>
        {
            ["cmd"] = new CompleterWithMeta()
        };
        var completer = new GrammarCompleter(grammar, completers);
        var document = new Document("", 0);

        var completions = completer.GetCompletions(document, new CompletionNs.CompleteEvent()).ToList();

        Assert.Single(completions);
        Assert.Equal("command", completions[0].Text);
        Assert.Equal("Display Text", completions[0].DisplayText);
        Assert.Equal("Meta Info", completions[0].DisplayMetaText);
    }

    [Fact]
    public async Task GetCompletionsAsync_ReturnsCompletions()
    {
        var grammar = Grammar.Compile(@"cd\s(?P<dir>\w*)");
        var completers = new Dictionary<string, CompletionNs.ICompleter>
        {
            ["dir"] = new WordCompleter(["home", "tmp"])
        };
        var completer = new GrammarCompleter(grammar, completers);
        var document = new Document("cd ", 3);

        var completions = new List<CompletionNs.Completion>();
        await foreach (var c in completer.GetCompletionsAsync(
            document, new CompletionNs.CompleteEvent(), TestContext.Current.CancellationToken))
        {
            completions.Add(c);
        }

        Assert.Equal(2, completions.Count);
    }

    [Fact]
    public async Task GetCompletionsAsync_RespectsCancellation()
    {
        var grammar = Grammar.Compile(@"(?P<cmd>\w*)");
        var completers = new Dictionary<string, CompletionNs.ICompleter>
        {
            ["cmd"] = new SlowCompleter()
        };
        var completer = new GrammarCompleter(grammar, completers);
        var document = new Document("", 0);

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
        {
#pragma warning disable xUnit1051 // Testing explicit cancellation behavior - using intentionally cancelled token
            await foreach (var _ in completer.GetCompletionsAsync(
                document, new CompletionNs.CompleteEvent(), cts.Token))
            {
            }
#pragma warning restore xUnit1051
        });
    }

    [Fact]
    public void GetCompletions_NullDocument_ThrowsArgumentNullException()
    {
        var grammar = Grammar.Compile(@"(?P<cmd>\w+)");
        var completer = new GrammarCompleter(grammar, new Dictionary<string, CompletionNs.ICompleter>());

        Assert.Throws<ArgumentNullException>(() =>
            completer.GetCompletions(null!, new CompletionNs.CompleteEvent()).ToList());
    }

    [Fact]
    public void GetCompletions_NullCompleteEvent_ThrowsArgumentNullException()
    {
        var grammar = Grammar.Compile(@"(?P<cmd>\w+)");
        var completer = new GrammarCompleter(grammar, new Dictionary<string, CompletionNs.ICompleter>());
        var document = new Document("test", 4);

        Assert.Throws<ArgumentNullException>(() =>
            completer.GetCompletions(document, null!).ToList());
    }

    /// <summary>
    /// Simple word completer for testing.
    /// </summary>
    private class WordCompleter : CompletionNs.ICompleter
    {
        private readonly IReadOnlyList<string> _words;

        public WordCompleter(IEnumerable<string> words)
        {
            _words = words.ToList();
        }

        public IEnumerable<CompletionNs.Completion> GetCompletions(Document document, CompletionNs.CompleteEvent completeEvent)
        {
            var text = document.TextBeforeCursor;
            foreach (var word in _words)
            {
                if (word.StartsWith(text, StringComparison.OrdinalIgnoreCase))
                {
                    yield return new CompletionNs.Completion(word, startPosition: -text.Length);
                }
            }
        }

        public async IAsyncEnumerable<CompletionNs.Completion> GetCompletionsAsync(
            Document document,
            CompletionNs.CompleteEvent completeEvent,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            foreach (var c in GetCompletions(document, completeEvent))
            {
                cancellationToken.ThrowIfCancellationRequested();
                await Task.Yield();
                yield return c;
            }
        }
    }

    /// <summary>
    /// Completer that returns completions with display and meta.
    /// </summary>
    private class CompleterWithMeta : CompletionNs.ICompleter
    {
        public IEnumerable<CompletionNs.Completion> GetCompletions(Document document, CompletionNs.CompleteEvent completeEvent)
        {
            yield return new CompletionNs.Completion(
                text: "command",
                startPosition: 0,
                display: "Display Text",
                displayMeta: "Meta Info");
        }

        public async IAsyncEnumerable<CompletionNs.Completion> GetCompletionsAsync(
            Document document,
            CompletionNs.CompleteEvent completeEvent,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            foreach (var c in GetCompletions(document, completeEvent))
            {
                await Task.Yield();
                yield return c;
            }
        }
    }

    /// <summary>
    /// Slow completer that yields after delay (for cancellation testing).
    /// </summary>
    private class SlowCompleter : CompletionNs.ICompleter
    {
        public IEnumerable<CompletionNs.Completion> GetCompletions(Document document, CompletionNs.CompleteEvent completeEvent)
        {
            yield return new CompletionNs.Completion("slow", 0);
        }

        public async IAsyncEnumerable<CompletionNs.Completion> GetCompletionsAsync(
            Document document,
            CompletionNs.CompleteEvent completeEvent,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await Task.Delay(1000, cancellationToken);
            yield return new CompletionNs.Completion("slow", 0);
        }
    }
}
