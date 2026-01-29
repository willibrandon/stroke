using System.Text.RegularExpressions;
using Stroke.Core;
using Stroke.Filters;
using Stroke.FormattedText;
using Stroke.Lexers;
using Xunit;

namespace Stroke.Tests.Lexers;

/// <summary>
/// Tests validating all code examples from quickstart.md compile and work correctly.
/// </summary>
public sealed class QuickstartValidationTests
{
    // Example: Display Plain Text
    [Fact]
    public void Quickstart_DisplayPlainText()
    {
        // SimpleLexer applies one style to all text
        var lexer = new SimpleLexer("class:input");
        var document = new Document("Hello, World!");

        var getLine = lexer.LexDocument(document);
        var tokens = getLine(0);
        // Result: [("class:input", "Hello, World!")]

        Assert.Single(tokens);
        Assert.Equal("class:input", tokens[0].Style);
        Assert.Equal("Hello, World!", tokens[0].Text);
    }

    // Example: Switch Lexers at Runtime
    [Fact]
    public void Quickstart_SwitchLexersAtRuntime()
    {
        var document = new Document("test content");

        // Track current lexer (e.g., based on file type)
        ILexer? activeLexer = null;

        var dynamicLexer = new DynamicLexer(() => activeLexer);

        // When file type changes
        activeLexer = GetLexerForExtension(".py");

        // DynamicLexer delegates to activeLexer
        var getLine = dynamicLexer.LexDocument(document);

        Assert.NotNull(getLine);
        var tokens = getLine(0);
        Assert.NotEmpty(tokens);
    }

    // Example: Syntax Highlighting with Caching
    [Fact]
    public void Quickstart_SyntaxHighlightingWithCaching()
    {
        // Create a PygmentsLexer with IPygmentsLexer implementation
        IPygmentsLexer pythonLexer = new TestPythonLexer();
        var lexer = new PygmentsLexer(pythonLexer);

        var document = new Document("""
            def greet(name):
                return f"Hello, {name}!"

            greet("World")
            """);

        var getLine = lexer.LexDocument(document);

        // Line 0: def greet(name):
        var line0 = getLine(0);
        Assert.NotEmpty(line0);

        // Cached access - instant
        var line0Again = getLine(0);
        Assert.Same(line0, line0Again);
    }

    // Example: Optimize Large Documents
    [Fact]
    public void Quickstart_OptimizeLargeDocuments()
    {
        var pythonLexer = new TestPythonLexer();

        // For large documents, disable sync from start
        var lexer = new PygmentsLexer(
            pythonLexer,
            syncFromStart: false,  // Don't lex entire document
            syntaxSync: new RegexSync(@"^\s*(class|def)\s+")  // Find safe start points
        );

        var largeLines = Enumerable.Range(0, 6000).Select(i => $"line{i}");
        var largeDoc = new Document(string.Join("\n", largeLines));
        var getLine = lexer.LexDocument(largeDoc);

        // Line 5000 doesn't re-lex from line 0
        // RegexSync finds nearby function/class definition to start from
        var line5000 = getLine(5000);
        Assert.NotEmpty(line5000);
    }

    // Example: Implement Custom IPygmentsLexer
    [Fact]
    public void Quickstart_CustomIPygmentsLexer()
    {
        var lexer = new SimpleKeywordLexer();
        var tokens = lexer.GetTokensUnprocessed("if x return y").ToList();

        Assert.NotEmpty(tokens);
        // Verify "if" is a keyword
        var ifToken = tokens.First(t => t.Text == "if");
        Assert.Contains("Keyword", ifToken.TokenType);
    }

    // Example: Cache Invalidation Check
    [Fact]
    public void Quickstart_CacheInvalidationCheck()
    {
        var document = new Document("test");
        ILexer? currentLexer = new SimpleLexer("style1");

        var dynamicLexer = new DynamicLexer(() => currentLexer);
        var previousHash = dynamicLexer.InvalidationHash();

        // ... user changes file type ...
        currentLexer = new SimpleLexer("style2");

        var currentHash = dynamicLexer.InvalidationHash();
        Assert.NotEqual(previousHash, currentHash);

        // Re-lex the document
        var newGetLine = dynamicLexer.LexDocument(document);
        Assert.NotNull(newGetLine);
    }

    // Example: Conditional Sync Behavior
    [Fact]
    public void Quickstart_ConditionalSyncBehavior()
    {
        var pythonLexer = new TestPythonLexer();
        var document = new Document("test content");

        // Sync from start for small documents, use RegexSync for large ones
        var smallDocThreshold = 1000;

        var lexer = new PygmentsLexer(
            pythonLexer,
            syncFromStart: new Condition(() => document.Lines.Count < smallDocThreshold)
        );

        var getLine = lexer.LexDocument(document);
        Assert.NotNull(getLine);
    }

