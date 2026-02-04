using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Logging;
using Stroke.Core.Primitives;
using Stroke.FormattedText;
using Stroke.Input.Pipe;
using Stroke.Output;
using Stroke.Rendering;
using Stroke.Shortcuts;
using Stroke.Styles;

using AppCtx = Stroke.Application.AppContext;
using RunInTerminal = Stroke.Application.RunInTerminal;

namespace Stroke.Contrib.Telnet;

/// <summary>
/// Represents a single telnet client connection.
/// </summary>
/// <remarks>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>TelnetConnection</c> class.
/// Each connection has its own isolated input/output streams and can run a Stroke
/// application independently.
/// </para>
/// <para>
/// Thread safety: This class is thread-safe. Methods can be called from any thread,
/// but the connection's internal application context is associated with a specific
/// async context.
/// </para>
/// </remarks>
public sealed class TelnetConnection
{
    private static readonly ILogger _logger = StrokeLogger.CreateLogger("Stroke.Telnet.Connection");

    private readonly Func<TelnetConnection, Task> _interact;
    private readonly IPipeInput _pipeInput;
    private readonly ConnectionStdout _stdout;
    private readonly TelnetProtocolParser _parser;
    private readonly Lock _lock = new();
    private readonly TaskCompletionSource<bool> _ready = new();
    private volatile bool _closed;
    private volatile bool _hasAppContext;
    private Size _size;
    private Vt100Output? _vt100Output;

    /// <summary>
    /// Initializes a new instance of the <see cref="TelnetConnection"/> class.
    /// </summary>
    /// <param name="socket">The connected TCP socket.</param>
    /// <param name="remoteAddress">The remote client address.</param>
    /// <param name="interact">The interaction callback.</param>
    /// <param name="server">The parent server.</param>
    /// <param name="encoding">Character encoding.</param>
    /// <param name="style">Optional style for formatted text.</param>
    /// <param name="pipeInput">The pipe input for this connection.</param>
    /// <param name="enableCpr">Enable cursor position requests.</param>
    internal TelnetConnection(
        Socket socket,
        IPEndPoint remoteAddress,
        Func<TelnetConnection, Task> interact,
        TelnetServer server,
        Encoding encoding,
        IStyle? style,
        IPipeInput pipeInput,
        bool enableCpr)
    {
        Socket = socket ?? throw new ArgumentNullException(nameof(socket));
        RemoteAddress = remoteAddress ?? throw new ArgumentNullException(nameof(remoteAddress));
        _interact = interact ?? throw new ArgumentNullException(nameof(interact));
        Server = server ?? throw new ArgumentNullException(nameof(server));
        Encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
        Style = style;
        _pipeInput = pipeInput ?? throw new ArgumentNullException(nameof(pipeInput));
        EnableCpr = enableCpr;

        // Default size (EC-004)
        _size = new Size(24, 80);

        // Create stdout wrapper
        _stdout = new ConnectionStdout(socket, encoding);

        // Create protocol parser with callbacks
        _parser = new TelnetProtocolParser(
            DataReceived,
            SizeReceived,
            TtypeReceived);
    }

    /// <summary>
    /// Gets the underlying TCP socket.
    /// </summary>
    public Socket Socket { get; }

    /// <summary>
    /// Gets the remote client address.
    /// </summary>
    public IPEndPoint RemoteAddress { get; }

    /// <summary>
    /// Gets the parent telnet server.
    /// </summary>
    public TelnetServer Server { get; }

    /// <summary>
    /// Gets the character encoding.
    /// </summary>
    public Encoding Encoding { get; }

    /// <summary>
    /// Gets the style for formatted text.
    /// </summary>
    public IStyle? Style { get; }

    /// <summary>
    /// Gets the current terminal size.
    /// </summary>
    /// <remarks>
    /// Updated when NAWS data is received from the client.
    /// </remarks>
    public Size Size => _size;

    /// <summary>
    /// Gets whether this connection has been closed.
    /// </summary>
    public bool IsClosed => _closed;

    /// <summary>
    /// Gets whether cursor position requests are enabled.
    /// </summary>
    public bool EnableCpr { get; }

    /// <summary>
    /// Sends formatted text to the client.
    /// </summary>
    /// <param name="formattedText">The formatted text to send.</param>
    /// <remarks>
    /// <para>
    /// The text is rendered using the connection's style and sent as ANSI escape
    /// sequences. This method is safe to call from any thread.
    /// </para>
    /// <para>
    /// If the connection is closed, this method is a no-op.
    /// </para>
    /// </remarks>
    public void Send(AnyFormattedText formattedText)
    {
        if (_closed || _vt100Output == null)
        {
            return;
        }

        using (_lock.EnterScope())
        {
            if (_closed || _vt100Output == null)
            {
                return;
            }

            var formatted = formattedText.ToFormattedText();
            var styleToUse = Style ?? DummyStyle.Instance;
            RendererUtils.PrintFormattedText(_vt100Output, formatted, styleToUse);
        }
    }

