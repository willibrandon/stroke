using Stroke.FormattedText;
using Stroke.Input.Pipe;
using Stroke.Layout;
using Stroke.Output;
using Stroke.Shortcuts;
using Stroke.Shortcuts.ProgressBarFormatters;
using Xunit;

namespace Stroke.Tests.Shortcuts;

/// <summary>
/// Tests for all <see cref="Formatter"/> subclasses and <see cref="FormatterUtils"/>.
/// </summary>
public sealed class ProgressBarFormatterTests
{
    private static ProgressBar CreateTestProgressBar() =>
        new(output: new DummyOutput(), input: new SimplePipeInput(), cancelCallback: () => { });

    private static string ToPlainText(AnyFormattedText text) =>
        FormattedTextUtils.FragmentListToText(FormattedTextUtils.ToFormattedText(text));

    #region FormatterUtils.FormatTimeDelta

    [Theory]
    [InlineData(0, 0, 5, "00:05")]
    [InlineData(0, 1, 30, "01:30")]
    [InlineData(1, 5, 30, "1:05:30")]
    [InlineData(0, 0, 0, "00:00")]
    [InlineData(0, 10, 0, "10:00")]
    [InlineData(2, 0, 0, "2:00:00")]
    public void FormatTimeDelta_FormatsCorrectly(int hours, int minutes, int seconds, string expected)
    {
        var ts = new TimeSpan(hours, minutes, seconds);
        Assert.Equal(expected, FormatterUtils.FormatTimeDelta(ts));
    }

    #endregion

    #region FormatterUtils.CreateDefaultFormatters

    [Fact]
    public void CreateDefaultFormatters_ReturnsNonEmpty()
    {
        var formatters = FormatterUtils.CreateDefaultFormatters();
        Assert.NotEmpty(formatters);
    }

    [Fact]
    public void CreateDefaultFormatters_ContainsExpectedTypes()
    {
        var formatters = FormatterUtils.CreateDefaultFormatters();
        Assert.Contains(formatters, f => f is Label);
        Assert.Contains(formatters, f => f is Percentage);
        Assert.Contains(formatters, f => f is Bar);
        Assert.Contains(formatters, f => f is Progress);
        Assert.Contains(formatters, f => f is TimeLeft);
    }

    #endregion

    #region Text Formatter

    [Fact]
    public async Task Text_Format_ReturnsStaticText()
    {
        await using var pb = CreateTestProgressBar();
        var counter = pb.Iterate(Enumerable.Range(0, 10));
        var view = pb.Counters[0];

        var formatter = new Text("hello");
        var result = ToPlainText(formatter.Format(pb, view, width: 20));
        Assert.Equal("hello", result);
    }

    [Fact]
    public async Task Text_GetWidth_ReturnsExactWidth()
    {
        await using var pb = CreateTestProgressBar();
        var formatter = new Text("abc");
        var dim = formatter.GetWidth(pb);
        Assert.Equal(3, dim.Preferred);
    }

    [Fact]
    public async Task Text_Format_WithStyle_StillReturnsText()
    {
        await using var pb = CreateTestProgressBar();
        var counter = pb.Iterate(Enumerable.Range(0, 10));
        var view = pb.Counters[0];

        var formatter = new Text(" | ", style: "class:separator");
        var result = ToPlainText(formatter.Format(pb, view, width: 20));
        Assert.Equal(" | ", result);
    }

    #endregion

    #region Label Formatter

    [Fact]
    public async Task Label_Format_IncludesSuffix()
    {
        await using var pb = CreateTestProgressBar();
        var counter = pb.Iterate(Enumerable.Range(0, 10), label: "Download");
        var view = pb.Counters[0];

        var formatter = new Label(suffix: ": ");
        var result = ToPlainText(formatter.Format(pb, view, width: 50));
        Assert.Contains("Download", result);
        Assert.Contains(": ", result);
    }

    [Fact]
    public async Task Label_Format_WithoutSuffix()
    {
        await using var pb = CreateTestProgressBar();
        var counter = pb.Iterate(Enumerable.Range(0, 10), label: "Task");
        var view = pb.Counters[0];

        var formatter = new Label();
        var result = ToPlainText(formatter.Format(pb, view, width: 50));
        Assert.Equal("Task", result);
    }

