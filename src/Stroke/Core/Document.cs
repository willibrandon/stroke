using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Stroke.Core;

/// <summary>
/// An immutable class around text and cursor position, containing methods for
/// querying this data, e.g. to give the text before the cursor.
/// </summary>
/// <remarks>
/// This class is usually instantiated by a Buffer object, and accessed as the
/// Document property of that class.
/// </remarks>
public sealed partial class Document : IEquatable<Document>
{
    #region Static Fields - Word Navigation Regex Patterns

    // Regex for finding "words" in documents. (We consider a group of alnum
    // characters a word, but also a group of special characters a word, as long as
    // it doesn't contain a space.)
    // (This is a 'word' in Vi.)
    private static readonly Regex FindWordRegex = new(@"([a-zA-Z0-9_]+|[^a-zA-Z0-9_\s]+)", RegexOptions.Compiled);
    private static readonly Regex FindCurrentWordRegex = new(@"^([a-zA-Z0-9_]+|[^a-zA-Z0-9_\s]+)", RegexOptions.Compiled);
    private static readonly Regex FindCurrentWordIncludeTrailingWhitespaceRegex = new(@"^(([a-zA-Z0-9_]+|[^a-zA-Z0-9_\s]+)\s*)", RegexOptions.Compiled);

    // Regex for finding "WORDS" in documents.
    // (This is a 'WORD' in Vi.)
    private static readonly Regex FindBigWordRegex = new(@"([^\s]+)", RegexOptions.Compiled);
    private static readonly Regex FindCurrentBigWordRegex = new(@"^([^\s]+)", RegexOptions.Compiled);
    private static readonly Regex FindCurrentBigWordIncludeTrailingWhitespaceRegex = new(@"^([^\s]+\s*)", RegexOptions.Compiled);

    // Alphanumeric alphabet for word boundary detection
    private const string WordAlphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_";

    #endregion

    #region Static Fields - Cache

    /// <summary>
    /// Share the DocumentCache between all Document instances with identical text.
    /// Uses ConditionalWeakTable for automatic cleanup when strings are no longer referenced.
    /// </summary>
    private static readonly ConditionalWeakTable<string, DocumentCache> TextToCache = new();

    #endregion

    #region Instance Fields

