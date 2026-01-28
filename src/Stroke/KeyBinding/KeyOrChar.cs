using Stroke.Input;

namespace Stroke.KeyBinding;

/// <summary>
/// Union type representing either a <see cref="Keys"/> enum value or a single character.
/// </summary>
/// <remarks>
/// <para>
/// This type is thread-safe as it is immutable.
/// </para>
/// <para>
/// Equivalent to Python Prompt Toolkit's <c>Keys | str</c> union type where str is a single character.
/// </para>
/// </remarks>
public readonly record struct KeyOrChar : IEquatable<KeyOrChar>
{
    private readonly Keys? _key;
    private readonly char? _char;

    /// <summary>Gets whether this represents a <see cref="Keys"/> enum value.</summary>
    public bool IsKey => _key.HasValue;

    /// <summary>Gets whether this represents a single character.</summary>
    public bool IsChar => _char.HasValue;

    /// <summary>Gets the key value. Throws if <see cref="IsKey"/> is false.</summary>
    /// <exception cref="InvalidOperationException">This is not a key.</exception>
    public Keys Key => _key ?? throw new InvalidOperationException("This KeyOrChar does not contain a key. Check IsKey before accessing.");

    /// <summary>Gets the character value. Throws if <see cref="IsChar"/> is false.</summary>
    /// <exception cref="InvalidOperationException">This is not a character.</exception>
    public char Char => _char ?? throw new InvalidOperationException("This KeyOrChar does not contain a character. Check IsChar before accessing.");

    /// <summary>
    /// Creates a KeyOrChar from a Keys enum value.
    /// </summary>
    /// <param name="key">The Keys enum value.</param>
    public KeyOrChar(Keys key)
    {
        _key = key;
        _char = null;
    }

    /// <summary>
    /// Creates a KeyOrChar from a single character.
    /// </summary>
    /// <param name="c">The character.</param>
    public KeyOrChar(char c)
    {
        _key = null;
        _char = c;
    }

    /// <summary>
    /// Implicit conversion from Keys.
    /// </summary>
    /// <param name="key">The Keys enum value.</param>
    public static implicit operator KeyOrChar(Keys key) => new(key);

    /// <summary>
    /// Implicit conversion from char.
    /// </summary>
    /// <param name="c">The character.</param>
    public static implicit operator KeyOrChar(char c) => new(c);

    /// <summary>
    /// Implicit conversion from single-character string.
    /// </summary>
    /// <param name="s">The string (must be exactly one character).</param>
    /// <exception cref="ArgumentNullException">String is null.</exception>
    /// <exception cref="ArgumentException">String is not exactly one character.</exception>
    public static implicit operator KeyOrChar(string s)
    {
        ArgumentNullException.ThrowIfNull(s);
        if (s.Length != 1)
        {
            throw new ArgumentException("String must be exactly one character.", nameof(s));
        }
        return new KeyOrChar(s[0]);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        if (IsKey)
        {
            return _key!.Value.ToString();
        }
        return _char!.Value.ToString();
    }
}
