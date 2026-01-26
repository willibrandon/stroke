using System.Collections.Frozen;

namespace Stroke.Input.Vt100;

/// <summary>
/// Static dictionary of ANSI escape sequences to key mappings.
/// </summary>
/// <remarks>
/// <para>
/// This class provides optimized lookups for VT100/ANSI escape sequence parsing.
/// Uses <see cref="FrozenDictionary{TKey, TValue}"/> for O(1) read-heavy workloads.
/// This is a faithful port of Python Prompt Toolkit's ANSI_SEQUENCES from
/// <c>prompt_toolkit.input.vt100_parser</c>.
/// </para>
/// <para>
/// Thread safety: All members are read-only and inherently thread-safe.
/// </para>
/// </remarks>
public static class AnsiSequences
{
    /// <summary>
    /// Mapping from escape sequences to <see cref="Keys"/> enum values.
    /// </summary>
    public static FrozenDictionary<string, Keys> Sequences { get; } = BuildSequences();

    /// <summary>
    /// Reverse mapping from <see cref="Keys"/> to escape sequences.
    /// </summary>
    public static FrozenDictionary<Keys, string> ReverseSequences { get; } = BuildReverseSequences();

    /// <summary>
    /// Set of valid prefixes that could match longer sequences.
    /// </summary>
    /// <remarks>
    /// Used by the parser to determine whether to wait for more input
    /// or emit the current buffer.
    /// </remarks>
    public static FrozenSet<string> ValidPrefixes { get; } = BuildValidPrefixes();

