# API Contract: SystemCompleter

**Feature**: 058-system-completer
**Date**: 2026-02-03

## Namespace

```csharp
namespace Stroke.Contrib.Completers;
```

## Class: SystemCompleter

```csharp
/// <summary>
/// Completer for system shell commands.
/// </summary>
/// <remarks>
/// <para>
/// Provides completion for executable names (at command position) and file paths
/// (at argument positions) using a grammar-based approach.
/// </para>
/// <para>
/// Supports three path formats:
/// <list type="bullet">
///   <item>Unquoted paths: <c>cat /home/user/file.txt</c></item>
///   <item>Double-quoted paths: <c>cat "/home/user/my file.txt"</c></item>
///   <item>Single-quoted paths: <c>cat '/home/user/my file.txt'</c></item>
/// </list>
/// </para>
/// <para>
/// This class is thread-safe; all operations can be called concurrently.
/// </para>
/// </remarks>
public sealed class SystemCompleter : GrammarCompleter
{
    /// <summary>
    /// Creates a new SystemCompleter with default configuration.
    /// </summary>
    public SystemCompleter();
}
```

## Inherited Members

From `GrammarCompleter`:

```csharp
/// <inheritdoc/>
public IEnumerable<Completion> GetCompletions(Document document, CompleteEvent completeEvent);

/// <inheritdoc/>
public IAsyncEnumerable<Completion> GetCompletionsAsync(
    Document document,
    CompleteEvent completeEvent,
    CancellationToken cancellationToken = default);
```

## Grammar Pattern

The internal grammar pattern (not exposed publicly):

```regex
# First we have an executable.
(?P<executable>[^\s]+)

# Ignore literals in between.
(
    \s+
    ("[^"]*" | '[^']*' | [^'"]+ )
)*

\s+

# Filename as parameters.
(
    (?P<filename>[^\s]+) |
    "(?P<double_quoted_filename>[^\s]+)" |
    '(?P<single_quoted_filename>[^\s]+)'
)
```

## Escape Functions

| Variable | Escape | Unescape |
|----------|--------|----------|
| `double_quoted_filename` | `"` → `\"` | `\"` → `"` |
| `single_quoted_filename` | `'` → `\'` | `\'` → `'` |

## Usage Example

```csharp
using Stroke.Completion;
using Stroke.Contrib.Completers;
using Stroke.Core;

// Create completer
var completer = new SystemCompleter();

// Get completions for partial command
var document = new Document("cat /ho", 7);
var completions = completer.GetCompletions(document, new CompleteEvent());

// completions might include: "/home" if /home exists on the system
```
