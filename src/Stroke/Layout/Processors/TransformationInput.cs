using Stroke.Core;
using Stroke.FormattedText;
using Stroke.Layout.Controls;

namespace Stroke.Layout.Processors;

/// <summary>
/// Input data for a processor transformation. Contains the rendering context
/// for a single line being processed.
/// </summary>
/// <remarks>
/// <para>
/// Port of Python Prompt Toolkit's <c>TransformationInput</c> class from
/// <c>prompt_toolkit.layout.processors</c>.
/// </para>
/// <para>
/// This type is immutable — all properties are set at construction time.
/// </para>
/// </remarks>
public sealed class TransformationInput
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TransformationInput"/> class.
    /// </summary>
    /// <param name="bufferControl">The buffer control being rendered.</param>
    /// <param name="document">The current document state.</param>
    /// <param name="lineNumber">The line number (0-indexed) being transformed.</param>
    /// <param name="sourceToDisplay">Position mapping from source to display coordinates,
    /// accounting for all prior processors in the chain.</param>
    /// <param name="fragments">The input fragments to transform.</param>
    /// <param name="width">Available viewport width.</param>
    /// <param name="height">Available viewport height.</param>
    /// <param name="getLine">Optional callable to get fragments for another line.</param>
    public TransformationInput(
        BufferControl bufferControl,
        Document document,
        int lineNumber,
        Func<int, int> sourceToDisplay,
        IReadOnlyList<StyleAndTextTuple> fragments,
        int width,
        int height,
        Func<int, IReadOnlyList<StyleAndTextTuple>>? getLine = null)
    {
        BufferControl = bufferControl;
        Document = document;
        LineNumber = lineNumber;
        SourceToDisplay = sourceToDisplay;
        Fragments = fragments;
        Width = width;
        Height = height;
        GetLine = getLine;
    }

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
    /// <returns>A tuple containing the key properties of this input.</returns>
    public (BufferControl BufferControl, Document Document, int LineNumber,
            Func<int, int> SourceToDisplay,
            IReadOnlyList<StyleAndTextTuple> Fragments,
            int Width, int Height) Unpack()
    {
        return (BufferControl, Document, LineNumber, SourceToDisplay, Fragments, Width, Height);
    }
}
