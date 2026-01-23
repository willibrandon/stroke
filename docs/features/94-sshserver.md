# Feature 94: SSH Server

## Overview

Implement an SSH server integration that allows running prompt toolkit applications over SSH connections. This leverages SSH.NET for the underlying SSH protocol handling.

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/contrib/ssh/server.py`

Note: Python uses `asyncssh` library. The C# port will use SSH.NET.

## Public API

### PromptToolkitSshServer

```csharp
namespace Stroke.Contrib.Ssh;

/// <summary>
/// Run prompt_toolkit applications over an SSH server.
/// Each connection gets its own AppSession for concurrent UI interactions.
/// </summary>
public sealed class PromptToolkitSshServer
{
    /// <summary>
    /// Create an SSH server for prompt toolkit applications.
    /// </summary>
    /// <param name="interact">Async callback for each session.</param>
    /// <param name="enableCpr">Enable cursor position requests.</param>
    public PromptToolkitSshServer(
        Func<PromptToolkitSshSession, Task> interact,
        bool enableCpr = true);

    /// <summary>
    /// The interact callback for sessions.
    /// </summary>
    public Func<PromptToolkitSshSession, Task> Interact { get; }

    /// <summary>
    /// Whether cursor position requests are enabled.
    /// </summary>
    public bool EnableCpr { get; }

    /// <summary>
    /// Called when authentication begins.
    /// Override to implement custom authentication.
    /// </summary>
    /// <param name="username">The username attempting to authenticate.</param>
    /// <returns>True to require authentication, false to allow without auth.</returns>
    public virtual bool BeginAuth(string username);

    /// <summary>
    /// Called when a session is requested.
    /// </summary>
    /// <returns>A new SSH session.</returns>
    public virtual PromptToolkitSshSession CreateSession();
}
```

### PromptToolkitSshSession

```csharp
namespace Stroke.Contrib.Ssh;

/// <summary>
/// Represents a single SSH session running a prompt toolkit application.
/// </summary>
public sealed class PromptToolkitSshSession
{
    /// <summary>
    /// The interact callback for this session.
    /// </summary>
    public Func<PromptToolkitSshSession, Task> Interact { get; }

    /// <summary>
    /// Whether cursor position requests are enabled.
    /// </summary>
    public bool EnableCpr { get; }

    /// <summary>
    /// The app session for this SSH session.
    /// </summary>
    public AppSession? AppSession { get; }

    /// <summary>
    /// The interact task if running.
    /// </summary>
    public Task? InteractTask { get; }

    /// <summary>
    /// Get the current terminal size.
    /// </summary>
    public Size GetSize();

    /// <summary>
    /// Called when connection is established.
    /// </summary>
    /// <param name="channel">The SSH channel.</param>
    internal void ConnectionMade(ISshChannel channel);

    /// <summary>
    /// Called when shell is requested.
    /// </summary>
    /// <returns>True to accept shell request.</returns>
    internal bool ShellRequested();

    /// <summary>
    /// Called when session starts.
    /// </summary>
    internal void SessionStarted();

    /// <summary>
    /// Called when terminal size changes.
    /// </summary>
    /// <param name="width">New width in columns.</param>
    /// <param name="height">New height in rows.</param>
    internal void TerminalSizeChanged(int width, int height);

    /// <summary>
    /// Called when data is received from client.
    /// </summary>
    /// <param name="data">The received data.</param>
    internal void DataReceived(string data);
}
```

### ISshChannel

```csharp
namespace Stroke.Contrib.Ssh;

/// <summary>
/// Abstraction over SSH channel for testability.
/// </summary>
public interface ISshChannel
{
    /// <summary>
    /// Write data to the channel.
    /// </summary>
    /// <param name="data">Data to write.</param>
    void Write(string data);

    /// <summary>
    /// Close the channel.
    /// </summary>
    void Close();

    /// <summary>
    /// Get the terminal type.
    /// </summary>
    /// <returns>Terminal type string (e.g., "xterm-256color").</returns>
    string GetTerminalType();

    /// <summary>
    /// Get the current terminal size.
    /// </summary>
    /// <returns>Tuple of (width, height, pixWidth, pixHeight).</returns>
    (int Width, int Height, int PixWidth, int PixHeight) GetTerminalSize();

    /// <summary>
    /// Get the encoding for this channel.
    /// </summary>
    /// <returns>The encoding name.</returns>
    string GetEncoding();

    /// <summary>
    /// Set line mode on/off.
    /// </summary>
    /// <param name="enabled">Whether line mode is enabled.</param>
    void SetLineMode(bool enabled);
}
```

## Project Structure

```
src/Stroke/
└── Contrib/
    └── Ssh/
        ├── PromptToolkitSshServer.cs
        ├── PromptToolkitSshSession.cs
        └── ISshChannel.cs
tests/Stroke.Tests/
└── Contrib/
    └── Ssh/
        └── SshServerTests.cs
