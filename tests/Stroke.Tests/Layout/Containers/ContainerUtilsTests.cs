using Stroke.Layout.Containers;
using Stroke.Layout.Controls;
using Xunit;

namespace Stroke.Tests.Layout.Containers;

/// <summary>
/// Tests for ContainerUtils static class.
/// </summary>
public sealed class ContainerUtilsTests
{
    #region ToContainer Tests

    [Fact]
    public void ToContainer_Window_ReturnsSame()
    {
        var window = new Window(content: new DummyControl());
        var result = ContainerUtils.ToContainer(new AnyContainer(window));

        Assert.Same(window, result);
    }

    [Fact]
    public void ToContainer_HSplit_ReturnsSame()
    {
        var hsplit = new HSplit([new Window()]);
        var result = ContainerUtils.ToContainer(new AnyContainer(hsplit));

        Assert.Same(hsplit, result);
    }

    [Fact]
    public void ToContainer_VSplit_ReturnsSame()
    {
        var vsplit = new VSplit([new Window()]);
        var result = ContainerUtils.ToContainer(new AnyContainer(vsplit));

        Assert.Same(vsplit, result);
    }

    [Fact]
    public void ToContainer_FloatContainer_ReturnsSame()
    {
        var floatContainer = new FloatContainer(new AnyContainer(new Window()));
        var result = ContainerUtils.ToContainer(new AnyContainer(floatContainer));

        Assert.Same(floatContainer, result);
    }

    [Fact]
    public void ToContainer_ConditionalContainer_ReturnsSame()
    {
        var conditional = new ConditionalContainer(new AnyContainer(new Window()));
        var result = ContainerUtils.ToContainer(new AnyContainer(conditional));

        Assert.Same(conditional, result);
    }

    [Fact]
    public void ToContainer_DynamicContainer_ReturnsSame()
    {
        var dynamic = new DynamicContainer(() => new AnyContainer(new Window()));
        var result = ContainerUtils.ToContainer(new AnyContainer(dynamic));

        Assert.Same(dynamic, result);
    }

    #endregion

    #region IMagicContainer Tests

    [Fact]
    public void ToContainer_MagicContainer_CallsPtContainer()
    {
        var window = new Window(content: new DummyControl());
        var magic = new TestMagicContainer(window);
        var result = ContainerUtils.ToContainer(new AnyContainer(magic));

        Assert.Same(window, result);
    }

    #endregion

    #region IsContainer Tests

    [Fact]
    public void IsContainer_IContainer_ReturnsTrue()
    {
        Assert.True(ContainerUtils.IsContainer(new Window()));
    }

    [Fact]
    public void IsContainer_IMagicContainer_ReturnsTrue()
    {
        Assert.True(ContainerUtils.IsContainer(new TestMagicContainer(new Window())));
    }

    [Fact]
    public void IsContainer_OtherType_ReturnsFalse()
    {
        Assert.False(ContainerUtils.IsContainer("not a container"));
    }

    [Fact]
    public void IsContainer_Null_ReturnsFalse()
    {
        Assert.False(ContainerUtils.IsContainer(null));
    }

    #endregion

    #region ToWindow Tests

    [Fact]
    public void ToWindow_Window_ReturnsSame()
    {
        var window = new Window();
        var result = ContainerUtils.ToWindow(new AnyContainer(window));

        Assert.Same(window, result);
    }

    [Fact]
    public void ToWindow_HSplitWithWindow_FindsWindow()
    {
        var window = new Window();
        var hsplit = new HSplit([window]);
        var result = ContainerUtils.ToWindow(new AnyContainer(hsplit));

        Assert.Same(window, result);
    }

    [Fact]
    public void ToWindow_NestedContainers_FindsLeafWindow()
    {
        var window = new Window();
        var inner = new HSplit(new IContainer[] { window });
        var outer = new VSplit(new IContainer[] { inner });
        var result = ContainerUtils.ToWindow(new AnyContainer(outer));

        Assert.Same(window, result);
    }

    #endregion

    private sealed class TestMagicContainer : IMagicContainer
    {
        private readonly IContainer _container;

        public TestMagicContainer(IContainer container) => _container = container;

        public IContainer PtContainer() => _container;
    }
}
