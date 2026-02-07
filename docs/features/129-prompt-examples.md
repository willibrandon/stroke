# Feature 129: Prompt Examples (Complete Set)

## Overview

Implement ALL 56 Python Prompt Toolkit prompt examples in the `Stroke.Examples.Prompts` project. These examples demonstrate the PromptSession API, completion styles, key bindings, validation, history, auto-suggestion, styling, and various prompt patterns. Four examples already exist (`GetInput`, `AutoSuggestion`, `Autocompletion`, `FuzzyWordCompleter`); the remaining 52 must be implemented.

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/examples/prompts/`

### Core/Basic Prompts

| # | Python File | C# File | Description | Status |
|---|-------------|---------|-------------|--------|
| 1 | `get-input.py` | `GetInput.cs` | Simplest prompt example | Done |
| 2 | `get-input-with-default.py` | `GetInputWithDefault.cs` | Prompt with pre-filled editable default | TODO |
| 3 | `get-input-vi-mode.py` | `GetInputViMode.cs` | Vi editing mode prompt | TODO |
| 4 | `get-password.py` | `GetPassword.cs` | Masked password input | TODO |
| 5 | `get-multiline-input.py` | `GetMultilineInput.cs` | Multi-line text with continuation prompt | TODO |
| 6 | `accept-default.py` | `AcceptDefault.cs` | Auto-accept default without editing | TODO |
| 7 | `confirmation-prompt.py` | `ConfirmationPrompt.cs` | Yes/no confirmation dialog | TODO |
| 8 | `placeholder-text.py` | `PlaceholderText.cs` | Gray placeholder when input is empty | TODO |
| 9 | `mouse-support.py` | `MouseSupport.cs` | Mouse click selection in multiline | TODO |
| 10 | `no-wrapping.py` | `NoWrapping.cs` | Horizontal scroll instead of wrap | TODO |
| 11 | `multiline-prompt.py` | `MultilinePrompt.cs` | Basic multi-line input | TODO |
| 12 | `operate-and-get-next.py` | `OperateAndGetNext.cs` | Session reuse in REPL loop | TODO |
| 13 | `enforce-tty-input-output.py` | `EnforceTtyInputOutput.cs` | Force TTY even when piped | TODO |

### Password & Security

| # | Python File | C# File | Description | Status |
|---|-------------|---------|-------------|--------|
| 14 | `get-password-with-toggle-display-shortcut.py` | `GetPasswordWithToggle.cs` | Ctrl-T toggles password visibility | TODO |

### Styling & Formatting

| # | Python File | C# File | Description | Status |
|---|-------------|---------|-------------|--------|
| 15 | `colored-prompt.py` | `ColoredPrompt.cs` | Style tuples, HTML, and ANSI prompts | TODO |
| 16 | `bottom-toolbar.py` | `BottomToolbar.cs` | 7 toolbar variants (fixed, callable, HTML, ANSI, styled, tokens, multiline) | TODO |
| 17 | `rprompt.py` | `RightPrompt.cs` | Right-aligned prompt (ZSH RPROMPT) | TODO |
| 18 | `clock-input.py` | `ClockInput.cs` | Dynamic time in prompt with refresh_interval | TODO |
| 19 | `fancy-zsh-prompt.py` | `FancyZshPrompt.cs` | Two-part prompt with dynamic padding | TODO |
| 20 | `terminal-title.py` | `TerminalTitle.cs` | Set terminal window title | TODO |
| 21 | `swap-light-and-dark-colors.py` | `SwapLightDarkColors.cs` | Ctrl-T toggles color swap | TODO |
| 22 | `cursor-shapes.py` | `CursorShapes.cs` | Block, underline, beam, modal cursors | TODO |

### Key Bindings & Input Handling

| # | Python File | C# File | Description | Status |
|---|-------------|---------|-------------|--------|
| 23 | `custom-key-binding.py` | `CustomKeyBinding.cs` | F4 inserts text, xy→z, abc→d, Ctrl-T prints, Ctrl-K async | TODO |
| 24 | `custom-vi-operator-and-text-object.py` | `CustomViOperator.cs` | Custom Vi operator 'R' (reverse) and text object 'A' (all) | TODO |
| 25 | `system-prompt.py` | `SystemPrompt.cs` | Meta-! system commands, Ctrl-Z suspend, Ctrl-X Ctrl-E editor | TODO |
| 26 | `switch-between-vi-emacs.py` | `SwitchViEmacs.cs` | F4 toggles Vi/Emacs editing modes | TODO |
| 27 | `autocorrection.py` | `Autocorrection.cs` | Auto-correct typos on space press | TODO |

### Auto-Suggestion & History

| # | Python File | C# File | Description | Status |
|---|-------------|---------|-------------|--------|
| 28 | `auto-suggestion.py` | `AutoSuggestion.cs` | Fish-style history suggestions | Done |
| 29 | `multiline-autosuggest.py` | `MultilineAutosuggest.cs` | Multi-line LLM-style suggestions with custom processor | TODO |
| 30 | `up-arrow-partial-string-matching.py` | `UpArrowPartialMatch.cs` | History prefix search with up arrow | TODO |

### Auto-Completion (Subdirectory)

| # | Python File | C# File | Description | Status |
|---|-------------|---------|-------------|--------|
| 31 | `auto-completion/autocompletion.py` | `AutoCompletion/BasicCompletion.cs` | Basic WordCompleter with Tab | Done |
| 32 | `auto-completion/autocomplete-with-control-space.py` | `AutoCompletion/ControlSpaceTrigger.cs` | Ctrl-Space triggers/cycles completions | TODO |
| 33 | `auto-completion/autocompletion-like-readline.py` | `AutoCompletion/ReadlineStyle.cs` | Readline-style completion display | TODO |
| 34 | `auto-completion/colored-completions.py` | `AutoCompletion/ColoredCompletions.cs` | Per-completion color styling | TODO |
| 35 | `auto-completion/colored-completions-with-formatted-text.py` | `AutoCompletion/FormattedCompletions.cs` | HTML display and display_meta | TODO |
| 36 | `auto-completion/combine-multiple-completers.py` | `AutoCompletion/MergedCompleters.cs` | merge_completers() combining sources | TODO |
| 37 | `auto-completion/fuzzy-word-completer.py` | `AutoCompletion/FuzzyWordCompleter.cs` | FuzzyWordCompleter with live typing | Done |
| 38 | `auto-completion/fuzzy-custom-completer.py` | `AutoCompletion/FuzzyCustomCompleter.cs` | Custom completer wrapped in FuzzyCompleter | TODO |
| 39 | `auto-completion/multi-column-autocompletion.py` | `AutoCompletion/MultiColumn.cs` | Multi-column completion grid | TODO |
| 40 | `auto-completion/multi-column-autocompletion-with-meta.py` | `AutoCompletion/MultiColumnWithMeta.cs` | Multi-column with meta descriptions | TODO |
| 41 | `auto-completion/nested-autocompletion.py` | `AutoCompletion/NestedCompletion.cs` | Hierarchical NestedCompleter | TODO |
| 42 | `auto-completion/slow-completions.py` | `AutoCompletion/SlowCompletions.cs` | Threaded slow completer with loading indicator | TODO |

### History (Subdirectory)

| # | Python File | C# File | Description | Status |
|---|-------------|---------|-------------|--------|
| 43 | `history/persistent-history.py` | `History/PersistentHistory.cs` | File-backed history across sessions | TODO |
| 44 | `history/slow-history.py` | `History/SlowHistory.cs` | ThreadedHistory with background loading | TODO |

### Validation & Lexing

| # | Python File | C# File | Description | Status |
|---|-------------|---------|-------------|--------|
| 45 | `input-validation.py` | `InputValidation.cs` | Email validation (must contain @) | TODO |
| 46 | `regular-language.py` | `RegularLanguage.cs` | Grammar-based calculator REPL | TODO |
| 47 | `html-input.py` | `HtmlInput.cs` | HTML syntax highlighting via PygmentsLexer | TODO |
| 48 | `custom-lexer.py` | `CustomLexer.cs` | Rainbow lexer coloring each character | TODO |

### Advanced Features

| # | Python File | C# File | Description | Status |
|---|-------------|---------|-------------|--------|
| 49 | `asyncio-prompt.py` | `AsyncPrompt.cs` | Async prompt with background tasks | TODO |
| 50 | `patch-stdout.py` | `PatchStdout.cs` | Background thread printing above prompt | TODO |
| 51 | `inputhook.py` | `InputHook.cs` | External event loop integration | TODO |
| 52 | `finalterm-shell-integration.py` | `ShellIntegration.cs` | iTerm2 Final Term escape markers | TODO |
| 53 | `system-clipboard-integration.py` | `SystemClipboard.cs` | OS clipboard via Clipboard class | TODO |

### With Frames (Subdirectory)

| # | Python File | C# File | Description | Status |
|---|-------------|---------|-------------|--------|
| 54 | `with-frames/with-frame.py` | `WithFrames/BasicFrame.cs` | Simple frame border around prompt | TODO |
| 55 | `with-frames/gray-frame-on-accept.py` | `WithFrames/GrayFrameOnAccept.cs` | Frame color changes on accept | TODO |
| 56 | `with-frames/frame-and-autocompletion.py` | `WithFrames/FrameWithCompletion.cs` | Frame with completion menu | TODO |

## Representative Python Examples

### get-input-with-default.py

```python
from prompt_toolkit import prompt

