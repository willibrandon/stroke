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

# List available benchmarks
dotnet run -c Release --list flat
```

## Performance Targets

From the completion system spec:

| Benchmark | Target | Measured |
|-----------|--------|----------|
| WordCompleter (10K words) | ≤100ms | ~108 μs |
| PathCompleter (1K files) | ≤200ms | ~1.76 ms |
| FuzzyCompleter overhead | ≤50ms | ~4.3 ms |
| ThreadedCompleter first completion | ≤10ms | ~501 μs |

## Benchmark Classes

- **CompletionBenchmarks** - Core completion system benchmarks validating spec requirements
- **PathCompleterBenchmarks** - Filesystem completion with controlled temp directory
- **WordCompleterScalingBenchmarks** - Word completion scaling (100, 1K, 10K words)
- **FuzzyCompleterScalingBenchmarks** - Fuzzy matching scaling (100, 1K, 10K words)
