using Stroke.Clipboard;
using Stroke.Core;

namespace Stroke.KeyBinding;

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
    public TextObject(int start, int end = 0, TextObjectType type = TextObjectType.Exclusive)
    {
        Start = start;
        End = end;
        Type = type;
    }

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
    public SelectionType SelectionType => Type switch
    {
        TextObjectType.Linewise => SelectionType.Lines,
        TextObjectType.Block => SelectionType.Block,
        _ => SelectionType.Characters,
    };

    /// <summary>
    /// Returns <see cref="Start"/> and <see cref="End"/> sorted so the first value
    /// is always less than or equal to the second.
    /// </summary>
    /// <returns>A tuple of (min, max) offsets.</returns>
    public (int Start, int End) Sorted()
    {
        if (Start < End)
            return (Start, End);
        return (End, Start);
    }

    /// <summary>
    /// Computes the absolute document positions (from, to) for applying an operator,
    /// adjusting for inclusive/linewise semantics.
    /// </summary>
    /// <param name="document">The document to compute positions against.</param>
    /// <returns>
    /// A tuple of (from, to) absolute positions.
    /// <list type="bullet">
    /// <item><description>Exclusive: from = cursor + min(start, end), to = cursor + max(start, end).
    /// If the end of motion is on the first column, end position becomes end of previous line.</description></item>
    /// <item><description>Inclusive: same as Exclusive but to += 1 (end position IS included)</description></item>
    /// <item><description>Linewise: from/to expanded to cover full line boundaries (start of first line to end of last line)</description></item>
    /// <item><description>Block: same as Exclusive (block handling done by caller)</description></item>
    /// </list>
    /// </returns>
    public (int From, int To) OperatorRange(Document document)
    {
        var (start, end) = Sorted();

        if (Type == TextObjectType.Exclusive
            && document.TranslateIndexToPosition(end + document.CursorPosition).Col == 0)
        {
            // If the motion is exclusive and the end of motion is on the first
            // column, the end position becomes end of previous line.
            end -= 1;
        }

        if (Type == TextObjectType.Inclusive)
        {
            end += 1;
        }

        if (Type == TextObjectType.Linewise)
        {
            // Select whole lines.
            var (startRow, _) = document.TranslateIndexToPosition(start + document.CursorPosition);
            start = document.TranslateRowColToIndex(startRow, 0) - document.CursorPosition;

            var (endRow, _) = document.TranslateIndexToPosition(end + document.CursorPosition);
            end = document.TranslateRowColToIndex(endRow, document.Lines[endRow].Length)
                  - document.CursorPosition;
        }

        return (start, end);
    }

    /// <summary>
    /// Computes the line numbers spanned by this text object.
    /// </summary>
    /// <param name="buffer">The buffer to compute line numbers from.</param>
    /// <returns>A tuple of (startLine, endLine) zero-based line numbers.</returns>
    public (int StartLine, int EndLine) GetLineNumbers(Core.Buffer buffer)
    {
        // Get absolute cursor positions from the text object.
        var (from, to) = OperatorRange(buffer.Document);
        from += buffer.CursorPosition;
        to += buffer.CursorPosition;

        // Take the start of the lines.
        var (fromLine, _) = buffer.Document.TranslateIndexToPosition(from);
        var (toLine, _) = buffer.Document.TranslateIndexToPosition(to);

        return (fromLine, toLine);
    }

    /// <summary>
    /// Cuts the text covered by this text object from the buffer.
    /// </summary>
    /// <param name="buffer">The buffer to cut from.</param>
    /// <returns>
    /// A tuple of the resulting <see cref="Document"/> and the
    /// <see cref="ClipboardData"/> containing the cut text.
    /// </returns>
    public (Document Document, ClipboardData Data) Cut(Core.Buffer buffer)
    {
        var (from, to) = OperatorRange(buffer.Document);

        from += buffer.CursorPosition;
        to += buffer.CursorPosition;

        if (Type == TextObjectType.Linewise)
        {
            // For linewise cuts, include the trailing newline so the entire
            // line (including its terminator) is removed. In Python, this
            // happens because selection_ranges() adds +1 for Vi mode after
            // the LINES boundary adjustment, and cut() intentionally does NOT
            // subtract 1 for linewise. Since our SelectionRanges() doesn't
            // have the Vi mode +1, we compute the linewise cut directly.
            if (to < buffer.Text.Length)
                to += 1;

            var cutText = buffer.Text[from..to];
            var remaining = string.Concat(
                buffer.Text.AsSpan(0, from),
                buffer.Text.AsSpan(to));

            // Strip trailing newline from clipboard data (matches Python's
            // CutSelection behavior for LINES selections).
            if (cutText.EndsWith('\n'))
                cutText = cutText[..^1];

            var newCursor = Math.Min(from, remaining.Length);
            return (
                new Document(remaining, newCursor),
                new ClipboardData(cutText, SelectionType.Lines));
        }

        // For non-linewise types, Python's cut() subtracts 1 from 'to' and
        // selection_ranges() adds +1 for Vi mode, netting to zero. Since our
        // SelectionRanges() doesn't add +1, we also skip the subtraction —
        // the OperatorRange result is already the correct exclusive upper bound.
        var document = new Document(
            buffer.Text,
            to,
            new SelectionState(originalCursorPosition: from, type: SelectionType));

        var (newDocument, clipboardData) = document.CutSelection();
        return (newDocument, clipboardData);
    }
}
