# Stroke Examples Mapping

## Overview

This document provides a complete 1:1 mapping of all 129 Python Prompt Toolkit examples to their .NET/C# equivalents in Stroke. Each Python example has a corresponding C# example with idiomatic naming and patterns.

## Organizational Structure

### Python PTK Structure
```
examples/
├── prompts/              (56 files)
│   ├── auto-completion/  (12 files)
│   ├── history/          (2 files)
│   └── with-frames/      (3 files)
├── full-screen/          (25 files)
│   ├── simple-demos/     (13 files)
│   └── scrollable-panes/ (2 files)
├── progress-bar/         (15 files)
├── dialogs/              (9 files)
├── print-text/           (9 files)
├── choices/              (8 files)
├── telnet/               (4 files)
├── ssh/                  (1 file)
├── tutorial/             (1 file)
└── gevent-get-input.py   (1 file)
```

### Stroke Structure
```
examples/
├── Stroke.Examples.Prompts/           (56 examples)
│   ├── AutoCompletion/                (12 examples)
│   ├── History/                       (2 examples)
│   └── WithFrames/                    (3 examples)
├── Stroke.Examples.FullScreen/        (25 examples)
│   ├── SimpleDemos/                   (13 examples)
│   └── ScrollablePanes/               (2 examples)
├── Stroke.Examples.ProgressBar/       (15 examples)
├── Stroke.Examples.Dialogs/           (9 examples)
├── Stroke.Examples.PrintText/         (9 examples)
├── Stroke.Examples.Choices/           (8 examples)
├── Stroke.Examples.Telnet/            (4 examples)
├── Stroke.Examples.Ssh/               (1 example)
└── Stroke.Examples.Tutorial/          (1 example)
```

## Naming Conventions

| Python Pattern | C# Pattern |
|----------------|------------|
| `get-input.py` | `GetInput.cs` |
| `auto_suggestion.py` | `AutoSuggestion.cs` |
| `full-screen-demo.py` | `FullScreenDemo.cs` |
| `a-lot-of-parallel-tasks.py` | `ManyParallelTasks.cs` |
| `styled-1.py` | `Styled1.cs` |
| `styled-tqdm-1.py` | `StyledTqdm1.cs` |

**Rules:**
1. Hyphens (`-`) → PascalCase word boundary
2. Underscores (`_`) → PascalCase word boundary
3. Numbers preserved
4. File extension: `.py` → `.cs`
5. Each example is a standalone console application

---

## Complete Example Mapping

### Prompts (56 Examples)

#### Core/Basic Prompts

| # | Python File | C# File | Description | Complexity |
|---|------------|---------|-------------|------------|
| 1 | `prompts/get-input.py` | `Prompts/GetInput.cs` | Simplest prompt example | Basic |
| 2 | `prompts/get-input-with-default.py` | `Prompts/GetInputWithDefault.cs` | Prompt with pre-filled default | Basic |
| 3 | `prompts/get-input-vi-mode.py` | `Prompts/GetInputViMode.cs` | Vi editing mode | Basic |
| 4 | `prompts/get-password.py` | `Prompts/GetPassword.cs` | Masked password input | Basic |
| 5 | `prompts/get-multiline-input.py` | `Prompts/GetMultilineInput.cs` | Multi-line text input | Basic |
| 6 | `prompts/accept-default.py` | `Prompts/AcceptDefault.cs` | Auto-accept default | Basic |
| 7 | `prompts/confirmation-prompt.py` | `Prompts/ConfirmationPrompt.cs` | Yes/no confirmation | Basic |
| 8 | `prompts/placeholder-text.py` | `Prompts/PlaceholderText.cs` | Placeholder when empty | Basic |
| 9 | `prompts/mouse-support.py` | `Prompts/MouseSupport.cs` | Mouse selection support | Basic |
| 10 | `prompts/no-wrapping.py` | `Prompts/NoWrapping.cs` | Disable line wrapping | Basic |
| 11 | `prompts/multiline-prompt.py` | `Prompts/MultilinePrompt.cs` | Basic multi-line | Basic |
| 12 | `prompts/operate-and-get-next.py` | `Prompts/OperateAndGetNext.cs` | Session reuse pattern | Basic |
| 13 | `prompts/enforce-tty-input-output.py` | `Prompts/EnforceTtyInputOutput.cs` | Force TTY mode | Intermediate |

