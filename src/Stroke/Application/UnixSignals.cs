using System.Runtime.InteropServices;

namespace Stroke.Application;

/// <summary>
/// Native interop for Unix signal sending.
/// Used by <see cref="Application{TResult}.SuspendToBackground"/> to send SIGTSTP.
/// </summary>
internal static partial class UnixSignals
{
    /// <summary>
    /// SIGTSTP signal number. 18 on macOS/BSD, 20 on Linux.
    /// Python: <c>signal.SIGTSTP</c>.
    /// </summary>
    internal static readonly int SIGTSTP = OperatingSystem.IsMacOS() ? 18 : 20;

    /// <summary>Send a signal to a process or process group.</summary>
    [LibraryImport("libc", SetLastError = true, EntryPoint = "kill")]
    internal static partial int Kill(int pid, int sig);
}
