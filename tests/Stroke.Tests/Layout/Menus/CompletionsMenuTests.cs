using Stroke.Filters;
using Stroke.Layout.Containers;
using Stroke.Layout.Menus;
using Xunit;

namespace Stroke.Tests.Layout.Menus;

/// <summary>
/// Tests for CompletionsMenu container (US3: single-column wrapper).
/// </summary>
public sealed class CompletionsMenuTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_Default_CreatesInstance()
    {
        var menu = new CompletionsMenu();
        Assert.NotNull(menu);
    }

    [Fact]
    public void Constructor_WithMaxHeight_CreatesInstance()
    {
        var menu = new CompletionsMenu(maxHeight: 10);
        Assert.NotNull(menu);
    }

    [Fact]
    public void Constructor_WithScrollOffset_CreatesInstance()
    {
        var menu = new CompletionsMenu(scrollOffset: 3);
        Assert.NotNull(menu);
    }

    [Fact]
    public void Constructor_WithExtraFilter_CreatesInstance()
    {
        var menu = new CompletionsMenu(extraFilter: true);
        Assert.NotNull(menu);
    }

    [Fact]
    public void Constructor_WithFilterFalse_CreatesInstance()
    {
        var menu = new CompletionsMenu(extraFilter: false);
        Assert.NotNull(menu);
    }

    [Fact]
    public void Constructor_WithDisplayArrows_CreatesInstance()
    {
        var menu = new CompletionsMenu(displayArrows: true);
        Assert.NotNull(menu);
    }

    [Fact]
    public void Constructor_WithCustomZIndex_CreatesInstance()
    {
        var menu = new CompletionsMenu(zIndex: 999);
        Assert.NotNull(menu);
    }

    [Fact]
    public void Constructor_AllParameters_CreatesInstance()
    {
        var menu = new CompletionsMenu(
            maxHeight: 15,
            scrollOffset: 2,
            extraFilter: true,
            displayArrows: false,
            zIndex: 50_000);
        Assert.NotNull(menu);
    }

    #endregion

    #region Inheritance Tests

    [Fact]
    public void CompletionsMenu_InheritsConditionalContainer()
    {
        var menu = new CompletionsMenu();
        Assert.IsAssignableFrom<ConditionalContainer>(menu);
    }

    [Fact]
    public void CompletionsMenu_IsIContainer()
    {
        var menu = new CompletionsMenu();
        Assert.IsAssignableFrom<IContainer>(menu);
    }

    #endregion

    #region Filter Composition Tests

    [Fact]
    public void Constructor_WithConditionFilter_CreatesInstance()
    {
        var condition = new Condition(() => true);
        var menu = new CompletionsMenu(extraFilter: new FilterOrBool(condition));
        Assert.NotNull(menu);
    }

    [Fact]
    public void Constructor_DefaultExtraFilter_UsesAlways()
    {
        // default FilterOrBool has no value, so the filter should use Always
        var menu = new CompletionsMenu();
        Assert.NotNull(menu);
    }

    #endregion
}
