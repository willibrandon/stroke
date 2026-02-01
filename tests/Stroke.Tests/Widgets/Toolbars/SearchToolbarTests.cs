using Stroke.Filters;
using Stroke.FormattedText;
using Stroke.Layout.Containers;
using Stroke.Layout.Controls;
using Stroke.Layout.Processors;
using Stroke.Lexers;
using Stroke.Widgets.Toolbars;
using Xunit;

// Alias to avoid ambiguity with System.Buffer
using Buffer = Stroke.Core.Buffer;

namespace Stroke.Tests.Widgets.Toolbars;

/// <summary>
/// Tests for SearchToolbar (incremental search toolbar with direction-aware prompts).
/// </summary>
public sealed class SearchToolbarTests
{
    #region Constructor Default Tests

    [Fact]
    public void Constructor_DefaultParams_CreatesNewBuffer()
    {
        var toolbar = new SearchToolbar();

        Assert.NotNull(toolbar.SearchBuffer);
    }

    [Fact]
    public void Constructor_DefaultParams_CreatesDistinctBufferPerInstance()
    {
        var toolbar1 = new SearchToolbar();
        var toolbar2 = new SearchToolbar();

        Assert.NotSame(toolbar1.SearchBuffer, toolbar2.SearchBuffer);
    }

    [Fact]
    public void Constructor_DefaultParams_CreatesInstance()
    {
        var toolbar = new SearchToolbar();

        Assert.NotNull(toolbar);
    }

    #endregion

    #region Constructor With Provided SearchBuffer Tests

    [Fact]
    public void Constructor_WithProvidedSearchBuffer_UsesThatBuffer()
    {
        var buffer = new Buffer();
        var toolbar = new SearchToolbar(searchBuffer: buffer);

        Assert.Same(buffer, toolbar.SearchBuffer);
    }

    [Fact]
    public void Constructor_WithProvidedSearchBuffer_DoesNotCreateNew()
    {
        var buffer = new Buffer();
        var toolbar = new SearchToolbar(searchBuffer: buffer);

        Assert.Same(buffer, toolbar.SearchBuffer);
        Assert.Same(buffer, toolbar.Control.Buffer);
    }

    [Fact]
    public void Constructor_WithNullSearchBuffer_CreatesNewBuffer()
    {
        var toolbar = new SearchToolbar(searchBuffer: null);

        Assert.NotNull(toolbar.SearchBuffer);
    }

    #endregion

    #region SearchBuffer Property Tests

    [Fact]
    public void SearchBuffer_ReturnsCorrectBuffer_WhenProvided()
    {
        var buffer = new Buffer();
        var toolbar = new SearchToolbar(searchBuffer: buffer);

        Assert.Same(buffer, toolbar.SearchBuffer);
    }

    [Fact]
    public void SearchBuffer_ReturnsCorrectBuffer_WhenDefault()
    {
        var toolbar = new SearchToolbar();

        // SearchBuffer should be the same instance used by the Control
        Assert.Same(toolbar.SearchBuffer, toolbar.Control.Buffer);
    }

    #endregion

    #region Control Tests

    [Fact]
    public void Control_IsSearchBufferControl()
    {
        var toolbar = new SearchToolbar();

        Assert.IsType<SearchBufferControl>(toolbar.Control);
    }

    [Fact]
    public void Control_IsNotNull()
    {
        var toolbar = new SearchToolbar();

        Assert.NotNull(toolbar.Control);
    }

    [Fact]
    public void Control_UsesSearchBuffer()
    {
        var buffer = new Buffer();
        var toolbar = new SearchToolbar(searchBuffer: buffer);

        Assert.Same(buffer, toolbar.Control.Buffer);
    }

    [Fact]
    public void Control_HasInputProcessors()
    {
        var toolbar = new SearchToolbar();

        Assert.NotNull(toolbar.Control.InputProcessors);
        Assert.NotEmpty(toolbar.Control.InputProcessors);
    }

    [Fact]
    public void Control_FirstInputProcessor_IsBeforeInput()
    {
        var toolbar = new SearchToolbar();

        var processors = toolbar.Control.InputProcessors;
        Assert.NotNull(processors);
        Assert.IsType<BeforeInput>(processors[0]);
    }

    [Fact]
    public void Control_BeforeInputProcessor_HasSearchToolbarPromptStyle()
    {
        var toolbar = new SearchToolbar();

        var processors = toolbar.Control.InputProcessors;
        Assert.NotNull(processors);
        var beforeInput = Assert.IsType<BeforeInput>(processors[0]);
        Assert.Equal("class:search-toolbar.prompt", beforeInput.Style);
    }

    [Fact]
    public void Control_HasExactlyOneInputProcessor()
    {
        var toolbar = new SearchToolbar();

        var processors = toolbar.Control.InputProcessors;
        Assert.NotNull(processors);
        Assert.Single(processors);
    }

    [Fact]
    public void Control_Lexer_IsSimpleLexer()
    {
        var toolbar = new SearchToolbar();

        Assert.IsType<SimpleLexer>(toolbar.Control.Lexer);
    }

    #endregion

    #region Container Tests

    [Fact]
    public void Container_IsConditionalContainer()
    {
        var toolbar = new SearchToolbar();

        Assert.IsType<ConditionalContainer>(toolbar.Container);
    }

    [Fact]
    public void Container_IsNotNull()
    {
        var toolbar = new SearchToolbar();

        Assert.NotNull(toolbar.Container);
    }

    [Fact]
    public void Container_HasFilter()
    {
        var toolbar = new SearchToolbar();

        Assert.NotNull(toolbar.Container.Filter);
    }

