# Quickstart: Progress Bar and Print Text Examples

**Feature**: 068-progressbar-printtext-examples
**Date**: 2026-02-07

## Prerequisites

- .NET 10 SDK installed
- Stroke library builds successfully (`dotnet build src/Stroke/Stroke.csproj`)
- For ProgressBar examples: Feature 71 (ProgressBar API) must be implemented

## Build

```bash
# Build both example projects
dotnet build examples/Stroke.Examples.PrintText/Stroke.Examples.PrintText.csproj
dotnet build examples/Stroke.Examples.ProgressBar/Stroke.Examples.ProgressBar.csproj

# Or build the entire solution
dotnet build examples/Stroke.Examples.sln
```

## Run Print Text Examples

```bash
# List available examples
dotnet run --project examples/Stroke.Examples.PrintText

# Run a specific example
dotnet run --project examples/Stroke.Examples.PrintText -- print-formatted-text
dotnet run --project examples/Stroke.Examples.PrintText -- true-color-demo
dotnet run --project examples/Stroke.Examples.PrintText -- ansi-colors
dotnet run --project examples/Stroke.Examples.PrintText -- ansi
dotnet run --project examples/Stroke.Examples.PrintText -- html
dotnet run --project examples/Stroke.Examples.PrintText -- named-colors
dotnet run --project examples/Stroke.Examples.PrintText -- print-frame
dotnet run --project examples/Stroke.Examples.PrintText -- pygments-tokens
dotnet run --project examples/Stroke.Examples.PrintText -- logo-ansi-art
```

## Run Progress Bar Examples

```bash
# List available examples
dotnet run --project examples/Stroke.Examples.ProgressBar

# Run basic examples
dotnet run --project examples/Stroke.Examples.ProgressBar -- simple-progress-bar
dotnet run --project examples/Stroke.Examples.ProgressBar -- two-tasks
dotnet run --project examples/Stroke.Examples.ProgressBar -- unknown-length

# Run styled examples
dotnet run --project examples/Stroke.Examples.ProgressBar -- styled1
dotnet run --project examples/Stroke.Examples.ProgressBar -- styled2
dotnet run --project examples/Stroke.Examples.ProgressBar -- styled-apt-get
dotnet run --project examples/Stroke.Examples.ProgressBar -- styled-rainbow
dotnet run --project examples/Stroke.Examples.ProgressBar -- styled-tqdm1
dotnet run --project examples/Stroke.Examples.ProgressBar -- styled-tqdm2

# Run parallel/nested examples
dotnet run --project examples/Stroke.Examples.ProgressBar -- nested-progress-bars
dotnet run --project examples/Stroke.Examples.ProgressBar -- many-parallel-tasks
dotnet run --project examples/Stroke.Examples.ProgressBar -- lot-of-parallel-tasks

# Run advanced examples
dotnet run --project examples/Stroke.Examples.ProgressBar -- custom-key-bindings
dotnet run --project examples/Stroke.Examples.ProgressBar -- colored-title-label
dotnet run --project examples/Stroke.Examples.ProgressBar -- scrolling-task-name
```

## Verification

### PrintText Examples

Each print text example should:
1. Produce visible colored/styled output in the terminal
2. Exit cleanly with code 0
3. Match the visual output of the corresponding Python Prompt Toolkit example

### ProgressBar Examples

Each progress bar example should:
1. Display a live-updating progress bar
2. Complete iteration and exit cleanly
3. Handle Ctrl-C gracefully (restore terminal state)
4. Match the visual behavior of the corresponding Python example

### TUI Driver Verification

```bash
# Verify print text example output
tui_launch: dotnet run --project examples/Stroke.Examples.PrintText -- print-formatted-text
tui_wait_for_idle: Wait for output to complete
tui_text: Capture and verify colored text appears
tui_close: Clean up
```

## Python Reference

The Python Prompt Toolkit originals are at:
- `/Users/brandon/src/python-prompt-toolkit/examples/print-text/`
- `/Users/brandon/src/python-prompt-toolkit/examples/progress-bar/`

To run a Python example for side-by-side comparison:
```bash
cd /Users/brandon/src/python-prompt-toolkit
python examples/print-text/print-formatted-text.py
python examples/progress-bar/simple-progress-bar.py
```
