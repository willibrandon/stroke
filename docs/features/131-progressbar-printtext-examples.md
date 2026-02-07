# Feature 131: Progress Bar and Print Text Examples (Complete Set)

## Overview

Implement ALL 24 remaining non-tutorial Python Prompt Toolkit examples across two new projects: `Stroke.Examples.ProgressBar` (15 examples) and `Stroke.Examples.PrintText` (9 examples). These examples demonstrate the ProgressBar shortcut API with formatters, threading, nested bars, styling, and key bindings, as well as the `FormattedTextOutput.Print()` and `PrintContainer()` APIs for non-interactive formatted text output including ANSI colors, HTML markup, named colors, and true color gradients.

**Note**: `gevent-get-input.py` from the Python Prompt Toolkit examples directory is excluded — it is a gevent compatibility test, not a real example. Stroke has a different testing approach per Constitution VIII.

## Python Prompt Toolkit Reference

### Progress Bar Examples

**Source:** `/Users/brandon/src/python-prompt-toolkit/examples/progress-bar/`

| # | Python File | C# File | Description | Status |
|---|-------------|---------|-------------|--------|
| 1 | `simple-progress-bar.py` | `SimpleProgressBar.cs` | Basic progress bar iterating over range | TODO |
| 2 | `two-tasks.py` | `TwoTasks.cs` | Two parallel progress bars with threads | TODO |
| 3 | `unknown-length.py` | `UnknownLength.cs` | Generator with no known length (no ETA) | TODO |
| 4 | `nested-progress-bars.py` | `NestedProgressBars.cs` | Outer/inner nested bars with remove_when_done | TODO |
| 5 | `colored-title-and-label.py` | `ColoredTitleLabel.cs` | HTML-colored title and label | TODO |
| 6 | `scrolling-task-name.py` | `ScrollingTaskName.cs` | Long label that scrolls horizontally | TODO |
| 7 | `styled-1.py` | `Styled1.cs` | Custom styling via Style dictionary (9 keys) | TODO |
| 8 | `styled-2.py` | `Styled2.cs` | Custom formatters (SpinningWheel, Bar, TimeLeft) | TODO |
| 9 | `styled-apt-get-install.py` | `StyledAptGet.cs` | Apt-get-style progress bar | TODO |
| 10 | `styled-rainbow.py` | `StyledRainbow.cs` | Rainbow formatter wrapper with color depth prompt | TODO |
| 11 | `styled-tqdm-1.py` | `StyledTqdm1.cs` | Tqdm-style with IterationsPerSecond | TODO |
| 12 | `styled-tqdm-2.py` | `StyledTqdm2.cs` | Tqdm-style with reverse-video bar | TODO |
| 13 | `custom-key-bindings.py` | `CustomKeyBindings.cs` | Key bindings (f/q/x) with patch_stdout | TODO |
| 14 | `many-parallel-tasks.py` | `ManyParallelTasks.cs` | 8 concurrent tasks with HTML title/toolbar | TODO |
| 15 | `a-lot-of-parallel-tasks.py` | `LotOfParallelTasks.cs` | 160 tasks with random durations and stop conditions | TODO |

### Print Text Examples

**Source:** `/Users/brandon/src/python-prompt-toolkit/examples/print-text/`

| # | Python File | C# File | Description | Status |
|---|-------------|---------|-------------|--------|
| 16 | `ansi-colors.py` | `AnsiColors.cs` | All 16 ANSI foreground and background colors | TODO |
| 17 | `ansi.py` | `Ansi.cs` | ANSI escape sequences (bold, italic, 256-color) | TODO |
| 18 | `html.py` | `Html.cs` | HTML formatting (`<b>`, `<i>`, `<ansired>`, interpolation) | TODO |
| 19 | `named-colors.py` | `NamedColors.cs` | All NAMED_COLORS at 3 color depths | TODO |
| 20 | `print-formatted-text.py` | `PrintFormattedText.cs` | 4 formatting methods (tuples, HTML, inline, ANSI) | TODO |
| 21 | `print-frame.py` | `PrintFrame.cs` | Non-interactive container printing via PrintContainer | TODO |
| 22 | `true-color-demo.py` | `TrueColorDemo.cs` | 7 RGB gradients at 3 color depths | TODO |
| 23 | `pygments-tokens.py` | `PygmentsTokens.cs` | Syntax highlighting via Pygments token types | TODO |
| 24 | `prompt-toolkit-logo-ansi-art.py` | `LogoAnsiArt.cs` | ANSI art logo with true color RGB | TODO |

