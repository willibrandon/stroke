# Contracts: Progress Bar Examples

**Feature**: 068-progressbar-printtext-examples
**Date**: 2026-02-07
**Project**: Stroke.Examples.ProgressBar

## Project Configuration

```xml
<!-- Stroke.Examples.ProgressBar.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <OutputType>Exe</OutputType>
    <LangVersion>13</LangVersion>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\..\src\Stroke\Stroke.csproj" />
  </ItemGroup>
</Project>
```

## Program.cs — Routing Contract

```csharp
namespace Stroke.Examples.ProgressBarExamples;

internal static class Program
{
    private static readonly Dictionary<string, Func<Task>> Examples =
        new(StringComparer.OrdinalIgnoreCase)
    {
        ["simple-progress-bar"] = SimpleProgressBar.Run,
        ["two-tasks"] = TwoTasks.Run,
        ["unknown-length"] = UnknownLength.Run,
        ["nested-progress-bars"] = NestedProgressBars.Run,
        ["colored-title-label"] = ColoredTitleLabel.Run,
        ["scrolling-task-name"] = ScrollingTaskName.Run,
        ["styled1"] = Styled1.Run,
        ["styled2"] = Styled2.Run,
        ["styled-apt-get"] = StyledAptGet.Run,
        ["styled-rainbow"] = StyledRainbow.Run,
        ["styled-tqdm1"] = StyledTqdm1.Run,
        ["styled-tqdm2"] = StyledTqdm2.Run,
        ["custom-key-bindings"] = CustomKeyBindings.Run,
        ["many-parallel-tasks"] = ManyParallelTasks.Run,
        ["lot-of-parallel-tasks"] = LotOfParallelTasks.Run,
    };

    public static async Task<int> Main(string[] args)
    {
        // Route to example or show usage
        // Catch KeyboardInterruptException and EOFException at top level (FR-020)
    }

    private static void ShowUsage() { /* list examples */ }
}
```

**Note**: Namespace is `Stroke.Examples.ProgressBarExamples` to avoid collision with the `ProgressBar` class from `Stroke.Shortcuts`. Program.Main returns `Task<int>` for async support and exit codes.

## Example Class Contracts

### SimpleProgressBar.cs (FR-021)
**Python source**: `progress-bar/simple-progress-bar.py` (20 lines)

```csharp
namespace Stroke.Examples.ProgressBarExamples;

/// <summary>
/// Basic progress bar iterating over 800 items.
/// Port of Python Prompt Toolkit's simple-progress-bar.py example.
/// </summary>
public static class SimpleProgressBar
{
    public static async Task Run()
    {
        // await using var pb = new ProgressBar();
        // await foreach (var i in pb.Iterate(Enumerable.Range(0, 800)))
        //     await Task.Delay(10);
    }
}
```

### TwoTasks.cs (FR-022)
**Python source**: `progress-bar/two-tasks.py` (41 lines)

```csharp
namespace Stroke.Examples.ProgressBarExamples;

/// <summary>
/// Two parallel progress bars on separate threads.
/// Port of Python Prompt Toolkit's two-tasks.py example.
/// </summary>
public static class TwoTasks
{
    public static async Task Run()
    {
        // await using var pb = new ProgressBar();
        // Thread t1: pb.Iterate(range(100), label: "Task 1") with 50ms sleep
        // Thread t2: pb.Iterate(range(150), label: "Task 2") with 80ms sleep
        // Both threads: IsBackground = true
        // Join with 500ms timeout for Windows Ctrl-C compatibility
    }
}
```

### UnknownLength.cs (FR-023)
**Python source**: `progress-bar/unknown-length.py` (28 lines)

