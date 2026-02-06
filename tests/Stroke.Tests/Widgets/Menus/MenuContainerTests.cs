using Stroke.Layout;
using Stroke.Layout.Containers;
using Stroke.Layout.Controls;
using Stroke.Widgets.Base;
using Stroke.Widgets.Menus;
using Xunit;

namespace Stroke.Tests.Widgets.Menus;

/// <summary>
/// Tests for <see cref="MenuContainer"/>.
/// </summary>
public sealed class MenuContainerTests
{
    private static AnyContainer CreateBody()
    {
        return new AnyContainer(new Window(content: new DummyControl()));
    }

    [Fact]
    public void Constructor_SetsBody()
    {
        var body = CreateBody();
        var items = new List<MenuItem> { new("File") };

        var container = new MenuContainer(body, items);

        Assert.Equal(body, container.Body);
    }

    [Fact]
    public void Constructor_SetsMenuItems()
    {
        var body = CreateBody();
        var items = new List<MenuItem> { new("File"), new("Edit"), new("View") };

        var container = new MenuContainer(body, items);

        Assert.Equal(3, container.MenuItems.Count);
        Assert.Equal("File", container.MenuItems[0].Text);
        Assert.Equal("Edit", container.MenuItems[1].Text);
        Assert.Equal("View", container.MenuItems[2].Text);
    }

    [Fact]
    public void Constructor_CreatesControl()
    {
        var body = CreateBody();
        var items = new List<MenuItem> { new("File") };

        var container = new MenuContainer(body, items);

        Assert.NotNull(container.Control);
    }

    [Fact]
    public void Constructor_CreatesWindow()
    {
        var body = CreateBody();
        var items = new List<MenuItem> { new("File") };

        var container = new MenuContainer(body, items);

        Assert.NotNull(container.Window);
    }

    [Fact]
    public void Constructor_CreatesFloats()
    {
        var body = CreateBody();
        var items = new List<MenuItem> { new("File") };

        var container = new MenuContainer(body, items);

        // 3 submenu floats (levels 0, 1, 2)
        Assert.Equal(3, container.Floats.Count);
    }

    [Fact]
    public void Constructor_WithAdditionalFloats_AddsThemToList()
    {
        var body = CreateBody();
        var items = new List<MenuItem> { new("File") };
        var extraFloat = new Float(
            content: new AnyContainer(new Window(content: new DummyControl())));

        var container = new MenuContainer(body, items, floats: [extraFloat]);

        // 3 submenu floats + 1 extra
        Assert.Equal(4, container.Floats.Count);
    }

    [Fact]
    public void PtContainer_ReturnsFloatContainer()
    {
        var body = CreateBody();
        var items = new List<MenuItem> { new("File") };

        var container = new MenuContainer(body, items);

        Assert.NotNull(container.PtContainer());
        Assert.IsType<FloatContainer>(container.PtContainer());
    }

    [Fact]
    public void Constructor_WithNestedMenuItems_CreatesContainer()
    {
        var body = CreateBody();
        var items = new List<MenuItem>
        {
            new("File", children:
            [
                new("New"),
                new("Open", children:
                [
                    new("Recent"),
                    new("From Disk"),
                ]),
                new("-"),  // separator
                new("Exit", handler: () => { }),
            ]),
            new("Edit", children:
            [
                new("Undo"),
                new("Redo"),
            ]),
        };

        var container = new MenuContainer(body, items);

        Assert.Equal(2, container.MenuItems.Count);
        Assert.Equal(4, container.MenuItems[0].Children.Count);
        Assert.Equal(2, container.MenuItems[0].Children[1].Children.Count);
    }

    [Fact]
    public void Constructor_WithDisabledItems_CreatesContainer()
    {
        var body = CreateBody();
        var items = new List<MenuItem>
        {
            new("File", children:
            [
                new("New"),
                new("Paste", disabled: true),
            ]),
        };

        var container = new MenuContainer(body, items);

        Assert.True(container.MenuItems[0].Children[1].Disabled);
    }

    [Fact]
    public void Constructor_EmptyMenuItems_CreatesContainer()
    {
        var body = CreateBody();
        var items = new List<MenuItem>();

        var container = new MenuContainer(body, items);

        Assert.Empty(container.MenuItems);
    }
}
