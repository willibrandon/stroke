using System.Collections.Frozen;

namespace Stroke.Input;

/// <summary>
/// Extension methods for the <see cref="Keys"/> enum.
/// </summary>
/// <remarks>
/// <para>
/// This class provides string conversion functionality for the Keys enum,
/// replacing the Python StrEnum's inherent string conversion with explicit methods.
/// </para>
/// <para>
/// Thread safety: This class is thread-safe. All static dictionaries are readonly
/// and initialized via static constructor (thread-safe by CLR guarantee).
/// </para>
/// </remarks>
public static class KeysExtensions
{
    /// <summary>
    /// Mapping from Keys enum values to their canonical string representations.
    /// </summary>
    private static readonly FrozenDictionary<Keys, string> KeyStrings = new Dictionary<Keys, string>
    {
        // Escape keys (2)
        [Keys.Escape] = "escape",
        [Keys.ShiftEscape] = "s-escape",

        // Control characters (31)
        [Keys.ControlAt] = "c-@",
        [Keys.ControlA] = "c-a",
        [Keys.ControlB] = "c-b",
        [Keys.ControlC] = "c-c",
        [Keys.ControlD] = "c-d",
        [Keys.ControlE] = "c-e",
        [Keys.ControlF] = "c-f",
        [Keys.ControlG] = "c-g",
        [Keys.ControlH] = "c-h",
        [Keys.ControlI] = "c-i",
        [Keys.ControlJ] = "c-j",
        [Keys.ControlK] = "c-k",
        [Keys.ControlL] = "c-l",
        [Keys.ControlM] = "c-m",
        [Keys.ControlN] = "c-n",
        [Keys.ControlO] = "c-o",
        [Keys.ControlP] = "c-p",
        [Keys.ControlQ] = "c-q",
        [Keys.ControlR] = "c-r",
        [Keys.ControlS] = "c-s",
        [Keys.ControlT] = "c-t",
        [Keys.ControlU] = "c-u",
        [Keys.ControlV] = "c-v",
        [Keys.ControlW] = "c-w",
        [Keys.ControlX] = "c-x",
        [Keys.ControlY] = "c-y",
        [Keys.ControlZ] = "c-z",
        [Keys.ControlBackslash] = "c-\\",
        [Keys.ControlSquareClose] = "c-]",
        [Keys.ControlCircumflex] = "c-^",
        [Keys.ControlUnderscore] = "c-_",

        // Control + Numbers (10)
        [Keys.Control1] = "c-1",
        [Keys.Control2] = "c-2",
        [Keys.Control3] = "c-3",
        [Keys.Control4] = "c-4",
        [Keys.Control5] = "c-5",
        [Keys.Control6] = "c-6",
        [Keys.Control7] = "c-7",
        [Keys.Control8] = "c-8",
        [Keys.Control9] = "c-9",
        [Keys.Control0] = "c-0",

        // Control + Shift + Numbers (10)
        [Keys.ControlShift1] = "c-s-1",
        [Keys.ControlShift2] = "c-s-2",
        [Keys.ControlShift3] = "c-s-3",
        [Keys.ControlShift4] = "c-s-4",
        [Keys.ControlShift5] = "c-s-5",
        [Keys.ControlShift6] = "c-s-6",
        [Keys.ControlShift7] = "c-s-7",
        [Keys.ControlShift8] = "c-s-8",
        [Keys.ControlShift9] = "c-s-9",
        [Keys.ControlShift0] = "c-s-0",

        // Navigation keys (10)
        [Keys.Left] = "left",
        [Keys.Right] = "right",
        [Keys.Up] = "up",
        [Keys.Down] = "down",
        [Keys.Home] = "home",
        [Keys.End] = "end",
        [Keys.Insert] = "insert",
        [Keys.Delete] = "delete",
        [Keys.PageUp] = "pageup",
        [Keys.PageDown] = "pagedown",

        // Control + Navigation (10)
        [Keys.ControlLeft] = "c-left",
        [Keys.ControlRight] = "c-right",
        [Keys.ControlUp] = "c-up",
        [Keys.ControlDown] = "c-down",
        [Keys.ControlHome] = "c-home",
        [Keys.ControlEnd] = "c-end",
        [Keys.ControlInsert] = "c-insert",
        [Keys.ControlDelete] = "c-delete",
        [Keys.ControlPageUp] = "c-pageup",
        [Keys.ControlPageDown] = "c-pagedown",

        // Shift + Navigation (10)
        [Keys.ShiftLeft] = "s-left",
        [Keys.ShiftRight] = "s-right",
        [Keys.ShiftUp] = "s-up",
        [Keys.ShiftDown] = "s-down",
        [Keys.ShiftHome] = "s-home",
        [Keys.ShiftEnd] = "s-end",
        [Keys.ShiftInsert] = "s-insert",
        [Keys.ShiftDelete] = "s-delete",
        [Keys.ShiftPageUp] = "s-pageup",
        [Keys.ShiftPageDown] = "s-pagedown",

        // Control + Shift + Navigation (10)
        [Keys.ControlShiftLeft] = "c-s-left",
        [Keys.ControlShiftRight] = "c-s-right",
        [Keys.ControlShiftUp] = "c-s-up",
        [Keys.ControlShiftDown] = "c-s-down",
        [Keys.ControlShiftHome] = "c-s-home",
        [Keys.ControlShiftEnd] = "c-s-end",
        [Keys.ControlShiftInsert] = "c-s-insert",
        [Keys.ControlShiftDelete] = "c-s-delete",
        [Keys.ControlShiftPageUp] = "c-s-pageup",
        [Keys.ControlShiftPageDown] = "c-s-pagedown",

        // BackTab (1)
        [Keys.BackTab] = "s-tab",

        // Function keys (24)
        [Keys.F1] = "f1",
        [Keys.F2] = "f2",
        [Keys.F3] = "f3",
        [Keys.F4] = "f4",
        [Keys.F5] = "f5",
        [Keys.F6] = "f6",
        [Keys.F7] = "f7",
        [Keys.F8] = "f8",
        [Keys.F9] = "f9",
        [Keys.F10] = "f10",
        [Keys.F11] = "f11",
        [Keys.F12] = "f12",
        [Keys.F13] = "f13",
        [Keys.F14] = "f14",
        [Keys.F15] = "f15",
        [Keys.F16] = "f16",
        [Keys.F17] = "f17",
        [Keys.F18] = "f18",
        [Keys.F19] = "f19",
        [Keys.F20] = "f20",
        [Keys.F21] = "f21",
        [Keys.F22] = "f22",
        [Keys.F23] = "f23",
        [Keys.F24] = "f24",

        // Control + Function keys (24)
        [Keys.ControlF1] = "c-f1",
        [Keys.ControlF2] = "c-f2",
        [Keys.ControlF3] = "c-f3",
        [Keys.ControlF4] = "c-f4",
        [Keys.ControlF5] = "c-f5",
        [Keys.ControlF6] = "c-f6",
        [Keys.ControlF7] = "c-f7",
        [Keys.ControlF8] = "c-f8",
        [Keys.ControlF9] = "c-f9",
        [Keys.ControlF10] = "c-f10",
        [Keys.ControlF11] = "c-f11",
        [Keys.ControlF12] = "c-f12",
        [Keys.ControlF13] = "c-f13",
        [Keys.ControlF14] = "c-f14",
        [Keys.ControlF15] = "c-f15",
        [Keys.ControlF16] = "c-f16",
        [Keys.ControlF17] = "c-f17",
        [Keys.ControlF18] = "c-f18",
        [Keys.ControlF19] = "c-f19",
        [Keys.ControlF20] = "c-f20",
        [Keys.ControlF21] = "c-f21",
        [Keys.ControlF22] = "c-f22",
        [Keys.ControlF23] = "c-f23",
        [Keys.ControlF24] = "c-f24",

        // Special keys (9)
        [Keys.Any] = "<any>",
        [Keys.ScrollUp] = "<scroll-up>",
        [Keys.ScrollDown] = "<scroll-down>",
        [Keys.CPRResponse] = "<cursor-position-response>",
        [Keys.Vt100MouseEvent] = "<vt100-mouse-event>",
        [Keys.WindowsMouseEvent] = "<windows-mouse-event>",
        [Keys.BracketedPaste] = "<bracketed-paste>",
        [Keys.SIGINT] = "<sigint>",
        [Keys.Ignore] = "<ignore>",
    }.ToFrozenDictionary();

