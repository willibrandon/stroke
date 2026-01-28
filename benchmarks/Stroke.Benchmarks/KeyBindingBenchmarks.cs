using System.Collections.Immutable;
using BenchmarkDotNet.Attributes;
using Stroke.Filters;
using Stroke.Input;
using Stroke.KeyBinding;

namespace Stroke.Benchmarks;

/// <summary>
/// Benchmarks for the key binding system per SC-001, SC-002, SC-006.
/// </summary>
/// <remarks>
/// <para>SC-001: GetBindingsForKeys &lt;1ms p99 for 1000 bindings with warm cache</para>
/// <para>SC-002: &gt;95% cache hit rate for 100 bindings, 20 queries round-robin</para>
/// <para>SC-006: GetBindingsForKeys &lt;10ms p99 for 10,000 bindings</para>
/// </remarks>
[SimpleJob(BenchmarkDotNet.Jobs.RuntimeMoniker.Net10_0)]
[MemoryDiagnoser]
public class KeyBindingBenchmarks
{
    private KeyBindings _smallRegistry = null!;  // 100 bindings
    private KeyBindings _mediumRegistry = null!; // 1000 bindings
    private KeyBindings _largeRegistry = null!;  // 10000 bindings
    private ImmutableArray<KeyOrChar>[] _queryKeys = null!;

    [GlobalSetup]
    public void Setup()
    {
        _smallRegistry = CreateRegistry(100);
        _mediumRegistry = CreateRegistry(1000);
        _largeRegistry = CreateRegistry(10000);

        // Create 20 query keys for round-robin testing
        _queryKeys = new ImmutableArray<KeyOrChar>[20];
        for (int i = 0; i < 20; i++)
        {
            _queryKeys[i] = [Keys.ControlA + i];
        }

        // Warm the caches
        foreach (var keys in _queryKeys)
        {
            _smallRegistry.GetBindingsForKeys(keys);
            _mediumRegistry.GetBindingsForKeys(keys);
            _largeRegistry.GetBindingsForKeys(keys);
        }
    }

    private static KeyBindings CreateRegistry(int bindingCount)
    {
        var registry = new KeyBindings();

        for (int i = 0; i < bindingCount; i++)
        {
            KeyOrChar[] keys = i switch
            {
                < 26 => [Keys.ControlA + i],
                < 38 => [Keys.F1 + (i - 26)],
                _ => [(char)('a' + (i % 26))] // Use character keys for the rest
            };

            registry.Add<KeyHandlerCallable>(keys)(_ => null);
        }

        return registry;
    }

    /// <summary>
    /// SC-001: GetBindingsForKeys with 1000 bindings (warm cache).
    /// Target: &lt;1ms p99
    /// </summary>
    [Benchmark(Description = "GetBindingsForKeys (1000 bindings, warm cache)")]
    public IReadOnlyList<Binding> GetBindingsForKeys_1000_WarmCache()
    {
        return _mediumRegistry.GetBindingsForKeys(_queryKeys[0]);
    }

    /// <summary>
    /// SC-002: Cache hit rate test - 20 queries round-robin on 100 bindings.
    /// Target: &gt;95% hit rate
    /// </summary>
    [Benchmark(Description = "GetBindingsForKeys round-robin x20 (100 bindings)")]
    public int GetBindingsForKeys_RoundRobin_100()
    {
        var count = 0;
        for (int i = 0; i < 20; i++)
        {
            var result = _smallRegistry.GetBindingsForKeys(_queryKeys[i % 20]);
            count += result.Count;
        }
        return count;
    }

    /// <summary>
    /// SC-006: GetBindingsForKeys with 10,000 bindings.
    /// Target: &lt;10ms p99
    /// </summary>
    [Benchmark(Description = "GetBindingsForKeys (10000 bindings, warm cache)")]
    public IReadOnlyList<Binding> GetBindingsForKeys_10000_WarmCache()
    {
        return _largeRegistry.GetBindingsForKeys(_queryKeys[0]);
    }

    /// <summary>
    /// Cache miss scenario - query for non-existent keys.
    /// </summary>
    [Benchmark(Description = "GetBindingsForKeys cache miss (1000 bindings)")]
    public IReadOnlyList<Binding> GetBindingsForKeys_CacheMiss()
    {
        // Create new keys not in cache
        ImmutableArray<KeyOrChar> newKeys = [Keys.Escape, Keys.ControlC];
        return _mediumRegistry.GetBindingsForKeys(newKeys);
    }

    /// <summary>
    /// GetBindingsStartingWithKeys performance.
    /// </summary>
    [Benchmark(Description = "GetBindingsStartingWithKeys (1000 bindings)")]
    public IReadOnlyList<Binding> GetBindingsStartingWithKeys_1000()
    {
        return _mediumRegistry.GetBindingsStartingWithKeys(_queryKeys[0]);
    }

    /// <summary>
    /// Version property access.
    /// </summary>
    [Benchmark(Description = "Version property access x1000")]
    public int Version_Access_1000()
    {
        var count = 0;
        for (int i = 0; i < 1000; i++)
        {
            var version = _mediumRegistry.Version;
            if (version is int v) count += v;
        }
        return count;
    }

