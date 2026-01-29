using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Stroke.Contrib.RegularLanguages;

namespace Stroke.Benchmarks;

/// <summary>
/// Benchmarks for grammar compilation performance (SC-002).
/// </summary>
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net10_0)]
public class GrammarCompilationBenchmarks
{
    private const string SimplePattern = @"hello";
    private const string VariablePattern = @"(?P<cmd>\w+)\s(?P<arg>\w+)";
    private const string ComplexPattern = @"\s*(pwd|ls|(cd\s+(?P<directory>[^\s]+))|(cat\s+(?P<filename>[^\s]+)))\s*";
    private const string NestedPattern = @"(?P<outer>(?P<inner>(?P<deepest>\w+)))";

    /// <summary>
    /// Compile a simple pattern with no variables.
    /// </summary>
    [Benchmark(Baseline = true)]
    public CompiledGrammar CompileSimplePattern()
    {
        return Grammar.Compile(SimplePattern);
    }

    /// <summary>
    /// Compile a pattern with two named variables.
    /// </summary>
    [Benchmark]
    public CompiledGrammar CompilePatternWithVariables()
    {
        return Grammar.Compile(VariablePattern);
    }

    /// <summary>
    /// Compile a complex shell-like grammar with alternations.
    /// </summary>
    [Benchmark]
    public CompiledGrammar CompileComplexPattern()
    {
        return Grammar.Compile(ComplexPattern);
    }

    /// <summary>
    /// Compile a pattern with nested groups.
    /// </summary>
    [Benchmark]
    public CompiledGrammar CompileNestedPattern()
    {
        return Grammar.Compile(NestedPattern);
    }

    /// <summary>
    /// Compile a pattern with escape/unescape functions.
    /// </summary>
    [Benchmark]
    public CompiledGrammar CompileWithEscapeFunctions()
    {
        var escapeFuncs = new Dictionary<string, Func<string, string>>
        {
            ["path"] = s => s.Replace(" ", @"\ ")
        };
        var unescapeFuncs = new Dictionary<string, Func<string, string>>
        {
            ["path"] = s => s.Replace(@"\ ", " ")
        };
        return Grammar.Compile(@"(?P<path>.+)", escapeFuncs, unescapeFuncs);
    }
}
