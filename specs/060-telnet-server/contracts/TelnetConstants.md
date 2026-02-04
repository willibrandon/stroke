# Contract: TelnetConstants

**Namespace**: `Stroke.Contrib.Telnet`
**Python Source**: `prompt_toolkit.contrib.telnet.protocol` (module-level constants)

## Class Signature

```csharp
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
```

## Functional Requirements Coverage

| Requirement | Constants Used |
|-------------|----------------|
| FR-002: Send initialization sequences | `IAC`, `DO`, `WILL`, `LINEMODE`, `SUPPRESS_GO_AHEAD`, `ECHO`, `NAWS`, `TTYPE`, `SB`, `SE`, `MODE`, `SEND` |
| FR-003: Parse IAC sequences | `IAC` |
| FR-004: Handle NAWS | `NAWS`, `SB`, `SE` |
| FR-005: Handle TTYPE | `TTYPE`, `IS`, `SEND`, `SB`, `SE` |
| FR-016: Handle double-IAC | `IAC` |

## Python API Mapping

| Python | C# |
|--------|-----|
| `NOP = int2byte(0)` | `NOP = 0` |
| `SGA = int2byte(3)` | `SGA = 3` |
| `IAC = int2byte(255)` | `IAC = 255` |
| `DO = int2byte(253)` | `DO = 253` |
| `DONT = int2byte(254)` | `DONT = 254` |
| `WILL = int2byte(251)` | `WILL = 251` |
| `WONT = int2byte(252)` | `WONT = 252` |
| `SB = int2byte(250)` | `SB = 250` |
| `SE = int2byte(240)` | `SE = 240` |
| `LINEMODE = int2byte(34)` | `LINEMODE = 34` |
| `MODE = int2byte(1)` | `MODE = 1` |
| `ECHO = int2byte(1)` | `ECHO = 1` |
| `NAWS = int2byte(31)` | `NAWS = 31` |
| `SUPPRESS_GO_AHEAD = int2byte(3)` | `SUPPRESS_GO_AHEAD = 3` |
| `TTYPE = int2byte(24)` | `TTYPE = 24` |
| `SEND = int2byte(1)` | `SEND = 1` |
| `IS = int2byte(0)` | `IS = 0` |
| `DM = int2byte(242)` | `DM = 242` |
| `BRK = int2byte(243)` | `BRK = 243` |
| `IP = int2byte(244)` | `IP = 244` |
| `AO = int2byte(245)` | `AO = 245` |
| `AYT = int2byte(246)` | `AYT = 246` |
| `EC = int2byte(247)` | `EC = 247` |
| `EL = int2byte(248)` | `EL = 248` |
| `GA = int2byte(249)` | `GA = 249` |

## Initialization Sequence

The server sends this sequence to initialize a new connection (FR-002):

```csharp
// Pseudo-code using constants
socket.Send(IAC, DO, LINEMODE);           // Request linemode
socket.Send(IAC, WILL, SUPPRESS_GO_AHEAD); // Enable full-duplex
socket.Send(IAC, SB, LINEMODE, MODE, 0, IAC, SE); // Disable line editing
socket.Send(IAC, WILL, ECHO);             // Server will echo
socket.Send(IAC, DO, NAWS);               // Request window size
socket.Send(IAC, DO, TTYPE);              // Request terminal type
socket.Send(IAC, SB, TTYPE, SEND, IAC, SE); // Send terminal type
```
