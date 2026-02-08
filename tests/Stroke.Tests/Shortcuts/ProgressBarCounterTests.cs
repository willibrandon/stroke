using Stroke.FormattedText;
using Stroke.Input.Pipe;
using Stroke.Output;
using Stroke.Shortcuts;
using Xunit;

namespace Stroke.Tests.Shortcuts;

/// <summary>
/// Tests for <see cref="ProgressBarCounter{T}"/> and the non-generic <see cref="ProgressBarCounter"/> view.
/// </summary>
public sealed class ProgressBarCounterTests
{
    private static ProgressBar CreateTestProgressBar() =>
        new(output: new DummyOutput(), input: new SimplePipeInput(), cancelCallback: () => { });

    private static IEnumerable<int> YieldItems(int count)
    {
        for (var i = 0; i < count; i++)
            yield return i;
    }

    #region Total Inference

    [Fact]
    public async Task Total_FromList_InfersCount()
    {
        await using var pb = CreateTestProgressBar();
        var counter = pb.Iterate(new List<int> { 1, 2, 3 });
        Assert.Equal(3, counter.Total);
    }

    [Fact]
    public async Task Total_FromArray_InfersCount()
    {
        await using var pb = CreateTestProgressBar();
        var counter = pb.Iterate(new[] { 10, 20, 30, 40 });
        Assert.Equal(4, counter.Total);
    }

    [Fact]
    public async Task Total_ExplicitOverride_UsesProvided()
    {
        await using var pb = CreateTestProgressBar();
        var counter = pb.Iterate(new List<int> { 1, 2, 3 }, total: 100);
        Assert.Equal(100, counter.Total);
    }

    [Fact]
    public async Task Total_FromEnumerableOnly_IsNull()
    {
        await using var pb = CreateTestProgressBar();
        var counter = pb.Iterate(YieldItems(5));
        Assert.Null(counter.Total);
    }

    [Fact]
    public async Task Total_EmptyCollection_IsZero()
    {
        await using var pb = CreateTestProgressBar();
        var counter = pb.Iterate(new List<int>());
        Assert.Equal(0, counter.Total);
    }

    #endregion

    #region Percentage

    [Fact]
    public async Task Percentage_AtStart_IsZero()
    {
        await using var pb = CreateTestProgressBar();
        var counter = pb.Iterate(Enumerable.Range(0, 100));
        Assert.Equal(0, counter.Percentage);
    }

    [Fact]
    public async Task Percentage_AtHalf_Is50()
    {
        await using var pb = CreateTestProgressBar();
        var counter = pb.Iterate(Enumerable.Range(0, 100));
        counter.ItemsCompleted = 50;
        Assert.Equal(50.0, counter.Percentage);
    }

    [Fact]
    public async Task Percentage_At100_Is100()
    {
        await using var pb = CreateTestProgressBar();
        var counter = pb.Iterate(Enumerable.Range(0, 100));
        counter.ItemsCompleted = 100;
        Assert.Equal(100.0, counter.Percentage);
    }

    [Fact]
    public async Task Percentage_WhenTotalIsNull_IsZero()
    {
        await using var pb = CreateTestProgressBar();
        var counter = pb.Iterate(YieldItems(5));
        counter.ItemsCompleted = 3;
        Assert.Equal(0, counter.Percentage);
    }

    [Fact]
    public async Task Percentage_WhenTotalIsZero_IsZero()
    {
        await using var pb = CreateTestProgressBar();
        var counter = pb.Iterate(new List<int>(), total: 0);
        Assert.Equal(0, counter.Percentage);
    }

    #endregion

    #region State Transitions

    [Fact]
    public async Task Done_DefaultIsFalse()
    {
        await using var pb = CreateTestProgressBar();
        var counter = pb.Iterate(Enumerable.Range(0, 10));
        Assert.False(counter.Done);
    }

    [Fact]
    public async Task Stopped_DefaultIsFalse()
    {
        await using var pb = CreateTestProgressBar();
        var counter = pb.Iterate(Enumerable.Range(0, 10));
        Assert.False(counter.Stopped);
    }

    [Fact]
    public async Task Done_SetTrue_AlsoSetsStopped()
    {
        await using var pb = CreateTestProgressBar();
        var counter = pb.Iterate(Enumerable.Range(0, 10));
        counter.Done = true;
        Assert.True(counter.Done);
        Assert.True(counter.Stopped);
    }

    [Fact]
    public async Task Stopped_CanBeSetIndependently_WithoutDone()
    {
        await using var pb = CreateTestProgressBar();
        var counter = pb.Iterate(Enumerable.Range(0, 10));
        counter.Stopped = true;
        Assert.True(counter.Stopped);
        Assert.False(counter.Done);
    }

