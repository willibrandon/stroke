using BenchmarkDotNet.Attributes;
using Stroke.Filters;

namespace Stroke.Benchmarks;

/// <summary>
/// Benchmarks for the filter system per SC-002.
/// Target: Filter combinations with 1000+ operations should complete evaluation in under 1ms.
/// </summary>
[SimpleJob(BenchmarkDotNet.Jobs.RuntimeMoniker.Net10_0)]
[MemoryDiagnoser]
public class FilterBenchmarks
{
    private Condition _trueCondition = null!;
    private Condition _falseCondition = null!;
    private IFilter _andFilter = null!;
    private IFilter _orFilter = null!;
    private IFilter _complexExpression = null!;
    private IFilter _deepChain = null!;

    [GlobalSetup]
    public void Setup()
    {
        _trueCondition = new Condition(() => true);
        _falseCondition = new Condition(() => false);

        // Pre-create combined filters
        _andFilter = _trueCondition.And(_falseCondition);
        _orFilter = _falseCondition.Or(_trueCondition);

        // Complex expression: (a & b) | c
        var a = new Condition(() => true);
        var b = new Condition(() => false);
        var c = new Condition(() => true);
        _complexExpression = ((Filter)a.And(b)).Or(c);

        // Deep chain of 10 ANDed conditions
        var conditions = Enumerable.Range(0, 10).Select(_ => new Condition(() => true)).ToArray();
        IFilter chain = conditions[0].And(conditions[1]);
        for (int i = 2; i < conditions.Length; i++)
        {
            chain = ((Filter)chain).And(conditions[i]);
        }
        _deepChain = chain;
    }

    /// <summary>
    /// SC-002: Condition.Invoke() - 1000 operations.
    /// </summary>
    [Benchmark(Description = "Condition.Invoke() x1000")]
    public int Condition_Invoke_1000()
    {
        var count = 0;
        for (int i = 0; i < 1000; i++)
        {
            if (_trueCondition.Invoke()) count++;
        }
        return count;
    }

    /// <summary>
    /// SC-002: Always.Invoke() - 1000 operations.
    /// </summary>
    [Benchmark(Description = "Always.Invoke() x1000")]
    public int Always_Invoke_1000()
    {
        var count = 0;
        for (int i = 0; i < 1000; i++)
        {
            if (Always.Instance.Invoke()) count++;
        }
        return count;
    }

    /// <summary>
    /// SC-002: Never.Invoke() - 1000 operations.
    /// </summary>
    [Benchmark(Description = "Never.Invoke() x1000")]
    public int Never_Invoke_1000()
    {
        var count = 0;
        for (int i = 0; i < 1000; i++)
        {
            if (!Never.Instance.Invoke()) count++;
        }
        return count;
    }

    /// <summary>
    /// SC-002: Cached And() lookup - 1000 operations.
    /// </summary>
    [Benchmark(Description = "Cached And() lookup x1000")]
    public int And_CachedLookup_1000()
    {
        var count = 0;
        for (int i = 0; i < 1000; i++)
        {
            var result = _trueCondition.And(_falseCondition);
            if (ReferenceEquals(result, _andFilter)) count++;
        }
        return count;
    }

    /// <summary>
    /// SC-002: Cached Or() lookup - 1000 operations.
    /// </summary>
    [Benchmark(Description = "Cached Or() lookup x1000")]
    public int Or_CachedLookup_1000()
    {
        var count = 0;
        for (int i = 0; i < 1000; i++)
        {
            var result = _falseCondition.Or(_trueCondition);
            if (ReferenceEquals(result, _orFilter)) count++;
        }
        return count;
    }

    /// <summary>
    /// SC-002: Cached Invert() lookup - 1000 operations.
    /// </summary>
    [Benchmark(Description = "Cached Invert() lookup x1000")]
    public int Invert_CachedLookup_1000()
    {
        var inverted = _trueCondition.Invert();
        var count = 0;
        for (int i = 0; i < 1000; i++)
        {
            var result = _trueCondition.Invert();
            if (ReferenceEquals(result, inverted)) count++;
        }
        return count;
    }

    /// <summary>
    /// SC-002: Complex expression evaluation - 1000 operations.
    /// </summary>
    [Benchmark(Description = "Complex expression Invoke() x1000")]
    public int ComplexExpression_Invoke_1000()
    {
        var count = 0;
        for (int i = 0; i < 1000; i++)
        {
            if (_complexExpression.Invoke()) count++;
        }
        return count;
    }

