/// <summary>
/// Example of a telnet application that displays a dialog window.
///
/// This is a faithful port of Python Prompt Toolkit's dialog.py example.
/// </summary>

using Stroke.Contrib.Telnet;
using Stroke.Shortcuts;

namespace Stroke.Examples.Telnet;

public static class Dialog
{
    public static async Task RunAsync(CancellationToken cancellationToken = default)
    {
        var server = new TelnetServer(interact: InteractAsync, port: 2323);
        await server.RunAsync(cancellationToken: cancellationToken);
    }

    private static async Task InteractAsync(TelnetConnection connection)
    {
        var result = await Dialogs.YesNoDialogAsync(
            title: "Yes/no dialog demo",
            text: "Press yes or no");

        connection.Send($"You said: {result}\n");
        connection.Send("Bye.\n");
    }
}
