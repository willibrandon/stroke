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
    public static IKeyBindingsBase Load()
    {
        // Load basic bindings which include self-insert, navigation, etc.
        // Mouse and CPR bindings are always active (not conditional on buffer focus),
        // matching Python Prompt Toolkit's load_key_bindings().
        return new MergedKeyBindings(
            BasicBindings.LoadBasicBindings(),
            MouseBindings.LoadMouseBindings(),
            CprBindings.LoadCprBindings());
    }

    /// <summary>
    /// Load page navigation bindings (Emacs and Vi page up/down, scroll).
    /// Conditional on buffer_has_focus.
    /// </summary>
    /// <returns>Page navigation key bindings (currently empty stub).</returns>
    public static IKeyBindingsBase LoadPageNavigation()
    {
        // Return empty merged bindings — actual page navigation bindings are a separate feature.
        return new MergedKeyBindings(Array.Empty<IKeyBindingsBase>());
    }
}