answer = prompt("What is your name: ", default="anonymous")
print(f"You said: {answer}")
```

### bottom-toolbar.py (7 variants)

```python
import time
from prompt_toolkit import prompt
from prompt_toolkit.formatted_text import ANSI, HTML
from prompt_toolkit.styles import Style

# Example 1: fixed text
text = prompt("Say something: ", bottom_toolbar="This is a toolbar")

# Example 2: callable with refresh
def get_toolbar():
    return f"Bottom toolbar: time={time.time()!r}"
text = prompt("Say something: ", bottom_toolbar=get_toolbar, refresh_interval=0.5)

# Example 3: HTML
text = prompt("Say something: ",
    bottom_toolbar=HTML('(html) <b>This</b> <u>is</u> a <style bg="ansired">toolbar</style>'))

# Example 4: ANSI
text = prompt("Say something: ",
    bottom_toolbar=ANSI("(ansi): \x1b[1mThis\x1b[0m \x1b[4mis\x1b[0m a \x1b[91mtoolbar"))

# Example 5: custom style
style = Style.from_dict({
    "bottom-toolbar": "#aaaa00 bg:#ff0000",
    "bottom-toolbar.text": "#aaaa44 bg:#aa4444",
})
text = prompt("Say something: ", bottom_toolbar="This is a toolbar", style=style)

# Example 6: style tuples
def get_bottom_toolbar():
    return [("", " "), ("bg:#ff0000 fg:#000000", "This"), ("", " is a "),
            ("bg:#ff0000 fg:#000000", "toolbar"), ("", ". ")]
text = prompt("Say something: ", bottom_toolbar=get_bottom_toolbar)

# Example 7: multiline
text = prompt("Say something: ", bottom_toolbar="This is\na multiline toolbar")
```

### colored-prompt.py (3 methods)

```python
from prompt_toolkit import prompt
from prompt_toolkit.formatted_text import ANSI, HTML
from prompt_toolkit.styles import Style

style = Style.from_dict({
    "username": "#884444 italic", "at": "#00aa00",
    "host": "#000088 bg:#aaaaff", "path": "#884444 underline",
})

# Method 1: style tuples
prompt_fragments = [
    ("class:username", "john"), ("class:at", "@"),
    ("class:host", "localhost"), ("class:path", "/user/john"),
    ("bg:#00aa00 #ffffff", "#"), ("", " "),
]
answer = prompt(prompt_fragments, style=style)

# Method 2: HTML
answer = prompt(HTML("<username>john</username><at>@</at><host>localhost</host>#"), style=style)

# Method 3: ANSI
answer = prompt(ANSI("\x1b[31mjohn\x1b[0m@\x1b[44mlocalhost\x1b[0m# "))
```

### custom-key-binding.py (key pattern)

```python
from prompt_toolkit import prompt
from prompt_toolkit.application import run_in_terminal
from prompt_toolkit.key_binding import KeyBindings

bindings = KeyBindings()

@bindings.add("f4")
def _(event):
    event.app.current_buffer.insert_text("hello world")

@bindings.add("x", "y")
def _(event):
    event.app.current_buffer.insert_text("z")

