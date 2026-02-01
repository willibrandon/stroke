using Stroke.Application;
using Stroke.Filters;
using Stroke.Layout.Containers;
using Stroke.Widgets.Toolbars;
using Xunit;

namespace Stroke.Tests.Widgets.Toolbars;

/// <summary>
/// Tests for CompletionsToolbar (horizontal completion list toolbar with pagination arrows).
/// </summary>
public sealed class CompletionsToolbarTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_CreatesInstance()
    {
        var toolbar = new CompletionsToolbar();

        Assert.NotNull(toolbar);
    }

    #endregion

    #region Container Tests

    [Fact]
    public void Container_IsNotNull()
    {
        var toolbar = new CompletionsToolbar();

        Assert.NotNull(toolbar.Container);
    }

    [Fact]
    public void Container_IsConditionalContainer()
    {
        var toolbar = new CompletionsToolbar();

        Assert.IsType<ConditionalContainer>(toolbar.Container);
    }

    [Fact]
    public void Container_HasFilter()
    {
        var toolbar = new CompletionsToolbar();

        Assert.NotNull(toolbar.Container.Filter);
    }

    [Fact]
    public void Container_Filter_IsHasCompletions()
    {
        var toolbar = new CompletionsToolbar();

        Assert.Same(AppFilters.HasCompletions, toolbar.Container.Filter);
    }

    [Fact]
    public void Container_Filter_IsCondition()
    {
        var toolbar = new CompletionsToolbar();

        Assert.IsType<Condition>(toolbar.Container.Filter);
    }

    #endregion

    #region PtContainer Tests

    [Fact]
    public void PtContainer_ReturnsContainer()
    {
        var toolbar = new CompletionsToolbar();

        Assert.Same(toolbar.Container, toolbar.PtContainer());
    }

    [Fact]
    public void PtContainer_ReturnsIContainer()
    {
        var toolbar = new CompletionsToolbar();

        Assert.IsAssignableFrom<IContainer>(toolbar.PtContainer());
    }

    [Fact]
    public void PtContainer_ReturnsConditionalContainer()
    {
        var toolbar = new CompletionsToolbar();

        Assert.IsType<ConditionalContainer>(toolbar.PtContainer());
    }

    #endregion

    #region IMagicContainer Tests

    [Fact]
    public void CompletionsToolbar_ImplementsIMagicContainer()
    {
        var toolbar = new CompletionsToolbar();

        Assert.IsAssignableFrom<IMagicContainer>(toolbar);
    }

    #endregion

    #region Multiple Instance Tests

    [Fact]
    public void MultipleInstances_HaveDistinctContainers()
    {
        var toolbar1 = new CompletionsToolbar();
        var toolbar2 = new CompletionsToolbar();

        Assert.NotSame(toolbar1.Container, toolbar2.Container);
    }

    [Fact]
    public void MultipleInstances_ShareSameFilter()
    {
        var toolbar1 = new CompletionsToolbar();
        var toolbar2 = new CompletionsToolbar();

        // Both should reference the same static AppFilters.HasCompletions filter
        Assert.Same(toolbar1.Container.Filter, toolbar2.Container.Filter);
    }

    #endregion
}
