using Stroke.Application;
using Stroke.FormattedText;
using Stroke.KeyBinding;
using Xunit;

using AppContext = Stroke.Application.AppContext;

namespace Stroke.Tests.Application;

public class DummyApplicationTests
{
    [Fact]
    public void Constructor_Creates()
    {
        var dummy = new DummyApplication();
        Assert.NotNull(dummy);
    }

    [Fact]
    public void Constructor_IsNotRunning()
    {
        var dummy = new DummyApplication();
        Assert.False(dummy.IsRunning);
    }

    [Fact]
    public void Constructor_HasLayout()
    {
        var dummy = new DummyApplication();
        Assert.NotNull(dummy.Layout);
    }

    [Fact]
    public void Constructor_HasViState()
    {
        var dummy = new DummyApplication();
        Assert.NotNull(dummy.ViState);
    }

    [Fact]
    public void Constructor_HasEmacsState()
    {
        var dummy = new DummyApplication();
        Assert.NotNull(dummy.EmacsState);
    }

    [Fact]
    public void Constructor_HasRenderer()
    {
        var dummy = new DummyApplication();
        Assert.NotNull(dummy.Renderer);
    }

    [Fact]
    public void Constructor_HasKeyProcessor()
    {
        var dummy = new DummyApplication();
        Assert.NotNull(dummy.KeyProcessor);
    }

    [Fact]
    public void Constructor_EditingModeIsEmacs()
    {
        var dummy = new DummyApplication();
        Assert.Equal(EditingMode.Emacs, dummy.EditingMode);
    }

    [Fact]
    public void Run_ThrowsNotImplementedException()
    {
        var dummy = new DummyApplication();
        Assert.Throws<NotImplementedException>(() => dummy.Run());
    }

    [Fact]
    public void RunAsync_ThrowsNotImplementedException()
    {
        var dummy = new DummyApplication();
        // DummyApplication.RunAsync throws synchronously (new keyword hides base)
        // but returns Task<object?>, so we need to handle the synchronous throw
        Assert.Throws<NotImplementedException>(() => { _ = dummy.RunAsync(); });
    }

    [Fact]
    public void RunSystemCommandAsync_ThrowsNotImplementedException()
    {
        var dummy = new DummyApplication();
        Assert.Throws<NotImplementedException>(() => { _ = dummy.RunSystemCommandAsync("echo test"); });
    }

    [Fact]
    public void SuspendToBackground_ThrowsNotImplementedException()
    {
        var dummy = new DummyApplication();
        Assert.Throws<NotImplementedException>(() => dummy.SuspendToBackground());
    }

    [Fact]
    public void GetApp_ReturnsDummyApplicationWhenNoneRunning()
    {
        // AppContext.GetApp() returns DummyApplication when no app is running
        var app = AppContext.GetApp();
        Assert.IsType<DummyApplication>(app);
    }

    [Fact]
    public void GetAppOrNull_ReturnsNullWhenNoneRunning()
    {
        var app = AppContext.GetAppOrNull();
        Assert.Null(app);
    }

    [Fact]
    public void Constructor_InheritsFromApplication()
    {
        var dummy = new DummyApplication();
        Assert.IsAssignableFrom<Application<object?>>(dummy);
    }

    [Fact]
    public void Constructor_FullScreenIsFalse()
    {
        var dummy = new DummyApplication();
        Assert.False(dummy.FullScreen);
    }

    [Fact]
    public void Constructor_EraseWhenDoneIsFalse()
    {
        var dummy = new DummyApplication();
        Assert.False(dummy.EraseWhenDone);
    }

    [Fact]
    public void Run_WithParameters_ThrowsNotImplementedException()
    {
        var dummy = new DummyApplication();
        Assert.Throws<NotImplementedException>(() =>
            dummy.Run(preRun: () => { }, setExceptionHandler: false));
    }

    [Fact]
    public void RunAsync_WithParameters_ThrowsNotImplementedException()
    {
        var dummy = new DummyApplication();
        Assert.Throws<NotImplementedException>(() =>
            { _ = dummy.RunAsync(preRun: () => { }, setExceptionHandler: false); });
    }

    [Fact]
    public void RunSystemCommandAsync_WithParameters_ThrowsNotImplementedException()
    {
        var dummy = new DummyApplication();
        Assert.Throws<NotImplementedException>(() =>
            { _ = dummy.RunSystemCommandAsync("ls", waitForEnter: false); });
    }

    [Fact]
    public void SuspendToBackground_WithParameters_ThrowsNotImplementedException()
    {
        var dummy = new DummyApplication();
        Assert.Throws<NotImplementedException>(() =>
            dummy.SuspendToBackground(suspendGroup: false));
    }
}
