using System.Collections.Frozen;

namespace Stroke.Layout;

/// <summary>
/// Provides mappings from control characters to their display representations.
/// </summary>
/// <remarks>
/// <para>
/// Contains 66 mappings covering:
/// <list type="bullet">
/// <item>C0 control characters 0x00-0x1F (32 entries) → caret notation (^@, ^A, ...)</item>
/// <item>Delete character 0x7F (1 entry) → ^?</item>
/// <item>C1 control characters 0x80-0x9F (32 entries) → hex notation (&lt;80&gt;, ...)</item>
/// <item>Non-breaking space 0xA0 (1 entry) → single space</item>
/// </list>
/// Total: 32 + 1 + 32 + 1 = 66 mappings.
/// </para>
/// <para>
/// Port of Python Prompt Toolkit's <c>Char.display_mappings</c> class attribute
/// from <c>layout/screen.py</c>.
/// </para>
/// </remarks>
public static class CharacterDisplayMappings
{
    /// <summary>
    /// Gets the complete mapping of characters to their display representations.
    /// </summary>
    /// <remarks>
    /// Returns a <see cref="FrozenDictionary{TKey, TValue}"/> for O(1) lookup performance.
    /// The dictionary is created once at static initialization and is immutable.
    /// </remarks>
    public static FrozenDictionary<char, string> Mappings { get; }

    static CharacterDisplayMappings()
    {
        var mappings = new Dictionary<char, string>(66);

        // C0 control characters (0x00-0x1F): caret notation
        // ^@ ^A ^B ^C ^D ^E ^F ^G ^H ^I ^J ^K ^L ^M ^N ^O ^P ^Q ^R ^S ^T ^U ^V ^W ^X ^Y ^Z ^[ ^\ ^] ^^ ^_
        for (int i = 0; i <= 0x1F; i++)
        {
            char displayChar = (char)('@' + i);
            mappings[(char)i] = $"^{displayChar}";
        }

        // DEL (0x7F): caret notation
        mappings[(char)0x7F] = "^?";

        // C1 control characters (0x80-0x9F): hex notation
        for (int i = 0x80; i <= 0x9F; i++)
        {
            mappings[(char)i] = $"<{i:x2}>";
        }

        // Non-breaking space (0xA0): display as regular space
        mappings[(char)0xA0] = " ";

        Mappings = mappings.ToFrozenDictionary();
    }

    /// <summary>
    /// Attempts to get the display representation for a character.
    /// </summary>
    /// <param name="c">The character to look up.</param>
    /// <param name="display">When this method returns, contains the display string if found.</param>
    /// <returns><c>true</c> if the character has a display mapping; otherwise, <c>false</c>.</returns>
    public static bool TryGetDisplay(char c, out string display)
    {
        return Mappings.TryGetValue(c, out display!);
    }

    /// <summary>
    /// Gets the display representation for a character, or returns the character itself.
    /// </summary>
    /// <param name="c">The character to look up.</param>
    /// <returns>The display representation if mapped; otherwise, the character as a string.</returns>
    public static string GetDisplayOrDefault(char c)
    {
        return Mappings.TryGetValue(c, out var display) ? display : c.ToString();
    }

    /// <summary>
    /// Determines whether a character has a display mapping (is a control character or NBSP).
    /// </summary>
    /// <param name="c">The character to check.</param>
    /// <returns><c>true</c> if the character has a mapping; otherwise, <c>false</c>.</returns>
    public static bool IsControlCharacter(char c)
    {
        // C0 controls: 0x00-0x1F
        // DEL: 0x7F
        // C1 controls: 0x80-0x9F
        // Note: This does NOT include NBSP (0xA0) since that's not technically a control character
        return c <= 0x1F || c == 0x7F || (c >= 0x80 && c <= 0x9F);
    }

    /// <summary>
    /// Determines whether a character is the non-breaking space (0xA0).
    /// </summary>
    /// <param name="c">The character to check.</param>
    /// <returns><c>true</c> if the character is non-breaking space.</returns>
    public static bool IsNonBreakingSpace(char c)
    {
        return c == '\xA0';
    }
}