```

## Implementation Notes

### SshSession Implementation

```csharp
public sealed class PromptToolkitSshSession
{
    private ISshChannel? _channel;
    private PipeInput? _input;
    private Vt100Output? _output;

    public PromptToolkitSshSession(
        Func<PromptToolkitSshSession, Task> interact,
        bool enableCpr)
    {
        Interact = interact;
        EnableCpr = enableCpr;
    }

    public Size GetSize()
    {
        if (_channel == null)
            return new Size(79, 20);

        var (width, height, _, _) = _channel.GetTerminalSize();
        return new Size(width, height);
    }

    internal void ConnectionMade(ISshChannel channel)
    {
        _channel = channel;
    }

    internal bool ShellRequested() => true;

    internal void SessionStarted()
    {
        InteractTask = Task.Run(InteractAsync);
    }

    private async Task InteractAsync()
    {
        if (_channel == null)
            throw new InvalidOperationException("Session started before connection made");

        // Disable SSH library line editing
        _channel.SetLineMode(false);

        var term = _channel.GetTerminalType();

        // Create stdout wrapper that writes to SSH channel
        var stdout = new SshChannelStdout(_channel);

        _output = new Vt100Output(stdout, GetSize, term, EnableCpr);

        using (_input = PipeInput.Create())
        using (var session = AppSession.Create(_input, _output))
        {
            AppSession = session;
            try
            {
                await Interact(this);
            }
            finally
            {
                _channel.Close();
            }
        }
    }

    internal void TerminalSizeChanged(int width, int height)
    {
        // Notify the application of resize
        AppSession?.App?._OnResize();
    }

    internal void DataReceived(string data)
    {
        _input?.SendText(data);
    }
}
```

### SshChannelStdout Wrapper

```csharp
internal sealed class SshChannelStdout : TextWriter
{
    private readonly ISshChannel _channel;

    public SshChannelStdout(ISshChannel channel)
    {
        _channel = channel;
    }

    public override Encoding Encoding => Encoding.UTF8;

    public override void Write(string? value)
    {
        if (value == null) return;

        try
        {
            // Convert LF to CRLF
            _channel.Write(value.Replace("\n", "\r\n"));
        }
        catch (IOException)
        {
            // Channel closed
        }
    }

    public override void Flush() { }

    public bool IsAtty => true;
}
```

### Usage Example

```csharp
async Task Interact(PromptToolkitSshSession session)
{
    // Show a dialog
    var result = await YesNoDialog.ShowAsync("Welcome", "Ready to continue?");

    // Run a prompt session
    var promptSession = new PromptSession();
    var text = await promptSession.PromptAsync("Enter command: ");

    Console.WriteLine($"You entered: {text}");
}

// Create the server
var server = new PromptToolkitSshServer(Interact, enableCpr: true);

// Start SSH server with SSH.NET
await SshServer.CreateAsync(
    () => server,
    port: 2222,
    hostKeys: new[] { "/etc/ssh/ssh_host_rsa_key" }
);
```

### Integration with SSH.NET

```csharp
// Example adapter for SSH.NET library
public class SshNetChannelAdapter : ISshChannel
{
    private readonly SshChannel _channel;

    public SshNetChannelAdapter(SshChannel channel)
    {
        _channel = channel;
    }

    public void Write(string data) => _channel.Write(Encoding.UTF8.GetBytes(data));
    public void Close() => _channel.Close();
    public string GetTerminalType() => _channel.TerminalType;

    public (int Width, int Height, int PixWidth, int PixHeight) GetTerminalSize()
        => (_channel.TerminalColumns, _channel.TerminalRows, 0, 0);

    public string GetEncoding() => "utf-8";
    public void SetLineMode(bool enabled) => _channel.LineMode = enabled;
}
```

### Concurrent Sessions

```csharp
// Each SSH connection gets its own:
// 1. PromptToolkitSshSession instance
// 2. PipeInput for feeding keystrokes
// 3. Vt100Output for rendering
// 4. AppSession context

// This allows multiple users to run the application simultaneously
// with isolated state and independent UI
```

## Dependencies

- Feature 1: Document model
- Feature 3: Application
- Feature 5: Input abstraction (PipeInput)
- Feature 6: VT100 Output
- SSH.NET library (or similar)

## Implementation Tasks

1. Define ISshChannel abstraction
2. Implement PromptToolkitSshSession
3. Implement SshChannelStdout wrapper
4. Implement PromptToolkitSshServer
5. Handle terminal size change notifications
6. Integrate with PipeInput for keyboard
7. Create AppSession per connection
8. Handle session cleanup
9. Write unit tests

## Acceptance Criteria

- [ ] SSH connections create isolated sessions
- [ ] Terminal type detected correctly
- [ ] Terminal size reported and updated
- [ ] Keyboard input routed to PipeInput
- [ ] VT100 output sent to SSH channel
- [ ] LF converted to CRLF
- [ ] Cursor position requests work if enabled
- [ ] Multiple concurrent sessions supported
- [ ] Session cleanup on disconnect
- [ ] Unit tests achieve 80% coverage
