namespace Stroke.Examples.Prompts;

/// <summary>
/// Entry point for prompt examples with command-line routing.
/// </summary>
/// <remarks>
/// <para>
/// Usage: dotnet run -- [example-name]
/// </para>
/// <para>
/// 55 prompt examples organized by category: basic prompts, password/security,
/// styling/formatting, auto-suggestion, auto-completion, key bindings,
/// history, validation/lexing, advanced features, and frames.
/// </para>
/// </remarks>
internal static class Program
{
    /// <summary>
    /// Dictionary mapping example names to their run functions.
    /// </summary>
    private static readonly Dictionary<string, Action> Examples = new(StringComparer.OrdinalIgnoreCase)
    {
        // Basic Prompts
        ["accept-default"] = AcceptDefault.Run,
        ["confirmation-prompt"] = ConfirmationPrompt.Run,
        ["enforce-tty-input-output"] = EnforceTtyInputOutput.Run,
        ["get-input"] = GetInput.Run,
        ["get-input-vi-mode"] = GetInputViMode.Run,
        ["get-input-with-default"] = GetInputWithDefault.Run,
        ["get-multiline-input"] = GetMultilineInput.Run,
        ["get-password"] = GetPassword.Run,
        ["mouse-support"] = MouseSupport.Run,
        ["multiline-prompt"] = MultilinePrompt.Run,
        ["no-wrapping"] = NoWrapping.Run,
        ["operate-and-get-next"] = OperateAndGetNext.Run,
        ["placeholder-text"] = PlaceholderText.Run,

        // Password & Security
        ["get-password-with-toggle"] = GetPasswordWithToggle.Run,

        // Styling & Formatting
        ["bottom-toolbar"] = BottomToolbar.Run,
        ["clock-input"] = ClockInput.Run,
        ["colored-prompt"] = ColoredPrompt.Run,
        ["cursor-shapes"] = CursorShapes.Run,
        ["fancy-zsh-prompt"] = FancyZshPrompt.Run,
        ["right-prompt"] = RightPrompt.Run,
        ["swap-light-dark-colors"] = SwapLightDarkColors.Run,
        ["terminal-title"] = TerminalTitle.Run,

        // Auto-Suggestion
        ["auto-suggestion"] = AutoSuggestion.Run,
        ["multiline-autosuggest"] = MultilineAutosuggest.Run,

        // Auto-Completion
        ["autocompletion"] = Autocompletion.Run,
        ["fuzzy-word-completer"] = FuzzyWordCompleterExample.Run,
        ["auto-completion/control-space-trigger"] = ControlSpaceTrigger.Run,
        ["auto-completion/readline-style"] = ReadlineStyle.Run,
        ["auto-completion/colored-completions"] = ColoredCompletions.Run,
        ["auto-completion/formatted-completions"] = FormattedCompletions.Run,
        ["auto-completion/merged-completers"] = MergedCompleters.Run,
        ["auto-completion/fuzzy-custom-completer"] = FuzzyCustomCompleter.Run,
        ["auto-completion/multi-column"] = MultiColumn.Run,
        ["auto-completion/multi-column-with-meta"] = MultiColumnWithMeta.Run,
        ["auto-completion/nested-completion"] = NestedCompletion.Run,
        ["auto-completion/slow-completions"] = SlowCompletions.Run,

        // Backward-compatibility aliases for existing completion examples
        ["auto-completion/basic-completion"] = Autocompletion.Run,
        ["auto-completion/fuzzy-word-completer"] = FuzzyWordCompleterExample.Run,

        // Key Bindings & Editing Modes
        ["autocorrection"] = Autocorrection.Run,
        ["custom-key-binding"] = CustomKeyBinding.Run,
        ["custom-vi-operator"] = CustomViOperator.Run,
        ["switch-vi-emacs"] = SwitchViEmacs.Run,
        ["system-prompt"] = SystemPrompt.Run,

        // History
        ["history/persistent-history"] = PersistentHistory.Run,
        ["history/slow-history"] = SlowHistory.Run,
        ["up-arrow-partial-match"] = UpArrowPartialMatch.Run,

        // Validation & Lexing
        ["custom-lexer"] = CustomLexer.Run,
        ["html-input"] = HtmlInput.Run,
        ["input-validation"] = InputValidation.Run,
        ["regular-language"] = RegularLanguage.Run,

        // Advanced Features
        ["async-prompt"] = AsyncPrompt.Run,
        ["patch-stdout"] = PatchStdoutExample.Run,
        ["shell-integration"] = ShellIntegration.Run,
        ["system-clipboard"] = SystemClipboard.Run,

        // Frames
        ["with-frames/basic-frame"] = BasicFrame.Run,
        ["with-frames/gray-frame-on-accept"] = GrayFrameOnAccept.Run,
        ["with-frames/frame-with-completion"] = FrameWithCompletion.Run,
    };

    public static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            ShowUsage();
            return;
        }

        var exampleName = args[0];

        if (Examples.TryGetValue(exampleName, out var runAction))
        {
            runAction();
        }
        else
        {
            Console.Error.WriteLine($"Unknown example: '{exampleName}'");
            Console.Error.WriteLine();
            ShowUsage();
            Environment.Exit(1);
        }
    }

    private static void ShowUsage()
    {
        Console.WriteLine("Stroke Prompt Examples");
        Console.WriteLine();
        Console.WriteLine("Usage: dotnet run --project examples/Stroke.Examples.Prompts -- <example-name>");
        Console.WriteLine();
        Console.WriteLine("Available examples:");
        foreach (var name in Examples.Keys.Order())
        {
            Console.WriteLine($"  {name}");
        }
    }
}
