using Stroke.Application;
using Stroke.AutoSuggest;
using Stroke.Core;
using Stroke.Filters;
using Stroke.FormattedText;
using Stroke.KeyBinding;
using Stroke.Layout.Processors;
using Stroke.Shortcuts;

namespace Stroke.Examples.Prompts;

/// <summary>
/// Demonstrates fish-style auto suggestion across multiple lines.
/// Port of Python Prompt Toolkit's multiline-autosuggest example.
/// </summary>
/// <remarks>
/// <para>
/// This example shows how multiline auto-suggest can be used for LLM-style
/// completions that may return multi-line responses.
/// </para>
/// <para>
/// Unlike simple auto-suggest, multiline requires a custom processor to handle
/// the various cases: appending to the cursor line, rendering suggestion lines,
/// and shifting existing buffer text below the suggestion.
/// </para>
/// </remarks>
public static class MultilineAutosuggest
{
    private static readonly string[] UniversalDeclarationOfHumanRights =
        """
        All human beings are born free and equal in dignity and rights.
        They are endowed with reason and conscience and should act towards one another
        in a spirit of brotherhood
        Everyone is entitled to all the rights and freedoms set forth in this
        Declaration, without distinction of any kind, such as race, colour, sex,
        language, religion, political or other opinion, national or social origin,
        property, birth or other status. Furthermore, no distinction shall be made on
        the basis of the political, jurisdictional or international status of the
        country or territory to which a person belongs, whether it be independent,
        trust, non-self-governing or under any other limitation of sovereignty.
        """.Trim().Split('\n');

    /// <summary>
    /// Fake LLM-style auto-suggest that completes lines from the
    /// Universal Declaration of Human Rights.
    /// </summary>
    private sealed class FakeLlmAutoSuggest : IAutoSuggest
    {
        public Suggestion? GetSuggestion(IBuffer buffer, Document document)
        {
            if (document.LineCount == 1)
                return new Suggestion(" (Add a few new lines to see multiline completion)");

            var cursorLine = document.CursorPositionRow;
            var lines = document.Text.Split('\n');
            var text = lines[cursorLine];

            if (string.IsNullOrWhiteSpace(text))
                return null;

            // Find matching line in the corpus
            int? index = null;
            for (int i = 0; i < UniversalDeclarationOfHumanRights.Length; i++)
            {
                if (UniversalDeclarationOfHumanRights[i].StartsWith(text, StringComparison.Ordinal))
                {
                    index = i;
                    break;
                }
            }

            if (index is null)
                return null;

            // Build suggestion: rest of current line + all subsequent lines
            var restOfLine = UniversalDeclarationOfHumanRights[index.Value][text.Length..];
            var remainingLines = string.Join("\n",
                UniversalDeclarationOfHumanRights[(index.Value + 1)..]);

            return new Suggestion(restOfLine + "\n" + remainingLines);
        }

        public ValueTask<Suggestion?> GetSuggestionAsync(IBuffer buffer, Document document) =>
            ValueTask.FromResult(GetSuggestion(buffer, document));
    }

    /// <summary>
    /// Processor that renders multi-line auto-suggestion text across multiple lines.
    /// </summary>
    /// <remarks>
    /// Handles three distinct rendering regions:
    /// <list type="number">
    /// <item>Lines before cursor: unchanged.</item>
    /// <item>Cursor line (delta 0): appends first suggestion line.</item>
    /// <item>Lines within suggestion range: replaced with suggestion text.</item>
    /// <item>Lines beyond suggestion: shifted down to make room.</item>
    /// </list>
    /// </remarks>
    private sealed class AppendMultilineAutoSuggestionInAnyLine : IProcessor
    {
        private readonly string _style;

        public AppendMultilineAutoSuggestionInAnyLine(string style = "class:auto-suggestion")
        {
            _style = style;
        }

        public Transformation ApplyTransformation(TransformationInput ti)
        {
            var noop = new Transformation(ti.Fragments);

            // Let prompt_toolkit handle single-line mode.
            if (ti.Document.LineCount == 1)
                return noop;

            // Lines before the cursor are unchanged.
            if (ti.LineNumber < ti.Document.CursorPositionRow)
                return noop;

            var buffer = ti.BufferControl.Buffer;
            if (buffer.Suggestion is null || !ti.Document.IsCursorAtTheEndOfLine)
                return noop;

            // Delta between the line being rendered and the cursor line.
            var delta = ti.LineNumber - ti.Document.CursorPositionRow;

            // Split suggestion into lines.
            var suggestions = buffer.Suggestion.Text.Split('\n');
            if (suggestions.Length == 0)
                return noop;

            if (delta == 0)
            {
                // Append first suggestion line to cursor line.
                var fragments = new List<StyleAndTextTuple>(ti.Fragments)
                {
                    new(_style, suggestions[0])
                };
                return new Transformation(fragments);
            }
            else if (delta < suggestions.Length)
            {
                // Render the nth suggestion line.
                return new Transformation([new StyleAndTextTuple(_style, suggestions[delta])]);
            }
            else
            {
                // Shift existing lines down past the suggestion.
                // First suggestion line doesn't shift, so offset by 1.
                var shift = ti.LineNumber - suggestions.Length + 1;
                if (ti.GetLine is not null)
                    return new Transformation(ti.GetLine(shift));
                return noop;
            }
        }
    }

    public static void Run()
    {
        var autoSuggest = new FakeLlmAutoSuggest();

        // Print help.
        Console.WriteLine("This CLI has fish-style auto-suggestion enabled across multiple lines.");
        Console.WriteLine("This will try to complete the universal declaration of human rights.");
        Console.WriteLine();
        foreach (var line in UniversalDeclarationOfHumanRights)
            Console.WriteLine($"   {line}");
        Console.WriteLine();
        Console.WriteLine("Add a few new lines to see multiline completion, and start typing.");
        Console.WriteLine("Press Control-C to retry. Control-D to exit.");
        Console.WriteLine();

        var session = new PromptSession<string>(
            autoSuggest: autoSuggest,
            enableHistorySearch: false,
            reserveSpaceForMenu: 5,
            multiline: true,
            promptContinuation: "... ",
            inputProcessors:
            [
                new ConditionalProcessor(
                    new AppendMultilineAutoSuggestionInAnyLine(),
                    new Condition(() =>
                        AppFilters.HasFocus(BufferNames.Default).Invoke()
                        && !AppFilters.IsDone.Invoke())),
            ]);

        while (true)
        {
            try
            {
                var text = session.Prompt(
                    "Say something (Esc-enter : accept, enter : new line): ");
                Console.WriteLine($"You said: {text}");
                break;
            }
            catch (KeyboardInterruptException)
            {
                // Ctrl+C pressed. Try again.
            }
            catch (EOFException)
            {
                break;
            }
        }
    }
}
