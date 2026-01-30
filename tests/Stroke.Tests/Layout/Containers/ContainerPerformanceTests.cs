using System.Diagnostics;
using Stroke.KeyBinding;
using Stroke.Layout;
using Stroke.Layout.Containers;
using Xunit;

namespace Stroke.Tests.Layout.Containers;

/// <summary>
/// Performance tests for container rendering (SC-002).
/// </summary>
public sealed class ContainerPerformanceTests
{
    #region Helper Classes

    /// <summary>
    /// Lightweight test container for performance testing.
    /// </summary>
    private sealed class PerfTestContainer : IContainer
    {
        public bool IsModal => false;

        public void Reset() { }

        public Dimension PreferredWidth(int maxAvailableWidth) => new Dimension();

        public Dimension PreferredHeight(int width, int maxAvailableHeight) => new Dimension(1);

        public void WriteToScreen(
            Screen screen,
            MouseHandlers? mouseHandlers,
            WritePosition writePosition,
            string parentStyle,
            bool eraseBg,
            int? zIndex)
        {
            // Minimal write - just mark one cell
            if (writePosition.Height > 0 && writePosition.Width > 0)
            {
                screen[writePosition.YPos, writePosition.XPos] = Stroke.Layout.Char.Create("X", parentStyle);
            }
        }

        public IKeyBindingsBase? GetKeyBindings() => null;

        public IReadOnlyList<IContainer> GetChildren() => [];
    }

    #endregion

    /// <summary>
    /// Verifies that 50 containers can render in less than 16ms (SC-002).
    /// This ensures smooth 60fps rendering is achievable.
    /// </summary>
    [Fact]
    public void WriteToScreen_50Containers_CompletesIn16ms()
    {
        // Arrange: Create a complex layout with 50 containers
        // Using a mix of HSplit and VSplit with nested containers
        var containers = new IContainer[50];

        for (int i = 0; i < 50; i++)
        {
            containers[i] = new PerfTestContainer();
        }

        // Create a master container holding all 50
        var root = new HSplit(containers);

        // Create a screen to render to
        var screen = new Screen();
        var writePosition = new WritePosition(0, 0, 80, 200); // Large enough for all containers
        var parentStyle = "";
        var emptyErase = false;
        int? zIndex = 0;

        // Warmup run to JIT compile
        var mouseHandlers = new MouseHandlers();
        root.WriteToScreen(screen, mouseHandlers, writePosition, parentStyle, emptyErase, zIndex);
        screen.Clear();

        // Act: Time the render
        var stopwatch = Stopwatch.StartNew();
        root.WriteToScreen(screen, mouseHandlers, writePosition, parentStyle, emptyErase, zIndex);
        stopwatch.Stop();

        // Assert: Must complete in under 16ms for 60fps
        Assert.True(stopwatch.ElapsedMilliseconds < 16,
            $"Rendering 50 containers took {stopwatch.ElapsedMilliseconds}ms, expected <16ms for 60fps");
    }

    /// <summary>
    /// Verifies that deeply nested HSplit/VSplit (alternating) performs well.
    /// </summary>
    [Fact]
    public void WriteToScreen_DeepNesting_CompletesIn16ms()
    {
        // Arrange: Create 25 levels of alternating HSplit/VSplit
        IContainer current = new PerfTestContainer();

        for (int i = 0; i < 25; i++)
        {
            if (i % 2 == 0)
            {
                current = new HSplit([current]);
            }
            else
            {
                current = new VSplit([current]);
            }
        }

        var screen = new Screen();
        var writePosition = new WritePosition(0, 0, 80, 24);
        var parentStyle = "";
        var emptyErase = false;
        int? zIndex = 0;

        // Warmup
        var mouseHandlers = new MouseHandlers();
        current.WriteToScreen(screen, mouseHandlers, writePosition, parentStyle, emptyErase, zIndex);
        screen.Clear();

        // Act
        var stopwatch = Stopwatch.StartNew();
        current.WriteToScreen(screen, mouseHandlers, writePosition, parentStyle, emptyErase, zIndex);
        stopwatch.Stop();

        // Assert
        Assert.True(stopwatch.ElapsedMilliseconds < 16,
            $"Rendering 25 levels of nesting took {stopwatch.ElapsedMilliseconds}ms, expected <16ms for 60fps");
    }

    /// <summary>
    /// Verifies that a grid-like layout of HSplits inside VSplits performs well.
    /// </summary>
    [Fact]
    public void WriteToScreen_GridLayout_CompletesIn16ms()
    {
        // Arrange: Create a 5x10 grid (50 controls total)
        var rows = new IContainer[5];

        for (int row = 0; row < 5; row++)
        {
            var cols = new IContainer[10];
            for (int col = 0; col < 10; col++)
            {
                cols[col] = new PerfTestContainer();
            }
            rows[row] = new VSplit(cols);
        }

        var root = new HSplit(rows);

        var screen = new Screen();
        var writePosition = new WritePosition(0, 0, 80, 24);
        var parentStyle = "";
        var emptyErase = false;
        int? zIndex = 0;

        // Warmup
        var mouseHandlers = new MouseHandlers();
        root.WriteToScreen(screen, mouseHandlers, writePosition, parentStyle, emptyErase, zIndex);
        screen.Clear();

        // Act
        var stopwatch = Stopwatch.StartNew();
        root.WriteToScreen(screen, mouseHandlers, writePosition, parentStyle, emptyErase, zIndex);
        stopwatch.Stop();

        // Assert
        Assert.True(stopwatch.ElapsedMilliseconds < 16,
            $"Rendering 5x10 grid took {stopwatch.ElapsedMilliseconds}ms, expected <16ms for 60fps");
    }
}
