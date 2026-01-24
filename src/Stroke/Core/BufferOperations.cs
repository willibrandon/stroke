using System.Text;
using System.Text.RegularExpressions;

namespace Stroke.Core;

/// <summary>
/// Static helper class containing buffer operations such as indent, unindent, and text reshaping.
/// </summary>
public static class BufferOperations
{
    /// <summary>
    /// Default indentation string (4 spaces).
    /// </summary>
    private const string DefaultIndent = "    ";

    /// <summary>
    /// Default text width for reshaping operations.
    /// </summary>
    private const int DefaultTextWidth = 80;

    // ════════════════════════════════════════════════════════════════════════
    // INDENT OPERATIONS
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Indent text of a Buffer object.
    /// </summary>
    /// <param name="buffer">The buffer to modify.</param>
    /// <param name="fromRow">Starting row (0-indexed, inclusive).</param>
    /// <param name="toRow">Ending row (0-indexed, exclusive).</param>
    /// <param name="count">Number of indentation levels to add.</param>
    public static void Indent(Buffer buffer, int fromRow, int toRow, int count = 1)
    {
        if (buffer == null)
        {
            throw new ArgumentNullException(nameof(buffer));
        }

        if (count <= 0)
        {
            return;
        }

        var currentRow = buffer.Document.CursorPositionRow;
        var currentCol = buffer.Document.CursorPositionCol;
        var lineRange = Enumerable.Range(fromRow, toRow - fromRow);

        // Apply transformation
        var indentContent = string.Concat(Enumerable.Repeat(DefaultIndent, count));
        var newText = buffer.TransformLines(lineRange, line => indentContent + line);

        // Create new document with cursor at start of current row
        var newDoc = new Document(newText);
        var newCursorPos = newDoc.TranslateRowColToIndex(currentRow, 0);

        buffer.SetDocument(new Document(newText, newCursorPos));

        // Place cursor in the same position in text after indenting
        buffer.CursorPosition += currentCol + indentContent.Length;
    }

    /// <summary>
    /// Unindent text of a Buffer object.
    /// </summary>
    /// <param name="buffer">The buffer to modify.</param>
    /// <param name="fromRow">Starting row (0-indexed, inclusive).</param>
    /// <param name="toRow">Ending row (0-indexed, exclusive).</param>
    /// <param name="count">Number of indentation levels to remove.</param>
    public static void Unindent(Buffer buffer, int fromRow, int toRow, int count = 1)
    {
        if (buffer == null)
        {
            throw new ArgumentNullException(nameof(buffer));
        }

        if (count <= 0)
        {
            return;
        }

        var currentRow = buffer.Document.CursorPositionRow;
        var currentCol = buffer.Document.CursorPositionCol;
        var lineRange = Enumerable.Range(fromRow, toRow - fromRow);

        var indentContent = string.Concat(Enumerable.Repeat(DefaultIndent, count));

        string Transform(string text)
        {
            if (text.StartsWith(indentContent, StringComparison.Ordinal))
            {
                return text[indentContent.Length..];
            }
            else
            {
                // Remove all leading whitespace if can't remove full indent
                return text.TrimStart();
            }
        }

        // Apply transformation
        var newText = buffer.TransformLines(lineRange, Transform);

        // Create new document with cursor at start of current row
        var newDoc = new Document(newText);
        var newCursorPos = newDoc.TranslateRowColToIndex(currentRow, 0);

        buffer.SetDocument(new Document(newText, newCursorPos));

        // Place cursor in the same position in text after dedent (clamped to >= 0)
        buffer.CursorPosition += Math.Max(0, currentCol - indentContent.Length);
    }

    // ════════════════════════════════════════════════════════════════════════
    // TEXT RESHAPING (Vi 'gq' operator)
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Reformat text, taking the width into account.
    /// This is the Vi 'gq' operator.
    /// </summary>
    /// <param name="buffer">The buffer to modify.</param>
    /// <param name="fromRow">Starting row (0-indexed, inclusive).</param>
    /// <param name="toRow">Ending row (0-indexed, inclusive - note: unlike other methods, this is INCLUSIVE).</param>
    /// <param name="textWidth">Optional text width. If null, uses buffer's TextWidth or defaults to 80.</param>
    public static void ReshapeText(Buffer buffer, int fromRow, int toRow, int? textWidth = null)
    {
        if (buffer == null)
        {
            throw new ArgumentNullException(nameof(buffer));
        }

        // Get current lines
        var lines = buffer.Text.Split('\n');
        var totalLines = lines.Length;

        // Clamp row indices
        fromRow = Math.Max(0, Math.Min(fromRow, totalLines - 1));
        toRow = Math.Max(fromRow, Math.Min(toRow, totalLines - 1));

        // Split lines into before, to-reformat, and after
        var linesBefore = lines.Take(fromRow).ToList();
        var linesToReformat = lines.Skip(fromRow).Take(toRow - fromRow + 1).ToList();
        var linesAfter = lines.Skip(toRow + 1).ToList();

        if (linesToReformat.Count == 0)
        {
            return;
        }

        // Take indentation from the first line
        var match = Regex.Match(linesToReformat[0], @"^\s*");
        var indent = match.Success ? match.Value : "";

        // Take all the 'words' from the lines to be reshaped
        var allText = string.Join(" ", linesToReformat);
        var words = allText.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);

        if (words.Length == 0)
        {
            return;
        }

        // Reshape with specified width
        var effectiveWidth = textWidth ?? (buffer.TextWidth > 0 ? buffer.TextWidth : DefaultTextWidth);
        var width = effectiveWidth - indent.Length;
        width = Math.Max(10, width); // Minimum width of 10

        var reshapedLines = new List<string>();
        var currentLine = new StringBuilder(indent);
        var currentWidth = 0;

        foreach (var word in words)
        {
            if (currentWidth > 0)
            {
                if (word.Length + currentWidth + 1 > width)
                {
                    // Start new line
                    reshapedLines.Add(currentLine.ToString());
                    currentLine.Clear();
                    currentLine.Append(indent);
                    currentWidth = 0;
                }
                else
                {
                    currentLine.Append(' ');
                    currentWidth += 1;
                }
            }

            currentLine.Append(word);
            currentWidth += word.Length;
        }

        // Add final line
        if (currentLine.Length > 0)
        {
            reshapedLines.Add(currentLine.ToString());
        }

        // Build new text
        var allLines = linesBefore.Concat(reshapedLines).Concat(linesAfter);
        var newText = string.Join("\n", allLines);

        // Calculate cursor position at end of reshaped text
        var beforeText = string.Join("\n", linesBefore);
        var reshapedText = string.Join("\n", reshapedLines);
        var newCursorPos = beforeText.Length;
        if (beforeText.Length > 0)
        {
            newCursorPos += 1; // Account for newline separator
        }
        newCursorPos += reshapedText.Length;

        buffer.SetDocument(new Document(newText, newCursorPos));
    }
}