    private static FrozenDictionary<string, Keys> BuildSequences()
    {
        var dict = new Dictionary<string, Keys>
        {
            // Navigation Keys
            ["\x1b[A"] = Keys.Up,
            ["\x1b[B"] = Keys.Down,
            ["\x1b[C"] = Keys.Right,
            ["\x1b[D"] = Keys.Left,
            ["\x1b[H"] = Keys.Home,
            ["\x1b[F"] = Keys.End,
            ["\x1b[2~"] = Keys.Insert,
            ["\x1b[3~"] = Keys.Delete,
            ["\x1b[5~"] = Keys.PageUp,
            ["\x1b[6~"] = Keys.PageDown,

            // Alternative Home/End sequences (some terminals)
            ["\x1b[1~"] = Keys.Home,
            ["\x1b[4~"] = Keys.End,
            ["\x1bOH"] = Keys.Home,
            ["\x1bOF"] = Keys.End,

            // Control + Navigation Keys (modifier 5 = Ctrl)
            ["\x1b[1;5A"] = Keys.ControlUp,
            ["\x1b[1;5B"] = Keys.ControlDown,
            ["\x1b[1;5C"] = Keys.ControlRight,
            ["\x1b[1;5D"] = Keys.ControlLeft,
            ["\x1b[1;5H"] = Keys.ControlHome,
            ["\x1b[1;5F"] = Keys.ControlEnd,
            ["\x1b[2;5~"] = Keys.ControlInsert,
            ["\x1b[3;5~"] = Keys.ControlDelete,
            ["\x1b[5;5~"] = Keys.ControlPageUp,
            ["\x1b[6;5~"] = Keys.ControlPageDown,

            // Shift + Navigation Keys (modifier 2 = Shift)
            ["\x1b[1;2A"] = Keys.ShiftUp,
            ["\x1b[1;2B"] = Keys.ShiftDown,
            ["\x1b[1;2C"] = Keys.ShiftRight,
            ["\x1b[1;2D"] = Keys.ShiftLeft,
            ["\x1b[1;2H"] = Keys.ShiftHome,
            ["\x1b[1;2F"] = Keys.ShiftEnd,
            ["\x1b[2;2~"] = Keys.ShiftInsert,
            ["\x1b[3;2~"] = Keys.ShiftDelete,
            ["\x1b[5;2~"] = Keys.ShiftPageUp,
            ["\x1b[6;2~"] = Keys.ShiftPageDown,

            // Control + Shift + Navigation Keys (modifier 6 = Ctrl+Shift)
            ["\x1b[1;6A"] = Keys.ControlShiftUp,
            ["\x1b[1;6B"] = Keys.ControlShiftDown,
            ["\x1b[1;6C"] = Keys.ControlShiftRight,
            ["\x1b[1;6D"] = Keys.ControlShiftLeft,
            ["\x1b[1;6H"] = Keys.ControlShiftHome,
            ["\x1b[1;6F"] = Keys.ControlShiftEnd,
            ["\x1b[2;6~"] = Keys.ControlShiftInsert,
            ["\x1b[3;6~"] = Keys.ControlShiftDelete,
            ["\x1b[5;6~"] = Keys.ControlShiftPageUp,
            ["\x1b[6;6~"] = Keys.ControlShiftPageDown,

            // BackTab (Shift+Tab)
            ["\x1b[Z"] = Keys.BackTab,

            // Function Keys F1-F4 (SS3 sequences)
            ["\x1bOP"] = Keys.F1,
            ["\x1bOQ"] = Keys.F2,
            ["\x1bOR"] = Keys.F3,
            ["\x1bOS"] = Keys.F4,

            // Function Keys F1-F4 (alternative CSI sequences for some terminals)
            ["\x1b[11~"] = Keys.F1,
            ["\x1b[12~"] = Keys.F2,
            ["\x1b[13~"] = Keys.F3,
            ["\x1b[14~"] = Keys.F4,

            // Function Keys F5-F20
            ["\x1b[15~"] = Keys.F5,
            ["\x1b[17~"] = Keys.F6,
            ["\x1b[18~"] = Keys.F7,
            ["\x1b[19~"] = Keys.F8,
            ["\x1b[20~"] = Keys.F9,
            ["\x1b[21~"] = Keys.F10,
            ["\x1b[23~"] = Keys.F11,
            ["\x1b[24~"] = Keys.F12,
            ["\x1b[25~"] = Keys.F13,
            ["\x1b[26~"] = Keys.F14,
            ["\x1b[28~"] = Keys.F15,
            ["\x1b[29~"] = Keys.F16,
            ["\x1b[31~"] = Keys.F17,
            ["\x1b[32~"] = Keys.F18,
            ["\x1b[33~"] = Keys.F19,
            ["\x1b[34~"] = Keys.F20,

            // Control + Function Keys F1-F4 (with modifier)
            ["\x1b[1;5P"] = Keys.ControlF1,
            ["\x1b[1;5Q"] = Keys.ControlF2,
            ["\x1b[1;5R"] = Keys.ControlF3,
            ["\x1b[1;5S"] = Keys.ControlF4,

            // Control + Function Keys F5-F12
            ["\x1b[15;5~"] = Keys.ControlF5,
            ["\x1b[17;5~"] = Keys.ControlF6,
            ["\x1b[18;5~"] = Keys.ControlF7,
            ["\x1b[19;5~"] = Keys.ControlF8,
            ["\x1b[20;5~"] = Keys.ControlF9,
            ["\x1b[21;5~"] = Keys.ControlF10,
            ["\x1b[23;5~"] = Keys.ControlF11,
            ["\x1b[24;5~"] = Keys.ControlF12,

            // Shift + Function Keys F1-F4 (with modifier 2)
            ["\x1b[1;2P"] = Keys.F13, // Shift+F1 maps to F13 in many terminals
            ["\x1b[1;2Q"] = Keys.F14, // Shift+F2 maps to F14
            ["\x1b[1;2R"] = Keys.F15, // Shift+F3 maps to F15
            ["\x1b[1;2S"] = Keys.F16, // Shift+F4 maps to F16

            // Bracketed paste mode markers
            ["\x1b[200~"] = Keys.BracketedPaste, // Start
            ["\x1b[201~"] = Keys.BracketedPaste, // End (handled specially by parser)

            // Cursor Position Report
            // Note: CPR has variable content like \x1b[24;80R - handled specially by parser
        };

        return dict.ToFrozenDictionary();
    }