#### Password & Security

| # | Python File | C# File | Description | Complexity |
|---|------------|---------|-------------|------------|
| 14 | `prompts/get-password-with-toggle-display-shortcut.py` | `Prompts/GetPasswordWithToggle.cs` | Toggle password visibility | Intermediate |

#### Styling & Formatting

| # | Python File | C# File | Description | Complexity |
|---|------------|---------|-------------|------------|
| 15 | `prompts/colored-prompt.py` | `Prompts/ColoredPrompt.cs` | Multi-style prompts | Intermediate |
| 16 | `prompts/bottom-toolbar.py` | `Prompts/BottomToolbar.cs` | Bottom toolbar display | Intermediate |
| 17 | `prompts/rprompt.py` | `Prompts/RightPrompt.cs` | Right-aligned prompt | Intermediate |
| 18 | `prompts/clock-input.py` | `Prompts/ClockInput.cs` | Dynamic time display | Intermediate |
| 19 | `prompts/fancy-zsh-prompt.py` | `Prompts/FancyZshPrompt.cs` | Complex Zsh-style prompt | Advanced |
| 20 | `prompts/terminal-title.py` | `Prompts/TerminalTitle.cs` | Set terminal title | Basic |
| 21 | `prompts/swap-light-and-dark-colors.py` | `Prompts/SwapLightDarkColors.cs` | Toggle color schemes | Advanced |
| 22 | `prompts/cursor-shapes.py` | `Prompts/CursorShapes.cs` | Cursor customization | Intermediate |

#### Key Bindings & Input Handling

| # | Python File | C# File | Description | Complexity |
|---|------------|---------|-------------|------------|
| 23 | `prompts/custom-key-binding.py` | `Prompts/CustomKeyBinding.cs` | Custom key bindings | Intermediate |
| 24 | `prompts/custom-vi-operator-and-text-object.py` | `Prompts/CustomViOperator.cs` | Custom Vi operators | Advanced |
| 25 | `prompts/system-prompt.py` | `Prompts/SystemPrompt.cs` | System command execution | Intermediate |
| 26 | `prompts/switch-between-vi-emacs.py` | `Prompts/SwitchViEmacs.cs` | Toggle editing modes | Intermediate |
| 27 | `prompts/autocorrection.py` | `Prompts/Autocorrection.cs` | Auto-correct on space | Intermediate |

#### Auto-Suggestion & History

| # | Python File | C# File | Description | Complexity |
|---|------------|---------|-------------|------------|
| 28 | `prompts/auto-suggestion.py` | `Prompts/AutoSuggestion.cs` | Fish-style suggestions | Intermediate |
| 29 | `prompts/multiline-autosuggest.py` | `Prompts/MultilineAutosuggest.cs` | Multi-line suggestions | Advanced |
| 30 | `prompts/up-arrow-partial-string-matching.py` | `Prompts/UpArrowPartialMatch.cs` | History prefix search | Intermediate |

#### Auto-Completion (Subdirectory)

| # | Python File | C# File | Description | Complexity |
|---|------------|---------|-------------|------------|
| 31 | `prompts/auto-completion/autocompletion.py` | `Prompts/AutoCompletion/BasicCompletion.cs` | Basic word completion | Intermediate |
| 32 | `prompts/auto-completion/autocomplete-with-control-space.py` | `Prompts/AutoCompletion/ControlSpaceTrigger.cs` | Custom trigger | Intermediate |
| 33 | `prompts/auto-completion/autocompletion-like-readline.py` | `Prompts/AutoCompletion/ReadlineStyle.cs` | Readline-style | Intermediate |
| 34 | `prompts/auto-completion/colored-completions.py` | `Prompts/AutoCompletion/ColoredCompletions.cs` | Color-coded completions | Intermediate |
| 35 | `prompts/auto-completion/colored-completions-with-formatted-text.py` | `Prompts/AutoCompletion/FormattedCompletions.cs` | HTML-formatted completions | Advanced |
| 36 | `prompts/auto-completion/combine-multiple-completers.py` | `Prompts/AutoCompletion/MergedCompleters.cs` | Merged completers | Intermediate |
| 37 | `prompts/auto-completion/fuzzy-word-completer.py` | `Prompts/AutoCompletion/FuzzyWordCompleter.cs` | Fuzzy matching | Intermediate |
| 38 | `prompts/auto-completion/fuzzy-custom-completer.py` | `Prompts/AutoCompletion/FuzzyCustomCompleter.cs` | Custom fuzzy completer | Intermediate |
| 39 | `prompts/auto-completion/multi-column-autocompletion.py` | `Prompts/AutoCompletion/MultiColumn.cs` | Multi-column display | Intermediate |
| 40 | `prompts/auto-completion/multi-column-autocompletion-with-meta.py` | `Prompts/AutoCompletion/MultiColumnWithMeta.cs` | Multi-column + metadata | Intermediate |
| 41 | `prompts/auto-completion/nested-autocompletion.py` | `Prompts/AutoCompletion/NestedCompletion.cs` | Hierarchical completion | Intermediate |
| 42 | `prompts/auto-completion/slow-completions.py` | `Prompts/AutoCompletion/SlowCompletions.cs` | Threaded completions | Advanced |

