namespace Stroke.Core;

/// <summary>
/// Utility methods for converting lazy (callable) values to concrete values.
/// </summary>
/// <remarks>
/// <para>
/// These utilities allow APIs to accept both immediate values and functions that
/// return values, normalizing them to their concrete form when needed.
/// </para>
/// <para>
/// Port of Python Prompt Toolkit's <c>to_str</c>, <c>to_int</c>, and <c>to_float</c>
/// functions from <c>utils.py</c>.
/// </para>
/// <para>
/// Nested callables are recursively unwrapped until a concrete value is found.
/// </para>
/// </remarks>
public static class ConversionUtils
{
    #region ToStr Overloads

    /// <summary>
    /// Converts a string or callable returning a string to a string.
    /// </summary>
    /// <param name="value">A string value.</param>
    /// <returns>The string value, or an empty string if <paramref name="value"/> is null.</returns>
    public static string ToStr(string? value)
    {
        return value ?? "";
    }

    /// <summary>
    /// Converts a callable returning a string to a string.
    /// </summary>
    /// <param name="getter">A function that returns a string.</param>
    /// <returns>
    /// The result of invoking the function. If the function returns another callable,
    /// it is recursively invoked. Returns an empty string if <paramref name="getter"/> is null.
    /// </returns>
    public static string ToStr(Func<string?>? getter)
    {
        if (getter is null)
        {
            return "";
        }

        return ToStr(getter());
    }

    /// <summary>
    /// Converts a callable returning a callable returning a string to a string.
    /// </summary>
    /// <param name="getter">A nested callable.</param>
    /// <returns>The final string value after recursive unwrapping.</returns>
    public static string ToStr(Func<Func<string?>?>? getter)
    {
        if (getter is null)
        {
            return "";
        }

        var inner = getter();
        return ToStr(inner);
    }

    /// <summary>
    /// Converts an object to a string, handling callables.
    /// </summary>
    /// <param name="value">A value that may be a string, callable, or other object.</param>
    /// <returns>
    /// The string representation. Callables are invoked recursively.
    /// Returns an empty string if <paramref name="value"/> is null.
    /// </returns>
    public static string ToStr(object? value)
    {
        return value switch
        {
            null => "",
            string s => s,
            Func<string?> getter => ToStr(getter),
            Func<Func<string?>?> nested => ToStr(nested),
            _ => value.ToString() ?? ""
        };
    }

    #endregion

    #region ToInt Overloads

    /// <summary>
    /// Converts an integer value to an integer.
    /// </summary>
    /// <param name="value">An integer value.</param>
    /// <returns>The integer value.</returns>
    public static int ToInt(int value)
    {
        return value;
    }

    /// <summary>
    /// Converts a callable returning an integer to an integer.
    /// </summary>
    /// <param name="getter">A function that returns an integer.</param>
    /// <returns>
    /// The result of invoking the function. If the function returns another callable,
    /// it is recursively invoked. Returns 0 if <paramref name="getter"/> is null.
    /// </returns>
    public static int ToInt(Func<int>? getter)
    {
        if (getter is null)
        {
            return 0;
        }

        return getter();
    }

    /// <summary>
    /// Converts an object to an integer, handling callables.
    /// </summary>
    /// <param name="value">A value that may be an integer, callable, or other object.</param>
    /// <returns>
    /// The integer value. Callables are invoked recursively.
    /// Returns 0 if <paramref name="value"/> is null.
    /// </returns>
    public static int ToInt(object? value)
    {
        switch (value)
        {
            case null:
                return 0;
            case int i:
                return i;
            case Func<int> getter:
                return ToInt(getter);
            default:
                try
                {
                    return Convert.ToInt32(value);
                }
                catch
                {
                    return 0;
                }
        }
    }

    #endregion

    #region ToFloat Overloads

    /// <summary>
    /// Converts a double value to a double.
    /// </summary>
    /// <param name="value">A double value.</param>
    /// <returns>The double value.</returns>
    public static double ToFloat(double value)
    {
        return value;
    }

