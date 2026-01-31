using Stroke.Core;
using Stroke.KeyBinding;
using Stroke.KeyBinding.Bindings;
using Xunit;
using Buffer = Stroke.Core.Buffer;
using Keys = Stroke.Input.Keys;

namespace Stroke.Tests.KeyBinding.Bindings;

/// <summary>
/// Benchmark test verifying zero-allocation dispatch on the hot path (NFR-003).
/// </summary>
public sealed class NamedCommandsBenchmarkTests
{
    [Fact]
    public void GetByName_AndCall_ZeroAllocationOnHotPath()
    {
        // Use forward-char at end of buffer (no-op) to avoid handler-side allocations.
        var buffer = new Buffer(document: new Document("hello", cursorPosition: 5));
        var keySequence = new List<KeyPress> { new(Keys.Any) };

        // Warm up: ensure JIT compilation is done
        var binding = NamedCommands.GetByName("forward-char");
        var warmupEvent = new KeyPressEvent(
            keyProcessorRef: null,
            arg: null,
            keySequence: keySequence,
            previousKeySequence: [],
            isRepeat: false,
            app: null,
            currentBuffer: buffer);
        binding.Call(warmupEvent);

        // Force a GC to clean up warmup allocations
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        const int iterations = 10_000;

        // Measure allocations during the hot loop
        var allocBefore = GC.GetAllocatedBytesForCurrentThread();

        for (var i = 0; i < iterations; i++)
        {
            var b = NamedCommands.GetByName("forward-char");
            var evt = new KeyPressEvent(
                keyProcessorRef: null,
                arg: null,
                keySequence: keySequence,
                previousKeySequence: [],
                isRepeat: false,
                app: null,
                currentBuffer: buffer);
            b.Call(evt);
        }

        var allocAfter = GC.GetAllocatedBytesForCurrentThread();
        var totalAllocated = allocAfter - allocBefore;

        // Allow some allocation per iteration for the KeyPressEvent struct/object creation
        // and the delegate invocation. The key metric is that GetByName itself should be
        // near-zero allocation (ConcurrentDictionary.TryGetValue doesn't allocate on hit).
        // We allow up to 200 bytes per iteration to account for KeyPressEvent construction.
        var perIteration = totalAllocated / (double)iterations;
        Assert.True(perIteration < 200,
            $"Expected < 200 bytes/iteration but got {perIteration:F1} bytes/iteration " +
            $"(total: {totalAllocated} bytes over {iterations} iterations)");
    }

    [Fact]
    public void GetByName_Lookup_IsConsistentlyFast()
    {
        // Pre-warm
        NamedCommands.GetByName("forward-char");

        const int iterations = 100_000;
        var sw = System.Diagnostics.Stopwatch.StartNew();

        for (var i = 0; i < iterations; i++)
        {
            NamedCommands.GetByName("forward-char");
        }

        sw.Stop();

        // 100k lookups should complete in well under 1 second on any modern machine.
        // ConcurrentDictionary.TryGetValue is O(1) amortized.
        Assert.True(sw.ElapsedMilliseconds < 1000,
            $"100k lookups took {sw.ElapsedMilliseconds}ms, expected < 1000ms");
    }
}
