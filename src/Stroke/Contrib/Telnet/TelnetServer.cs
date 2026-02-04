using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Logging;
using Stroke.Input.Pipe;
using Stroke.Styles;

namespace Stroke.Contrib.Telnet;

/// <summary>
/// Telnet server implementation that allows running Stroke applications over the
/// telnet protocol.
/// </summary>
/// <remarks>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>TelnetServer</c> class.
/// It enables network-accessible REPLs, command-line interfaces, and interactive shells.
/// </para>
/// <para>
/// Thread safety: This class is thread-safe. The <see cref="Connections"/> set is
/// concurrently accessible, and all public methods can be called from any thread.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// async Task InteractAsync(TelnetConnection connection)
/// {
///     connection.Send("Welcome!\n");
///     var session = new PromptSession&lt;string&gt;();
///     var result = await session.PromptAsync("Say something: ");
///     connection.Send($"You said: {result}\n");
/// }
///
/// var server = new TelnetServer(interact: InteractAsync, port: 2323);
/// await server.RunAsync();
/// </code>
/// </example>
public sealed class TelnetServer
{
    private static readonly ILogger _logger = StrokeLogger.CreateLogger("Stroke.Telnet.Server");

    private readonly Func<TelnetConnection, Task> _interact;
    private readonly ConcurrentDictionary<TelnetConnection, byte> _connections = new();
    private readonly List<Task> _connectionTasks = new();
    private readonly Lock _tasksLock = new();
    private CancellationTokenSource? _runCts;
    private Task? _runTask;

    /// <summary>
    /// Initializes a new instance of the <see cref="TelnetServer"/> class.
    /// </summary>
    /// <param name="host">The host address to bind to (default: "127.0.0.1").</param>
    /// <param name="port">The port number to listen on (default: 23).</param>
    /// <param name="interact">
    /// Async callback invoked for each new connection. The callback receives the
    /// <see cref="TelnetConnection"/> and should implement the interactive session logic.
    /// </param>
    /// <param name="encoding">Character encoding (default: UTF-8).</param>
    /// <param name="style">Optional style for formatted text output.</param>
    /// <param name="enableCpr">Enable cursor position requests (default: true).</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown if <paramref name="port"/> is not in the range 1-65535.
    /// </exception>
    public TelnetServer(
        string host = "127.0.0.1",
        int port = 23,
        Func<TelnetConnection, Task>? interact = null,
        Encoding? encoding = null,
        IStyle? style = null,
        bool enableCpr = true)
    {
        if (port < 0 || port > 65535)
        {
            throw new ArgumentOutOfRangeException(nameof(port), port, "Port must be between 0 and 65535.");
        }

        Host = host ?? "127.0.0.1";
        Port = port;
        _interact = interact ?? DummyInteractAsync;
        Encoding = encoding ?? Encoding.UTF8;
        Style = style;
        EnableCpr = enableCpr;
    }

    /// <summary>
    /// Gets the host address this server binds to.
    /// </summary>
    public string Host { get; }

    /// <summary>
    /// Gets the port number this server listens on.
    /// </summary>
    public int Port { get; private set; }

    /// <summary>
    /// Gets the character encoding used for this server.
    /// </summary>
    public Encoding Encoding { get; }

    /// <summary>
    /// Gets the style used for formatted text output.
    /// </summary>
    public IStyle? Style { get; }

    /// <summary>
    /// Gets whether cursor position requests are enabled.
    /// </summary>
    public bool EnableCpr { get; }

    /// <summary>
    /// Gets the set of currently active connections.
    /// </summary>
    /// <remarks>
    /// Thread safety: This set is a snapshot and can be safely enumerated while
    /// connections are being added or removed.
    /// </remarks>
    public IReadOnlySet<TelnetConnection> Connections =>
        new HashSet<TelnetConnection>(_connections.Keys);

