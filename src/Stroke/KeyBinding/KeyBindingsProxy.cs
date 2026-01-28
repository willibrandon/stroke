namespace Stroke.KeyBinding;

/// <summary>
/// Abstract base class for key binding wrappers that delegate to another registry.
/// </summary>
/// <remarks>
/// <para>
/// This class is thread-safe. Cache updates are protected by an internal lock.
/// </para>
/// <para>
/// Equivalent to Python Prompt Toolkit's <c>_Proxy</c> class from <c>key_bindings.py</c>.
/// </para>
/// </remarks>
public abstract class KeyBindingsProxy : IKeyBindingsBase
{
    /// <summary>
    /// Lock for thread-safe cache updates.
    /// </summary>
    protected readonly Lock Lock = new();

    /// <summary>
    /// Cached delegate registry. Updated when source version changes.
    /// </summary>
    protected IKeyBindingsBase? CachedBindings;

    /// <summary>
    /// Last seen version for cache invalidation.
    /// </summary>
    protected object? LastVersion;

    /// <summary>
    /// Gets the version for cache invalidation.
    /// Derived classes override to provide composite versions if needed.
    /// </summary>
    public virtual object Version
    {
        get
        {
            using (Lock.EnterScope())
            {
                UpdateCacheIfNeeded();
                return CachedBindings?.Version ?? new object();
            }
        }
    }

    /// <summary>
    /// Gets all bindings in this registry.
    /// </summary>
    public IReadOnlyList<Binding> Bindings
    {
        get
        {
            using (Lock.EnterScope())
            {
                UpdateCacheIfNeeded();
                return CachedBindings?.Bindings ?? [];
            }
        }
    }

    /// <summary>
    /// Returns bindings that exactly match the given key sequence.
    /// </summary>
    /// <param name="keys">The key sequence to match.</param>
    /// <returns>Matching bindings including inactive ones.</returns>
    public IReadOnlyList<Binding> GetBindingsForKeys(IReadOnlyList<KeyOrChar> keys)
    {
        using (Lock.EnterScope())
        {
            UpdateCacheIfNeeded();
            return CachedBindings?.GetBindingsForKeys(keys) ?? [];
        }
    }

    /// <summary>
    /// Returns bindings with sequences longer than and starting with the given prefix.
    /// </summary>
    /// <param name="keys">The prefix to match.</param>
    /// <returns>Bindings that could complete the sequence.</returns>
    public IReadOnlyList<Binding> GetBindingsStartingWithKeys(IReadOnlyList<KeyOrChar> keys)
    {
        using (Lock.EnterScope())
        {
            UpdateCacheIfNeeded();
            return CachedBindings?.GetBindingsStartingWithKeys(keys) ?? [];
        }
    }

    /// <summary>
    /// Updates the cached bindings if the source version has changed.
    /// Must be called within a lock.
    /// </summary>
    protected void UpdateCacheIfNeeded()
    {
        var currentVersion = GetSourceVersion();
        if (!Equals(LastVersion, currentVersion))
        {
            CachedBindings = CreateCachedBindings();
            LastVersion = currentVersion;
        }
    }

    /// <summary>
    /// Gets the current version of the source bindings.
    /// Derived classes override to provide version tracking.
    /// </summary>
    /// <returns>The source version object.</returns>
    protected abstract object GetSourceVersion();

    /// <summary>
    /// Creates the cached bindings by transforming the source.
    /// Derived classes override to apply filters, merge registries, etc.
    /// </summary>
    /// <returns>The transformed key bindings.</returns>
    protected abstract IKeyBindingsBase CreateCachedBindings();
}
