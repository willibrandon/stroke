using Stroke.FormattedText;
using Stroke.Shortcuts;
using Stroke.Styles;

namespace Stroke.Examples.Prompts;

/// <summary>
/// Fancy ZSH-style prompt with dynamic width padding between left and right parts.
/// Port of Python Prompt Toolkit's fancy-zsh-prompt.py example.
/// </summary>
public static class FancyZshPrompt
{
    private static readonly Style PromptStyle = new(
    [
        ("username", "#aaaaaa italic"),
        ("path", "#ffffff bold"),
        ("branch", "bg:#666666"),
        ("branch exclamation-mark", "#ff0000"),
        ("env", "bg:#666666"),
        ("left-part", "bg:#444444"),
        ("right-part", "bg:#444444"),
        ("padding", "bg:#444444"),
    ]);

    private static AnyFormattedText GetPrompt()
    {
        var leftPart = new Html(
            "<left-part>"
            + " <username>root</username> "
            + " abc "
            + "<path>~/.oh-my-zsh/themes</path>"
            + "</left-part>");

        var rightPart = new Html(
            "<right-part> "
            + "<branch> master<exclamation-mark>!</exclamation-mark> </branch> "
            + " <env> py36 </env> "
            + $" <time>{DateTime.Now:O}</time> "
            + "</right-part>");

        var leftFragments = FormattedTextUtils.ToFormattedText(leftPart);
        var rightFragments = FormattedTextUtils.ToFormattedText(rightPart);

        var usedWidth =
            FormattedTextUtils.FragmentListWidth(leftFragments)
            + FormattedTextUtils.FragmentListWidth(rightFragments);

        var totalWidth = Console.WindowWidth;
        var paddingSize = Math.Max(0, totalWidth - usedWidth);

        var padding = new Html("<padding>%s</padding>") % new string(' ', paddingSize);

        return FormattedTextUtils.Merge(leftPart, padding, rightPart, "\n", "# ")();
    }

    public static void Run()
    {
        var session = new PromptSession<string>(
            style: PromptStyle,
            refreshInterval: 1);

        while (true)
        {
            try
            {
                var answer = session.Prompt(
                    message: (Func<AnyFormattedText>)GetPrompt);
                Console.WriteLine($"You said: {answer}");
            }
            catch (KeyboardInterruptException)
            {
                // Ctrl+C pressed - continue loop
            }
            catch (EOFException)
            {
                // Ctrl+D pressed - exit loop
                break;
            }
        }
    }
}