```csharp
namespace Stroke.Examples.ProgressBarExamples;

/// <summary>
/// Progress bar for iteration with no known total length.
/// Port of Python Prompt Toolkit's unknown-length.py example.
/// </summary>
public static class UnknownLength
{
    public static async Task Run()
    {
        // IEnumerable<int> generator with yield (no Length)
        // await using var pb = new ProgressBar();
        // await foreach (var i in pb.Iterate(generator))
        //     await Task.Delay(100);
    }
}
```

### NestedProgressBars.cs (FR-024)
**Python source**: `progress-bar/nested-progress-bars.py` (24 lines)

```csharp
namespace Stroke.Examples.ProgressBarExamples;

/// <summary>
/// Nested progress bars with inner bars that appear and disappear.
/// Port of Python Prompt Toolkit's nested-progress-bars.py example.
/// </summary>
public static class NestedProgressBars
{
    public static async Task Run()
    {
        // await using var pb = new ProgressBar(title: Html("..."), bottomToolbar: Html("..."));
        // Outer: pb.Iterate(range(6), label: "Main task")
        //   Inner: pb.Iterate(range(200), label: "Subtask", removeWhenDone: true)
    }
}
```

### ColoredTitleLabel.cs (FR-025)
**Python source**: `progress-bar/colored-title-and-label.py` (24 lines)

```csharp
namespace Stroke.Examples.ProgressBarExamples;

/// <summary>
/// Progress bar with HTML-colored title and label text.
/// Port of Python Prompt Toolkit's colored-title-and-label.py example.
/// </summary>
public static class ColoredTitleLabel
{
    public static async Task Run()
    {
        // Html title: "Downloading <style bg='yellow' fg='black'>4 files...</style>"
        // Html label: "<ansired>some file</ansired>:"
        // 800 items, 10ms sleep
    }
}
```

### ScrollingTaskName.cs (FR-026)
**Python source**: `progress-bar/scrolling-task-name.py` (25 lines)

```csharp
namespace Stroke.Examples.ProgressBarExamples;

/// <summary>
/// Progress bar with a long label that scrolls when terminal is narrow.
/// Port of Python Prompt Toolkit's scrolling-task-name.py example.
/// </summary>
public static class ScrollingTaskName
{
    public static async Task Run()
    {
        // Very long label string
        // Custom title warning about window size
        // 800 items, 10ms sleep
    }
}
```

### Styled1.cs (FR-027)
**Python source**: `progress-bar/styled-1.py` (38 lines)

```csharp
namespace Stroke.Examples.ProgressBarExamples;

/// <summary>
/// Progress bar with custom Style affecting 10 visual elements.
/// Port of Python Prompt Toolkit's styled-1.py example.
/// </summary>
public static class Styled1
{
    public static async Task Run()
    {
        // Style.FromDict with 10 keys:
        //   title, label, percentage, bar-a, bar-b, bar-c,
        //   current, total, time-elapsed, time-left
        // 1600 items, 10ms sleep
    }
}
```

### Styled2.cs (FR-028)
**Python source**: `progress-bar/styled-2.py` (51 lines)

```csharp
namespace Stroke.Examples.ProgressBarExamples;

/// <summary>
/// Progress bar with custom formatters: SpinningWheel, Bar, TimeLeft.
/// Port of Python Prompt Toolkit's styled-2.py example.
/// </summary>
public static class Styled2
{
    public static async Task Run()
    {
        // Custom formatters list:
        //   Label(), SpinningWheel(), Text(" ~~~~ ", style: "fg:ansicyan"),
        //   Bar(sym_a: "#", sym_b: "#", sym_c: "."), Text(" | "), TimeLeft()
        // Custom style for progressbar title, item-title, bar segments, spinning-wheel
        // 20 items, 1s sleep
    }
}
```

### StyledAptGet.cs (FR-029)
**Python source**: `progress-bar/styled-apt-get-install.py` (40 lines)

