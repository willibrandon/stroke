using Stroke.Application;
using Stroke.Application.Bindings;
using Stroke.Input;
using Stroke.KeyBinding;
using Xunit;
using Keys = Stroke.Input.Keys;

namespace Stroke.Tests.Application.Bindings;

/// <summary>
/// Tests for the 3 binding loaders in <see cref="PageNavigationBindings"/>.
/// </summary>
public sealed class PageNavigationBindingsTests
{
    #region US5: LoadEmacsPageNavigationBindings Tests

    [Fact]
    public void LoadEmacs_ReturnsConditionalKeyBindings()
    {
        var result = PageNavigationBindings.LoadEmacsPageNavigationBindings();

        Assert.NotNull(result);
        Assert.IsType<ConditionalKeyBindings>(result);
    }

    [Fact]
    public void LoadEmacs_ContainsExactly4Bindings()
    {
        var result = PageNavigationBindings.LoadEmacsPageNavigationBindings();

        Assert.Equal(4, result.Bindings.Count);
    }

    [Fact]
    public void LoadEmacs_FilterIsEmacsMode()
    {
        var result = (ConditionalKeyBindings)PageNavigationBindings.LoadEmacsPageNavigationBindings();

        Assert.Same(EmacsFilters.EmacsMode, result.Filter);
    }

    [Fact]
    public void LoadEmacs_CtrlV_MapsToScrollPageDown()
    {
        var result = PageNavigationBindings.LoadEmacsPageNavigationBindings();

        var binding = result.Bindings.Single(b =>
            b.Keys.Count == 1 && b.Keys[0].IsKey && b.Keys[0].Key == Keys.ControlV);

        Assert.Equal(ScrollBindings.ScrollPageDown, binding.Handler);
    }

    [Fact]
    public void LoadEmacs_PageDown_MapsToScrollPageDown()
    {
        var result = PageNavigationBindings.LoadEmacsPageNavigationBindings();

        var binding = result.Bindings.Single(b =>
            b.Keys.Count == 1 && b.Keys[0].IsKey && b.Keys[0].Key == Keys.PageDown);

        Assert.Equal(ScrollBindings.ScrollPageDown, binding.Handler);
    }

    [Fact]
    public void LoadEmacs_EscapeV_MapsToScrollPageUp()
    {
        var result = PageNavigationBindings.LoadEmacsPageNavigationBindings();

        // Escape+V is a two-key sequence
        var binding = result.Bindings.Single(b =>
            b.Keys.Count == 2
            && b.Keys[0].IsKey && b.Keys[0].Key == Keys.Escape
            && b.Keys[1].IsChar && b.Keys[1].Char == 'v');

        Assert.Equal(ScrollBindings.ScrollPageUp, binding.Handler);
    }

    [Fact]
    public void LoadEmacs_PageUp_MapsToScrollPageUp()
    {
        var result = PageNavigationBindings.LoadEmacsPageNavigationBindings();

        var binding = result.Bindings.Single(b =>
            b.Keys.Count == 1 && b.Keys[0].IsKey && b.Keys[0].Key == Keys.PageUp);

        Assert.Equal(ScrollBindings.ScrollPageUp, binding.Handler);
    }

    #endregion

    #region US6: LoadViPageNavigationBindings Tests

    [Fact]
    public void LoadVi_ReturnsConditionalKeyBindings()
    {
        var result = PageNavigationBindings.LoadViPageNavigationBindings();

        Assert.NotNull(result);
        Assert.IsType<ConditionalKeyBindings>(result);
    }

    [Fact]
    public void LoadVi_ContainsExactly8Bindings()
    {
        var result = PageNavigationBindings.LoadViPageNavigationBindings();

        Assert.Equal(8, result.Bindings.Count);
    }

    [Fact]
    public void LoadVi_FilterIsViMode()
    {
        var result = (ConditionalKeyBindings)PageNavigationBindings.LoadViPageNavigationBindings();

        Assert.Same(ViFilters.ViMode, result.Filter);
    }

