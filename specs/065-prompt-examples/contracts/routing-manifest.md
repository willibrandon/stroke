# Routing Manifest: All 56 Prompt Examples

**Feature**: 065-prompt-examples

## Complete Routing Table

This is the authoritative list of all routing entries for Program.cs.

### Basic Prompts (13)

| # | Routing Name | Class | File | Status |
|---|-------------|-------|------|--------|
| 1 | `get-input` | `GetInput` | `GetInput.cs` | EXISTING |
| 2 | `get-input-with-default` | `GetInputWithDefault` | `GetInputWithDefault.cs` | NEW |
| 3 | `get-input-vi-mode` | `GetInputViMode` | `GetInputViMode.cs` | NEW |
| 4 | `get-password` | `GetPassword` | `GetPassword.cs` | NEW |
| 5 | `get-multiline-input` | `GetMultilineInput` | `GetMultilineInput.cs` | NEW |
| 6 | `accept-default` | `AcceptDefault` | `AcceptDefault.cs` | NEW |
| 7 | `confirmation-prompt` | `ConfirmationPrompt` | `ConfirmationPrompt.cs` | NEW |
| 8 | `placeholder-text` | `PlaceholderText` | `PlaceholderText.cs` | NEW |
| 9 | `mouse-support` | `MouseSupport` | `MouseSupport.cs` | NEW |
| 10 | `no-wrapping` | `NoWrapping` | `NoWrapping.cs` | NEW |
| 11 | `multiline-prompt` | `MultilinePrompt` | `MultilinePrompt.cs` | NEW |
| 12 | `operate-and-get-next` | `OperateAndGetNext` | `OperateAndGetNext.cs` | NEW |
| 13 | `enforce-tty-input-output` | `EnforceTtyInputOutput` | `EnforceTtyInputOutput.cs` | NEW |

### Password & Security (1)

| # | Routing Name | Class | File | Status |
|---|-------------|-------|------|--------|
| 14 | `get-password-with-toggle` | `GetPasswordWithToggle` | `GetPasswordWithToggle.cs` | NEW |

### Styling & Formatting (8)

| # | Routing Name | Class | File | Status |
|---|-------------|-------|------|--------|
| 15 | `colored-prompt` | `ColoredPrompt` | `ColoredPrompt.cs` | NEW |
| 16 | `bottom-toolbar` | `BottomToolbar` | `BottomToolbar.cs` | NEW |
| 17 | `right-prompt` | `RightPrompt` | `RightPrompt.cs` | NEW |
| 18 | `clock-input` | `ClockInput` | `ClockInput.cs` | NEW |
| 19 | `fancy-zsh-prompt` | `FancyZshPrompt` | `FancyZshPrompt.cs` | NEW |
| 20 | `terminal-title` | `TerminalTitle` | `TerminalTitle.cs` | NEW |
| 21 | `swap-light-dark-colors` | `SwapLightDarkColors` | `SwapLightDarkColors.cs` | NEW |
| 22 | `cursor-shapes` | `CursorShapes` | `CursorShapes.cs` | NEW |

### Key Bindings & Input Handling (5)

| # | Routing Name | Class | File | Status |
|---|-------------|-------|------|--------|
| 23 | `custom-key-binding` | `CustomKeyBinding` | `CustomKeyBinding.cs` | NEW |
| 24 | `custom-vi-operator` | `CustomViOperator` | `CustomViOperator.cs` | NEW |
| 25 | `system-prompt` | `SystemPrompt` | `SystemPrompt.cs` | NEW |
| 26 | `switch-vi-emacs` | `SwitchViEmacs` | `SwitchViEmacs.cs` | NEW |
| 27 | `autocorrection` | `Autocorrection` | `Autocorrection.cs` | NEW |

### Auto-Suggestion & History (3 new)

| # | Routing Name | Class | File | Status |
|---|-------------|-------|------|--------|
| 28 | `auto-suggestion` | `AutoSuggestion` | `AutoSuggestion.cs` | EXISTING |
| 29 | `multiline-autosuggest` | `MultilineAutosuggest` | `MultilineAutosuggest.cs` | NEW |
| 30 | `up-arrow-partial-match` | `UpArrowPartialMatch` | `UpArrowPartialMatch.cs` | NEW |

