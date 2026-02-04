# Quickstart: System Completer

**Feature**: 058-system-completer
**Date**: 2026-02-03

## What is SystemCompleter?

SystemCompleter provides autocompletion for shell-like commands. It automatically completes:

- **Executable names** from your PATH when typing a command
- **File paths** when typing arguments after the command

## Basic Usage

```csharp
using Stroke.Completion;
using Stroke.Contrib.Completers;
using Stroke.Shortcuts;

// Use SystemCompleter with a prompt
var result = Prompt.RunPrompt(
    completer: new SystemCompleter()
);
```

## How It Works

SystemCompleter uses a grammar pattern to understand shell command structure:

1. **First word** → completed as an executable from PATH
2. **Subsequent words** → completed as file paths

### Example Completions

| Input | Cursor Position | Completions |
|-------|-----------------|-------------|
| `gi` | after "gi" | `git`, `gist`, ... (executables starting with "gi") |
| `cat /ho` | after "ho" | `/home` (if exists) |
| `ls ~/Doc` | after "Doc" | `~/Documents`, `~/Docker` (if exist) |
| `cat "my fi` | inside quotes | `"my file.txt"` (with escaping) |

## Quoted Paths

For files with spaces, use quotes:

```bash
cat "path with spaces/file.txt"
cat 'another path/file.txt'
```

SystemCompleter handles escape characters automatically:
- Double quotes: `"` in filename → `\"`
- Single quotes: `'` in filename → `\'`

## Configuration

SystemCompleter is pre-configured and requires no setup. It automatically:

- Searches all directories in PATH for executables
- Expands `~` to your home directory
- Handles platform-specific executable detection (Windows extensions, Unix permissions)

## Integration with Prompt

```csharp
var session = new PromptSession<string>(
    completer: new SystemCompleter(),
    completeWhileTyping: true
);

while (true)
{
    var command = await session.PromptAsync("$ ");
    // Execute command...
}
```
