using Stroke.Core;
using Stroke.FormattedText;
using Stroke.Lexers;
using Stroke.Shortcuts;
using Stroke.Styles;

namespace Stroke.Examples.Prompts;

/// <summary>
/// Demonstrates a custom ILexer that applies rainbow coloring to each character.
/// Port of Python Prompt Toolkit's custom-lexer.py example.
/// </summary>
public static class CustomLexer
{
    private static readonly string[] RainbowColors =
    [
        "Red", "Orange", "Yellow", "Green", "DeepSkyBlue", "MediumPurple", "Violet",
    ];

    private sealed class RainbowLexer : ILexer
    {
        public Func<int, IReadOnlyList<StyleAndTextTuple>> LexDocument(Document document)
        {
            return (int lineNumber) =>
            {
                var line = lineNumber < document.LineCount ? document.Lines[lineNumber] : "";
                var fragments = new List<StyleAndTextTuple>();
                for (var i = 0; i < line.Length; i++)
                {
                    var color = RainbowColors[i % RainbowColors.Length];
                    fragments.Add(new StyleAndTextTuple($"fg:{color}", line[i].ToString()));
                }
                if (fragments.Count == 0)
                    fragments.Add(new StyleAndTextTuple("", ""));
                return fragments;
            };
        }

        public object InvalidationHash() => this;
    }

    public static void Run()
    {
        try
        {
            var text = Prompt.RunPrompt(
                "Enter text (rainbow colored!): ",
                lexer: new RainbowLexer(),
                multiline: true);
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
