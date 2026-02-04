/// <summary>
/// Example of a telnet application that displays a bottom toolbar and completions in the prompt.
///
/// This is a faithful port of Python Prompt Toolkit's toolbar.py example.
/// </summary>

using Stroke.Completion;
using Stroke.Contrib.Telnet;
using Stroke.Shortcuts;

namespace Stroke.Examples.Telnet;

public static class Toolbar
{
    public static async Task RunAsync(CancellationToken cancellationToken = default)
    {
        var server = new TelnetServer(interact: InteractAsync, port: 2323);
        await server.RunAsync(cancellationToken: cancellationToken);
    }

    private static async Task InteractAsync(TelnetConnection connection)
    {
        // When a client is connected, erase the screen from the client and say Hello.
        connection.Send("Welcome!\n");

        // Display prompt with bottom toolbar.
        var animalCompleter = new WordCompleter(["alligator", "ant"]);

        var session = new PromptSession<string>();
        var result = await session.PromptAsync(
            message: "Say something: ",
            bottomToolbar: "Bottom toolbar...",
            completer: animalCompleter);

        connection.Send($"You said: {result}\n");
        connection.Send("Bye.\n");
    }
}
