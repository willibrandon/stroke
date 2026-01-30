using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Stroke.Layout;
using Stroke.Layout.Containers;
using Stroke.Layout.Controls;

namespace Stroke.Benchmarks;

/// <summary>
/// Benchmarks for rendering many containers simultaneously (SC-002).
/// Targets: 50 containers rendering in under 50ms.
/// </summary>
[SimpleJob(RuntimeMoniker.Net10_0, iterationCount: 3, warmupCount: 1)]
[MemoryDiagnoser]
public class ManyContainersBenchmarks
{
    private IContainer _fiftyContainers = null!;
    private IContainer _hundredContainers = null!;
    private IContainer _mixedContainers = null!;

    [GlobalSetup]
    public void Setup()
    {
        _fiftyContainers = CreateContainerGrid(50);
        _hundredContainers = CreateContainerGrid(100);
        _mixedContainers = CreateMixedContainerGrid(50);
    }

    [Benchmark(Description = "50 Window containers in HSplit")]
    public void FiftyContainers()
    {
        var screen = new Screen();
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 200, 80);

        _fiftyContainers.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);
    }

    [Benchmark(Description = "100 Window containers in HSplit")]
    public void HundredContainers()
    {
        var screen = new Screen();
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 200, 100);

        _hundredContainers.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);
    }

    [Benchmark(Description = "50 mixed containers (HSplit + VSplit + Float)")]
    public void MixedContainers()
    {
        var screen = new Screen();
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 200, 80);

        _mixedContainers.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);
        screen.DrawAllFloats();
    }

    private static IContainer CreateContainerGrid(int count)
    {
        var children = new IContainer[count];
        for (int i = 0; i < count; i++)
        {
            children[i] = new Window(content: new FormattedTextControl($"Window {i}: Some content here"));
        }

        return new HSplit(children);
    }

    private static IContainer CreateMixedContainerGrid(int count)
    {
        var rows = new List<IContainer>();

        for (int i = 0; i < count / 5; i++)
        {
            var cols = new IContainer[5];
            for (int j = 0; j < 5; j++)
            {
                var idx = i * 5 + j;
                cols[j] = new Window(content: new FormattedTextControl($"Cell [{i},{j}]"));
            }

            rows.Add(new VSplit(cols));
        }

        // Add a FloatContainer on top
        var background = new HSplit(rows.ToArray());
        var floatWindow = new Window(content: new FormattedTextControl("Floating Dialog"));
        var floats = new[] { new Float(new AnyContainer(floatWindow), left: 10, top: 5, width: 30, height: 10) };

        return new FloatContainer(new AnyContainer(background), floats: floats);
    }
}