    [Fact]
    public async Task Stopped_FreezesTimeElapsed()
    {
        await using var pb = CreateTestProgressBar();
        var counter = pb.Iterate(Enumerable.Range(0, 10));
        Thread.Sleep(10);
        counter.Stopped = true;
        var elapsed1 = counter.TimeElapsed;
        Thread.Sleep(50);
        var elapsed2 = counter.TimeElapsed;
        Assert.Equal(elapsed1, elapsed2);
    }

    [Fact]
    public async Task Stopped_Reset_UnfreezesTimeElapsed()
    {
        await using var pb = CreateTestProgressBar();
        var counter = pb.Iterate(Enumerable.Range(0, 10));
        counter.Stopped = true;
        var frozenElapsed = counter.TimeElapsed;
        counter.Stopped = false;
        Thread.Sleep(50);
        var unfrozenElapsed = counter.TimeElapsed;
        Assert.True(unfrozenElapsed > frozenElapsed);
    }

    [Fact]
    public async Task Stopped_SetTwice_DoesNotChangeStopTime()
    {
        await using var pb = CreateTestProgressBar();
        var counter = pb.Iterate(Enumerable.Range(0, 10));
        counter.Stopped = true;
        var elapsed1 = counter.TimeElapsed;
        Thread.Sleep(20);
        counter.Stopped = true; // Set again — should not update _stopTime
        var elapsed2 = counter.TimeElapsed;
        Assert.Equal(elapsed1, elapsed2);
    }

    #endregion

    #region ItemCompleted

    [Fact]
    public async Task ItemCompleted_IncrementsCount()
    {
        await using var pb = CreateTestProgressBar();
        var counter = pb.Iterate(Enumerable.Range(0, 10));
        Assert.Equal(0, counter.ItemsCompleted);
        counter.ItemCompleted();
        Assert.Equal(1, counter.ItemsCompleted);
        counter.ItemCompleted();
        Assert.Equal(2, counter.ItemsCompleted);
    }

    [Fact]
    public async Task ItemsCompleted_PublicSetter_Works()
    {
        await using var pb = CreateTestProgressBar();
        var counter = pb.Iterate(Enumerable.Range(0, 100));
        counter.ItemsCompleted = 42;
        Assert.Equal(42, counter.ItemsCompleted);
    }

    [Fact]
    public async Task ItemsCompleted_ConcurrentIncrements_AreAtomic()
    {
        await using var pb = CreateTestProgressBar();
        var counter = pb.Iterate(Enumerable.Range(0, 10000));
        var tasks = Enumerable.Range(0, 100).Select(_ =>
            Task.Run(() =>
            {
                for (var i = 0; i < 100; i++)
                    counter.ItemCompleted();
            }));
        await Task.WhenAll(tasks);
        Assert.Equal(10000, counter.ItemsCompleted);
    }

    #endregion

    #region Iteration

    [Fact]
    public async Task GetEnumerator_YieldsAllItems()
    {
        await using var pb = CreateTestProgressBar();
        var data = new List<int> { 10, 20, 30 };
        var counter = pb.Iterate(data);
        Assert.Equal(data, counter.ToList());
    }

    [Fact]
    public async Task GetEnumerator_TracksProgress()
    {
        await using var pb = CreateTestProgressBar();
        var counter = pb.Iterate(new List<int> { 1, 2, 3 });
        var seenCounts = new List<int>();
        foreach (var _ in counter)
        {
            seenCounts.Add(counter.ItemsCompleted);
        }

        // ItemCompleted is called AFTER yield return, so on the NEXT MoveNext
        // First yield: ItemsCompleted=0, next MoveNext calls ItemCompleted → 1
        // At the time we see each item, the PREVIOUS item's ItemCompleted has run
        Assert.Equal(3, counter.ItemsCompleted);
    }

    [Fact]
    public async Task GetEnumerator_SetsDoneOnCompletion()
    {
        await using var pb = CreateTestProgressBar();
        var counter = pb.Iterate(new List<int> { 1, 2, 3 });
        foreach (var _ in counter) { }
        Assert.True(counter.Done);
        Assert.True(counter.Stopped);
    }

    [Fact]
    public async Task GetEnumerator_SetsStopped_OnEarlyBreak()
    {
        await using var pb = CreateTestProgressBar();
        var counter = pb.Iterate(new List<int> { 1, 2, 3, 4, 5 });
        foreach (var item in counter)
        {
            if (item == 3) break;
        }

        Assert.True(counter.Stopped);
        Assert.False(counter.Done); // Not done — loop was interrupted
    }

