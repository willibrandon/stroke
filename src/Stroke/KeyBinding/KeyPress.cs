using Stroke.Input;

namespace Stroke.KeyBinding;

/// <summary>
/// Represents a single key press with the key value and optional raw terminal data.
/// </summary>
/// <remarks>
/// <para>
/// This type is thread-safe as it is immutable.
/// </para>
/// <para>
/// Equivalent to Python Prompt Toolkit's <c>KeyPress</c> class from <c>key_processor.py</c>.
/// </para>
/// </remarks>
public readonly record struct KeyPress : IEquatable<KeyPress>
{
    /// <summary>Gets the key that was pressed.</summary>
    public KeyOrChar Key { get; }

    /// <summary>
    /// Gets the raw terminal data (escape sequence or character).
    /// If not provided at construction, defaults to the key's string representation:
    /// for characters, the character itself; for keys, the enum name (e.g., "ControlA", "Enter").
    /// </summary>
    public string Data { get; }

    /// <summary>
    /// Creates a new KeyPress with the specified key and optional raw data.
    /// </summary>
    /// <param name="key">The key or character pressed.</param>
    /// <param name="data">
    /// Raw terminal data. If null, defaults based on key type:
    /// for characters, the character as a string; for keys, the enum name.
    /// </param>
    public KeyPress(KeyOrChar key, string? data = null)
    {
        Key = key;
        Data = data ?? GetDefaultData(key);
    }

    /// <summary>
    /// Creates a KeyPress from a Keys enum value.
    /// </summary>
    /// <param name="key">The Keys enum value.</param>
    /// <param name="data">Raw terminal data. If null, defaults to the enum name.</param>
    public KeyPress(Keys key, string? data = null)
        : this(new KeyOrChar(key), data)
    {
    }

    /// <summary>
    /// Creates a KeyPress from a character.
    /// </summary>
    /// <param name="c">The character.</param>
    /// <param name="data">Raw terminal data. If null, defaults to the character as a string.</param>
    public KeyPress(char c, string? data = null)
        : this(new KeyOrChar(c), data)
    {
    }

    /// <summary>
    /// Gets the default data string for a KeyOrChar value.
    /// </summary>
    /// <param name="key">The key or character.</param>
    /// <returns>
    /// For characters: the character as a string.
    /// For keys: the enum name (e.g., "ControlA", "Enter").
    /// </returns>
    private static string GetDefaultData(KeyOrChar key)
    {
        if (key.IsChar)
        {
            return key.Char.ToString();
        }
        return key.Key.ToString();
    }

    /// <summary>
    /// Implicit conversion from Keys enum value.
    /// </summary>
    /// <param name="key">The Keys enum value.</param>
    public static implicit operator KeyPress(Keys key) => new(key);

    /// <summary>
    /// Implicit conversion from character.
    /// </summary>
    /// <param name="c">The character.</param>
    public static implicit operator KeyPress(char c) => new(c);

    /// <summary>
    /// Implicit conversion from KeyOrChar.
    /// </summary>
    /// <param name="key">The KeyOrChar value.</param>
    public static implicit operator KeyPress(KeyOrChar key) => new(key);

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"KeyPress(Key={Key}, Data=\"{Data}\")";
    }
}
