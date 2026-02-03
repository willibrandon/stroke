using System.Runtime.CompilerServices;
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
    /// Gets the <see cref="Application{TResult}"/> instance from the event, cast to
    /// <c>Application&lt;object&gt;</c>.
    /// </summary>
    /// <param name="event">The key press event.</param>
    /// <returns>The typed Application instance.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <see cref="KeyPressEvent.App"/> is null or not an
    /// <see cref="Application{TResult}"/>.
    /// </exception>
    /// <remarks>
    /// C# generics are invariant, so <c>Application&lt;string&gt;</c> is NOT assignable to
    /// <c>Application&lt;object&gt;</c>. This method uses <see cref="Unsafe.As{T}"/> to
    /// safely reinterpret any <c>Application&lt;T&gt;</c> variant, matching the pattern
    /// used in <see cref="AppContext"/>.
    /// </remarks>
    internal static Application<object> GetApp(this KeyPressEvent @event)
    {
        if (@event.App is not null)
        {
            var appType = @event.App.GetType();
            if (appType.IsGenericType && appType.GetGenericTypeDefinition() == typeof(Application<>))
            {
                return Unsafe.As<Application<object>>(@event.App);
            }
        }

        throw new InvalidOperationException(
            @event.App is null
                ? "KeyPressEvent.App is null. No Application is associated with this event."
                : $"KeyPressEvent.App is not an Application<T>. Actual type: {@event.App.GetType().FullName}");
    }
}
