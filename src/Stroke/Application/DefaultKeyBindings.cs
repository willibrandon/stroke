using Stroke.Application.Bindings;
using Stroke.Filters;
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
    /// The structure follows Python Prompt Toolkit's <c>load_key_bindings()</c>:
    /// </para>
    /// <list type="bullet">
    /// <item>Basic/Emacs/Vi bindings are wrapped in <see cref="ConditionalKeyBindings"/>
    /// with <see cref="AppFilters.BufferHasFocus"/> so they only apply when a buffer is focused.</item>
    /// <item>Mouse and CPR bindings are unconditional — they must work even when
    /// no buffer has focus (e.g., dialog boxes, menu bars).</item>
    /// </list>
    /// </remarks>
    public static IKeyBindingsBase Load()
    {
        // Buffer-focused bindings (only active when a buffer control has focus)
        var bufferBindings = new MergedKeyBindings(
            BasicBindings.LoadBasicBindings(),
            EmacsBindings.LoadEmacsBindings(),
            ViBindings.LoadViBindings()
        );

        return new MergedKeyBindings(
            // Make sure that the above key bindings are only active if the
            // currently focused control is a BufferControl. For other controls, we
            // don't want these key bindings to intervene.
            new ConditionalKeyBindings(bufferBindings, AppFilters.BufferHasFocus),
            // Active even when no buffer has been focused.
            MouseBindings.LoadMouseBindings(),
            CprBindings.LoadCprBindings()
        );
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