    private readonly string _text;
    private readonly int _cursorPosition;
    private readonly SelectionState? _selection;
    private readonly DocumentCache _cache;

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="Document"/> class.
    /// </summary>
    /// <param name="text">The document text.</param>
    /// <param name="cursorPosition">The cursor position. If null, defaults to end of text.</param>
    /// <param name="selection">The selection state.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when cursor position is negative or greater than text length.
    /// </exception>
    public Document(string? text = "", int? cursorPosition = null, SelectionState? selection = null)
    {
        // Handle null text as empty string (IC-017)
        // Normalize line endings: \r\n → \n and bare \r → \n.
        // Python text mode I/O handles this transparently; C# preserves \r\n on Windows.
        _text = (text ?? "").Replace("\r\n", "\n").Replace("\r", "\n");

        // By default, if no cursor position was given, put the cursor at the end
        if (cursorPosition is null)
        {
            _cursorPosition = _text.Length;
        }
        else
        {
            // Validate cursor position (IC-016)
            if (cursorPosition.Value < 0 || cursorPosition.Value > _text.Length)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(cursorPosition),
                    cursorPosition.Value,
                    $"Cursor position must be between 0 and {_text.Length} (text length).");
            }
            _cursorPosition = cursorPosition.Value;
        }

        _selection = selection;

        // Get or create cache for this text (flyweight pattern)
        _cache = TextToCache.GetOrCreateValue(_text);
    }

    #endregion

    #region Basic Properties

    /// <summary>
    /// Gets the document text.
    /// </summary>
    public string Text => _text;

    /// <summary>
    /// Gets the cursor position.
    /// </summary>
    public int CursorPosition => _cursorPosition;

    /// <summary>
    /// Gets the selection state.
    /// </summary>
    public SelectionState? Selection => _selection;

    #endregion

    #region Text Access Properties (User Story 1)

    /// <summary>
    /// Gets the character at cursor position, or '\0' if at end of document.
    /// </summary>
    public char CurrentChar => GetCharRelativeToCursor(0);

    /// <summary>
    /// Gets the character before cursor position, or '\0' if at position 0.
    /// </summary>
    public char CharBeforeCursor => GetCharRelativeToCursor(-1);

    /// <summary>
    /// Gets the text from start to cursor position.
    /// </summary>
    public string TextBeforeCursor => _text[.._cursorPosition];

    /// <summary>
    /// Gets the text from cursor position to end.
    /// </summary>
    public string TextAfterCursor => _text[_cursorPosition..];

    /// <summary>
    /// Gets the text from cursor position to end of current line.
    /// </summary>
    public string CurrentLineAfterCursor
    {
        get
        {
            var text = TextAfterCursor;
            var newlineIndex = text.IndexOf('\n');
            return newlineIndex >= 0 ? text[..newlineIndex] : text;
        }
    }

    /// <summary>
    /// Gets the text from start of current line to cursor position.
    /// </summary>
    public string CurrentLineBeforeCursor
    {
        get
        {
            var text = TextBeforeCursor;
            var newlineIndex = text.LastIndexOf('\n');
            return newlineIndex >= 0 ? text[(newlineIndex + 1)..] : text;
        }
    }

    /// <summary>
    /// Gets the entire current line.
    /// </summary>
    public string CurrentLine => CurrentLineBeforeCursor + CurrentLineAfterCursor;

    /// <summary>
    /// Gets the leading whitespace in the current line.
    /// </summary>
    public string LeadingWhitespaceInCurrentLine
    {
        get
        {
            var currentLine = CurrentLine;
            var length = currentLine.Length - currentLine.TrimStart().Length;
            return currentLine[..length];
        }
    }

    /// <summary>
    /// Gets all lines in the document.
    /// </summary>
    public IReadOnlyList<string> Lines
    {
        get
        {
            if (_cache.Lines is null)
            {
                _cache.Lines = [.. _text.Split('\n')];
            }
            return _cache.Lines.Value;
        }
    }

    /// <summary>
    /// Gets the lines from current line to end of document.
    /// </summary>
    public IReadOnlyList<string> LinesFromCurrent
    {
        get
        {
            var lines = Lines;
            var row = CursorPositionRow;
            var result = new string[lines.Count - row];
            for (int i = row; i < lines.Count; i++)
            {
                result[i - row] = lines[i];
            }
            return result;
        }
    }

    /// <summary>
    /// Gets the number of lines in the document.
    /// </summary>
    public int LineCount => Lines.Count;

    /// <summary>
    /// Gets the current row (0-based).
    /// </summary>
    public int CursorPositionRow
    {
        get
        {
            var (row, _) = FindLineStartIndex(_cursorPosition);
            return row;
        }
    }

    /// <summary>
    /// Gets the current column (0-based).
    /// </summary>
    public int CursorPositionCol
    {
        get
        {
            var (_, lineStartIndex) = FindLineStartIndex(_cursorPosition);
            return _cursorPosition - lineStartIndex;
        }
    }

    /// <summary>
    /// Gets a value indicating whether the cursor is at the end of the document.
    /// </summary>
    public bool IsCursorAtTheEnd => _cursorPosition == _text.Length;

    /// <summary>
    /// Gets a value indicating whether the cursor is at the end of the current line.
    /// </summary>
    public bool IsCursorAtTheEndOfLine
    {
        get
        {
            var currentChar = CurrentChar;
            return currentChar == '\n' || currentChar == '\0';
        }
    }

    /// <summary>
    /// Gets a value indicating whether the cursor is on the first line.
    /// </summary>
    public bool OnFirstLine => CursorPositionRow == 0;

    /// <summary>
    /// Gets a value indicating whether the cursor is on the last line.
    /// </summary>
    public bool OnLastLine => CursorPositionRow == LineCount - 1;

    /// <summary>
    /// Gets the number of empty lines at the end of the document.
    /// </summary>
    public int EmptyLineCountAtTheEnd
    {
        get
        {
            int count = 0;
            var lines = Lines;
            for (int i = lines.Count - 1; i >= 0; i--)
            {
                if (string.IsNullOrWhiteSpace(lines[i]))
                    count++;
                else
                    break;
            }
            return count;
        }
    }

    #endregion

    #region Line Index Computation

    /// <summary>
    /// Gets the array of line start indexes. Computed lazily and cached.
    /// </summary>
    private int[] LineStartIndexes
    {
        get
        {
            if (_cache.LineIndexes is null)
            {
                var lines = Lines;
                var indexes = new int[lines.Count];
                indexes[0] = 0;
                int pos = 0;

                for (int i = 0; i < lines.Count - 1; i++)
                {
                    pos += lines[i].Length + 1; // +1 for newline
                    indexes[i + 1] = pos;
                }

                _cache.LineIndexes = indexes;
            }
            return _cache.LineIndexes;
        }
    }

    /// <summary>
    /// Find the line containing the given index.
    /// </summary>
    /// <param name="index">The character index.</param>
    /// <returns>A tuple of (row, lineStartIndex).</returns>
    private (int Row, int LineStartIndex) FindLineStartIndex(int index)
    {
        var indexes = LineStartIndexes;

        // Binary search for the line containing this index
        int pos = Array.BinarySearch(indexes, index);

        // If exact match, return that position
        if (pos >= 0)
        {
            return (pos, indexes[pos]);
        }

        // BinarySearch returns ~insertionPoint if not found
        // We want the previous line
        pos = ~pos - 1;
        return (pos, indexes[pos]);
    }

    /// <summary>
    /// Translate a character index to (row, col) position.
    /// </summary>
    /// <param name="index">The character index.</param>
    /// <returns>A tuple of (row, col), both 0-based.</returns>
    public (int Row, int Col) TranslateIndexToPosition(int index)
    {
        var (row, rowIndex) = FindLineStartIndex(index);
        var col = index - rowIndex;
        return (row, col);
    }

    /// <summary>
    /// Translate (row, col) position to character index.
    /// </summary>
    /// <param name="row">The row (0-based).</param>
    /// <param name="col">The column (0-based).</param>
    /// <returns>The character index.</returns>
    public int TranslateRowColToIndex(int row, int col)
    {
        var indexes = LineStartIndexes;
        var lines = Lines;

        int result;
        string line;

        if (row < 0)
        {
            result = indexes[0];
            line = lines[0];
        }
        else if (row >= indexes.Length)
        {
            result = indexes[^1];
            line = lines[^1];
        }
        else
        {
            result = indexes[row];
            line = lines[row];
        }

        result += Math.Max(0, Math.Min(col, line.Length));

        // Keep in range
        result = Math.Max(0, Math.Min(result, _text.Length));
        return result;
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Get character relative to cursor position.
    /// </summary>
    private char GetCharRelativeToCursor(int offset)
    {
        int index = _cursorPosition + offset;
        if (index >= 0 && index < _text.Length)
        {
            return _text[index];
        }
        return '\0';
    }

    #endregion

    #region Equality

    /// <summary>
    /// Determines whether this document equals another object.
    /// </summary>
    public override bool Equals(object? obj) => Equals(obj as Document);

    /// <summary>
    /// Determines whether this document equals another document.
    /// </summary>
    public bool Equals(Document? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return _text == other._text
            && _cursorPosition == other._cursorPosition
            && Equals(_selection, other._selection);
    }

    /// <summary>
    /// Gets the hash code for this document.
    /// </summary>
    public override int GetHashCode() => HashCode.Combine(_text, _cursorPosition, _selection);

    /// <summary>
    /// Equality operator.
    /// </summary>
    public static bool operator ==(Document? left, Document? right) =>
        left is null ? right is null : left.Equals(right);

    /// <summary>
    /// Inequality operator.
    /// </summary>
    public static bool operator !=(Document? left, Document? right) => !(left == right);

    #endregion

    #region Object Overrides

    /// <summary>
    /// Returns a string representation of this document.
    /// </summary>
    public override string ToString() => $"Document({_text}, {_cursorPosition})";

    #endregion
}
