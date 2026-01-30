using System.Runtime.InteropServices;

namespace Stroke.Application;

/// <summary>
/// Native interop for Unix signal sending.
/// Used by <see cref="Application{TResult}.SuspendToBackground"/> to send SIGTSTP.
/// </summary>
internal static partial class UnixSignals
{
    /// <summary>SIGTSTP signal number (20 on Linux and macOS).</summary>
    internal const int SIGTSTP = 20;

    /// <summary>Send a signal to a process or process group.</summary>
    [LibraryImport("libc", SetLastError = true, EntryPoint = "kill")]
    internal static partial int Kill(int pid, int sig);
}
