using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Stroke.Input;
using Stroke.Input.Pipe;
using Stroke.Input.Vt100;

namespace Stroke.Benchmarks;

/// <summary>
/// Benchmarks for the Input System NFRs.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>NFR-001: Raw mode entry/exit MUST complete within 10ms</item>
/// <item>NFR-002: Escape sequence lookup MUST be O(1) using FrozenDictionary</item>
/// <item>NFR-003: Single character input MUST NOT allocate (steady-state)</item>
/// <item>NFR-004: Parser buffer reuse MUST minimize GC pressure</item>
/// <item>NFR-005: PipeInput MUST support 10,000+ key presses per second</item>
/// </list>
/// </remarks>
[SimpleJob(RuntimeMoniker.Net10_0)]
[MemoryDiagnoser]
public class InputBenchmarks
{
    private SimplePipeInput _pipeInput = null!;
    private Vt100Parser _parser = null!;
    private List<KeyPress> _keyPresses = null!;
    private string _singleChar = null!;
    private string _escapeSequence = null!;
    private string _bulkInput = null!;

    [GlobalSetup]
    public void Setup()
    {
        _pipeInput = new SimplePipeInput();
        _keyPresses = new List<KeyPress>(1000);
        _parser = new Vt100Parser(kp => _keyPresses.Add(kp));
        _singleChar = "a";
        _escapeSequence = "\x1b[A"; // Up arrow

        // Create bulk input for throughput test (10,000 characters)
        _bulkInput = new string('a', 10000);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _pipeInput?.Dispose();
    }

    #region NFR-002: Escape Sequence Lookup O(1)

    /// <summary>
    /// NFR-002: Escape sequence lookup MUST be O(1) using FrozenDictionary.
    /// Tests direct dictionary lookup performance.
    /// </summary>
    [Benchmark(Description = "NFR-002: AnsiSequences.TryGetKey O(1) lookup")]
    public Keys AnsiSequences_TryGetKey()
    {
        AnsiSequences.TryGetKey("\x1b[A", out var key);
        return key;
    }

    /// <summary>
    /// NFR-002: Verify IsPrefixOfLongerSequence is also efficient.
    /// </summary>
    [Benchmark(Description = "NFR-002: AnsiSequences.IsPrefixOfLongerSequence")]
    public bool AnsiSequences_IsPrefixOfLongerSequence()
    {
        return AnsiSequences.IsPrefixOfLongerSequence("\x1b[");
    }

    #endregion

    #region NFR-003 & NFR-004: Single Character & Buffer Reuse

    /// <summary>
    /// NFR-003: Single character input MUST NOT allocate (steady-state).
    /// NFR-004: Parser buffer reuse minimizes GC pressure.
    /// </summary>
    [Benchmark(Description = "NFR-003/004: Parse single character (steady-state)")]
    public int Parser_SingleCharacter()
    {
        _keyPresses.Clear();
        _parser.FeedAndFlush(_singleChar);
        return _keyPresses.Count;
    }

    /// <summary>
    /// NFR-003: Parse escape sequence (steady-state).
    /// </summary>
    [Benchmark(Description = "NFR-003/004: Parse escape sequence (steady-state)")]
    public int Parser_EscapeSequence()
    {
        _keyPresses.Clear();
        _parser.FeedAndFlush(_escapeSequence);
        return _keyPresses.Count;
    }

    #endregion

    #region NFR-005: PipeInput Throughput

    /// <summary>
    /// NFR-005: PipeInput MUST support 10,000+ key presses per second.
    /// This benchmark tests the raw throughput of SendText + ReadKeys.
    /// </summary>
    [Benchmark(Description = "NFR-005: PipeInput 10,000 characters throughput")]
    public int PipeInput_TenThousandKeys()
    {
        _pipeInput.SendText(_bulkInput);
        var keys = _pipeInput.ReadKeys();
        return keys.Count;
    }

    /// <summary>
    /// NFR-005: Test incremental reads (more realistic pattern).
    /// </summary>
    [Benchmark(Description = "NFR-005: PipeInput 100 reads of 100 chars")]
    public int PipeInput_IncrementalReads()
    {
        var total = 0;
        var chunk = new string('x', 100);
        for (int i = 0; i < 100; i++)
        {
            _pipeInput.SendText(chunk);
            total += _pipeInput.ReadKeys().Count;
        }
        return total;
    }

    #endregion
}

/// <summary>
/// Benchmarks for parser scalability with various input sizes.
/// </summary>
[SimpleJob(RuntimeMoniker.Net10_0)]
[MemoryDiagnoser]
public class ParserScalingBenchmarks
{
    private Vt100Parser _parser = null!;
    private List<KeyPress> _keyPresses = null!;

    [Params(10, 100, 1000, 10000)]
    public int InputSize { get; set; }

    private string _input = null!;
    private string _mixedInput = null!;

    [GlobalSetup]
    public void Setup()
    {
        _keyPresses = new List<KeyPress>(InputSize);
        _parser = new Vt100Parser(kp => _keyPresses.Add(kp));

        // Plain text input
        _input = new string('a', InputSize);

        // Mixed input with escape sequences
        var mixed = new System.Text.StringBuilder();
        for (int i = 0; i < InputSize; i++)
        {
            if (i % 10 == 0)
                mixed.Append("\x1b[A"); // Arrow key every 10 chars
            else
                mixed.Append((char)('a' + (i % 26)));
        }
        _mixedInput = mixed.ToString();
    }

    [Benchmark(Description = "Plain text parsing")]
    public int ParsePlainText()
    {
        _keyPresses.Clear();
        _parser.FeedAndFlush(_input);
        return _keyPresses.Count;
    }

    [Benchmark(Description = "Mixed text + escape sequences")]
    public int ParseMixedInput()
    {
        _keyPresses.Clear();
        _parser.FeedAndFlush(_mixedInput);
        return _keyPresses.Count;
    }
}
