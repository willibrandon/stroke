using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Stroke.FormattedText;
using FT = Stroke.FormattedText.FormattedText;

namespace Stroke.Benchmarks;

/// <summary>
/// Benchmarks for the FormattedText system per Constitution VIII and spec requirements.
/// </summary>
[SimpleJob(RuntimeMoniker.Net10_0)]
[MemoryDiagnoser]
public class FormattedTextBenchmarks
{
    private string _plainText1Kb = null!;
    private string _plainText5Kb = null!;
    private string _plainText10Kb = null!;
    private string _htmlText100Kb = null!;
    private string _ansiText1Kb = null!;
    private string _ansiText10Kb = null!;
    private string _ansiText100Kb = null!;

    [GlobalSetup]
    public void Setup()
    {
        // Create plain text of various sizes for T110
        _plainText1Kb = GeneratePlainText(1024);
        _plainText5Kb = GeneratePlainText(5 * 1024);
        _plainText10Kb = GeneratePlainText(10 * 1024);

        // Create HTML text with markup for T111
        _htmlText100Kb = GenerateHtmlText(100 * 1024);

        // Create ANSI text with escape sequences for T112
        _ansiText1Kb = GenerateAnsiText(1024);
        _ansiText10Kb = GenerateAnsiText(10 * 1024);
        _ansiText100Kb = GenerateAnsiText(100 * 1024);
    }

    private static string GeneratePlainText(int approximateSize)
    {
        const string baseText = "The quick brown fox jumps over the lazy dog. ";
        var builder = new System.Text.StringBuilder(approximateSize + baseText.Length);
        while (builder.Length < approximateSize)
        {
            builder.Append(baseText);
        }
        return builder.ToString();
    }

    private static string GenerateHtmlText(int approximateSize)
    {
        // Create realistic HTML with various elements
        const string template = "<b>bold text</b> and <i>italic</i> with <style fg='red'>colored</style> content. ";
        var builder = new System.Text.StringBuilder(approximateSize + template.Length);
        while (builder.Length < approximateSize)
        {
            builder.Append(template);
        }
        return builder.ToString();
    }

    private static string GenerateAnsiText(int approximateSize)
    {
        // Create realistic ANSI text with various SGR sequences
        // Includes: basic colors, 256-color, true color, reset
        const string template = "\x1b[31mred\x1b[0m \x1b[38;5;208morange\x1b[0m \x1b[38;2;100;149;237mcornflower\x1b[0m normal ";
        var builder = new System.Text.StringBuilder(approximateSize + template.Length);
        while (builder.Length < approximateSize)
        {
            builder.Append(template);
        }
        return builder.ToString();
    }

    #region T110: ToFormattedText with 1KB/5KB/10KB inputs

    /// <summary>
    /// T110: Convert 1KB plain text to FormattedText via AnyFormattedText.
    /// </summary>
    [Benchmark(Description = "Plain text 1KB to fragments")]
    public int PlainText_1Kb_ToFormattedText()
    {
        AnyFormattedText aft = _plainText1Kb;
        return aft.ToFormattedText().Count;
    }

    /// <summary>
    /// T110: Convert 5KB plain text to FormattedText via AnyFormattedText.
    /// </summary>
    [Benchmark(Description = "Plain text 5KB to fragments")]
    public int PlainText_5Kb_ToFormattedText()
    {
        AnyFormattedText aft = _plainText5Kb;
        return aft.ToFormattedText().Count;
    }

    /// <summary>
    /// T110: Convert 10KB plain text to FormattedText via AnyFormattedText.
    /// </summary>
    [Benchmark(Description = "Plain text 10KB to fragments")]
    public int PlainText_10Kb_ToFormattedText()
    {
        AnyFormattedText aft = _plainText10Kb;
        return aft.ToFormattedText().Count;
    }

    #endregion

    #region T111: HTML parsing with 100KB input

