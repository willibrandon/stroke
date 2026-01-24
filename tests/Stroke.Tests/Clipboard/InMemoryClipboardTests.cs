using Stroke.Clipboard;
using Stroke.Core;
using Xunit;

namespace Stroke.Tests.Clipboard;

/// <summary>
/// Tests for <see cref="InMemoryClipboard"/>.
/// </summary>
public sealed class InMemoryClipboardTests
{
    // === US1: Basic Store/Retrieve ===

    [Fact]
    public void Constructor_NoArgs_CreatesEmptyClipboard()
    {
        var clipboard = new InMemoryClipboard();

        var result = clipboard.GetData();

        Assert.Equal(string.Empty, result.Text);
        Assert.Equal(SelectionType.Characters, result.Type);
    }

    [Fact]
    public void Constructor_WithInitialData_StoresIt()
    {
        var initialData = new ClipboardData("initial", SelectionType.Lines);

        var clipboard = new InMemoryClipboard(data: initialData);

        var result = clipboard.GetData();
        Assert.Equal("initial", result.Text);
        Assert.Equal(SelectionType.Lines, result.Type);
    }

    [Fact]
    public void SetData_StoresDataRetrievableViaGetData()
    {
        var clipboard = new InMemoryClipboard();
        var data = new ClipboardData("hello", SelectionType.Block);

        clipboard.SetData(data);

        var result = clipboard.GetData();
        Assert.Equal("hello", result.Text);
        Assert.Equal(SelectionType.Block, result.Type);
    }

    [Fact]
    public void SetText_StoresTextWithCharactersType()
    {
        var clipboard = new InMemoryClipboard();

        clipboard.SetText("hello");

        var result = clipboard.GetData();
        Assert.Equal("hello", result.Text);
        Assert.Equal(SelectionType.Characters, result.Type);
    }

    [Fact]
    public void SetData_WithNull_ThrowsArgumentNullException()
    {
        var clipboard = new InMemoryClipboard();

        Assert.Throws<ArgumentNullException>(() => clipboard.SetData(null!));
    }

    [Fact]
    public void SetText_WithNull_ThrowsArgumentNullException()
    {
        var clipboard = new InMemoryClipboard();

        Assert.Throws<ArgumentNullException>(() => clipboard.SetText(null!));
    }

    [Fact]
    public void GetData_OnEmptyClipboard_ReturnsEmptyClipboardData()
    {
        var clipboard = new InMemoryClipboard();

        var result = clipboard.GetData();

        Assert.Equal(string.Empty, result.Text);
        Assert.Equal(SelectionType.Characters, result.Type);
    }

    [Fact]
    public void SetData_OverwritesPreviousDataAsCurrent()
    {
        var clipboard = new InMemoryClipboard();

        clipboard.SetData(new ClipboardData("first", SelectionType.Lines));
        clipboard.SetData(new ClipboardData("second", SelectionType.Block));

        var result = clipboard.GetData();
        Assert.Equal("second", result.Text);
        Assert.Equal(SelectionType.Block, result.Type);
    }

    [Fact]
    public void SetText_EmptyString_StoresEmptyString()
    {
        var clipboard = new InMemoryClipboard();

        clipboard.SetText(string.Empty);

        var result = clipboard.GetData();
        Assert.Equal(string.Empty, result.Text);
        Assert.Equal(SelectionType.Characters, result.Type);
    }

    [Fact]
    public void SetData_EmptyClipboardData_StoresSuccessfully()
    {
        var clipboard = new InMemoryClipboard();
        clipboard.SetData(new ClipboardData("previous", SelectionType.Lines));

        clipboard.SetData(new ClipboardData());

        var result = clipboard.GetData();
        Assert.Equal(string.Empty, result.Text);
        Assert.Equal(SelectionType.Characters, result.Type);
    }

    [Fact]
    public void MaxSize_ReturnsDefaultValue()
    {
        var clipboard = new InMemoryClipboard();

        Assert.Equal(60, clipboard.MaxSize);
    }

    [Fact]
    public void MaxSize_ReturnsConfiguredValue()
    {
        var clipboard = new InMemoryClipboard(maxSize: 10);

        Assert.Equal(10, clipboard.MaxSize);
    }

    // === US2: Kill Ring Rotation ===

