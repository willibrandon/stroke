# Feature 92: System Completer

## Overview

Implement a system command completer that provides completions for executable files in the system PATH.

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/contrib/completers/system.py`

## Public API

### SystemCompleter

```csharp
namespace Stroke.Contrib.Completers;

/// <summary>
/// Completer that provides completions for system commands.
/// Searches PATH directories for executable files.
/// </summary>
public sealed class SystemCompleter : ICompleter
{
    /// <summary>
    /// Create a system completer.
    /// </summary>
    public SystemCompleter();

    /// <summary>
    /// Create a system completer with additional paths.
    /// </summary>
    /// <param name="additionalPaths">Extra directories to search.</param>
    public SystemCompleter(IEnumerable<string>? additionalPaths);

    /// <inheritdoc/>
    public IEnumerable<Completion> GetCompletions(
        Document document,
        CompleteEvent completeEvent);
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

### SystemCompleter Implementation

```csharp
public sealed class SystemCompleter : ICompleter
{
    private readonly HashSet<string> _additionalPaths;
    private HashSet<string>? _cachedCommands;
    private DateTime _cacheTime;
    private static readonly TimeSpan CacheExpiry = TimeSpan.FromMinutes(5);

    public SystemCompleter(IEnumerable<string>? additionalPaths = null)
    {
        _additionalPaths = additionalPaths?.ToHashSet() ?? new HashSet<string>();
    }

    public IEnumerable<Completion> GetCompletions(
        Document document,
        CompleteEvent completeEvent)
    {
        var word = document.GetWordBeforeCursor();
        if (string.IsNullOrEmpty(word))
            return Enumerable.Empty<Completion>();

        var commands = GetCommands();

        return commands
            .Where(cmd => cmd.StartsWith(word, StringComparison.OrdinalIgnoreCase))
            .OrderBy(cmd => cmd)
            .Select(cmd => new Completion(
                cmd.Substring(word.Length),
                startPosition: 0,
                displayText: cmd,
                displayMeta: "command"));
    }

    private HashSet<string> GetCommands()
    {
        if (_cachedCommands != null && DateTime.UtcNow - _cacheTime < CacheExpiry)
            return _cachedCommands;

        var commands = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var paths = GetSearchPaths();

        foreach (var path in paths)
        {
            if (!Directory.Exists(path))
                continue;

            try
            {
                foreach (var file in Directory.EnumerateFiles(path))
                {
                    if (IsExecutable(file))
                    {
                        var name = Path.GetFileNameWithoutExtension(file);
                        commands.Add(name);
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                // Skip directories we can't access
            }
        }

        _cachedCommands = commands;
        _cacheTime = DateTime.UtcNow;
        return commands;
    }

    private IEnumerable<string> GetSearchPaths()
    {
        var pathVar = Environment.GetEnvironmentVariable("PATH") ?? "";
        var separator = Platform.IsWindows ? ';' : ':';

        return pathVar.Split(separator)
            .Concat(_additionalPaths)
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Distinct();
    }

    private static bool IsExecutable(string filePath)
    {
        if (Platform.IsWindows)
        {
            var ext = Path.GetExtension(filePath).ToLowerInvariant();
            return ext is ".exe" or ".cmd" or ".bat" or ".com" or ".ps1";
        }
        else
        {
            // On Unix, check execute permission
            try
            {
                var info = new FileInfo(filePath);
                // Check if file is executable (simplified)
                return (info.Attributes & FileAttributes.Normal) != 0;
            }
            catch
            {
                return false;
            }
        }
    }
}
```

### Usage Example

```csharp
// Create a prompt with system command completion
var session = new PromptSession(
    completer: new MergeCompleter(
        new SystemCompleter(),
        new PathCompleter()
    )
);

// Commands like "git", "python", "node" will be completed
var command = await session.PromptAsync("$ ");
```

### Integration with Grammar

```csharp
// Use with regular language grammar
var grammar = GrammarCompiler.Compile(@"
    (?P<command>\S+) \s* (?P<args>.*)
");

var completer = grammar.CreateCompleter(new Dictionary<string, ICompleter>
{
    ["command"] = new SystemCompleter(),
    ["args"] = new PathCompleter()
});
```

## Dependencies

- Feature 11: Completion (ICompleter, Completion)
- Feature 90: Platform utilities

## Implementation Tasks

1. Implement PATH parsing
2. Implement executable detection per platform
3. Implement command caching
4. Implement completion matching
5. Add cache expiry
6. Add additional paths support
7. Write unit tests

## Acceptance Criteria

- [ ] Finds executables in PATH directories
- [ ] Respects platform-specific executable extensions
- [ ] Caches results for performance
- [ ] Cache expires after timeout
- [ ] Supports additional search paths
- [ ] Case-insensitive matching on Windows
- [ ] Handles inaccessible directories gracefully
- [ ] Unit tests achieve 80% coverage
