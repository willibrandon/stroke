using Stroke.Completion;
using Stroke.Filters;
using Stroke.FormattedText;
using Stroke.Input;
using Stroke.KeyBinding;
using Stroke.Lexers;
using Stroke.Shortcuts;
using Stroke.Styles;

namespace Stroke.Examples.Prompts;

/// <summary>
/// Demonstrates swapping light/dark colors with a Ctrl-T toggle.
/// Port of Python Prompt Toolkit's swap-light-and-dark-colors.py example.
/// </summary>
public static class SwapLightDarkColors
{
    private static readonly WordCompleter HtmlCompleter = new(
    [
        "<body>", "<div>", "<head>", "<html>", "<img>", "<li>",
        "<link>", "<ol>", "<p>", "<span>", "<table>", "<td>",
        "<th>", "<tr>", "<ul>",
    ],
    ignoreCase: true);

    public static void Run()
    {
        var swapped = false;
        var bindings = new KeyBindings();

        bindings.Add<KeyHandlerCallable>([new KeyOrChar(Keys.ControlT)])(_ =>
        {
            swapped = !swapped;
            return null;
        });

        try
        {
            var session = new PromptSession<string>(
                completer: HtmlCompleter,
                completeWhileTyping: true,
                bottomToolbar: (Func<AnyFormattedText>)(() =>
                {
                    var on = swapped ? "on=true" : "on=false";
                    return new Html(
                        "Press <style bg=\"#222222\" fg=\"#ff8888\">[control-t]</style> "
                        + "to swap between dark/light colors. "
                        + $"<style bg=\"ansiblack\" fg=\"ansiwhite\">[{on}]</style>");
                }),
                keyBindings: bindings,
                lexer: PygmentsLexer.FromFilename("example.html"),
                swapLightAndDarkColors: new Condition(() => swapped));

            var text = session.Prompt(
                new Html("<style fg=\"#aaaaaa\">Give some animals</style>: "));
            Console.WriteLine($"You said: {text}");
        }
        catch (KeyboardInterruptException)
        {
            // Ctrl+C pressed - exit gracefully
        }
        catch (EOFException)
        {
            // Ctrl+D pressed - exit gracefully
        }
    }
}
