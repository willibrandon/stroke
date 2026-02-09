using System.Collections.Concurrent;
using System.Text;

namespace Stroke.Application;

/// <summary>
/// A <see cref="TextWriter"/> that intercepts console output and routes it above
/// the current active prompt. Compatible with other TextWriter consumers and can
/// be used as a drop-in replacement for <see cref="Console.Out"/> or passed to
/// logging handlers.
/// </summary>
/// <remarks>
/// <para>
/// The current application, above which output is printed, is determined by the
/// <see cref="AppSession"/> that is active during construction of this instance.
/// </para>
/// <para>
/// To avoid continuous prompt repainting for every small write, a short delay
/// of <see cref="SleepBetweenWrites"/> is added between consecutive flushes to
/// batch many smaller writes in a short timespan.
/// </para>
/// <para>
/// This class is thread-safe. Multiple threads may write concurrently.
/// </para>
/// <para>
/// Port of Python Prompt Toolkit's <c>StdoutProxy</c> from <c>patch_stdout.py</c>.
/// </para>
/// </remarks>
public sealed class StdoutProxy : TextWriter
{
    /// <summary>
    /// Captures the process-level original stdout at first access, equivalent to
    /// Python's <c>sys.__stdout__</c>. Used as fallback in <see cref="OriginalStdout"/>.
    /// </summary>
    private static readonly TextWriter? _originalProcessStdout = Console.Out;

    private readonly Lock _lock = new();
    private readonly List<string> _buffer = [];
    private readonly AppSession _appSession;
    private readonly Output.IOutput _output;
    private readonly BlockingCollection<FlushItem> _flushQueue = new();
    private readonly Thread _flushThread;

    /// <summary>
    /// Creates a new <see cref="StdoutProxy"/> that routes output above the current prompt.
    /// </summary>
    /// <param name="sleepBetweenWrites">
    /// Delay between consecutive flushes to batch rapid writes. Default: 200ms.
    /// Must be non-negative; zero disables the delay.
    /// </param>
    /// <param name="raw">
    /// When <c>true</c>, VT100 escape sequences are passed through unmodified.
    /// When <c>false</c> (default), escape sequences are escaped by the output system.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="sleepBetweenWrites"/> is negative.
    /// </exception>
    public StdoutProxy(TimeSpan? sleepBetweenWrites = null, bool raw = false)
    {
        SleepBetweenWrites = sleepBetweenWrites ?? TimeSpan.FromMilliseconds(200);

        if (SleepBetweenWrites < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(sleepBetweenWrites),
                SleepBetweenWrites,
                "SleepBetweenWrites must be non-negative.");
        }

        Raw = raw;

        // Capture the current app session and output *now*, before this proxy
        // is potentially assigned to Console.Out. This prevents recursive
        // initialization when OutputFactory tries to inspect Console.Out.
        // Port of Python: self.app_session = get_app_session()
        _appSession = AppContext.GetAppSession();
        // Port of Python: self._output = self.app_session.output
        _output = _appSession.Output;

