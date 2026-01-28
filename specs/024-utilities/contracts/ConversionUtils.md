# Contract: ConversionUtils

**Namespace**: `Stroke.Core`
**File**: `src/Stroke/Core/ConversionUtils.cs`

## API Contract

```csharp
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
    /// <summary>
    /// Converts a string or callable returning a string to a string.
    /// </summary>
    /// <param name="value">A string value.</param>
    /// <returns>The string value, or an empty string if <paramref name="value"/> is null.</returns>
    public static string ToStr(string? value);

    /// <summary>
    /// Converts a callable returning a string to a string.
    /// </summary>
    /// <param name="getter">A function that returns a string.</param>
    /// <returns>
    /// The result of invoking the function. If the function returns another callable,
    /// it is recursively invoked. Returns an empty string if <paramref name="getter"/> is null.
    /// </returns>
    public static string ToStr(Func<string?>? getter);

    /// <summary>
    /// Converts a callable returning a callable returning a string to a string.
    /// </summary>
    /// <param name="getter">A nested callable.</param>
    /// <returns>The final string value after recursive unwrapping.</returns>
    public static string ToStr(Func<Func<string?>?>? getter);

    /// <summary>
    /// Converts an object to a string, handling callables.
    /// </summary>
    /// <param name="value">A value that may be a string, callable, or other object.</param>
    /// <returns>
    /// The string representation. Callables are invoked recursively.
    /// Returns an empty string if <paramref name="value"/> is null.
    /// </returns>
    public static string ToStr(object? value);

    /// <summary>
    /// Converts an integer value to an integer.
    /// </summary>
    /// <param name="value">An integer value.</param>
    /// <returns>The integer value.</returns>
    public static int ToInt(int value);

    /// <summary>
    /// Converts a callable returning an integer to an integer.
    /// </summary>
    /// <param name="getter">A function that returns an integer.</param>
    /// <returns>
    /// The result of invoking the function. If the function returns another callable,
    /// it is recursively invoked. Returns 0 if <paramref name="getter"/> is null.
    /// </returns>
    public static int ToInt(Func<int>? getter);

    /// <summary>
    /// Converts an object to an integer, handling callables.
    /// </summary>
    /// <param name="value">A value that may be an integer, callable, or other object.</param>
    /// <returns>
    /// The integer value. Callables are invoked recursively.
    /// Returns 0 if <paramref name="value"/> is null.
    /// </returns>
    public static int ToInt(object? value);

    /// <summary>
    /// Converts a double value to a double.
    /// </summary>
    /// <param name="value">A double value.</param>
    /// <returns>The double value.</returns>
    public static double ToFloat(double value);

    /// <summary>
    /// Converts a callable returning a double to a double.
    /// </summary>
    /// <param name="getter">A function that returns a double.</param>
    /// <returns>
    /// The result of invoking the function. If the function returns another callable,
    /// it is recursively invoked. Returns 0.0 if <paramref name="getter"/> is null.
    /// </returns>
    public static double ToFloat(Func<double>? getter);

    /// <summary>
    /// Converts an AnyFloat to a double.
    /// </summary>
    /// <param name="value">An AnyFloat value (concrete or callable).</param>
    /// <returns>The double value.</returns>
    public static double ToFloat(AnyFloat value);

    /// <summary>
    /// Converts an object to a double, handling callables.
    /// </summary>
    /// <param name="value">A value that may be a double, callable, or other object.</param>
    /// <returns>
    /// The double value. Callables are invoked recursively.
    /// Returns 0.0 if <paramref name="value"/> is null.
    /// </returns>
    public static double ToFloat(object? value);
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
    /// <summary>
    /// Gets the concrete double value, invoking the callable if necessary.
    /// </summary>
    public double Value { get; }

    /// <summary>
    /// Gets a value indicating whether this instance has a value (concrete or callable).
    /// </summary>
    public bool HasValue { get; }

    /// <summary>
    /// Implicitly converts a double to an AnyFloat.
    /// </summary>
    public static implicit operator AnyFloat(double value);

    /// <summary>
    /// Implicitly converts a callable to an AnyFloat.
    /// </summary>
    public static implicit operator AnyFloat(Func<double> getter);

    /// <summary>
    /// Explicitly converts an AnyFloat to a double.
    /// </summary>
    public static explicit operator double(AnyFloat value);

    /// <inheritdoc/>
    public bool Equals(AnyFloat other);

    /// <inheritdoc/>
    public override bool Equals(object? obj);

    /// <inheritdoc/>
    public override int GetHashCode();

    /// <summary>
    /// Equality operator.
    /// </summary>
    public static bool operator ==(AnyFloat left, AnyFloat right);

    /// <summary>
    /// Inequality operator.
    /// </summary>
    public static bool operator !=(AnyFloat left, AnyFloat right);
}
```

## Functional Requirements Coverage

| Requirement | Method |
|-------------|--------|
| FR-018 | `ToStr(string?)`, `ToStr(Func<string?>?)` |
| FR-019 | `ToInt(int)`, `ToInt(Func<int>?)` |
| FR-020 | `ToFloat(double)`, `ToFloat(Func<double>?)`, `AnyFloat` |

## Edge Cases

| Scenario | Behavior |
|----------|----------|
| Null string | Returns "" |
| Null Func<string> | Returns "" |
| Nested callable (Func returning Func) | Recursively unwraps until non-callable |
| Null for ToInt | Returns 0 |
| Null for ToFloat | Returns 0.0 |
| AnyFloat default | HasValue is false, Value is 0.0 |
| ToStr on non-string object | Calls `ToString()`; returns "" if null |
| ToInt on non-integer object | Uses `Convert.ToInt32()`; returns 0 on failure |
| ToFloat on non-double object | Uses `Convert.ToDouble()`; returns 0.0 on failure |
| Circular callable reference | Stack overflow (no protection; caller's responsibility) |
| AnyFloat equality with callables | Reference equality for delegate comparison |
| AnyFloat from callable | `Value` invokes callable each time (not cached) |

## Thread Safety

- **ConversionUtils**: Thread-safe (stateless static methods)
- **AnyFloat**: Thread-safe as an immutable struct; however, callable invocation may have side effects (caller's responsibility)