@bindings.add("c-t")
def _(event):
    def print_hello():
        print("hello world")
    run_in_terminal(print_hello)

text = prompt("> ", key_bindings=bindings)
```

### regular-language.py (grammar REPL)

```python
from prompt_toolkit import prompt
from prompt_toolkit.completion import WordCompleter
from prompt_toolkit.contrib.regular_languages.compiler import compile
from prompt_toolkit.contrib.regular_languages.completion import GrammarCompleter
from prompt_toolkit.contrib.regular_languages.lexer import GrammarLexer
from prompt_toolkit.lexers import SimpleLexer

g = compile(r"""
    (\s*  (?P<operator1>[a-z]+)  \s+  (?P<var1>[0-9.]+)  \s+  (?P<var2>[0-9.]+)  \s*) |
    (\s*  (?P<operator2>[a-z]+)  \s+  (?P<var1>[0-9.]+)  \s*)
""")

lexer = GrammarLexer(g, lexers={
    "operator1": SimpleLexer("class:operator"),
    "var1": SimpleLexer("class:number"),
})
completer = GrammarCompleter(g, {
    "operator1": WordCompleter(["add", "sub", "div", "mul"]),
    "operator2": WordCompleter(["cos", "sin"]),
})

while True:
    text = prompt("Calculate: ", lexer=lexer, completer=completer)
    m = g.match(text)
    # ... evaluate expression
```

### fancy-zsh-prompt.py (dynamic layout)

```python
from prompt_toolkit import prompt
from prompt_toolkit.application import get_app
from prompt_toolkit.formatted_text import HTML, fragment_list_width, merge_formatted_text, to_formatted_text

def get_prompt() -> HTML:
    left_part = HTML("<left-part> <username>root</username> <path>~/themes</path></left-part>")
    right_part = HTML("<right-part> <branch> master! </branch> <time>%s</time></right-part>") % (
        datetime.datetime.now().isoformat(),)

    used_width = sum([fragment_list_width(to_formatted_text(left_part)),
                      fragment_list_width(to_formatted_text(right_part))])
    total_width = get_app().output.get_size().columns
    padding = HTML("<padding>%s</padding>") % (" " * (total_width - used_width),)

    return merge_formatted_text([left_part, padding, right_part, "\n", "# "])

answer = prompt(get_prompt, style=style, refresh_interval=1)
```

### multiline-autosuggest.py (custom processor)

```python
from prompt_toolkit import PromptSession
from prompt_toolkit.auto_suggest import AutoSuggest, Suggestion
from prompt_toolkit.layout.processors import ConditionalProcessor, Processor, Transformation

class FakeLLMAutoSuggest(AutoSuggest):
    def get_suggestion(self, buffer, document):
        # Returns multi-line suggestion matching universal declaration of human rights
        return Suggestion(remaining_text + "\n" + "\n".join(remaining_lines))

class AppendMultilineAutoSuggestionInAnyLine(Processor):
    def apply_transformation(self, ti):
        # Handles suggestion rendering across multiple lines
        delta = ti.lineno - ti.document.cursor_position_row
        if delta == 0:
            return Transformation(fragments=ti.fragments + [(self.style, suggestions[0])])
        elif delta < len(suggestions):
            return Transformation([(self.style, suggestions[delta])])
        else:
            return Transformation(ti.get_line(ti.lineno - len(suggestions) + 1))

session = PromptSession(
    auto_suggest=FakeLLMAutoSuggest(),
    multiline=True,
    input_processors=[ConditionalProcessor(
        processor=AppendMultilineAutoSuggestionInAnyLine(),
        filter=HasFocus(DEFAULT_BUFFER) & ~IsDone(),
    )],
)
```

### slow-completions.py (threaded)

```python
from prompt_toolkit.completion import Completer, Completion

class SlowCompleter(Completer):
    def __init__(self):
        self.loading = 0

    def get_completions(self, document, complete_event):
        self.loading += 1
        try:
            for word in WORDS:
                if word.startswith(document.get_word_before_cursor()):
                    time.sleep(0.2)  # Simulate slowness
                    yield Completion(word, -len(document.get_word_before_cursor()))
        finally:
            self.loading -= 1

slow_completer = SlowCompleter()
text = prompt("Give some animals: ",
    completer=slow_completer, complete_in_thread=True,
    complete_while_typing=True,
    bottom_toolbar=lambda: " Loading... " if slow_completer.loading > 0 else "",
    complete_style=CompleteStyle.MULTI_COLUMN)
```

## Public API (C# Examples)

### GetInputWithDefault.cs

```csharp
using Stroke.Shortcuts;

namespace Stroke.Examples.Prompts;

public static class GetInputWithDefault
{
    public static void Run()
    {
        var answer = Prompt.RunPrompt("What is your name: ",
            @default: Environment.UserName);
        Console.WriteLine($"You said: {answer}");
    }
}
```

### GetInputViMode.cs

```csharp
using Stroke.Shortcuts;

namespace Stroke.Examples.Prompts;

public static class GetInputViMode
{
    public static void Run()
    {
        var answer = Prompt.RunPrompt("Say something: ", viMode: true);
        Console.WriteLine($"You said: {answer}");
    }
}
```

### BottomToolbar.cs

```csharp
using Stroke.FormattedText;
using Stroke.Shortcuts;
using Stroke.Styles;

namespace Stroke.Examples.Prompts;

