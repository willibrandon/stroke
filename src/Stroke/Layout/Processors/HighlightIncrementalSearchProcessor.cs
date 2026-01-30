using Stroke.Layout.Controls;

namespace Stroke.Layout.Processors;

/// <summary>
/// Highlight incremental search matches. Uses "incsearch" style class.
/// Reads search text from the search buffer.
/// </summary>
/// <remarks>
/// <para>
/// Important: this requires the <c>previewSearch=true</c> flag to be set for the
/// <see cref="BufferControl"/>. Otherwise, the cursor position won't be set to the search
/// match while searching, and nothing happens.
/// </para>
/// <para>
/// Port of Python Prompt Toolkit's <c>HighlightIncrementalSearchProcessor</c> class from
/// <c>prompt_toolkit.layout.processors</c>.
/// </para>
/// </remarks>
public class HighlightIncrementalSearchProcessor : HighlightSearchProcessor
{
    /// <inheritdoc/>
    protected override string ClassName => "incsearch";

    /// <inheritdoc/>
    protected override string ClassNameCurrent => "incsearch.current";

    /// <inheritdoc/>
    protected override string GetSearchText(BufferControl bufferControl)
    {
        // When the search buffer has focus, take that text.
        var searchBuffer = bufferControl.SearchBuffer;
        if (searchBuffer is not null && !string.IsNullOrEmpty(searchBuffer.Text))
        {
            return searchBuffer.Text;
        }
        return "";
    }
}