## Representative Python Examples

### simple-progress-bar.py

```python
from prompt_toolkit.shortcuts import ProgressBar

with ProgressBar() as pb:
    for i in pb(range(800)):
        time.sleep(0.01)
```

### two-tasks.py (parallel threads)

```python
from prompt_toolkit.shortcuts import ProgressBar

with ProgressBar() as pb:
    def task_1():
        for i in pb(range(100)):
            time.sleep(0.05)

    def task_2():
        for i in pb(range(150)):
            time.sleep(0.08)

    t1 = threading.Thread(target=task_1, daemon=True)
    t2 = threading.Thread(target=task_2, daemon=True)
    t1.start()
    t2.start()

    for t in [t1, t2]:
        while t.is_alive():
            t.join(timeout=0.5)
```

### nested-progress-bars.py

```python
from prompt_toolkit import HTML
from prompt_toolkit.shortcuts import ProgressBar

with ProgressBar(
    title=HTML('<b fg="#aa00ff">Nested progress bars</b>'),
    bottom_toolbar=HTML(" <b>[Control-L]</b> clear  <b>[Control-C]</b> abort"),
) as pb:
    for i in pb(range(6), label="Main task"):
        for j in pb(range(200), label=f"Subtask <{i + 1}>", remove_when_done=True):
            time.sleep(0.01)
```

### styled-2.py (custom formatters)

```python
from prompt_toolkit.shortcuts import ProgressBar
from prompt_toolkit.shortcuts.progress_bar import formatters

custom_formatters = [
    formatters.Label(),
    formatters.Text(" "),
    formatters.SpinningWheel(),
    formatters.Text(" "),
    formatters.Text(HTML("<tildes>~~~</tildes>")),
    formatters.Bar(sym_a="#", sym_b="#", sym_c="."),
    formatters.Text(" left: "),
    formatters.TimeLeft(),
]
with ProgressBar(title="Custom formatter.", formatters=custom_formatters, style=style) as pb:
    for i in pb(range(20), label="Downloading..."):
        time.sleep(1)
```

### styled-rainbow.py (color depth selection)

```python
from prompt_toolkit.output import ColorDepth
from prompt_toolkit.shortcuts import ProgressBar
from prompt_toolkit.shortcuts.progress_bar import formatters
from prompt_toolkit.shortcuts.prompt import confirm

true_color = confirm("Yes true colors? (y/n) ")

custom_formatters = [
    formatters.Label(),
    formatters.Text(" "),
    formatters.Rainbow(formatters.Bar()),
    formatters.Text(" left: "),
    formatters.Rainbow(formatters.TimeLeft()),
]

color_depth = ColorDepth.DEPTH_24_BIT if true_color else ColorDepth.DEPTH_8_BIT
with ProgressBar(formatters=custom_formatters, color_depth=color_depth) as pb:
    for i in pb(range(20), label="Downloading..."):
        time.sleep(1)
```

### custom-key-bindings.py (key bindings + patch_stdout)

```python
from prompt_toolkit import HTML
from prompt_toolkit.key_binding import KeyBindings
from prompt_toolkit.patch_stdout import patch_stdout
from prompt_toolkit.shortcuts import ProgressBar

kb = KeyBindings()
cancel = [False]

@kb.add("f")
def _(event):
    print("You pressed `f`.")

@kb.add("q")
def _(event):
    cancel[0] = True

@kb.add("x")
def _(event):
    os.kill(os.getpid(), signal.SIGINT)

with patch_stdout():
    with ProgressBar(key_bindings=kb, bottom_toolbar=bottom_toolbar) as pb:
        for i in pb(range(800)):
            time.sleep(0.01)
            if cancel[0]:
                break
```

### print-formatted-text.py (4 methods)

