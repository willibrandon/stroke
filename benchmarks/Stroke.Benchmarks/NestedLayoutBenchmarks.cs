using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Stroke.Layout;
using Stroke.Layout.Containers;
using Stroke.Layout.Controls;

namespace Stroke.Benchmarks;

/// <summary>
/// Benchmarks for nested layout container rendering (SC-001).
/// Tests that deeply nested containers render efficiently.
/// </summary>
[SimpleJob(RuntimeMoniker.Net10_0, iterationCount: 3, warmupCount: 1)]
[MemoryDiagnoser]
public class NestedLayoutBenchmarks
{
    private IContainer _shallowLayout = null!;
    private IContainer _deepLayout = null!;
    private IContainer _wideLayout = null!;

    [GlobalSetup]
    public void Setup()
    {
        // Shallow: 3 levels of nesting
        _shallowLayout = CreateNestedLayout(3);

        // Deep: 10 levels of nesting
        _deepLayout = CreateNestedLayout(10);

        // Wide: 2 levels but many children per level
        _wideLayout = CreateWideLayout(20);
    }

    [Benchmark(Description = "3-level nested HSplit/VSplit")]
    public void ShallowNestedLayout()
    {
        var screen = new Screen();
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 120, 40);

        _shallowLayout.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);
    }

    [Benchmark(Description = "10-level nested HSplit/VSplit")]
    public void DeepNestedLayout()
    {
        var screen = new Screen();
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 120, 40);

        _deepLayout.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);
    }

    [Benchmark(Description = "2-level wide layout (20 children)")]
    public void WideNestedLayout()
    {
        var screen = new Screen();
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 120, 40);

        _wideLayout.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);
    }

    private static IContainer CreateNestedLayout(int depth)
    {
        IContainer current = new Window(content: new FormattedTextControl("Leaf"));

        for (int i = 0; i < depth; i++)
        {
            if (i % 2 == 0)
                current = new HSplit(new IContainer[] { current, new Window(content: new FormattedTextControl($"Level {i}")) });
            else
                current = new VSplit(new IContainer[] { current, new Window(content: new FormattedTextControl($"Level {i}")) });
        }

        return current;
    }

    private static IContainer CreateWideLayout(int childCount)
    {
        var children = new IContainer[childCount];
        for (int i = 0; i < childCount; i++)
        {
            children[i] = new Window(content: new FormattedTextControl($"Child {i}"));
        }

        return new HSplit(children);
    }
}