    /// <summary>
    /// Runs the telnet server until cancelled.
    /// </summary>
    /// <param name="readyCallback">
    /// Optional callback invoked when the server is ready to accept connections.
    /// </param>
    /// <param name="cancellationToken">Token to stop the server.</param>
    /// <returns>A task that completes when the server is stopped.</returns>
    /// <remarks>
    /// <para>
    /// This method blocks until the <paramref name="cancellationToken"/> is cancelled.
    /// When cancelled, the server:
    /// <list type="number">
    /// <item>Stops accepting new connections</item>
    /// <item>Cancels all running connection tasks</item>
    /// <item>Waits for all connections to complete cleanup</item>
    /// <item>Closes the listening socket</item>
    /// </list>
    /// </para>
    /// </remarks>
    public async Task RunAsync(Action? readyCallback = null, CancellationToken cancellationToken = default)
    {
        using var socket = CreateSocket(Host, Port);

        // If port was 0, get the actual assigned port
        if (Port == 0)
        {
            Port = ((IPEndPoint)socket.LocalEndPoint!).Port;
        }

        // Invoke ready callback (API-001)
        readyCallback?.Invoke();

        try
        {
            // Accept loop
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var client = await socket.AcceptAsync(cancellationToken).ConfigureAwait(false);
                    _ = HandleConnectionAsync(client, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    // Normal shutdown
                    break;
                }
                catch (SocketException)
                {
                    // Socket closed during shutdown
                    break;
                }
            }
        }
        finally
        {
            // TS-007: Shutdown sequence
            // 1. Stop accepting new connections (socket closed by using)

            // 2. Cancel in-progress connections
            Task[] tasksToWait;
            using (_tasksLock.EnterScope())
            {
                tasksToWait = _connectionTasks.ToArray();
            }

            // 3. Wait for all connections to complete (with timeout)
            if (tasksToWait.Length > 0)
            {
                try
                {
                    await Task.WhenAll(tasksToWait).WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
                }
                catch (TimeoutException)
                {
                    // Some connections didn't close in time - continue anyway
                }
                catch (Exception)
                {
                    // Swallow exceptions from connection tasks
                }
            }

            // 4. Close listening socket (handled by using)
        }
    }

    /// <summary>
    /// [Deprecated] Starts the telnet server in the background.
    /// </summary>
    /// <remarks>
    /// Use <see cref="RunAsync"/> instead. This method is provided for API compatibility
    /// with Python Prompt Toolkit.
    /// </remarks>
    [Obsolete("Use RunAsync instead")]
    public void Start()
    {
        if (_runTask != null)
        {
            return;
        }

        _runCts = new CancellationTokenSource();
        _runTask = RunAsync(cancellationToken: _runCts.Token);
    }

    /// <summary>
    /// [Deprecated] Stops a telnet server that was started using <see cref="Start"/>.
    /// </summary>
    /// <returns>A task that completes when the server has stopped.</returns>
    /// <remarks>
    /// Use <see cref="RunAsync"/> with cancellation instead. This method is provided
    /// for API compatibility with Python Prompt Toolkit.
    /// </remarks>
    [Obsolete("Use RunAsync with cancellation instead")]
    public async Task StopAsync()
    {
        if (_runCts == null || _runTask == null)
        {
            return;
        }

        _runCts.Cancel();

        try
        {
            await _runTask.ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // Expected
        }
        finally
        {
            _runCts.Dispose();
            _runCts = null;
            _runTask = null;
        }
    }

    private static Socket CreateSocket(string host, int port)
    {
        var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        socket.Bind(new IPEndPoint(IPAddress.Parse(host), port));
        socket.Listen(50);
        return socket;
    }

    private async Task HandleConnectionAsync(Socket clientSocket, CancellationToken cancellationToken)
    {
        var remoteEndPoint = (IPEndPoint)clientSocket.RemoteEndPoint!;
        TelnetConnection? connection = null;

        var task = Task.CompletedTask;

        try
        {
            // Create pipe input for this connection (ISO-001)
            using var pipeInput = new SimplePipeInput();

            connection = new TelnetConnection(
                clientSocket,
                remoteEndPoint,
                _interact,
                this,
                Encoding,
                Style,
                pipeInput,
                EnableCpr);

            // Add to connections set
            _connections.TryAdd(connection, 0);

            // Track task
            task = RunConnectionAsync(connection, clientSocket, cancellationToken);
            using (_tasksLock.EnterScope())
            {
                _connectionTasks.Add(task);
            }

            // Send initialization sequences (FR-002)
            SendInitializationSequences(clientSocket);

            // Run the connection
            await task.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            // ERR-003: Log but don't crash
            _logger.LogError(ex, "Error handling connection from {RemoteEndPoint}", remoteEndPoint);
        }
        finally
        {
            if (connection != null)
            {
                _connections.TryRemove(connection, out _);
                connection.Close();
            }

            using (_tasksLock.EnterScope())
            {
                _connectionTasks.Remove(task);
            }
        }
    }

    private async Task RunConnectionAsync(TelnetConnection connection, Socket clientSocket, CancellationToken cancellationToken)
    {
        // Start receiving data
        var receiveTask = ReceiveDataAsync(connection, clientSocket, cancellationToken);

        // Run the application
        var appTask = connection.RunApplicationAsync();

        // Wait for either to complete
        await Task.WhenAny(receiveTask, appTask).ConfigureAwait(false);

        // Close connection (will cause the other task to complete)
        connection.Close();

        // Wait for both to finish
        try
        {
            await Task.WhenAll(receiveTask, appTask).ConfigureAwait(false);
        }
        catch
        {
            // Swallow exceptions from cleanup
        }
    }

    private static async Task ReceiveDataAsync(TelnetConnection connection, Socket socket, CancellationToken cancellationToken)
    {
        var buffer = new byte[1024];

        try
        {
            while (!cancellationToken.IsCancellationRequested && !connection.IsClosed)
            {
                var received = await socket.ReceiveAsync(buffer, SocketFlags.None, cancellationToken).ConfigureAwait(false);

                if (received == 0)
                {
                    // Connection closed by client
                    break;
                }

                connection.Feed(buffer.AsSpan(0, received));
            }
        }
        catch (OperationCanceledException)
        {
            // Normal shutdown
        }
        catch (SocketException)
        {
            // Connection closed
        }
        catch (ObjectDisposedException)
        {
            // Socket disposed
        }
    }

    private static void SendInitializationSequences(Socket socket)
    {
        // FR-002: Send 7 initialization sequences
        // 1. IAC DO LINEMODE
        socket.Send([TelnetConstants.IAC, TelnetConstants.DO, TelnetConstants.LINEMODE]);

        // 2. IAC WILL SUPPRESS_GO_AHEAD
        socket.Send([TelnetConstants.IAC, TelnetConstants.WILL, TelnetConstants.SUPPRESS_GO_AHEAD]);

        // 3. IAC SB LINEMODE MODE 0 IAC SE
        socket.Send([TelnetConstants.IAC, TelnetConstants.SB, TelnetConstants.LINEMODE,
                     TelnetConstants.MODE, 0, TelnetConstants.IAC, TelnetConstants.SE]);

        // 4. IAC WILL ECHO
        socket.Send([TelnetConstants.IAC, TelnetConstants.WILL, TelnetConstants.ECHO]);

        // 5. IAC DO NAWS
        socket.Send([TelnetConstants.IAC, TelnetConstants.DO, TelnetConstants.NAWS]);

        // 6. IAC DO TTYPE
        socket.Send([TelnetConstants.IAC, TelnetConstants.DO, TelnetConstants.TTYPE]);

        // 7. IAC SB TTYPE SEND IAC SE
        socket.Send([TelnetConstants.IAC, TelnetConstants.SB, TelnetConstants.TTYPE,
                     TelnetConstants.SEND, TelnetConstants.IAC, TelnetConstants.SE]);
    }

    private static Task DummyInteractAsync(TelnetConnection connection)
    {
        return Task.CompletedTask;
    }
}
