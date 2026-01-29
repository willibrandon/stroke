# Contract: RegexSync

**Namespace**: `Stroke.Lexers`
**Type**: Sealed Class

## Definition

```csharp
namespace Stroke.Lexers;

/// <summary>
/// Synchronization strategy that finds a sync point by matching a regex pattern.
/// </summary>
/// <remarks>
/// <para>
/// This strategy scans backwards from the requested line to find a line matching
/// the given regex pattern. Common patterns match function definitions, class
/// declarations, or tag boundaries that represent "safe" points to start lexing.
/// </para>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>RegexSync</c> class
/// from <c>prompt_toolkit.lexers.pygments</c>.
/// </para>
/// <para>
/// This type is thread-safe. It is immutable after construction, and
/// <see cref="Regex"/> instances are thread-safe for matching operations.
/// </para>
/// </remarks>
public sealed class RegexSync : ISyntaxSync
{
    /// <summary>
    /// Maximum number of lines to scan backwards. Never go more than this amount
    /// of lines backwards for synchronization, as that would be too CPU intensive.
    /// </summary>
    public const int MaxBackwards = 500;

    /// <summary>
    /// If no synchronization position is found and we're within this many lines
    /// from the start, start lexing from the beginning.
    /// </summary>
    public const int FromStartIfNoSyncPosFound = 100;

    /// <summary>
    /// Initializes a new instance with the given regex pattern.
    /// </summary>
    /// <param name="pattern">The regex pattern to match for sync points.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="pattern"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="pattern"/> is an invalid regex.</exception>
    public RegexSync(string pattern);

    /// <inheritdoc/>
    /// <remarks>
    /// <para>
    /// Scans backwards up to <see cref="MaxBackwards"/> lines to find a pattern match.
    /// </para>
    /// <para>
    /// If no match is found:
    /// <list type="bullet">
    ///   <item>If within <see cref="FromStartIfNoSyncPosFound"/> lines of start: returns (0, 0)</item>
    ///   <item>Otherwise: returns (lineNo, 0) - start at the requested line</item>
    /// </list>
    /// </para>
    /// </remarks>
    public (int Row, int Column) GetSyncStartPosition(Document document, int lineNo);

    /// <summary>
    /// Creates a <see cref="RegexSync"/> instance with a pattern appropriate for the given language.
    /// </summary>
    /// <param name="language">The language name (e.g., "Python", "HTML", "JavaScript").</param>
    /// <returns>A configured <see cref="RegexSync"/> instance.</returns>
    /// <remarks>
    /// <para>
    /// Known patterns:
    /// <list type="bullet">
    ///   <item>"Python", "Python 3": <c>^\s*(class|def)\s+</c></item>
    ///   <item>"HTML": <c>&lt;[/a-zA-Z]</c></item>
    ///   <item>"JavaScript": <c>\bfunction\b</c></item>
    ///   <item>Others: <c>^</c> (matches any line start)</item>
    /// </list>
    /// </para>
    /// </remarks>
    public static RegexSync ForLanguage(string language);
}
```

## Usage Examples

```csharp
// Custom pattern
var pythonSync = new RegexSync(@"^\s*(class|def)\s+");

var doc = new Document("""
    import os

    class Foo:
        def bar(self):
            x = 1
            y = 2
            return x + y

    def baz():
        pass
    """);

// Find sync position for line 6 (return x + y)
var pos = pythonSync.GetSyncStartPosition(doc, 6);
// Returns (3, 0) - "def bar(self):" on line 3

// Using ForLanguage factory
var htmlSync = RegexSync.ForLanguage("HTML");
var htmlDoc = new Document("<html>\n<body>\n<p>text</p>\n</body>\n</html>");
var htmlPos = htmlSync.GetSyncStartPosition(htmlDoc, 3); // (2, 0) - "<p>text</p>"

// Unknown language gets default pattern
var defaultSync = RegexSync.ForLanguage("Unknown");
// Uses "^" pattern - matches any line start
```

## Implementation Notes

```csharp
public (int Row, int Column) GetSyncStartPosition(Document document, int lineNo)
{
    ArgumentNullException.ThrowIfNull(document);

    var lines = document.Lines;
    var maxLine = Math.Max(-1, lineNo - MaxBackwards);

    // Scan upwards to find a sync point
    for (int i = lineNo; i > maxLine; i--)
    {
        if (i >= 0 && i < lines.Length)
        {
            var match = _compiledPattern.Match(lines[i]);
            if (match.Success)
                return (i, match.Index);
        }
    }

    // No sync point found
    if (lineNo < FromStartIfNoSyncPosFound)
        return (0, 0);
    else
        return (lineNo, 0);
}
```

## Language Patterns

| Language | Pattern | Matches |
|----------|---------|---------|
| Python | `^\s*(class\|def)\s+` | Class and function definitions |
| Python 3 | `^\s*(class\|def)\s+` | Class and function definitions |
| HTML | `<[/a-zA-Z]` | Opening and closing tags |
| JavaScript | `\bfunction\b` | Function declarations |
| (default) | `^` | Any line start |

## Invariants

1. Pattern is compiled once at construction (RegexOptions.Compiled)
2. Never scans more than MaxBackwards (500) lines backwards
3. Returns (0, 0) for small documents (lineNo < FromStartIfNoSyncPosFound)
4. Column is always the match start position or 0
