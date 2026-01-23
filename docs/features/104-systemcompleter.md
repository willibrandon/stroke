# Feature 104: System Completer

## Overview

Implement SystemCompleter - a completer for system shell commands that uses GrammarCompleter to provide completion for executables and file paths as command arguments.

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/contrib/completers/system.py`

## Public API

### SystemCompleter

```csharp
namespace Stroke.Contrib.Completers;

/// <summary>
/// Completer for system shell commands.
/// Provides completion for:
/// - Executables (first word)
/// - File paths as arguments (subsequent words)
/// Handles both unquoted and quoted (single/double) file paths.
/// </summary>
/// <example>
/// var completer = new SystemCompleter();
/// // Input: "ls /ho" → completes to "/home"
/// // Input: "cat '/home/user/my fi" → completes to "my file.txt"
/// </example>
public sealed class SystemCompleter : GrammarCompleter
{
    /// <summary>
    /// Create a system command completer.
    /// </summary>
    public SystemCompleter();
}
```

## Project Structure

```
src/Stroke/
└── Contrib/
    └── Completers/
        └── SystemCompleter.cs
tests/Stroke.Tests/
└── Contrib/
    └── Completers/
        └── SystemCompleterTests.cs
```

## Implementation Notes

### SystemCompleter Grammar

The grammar handles:
1. An executable name (first word)
2. Zero or more arguments, which can be:
   - Unquoted paths
   - Double-quoted paths (with backslash escaping)
   - Single-quoted paths (with backslash escaping)

```csharp
public sealed class SystemCompleter : GrammarCompleter
{
    private static readonly CompiledGrammar _grammar;
    private static readonly Dictionary<string, ICompleter> _completers;

    static SystemCompleter()
    {
        // Compile the grammar for shell commands
        _grammar = Grammar.Compile(
            @"
                # First we have an executable
                (?P<executable>[^\s]+)

                # Ignore literals in between
                (
                    \s+
                    (""[^""]*"" | '[^']*' | [^'""]+ )
                )*

                \s+

                # Filename as parameters
                (
                    (?P<filename>[^\s]+) |
                    ""(?P<double_quoted_filename>[^\s]+)"" |
                    '(?P<single_quoted_filename>[^\s]+)'
                )
            ",
            escapeFuncs: new Dictionary<string, Func<string, string>>
            {
                ["double_quoted_filename"] = s => s.Replace("\"", "\\\""),
                ["single_quoted_filename"] = s => s.Replace("'", "\\'")
            },
            unescapeFuncs: new Dictionary<string, Func<string, string>>
            {
                ["double_quoted_filename"] = s => s.Replace("\\\"", "\""),
                ["single_quoted_filename"] = s => s.Replace("\\'", "'")
            }
        );

        // Set up completers for each variable
        _completers = new Dictionary<string, ICompleter>
        {
            ["executable"] = new ExecutableCompleter(),
            ["filename"] = new PathCompleter(onlyDirectories: false, expandUser: true),
            ["double_quoted_filename"] = new PathCompleter(onlyDirectories: false, expandUser: true),
            ["single_quoted_filename"] = new PathCompleter(onlyDirectories: false, expandUser: true)
        };
    }

    public SystemCompleter()
        : base(_grammar, _completers)
    {
    }
}
```

### ExecutableCompleter

```csharp
namespace Stroke.Contrib.Completers;

/// <summary>
/// Completer for executable names in PATH.
/// </summary>
public sealed class ExecutableCompleter : ICompleter
{
    /// <inheritdoc/>
    public IEnumerable<Completion> GetCompletions(
        Document document,
        CompleteEvent completeEvent)
    {
        var text = document.TextBeforeCursor;

        // Get executables from PATH
        foreach (var executable in GetExecutablesInPath())
        {
            if (executable.StartsWith(text, StringComparison.OrdinalIgnoreCase))
            {
                yield return new Completion(
                    text: executable,
                    startPosition: -text.Length,
                    displayMeta: "executable");
            }
        }
    }

    private static IEnumerable<string> GetExecutablesInPath()
    {
        var path = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrEmpty(path))
            yield break;

        var separator = OperatingSystem.IsWindows() ? ';' : ':';
        var extensions = OperatingSystem.IsWindows()
            ? new[] { ".exe", ".cmd", ".bat", ".com", ".ps1" }
            : Array.Empty<string>();

        foreach (var dir in path.Split(separator))
        {
            if (!Directory.Exists(dir))
                continue;

            foreach (var file in Directory.EnumerateFiles(dir))
            {
                var name = Path.GetFileName(file);

                if (OperatingSystem.IsWindows())
                {
                    var ext = Path.GetExtension(file).ToLowerInvariant();
                    if (extensions.Contains(ext))
                    {
                        yield return Path.GetFileNameWithoutExtension(name);
                    }
                }
                else
                {
                    // On Unix, check if file is executable
                    if (IsExecutable(file))
                    {
                        yield return name;
                    }
                }
            }
        }
    }

    private static bool IsExecutable(string path)
    {
        if (OperatingSystem.IsWindows())
            return true;

        try
        {
            var info = new FileInfo(path);
            // Check for execute permission (simplified)
            return (info.UnixFileMode & UnixFileMode.UserExecute) != 0 ||
                   (info.UnixFileMode & UnixFileMode.GroupExecute) != 0 ||
                   (info.UnixFileMode & UnixFileMode.OtherExecute) != 0;
        }
        catch
        {
            return false;
        }
    }
}
```

### Usage Example

```csharp
// Create a simple shell with system command completion
var session = new PromptSession(
    completer: new SystemCompleter()
);

while (true)
{
    var command = await session.PromptAsync("$ ");

    if (string.IsNullOrWhiteSpace(command))
        continue;

    if (command == "exit")
        break;

    // Execute the command
    var psi = new ProcessStartInfo
    {
        FileName = OperatingSystem.IsWindows() ? "cmd.exe" : "/bin/sh",
        Arguments = OperatingSystem.IsWindows() ? $"/c {command}" : $"-c \"{command}\"",
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false
    };

    using var process = Process.Start(psi);
    Console.WriteLine(await process.StandardOutput.ReadToEndAsync());
    Console.Error.WriteLine(await process.StandardError.ReadToEndAsync());
    await process.WaitForExitAsync();
}
```

## Dependencies

- Feature 103: Regular Language Grammar (GrammarCompleter)
- Feature 86: Path Completer
- Feature 11: Completion (ICompleter)

## Implementation Tasks

1. Implement ExecutableCompleter for PATH lookup
2. Handle Windows executable extensions
3. Handle Unix executable permission check
4. Define shell command grammar
5. Implement escape/unescape for quoted strings
6. Implement SystemCompleter as GrammarCompleter subclass
7. Write unit tests

## Acceptance Criteria

- [ ] Completes executable names from PATH
- [ ] Completes file paths as arguments
- [ ] Handles unquoted file paths
- [ ] Handles double-quoted file paths with escaping
- [ ] Handles single-quoted file paths with escaping
- [ ] Works on Windows (exe, cmd, bat extensions)
- [ ] Works on Unix (executable permission check)
- [ ] Unit tests achieve 80% coverage
