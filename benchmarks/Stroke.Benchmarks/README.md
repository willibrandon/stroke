# Stroke.Benchmarks

Performance benchmarks for the Stroke library using [BenchmarkDotNet](https://benchmarkdotnet.org/).

## Running Benchmarks

```bash
# Run all benchmarks
dotnet run -c Release

# Run specific benchmark class
dotnet run -c Release --filter '*CompletionBenchmarks*'
dotnet run -c Release --filter '*PathCompleterBenchmarks*'
dotnet run -c Release --filter '*WordCompleterScalingBenchmarks*'
dotnet run -c Release --filter '*FuzzyCompleterScalingBenchmarks*'
dotnet run -c Release --filter '*FilterBenchmarks*'
dotnet run -c Release --filter '*FilterCreationBenchmarks*'
dotnet run -c Release --filter '*InputBenchmarks*'
dotnet run -c Release --filter '*ParserScalingBenchmarks*'
dotnet run -c Release --filter '*FormattedTextBenchmarks*'
dotnet run -c Release --filter '*TemplateScalingBenchmarks*'
dotnet run -c Release --filter '*AnyFormattedTextBenchmarks*'
dotnet run -c Release --filter '*FormattedTextUtilsBenchmarks*'

# List available benchmarks
dotnet run -c Release --list flat
```

## Performance Targets

### Completion System (SC-012)

| Benchmark | Target | Measured |
|-----------|--------|----------|
| WordCompleter (10K words) | ≤100ms | ~108 μs |
| PathCompleter (1K files) | ≤200ms | ~1.76 ms |
| FuzzyCompleter overhead | ≤50ms | ~4.3 ms |
| ThreadedCompleter first completion | ≤10ms | ~501 μs |

### Filter System (SC-002)

| Benchmark | Target | Measured |
|-----------|--------|----------|
| Condition.Invoke() x1000 | ≤1ms | ~0.28 μs |
| Always.Invoke() x1000 | ≤1ms | ~0.23 μs |
| Never.Invoke() x1000 | ≤1ms | ~0.24 μs |
| Cached And() lookup x1000 | ≤1ms | ~3.8 μs |
| Cached Or() lookup x1000 | ≤1ms | ~4.6 μs |
| Cached Invert() lookup x1000 | ≤1ms | ~5.0 μs |
| Complex expression Invoke() x1000 | ≤1ms | ~21 μs |
| Deep chain (10 ANDs) Invoke() x1000 | ≤1ms | ~24 μs |

### Input System (NFR-001 to NFR-005)

| Benchmark | Target | Description |
|-----------|--------|-------------|
| AnsiSequences.TryGetKey | O(1) | NFR-002: FrozenDictionary lookup |
| AnsiSequences.IsPrefixOfLongerSequence | O(1) | NFR-002: Prefix check |
| Parse single character | No alloc | NFR-003/004: Steady-state parsing |
| Parse escape sequence | No alloc | NFR-003/004: Steady-state parsing |
| PipeInput 10K characters | 10K+/sec | NFR-005: Throughput test |

### FormattedText System

| Benchmark | Description |
|-----------|-------------|
| Plain text 1KB/5KB/10KB | T110: ToFormattedText conversion |
| HTML 100KB parsing | T111: HTML markup parsing |
| ANSI 1KB/10KB/100KB parsing | T112: ANSI escape sequence parsing |

## Benchmark Classes

### Completion System
- **CompletionBenchmarks** - Core completion system benchmarks validating spec requirements
- **PathCompleterBenchmarks** - Filesystem completion with controlled temp directory
- **WordCompleterScalingBenchmarks** - Word completion scaling (100, 1K, 10K words)
- **FuzzyCompleterScalingBenchmarks** - Fuzzy matching scaling (100, 1K, 10K words)

### Filter System
- **FilterBenchmarks** - Filter evaluation, caching, short-circuit, and utility benchmarks (SC-002)
- **FilterCreationBenchmarks** - Uncached filter instance creation benchmarks

### Input System
- **InputBenchmarks** - Escape sequence lookup, parser steady-state, PipeInput throughput (NFR-001 to NFR-005)
- **ParserScalingBenchmarks** - Vt100Parser scaling with 10/100/1K/10K input sizes

### FormattedText System
- **FormattedTextBenchmarks** - Plain text, HTML, and ANSI parsing benchmarks (T110-T112)
- **TemplateScalingBenchmarks** - Template interpolation with 5/50 placeholders
- **AnyFormattedTextBenchmarks** - Implicit conversion overhead from string/Html/Ansi/FormattedText
- **FormattedTextUtilsBenchmarks** - FragmentListToText, FragmentListWidth, SplitLines with 10/1000 fragments
