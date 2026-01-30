using Stroke.Core;
using Stroke.Input;
using Stroke.KeyBinding;
using Stroke.Layout.Windows;

namespace Stroke.Layout.Controls;

/// <summary>
/// Base interface for UI controls that can be rendered in a Window.
/// </summary>
/// <remarks>
/// <para>
/// UI controls are the leaf nodes in the layout tree that produce actual content.
/// Each control creates <see cref="UIContent"/> containing the styled text fragments
/// to display.
/// </para>
/// <para>
/// Port of Python Prompt Toolkit's <c>UIControl</c> abstract class from <c>layout/controls.py</c>.
/// </para>
/// </remarks>
public interface IUIControl
{
    /// <summary>
    /// Create the content to be rendered.
    /// </summary>
    /// <param name="width">The available width in columns.</param>
    /// <param name="height">The available height in rows.</param>
    /// <returns>The UI content for this frame.</returns>
    UIContent CreateContent(int width, int height);

    /// <summary>
    /// Reset control state.
    /// </summary>
    /// <remarks>
    /// Called at the start of a new render pass. Override to clear caches.
    /// Default implementation does nothing.
    /// </remarks>
    void Reset() { }

    /// <summary>
    /// Return preferred width, or null if any width is acceptable.
    /// </summary>
    /// <param name="maxAvailableWidth">Maximum width available from parent.</param>
    /// <returns>Preferred width or null.</returns>
    int? PreferredWidth(int maxAvailableWidth) => null;

    /// <summary>
    /// Return preferred height, or null if any height is acceptable.
    /// </summary>
    /// <param name="width">The actual width that will be used.</param>
    /// <param name="maxAvailableHeight">Maximum height available from parent.</param>
    /// <param name="wrapLines">Whether line wrapping is enabled.</param>
    /// <param name="getLinePrefix">Optional callback for line prefixes.</param>
    /// <returns>Preferred height or null.</returns>
    int? PreferredHeight(
        int width,
        int maxAvailableHeight,
        bool wrapLines,
        GetLinePrefixCallable? getLinePrefix) => null;

    /// <summary>
    /// Gets whether this control can receive focus.
    /// </summary>
    bool IsFocusable { get; }

    /// <summary>
    /// Handle mouse events.
    /// </summary>
    /// <param name="mouseEvent">The mouse event.</param>
    /// <returns>
    /// <see cref="NotImplementedOrNone.NotImplemented"/> if not handled,
    /// <see cref="NotImplementedOrNone.None"/> if handled.
    /// </returns>
    NotImplementedOrNone MouseHandler(MouseEvent mouseEvent)
        => NotImplementedOrNone.NotImplemented;

    /// <summary>
    /// Move cursor down (scroll request from Window).
    /// </summary>
    void MoveCursorDown() { }

    /// <summary>
    /// Move cursor up (scroll request from Window).
    /// </summary>
    void MoveCursorUp() { }

    /// <summary>
    /// Get key bindings for this control.
    /// </summary>
    /// <returns>Key bindings or null if none.</returns>
    IKeyBindingsBase? GetKeyBindings() => null;

    /// <summary>
    /// Get events that trigger a redraw of this control.
    /// </summary>
    /// <returns>Collection of invalidation events.</returns>
    IEnumerable<Event<object>> GetInvalidateEvents() => [];
}