#### History (Subdirectory)

| # | Python File | C# File | Description | Complexity |
|---|------------|---------|-------------|------------|
| 43 | `prompts/history/persistent-history.py` | `Prompts/History/PersistentHistory.cs` | File-based history | Intermediate |
| 44 | `prompts/history/slow-history.py` | `Prompts/History/SlowHistory.cs` | Threaded history | Advanced |

#### Validation & Lexing

| # | Python File | C# File | Description | Complexity |
|---|------------|---------|-------------|------------|
| 45 | `prompts/input-validation.py` | `Prompts/InputValidation.cs` | Email validation | Intermediate |
| 46 | `prompts/regular-language.py` | `Prompts/RegularLanguage.cs` | Grammar-based validation | Advanced |
| 47 | `prompts/html-input.py` | `Prompts/HtmlInput.cs` | HTML syntax highlighting | Intermediate |
| 48 | `prompts/custom-lexer.py` | `Prompts/CustomLexer.cs` | Custom rainbow lexer | Intermediate |

#### Advanced Features

| # | Python File | C# File | Description | Complexity |
|---|------------|---------|-------------|------------|
| 49 | `prompts/asyncio-prompt.py` | `Prompts/AsyncPrompt.cs` | Async prompt with background tasks | Advanced |
| 50 | `prompts/patch-stdout.py` | `Prompts/PatchStdout.cs` | Thread-safe stdout | Advanced |
| 51 | `prompts/inputhook.py` | `Prompts/InputHook.cs` | External event loop integration | Advanced |
| 52 | `prompts/finalterm-shell-integration.py` | `Prompts/ShellIntegration.cs` | Terminal integration markers | Advanced |
| 53 | `prompts/system-clipboard-integration.py` | `Prompts/SystemClipboard.cs` | System clipboard support | Intermediate |

#### With Frames (Subdirectory)

| # | Python File | C# File | Description | Complexity |
|---|------------|---------|-------------|------------|
| 54 | `prompts/with-frames/with-frame.py` | `Prompts/WithFrames/BasicFrame.cs` | Simple frame border | Basic |
| 55 | `prompts/with-frames/gray-frame-on-accept.py` | `Prompts/WithFrames/GrayFrameOnAccept.cs` | Frame styling on accept | Intermediate |
| 56 | `prompts/with-frames/frame-and-autocompletion.py` | `Prompts/WithFrames/FrameWithCompletion.cs` | Frame + completion | Intermediate |

---

### Full-Screen Applications (25 Examples)

#### Core/Basic

| # | Python File | C# File | Description | Complexity |
|---|------------|---------|-------------|------------|
| 57 | `full-screen/hello-world.py` | `FullScreen/HelloWorld.cs` | Minimal application | Basic |
| 58 | `full-screen/dummy-app.py` | `FullScreen/DummyApp.cs` | Empty full-screen app | Basic |
| 59 | `full-screen/no-layout.py` | `FullScreen/NoLayout.cs` | App without layout | Basic |

#### Layout & Containers