```python
from prompt_toolkit import print_formatted_text
from prompt_toolkit.formatted_text import ANSI, HTML, FormattedText
from prompt_toolkit.styles import Style

print = print_formatted_text

style = Style.from_dict({"hello": "#ff0066", "world": "#44ff44 italic"})

# Method 1: FormattedText tuples
text_fragments = FormattedText([("class:hello", "Hello "), ("class:world", "World"), ("", "\n")])
print(text_fragments, style=style)

# Method 2: HTML with style classes
print(HTML("<hello>hello</hello> <world>world</world>\n"), style=style)

# Method 3: HTML with inline styles
print(HTML('<style fg="#ff0066">hello</style> <style fg="#44ff44"><i>world</i></style>\n'))

# Method 4: ANSI escape sequences
print(ANSI("\x1b[31mhello \x1b[32mworld\n"))
```

### print-frame.py (container printing)

```python
from prompt_toolkit.shortcuts import print_container
from prompt_toolkit.widgets import Frame, TextArea

print_container(Frame(TextArea(text="Hello world!\n"), title="Stage: parse"))
```

### true-color-demo.py (RGB gradients)

```python
from prompt_toolkit import print_formatted_text
from prompt_toolkit.formatted_text import HTML, FormattedText
from prompt_toolkit.output import ColorDepth

print = print_formatted_text

for template in [
    "bg:#{0:02x}0000",   # Red
    "bg:#00{0:02x}00",   # Green
    "bg:#0000{0:02x}",   # Blue
    "bg:#{0:02x}{0:02x}00",      # Yellow
    "bg:#{0:02x}00{0:02x}",      # Magenta
    "bg:#00{0:02x}{0:02x}",      # Cyan
    "bg:#{0:02x}{0:02x}{0:02x}", # Gray
]:
    fragments = [(template.format(i), " ") for i in range(0, 256, 4)]
    print(FormattedText(fragments), color_depth=ColorDepth.DEPTH_4_BIT)
    print(FormattedText(fragments), color_depth=ColorDepth.DEPTH_8_BIT)
    print(FormattedText(fragments), color_depth=ColorDepth.DEPTH_24_BIT)
    print()
```

## Public API (C# Examples)

### SimpleProgressBar.cs

```csharp
using Stroke.Shortcuts;

namespace Stroke.Examples.ProgressBar;

public static class SimpleProgressBar
{
    public static async Task Run()
    {
        await using var pb = new ProgressBar();
        await foreach (var i in pb.Iterate(Enumerable.Range(0, 800)))
        {
            await Task.Delay(10);
        }
    }
}
```

### TwoTasks.cs (parallel threads)

```csharp
using Stroke.Shortcuts;

namespace Stroke.Examples.ProgressBar;

public static class TwoTasks
{
    public static async Task Run()
    {
        await using var pb = new ProgressBar();

        var t1 = Task.Run(async () =>
        {
            await foreach (var i in pb.Iterate(Enumerable.Range(0, 100)))
                await Task.Delay(50);
        });

        var t2 = Task.Run(async () =>
        {
            await foreach (var i in pb.Iterate(Enumerable.Range(0, 150)))
                await Task.Delay(80);
        });

        await Task.WhenAll(t1, t2);
    }
}
```

### NestedProgressBars.cs

```csharp
using Stroke.FormattedText;
using Stroke.Shortcuts;

namespace Stroke.Examples.ProgressBar;

public static class NestedProgressBars
{
    public static async Task Run()
    {
        await using var pb = new ProgressBar(
            title: new Html("<b fg=\"#aa00ff\">Nested progress bars</b>"),
            bottomToolbar: new Html(" <b>[Control-L]</b> clear  <b>[Control-C]</b> abort"));

        await foreach (var i in pb.Iterate(Enumerable.Range(0, 6), label: "Main task"))
        {
            await foreach (var j in pb.Iterate(Enumerable.Range(0, 200),
                label: $"Subtask <{i + 1}>", removeWhenDone: true))
            {
                await Task.Delay(10);
            }
        }
    }
}
```

### Styled2.cs (custom formatters)

