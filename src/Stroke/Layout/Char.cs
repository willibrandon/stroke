using Stroke.Core;

namespace Stroke.Layout;

/// <summary>
/// Represents a single character in a Screen.
/// </summary>
/// <remarks>
/// <para>
/// This type is immutable and uses value equality semantics based on
/// <see cref="Character"/> and <see cref="Style"/> properties.
/// </para>
/// <para>
/// Control characters (0x00-0x1F, 0x7F) are automatically transformed to
/// caret notation (e.g., ^A) with "class:control-character" style.
/// High-byte characters (0x80-0x9F) are transformed to hex notation (e.g., "&lt;80&gt;").
/// Non-breaking space (0xA0) displays as space with "class:nbsp" style.
/// </para>
/// <para>
/// Port of Python Prompt Toolkit's <c>Char</c> class from <c>layout/screen.py</c>.
/// </para>
/// </remarks>
public sealed class Char : IEquatable<Char>
{
    /// <summary>
    /// Style string indicating a transparent (default) character.
    /// </summary>
    /// <remarks>
    /// Equivalent to Python Prompt Toolkit's <c>Transparent</c> constant.
    /// </remarks>
    public const string Transparent = "[Transparent]";

    // Internal cache for character interning (1,000,000 entries per Python PTK)
    private static readonly FastDictCache<(string Character, string Style), Char> _cache =
        new(key => new Char(key.Character, key.Style, skipCache: true), 1_000_000);

    /// <summary>
    /// Gets the displayed character string.
    /// </summary>
    /// <remarks>
    /// May be multiple characters for caret notation (e.g., "^A" for Ctrl+A)
    /// or hex notation (e.g., "&lt;80&gt;").
    /// </remarks>
    public string Character { get; }

    /// <summary>
    /// Gets the style string containing CSS-like class names.
    /// </summary>
    /// <remarks>
    /// May include "class:control-character" or "class:nbsp" added automatically
    /// during construction for special characters.
    /// </remarks>
    public string Style { get; }

    /// <summary>
    /// Gets the display width of this character in terminal cells.
    /// </summary>
    /// <remarks>
    /// 0 for combining/zero-width characters, 1 for standard width, 2 for wide (CJK) characters.
    /// Calculated using <see cref="UnicodeWidth.GetWidth(string)"/>.
    /// </remarks>
    public int Width { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Char"/> class.
    /// </summary>
    /// <param name="character">The character string. Default is a single space.</param>
    /// <param name="style">The style string. Default is empty.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="character"/> or <paramref name="style"/> is <c>null</c>.
    /// </exception>
    /// <remarks>
    /// Control characters are automatically transformed to display representations.
    /// Use <see cref="Create"/> for cached instances to improve memory efficiency.
    /// </remarks>
    public Char(string character = " ", string style = "")
        : this(character, style, skipCache: false)
    {
    }

    /// <summary>
    /// Internal constructor that optionally skips cache behavior to avoid infinite recursion.
    /// </summary>
    private Char(string character, string style, bool skipCache)
    {
        ArgumentNullException.ThrowIfNull(character);
        ArgumentNullException.ThrowIfNull(style);

        // Transform control characters if input is single char
        if (character.Length == 1)
        {
            char c = character[0];

            // C0 controls (0x00-0x1F) and DEL (0x7F) and C1 controls (0x80-0x9F)
            if (CharacterDisplayMappings.IsControlCharacter(c))
            {
                character = CharacterDisplayMappings.GetDisplayOrDefault(c);
                style = string.IsNullOrEmpty(style)
                    ? "class:control-character"
                    : $"class:control-character {style}";
            }
            // Non-breaking space (0xA0)
            else if (CharacterDisplayMappings.IsNonBreakingSpace(c))
            {
                character = " ";
                style = string.IsNullOrEmpty(style)
                    ? "class:nbsp"
                    : $"class:nbsp {style}";
            }
        }

        Character = character;
        Style = style;
        Width = UnicodeWidth.GetWidth(character);
    }

    /// <summary>
    /// Creates or retrieves a cached <see cref="Char"/> instance.
    /// </summary>
    /// <param name="character">The character string.</param>
    /// <param name="style">The style string.</param>
    /// <returns>A <see cref="Char"/> instance, potentially from cache.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="character"/> or <paramref name="style"/> is <c>null</c>.
    /// </exception>
    /// <remarks>
    /// Uses an internal cache of up to 1,000,000 entries for memory efficiency.
    /// Prefer this method over direct construction for frequently used characters.
    /// </remarks>
    public static Char Create(string character, string style)
    {
        ArgumentNullException.ThrowIfNull(character);
        ArgumentNullException.ThrowIfNull(style);

        return _cache[(character, style)];
    }

    /// <summary>
    /// Determines whether the specified <see cref="Char"/> is equal to this instance.
    /// </summary>
    /// <param name="other">The <see cref="Char"/> to compare.</param>
    /// <returns><c>true</c> if Character and Style are equal; otherwise, <c>false</c>.</returns>
    public bool Equals(Char? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Character == other.Character && Style == other.Style;
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        return obj is Char other && Equals(other);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return HashCode.Combine(Character, Style);
    }

    /// <summary>
    /// Returns a debug-friendly string representation.
    /// </summary>
    /// <returns>A string in the format <c>Char('{Character}', '{Style}')</c>.</returns>
    public override string ToString()
    {
        return $"Char('{Character}', '{Style}')";
    }

    /// <summary>
    /// Determines whether two <see cref="Char"/> instances are equal.
    /// </summary>
    public static bool operator ==(Char? left, Char? right)
    {
        if (left is null) return right is null;
        return left.Equals(right);
    }

    /// <summary>
    /// Determines whether two <see cref="Char"/> instances are not equal.
    /// </summary>
    public static bool operator !=(Char? left, Char? right)
    {
        return !(left == right);
    }
}
