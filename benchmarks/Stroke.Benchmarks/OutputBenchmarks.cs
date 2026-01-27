using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Stroke.Output;
using Stroke.Styles;

namespace Stroke.Benchmarks;

/// <summary>
/// Benchmarks for the Output system, including color cache memory usage (NFR-003).
/// </summary>
/// <remarks>
/// NFR-003 states: Color cache memory usage ≤10KB for typical use cases.
/// These benchmarks measure memory allocation for various cache operations.
/// Uses DummyOutput to avoid StringWriter memory accumulation issues.
/// </remarks>
[SimpleJob(RuntimeMoniker.Net10_0)]
[MemoryDiagnoser]
public class OutputBenchmarks
{
    private DummyOutput _output = null!;
    private readonly Attrs _colorAttrs = new(Color: "ff0000", BgColor: "00ff00");

    [GlobalSetup]
    public void Setup()
    {
        _output = new DummyOutput();
    }

    /// <summary>
    /// Baseline: single write operation.
    /// </summary>
    [Benchmark(Baseline = true, Description = "Write: single text")]
    public void Write_SingleText()
    {
        _output.Write("Hello, World!");
    }

    /// <summary>
    /// Write with escape sequences.
    /// </summary>
    [Benchmark(Description = "WriteRaw: escape sequence")]
    public void WriteRaw_EscapeSequence()
    {
        _output.WriteRaw("\x1b[32mGreen\x1b[0m");
    }

    /// <summary>
    /// SetAttributes with 24-bit color (triggers cache lookup).
    /// </summary>
    [Benchmark(Description = "SetAttributes: 24-bit color")]
    public void SetAttributes_24Bit()
    {
        _output.SetAttributes(_colorAttrs, ColorDepth.Depth24Bit);
    }

    /// <summary>
    /// SetAttributes with 256-color (triggers cache lookup and color mapping).
    /// </summary>
    [Benchmark(Description = "SetAttributes: 256-color")]
    public void SetAttributes_256Color()
    {
        _output.SetAttributes(_colorAttrs, ColorDepth.Depth8Bit);
    }

    /// <summary>
    /// SetAttributes with 16-color (triggers cache lookup and nearest color search).
    /// </summary>
    [Benchmark(Description = "SetAttributes: 16-color")]
    public void SetAttributes_16Color()
    {
        _output.SetAttributes(_colorAttrs, ColorDepth.Depth4Bit);
    }

    /// <summary>
    /// Cursor movement operations.
    /// </summary>
    [Benchmark(Description = "Cursor: movement sequence")]
    public void CursorMovement()
    {
        _output.CursorGoto(10, 20);
        _output.CursorUp(5);
        _output.CursorDown(3);
        _output.CursorForward(10);
        _output.CursorBackward(5);
    }
}

/// <summary>
/// Memory benchmarks for color cache (NFR-003: ≤10KB memory usage).
/// </summary>
/// <remarks>
/// These benchmarks measure the memory footprint of color cache operations
/// to ensure we stay within the 10KB budget for typical use cases.
/// Uses DummyOutput to avoid StringWriter memory accumulation.
/// </remarks>
[SimpleJob(RuntimeMoniker.Net10_0)]
[MemoryDiagnoser]
public class ColorCacheMemoryBenchmarks
{
    private DummyOutput _output = null!;
    private Attrs[] _randomColors = null!;

    /// <summary>
    /// Number of unique colors to cache (simulates typical use case).
    /// </summary>
    [Params(10, 50, 100)]
    public int UniqueColorCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _output = new DummyOutput();

        // Generate random colors for testing
        var random = new Random(42); // Fixed seed for reproducibility
        _randomColors = new Attrs[UniqueColorCount];
        for (var i = 0; i < UniqueColorCount; i++)
        {
            var fg = $"{random.Next(256):X2}{random.Next(256):X2}{random.Next(256):X2}";
            var bg = $"{random.Next(256):X2}{random.Next(256):X2}{random.Next(256):X2}";
            _randomColors[i] = new Attrs(Color: fg, BgColor: bg);
        }
    }

    /// <summary>
    /// Warm up the cache with unique colors and measure memory impact.
    /// This simulates a typical application that uses a variety of colors.
    /// </summary>
    [Benchmark(Description = "Cache warmup: N unique colors")]
    public int WarmupCache()
    {
        var count = 0;
        foreach (var attrs in _randomColors)
        {
            _output.SetAttributes(attrs, ColorDepth.Depth8Bit);
            count++;
        }
        return count;
    }

    /// <summary>
    /// Cache hits after warmup - should have zero allocations.
    /// </summary>
    [Benchmark(Description = "Cache hits: N colors (after warmup)")]
    public int CacheHits()
    {
        var count = 0;
        // Access same colors again - should be cached
        foreach (var attrs in _randomColors)
        {
            _output.SetAttributes(attrs, ColorDepth.Depth8Bit);
            count++;
        }
        return count;
    }
}