    private static FrozenDictionary<Keys, string> BuildReverseSequences()
    {
        var dict = new Dictionary<Keys, string>
        {
            // Navigation Keys (primary sequences only)
            [Keys.Up] = "\x1b[A",
            [Keys.Down] = "\x1b[B",
            [Keys.Right] = "\x1b[C",
            [Keys.Left] = "\x1b[D",
            [Keys.Home] = "\x1b[H",
            [Keys.End] = "\x1b[F",
            [Keys.Insert] = "\x1b[2~",
            [Keys.Delete] = "\x1b[3~",
            [Keys.PageUp] = "\x1b[5~",
            [Keys.PageDown] = "\x1b[6~",

            // Control + Navigation Keys
            [Keys.ControlUp] = "\x1b[1;5A",
            [Keys.ControlDown] = "\x1b[1;5B",
            [Keys.ControlRight] = "\x1b[1;5C",
            [Keys.ControlLeft] = "\x1b[1;5D",
            [Keys.ControlHome] = "\x1b[1;5H",
            [Keys.ControlEnd] = "\x1b[1;5F",
            [Keys.ControlInsert] = "\x1b[2;5~",
            [Keys.ControlDelete] = "\x1b[3;5~",
            [Keys.ControlPageUp] = "\x1b[5;5~",
            [Keys.ControlPageDown] = "\x1b[6;5~",

            // Shift + Navigation Keys
            [Keys.ShiftUp] = "\x1b[1;2A",
            [Keys.ShiftDown] = "\x1b[1;2B",
            [Keys.ShiftRight] = "\x1b[1;2C",
            [Keys.ShiftLeft] = "\x1b[1;2D",
            [Keys.ShiftHome] = "\x1b[1;2H",
            [Keys.ShiftEnd] = "\x1b[1;2F",
            [Keys.ShiftInsert] = "\x1b[2;2~",
            [Keys.ShiftDelete] = "\x1b[3;2~",
            [Keys.ShiftPageUp] = "\x1b[5;2~",
            [Keys.ShiftPageDown] = "\x1b[6;2~",

            // Control + Shift + Navigation Keys
            [Keys.ControlShiftUp] = "\x1b[1;6A",
            [Keys.ControlShiftDown] = "\x1b[1;6B",
            [Keys.ControlShiftRight] = "\x1b[1;6C",
            [Keys.ControlShiftLeft] = "\x1b[1;6D",
            [Keys.ControlShiftHome] = "\x1b[1;6H",
            [Keys.ControlShiftEnd] = "\x1b[1;6F",
            [Keys.ControlShiftInsert] = "\x1b[2;6~",
            [Keys.ControlShiftDelete] = "\x1b[3;6~",
            [Keys.ControlShiftPageUp] = "\x1b[5;6~",
            [Keys.ControlShiftPageDown] = "\x1b[6;6~",

            // BackTab
            [Keys.BackTab] = "\x1b[Z",

            // Function Keys
            [Keys.F1] = "\x1bOP",
            [Keys.F2] = "\x1bOQ",
            [Keys.F3] = "\x1bOR",
            [Keys.F4] = "\x1bOS",
            [Keys.F5] = "\x1b[15~",
            [Keys.F6] = "\x1b[17~",
            [Keys.F7] = "\x1b[18~",
            [Keys.F8] = "\x1b[19~",
            [Keys.F9] = "\x1b[20~",
            [Keys.F10] = "\x1b[21~",
            [Keys.F11] = "\x1b[23~",
            [Keys.F12] = "\x1b[24~",
            [Keys.F13] = "\x1b[25~",
            [Keys.F14] = "\x1b[26~",
            [Keys.F15] = "\x1b[28~",
            [Keys.F16] = "\x1b[29~",
            [Keys.F17] = "\x1b[31~",
            [Keys.F18] = "\x1b[32~",
            [Keys.F19] = "\x1b[33~",
            [Keys.F20] = "\x1b[34~",

            // Control + Function Keys
            [Keys.ControlF1] = "\x1b[1;5P",
            [Keys.ControlF2] = "\x1b[1;5Q",
            [Keys.ControlF3] = "\x1b[1;5R",
            [Keys.ControlF4] = "\x1b[1;5S",
            [Keys.ControlF5] = "\x1b[15;5~",
            [Keys.ControlF6] = "\x1b[17;5~",
            [Keys.ControlF7] = "\x1b[18;5~",
            [Keys.ControlF8] = "\x1b[19;5~",
            [Keys.ControlF9] = "\x1b[20;5~",
            [Keys.ControlF10] = "\x1b[21;5~",
            [Keys.ControlF11] = "\x1b[23;5~",
            [Keys.ControlF12] = "\x1b[24;5~",

            // Escape
            [Keys.Escape] = "\x1b",

            // Control characters
            [Keys.ControlAt] = "\x00",
            [Keys.ControlA] = "\x01",
            [Keys.ControlB] = "\x02",
            [Keys.ControlC] = "\x03",
            [Keys.ControlD] = "\x04",
            [Keys.ControlE] = "\x05",
            [Keys.ControlF] = "\x06",
            [Keys.ControlG] = "\x07",
            [Keys.ControlH] = "\x08",
            [Keys.ControlI] = "\x09",
            [Keys.ControlJ] = "\x0a",
            [Keys.ControlK] = "\x0b",
            [Keys.ControlL] = "\x0c",
            [Keys.ControlM] = "\x0d",
            [Keys.ControlN] = "\x0e",
            [Keys.ControlO] = "\x0f",
            [Keys.ControlP] = "\x10",
            [Keys.ControlQ] = "\x11",
            [Keys.ControlR] = "\x12",
            [Keys.ControlS] = "\x13",
            [Keys.ControlT] = "\x14",
            [Keys.ControlU] = "\x15",
            [Keys.ControlV] = "\x16",
            [Keys.ControlW] = "\x17",
            [Keys.ControlX] = "\x18",
            [Keys.ControlY] = "\x19",
            [Keys.ControlZ] = "\x1a",
            [Keys.ControlBackslash] = "\x1c",
            [Keys.ControlSquareClose] = "\x1d",
            [Keys.ControlCircumflex] = "\x1e",
            [Keys.ControlUnderscore] = "\x1f",
        };

        return dict.ToFrozenDictionary();
    }