    [Fact]
    public async Task Label_GetWidth_AutoSizesFromCounters()
    {
        await using var pb = CreateTestProgressBar();
        pb.Iterate(Enumerable.Range(0, 10), label: "short");
        pb.Iterate(Enumerable.Range(0, 10), label: "much longer label");

        var formatter = new Label();
        var dim = formatter.GetWidth(pb);
        Assert.True(dim.Preferred > 0);
    }

    [Fact]
    public async Task Label_GetWidth_NoCounters_ReturnsDefault()
    {
        await using var pb = CreateTestProgressBar();
        var formatter = new Label();
        var dim = formatter.GetWidth(pb);
        // Default Dimension when no counters
        Assert.NotNull(dim);
    }

    [Fact]
    public async Task Label_GetWidth_ExplicitDimension_Overrides()
    {
        await using var pb = CreateTestProgressBar();
        pb.Iterate(Enumerable.Range(0, 10), label: "test");

        var explicit_dim = Dimension.Exact(20);
        var formatter = new Label(width: explicit_dim);
        var dim = formatter.GetWidth(pb);
        Assert.Equal(20, dim.Preferred);
    }

    #endregion

    #region Percentage Formatter

    [Fact]
    public async Task Percentage_Format_AtZero()
    {
        await using var pb = CreateTestProgressBar();
        var counter = pb.Iterate(Enumerable.Range(0, 100));
        var view = pb.Counters[0];

        var formatter = new Percentage();
        var result = ToPlainText(formatter.Format(pb, view, width: 10));
        Assert.Contains("0.0%", result);
    }

    [Fact]
    public async Task Percentage_Format_At50()
    {
        await using var pb = CreateTestProgressBar();
        var counter = pb.Iterate(Enumerable.Range(0, 100));
        counter.ItemsCompleted = 50;
        var view = pb.Counters[0];

        var formatter = new Percentage();
        var result = ToPlainText(formatter.Format(pb, view, width: 10));
        Assert.Contains("50.0%", result);
    }

    [Fact]
    public async Task Percentage_Format_At100()
    {
        await using var pb = CreateTestProgressBar();
        var counter = pb.Iterate(Enumerable.Range(0, 100));
        counter.ItemsCompleted = 100;
        var view = pb.Counters[0];

        var formatter = new Percentage();
        var result = ToPlainText(formatter.Format(pb, view, width: 10));
        Assert.Contains("100.0%", result);
    }

    [Fact]
    public async Task Percentage_GetWidth_IsSix()
    {
        await using var pb = CreateTestProgressBar();
        var formatter = new Percentage();
        Assert.Equal(6, formatter.GetWidth(pb).Preferred);
    }

    #endregion

    #region Bar Formatter

    [Fact]
    public async Task Bar_Format_Done_ShowsFullBar()
    {
        await using var pb = CreateTestProgressBar();
        var counter = pb.Iterate(Enumerable.Range(0, 100));
        counter.ItemsCompleted = 100;
        counter.Done = true;
        var view = pb.Counters[0];

        var formatter = new Bar();
        var result = ToPlainText(formatter.Format(pb, view, width: 22));
        Assert.Contains("[", result);
        Assert.Contains("]", result);
        Assert.Contains("=", result); // Bar filled with symA
    }

    [Fact]
    public async Task Bar_Format_AtZero_ShowsEmptyBar()
    {
        await using var pb = CreateTestProgressBar();
        var counter = pb.Iterate(Enumerable.Range(0, 100));
        var view = pb.Counters[0];

        var formatter = new Bar();
        var result = ToPlainText(formatter.Format(pb, view, width: 22));
        Assert.Contains("[", result);
        Assert.Contains("]", result);
        Assert.Contains(">", result); // symB (cursor position)
    }

    [Fact]
    public async Task Bar_Format_CustomSymbols()
    {
        await using var pb = CreateTestProgressBar();
        var counter = pb.Iterate(Enumerable.Range(0, 100));
        counter.ItemsCompleted = 50;
        var view = pb.Counters[0];

        var formatter = new Bar(start: "|", end: "|", symA: "#", symB: "*", symC: "-");
        var result = ToPlainText(formatter.Format(pb, view, width: 22));
        Assert.Contains("|", result);
        Assert.Contains("#", result);
    }

    [Fact]
    public async Task Bar_GetWidth_MinIsNine()
    {
        await using var pb = CreateTestProgressBar();
        var formatter = new Bar();
        var dim = formatter.GetWidth(pb);
        Assert.Equal(9, dim.Min);
    }

