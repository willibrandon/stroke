/// <summary>
/// Example of running a Stroke application in an SSH server.
///
/// This is a faithful port of Python Prompt Toolkit's asyncssh-server.py example.
/// </summary>

using Stroke.Completion;
using Stroke.Contrib.Ssh;
using Stroke.Shortcuts;

namespace Stroke.Examples.Ssh;

public static class AsyncsshServer
{
    private const string HostKeyPath = "ssh_host_key";

    // Animal completer - faithful port from Python example
    private static readonly WordCompleter AnimalCompleter = new(
        words:
        [
            "alligator",
            "ant",
            "ape",
            "bat",
            "bear",
            "beaver",
            "bee",
            "bison",
            "butterfly",
            "cat",
            "chicken",
            "crocodile",
            "dinosaur",
            "dog",
            "dolphin",
            "dove",
            "duck",
            "eagle",
            "elephant",
            "fish",
            "goat",
            "gorilla",
            "kangaroo",
            "leopard",
            "lion",
            "mouse",
            "rabbit",
            "rat",
            "snake",
            "spider",
            "turkey",
            "turtle",
        ],
        ignoreCase: true);

    public static async Task RunAsync(CancellationToken cancellationToken = default)
    {
        EnsureHostKey();

        var server = new StrokeSshServer(
            interact: InteractAsync,
            port: 2222,
            hostKeyPath: HostKeyPath);

        await server.RunAsync(cancellationToken: cancellationToken);
    }

    /// <summary>
    /// The application interaction.
    ///
    /// This will run automatically in a Stroke AppSession, which means
    /// that any Stroke application (dialogs, prompts, etc.) will use the
    /// SSH channel for input and output.
    /// </summary>
    private static async Task InteractAsync(StrokeSshSession session)
    {
        var promptSession = new PromptSession<string>();

        // Use FormattedTextOutput.Print to send output to the SSH client
        FormattedTextOutput.Print("We will be running a few Stroke applications through this ");
        FormattedTextOutput.Print("SSH connection.\n");

        // Progress demonstration using ProgressDialogAsync
        FormattedTextOutput.Print("\nShowing progress bar demo...\n");
        await Dialogs.ProgressDialogAsync(
            title: "Progress Demo",
            text: "Processing...",
            runCallback: (setPercentage, logText) =>
            {
                for (int i = 0; i <= 100; i += 2)
                {
                    setPercentage(i);
                    logText($"Processing step {i / 2 + 1} of 51\n");
                    Thread.Sleep(50);
                }
            });

        // Normal prompt
        var text = await promptSession.PromptAsync("(normal prompt) Type something: ");
        FormattedTextOutput.Print($"You typed: {text}\n");

        // Prompt with auto completion (shows completions as you type, like pgcli)
        text = await promptSession.PromptAsync(
            message: "(autocompletion) Type an animal: ",
            completer: AnimalCompleter,
            completeWhileTyping: true);
        FormattedTextOutput.Print($"You typed: {text}\n");

        // Note: HTML syntax highlighting would require a PygmentsLexer equivalent.
        // For now, we'll just do a simple prompt.
        text = await promptSession.PromptAsync("(simple input) Type something: ");
        FormattedTextOutput.Print($"You typed: {text}\n");

        // Show yes/no dialog
        await promptSession.PromptAsync("Showing yes/no dialog... [ENTER]");
        var yesNo = await Dialogs.YesNoDialogAsync(
            title: "Yes/no dialog",
            text: "Running over SSH");
        FormattedTextOutput.Print($"You selected: {(yesNo ? "Yes" : "No")}\n");

        // Show input dialog
        await promptSession.PromptAsync("Showing input dialog... [ENTER]");
        var input = await Dialogs.InputDialogAsync(
            title: "Input dialog",
            text: "Running over SSH");
        FormattedTextOutput.Print($"You entered: {input ?? "(cancelled)"}\n");

        FormattedTextOutput.Print("\nSession complete. Goodbye!\n");
    }

    private static void EnsureHostKey()
    {
        if (!File.Exists(HostKeyPath))
        {
            Console.Error.WriteLine("Host key not found. Generate one with:");
            Console.Error.WriteLine("  ssh-keygen -t rsa -m PEM -f ssh_host_key -N \"\"");
            throw new FileNotFoundException($"Host key file not found: {HostKeyPath}");
        }
    }
}
