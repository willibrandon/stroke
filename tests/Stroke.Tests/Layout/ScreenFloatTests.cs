using Stroke.Layout;
using Xunit;

namespace Stroke.Tests.Layout;

/// <summary>
/// Tests for Screen z-index float drawing (User Story 5).
/// </summary>
public class ScreenFloatTests
{
    [Fact]
    public void DrawWithZIndex_SingleFunction_Executes()
    {
        var screen = new Screen();
        var executed = false;

        screen.DrawWithZIndex(5, () => executed = true);
        screen.DrawAllFloats();

        Assert.True(executed);
    }

    [Fact]
    public void DrawAllFloats_MultipleZIndex_ExecutesInOrder()
    {
        var screen = new Screen();
        var order = new List<int>();

        screen.DrawWithZIndex(10, () => order.Add(10));
        screen.DrawWithZIndex(5, () => order.Add(5));
        screen.DrawWithZIndex(8, () => order.Add(8));

        screen.DrawAllFloats();

        Assert.Equal([5, 8, 10], order);
    }

    [Fact]
    public void DrawAllFloats_EqualZIndex_ExecutesInFIFOOrder()
    {
        var screen = new Screen();
        var order = new List<string>();

        screen.DrawWithZIndex(5, () => order.Add("first"));
        screen.DrawWithZIndex(5, () => order.Add("second"));
        screen.DrawWithZIndex(5, () => order.Add("third"));

        screen.DrawAllFloats();

        Assert.Equal(["first", "second", "third"], order);
    }

    [Fact]
    public void DrawAllFloats_MixedZIndex_CorrectOrder()
    {
        var screen = new Screen();
        var order = new List<int>();

        // Queue: z=5, z=2, z=8, z=5 (second at 5)
        screen.DrawWithZIndex(5, () => order.Add(51)); // first 5
        screen.DrawWithZIndex(2, () => order.Add(2));
        screen.DrawWithZIndex(8, () => order.Add(8));
        screen.DrawWithZIndex(5, () => order.Add(52)); // second 5

        screen.DrawAllFloats();

        // Expected: 2, 5 (first), 5 (second), 8
        Assert.Equal([2, 51, 52, 8], order);
    }

    [Fact]
    public void DrawAllFloats_NestedQueuing_ProcessesAll()
    {
        var screen = new Screen();
        var order = new List<int>();

        screen.DrawWithZIndex(10, () =>
        {
            order.Add(10);
            // Queue another during execution
            screen.DrawWithZIndex(5, () => order.Add(5));
        });

        screen.DrawAllFloats();

        // 10 runs first, queues 5, then 5 runs (iterative processing)
        Assert.Equal([10, 5], order);
    }

    [Fact]
    public void DrawAllFloats_DeepNesting_ProcessesAll()
    {
        var screen = new Screen();
        var order = new List<int>();

        screen.DrawWithZIndex(1, () =>
        {
            order.Add(1);
            screen.DrawWithZIndex(2, () =>
            {
                order.Add(2);
                screen.DrawWithZIndex(3, () => order.Add(3));
            });
        });

        screen.DrawAllFloats();

        Assert.Equal([1, 2, 3], order);
    }

    [Fact]
    public void DrawAllFloats_EmptyQueue_NoException()
    {
        var screen = new Screen();

        // Should complete without exception
        screen.DrawAllFloats();
    }

    [Fact]
    public void DrawAllFloats_CalledTwice_SecondCallNoOp()
    {
        var screen = new Screen();
        var count = 0;

        screen.DrawWithZIndex(5, () => count++);

        screen.DrawAllFloats();
        screen.DrawAllFloats();

        Assert.Equal(1, count);
    }

    [Fact]
    public void DrawAllFloats_Exception_ClearsQueueAndRethrows()
    {
        var screen = new Screen();
        var executed = new List<int>();

        screen.DrawWithZIndex(1, () => executed.Add(1));
        screen.DrawWithZIndex(2, () => throw new InvalidOperationException("test error"));
        screen.DrawWithZIndex(3, () => executed.Add(3));

        var ex = Assert.Throws<InvalidOperationException>(() => screen.DrawAllFloats());
        Assert.Equal("test error", ex.Message);

        // First function should have executed, third should not
        Assert.Equal([1], executed);

        // Queue should be cleared - calling again should be no-op
        screen.DrawAllFloats();
        Assert.Equal([1], executed);
    }

    [Fact]
    public void DrawWithZIndex_NullDrawFunc_ThrowsArgumentNullException()
    {
        var screen = new Screen();

        Assert.Throws<ArgumentNullException>(() =>
            screen.DrawWithZIndex(5, null!));
    }

    [Fact]
    public void DrawAllFloats_NegativeZIndex_ValidAndSortedCorrectly()
    {
        var screen = new Screen();
        var order = new List<int>();

        screen.DrawWithZIndex(-10, () => order.Add(-10));
        screen.DrawWithZIndex(5, () => order.Add(5));
        screen.DrawWithZIndex(-5, () => order.Add(-5));

        screen.DrawAllFloats();

        Assert.Equal([-10, -5, 5], order);
    }

    [Fact]
    public void DrawAllFloats_ZeroZIndex_ValidAndSortedCorrectly()
    {
        var screen = new Screen();
        var order = new List<int>();

        screen.DrawWithZIndex(10, () => order.Add(10));
        screen.DrawWithZIndex(0, () => order.Add(0));
        screen.DrawWithZIndex(-5, () => order.Add(-5));

        screen.DrawAllFloats();

        Assert.Equal([-5, 0, 10], order);
    }
}
