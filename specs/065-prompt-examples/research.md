# Research: Prompt Examples (Complete Set)

**Feature**: 065-prompt-examples
**Date**: 2026-02-06

## Research Tasks

### RT-001: Stroke Public API Completeness for 56 Examples

**Decision**: All 56 prompt examples can be implemented using the existing Stroke public API.

**Rationale**: A comprehensive audit of the Stroke source (`/Users/brandon/src/stroke/src/Stroke/`) against all Python Prompt Toolkit prompt example imports confirms:

| API Category | Status | Notes |
|-------------|--------|-------|
| `Prompt.RunPrompt()` (43 params) | Complete | Direct mapping from Python's `prompt()` |
| `PromptSession<T>` (44-param ctor) | Complete | Session-based prompts for history reuse |
| `Prompt.Confirm()` / `CreateConfirmSession()` | Complete | Yes/no confirmation prompts |
| `WordCompleter` | Complete | Basic word list completion |
| `FuzzyCompleter` / `FuzzyWordCompleter` | Complete | Fuzzy matching wrappers |
| `NestedCompleter` | Complete | Hierarchical command completion |
| `CompletionUtils.Merge()` | Complete | Combines multiple completers (replaces `merge_completers`) |
| `ThreadedCompleter` | Complete | Background thread completion |
| `PathCompleter` | Complete | File system path completion |
| `Grammar.Compile()` / `GrammarCompleter` | Complete | Regular language grammar-based completion |
| `GrammarLexer` / `GrammarValidator` | Complete | Grammar-based syntax highlighting and validation |
| `FileHistory` / `ThreadedHistory` | Complete | Persistent and background-loaded history |
| `InMemoryHistory` | Complete | Session-scoped history |
| `ValidatorBase.FromCallable()` | Complete | Lambda-based validators (2 overloads) |
| `AutoSuggestFromHistory` | Complete | Fish-style history auto-suggestions |
| `ThreadedAutoSuggest` | Complete | Background thread suggestions |
| `Style` constructor / `Style.FromDict()` | Complete | CSS-like style definitions |
| `SwapLightAndDarkStyleTransformation` | Complete | Light/dark theme toggle |
| `Html` / `Ansi` formatted text | Complete | Rich prompt formatting |
| `AnyFormattedText` | Complete | Polymorphic text type with implicit conversions |
| `FormattedTextUtils.Merge()` | Complete | Combines formatted text fragments |
| `KeyBindings.Add<T>()` | Complete | Custom key binding registration |
| `Keys` enum (151 values) | Complete | All key constants |
| `CursorShape` enum / `ModalCursorShapeConfig` | Complete | Cursor shape customization |
| `ILexer` / `SimpleLexer` / `PygmentsLexer` | Complete | Syntax highlighting |
| `Application<T>.Run()` / `RunAsync()` | Complete | Full application lifecycle |
| `RunInTerminal` | Complete | Sync output during active prompts |
| `StdoutPatching.PatchStdout()` | Complete | Thread-safe stdout interception |
| `CompleteStyle` enum | Complete | Column, MultiColumn, ReadlineLike |
| `ColorDepth` enum | Complete | 1/4/8/24-bit color depths |
| `FilterOrBool` | Complete | Conditional parameter type |
| `IFilter` operators (`&`, `|`, `~`) | Complete | Composable boolean conditions |
| `EditingMode` enum | Complete | Default, Emacs, Vi |
| `InputHook` delegate | Complete | Event loop integration |
| Frame support (`showFrame` param) | Complete | Frame border around prompts |
| `AppFilters.IsDone` | Complete | Filter for accepted/done state |

**Alternatives Considered**: None — the Stroke library was specifically designed to match Python Prompt Toolkit's API surface.

### RT-002: Example Class Pattern (Established)

**Decision**: Use `public static class` with `public static void Run()` method, `try/catch` for `KeyboardInterruptException`/`EOFException`, kebab-case routing names.

