using Stroke.Input.Pipe;
using Stroke.Output;
using Stroke.Shortcuts;
using Stroke.Shortcuts.ProgressBarFormatters;
using Xunit;

namespace Stroke.Tests.Shortcuts;

/// <summary>
/// Tests for the internal <see cref="ProgressControl"/> class.
/// </summary>
public sealed class ProgressControlTests
{
    private static ProgressBar CreateTestProgressBar() =>
        new(output: new DummyOutput(), input: new SimplePipeInput(), cancelCallback: () => { });

    [Fact]
    public async Task IsFocusable_ReturnsTrue()
    {
        await using var pb = CreateTestProgressBar();
        var formatter = new Percentage();
        var control = new ProgressControl(pb, formatter, cancelCallback: null);
        Assert.True(control.IsFocusable);
    }

    [Fact]
    public async Task GetKeyBindings_ReturnsNonNull()
    {
        await using var pb = CreateTestProgressBar();
        var formatter = new Percentage();
        var control = new ProgressControl(pb, formatter, cancelCallback: () => { });
        Assert.NotNull(control.GetKeyBindings());
    }

    [Fact]
    public async Task GetKeyBindings_WithNullCallback_StillReturnsNonNull()
    {
        await using var pb = CreateTestProgressBar();
        var formatter = new Percentage();
        var control = new ProgressControl(pb, formatter, cancelCallback: null);
        Assert.NotNull(control.GetKeyBindings());
    }

    [Fact]
    public async Task CreateContent_NoCounters_ReturnsZeroLines()
    {
        await using var pb = CreateTestProgressBar();
        var formatter = new Percentage();
        var control = new ProgressControl(pb, formatter, cancelCallback: null);

        var content = control.CreateContent(width: 20, height: 10);
        Assert.Equal(0, content.LineCount);
    }

    [Fact]
    public async Task CreateContent_WithCounters_ReturnsMatchingLineCount()
    {
        await using var pb = CreateTestProgressBar();
        pb.Iterate(Enumerable.Range(0, 10), label: "task1");
        pb.Iterate(Enumerable.Range(0, 20), label: "task2");
        pb.Iterate(Enumerable.Range(0, 30), label: "task3");

        var formatter = new Percentage();
        var control = new ProgressControl(pb, formatter, cancelCallback: null);

        var content = control.CreateContent(width: 20, height: 10);
        Assert.Equal(3, content.LineCount);
    }

    [Fact]
    public async Task CreateContent_LinesContainFormattedOutput()
    {
        await using var pb = CreateTestProgressBar();
        var counter = pb.Iterate(Enumerable.Range(0, 100));
        counter.ItemsCompleted = 50;

        var formatter = new Percentage();
        var control = new ProgressControl(pb, formatter, cancelCallback: null);

        var content = control.CreateContent(width: 20, height: 10);
        Assert.Equal(1, content.LineCount);

        // Line 0 should have formatted text fragments
        var line = content.GetLine(0);
        Assert.NotNull(line);
        Assert.NotEmpty(line);
    }

    [Fact]
    public async Task CreateContent_IndexOutOfRange_ReturnsEmptyLine()
    {
        await using var pb = CreateTestProgressBar();
        pb.Iterate(Enumerable.Range(0, 10));

        var formatter = new Percentage();
        var control = new ProgressControl(pb, formatter, cancelCallback: null);

        var content = control.CreateContent(width: 20, height: 10);
        // Requesting a line beyond LineCount should return empty
        var line = content.GetLine(99);
        Assert.Empty(line);
    }
}
