using Stroke.Shortcuts;
using Xunit;

namespace Stroke.Tests.Shortcuts;

/// <summary>
/// Tests for <see cref="KeyboardInterrupt"/> and <see cref="KeyboardInterruptException"/>.
/// </summary>
public sealed class KeyboardInterruptTests
{
    [Fact]
    public void DefaultConstructor_CreatesException()
    {
        var ex = new KeyboardInterrupt();

        Assert.NotNull(ex);
        Assert.IsAssignableFrom<Exception>(ex);
    }

    [Fact]
    public void MessageConstructor_SetsMessage()
    {
        var ex = new KeyboardInterrupt("user cancelled");

        Assert.Equal("user cancelled", ex.Message);
    }

    [Fact]
    public void InnerExceptionConstructor_SetsInnerException()
    {
        var inner = new InvalidOperationException("inner");
        var ex = new KeyboardInterrupt("outer", inner);

        Assert.Equal("outer", ex.Message);
        Assert.Same(inner, ex.InnerException);
    }

    [Fact]
    public void KeyboardInterruptException_DefaultConstructor()
    {
        var ex = new KeyboardInterruptException();

        Assert.NotNull(ex);
        Assert.IsAssignableFrom<Exception>(ex);
    }

    [Fact]
    public void KeyboardInterruptException_MessageConstructor()
    {
        var ex = new KeyboardInterruptException("test");

        Assert.Equal("test", ex.Message);
    }

    [Fact]
    public void KeyboardInterruptException_InnerExceptionConstructor()
    {
        var inner = new InvalidOperationException("cause");
        var ex = new KeyboardInterruptException("wrapper", inner);

        Assert.Equal("wrapper", ex.Message);
        Assert.Same(inner, ex.InnerException);
    }
}