    /// <summary>
    /// Converts a callable returning a double to a double.
    /// </summary>
    /// <param name="getter">A function that returns a double.</param>
    /// <returns>
    /// The result of invoking the function. If the function returns another callable,
    /// it is recursively invoked. Returns 0.0 if <paramref name="getter"/> is null.
    /// </returns>
    public static double ToFloat(Func<double>? getter)
    {
        if (getter is null)
        {
            return 0.0;
        }

        return getter();
    }

    /// <summary>
    /// Converts an AnyFloat to a double.
    /// </summary>
    /// <param name="value">An AnyFloat value (concrete or callable).</param>
    /// <returns>The double value.</returns>
    public static double ToFloat(AnyFloat value)
    {
        return value.Value;
    }

    /// <summary>
    /// Converts an object to a double, handling callables.
    /// </summary>
    /// <param name="value">A value that may be a double, callable, or other object.</param>
    /// <returns>
    /// The double value. Callables are invoked recursively.
    /// Returns 0.0 if <paramref name="value"/> is null.
    /// </returns>
    public static double ToFloat(object? value)
    {
        switch (value)
        {
            case null:
                return 0.0;
            case double d:
                return d;
            case float f:
                return f;
            case AnyFloat af:
                return af.Value;
            case Func<double> getter:
                return ToFloat(getter);
            default:
                try
                {
                    return Convert.ToDouble(value);
                }
                catch
                {
                    return 0.0;
                }
        }
    }

    #endregion
}

/// <summary>
/// A value type that can hold either a concrete double value or a callable that returns a double.
/// </summary>
/// <remarks>
/// This type provides implicit conversions from <see cref="double"/> and <see cref="Func{T}"/>
/// to support flexible API design where callers can provide either immediate or lazy values.
/// </remarks>
public readonly struct AnyFloat : IEquatable<AnyFloat>
{
    private readonly double? _value;
    private readonly Func<double>? _getter;
    private readonly bool _hasValue;

    private AnyFloat(double value)
    {
        _value = value;
        _getter = null;
        _hasValue = true;
    }

    private AnyFloat(Func<double> getter)
    {
        _value = null;
        _getter = getter;
        _hasValue = true;
    }

    /// <summary>
    /// Gets the concrete double value, invoking the callable if necessary.
    /// </summary>
    public double Value => _value ?? _getter?.Invoke() ?? 0.0;

    /// <summary>
    /// Gets a value indicating whether this instance has a value (concrete or callable).
    /// </summary>
    public bool HasValue => _hasValue;

    /// <summary>
    /// Implicitly converts a double to an AnyFloat.
    /// </summary>
    public static implicit operator AnyFloat(double value) => new(value);

    /// <summary>
    /// Implicitly converts a callable to an AnyFloat.
    /// </summary>
    public static implicit operator AnyFloat(Func<double> getter) => new(getter);

    /// <summary>
    /// Explicitly converts an AnyFloat to a double.
    /// </summary>
    public static explicit operator double(AnyFloat value) => value.Value;

    /// <inheritdoc/>
    public bool Equals(AnyFloat other)
    {
        // If both are default, they are equal
        if (!_hasValue && !other._hasValue)
        {
            return true;
        }

        // If one has value and the other doesn't, not equal
        if (_hasValue != other._hasValue)
        {
            return false;
        }

        // If both have concrete values, compare values
        if (_value.HasValue && other._value.HasValue)
        {
            return _value.Value.Equals(other._value.Value);
        }

        // If both have getters, compare by reference
        if (_getter is not null && other._getter is not null)
        {
            return ReferenceEquals(_getter, other._getter);
        }

        // One has concrete value, other has getter - not equal
        return false;
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        return obj is AnyFloat other && Equals(other);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        if (!_hasValue)
        {
            return 0;
        }

        if (_value.HasValue)
        {
            return _value.Value.GetHashCode();
        }

        return _getter?.GetHashCode() ?? 0;
    }

    /// <summary>
    /// Equality operator.
    /// </summary>
    public static bool operator ==(AnyFloat left, AnyFloat right) => left.Equals(right);

    /// <summary>
    /// Inequality operator.
    /// </summary>
    public static bool operator !=(AnyFloat left, AnyFloat right) => !left.Equals(right);
}
