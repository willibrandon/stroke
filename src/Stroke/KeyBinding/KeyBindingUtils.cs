using Stroke.Input;

namespace Stroke.KeyBinding;

/// <summary>
/// Utility methods for key bindings.
/// </summary>
/// <remarks>
/// <para>
/// Equivalent to Python Prompt Toolkit's key binding utility functions.
/// </para>
/// </remarks>
public static class KeyBindingUtils
{
    /// <summary>
    /// Merges multiple key binding registries into one.
    /// </summary>
    /// <param name="registries">The registries to merge.</param>
    /// <returns>A merged key bindings view.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="registries"/> is null.</exception>
    public static IKeyBindingsBase Merge(IEnumerable<IKeyBindingsBase> registries)
    {
        ArgumentNullException.ThrowIfNull(registries);
        return new MergedKeyBindings(registries);
    }

    /// <summary>
    /// Merges multiple key binding registries into one.
    /// </summary>
    /// <param name="registries">The registries to merge.</param>
    /// <returns>A merged key bindings view.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="registries"/> is null.</exception>
    public static IKeyBindingsBase Merge(params IKeyBindingsBase[] registries)
    {
        ArgumentNullException.ThrowIfNull(registries);
        return new MergedKeyBindings(registries);
    }

    /// <summary>
    /// Parses a key name into a Keys enum value.
    /// </summary>
    /// <param name="keyName">
    /// The key name to parse. Supports aliases:
    /// <list type="bullet">
    /// <item><description>c-x → ControlX</description></item>
    /// <item><description>m-x → AltX (Meta key)</description></item>
    /// <item><description>s-x → ShiftX</description></item>
    /// <item><description>space, tab, enter → Keys.Space, Keys.ControlI, Keys.ControlM</description></item>
    /// </list>
    /// </param>
    /// <returns>The parsed Keys value.</returns>
    /// <exception cref="ArgumentException">Thrown when the key name is invalid.</exception>
    public static Keys ParseKey(string keyName)
    {
        ArgumentNullException.ThrowIfNull(keyName);

        var normalized = keyName.ToLowerInvariant().Trim();

        // Handle special names
        return normalized switch
        {
            "space" => Keys.ControlAt, // Space is Control-@ (NUL)
            "tab" => Keys.ControlI, // Tab is Ctrl+I
            "enter" or "return" => Keys.ControlM, // Enter is Ctrl+M
            "escape" or "esc" => Keys.Escape,
            "backspace" or "bs" => Keys.ControlH, // Backspace is Ctrl+H
            "delete" or "del" => Keys.Delete,
            "up" => Keys.Up,
            "down" => Keys.Down,
            "left" => Keys.Left,
            "right" => Keys.Right,
            "home" => Keys.Home,
            "end" => Keys.End,
            "pageup" or "pgup" => Keys.PageUp,
            "pagedown" or "pgdown" => Keys.PageDown,
            "insert" or "ins" => Keys.Insert,
            _ => ParseKeyWithModifiers(normalized)
        };
    }

    /// <summary>
    /// Parses a key name with optional modifiers (c-, m-, s-).
    /// </summary>
    private static Keys ParseKeyWithModifiers(string keyName)
    {
        // Handle c-x pattern (Control + key)
        if (keyName.StartsWith("c-") && keyName.Length == 3)
        {
            char c = char.ToUpperInvariant(keyName[2]);
            var enumName = $"Control{c}";
            if (Enum.TryParse<Keys>(enumName, out var controlKey))
            {
                return controlKey;
            }
            throw new ArgumentException($"Invalid control key: {keyName}", nameof(keyName));
        }

        // Handle m-x pattern (Meta/Alt + key)
        if (keyName.StartsWith("m-") && keyName.Length == 3)
        {
            char c = char.ToUpperInvariant(keyName[2]);
            var enumName = $"Escape{c}"; // Meta is typically Escape + char
            if (Enum.TryParse<Keys>(enumName, out var metaKey))
            {
                return metaKey;
            }
            throw new ArgumentException($"Invalid meta key: {keyName}", nameof(keyName));
        }

        // Handle s-x pattern (Shift + key)
        if (keyName.StartsWith("s-") && keyName.Length == 3)
        {
            char c = char.ToUpperInvariant(keyName[2]);
            var enumName = $"Shift{c}";
            if (Enum.TryParse<Keys>(enumName, out var shiftKey))
            {
                return shiftKey;
            }
            throw new ArgumentException($"Invalid shift key: {keyName}", nameof(keyName));
        }

        // Handle function keys (f1-f24)
        if (keyName.StartsWith("f") && int.TryParse(keyName.AsSpan(1), out int fNum) && fNum >= 1 && fNum <= 24)
        {
            if (Enum.TryParse<Keys>($"F{fNum}", out var fKey))
            {
                return fKey;
            }
        }

        // Try direct enum parse
        if (Enum.TryParse<Keys>(keyName, ignoreCase: true, out var directKey))
        {
            return directKey;
        }

        throw new ArgumentException($"Invalid key name: {keyName}", nameof(keyName));
    }
}