### Auto-Completion (12, 2 existing)

| # | Routing Name | Class | File | Status |
|---|-------------|-------|------|--------|
| 31 | `auto-completion/basic-completion` | `Autocompletion` | `AutoCompletion/Autocompletion.cs` | EXISTING |
| 32 | `auto-completion/control-space-trigger` | `ControlSpaceTrigger` | `AutoCompletion/ControlSpaceTrigger.cs` | NEW |
| 33 | `auto-completion/readline-style` | `ReadlineStyle` | `AutoCompletion/ReadlineStyle.cs` | NEW |
| 34 | `auto-completion/colored-completions` | `ColoredCompletions` | `AutoCompletion/ColoredCompletions.cs` | NEW |
| 35 | `auto-completion/formatted-completions` | `FormattedCompletions` | `AutoCompletion/FormattedCompletions.cs` | NEW |
| 36 | `auto-completion/merged-completers` | `MergedCompleters` | `AutoCompletion/MergedCompleters.cs` | NEW |
| 37 | `auto-completion/fuzzy-word-completer` | `FuzzyWordCompleterExample` | `AutoCompletion/FuzzyWordCompleter.cs` | EXISTING |
| 38 | `auto-completion/fuzzy-custom-completer` | `FuzzyCustomCompleter` | `AutoCompletion/FuzzyCustomCompleter.cs` | NEW |
| 39 | `auto-completion/multi-column` | `MultiColumn` | `AutoCompletion/MultiColumn.cs` | NEW |
| 40 | `auto-completion/multi-column-with-meta` | `MultiColumnWithMeta` | `AutoCompletion/MultiColumnWithMeta.cs` | NEW |
| 41 | `auto-completion/nested-completion` | `NestedCompletion` | `AutoCompletion/NestedCompletion.cs` | NEW |
| 42 | `auto-completion/slow-completions` | `SlowCompletions` | `AutoCompletion/SlowCompletions.cs` | NEW |

### History (2)

| # | Routing Name | Class | File | Status |
|---|-------------|-------|------|--------|
| 43 | `history/persistent-history` | `PersistentHistory` | `History/PersistentHistory.cs` | NEW |
| 44 | `history/slow-history` | `SlowHistory` | `History/SlowHistory.cs` | NEW |

### Validation & Lexing (4)

| # | Routing Name | Class | File | Status |
|---|-------------|-------|------|--------|
| 45 | `input-validation` | `InputValidation` | `InputValidation.cs` | NEW |
| 46 | `regular-language` | `RegularLanguage` | `RegularLanguage.cs` | NEW |
| 47 | `html-input` | `HtmlInput` | `HtmlInput.cs` | NEW |
| 48 | `custom-lexer` | `CustomLexer` | `CustomLexer.cs` | NEW |

### Advanced Features (5)

| # | Routing Name | Class | File | Status |
|---|-------------|-------|------|--------|
| 49 | `async-prompt` | `AsyncPrompt` | `AsyncPrompt.cs` | NEW |
| 50 | `patch-stdout` | `PatchStdout` | `PatchStdout.cs` | NEW |
| 51 | `input-hook` | `InputHook` | `InputHook.cs` | NEW |
| 52 | `shell-integration` | `ShellIntegration` | `ShellIntegration.cs` | NEW |
| 53 | `system-clipboard` | `SystemClipboard` | `SystemClipboard.cs` | NEW |

### With Frames (3)

| # | Routing Name | Class | File | Status |
|---|-------------|-------|------|--------|
| 54 | `with-frames/basic-frame` | `BasicFrame` | `WithFrames/BasicFrame.cs` | NEW |
| 55 | `with-frames/gray-frame-on-accept` | `GrayFrameOnAccept` | `WithFrames/GrayFrameOnAccept.cs` | NEW |
| 56 | `with-frames/frame-with-completion` | `FrameWithCompletion` | `WithFrames/FrameWithCompletion.cs` | NEW |

### Backward Compatibility Aliases

| Alias | Target | Reason |
|-------|--------|--------|
| `autocompletion` | `Autocompletion.Run` | Existing entry (pre-065) |
| `fuzzy-word-completer` | `FuzzyWordCompleterExample.Run` | Existing entry (pre-065) |

**Total routing entries**: 56 primary + 2 aliases = 58 entries
