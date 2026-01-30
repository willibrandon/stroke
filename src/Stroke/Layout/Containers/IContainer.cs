using Stroke.KeyBinding;

namespace Stroke.Layout.Containers;

/// <summary>
/// Base interface for all layout containers.
/// </summary>
/// <remarks>
/// <para>
/// Containers form the layout hierarchy, arranging their children in the terminal.
/// Each container can have child containers and controls what region of the screen
/// each child occupies.
/// </para>
/// <para>
/// Port of Python Prompt Toolkit's <c>Container</c> abstract class from <c>layout/containers.py</c>.
/// </para>
/// </remarks>
public interface IContainer
{
    /// <summary>
    /// Reset the state of the container and all children.
    /// </summary>
    /// <remarks>
    /// Called at the start of a new render pass to reset caches and scroll state.
    /// </remarks>
    void Reset();

    /// <summary>
    /// Return the preferred width for this container.
    /// </summary>
    /// <param name="maxAvailableWidth">The maximum width available from the parent.</param>
    /// <returns>A dimension representing width constraints and preferences.</returns>
    Dimension PreferredWidth(int maxAvailableWidth);

    /// <summary>
    /// Return the preferred height for this container.
    /// </summary>
    /// <param name="width">The actual width that will be used.</param>
    /// <param name="maxAvailableHeight">The maximum height available from the parent.</param>
    /// <returns>A dimension representing height constraints and preferences.</returns>
    Dimension PreferredHeight(int width, int maxAvailableHeight);

    /// <summary>
    /// Write the container content to the screen.
    /// </summary>
    /// <param name="screen">The screen buffer to write to.</param>
    /// <param name="mouseHandlers">Registry for mouse event handlers.</param>
    /// <param name="writePosition">The rectangular region to render in.</param>
    /// <param name="parentStyle">Style class inherited from parent container.</param>
    /// <param name="eraseBg">Whether to erase background before rendering.</param>
    /// <param name="zIndex">Z-index for deferred float rendering. Null for immediate render.</param>
    void WriteToScreen(
        Screen screen,
        MouseHandlers mouseHandlers,
        WritePosition writePosition,
        string parentStyle,
        bool eraseBg,
        int? zIndex);

    /// <summary>
    /// Gets whether key bindings in this container are modal.
    /// </summary>
    /// <remarks>
    /// When true, this container's key bindings take precedence and
    /// prevent bindings from parent containers from being active.
    /// </remarks>
    bool IsModal { get; }

    /// <summary>
    /// Gets the key bindings for this container.
    /// </summary>
    /// <returns>Key bindings, or null if none.</returns>
    IKeyBindingsBase? GetKeyBindings();

    /// <summary>
    /// Return the list of direct children containers.
    /// </summary>
    /// <returns>List of child containers. Empty list if no children.</returns>
    IReadOnlyList<IContainer> GetChildren();
}
