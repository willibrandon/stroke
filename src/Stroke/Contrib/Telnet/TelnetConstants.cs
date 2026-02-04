namespace Stroke.Contrib.Telnet;

/// <summary>
/// Telnet protocol constants.
/// </summary>
/// <remarks>
/// <para>
/// This is a faithful port of the protocol constants from Python Prompt Toolkit's
/// <c>prompt_toolkit.contrib.telnet.protocol</c> module.
/// </para>
/// <para>
/// These constants define the byte values used in the telnet protocol for
/// commands, options, and subnegotiation.
/// </para>
/// </remarks>
public static class TelnetConstants
{
    #region Commands

    /// <summary>No operation.</summary>
    public const byte NOP = 0;

    /// <summary>Suppress Go-Ahead option.</summary>
    public const byte SGA = 3;

    /// <summary>Interpret As Command - escape byte.</summary>
    public const byte IAC = 255;

    /// <summary>Request the other party to perform option.</summary>
    public const byte DO = 253;

    /// <summary>Refuse to perform option.</summary>
    public const byte DONT = 254;

    /// <summary>Agree to perform option.</summary>
    public const byte WILL = 251;

    /// <summary>Refuse to perform option.</summary>
    public const byte WONT = 252;

    /// <summary>Begin subnegotiation.</summary>
    public const byte SB = 250;

    /// <summary>End subnegotiation.</summary>
    public const byte SE = 240;

    #endregion

    #region Simple Commands (used with IAC)

    /// <summary>Data Mark.</summary>
    public const byte DM = 242;

    /// <summary>Break.</summary>
    public const byte BRK = 243;

    /// <summary>Interrupt Process.</summary>
    public const byte IP = 244;

    /// <summary>Abort Output.</summary>
    public const byte AO = 245;

    /// <summary>Are You There.</summary>
    public const byte AYT = 246;

    /// <summary>Erase Character.</summary>
    public const byte EC = 247;

    /// <summary>Erase Line.</summary>
    public const byte EL = 248;

    /// <summary>Go Ahead.</summary>
    public const byte GA = 249;

    #endregion

    #region Options

    /// <summary>Echo option.</summary>
    public const byte ECHO = 1;

    /// <summary>Suppress Go-Ahead option (same as SGA).</summary>
    public const byte SUPPRESS_GO_AHEAD = 3;

    /// <summary>Terminal Type option.</summary>
    public const byte TTYPE = 24;

    /// <summary>Negotiate About Window Size option.</summary>
    public const byte NAWS = 31;

    /// <summary>Linemode option.</summary>
    public const byte LINEMODE = 34;

    #endregion

    #region Subnegotiation

    /// <summary>TTYPE subnegotiation: IS (terminal type follows).</summary>
    public const byte IS = 0;

    /// <summary>TTYPE subnegotiation: SEND (request terminal type).</summary>
    public const byte SEND = 1;

    /// <summary>LINEMODE subnegotiation: MODE.</summary>
    public const byte MODE = 1;

    #endregion

    #region Helper Methods

    /// <summary>
    /// Creates a single-byte array from a byte value.
    /// </summary>
    /// <param name="value">The byte value.</param>
    /// <returns>A single-element byte array.</returns>
    /// <remarks>
    /// Equivalent to Python's <c>int2byte()</c> helper.
    /// </remarks>
    public static byte[] ToByte(byte value) => [value];

    #endregion
}
