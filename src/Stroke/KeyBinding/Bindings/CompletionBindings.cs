using Stroke.Completion;
using Stroke.Core;

namespace Stroke.KeyBinding.Bindings;

/// <summary>
/// Key binding handlers for displaying completions.
/// </summary>
/// <remarks>
/// <para>
/// Port of Python Prompt Toolkit's <c>prompt_toolkit.key_binding.bindings.completion</c> module.
/// Provides two public functions: <see cref="GenerateCompletions"/> and
/// <see cref="DisplayCompletionsLikeReadline"/>.
/// </para>
/// </remarks>
public static class CompletionBindings
{
    /// <summary>
    /// Tab-completion: where the first tab completes the common suffix and the
    /// second tab lists all the completions.
    /// </summary>
    /// <param name="event">The key press event.</param>
    public static void GenerateCompletions(KeyPressEvent @event)
    {
        var b = @event.CurrentBuffer!;

        // When already navigating through completions, select the next one.
        if (b.CompleteState is not null)
        {
            b.CompleteNext();
        }
        else
        {
            b.StartCompletion(insertCommonPart: true);
        }
    }

    /// <summary>
    /// Readline-style tab completion. Generates completions immediately (blocking)
    /// and displays them above the prompt in columns.
    /// </summary>
    /// <param name="event">The key press event.</param>
    public static void DisplayCompletionsLikeReadline(KeyPressEvent @event)
    {
        // Request completions.
        var b = @event.CurrentBuffer!;
        if (b.Completer is null)
        {
            return;
        }

        var completeEvent = new CompleteEvent(CompletionRequested: true);
        var completions = b.Completer.GetCompletions(b.Document, completeEvent).ToList();

        // Calculate the common suffix.
        var commonSuffix = CompletionUtils.GetCommonCompleteSuffix(b.Document, completions);

        // One completion: insert it.
        if (completions.Count == 1)
        {
            b.DeleteBeforeCursor(-completions[0].StartPosition);
            b.InsertText(completions[0].Text);
        }
        // Multiple completions with common part.
        else if (!string.IsNullOrEmpty(commonSuffix))
        {
            b.InsertText(commonSuffix);
        }
        // Otherwise: display all completions.
        else if (completions.Count > 0)
        {
            DisplayCompletionsLikeReadlineInternal(@event.GetApp(), completions);
        }
    }

