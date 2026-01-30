using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Stroke.Core.Primitives;
using Stroke.Layout;
using Stroke.Layout.Containers;
using Stroke.Layout.Controls;
using Stroke.Layout.Windows;

namespace Stroke.Benchmarks;

/// <summary>
/// Benchmarks for scrolling through large buffers (SC-003).
/// Targets: 10,000-line scroll rendering in under 16ms (60fps).
/// </summary>
[SimpleJob(RuntimeMoniker.Net10_0, iterationCount: 3, warmupCount: 1)]
[MemoryDiagnoser]
public class LargeBufferScrollBenchmarks
{
    private string _tenKLines = null!;
    private string _fiftyKLines = null!;

    [GlobalSetup]
    public void Setup()
    {
        _tenKLines = string.Join("\n", Enumerable.Range(0, 10_000).Select(i => $"Line {i}: Some sample text content for this line"));
        _fiftyKLines = string.Join("\n", Enumerable.Range(0, 50_000).Select(i => $"Line {i}: Content"));
    }

    [Benchmark(Description = "10K lines - render at top")]
    public void TenKLines_RenderAtTop()
    {
        var control = new FormattedTextControl(_tenKLines,
            getCursorPosition: () => new Point(0, 0));
        var window = new Window(content: control);

        var screen = new Screen();
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 120, 40);

        window.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);
    }

    [Benchmark(Description = "10K lines - render at middle")]
    public void TenKLines_RenderAtMiddle()
    {
        var control = new FormattedTextControl(_tenKLines,
            getCursorPosition: () => new Point(0, 5000));
        var window = new Window(content: control);

        var screen = new Screen();
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 120, 40);

        window.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);
    }

    [Benchmark(Description = "10K lines - render at bottom")]
    public void TenKLines_RenderAtBottom()
    {
        var control = new FormattedTextControl(_tenKLines,
            getCursorPosition: () => new Point(0, 9999));
        var window = new Window(content: control);

        var screen = new Screen();
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 120, 40);

        window.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);
    }

    [Benchmark(Description = "10K lines - with scroll offsets")]
    public void TenKLines_WithScrollOffsets()
    {
        var control = new FormattedTextControl(_tenKLines,
            getCursorPosition: () => new Point(0, 5000));
        var window = new Window(
            content: control,
            scrollOffsets: new ScrollOffsets(top: 3, bottom: 3));

        var screen = new Screen();
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 120, 40);

        window.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);
    }

    [Benchmark(Description = "50K lines - render at middle")]
    public void FiftyKLines_RenderAtMiddle()
    {
        var control = new FormattedTextControl(_fiftyKLines,
            getCursorPosition: () => new Point(0, 25000));
        var window = new Window(content: control);

        var screen = new Screen();
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 120, 40);

        window.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);
    }

    [Benchmark(Description = "10K lines - consecutive scroll")]
    public void TenKLines_ConsecutiveScroll()
    {
        var cursorRow = 0;
        var control = new FormattedTextControl(_tenKLines,
            getCursorPosition: () => new Point(0, cursorRow));
        var window = new Window(content: control);
        var writePosition = new WritePosition(0, 0, 120, 40);

        // Simulate scrolling through 100 positions
        for (int i = 0; i < 100; i++)
        {
            cursorRow = i * 100;
            var screen = new Screen();
            var mouseHandlers = new MouseHandlers();
            window.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);
        }
    }
}
