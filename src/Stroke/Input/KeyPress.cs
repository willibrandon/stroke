namespace Stroke.Input;

/// <summary>
/// Represents a single key press event.
/// </summary>
/// <remarks>
/// <para>
/// A KeyPress contains both the logical key identity (which key was pressed) and the raw
/// input data (the bytes/characters received from the terminal). This is a faithful port
/// of Python Prompt Toolkit's <c>prompt_toolkit.key_binding.KeyPress</c>.
/// </para>
/// <para>
/// Thread safety: This type is immutable and inherently thread-safe.
/// </para>
/// <para>
/// Equality semantics: As a <c>record struct</c>, KeyPress uses value equality comparing
/// both <see cref="Key"/> and <see cref="Data"/> properties. Data comparison is ordinal
/// (case-sensitive, culture-invariant).
/// </para>
/// </remarks>
/// <param name="Key">The logical key identity.</param>
/// <param name="Data">The raw input data received from the terminal. If null, defaults to the key's string representation.</param>
public readonly record struct KeyPress(Keys Key, string? Data = null)
{
    /// <summary>
    /// Gets the raw input data received from the terminal.
    /// </summary>
    /// <remarks>
    /// Never null. If not explicitly provided, defaults to the key's default data representation.
    /// </remarks>
    public string Data { get; } = Data ?? GetDefaultData(Key);

    /// <summary>
    /// Gets the default data string for a given key.
    /// </summary>
    /// <param name="key">The key to get default data for.</param>
    /// <returns>The default data string for the key.</returns>
    private static string GetDefaultData(Keys key) => key switch
    {
        // Escape Keys
        Keys.Escape => "\x1b",
        Keys.ShiftEscape => "\x1b",

        // Control Characters (ASCII control codes)
        Keys.ControlAt => "\x00",
        Keys.ControlA => "\x01",
        Keys.ControlB => "\x02",
        Keys.ControlC => "\x03",
        Keys.ControlD => "\x04",
        Keys.ControlE => "\x05",
        Keys.ControlF => "\x06",
        Keys.ControlG => "\x07",
        Keys.ControlH => "\x08",
        Keys.ControlI => "\x09",
        Keys.ControlJ => "\x0a",
        Keys.ControlK => "\x0b",
        Keys.ControlL => "\x0c",
        Keys.ControlM => "\x0d",
        Keys.ControlN => "\x0e",
        Keys.ControlO => "\x0f",
        Keys.ControlP => "\x10",
        Keys.ControlQ => "\x11",
        Keys.ControlR => "\x12",
        Keys.ControlS => "\x13",
        Keys.ControlT => "\x14",
        Keys.ControlU => "\x15",
        Keys.ControlV => "\x16",
        Keys.ControlW => "\x17",
        Keys.ControlX => "\x18",
        Keys.ControlY => "\x19",
        Keys.ControlZ => "\x1a",
        Keys.ControlBackslash => "\x1c",
        Keys.ControlSquareClose => "\x1d",
        Keys.ControlCircumflex => "\x1e",
        Keys.ControlUnderscore => "\x1f",

        // Control + Numbers (no standard ASCII mapping)
        Keys.Control0 => "Control0",
        Keys.Control1 => "Control1",
        Keys.Control2 => "Control2",
        Keys.Control3 => "Control3",
        Keys.Control4 => "Control4",
        Keys.Control5 => "Control5",
        Keys.Control6 => "Control6",
        Keys.Control7 => "Control7",
        Keys.Control8 => "Control8",
        Keys.Control9 => "Control9",

        // Control + Shift + Numbers
        Keys.ControlShift0 => "ControlShift0",
        Keys.ControlShift1 => "ControlShift1",
        Keys.ControlShift2 => "ControlShift2",
        Keys.ControlShift3 => "ControlShift3",
        Keys.ControlShift4 => "ControlShift4",
        Keys.ControlShift5 => "ControlShift5",
        Keys.ControlShift6 => "ControlShift6",
        Keys.ControlShift7 => "ControlShift7",
        Keys.ControlShift8 => "ControlShift8",
        Keys.ControlShift9 => "ControlShift9",

        // Navigation Keys (VT100 sequences)
        Keys.Up => "\x1b[A",
        Keys.Down => "\x1b[B",
        Keys.Right => "\x1b[C",
        Keys.Left => "\x1b[D",
        Keys.Home => "\x1b[H",
        Keys.End => "\x1b[F",
        Keys.Insert => "\x1b[2~",
        Keys.Delete => "\x1b[3~",
        Keys.PageUp => "\x1b[5~",
        Keys.PageDown => "\x1b[6~",

        // Control + Navigation Keys
        Keys.ControlUp => "\x1b[1;5A",
        Keys.ControlDown => "\x1b[1;5B",
        Keys.ControlRight => "\x1b[1;5C",
        Keys.ControlLeft => "\x1b[1;5D",
        Keys.ControlHome => "\x1b[1;5H",
        Keys.ControlEnd => "\x1b[1;5F",
        Keys.ControlInsert => "\x1b[2;5~",
        Keys.ControlDelete => "\x1b[3;5~",
        Keys.ControlPageUp => "\x1b[5;5~",
        Keys.ControlPageDown => "\x1b[6;5~",

        // Shift + Navigation Keys
        Keys.ShiftUp => "\x1b[1;2A",
        Keys.ShiftDown => "\x1b[1;2B",
        Keys.ShiftRight => "\x1b[1;2C",
        Keys.ShiftLeft => "\x1b[1;2D",
        Keys.ShiftHome => "\x1b[1;2H",
        Keys.ShiftEnd => "\x1b[1;2F",
        Keys.ShiftInsert => "\x1b[2;2~",
        Keys.ShiftDelete => "\x1b[3;2~",
        Keys.ShiftPageUp => "\x1b[5;2~",
        Keys.ShiftPageDown => "\x1b[6;2~",

        // Control + Shift + Navigation Keys
        Keys.ControlShiftUp => "\x1b[1;6A",
        Keys.ControlShiftDown => "\x1b[1;6B",
        Keys.ControlShiftRight => "\x1b[1;6C",
        Keys.ControlShiftLeft => "\x1b[1;6D",
        Keys.ControlShiftHome => "\x1b[1;6H",
        Keys.ControlShiftEnd => "\x1b[1;6F",
        Keys.ControlShiftInsert => "\x1b[2;6~",
        Keys.ControlShiftDelete => "\x1b[3;6~",
        Keys.ControlShiftPageUp => "\x1b[5;6~",
        Keys.ControlShiftPageDown => "\x1b[6;6~",

        // Tab
        Keys.BackTab => "\x1b[Z",

        // Function Keys F1-F20 (VT100 sequences)
        Keys.F1 => "\x1bOP",
        Keys.F2 => "\x1bOQ",
        Keys.F3 => "\x1bOR",
        Keys.F4 => "\x1bOS",
        Keys.F5 => "\x1b[15~",
        Keys.F6 => "\x1b[17~",
        Keys.F7 => "\x1b[18~",
        Keys.F8 => "\x1b[19~",
        Keys.F9 => "\x1b[20~",
        Keys.F10 => "\x1b[21~",
        Keys.F11 => "\x1b[23~",
        Keys.F12 => "\x1b[24~",
        Keys.F13 => "\x1b[25~",
        Keys.F14 => "\x1b[26~",
        Keys.F15 => "\x1b[28~",
        Keys.F16 => "\x1b[29~",
        Keys.F17 => "\x1b[31~",
        Keys.F18 => "\x1b[32~",
        Keys.F19 => "\x1b[33~",
        Keys.F20 => "\x1b[34~",
        Keys.F21 => "F21",
        Keys.F22 => "F22",
        Keys.F23 => "F23",
        Keys.F24 => "F24",

        // Control + Function Keys F1-F12 (VT100 sequences with modifier)
        Keys.ControlF1 => "\x1b[1;5P",
        Keys.ControlF2 => "\x1b[1;5Q",
        Keys.ControlF3 => "\x1b[1;5R",
        Keys.ControlF4 => "\x1b[1;5S",
        Keys.ControlF5 => "\x1b[15;5~",
        Keys.ControlF6 => "\x1b[17;5~",
        Keys.ControlF7 => "\x1b[18;5~",
        Keys.ControlF8 => "\x1b[19;5~",
        Keys.ControlF9 => "\x1b[20;5~",
        Keys.ControlF10 => "\x1b[21;5~",
        Keys.ControlF11 => "\x1b[23;5~",
        Keys.ControlF12 => "\x1b[24;5~",
        Keys.ControlF13 => "ControlF13",
        Keys.ControlF14 => "ControlF14",
        Keys.ControlF15 => "ControlF15",
        Keys.ControlF16 => "ControlF16",
        Keys.ControlF17 => "ControlF17",
        Keys.ControlF18 => "ControlF18",
        Keys.ControlF19 => "ControlF19",
        Keys.ControlF20 => "ControlF20",
        Keys.ControlF21 => "ControlF21",
        Keys.ControlF22 => "ControlF22",
        Keys.ControlF23 => "ControlF23",
        Keys.ControlF24 => "ControlF24",

        // Special Keys (actual data should be in Data property when parsed)
        Keys.Any => "",
        Keys.ScrollUp => "ScrollUp",
        Keys.ScrollDown => "ScrollDown",
        Keys.CPRResponse => "CPRResponse",
        Keys.Vt100MouseEvent => "Vt100MouseEvent",
        Keys.WindowsMouseEvent => "WindowsMouseEvent",
        Keys.BracketedPaste => "BracketedPaste",
        Keys.SIGINT => "SIGINT",
        Keys.Ignore => "Ignore",

        // Default: use key name
        _ => key.ToString()
    };
}