    /// <summary>
    /// Bindings snapshot access.
    /// </summary>
    [Benchmark(Description = "Bindings snapshot access")]
    public IReadOnlyList<Binding> Bindings_Snapshot()
    {
        return _mediumRegistry.Bindings;
    }
}

/// <summary>
/// Benchmarks for key binding add/remove operations.
/// </summary>
[SimpleJob(BenchmarkDotNet.Jobs.RuntimeMoniker.Net10_0)]
[MemoryDiagnoser]
public class KeyBindingMutationBenchmarks
{
    private KeyBindings _registry = null!;
    private KeyHandlerCallable _handler = null!;
    private int _keyCounter;

    [GlobalSetup]
    public void Setup()
    {
        _handler = _ => null;
        _keyCounter = 0;
    }

    [IterationSetup]
    public void IterationSetup()
    {
        _registry = new KeyBindings();
        _keyCounter = 0;
    }

    /// <summary>
    /// Add binding performance.
    /// </summary>
    [Benchmark(Description = "Add binding x100")]
    public int Add_Binding_100()
    {
        var count = 0;
        for (int i = 0; i < 100; i++)
        {
            KeyOrChar[] keys = [(Keys)(_keyCounter++ + 1000)];
            _registry.Add<KeyHandlerCallable>(keys)(_handler);
            count++;
        }
        return count;
    }

    /// <summary>
    /// Add binding with filter.
    /// </summary>
    [Benchmark(Description = "Add binding with filter x100")]
    public int Add_BindingWithFilter_100()
    {
        var count = 0;
        var filter = new Condition(() => true);
        for (int i = 0; i < 100; i++)
        {
            KeyOrChar[] keys = [(Keys)(_keyCounter++ + 1000)];
            _registry.Add<KeyHandlerCallable>(keys, filter: filter)(_handler);
            count++;
        }
        return count;
    }
}

/// <summary>
/// Benchmarks for merged and conditional key bindings.
/// </summary>
[SimpleJob(BenchmarkDotNet.Jobs.RuntimeMoniker.Net10_0)]
[MemoryDiagnoser]
public class KeyBindingProxyBenchmarks
{
    private KeyBindings _registry1 = null!;
    private KeyBindings _registry2 = null!;
    private MergedKeyBindings _merged = null!;
    private ConditionalKeyBindings _conditional = null!;
    private GlobalOnlyKeyBindings _globalOnly = null!;
    private DynamicKeyBindings _dynamic = null!;
    private ImmutableArray<KeyOrChar> _queryKey;

    [GlobalSetup]
    public void Setup()
    {
        _registry1 = new KeyBindings();
        _registry2 = new KeyBindings();

        // Add 50 bindings to each registry
        for (int i = 0; i < 50; i++)
        {
            KeyOrChar[] keys = [Keys.ControlA + i];
            _registry1.Add<KeyHandlerCallable>(keys)(_ => null);
            _registry2.Add<KeyHandlerCallable>(keys, isGlobal: true)(_ => null);
        }

        _merged = new MergedKeyBindings([_registry1, _registry2]);
        _conditional = new ConditionalKeyBindings(_registry1, new Condition(() => true));
        _globalOnly = new GlobalOnlyKeyBindings(_registry2);
        _dynamic = new DynamicKeyBindings(() => _registry1);

        _queryKey = [Keys.ControlC];

        // Warm caches
        _merged.GetBindingsForKeys(_queryKey);
        _conditional.GetBindingsForKeys(_queryKey);
        _globalOnly.GetBindingsForKeys(_queryKey);
        _dynamic.GetBindingsForKeys(_queryKey);
    }

    /// <summary>
    /// MergedKeyBindings lookup.
    /// </summary>
    [Benchmark(Description = "MergedKeyBindings.GetBindingsForKeys")]
    public IReadOnlyList<Binding> Merged_GetBindingsForKeys()
    {
        return _merged.GetBindingsForKeys(_queryKey);
    }

    /// <summary>
    /// ConditionalKeyBindings lookup.
    /// </summary>
    [Benchmark(Description = "ConditionalKeyBindings.GetBindingsForKeys")]
    public IReadOnlyList<Binding> Conditional_GetBindingsForKeys()
    {
        return _conditional.GetBindingsForKeys(_queryKey);
    }

    /// <summary>
    /// GlobalOnlyKeyBindings lookup.
    /// </summary>
    [Benchmark(Description = "GlobalOnlyKeyBindings.GetBindingsForKeys")]
    public IReadOnlyList<Binding> GlobalOnly_GetBindingsForKeys()
    {
        return _globalOnly.GetBindingsForKeys(_queryKey);
    }

    /// <summary>
    /// DynamicKeyBindings lookup.
    /// </summary>
    [Benchmark(Description = "DynamicKeyBindings.GetBindingsForKeys")]
    public IReadOnlyList<Binding> Dynamic_GetBindingsForKeys()
    {
        return _dynamic.GetBindingsForKeys(_queryKey);
    }
}