**Rationale**: All existing example projects (Prompts, FullScreen, Dialogs, Choices, Telnet, Ssh) follow identical patterns:

```csharp
// Namespace: Stroke.Examples.Prompts (no sub-namespace)
namespace Stroke.Examples.Prompts;

/// <summary>
/// Brief description. Port of Python Prompt Toolkit's X example.
/// </summary>
public static class ExampleName
{
    public static void Run()
    {
        try
        {
            // Example code using Stroke public API
        }
        catch (KeyboardInterruptException) { /* Ctrl+C */ }
        catch (EOFException) { /* Ctrl+D */ }
    }
}
```

- **Program.cs**: `Dictionary<string, Action>` with `StringComparer.OrdinalIgnoreCase`, kebab-case keys
- **ShowUsage()**: Lists sorted example names
- **Exit on unknown**: `Console.Error.WriteLine` + `Environment.Exit(1)`
- **No RootNamespace**: The Prompts project does NOT set `<RootNamespace>` (unlike FullScreen which sets `Stroke.Examples.FullScreenExamples`)
- **.csproj**: Simple `net10.0` console app referencing `../../src/Stroke/Stroke.csproj`

**Alternatives Considered**: `async Task Run()` was considered but rejected — Python examples are synchronous, and the existing C# examples use `void Run()`. The async example (#49) uses `PromptAsync()` internally but `Run()` remains synchronous.

### RT-003: Routing Name Conventions

**Decision**: Use kebab-case routing names that mirror the Python file names, with backward compatibility for the 4 existing entries.

**Rationale**:
- Existing entries: `autocompletion`, `auto-suggestion`, `fuzzy-word-completer`, `get-input`
- New entries follow Python file names: `get-input-with-default`, `bottom-toolbar`, `colored-prompt`, etc.
- Subdirectory examples use path notation: `auto-completion/basic-completion`, `history/persistent-history`, `with-frames/basic-frame`
- Case-insensitive matching via `StringComparer.OrdinalIgnoreCase`
- **Note**: The existing `autocompletion` entry maps to the same example as `auto-completion/basic-completion`. Both routes should work.

**Alternatives Considered**: PascalCase routing was considered but rejected for consistency with existing kebab-case convention.

### RT-004: Python API Patterns → C# Translations

**Decision**: Apply standard Python → C# idiom translations.

| Python Pattern | C# Equivalent |
|---------------|---------------|
| `prompt("msg", ...)` | `Prompt.RunPrompt(message: "msg", ...)` |
| `session.prompt("msg")` | `session.Prompt(message: "msg")` |
| `PromptSession(history=h)` | `new PromptSession<string>(history: h)` |
| `Style.from_dict({...})` | `new Style([("k", "v"), ...])` |
| `WordCompleter([...], ignore_case=True)` | `new WordCompleter([...], ignoreCase: true)` |
| `lambda: "text"` (for toolbar) | `() => "text"` (via `AnyFormattedText` implicit conversion) |
| `HTML("<b>text</b>")` | `new Html("<b>text</b>")` |
| `ANSI("\x1b[31mtext")` | `new Ansi("\x1b[31mtext")` |
| `merge_completers([c1, c2])` | `CompletionUtils.Merge([c1, c2])` |
| `FileHistory(".file")` | `new FileHistory(".file")` |
| `ThreadedHistory(h)` | `new ThreadedHistory(h)` |
| `ThreadedCompleter(c)` or `complete_in_thread=True` | `completeInThread: true` parameter |
| `Validator.from_callable(fn, error_message="")` | `ValidatorBase.FromCallable(fn, errorMessage: "")` |
| `is_done` (filter) | `AppFilters.IsDone` |
| `~is_done` (negated filter) | `~AppFilters.IsDone` |
| `class MyCompleter(Completer):` | `class MyCompleter : ICompleter` |
| `class MyHistory(History):` | `class MyHistory : IHistory` (or `: HistoryBase`) |
| `yield Completion(...)` | Return `IEnumerable<Completion>` or `IAsyncEnumerable<Completion>` |
| `@kb.add('c-t')` | `kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.ControlT)])(handler)` |
| `run_in_terminal(fn)` | `RunInTerminal.RunAsync(() => fn())` |
| `patch_stdout()` context manager | `using (StdoutPatching.PatchStdout())` |
| `key_bindings=kb` | `keyBindings: kb` |
| `async def main():` + `asyncio.run(main())` | Synchronous `Run()` with async internals |