public static class BottomToolbar
{
    public static void Run()
    {
        // Example 1: fixed text
        var text = Prompt.RunPrompt("Say something: ",
            bottomToolbar: "This is a toolbar");
        Console.WriteLine($"You said: {text}");

        // Example 2: callable with refresh
        text = Prompt.RunPrompt("Say something: ",
            bottomToolbar: () => $"Bottom toolbar: time={DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
            refreshInterval: 0.5f);
        Console.WriteLine($"You said: {text}");

        // Example 3: HTML
        text = Prompt.RunPrompt("Say something: ",
            bottomToolbar: new Html("(html) <b>This</b> <u>is</u> a <style bg=\"ansired\">toolbar</style>"));
        Console.WriteLine($"You said: {text}");

        // Example 4: ANSI
        text = Prompt.RunPrompt("Say something: ",
            bottomToolbar: new Ansi("(ansi): \x1b[1mThis\x1b[0m \x1b[4mis\x1b[0m a \x1b[91mtoolbar"));
        Console.WriteLine($"You said: {text}");

        // Example 5: custom style
        var style = new Style([
            ("bottom-toolbar", "#aaaa00 bg:#ff0000"),
            ("bottom-toolbar.text", "#aaaa44 bg:#aa4444"),
        ]);
        text = Prompt.RunPrompt("Say something: ",
            bottomToolbar: "This is a toolbar", style: style);
        Console.WriteLine($"You said: {text}");

        // Example 6: style tuples
        text = Prompt.RunPrompt("Say something: ",
            bottomToolbar: () => new AnyFormattedText([
                ("", " "), ("bg:#ff0000 fg:#000000", "This"),
                ("", " is a "), ("bg:#ff0000 fg:#000000", "toolbar"), ("", ". "),
            ]));
        Console.WriteLine($"You said: {text}");

        // Example 7: multiline
        text = Prompt.RunPrompt("Say something: ",
            bottomToolbar: "This is\na multiline toolbar");
        Console.WriteLine($"You said: {text}");
    }
}
```

### ColoredPrompt.cs

```csharp
using Stroke.FormattedText;
using Stroke.Shortcuts;
using Stroke.Styles;

namespace Stroke.Examples.Prompts;

public static class ColoredPrompt
{
    public static void Run()
    {
        var style = new Style([
            ("", "#ff0066"),
            ("username", "#884444 italic"),
            ("at", "#00aa00"),
            ("host", "#000088 bg:#aaaaff"),
            ("path", "#884444 underline"),
            ("selected-text", "reverse underline"),
        ]);

        // Method 1: style tuples
        var promptFragments = new AnyFormattedText([
            ("class:username", "john"), ("class:at", "@"),
            ("class:host", "localhost"), ("class:colon", ":"),
            ("class:path", "/user/john"),
            ("bg:#00aa00 #ffffff", "#"), ("", " "),
        ]);
        var answer = Prompt.RunPrompt(promptFragments, style: style);
        Console.WriteLine($"You said: {answer}");

        // Method 2: HTML
        answer = Prompt.RunPrompt(
            new Html("<username>john</username><at>@</at><host>localhost</host>"
                   + "<colon>:</colon><path>/user/john</path>"
                   + "<style bg=\"#00aa00\" fg=\"#ffffff\">#</style> "),
            style: style);
        Console.WriteLine($"You said: {answer}");

        // Method 3: ANSI
        answer = Prompt.RunPrompt(
            new Ansi("\x1b[31mjohn\x1b[0m@\x1b[44mlocalhost\x1b[0m:\x1b[4m/user/john\x1b[0m# "));
        Console.WriteLine($"You said: {answer}");
    }
}
```

### CustomKeyBinding.cs

```csharp
using Stroke.Application;
using Stroke.KeyBinding;
using Stroke.Shortcuts;

namespace Stroke.Examples.Prompts;

public static class CustomKeyBinding
{
    public static void Run()
    {
        var bindings = new KeyBindings();

        bindings.Add("f4", (KeyPressEventArgs e) =>
            e.App.CurrentBuffer.InsertText("hello world"));

        bindings.Add("x", "y", handler: (KeyPressEventArgs e) =>
            e.App.CurrentBuffer.InsertText("z"));

        bindings.Add("a", "b", "c", handler: (KeyPressEventArgs e) =>
            e.App.CurrentBuffer.InsertText("d"));

        bindings.Add("c-t", (KeyPressEventArgs e) =>
            RunInTerminal.Run(() => Console.WriteLine("hello world")));

        Console.WriteLine("Press F4 to insert \"hello world\", type \"xy\" to insert \"z\":");
        var text = Prompt.RunPrompt("> ", keyBindings: bindings);
        Console.WriteLine($"You said: {text}");
    }
}
```

### RegularLanguage.cs (grammar REPL)

```csharp
using Stroke.Completion;
using Stroke.Contrib.RegularLanguages;
using Stroke.Lexers;
using Stroke.Shortcuts;
using Stroke.Styles;

namespace Stroke.Examples.Prompts;

public static class RegularLanguage
{
    public static void Run()
    {
        var g = Grammar.Compile(@"
            (\s*  (?P<operator1>[a-z]+)  \s+  (?P<var1>[0-9.]+)  \s+  (?P<var2>[0-9.]+)  \s*) |
            (\s*  (?P<operator2>[a-z]+)  \s+  (?P<var1>[0-9.]+)  \s*)
        ");

        var lexer = new GrammarLexer(g, lexers: new Dictionary<string, ILexer>
        {
            ["operator1"] = new SimpleLexer("class:operator"),
            ["operator2"] = new SimpleLexer("class:operator"),
            ["var1"] = new SimpleLexer("class:number"),
            ["var2"] = new SimpleLexer("class:number"),
        });

        var completer = new GrammarCompleter(g, new Dictionary<string, ICompleter>
        {
            ["operator1"] = new WordCompleter(["add", "sub", "div", "mul"]),
            ["operator2"] = new WordCompleter(["cos", "sin"]),
        });

        var style = new Style([
            ("operator", "#33aa33 bold"),
            ("number", "#ff0000 bold"),
            ("trailing-input", "bg:#662222 #ffffff"),
        ]);

        while (true)
        {
            try
            {
                var text = Prompt.RunPrompt("Calculate: ",
                    lexer: lexer, completer: completer, style: style);
                var m = g.Match(text);
                if (m is null) { Console.WriteLine("Invalid command\n"); continue; }
                var vars = m.Variables();
                var var1 = double.Parse(vars.GetValueOrDefault("var1", "0"));
                var var2 = double.Parse(vars.GetValueOrDefault("var2", "0"));
                var op = vars.GetValueOrDefault("operator1") ?? vars.GetValueOrDefault("operator2");
                var result = op switch
                {
                    "add" => var1 + var2, "sub" => var1 - var2,
                    "mul" => var1 * var2, "div" => var1 / var2,
                    "sin" => Math.Sin(var1), "cos" => Math.Cos(var1),
                    _ => double.NaN,
                };
                Console.WriteLine($"Result: {result}\n");
            }
            catch (EOFException) { break; }
        }
    }
}
```

### SlowCompletions.cs (threaded)

```csharp
using Stroke.Completion;
using Stroke.Shortcuts;

namespace Stroke.Examples.Prompts.AutoCompletion;

public static class SlowCompletions
{
    private class SlowCompleter : ICompleter
    {
        private static readonly string[] Words = [
            "alligator", "ant", "ape", "bat", "bear", "beaver", "bee", "bison",
            "butterfly", "cat", "chicken", "crocodile", "dinosaur", "dog",
            "dolphin", "dove", "duck", "eagle", "elephant", "fish",
        ];

