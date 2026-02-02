using Stroke.Layout.Containers;
using Stroke.Widgets.Base;
using Xunit;

namespace Stroke.Tests.Widgets.Base;

public class ProgressBarTests
{
    [Fact]
    public void DefaultPercentage_IsSixty()
    {
        var bar = new ProgressBar();
        Assert.Equal(60, bar.Percentage);
    }

    [Fact]
    public void DefaultLabel_ShowsSixtyPercent()
    {
        var bar = new ProgressBar();
        var text = Stroke.FormattedText.FormattedTextUtils.FragmentListToText(
            Stroke.FormattedText.FormattedTextUtils.ToFormattedText(bar.Label.Text));
        Assert.Equal("60%", text);
    }

    [Fact]
    public void SetPercentage_UpdatesLabelText()
    {
        var bar = new ProgressBar();
        bar.Percentage = 75;

        Assert.Equal(75, bar.Percentage);
        var text = Stroke.FormattedText.FormattedTextUtils.FragmentListToText(
            Stroke.FormattedText.FormattedTextUtils.ToFormattedText(bar.Label.Text));
        Assert.Equal("75%", text);
    }

    [Fact]
    public void PtContainer_ReturnsFloatContainer()
    {
        var bar = new ProgressBar();
        var container = bar.PtContainer();
        Assert.IsType<FloatContainer>(container);
        Assert.Same(bar.Container, container);
    }

    [Fact]
    public void Percentage_Zero_IsValid()
    {
        var bar = new ProgressBar();
        bar.Percentage = 0;

        Assert.Equal(0, bar.Percentage);
        var text = Stroke.FormattedText.FormattedTextUtils.FragmentListToText(
            Stroke.FormattedText.FormattedTextUtils.ToFormattedText(bar.Label.Text));
        Assert.Equal("0%", text);
    }

    [Fact]
    public void Percentage_Hundred_IsValid()
    {
        var bar = new ProgressBar
        {
            Percentage = 100
        };

        Assert.Equal(100, bar.Percentage);
        var text = Stroke.FormattedText.FormattedTextUtils.FragmentListToText(
            Stroke.FormattedText.FormattedTextUtils.ToFormattedText(bar.Label.Text));
        Assert.Equal("100%", text);
    }

    [Fact]
    public void Percentage_Negative_IsNotClamped()
    {
        // Python does not clamp, so neither should we
        var bar = new ProgressBar
        {
            Percentage = -5
        };

        Assert.Equal(-5, bar.Percentage);
        var text = Stroke.FormattedText.FormattedTextUtils.FragmentListToText(
            Stroke.FormattedText.FormattedTextUtils.ToFormattedText(bar.Label.Text));
        Assert.Equal("-5%", text);
    }

    [Fact]
    public void Percentage_OverHundred_IsNotClamped()
    {
        var bar = new ProgressBar
        {
            Percentage = 150
        };

        Assert.Equal(150, bar.Percentage);
        var text = Stroke.FormattedText.FormattedTextUtils.FragmentListToText(
            Stroke.FormattedText.FormattedTextUtils.ToFormattedText(bar.Label.Text));
        Assert.Equal("150%", text);
    }

    [Fact]
    public void Container_HasFloats()
    {
        var bar = new ProgressBar();
        // FloatContainer should have 2 floats: label and VSplit
        Assert.Equal(2, bar.Container.Floats.Count);
    }

    [Fact]
    public void Label_IsAccessible()
    {
        var bar = new ProgressBar();
        Assert.NotNull(bar.Label);
        Assert.IsType<Label>(bar.Label);
    }

    [Fact]
    public async Task ThreadSafety_ConcurrentPercentageUpdates()
    {
        var bar = new ProgressBar();
        var tasks = new Task[100];

        var ct = TestContext.Current.CancellationToken;

        for (int i = 0; i < 100; i++)
        {
            int value = i;
            tasks[i] = Task.Run(() =>
            {
                bar.Percentage = value;
                // Read back to exercise both paths under concurrency
                _ = bar.Percentage;
            }, ct);
        }

        await Task.WhenAll(tasks);

        // After all tasks complete, percentage should be some value 0-99
        var final = bar.Percentage;
        Assert.InRange(final, 0, 99);

        // Label text should be consistent with the percentage
        var text = Stroke.FormattedText.FormattedTextUtils.FragmentListToText(
            Stroke.FormattedText.FormattedTextUtils.ToFormattedText(bar.Label.Text));
        Assert.Equal($"{final}%", text);
    }
}
