using Stroke.Application;

namespace Stroke.KeyBinding.Bindings;

/// <summary>
/// Extension methods for <see cref="KeyPressEvent"/> to provide typed Application access.
/// </summary>
/// <remarks>
/// Port of the <c>event.app</c> typed access pattern from Python Prompt Toolkit's
/// <c>prompt_toolkit.key_binding.key_processor.KeyPressEvent</c>.
/// </remarks>
internal static class KeyPressEventExtensions
{
    /// <summary>
    /// Gets the <see cref="IApplication"/> instance from the event.
    /// </summary>
    /// <param name="event">The key press event.</param>
    /// <returns>The application instance.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <see cref="KeyPressEvent.App"/> is null.
    /// </exception>
    internal static IApplication GetApp(this KeyPressEvent @event)
    {
        return @event.App
            ?? throw new InvalidOperationException(
                "KeyPressEvent.App is null. No Application is associated with this event.");
    }
}
