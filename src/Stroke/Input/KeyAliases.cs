// Copyright (c) 2025 Brandon Pugh. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Stroke.Input;

/// <summary>
/// Provides readable alias constants for common keys.
/// </summary>
/// <remarks>
/// <para>
/// This class provides human-readable names for keys that are commonly
/// referred to by their function rather than their control character equivalent.
/// For example, <see cref="Tab"/> is more readable than <see cref="Keys.ControlI"/>.
/// </para>
/// <para>
/// All aliases resolve to their underlying <see cref="Keys"/> enum values
/// and can be used interchangeably with the canonical enum values.
/// </para>
/// <para>
/// Thread safety: This class is thread-safe. All fields are static readonly
/// and initialized at class load time.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // These are equivalent:
/// Keys tab1 = Keys.ControlI;
/// Keys tab2 = KeyAliases.Tab;
/// Assert.Equal(tab1, tab2);  // true
///
/// // More readable code:
/// keyBindings.Add(KeyAliases.Enter, handler);
/// keyBindings.Add(KeyAliases.Tab, handler);
/// keyBindings.Add(KeyAliases.Backspace, handler);
/// </code>
/// </example>
public static class KeyAliases
{
    /// <summary>
    /// The Tab key (Control-I / horizontal tab).
    /// </summary>
    /// <remarks>
    /// Alias for <see cref="Keys.ControlI"/>.
    /// Key string representation: "c-i".
    /// </remarks>
    public static readonly Keys Tab = Keys.ControlI;

    /// <summary>
    /// The Enter/Return key (Control-M / carriage return).
    /// </summary>
    /// <remarks>
    /// Alias for <see cref="Keys.ControlM"/>.
    /// Key string representation: "c-m".
    /// </remarks>
    public static readonly Keys Enter = Keys.ControlM;

    /// <summary>
    /// The Backspace key (Control-H).
    /// </summary>
    /// <remarks>
    /// Alias for <see cref="Keys.ControlH"/>.
    /// Key string representation: "c-h".
    /// </remarks>
    public static readonly Keys Backspace = Keys.ControlH;

    /// <summary>
    /// The Control-Space key combination (Control-@).
    /// </summary>
    /// <remarks>
    /// Alias for <see cref="Keys.ControlAt"/>.
    /// Key string representation: "c-@".
    /// </remarks>
    public static readonly Keys ControlSpace = Keys.ControlAt;

    // Backwards-compatibility aliases
    // ShiftControl was renamed to ControlShift in Python Prompt Toolkit
    // (commit 888fcb6fa4efea0de8333177e1bbc792f3ff3c24, 20 Feb 2020)

    /// <summary>
    /// Backwards-compatibility alias for <see cref="Keys.ControlShiftLeft"/>.
    /// </summary>
    /// <remarks>
    /// Use <see cref="Keys.ControlShiftLeft"/> for new code.
    /// </remarks>
    public static readonly Keys ShiftControlLeft = Keys.ControlShiftLeft;

    /// <summary>
    /// Backwards-compatibility alias for <see cref="Keys.ControlShiftRight"/>.
    /// </summary>
    /// <remarks>
    /// Use <see cref="Keys.ControlShiftRight"/> for new code.
    /// </remarks>
    public static readonly Keys ShiftControlRight = Keys.ControlShiftRight;

    /// <summary>
    /// Backwards-compatibility alias for <see cref="Keys.ControlShiftHome"/>.
    /// </summary>
    /// <remarks>
    /// Use <see cref="Keys.ControlShiftHome"/> for new code.
    /// </remarks>
    public static readonly Keys ShiftControlHome = Keys.ControlShiftHome;

    /// <summary>
    /// Backwards-compatibility alias for <see cref="Keys.ControlShiftEnd"/>.
    /// </summary>
    /// <remarks>
    /// Use <see cref="Keys.ControlShiftEnd"/> for new code.
    /// </remarks>
    public static readonly Keys ShiftControlEnd = Keys.ControlShiftEnd;
}
