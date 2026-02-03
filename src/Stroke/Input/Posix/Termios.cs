using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Stroke.Input.Posix;

/// <summary>
/// P/Invoke wrapper for POSIX termios functions.
/// </summary>
/// <remarks>
/// <para>
/// This class provides access to terminal I/O control on POSIX systems (Linux, macOS).
/// It wraps the termios structure and related functions for raw/cooked mode switching.
/// </para>
/// <para>
/// Thread safety: Individual P/Invoke calls are thread-safe, but terminal mode changes
/// affect the entire process. Callers should ensure mutual exclusion when modifying
/// terminal settings.
/// </para>
/// <para>
/// Implementation notes: Uses <see cref="LibraryImportAttribute"/> with unsafe struct
/// pointers for modern source-generated P/Invoke. The termios struct uses a fixed-size
/// buffer instead of a managed array for compatibility with LibraryImport.
/// </para>
/// </remarks>
[SupportedOSPlatform("linux")]
[SupportedOSPlatform("macos")]
[SupportedOSPlatform("freebsd")]
public static unsafe partial class Termios
{
    private const string LibC = "libc";

    /// <summary>
    /// Gets the terminal attributes for the specified file descriptor.
    /// </summary>
    /// <param name="fd">The file descriptor (0 for stdin).</param>
    /// <param name="termios">Pointer to the termios structure to fill.</param>
    /// <returns>0 on success, -1 on error.</returns>
    [LibraryImport(LibC, EntryPoint = "tcgetattr", SetLastError = true)]
    public static partial int GetAttr(int fd, TermiosStruct* termios);

    /// <summary>
    /// Sets the terminal attributes for the specified file descriptor.
    /// </summary>
    /// <param name="fd">The file descriptor (0 for stdin).</param>
    /// <param name="optionalActions">When to apply the changes (TCSANOW, TCSADRAIN, TCSAFLUSH).</param>
    /// <param name="termios">Pointer to the termios structure with new settings.</param>
    /// <returns>0 on success, -1 on error.</returns>
    [LibraryImport(LibC, EntryPoint = "tcsetattr", SetLastError = true)]
    public static partial int SetAttr(int fd, int optionalActions, TermiosStruct* termios);

    /// <summary>
    /// Checks if a file descriptor refers to a terminal.
    /// </summary>
    /// <param name="fd">The file descriptor to check.</param>
    /// <returns>1 if terminal, 0 if not.</returns>
    [LibraryImport(LibC, EntryPoint = "isatty")]
    public static partial int IsATty(int fd);

    /// <summary>Apply terminal attribute changes immediately.</summary>
    public const int TCSANOW = 0;

    /// <summary>Apply terminal attribute changes after output is written.</summary>
    public const int TCSADRAIN = 1;

    /// <summary>Apply terminal attribute changes after output is written, discarding unread input.</summary>
    public const int TCSAFLUSH = 2;

    /// <summary>Standard file descriptor for stdin.</summary>
    public const int STDIN_FILENO = 0;

    #region Input Mode Flags (c_iflag)

    /// <summary>Ignore BREAK condition.</summary>
    public const nuint IGNBRK = 0x00000001;

    /// <summary>Signal interrupt on BREAK.</summary>
    public const nuint BRKINT = 0x00000002;

    /// <summary>Ignore parity errors.</summary>
    public const nuint IGNPAR = 0x00000004;

    /// <summary>Mark parity errors.</summary>
    public const nuint PARMRK = 0x00000008;

    /// <summary>Enable parity checking.</summary>
    public const nuint INPCK = 0x00000010;

    /// <summary>Strip 8th bit.</summary>
    public const nuint ISTRIP = 0x00000020;

    /// <summary>Translate NL to CR on input.</summary>
    public const nuint INLCR = 0x00000040;

    /// <summary>Ignore CR on input.</summary>
    public const nuint IGNCR = 0x00000080;

    /// <summary>Translate CR to NL on input.</summary>
    public const nuint ICRNL = 0x00000100;

    /// <summary>Enable start/stop output control.</summary>
    public const nuint IXON = 0x00000200;

