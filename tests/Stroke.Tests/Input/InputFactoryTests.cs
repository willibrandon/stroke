using System.Runtime.Versioning;
using Stroke.Input;
using Stroke.Input.Pipe;
using Stroke.Input.Vt100;
using Stroke.Input.Windows;
using Xunit;

namespace Stroke.Tests.Input;

public class InputFactoryTests
{
    #region T057: Cross-Platform Factory Tests
    [Fact]
    public void Create_ReturnsInput()
    {
        using var input = InputFactory.Create();

        Assert.NotNull(input);
        Assert.IsAssignableFrom<IInput>(input);
    }

    [Fact]
    public void Create_WithAlwaysPreferTty_ReturnsInput()
    {
        using var input = InputFactory.Create(alwaysPreferTty: true);

        Assert.NotNull(input);
        Assert.IsAssignableFrom<IInput>(input);
    }

    [Fact]
    public void CreatePipe_ReturnsPipeInput()
    {
        using var pipeInput = InputFactory.CreatePipe();

        Assert.NotNull(pipeInput);
        Assert.IsAssignableFrom<IPipeInput>(pipeInput);
    }

    [Fact]
    public void CreatePipe_ReturnsSimplePipeInput()
    {
        using var pipeInput = InputFactory.CreatePipe();

        Assert.IsType<SimplePipeInput>(pipeInput);
    }

    [Fact]
    public void CreatePipe_MultipleInstances_AreIndependent()
    {
        using var pipe1 = InputFactory.CreatePipe();
        using var pipe2 = InputFactory.CreatePipe();

        pipe1.SendText("hello");

        var keys1 = pipe1.ReadKeys();
        var keys2 = pipe2.ReadKeys();

        Assert.NotEmpty(keys1);
        Assert.Empty(keys2);
    }

    #endregion

    #region T057: Cross-Platform Input Type Tests

    [Fact]
    [SupportedOSPlatform("macos")]
    public void Create_OnMacOS_ReturnsVt100Input()
    {
        if (!OperatingSystem.IsMacOS())
            return; // Skip on other platforms

        // When stdin is not redirected (i.e., running in a real terminal)
        // the factory should return Vt100Input on macOS
        using var input = InputFactory.Create(alwaysPreferTty: true);

        Assert.IsType<Vt100Input>(input);
    }

    [Fact]
    [SupportedOSPlatform("linux")]
    public void Create_OnLinux_ReturnsVt100Input()
    {
        if (!OperatingSystem.IsLinux())
            return; // Skip on other platforms

        // When stdin is not redirected (i.e., running in a real terminal)
        // the factory should return Vt100Input on Linux
        using var input = InputFactory.Create(alwaysPreferTty: true);

        Assert.IsType<Vt100Input>(input);
    }

    [Fact]
    [SupportedOSPlatform("freebsd")]
    public void Create_OnFreeBSD_ReturnsVt100Input()
    {
        if (!OperatingSystem.IsFreeBSD())
            return; // Skip on other platforms

        using var input = InputFactory.Create(alwaysPreferTty: true);

        Assert.IsType<Vt100Input>(input);
    }

    [Fact]
    [SupportedOSPlatform("windows")]
    public void Create_OnWindows_ReturnsWin32Input()
    {
        if (!OperatingSystem.IsWindows())
            return; // Skip on other platforms

        // When stdin is not redirected (i.e., running in a real terminal)
        // the factory should return Win32Input on Windows
        using var input = InputFactory.Create(alwaysPreferTty: true);

        Assert.IsType<Win32Input>(input);
    }

    [Fact]
    public void Create_AlwaysPreferTty_ReturnsCorrectPlatformType()
    {
        using var input = InputFactory.Create(alwaysPreferTty: true);

        if (OperatingSystem.IsWindows())
        {
            Assert.IsType<Win32Input>(input);
        }
        else if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS() || OperatingSystem.IsFreeBSD())
        {
            Assert.IsType<Vt100Input>(input);
        }
        else
        {
            Assert.IsType<DummyInput>(input);
        }
    }

    #endregion
}