### RT-005: Advanced Example Patterns

**Decision**: Document special handling needed for the 10 advanced examples.

**Findings**:

1. **asyncio-prompt.py** → `AsyncPrompt.cs`: Python uses `asyncio.run()` with `prompt_toolkit_patch_stdout()`. C# uses `StdoutPatching.PatchStdout()` with background threads writing `Console.Write()`.

2. **patch-stdout.py** → `PatchStdout.cs`: Python's `patch_stdout()` context manager maps to `using (StdoutPatching.PatchStdout())`. Background thread prints every second.

3. **inputhook.py** → `InputHook.cs`: Python uses `inputhook` parameter with callable. C# uses `InputHook` delegate parameter on `Prompt.RunPrompt()`.

4. **finalterm-shell-integration.py** → `ShellIntegration.cs`: Emits OSC escape sequences (133;A, 133;B, 133;C, 133;D) around prompts. C# will use the same escape codes via a custom prompt continuation function.

5. **fancy-zsh-prompt.py** → `FancyZshPrompt.cs`: Dynamically measures terminal width and pads between left/right prompt parts. C# uses `FormattedTextUtils` + terminal width from `AppContext.GetAppSession()` or `Console.WindowWidth`.

6. **regular-language.py** → `RegularLanguage.cs`: Full calculator REPL with grammar-based completion/lexing/validation. Uses `Grammar.Compile()`, `GrammarCompleter`, `GrammarLexer`, `GrammarValidator`. REPL loop with `while(true)` + `EOFException` break.

7. **custom-vi-operator-and-text-object.py** → `CustomViOperator.cs`: Defines custom Vi operators ('R' for reverse) and text objects ('A' for all). Uses `ViMode.WaitingForTextObjectOrMotion` filter and `KeyPressEvent.Arg` for repeat counts.

8. **multiline-autosuggest.py** → `MultilineAutosuggest.cs`: Custom `IAutoSuggest` implementation + custom `IProcessor` for multi-line suggestion rendering. The processor transforms text tokens to show suggestions across multiple lines.

9. **slow-completions.py** → `SlowCompletions.cs`: Custom `ICompleter` with deliberate 200ms delay per completion. Uses `completeInThread: true` and a loading counter for toolbar display.

10. **swap-light-and-dark-colors.py** → `SwapLightDarkColors.cs`: Uses `SwapLightAndDarkStyleTransformation` and a toggle flag. Ctrl-T toggles `swapLightAndDarkColors` filter.

### RT-006: Existing Example Backward Compatibility

**Decision**: Preserve existing 4 routing entries as-is. Add new examples alongside them.

**Rationale**: The existing `Autocompletion.cs` (in `AutoCompletion/` subdirectory) maps to the same Python example as `auto-completion/autocompletion.py`. Per `examples-mapping.md`, the C# equivalent is `AutoCompletion/BasicCompletion.cs`. However, the existing file is already named `Autocompletion.cs` and has the routing key `"autocompletion"`.

**Resolution**:
- Keep `Autocompletion.cs` as-is (backward compatibility for `"autocompletion"` route)
- Add `"auto-completion/basic-completion"` as an alias pointing to the same `Autocompletion.Run`
- Similarly keep `FuzzyWordCompleter.cs` as-is, add `"auto-completion/fuzzy-word-completer"` alias
- Keep `AutoSuggestion.cs` and `GetInput.cs` as-is with their current routing keys
- All new examples get their own routing entries

