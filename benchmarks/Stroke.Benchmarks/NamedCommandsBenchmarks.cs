using BenchmarkDotNet.Attributes;
using Stroke.Core;
using Stroke.KeyBinding;
using Stroke.KeyBinding.Bindings;
using Buffer = Stroke.Core.Buffer;
using Keys = Stroke.Input.Keys;

namespace Stroke.Benchmarks;

/// <summary>
/// Benchmarks for the NamedCommands static registry: lookup, dispatch, and registration.
/// </summary>
[SimpleJob(BenchmarkDotNet.Jobs.RuntimeMoniker.Net10_0)]
[MemoryDiagnoser]
public class NamedCommandsBenchmarks
{
    private Binding _cachedBinding = null!;
    private KeyPressEvent _event = null!;
    private Buffer _buffer = null!;

    [GlobalSetup]
    public void Setup()
    {
        _buffer = new Buffer(document: new Document("hello", cursorPosition: 5));
        _cachedBinding = NamedCommands.GetByName("forward-char");
        _event = new KeyPressEvent(
            keyProcessorRef: null,
            arg: null,
            keySequence: [new KeyPress(Keys.Any)],
            previousKeySequence: [],
            isRepeat: false,
            app: null,
            currentBuffer: _buffer);
    }

    /// <summary>
    /// Single GetByName lookup from the ConcurrentDictionary.
    /// </summary>
    [Benchmark(Description = "GetByName single lookup")]
    public Binding GetByName_Single()
    {
        return NamedCommands.GetByName("forward-char");
    }

    /// <summary>
    /// Lookup + Call dispatch (forward-char at end of buffer = no-op).
    /// </summary>
    [Benchmark(Description = "GetByName + Call dispatch")]
    public void GetByName_AndCall()
    {
        var binding = NamedCommands.GetByName("forward-char");
        binding.Call(_event);
    }

    /// <summary>
    /// Call dispatch only, with pre-cached binding reference.
    /// </summary>
    [Benchmark(Description = "Call dispatch (cached binding)")]
    public void Call_CachedBinding()
    {
        _cachedBinding.Call(_event);
    }

    /// <summary>
    /// Register a new command (overwrites each iteration).
    /// </summary>
    [Benchmark(Description = "Register command")]
    public void Register_Command()
    {
        NamedCommands.Register("bench-temp", _ => null);
    }
}