    /// <summary>
    /// Display the list of completions in columns above the prompt.
    /// Asks for confirmation if there are too many completions to fit
    /// on a single page and provides a paginator to walk through them.
    /// </summary>
    /// <remarks>
    /// Port of Python's <c>_display_completions_like_readline</c> which uses
    /// <c>create_confirm_session</c> and <c>_create_more_session</c> for user
    /// interaction. Since those depend on the Shortcuts layer (PromptSession),
    /// this port uses raw <see cref="Console"/> I/O within
    /// <see cref="Application.RunInTerminal"/> for the confirmation and
    /// pagination prompts.
    /// </remarks>
    private static void DisplayCompletionsLikeReadlineInternal(
        Application.Application<object> app,
        List<Completion.Completion> completions)
    {
        // Get terminal dimensions.
        var termSize = app.Output.GetSize();
        var termWidth = termSize.Columns;
        var termHeight = termSize.Rows;

        // Calculate amount of required columns/rows for displaying the
        // completions. (Keep in mind that completions are displayed
        // alphabetically column-wise.)
        var maxComplWidth = Math.Min(
            termWidth,
            completions.Max(c => UnicodeWidth.GetWidth(c.DisplayText)) + 1);
        var columnCount = Math.Max(1, termWidth / maxComplWidth);
        var completionsPerPage = columnCount * (termHeight - 1);
        var pageCount = (int)Math.Ceiling((double)completions.Count / completionsPerPage);

        void Display(int page)
        {
            var pageCompletions = completions
                .Skip(page * completionsPerPage)
                .Take(completionsPerPage)
                .ToList();

            var pageRowCount = (int)Math.Ceiling((double)pageCompletions.Count / columnCount);
            var pageColumns = new List<List<Completion.Completion>>();
            for (var i = 0; i < columnCount; i++)
            {
                pageColumns.Add(pageCompletions
                    .Skip(i * pageRowCount)
                    .Take(pageRowCount)
                    .ToList());
            }

            for (var r = 0; r < pageRowCount; r++)
            {
                var line = "";
                for (var c = 0; c < columnCount; c++)
                {
                    if (c < pageColumns.Count && r < pageColumns[c].Count)
                    {
                        var completion = pageColumns[c][r];
                        var displayText = completion.DisplayText;
                        line += displayText;

                        // Add padding.
                        var padding = maxComplWidth - UnicodeWidth.GetWidth(displayText);
                        if (padding > 0)
                        {
                            line += new string(' ', padding);
                        }
                    }
                }

                app.Output.WriteRaw(line + "\n");
            }
        }

        // Run in terminal to display completions above the prompt.
        // RunInTerminal suspends the TUI, giving raw terminal access for
        // confirmation prompts and pagination, matching Python's in_terminal().
        _ = app.CreateBackgroundTask(async ct =>
        {
            await Application.RunInTerminal.RunAsync(() =>
            {
                if (completions.Count > completionsPerPage)
                {
                    // Ask confirmation if it doesn't fit on the screen.
                    // Port of create_confirm_session("Display all N possibilities?")
                    Console.Write($"Display all {completions.Count} possibilities? (y or n) ");
                    var confirm = ReadConfirmation();

                    if (confirm)
                    {
                        // Display pages.
                        for (var page = 0; page < pageCount; page++)
                        {
                            Display(page);

                            if (page != pageCount - 1)
                            {
                                // Display --MORE-- and wait for user input.
                                // Port of _create_more_session("--MORE--")
                                Console.Write("--MORE--");
                                var showMore = ReadMorePrompt();

                                // Erase the --MORE-- prompt.
                                Console.Write("\r" + new string(' ', 8) + "\r");

                                if (!showMore)
                                {
                                    return;
                                }
                            }
                        }
                    }
                    else
                    {
                        app.Output.Flush();
                    }
                }
                else
                {
                    // Display all completions.
                    Display(0);
                }
            }, renderCliDone: true);
        });
    }

    /// <summary>
    /// Read a y/n confirmation from the terminal.
    /// Port of Python's <c>create_confirm_session</c> key bindings.
    /// </summary>
    /// <returns>True if confirmed, false otherwise.</returns>
    private static bool ReadConfirmation()
    {
        while (true)
        {
            var key = Console.ReadKey(intercept: true);
            switch (key.KeyChar)
            {
                case 'y':
                case 'Y':
                    Console.WriteLine();
                    return true;
                case 'n':
                case 'N':
                case 'q':
                case 'Q':
                    Console.WriteLine();
                    return false;
            }

            // Ctrl+C also means no.
            if (key.Key == ConsoleKey.C && key.Modifiers.HasFlag(ConsoleModifiers.Control))
            {
                Console.WriteLine();
                return false;
            }
        }
    }

    /// <summary>
    /// Read a --MORE-- prompt response from the terminal.
    /// Port of Python's <c>_create_more_session</c> key bindings:
    /// space/y/Y/Enter/Tab = continue, n/N/q/Q/Ctrl+C = stop.
    /// </summary>
    /// <returns>True to show more, false to stop.</returns>
    private static bool ReadMorePrompt()
    {
        while (true)
        {
            var key = Console.ReadKey(intercept: true);

            // Continue keys: space, y, Y, Enter, Tab
            switch (key.KeyChar)
            {
                case ' ':
                case 'y':
                case 'Y':
                    return true;
            }

            if (key.Key is ConsoleKey.Enter or ConsoleKey.Tab)
            {
                return true;
            }

            // Stop keys: n, N, q, Q
            switch (key.KeyChar)
            {
                case 'n':
                case 'N':
                case 'q':
                case 'Q':
                    return false;
            }

            // Ctrl+C also means stop.
            if (key.Key == ConsoleKey.C && key.Modifiers.HasFlag(ConsoleModifiers.Control))
            {
                return false;
            }

            // Any other key is ignored (matching Python's Keys.Any handler that does nothing).
        }
    }
}
