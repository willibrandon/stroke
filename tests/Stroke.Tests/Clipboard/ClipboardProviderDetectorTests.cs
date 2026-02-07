using System.Runtime.InteropServices;
using Stroke.Clipboard;
using Xunit;

namespace Stroke.Tests.Clipboard;

/// <summary>
/// Tests for <see cref="ClipboardProviderDetector"/>.
/// </summary>
[Collection("SystemClipboard")]
public sealed class ClipboardProviderDetectorTests
{
    [Fact]
    public void Detect_ReturnsNonNullProvider()
    {
        // On any supported platform (macOS dev machine), Detect should succeed
        var provider = ClipboardProviderDetector.Detect();

        Assert.NotNull(provider);
    }

    [Fact]
    public void Detect_OnMacOS_ReturnsMacOsProvider()
    {
        if (!OperatingSystem.IsMacOS())
        {
            return; // Platform-gated
        }

        var provider = ClipboardProviderDetector.Detect();

        Assert.IsType<MacOsClipboardProvider>(provider);
    }

    [Fact]
    public void Detect_OnWindows_ReturnsWindowsProvider()
    {
        if (!OperatingSystem.IsWindows())
        {
            return; // Platform-gated
        }

        var provider = ClipboardProviderDetector.Detect();

        Assert.IsType<WindowsClipboardProvider>(provider);
    }

    [Fact]
    public void Detect_ProviderRoundTrip_Works()
    {
        // Integration test: the detected provider can actually read/write clipboard
        var provider = ClipboardProviderDetector.Detect();
        var testText = $"stroke-test-{Guid.NewGuid():N}";

        provider.SetText(testText);
        var result = provider.GetText();

        Assert.Equal(testText, result);
    }
}