| # | Python File | C# File | Description | Complexity |
|---|------------|---------|-------------|------------|
| 60 | `full-screen/simple-demos/horizontal-split.py` | `FullScreen/SimpleDemos/HorizontalSplit.cs` | HSplit container | Basic |
| 61 | `full-screen/simple-demos/vertical-split.py` | `FullScreen/SimpleDemos/VerticalSplit.cs` | VSplit container | Basic |
| 62 | `full-screen/split-screen.py` | `FullScreen/SplitScreen.cs` | Complex split layout | Intermediate |

#### Alignment

| # | Python File | C# File | Description | Complexity |
|---|------------|---------|-------------|------------|
| 63 | `full-screen/simple-demos/alignment.py` | `FullScreen/SimpleDemos/Alignment.cs` | Text alignment options | Intermediate |
| 64 | `full-screen/simple-demos/horizontal-align.py` | `FullScreen/SimpleDemos/HorizontalAlign.cs` | HSplit alignment | Intermediate |
| 65 | `full-screen/simple-demos/vertical-align.py` | `FullScreen/SimpleDemos/VerticalAlign.cs` | VSplit alignment | Intermediate |

#### Text Input & Buffers

| # | Python File | C# File | Description | Complexity |
|---|------------|---------|-------------|------------|
| 66 | `full-screen/buttons.py` | `FullScreen/Buttons.cs` | Button widgets | Intermediate |
| 67 | `full-screen/calculator.py` | `FullScreen/Calculator.cs` | Calculator REPL | Intermediate |
| 68 | `full-screen/simple-demos/autocompletion.py` | `FullScreen/SimpleDemos/AutoCompletion.cs` | Completion menu | Intermediate |

#### Visual Display

| # | Python File | C# File | Description | Complexity |
|---|------------|---------|-------------|------------|
| 69 | `full-screen/ansi-art-and-textarea.py` | `FullScreen/AnsiArtAndTextArea.cs` | ANSI art rendering | Intermediate |
| 70 | `full-screen/simple-demos/colorcolumn.py` | `FullScreen/SimpleDemos/ColorColumn.cs` | Column indicators | Intermediate |
| 71 | `full-screen/simple-demos/cursorcolumn-cursorline.py` | `FullScreen/SimpleDemos/CursorHighlight.cs` | Cursor highlighting | Intermediate |
| 72 | `full-screen/simple-demos/margins.py` | `FullScreen/SimpleDemos/Margins.cs` | Line numbers, scrollbars | Intermediate |
| 73 | `full-screen/simple-demos/line-prefixes.py` | `FullScreen/SimpleDemos/LinePrefixes.cs` | Custom line prefixes | Intermediate |

#### Floating Windows

| # | Python File | C# File | Description | Complexity |
|---|------------|---------|-------------|------------|
| 74 | `full-screen/simple-demos/floats.py` | `FullScreen/SimpleDemos/Floats.cs` | Multiple floats | Intermediate |
| 75 | `full-screen/simple-demos/float-transparency.py` | `FullScreen/SimpleDemos/FloatTransparency.cs` | Float transparency | Intermediate |

#### Scrollable Panes

| # | Python File | C# File | Description | Complexity |
|---|------------|---------|-------------|------------|
| 76 | `full-screen/scrollable-panes/simple-example.py` | `FullScreen/ScrollablePanes/SimpleExample.cs` | Basic scrollable | Intermediate |
| 77 | `full-screen/scrollable-panes/with-completion-menu.py` | `FullScreen/ScrollablePanes/WithCompletionMenu.cs` | Scrollable + completion | Intermediate |

#### Search & Navigation

| # | Python File | C# File | Description | Complexity |
|---|------------|---------|-------------|------------|
| 78 | `full-screen/pager.py` | `FullScreen/Pager.cs` | Pager application | Intermediate |
| 79 | `full-screen/simple-demos/focus.py` | `FullScreen/SimpleDemos/Focus.cs` | Focus management | Intermediate |

#### Full Applications

| # | Python File | C# File | Description | Complexity |
|---|------------|---------|-------------|------------|
| 80 | `full-screen/full-screen-demo.py` | `FullScreen/FullScreenDemo.cs` | UI component showcase | Advanced |
| 81 | `full-screen/text-editor.py` | `FullScreen/TextEditor.cs` | Complete text editor | Advanced |

---

### Progress Bar (15 Examples)

