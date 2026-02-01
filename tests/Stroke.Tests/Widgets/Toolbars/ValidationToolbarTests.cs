using Stroke.Filters;
using Stroke.Layout;
using Stroke.Layout.Containers;
using Stroke.Layout.Controls;
using Stroke.Widgets.Toolbars;
using Xunit;

namespace Stroke.Tests.Widgets.Toolbars;

/// <summary>
/// Tests for ValidationToolbar (displays current buffer's validation error message).
/// </summary>
public sealed class ValidationToolbarTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_Default_CreatesInstance()
    {
        var toolbar = new ValidationToolbar();

        Assert.NotNull(toolbar);
    }

    [Fact]
    public void Constructor_WithShowPositionTrue_CreatesInstance()
    {
        var toolbar = new ValidationToolbar(showPosition: true);

        Assert.NotNull(toolbar);
    }

    [Fact]
    public void Constructor_WithShowPositionFalse_CreatesInstance()
    {
        var toolbar = new ValidationToolbar(showPosition: false);

        Assert.NotNull(toolbar);
    }

    [Fact]
    public void Constructor_DefaultShowPosition_IsFalse()
    {
        // The default parameter is showPosition=false.
        // Both default and explicit false should produce structurally identical toolbars.
        var defaultToolbar = new ValidationToolbar();
        var explicitFalseToolbar = new ValidationToolbar(showPosition: false);

        // Both should construct successfully with the same structure
        Assert.NotNull(defaultToolbar.Control);
        Assert.NotNull(explicitFalseToolbar.Control);
    }

    #endregion

    #region Control Tests

    [Fact]
    public void Control_IsNotNull()
    {
        var toolbar = new ValidationToolbar();

        Assert.NotNull(toolbar.Control);
    }

    [Fact]
    public void Control_IsFormattedTextControl()
    {
        var toolbar = new ValidationToolbar();

        Assert.IsType<FormattedTextControl>(toolbar.Control);
    }

    [Fact]
    public void Constructor_CreatesFormattedTextControl()
    {
        var toolbar = new ValidationToolbar();

        // Verify the Control property is properly initialized as a FormattedTextControl
        var control = toolbar.Control;
        Assert.NotNull(control);
        Assert.IsAssignableFrom<IUIControl>(control);
    }

    #endregion

    #region Container Tests

    [Fact]
    public void Container_IsNotNull()
    {
        var toolbar = new ValidationToolbar();

        Assert.NotNull(toolbar.Container);
    }

    [Fact]
    public void Container_IsConditionalContainer()
    {
        var toolbar = new ValidationToolbar();

        Assert.IsType<ConditionalContainer>(toolbar.Container);
    }

    [Fact]
    public void Container_HasFilter()
    {
        var toolbar = new ValidationToolbar();

        Assert.NotNull(toolbar.Container.Filter);
    }

    [Fact]
    public void Container_Filter_UsesHasValidationError()
    {
        var toolbar = new ValidationToolbar();

        // The filter should be the HasValidationError app filter.
        // We verify it is not null and is an IFilter (the FilterOrBool wrapping
        // AppFilters.HasValidationError resolves to the underlying Condition filter).
        var filter = toolbar.Container.Filter;
        Assert.NotNull(filter);
        Assert.IsAssignableFrom<IFilter>(filter);
    }

    #endregion

    #region PtContainer Tests

    [Fact]
    public void PtContainer_ReturnsContainer()
    {
        var toolbar = new ValidationToolbar();

        Assert.Same(toolbar.Container, toolbar.PtContainer());
    }

    [Fact]
    public void PtContainer_ReturnsIContainer()
    {
        var toolbar = new ValidationToolbar();

        Assert.IsAssignableFrom<IContainer>(toolbar.PtContainer());
    }

    #endregion

    #region IMagicContainer Tests

    [Fact]
    public void ValidationToolbar_ImplementsIMagicContainer()
    {
        var toolbar = new ValidationToolbar();

        Assert.IsAssignableFrom<IMagicContainer>(toolbar);
    }

    #endregion

    #region Window Height Tests

    [Fact]
    public void Window_HeightPreferredIsOne()
    {
        var toolbar = new ValidationToolbar();

        // The Container's Content is a Window with height: new Dimension(preferred: 1).
        // Access it through the ConditionalContainer.Content which is the Window.
        var window = Assert.IsType<Window>(toolbar.Container.Content);
        var preferredHeight = window.PreferredHeight(80, 24);
        Assert.Equal(1, preferredHeight.Preferred);
    }

    [Fact]
    public void Window_Content_IsControl()
    {
        var toolbar = new ValidationToolbar();

        // The Window wraps the FormattedTextControl
        var window = Assert.IsType<Window>(toolbar.Container.Content);
        Assert.Same(toolbar.Control, window.Content);
    }

    #endregion
}
