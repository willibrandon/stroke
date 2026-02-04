using System.Collections.Concurrent;
using System.Net;
using System.Text;
using FxSsh;
using FxSsh.Services;
using Microsoft.Extensions.Logging;
using Stroke.Styles;

namespace Stroke.Contrib.Ssh;

/// <summary>
/// SSH server implementation that allows running Stroke applications over the
/// SSH protocol.
/// </summary>
/// <remarks>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>PromptToolkitSSHServer</c> class.
/// It enables network-accessible REPLs, command-line interfaces, and interactive shells
/// over secure SSH connections.
/// </para>
/// <para>
/// Thread safety: This class is thread-safe. The <see cref="Connections"/> set is
/// concurrently accessible, and all public methods can be called from any thread.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// async Task InteractAsync(StrokeSshSession session)
/// {
///     var prompt = new PromptSession&lt;string&gt;();
///     var result = await prompt.PromptAsync("Say something: ");
///     Console.WriteLine($"You said: {result}");
/// }
///
/// var server = new StrokeSshServer(
///     interact: InteractAsync,
///     port: 2222,
///     hostKeyPath: "ssh_host_key");
/// await server.RunAsync();
/// </code>
/// </example>
public class StrokeSshServer
{
    private static readonly ILogger _logger = StrokeLogger.CreateLogger("Stroke.Ssh.Server");