    [Fact]
    public async Task Bar_Format_UnknownTotal_ShowsAnimation()
    {
        await using var pb = CreateTestProgressBar();
        // IEnumerable without count → Total is null
        var counter = pb.Iterate(YieldItems(10));
        var view = pb.Counters[0];

        var formatter = new Bar();
        var result = ToPlainText(formatter.Format(pb, view, width: 22));
        // Should contain the unknown symbol (#)
        Assert.Contains("#", result);
    }

    private static IEnumerable<int> YieldItems(int count)
    {
        for (var i = 0; i < count; i++)
            yield return i;
    }

    #endregion

    #region Progress Formatter

    [Fact]
    public async Task Progress_Format_KnownTotal()
    {
        await using var pb = CreateTestProgressBar();
        var counter = pb.Iterate(Enumerable.Range(0, 50));
        counter.ItemsCompleted = 25;
        var view = pb.Counters[0];

        var formatter = new Progress();
        var result = ToPlainText(formatter.Format(pb, view, width: 20));
        Assert.Contains("25", result);
        Assert.Contains("50", result);
        Assert.Contains("/", result);
    }

    [Fact]
    public async Task Progress_Format_UnknownTotal()
    {
        await using var pb = CreateTestProgressBar();
        var counter = pb.Iterate(YieldItems(10));
        counter.ItemsCompleted = 3;
        var view = pb.Counters[0];

        var formatter = new Progress();
        var result = ToPlainText(formatter.Format(pb, view, width: 20));
        Assert.Contains("3", result);
        Assert.Contains("?", result);
    }

    [Fact]
    public async Task Progress_GetWidth_ScalesWithCounters()
    {
        await using var pb = CreateTestProgressBar();
        pb.Iterate(Enumerable.Range(0, 1000));

        var formatter = new Progress();
        var dim = formatter.GetWidth(pb);
        Assert.True(dim.Preferred > 0);
    }

    [Fact]
    public async Task Progress_GetWidth_NoCounters_ReturnsMinimal()
    {
        await using var pb = CreateTestProgressBar();
        var formatter = new Progress();
        var dim = formatter.GetWidth(pb);
        Assert.True(dim.Preferred >= 1);
    }

    #endregion

    #region TimeElapsed Formatter

    [Fact]
    public async Task TimeElapsed_Format_ProducesOutput()
    {
        await using var pb = CreateTestProgressBar();
        var counter = pb.Iterate(Enumerable.Range(0, 10));
        var view = pb.Counters[0];

        var formatter = new TimeElapsed();
        var result = ToPlainText(formatter.Format(pb, view, width: 10));
        Assert.NotEmpty(result);
        Assert.Contains(":", result); // Time format includes colons
    }

    [Fact]
    public async Task TimeElapsed_GetWidth_WithCounters_ReturnsPositive()
    {
        await using var pb = CreateTestProgressBar();
        pb.Iterate(Enumerable.Range(0, 10));

        var formatter = new TimeElapsed();
        var dim = formatter.GetWidth(pb);
        Assert.True(dim.Preferred > 0);
    }

    [Fact]
    public async Task TimeElapsed_GetWidth_NoCounters_ReturnsZero()
    {
        await using var pb = CreateTestProgressBar();
        var formatter = new TimeElapsed();
        Assert.Equal(0, formatter.GetWidth(pb).Preferred);
    }

    #endregion

    #region TimeLeft Formatter

    [Fact]
    public async Task TimeLeft_Format_UnknownTotal_ShowsQuestionMarks()
    {
        await using var pb = CreateTestProgressBar();
        var counter = pb.Iterate(YieldItems(10));
        var view = pb.Counters[0];

        var formatter = new TimeLeft();
        var result = ToPlainText(formatter.Format(pb, view, width: 10));
        Assert.Contains("?", result);
    }

    [Fact]
    public async Task TimeLeft_Format_KnownTotal_ShowsTime()
    {
        await using var pb = CreateTestProgressBar();
        var counter = pb.Iterate(Enumerable.Range(0, 100));
        Thread.Sleep(10);
        counter.ItemsCompleted = 50;
        var view = pb.Counters[0];

        var formatter = new TimeLeft();
        var result = ToPlainText(formatter.Format(pb, view, width: 10));
        Assert.Contains(":", result); // Time format
    }

    [Fact]
    public async Task TimeLeft_GetWidth_WithCounters_ReturnsPositive()
    {
        await using var pb = CreateTestProgressBar();
        pb.Iterate(Enumerable.Range(0, 10));

        var formatter = new TimeLeft();
        var dim = formatter.GetWidth(pb);
        Assert.True(dim.Preferred > 0);
    }

