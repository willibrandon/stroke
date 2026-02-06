using Stroke.Application.Bindings;
using Stroke.KeyBinding;
using Stroke.KeyBinding.Bindings;

namespace Stroke.Application;

/// <summary>
/// Provides the default key bindings for Stroke applications.
/// </summary>
/// <remarks>
/// <para>
/// Port of Python Prompt Toolkit's <c>load_key_bindings</c> and
/// <c>load_page_navigation_bindings</c> from <c>prompt_toolkit.key_binding.defaults</c>.
/// </para>
/// </remarks>
public static class DefaultKeyBindings
{
    /// <summary>
    /// Load the default key bindings that merge basic, Emacs, Vi, mouse, and CPR bindings.
    /// The editing-mode-specific bindings are conditional on buffer_has_focus.
    /// </summary>
    /// <returns>Merged default key bindings.</returns>
    /// <remarks>
    /// <para>
    /// Editing bindings (basic, Emacs, Vi) are wrapped with <see cref="ConditionalKeyBindings"/>
    /// using <see cref="AppFilters.BufferHasFocus"/>. This ensures these bindings only activate
    /// when a <c>BufferControl</c> has focus, allowing other controls (like custom terminal
    /// emulators) to handle <c>Keys.Any</c> themselves.
    /// </para>
    /// <para>
    /// Mouse and CPR bindings are always active, even when no buffer has focus.
    /// </para>
    /// </remarks>
    public static IKeyBindingsBase Load()
    {
        // First, merge all editing bindings (basic, Emacs, Vi)
        var allEditingBindings = new MergedKeyBindings(
            BasicBindings.LoadBasicBindings(),
            EmacsBindings.LoadEmacsBindings(),
            SearchBindings.LoadEmacsSearchBindings(),
            EmacsBindings.LoadEmacsShiftSelectionBindings(),
            ViBindings.LoadViBindings(),
            SearchBindings.LoadViSearchBindings());

        // Wrap editing bindings with buffer_has_focus condition.
        // This ensures editing bindings only activate when a BufferControl has focus,
        // allowing other controls to handle Keys.Any themselves (e.g., ptterm).
        var conditionalEditingBindings = new ConditionalKeyBindings(
            allEditingBindings,
            AppFilters.BufferHasFocus);

        // Merge with mouse and CPR bindings (always active, not conditional on buffer focus)
        return new MergedKeyBindings(
            conditionalEditingBindings,
            MouseBindings.LoadMouseBindings(),
            CprBindings.LoadCprBindings());
    }

    /// <summary>
    /// Load page navigation bindings (Emacs and Vi page up/down, scroll).
    /// Conditional on buffer_has_focus.
    /// </summary>
    /// <returns>Page navigation key bindings.</returns>
    public static IKeyBindingsBase LoadPageNavigation()
    {
        return PageNavigationBindings.LoadPageNavigationBindings();
    }
}
