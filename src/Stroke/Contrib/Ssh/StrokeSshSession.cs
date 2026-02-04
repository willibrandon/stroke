using System.Text;
using Microsoft.Extensions.Logging;
using Stroke.Core.Primitives;
using Stroke.Input.Pipe;
using Stroke.Output;
using Stroke.Shortcuts;

using AppCtx = Stroke.Application.AppContext;

namespace Stroke.Contrib.Ssh;

/// <summary>
/// Represents a single SSH client session.
/// </summary>
/// <remarks>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>PromptToolkitSSHSession</c> class.
/// Each session has its own isolated input/output streams and can run a Stroke
/// application independently.
/// </para>
/// <para>
/// Thread safety: This class is thread-safe. Methods can be called from any thread,
/// but the session's internal application context is associated with a specific
/// async context.
/// </para>
/// </remarks>
public class StrokeSshSession
{
    private static readonly ILogger _logger = StrokeLogger.CreateLogger("Stroke.Ssh.Session");

    private readonly ISshChannel _channel;
    private readonly SshChannelStdout _stdout;
    private readonly Lock _lock = new();
    private IPipeInput? _pipeInput;
    private Vt100Output? _vt100Output;
    private Size _size;
    private volatile bool _closed;
    private volatile bool _hasAppContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="StrokeSshSession"/> class.
    /// </summary>
    /// <param name="channel">The SSH channel adapter.</param>
    /// <param name="interact">The interaction callback.</param>
    /// <param name="enableCpr">Enable cursor position requests.</param>
    internal StrokeSshSession(
        ISshChannel channel,
        Func<StrokeSshSession, Task> interact,
        bool enableCpr)
    {
        _channel = channel ?? throw new ArgumentNullException(nameof(channel));
        Interact = interact ?? throw new ArgumentNullException(nameof(interact));
        EnableCpr = enableCpr;

        // Get initial size from channel (set by PTY request), or use default per FR-004
        var (width, height) = channel.GetTerminalSize();
        _size = new Size(height, width);

        // Create stdout wrapper with LF→CRLF conversion
        _stdout = new SshChannelStdout(_channel);
    }

    /// <summary>
    /// Gets the interact callback for this session.
    /// </summary>
    public Func<StrokeSshSession, Task> Interact { get; }

    /// <summary>
    /// Gets whether cursor position requests are enabled.
    /// </summary>
    public bool EnableCpr { get; }

    /// <summary>
    /// Gets the current application session, or null if not yet started.
    /// </summary>
    public Application.AppSession? AppSession { get; private set; }

    /// <summary>
    /// Gets the running interact task, or null if not started.
    /// </summary>
    public Task? InteractTask { get; private set; }

    /// <summary>
    /// Gets whether this session has been closed.
    /// </summary>
    public bool IsClosed => _closed;

    /// <summary>
    /// Gets the current terminal size.
    /// </summary>
    /// <returns>
    /// The terminal size, or (79, 20) if not yet negotiated.
    /// </returns>
    public Size GetSize()
    {
        using (_lock.EnterScope())
        {
            return _size;
        }
    }

    /// <summary>
    /// Called when data is received from the SSH client.
    /// </summary>
    /// <param name="data">The received data as bytes.</param>
    /// <remarks>
    /// Routes data to the session's PipeInput for keyboard handling.
    /// </remarks>
    public void DataReceived(byte[] data)
    {
        if (_closed || data == null || data.Length == 0)
        {
            return;
        }

        var pipeInput = _pipeInput;
        if (pipeInput != null)
        {
            pipeInput.SendBytes(data);
        }
    }

    /// <summary>
    /// Called when the terminal size changes.
    /// </summary>
    /// <param name="width">New terminal width in columns.</param>
    /// <param name="height">New terminal height in rows.</param>
    public void TerminalSizeChanged(int width, int height)
    {
        // Clamp dimensions to [1, 500] per spec edge cases
        width = Math.Clamp(width, 1, 500);
        height = Math.Clamp(height, 1, 500);

        using (_lock.EnterScope())
        {
            _size = new Size(height, width);
        }

        // Trigger app invalidation if running (FR-008)
        if (_hasAppContext)
        {
            try
            {
                var app = AppCtx.GetAppOrNull();
                app?.Invalidate();
            }
            catch
            {
                // Ignore resize notification errors
            }
        }
    }

    /// <summary>
    /// Starts the interactive session.
    /// </summary>
    /// <returns>A task that completes when the session ends.</returns>
    internal async Task RunAsync()
    {
        try
        {
            // Create pipe input for this session (FR-003)
            using var pipeInput = new SimplePipeInput();
            _pipeInput = pipeInput;

            // Set line mode to false (FR-011) - no-op for SSH but maintains API consistency
            _channel.SetLineMode(false);

            // Get terminal info from channel
            var terminalType = _channel.GetTerminalType();

            // Create Vt100Output now that we have terminal info
            _vt100Output = Vt100Output.Create(
                _stdout,
                GetSize,
                term: terminalType,
                enableCpr: EnableCpr);

            // Create app session with isolated input/output (FR-003)
            using var session = AppCtx.CreateAppSession(pipeInput, _vt100Output);
            AppSession = session;

            // Signal that we have an app context
            _hasAppContext = true;

            try
            {
                // Invoke interact callback
                InteractTask = Interact(this);
                await InteractTask.ConfigureAwait(false);
            }
            finally
            {
                _hasAppContext = false;
                InteractTask = null;
            }
        }
        catch (EOFException)
        {
            // Client disconnected - normal exit
            _logger.LogDebug("SSH client disconnected (EOF)");
        }
        catch (Exception ex)
        {
            // Log and swallow - don't crash server (ERR-003 equivalent)
            _logger.LogError(ex, "Error in SSH interact callback");
        }
        finally
        {
            AppSession = null;
            _pipeInput = null;
            Close();
        }
    }

    /// <summary>
    /// Closes this session.
    /// </summary>
    /// <remarks>
    /// Closes the channel, input, and output. After closing:
    /// <list type="bullet">
    /// <item><see cref="IsClosed"/> returns true</item>
    /// <item>Data routing becomes a no-op</item>
    /// </list>
    /// This method is idempotent; multiple calls have no effect.
    /// </remarks>
    public void Close()
    {
        if (_closed)
        {
            return;
        }

        using (_lock.EnterScope())
        {
            if (_closed)
            {
                return;
            }

            _closed = true;

            // Dispose in order: PipeInput → Vt100Output → AppSession → Channel (FR-012)
            try
            {
                _pipeInput?.Close();
            }
            catch { }

            try
            {
                _stdout.Close();
            }
            catch { }

            try
            {
                _channel.Close();
            }
            catch { }
        }
    }
}
