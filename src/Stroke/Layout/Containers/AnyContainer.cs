namespace Stroke.Layout.Containers;

/// <summary>
/// Union type that can hold an <see cref="IContainer"/> or an object implementing
/// the <see cref="IMagicContainer"/> protocol.
/// </summary>
/// <remarks>
/// <para>
/// This struct provides implicit conversions from all container types, allowing methods
/// to accept any container-like object without requiring explicit conversion.
/// </para>
/// <para>
/// Port of Python Prompt Toolkit's <c>AnyContainer</c> type alias which accepts
/// any object with a <c>__pt_container__()</c> method.
/// </para>
/// </remarks>
public readonly struct AnyContainer
{
    private readonly object? _value;

    /// <summary>
    /// Initializes a new instance with an <see cref="IContainer"/>.
    /// </summary>
    /// <param name="container">The container.</param>
    /// <exception cref="ArgumentNullException"><paramref name="container"/> is null.</exception>
    public AnyContainer(IContainer container)
    {
        ArgumentNullException.ThrowIfNull(container);
        _value = container;
    }

    /// <summary>
    /// Initializes a new instance with an <see cref="IMagicContainer"/>.
    /// </summary>
    /// <param name="magicContainer">The magic container.</param>
    /// <exception cref="ArgumentNullException"><paramref name="magicContainer"/> is null.</exception>
    public AnyContainer(IMagicContainer magicContainer)
    {
        ArgumentNullException.ThrowIfNull(magicContainer);
        _value = magicContainer;
    }

    /// <summary>
    /// Gets whether this instance holds a value.
    /// </summary>
    public bool HasValue => _value is not null;

    /// <summary>
    /// Converts to the underlying <see cref="IContainer"/>.
    /// </summary>
    /// <returns>The container.</returns>
    /// <exception cref="InvalidOperationException">This instance has no value.</exception>
    public IContainer ToContainer()
    {
        return _value switch
        {
            IContainer container => container,
            IMagicContainer magic => magic.PtContainer(),
            null => throw new InvalidOperationException("AnyContainer has no value."),
            _ => throw new InvalidOperationException($"Unexpected value type: {_value.GetType()}")
        };
    }

    /// <summary>
    /// Creates an <see cref="AnyContainer"/> from a container-like object.
    /// </summary>
    /// <param name="value">The value to wrap.</param>
    /// <returns>A new AnyContainer wrapping the value.</returns>
    /// <exception cref="ArgumentException">The value is not a container type.</exception>
    public static AnyContainer From(object value)
    {
        return value switch
        {
            IContainer container => new AnyContainer(container),
            IMagicContainer magic => new AnyContainer(magic),
            _ => throw new ArgumentException($"Cannot convert {value?.GetType().Name ?? "null"} to AnyContainer.", nameof(value))
        };
    }

    // Note: C# does not allow implicit conversions from interfaces.
    // Use constructors or From() method for IContainer/IMagicContainer.
    // Implicit conversions for concrete types will be added as they are implemented.
}
