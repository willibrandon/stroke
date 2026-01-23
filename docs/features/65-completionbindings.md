# Feature 65: Completion Bindings

## Overview

Implement the key binding handlers for displaying completions, including the standard completion trigger and readline-style column display.

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/key_binding/bindings/completion.py`

## Public API

### Completion Functions

```csharp
namespace Stroke.KeyBinding.Bindings;

public static class CompletionFunctions
{
    /// <summary>
    /// Tab-completion handler where first tab completes common suffix
    /// and second tab lists all completions.
    /// </summary>
    public static void GenerateCompletions(KeyPressEvent @event);

    /// <summary>
    /// Readline-style tab completion that displays completions in columns
    /// above the prompt. Blocks to generate completions immediately.
    /// </summary>
    public static void DisplayCompletionsLikeReadline(KeyPressEvent @event);
}
```

## Project Structure

```
src/Stroke/
└── KeyBinding/
    └── Bindings/
        └── CompletionFunctions.cs
tests/Stroke.Tests/
└── KeyBinding/
    └── Bindings/
        └── CompletionFunctionsTests.cs
```

## Implementation Notes

### GenerateCompletions

```csharp
public static void GenerateCompletions(KeyPressEvent @event)
{
    var buffer = @event.CurrentBuffer;

    // If already navigating completions, select next
    if (buffer.CompleteState != null)
    {
        buffer.CompleteNext();
    }
    else
    {
        // Start completion, inserting common part
        buffer.StartCompletion(insertCommonPart: true);
    }
}
```

### DisplayCompletionsLikeReadline

```csharp
public static void DisplayCompletionsLikeReadline(KeyPressEvent @event)
{
    var buffer = @event.CurrentBuffer;
    if (buffer.Completer == null) return;

    // Generate completions immediately (blocking)
    var completeEvent = new CompleteEvent(completionRequested: true);
    var completions = buffer.Completer
        .GetCompletions(buffer.Document, completeEvent)
        .ToList();

    // Calculate common suffix
    var commonSuffix = GetCommonCompleteSuffix(buffer.Document, completions);

    if (completions.Count == 1)
    {
        // One completion: insert it
        buffer.DeleteBeforeCursor(-completions[0].StartPosition);
        buffer.InsertText(completions[0].Text);
    }
    else if (!string.IsNullOrEmpty(commonSuffix))
    {
        // Multiple completions with common part: insert common
        buffer.InsertText(commonSuffix);
    }
    else if (completions.Count > 0)
    {
        // Display all completions in columns
        DisplayCompletionsInColumns(@event.App, completions);
    }
}
```

### Column Display Algorithm

```csharp
private static async Task DisplayCompletionsInColumns(
    Application app,
    IReadOnlyList<Completion> completions)
{
    // Get terminal dimensions
    var termSize = app.Output.GetSize();
    var termWidth = termSize.Columns;
    var termHeight = termSize.Rows;

    // Calculate column layout
    var maxComplWidth = Math.Min(
        termWidth,
        completions.Max(c => UnicodeWidth.GetWidth(c.DisplayText)) + 1);
    var columnCount = Math.Max(1, termWidth / maxComplWidth);
    var completionsPerPage = columnCount * (termHeight - 1);
    var pageCount = (int)Math.Ceiling((double)completions.Count / completionsPerPage);

    void DisplayPage(int page)
    {
        var pageCompletions = completions
            .Skip(page * completionsPerPage)
            .Take(completionsPerPage)
            .ToList();

        var pageRowCount = (int)Math.Ceiling((double)pageCompletions.Count / columnCount);
        var pageColumns = new List<List<Completion>>();

        for (var i = 0; i < columnCount; i++)
        {
            pageColumns.Add(pageCompletions
                .Skip(i * pageRowCount)
                .Take(pageRowCount)
                .ToList());
        }

        var fragments = new List<(string Style, string Text)>();

        for (var r = 0; r < pageRowCount; r++)
        {
            for (var c = 0; c < columnCount; c++)
            {
                if (c < pageColumns.Count && r < pageColumns[c].Count)
                {
                    var completion = pageColumns[c][r];
                    var style = "class:readline-like-completions.completion " +
                        (completion.Style ?? "");

                    fragments.AddRange(completion.Display.WithStyle(style));

                    // Add padding
                    var padding = maxComplWidth - UnicodeWidth.GetWidth(completion.DisplayText);
                    fragments.Add((completion.Style ?? "", new string(' ', padding)));
                }
            }
            fragments.Add(("", "\n"));
        }

        app.PrintText(new FormattedText(fragments)
            .WithStyle("class:readline-like-completions"));
    }

    await app.RunInTerminalAsync(async () =>
    {
        if (completions.Count > completionsPerPage)
        {
            // Ask confirmation
            var confirm = await PromptSession.CreateConfirmSession(
                $"Display all {completions.Count} possibilities?")
                .PromptAsync();

            if (confirm)
            {
                for (var page = 0; page < pageCount; page++)
                {
                    DisplayPage(page);

                    if (page < pageCount - 1)
                    {
                        // Display --MORE-- prompt
                        var showMore = await CreateMoreSession("--MORE--")
                            .PromptAsync();

                        if (!showMore) return;
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
            // Display all completions on single page
            DisplayPage(0);
        }
    }, renderCliDone: true);
}
```

### GetCommonCompleteSuffix

```csharp
private static string GetCommonCompleteSuffix(
    Document document,
    IReadOnlyList<Completion> completions)
{
    if (completions.Count == 0) return "";

    // All completions must start with the same prefix
    // Find the common suffix beyond the current word

    var textBeforeCursor = document.TextBeforeCursor;
    var commonSuffix = completions[0].Text;

    foreach (var completion in completions.Skip(1))
    {
        // Find common prefix between current common and this completion
        var minLength = Math.Min(commonSuffix.Length, completion.Text.Length);
        var commonLength = 0;

        while (commonLength < minLength &&
            commonSuffix[commonLength] == completion.Text[commonLength])
        {
            commonLength++;
        }

        commonSuffix = commonSuffix.Substring(0, commonLength);

        if (string.IsNullOrEmpty(commonSuffix)) break;
    }

    // Remove the part that's already typed
    var startPosition = completions[0].StartPosition;
    var alreadyTyped = textBeforeCursor.Substring(
        textBeforeCursor.Length + startPosition);

    if (commonSuffix.StartsWith(alreadyTyped))
    {
        return commonSuffix.Substring(alreadyTyped.Length);
    }

    return "";
}
```

### --MORE-- Prompt Session

```csharp
private static PromptSession<bool> CreateMoreSession(string message = "--MORE--")
{
    var bindings = new KeyBindings();

    // Accept keys
    bindings.Add(" ", e => e.App.Exit(true));
    bindings.Add("y", e => e.App.Exit(true));
    bindings.Add("Y", e => e.App.Exit(true));
    bindings.Add("enter", e => e.App.Exit(true));
    bindings.Add("tab", e => e.App.Exit(true));

    // Reject keys
    bindings.Add("n", e => e.App.Exit(false));
    bindings.Add("N", e => e.App.Exit(false));
    bindings.Add("q", e => e.App.Exit(false));
    bindings.Add("Q", e => e.App.Exit(false));
    bindings.Add("c-c", e => e.App.Exit(false));

    // Ignore other keys
    bindings.Add(Keys.Any, e => { });

    return new PromptSession<bool>(message,
        keyBindings: bindings,
        eraseWhenDone: true);
}
```

## Dependencies

- `Stroke.KeyBinding.KeyBindings` (Feature 19) - KeyBindings class
- `Stroke.Completion` (Feature 18) - Completion system
- `Stroke.Core.Buffer` (Feature 06) - Buffer with completion state
- `Stroke.Application` (Feature 37) - Application for RunInTerminal
- `Stroke.Shortcuts.PromptSession` (Feature 32) - Confirm prompts

## Implementation Tasks

1. Implement `GenerateCompletions` function
2. Implement `DisplayCompletionsLikeReadline` function
3. Implement column layout calculation
4. Implement page display logic
5. Implement `GetCommonCompleteSuffix` helper
6. Implement --MORE-- prompt session
7. Implement confirmation prompt for large lists
8. Write comprehensive unit tests

## Acceptance Criteria

- [ ] First tab completes common suffix
- [ ] Second tab shows completion menu
- [ ] Readline mode displays completions in columns
- [ ] Columns are calculated based on terminal width
- [ ] Pagination works for large completion lists
- [ ] --MORE-- prompt allows continuing or quitting
- [ ] Confirmation prompt for many completions
- [ ] Common suffix is correctly calculated
- [ ] Wide characters are properly measured
- [ ] Unit tests achieve 80% coverage