    /// <summary>
    /// Sends formatted text above the current prompt.
    /// </summary>
    /// <param name="formattedText">The formatted text to send.</param>
    /// <remarks>
    /// <para>
    /// This method uses the "run in terminal" pattern to print text above any
    /// active prompt without disrupting user input. Useful for notifications,
    /// chat messages, or alerts.
    /// </para>
    /// <para>
    /// Requires an active application context. If no application is running,
    /// throws <see cref="InvalidOperationException"/>.
    /// </para>
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown if called outside of an active application context.
    /// </exception>
    public void SendAbovePrompt(AnyFormattedText formattedText)
    {
        if (!_hasAppContext)
        {
            throw new InvalidOperationException(
                "SendAbovePrompt can only be called from within an active application context.");
        }

        // Use RunInTerminal to print above the prompt
        var text = formattedText;
        RunInTerminal.RunAsync(() => Send(text)).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Erases the screen and moves the cursor to the top-left position.
    /// </summary>
    /// <remarks>
    /// If the connection is closed, this method is a no-op.
    /// </remarks>
    public void EraseScreen()
    {
        if (_closed || _vt100Output == null)
        {
            return;
        }

        using (_lock.EnterScope())
        {
            if (_closed || _vt100Output == null)
            {
                return;
            }

            _vt100Output.EraseScreen();
            _vt100Output.CursorGoto(0, 0);
            _vt100Output.Flush();
        }
    }

    /// <summary>
    /// Closes this connection.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Closes the socket, input, and output. After closing:
    /// <list type="bullet">
    /// <item><see cref="IsClosed"/> returns true</item>
    /// <item><see cref="Send"/> and other methods become no-ops</item>
    /// </list>
    /// </para>
    /// <para>
    /// This method is idempotent; multiple calls have no effect.
    /// </para>
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

            // Close pipe input
            _pipeInput.Close();

            // Close stdout wrapper
            _stdout.Close();

            // Close socket
            try
            {
                Socket.Shutdown(SocketShutdown.Both);
            }
            catch (SocketException)
            {
                // Already closed
            }
            catch (ObjectDisposedException)
            {
                // Already disposed
            }

            try
            {
                Socket.Close();
            }
            catch (ObjectDisposedException)
            {
                // Already disposed
            }
        }
    }

    /// <summary>
    /// Feeds raw data to the telnet protocol parser.
    /// </summary>
    /// <param name="data">Raw bytes received from the socket.</param>
    /// <remarks>
    /// Internal use by TelnetServer. Parses telnet protocol sequences and
    /// forwards user data to the pipe input.
    /// </remarks>
    internal void Feed(ReadOnlySpan<byte> data)
    {
        if (_closed)
        {
            return;
        }

        _parser.Feed(data);
    }

    /// <summary>
    /// Runs the application for this connection.
    /// </summary>
    /// <returns>A task that completes when the connection ends.</returns>
    /// <remarks>
    /// Internal use by TelnetServer. Sets up the app session and invokes
    /// the interact callback.
    /// </remarks>
    internal async Task RunApplicationAsync()
    {
        try
        {
            // Wait for TTYPE to be received (triggers _ready)
            // Use timeout per EC-011 (500ms)
            using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(500));
            try
            {
                await _ready.Task.WaitAsync(cts.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // Timeout - use default VT100 (EC-002)
                TtypeReceived("VT100");
            }

            if (_vt100Output == null)
            {
                // Should not happen, but defensive
                return;
            }

            // Create app session with isolated input/output (FR-007, ISO-001/002)
            using var session = AppCtx.CreateAppSession(_pipeInput, _vt100Output);

            // Signal that we have an app context for SendAbovePrompt
            _hasAppContext = true;

            try
            {
                // Invoke interact callback
                await _interact(this).ConfigureAwait(false);
            }
            finally
            {
                _hasAppContext = false;
            }
        }
        catch (EOFException)
        {
            // Client disconnected
            _logger.LogDebug("Client {RemoteAddress} disconnected (EOF)", RemoteAddress);
        }
        catch (Exception ex)
        {
            // ERR-003: Log and swallow - don't crash server
            _logger.LogError(ex, "Error in interact callback for {RemoteAddress}", RemoteAddress);
        }
        finally
        {
            _logger.LogDebug("Connection closed for {RemoteAddress}", RemoteAddress);
            Close();
        }
    }

    /// <summary>
    /// Waits until the connection is ready (TTYPE received).
    /// </summary>
    internal Task WaitUntilReadyAsync() => _ready.Task;

    private void DataReceived(ReadOnlySpan<byte> data)
    {
        // Forward user data to pipe input
        _pipeInput.SendBytes(data);
    }

    private void SizeReceived(int rows, int columns)
    {
        // Clamp values to [1, 500] - handles EC-004 (overflow) and EC-009 (0x0 size)
        rows = Math.Clamp(rows, 1, 500);
        columns = Math.Clamp(columns, 1, 500);

        _size = new Size(rows, columns);

        // Notify running application of resize (FR-017)
        if (_vt100Output != null && _hasAppContext)
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

    private void TtypeReceived(string ttype)
    {
        if (_vt100Output != null)
        {
            // Already set
            return;
        }

        // Create Vt100Output now that we know terminal type
        _vt100Output = Vt100Output.Create(
            _stdout,
            GetSize,
            term: ttype,
            enableCpr: EnableCpr);

        // Signal ready
        _ready.TrySetResult(true);
    }

    private Size GetSize() => _size;
}
