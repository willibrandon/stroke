using Stroke.KeyBinding;

namespace Stroke.Application;

/// <summary>
/// Provides the default key bindings for Stroke applications.
/// </summary>
/// <remarks>
/// <para>
/// Port of Python Prompt Toolkit's <c>load_key_bindings</c> and
/// <c>load_page_navigation_bindings</c> from <c>prompt_toolkit.key_binding.defaults</c>.
/// </para>
/// <para>
/// Both methods currently return empty merged bindings as stubs. The actual
/// editing mode bindings (Emacs, Vi, mouse, CPR) are separate features that
/// will be implemented later. The infrastructure to load and merge them is
/// established here.
/// </para>
/// </remarks>
public static class DefaultKeyBindings
{
    /// <summary>
    /// Load the default key bindings that merge basic, Emacs, Vi, mouse, and CPR bindings.
    /// The editing-mode-specific bindings are conditional on buffer_has_focus.
    /// </summary>
    /// <returns>Merged default key bindings (currently empty stubs).</returns>
    public static IKeyBindingsBase Load()
    {
        // Return empty merged bindings — actual editing mode bindings are separate features.
        return new MergedKeyBindings(Array.Empty<IKeyBindingsBase>());
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
