using Stroke.Clipboard;
using Xunit;

namespace Stroke.Tests.Clipboard;

/// <summary>
/// Tests for <see cref="StringClipboardProvider"/>.
/// </summary>
public sealed class StringClipboardProviderTests
{
    [Fact]
    public void DefaultConstructor_GivesEmptyText()
    {
        var provider = new StringClipboardProvider();

        Assert.Equal("", provider.GetText());
    }

    [Fact]
    public void Constructor_WithInitialText_SetsText()
    {
        var provider = new StringClipboardProvider("hello");

        Assert.Equal("hello", provider.GetText());
    }

    [Fact]
    public void SetText_ThenGetText_RoundTrips()
    {
        var provider = new StringClipboardProvider();

        provider.SetText("test value");
        var result = provider.GetText();

        Assert.Equal("test value", result);
    }

    [Fact]
    public void SetText_Overwrites_PreviousText()
    {
        var provider = new StringClipboardProvider("initial");

        provider.SetText("updated");

        Assert.Equal("updated", provider.GetText());
    }

    [Fact]
    public void SetText_EmptyString_ClearsText()
    {
        var provider = new StringClipboardProvider("not empty");

        provider.SetText("");

        Assert.Equal("", provider.GetText());
    }

    [Fact]
    public async Task ConcurrentAccess_NoDataCorruption()
    {
        var provider = new StringClipboardProvider();
        var tasks = new List<Task>();
        var ct = TestContext.Current.CancellationToken;

        for (int i = 0; i < 10; i++)
        {
            int threadId = i;
            tasks.Add(Task.Run(() =>
            {
                for (int j = 0; j < 100; j++)
                {
                    provider.SetText($"thread-{threadId}-{j}");
                    var text = provider.GetText();
                    Assert.NotNull(text);
                }
            }, ct));
        }

        await Task.WhenAll(tasks);

        // Provider should still be functional
        var final = provider.GetText();
        Assert.NotNull(final);
    }
}
