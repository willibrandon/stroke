# Contract: TextObject & TextObjectType

**Feature**: 043-vi-key-bindings
**Namespace**: `Stroke.KeyBinding`
**Date**: 2026-01-31

## TextObjectType Enum

```csharp
namespace Stroke.KeyBinding;

/// <summary>
/// Classifies how a Vi text object's boundary positions are interpreted.
/// </summary>
/// <remarks>
/// Port of Python Prompt Toolkit's <c>TextObjectType</c> from
/// <c>prompt_toolkit.key_binding.bindings.vi</c>.
/// </remarks>
public enum TextObjectType
{
    /// <summary>End position is not included in the range.</summary>
    Exclusive,

    /// <summary>End position is included in the range.</summary>
    Inclusive,

    /// <summary>Full lines from start to end are included.</summary>
    Linewise,

    /// <summary>Rectangular column selection.</summary>
    Block
}
```

## TextObject Class

```csharp
namespace Stroke.KeyBinding;

using Stroke.Clipboard;
using Stroke.Core;

/// <summary>
/// Represents a text region relative to the cursor position, returned by Vi text object
/// and motion handlers.
/// </summary>
/// <remarks>
/// <para>
/// Immutable type. All properties are set at construction time.
/// Thread safety: Inherently thread-safe (immutable).
/// </para>
/// <para>
/// Port of Python Prompt Toolkit's <c>TextObject</c> class from
/// <c>prompt_toolkit.key_binding.bindings.vi</c>.
/// </para>
/// </remarks>
public sealed class TextObject
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TextObject"/> class.
    /// </summary>
    /// <param name="start">Start offset relative to cursor position.</param>
    /// <param name="end">End offset relative to cursor position (default: 0).</param>
    /// <param name="type">How the boundary is interpreted (default: Exclusive).</param>
    public TextObject(int start, int end = 0, TextObjectType type = TextObjectType.Exclusive);

    /// <summary>Gets the start offset relative to cursor position.</summary>
    public int Start { get; }

    /// <summary>Gets the end offset relative to cursor position.</summary>
    public int End { get; }

    /// <summary>Gets how the text object boundary is interpreted.</summary>
    public TextObjectType Type { get; }

    /// <summary>
    /// Gets the <see cref="Core.SelectionType"/> corresponding to this text object's type.
    /// </summary>
    /// <remarks>
    /// Mapping: Exclusive/Inclusive → Characters, Linewise → Lines, Block → Block.
    /// </remarks>
    public SelectionType SelectionType { get; }

    /// <summary>
    /// Returns <see cref="Start"/> and <see cref="End"/> sorted so the first value
    /// is always less than or equal to the second.
    /// </summary>
    /// <returns>A tuple of (min, max) offsets.</returns>
    public (int Start, int End) Sorted();

    /// <summary>
    /// Computes the absolute document positions (from, to) for applying an operator,
    /// adjusting for inclusive/linewise semantics.
    /// </summary>
    /// <param name="document">The document to compute positions against.</param>
    /// <returns>
    /// A tuple of (from, to) absolute positions.
    /// <list type="bullet">
    /// <item><description>Exclusive: from = cursor + min(start, end), to = cursor + max(start, end)</description></item>
    /// <item><description>Inclusive: same as Exclusive but to += 1 (end position IS included)</description></item>
    /// <item><description>Linewise: from/to expanded to cover full line boundaries (start of first line to end of last line)</description></item>
    /// <item><description>Block: same as Exclusive (block handling done by caller)</description></item>
    /// </list>
    /// </returns>
    public (int From, int To) OperatorRange(Document document);

    /// <summary>
    /// Computes the line numbers spanned by this text object.
    /// </summary>
    /// <param name="buffer">The buffer to compute line numbers from.</param>
    /// <returns>A tuple of (startLine, endLine) zero-based line numbers.</returns>
    public (int StartLine, int EndLine) GetLineNumbers(Buffer buffer);

    /// <summary>
    /// Cuts the text covered by this text object from the buffer.
    /// </summary>
    /// <param name="buffer">The buffer to cut from.</param>
    /// <returns>
    /// A tuple of the resulting <see cref="Document"/> and the
    /// <see cref="ClipboardData"/> containing the cut text.
    /// </returns>
    public (Document Document, ClipboardData Data) Cut(Buffer buffer);
}
```

## OperatorFuncDelegate (Updated)

```csharp
namespace Stroke.KeyBinding;

/// <summary>
/// Callback signature for pending Vi operator functions.
/// </summary>
/// <param name="e">The key press event that completes the operator.</param>
/// <param name="textObject">The text object describing the range to operate on.</param>
/// <returns><see cref="NotImplementedOrNone"/> indicating if the event was handled.</returns>
/// <remarks>
/// Port of Python Prompt Toolkit's operator function signature:
/// <c>Callable[[KeyPressEvent, TextObject], None]</c>.
/// </remarks>
public delegate NotImplementedOrNone OperatorFuncDelegate(KeyPressEvent e, TextObject textObject);
```