    [Fact]
    public async Task TimeLeft_GetWidth_NoCounters_ReturnsZero()
    {
        await using var pb = CreateTestProgressBar();
        var formatter = new TimeLeft();
        Assert.Equal(0, formatter.GetWidth(pb).Preferred);
    }

    #endregion

    #region IterationsPerSecond Formatter

    [Fact]
    public async Task IterationsPerSecond_Format_ProducesOutput()
    {
        await using var pb = CreateTestProgressBar();
        var counter = pb.Iterate(Enumerable.Range(0, 100));
        Thread.Sleep(10);
        counter.ItemsCompleted = 10;
        var view = pb.Counters[0];

        var formatter = new IterationsPerSecond();
        var result = ToPlainText(formatter.Format(pb, view, width: 10));
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task IterationsPerSecond_Format_AtZeroElapsed_ReturnsZero()
    {
        await using var pb = CreateTestProgressBar();
        var counter = pb.Iterate(Enumerable.Range(0, 100));
        // Immediately check — elapsed ≈ 0
        var view = pb.Counters[0];

        var formatter = new IterationsPerSecond();
        var result = ToPlainText(formatter.Format(pb, view, width: 10));
        Assert.Contains("0.00", result);
    }

    [Fact]
    public async Task IterationsPerSecond_GetWidth_WithCounters_ReturnsPositive()
    {
        await using var pb = CreateTestProgressBar();
        pb.Iterate(Enumerable.Range(0, 10));
        Thread.Sleep(10);

        var formatter = new IterationsPerSecond();
        var dim = formatter.GetWidth(pb);
        Assert.True(dim.Preferred > 0);
    }

    [Fact]
    public async Task IterationsPerSecond_GetWidth_NoCounters_ReturnsZero()
    {
        await using var pb = CreateTestProgressBar();
        var formatter = new IterationsPerSecond();
        Assert.Equal(0, formatter.GetWidth(pb).Preferred);
    }

    #endregion

    #region SpinningWheel Formatter

    [Fact]
    public async Task SpinningWheel_Format_ReturnsSingleChar()
    {
        await using var pb = CreateTestProgressBar();
        var counter = pb.Iterate(Enumerable.Range(0, 10));
        var view = pb.Counters[0];

        var formatter = new SpinningWheel();
        var result = ToPlainText(formatter.Format(pb, view, width: 5));
        Assert.Equal(1, result.Length);
        Assert.Contains(result[0], @"/-\|");
    }

    [Fact]
    public async Task SpinningWheel_GetWidth_IsOne()
    {
        await using var pb = CreateTestProgressBar();
        var formatter = new SpinningWheel();
        Assert.Equal(1, formatter.GetWidth(pb).Preferred);
    }

    #endregion

    #region Rainbow Formatter

    [Fact]
    public async Task Rainbow_Format_PreservesInnerText()
    {
        await using var pb = CreateTestProgressBar();
        var counter = pb.Iterate(Enumerable.Range(0, 10));
        var view = pb.Counters[0];

        var inner = new Text("hello");
        var formatter = new Rainbow(inner);
        var result = ToPlainText(formatter.Format(pb, view, width: 20));
        Assert.Equal("hello", result);
    }

    [Fact]
    public async Task Rainbow_Format_AppliesColorStyles()
    {
        await using var pb = CreateTestProgressBar();
        var counter = pb.Iterate(Enumerable.Range(0, 10));
        var view = pb.Counters[0];

        var inner = new Text("hello");
        var formatter = new Rainbow(inner);
        var result = formatter.Format(pb, view, width: 20);
        var fragments = FormattedTextUtils.ToFormattedText(result);

        // Each character should have a different color in its style
        var styles = fragments.Select(f => f.Style).ToHashSet();
        Assert.True(styles.Count > 1, "Rainbow should apply multiple different color styles");
    }

    [Fact]
    public async Task Rainbow_GetWidth_DelegatesToInner()
    {
        await using var pb = CreateTestProgressBar();
        var inner = new Text("test");
        var formatter = new Rainbow(inner);

        var innerWidth = inner.GetWidth(pb);
        var rainbowWidth = formatter.GetWidth(pb);
        Assert.Equal(innerWidth.Preferred, rainbowWidth.Preferred);
    }

    #endregion
}
