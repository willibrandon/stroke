namespace Stroke.Tests.Contrib.RegularLanguages;

using Stroke.Contrib.RegularLanguages;
using Xunit;

/// <summary>
/// Tests for Grammar.Compile and CompiledGrammar.
/// </summary>
public class GrammarCompileTests
{
    [Fact]
    public void Compile_SimplePattern_ReturnsGrammar()
    {
        var grammar = Grammar.Compile(@"hello");

        Assert.NotNull(grammar);
    }

    [Fact]
    public void Compile_NullExpression_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => Grammar.Compile(null!));
    }

    [Fact]
    public void Compile_WithNamedGroup_ReturnsGrammar()
    {
        var grammar = Grammar.Compile(@"(?P<name>\w+)");

        Assert.NotNull(grammar);
    }

    [Fact]
    public void Compile_ComplexPattern_ReturnsGrammar()
    {
        var grammar = Grammar.Compile(@"
            \s*
            (
                pwd |
                ls |
                (cd \s+ (?P<directory>[^\s]+)) |
                (cat \s+ (?P<filename>[^\s]+))
            )
            \s*
        ");

        Assert.NotNull(grammar);
    }

    [Fact]
    public void Compile_WithEscapeFuncs_UsesEscapeFuncs()
    {
        var escapeFuncs = new Dictionary<string, Func<string, string>>
        {
            ["path"] = s => s.Replace(" ", @"\ ")
        };

        var grammar = Grammar.Compile(@"(?P<path>.+)", escapeFuncs);

        Assert.Equal(@"hello\ world", grammar.Escape("path", "hello world"));
    }

    [Fact]
    public void Compile_WithUnescapeFuncs_UsesUnescapeFuncs()
    {
        var unescapeFuncs = new Dictionary<string, Func<string, string>>
        {
            ["path"] = s => s.Replace(@"\ ", " ")
        };

        var grammar = Grammar.Compile(@"(?P<path>.+)", null, unescapeFuncs);

        Assert.Equal("hello world", grammar.Unescape("path", @"hello\ world"));
    }
}

public class CompiledGrammarMatchTests
{
    [Fact]
    public void Match_ExactMatch_ReturnsMatch()
    {
        var grammar = Grammar.Compile(@"hello");
        var match = grammar.Match("hello");

        Assert.NotNull(match);
    }

    [Fact]
    public void Match_NoMatch_ReturnsNull()
    {
        var grammar = Grammar.Compile(@"hello");
        var match = grammar.Match("world");

        Assert.Null(match);
    }

    [Fact]
    public void Match_PartialInput_ReturnsNull()
    {
        var grammar = Grammar.Compile(@"hello world");
        var match = grammar.Match("hello");

        Assert.Null(match);
    }

    [Fact]
    public void Match_ExtraInput_ReturnsNull()
    {
        var grammar = Grammar.Compile(@"hello");
        var match = grammar.Match("hello world");

        Assert.Null(match);
    }

    [Fact]
    public void Match_WithVariable_ExtractsValue()
    {
        var grammar = Grammar.Compile(@"hello\s(?P<name>\w+)");
        var match = grammar.Match("hello world");

        Assert.NotNull(match);
        var vars = match.Variables();
        Assert.Equal("world", vars["name"]);
    }

    [Fact]
    public void Match_MultipleVariables_ExtractsAll()
    {
        var grammar = Grammar.Compile(@"(?P<a>\d+)\+(?P<b>\d+)");
        var match = grammar.Match("12+34");

        Assert.NotNull(match);
        var vars = match.Variables();
        Assert.Equal("12", vars["a"]);
        Assert.Equal("34", vars["b"]);
    }

    [Fact]
    public void Match_NullInput_ThrowsArgumentNullException()
    {
        var grammar = Grammar.Compile(@"hello");

        Assert.Throws<ArgumentNullException>(() => grammar.Match(null!));
    }
}

public class CompiledGrammarMatchPrefixTests
{
    [Fact]
    public void MatchPrefix_FullMatch_ReturnsMatch()
    {
        var grammar = Grammar.Compile(@"hello");
        var match = grammar.MatchPrefix("hello");

        Assert.NotNull(match);
    }