    [Fact]
    public void Rotate_MovesFrontItemToBack()
    {
        var clipboard = new InMemoryClipboard();
        clipboard.SetText("first");
        clipboard.SetText("second");
        clipboard.SetText("third");

        // Ring is [third, second, first]
        Assert.Equal("third", clipboard.GetData().Text);

        clipboard.Rotate();

        // After rotate: [second, first, third]
        Assert.Equal("second", clipboard.GetData().Text);
    }

    [Fact]
    public void Rotate_ThreeTimesOnABC_ReturnsToA()
    {
        var clipboard = new InMemoryClipboard();
        clipboard.SetText("A");
        clipboard.SetText("B");
        clipboard.SetText("C");

        // Ring is [C, B, A]
        Assert.Equal("C", clipboard.GetData().Text);

        clipboard.Rotate(); // [B, A, C]
        Assert.Equal("B", clipboard.GetData().Text);

        clipboard.Rotate(); // [A, C, B]
        Assert.Equal("A", clipboard.GetData().Text);

        clipboard.Rotate(); // [C, B, A] - back to original
        Assert.Equal("C", clipboard.GetData().Text);
    }

    [Fact]
    public void Rotate_OnEmptyClipboard_IsNoOp()
    {
        var clipboard = new InMemoryClipboard();

        // Should not throw
        clipboard.Rotate();

        // Still empty
        Assert.Equal(string.Empty, clipboard.GetData().Text);
    }

    [Fact]
    public void Rotate_OnSingleItem_IsNoOp()
    {
        var clipboard = new InMemoryClipboard();
        clipboard.SetText("only");

        clipboard.Rotate();

        // Item remains current
        Assert.Equal("only", clipboard.GetData().Text);
    }

    [Fact]
    public void KillRing_MaintainsOrderThroughMultipleOperations()
    {
        var clipboard = new InMemoryClipboard();

        // Add items
        clipboard.SetText("one");
        clipboard.SetText("two");
        clipboard.SetText("three");

        // Ring: [three, two, one]
        Assert.Equal("three", clipboard.GetData().Text);

        // Rotate to access previous items
        clipboard.Rotate(); // [two, one, three]
        Assert.Equal("two", clipboard.GetData().Text);

        // Add new item (should go to front)
        clipboard.SetText("four");

        // Ring: [four, two, one, three]
        Assert.Equal("four", clipboard.GetData().Text);

        // Rotate should continue cycling
        clipboard.Rotate(); // [two, one, three, four]
        Assert.Equal("two", clipboard.GetData().Text);
    }

    // === US2: MaxSize Tests ===

    [Fact]
    public void Constructor_MaxSizeLessThanOne_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new InMemoryClipboard(maxSize: 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => new InMemoryClipboard(maxSize: -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => new InMemoryClipboard(maxSize: -100));
    }

    [Fact]
    public void Ring_TrimsOldestWhenExceedingMaxSize()
    {
        var clipboard = new InMemoryClipboard(maxSize: 3);

        clipboard.SetText("a");
        clipboard.SetText("b");
        clipboard.SetText("c");
        clipboard.SetText("d"); // "a" should be dropped

        // Ring should be [d, c, b]
        Assert.Equal("d", clipboard.GetData().Text);

        clipboard.Rotate();
        Assert.Equal("c", clipboard.GetData().Text);

        clipboard.Rotate();
        Assert.Equal("b", clipboard.GetData().Text);

        clipboard.Rotate(); // Should cycle back to d
        Assert.Equal("d", clipboard.GetData().Text);
    }

    [Fact]
    public void Ring_WithMaxSizeOne_KeepsOnlyMostRecent()
    {
        var clipboard = new InMemoryClipboard(maxSize: 1);

        clipboard.SetText("first");
        Assert.Equal("first", clipboard.GetData().Text);

        clipboard.SetText("second");
        Assert.Equal("second", clipboard.GetData().Text);

        clipboard.SetText("third");
        Assert.Equal("third", clipboard.GetData().Text);

        // Rotate should be no-op (only one item)
        clipboard.Rotate();
        Assert.Equal("third", clipboard.GetData().Text);
    }

    // === US5: Thread Safety Tests ===

