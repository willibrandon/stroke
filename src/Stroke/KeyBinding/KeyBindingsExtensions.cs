using Stroke.Filters;

namespace Stroke.KeyBinding;

/// <summary>
/// Extension methods for <see cref="IKeyBindingsBase"/>.
/// </summary>
/// <remarks>
/// <para>
/// Equivalent to Python Prompt Toolkit's key binding extension methods.
/// </para>
/// </remarks>
public static class KeyBindingsExtensions
{
    /// <summary>
    /// Merges this key bindings with another registry (per FR-040).
    /// </summary>
    /// <param name="keyBindings">The first key bindings registry.</param>
    /// <param name="other">The registry to merge with.</param>
    /// <returns>A merged key bindings view.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="keyBindings"/> or <paramref name="other"/> is null.</exception>
    public static IKeyBindingsBase Merge(this IKeyBindingsBase keyBindings, IKeyBindingsBase other)
    {
        ArgumentNullException.ThrowIfNull(keyBindings);
        ArgumentNullException.ThrowIfNull(other);
        return new MergedKeyBindings(keyBindings, other);
    }

    /// <summary>
    /// Wraps the key bindings with a conditional filter.
    /// </summary>
    /// <param name="keyBindings">The key bindings registry.</param>
    /// <param name="filter">The filter to apply to all bindings.</param>
    /// <returns>A conditional key bindings wrapper.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="keyBindings"/> or <paramref name="filter"/> is null.</exception>
    public static IKeyBindingsBase WithFilter(this IKeyBindingsBase keyBindings, IFilter filter)
    {
        ArgumentNullException.ThrowIfNull(keyBindings);
        ArgumentNullException.ThrowIfNull(filter);
        return new ConditionalKeyBindings(keyBindings, filter);
    }

    /// <summary>
    /// Returns only the global bindings from this registry.
    /// </summary>
    /// <param name="keyBindings">The key bindings registry.</param>
    /// <returns>A key bindings view containing only global bindings.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="keyBindings"/> is null.</exception>
    public static IKeyBindingsBase GlobalOnly(this IKeyBindingsBase keyBindings)
    {
        ArgumentNullException.ThrowIfNull(keyBindings);
        return new GlobalOnlyKeyBindings(keyBindings);
    }
}