```csharp
namespace Stroke.Examples.ProgressBarExamples;

/// <summary>
/// apt-get install style progress bar.
/// Port of Python Prompt Toolkit's styled-apt-get-install.py example.
/// </summary>
public static class StyledAptGet
{
    public static async Task Run()
    {
        // Formatters: Label(suffix: ": "), Percentage(), Text(" "),
        //   Bar(sym_a: "#", sym_b: "#", sym_c: "."), Text(" "), Progress(),
        //   Text(" "), TimeLeft(), Text(" "), TimeElapsed()
        // Style: label with yellow background
        // 1600 items, 10ms sleep
    }
}
```

### StyledRainbow.cs (FR-030)
**Python source**: `progress-bar/styled-rainbow.py` (37 lines)

```csharp
namespace Stroke.Examples.ProgressBarExamples;

/// <summary>
/// Rainbow-colored progress bar with color depth prompt.
/// Port of Python Prompt Toolkit's styled-rainbow.py example.
/// </summary>
public static class StyledRainbow
{
    public static async Task Run()
    {
        // Prompt user: confirm("Is your terminal a true-color terminal?")
        // Set colorDepth based on answer: Depth24Bit or Depth8Bit
        // Formatters: Label(), Text(" "), Rainbow(Bar()), Text(" "), Rainbow(TimeLeft())
        // 20 items, 1s sleep
    }
}
```

### StyledTqdm1.cs (FR-031)
**Python source**: `progress-bar/styled-tqdm-1.py` (42 lines)

```csharp
namespace Stroke.Examples.ProgressBarExamples;

/// <summary>
/// tqdm-inspired progress bar format with iterations per second.
/// Port of Python Prompt Toolkit's styled-tqdm-1.py example.
/// </summary>
public static class StyledTqdm1
{
    public static async Task Run()
    {
        // Formatters: Label(suffix: ": "), Text("|"), Bar(sym_a: "█", sym_b: "█", sym_c: " "),
        //   Text("| "), Progress(), Text(" "), Percentage(), Text(" [elapsed: "),
        //   TimeElapsed(), Text(" left: "), TimeLeft(), Text(", "),
        //   IterationsPerSecond(), Text(" iters/sec]")
        // Cyan style
        // 1600 items, 10ms sleep
    }
}
```

### StyledTqdm2.cs (FR-032)
**Python source**: `progress-bar/styled-tqdm-2.py` (40 lines)

```csharp
namespace Stroke.Examples.ProgressBarExamples;

/// <summary>
/// tqdm-style progress bar with reverse-video bar.
/// Port of Python Prompt Toolkit's styled-tqdm-2.py example.
/// </summary>
public static class StyledTqdm2
{
    public static async Task Run()
    {
        // Formatters: Label(suffix: ": "), Percentage(), Text(" "),
        //   Bar(sym_a: " ", sym_b: " ", sym_c: " "), Text(" "), Progress(),
        //   Text(" ["), TimeElapsed(), Text("<"), TimeLeft(), Text(", "),
        //   IterationsPerSecond(), Text("it/s]")
        // Bar uses reverse style for background-fill effect
        // 1600 items, 10ms sleep
    }
}
```

### CustomKeyBindings.cs (FR-033)
**Python source**: `progress-bar/custom-key-bindings.py` (53 lines)

```csharp
namespace Stroke.Examples.ProgressBarExamples;

/// <summary>
/// Progress bar with custom key bindings: f prints text, q cancels, x sends interrupt.
/// Port of Python Prompt Toolkit's custom-key-bindings.py example.
/// </summary>
public static class CustomKeyBindings
{
    public static async Task Run()
    {
        // KeyBindings:
        //   'f': Console.WriteLine("You pressed `f`.") via PatchStdout
        //   'q': Set cancel flag, break loop
        //   'x': Raise KeyboardInterruptException (equivalent to SIGINT)
        // Bottom toolbar: Html with key hints
        // PatchStdout wraps the progress bar
        // 800 items, 10ms sleep, check cancel flag each iteration
    }
}
```

