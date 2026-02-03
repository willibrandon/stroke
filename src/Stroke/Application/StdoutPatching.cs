namespace Stroke.Application;

/// <summary>
/// Static entry point for patching <see cref="Console.Out"/> and <see cref="Console.Error"/>
/// with a <see cref="StdoutProxy"/> that routes output above the current prompt.
/// </summary>
/// <remarks>
/// <para>
/// Port of Python Prompt Toolkit's <c>patch_stdout</c> context manager from
/// <c>patch_stdout.py</c>.
/// </para>
/// </remarks>
public static class StdoutPatching
{
    /// <summary>
    /// Replaces <see cref="Console.Out"/> and <see cref="Console.Error"/> with a
    /// <see cref="StdoutProxy"/> instance. Returns an <see cref="IDisposable"/> that
    /// restores the original streams when disposed.
    /// </summary>
    /// <param name="raw">
    /// When <c>true</c>, VT100 escape sequences are passed through unmodified.
    /// Default: <c>false</c>.
    /// </param>
    /// <returns>
    /// An <see cref="IDisposable"/> that, when disposed, restores the original
    /// <see cref="Console.Out"/> and <see cref="Console.Error"/> streams and
    /// disposes the proxy.
    /// </returns>
    /// <example>
    /// <code>
    /// using (StdoutPatching.PatchStdout())
    /// {
    ///     // All Console.Write/WriteLine calls are routed above the prompt
    ///     Console.WriteLine("This appears above the prompt!");
    /// }
    /// // Console.Out and Console.Error are restored
    /// </code>
    /// </example>
    public static IDisposable PatchStdout(bool raw = false)
    {
        var proxy = new StdoutProxy(raw: raw);

        var originalStdout = Console.Out;
        var originalStderr = Console.Error;

        Console.SetOut(proxy);
        Console.SetError(proxy);

        return new PatchScope(proxy, originalStdout, originalStderr);
    }

    /// <summary>
    /// Disposable scope that restores original console streams and disposes the proxy.
    /// </summary>
    private sealed class PatchScope(
        StdoutProxy proxy,
        TextWriter originalStdout,
        TextWriter originalStderr) : IDisposable
    {
        private int _disposed;

        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _disposed, 1, 0) != 0)
                return;

            // Restore originals first, then dispose proxy.
            // This matches Python's finally-block ordering.
            Console.SetOut(originalStdout);
            Console.SetError(originalStderr);

            proxy.Dispose();
        }
    }
}
