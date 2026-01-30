using Stroke.Layout.Containers;
using Stroke.Layout.Controls;
using Xunit;

namespace Stroke.Tests.Layout.Containers;

/// <summary>
/// Tests for AnyContainer implicit conversions and behavior.
/// </summary>
public sealed class AnyContainerTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_WithIContainer_HasValue()
    {
        var container = new AnyContainer(new Window());

        Assert.True(container.HasValue);
    }

    [Fact]
    public void Constructor_WithIMagicContainer_HasValue()
    {
        var magic = new TestMagicContainer(new Window());
        var container = new AnyContainer(magic);

        Assert.True(container.HasValue);
    }

    [Fact]
    public void Default_HasNoValue()
    {
        AnyContainer container = default;

        Assert.False(container.HasValue);
    }

    #endregion

    #region ToContainer Tests

    [Fact]
    public void ToContainer_Window_ReturnsSame()
    {
        var window = new Window();
        var container = new AnyContainer(window);

        Assert.Same(window, container.ToContainer());
    }

    [Fact]
    public void ToContainer_Magic_CallsPtContainer()
    {
        var window = new Window();
        var magic = new TestMagicContainer(window);
        var container = new AnyContainer(magic);

        Assert.Same(window, container.ToContainer());
    }

    [Fact]
    public void ToContainer_Default_ThrowsInvalidOperationException()
    {
        AnyContainer container = default;

        Assert.Throws<InvalidOperationException>(() => container.ToContainer());
    }

    #endregion

    #region Null Argument Tests

    [Fact]
    public void Constructor_NullIContainer_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new AnyContainer((IContainer)null!));
    }

    [Fact]
    public void Constructor_NullIMagicContainer_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new AnyContainer((IMagicContainer)null!));
    }

    #endregion

    #region From Tests

    [Fact]
    public void From_IContainer_CreatesAnyContainer()
    {
        var window = new Window();
        var container = AnyContainer.From(window);

        Assert.True(container.HasValue);
        Assert.Same(window, container.ToContainer());
    }

    [Fact]
    public void From_IMagicContainer_CreatesAnyContainer()
    {
        var magic = new TestMagicContainer(new Window());
        var container = AnyContainer.From(magic);

        Assert.True(container.HasValue);
    }

    [Fact]
    public void From_InvalidType_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => AnyContainer.From("not a container"));
    }

    #endregion

    #region Container Type Tests

    [Fact]
    public void AnyContainer_AllContainerTypes_WorkCorrectly()
    {
        // Test all container types work through AnyContainer
        var window = new Window();
        Assert.Same(window, new AnyContainer(window).ToContainer());

        var hsplit = new HSplit([window]);
        Assert.Same(hsplit, new AnyContainer(hsplit).ToContainer());

        var vsplit = new VSplit([window]);
        Assert.Same(vsplit, new AnyContainer(vsplit).ToContainer());

        var floatContainer = new FloatContainer(new AnyContainer(new Window()));
        Assert.Same(floatContainer, new AnyContainer(floatContainer).ToContainer());

        var conditional = new ConditionalContainer(new AnyContainer(new Window()));
        Assert.Same(conditional, new AnyContainer(conditional).ToContainer());

        var dynamic = new DynamicContainer(() => new AnyContainer(new Window()));
        Assert.Same(dynamic, new AnyContainer(dynamic).ToContainer());
    }

    #endregion

    private sealed class TestMagicContainer : IMagicContainer
    {
        private readonly IContainer _container;

        public TestMagicContainer(IContainer container) => _container = container;

        public IContainer PtContainer() => _container;
    }
}
