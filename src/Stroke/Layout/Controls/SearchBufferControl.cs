using Stroke.Core;
using Stroke.Filters;
using Stroke.KeyBinding;
using Stroke.Lexers;

// Alias to avoid ambiguity with System.Buffer
using Buffer = Stroke.Core.Buffer;

namespace Stroke.Layout.Controls;

/// <summary>
/// Specialized BufferControl for search input.
/// </summary>
/// <remarks>
/// <para>
/// SearchBufferControl extends BufferControl with additional search-specific
/// functionality, including an IgnoreCase filter and a reference to the
/// associated SearchState for highlighting matches.
/// </para>
/// <para>
/// Port of Python Prompt Toolkit's <c>SearchBufferControl</c> class from <c>layout/controls.py</c>.
/// </para>
/// </remarks>
public class SearchBufferControl : BufferControl
{
    /// <summary>
    /// Gets the filter that determines whether search is case-insensitive.
    /// </summary>
    public IFilter IgnoreCase { get; }

    /// <summary>
    /// Gets the SearchState associated with this search control.
    /// </summary>
    public SearchState? SearcherSearchState { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SearchBufferControl"/> class.
    /// </summary>
    /// <param name="buffer">The search buffer, or null to create one.</param>
    /// <param name="ignoreCase">Filter controlling case-insensitive search.</param>
    /// <param name="searcherSearchState">The associated SearchState.</param>
    /// <param name="lexer">Optional lexer for syntax highlighting.</param>
    /// <param name="focusable">Whether the control is focusable. Default is false for search controls.</param>
    /// <param name="keyBindings">Optional key bindings.</param>
    public SearchBufferControl(
        Buffer? buffer = null,
        FilterOrBool ignoreCase = default,
        SearchState? searcherSearchState = null,
        ILexer? lexer = null,
        FilterOrBool focusable = default,
        IKeyBindingsBase? keyBindings = null)
        : base(
            buffer: buffer,
            lexer: lexer,
            focusable: focusable.HasValue ? focusable : new FilterOrBool(false),
            keyBindings: keyBindings)
    {
        IgnoreCase = ignoreCase.HasValue
            ? FilterUtils.ToFilter(ignoreCase)
            : Never.Instance;
        SearcherSearchState = searcherSearchState;
    }

    /// <summary>
    /// Returns a string representation.
    /// </summary>
    public override string ToString()
    {
        return "SearchBufferControl(...)";
    }
}