| # | Python File | C# File | Description | Complexity |
|---|------------|---------|-------------|------------|
| 82 | `progress-bar/simple-progress-bar.py` | `ProgressBar/SimpleProgressBar.cs` | Basic progress bar | Basic |
| 83 | `progress-bar/two-tasks.py` | `ProgressBar/TwoTasks.cs` | Parallel progress bars | Intermediate |
| 84 | `progress-bar/unknown-length.py` | `ProgressBar/UnknownLength.cs` | Generator iteration | Basic |
| 85 | `progress-bar/nested-progress-bars.py` | `ProgressBar/NestedProgressBars.cs` | Hierarchical progress | Intermediate |
| 86 | `progress-bar/styled-1.py` | `ProgressBar/Styled1.cs` | Custom styling | Intermediate |
| 87 | `progress-bar/styled-2.py` | `ProgressBar/Styled2.cs` | Custom formatters | Intermediate |
| 88 | `progress-bar/styled-rainbow.py` | `ProgressBar/StyledRainbow.cs` | Rainbow colors | Intermediate |
| 89 | `progress-bar/styled-apt-get-install.py` | `ProgressBar/StyledAptGet.cs` | Apt-get style | Intermediate |
| 90 | `progress-bar/styled-tqdm-1.py` | `ProgressBar/StyledTqdm1.cs` | Tqdm style 1 | Intermediate |
| 91 | `progress-bar/styled-tqdm-2.py` | `ProgressBar/StyledTqdm2.cs` | Tqdm style 2 | Intermediate |
| 92 | `progress-bar/colored-title-and-label.py` | `ProgressBar/ColoredTitleLabel.cs` | HTML title/label | Basic |
| 93 | `progress-bar/custom-key-bindings.py` | `ProgressBar/CustomKeyBindings.cs` | Key bindings in progress | Intermediate |
| 94 | `progress-bar/many-parallel-tasks.py` | `ProgressBar/ManyParallelTasks.cs` | 8 parallel tasks | Advanced |
| 95 | `progress-bar/a-lot-of-parallel-tasks.py` | `ProgressBar/LotOfParallelTasks.cs` | 160 parallel tasks | Advanced |
| 96 | `progress-bar/scrolling-task-name.py` | `ProgressBar/ScrollingTaskName.cs` | Scrolling labels | Basic |

---

### Dialogs (9 Examples)

| # | Python File | C# File | Description | Complexity |
|---|------------|---------|-------------|------------|
| 97 | `dialogs/button_dialog.py` | `Dialogs/ButtonDialog.cs` | Multiple button options | Basic |
| 98 | `dialogs/checkbox_dialog.py` | `Dialogs/CheckboxDialog.cs` | Multi-select checkbox | Intermediate |
| 99 | `dialogs/input_dialog.py` | `Dialogs/InputDialog.cs` | Text input dialog | Basic |
| 100 | `dialogs/messagebox.py` | `Dialogs/MessageBox.cs` | Simple message box | Basic |
| 101 | `dialogs/password_dialog.py` | `Dialogs/PasswordDialog.cs` | Password input | Basic |
| 102 | `dialogs/progress_dialog.py` | `Dialogs/ProgressDialog.cs` | Background task progress | Advanced |
| 103 | `dialogs/radio_dialog.py` | `Dialogs/RadioDialog.cs` | Single-select radio | Intermediate |
| 104 | `dialogs/styled_messagebox.py` | `Dialogs/StyledMessageBox.cs` | Styled message box | Intermediate |
| 105 | `dialogs/yes_no_dialog.py` | `Dialogs/YesNoDialog.cs` | Boolean confirmation | Basic |

---

### Print Text (9 Examples)