    [Fact]
    public void MatchPrefix_PartialMatch_ReturnsMatch()
    {
        var grammar = Grammar.Compile(@"hello world");
        var match = grammar.MatchPrefix("hello ");

        Assert.NotNull(match);
    }

    [Fact]
    public void MatchPrefix_NoMatch_ReturnsMatchWithTrailing()
    {
        // Prefix matching always succeeds since empty string is valid prefix
        // Non-matching input becomes trailing input
        var grammar = Grammar.Compile(@"hello");
        var match = grammar.MatchPrefix("xyz");

        Assert.NotNull(match);
        var trailing = match.TrailingInput();
        Assert.NotNull(trailing);
        Assert.Equal("xyz", trailing.Value);
    }

    [Fact]
    public void MatchPrefix_EmptyInput_ReturnsMatch()
    {
        var grammar = Grammar.Compile(@"hello");
        var match = grammar.MatchPrefix("");

        Assert.NotNull(match);
    }

    [Fact]
    public void MatchPrefix_WithVariable_ExtractsPartialValue()
    {
        var grammar = Grammar.Compile(@"(?P<cmd>\w+)\s+(?P<arg>\w+)");
        var match = grammar.MatchPrefix("hello wor");

        Assert.NotNull(match);
        var vars = match.Variables();
        Assert.Equal("hello", vars["cmd"]);
        Assert.Equal("wor", vars["arg"]);
    }

    [Fact]
    public void MatchPrefix_NullInput_ThrowsArgumentNullException()
    {
        var grammar = Grammar.Compile(@"hello");

        Assert.Throws<ArgumentNullException>(() => grammar.MatchPrefix(null!));
    }

    [Fact]
    public void MatchPrefix_AlternativePaths_MatchesAll()
    {
        var grammar = Grammar.Compile(@"(cd\s(?P<dir>.+)|ls)");

        var match1 = grammar.MatchPrefix("cd /tmp");
        Assert.NotNull(match1);
        Assert.Equal("/tmp", match1.Variables()["dir"]);

        var match2 = grammar.MatchPrefix("ls");
        Assert.NotNull(match2);
    }
}

public class CompiledGrammarEscapeUnescapeTests
{
    [Fact]
    public void Escape_WithFunction_AppliesFunction()
    {
        var escapeFuncs = new Dictionary<string, Func<string, string>>
        {
            ["path"] = s => $"\"{s}\""
        };
        var grammar = Grammar.Compile(@"(?P<path>.+)", escapeFuncs);

        Assert.Equal("\"test\"", grammar.Escape("path", "test"));
    }

    [Fact]
    public void Escape_WithoutFunction_ReturnsOriginal()
    {
        var grammar = Grammar.Compile(@"(?P<path>.+)");

        Assert.Equal("test", grammar.Escape("path", "test"));
    }

    [Fact]
    public void Escape_UnknownVariable_ReturnsOriginal()
    {
        var escapeFuncs = new Dictionary<string, Func<string, string>>
        {
            ["other"] = s => s.ToUpper()
        };
        var grammar = Grammar.Compile(@"(?P<path>.+)", escapeFuncs);

        Assert.Equal("test", grammar.Escape("path", "test"));
    }

    [Fact]
    public void Unescape_WithFunction_AppliesFunction()
    {
        var unescapeFuncs = new Dictionary<string, Func<string, string>>
        {
            ["path"] = s => s.Trim('"')
        };
        var grammar = Grammar.Compile(@"(?P<path>.+)", null, unescapeFuncs);

        Assert.Equal("test", grammar.Unescape("path", "\"test\""));
    }

    [Fact]
    public void Unescape_WithoutFunction_ReturnsOriginal()
    {
        var grammar = Grammar.Compile(@"(?P<path>.+)");

        Assert.Equal("\"test\"", grammar.Unescape("path", "\"test\""));
    }