    /// <summary>
    /// SC-002: Deep chain (10 ANDs) evaluation - 1000 operations.
    /// </summary>
    [Benchmark(Description = "Deep chain (10 ANDs) Invoke() x1000")]
    public int DeepChain_Invoke_1000()
    {
        var count = 0;
        for (int i = 0; i < 1000; i++)
        {
            if (_deepChain.Invoke()) count++;
        }
        return count;
    }

    /// <summary>
    /// SC-002: Short-circuit AND (first false) - 1000 operations.
    /// </summary>
    [Benchmark(Description = "Short-circuit And (first false) x1000")]
    public int ShortCircuit_And_FirstFalse_1000()
    {
        var falseFirst = new Condition(() => false);
        var expensive = new Condition(() => { Thread.SpinWait(100); return true; });
        var andFilter = (Filter)falseFirst.And(expensive);

        var count = 0;
        for (int i = 0; i < 1000; i++)
        {
            if (!andFilter.Invoke()) count++;
        }
        return count;
    }

    /// <summary>
    /// SC-002: Short-circuit OR (first true) - 1000 operations.
    /// </summary>
    [Benchmark(Description = "Short-circuit Or (first true) x1000")]
    public int ShortCircuit_Or_FirstTrue_1000()
    {
        var trueFirst = new Condition(() => true);
        var expensive = new Condition(() => { Thread.SpinWait(100); return false; });
        var orFilter = (Filter)trueFirst.Or(expensive);

        var count = 0;
        for (int i = 0; i < 1000; i++)
        {
            if (orFilter.Invoke()) count++;
        }
        return count;
    }

    /// <summary>
    /// SC-002: FilterUtils.ToFilter() - 1000 operations.
    /// </summary>
    [Benchmark(Description = "FilterUtils.ToFilter(bool) x1000")]
    public int ToFilter_Bool_1000()
    {
        var count = 0;
        for (int i = 0; i < 1000; i++)
        {
            var filter = FilterUtils.ToFilter(i % 2 == 0);
            if (filter == Always.Instance || filter == Never.Instance) count++;
        }
        return count;
    }

    /// <summary>
    /// SC-002: FilterUtils.IsTrue() - 1000 operations.
    /// </summary>
    [Benchmark(Description = "FilterUtils.IsTrue(bool) x1000")]
    public int IsTrue_Bool_1000()
    {
        var count = 0;
        for (int i = 0; i < 1000; i++)
        {
            if (FilterUtils.IsTrue(i % 2 == 0)) count++;
        }
        return count;
    }

    /// <summary>
    /// SC-002: FilterUtils.IsTrue(filter) - 1000 operations.
    /// </summary>
    [Benchmark(Description = "FilterUtils.IsTrue(filter) x1000")]
    public int IsTrue_Filter_1000()
    {
        var count = 0;
        for (int i = 0; i < 1000; i++)
        {
            if (FilterUtils.IsTrue(_trueCondition)) count++;
        }
        return count;
    }
}

/// <summary>
/// Benchmarks for filter combination creation (not cached).
/// </summary>
[SimpleJob(BenchmarkDotNet.Jobs.RuntimeMoniker.Net10_0)]
[MemoryDiagnoser]
public class FilterCreationBenchmarks
{
    /// <summary>
    /// Creating new Condition instances.
    /// </summary>
    [Benchmark(Description = "new Condition() x1000")]
    public int Condition_Creation_1000()
    {
        var count = 0;
        for (int i = 0; i < 1000; i++)
        {
            var condition = new Condition(() => true);
            if (condition.Invoke()) count++;
        }
        return count;
    }

    /// <summary>
    /// Creating new And combinations (uncached - different filter instances each time).
    /// </summary>
    [Benchmark(Description = "And() uncached creation x1000")]
    public int And_Uncached_1000()
    {
        var count = 0;
        for (int i = 0; i < 1000; i++)
        {
            var a = new Condition(() => true);
            var b = new Condition(() => true);
            var combined = a.And(b);
            if (combined.Invoke()) count++;
        }
        return count;
    }

    /// <summary>
    /// Creating new Or combinations (uncached - different filter instances each time).
    /// </summary>
    [Benchmark(Description = "Or() uncached creation x1000")]
    public int Or_Uncached_1000()
    {
        var count = 0;
        for (int i = 0; i < 1000; i++)
        {
            var a = new Condition(() => false);
            var b = new Condition(() => true);
            var combined = a.Or(b);
            if (combined.Invoke()) count++;
        }
        return count;
    }
}
