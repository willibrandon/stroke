# Contract: TelnetServer

**Namespace**: `Stroke.Contrib.Telnet`
**Python Source**: `prompt_toolkit.contrib.telnet.server.TelnetServer`

## Class Signature

```csharp
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
    public TelnetServer(
        string host = "127.0.0.1",
        int port = 23,
        Func<TelnetConnection, Task>? interact = null,
        Encoding? encoding = null,
        IStyle? style = null,
        bool enableCpr = true);

    /// <summary>
    /// Gets the host address this server binds to.
    /// </summary>
    public string Host { get; }

    /// <summary>
    /// Gets the port number this server listens on.
    /// </summary>
    public int Port { get; }

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
    public IReadOnlySet<TelnetConnection> Connections { get; }

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
    public Task RunAsync(Action? readyCallback = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// [Deprecated] Starts the telnet server in the background.
    /// </summary>
    /// <remarks>
    /// Use <see cref="RunAsync"/> instead. This method is provided for API compatibility
    /// with Python Prompt Toolkit.
    /// </remarks>
    [Obsolete("Use RunAsync instead")]
    public void Start();

    /// <summary>
    /// [Deprecated] Stops a telnet server that was started using <see cref="Start"/>.
    /// </summary>
    /// <returns>A task that completes when the server has stopped.</returns>
    /// <remarks>
    /// Use <see cref="RunAsync"/> with cancellation instead. This method is provided
    /// for API compatibility with Python Prompt Toolkit.
    /// </remarks>
    [Obsolete("Use RunAsync with cancellation instead")]
    public Task StopAsync();
}
```

## Functional Requirements Coverage

| Requirement | Method/Property |
|-------------|-----------------|
| FR-001: Accept TCP connections | `RunAsync()` |
| FR-008: Invoke interact callback | Constructor `interact` parameter |
| FR-009: Multiple concurrent connections | `Connections` set |
| FR-010: Track active connections | `Connections` property |
| FR-012: Support cancellation | `RunAsync(cancellationToken)` |
| FR-018: Configurable encoding | `Encoding` property |
| FR-019: Configurable style | `Style` property |
| FR-020: Enable/disable CPR | `EnableCpr` property |

## Python API Mapping

| Python | C# |
|--------|-----|
| `TelnetServer(host, port, interact, encoding, style, enable_cpr)` | Constructor |
| `host` | `Host` |
| `port` | `Port` |
| `encoding` | `Encoding` |
| `style` | `Style` |
| `enable_cpr` | `EnableCpr` |
| `connections` | `Connections` |
| `run(ready_cb)` | `RunAsync(readyCallback, cancellationToken)` |
| `start()` | `Start()` [Obsolete] |
| `stop()` | `StopAsync()` [Obsolete] |