        public volatile int Loading;

        public IEnumerable<Completion> GetCompletions(Document document, CompleteEvent completeEvent)
        {
            Interlocked.Increment(ref Loading);
            try
            {
                var word = document.GetWordBeforeCursor();
                foreach (var w in Words)
                {
                    if (w.StartsWith(word, StringComparison.Ordinal))
                    {
                        Thread.Sleep(200); // Simulate slowness
                        yield return new Completion(w, -word.Length);
                    }
                }
            }
            finally { Interlocked.Decrement(ref Loading); }
        }
    }

    public static void Run()
    {
        var slow = new SlowCompleter();
        var text = Prompt.RunPrompt("Give some animals: ",
            completer: slow,
            completeInThread: true,
            completeWhileTyping: true,
            bottomToolbar: () => slow.Loading > 0 ? " Loading completions... " : "",
            completeStyle: CompleteStyle.MultiColumn);
        Console.WriteLine($"You said: {text}");
    }
}
```

### FancyZshPrompt.cs (dynamic layout)

```csharp
using Stroke.Application;
using Stroke.FormattedText;
using Stroke.Shortcuts;
using Stroke.Styles;

namespace Stroke.Examples.Prompts;

public static class FancyZshPrompt
{
    public static void Run()
    {
        var style = new Style([
            ("username", "#aaaaaa italic"),
            ("path", "#ffffff bold"),
            ("branch", "bg:#666666"),
            ("branch exclamation-mark", "#ff0000"),
            ("left-part", "bg:#444444"),
            ("right-part", "bg:#444444"),
            ("padding", "bg:#444444"),
        ]);

        AnyFormattedText GetPrompt()
        {
            var leftPart = new Html(
                "<left-part> <username>root</username> <path>~/.oh-my-zsh/themes</path></left-part>");
            var rightPart = new Html(
                "<right-part> <branch> master<exclamation-mark>!</exclamation-mark> </branch>"
              + " <time>%s</time></right-part>") % DateTime.Now.ToString("o");

            var usedWidth = FormattedTextUtils.FragmentListWidth(leftPart.ToFormattedText())
                          + FormattedTextUtils.FragmentListWidth(rightPart.ToFormattedText());
            var totalWidth = AppContext.GetApp<object?>().Output.GetSize().Columns;
            var paddingSize = Math.Max(0, totalWidth - usedWidth);
            var padding = new Html("<padding>%s</padding>") % new string(' ', paddingSize);

            return FormattedTextUtils.MergeFormattedText([leftPart, padding, rightPart, "\n", "# "]);
        }

        while (true)
        {
            var answer = Prompt.RunPrompt((Func<AnyFormattedText>)GetPrompt,
                style: style, refreshInterval: 1.0f);
            Console.WriteLine($"You said: {answer}");
        }
    }
}
```

### InputValidation.cs

```csharp
using Stroke.Shortcuts;
using Stroke.Validation;

namespace Stroke.Examples.Prompts;

public static class InputValidation
{
    public static void Run()
    {
        var validator = Validator.FromCallable(
            text => text.Contains('@'),
            errorMessage: "Not a valid e-mail address (does not contain '@').",
            moveToEnd: true);

        var text = Prompt.RunPrompt("Enter e-mail: ",
            validator: validator, validateWhileTyping: true);
        Console.WriteLine($"You said: {text}");
    }
}
```

### WithFrames/BasicFrame.cs

```csharp
using Stroke.Shortcuts;
using Stroke.Styles;

namespace Stroke.Examples.Prompts.WithFrames;

public static class BasicFrame
{
    public static void Run()
    {
        var style = new Style([("frame.border", "#888888")]);
        var text = Prompt.RunPrompt("Enter: ", showFrame: true, style: style);
        Console.WriteLine($"You said: {text}");
    }
}
```

## Project Structure

```
examples/Stroke.Examples.Prompts/
├── Stroke.Examples.Prompts.csproj
├── Program.cs                          # Entry point with dictionary-based routing
├── GetInput.cs                         # ✓ Done
├── GetInputWithDefault.cs
├── GetInputViMode.cs
├── GetPassword.cs
├── GetMultilineInput.cs
├── AcceptDefault.cs
├── ConfirmationPrompt.cs
├── PlaceholderText.cs
├── MouseSupport.cs
├── NoWrapping.cs
├── MultilinePrompt.cs
├── OperateAndGetNext.cs
├── EnforceTtyInputOutput.cs
├── GetPasswordWithToggle.cs
├── ColoredPrompt.cs
├── BottomToolbar.cs
├── RightPrompt.cs
├── ClockInput.cs
├── FancyZshPrompt.cs
├── TerminalTitle.cs
├── SwapLightDarkColors.cs
├── CursorShapes.cs
├── CustomKeyBinding.cs
├── CustomViOperator.cs
├── SystemPrompt.cs
├── SwitchViEmacs.cs
├── Autocorrection.cs
├── AutoSuggestion.cs                   # ✓ Done
├── MultilineAutosuggest.cs
├── UpArrowPartialMatch.cs
├── AsyncPrompt.cs
├── PatchStdout.cs
├── InputHook.cs
├── ShellIntegration.cs
├── SystemClipboard.cs
├── RegularLanguage.cs
├── HtmlInput.cs
├── CustomLexer.cs
├── InputValidation.cs
├── AutoCompletion/                     # Subdirectory
│   ├── BasicCompletion.cs              # ✓ Done (currently Autocompletion.cs)
│   ├── ControlSpaceTrigger.cs
│   ├── ReadlineStyle.cs
│   ├── ColoredCompletions.cs
│   ├── FormattedCompletions.cs
│   ├── MergedCompleters.cs
│   ├── FuzzyWordCompleter.cs           # ✓ Done
│   ├── FuzzyCustomCompleter.cs
│   ├── MultiColumn.cs
│   ├── MultiColumnWithMeta.cs
│   ├── NestedCompletion.cs
│   └── SlowCompletions.cs
├── History/                            # Subdirectory
│   ├── PersistentHistory.cs
│   └── SlowHistory.cs
└── WithFrames/                         # Subdirectory
    ├── BasicFrame.cs
    ├── GrayFrameOnAccept.cs
    └── FrameWithCompletion.cs
```

## Program.cs

```csharp
namespace Stroke.Examples.Prompts;

public static class Program
{
    private static readonly Dictionary<string, Action> Examples = new(StringComparer.OrdinalIgnoreCase)
    {
        // Core/Basic Prompts (1-13)
        ["GetInput"] = GetInput.Run,
        ["GetInputWithDefault"] = GetInputWithDefault.Run,
        ["GetInputViMode"] = GetInputViMode.Run,
        ["GetPassword"] = GetPassword.Run,
        ["GetMultilineInput"] = GetMultilineInput.Run,
        ["AcceptDefault"] = AcceptDefault.Run,
        ["ConfirmationPrompt"] = ConfirmationPrompt.Run,
        ["PlaceholderText"] = PlaceholderText.Run,
        ["MouseSupport"] = MouseSupport.Run,
        ["NoWrapping"] = NoWrapping.Run,
        ["MultilinePrompt"] = MultilinePrompt.Run,
        ["OperateAndGetNext"] = OperateAndGetNext.Run,
        ["EnforceTtyInputOutput"] = EnforceTtyInputOutput.Run,

        // Password & Security (14)
        ["GetPasswordWithToggle"] = GetPasswordWithToggle.Run,

        // Styling & Formatting (15-22)
        ["ColoredPrompt"] = ColoredPrompt.Run,
        ["BottomToolbar"] = BottomToolbar.Run,
        ["RightPrompt"] = RightPrompt.Run,
        ["ClockInput"] = ClockInput.Run,
        ["FancyZshPrompt"] = FancyZshPrompt.Run,
        ["TerminalTitle"] = TerminalTitle.Run,
        ["SwapLightDarkColors"] = SwapLightDarkColors.Run,
        ["CursorShapes"] = CursorShapes.Run,

        // Key Bindings & Input Handling (23-27)
        ["CustomKeyBinding"] = CustomKeyBinding.Run,
        ["CustomViOperator"] = CustomViOperator.Run,
        ["SystemPrompt"] = SystemPrompt.Run,
        ["SwitchViEmacs"] = SwitchViEmacs.Run,
        ["Autocorrection"] = Autocorrection.Run,

        // Auto-Suggestion & History (28-30)
        ["AutoSuggestion"] = AutoSuggestion.Run,
        ["MultilineAutosuggest"] = MultilineAutosuggest.Run,
        ["UpArrowPartialMatch"] = UpArrowPartialMatch.Run,

        // Auto-Completion (31-42)
        ["AutoCompletion/BasicCompletion"] = AutoCompletion.BasicCompletion.Run,
        ["AutoCompletion/ControlSpaceTrigger"] = AutoCompletion.ControlSpaceTrigger.Run,
        ["AutoCompletion/ReadlineStyle"] = AutoCompletion.ReadlineStyle.Run,
        ["AutoCompletion/ColoredCompletions"] = AutoCompletion.ColoredCompletions.Run,
        ["AutoCompletion/FormattedCompletions"] = AutoCompletion.FormattedCompletions.Run,
        ["AutoCompletion/MergedCompleters"] = AutoCompletion.MergedCompleters.Run,
        ["AutoCompletion/FuzzyWordCompleter"] = AutoCompletion.FuzzyWordCompleterExample.Run,
        ["AutoCompletion/FuzzyCustomCompleter"] = AutoCompletion.FuzzyCustomCompleter.Run,
        ["AutoCompletion/MultiColumn"] = AutoCompletion.MultiColumn.Run,
        ["AutoCompletion/MultiColumnWithMeta"] = AutoCompletion.MultiColumnWithMeta.Run,
        ["AutoCompletion/NestedCompletion"] = AutoCompletion.NestedCompletion.Run,
        ["AutoCompletion/SlowCompletions"] = AutoCompletion.SlowCompletions.Run,

        // History (43-44)
        ["History/PersistentHistory"] = History.PersistentHistory.Run,
        ["History/SlowHistory"] = History.SlowHistory.Run,

        // Validation & Lexing (45-48)
        ["InputValidation"] = InputValidation.Run,
        ["RegularLanguage"] = RegularLanguage.Run,
        ["HtmlInput"] = HtmlInput.Run,
        ["CustomLexer"] = CustomLexer.Run,

        // Advanced Features (49-53)
        ["AsyncPrompt"] = AsyncPrompt.Run,
        ["PatchStdout"] = PatchStdout.Run,
        ["InputHook"] = InputHook.Run,
        ["ShellIntegration"] = ShellIntegration.Run,
        ["SystemClipboard"] = SystemClipboard.Run,

        // With Frames (54-56)
        ["WithFrames/BasicFrame"] = WithFrames.BasicFrame.Run,
        ["WithFrames/GrayFrameOnAccept"] = WithFrames.GrayFrameOnAccept.Run,
        ["WithFrames/FrameWithCompletion"] = WithFrames.FrameWithCompletion.Run,
    };

