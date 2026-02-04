/// <summary>
/// A simple chat application over telnet.
/// Everyone that connects is asked for their name, and then people can chat with each other.
///
/// This is a faithful port of Python Prompt Toolkit's chat-app.py example.
/// </summary>

using System.Collections.Concurrent;
using Stroke.Contrib.Telnet;
using Stroke.FormattedText;
using Stroke.Shortcuts;

namespace Stroke.Examples.Telnet;

public static class ChatApp
{
    // List of connections.
    private static readonly List<TelnetConnection> _connections = [];
    private static readonly ConcurrentDictionary<TelnetConnection, string> _connectionToColor = new();
    private static readonly Lock _connectionsLock = new();

    private static readonly string[] Colors =
    [
        "ansired",
        "ansigreen",
        "ansiyellow",
        "ansiblue",
        "ansifuchsia",
        "ansiturquoise",
        "ansilightgray",
        "ansidarkgray",
        "ansidarkred",
        "ansidarkgreen",
        "ansibrown",
        "ansidarkblue",
        "ansipurple",
        "ansiteal"
    ];

    public static async Task RunAsync(CancellationToken cancellationToken = default)
    {
        var server = new TelnetServer(interact: InteractAsync, port: 2323);
        await server.RunAsync(cancellationToken: cancellationToken);
    }

    private static async Task InteractAsync(TelnetConnection connection)
    {
        var promptSession = new PromptSession<string>();

        // When a client is connected, erase the screen from the client and say Hello.
        connection.EraseScreen();
        connection.Send("Welcome to our chat application!\n");
        connection.Send("All connected clients will receive what you say.\n");

        var name = await promptSession.PromptAsync(message: "Type your name: ");

        // Random color.
        var color = Colors[Random.Shared.Next(Colors.Length)];
        _connectionToColor[connection] = color;

        // Send 'connected' message.
        SendToEveryone(connection, name, "(connected)", color);

        // Prompt with colored name.
        var promptMsg = new Html($"<reverse fg=\"{color}\">[{name}]</reverse> &gt; ");

        using (_connectionsLock.EnterScope())
        {
            _connections.Add(connection);
        }

        try
        {
            while (!connection.IsClosed)
            {
                try
                {
                    var result = await promptSession.PromptAsync(message: promptMsg);
                    SendToEveryone(connection, name, result, color);
                }
                catch (OperationCanceledException)
                {
                    // Ctrl+C - ignore and continue
                }
            }
        }
        catch (EOFException)
        {
            SendToEveryone(connection, name, "(leaving)", color);
        }
        finally
        {
            using (_connectionsLock.EnterScope())
            {
                _connections.Remove(connection);
            }
            _connectionToColor.TryRemove(connection, out _);
        }
    }

    /// <summary>
    /// Send a message to all the clients.
    /// </summary>
    private static void SendToEveryone(TelnetConnection senderConnection, string name, string message, string color)
    {
        List<TelnetConnection> connectionsCopy;
        using (_connectionsLock.EnterScope())
        {
            connectionsCopy = _connections.ToList();
        }

        foreach (var c in connectionsCopy)
        {
            if (c != senderConnection && !c.IsClosed)
            {
                try
                {
                    // Use Send with formatted text (SendAbovePrompt requires active app context)
                    var formattedMessage = new Stroke.FormattedText.FormattedText([
                        new StyleAndTextTuple($"fg:{color}", $"[{name}]"),
                        new StyleAndTextTuple("", " "),
                        new StyleAndTextTuple($"fg:{color}", $"{message}\n")
                    ]);
                    c.Send(formattedMessage);
                }
                catch
                {
                    // Ignore send errors - client may have disconnected
                }
            }
        }
    }
}