| # | Python File | C# File | Description | Complexity |
|---|------------|---------|-------------|------------|
| 106 | `print-text/ansi-colors.py` | `PrintText/AnsiColors.cs` | 16 ANSI colors | Basic |
| 107 | `print-text/pygments-tokens.py` | `PrintText/PygmentsTokens.cs` | Syntax highlighting | Basic |
| 108 | `print-text/html.py` | `PrintText/Html.cs` | HTML formatting | Basic |
| 109 | `print-text/named-colors.py` | `PrintText/NamedColors.cs` | Named color palette | Basic |
| 110 | `print-text/true-color-demo.py` | `PrintText/TrueColorDemo.cs` | 24-bit color gradients | Basic |
| 111 | `print-text/ansi.py` | `PrintText/Ansi.cs` | ANSI escape sequences | Basic |
| 112 | `print-text/print-frame.py` | `PrintText/PrintFrame.cs` | Container printing | Basic |
| 113 | `print-text/print-formatted-text.py` | `PrintText/PrintFormattedText.cs` | Multiple format methods | Basic |
| 114 | `print-text/prompt-toolkit-logo-ansi-art.py` | `PrintText/LogoAnsiArt.cs` | ANSI art logo | Basic |

---

### Choices (8 Examples)

| # | Python File | C# File | Description | Complexity |
|---|------------|---------|-------------|------------|
| 115 | `choices/default.py` | `Choices/Default.cs` | Default selection | Basic |
| 116 | `choices/simple-selection.py` | `Choices/SimpleSelection.cs` | Basic selection | Basic |
| 117 | `choices/color.py` | `Choices/Color.cs` | Colored choices | Basic |
| 118 | `choices/mouse-support.py` | `Choices/MouseSupport.cs` | Mouse selection | Basic |
| 119 | `choices/frame-and-bottom-toolbar.py` | `Choices/FrameAndToolbar.cs` | Frame + toolbar | Intermediate |
| 120 | `choices/gray-frame-on-accept.py` | `Choices/GrayFrameOnAccept.cs` | Style on accept | Intermediate |
| 121 | `choices/multiselect.py` | `Choices/Multiselect.cs` | Multi-select | Intermediate |
| 122 | `choices/checkbox.py` | `Choices/Checkbox.cs` | Checkbox list | Intermediate |

---

### Telnet (4 Examples)

| # | Python File | C# File | Description | Complexity |
|---|------------|---------|-------------|------------|
| 123 | `telnet/hello-world.py` | `Telnet/HelloWorld.cs` | Basic telnet server | Intermediate |
| 124 | `telnet/toolbar.py` | `Telnet/Toolbar.cs` | Telnet with toolbar | Intermediate |
| 125 | `telnet/dialog.py` | `Telnet/Dialog.cs` | Telnet dialog | Intermediate |
| 126 | `telnet/chat-app.py` | `Telnet/ChatApp.cs` | Multi-user chat | Advanced |

---

### SSH (1 Example)

| # | Python File | C# File | Description | Complexity |
|---|------------|---------|-------------|------------|
| 127 | `ssh/asyncssh-server.py` | `Ssh/SshServer.cs` | SSH server integration | Advanced |

---

### Tutorial (1 Example)

| # | Python File | C# File | Description | Complexity |
|---|------------|---------|-------------|------------|
| 128 | `tutorial/sqlite-cli.py` | `Tutorial/SqliteCli.cs` | SQLite REPL | Advanced |

---

### Standalone (1 Example)

| # | Python File | C# File | Description | Complexity |
|---|------------|---------|-------------|------------|
| 129 | `gevent-get-input.py` | `Advanced/AsyncEventLoop.cs` | Async event loop | Advanced |

---

## Project Structure

### Solution Layout

```
examples/
├── Stroke.Examples.sln
│
├── Stroke.Examples.Prompts/
│   ├── Stroke.Examples.Prompts.csproj
│   ├── Program.cs                      # Entry point with example selector
│   ├── GetInput.cs
│   ├── GetPassword.cs
│   ├── ColoredPrompt.cs
│   ├── AutoCompletion/
│   │   ├── BasicCompletion.cs
│   │   ├── FuzzyWordCompleter.cs
│   │   └── ...
│   ├── History/
│   │   ├── PersistentHistory.cs
│   │   └── SlowHistory.cs
│   └── WithFrames/
│       ├── BasicFrame.cs
│       └── ...
│
├── Stroke.Examples.FullScreen/
│   ├── Stroke.Examples.FullScreen.csproj
│   ├── Program.cs
│   ├── HelloWorld.cs
│   ├── TextEditor.cs
│   ├── SimpleDemos/
│   │   ├── HorizontalSplit.cs
│   │   └── ...
│   └── ScrollablePanes/
│       └── ...
│
├── Stroke.Examples.ProgressBar/
│   ├── Stroke.Examples.ProgressBar.csproj
│   └── ...
│
├── Stroke.Examples.Dialogs/
│   ├── Stroke.Examples.Dialogs.csproj
│   └── ...
│
├── Stroke.Examples.PrintText/
│   ├── Stroke.Examples.PrintText.csproj
│   └── ...
│
├── Stroke.Examples.Choices/
│   ├── Stroke.Examples.Choices.csproj
│   └── ...
│
├── Stroke.Examples.Telnet/
│   ├── Stroke.Examples.Telnet.csproj
│   └── ...
│
├── Stroke.Examples.Ssh/
│   ├── Stroke.Examples.Ssh.csproj
│   └── ...
│
└── Stroke.Examples.Tutorial/
    ├── Stroke.Examples.Tutorial.csproj
    └── SqliteCli.cs
```

