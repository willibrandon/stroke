namespace Stroke.Layout.Containers;

/// <summary>
/// Utility functions for container manipulation.
/// </summary>
/// <remarks>
/// Port of Python Prompt Toolkit's container utility functions from <c>layout/containers.py</c>.
/// </remarks>
public static class ContainerUtils
{
    /// <summary>
    /// Convert any container-like object to <see cref="IContainer"/>.
    /// </summary>
    /// <param name="value">The container-like value.</param>
    /// <returns>The underlying container.</returns>
    public static IContainer ToContainer(AnyContainer value) => value.ToContainer();

    /// <summary>
    /// Convert any container-like object to a Window.
    /// </summary>
    /// <param name="value">The container-like value.</param>
    /// <returns>The window.</returns>
    /// <exception cref="InvalidOperationException">
    /// The value is not a Window or does not resolve to a Window.
    /// </exception>
    /// <remarks>
    /// This method traverses the container hierarchy to find the leaf Window.
    /// For containers with multiple children, it follows the first child path.
    /// </remarks>
    public static IWindow ToWindow(AnyContainer value)
    {
        var container = value.ToContainer();

        // If it's already a Window, return it
        if (container is IWindow window)
        {
            return window;
        }

        // Try to find a Window in the children
        var children = container.GetChildren();
        if (children.Count > 0)
        {
            return ToWindow(new AnyContainer(children[0]));
        }

        throw new InvalidOperationException(
            $"Cannot convert {container.GetType().Name} to Window: no Window found in hierarchy.");
    }

    /// <summary>
    /// Check if a value can be converted to <see cref="IContainer"/>.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns>True if the value is a container or magic container.</returns>
    public static bool IsContainer(object? value)
    {
        return value is IContainer or IMagicContainer;
    }
}