### ManyParallelTasks.cs (FR-034)
**Python source**: `progress-bar/many-parallel-tasks.py` (48 lines)

```csharp
namespace Stroke.Examples.ProgressBarExamples;

/// <summary>
/// 8 concurrent parallel tasks with HTML title and bottom toolbar.
/// Port of Python Prompt Toolkit's many-parallel-tasks.py example.
/// </summary>
public static class ManyParallelTasks
{
    public static async Task Run()
    {
        // ProgressBar with Html title and bottomToolbar
        // 8 tasks with varying totals (8-220) and sleep times (0.05-3s):
        //   Task1: 8 items, 3s | Task2: 50 items, 0.2s | Task3: 12 items, 0.5s
        //   Task4: 220 items, 0.05s | Task5: 24 items, 0.1s | Task6: 100 items, 0.08s
        //   Task7: 40 items, 0.3s | Task8: 32 items, 0.2s
        // All threads: IsBackground = true
        // Join with 500ms timeout
    }
}
```

### LotOfParallelTasks.cs (FR-035)
**Python source**: `progress-bar/a-lot-of-parallel-tasks.py` (67 lines)

```csharp
namespace Stroke.Examples.ProgressBarExamples;

/// <summary>
/// 160 parallel tasks with random durations, some breaking early.
/// Port of Python Prompt Toolkit's a-lot-of-parallel-tasks.py example.
/// </summary>
public static class LotOfParallelTasks
{
    public static async Task Run()
    {
        // ProgressBar with Html title and bottomToolbar
        // 160 threads, each with:
        //   Random total (50-200), random sleep (0.05-0.20s)
        //   Randomly assigned to RunTask (complete) or StopTask (break at random point)
        // StopTask: break at random index, update label to "{label} BREAK"
        // All threads: IsBackground = true
        // Join with 500ms timeout
    }
}
```

## CLI Routing Names

| CLI Name | Class | Python Source |
|----------|-------|--------------|
| `simple-progress-bar` | `SimpleProgressBar` | `simple-progress-bar.py` |
| `two-tasks` | `TwoTasks` | `two-tasks.py` |
| `unknown-length` | `UnknownLength` | `unknown-length.py` |
| `nested-progress-bars` | `NestedProgressBars` | `nested-progress-bars.py` |
| `colored-title-label` | `ColoredTitleLabel` | `colored-title-and-label.py` |
| `scrolling-task-name` | `ScrollingTaskName` | `scrolling-task-name.py` |
| `styled1` | `Styled1` | `styled-1.py` |
| `styled2` | `Styled2` | `styled-2.py` |
| `styled-apt-get` | `StyledAptGet` | `styled-apt-get-install.py` |
| `styled-rainbow` | `StyledRainbow` | `styled-rainbow.py` |
| `styled-tqdm1` | `StyledTqdm1` | `styled-tqdm-1.py` |
| `styled-tqdm2` | `StyledTqdm2` | `styled-tqdm-2.py` |
| `custom-key-bindings` | `CustomKeyBindings` | `custom-key-bindings.py` |
| `many-parallel-tasks` | `ManyParallelTasks` | `many-parallel-tasks.py` |
| `lot-of-parallel-tasks` | `LotOfParallelTasks` | `a-lot-of-parallel-tasks.py` |

## Threading Model

All parallel progress bar examples (TwoTasks, ManyParallelTasks, LotOfParallelTasks) use the same pattern:

```csharp
var thread = new Thread(() =>
{
    foreach (var i in pb.Iterate(Enumerable.Range(0, total), label: label))
    {
        Thread.Sleep(sleepMs);
    }
}) { IsBackground = true };
thread.Start();

// Later, join with timeout:
while (thread.IsAlive)
    thread.Join(TimeSpan.FromMilliseconds(500));
```

This matches Python's `threading.Thread(target=..., daemon=True)` with `t.join(timeout=0.5)` for cross-platform Ctrl-C compatibility.
