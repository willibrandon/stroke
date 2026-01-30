using Stroke.Core;
using Stroke.Filters;
using Stroke.Layout.Controls;
using Xunit;

using Buffer = Stroke.Core.Buffer;

namespace Stroke.Tests.Layout.Controls;

/// <summary>
/// Tests for SearchBufferControl.
/// </summary>
public sealed class SearchBufferControlTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_Default_CreatesControl()
    {
        var control = new SearchBufferControl();

        Assert.NotNull(control);
    }

    [Fact]
    public void Constructor_Default_NotFocusable()
    {
        var control = new SearchBufferControl();

        Assert.False(control.IsFocusable);
    }

    [Fact]
    public void Constructor_WithFocusable_SetsFocusable()
    {
        var control = new SearchBufferControl(focusable: new FilterOrBool(true));

        Assert.True(control.IsFocusable);
    }

    [Fact]
    public void Constructor_WithBuffer_UsesBuffer()
    {
        var buffer = new Buffer();
        var control = new SearchBufferControl(buffer: buffer);

        Assert.NotNull(control);
    }

    [Fact]
    public void Constructor_WithSearcherSearchState_StoresState()
    {
        var searchState = new SearchState();
        var control = new SearchBufferControl(searcherSearchState: searchState);

        Assert.Same(searchState, control.SearcherSearchState);
    }

    [Fact]
    public void Constructor_WithoutSearcherSearchState_IsNull()
    {
        var control = new SearchBufferControl();

        Assert.Null(control.SearcherSearchState);
    }

    #endregion

    #region IgnoreCase Tests

    [Fact]
    public void IgnoreCase_Default_IsFalse()
    {
        var control = new SearchBufferControl();

        Assert.False(control.IgnoreCase.Invoke());
    }

    [Fact]
    public void IgnoreCase_WhenTrue_ReturnsTrue()
    {
        var control = new SearchBufferControl(ignoreCase: new FilterOrBool(true));

        Assert.True(control.IgnoreCase.Invoke());
    }

    [Fact]
    public void IgnoreCase_DynamicFilter_ReflectsState()
    {
        var isIgnoreCase = false;
        var control = new SearchBufferControl(
            ignoreCase: new FilterOrBool(new Condition(() => isIgnoreCase)));

        Assert.False(control.IgnoreCase.Invoke());

        isIgnoreCase = true;
        Assert.True(control.IgnoreCase.Invoke());
    }

    #endregion

    #region CreateContent Tests

    [Fact]
    public void CreateContent_ReturnsContent()
    {
        var control = new SearchBufferControl();

        var content = control.CreateContent(80, 24);

        Assert.NotNull(content);
        Assert.True(content.LineCount >= 1);
    }

    #endregion

    #region ToString Tests

    [Fact]
    public void ToString_ReturnsDescriptiveString()
    {
        var control = new SearchBufferControl();

        var result = control.ToString();

        Assert.Contains("SearchBufferControl", result);
    }

    #endregion
}
