using Stroke.Application;
using Stroke.Input;

namespace Stroke.KeyBinding.Bindings;

/// <summary>
/// Key binding loaders for page navigation in Vi and Emacs editing modes.
/// </summary>
/// <remarks>
/// <para>
/// Port of Python Prompt Toolkit's <c>prompt_toolkit.key_binding.bindings.page_navigation</c> module.
/// Provides 3 binding loaders: Emacs, Vi, and combined (merged with buffer-has-focus guard).
/// </para>
/// <para>
/// This type is stateless and inherently thread-safe. Each loader creates and returns a new
/// <see cref="IKeyBindingsBase"/> instance.
/// </para>
/// </remarks>
public static class PageNavigationBindings
{
    /// <summary>
    /// Load both the Vi and Emacs bindings for page navigation, guarded by a
    /// buffer-has-focus condition.
    /// </summary>
    /// <returns>
    /// A <see cref="ConditionalKeyBindings"/> wrapping the merged Emacs and Vi bindings,
    /// filtered by <see cref="AppFilters.BufferHasFocus"/>.
    /// </returns>
    public static IKeyBindingsBase LoadPageNavigationBindings()
    {
        // Only enable when a Buffer is focused, otherwise, we would catch keys
        // when another widget is focused (like for instance c-d in a terminal).
        return new ConditionalKeyBindings(
            new MergedKeyBindings(
                LoadEmacsPageNavigationBindings(),
                LoadViPageNavigationBindings()
            ),
            AppFilters.BufferHasFocus
        );
    }

    /// <summary>
    /// Key bindings for scrolling up and down through pages in Emacs mode.
    /// These are separate bindings because GNU readline doesn't have them.
    /// </summary>
    /// <returns>
    /// A <see cref="ConditionalKeyBindings"/> with Emacs page navigation keys,
    /// filtered by <see cref="EmacsFilters.EmacsMode"/>.
    /// </returns>
    public static IKeyBindingsBase LoadEmacsPageNavigationBindings()
    {
        var kb = new KeyBindings();

        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.ControlV)])(ScrollBindings.ScrollPageDown);
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.PageDown)])(ScrollBindings.ScrollPageDown);
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.Escape), new KeyOrChar('v')])(ScrollBindings.ScrollPageUp);
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.PageUp)])(ScrollBindings.ScrollPageUp);

        return new ConditionalKeyBindings(kb, EmacsFilters.EmacsMode);
    }

    /// <summary>
    /// Key bindings for scrolling up and down through pages in Vi mode.
    /// These are separate bindings because GNU readline doesn't have them.
    /// </summary>
    /// <returns>
    /// A <see cref="ConditionalKeyBindings"/> with Vi page navigation keys,
    /// filtered by <see cref="ViFilters.ViMode"/>.
    /// </returns>
    public static IKeyBindingsBase LoadViPageNavigationBindings()
    {
        var kb = new KeyBindings();

        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.ControlF)])(ScrollBindings.ScrollForward);
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.ControlB)])(ScrollBindings.ScrollBackward);
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.ControlD)])(ScrollBindings.ScrollHalfPageDown);
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.ControlU)])(ScrollBindings.ScrollHalfPageUp);
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.ControlE)])(ScrollBindings.ScrollOneLineDown);
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.ControlY)])(ScrollBindings.ScrollOneLineUp);
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.PageDown)])(ScrollBindings.ScrollPageDown);
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.PageUp)])(ScrollBindings.ScrollPageUp);

        return new ConditionalKeyBindings(kb, ViFilters.ViMode);
    }
}