### Project File Template

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <LangVersion>13</LangVersion>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\src\Stroke\Stroke.csproj" />
  </ItemGroup>

</Project>
```

---

## Code Pattern Mappings

### Python → C# Idiom Translations

#### Basic Prompt

**Python:**
```python
from prompt_toolkit import prompt

result = prompt("Enter your name: ")
print(f"Hello, {result}!")
```

**C#:**
```csharp
using Stroke.Shortcuts;

var result = await Prompt.PromptAsync("Enter your name: ");
Console.WriteLine($"Hello, {result}!");
```

#### Prompt with Completion

**Python:**
```python
from prompt_toolkit import prompt
from prompt_toolkit.completion import WordCompleter

completer = WordCompleter(['apple', 'banana', 'cherry'])
result = prompt("Pick a fruit: ", completer=completer)
```

**C#:**
```csharp
using Stroke.Shortcuts;
using Stroke.Completion;

var completer = new WordCompleter(["apple", "banana", "cherry"]);
var result = await Prompt.PromptAsync("Pick a fruit: ", completer: completer);
```

#### Styled Prompt

**Python:**
```python
from prompt_toolkit import prompt, HTML
from prompt_toolkit.styles import Style

style = Style.from_dict({
    'prompt': '#00aa00 bold',
})
result = prompt(HTML('<prompt>>>> </prompt>'), style=style)
```

**C#:**
```csharp
using Stroke.Shortcuts;
using Stroke.FormattedText;
using Stroke.Styles;

var style = Style.FromDict(new Dictionary<string, string>
{
    ["prompt"] = "#00aa00 bold"
});
var result = await Prompt.PromptAsync(
    new Html("<prompt>>>> </prompt>"),
    style: style);
```

#### Key Bindings

**Python:**
```python
from prompt_toolkit import prompt
from prompt_toolkit.key_binding import KeyBindings

bindings = KeyBindings()

@bindings.add('c-t')
def _(event):
    print("Control-T pressed!")

result = prompt("> ", key_bindings=bindings)
```

**C#:**
```csharp
using Stroke.Shortcuts;
using Stroke.KeyBinding;

var bindings = new KeyBindings();
bindings.Add(Keys.ControlT, (e) =>
{
    Console.WriteLine("Control-T pressed!");
});

var result = await Prompt.PromptAsync("> ", keyBindings: bindings);
```

#### Full-Screen Application

**Python:**
```python
from prompt_toolkit import Application
from prompt_toolkit.layout import Layout, HSplit, Window
from prompt_toolkit.layout.controls import FormattedTextControl
from prompt_toolkit.key_binding import KeyBindings

kb = KeyBindings()

@kb.add('c-c')
def _(event):
    event.app.exit()

layout = Layout(
    HSplit([
        Window(FormattedTextControl("Hello, World!")),
    ])
)

app = Application(layout=layout, key_bindings=kb, full_screen=True)
app.run()
```

**C#:**
```csharp
using Stroke.Application;
using Stroke.Layout;
using Stroke.Layout.Controls;
using Stroke.KeyBinding;

var kb = new KeyBindings();
kb.Add(Keys.ControlC, (e) => e.App.Exit());

var layout = new Layout(
    new HSplit(
        new Window(new FormattedTextControl("Hello, World!"))
    )
);