    /// <summary>
    /// T111: Parse 100KB HTML markup to FormattedText fragments.
    /// </summary>
    [Benchmark(Description = "HTML 100KB parsing")]
    public int Html_100Kb_ToFormattedText()
    {
        var html = new Html(_htmlText100Kb);
        return html.ToFormattedText().Count;
    }

    #endregion

    #region T112: ANSI parsing throughput

    /// <summary>
    /// T112: Parse 1KB ANSI text to FormattedText fragments.
    /// </summary>
    [Benchmark(Description = "ANSI 1KB parsing")]
    public int Ansi_1Kb_ToFormattedText()
    {
        var ansi = new Ansi(_ansiText1Kb);
        return ansi.ToFormattedText().Count;
    }

    /// <summary>
    /// T112: Parse 10KB ANSI text to FormattedText fragments.
    /// </summary>
    [Benchmark(Description = "ANSI 10KB parsing")]
    public int Ansi_10Kb_ToFormattedText()
    {
        var ansi = new Ansi(_ansiText10Kb);
        return ansi.ToFormattedText().Count;
    }

    /// <summary>
    /// T112: Parse 100KB ANSI text to FormattedText fragments.
    /// </summary>
    [Benchmark(Description = "ANSI 100KB parsing")]
    public int Ansi_100Kb_ToFormattedText()
    {
        var ansi = new Ansi(_ansiText100Kb);
        return ansi.ToFormattedText().Count;
    }

    #endregion
}

/// <summary>
/// Benchmarks for Template interpolation with various placeholder counts.
/// </summary>
[SimpleJob(RuntimeMoniker.Net10_0)]
[MemoryDiagnoser]
public class TemplateScalingBenchmarks
{
    private Template _templateFewPlaceholders = null!;
    private Template _templateManyPlaceholders = null!;
    private AnyFormattedText[] _argsFew = null!;
    private AnyFormattedText[] _argsMany = null!;

    [GlobalSetup]
    public void Setup()
    {
        // Template with few placeholders (5) - Template uses {} not {0}
        _templateFewPlaceholders = new Template("Hello {}, welcome to {}. Your id is {}. Status: {}. Level: {}.");
        _argsFew = [(AnyFormattedText)"user", "system", "12345", "active", "99"];

        // Template with many placeholders (50)
        var manyTemplate = string.Join(" ", Enumerable.Range(0, 50).Select(i => $"arg{i}={{}}"));
        _templateManyPlaceholders = new Template(manyTemplate);
        _argsMany = Enumerable.Range(0, 50).Select(i => (AnyFormattedText)$"value{i}").ToArray();
    }

    /// <summary>
    /// Template interpolation with 5 placeholders.
    /// </summary>
    [Benchmark(Description = "Template: 5 placeholders")]
    public int Template_FewPlaceholders()
    {
        var func = _templateFewPlaceholders.Format(_argsFew);
        return func().ToFormattedText().Count;
    }

    /// <summary>
    /// Template interpolation with 50 placeholders.
    /// </summary>
    [Benchmark(Description = "Template: 50 placeholders")]
    public int Template_ManyPlaceholders()
    {
        var func = _templateManyPlaceholders.Format(_argsMany);
        return func().ToFormattedText().Count;
    }
}

/// <summary>
/// Benchmarks for AnyFormattedText conversion overhead.
/// </summary>
[SimpleJob(RuntimeMoniker.Net10_0)]
[MemoryDiagnoser]
public class AnyFormattedTextBenchmarks
{
    private string _plainText = null!;
    private Html _html = null!;
    private Ansi _ansi = null!;
    private FT _formattedText = null!;

    [GlobalSetup]
    public void Setup()
    {
        _plainText = "Hello, World!";
        _html = new Html("<b>Hello</b>, <i>World</i>!");
        _ansi = new Ansi("\x1b[1mHello\x1b[0m, \x1b[3mWorld\x1b[0m!");
        _formattedText = new FT(new StyleAndTextTuple("", "Hello, World!"));
    }

    /// <summary>
    /// Implicit conversion from string to AnyFormattedText.
    /// </summary>
    [Benchmark(Description = "AnyFormattedText from string")]
    public int FromString()
    {
        AnyFormattedText aft = _plainText;
        return aft.ToFormattedText().Count;
    }