        // Start the background flush thread.
        _flushThread = StartWriteThread();
    }

    /// <summary>
    /// Gets the delay between consecutive flushes to batch rapid writes.
    /// </summary>
    public TimeSpan SleepBetweenWrites { get; }

    /// <summary>
    /// Gets whether raw mode is enabled (VT100 escape sequences pass through unmodified).
    /// </summary>
    public bool Raw { get; }

    /// <summary>
    /// Gets whether this proxy has been closed/disposed.
    /// </summary>
    public bool Closed { get; private set; }

    /// <summary>
    /// Gets the original stdout stream that this proxy wraps.
    /// Falls back to the process-level original stdout when the output has no stdout stream.
    /// </summary>
    public TextWriter? OriginalStdout => _output.Stdout ?? _originalProcessStdout;

    /// <summary>
    /// Gets the error handling mode. Always returns "strict" for compatibility
    /// with Python's <c>TextIO.errors</c> protocol.
    /// </summary>
    public string Errors => "strict";

    /// <summary>
    /// Gets the encoding of the underlying output.
    /// </summary>
    public override Encoding Encoding => Encoding.UTF8;

    /// <summary>
    /// Writes text to the proxy. Text is buffered until a newline is encountered,
    /// at which point complete lines are queued for output.
    /// </summary>
    /// <param name="value">The text to write. Null and empty strings are ignored.</param>
    public override void Write(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return;

        using (_lock.EnterScope())
        {
            if (Closed)
                return;

            WriteCore(value);
        }
    }

    /// <summary>
    /// Writes a single character to the proxy.
    /// </summary>
    /// <param name="value">The character to write.</param>
    public override void Write(char value)
    {
        Write(value.ToString());
    }

    /// <summary>
    /// Flushes all buffered content to the output queue, even without a trailing newline.
    /// </summary>
    public override void Flush()
    {
        using (_lock.EnterScope())
        {
            if (Closed)
                return;

            FlushCore();
        }
    }

    /// <summary>
    /// Closes the proxy, flushing remaining content and terminating the background thread.
    /// Idempotent — safe to call multiple times.
    /// </summary>
    public override void Close()
    {
        using (_lock.EnterScope())
        {
            if (Closed)
                return;

            FlushCore();
            Closed = true;
        }

        // Queue the sentinel outside the lock — BlockingCollection is thread-safe.
        _flushQueue.Add(new FlushItem.Done());
        _flushThread.Join();
        _flushQueue.Dispose();
    }

    /// <summary>
    /// Returns the file descriptor of the underlying output stream.
    /// </summary>
    /// <returns>The file descriptor number.</returns>
    public int Fileno() => _output.Fileno();

    /// <summary>
    /// Returns whether the underlying output is a terminal.
    /// </summary>
    /// <returns><c>true</c> if the output is a terminal; otherwise <c>false</c>.</returns>
    public bool IsAtty()
    {
        var stdout = _output.Stdout;
        if (stdout is null)
            return false;

        // Match Python's stdout.isatty(). .NET's TextWriter base class doesn't
        // have isatty(), but if the underlying stream wraps a console, we can
        // detect it via Console.IsOutputRedirected (when the stream is Console.Out).
        // For StreamWriter wrapping a FileStream, check if it's a terminal handle.
        return !Console.IsOutputRedirected;
    }

    /// <summary>
    /// Disposes the proxy by calling <see cref="Close"/>.
    /// </summary>
    /// <param name="disposing">Whether managed resources should be disposed.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Close();
        }

        base.Dispose(disposing);
    }

    // ── Internal: Newline-Gated Buffering ───────────────────────────────

    /// <summary>
    /// Port of Python's <c>_write(data)</c>. Must be called under lock.
    /// </summary>
    private void WriteCore(string data)
    {
        int lastNewline = data.LastIndexOf('\n');

        if (lastNewline >= 0)
        {
            // Split at last newline: flush everything up to and including it.
            string before = data[..(lastNewline + 1)];
            string after = data[(lastNewline + 1)..];

            _buffer.Add(before);
            string text = string.Join("", _buffer);
            _buffer.Clear();

            if (after.Length > 0)
            {
                _buffer.Add(after);
            }

            _flushQueue.Add(new FlushItem.Text(text));
        }
        else
        {
            // No newline — just buffer.
            _buffer.Add(data);
        }
    }

    /// <summary>
    /// Port of Python's <c>_flush()</c>. Must be called under lock.
    /// </summary>
    private void FlushCore()
    {
        if (_buffer.Count == 0)
            return;

        string text = string.Join("", _buffer);
        _buffer.Clear();
        _flushQueue.Add(new FlushItem.Text(text));
    }

    // ── Internal: Background Flush Thread ───────────────────────────────

    /// <summary>
    /// Port of Python's <c>_start_write_thread()</c>.
    /// </summary>
    private Thread StartWriteThread()
    {
        var thread = new Thread(WriteThread)
        {
            IsBackground = true,
            Name = "patch-stdout-flush-thread",
        };
        thread.Start();
        return thread;
    }

    /// <summary>
    /// Small delay after the blocking Take() to allow rapid writes to accumulate
    /// before draining. This ensures batching works even without the sleep-between-writes
    /// delay (which only applies when an app is running).
    /// </summary>
    private const int BatchingWindowMs = 1;

    /// <summary>
    /// Port of Python's <c>_write_thread()</c>. Consumer loop for the flush queue.
    /// </summary>
    private void WriteThread()
    {
        bool done = false;

        while (!done)
        {
            FlushItem item;
            try
            {
                item = _flushQueue.Take();
            }
            catch (InvalidOperationException)
            {
                // CompleteAdding was called — exit.
                break;
            }

            if (item is FlushItem.Done)
                break;

            if (item is not FlushItem.Text { Value: { Length: > 0 } firstText })
                continue;

            // Small delay to allow rapid writes to accumulate in the queue.
            // Python relies on the sleep_between_writes for batching, but that only
            // applies when an app is running. This ensures batching works regardless.
            Thread.Sleep(BatchingWindowMs);

            // Drain remaining items from the queue.
            var textParts = new List<string> { firstText };

            while (_flushQueue.TryTake(out var next))
            {
                if (next is FlushItem.Done)
                {
                    done = true;
                    break;
                }

                if (next is FlushItem.Text { Value: { Length: > 0 } value })
                {
                    textParts.Add(value);
                }
            }

            string combined = textParts.Count == 1
                ? textParts[0]
                : string.Join("", textParts);

            // Write output, swallowing exceptions (FR-022).
            try
            {
                WriteAndFlush(combined);
            }
            catch
            {
                // Swallow all exceptions from IOutput/RunInTerminal to keep
                // the flush thread alive (FR-022).
            }

            // If an application is running, sleep to batch writes (FR-005).
            if (GetAppOrNull() is not null && SleepBetweenWrites > TimeSpan.Zero)
            {
                Thread.Sleep(SleepBetweenWrites);
            }
        }
    }

    // ── Internal: Output Coordination ───────────────────────────────────

    /// <summary>
    /// Check whether an application is currently running in our captured session.
    /// Port of Python's <c>_get_app_loop()</c>.
    /// </summary>
    private IApplication? GetAppOrNull()
    {
        var app = _appSession.App;
        if (app is not null && app.IsRunning)
        {
            return app;
        }

        return null;
    }

    /// <summary>
    /// Port of Python's <c>_write_and_flush(loop, text)</c>. Writes text to the
    /// terminal, coordinating with the application if one is running.
    /// </summary>
    private void WriteAndFlush(string text)
    {
        void WriteOutput(string data)
        {
            _output.EnableAutowrap();

            if (Raw)
            {
                _output.WriteRaw(data);
            }
            else
            {
                _output.Write(data);
            }

            _output.Flush();
        }

        if (GetAppOrNull() is not null)
        {
            // Application is running — use RunInTerminal to coordinate with renderer.
            // OPOST remains enabled (matching Python Prompt Toolkit's raw_mode which
            // never clears c_oflag), so the kernel handles LF → CRLF conversion.
            // The flush thread is a raw Thread, so the AsyncLocal<AppSession> doesn't
            // flow from the creating thread. Temporarily activate our captured session
            // so that RunInTerminal can find the running application.
            using (AppContext.ActivateSession(_appSession))
            {
                RunInTerminal.RunAsync(() => WriteOutput(text), inExecutor: false)
                    .GetAwaiter()
                    .GetResult();
            }
        }
        else
        {
            // No application running — write directly.
            WriteOutput(text);
        }
    }
}
