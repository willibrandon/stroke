# Quickstart: Telnet Server

**Feature**: 060-telnet-server
**Date**: 2026-02-03

## Overview

The Telnet Server feature enables running Stroke prompt toolkit applications over the telnet protocol. This allows building network-accessible REPLs, command-line interfaces, and interactive shells.

## Basic Usage

### Minimal Server

```csharp
using Stroke.Contrib.Telnet;
using Stroke.Shortcuts;

async Task InteractAsync(TelnetConnection connection)
{
    connection.Send("Welcome to the Telnet REPL!\n");

    var session = new PromptSession<string>();
    while (true)
    {
        var result = await session.PromptAsync(">>> ");
        connection.Send($"You entered: {result}\n");
    }
}

var server = new TelnetServer(
    host: "0.0.0.0",
    port: 2323,
    interact: InteractAsync);

await server.RunAsync();
```

### Connect with a Client

```bash
telnet localhost 2323
```

## Configuration Options

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `host` | `string` | `"127.0.0.1"` | Bind address |
| `port` | `int` | `23` | Listen port |
| `interact` | `Func<TelnetConnection, Task>` | No-op | Session handler |
| `encoding` | `Encoding` | UTF-8 | Character encoding |
| `style` | `IStyle?` | `null` | Formatted text style |
| `enableCpr` | `bool` | `true` | Cursor position requests |

## Advanced Patterns

### Graceful Shutdown

```csharp
var cts = new CancellationTokenSource();

// Handle Ctrl+C
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    cts.Cancel();
};

var server = new TelnetServer(interact: InteractAsync, port: 2323);

Console.WriteLine("Server starting...");
await server.RunAsync(
    readyCallback: () => Console.WriteLine("Server ready on port 2323"),
    cancellationToken: cts.Token);

Console.WriteLine("Server stopped.");
```

### Send Messages to All Clients

```csharp
var server = new TelnetServer(interact: InteractAsync, port: 2323);

// Background task to broadcast messages
_ = Task.Run(async () =>
{
    while (true)
    {
        await Task.Delay(TimeSpan.FromMinutes(1));
        foreach (var connection in server.Connections)
        {
            connection.SendAbovePrompt("[System] Server heartbeat\n");
        }
    }
});

await server.RunAsync();
```

### Chat Server

```csharp
async Task ChatInteractAsync(TelnetConnection connection)
{
    connection.Send("Enter your name: ");
    var session = new PromptSession<string>();
    var name = await session.PromptAsync("");

    // Broadcast join
    foreach (var c in connection.Server.Connections)
    {
        if (c != connection)
            c.SendAbovePrompt($"[{name} joined]\n");
    }

    while (true)
    {
        var message = await session.PromptAsync($"{name}> ");

        // Broadcast message
        foreach (var c in connection.Server.Connections)
        {
            if (c != connection)
                c.SendAbovePrompt($"{name}: {message}\n");
        }
    }
}
```

### Formatted Output

```csharp
using Stroke.FormattedText;
using Stroke.Styles;

async Task ColorfulInteractAsync(TelnetConnection connection)
{
    // Send formatted text with colors
    connection.Send(new Html("<green>Welcome!</green> Type <b>help</b> for commands.\n"));

    // Using ANSI
    connection.Send(new Ansi("\x1b[1;34mBlue and bold\x1b[0m\n"));
}

var server = new TelnetServer(
    interact: ColorfulInteractAsync,
    style: Style.Default);
```

## Testing

### Unit Test with PipeInput

```csharp
[Fact]
public async Task TelnetServer_AcceptsConnection()
{
    var interactCalled = new TaskCompletionSource<bool>();

    var server = new TelnetServer(
        host: "127.0.0.1",
        port: 0, // Let OS assign port
        interact: async connection =>
        {
            interactCalled.SetResult(true);
            await Task.Delay(100);
        });

    var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
    var serverTask = server.RunAsync(cancellationToken: cts.Token);

    // Connect as client
    using var client = new TcpClient();
    await client.ConnectAsync("127.0.0.1", server.Port);

    Assert.True(await interactCalled.Task);

    cts.Cancel();
    await serverTask;
}
```

## Dependencies

This feature requires:
- `Stroke.Application` - AppSession management
- `Stroke.Output` - Vt100Output for terminal rendering
- `Stroke.Input` - IPipeInput for connection input
- `Stroke.Styles` - Style support for formatted text

## Limitations

- **No Authentication**: Telnet protocol is unencrypted. Use SSH for secure connections.
- **No Flow Control**: Large output may overwhelm slow clients.
- **Single Encoding**: Each server uses one encoding for all connections.

## Migration from Python

| Python | C# |
|--------|-----|
| `TelnetServer(interact=interact)` | `new TelnetServer(interact: InteractAsync)` |
| `server.run()` | `await server.RunAsync()` |
| `connection.send(text)` | `connection.Send(text)` |
| `connection.send_above_prompt(text)` | `connection.SendAbovePrompt(text)` |
| `with create_pipe_input() as input:` | (handled internally) |
