using BenchmarkDotNet.Running;
using Stroke.Benchmarks;

BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, new StrokeBenchmarkConfig());