/// <summary>
/// Benchmarks for measuring color cache memory under stress (NFR-003 verification).
/// </summary>
[SimpleJob(RuntimeMoniker.Net10_0)]
[MemoryDiagnoser]
public class ColorCacheStressBenchmarks
{
    private StringWriter _writer = null!;

    /// <summary>
    /// Measures memory for creating a fresh Vt100Output and exercising color cache.
    /// </summary>
    [Benchmark(Description = "Full cache build: 256 colors")]
    public long FullCacheBuild()
    {
        _writer = new StringWriter();
        var output = Vt100Output.FromPty(_writer);

        // Exercise all 256 grayscale colors to fill 256-color cache
        for (var i = 0; i < 256; i++)
        {
            var hex = $"{i:X2}{i:X2}{i:X2}";
            output.SetAttributes(new Attrs(Color: hex), ColorDepth.Depth8Bit);
        }

        return _writer.ToString().Length;
    }

    /// <summary>
    /// Measures memory for 16-color cache with all ANSI colors.
    /// </summary>
    [Benchmark(Description = "16-color cache: all ANSI colors")]
    public long SixteenColorCacheBuild()
    {
        _writer = new StringWriter();
        var output = Vt100Output.FromPty(_writer);

        // Named ANSI colors
        string[] ansiColors =
        [
            "ansiblack", "ansired", "ansigreen", "ansiyellow",
            "ansiblue", "ansimagenta", "ansicyan", "ansigray",
            "ansibrightblack", "ansibrightred", "ansibrightgreen", "ansibrightyellow",
            "ansibrightblue", "ansibrightmagenta", "ansibrightcyan", "ansiwhite"
        ];

        foreach (var color in ansiColors)
        {
            output.SetAttributes(new Attrs(Color: color), ColorDepth.Depth4Bit);
        }

        // Also exercise RGB -> 16 color mapping
        for (var r = 0; r < 256; r += 32)
        {
            for (var g = 0; g < 256; g += 32)
            {
                for (var b = 0; b < 256; b += 32)
                {
                    var hex = $"{r:X2}{g:X2}{b:X2}";
                    output.SetAttributes(new Attrs(Color: hex), ColorDepth.Depth4Bit);
                }
            }
        }

        return _writer.ToString().Length;
    }

    /// <summary>
    /// Measures memory for 24-bit color cache (attribute to escape sequence caching).
    /// </summary>
    [Benchmark(Description = "24-bit cache: 100 unique colors")]
    public long TrueColorCacheBuild()
    {
        _writer = new StringWriter();
        var output = Vt100Output.FromPty(_writer);

        var random = new Random(42);
        for (var i = 0; i < 100; i++)
        {
            var r = random.Next(256);
            var g = random.Next(256);
            var b = random.Next(256);
            var hex = $"{r:X2}{g:X2}{b:X2}";
            output.SetAttributes(new Attrs(Color: hex, Bold: i % 2 == 0), ColorDepth.Depth24Bit);
        }

        return _writer.ToString().Length;
    }
}

/// <summary>
/// Benchmarks for PlainTextOutput and DummyOutput (baseline comparison).
/// Uses IterationSetup to create fresh writers for each iteration to avoid memory accumulation.
/// </summary>
[SimpleJob(RuntimeMoniker.Net10_0)]
[MemoryDiagnoser]
public class OutputImplementationBenchmarks
{
    private Vt100Output _vt100 = null!;
    private PlainTextOutput _plainText = null!;
    private DummyOutput _dummy = null!;
    private readonly Attrs _boldAttrs = new(Bold: true);

    [IterationSetup]
    public void IterationSetup()
    {
        _vt100 = Vt100Output.FromPty(new StringWriter());
        _plainText = new PlainTextOutput(new StringWriter());
        _dummy = new DummyOutput();
    }

    [Benchmark(Baseline = true, Description = "Vt100Output: write sequence")]
    public void Vt100Output_WriteSequence()
    {
        _vt100.Write("Hello");
        _vt100.CursorForward(5);
        _vt100.SetAttributes(_boldAttrs, ColorDepth.Depth8Bit);
        _vt100.WriteRaw("World");
        _vt100.ResetAttributes();
        _vt100.Flush();
    }

    [Benchmark(Description = "PlainTextOutput: write sequence")]
    public void PlainTextOutput_WriteSequence()
    {
        _plainText.Write("Hello");
        _plainText.CursorForward(5);
        _plainText.SetAttributes(_boldAttrs, ColorDepth.Depth8Bit);
        _plainText.WriteRaw("World");
        _plainText.ResetAttributes();
        _plainText.Flush();
    }

    [Benchmark(Description = "DummyOutput: write sequence")]
    public void DummyOutput_WriteSequence()
    {
        _dummy.Write("Hello");
        _dummy.CursorForward(5);
        _dummy.SetAttributes(_boldAttrs, ColorDepth.Depth8Bit);
        _dummy.WriteRaw("World");
        _dummy.ResetAttributes();
        _dummy.Flush();
    }
}