    private readonly Func<StrokeSshSession, Task> _interact;
    private readonly string _hostKeyPath;
    private readonly ConcurrentDictionary<StrokeSshSession, byte> _connections = new();
    private readonly ConcurrentDictionary<SessionChannel, StrokeSshSession> _channelToSession = new();
    private readonly ConcurrentDictionary<SessionChannel, SshChannel> _channelToAdapter = new();
    private readonly List<Task> _sessionTasks = new();
    private readonly Lock _tasksLock = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="StrokeSshServer"/> class.
    /// </summary>
    /// <param name="host">The host address to bind to (default: "127.0.0.1").</param>
    /// <param name="port">The port number to listen on (default: 2222).</param>
    /// <param name="interact">
    /// Async callback invoked for each new connection. The callback receives the
    /// <see cref="StrokeSshSession"/> and should implement the interactive session logic.
    /// </param>
    /// <param name="hostKeyPath">Path to the SSH host key file (PEM format).</param>
    /// <param name="encoding">Character encoding (default: UTF-8).</param>
    /// <param name="style">Optional style for formatted text output.</param>
    /// <param name="enableCpr">Enable cursor position requests (default: true).</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown if <paramref name="port"/> is not in the range 0-65535.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="interact"/> or <paramref name="hostKeyPath"/> is null.
    /// </exception>
    public StrokeSshServer(
        string host = "127.0.0.1",
        int port = 2222,
        Func<StrokeSshSession, Task>? interact = null,
        string? hostKeyPath = null,
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
        _hostKeyPath = hostKeyPath ?? throw new ArgumentNullException(nameof(hostKeyPath), "Host key path is required.");
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
    /// Gets the set of currently active sessions.
    /// </summary>
    /// <remarks>
    /// Thread safety: This set is a snapshot and can be safely enumerated while
    /// sessions are being added or removed.
    /// </remarks>
    public IReadOnlySet<StrokeSshSession> Connections =>
        new HashSet<StrokeSshSession>(_connections.Keys);

    /// <summary>
    /// Runs the SSH server until cancelled.
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
    /// <item>Waits for existing sessions to complete (with 5-second timeout per SC-004)</item>
    /// <item>Force-closes any remaining sessions</item>
    /// </list>
    /// </para>
    /// </remarks>
    public async Task RunAsync(Action? readyCallback = null, CancellationToken cancellationToken = default)
    {
        // Load host key
        var hostKeyPem = await File.ReadAllTextAsync(_hostKeyPath, cancellationToken).ConfigureAwait(false);

        // Create FxSsh server
        var startingInfo = new StartingInfo(IPAddress.Parse(Host), Port, "SSH-2.0-Stroke");
        using var sshServer = new SshServer(startingInfo);

        // Add host key - detect type from content
        AddHostKeyFromPem(sshServer, hostKeyPem);

        // Wire up events
        sshServer.ConnectionAccepted += OnConnectionAccepted;
        // Note: FxSsh has a typo in the event name
        sshServer.ExceptionRasied += OnExceptionRaised;

        try
        {
            // Start the server
            sshServer.Start();

            // Invoke ready callback
            readyCallback?.Invoke();

            // Wait for cancellation
            try
            {
                await Task.Delay(Timeout.Infinite, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // Normal shutdown
            }
        }
        finally
        {
            // Graceful shutdown (FR-012, SC-004)
            // 1. Stop accepting new connections
            sshServer.Stop();

            // 2. Wait for existing sessions to complete (with 5-second timeout)
            Task[] tasksToWait;
            using (_tasksLock.EnterScope())
            {
                tasksToWait = _sessionTasks.ToArray();
            }

            if (tasksToWait.Length > 0)
            {
                try
                {
                    await Task.WhenAll(tasksToWait).WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
                }
                catch (TimeoutException)
                {
                    // 3. Force-close remaining sessions
                    _logger.LogWarning("Some SSH sessions didn't close within 5 seconds, force-closing");
                    foreach (var session in _connections.Keys)
                    {
                        try
                        {
                            session.Close();
                        }
                        catch { }
                    }
                }
                catch (Exception)
                {
                    // Swallow exceptions from session tasks
                }
            }
        }
    }

    /// <summary>
    /// Called when authentication begins for a user.
    /// </summary>
    /// <param name="username">The username attempting to authenticate.</param>
    /// <returns>
    /// <c>true</c> if authentication is required; <c>false</c> to skip authentication.
    /// </returns>
    /// <remarks>
    /// Override this method to implement custom authentication logic.
    /// The default implementation returns <c>false</c> (no authentication required).
    /// </remarks>
    protected virtual bool BeginAuth(string username)
    {
        // Default: no authentication required (matches Python PTK behavior)
        return false;
    }

    /// <summary>
    /// Creates a new session for an incoming connection.
    /// </summary>
    /// <param name="channel">The SSH channel adapter.</param>
    /// <returns>A new session instance.</returns>
    /// <remarks>
    /// Override this method to create custom session types.
    /// </remarks>
    protected virtual StrokeSshSession CreateSession(ISshChannel channel)
    {
        return new StrokeSshSession(channel, _interact, EnableCpr);
    }

    private void OnConnectionAccepted(object? sender, Session session)
    {
        _logger.LogDebug("SSH connection accepted");

        // Wire up service registration to get UserAuthService and ConnectionService
        session.ServiceRegistered += (s, service) => OnServiceRegistered(session, service);
    }

    private void OnServiceRegistered(Session session, SshService service)
    {
        if (service is UserauthService userauthService)
        {
            // Wire up authentication (FR-010)
            userauthService.Userauth += (s, args) =>
            {
                // Call virtual BeginAuth to decide if auth is needed
                var authRequired = BeginAuth(args.Username);

                if (!authRequired)
                {
                    // No auth required - approve
                    args.Result = true;
                }
                else
                {
                    // Auth required - for now, deny (subclasses can override BeginAuth and implement validation)
                    // Future: Add password validation hook
                    args.Result = false;
                    _logger.LogWarning("SSH authentication failed for user {Username} (auth required but not implemented)", args.Username);
                }
            };
        }
        else if (service is ConnectionService connectionService)
        {
            // Wire up terminal events
            connectionService.PtyReceived += OnPtyReceived;
            connectionService.WindowChange += OnWindowChange;
            connectionService.CommandOpened += OnCommandOpened;
        }
    }

    private void OnPtyReceived(object? sender, PtyArgs e)
    {
        // Store initial terminal type and size in the channel adapter
        if (_channelToAdapter.TryGetValue(e.Channel, out var adapter))
        {
            adapter.SetTerminalType(e.Terminal);
            adapter.SetTerminalSize((int)e.WidthChars, (int)e.HeightRows);
        }
        else
        {
            // Channel not yet created - store in temporary dictionary or create adapter now
            var adapter2 = new SshChannel(e.Channel, Encoding);
            adapter2.SetTerminalType(e.Terminal);
            adapter2.SetTerminalSize((int)e.WidthChars, (int)e.HeightRows);
            _channelToAdapter.TryAdd(e.Channel, adapter2);
        }
    }

    private void OnWindowChange(object? sender, WindowChangeArgs e)
    {
        // Update session size
        if (_channelToSession.TryGetValue(e.Channel, out var session))
        {
            session.TerminalSizeChanged((int)e.WidthColumns, (int)e.HeightRows);
        }
    }

    private void OnCommandOpened(object? sender, CommandRequestedArgs e)
    {
        // FxSsh 1.3.0 sends ChannelSuccessMessage before firing this event

        // Get or create the channel adapter
        if (!_channelToAdapter.TryGetValue(e.Channel, out var adapter))
        {
            adapter = new SshChannel(e.Channel, Encoding);
            _channelToAdapter.TryAdd(e.Channel, adapter);
        }

        // Create session via virtual factory method (FR-009)
        var session = CreateSession(adapter);
        _connections.TryAdd(session, 0);
        _channelToSession.TryAdd(e.Channel, session);

        // Wire up channel events
        e.Channel.DataReceived += (s, data) => session.DataReceived(data);
        e.Channel.CloseReceived += (s, _) => OnChannelClosed(e.Channel, session);

        // Start the session
        var task = RunSessionAsync(session, e.Channel);
        using (_tasksLock.EnterScope())
        {
            _sessionTasks.Add(task);
        }
    }

    private async Task RunSessionAsync(StrokeSshSession session, SessionChannel channel)
    {
        try
        {
            await session.RunAsync().ConfigureAwait(false);
        }
        finally
        {
            OnChannelClosed(channel, session);
        }
    }

    private void OnChannelClosed(SessionChannel channel, StrokeSshSession session)
    {
        _logger.LogDebug("SSH channel closed");

        // Clean up mappings
        _connections.TryRemove(session, out _);
        _channelToSession.TryRemove(channel, out _);
        _channelToAdapter.TryRemove(channel, out _);

        // Close session
        session.Close();
    }

    private void OnExceptionRaised(object? sender, Exception e)
    {
        _logger.LogError(e, "SSH server exception");
    }

    private static void AddHostKeyFromPem(SshServer server, string pem)
    {
        // Detect key type from PEM header
        if (pem.Contains("RSA PRIVATE KEY") || pem.Contains("RSA PRIVATE"))
        {
            server.AddHostKey("rsa-sha2-256", pem);
            server.AddHostKey("rsa-sha2-512", pem);
        }
        else if (pem.Contains("EC PRIVATE KEY") || pem.Contains("ECDSA"))
        {
            // Detect curve from key content
            if (pem.Contains("nistp256") || pem.Length < 300)
            {
                server.AddHostKey("ecdsa-sha2-nistp256", pem);
            }
            else if (pem.Contains("nistp384"))
            {
                server.AddHostKey("ecdsa-sha2-nistp384", pem);
            }
            else if (pem.Contains("nistp521"))
            {
                server.AddHostKey("ecdsa-sha2-nistp521", pem);
            }
            else
            {
                // Default to nistp256
                server.AddHostKey("ecdsa-sha2-nistp256", pem);
            }
        }
        else if (pem.Contains("OPENSSH PRIVATE KEY"))
        {
            // OpenSSH format - could be RSA, ECDSA, or Ed25519
            // For now, try RSA (most common)
            server.AddHostKey("rsa-sha2-256", pem);
        }
        else
        {
            // Unknown format - try RSA as fallback
            server.AddHostKey("rsa-sha2-256", pem);
        }
    }

    private static Task DummyInteractAsync(StrokeSshSession session)
    {
        return Task.CompletedTask;
    }
}