    [Fact]
    public void Unescape_UnknownVariable_ReturnsOriginal()
    {
        var unescapeFuncs = new Dictionary<string, Func<string, string>>
        {
            ["other"] = s => s.ToLower()
        };
        var grammar = Grammar.Compile(@"(?P<path>.+)", null, unescapeFuncs);

        Assert.Equal("TEST", grammar.Unescape("path", "TEST"));
    }
}

public class CompiledGrammarTrailingInputTests
{
    [Fact]
    public void MatchPrefix_WithTrailingInput_CapturesTrailing()
    {
        var grammar = Grammar.Compile(@"hello");
        var match = grammar.MatchPrefix("hello world");

        Assert.NotNull(match);
        var trailing = match.TrailingInput();
        Assert.NotNull(trailing);
        Assert.Equal("<trailing_input>", trailing.VarName);
        Assert.Contains("world", trailing.Value);
    }

    [Fact]
    public void Match_NoTrailingInput_ReturnsNull()
    {
        var grammar = Grammar.Compile(@"hello");
        var match = grammar.Match("hello");

        Assert.NotNull(match);
        Assert.Null(match.TrailingInput());
    }
}

public class CompiledGrammarEndNodesTests
{
    [Fact]
    public void EndNodes_VariableAtEnd_ReturnsVariable()
    {
        var grammar = Grammar.Compile(@"cmd\s(?P<arg>\w+)");
        var match = grammar.MatchPrefix("cmd test");

        Assert.NotNull(match);
        var endNodes = match.EndNodes().ToList();
        Assert.NotEmpty(endNodes);
        Assert.Contains(endNodes, e => e.VarName == "arg");
    }

    [Fact]
    public void EndNodes_NoVariableAtEnd_ReturnsEmpty()
    {
        var grammar = Grammar.Compile(@"(?P<cmd>\w+)\send");
        var match = grammar.Match("hello end");

        Assert.NotNull(match);
        var endNodes = match.EndNodes().ToList();
        Assert.Empty(endNodes);
    }

    [Fact]
    public void EndNodes_MultiplePathsToEnd_ReturnsEndingVariables()
    {
        var grammar = Grammar.Compile(@"(a\s(?P<x>\w+)|b\s(?P<y>\w+))");

        var match1 = grammar.MatchPrefix("a test");
        Assert.NotNull(match1);
        var endNodes1 = match1.EndNodes().ToList();
        Assert.NotEmpty(endNodes1);
        Assert.Contains(endNodes1, e => e.VarName == "x");

        var match2 = grammar.MatchPrefix("b test");
        Assert.NotNull(match2);
        var endNodes2 = match2.EndNodes().ToList();
        Assert.NotEmpty(endNodes2);
        Assert.Contains(endNodes2, e => e.VarName == "y");
    }
}

