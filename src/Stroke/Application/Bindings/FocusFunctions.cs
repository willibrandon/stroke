using Stroke.KeyBinding;
using Stroke.KeyBinding.Bindings;

namespace Stroke.Application.Bindings;

/// <summary>
/// Focus navigation handler functions for moving between visible focusable windows.
/// </summary>
/// <remarks>
/// <para>
/// Port of Python Prompt Toolkit's <c>prompt_toolkit.key_binding.bindings.focus</c> module.
/// Provides two handler functions: <see cref="FocusNext"/> (often bound to Tab) and
/// <see cref="FocusPrevious"/> (often bound to BackTab/Shift+Tab) that cycle focus
/// between visible focusable windows in the layout.
/// </para>
/// <para>
/// This type is stateless and inherently thread-safe.
/// </para>
/// </remarks>
public static class FocusFunctions
{
    /// <summary>
    /// Focus the next visible window. Often bound to the Tab key.
    /// </summary>
    /// <param name="event">The key press event providing access to the application context.</param>
    /// <returns><see langword="null"/> always.</returns>
    public static NotImplementedOrNone? FocusNext(KeyPressEvent @event)
    {
        @event.GetApp().Layout.FocusNext();
        return null;
    }

    /// <summary>
    /// Focus the previous visible window. Often bound to the BackTab (Shift+Tab) key.
    /// </summary>
    /// <param name="event">The key press event providing access to the application context.</param>
    /// <returns><see langword="null"/> always.</returns>
    public static NotImplementedOrNone? FocusPrevious(KeyPressEvent @event)
    {
        @event.GetApp().Layout.FocusPrevious();
        return null;
    }
}
