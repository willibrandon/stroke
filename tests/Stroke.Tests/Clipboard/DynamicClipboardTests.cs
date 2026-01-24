using Stroke.Clipboard;
using Stroke.Core;
using Xunit;

namespace Stroke.Tests.Clipboard;

/// <summary>
/// Tests for <see cref="DynamicClipboard"/>.
/// </summary>
public sealed class DynamicClipboardTests
{
    // === US3: Constructor Tests ===

    [Fact]
    public void Constructor_WithNullDelegate_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new DynamicClipboard(null!));
    }

    [Fact]
    public void Constructor_WithValidDelegate_Succeeds()
    {
        var clipboard = new DynamicClipboard(() => new InMemoryClipboard());

        Assert.NotNull(clipboard);
    }

    // === US3: Delegation Tests ===

    [Fact]
    public void SetData_DelegatesToUnderlyingClipboard()
    {
        var underlying = new InMemoryClipboard();
        var dynamic = new DynamicClipboard(() => underlying);
        var data = new ClipboardData("test", SelectionType.Lines);

        dynamic.SetData(data);

        var result = underlying.GetData();
        Assert.Equal("test", result.Text);
        Assert.Equal(SelectionType.Lines, result.Type);
    }

    [Fact]
    public void GetData_DelegatesToUnderlyingClipboard()
    {
        var underlying = new InMemoryClipboard();
        underlying.SetText("underlying data");
        var dynamic = new DynamicClipboard(() => underlying);

        var result = dynamic.GetData();

        Assert.Equal("underlying data", result.Text);
    }

    [Fact]
    public void SetText_DelegatesToUnderlyingClipboard()
    {
        var underlying = new InMemoryClipboard();
        var dynamic = new DynamicClipboard(() => underlying);

        dynamic.SetText("hello");

        Assert.Equal("hello", underlying.GetData().Text);
    }

    [Fact]
    public void Rotate_DelegatesToUnderlyingClipboard()
    {
        var underlying = new InMemoryClipboard();
        underlying.SetText("first");
        underlying.SetText("second");
        underlying.SetText("third");
        var dynamic = new DynamicClipboard(() => underlying);

        // Ring is [third, second, first]
        Assert.Equal("third", dynamic.GetData().Text);

        dynamic.Rotate();

        // Ring should now be [second, first, third]
        Assert.Equal("second", dynamic.GetData().Text);
        Assert.Equal("second", underlying.GetData().Text);
    }

    // === US3: Null Fallback Tests ===

    [Fact]
    public void WhenDelegateReturnsNull_SetData_FallsBackToDummyBehavior()
    {
        var dynamic = new DynamicClipboard(() => null);

        // Should not throw - DummyClipboard behavior is no-op
        dynamic.SetData(new ClipboardData("ignored"));
    }

    [Fact]
    public void WhenDelegateReturnsNull_GetData_ReturnsEmptyClipboardData()
    {
        var dynamic = new DynamicClipboard(() => null);

        var result = dynamic.GetData();

        Assert.Equal(string.Empty, result.Text);
        Assert.Equal(SelectionType.Characters, result.Type);
    }

    [Fact]
    public void WhenDelegateReturnsNull_SetText_FallsBackToDummyBehavior()
    {
        var dynamic = new DynamicClipboard(() => null);

        // Should not throw - DummyClipboard behavior is no-op
        dynamic.SetText("ignored");

        // Verify nothing was stored
        Assert.Equal(string.Empty, dynamic.GetData().Text);
    }

    [Fact]
    public void WhenDelegateReturnsNull_Rotate_FallsBackToDummyBehavior()
    {
        var dynamic = new DynamicClipboard(() => null);

        // Should not throw - DummyClipboard behavior is no-op
        dynamic.Rotate();
    }

    // === US3: Exception Propagation Tests ===

    [Fact]
    public void WhenDelegateThrows_ExceptionPropagatesToCaller()
    {
        var dynamic = new DynamicClipboard(() => throw new InvalidOperationException("Test exception"));

        var ex = Assert.Throws<InvalidOperationException>(() => dynamic.GetData());
        Assert.Equal("Test exception", ex.Message);
    }

    [Fact]
    public void WhenDelegateThrows_OnSetData_ExceptionPropagates()
    {
        var dynamic = new DynamicClipboard(() => throw new InvalidOperationException("SetData test"));

        Assert.Throws<InvalidOperationException>(() => dynamic.SetData(new ClipboardData("test")));
    }

    [Fact]
    public void WhenDelegateThrows_OnSetText_ExceptionPropagates()
    {
        var dynamic = new DynamicClipboard(() => throw new InvalidOperationException("SetText test"));

        Assert.Throws<InvalidOperationException>(() => dynamic.SetText("test"));
    }

    [Fact]
    public void WhenDelegateThrows_OnRotate_ExceptionPropagates()
    {
        var dynamic = new DynamicClipboard(() => throw new InvalidOperationException("Rotate test"));

        Assert.Throws<InvalidOperationException>(() => dynamic.Rotate());
    }

    // === US3: Dynamic Switching Tests ===

    [Fact]
    public void BackingClipboardChange_UsesCurrentClipboard()
    {
        var clipboard1 = new InMemoryClipboard();
        var clipboard2 = new InMemoryClipboard();
        IClipboard? current = clipboard1;

        var dynamic = new DynamicClipboard(() => current);

        // Store in clipboard1
        dynamic.SetText("first");
        Assert.Equal("first", clipboard1.GetData().Text);
        Assert.Equal(string.Empty, clipboard2.GetData().Text);

        // Switch to clipboard2
        current = clipboard2;

        // Store in clipboard2
        dynamic.SetText("second");
        Assert.Equal("first", clipboard1.GetData().Text);
        Assert.Equal("second", clipboard2.GetData().Text);

        // GetData should return from clipboard2
        Assert.Equal("second", dynamic.GetData().Text);
    }

    [Fact]
    public void DelegateCalledOnEveryOperation()
    {
        int callCount = 0;
        var underlying = new InMemoryClipboard();
        var dynamic = new DynamicClipboard(() =>
        {
            callCount++;
            return underlying;
        });

        dynamic.SetText("a");
        Assert.Equal(1, callCount);

        dynamic.GetData();
        Assert.Equal(2, callCount);

        dynamic.Rotate();
        Assert.Equal(3, callCount);

        dynamic.SetData(new ClipboardData("b"));
        Assert.Equal(4, callCount);
    }
}
