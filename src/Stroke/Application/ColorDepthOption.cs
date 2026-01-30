using Stroke.Output;

namespace Stroke.Application;

/// <summary>
/// Represents a color depth option that can be a fixed value, a callable, or null (auto-detect).
/// </summary>
/// <remarks>
/// <para>
/// Port of the <c>color_depth</c> parameter handling in Python Prompt Toolkit's
/// <c>Application.__init__</c>. The Python version accepts <c>ColorDepth</c>,
/// <c>Callable[[], ColorDepth | None]</c>, or <c>None</c>.
/// </para>
/// <para>
/// Resolution priority:
/// <list type="number">
/// <item>If a fixed <see cref="ColorDepth"/> value was provided, return it.</item>
/// <item>If a callable was provided, invoke it. If non-null, return that value.</item>
/// <item>Fall back to the output device's default color depth.</item>
/// </list>
/// </para>
/// </remarks>
public readonly struct ColorDepthOption : IEquatable<ColorDepthOption>
{
    private readonly ColorDepth? _value;
    private readonly Func<ColorDepth?>? _factory;
    private readonly bool _hasValue;

    /// <summary>
    /// Creates a new instance with a fixed color depth value.
    /// </summary>
    /// <param name="value">The fixed color depth.</param>
    public ColorDepthOption(ColorDepth value)
    {
        _value = value;
        _factory = null;
        _hasValue = true;
    }

    /// <summary>
    /// Creates a new instance with a factory that produces a color depth at resolution time.
    /// </summary>
    /// <param name="factory">The factory function. May return null to fall back to output default.</param>
    public ColorDepthOption(Func<ColorDepth?> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);
        _value = null;
        _factory = factory;
        _hasValue = true;
    }

    /// <summary>
    /// Implicit conversion from a fixed <see cref="ColorDepth"/> value.
    /// </summary>
    public static implicit operator ColorDepthOption(ColorDepth value) => new(value);

    /// <summary>
    /// Implicit conversion from a nullable <see cref="ColorDepth"/> value.
    /// Returns default (auto-detect) if null.
    /// </summary>
    public static implicit operator ColorDepthOption(ColorDepth? value) =>
        value.HasValue ? new ColorDepthOption(value.Value) : default;

    /// <summary>
    /// Implicit conversion from a factory function.
    /// </summary>
    public static implicit operator ColorDepthOption(Func<ColorDepth?> factory) => new(factory);

    /// <summary>
    /// Resolve the color depth using the priority order:
    /// <list type="number">
    /// <item>If a fixed <see cref="ColorDepth"/> value was provided, return it.</item>
    /// <item>If a callable was provided, invoke it. If non-null, return that value.</item>
    /// <item>Fall back to <paramref name="output"/>.GetDefaultColorDepth().</item>
    /// </list>
    /// </summary>
    /// <param name="output">The output device for fallback color depth detection.</param>
    /// <returns>The resolved color depth.</returns>
    public ColorDepth Resolve(IOutput output)
    {
        ArgumentNullException.ThrowIfNull(output);

        if (_value.HasValue)
        {
            return _value.Value;
        }

        if (_factory is not null)
        {
            var result = _factory();
            if (result.HasValue)
            {
                return result.Value;
            }
        }

        return output.GetDefaultColorDepth();
    }

    /// <inheritdoc/>
    public bool Equals(ColorDepthOption other) =>
        _hasValue == other._hasValue &&
        _value == other._value &&
        ReferenceEquals(_factory, other._factory);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is ColorDepthOption other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(_hasValue, _value, _factory?.GetHashCode() ?? 0);

    /// <summary>Equality operator.</summary>
    public static bool operator ==(ColorDepthOption left, ColorDepthOption right) => left.Equals(right);

    /// <summary>Inequality operator.</summary>
    public static bool operator !=(ColorDepthOption left, ColorDepthOption right) => !left.Equals(right);

    /// <inheritdoc/>
    public override string ToString()
    {
        if (_value.HasValue) return $"ColorDepthOption({_value.Value})";
        if (_factory is not null) return "ColorDepthOption(Func)";
        return "ColorDepthOption(auto)";
    }
}
