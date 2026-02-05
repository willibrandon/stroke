namespace Stroke.Examples.DialogExamples;

/// <summary>
/// Entry point for dialog examples with command-line routing.
/// </summary>
/// <remarks>
/// <para>
/// Usage: dotnet run -- [example-name]
/// </para>
/// <para>
/// Available examples:
/// - message-box: Simple message dialog with OK button
/// - yes-no-dialog: Yes/No confirmation returning boolean
/// - button-dialog: Custom buttons with nullable values
/// - input-dialog: Text input with prompt
/// - password-dialog: Masked password input
/// - radio-dialog: Single-selection list (plain + styled)
/// - checkbox-dialog: Multi-selection with custom styling
/// - progress-dialog: Background task with progress bar
/// - styled-message-box: Custom colors via Style.FromDict()
/// </para>
/// </remarks>
internal static class Program
{
    /// <summary>
    /// Dictionary mapping example names to their run functions.
    /// </summary>
    private static readonly Dictionary<string, Action> Examples = new(StringComparer.OrdinalIgnoreCase)
    {
        ["message-box"] = MessageBoxExample.Run,
        ["yes-no-dialog"] = YesNoDialogExample.Run,
        ["button-dialog"] = ButtonDialogExample.Run,
        ["input-dialog"] = InputDialogExample.Run,
        ["password-dialog"] = PasswordDialogExample.Run,
        ["radio-dialog"] = RadioDialogExample.Run,
        ["checkbox-dialog"] = CheckboxDialogExample.Run,
        ["progress-dialog"] = ProgressDialogExample.Run,
        ["styled-message-box"] = StyledMessageBoxExample.Run,
    };

    public static void Main(string[] args)
    {
        // Default to message-box when no arguments provided (FR-003)
        var exampleName = args.Length > 0 ? args[0] : "message-box";

        if (Examples.TryGetValue(exampleName, out var runAction))
        {
            runAction();
        }
        else
        {
            // Unknown example name - show usage and exit with error code 1 (FR-004)
            Console.Error.WriteLine($"Unknown example: '{exampleName}'");
            Console.Error.WriteLine();
            ShowUsage();
            Environment.Exit(1);
        }
    }

    private static void ShowUsage()
    {
        Console.WriteLine("Stroke Dialog Examples");
        Console.WriteLine();
        Console.WriteLine("Usage: dotnet run --project examples/Stroke.Examples.Dialogs -- <example-name>");
        Console.WriteLine();
        Console.WriteLine("Available examples:");
        foreach (var name in Examples.Keys.Order())
        {
            Console.WriteLine($"  {name}");
        }
    }
}
