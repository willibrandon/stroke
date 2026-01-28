namespace Stroke.KeyBinding;

/// <summary>
/// A key bindings wrapper that delegates to a callable returning a registry at runtime.
/// </summary>
/// <remarks>
/// <para>
/// The GetKeyBindings callable is invoked on each access to determine the current registry.
/// If the callable returns null, an empty KeyBindings is used.
/// </para>
/// <para>
/// Cache invalidation occurs when the callable returns a different registry instance
/// or when the returned registry's version changes (per FR-054).
/// </para>
/// <para>
/// This type is thread-safe. Cache operations and callable invocation are synchronized
/// using <see cref="System.Threading.Lock"/> (per FR-053).
/// </para>
/// <para>
/// Equivalent to Python Prompt Toolkit's <c>DynamicKeyBindings</c> class.
/// </para>
/// </remarks>
public sealed class DynamicKeyBindings : KeyBindingsProxy
{
    private readonly Func<IKeyBindingsBase?> _getKeyBindings;
    private readonly KeyBindings _emptyBindings = new();
    private IKeyBindingsBase? _currentSource;

    /// <summary>
    /// Creates a new DynamicKeyBindings with the specified callable.
    /// </summary>
    /// <param name="getKeyBindings">A callable that returns the current key bindings registry.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="getKeyBindings"/> is null.</exception>
    public DynamicKeyBindings(Func<IKeyBindingsBase?> getKeyBindings)
    {
        ArgumentNullException.ThrowIfNull(getKeyBindings);
        _getKeyBindings = getKeyBindings;
    }

    /// <summary>
    /// Gets the callable that returns the current key bindings registry.
    /// </summary>
    public Func<IKeyBindingsBase?> GetKeyBindings => _getKeyBindings;

    /// <inheritdoc/>
    protected override object GetSourceVersion()
    {
        // Invoke callable within lock per FR-053
        var source = _getKeyBindings();
        _currentSource = source;

        if (source is null)
        {
            // Return a stable version when source is null
            return (_emptyBindings, _emptyBindings.Version);
        }

        // Version is tuple of (identity, version) to detect registry changes per FR-054
        return (source, source.Version);
    }

    /// <inheritdoc/>
    protected override IKeyBindingsBase CreateCachedBindings()
    {
        // Use the source captured by GetSourceVersion
        return _currentSource ?? _emptyBindings;
    }
}