    private static FrozenSet<string> BuildValidPrefixes()
    {
        var prefixes = new HashSet<string>();

        // For each sequence, add all its prefixes (excluding the full sequence)
        foreach (var sequence in Sequences.Keys)
        {
            for (int i = 1; i < sequence.Length; i++)
            {
                prefixes.Add(sequence[..i]);
            }
        }

        // Add known prefix patterns for variable-length sequences
        // Mouse events: \x1b[M... (X10) or \x1b[<... (SGR)
        prefixes.Add("\x1b[M");
        prefixes.Add("\x1b[<");

        // CPR responses: \x1b[n;mR where n,m are digits
        // Add prefixes for CPR pattern
        for (int row = 0; row <= 9; row++)
        {
            prefixes.Add($"\x1b[{row}");
            for (int col = 0; col <= 9; col++)
            {
                prefixes.Add($"\x1b[{row};");
                prefixes.Add($"\x1b[{row};{col}");
            }
        }

        return prefixes.ToFrozenSet();
    }

    /// <summary>
    /// Checks if a sequence is a valid prefix of a longer known sequence.
    /// </summary>
    /// <param name="sequence">The sequence to check.</param>
    /// <returns>True if the sequence is a prefix of a known longer sequence.</returns>
    public static bool IsPrefixOfLongerSequence(string sequence)
    {
        return ValidPrefixes.Contains(sequence);
    }

    /// <summary>
    /// Tries to get the key for a given escape sequence.
    /// </summary>
    /// <param name="sequence">The escape sequence.</param>
    /// <param name="key">The key if found.</param>
    /// <returns>True if the sequence maps to a key.</returns>
    public static bool TryGetKey(string sequence, out Keys key)
    {
        return Sequences.TryGetValue(sequence, out key);
    }

    /// <summary>
    /// Tries to get the escape sequence for a given key.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="sequence">The escape sequence if found.</param>
    /// <returns>True if the key has an escape sequence.</returns>
    public static bool TryGetSequence(Keys key, out string? sequence)
    {
        return ReverseSequences.TryGetValue(key, out sequence);
    }
}
