# Contract: SyncFromStart

**Namespace**: `Stroke.Lexers`
**Type**: Sealed Class (Singleton)

## Definition

```csharp
namespace Stroke.Lexers;

/// <summary>
/// Synchronization strategy that always starts from the beginning of the document.
/// </summary>
/// <remarks>
/// <para>
/// This strategy always returns position (0, 0), ensuring the lexer starts from
/// the very beginning of the document. While this gives the best highlighting
/// results, it can be slow for large documents.
/// </para>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>SyncFromStart</c> class
/// from <c>prompt_toolkit.lexers.pygments</c>.
/// </para>
/// <para>
/// This type is thread-safe. It is a stateless singleton.
/// </para>
/// </remarks>
public sealed class SyncFromStart : ISyntaxSync
{
    /// <summary>
    /// Gets the singleton instance.
    /// </summary>
    public static SyncFromStart Instance { get; }

    /// <summary>
    /// Private constructor to enforce singleton pattern.
    /// </summary>
    private SyncFromStart();

    /// <inheritdoc/>
    /// <remarks>
    /// Always returns (0, 0) regardless of the requested line number.
    /// </remarks>
    public (int Row, int Column) GetSyncStartPosition(Document document, int lineNo);
}
```

## Usage Examples

```csharp
// Get the singleton instance
var sync = SyncFromStart.Instance;

// Any document, any line number - always returns (0, 0)
var doc = new Document("line1\nline2\nline3\n... thousands of lines ...");

var pos0 = sync.GetSyncStartPosition(doc, 0);    // (0, 0)
var pos100 = sync.GetSyncStartPosition(doc, 100); // (0, 0)
var pos1000 = sync.GetSyncStartPosition(doc, 1000); // (0, 0)
```

## Implementation Notes

```csharp
public sealed class SyncFromStart : ISyntaxSync
{
    public static SyncFromStart Instance { get; } = new();

    private SyncFromStart() { }

    public (int Row, int Column) GetSyncStartPosition(Document document, int lineNo)
    {
        ArgumentNullException.ThrowIfNull(document);
        return (0, 0);
    }
}
```

## Invariants

1. `Instance` is never null
2. Always returns (0, 0) for any input
3. Only one instance exists (singleton)