    public static void Main(string[] args)
    {
        var exampleName = args.Length > 0 ? args[0] : "";
        if (string.IsNullOrEmpty(exampleName))
        {
            Console.WriteLine("Stroke Prompt Examples");
            Console.WriteLine();
            Console.WriteLine("Usage: dotnet run --project examples/Stroke.Examples.Prompts -- <example-name>");
            Console.WriteLine();
            Console.WriteLine("Available examples:");
            foreach (var name in Examples.Keys.Order())
                Console.WriteLine($"  {name}");
            return;
        }

        if (Examples.TryGetValue(exampleName, out var runExample))
        {
            try { runExample(); }
            catch (KeyboardInterruptException) { }
            catch (EOFException) { }
        }
        else
        {
            Console.WriteLine($"Unknown example: {exampleName}");
            Console.WriteLine($"Available: {string.Join(", ", Examples.Keys)}");
            Environment.Exit(1);
        }
    }
}
```

## Key Concepts Demonstrated

| Category | Examples | Stroke API |
|----------|----------|------------|
| Basic Prompt | GetInput, GetInputWithDefault, GetInputViMode | `Prompt.RunPrompt()`, `default`, `viMode` |
| Password | GetPassword, GetPasswordWithToggle | `isPassword`, `Condition`, toggle binding |
| Multi-line | GetMultilineInput, MultilinePrompt, NoWrapping, MouseSupport | `multiline`, `promptContinuation`, `wrapLines`, `mouseSupport` |
| Confirmation | ConfirmationPrompt, AcceptDefault | `Prompt.Confirm()`, `acceptDefault` |
| Styling | ColoredPrompt, BottomToolbar, RightPrompt | `Style`, `Html`, `Ansi`, `bottomToolbar`, `rprompt` |
| Dynamic | ClockInput, FancyZshPrompt | Callable prompt, `refreshInterval`, `FormattedTextUtils` |
| Terminal | TerminalTitle, ShellIntegration, CursorShapes | `TerminalUtils.SetTitle()`, `CursorShape`, zero-width escapes |
| Key Bindings | CustomKeyBinding, Autocorrection, CustomViOperator | `KeyBindings`, `RunInTerminal`, operator/text-object decorators |
| Editing Modes | SwitchViEmacs, SystemPrompt | `EditingMode`, `enableSystemPrompt`, `enableSuspend` |
| Completion | BasicCompletion through SlowCompletions (12 examples) | `WordCompleter`, `FuzzyCompleter`, `NestedCompleter`, `CompleteStyle`, `completeInThread` |
| History | PersistentHistory, SlowHistory, UpArrowPartialMatch | `FileHistory`, `ThreadedHistory`, `enableHistorySearch` |
| Suggestion | AutoSuggestion, MultilineAutosuggest | `AutoSuggestFromHistory`, custom `AutoSuggest`, `Processor` |
| Validation | InputValidation | `Validator.FromCallable()`, `validateWhileTyping` |
| Lexing | RegularLanguage, HtmlInput, CustomLexer | `GrammarLexer`, `PygmentsLexer`, custom `Lexer` |
| Color | SwapLightDarkColors | `swapLightAndDarkColors`, `Condition` |
| Advanced | AsyncPrompt, PatchStdout | `PromptAsync()`, `StdoutPatching.PatchStdout()` |
| Frames | BasicFrame, GrayFrameOnAccept, FrameWithCompletion | `showFrame`, frame styling, `~IsDone` filter |
| Integration | EnforceTtyInputOutput, SystemClipboard, InputHook | `CreateAppSessionFromTty()`, `Clipboard`, input hook |

## Dependencies

All dependencies already implemented:
- Feature 47: PromptSession (44-parameter constructor, Prompt static class)
- Feature 22: KeyBindings (registry, proxy types)
- Feature 12: Completion System (WordCompleter, FuzzyCompleter, NestedCompleter)
- Feature 18: Styles System (Style, Attrs, named colors)
- Feature 15: Formatted Text (Html, Ansi, Template, FormattedTextUtils)
- Feature 8: History (FileHistory, InMemoryHistory, ThreadedHistory)
- Feature 9: Validation (Validator.FromCallable)
- Feature 5: Auto-Suggest (AutoSuggestFromHistory)
- Feature 25: Lexer System (PygmentsLexer, SimpleLexer)
- Feature 27: Regular Languages (Grammar.Compile, GrammarCompleter, GrammarLexer)
- Feature 30: Application System (Application, RunInTerminal)
- Feature 49: Patch Stdout (StdoutPatching, StdoutProxy)
- Feature 48: Cursor Shapes (CursorShape, ModalCursorShapeConfig)

## Acceptance Criteria

### General
- [ ] All 56 examples build and run without errors
- [ ] Ctrl-C/Ctrl-D exit gracefully in all examples
- [ ] Project included in `Stroke.Examples.sln`
- [ ] Program.cs routing dictionary includes all 56 entries
- [ ] Existing 4 examples remain functional (GetInput, AutoSuggestion, BasicCompletion, FuzzyWordCompleter)

### Core/Basic Prompts
- [ ] GetInput: Simple prompt, prints response
- [ ] GetInputWithDefault: Shows editable default text
- [ ] GetInputViMode: Vi navigation and editing works
- [ ] GetPassword: Input masked with asterisks
- [ ] GetMultilineInput: Multi-line with line-number continuation
- [ ] AcceptDefault: Auto-accepts without user editing
- [ ] ConfirmationPrompt: Returns true/false for y/n
- [ ] PlaceholderText: Gray text disappears on typing
- [ ] MouseSupport: Click positions cursor in multiline
- [ ] NoWrapping: Long lines scroll horizontally
- [ ] MultilinePrompt: Basic multi-line entry
- [ ] OperateAndGetNext: Session reuse across iterations
- [ ] EnforceTtyInputOutput: Works when piped

### Password & Security
- [ ] GetPasswordWithToggle: Ctrl-T shows/hides password

### Styling & Formatting
- [ ] ColoredPrompt: All 3 methods (tuples, HTML, ANSI) produce colored prompts
- [ ] BottomToolbar: All 7 variants display correctly
- [ ] RightPrompt: Right-aligned text, auto-hides when input is long
- [ ] ClockInput: Time updates every 0.5 seconds
- [ ] FancyZshPrompt: Two-part prompt with dynamic padding fills terminal width
- [ ] TerminalTitle: Terminal window title changes
- [ ] SwapLightDarkColors: Ctrl-T toggles color scheme
- [ ] CursorShapes: Block, underline, beam cursors display correctly

### Key Bindings & Input
- [ ] CustomKeyBinding: F4 inserts text, xy→z sequence works, Ctrl-T prints
- [ ] CustomViOperator: 'R' reverses text, 'A' selects all (in Vi mode)
- [ ] SystemPrompt: Meta-! opens system command line
- [ ] SwitchViEmacs: F4 toggles modes, toolbar shows current mode
- [ ] Autocorrection: Typos corrected on space press

### Completion
- [ ] BasicCompletion: Tab completes animal names
- [ ] ControlSpaceTrigger: Ctrl-Space triggers/cycles
- [ ] ReadlineStyle: Completions displayed below prompt
- [ ] ColoredCompletions: Colors shown per completion
- [ ] FormattedCompletions: HTML-formatted display and meta
- [ ] MergedCompleters: Multiple sources merged
- [ ] FuzzyWordCompleter: Fuzzy matching while typing
- [ ] FuzzyCustomCompleter: Custom completer with fuzzy wrapper
- [ ] MultiColumn: Grid layout display
- [ ] MultiColumnWithMeta: Grid with metadata descriptions
- [ ] NestedCompletion: Hierarchical "show version", "show ip interface brief"
- [ ] SlowCompletions: Loading indicator in toolbar, background thread

### History
- [ ] PersistentHistory: History survives across runs (uses temp file)
- [ ] SlowHistory: Background loading with ThreadedHistory

### Validation & Lexing
- [ ] InputValidation: @ validation shown on Enter and while typing
- [ ] RegularLanguage: Calculator REPL evaluates add/sub/mul/div/sin/cos
- [ ] HtmlInput: HTML tags syntax-highlighted
- [ ] CustomLexer: Each character colored differently

### Advanced Features
- [ ] AsyncPrompt: Background tasks print above prompt
- [ ] PatchStdout: Thread output interleaves without corruption
- [ ] InputHook: External event source integration
- [ ] ShellIntegration: Final Term escape markers emitted
- [ ] SystemClipboard: Clipboard paste/copy works

### With Frames
- [ ] BasicFrame: Border drawn around prompt
- [ ] GrayFrameOnAccept: Frame color changes on accept
- [ ] FrameWithCompletion: Frame + completion menu coexist

## Verification with TUI Driver

```javascript
// GetInput (basic)
const session = await tui_launch({
  command: "dotnet",
  args: ["run", "--project", "examples/Stroke.Examples.Prompts", "--", "GetInput"],
  cols: 80, rows: 24
});
await tui_wait_for_text({ session_id: session.id, text: "Give me some input:" });
await tui_send_text({ session_id: session.id, text: "hello" });
await tui_press_key({ session_id: session.id, key: "Enter" });
await tui_wait_for_text({ session_id: session.id, text: "You said: hello" });
await tui_close({ session_id: session.id });

// BottomToolbar
const toolbar = await tui_launch({
  command: "dotnet",
  args: ["run", "--project", "examples/Stroke.Examples.Prompts", "--", "BottomToolbar"],
  cols: 80, rows: 24
});
await tui_wait_for_text({ session_id: toolbar.id, text: "This is a toolbar" });
await tui_send_text({ session_id: toolbar.id, text: "test" });
await tui_press_key({ session_id: toolbar.id, key: "Enter" });
await tui_close({ session_id: toolbar.id });

// ColoredPrompt
const colored = await tui_launch({
  command: "dotnet",
  args: ["run", "--project", "examples/Stroke.Examples.Prompts", "--", "ColoredPrompt"],
  cols: 80, rows: 24
});
await tui_wait_for_text({ session_id: colored.id, text: "john@localhost" });
await tui_screenshot({ session_id: colored.id }); // Verify colors
await tui_send_text({ session_id: colored.id, text: "test" });
await tui_press_key({ session_id: colored.id, key: "Enter" });
await tui_close({ session_id: colored.id });

// CustomKeyBinding
const keybind = await tui_launch({
  command: "dotnet",
  args: ["run", "--project", "examples/Stroke.Examples.Prompts", "--", "CustomKeyBinding"],
  cols: 80, rows: 24
});
await tui_wait_for_text({ session_id: keybind.id, text: ">" });
await tui_press_key({ session_id: keybind.id, key: "F4" });
await tui_wait_for_text({ session_id: keybind.id, text: "hello world" });
await tui_press_key({ session_id: keybind.id, key: "Enter" });
await tui_close({ session_id: keybind.id });

// InputValidation
const valid = await tui_launch({
  command: "dotnet",
  args: ["run", "--project", "examples/Stroke.Examples.Prompts", "--", "InputValidation"],
  cols: 80, rows: 24
});
await tui_wait_for_text({ session_id: valid.id, text: "Enter e-mail:" });
await tui_send_text({ session_id: valid.id, text: "notanemail" });
await tui_press_key({ session_id: valid.id, key: "Enter" });
await tui_wait_for_text({ session_id: valid.id, text: "does not contain" });
await tui_close({ session_id: valid.id });

// RegularLanguage (calculator)
const calc = await tui_launch({
  command: "dotnet",
  args: ["run", "--project", "examples/Stroke.Examples.Prompts", "--", "RegularLanguage"],
  cols: 80, rows: 24
});
await tui_wait_for_text({ session_id: calc.id, text: "Calculate:" });
await tui_send_text({ session_id: calc.id, text: "add 4 4" });
await tui_press_key({ session_id: calc.id, key: "Enter" });
await tui_wait_for_text({ session_id: calc.id, text: "Result: 8" });
await tui_press_key({ session_id: calc.id, key: "Ctrl+d" });
await tui_close({ session_id: calc.id });

// AutoCompletion/SlowCompletions
const slow = await tui_launch({
  command: "dotnet",
  args: ["run", "--project", "examples/Stroke.Examples.Prompts", "--", "AutoCompletion/SlowCompletions"],
  cols: 80, rows: 24
});
await tui_wait_for_text({ session_id: slow.id, text: "Give some animals:" });
await tui_send_text({ session_id: slow.id, text: "a" });
await tui_wait_for_text({ session_id: slow.id, text: "Loading", timeout_ms: 10000 });
await tui_press_key({ session_id: slow.id, key: "Ctrl+c" });
await tui_close({ session_id: slow.id });

// WithFrames/BasicFrame
const frame = await tui_launch({
  command: "dotnet",
  args: ["run", "--project", "examples/Stroke.Examples.Prompts", "--", "WithFrames/BasicFrame"],
  cols: 80, rows: 24
});
await tui_wait_for_text({ session_id: frame.id, text: "Enter:" });
const snap = await tui_snapshot({ session_id: frame.id });
// Verify frame border characters in snapshot
await tui_send_text({ session_id: frame.id, text: "test" });
await tui_press_key({ session_id: frame.id, key: "Enter" });
await tui_close({ session_id: frame.id });

// ConfirmationPrompt
const confirm = await tui_launch({
  command: "dotnet",
  args: ["run", "--project", "examples/Stroke.Examples.Prompts", "--", "ConfirmationPrompt"],
  cols: 80, rows: 24
});
await tui_wait_for_text({ session_id: confirm.id, text: "y/N" });
await tui_send_text({ session_id: confirm.id, text: "y" });
await tui_press_key({ session_id: confirm.id, key: "Enter" });
await tui_close({ session_id: confirm.id });
```
