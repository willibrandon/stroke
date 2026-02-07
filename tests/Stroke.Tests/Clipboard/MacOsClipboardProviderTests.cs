using Stroke.Clipboard;
using Xunit;

namespace Stroke.Tests.Clipboard;

/// <summary>
/// Platform-gated integration tests for <see cref="MacOsClipboardProvider"/>.
/// These tests exercise real OS clipboard interaction on macOS.
/// </summary>
[Collection("SystemClipboard")]
public sealed class MacOsClipboardProviderTests
{
    [Fact]
    public void SetText_ThenGetText_RoundTrips()
    {
        if (!OperatingSystem.IsMacOS()) return;

        var provider = CreateProvider();
        var testText = $"stroke-roundtrip-{Guid.NewGuid():N}";

        provider.SetText(testText);
        var result = provider.GetText();

        Assert.Equal(testText, result);
    }

    [Fact]
    public void SetText_NonEmptyText_SurvivesRoundTrip()
    {
        if (!OperatingSystem.IsMacOS()) return;

        var provider = CreateProvider();

        provider.SetText("hello from Stroke");
        var result = provider.GetText();

        Assert.Equal("hello from Stroke", result);
    }

    [Fact]
    public void SetText_EmptyText_RoundTrips()
    {
        if (!OperatingSystem.IsMacOS()) return;

        var provider = CreateProvider();

        provider.SetText("");
        var result = provider.GetText();

        Assert.Equal("", result);
    }

    [Fact]
    public void SetText_TextWithNewlines_PreservesNewlines()
    {
        if (!OperatingSystem.IsMacOS()) return;

        var provider = CreateProvider();
        var text = "line1\nline2\nline3";

        provider.SetText(text);
        var result = provider.GetText();

        Assert.Equal(text, result);
    }

    [Fact]
    public void SetText_UnicodeText_SurvivesRoundTrip()
    {
        if (!OperatingSystem.IsMacOS()) return;

        var provider = CreateProvider();
        var text = "Hello 世界 🌍";

        provider.SetText(text);
        var result = provider.GetText();

        Assert.Equal(text, result);
    }

    /// <summary>
    /// Creates a MacOsClipboardProvider. Must only be called within an
    /// <c>OperatingSystem.IsMacOS()</c> guard for platform analyzer compliance.
    /// </summary>
    private static MacOsClipboardProvider CreateProvider()
    {
        if (!OperatingSystem.IsMacOS())
        {
            throw new PlatformNotSupportedException("MacOsClipboardProvider requires macOS");
        }

        return new MacOsClipboardProvider();
    }
}
