using Stroke.Input;
using Stroke.KeyBinding;
using Stroke.Widgets.Menus;
using Xunit;

namespace Stroke.Tests.Widgets.Menus;

/// <summary>
/// Tests for <see cref="MenuItem"/>.
/// </summary>
public sealed class MenuItemTests
{
    [Fact]
    public void Constructor_Default_HasEmptyText()
    {
        var item = new MenuItem();

        Assert.Equal("", item.Text);
        Assert.Null(item.Handler);
        Assert.Empty(item.Children);
        Assert.Null(item.Shortcut);
        Assert.False(item.Disabled);
        Assert.Equal(0, item.SelectedItem);
    }

    [Fact]
    public void Constructor_WithText_SetsText()
    {
        var item = new MenuItem("File");

        Assert.Equal("File", item.Text);
    }

    [Fact]
    public void Constructor_WithHandler_SetsHandler()
    {
        var called = false;
        var item = new MenuItem("Open", handler: () => called = true);

        Assert.NotNull(item.Handler);
        item.Handler!();
        Assert.True(called);
    }

    [Fact]
    public void Constructor_WithChildren_SetsChildren()
    {
        var children = new List<MenuItem>
        {
            new("New"),
            new("Open"),
            new("Save"),
        };
        var item = new MenuItem("File", children: children);

        Assert.Equal(3, item.Children.Count);
        Assert.Equal("New", item.Children[0].Text);
        Assert.Equal("Open", item.Children[1].Text);
        Assert.Equal("Save", item.Children[2].Text);
    }

    [Fact]
    public void Constructor_WithShortcut_SetsShortcut()
    {
        var shortcut = new List<KeyOrChar> { new(Keys.ControlS) };
        var item = new MenuItem("Save", shortcut: shortcut);

        Assert.NotNull(item.Shortcut);
        Assert.Single(item.Shortcut!);
    }

    [Fact]
    public void Constructor_WithDisabled_SetsDisabled()
    {
        var item = new MenuItem("Paste", disabled: true);

        Assert.True(item.Disabled);
    }

    [Fact]
    public void SelectedItem_CanBeSet()
    {
        var item = new MenuItem("File");

        item.SelectedItem = 3;

        Assert.Equal(3, item.SelectedItem);
    }

    [Fact]
    public void Width_NoChildren_ReturnsZero()
    {
        var item = new MenuItem("File");

        Assert.Equal(0, item.Width);
    }

    [Fact]
    public void Width_WithChildren_ReturnsMaxChildWidth()
    {
        var children = new List<MenuItem>
        {
            new("New"),
            new("Open File"),
            new("Save"),
        };
        var item = new MenuItem("File", children: children);

        // "Open File" is 9 characters wide
        Assert.Equal(9, item.Width);
    }

    [Fact]
    public void Width_WithEmptyChildren_ReturnsMaxWidth()
    {
        var children = new List<MenuItem>
        {
            new("A"),
            new("AB"),
            new("ABC"),
        };
        var item = new MenuItem("File", children: children);

        Assert.Equal(3, item.Width);
    }

    [Fact]
    public void Width_WithSeparator_IncludesSeparatorWidth()
    {
        var children = new List<MenuItem>
        {
            new("New"),
            new("-"),  // separator
            new("Open"),
        };
        var item = new MenuItem("File", children: children);

        Assert.Equal(4, item.Width); // "Open" = 4
    }

    [Fact]
    public void Constructor_NullChildren_DefaultsToEmpty()
    {
        var item = new MenuItem("File", children: null);

        Assert.Empty(item.Children);
    }

    [Fact]
    public void NestedMenuItems_WorkCorrectly()
    {
        var submenu = new MenuItem("Recent Files", children:
        [
            new("file1.txt"),
            new("file2.txt"),
        ]);
        var item = new MenuItem("File", children: [submenu]);

        Assert.Single(item.Children);
        Assert.Equal(2, item.Children[0].Children.Count);
        Assert.Equal("file1.txt", item.Children[0].Children[0].Text);
    }
}
