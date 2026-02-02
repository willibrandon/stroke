# Quickstart: Prompt Session

**Feature**: 047-prompt-session
**Date**: 2026-02-01

## Overview

The Prompt Session feature provides the primary user-facing API for Stroke — a GNU Readline-like prompt that handles text input, completion, validation, history, and more. It is the composition layer that ties together all lower-level Stroke components.

## Quick Usage

### Simple Prompt (One-Shot)

```csharp
using Stroke.Shortcuts;

// Simplest possible usage — one-shot prompt
string name = PromptFunctions.Prompt("What is your name? ");
Console.WriteLine($"Hello, {name}!");
```

### Session with History (REPL)

```csharp
using Stroke.Shortcuts;

// Create a session — history persists across calls
var session = new PromptSession<string>(">>> ");

while (true)
{
    try
    {
        string input = session.Prompt();
        Console.WriteLine($"You said: {input}");
    }
    catch (EOFException)
    {
        break; // Ctrl-D exits
    }
    catch (KeyboardInterruptException)
    {
        continue; // Ctrl-C cancels current input
    }
}
```

### With Autocompletion

```csharp
using Stroke.Shortcuts;
using Stroke.Completion;

var completer = new WordCompleter(["select", "from", "where", "insert", "update", "delete"]);
var session = new PromptSession<string>(
    message: "sql> ",
    completer: completer,
    completeStyle: CompleteStyle.MultiColumn);

string query = session.Prompt();
```

### Confirmation Prompt

```csharp
using Stroke.Shortcuts;

bool confirmed = PromptFunctions.Confirm("Delete all files?");
if (confirmed)
    Console.WriteLine("Deleting...");
```

### Async Prompt

```csharp
using Stroke.Shortcuts;

string input = await PromptFunctions.PromptAsync("Enter value: ");
```

## Build & Test

```bash
# Build the project
dotnet build src/Stroke/Stroke.csproj

# Run all tests
dotnet test tests/Stroke.Tests/Stroke.Tests.csproj

# Run only prompt session tests
dotnet test tests/Stroke.Tests/Stroke.Tests.csproj --filter "FullyQualifiedName~Shortcuts"
```

## Key Design Decisions

1. **Partial class split**: `PromptSession<TResult>` spans 6 files to stay under 1,000 LOC each
2. **Lock-protected properties**: All mutable session properties use `System.Threading.Lock` for thread safety
3. **Dynamic conditions**: `DynCond()` creates `Condition` lambdas that read session properties at render time, enabling runtime changes without layout rebuilds
4. **Per-prompt overrides**: Passing non-null values to `Prompt()` updates session state permanently (current + future calls)
5. **Custom exception types**: `KeyboardInterruptException` (Ctrl-C) and `EOFException` (Ctrl-D) are configurable via constructor

## Dependencies

This feature depends on all previous features (001–046) being complete. Key dependencies:

| Component | Feature | Used For |
|-----------|---------|----------|
| `Application<TResult>` | 030 | Event loop, rendering, key processing |
| `Buffer` / `Document` | 007 / 002 | Text storage, undo/redo, cursor |
| `Layout` / `HSplit` / `FloatContainer` | 029 | UI composition |
| `CompletionsMenu` / `MultiColumnCompletionsMenu` | 033 | Completion display |
| `SearchToolbar` / `SystemToolbar` / `ValidationToolbar` | 044 | Toolbar widgets |
| `Frame` / `TextArea` | 045 | Widget wrappers |
| `KeyBindings` / `MergedKeyBindings` | 022 | Key binding composition |
| `DynamicCompleter` / `ThreadedCompleter` | 012 | Runtime completer switching |
| `IHistory` / `InMemoryHistory` | 008 | Command history |
| `FormattedTextOutput` / `TerminalUtils` | 046 | Print utilities |
