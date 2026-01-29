using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Stroke.Contrib.RegularLanguages;

namespace Stroke.Benchmarks;

/// <summary>
/// Benchmarks for matching and parsing performance (SC-003).
/// </summary>
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net10_0)]
public class MatchingBenchmarks
{
    private CompiledGrammar _simpleGrammar = null!;
    private CompiledGrammar _variableGrammar = null!;
    private CompiledGrammar _complexGrammar = null!;
    private CompiledGrammar _unescapeGrammar = null!;

    private const string SimpleInput = "hello";
    private const string VariableInput = "cmd arg";
    private const string ComplexInput = "cd /home/user/documents";
    private const string LongInput = "cmd abcdefghijklmnopqrstuvwxyz1234567890";
    private const string EscapedInput = @"hello\ world";

    [GlobalSetup]
    public void Setup()
    {
        _simpleGrammar = Grammar.Compile(@"hello");
        _variableGrammar = Grammar.Compile(@"(?P<cmd>\w+)\s(?P<arg>\w+)");
        _complexGrammar = Grammar.Compile(@"\s*(pwd|ls|(cd\s+(?P<directory>[^\s]+)))\s*");
        _unescapeGrammar = Grammar.Compile(
            @"(?P<path>.+)",
            null,
            new Dictionary<string, Func<string, string>>
            {
                ["path"] = s => s.Replace(@"\ ", " ")
            });
    }

    /// <summary>
    /// Match against a simple pattern with no variables.
    /// </summary>
    [Benchmark(Baseline = true)]
    public Match? MatchSimple()
    {
        return _simpleGrammar.Match(SimpleInput);
    }

    /// <summary>
    /// Match and extract two variables.
    /// </summary>
    [Benchmark]
    public Match? MatchWithVariables()
    {
        return _variableGrammar.Match(VariableInput);
    }

    /// <summary>
    /// Match against complex alternation grammar.
    /// </summary>
    [Benchmark]
    public Match? MatchComplex()
    {
        return _complexGrammar.Match(ComplexInput);
    }

    /// <summary>
    /// Match longer input with variables.
    /// </summary>
    [Benchmark]
    public Match? MatchLongInput()
    {
        return _variableGrammar.Match(LongInput);
    }

    /// <summary>
    /// Match prefix for autocompletion.
    /// </summary>
    [Benchmark]
    public Match? MatchPrefix()
    {
        return _variableGrammar.MatchPrefix("cmd a");
    }

    /// <summary>
    /// Match and extract variables with unescape function.
    /// </summary>
    [Benchmark]
    public Variables? MatchAndExtractVariables()
    {
        var match = _variableGrammar.Match(VariableInput);
        return match?.Variables();
    }

    /// <summary>
    /// Match with unescape function applied.
    /// </summary>
    [Benchmark]
    public Variables? MatchWithUnescape()
    {
        var match = _unescapeGrammar.Match(EscapedInput);
        return match?.Variables();
    }

    /// <summary>
    /// Get variable at cursor position.
    /// </summary>
    [Benchmark]
    public MatchVariable? VariableAtPosition()
    {
        var match = _variableGrammar.Match(VariableInput);
        return match?.VariableAtPosition(5);
    }

    /// <summary>
    /// Get end nodes for completion.
    /// </summary>
    [Benchmark]
    public int GetEndNodesCount()
    {
        var match = _variableGrammar.MatchPrefix("cmd a");
        return match?.EndNodes().Count() ?? 0;
    }

    /// <summary>
    /// Get trailing input detection.
    /// </summary>
    [Benchmark]
    public MatchVariable? GetTrailingInput()
    {
        var match = _simpleGrammar.MatchPrefix("hello world");
        return match?.TrailingInput();
    }
}
