using Stroke.Clipboard;
using Xunit;

namespace Stroke.Tests.Clipboard;

/// <summary>
/// Tests for <see cref="ClipboardProviderNotAvailableException"/>.
/// </summary>
public sealed class ClipboardProviderNotAvailableExceptionTests
{
    [Fact]
    public void Constructor_WithMessage_StoresMessage()
    {
        var ex = new ClipboardProviderNotAvailableException("No clipboard tool found");

        Assert.Equal("No clipboard tool found", ex.Message);
    }

    [Fact]
    public void Constructor_WithMessageAndInnerException_StoresBoth()
    {
        var inner = new InvalidOperationException("inner");
        var ex = new ClipboardProviderNotAvailableException("No clipboard tool found", inner);

        Assert.Equal("No clipboard tool found", ex.Message);
        Assert.Same(inner, ex.InnerException);
    }

    [Fact]
    public void Exception_IsCatchableAsException()
    {
        Exception caught = null!;

        try
        {
            throw new ClipboardProviderNotAvailableException("test");
        }
        catch (Exception ex)
        {
            caught = ex;
        }

        Assert.IsType<ClipboardProviderNotAvailableException>(caught);
    }
}