```csharp
using Stroke.FormattedText;
using Stroke.Shortcuts;
using Stroke.Shortcuts.ProgressBarFormatters;
using Stroke.Styles;

namespace Stroke.Examples.ProgressBar;

public static class Styled2
{
    public static async Task Run()
    {
        var style = new Style([
            ("progressbar title", "#0000ff"),
            ("item-title", "#ff4400 underline"),
            ("percentage", "#00ff00"),
            ("bar-a", "bg:#00ff00 #004400"),
            ("bar-b", "bg:#00ff00 #000000"),
            ("bar-c", "bg:#000000 #000000"),
            ("tildes", "#444488"),
            ("time-left", "bg:#88ff88 #ffffff"),
            ("spinning-wheel", "bg:#ffff00 #000000"),
        ]);

        Formatter[] customFormatters = [
            new Label(),
            new Text(" "),
            new SpinningWheel(),
            new Text(" "),
            new Text(new Html("<tildes>~~~</tildes>")),
            new Bar(symA: "#", symB: "#", symC: "."),
            new Text(" left: "),
            new TimeLeft(),
        ];

        await using var pb = new ProgressBar(
            title: "Progress bar example with custom formatter.",
            formatters: customFormatters,
            style: style);

        await foreach (var i in pb.Iterate(Enumerable.Range(0, 20), label: "Downloading..."))
        {
            await Task.Delay(1000);
        }
    }
}
```

### StyledRainbow.cs (color depth selection)

```csharp
using Stroke.Output;
using Stroke.Shortcuts;
using Stroke.Shortcuts.ProgressBarFormatters;

namespace Stroke.Examples.ProgressBar;

public static class StyledRainbow
{
    public static async Task Run()
    {
        var trueColor = Prompt.Confirm("Yes true colors? (y/n) ");

        Formatter[] customFormatters = [
            new Label(),
            new Text(" "),
            new Rainbow(new Bar()),
            new Text(" left: "),
            new Rainbow(new TimeLeft()),
        ];

        var colorDepth = trueColor ? ColorDepth.Depth24Bit : ColorDepth.Depth8Bit;

        await using var pb = new ProgressBar(
            formatters: customFormatters, colorDepth: colorDepth);

        await foreach (var i in pb.Iterate(Enumerable.Range(0, 20), label: "Downloading..."))
        {
            await Task.Delay(1000);
        }
    }
}
```

### CustomKeyBindings.cs (key bindings + PatchStdout)

```csharp
using Stroke.FormattedText;
using Stroke.KeyBinding;
using Stroke.Shortcuts;

namespace Stroke.Examples.ProgressBar;

public static class CustomKeyBindings
{
    public static async Task Run()
    {
        var bottomToolbar = new Html(
            " <b>[f]</b> Print \"f\" <b>[q]</b> Abort  <b>[x]</b> Send Control-C.");

        var kb = new KeyBindings();
        var cancel = false;

        kb.Add("f", (KeyPressEventArgs e) =>
            Console.WriteLine("You pressed `f`."));

        kb.Add("q", (KeyPressEventArgs e) =>
            cancel = true);

        kb.Add("x", (KeyPressEventArgs e) =>
            Environment.FailFast(null));

        using (StdoutPatching.PatchStdout())
        {
            await using var pb = new ProgressBar(
                keyBindings: kb, bottomToolbar: bottomToolbar);

            await foreach (var i in pb.Iterate(Enumerable.Range(0, 800)))
            {
                await Task.Delay(10);
                if (cancel) break;
            }
        }
    }
}
```

### PrintFormattedText.cs (4 methods)

```csharp
using Stroke.FormattedText;
using Stroke.Shortcuts;
using Stroke.Styles;

namespace Stroke.Examples.PrintText;

public static class PrintFormattedText
{
    public static void Run()
    {
        var style = new Style([
            ("hello", "#ff0066"),
            ("world", "#44ff44 italic"),
        ]);

        // Method 1: FormattedText tuples
        var textFragments = new FormattedText.FormattedText([
            ("class:hello", "Hello "),
            ("class:world", "World"),
            ("", "\n"),
        ]);
        FormattedTextOutput.Print(textFragments, style: style);

        // Method 2: HTML with style classes
        FormattedTextOutput.Print(
            new Html("<hello>hello</hello> <world>world</world>\n"), style: style);

        // Method 3: HTML with inline styles
        FormattedTextOutput.Print(
            new Html("<style fg=\"#ff0066\">hello</style> "
                   + "<style fg=\"#44ff44\"><i>world</i></style>\n"));

        // Method 4: ANSI escape sequences
        FormattedTextOutput.Print(new Ansi("\x1b[31mhello \x1b[32mworld\n"));
    }
}
```

