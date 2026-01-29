# Contract: ISyntaxSync

**Namespace**: `Stroke.Lexers`
**Type**: Interface

## Definition

```csharp
namespace Stroke.Lexers;

/// <summary>
/// Syntax synchronizer interface for finding a start position for lexing.
/// </summary>
/// <remarks>
/// <para>
/// This is especially important when editing large documents; we don't want to start
/// the highlighting by running the lexer from the beginning of the file, which would
/// be very slow.
/// </para>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>SyntaxSync</c> abstract class
/// from <c>prompt_toolkit.lexers.pygments</c>.
/// </para>
/// <para>
/// Implementations must be thread-safe per Constitution XI.
/// </para>
/// </remarks>
public interface ISyntaxSync
{
    /// <summary>
    /// Returns the position from where we can start lexing as a (row, column) tuple.
    /// </summary>
    /// <param name="document">The document that contains all the lines.</param>
    /// <param name="lineNo">
    /// The line number (0-based) that we want to highlight.
    /// The returned position must be this line or an earlier position.
    /// </param>
    /// <returns>
    /// A tuple of (row, column) representing the safe start position for lexing.
    /// Row and column are both 0-based.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="document"/> is <c>null</c>.</exception>
    (int Row, int Column) GetSyncStartPosition(Document document, int lineNo);
}
```

## Usage Examples

```csharp
// Using SyncFromStart
ISyntaxSync sync = SyncFromStart.Instance;
var doc = new Document("line1\nline2\nline3");

var pos = sync.GetSyncStartPosition(doc, 2); // (0, 0) - always starts from beginning

// Using RegexSync
ISyntaxSync regexSync = new RegexSync(@"^\s*(class|def)\s+");
var pyDoc = new Document("x = 1\ndef foo():\n    pass\nprint(x)");

var pos1 = regexSync.GetSyncStartPosition(pyDoc, 3); // (1, 0) - found "def foo():" on line 1
```

## Invariants

1. Returned row is always ≤ `lineNo`
2. Returned row is always ≥ 0
3. Returned column is always ≥ 0
4. For empty documents, returns (0, 0)
