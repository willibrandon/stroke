using Stroke.Core;
using Xunit;

namespace Stroke.Tests.Core;

/// <summary>
/// Tests for <see cref="DummyClipboard"/>.
/// </summary>
public sealed class DummyClipboardTests
{
    // === US1: Basic Behavior ===

    [Fact]
    public void SetData_IsNoOp()
    {
        var clipboard = new DummyClipboard();
        var data = new ClipboardData("test", SelectionType.Lines);

        clipboard.SetData(data);

        // GetData should still return empty (SetData is no-op)
        var result = clipboard.GetData();
        Assert.Equal(string.Empty, result.Text);
        Assert.Equal(SelectionType.Characters, result.Type);
    }

    [Fact]
    public void SetText_IsNoOp()
    {
        var clipboard = new DummyClipboard();

        clipboard.SetText("test");

        // GetData should still return empty (SetText is no-op)
        var result = clipboard.GetData();
        Assert.Equal(string.Empty, result.Text);
        Assert.Equal(SelectionType.Characters, result.Type);
    }

    [Fact]
    public void GetData_ReturnsEmptyClipboardData()
    {
        var clipboard = new DummyClipboard();

        var result = clipboard.GetData();

        Assert.Equal(string.Empty, result.Text);
        Assert.Equal(SelectionType.Characters, result.Type);
    }

    [Fact]
    public void Rotate_IsNoOp()
    {
        var clipboard = new DummyClipboard();

        // Should not throw
        clipboard.Rotate();

        // GetData should still return empty
        var result = clipboard.GetData();
        Assert.Equal(string.Empty, result.Text);
        Assert.Equal(SelectionType.Characters, result.Type);
    }

    [Fact]
    public void GetData_AlwaysReturnsNewEmptyInstance()
    {
        var clipboard = new DummyClipboard();

        var result1 = clipboard.GetData();
        var result2 = clipboard.GetData();

        // Should not be the same reference (each call creates new instance)
        Assert.NotSame(result1, result2);

        // Both should be empty
        Assert.Equal(string.Empty, result1.Text);
        Assert.Equal(string.Empty, result2.Text);
    }

    [Fact]
    public void MultipleSetData_AllIgnored()
    {
        var clipboard = new DummyClipboard();

        clipboard.SetData(new ClipboardData("first", SelectionType.Lines));
        clipboard.SetData(new ClipboardData("second", SelectionType.Block));
        clipboard.SetData(new ClipboardData("third", SelectionType.Characters));

        // All ignored - should still return empty
        var result = clipboard.GetData();
        Assert.Equal(string.Empty, result.Text);
        Assert.Equal(SelectionType.Characters, result.Type);
    }
}