### PrintFrame.cs (container printing)

```csharp
using Stroke.Shortcuts;
using Stroke.Widgets;

namespace Stroke.Examples.PrintText;

public static class PrintFrame
{
    public static void Run()
    {
        FormattedTextOutput.PrintContainer(
            new Frame(new TextArea(text: "Hello world!\n"), title: "Stage: parse"));
    }
}
```

### TrueColorDemo.cs (RGB gradients)

```csharp
using Stroke.FormattedText;
using Stroke.Output;
using Stroke.Shortcuts;

namespace Stroke.Examples.PrintText;

public static class TrueColorDemo
{
    public static void Run()
    {
        FormattedTextOutput.Print(new Html("\n<u>True color test.</u>"));

        string[] templates = [
            "bg:#{0:x2}0000",              // Red
            "bg:#00{0:x2}00",              // Green
            "bg:#0000{0:x2}",              // Blue
            "bg:#{0:x2}{0:x2}00",          // Yellow
            "bg:#{0:x2}00{0:x2}",          // Magenta
            "bg:#00{0:x2}{0:x2}",          // Cyan
            "bg:#{0:x2}{0:x2}{0:x2}",      // Gray
        ];

        foreach (var template in templates)
        {
            var fragments = new List<(string, string)>();
            for (var i = 0; i < 256; i += 4)
                fragments.Add((string.Format(template, i), " "));

            var text = new FormattedText.FormattedText(fragments);
            FormattedTextOutput.Print(text, colorDepth: ColorDepth.Depth4Bit);
            FormattedTextOutput.Print(text, colorDepth: ColorDepth.Depth8Bit);
            FormattedTextOutput.Print(text, colorDepth: ColorDepth.Depth24Bit);
            FormattedTextOutput.Print("");
        }
    }
}
```

## Project Structure

```
examples/Stroke.Examples.ProgressBar/
├── Stroke.Examples.ProgressBar.csproj
├── Program.cs                          # Entry point with dictionary-based routing
├── SimpleProgressBar.cs
├── TwoTasks.cs
├── UnknownLength.cs
├── NestedProgressBars.cs
├── ColoredTitleLabel.cs
├── ScrollingTaskName.cs
├── Styled1.cs
├── Styled2.cs
├── StyledAptGet.cs
├── StyledRainbow.cs
├── StyledTqdm1.cs
├── StyledTqdm2.cs
├── CustomKeyBindings.cs
├── ManyParallelTasks.cs
└── LotOfParallelTasks.cs

examples/Stroke.Examples.PrintText/
├── Stroke.Examples.PrintText.csproj
├── Program.cs                          # Entry point with dictionary-based routing
├── AnsiColors.cs
├── Ansi.cs
├── Html.cs
├── NamedColors.cs
├── PrintFormattedText.cs
├── PrintFrame.cs
├── TrueColorDemo.cs
├── PygmentsTokens.cs
└── LogoAnsiArt.cs
```

## Program.cs — ProgressBar

