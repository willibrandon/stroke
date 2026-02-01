using Stroke.Layout.Containers;
using Stroke.Layout.Controls;
using Stroke.Widgets.Toolbars;
using Xunit;

namespace Stroke.Tests.Widgets.Toolbars;

/// <summary>
/// Tests for ArgToolbar (displays current numeric prefix argument, e.g., "Repeat: 5").
/// </summary>
public sealed class ArgToolbarTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_CreatesInstance()
    {
        var toolbar = new ArgToolbar();

        Assert.NotNull(toolbar);
    }

    #endregion

    #region Window Tests

    [Fact]
    public void Window_IsNotNull()
    {
        var toolbar = new ArgToolbar();

        Assert.NotNull(toolbar.Window);
    }

    [Fact]
    public void Window_HeightPreferredIsOne()
    {
        var toolbar = new ArgToolbar();

        var preferredHeight = toolbar.Window.PreferredHeight(80, 24);
        Assert.Equal(1, preferredHeight.Preferred);
    }

    [Fact]
    public void Window_Content_IsNotNull()
    {
        var toolbar = new ArgToolbar();

        Assert.NotNull(toolbar.Window.Content);
    }

    [Fact]
    public void Window_Content_IsFormattedTextControl()
    {
        var toolbar = new ArgToolbar();

        Assert.IsType<FormattedTextControl>(toolbar.Window.Content);
    }

    #endregion

    #region Container Tests

    [Fact]
    public void Container_IsNotNull()
    {
        var toolbar = new ArgToolbar();

        Assert.NotNull(toolbar.Container);
    }

    [Fact]
    public void Container_IsConditionalContainer()
    {
        var toolbar = new ArgToolbar();

        Assert.IsType<ConditionalContainer>(toolbar.Container);
    }

    [Fact]
    public void Container_HasFilter()
    {
        var toolbar = new ArgToolbar();

        Assert.NotNull(toolbar.Container.Filter);
    }

    #endregion

    #region PtContainer Tests

    [Fact]
    public void PtContainer_ReturnsContainer()
    {
        var toolbar = new ArgToolbar();

        Assert.Same(toolbar.Container, toolbar.PtContainer());
    }

    [Fact]
    public void PtContainer_ReturnsIContainer()
    {
        var toolbar = new ArgToolbar();

        Assert.IsAssignableFrom<IContainer>(toolbar.PtContainer());
    }

    #endregion

    #region IMagicContainer Tests

    [Fact]
    public void ArgToolbar_ImplementsIMagicContainer()
    {
        var toolbar = new ArgToolbar();

        Assert.IsAssignableFrom<IMagicContainer>(toolbar);
    }

    #endregion
}