    [Fact]
    public async Task GetEnumerator_ThrowsWhenNoData()
    {
        await using var pb = CreateTestProgressBar();
        var counter = new ProgressBarCounter<int>(
            pb, data: null, label: default, removeWhenDone: false, total: null);
        Assert.Throws<InvalidOperationException>(() =>
        {
            foreach (var _ in counter) { }
        });
    }

    #endregion

    #region Label

    [Fact]
    public async Task Label_GetSet_RoundTrips()
    {
        await using var pb = CreateTestProgressBar();
        var counter = pb.Iterate(Enumerable.Range(0, 10), label: "initial");

        var text1 = FormattedTextUtils.FragmentListToText(
            FormattedTextUtils.ToFormattedText(counter.Label));
        Assert.Equal("initial", text1);

        counter.Label = "updated";
        var text2 = FormattedTextUtils.FragmentListToText(
            FormattedTextUtils.ToFormattedText(counter.Label));
        Assert.Equal("updated", text2);
    }

    #endregion

    #region RemoveWhenDone

    [Fact]
    public async Task Done_WithRemoveWhenDone_RemovesFromCounters()
    {
        await using var pb = CreateTestProgressBar();
        pb.Iterate(new List<int> { 1, 2 }, removeWhenDone: true);
        Assert.Single(pb.Counters);

        // Get the typed counter by iterating — Done setter triggers removal
        var counter = pb.Iterate(new List<int> { 3, 4 }, removeWhenDone: true);
        Assert.Equal(2, pb.Counters.Count);

        counter.Done = true;
        Assert.Single(pb.Counters); // Second counter removed, first remains
    }

    [Fact]
    public async Task Done_WithoutRemoveWhenDone_KeepsInCounters()
    {
        await using var pb = CreateTestProgressBar();
        var counter = pb.Iterate(new List<int> { 1, 2 }, removeWhenDone: false);
        Assert.Single(pb.Counters);
        counter.Done = true;
        Assert.Single(pb.Counters);
    }

    [Fact]
    public async Task RemoveWhenDone_PropertyIsReadOnly()
    {
        await using var pb = CreateTestProgressBar();
        var counter = pb.Iterate(new List<int> { 1 }, removeWhenDone: true);
        Assert.True(counter.RemoveWhenDone);
    }

    #endregion

    #region TimeLeft

    [Fact]
    public async Task TimeLeft_WhenTotalIsNull_ReturnsNull()
    {
        await using var pb = CreateTestProgressBar();
        var counter = pb.Iterate(YieldItems(5));
        Assert.Null(counter.TimeLeft);
    }

    [Fact]
    public async Task TimeLeft_WhenDone_ReturnsZero()
    {
        await using var pb = CreateTestProgressBar();
        var counter = pb.Iterate(Enumerable.Range(0, 10));
        counter.ItemsCompleted = 10;
        counter.Done = true;
        Assert.Equal(TimeSpan.Zero, counter.TimeLeft);
    }

    [Fact]
    public async Task TimeLeft_WhenStopped_ReturnsZero()
    {
        await using var pb = CreateTestProgressBar();
        var counter = pb.Iterate(Enumerable.Range(0, 10));
        counter.ItemsCompleted = 5;
        counter.Stopped = true;
        Assert.Equal(TimeSpan.Zero, counter.TimeLeft);
    }

    [Fact]
    public async Task TimeLeft_WhenInProgress_ReturnsPositiveValue()
    {
        await using var pb = CreateTestProgressBar();
        var counter = pb.Iterate(Enumerable.Range(0, 100));
        Thread.Sleep(10);
        counter.ItemsCompleted = 50;
        var timeLeft = counter.TimeLeft;
        Assert.NotNull(timeLeft);
        Assert.True(timeLeft.Value > TimeSpan.Zero);
    }

    [Fact]
    public async Task TimeLeft_AtZeroPercentage_ReturnsNull()
    {
        await using var pb = CreateTestProgressBar();
        var counter = pb.Iterate(Enumerable.Range(0, 100));
        // 0% progress → can't estimate time left
        Assert.Null(counter.TimeLeft);
    }

    #endregion

    #region TimeElapsed

    [Fact]
    public async Task TimeElapsed_IsNonNegative()
    {
        await using var pb = CreateTestProgressBar();
        var counter = pb.Iterate(Enumerable.Range(0, 10));
        Assert.True(counter.TimeElapsed >= TimeSpan.Zero);
    }

    [Fact]
    public async Task TimeElapsed_IncreasesOverTime()
    {
        await using var pb = CreateTestProgressBar();
        var counter = pb.Iterate(Enumerable.Range(0, 10));
        var t1 = counter.TimeElapsed;
        Thread.Sleep(30);
        var t2 = counter.TimeElapsed;
        Assert.True(t2 > t1);
    }