```csharp
namespace Stroke.Examples.ProgressBar;

public static class Program
{
    private static readonly Dictionary<string, Func<Task>> Examples = new(StringComparer.OrdinalIgnoreCase)
    {
        ["SimpleProgressBar"] = SimpleProgressBar.Run,
        ["TwoTasks"] = TwoTasks.Run,
        ["UnknownLength"] = UnknownLength.Run,
        ["NestedProgressBars"] = NestedProgressBars.Run,
        ["ColoredTitleLabel"] = ColoredTitleLabel.Run,
        ["ScrollingTaskName"] = ScrollingTaskName.Run,
        ["Styled1"] = Styled1.Run,
        ["Styled2"] = Styled2.Run,
        ["StyledAptGet"] = StyledAptGet.Run,
        ["StyledRainbow"] = StyledRainbow.Run,
        ["StyledTqdm1"] = StyledTqdm1.Run,
        ["StyledTqdm2"] = StyledTqdm2.Run,
        ["CustomKeyBindings"] = CustomKeyBindings.Run,
        ["ManyParallelTasks"] = ManyParallelTasks.Run,
        ["LotOfParallelTasks"] = LotOfParallelTasks.Run,
    };

    public static async Task Main(string[] args)
    {
        var exampleName = args.Length > 0 ? args[0] : "";
        if (string.IsNullOrEmpty(exampleName))
        {
            Console.WriteLine("Stroke Progress Bar Examples");
            Console.WriteLine();
            Console.WriteLine("Usage: dotnet run --project examples/Stroke.Examples.ProgressBar -- <example-name>");
            Console.WriteLine();
            Console.WriteLine("Available examples:");
            foreach (var name in Examples.Keys.Order())
                Console.WriteLine($"  {name}");
            return;
        }

        if (Examples.TryGetValue(exampleName, out var runExample))
        {
            try { await runExample(); }
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

## Program.cs — PrintText

```csharp
namespace Stroke.Examples.PrintText;

public static class Program
{
    private static readonly Dictionary<string, Action> Examples = new(StringComparer.OrdinalIgnoreCase)
    {
        ["AnsiColors"] = AnsiColors.Run,
        ["Ansi"] = Ansi.Run,
        ["Html"] = Html.Run,
        ["NamedColors"] = NamedColors.Run,
        ["PrintFormattedText"] = PrintFormattedText.Run,
        ["PrintFrame"] = PrintFrame.Run,
        ["TrueColorDemo"] = TrueColorDemo.Run,
        ["PygmentsTokens"] = PygmentsTokens.Run,
        ["LogoAnsiArt"] = LogoAnsiArt.Run,
    };

