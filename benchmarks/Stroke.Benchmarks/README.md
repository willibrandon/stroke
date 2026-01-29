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
dotnet run -c Release --filter '*OutputBenchmarks*'
dotnet run -c Release --filter '*KeyBinding*'
dotnet run -c Release --filter '*Lexer*'
dotnet run -c Release --filter '*Grammar*'
dotnet run -c Release --filter '*Matching*'

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

### Output System (NFR-003)

| Benchmark | Target | Description |
|-----------|--------|-------------|
| Color cache memory | ≤10KB | NFR-003: Typical use case memory budget |
| SetAttributes (24-bit) | O(1) cache | Escape code cache lookup |
| SetAttributes (256-color) | O(1) cache | Color palette mapping |
| SetAttributes (16-color) | O(1) cache | Nearest color search |
| Cache hits (after warmup) | 0 allocs | Steady-state should not allocate |

### Key Bindings System (SC-001, SC-002, SC-006)

| Benchmark | Target | Measured |
|-----------|--------|----------|
| GetBindingsForKeys (1000 bindings, warm cache) | <1ms p99 | ~16 ns |
| GetBindingsForKeys round-robin x20 (100 bindings) | >95% cache hit | ~231 ns |
| GetBindingsForKeys (10000 bindings, warm cache) | <10ms p99 | ~17 ns |
| GetBindingsForKeys cache miss (1000 bindings) | N/A | ~1.5 μs |
| GetBindingsStartingWithKeys (1000 bindings) | N/A | ~3.4 μs |
| Version property access x1000 | N/A | ~0.23 μs |
| MergedKeyBindings.GetBindingsForKeys | N/A | ~34 ns |
| ConditionalKeyBindings.GetBindingsForKeys | N/A | ~24 ns |
| GlobalOnlyKeyBindings.GetBindingsForKeys | N/A | ~35 ns |
| DynamicKeyBindings.GetBindingsForKeys | N/A | ~42 ns |

### Lexer System (SC-001, SC-004)

| Benchmark | Target | Measured |
|-----------|--------|----------|
| SimpleLexer single line | ≤1ms/line | ~16 ns |
| SimpleLexer 10K lines | ≤1ms/line | ~4.9 ns/line |
| PygmentsLexer cached access (cache hit) | O(1) | ~4.1 ns |
| PygmentsLexer first access (cache miss) | N/A | ~13.9 ms |
| PygmentsLexer sequential 100 lines | N/A | ~13.5 ms |
| DynamicLexer delegation overhead | N/A | ~27 ns |
| RegexSync find sync position | N/A | ~654 μs |

### Regular Languages (SC-002, SC-003, SC-004)

| Benchmark | Target | Measured |
|-----------|--------|----------|
| CompileSimplePattern | N/A | ~24 μs |
| CompilePatternWithVariables | N/A | ~67 μs |
| CompileComplexPattern | N/A | ~128 μs |
| MatchSimple | N/A | ~43 ns |
| MatchWithVariables | N/A | ~56 ns |
| MatchPrefix | N/A | ~122 ns |
| GetSimpleCompletions | N/A | ~652 ns |
| GetComplexCompletions | N/A | ~14 μs |

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

### Output System
- **OutputBenchmarks** - Basic output operations (write, cursor movement, SetAttributes)
- **ColorCacheMemoryBenchmarks** - Memory footprint with N unique colors (10, 50, 100)
- **ColorCacheStressBenchmarks** - Full cache build scenarios (256-color, 16-color, 24-bit)
- **OutputImplementationBenchmarks** - Comparison of Vt100Output, PlainTextOutput, DummyOutput

### Key Bindings System
- **KeyBindingBenchmarks** - Core key binding lookup benchmarks (SC-001, SC-002, SC-006)
- **KeyBindingMutationBenchmarks** - Add binding performance with and without filters
- **KeyBindingProxyBenchmarks** - Merged, Conditional, GlobalOnly, Dynamic proxy benchmarks

### Lexer System
- **LexerBenchmarks** - Core lexer operations: SimpleLexer, PygmentsLexer cache hit/miss, DynamicLexer delegation, RegexSync
- **LexerScalingBenchmarks** - Lexer scaling with 100/1K/10K line documents

### Regular Languages
- **GrammarCompilationBenchmarks** - Grammar compilation with simple, complex, nested, and escape function patterns (SC-002)
- **MatchingBenchmarks** - Pattern matching, variable extraction, prefix matching, EndNodes, TrailingInput (SC-003)
- **GrammarCompletionBenchmarks** - Grammar-based completion with simple and complex grammars (SC-004)