public class GrammarWhitespaceAndCommentsTests
{
    [Fact]
    public void Compile_IgnoresWhitespace()
    {
        var grammar = Grammar.Compile(@"
            hello
            \s+
            world
        ");
        var match = grammar.Match("hello world");

        Assert.NotNull(match);
    }

    [Fact]
    public void Compile_IgnoresComments()
    {
        var grammar = Grammar.Compile(@"
            hello  # This is a greeting
            \s+
            world  # The target
        ");
        var match = grammar.Match("hello world");

        Assert.NotNull(match);
    }

    [Fact]
    public void Compile_PreservesWhitespaceInCharClass()
    {
        var grammar = Grammar.Compile(@"[ \t]+");
        var match = grammar.Match("   ");

        Assert.NotNull(match);
    }

    [Fact]
    public void Compile_PreservesEscapedWhitespace()
    {
        var grammar = Grammar.Compile(@"hello\ world");
        var match = grammar.Match("hello world");

        Assert.NotNull(match);
    }
}

public class GrammarComplexPatternsTests
{
    [Fact]
    public void ShellCommandGrammar_MatchesCd()
    {
        var grammar = Grammar.Compile(@"
            \s*
            (
                pwd |
                ls |
                (cd \s+ (?P<directory>[^\s]+)) |
                (cat \s+ (?P<filename>[^\s]+))
            )
            \s*
        ");

        var match = grammar.Match("cd /home/user");
        Assert.NotNull(match);
        Assert.Equal("/home/user", match.Variables()["directory"]);
    }

    [Fact]
    public void ShellCommandGrammar_MatchesLs()
    {
        var grammar = Grammar.Compile(@"
            \s*
            (
                pwd |
                ls |
                (cd \s+ (?P<directory>[^\s]+))
            )
            \s*
        ");

        var match = grammar.Match("ls");
        Assert.NotNull(match);
    }

    [Fact]
    public void ShellCommandGrammar_MatchesPwd()
    {
        var grammar = Grammar.Compile(@"
            \s*
            (
                pwd |
                ls |
                (cd \s+ (?P<directory>[^\s]+))
            )
            \s*
        ");

        var match = grammar.Match("pwd");
        Assert.NotNull(match);
    }

    [Fact]
    public void NestedGroups_ExtractsCorrectly()
    {
        var grammar = Grammar.Compile(@"(?P<outer>(?P<inner>\d+))");
        var match = grammar.Match("123");

        Assert.NotNull(match);
        var vars = match.Variables();
        Assert.Equal("123", vars["outer"]);
        Assert.Equal("123", vars["inner"]);
    }

    [Fact]
    public void OptionalVariable_WhenPresent_ExtractsValue()
    {
        var grammar = Grammar.Compile(@"cmd(\s+(?P<arg>\w+))?");
        var match = grammar.Match("cmd test");

        Assert.NotNull(match);
        Assert.Equal("test", match.Variables()["arg"]);
    }

    [Fact]
    public void OptionalVariable_WhenAbsent_ReturnsNull()
    {
        var grammar = Grammar.Compile(@"cmd(\s+(?P<arg>\w+))?");
        var match = grammar.Match("cmd");

        Assert.NotNull(match);
        Assert.Null(match.Variables()["arg"]);
    }

    [Fact]
    public void RepeatedVariable_MatchesMultipleTimes()
    {
        var grammar = Grammar.Compile(@"((?P<item>\w+)\s*)+");
        var match = grammar.Match("a b c");

        Assert.NotNull(match);
        // The last match of the repeating group is captured
        Assert.NotNull(match.Variables()["item"]);
    }
}

public class GrammarNegativeLookaheadTests
{
    [Fact]
    public void NegativeLookahead_Excludes()
    {
        var grammar = Grammar.Compile(@"(?!bad)\w+");

        var match = grammar.Match("good");
        Assert.NotNull(match);

        var badMatch = grammar.Match("bad");
        Assert.Null(badMatch);
    }
}

public class GrammarPositiveLookaheadTests
{
    [Fact]
    public void PositiveLookahead_ThrowsNotSupported()
    {
        Assert.Throws<NotSupportedException>(() => Grammar.Compile(@"(?=good)\w+"));
    }
}

public class GrammarRepetitionTests
{
    [Fact]
    public void StarRepetition_MatchesZeroOrMore()
    {
        var grammar = Grammar.Compile(@"a*");

        Assert.NotNull(grammar.Match(""));
        Assert.NotNull(grammar.Match("a"));
        Assert.NotNull(grammar.Match("aaa"));
    }

    [Fact]
    public void PlusRepetition_MatchesOneOrMore()
    {
        var grammar = Grammar.Compile(@"a+");

        Assert.Null(grammar.Match(""));
        Assert.NotNull(grammar.Match("a"));
        Assert.NotNull(grammar.Match("aaa"));
    }

    [Fact]
    public void QuestionRepetition_MatchesZeroOrOne()
    {
        var grammar = Grammar.Compile(@"a?b");

        Assert.NotNull(grammar.Match("b"));
        Assert.NotNull(grammar.Match("ab"));
        Assert.Null(grammar.Match("aab"));
    }

    [Fact]
    public void NonGreedyRepetition_MatchesMinimal()
    {
        var grammar = Grammar.Compile(@"a+?b");

        Assert.NotNull(grammar.Match("ab"));
        Assert.NotNull(grammar.Match("aab"));
    }

    [Fact]
    public void BoundedRepetition_ThrowsNotSupported()
    {
        // {n,m} style repetition is not supported per Python Prompt Toolkit
        Assert.Throws<NotSupportedException>(() => Grammar.Compile(@"a{2,4}"));
    }
}

public class GrammarUnescapeIntegrationTests
{
    [Fact]
    public void Match_WithUnescapeFunc_AppliesUnescape()
    {
        var unescapeFuncs = new Dictionary<string, Func<string, string>>
        {
            ["path"] = s => s.Replace(@"\ ", " ")
        };
        var grammar = Grammar.Compile(@"(?P<path>.+)", null, unescapeFuncs);
        var match = grammar.Match(@"hello\ world");

        Assert.NotNull(match);
        Assert.Equal("hello world", match.Variables()["path"]);
    }
}

/// <summary>
/// Tests for Unicode character handling (SC-005).
/// </summary>
public class GrammarUnicodeTests
{
    [Fact]
    public void Match_CJKCharacters_ExtractsCorrectly()
    {
        var grammar = Grammar.Compile(@"(?P<text>.+)");
        var match = grammar.Match("你好世界");

        Assert.NotNull(match);
        Assert.Equal("你好世界", match.Variables()["text"]);
    }

    [Fact]
    public void Match_Emoji_ExtractsCorrectly()
    {
        var grammar = Grammar.Compile(@"(?P<emoji>.+)");
        var match = grammar.Match("🎉🎊🎈");

        Assert.NotNull(match);
        var vars = match.Variables();
        Assert.Equal("🎉🎊🎈", vars["emoji"]);
    }

    [Fact]
    public void Match_MixedUnicode_ExtractsCorrectly()
    {
        var grammar = Grammar.Compile(@"(?P<a>\w+)\s(?P<b>.+)");
        var match = grammar.Match("hello 世界🌍");

        Assert.NotNull(match);
        var vars = match.Variables();
        Assert.Equal("hello", vars["a"]);
        Assert.Equal("世界🌍", vars["b"]);
    }

    [Fact]
    public void Match_CombiningCharacters_ExtractsCorrectly()
    {
        var grammar = Grammar.Compile(@"(?P<text>.+)");
        // é as e + combining acute accent
        var match = grammar.Match("cafe\u0301");

        Assert.NotNull(match);
        Assert.Equal("cafe\u0301", match.Variables()["text"]);
    }

    [Fact]
    public void VariablePositions_Unicode_ReportsCorrectPositions()
    {
        var grammar = Grammar.Compile(@"(?P<prefix>.+)→(?P<suffix>.+)");
        var match = grammar.Match("你好→世界");

        Assert.NotNull(match);
        var vars = match.Variables().ToList();

        var prefix = vars.First(v => v.VarName == "prefix");
        var suffix = vars.First(v => v.VarName == "suffix");

        Assert.Equal("你好", prefix.Value);
        Assert.Equal("世界", suffix.Value);
        Assert.Equal(0, prefix.Start);
        Assert.Equal(2, prefix.Stop); // 2 CJK chars
        Assert.Equal(3, suffix.Start); // After arrow (1 char)
        Assert.Equal(5, suffix.Stop); // 2 more CJK chars
    }
}

/// <summary>
/// Tests for edge cases (FR-025, FR-026).
/// </summary>
public class GrammarEdgeCaseTests
{
    [Fact]
    public void Match_EmptyInput_MatchesEmptyPattern()
    {
        var grammar = Grammar.Compile(@"(?P<text>.*)");
        var match = grammar.Match("");

        Assert.NotNull(match);
        Assert.Equal("", match.Variables()["text"]);
    }

    [Fact]
    public void Match_EmptyVariable_MatchesEmpty()
    {
        var grammar = Grammar.Compile(@"prefix(?P<opt>\w*)suffix");
        var match = grammar.Match("prefixsuffix");

        Assert.NotNull(match);
        Assert.Equal("", match.Variables()["opt"]);
    }

    [Fact]
    public void Compile_EmptyExpression_ReturnsGrammar()
    {
        var grammar = Grammar.Compile(@"");
        var match = grammar.Match("");

        Assert.NotNull(match);
    }

    [Fact]
    public void PositiveLookahead_ThrowsNotSupportedException()
    {
        Assert.Throws<NotSupportedException>(() => Grammar.Compile(@"(?=abc)\w+"));
    }

    [Fact]
    public void BoundedRepetition_ThrowsNotSupportedException()
    {
        Assert.Throws<NotSupportedException>(() => Grammar.Compile(@"a{2,4}"));
    }

    [Fact]
    public void Match_VeryLongInput_HandlesCorrectly()
    {
        var grammar = Grammar.Compile(@"(?P<text>.+)");
        var longInput = new string('a', 10000);
        var match = grammar.Match(longInput);

        Assert.NotNull(match);
        Assert.Equal(longInput, match.Variables()["text"]);
    }

    [Fact]
    public void Match_SpecialRegexChars_EscapedCorrectly()
    {
        var grammar = Grammar.Compile(@"\[\]");
        var match = grammar.Match("[]");

        Assert.NotNull(match);
    }

    [Fact]
    public void MatchPrefix_InputLongerThanPattern_CapturesTrailing()
    {
        var grammar = Grammar.Compile(@"hello");
        var match = grammar.MatchPrefix("hello there friend");

        Assert.NotNull(match);
        var trailing = match.TrailingInput();
        Assert.NotNull(trailing);
        Assert.Contains("there", trailing.Value);
    }
}

/// <summary>
/// Thread safety tests (SC-008).
/// </summary>
public class GrammarThreadSafetyTests
{
    [Fact]
    public async Task CompiledGrammar_ConcurrentMatching_IsThreadSafe()
    {
        var grammar = Grammar.Compile(@"(?P<cmd>\w+)\s(?P<arg>\w+)");
        var iterations = 1000;
        var threadCount = 10;

        var tasks = Enumerable.Range(0, threadCount).Select(threadId =>
            Task.Run(() =>
            {
                for (int i = 0; i < iterations; i++)
                {
                    var input = $"cmd{threadId} arg{i}";
                    var match = grammar.Match(input);

                    Assert.NotNull(match);
                    Assert.Equal($"cmd{threadId}", match.Variables()["cmd"]);
                    Assert.Equal($"arg{i}", match.Variables()["arg"]);
                }
            })).ToArray();

        await Task.WhenAll(tasks);
    }

    [Fact]
    public async Task CompiledGrammar_ConcurrentMatchPrefix_IsThreadSafe()
    {
        var grammar = Grammar.Compile(@"prefix\s(?P<value>\w+)");
        var iterations = 500;
        var threadCount = 8;

        var tasks = Enumerable.Range(0, threadCount).Select(threadId =>
            Task.Run(() =>
            {
                for (int i = 0; i < iterations; i++)
                {
                    var input = $"prefix val{i}";
                    var match = grammar.MatchPrefix(input);

                    Assert.NotNull(match);
                    var vars = match.Variables();
                    Assert.Equal($"val{i}", vars["value"]);
                }
            })).ToArray();

        await Task.WhenAll(tasks);
    }

    [Fact]
    public async Task Match_ConcurrentVariableAccess_IsThreadSafe()
    {
        var grammar = Grammar.Compile(@"(?P<a>\w+)\s(?P<b>\w+)\s(?P<c>\w+)");
        var match = grammar.Match("one two three");

        Assert.NotNull(match);

        var iterations = 1000;
        var threadCount = 10;

        var tasks = Enumerable.Range(0, threadCount).Select(_ =>
            Task.Run(() =>
            {
                for (int i = 0; i < iterations; i++)
                {
                    var vars = match.Variables();
                    Assert.Equal("one", vars["a"]);
                    Assert.Equal("two", vars["b"]);
                    Assert.Equal("three", vars["c"]);
                }
            })).ToArray();

        await Task.WhenAll(tasks);
    }
}