    public static void Main(string[] args)
    {
        var exampleName = args.Length > 0 ? args[0] : "";
        if (string.IsNullOrEmpty(exampleName))
        {
            Console.WriteLine("Stroke Print Text Examples");
            Console.WriteLine();
            Console.WriteLine("Usage: dotnet run --project examples/Stroke.Examples.PrintText -- <example-name>");
            Console.WriteLine();
            Console.WriteLine("Available examples:");
            foreach (var name in Examples.Keys.Order())
                Console.WriteLine($"  {name}");
            return;
        }

        if (Examples.TryGetValue(exampleName, out var runExample))
        {
            runExample();
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
| Basic Progress | SimpleProgressBar, UnknownLength, ScrollingTaskName | `ProgressBar`, `Iterate()`, generator iteration |
| Parallel Tasks | TwoTasks, ManyParallelTasks, LotOfParallelTasks | `Task.Run`, multiple `Iterate()` calls on same `ProgressBar` |
| Nested Bars | NestedProgressBars | `removeWhenDone`, nested `Iterate()` calls |
| Styling | Styled1, ColoredTitleLabel | `Style`, `title`, `label`, HTML formatting |
| Custom Formatters | Styled2, StyledAptGet, StyledTqdm1, StyledTqdm2 | `Formatter[]`, `Label`, `Bar`, `Percentage`, `TimeElapsed`, `TimeLeft`, `IterationsPerSecond`, `Progress`, `SpinningWheel` |
| Rainbow Colors | StyledRainbow | `Rainbow` formatter wrapper, `ColorDepth` |
| Key Bindings | CustomKeyBindings | `KeyBindings`, `StdoutPatching.PatchStdout()`, cancel flag |
| FormattedText Tuples | AnsiColors, PrintFormattedText | `FormattedText`, style tuples `("class:name", "text")` |
| HTML Markup | Html, PrintFormattedText | `Html` class, `<b>`, `<i>`, `<ansired>`, `<style>` |
| ANSI Sequences | Ansi, PrintFormattedText | `Ansi` class, raw `\x1b[` codes |
| Named Colors | NamedColors | `NAMED_COLORS`, `"fg:colorname"` syntax |
| True Color | TrueColorDemo | RGB hex colors, `ColorDepth.Depth24Bit/Depth8Bit/Depth4Bit` |
| Container Printing | PrintFrame | `FormattedTextOutput.PrintContainer()`, `Frame`, `TextArea` |
| Syntax Highlighting | PygmentsTokens | `PygmentsTokens`, token-based styling |
| ANSI Art | LogoAnsiArt | 24-bit RGB `\x1b[48;2;R;G;Bm` background colors |

## Dependencies

### Progress Bar Examples

The ProgressBar shortcut API (Feature 71) must be implemented first. It provides:
- `ProgressBar` class (context manager with background thread lifecycle)
- `ProgressBarCounter<T>` (individual counter supporting iteration)
- 10 `Formatter` implementations (Label, Text, Bar, Percentage, Progress, TimeElapsed, TimeLeft, IterationsPerSecond, SpinningWheel, Rainbow)
- Default formatter factory

Other dependencies (all already implemented):
- Feature 22: KeyBindings (for CustomKeyBindings example)
- Feature 15: FormattedText (Html, Ansi for titles and toolbars)
- Feature 18: Styles System (Style class for custom styling)
- Feature 49: PatchStdout (StdoutPatching for print-above-progress)
- Feature 52: ColorDepth (for StyledRainbow color depth selection)

### Print Text Examples

All dependencies already implemented:
- Feature 70: FormattedTextOutput (Print, PrintContainer)
- Feature 15: FormattedText (Html, Ansi, FormattedText, PygmentsTokens)
- Feature 18: Styles System (Style, named colors)
- Feature 52: ColorDepth enum (Depth4Bit, Depth8Bit, Depth24Bit)
- Feature 114: NamedColors (NAMED_COLORS dictionary)
- Feature 45: Widgets (Frame, TextArea for PrintFrame)

## Acceptance Criteria

### General
- [ ] All 24 examples build and run without errors
- [ ] Ctrl-C exits gracefully in all examples
- [ ] Both projects included in `Stroke.Examples.sln`
- [ ] Program.cs routing dictionary includes all entries
- [ ] PrintText examples are synchronous (no async needed)
- [ ] ProgressBar examples are async

### Progress Bar (15 examples)
- [ ] SimpleProgressBar: Bar fills from 0% to 100%, displays percentage and ETA
- [ ] TwoTasks: Two bars update independently in parallel
- [ ] UnknownLength: Progress bar shows elapsed time but no ETA
- [ ] NestedProgressBars: Inner bars appear and disappear (remove_when_done)
- [ ] ColoredTitleLabel: Title and label display with HTML colors
- [ ] ScrollingTaskName: Long label scrolls when terminal is narrow
- [ ] Styled1: All 9 style keys affect visual appearance
- [ ] Styled2: SpinningWheel animates, custom bar characters display
- [ ] StyledAptGet: Apt-get-style `[###...] XX%` format
- [ ] StyledRainbow: Rainbow gradient on bar and time display
- [ ] StyledTqdm1: Iterations per second displayed
- [ ] StyledTqdm2: Reverse-video bar style
- [ ] CustomKeyBindings: f prints above bar, q breaks loop, x sends SIGINT
- [ ] ManyParallelTasks: 8 bars update concurrently
- [ ] LotOfParallelTasks: 160 bars with random durations, some break early

### Print Text (9 examples)
- [ ] AnsiColors: 16 foreground and 16 background ANSI colors displayed
- [ ] Ansi: Bold, italic, underline, strike, 256-color via raw ANSI codes
- [ ] Html: `<b>`, `<i>`, `<ansired>`, `<style>`, interpolation with escaping
- [ ] NamedColors: All named colors at 4-bit, 8-bit, 24-bit depths
- [ ] PrintFormattedText: All 4 formatting methods produce colored output
- [ ] PrintFrame: Frame border drawn around "Hello world!" with title
- [ ] TrueColorDemo: 7 color gradients, visible degradation at lower depths
- [ ] PygmentsTokens: Syntax tokens with custom keyword/string styling
- [ ] LogoAnsiArt: Logo renders with true color background blocks

## Verification with TUI Driver

```javascript
// SimpleProgressBar
const simple = await tui_launch({
  command: "dotnet",
  args: ["run", "--project", "examples/Stroke.Examples.ProgressBar", "--", "SimpleProgressBar"],
  cols: 80, rows: 24
});
await tui_wait_for_text({ session_id: simple.id, text: "%", timeout_ms: 10000 });
await tui_screenshot({ session_id: simple.id });
// Wait for completion or interrupt
await tui_press_key({ session_id: simple.id, key: "Ctrl+c" });
await tui_close({ session_id: simple.id });

// TwoTasks (parallel)
const two = await tui_launch({
  command: "dotnet",
  args: ["run", "--project", "examples/Stroke.Examples.ProgressBar", "--", "TwoTasks"],
  cols: 80, rows: 24
});
await tui_wait_for_text({ session_id: two.id, text: "%", timeout_ms: 10000 });
const snap = await tui_text({ session_id: two.id });
// Verify two separate progress lines
await tui_press_key({ session_id: two.id, key: "Ctrl+c" });
await tui_close({ session_id: two.id });

// CustomKeyBindings
const keys = await tui_launch({
  command: "dotnet",
  args: ["run", "--project", "examples/Stroke.Examples.ProgressBar", "--", "CustomKeyBindings"],
  cols: 80, rows: 24
});
await tui_wait_for_text({ session_id: keys.id, text: "[f]", timeout_ms: 10000 });
await tui_press_key({ session_id: keys.id, key: "f" });
await tui_wait_for_text({ session_id: keys.id, text: "pressed" });
await tui_press_key({ session_id: keys.id, key: "q" });
await tui_close({ session_id: keys.id });

// PrintFormattedText
const pft = await tui_launch({
  command: "dotnet",
  args: ["run", "--project", "examples/Stroke.Examples.PrintText", "--", "PrintFormattedText"],
  cols: 80, rows: 24
});
await tui_wait_for_text({ session_id: pft.id, text: "hello" });
await tui_screenshot({ session_id: pft.id }); // Verify colors
await tui_close({ session_id: pft.id });

// PrintFrame
const frame = await tui_launch({
  command: "dotnet",
  args: ["run", "--project", "examples/Stroke.Examples.PrintText", "--", "PrintFrame"],
  cols: 80, rows: 24
});
await tui_wait_for_text({ session_id: frame.id, text: "Stage: parse" });
await tui_wait_for_text({ session_id: frame.id, text: "Hello world!" });
await tui_close({ session_id: frame.id });

// TrueColorDemo
const truecolor = await tui_launch({
  command: "dotnet",
  args: ["run", "--project", "examples/Stroke.Examples.PrintText", "--", "TrueColorDemo"],
  cols: 80, rows: 24
});
await tui_wait_for_text({ session_id: truecolor.id, text: "True color test" });
await tui_screenshot({ session_id: truecolor.id }); // Verify gradients
await tui_close({ session_id: truecolor.id });

// NamedColors
const named = await tui_launch({
  command: "dotnet",
  args: ["run", "--project", "examples/Stroke.Examples.PrintText", "--", "NamedColors"],
  cols: 120, rows: 40
});
await tui_wait_for_text({ session_id: named.id, text: "Named colors" });
await tui_screenshot({ session_id: named.id }); // Verify color rendering
await tui_close({ session_id: named.id });

// AnsiColors
const ansi = await tui_launch({
  command: "dotnet",
  args: ["run", "--project", "examples/Stroke.Examples.PrintText", "--", "AnsiColors"],
  cols: 120, rows: 24
});
await tui_wait_for_text({ session_id: ansi.id, text: "Foreground" });
await tui_wait_for_text({ session_id: ansi.id, text: "Background" });
await tui_screenshot({ session_id: ansi.id });
await tui_close({ session_id: ansi.id });
```

## Completion Verification

After implementing this feature, the example port status across all Python Prompt Toolkit examples will be:

| Category | Python | Ported | Status |
|----------|--------|--------|--------|
| Prompts | 56 | 56 | Feature 129 |
| Full-Screen | 25 | 25 | Feature 128 |
| Dialogs | 9 | 9 | Feature 127 |
| Choices | 8 | 8 | Feature 126 |
| Telnet | 4 | 4 | Feature 060 |
| SSH | 1 | 1 | Feature 061 |
| **Progress Bar** | **15** | **15** | **This feature** |
| **Print Text** | **9** | **9** | **This feature** |
| Tutorial | 1 | 0 | Feature 132 |
| gevent (excluded) | 1 | — | Not a real example |
| **Total** | **129** | **127/128** | **99.2%** |
