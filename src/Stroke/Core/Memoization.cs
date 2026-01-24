// Copyright (c) 2024 Stroke Contributors
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Stroke.Core;

/// <summary>
/// Provides memoization utilities for caching function results.
/// </summary>
/// <remarks>
/// <para>Memoized functions cache results based on argument values.</para>
/// <para>The returned wrapper is thread-safe (uses <see cref="SimpleCache{TKey, TValue}"/> which is thread-safe).</para>
/// <para>Port of Python Prompt Toolkit's <c>memoized</c> decorator from <c>cache.py</c>.</para>
/// </remarks>
public static class Memoization
{
    /// <summary>
    /// Creates a memoized version of a single-argument function.
    /// </summary>
    /// <typeparam name="T1">The type of the function argument.</typeparam>
    /// <typeparam name="TResult">The return type of the function.</typeparam>
    /// <param name="func">The function to memoize.</param>
    /// <param name="maxSize">Maximum number of cached results. Default is 1024.</param>
    /// <returns>A memoized version of the function.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="func"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="maxSize"/> is less than or equal to 0.</exception>
    public static Func<T1, TResult> Memoize<T1, TResult>(
        Func<T1, TResult> func,
        int maxSize = 1024) where T1 : notnull
    {
        ArgumentNullException.ThrowIfNull(func);

        if (maxSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxSize), maxSize, "Maximum size must be greater than 0.");
        }

        var cache = new SimpleCache<T1, TResult>(maxSize);

        return arg => cache.Get(arg, () => func(arg));
    }

    /// <summary>
    /// Creates a memoized version of a two-argument function.
    /// </summary>
    /// <typeparam name="T1">The type of the first function argument.</typeparam>
    /// <typeparam name="T2">The type of the second function argument.</typeparam>
    /// <typeparam name="TResult">The return type of the function.</typeparam>
    /// <param name="func">The function to memoize.</param>
    /// <param name="maxSize">Maximum number of cached results. Default is 1024.</param>
    /// <returns>A memoized version of the function.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="func"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="maxSize"/> is less than or equal to 0.</exception>
    public static Func<T1, T2, TResult> Memoize<T1, T2, TResult>(
        Func<T1, T2, TResult> func,
        int maxSize = 1024)
        where T1 : notnull
        where T2 : notnull
    {
        ArgumentNullException.ThrowIfNull(func);

        if (maxSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxSize), maxSize, "Maximum size must be greater than 0.");
        }

        var cache = new SimpleCache<(T1, T2), TResult>(maxSize);

        return (arg1, arg2) => cache.Get((arg1, arg2), () => func(arg1, arg2));
    }

    /// <summary>
    /// Creates a memoized version of a three-argument function.
    /// </summary>
    /// <typeparam name="T1">The type of the first function argument.</typeparam>
    /// <typeparam name="T2">The type of the second function argument.</typeparam>
    /// <typeparam name="T3">The type of the third function argument.</typeparam>
    /// <typeparam name="TResult">The return type of the function.</typeparam>
    /// <param name="func">The function to memoize.</param>
    /// <param name="maxSize">Maximum number of cached results. Default is 1024.</param>
    /// <returns>A memoized version of the function.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="func"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="maxSize"/> is less than or equal to 0.</exception>
    public static Func<T1, T2, T3, TResult> Memoize<T1, T2, T3, TResult>(
        Func<T1, T2, T3, TResult> func,
        int maxSize = 1024)
        where T1 : notnull
        where T2 : notnull
        where T3 : notnull
    {
        ArgumentNullException.ThrowIfNull(func);

        if (maxSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxSize), maxSize, "Maximum size must be greater than 0.");
        }

        var cache = new SimpleCache<(T1, T2, T3), TResult>(maxSize);

        return (arg1, arg2, arg3) => cache.Get((arg1, arg2, arg3), () => func(arg1, arg2, arg3));
    }
}
