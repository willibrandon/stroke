# API Contracts: SSH Server Integration

**Feature**: 061-ssh-server
**Created**: 2026-02-03

## Namespace: Stroke.Contrib.Ssh

---

## PromptToolkitSshServer

Main SSH server class that creates isolated sessions for incoming connections.

```csharp
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
public class PromptToolkitSshServer
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PromptToolkitSshServer"/> class.
    /// </summary>
    /// <param name="host">The host address to bind to (default: "127.0.0.1").</param>
    /// <param name="port">The port number to listen on (default: 2222).</param>
    /// <param name="interact">
    /// Async callback invoked for each new connection. The callback receives the
    /// <see cref="PromptToolkitSshSession"/> and should implement the interactive session logic.
    /// </param>
    /// <param name="hostKeyPath">Path to the SSH host key file (PEM format).</param>
    /// <param name="encoding">Character encoding (default: UTF-8).</param>
    /// <param name="style">Optional style for formatted text output.</param>
    /// <param name="enableCpr">Enable cursor position requests (default: true).</param>
    public PromptToolkitSshServer(
        string host = "127.0.0.1",
        int port = 2222,
        Func<PromptToolkitSshSession, Task>? interact = null,
        string? hostKeyPath = null,
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
    /// Gets the set of currently active sessions.
    /// </summary>
    public IReadOnlySet<PromptToolkitSshSession> Connections { get; }

    /// <summary>
    /// Runs the SSH server until cancelled.
    /// </summary>
    /// <param name="readyCallback">
    /// Optional callback invoked when the server is ready to accept connections.
    /// </param>
    /// <param name="cancellationToken">Token to stop the server.</param>
    public Task RunAsync(Action? readyCallback = null, CancellationToken cancellationToken = default);

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
    protected virtual bool BeginAuth(string username);

    /// <summary>
    /// Creates a new session for an incoming connection.
    /// </summary>
    /// <param name="channel">The SSH channel for the connection.</param>
    /// <returns>A new session instance.</returns>
    /// <remarks>
    /// Override this method to create custom session types.
    /// </remarks>
    protected virtual PromptToolkitSshSession CreateSession(ISshChannel channel);
}
```

---

## PromptToolkitSshSession

Represents a single SSH session with isolated I/O.

```csharp
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
public class PromptToolkitSshSession
{
    /// <summary>
    /// Gets the interact callback for this session.
    /// </summary>
    public Func<PromptToolkitSshSession, Task> Interact { get; }

    /// <summary>
    /// Gets whether cursor position requests are enabled.
    /// </summary>
    public bool EnableCpr { get; }

    /// <summary>
    /// Gets the current application session, or null if not yet started.
    /// </summary>
    public AppSession? AppSession { get; }

    /// <summary>
    /// Gets the running interact task, or null if not started.
    /// </summary>
    public Task? InteractTask { get; }

    /// <summary>
    /// Gets the current terminal size.
    /// </summary>
    /// <returns>
    /// The terminal size, or (79, 20) if not yet negotiated.
    /// </returns>
    public Size GetSize();

    /// <summary>
    /// Called when data is received from the SSH client.
    /// </summary>
    /// <param name="data">The received data as a string.</param>
    /// <remarks>
    /// Routes data to the session's PipeInput for keyboard handling.
    /// </remarks>
    public void DataReceived(string data);

    /// <summary>
    /// Called when the terminal size changes.
    /// </summary>
    /// <param name="width">New terminal width in columns.</param>
    /// <param name="height">New terminal height in rows.</param>
    public void TerminalSizeChanged(int width, int height);
}
```

---

## ISshChannel

Abstraction interface for SSH channel operations (FR-014).

