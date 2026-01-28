namespace Stroke.KeyBinding;

/// <summary>
/// Interface defining the contract for key binding registries.
/// </summary>
/// <remarks>
/// <para>
/// Implementations must be thread-safe for concurrent read access.
/// </para>
/// <para>
/// Equivalent to Python Prompt Toolkit's <c>KeyBindingsBase</c> abstract class.
/// </para>
/// </remarks>
public interface IKeyBindingsBase
{
    /// <summary>
    /// Gets the version for cache invalidation.
    /// This value changes whenever bindings are added or removed.
    /// </summary>
    object Version { get; }

    /// <summary>
    /// Gets all bindings in this registry.
    /// </summary>
    IReadOnlyList<Binding> Bindings { get; }

    /// <summary>
    /// Returns bindings that exactly match the given key sequence.
    /// Results are sorted by Keys.Any count (fewer wildcards = higher priority).
    /// </summary>
    /// <param name="keys">The key sequence to match.</param>
    /// <returns>
    /// Matching bindings including inactive ones. Caller must check filters.
    /// Returns empty list if <paramref name="keys"/> is empty.
    /// </returns>
    IReadOnlyList<Binding> GetBindingsForKeys(IReadOnlyList<KeyOrChar> keys);

    /// <summary>
    /// Returns bindings with sequences longer than and starting with the given prefix.
    /// </summary>
    /// <param name="keys">The prefix to match.</param>
    /// <returns>
    /// Bindings that could complete the sequence. Caller must check filters.
    /// Returns all bindings if <paramref name="keys"/> is empty.
    /// </returns>
    IReadOnlyList<Binding> GetBindingsStartingWithKeys(IReadOnlyList<KeyOrChar> keys);
}
