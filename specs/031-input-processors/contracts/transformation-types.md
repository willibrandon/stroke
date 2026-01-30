# Contract: Transformation Types

**Feature**: 031-input-processors
**Namespace**: `Stroke.Layout.Processors`
**Python Source**: `prompt_toolkit/layout/processors.py` (lines 80-153)

## TransformationInput

```csharp
/// <summary>
/// Input data for a processor transformation. Contains the rendering context
/// for a single line being processed.
/// </summary>
/// <remarks>
/// Port of Python Prompt Toolkit's <c>TransformationInput</c> class.
/// This type is immutable — all properties are set at construction time.
/// </remarks>
public sealed class TransformationInput
{
    public TransformationInput(
        BufferControl bufferControl,
        Document document,
        int lineNumber,
        Func<int, int> sourceToDisplay,
        IReadOnlyList<StyleAndTextTuple> fragments,
        int width,
        int height,
        Func<int, IReadOnlyList<StyleAndTextTuple>>? getLine = null);

    /// <summary>The buffer control being rendered.</summary>
    public BufferControl BufferControl { get; }

    /// <summary>The current document state.</summary>
    public Document Document { get; }

    /// <summary>The line number (0-indexed) being transformed.</summary>
    public int LineNumber { get; }

    /// <summary>Position mapping from source to display coordinates,
    /// accounting for all prior processors in the chain.</summary>
    public Func<int, int> SourceToDisplay { get; }

    /// <summary>The input fragments to transform.</summary>
    public IReadOnlyList<StyleAndTextTuple> Fragments { get; }

    /// <summary>Available viewport width.</summary>
    public int Width { get; }

    /// <summary>Available viewport height.</summary>
    public int Height { get; }

    /// <summary>Optional callable to get fragments for another line.</summary>
    public Func<int, IReadOnlyList<StyleAndTextTuple>>? GetLine { get; }

    /// <summary>
    /// Unpack into a tuple for pattern matching.
    /// </summary>
    public (BufferControl BufferControl, Document Document, int LineNumber,
            Func<int, int> SourceToDisplay,
            IReadOnlyList<StyleAndTextTuple> Fragments,
            int Width, int Height) Unpack();
}
```

**Python equivalent**: `TransformationInput.__init__` (lines 94-112) + `unpack()` (lines 114-127).

**Naming changes**:
- `lineno` → `LineNumber` (PascalCase + more descriptive)
- `source_to_display` → `SourceToDisplay`
- `get_line` → `GetLine`
- `fragments` → `Fragments`

---

## Transformation

```csharp
/// <summary>
/// Result of a processor transformation. Contains the transformed fragments
/// and bidirectional position mapping functions.
/// </summary>
/// <remarks>
/// Port of Python Prompt Toolkit's <c>Transformation</c> class.
/// This type is immutable. When position mapping functions are not provided,
/// identity functions (i => i) are used as defaults.
/// </remarks>
public sealed class Transformation
{
    public Transformation(
        IReadOnlyList<StyleAndTextTuple> fragments,
        Func<int, int>? sourceToDisplay = null,
        Func<int, int>? displayToSource = null);

    /// <summary>The transformed fragments.</summary>
    public IReadOnlyList<StyleAndTextTuple> Fragments { get; }

    /// <summary>Source-to-display position mapping. Defaults to identity.</summary>
    public Func<int, int> SourceToDisplay { get; }

    /// <summary>Display-to-source position mapping. Defaults to identity.</summary>
    public Func<int, int> DisplayToSource { get; }
}
```

**Python equivalent**: `Transformation.__init__` (lines 145-153).

**Design notes**:
- Default position mappings are `i => i` (identity), matching Python's `lambda i: i`.
- `Fragments` uses `IReadOnlyList<StyleAndTextTuple>` to match the immutable output contract while allowing `List<StyleAndTextTuple>` or `ExplodedList` to be passed.
