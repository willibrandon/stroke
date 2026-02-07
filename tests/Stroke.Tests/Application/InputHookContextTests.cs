using Stroke.Application;
using Xunit;

namespace Stroke.Tests.Application;

/// <summary>
/// Tests for <see cref="InputHookContext"/>.
/// </summary>
public sealed class InputHookContextTests
{
    [Fact]
    public void FileDescriptor_ReturnsProvidedValue()
    {
        var context = new InputHookContext(42, () => { });

        Assert.Equal(42, context.FileDescriptor);
    }

    [Fact]
    public void InputIsReady_InvokesCallback()
    {
        var callbackInvoked = false;
        var context = new InputHookContext(0, () => callbackInvoked = true);

        context.InputIsReady();

        Assert.True(callbackInvoked);
    }

    [Fact]
    public void InputIsReady_CanBeCalledMultipleTimes()
    {
        var callCount = 0;
        var context = new InputHookContext(0, () => callCount++);

        context.InputIsReady();
        context.InputIsReady();
        context.InputIsReady();

        Assert.Equal(3, callCount);
    }

    [Fact]
    public void Constructor_NullCallback_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(
            () => new InputHookContext(0, null!));
        Assert.Equal("inputIsReadyCallback", exception.ParamName);
    }
}