var app = new Application(layout: layout, keyBindings: kb, fullScreen: true);
await app.RunAsync();
```

#### Progress Bar

**Python:**
```python
from prompt_toolkit.shortcuts import ProgressBar

with ProgressBar() as pb:
    for i in pb(range(100)):
        time.sleep(0.05)
```

**C#:**
```csharp
using Stroke.Shortcuts;

await using var pb = new ProgressBar();
await foreach (var i in pb.Iterate(Enumerable.Range(0, 100)))
{
    await Task.Delay(50);
}
```

#### Dialog

**Python:**
```python
from prompt_toolkit.shortcuts import yes_no_dialog

result = yes_no_dialog(
    title="Confirm",
    text="Do you want to continue?"
).run()
```

**C#:**
```csharp
using Stroke.Shortcuts;

var result = await Dialogs.YesNoDialogAsync(
    title: "Confirm",
    text: "Do you want to continue?");
```

---

## Example Categories by Complexity

### Basic (42 examples)
Entry-level examples demonstrating single features:
- Simple prompts, passwords, multi-line input
- Basic text formatting and colors
- Simple dialogs
- Minimal full-screen apps

### Intermediate (67 examples)
Examples combining multiple features:
- Styled prompts with completion
- Custom key bindings
- Layout containers with alignment
- Progress bars with styling
- Session management

### Advanced (20 examples)
Complex, production-quality examples:
- Full text editor
- Multi-user chat server
- SSH/Telnet servers
- Grammar-based parsing
- Async event loop integration
- Custom Vi operators

---

## Summary Statistics

| Category | Python Files | C# Files | Basic | Intermediate | Advanced |
|----------|-------------|----------|-------|--------------|----------|
| Prompts | 56 | 56 | 13 | 33 | 10 |
| Full-Screen | 25 | 25 | 3 | 19 | 3 |
| Progress Bar | 15 | 15 | 4 | 9 | 2 |
| Dialogs | 9 | 9 | 5 | 3 | 1 |
| Print Text | 9 | 9 | 9 | 0 | 0 |
| Choices | 8 | 8 | 4 | 4 | 0 |
| Telnet | 4 | 4 | 0 | 3 | 1 |
| SSH | 1 | 1 | 0 | 0 | 1 |
| Tutorial | 1 | 1 | 0 | 0 | 1 |
| Standalone | 1 | 1 | 0 | 0 | 1 |
| **TOTAL** | **129** | **129** | **38** | **71** | **20** |

---

## Implementation Order

### Phase 1: Foundation Examples
1. `PrintText/*` (9 examples) - Text output, colors, formatting
2. `Prompts/GetInput.cs`, `GetPassword.cs`, `GetMultilineInput.cs` - Basic input
3. `Dialogs/MessageBox.cs`, `YesNoDialog.cs`, `InputDialog.cs` - Simple dialogs

### Phase 2: Interactive Features
4. `Prompts/AutoCompletion/*` (12 examples) - Completion system
5. `Prompts/ColoredPrompt.cs`, `BottomToolbar.cs`, `RightPrompt.cs` - Styling
6. `Prompts/CustomKeyBinding.cs`, `SwitchViEmacs.cs` - Key bindings

### Phase 3: Full-Screen Applications
7. `FullScreen/HelloWorld.cs`, `SimpleDemos/*` (13 examples) - Layout basics
8. `FullScreen/Calculator.cs`, `Pager.cs` - Working applications
9. `FullScreen/TextEditor.cs` - Complex application

### Phase 4: Progress & Dialogs
10. `ProgressBar/*` (15 examples) - Progress feedback
11. `Dialogs/*` (remaining) - All dialog types
12. `Choices/*` (8 examples) - Selection interfaces

### Phase 5: Advanced Features
13. `Telnet/*`, `Ssh/*` - Server integration
14. `Tutorial/SqliteCli.cs` - Complete REPL
15. Advanced prompt features (async, grammar, etc.)

---

## Constitutional Compliance

All examples adhere to constitutional requirements:

- **100% API Fidelity**: Every Python example has a C# equivalent
- **No Scope Reduction**: All 129 examples mapped
- **Naming Conventions**: `snake_case` → `PascalCase`
- **Platform Support**: Cross-platform (Windows, macOS, Linux)
- **No Mocking**: Examples use real implementations