```csharp
/// <summary>
/// Abstraction over SSH channel operations for testability.
/// </summary>
/// <remarks>
/// This interface enables testing without actual SSH connections by providing
/// a mockable abstraction over channel operations.
/// </remarks>
public interface ISshChannel
{
    /// <summary>
    /// Writes data to the SSH channel.
    /// </summary>
    /// <param name="data">The data to send.</param>
    void Write(string data);

    /// <summary>
    /// Closes the SSH channel.
    /// </summary>
    void Close();

    /// <summary>
    /// Gets the negotiated terminal type.
    /// </summary>
    /// <returns>Terminal type string (e.g., "xterm", "vt100").</returns>
    string GetTerminalType();

    /// <summary>
    /// Gets the current terminal size.
    /// </summary>
    /// <returns>Tuple of (width, height) in columns/rows.</returns>
    (int Width, int Height) GetTerminalSize();

    /// <summary>
    /// Gets the channel encoding.
    /// </summary>
    /// <returns>The character encoding for the channel.</returns>
    Encoding GetEncoding();

    /// <summary>
    /// Sets the line editing mode.
    /// </summary>
    /// <param name="enabled">Whether line mode is enabled.</param>
    /// <remarks>
    /// For SSH, this is typically a no-op as SSH doesn't have a built-in line mode
    /// like Telnet. The Stroke application handles line editing.
    /// </remarks>
    void SetLineMode(bool enabled);
}
```

---

## SshChannelStdout

TextWriter wrapper for SSH channel with LF→CRLF conversion.

```csharp
/// <summary>
/// TextWriter that routes output to an SSH channel with NVT line ending conversion.
/// </summary>
/// <remarks>
/// <para>
/// Converts LF to CRLF per NVT (Network Virtual Terminal) specification.
/// Reports as a TTY for proper terminal detection by Vt100Output.
/// </para>
/// <para>
/// Thread safety: This class is thread-safe. All write operations are atomic.
/// </para>
/// </remarks>
internal sealed class SshChannelStdout : TextWriter
{
    /// <summary>
    /// Initializes a new instance wrapping the specified channel.
    /// </summary>
    /// <param name="channel">The SSH channel to write to.</param>
    public SshChannelStdout(ISshChannel channel);

    /// <summary>
    /// Gets the encoding for this writer.
    /// </summary>
    public override Encoding Encoding { get; }

    /// <summary>
    /// Gets whether this writer is connected to a terminal.
    /// </summary>
    /// <returns>Always <c>true</c> for SSH sessions.</returns>
    public bool IsAtty { get; }

    /// <summary>
    /// Writes a string with LF→CRLF conversion.
    /// </summary>
    public override void Write(string? value);

    /// <summary>
    /// Writes a character with LF→CRLF conversion.
    /// </summary>
    public override void Write(char value);

    /// <summary>
    /// Flushes the writer (no-op for SSH).
    /// </summary>
    public override void Flush();
}
```

---

## Usage Examples

### Basic SSH Server

```csharp
async Task InteractAsync(PromptToolkitSshSession session)
{
    // Access the app session for printing
    var output = AppContext.GetOutput();
    output.Write("Welcome to the SSH server!\n");
    output.Flush();

    var promptSession = new PromptSession<string>();
    var result = await promptSession.PromptAsync("Enter your name: ");

    output.Write($"Hello, {result}!\n");
    output.Flush();
}

// Create and run server
var server = new PromptToolkitSshServer(
    port: 2222,
    interact: InteractAsync,
    hostKeyPath: "ssh_host_key"
);

await server.RunAsync(
    readyCallback: () => Console.WriteLine("SSH server ready on port 2222"),
    cancellationToken: cts.Token
);
```

### Custom Authentication

```csharp
public class AuthenticatedSshServer : PromptToolkitSshServer
{
    private readonly Dictionary<string, string> _users;

    public AuthenticatedSshServer(Dictionary<string, string> users, ...)
        : base(...)
    {
        _users = users;
    }

    protected override bool BeginAuth(string username)
    {
        // Return true to require authentication
        return _users.ContainsKey(username);
    }
}
```

### Multi-User Chat Server

```csharp
var sessions = new ConcurrentBag<PromptToolkitSshSession>();

async Task ChatInteract(PromptToolkitSshSession session)
{
    sessions.Add(session);

    try
    {
        while (true)
        {
            var message = await promptSession.PromptAsync("> ");

            // Broadcast to all connected sessions
            foreach (var other in sessions.Where(s => s != session))
            {
                other.SendAbovePrompt($"[{username}]: {message}");
            }
        }
    }
    finally
    {
        sessions.TryTake(out _);
    }
}
```