    /// <summary>Any character restarts output.</summary>
    public const nuint IXANY = 0x00000800;

    /// <summary>Enable start/stop input control.</summary>
    public const nuint IXOFF = 0x00000400;

    /// <summary>Ring bell on input queue full.</summary>
    public const nuint IMAXBEL = 0x00002000;

    #endregion

    #region Output Mode Flags (c_oflag)

    /// <summary>Post-process output.</summary>
    public const nuint OPOST = 0x00000001;

    #endregion

    #region Local Mode Flags (c_lflag)

    /// <summary>Enable signals (INTR, QUIT, SUSP).</summary>
    public const nuint ISIG = 0x00000080;

    /// <summary>Canonical mode (line editing).</summary>
    public const nuint ICANON = 0x00000100;

    /// <summary>Enable echo.</summary>
    public const nuint ECHO = 0x00000008;

    /// <summary>Echo ERASE as BS-SP-BS.</summary>
    public const nuint ECHOE = 0x00000002;

    /// <summary>Echo KILL by erasing line.</summary>
    public const nuint ECHOK = 0x00000004;

    /// <summary>Echo NL even if ECHO is off.</summary>
    public const nuint ECHONL = 0x00000010;

    /// <summary>Disable flush after interrupt.</summary>
    public const nuint NOFLSH = 0x80000000;

    /// <summary>Stop background jobs.</summary>
    public const nuint TOSTOP = 0x00400000;

    /// <summary>Extended input processing.</summary>
    public const nuint IEXTEN = 0x00000400;

    #endregion

    #region Control Characters (c_cc indices)

    /// <summary>Minimum number of characters to read.</summary>
    public const int VMIN = 16;

    /// <summary>Timeout in deciseconds.</summary>
    public const int VTIME = 17;

    /// <summary>Size of the control characters array.</summary>
    public const int NCCS = 32;

    #endregion

    /// <summary>
    /// Creates a termios structure configured for raw mode.
    /// </summary>
    /// <param name="original">The original termios structure to modify.</param>
    /// <returns>A termios structure configured for raw mode.</returns>
    public static TermiosStruct MakeRaw(TermiosStruct original)
    {
        var raw = original;

        // Disable input processing
        raw.c_iflag &= ~(IGNBRK | BRKINT | PARMRK | ISTRIP | INLCR | IGNCR | ICRNL | IXON);

        // Disable output processing
        raw.c_oflag &= ~OPOST;

        // Disable canonical mode, echo, and signals
        raw.c_lflag &= ~(ECHO | ECHONL | ICANON | ISIG | IEXTEN);

        // Set VMIN=1, VTIME=0 for blocking read of single characters
        raw.c_cc[VMIN] = 1;
        raw.c_cc[VTIME] = 0;

        return raw;
    }
}

/// <summary>
/// POSIX termios structure.
/// </summary>
/// <remarks>
/// <para>
/// The exact layout varies between platforms:
/// - macOS/FreeBSD: tcflag_t and speed_t are 8 bytes (unsigned long), NCCS=20
/// - Linux: tcflag_t is 4 bytes (unsigned int), speed_t is 4 bytes, NCCS=32
/// </para>
/// <para>
/// This struct uses a fixed-size buffer for c_cc to enable compatibility with
/// <see cref="LibraryImportAttribute"/> source-generated P/Invoke.
/// </para>
/// </remarks>
[StructLayout(LayoutKind.Sequential)]
public unsafe struct TermiosStruct
{
    /// <summary>Input mode flags.</summary>
    public nuint c_iflag;

    /// <summary>Output mode flags.</summary>
    public nuint c_oflag;

    /// <summary>Control mode flags.</summary>
    public nuint c_cflag;

    /// <summary>Local mode flags.</summary>
    public nuint c_lflag;

    /// <summary>Control characters array (fixed-size buffer, sized for macOS NCCS=20 with padding).</summary>
    public fixed byte c_cc[20];

    // 4 bytes padding to align c_ispeed on 8-byte boundary (macOS)
    private readonly uint _padding;

    /// <summary>Input speed.</summary>
    public nuint c_ispeed;

    /// <summary>Output speed.</summary>
    public nuint c_ospeed;
}