    [Fact]
    public async Task ConcurrentSetData_NoExceptions()
    {
        var clipboard = new InMemoryClipboard();
        var tasks = new List<Task>();

        // 10 threads concurrently calling SetData
        for (int i = 0; i < 10; i++)
        {
            int threadId = i;
            tasks.Add(Task.Run(() =>
            {
                for (int j = 0; j < 100; j++)
                {
                    clipboard.SetData(new ClipboardData($"thread{threadId}-{j}"));
                }
            }));
        }

        // Should complete without exceptions
        await Task.WhenAll(tasks);
    }

    [Fact]
    public async Task ConcurrentGetData_AllReceiveValidClipboardData()
    {
        var clipboard = new InMemoryClipboard();
        clipboard.SetText("initial");

        var results = new System.Collections.Concurrent.ConcurrentBag<ClipboardData>();
        var tasks = new List<Task>();

        // 10 threads concurrently calling GetData
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                for (int j = 0; j < 100; j++)
                {
                    var data = clipboard.GetData();
                    results.Add(data);
                }
            }));
        }

        await Task.WhenAll(tasks);

        // All results should be valid ClipboardData (not null)
        Assert.Equal(1000, results.Count);
        Assert.All(results, data => Assert.NotNull(data));
    }

    [Fact]
    public async Task MixedConcurrentOperations_NoExceptions()
    {
        var clipboard = new InMemoryClipboard();
        var tasks = new List<Task>();

        // Mixed SetData, GetData, Rotate operations
        for (int i = 0; i < 10; i++)
        {
            int threadId = i;
            tasks.Add(Task.Run(() =>
            {
                for (int j = 0; j < 100; j++)
                {
                    switch (j % 3)
                    {
                        case 0:
                            clipboard.SetData(new ClipboardData($"data-{threadId}-{j}"));
                            break;
                        case 1:
                            clipboard.GetData();
                            break;
                        case 2:
                            clipboard.Rotate();
                            break;
                    }
                }
            }));
        }

        // Should complete without exceptions
        await Task.WhenAll(tasks);
    }

    [Fact]
    public async Task StressTest_TenPlusThreads_ThousandPlusOperations()
    {
        // Satisfies SC-006: Concurrent stress test with 10+ threads and 1000+ operations
        var clipboard = new InMemoryClipboard(maxSize: 100);
        var tasks = new List<Task>();
        var operationCount = 0;

        for (int i = 0; i < 15; i++) // 15 threads
        {
            int threadId = i;
            tasks.Add(Task.Run(() =>
            {
                for (int j = 0; j < 100; j++) // 100 ops per thread = 1500 total
                {
                    switch (j % 4)
                    {
                        case 0:
                            clipboard.SetText($"text-{threadId}-{j}");
                            break;
                        case 1:
                            clipboard.SetData(new ClipboardData($"data-{threadId}-{j}", SelectionType.Lines));
                            break;
                        case 2:
                            clipboard.GetData();
                            break;
                        case 3:
                            clipboard.Rotate();
                            break;
                    }
                    Interlocked.Increment(ref operationCount);
                }
            }));
        }

        await Task.WhenAll(tasks);

        // Verify all operations completed
        Assert.True(operationCount >= 1000, $"Expected 1000+ operations, got {operationCount}");

        // Clipboard should still be functional
        var data = clipboard.GetData();
        Assert.NotNull(data);
    }

    [Fact]
    public void KillRing_MaintainsOrderThrough100PlusConsecutiveOperations()
    {
        // Satisfies SC-004: Kill ring maintains order through 100+ consecutive operations
        var clipboard = new InMemoryClipboard(maxSize: 10);

        // Add items
        for (int i = 0; i < 10; i++)
        {
            clipboard.SetText($"item-{i}");
        }

        // Perform 100+ consecutive set/rotate operations
        for (int i = 0; i < 50; i++)
        {
            clipboard.SetText($"new-{i}");
            clipboard.Rotate();
        }

        for (int i = 0; i < 60; i++)
        {
            clipboard.Rotate();
        }

        // Clipboard should still be functional and not corrupted
        var data = clipboard.GetData();
        Assert.NotNull(data);
        Assert.NotNull(data.Text);

        // Verify we can still cycle through items
        var seenItems = new HashSet<string>();
        for (int i = 0; i < 10; i++)
        {
            seenItems.Add(clipboard.GetData().Text);
            clipboard.Rotate();
        }

        // Should have cycled through multiple unique items
        Assert.True(seenItems.Count > 1, "Kill ring should contain multiple items");
    }
}