    [Fact]
    public void LoadVi_CtrlF_MapsToScrollForward()
    {
        var result = PageNavigationBindings.LoadViPageNavigationBindings();

        var binding = result.Bindings.Single(b =>
            b.Keys.Count == 1 && b.Keys[0].IsKey && b.Keys[0].Key == Keys.ControlF);

        Assert.Equal(ScrollBindings.ScrollForward, binding.Handler);
    }

    [Fact]
    public void LoadVi_CtrlB_MapsToScrollBackward()
    {
        var result = PageNavigationBindings.LoadViPageNavigationBindings();

        var binding = result.Bindings.Single(b =>
            b.Keys.Count == 1 && b.Keys[0].IsKey && b.Keys[0].Key == Keys.ControlB);

        Assert.Equal(ScrollBindings.ScrollBackward, binding.Handler);
    }

    [Fact]
    public void LoadVi_CtrlD_MapsToScrollHalfPageDown()
    {
        var result = PageNavigationBindings.LoadViPageNavigationBindings();

        var binding = result.Bindings.Single(b =>
            b.Keys.Count == 1 && b.Keys[0].IsKey && b.Keys[0].Key == Keys.ControlD);

        Assert.Equal(ScrollBindings.ScrollHalfPageDown, binding.Handler);
    }

    [Fact]
    public void LoadVi_CtrlU_MapsToScrollHalfPageUp()
    {
        var result = PageNavigationBindings.LoadViPageNavigationBindings();

        var binding = result.Bindings.Single(b =>
            b.Keys.Count == 1 && b.Keys[0].IsKey && b.Keys[0].Key == Keys.ControlU);

        Assert.Equal(ScrollBindings.ScrollHalfPageUp, binding.Handler);
    }

    [Fact]
    public void LoadVi_CtrlE_MapsToScrollOneLineDown()
    {
        var result = PageNavigationBindings.LoadViPageNavigationBindings();

        var binding = result.Bindings.Single(b =>
            b.Keys.Count == 1 && b.Keys[0].IsKey && b.Keys[0].Key == Keys.ControlE);

        Assert.Equal(ScrollBindings.ScrollOneLineDown, binding.Handler);
    }

    [Fact]
    public void LoadVi_CtrlY_MapsToScrollOneLineUp()
    {
        var result = PageNavigationBindings.LoadViPageNavigationBindings();

        var binding = result.Bindings.Single(b =>
            b.Keys.Count == 1 && b.Keys[0].IsKey && b.Keys[0].Key == Keys.ControlY);

        Assert.Equal(ScrollBindings.ScrollOneLineUp, binding.Handler);
    }

    [Fact]
    public void LoadVi_PageDown_MapsToScrollPageDown()
    {
        var result = PageNavigationBindings.LoadViPageNavigationBindings();

        var binding = result.Bindings.Single(b =>
            b.Keys.Count == 1 && b.Keys[0].IsKey && b.Keys[0].Key == Keys.PageDown);

        Assert.Equal(ScrollBindings.ScrollPageDown, binding.Handler);
    }

    [Fact]
    public void LoadVi_PageUp_MapsToScrollPageUp()
    {
        var result = PageNavigationBindings.LoadViPageNavigationBindings();

        var binding = result.Bindings.Single(b =>
            b.Keys.Count == 1 && b.Keys[0].IsKey && b.Keys[0].Key == Keys.PageUp);

        Assert.Equal(ScrollBindings.ScrollPageUp, binding.Handler);
    }

    #endregion

    #region US7: LoadPageNavigationBindings Tests

    [Fact]
    public void LoadCombined_ReturnsConditionalKeyBindings()
    {
        var result = PageNavigationBindings.LoadPageNavigationBindings();

        Assert.NotNull(result);
        Assert.IsType<ConditionalKeyBindings>(result);
    }

    [Fact]
    public void LoadCombined_ContainsAll12Bindings()
    {
        var result = PageNavigationBindings.LoadPageNavigationBindings();

        // 4 Emacs + 8 Vi = 12 total
        Assert.Equal(12, result.Bindings.Count);
    }

    [Fact]
    public void LoadCombined_TopLevelFilterIsBufferHasFocus()
    {
        var result = (ConditionalKeyBindings)PageNavigationBindings.LoadPageNavigationBindings();

        Assert.Same(AppFilters.BufferHasFocus, result.Filter);
    }

    #endregion
}