    /// <summary>
    /// Mapping from canonical key strings to Keys enum values (case-insensitive).
    /// </summary>
    private static readonly FrozenDictionary<string, Keys> StringToKey;

    /// <summary>
    /// Static constructor to initialize the reverse lookup dictionary.
    /// </summary>
    static KeysExtensions()
    {
        var stringToKey = new Dictionary<string, Keys>(StringComparer.OrdinalIgnoreCase);

        // Add all canonical key strings
        foreach (var (key, value) in KeyStrings)
        {
            stringToKey[value] = key;
        }

        // Add alias mappings (from Python's KEY_ALIASES)
        stringToKey["backspace"] = Keys.ControlH;
        stringToKey["c-space"] = Keys.ControlAt;
        stringToKey["enter"] = Keys.ControlM;
        stringToKey["tab"] = Keys.ControlI;
        stringToKey["s-c-left"] = Keys.ControlShiftLeft;
        stringToKey["s-c-right"] = Keys.ControlShiftRight;
        stringToKey["s-c-home"] = Keys.ControlShiftHome;
        stringToKey["s-c-end"] = Keys.ControlShiftEnd;

        StringToKey = stringToKey.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Converts a <see cref="Keys"/> value to its canonical string representation.
    /// </summary>
    /// <param name="key">The key value to convert.</param>
    /// <returns>The canonical string representation of the key (e.g., "c-a", "escape", "&lt;any&gt;").</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the key value is not a valid <see cref="Keys"/> enum value.
    /// </exception>
    /// <example>
    /// <code>
    /// Keys.ControlA.ToKeyString();  // Returns "c-a"
    /// Keys.Escape.ToKeyString();    // Returns "escape"
    /// Keys.Any.ToKeyString();       // Returns "&lt;any&gt;"
    /// </code>
    /// </example>
    public static string ToKeyString(this Keys key)
    {
        if (KeyStrings.TryGetValue(key, out var result))
        {
            return result;
        }

        throw new ArgumentOutOfRangeException(nameof(key), key, $"Unknown Keys value: {key}");
    }

    /// <summary>
    /// Parses a key string into its corresponding <see cref="Keys"/> enum value.
    /// </summary>
    /// <param name="keyString">The key string to parse (e.g., "c-a", "escape", "enter").</param>
    /// <returns>
    /// The corresponding <see cref="Keys"/> value, or <c>null</c> if the string is not a valid key.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Parsing is case-insensitive. Both canonical strings (e.g., "c-a") and
    /// alias strings (e.g., "enter", "tab", "backspace") are supported.
    /// </para>
    /// <para>
    /// Alias strings are resolved to their canonical Keys values:
    /// <list type="bullet">
    /// <item><description>"enter" → Keys.ControlM</description></item>
    /// <item><description>"tab" → Keys.ControlI</description></item>
    /// <item><description>"backspace" → Keys.ControlH</description></item>
    /// <item><description>"c-space" → Keys.ControlAt</description></item>
    /// <item><description>"s-c-left" → Keys.ControlShiftLeft (modifier order normalization)</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// KeysExtensions.ParseKey("c-a");       // Returns Keys.ControlA
    /// KeysExtensions.ParseKey("enter");     // Returns Keys.ControlM
    /// KeysExtensions.ParseKey("ESCAPE");    // Returns Keys.Escape (case-insensitive)
    /// KeysExtensions.ParseKey("invalid");   // Returns null
    /// </code>
    /// </example>
    public static Keys? ParseKey(string? keyString)
    {
        if (string.IsNullOrEmpty(keyString))
        {
            return null;
        }

        if (StringToKey.TryGetValue(keyString, out var result))
        {
            return result;
        }

        return null;
    }
}