    [Fact]
    public void Container_Filter_IsCondition()
    {
        var toolbar = new SearchToolbar();

        // The filter wraps a Condition (isSearching)
        Assert.IsType<Condition>(toolbar.Container.Filter);
    }

    #endregion

    #region PtContainer Tests

    [Fact]
    public void PtContainer_ReturnsContainer()
    {
        var toolbar = new SearchToolbar();

        Assert.Same(toolbar.Container, toolbar.PtContainer());
    }

    [Fact]
    public void PtContainer_ReturnsIContainer()
    {
        var toolbar = new SearchToolbar();

        Assert.IsAssignableFrom<IContainer>(toolbar.PtContainer());
    }

    [Fact]
    public void PtContainer_ReturnsConditionalContainer()
    {
        var toolbar = new SearchToolbar();

        Assert.IsType<ConditionalContainer>(toolbar.PtContainer());
    }

    #endregion

    #region IMagicContainer Tests

    [Fact]
    public void SearchToolbar_ImplementsIMagicContainer()
    {
        var toolbar = new SearchToolbar();

        Assert.IsAssignableFrom<IMagicContainer>(toolbar);
    }

    [Fact]
    public void SearchToolbar_AsIMagicContainer_PtContainerReturnsContainer()
    {
        IMagicContainer toolbar = new SearchToolbar();

        Assert.IsAssignableFrom<IContainer>(toolbar.PtContainer());
    }

    #endregion

    #region IgnoreCase Forwarding Tests

    [Fact]
    public void IgnoreCase_Default_ForwardsToControl_AsNever()
    {
        var toolbar = new SearchToolbar();

        // Default FilterOrBool is empty, so SearchBufferControl sets IgnoreCase to Never
        Assert.IsType<Never>(toolbar.Control.IgnoreCase);
    }

    [Fact]
    public void IgnoreCase_True_ForwardsToControl_AsAlways()
    {
        var toolbar = new SearchToolbar(ignoreCase: new FilterOrBool(true));

        Assert.IsType<Always>(toolbar.Control.IgnoreCase);
    }

    [Fact]
    public void IgnoreCase_False_ForwardsToControl_AsNever()
    {
        var toolbar = new SearchToolbar(ignoreCase: new FilterOrBool(false));

        Assert.IsType<Never>(toolbar.Control.IgnoreCase);
    }

    [Fact]
    public void IgnoreCase_CustomFilter_ForwardsToControl()
    {
        var customFilter = new Condition(() => true);
        var toolbar = new SearchToolbar(ignoreCase: new FilterOrBool(customFilter));

        // Should not be Never since we provided an actual filter
        Assert.NotNull(toolbar.Control.IgnoreCase);
        Assert.IsNotType<Never>(toolbar.Control.IgnoreCase);
    }

    #endregion

    #region ViMode Parameter Tests

    [Fact]
    public void Constructor_ViModeDefault_CreatesInstance()
    {
        var toolbar = new SearchToolbar(viMode: false);

        Assert.NotNull(toolbar);
    }

    [Fact]
    public void Constructor_ViModeTrue_CreatesInstance()
    {
        var toolbar = new SearchToolbar(viMode: true);

        Assert.NotNull(toolbar);
    }

    #endregion

    #region Custom Prompt Parameter Tests

    [Fact]
    public void Constructor_WithCustomForwardSearchPrompt_CreatesInstance()
    {
        var toolbar = new SearchToolbar(forwardSearchPrompt: "Find: ");

        Assert.NotNull(toolbar);
        Assert.NotNull(toolbar.Control);
    }

    [Fact]
    public void Constructor_WithCustomBackwardSearchPrompt_CreatesInstance()
    {
        var toolbar = new SearchToolbar(backwardSearchPrompt: "Find backward: ");

        Assert.NotNull(toolbar);
        Assert.NotNull(toolbar.Control);
    }

    [Fact]
    public void Constructor_WithCustomTextIfNotSearching_CreatesInstance()
    {
        var toolbar = new SearchToolbar(textIfNotSearching: "Press / to search");

        Assert.NotNull(toolbar);
        Assert.NotNull(toolbar.Control);
    }

    [Fact]
    public void Constructor_WithAllCustomPrompts_CreatesInstance()
    {
        var toolbar = new SearchToolbar(
            viMode: true,
            textIfNotSearching: "idle",
            forwardSearchPrompt: "fwd: ",
            backwardSearchPrompt: "bwd: ");

        Assert.NotNull(toolbar);
        Assert.NotNull(toolbar.Control);
        Assert.NotNull(toolbar.Container);
    }

    #endregion

    #region Object Graph Consistency Tests

    [Fact]
    public void SearchBuffer_IsConsistentBetweenPropertyAndControl()
    {
        var toolbar = new SearchToolbar();

        Assert.Same(toolbar.SearchBuffer, toolbar.Control.Buffer);
    }

    [Fact]
    public void ProvidedBuffer_IsConsistentBetweenPropertyAndControl()
    {
        var buffer = new Buffer();
        var toolbar = new SearchToolbar(searchBuffer: buffer);

        Assert.Same(buffer, toolbar.SearchBuffer);
        Assert.Same(buffer, toolbar.Control.Buffer);
        Assert.Same(toolbar.SearchBuffer, toolbar.Control.Buffer);
    }

    [Fact]
    public void MultipleInstances_HaveIndependentObjectGraphs()
    {
        var toolbar1 = new SearchToolbar();
        var toolbar2 = new SearchToolbar();

        Assert.NotSame(toolbar1.SearchBuffer, toolbar2.SearchBuffer);
        Assert.NotSame(toolbar1.Control, toolbar2.Control);
        Assert.NotSame(toolbar1.Container, toolbar2.Container);
    }

    #endregion
}
