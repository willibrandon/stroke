using System.Collections.Concurrent;

namespace Stroke.Input.Typeahead;

/// <summary>
/// Global buffer for storing key presses read ahead of when they are needed.
/// </summary>
/// <remarks>
/// <para>
/// Input classes like <see cref="Vt100.Vt100Input"/> and <see cref="Windows.Win32Input"/>
/// read input in chunks for efficiency. This can result in reading more input than needed
/// for a single prompt. The typeahead buffer stores excess key presses so they can be
/// retrieved by the next prompt or application.
/// </para>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's
/// <c>prompt_toolkit.input.typeahead</c> module.
/// </para>
/// <para>
/// Thread safety: All operations are thread-safe using <see cref="ConcurrentDictionary{TKey, TValue}"/>.
/// </para>
/// </remarks>
public static class TypeaheadBuffer
{
    private static readonly ConcurrentDictionary<string, List<KeyPress>> _buffer = new();
    private static readonly Lock _lock = new();

    /// <summary>
    /// Stores typeahead key presses for the given input.
    /// </summary>
    /// <param name="input">The input source to store typeahead for.</param>
    /// <param name="keyPresses">The key presses to store.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="input"/> or <paramref name="keyPresses"/> is null.</exception>
    /// <remarks>
    /// Key presses are appended to any existing typeahead for this input.
    /// </remarks>
    public static void Store(IInput input, IEnumerable<KeyPress> keyPresses)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(keyPresses);

        var key = input.TypeaheadHash();
        var presses = keyPresses.ToList();

        if (presses.Count == 0)
            return;

        using (_lock.EnterScope())
        {
            var list = _buffer.GetOrAdd(key, _ => new List<KeyPress>());
            list.AddRange(presses);
        }
    }

    /// <summary>
    /// Retrieves and clears typeahead key presses for the given input.
    /// </summary>
    /// <param name="input">The input source to retrieve typeahead for.</param>
    /// <returns>The stored key presses, or an empty list if none stored.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="input"/> is null.</exception>
    /// <remarks>
    /// After calling this method, the typeahead buffer for this input is cleared.
    /// </remarks>
    public static IReadOnlyList<KeyPress> Get(IInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var key = input.TypeaheadHash();

        using (_lock.EnterScope())
        {
            if (_buffer.TryRemove(key, out var list))
            {
                return list;
            }

            return [];
        }
    }

    /// <summary>
    /// Clears the typeahead buffer for the given input.
    /// </summary>
    /// <param name="input">The input source to clear typeahead for.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="input"/> is null.</exception>
    public static void Clear(IInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var key = input.TypeaheadHash();

        using (_lock.EnterScope())
        {
            _buffer.TryRemove(key, out _);
        }
    }

    /// <summary>
    /// Clears all typeahead buffers.
    /// </summary>
    /// <remarks>
    /// Useful for testing or when resetting application state.
    /// </remarks>
    public static void ClearAll()
    {
        using (_lock.EnterScope())
        {
            _buffer.Clear();
        }
    }

    /// <summary>
    /// Gets whether there is any typeahead stored for the given input.
    /// </summary>
    /// <param name="input">The input source to check.</param>
    /// <returns>True if typeahead key presses are stored for this input.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="input"/> is null.</exception>
    public static bool HasTypeahead(IInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var key = input.TypeaheadHash();

        using (_lock.EnterScope())
        {
            return _buffer.TryGetValue(key, out var list) && list.Count > 0;
        }
    }
}
