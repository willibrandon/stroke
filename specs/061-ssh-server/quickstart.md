# Quickstart: SSH Server Integration

**Feature**: 061-ssh-server
**Created**: 2026-02-03

## Prerequisites

1. .NET 10+ SDK installed
2. SSH host key
3. Stroke library referenced

## Generate SSH Host Key

```bash
# Must use PEM format for FxSsh compatibility
ssh-keygen -t rsa -m PEM -f ssh_host_key -N ""
```

## Minimal Example

```csharp
using Stroke.Contrib.Ssh;
using Stroke.Shortcuts;

// Define interaction callback
async Task InteractAsync(PromptToolkitSshSession session)
{
    var prompt = new PromptSession<string>();
    var name = await prompt.PromptAsync("What is your name? ");
    Console.WriteLine($"Hello, {name}!");
}

// Create and run server
var server = new PromptToolkitSshServer(
    port: 2222,
    interact: InteractAsync,
    hostKeyPath: "ssh_host_key"
);

Console.WriteLine("Starting SSH server on port 2222...");
Console.WriteLine("Press Ctrl+C to stop.");

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };

await server.RunAsync(
    readyCallback: () => Console.WriteLine("Server ready!"),
    cancellationToken: cts.Token
);
```

## Connect to Server

```bash
ssh -p 2222 user@localhost
```

## Running the Example

```bash
# Navigate to examples directory
cd examples/Stroke.Examples.Ssh

# Run the SSH server example (port of asyncssh-server.py)
dotnet run -- asyncssh-server
```

This example demonstrates:
- Progress bar dialog over SSH
- Normal prompt with input
- Autocompletion (animal names)
- Yes/no dialog
- Input dialog

## Key Concepts

### Session Isolation

Each SSH connection gets its own:
- `PipeInput` for keyboard input
- `Vt100Output` for terminal rendering
- `AppSession` for application context

This means multiple users can connect and interact independently.

### Terminal Size

The session tracks terminal dimensions. Access via:

```csharp
var size = session.GetSize();
Console.WriteLine($"Terminal: {size.Columns}x{size.Rows}");
```

### LF to CRLF Conversion

Output automatically converts `\n` to `\r\n` per NVT specification. Write naturally:

```csharp
output.Write("Line 1\nLine 2\n");  // Sends: "Line 1\r\nLine 2\r\n"
```

## Common Patterns

### Ready Callback

Wait for server to be listening before connecting:

```csharp
var serverReady = new ManualResetEventSlim();

var serverTask = server.RunAsync(
    readyCallback: () => serverReady.Set(),
    cancellationToken: cts.Token
);

serverReady.Wait();
Console.WriteLine($"Server listening on port {server.Port}");
```

### Graceful Shutdown

```csharp
var cts = new CancellationTokenSource();

// Handle Ctrl+C
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    cts.Cancel();
};

// Server will stop accepting new connections
// and wait for existing sessions to complete (with timeout)
await server.RunAsync(cancellationToken: cts.Token);
```

### Port 0 (Auto-Assign)

```csharp
var server = new PromptToolkitSshServer(port: 0, ...);

await server.RunAsync(readyCallback: () =>
{
    // Port is now assigned
    Console.WriteLine($"Listening on port {server.Port}");
});
```

## Troubleshooting

### "Permission denied" on port 22

Use port 2222 or higher—port 22 requires root/admin privileges.

### "Host key not found"

Ensure `hostKeyPath` points to a valid private key file (not the `.pub` file).

### Client says "Host key verification failed"

First connection to new server—accept the host key or add it to known_hosts.

### No output on client

Ensure you're using `output.Flush()` after writing. The `Vt100Output` buffers writes for efficiency.