    #endregion

    #region StartTime

    [Fact]
    public async Task StartTime_IsSetOnConstruction()
    {
        var before = DateTime.Now;
        await using var pb = CreateTestProgressBar();
        var counter = pb.Iterate(Enumerable.Range(0, 10));
        var after = DateTime.Now;

        Assert.True(counter.StartTime >= before);
        Assert.True(counter.StartTime <= after);
    }

    #endregion

    #region Multiple Counters

    [Fact]
    public async Task MultipleCounters_AllTrackedInProgressBar()
    {
        await using var pb = CreateTestProgressBar();
        pb.Iterate(Enumerable.Range(0, 10), label: "first");
        pb.Iterate(Enumerable.Range(0, 20), label: "second");
        pb.Iterate(Enumerable.Range(0, 30), label: "third");

        Assert.Equal(3, pb.Counters.Count);
    }

    [Fact]
    public async Task Counters_ReturnsSnapshot()
    {
        await using var pb = CreateTestProgressBar();
        pb.Iterate(Enumerable.Range(0, 10));
        var snapshot = pb.Counters;
        pb.Iterate(Enumerable.Range(0, 20));

        // Snapshot is a copy — adding a counter doesn't change it
        Assert.Single(snapshot);
        Assert.Equal(2, pb.Counters.Count);
    }

    #endregion

    #region Non-Generic View

    [Fact]
    public async Task NonGenericView_DelegatesAllProperties()
    {
        await using var pb = CreateTestProgressBar();
        var typed = pb.Iterate(Enumerable.Range(0, 100), label: "test-label");
        typed.ItemsCompleted = 25;

        var view = pb.Counters[0];

        Assert.Equal(typed.ItemsCompleted, view.ItemsCompleted);
        Assert.Equal(typed.Total, view.Total);
        Assert.Equal(typed.Percentage, view.Percentage);
        Assert.Equal(typed.Done, view.Done);
        Assert.Equal(typed.Stopped, view.Stopped);

        var typedLabel = FormattedTextUtils.FragmentListToText(
            FormattedTextUtils.ToFormattedText(typed.Label));
        var viewLabel = FormattedTextUtils.FragmentListToText(
            FormattedTextUtils.ToFormattedText(view.Label));
        Assert.Equal(typedLabel, viewLabel);
    }

    [Fact]
    public async Task NonGenericView_LabelSet_PropagatesBack()
    {
        await using var pb = CreateTestProgressBar();
        var typed = pb.Iterate(Enumerable.Range(0, 10), label: "original");

        var view = pb.Counters[0];
        view.Label = "changed";

        var typedLabel = FormattedTextUtils.FragmentListToText(
            FormattedTextUtils.ToFormattedText(typed.Label));
        Assert.Equal("changed", typedLabel);
    }

    [Fact]
    public async Task NonGenericView_ReflectsStateChanges()
    {
        await using var pb = CreateTestProgressBar();
        var typed = pb.Iterate(Enumerable.Range(0, 100));
        var view = pb.Counters[0];

        Assert.False(view.Done);
        Assert.False(view.Stopped);

        typed.Done = true;
        Assert.True(view.Done);
        Assert.True(view.Stopped);
    }

    #endregion

    #region ProgressBar Properties

    [Fact]
    public async Task Title_IsSetFromConstructor()
    {
        await using var pb = new ProgressBar(
            title: "Test Title",
            output: new DummyOutput(),
            input: new SimplePipeInput(),
            cancelCallback: () => { });

        var text = FormattedTextUtils.FragmentListToText(
            FormattedTextUtils.ToFormattedText(pb.Title));
        Assert.Equal("Test Title", text);
    }

    [Fact]
    public async Task BottomToolbar_IsSetFromConstructor()
    {
        await using var pb = new ProgressBar(
            bottomToolbar: "Press Ctrl+C to cancel",
            output: new DummyOutput(),
            input: new SimplePipeInput(),
            cancelCallback: () => { });

        var text = FormattedTextUtils.FragmentListToText(
            FormattedTextUtils.ToFormattedText(pb.BottomToolbar));
        Assert.Equal("Press Ctrl+C to cancel", text);
    }

    [Fact]
    public async Task Counters_InitiallyEmpty()
    {
        await using var pb = CreateTestProgressBar();
        Assert.Empty(pb.Counters);
    }

    [Fact]
    public async Task Invalidate_DoesNotThrow_WhenAppNotRunning()
    {
        await using var pb = CreateTestProgressBar();
        // Should not throw even if called before app is fully running
        pb.Invalidate();
    }

    #endregion
}
