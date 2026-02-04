/// <summary>
/// A simple Telnet application that asks for input and responds.
///
/// This is a faithful port of Python Prompt Toolkit's hello-world.py example.
/// </summary>

using Stroke.Contrib.Telnet;
using Stroke.Shortcuts;

namespace Stroke.Examples.Telnet;

public static class HelloWorld
{
    public static async Task RunAsync(CancellationToken cancellationToken = default)
    {
        var server = new TelnetServer(interact: InteractAsync, port: 2323);
        await server.RunAsync(cancellationToken: cancellationToken);
    }

    private static async Task InteractAsync(TelnetConnection connection)
    {
        connection.EraseScreen();
        connection.Send("Welcome!\n");

        // Ask for input.
        var session = new PromptSession<string>();
        var result = await session.PromptAsync(message: "Say something: ");

        // Send output.
        connection.Send($"You said: {result}\n");
        connection.Send("Bye.\n");
    }
}
