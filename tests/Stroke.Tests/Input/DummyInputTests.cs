using Stroke.Input;
using Xunit;

namespace Stroke.Tests.Input;

public class DummyInputTests
{
    [Fact]
    public void Closed_AlwaysReturnsTrue()
    {
        using var input = new DummyInput();
        Assert.True(input.Closed);
    }

    [Fact]
    public void ReadKeys_ReturnsEmptyList()
    {
        using var input = new DummyInput();
        var keys = input.ReadKeys();

        Assert.NotNull(keys);
        Assert.Empty(keys);
    }

    [Fact]
    public void FlushKeys_ReturnsEmptyList()
    {
        using var input = new DummyInput();
        var keys = input.FlushKeys();

        Assert.NotNull(keys);
        Assert.Empty(keys);
    }

    [Fact]
    public void RawMode_ReturnsDisposable()
    {
        using var input = new DummyInput();
        using var rawMode = input.RawMode();

        Assert.NotNull(rawMode);
    }

    [Fact]
    public void RawMode_DisposableCanBeDisposedMultipleTimes()
    {
        using var input = new DummyInput();
        var rawMode = input.RawMode();

        rawMode.Dispose();
        rawMode.Dispose(); // Should not throw
    }

    [Fact]
    public void CookedMode_ReturnsDisposable()
    {
        using var input = new DummyInput();
        using var cookedMode = input.CookedMode();

        Assert.NotNull(cookedMode);
    }

    [Fact]
    public void Attach_ReturnsDisposable()
    {
        using var input = new DummyInput();
        using var attachment = input.Attach(() => { });

        Assert.NotNull(attachment);
    }

    [Fact]
    public void Attach_CallbackIsInvokedImmediately()
    {
        // DummyInput.Attach calls the callback immediately once, matching Python's
        // behavior. This triggers the application to check Input.Closed and raise
        // EndOfStreamException, enabling clean termination for PrintContainer.
        using var input = new DummyInput();
        var invoked = false;

        using var attachment = input.Attach(() => invoked = true);

        Assert.True(invoked);
    }

    [Fact]
    public void Attach_NullCallback_ThrowsArgumentNullException()
    {
        using var input = new DummyInput();

        Assert.Throws<ArgumentNullException>(() => input.Attach(null!));
    }

    [Fact]
    public void Detach_ReturnsDisposable()
    {
        using var input = new DummyInput();
        using var detach = input.Detach();

        Assert.NotNull(detach);
    }

    [Fact]
    public void FileNo_ThrowsNotSupportedException()
    {
        using var input = new DummyInput();

        Assert.Throws<NotSupportedException>(() => input.FileNo());
    }

    [Fact]
    public void TypeaheadHash_ReturnsUniqueString()
    {
        using var input1 = new DummyInput();
        using var input2 = new DummyInput();

        var hash1 = input1.TypeaheadHash();
        var hash2 = input2.TypeaheadHash();

        Assert.NotNull(hash1);
        Assert.NotNull(hash2);
        Assert.NotEqual(hash1, hash2);
        Assert.StartsWith("DummyInput-", hash1);
        Assert.StartsWith("DummyInput-", hash2);
    }

    [Fact]
    public void Close_CanBeCalledMultipleTimes()
    {
        using var input = new DummyInput();

        input.Close();
        input.Close(); // Should not throw
    }

    [Fact]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        var input = new DummyInput();

        input.Dispose();
        input.Dispose(); // Should not throw
    }

    [Fact]
    public void ReadKeys_AfterDispose_ThrowsObjectDisposedException()
    {
        var input = new DummyInput();
        input.Dispose();

        Assert.Throws<ObjectDisposedException>(() => input.ReadKeys());
    }

    [Fact]
    public void FlushKeys_AfterDispose_ThrowsObjectDisposedException()
    {
        var input = new DummyInput();
        input.Dispose();

        Assert.Throws<ObjectDisposedException>(() => input.FlushKeys());
    }

    [Fact]
    public void Attach_AfterDispose_ThrowsObjectDisposedException()
    {
        var input = new DummyInput();
        input.Dispose();

        Assert.Throws<ObjectDisposedException>(() => input.Attach(() => { }));
    }
}