### RT-007: File Organization

**Decision**: Follow `examples-mapping.md` directory structure exactly.

```
examples/Stroke.Examples.Prompts/
├── Program.cs                    (routing dictionary with 56+ entries)
├── Stroke.Examples.Prompts.csproj
├── GetInput.cs                   (EXISTING - #1)
├── GetInputWithDefault.cs        (#2 - NEW)
├── GetInputViMode.cs             (#3 - NEW)
├── GetPassword.cs                (#4 - NEW)
├── GetMultilineInput.cs          (#5 - NEW)
├── AcceptDefault.cs              (#6 - NEW)
├── ConfirmationPrompt.cs         (#7 - NEW)
├── PlaceholderText.cs            (#8 - NEW)
├── MouseSupport.cs               (#9 - NEW)
├── NoWrapping.cs                 (#10 - NEW)
├── MultilinePrompt.cs            (#11 - NEW)
├── OperateAndGetNext.cs          (#12 - NEW)
├── EnforceTtyInputOutput.cs      (#13 - NEW)
├── GetPasswordWithToggle.cs      (#14 - NEW)
├── ColoredPrompt.cs              (#15 - NEW)
├── BottomToolbar.cs              (#16 - NEW)
├── RightPrompt.cs                (#17 - NEW)
├── ClockInput.cs                 (#18 - NEW)
├── FancyZshPrompt.cs             (#19 - NEW)
├── TerminalTitle.cs              (#20 - NEW)
├── SwapLightDarkColors.cs        (#21 - NEW)
├── CursorShapes.cs               (#22 - NEW)
├── CustomKeyBinding.cs           (#23 - NEW)
├── CustomViOperator.cs           (#24 - NEW)
├── SystemPrompt.cs               (#25 - NEW)
├── SwitchViEmacs.cs              (#26 - NEW)
├── Autocorrection.cs             (#27 - NEW)
├── AutoSuggestion.cs             (EXISTING - #28)
├── MultilineAutosuggest.cs       (#29 - NEW)
├── UpArrowPartialMatch.cs        (#30 - NEW)
├── InputValidation.cs            (#45 - NEW)
├── RegularLanguage.cs            (#46 - NEW)
├── HtmlInput.cs                  (#47 - NEW)
├── CustomLexer.cs                (#48 - NEW)
├── AsyncPrompt.cs                (#49 - NEW)
├── PatchStdout.cs                (#50 - NEW)
├── InputHook.cs                  (#51 - NEW)
├── ShellIntegration.cs           (#52 - NEW)
├── SystemClipboard.cs            (#53 - NEW)
├── AutoCompletion/
│   ├── Autocompletion.cs         (EXISTING - #31, aliased as BasicCompletion)
│   ├── ControlSpaceTrigger.cs    (#32 - NEW)
│   ├── ReadlineStyle.cs          (#33 - NEW)
│   ├── ColoredCompletions.cs     (#34 - NEW)
│   ├── FormattedCompletions.cs   (#35 - NEW)
│   ├── MergedCompleters.cs       (#36 - NEW)
│   ├── FuzzyWordCompleter.cs     (EXISTING - #37, aliased as FuzzyWordCompleter)
│   ├── FuzzyCustomCompleter.cs   (#38 - NEW)
│   ├── MultiColumn.cs            (#39 - NEW)
│   ├── MultiColumnWithMeta.cs    (#40 - NEW)
│   ├── NestedCompletion.cs       (#41 - NEW)
│   └── SlowCompletions.cs        (#42 - NEW)
├── History/
│   ├── PersistentHistory.cs      (#43 - NEW)
│   └── SlowHistory.cs            (#44 - NEW)
└── WithFrames/
    ├── BasicFrame.cs             (#54 - NEW)
    ├── GrayFrameOnAccept.cs      (#55 - NEW)
    └── FrameWithCompletion.cs    (#56 - NEW)
```