    // Example: Fallback for Unknown File Types
    [Fact]
    public void Quickstart_FallbackForUnknownFileTypes()
    {
        // FromFilename returns SimpleLexer for unknown types
        ILexer lexer = PygmentsLexer.FromFilename("unknown.xyz");
        // Returns SimpleLexer (no syntax highlighting)

        Assert.IsType<SimpleLexer>(lexer);
    }

    // Example: API Reference - ILexer interface
    [Fact]
    public void Quickstart_ILexerInterface()
    {
        ILexer lexer = new SimpleLexer();
        var document = new Document("test");

        Func<int, IReadOnlyList<StyleAndTextTuple>> getLine = lexer.LexDocument(document);
        object hash = lexer.InvalidationHash();

        Assert.NotNull(getLine);
        Assert.NotNull(hash);
    }

    // Example: API Reference - SimpleLexer
    [Fact]
    public void Quickstart_SimpleLexerApi()
    {
        var lexer = new SimpleLexer("my-style");
        Assert.Equal("my-style", lexer.Style);
    }

    // Example: API Reference - DynamicLexer
    [Fact]
    public void Quickstart_DynamicLexerApi()
    {
        var lexer = new DynamicLexer(() => new SimpleLexer());
        Assert.NotNull(lexer);
    }

    // Example: API Reference - PygmentsLexer
    [Fact]
    public void Quickstart_PygmentsLexerApi()
    {
        Assert.Equal(50, PygmentsLexer.MinLinesBackwards);
        Assert.Equal(100, PygmentsLexer.ReuseGeneratorMaxDistance);

        var pythonLexer = new TestPythonLexer();
        var lexer = new PygmentsLexer(
            pythonLexer,
            syncFromStart: default,
            syntaxSync: null);

        Assert.NotNull(lexer);

        ILexer fallback = PygmentsLexer.FromFilename("test.xyz", default);
        Assert.IsType<SimpleLexer>(fallback);
    }

    // Example: API Reference - ISyntaxSync
    [Fact]
    public void Quickstart_ISyntaxSyncApi()
    {
        ISyntaxSync sync = SyncFromStart.Instance;
        var document = new Document("line0\nline1\nline2");
        var position = sync.GetSyncStartPosition(document, 2);

        Assert.Equal((0, 0), position);
    }

    // Example: API Reference - SyncFromStart
    [Fact]
    public void Quickstart_SyncFromStartApi()
    {
        SyncFromStart instance = SyncFromStart.Instance;
        Assert.NotNull(instance);
    }

    // Example: API Reference - RegexSync
    [Fact]
    public void Quickstart_RegexSyncApi()
    {
        Assert.Equal(500, RegexSync.MaxBackwards);
        Assert.Equal(100, RegexSync.FromStartIfNoSyncPosFound);

        var sync = new RegexSync(@"^\s*def\s+");
        Assert.NotNull(sync);

        var forPython = RegexSync.ForLanguage("Python");
        Assert.NotNull(forPython);
    }

    // Example: API Reference - IPygmentsLexer
    [Fact]
    public void Quickstart_IPygmentsLexerApi()
    {
        IPygmentsLexer lexer = new TestPythonLexer();
        string name = lexer.Name;
        IEnumerable<(int Index, IReadOnlyList<string> TokenType, string Text)> tokens =
            lexer.GetTokensUnprocessed("test");

        Assert.NotNull(name);
        Assert.NotNull(tokens);
    }

    // Helper method for example
    private static ILexer GetLexerForExtension(string extension)
    {
        // Simplified for test - return a SimpleLexer
        return new SimpleLexer($"class:{extension.TrimStart('.')}");
    }
}

/// <summary>
/// SimpleKeywordLexer from quickstart.md example.
/// </summary>
public partial class SimpleKeywordLexer : IPygmentsLexer
{
    private static readonly HashSet<string> Keywords =
        ["if", "else", "while", "for", "return"];

    public string Name => "SimpleKeyword";

    public IEnumerable<(int Index, IReadOnlyList<string> TokenType, string Text)>
        GetTokensUnprocessed(string text)
    {
        int index = 0;
        foreach (Match match in MyRegex().Matches(text))
        {
            string[] tokenType = Keywords.Contains(match.Value)
                ? ["Keyword"]
                : char.IsLetter(match.Value[0])
                    ? ["Name"]
                    : char.IsWhiteSpace(match.Value[0])
                        ? ["Text"]
                        : ["Punctuation"];

            yield return (index, tokenType, match.Value);
            index += match.Value.Length;
        }
    }

    [GeneratedRegex(@"\w+|\s+|.")]
    private static partial Regex MyRegex();
}