    /// <summary>
    /// Implicit conversion from Html to AnyFormattedText.
    /// </summary>
    [Benchmark(Description = "AnyFormattedText from Html")]
    public int FromHtml()
    {
        AnyFormattedText aft = _html;
        return aft.ToFormattedText().Count;
    }

    /// <summary>
    /// Implicit conversion from Ansi to AnyFormattedText.
    /// </summary>
    [Benchmark(Description = "AnyFormattedText from Ansi")]
    public int FromAnsi()
    {
        AnyFormattedText aft = _ansi;
        return aft.ToFormattedText().Count;
    }

    /// <summary>
    /// Implicit conversion from FormattedText to AnyFormattedText.
    /// </summary>
    [Benchmark(Description = "AnyFormattedText from FormattedText")]
    public int FromFormattedText()
    {
        AnyFormattedText aft = _formattedText;
        return aft.ToFormattedText().Count;
    }
}

/// <summary>
/// Benchmarks for FormattedTextUtils operations.
/// </summary>
[SimpleJob(RuntimeMoniker.Net10_0)]
[MemoryDiagnoser]
public class FormattedTextUtilsBenchmarks
{
    private System.Collections.Immutable.ImmutableArray<StyleAndTextTuple> _fragments = default;
    private System.Collections.Immutable.ImmutableArray<StyleAndTextTuple> _largeFragments = default;

    [GlobalSetup]
    public void Setup()
    {
        // Small fragment list (10 fragments)
        var smallBuilder = System.Collections.Immutable.ImmutableArray.CreateBuilder<StyleAndTextTuple>();
        for (int i = 0; i < 10; i++)
        {
            smallBuilder.Add(new StyleAndTextTuple($"class:style{i}", $"text{i} "));
        }
        _fragments = smallBuilder.ToImmutable();

        // Large fragment list (1000 fragments)
        var largeBuilder = System.Collections.Immutable.ImmutableArray.CreateBuilder<StyleAndTextTuple>();
        for (int i = 0; i < 1000; i++)
        {
            largeBuilder.Add(new StyleAndTextTuple($"class:style{i % 50}", $"text{i} content here "));
        }
        _largeFragments = largeBuilder.ToImmutable();
    }

    /// <summary>
    /// FragmentListToText with 10 fragments.
    /// </summary>
    [Benchmark(Description = "FragmentListToText: 10 fragments")]
    public string FragmentListToText_Small()
    {
        return FormattedTextUtils.FragmentListToText(_fragments);
    }

    /// <summary>
    /// FragmentListToText with 1000 fragments.
    /// </summary>
    [Benchmark(Description = "FragmentListToText: 1000 fragments")]
    public string FragmentListToText_Large()
    {
        return FormattedTextUtils.FragmentListToText(_largeFragments);
    }

    /// <summary>
    /// FragmentListWidth with 10 fragments.
    /// </summary>
    [Benchmark(Description = "FragmentListWidth: 10 fragments")]
    public int FragmentListWidth_Small()
    {
        return FormattedTextUtils.FragmentListWidth(_fragments);
    }

    /// <summary>
    /// FragmentListWidth with 1000 fragments.
    /// </summary>
    [Benchmark(Description = "FragmentListWidth: 1000 fragments")]
    public int FragmentListWidth_Large()
    {
        return FormattedTextUtils.FragmentListWidth(_largeFragments);
    }

    /// <summary>
    /// SplitLines with 10 fragments.
    /// </summary>
    [Benchmark(Description = "SplitLines: 10 fragments")]
    public int SplitLines_Small()
    {
        return FormattedTextUtils.SplitLines(_fragments).Count();
    }

    /// <summary>
    /// SplitLines with 1000 fragments.
    /// </summary>
    [Benchmark(Description = "SplitLines: 1000 fragments")]
    public int SplitLines_Large()
    {
        return FormattedTextUtils.SplitLines(_largeFragments).Count();
    }
}
