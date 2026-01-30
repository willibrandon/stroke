using Stroke.Filters;
using Stroke.Layout.Containers;
using Stroke.Layout.Menus;
using Xunit;

namespace Stroke.Tests.Layout.Menus;

/// <summary>
/// Tests for MultiColumnCompletionsMenu container (US7: multi-column wrapper).
/// </summary>
public sealed class MultiColumnCompletionsMenuTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_Default_CreatesInstance()
    {
        var menu = new MultiColumnCompletionsMenu();
        Assert.NotNull(menu);
    }

    [Fact]
    public void Constructor_WithMinRows_CreatesInstance()
    {
        var menu = new MultiColumnCompletionsMenu(minRows: 5);
        Assert.NotNull(menu);
    }

    [Fact]
    public void Constructor_WithSuggestedMaxColumnWidth_CreatesInstance()
    {
        var menu = new MultiColumnCompletionsMenu(suggestedMaxColumnWidth: 20);
        Assert.NotNull(menu);
    }

    [Fact]
    public void Constructor_WithShowMeta_CreatesInstance()
    {
        var menu = new MultiColumnCompletionsMenu(showMeta: true);
        Assert.NotNull(menu);
    }

    [Fact]
    public void Constructor_WithShowMetaFalse_CreatesInstance()
    {
        var menu = new MultiColumnCompletionsMenu(showMeta: false);
        Assert.NotNull(menu);
    }

    [Fact]
    public void Constructor_WithExtraFilter_CreatesInstance()
    {
        var menu = new MultiColumnCompletionsMenu(extraFilter: true);
        Assert.NotNull(menu);
    }

    [Fact]
    public void Constructor_WithExtraFilterCondition_CreatesInstance()
    {
        var condition = new Condition(() => true);
        var menu = new MultiColumnCompletionsMenu(extraFilter: new FilterOrBool(condition));
        Assert.NotNull(menu);
    }

    [Fact]
    public void Constructor_WithCustomZIndex_CreatesInstance()
    {
        var menu = new MultiColumnCompletionsMenu(zIndex: 500);
        Assert.NotNull(menu);
    }

    [Fact]
    public void Constructor_AllParameters_CreatesInstance()
    {
        var menu = new MultiColumnCompletionsMenu(
            minRows: 4,
            suggestedMaxColumnWidth: 25,
            showMeta: true,
            extraFilter: true,
            zIndex: 200_000);
        Assert.NotNull(menu);
    }

    #endregion

    #region Inheritance Tests

    [Fact]
    public void MultiColumnCompletionsMenu_InheritsHSplit()
    {
        var menu = new MultiColumnCompletionsMenu();
        Assert.IsAssignableFrom<HSplit>(menu);
    }

    [Fact]
    public void MultiColumnCompletionsMenu_IsIContainer()
    {
        var menu = new MultiColumnCompletionsMenu();
        Assert.IsAssignableFrom<IContainer>(menu);
    }

    #endregion

    #region Children Tests

    [Fact]
    public void Constructor_CreatesTwoChildren()
    {
        var menu = new MultiColumnCompletionsMenu();
        // HSplit should have two children: completions window and meta window
        var children = menu.GetChildren();
        Assert.Equal(2, children.Count);
    }

    [Fact]
    public void Constructor_ChildrenAreConditionalContainers()
    {
        var menu = new MultiColumnCompletionsMenu();
        var children = menu.GetChildren();
        foreach (var child in children)
        {
            Assert.IsType<ConditionalContainer>(child);
        }
    }

    #endregion
}
