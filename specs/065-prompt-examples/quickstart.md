# Quickstart: Prompt Examples (Complete Set)

**Feature**: 065-prompt-examples
**Date**: 2026-02-06

## Build Sequence

```bash
# 1. Build the Stroke library
dotnet build src/Stroke/Stroke.csproj

# 2. Build the examples project
dotnet build examples/Stroke.Examples.Prompts/Stroke.Examples.Prompts.csproj

# 3. Run any example by name
dotnet run --project examples/Stroke.Examples.Prompts -- get-input
dotnet run --project examples/Stroke.Examples.Prompts -- bottom-toolbar
dotnet run --project examples/Stroke.Examples.Prompts -- auto-completion/basic-completion
dotnet run --project examples/Stroke.Examples.Prompts -- history/persistent-history
dotnet run --project examples/Stroke.Examples.Prompts -- with-frames/basic-frame

# 4. List all available examples
dotnet run --project examples/Stroke.Examples.Prompts
```

## Implementation Order

Implementation follows a bottom-up dependency order:

### Phase 1: Basic Prompts (13 examples)
No Stroke API dependencies beyond `Prompt.RunPrompt()`. Establishes the pattern for all remaining examples.

1. `GetInputWithDefault` — default value parameter
2. `GetInputViMode` — `viMode: true`
3. `GetPassword` — `isPassword: true`
4. `GetMultilineInput` — `multiline: true`
5. `AcceptDefault` — `acceptDefault: true`
6. `ConfirmationPrompt` — `Prompt.Confirm()`
7. `PlaceholderText` — `placeholder` parameter
8. `MouseSupport` — `mouseSupport: true`
9. `NoWrapping` — `wrapLines: false`
10. `MultilinePrompt` — `multiline: true` (basic variant)
11. `OperateAndGetNext` — REPL loop with `PromptSession`
12. `EnforceTtyInputOutput` — `inThread: true`

### Phase 2: Styling & Formatting (9 examples)
Depends on `Style`, `AnyFormattedText`, `Html`, `Ansi`.

13. `GetPasswordWithToggle` — `isPassword` toggle with Ctrl-T
14. `ColoredPrompt` — style tuples, Html, Ansi
15. `BottomToolbar` — 7 toolbar variants
16. `RightPrompt` — `rprompt` parameter
17. `ClockInput` — `refreshInterval` parameter
18. `TerminalTitle` — terminal title escape sequence
19. `CursorShapes` — `CursorShape` / `ModalCursorShapeConfig`
20. `SwapLightDarkColors` — `SwapLightAndDarkStyleTransformation`
21. `FancyZshPrompt` — dynamic width padding

### Phase 3: Key Bindings & Editing (5 examples)
Depends on `KeyBindings`, `Keys`, `EditingMode`.

22. `CustomKeyBinding` — F4, multi-key, Ctrl-T, Ctrl-K
23. `SwitchViEmacs` — mode toggle with toolbar
24. `SystemPrompt` — `enableSystemPrompt` / `enableSuspend`
25. `Autocorrection` — space-triggered auto-correction
26. `CustomViOperator` — custom 'R' operator and 'A' text object

### Phase 4: Auto-Completion (10 new + 2 existing)
Depends on `WordCompleter`, `FuzzyCompleter`, `NestedCompleter`, `CompleteStyle`.

27. `ControlSpaceTrigger` — Ctrl-Space trigger
28. `ReadlineStyle` — `CompleteStyle.ReadlineLike`
29. `ColoredCompletions` — styled completions
30. `FormattedCompletions` — HTML-formatted display text
31. `MergedCompleters` — `CompletionUtils.Merge()`
32. `FuzzyCustomCompleter` — custom completer + fuzzy wrapper
33. `MultiColumn` — `CompleteStyle.MultiColumn`
34. `MultiColumnWithMeta` — multi-column with metadata
35. `NestedCompletion` — `NestedCompleter.FromNestedDict()`
36. `SlowCompletions` — `completeInThread: true` with loading toolbar

### Phase 5: History & Suggestion (4 examples)
Depends on `FileHistory`, `ThreadedHistory`, `AutoSuggestFromHistory`.

37. `PersistentHistory` — `FileHistory` with temp file
38. `SlowHistory` — custom `IHistory` + `ThreadedHistory`
39. `UpArrowPartialMatch` — `enableHistorySearch: true`
40. `MultilineAutosuggest` — custom `IAutoSuggest` + `IProcessor`

### Phase 6: Validation & Lexing (4 examples)
Depends on `ValidatorBase`, `Grammar`, `ILexer`.

41. `InputValidation` — `ValidatorBase.FromCallable()`
42. `RegularLanguage` — `Grammar.Compile()` calculator REPL
43. `HtmlInput` — `PygmentsLexer` for HTML
44. `CustomLexer` — custom `ILexer` (rainbow)

### Phase 7: Advanced Features (5 examples)
Depends on `StdoutPatching`, `RunInTerminal`, `InputHook`.

45. `AsyncPrompt` — `PromptAsync()` with background tasks
46. `PatchStdout` — `StdoutPatching.PatchStdout()`
47. `InputHook` — `inputHook` parameter
48. `ShellIntegration` — OSC 133 escape markers
49. `SystemClipboard` — system clipboard integration

### Phase 8: Frame Examples (3 examples)
Depends on `showFrame` parameter.

50. `BasicFrame` — `showFrame: true`
51. `GrayFrameOnAccept` — frame style transition
52. `FrameWithCompletion` — frame + completion + toolbar

### Phase 9: Program.cs Update & Verification
Final routing update and TUI Driver verification.

## Verification

```bash
# Build verification
dotnet build examples/Stroke.Examples.Prompts/Stroke.Examples.Prompts.csproj

# Run usage (lists all examples)
dotnet run --project examples/Stroke.Examples.Prompts

# Spot-check representative examples
dotnet run --project examples/Stroke.Examples.Prompts -- get-input
dotnet run --project examples/Stroke.Examples.Prompts -- colored-prompt
dotnet run --project examples/Stroke.Examples.Prompts -- auto-completion/basic-completion
dotnet run --project examples/Stroke.Examples.Prompts -- input-validation
dotnet run --project examples/Stroke.Examples.Prompts -- regular-language
dotnet run --project examples/Stroke.Examples.Prompts -- with-frames/basic-frame
```

## Key Files

| File | Purpose |
|------|---------|
| `examples/Stroke.Examples.Prompts/Program.cs` | Routing (58 entries) |
| `examples/Stroke.Examples.Prompts/Stroke.Examples.Prompts.csproj` | Project config |
| `examples/Stroke.Examples.sln` | Solution file |
| `specs/065-prompt-examples/contracts/routing-manifest.md` | Authoritative routing table |
| `docs/examples-mapping.md` | Python → C# mapping reference |
