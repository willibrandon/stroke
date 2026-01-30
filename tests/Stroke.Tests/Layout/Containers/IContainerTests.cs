using Stroke.KeyBinding;
using Stroke.Layout;
using Stroke.Layout.Containers;
using Xunit;

namespace Stroke.Tests.Layout.Containers;

/// <summary>
/// Tests for IContainer interface contract.
/// </summary>
public sealed class IContainerTests
{
    /// <summary>
    /// Test implementation of IContainer for verifying interface contract.
    /// </summary>
    private sealed class TestContainer : IContainer
    {
        public bool ResetCalled { get; private set; }
        public bool PreferredWidthCalled { get; private set; }
        public bool PreferredHeightCalled { get; private set; }
        public bool WriteToScreenCalled { get; private set; }
        public bool GetKeyBindingsCalled { get; private set; }
        public bool GetChildrenCalled { get; private set; }

        public int LastMaxAvailableWidth { get; private set; }
        public int LastWidth { get; private set; }
        public int LastMaxAvailableHeight { get; private set; }

        public bool IsModal => false;

        public void Reset()
        {
            ResetCalled = true;
        }

        public Dimension PreferredWidth(int maxAvailableWidth)
        {
            PreferredWidthCalled = true;
            LastMaxAvailableWidth = maxAvailableWidth;
            return new Dimension(min: 10, max: 100, preferred: 50);
        }

        public Dimension PreferredHeight(int width, int maxAvailableHeight)
        {
            PreferredHeightCalled = true;
            LastWidth = width;
            LastMaxAvailableHeight = maxAvailableHeight;
            return new Dimension(min: 5, max: 50, preferred: 25);
        }

        public void WriteToScreen(
            Screen screen,
            MouseHandlers mouseHandlers,
            WritePosition writePosition,
            string parentStyle,
            bool eraseBg,
            int? zIndex)
        {
            WriteToScreenCalled = true;
        }

        public IKeyBindingsBase? GetKeyBindings()
        {
            GetKeyBindingsCalled = true;
            return null;
        }

        public IReadOnlyList<IContainer> GetChildren()
        {
            GetChildrenCalled = true;
            return [];
        }
    }

    [Fact]
    public void IContainer_Reset_CanBeCalled()
    {
        var container = new TestContainer();

        container.Reset();

        Assert.True(container.ResetCalled);
    }

    [Fact]
    public void IContainer_PreferredWidth_ReturnsValidDimension()
    {
        var container = new TestContainer();

        var result = container.PreferredWidth(80);

        Assert.True(container.PreferredWidthCalled);
        Assert.Equal(80, container.LastMaxAvailableWidth);
        Assert.Equal(10, result.Min);
        Assert.Equal(100, result.Max);
        Assert.Equal(50, result.Preferred);
    }

    [Fact]
    public void IContainer_PreferredHeight_ReturnsValidDimension()
    {
        var container = new TestContainer();

        var result = container.PreferredHeight(40, 24);

        Assert.True(container.PreferredHeightCalled);
        Assert.Equal(40, container.LastWidth);
        Assert.Equal(24, container.LastMaxAvailableHeight);
        Assert.Equal(5, result.Min);
        Assert.Equal(50, result.Max);
        Assert.Equal(25, result.Preferred);
    }

    [Fact]
    public void IContainer_WriteToScreen_CanBeCalled()
    {
        var container = new TestContainer();
        var screen = new Screen(initialWidth: 80, initialHeight: 24);
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 80, 24);

        container.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);

        Assert.True(container.WriteToScreenCalled);
    }

    [Fact]
    public void IContainer_IsModal_DefaultsFalse()
    {
        var container = new TestContainer();

        Assert.False(container.IsModal);
    }

    [Fact]
    public void IContainer_GetKeyBindings_CanReturnNull()
    {
        var container = new TestContainer();

        var result = container.GetKeyBindings();

        Assert.True(container.GetKeyBindingsCalled);
        Assert.Null(result);
    }

    [Fact]
    public void IContainer_GetChildren_ReturnsEmptyListByDefault()
    {
        var container = new TestContainer();

        var result = container.GetChildren();

        Assert.True(container.GetChildrenCalled);
        Assert.Empty(result);
    }
}
