using Stroke.Application;
using Stroke.Core;
using Stroke.Filters;
using Stroke.FormattedText;
using Stroke.Layout;
using Stroke.Layout.Containers;
using Stroke.Layout.Controls;
using Stroke.Layout.Processors;
using Stroke.Lexers;

using AppContext = Stroke.Application.AppContext;

// Alias to avoid ambiguity with System.Buffer
using Buffer = Stroke.Core.Buffer;

namespace Stroke.Widgets.Toolbars;

/// <summary>
/// Toolbar for incremental search input with direction-aware prompts.
/// </summary>
/// <remarks>
/// <para>
/// Displays "I-search: " / "I-search backward: " in Emacs mode, or "/" / "?" in Vi mode.
/// Visible only when the search control is registered in the layout's search links.
/// </para>
/// <para>
/// Port of Python Prompt Toolkit's <c>SearchToolbar</c> from <c>widgets/toolbars.py</c>.
/// </para>
/// </remarks>
public class SearchToolbar : IMagicContainer
{
    /// <summary>Gets the search buffer.</summary>
    public Buffer SearchBuffer { get; }

    /// <summary>Gets the search buffer control.</summary>
    public SearchBufferControl Control { get; }

    /// <summary>Gets the conditional container (visible when searching).</summary>
    public ConditionalContainer Container { get; }

    /// <summary>
    /// Initializes a new SearchToolbar.
    /// </summary>
    /// <param name="searchBuffer">Buffer for search input. Creates new if null.</param>
    /// <param name="viMode">If true, use "/" and "?" prompts instead of "I-search".</param>
    /// <param name="textIfNotSearching">Text to show when not actively searching.</param>
    /// <param name="forwardSearchPrompt">Prompt for forward search. Default: "I-search: ".</param>
    /// <param name="backwardSearchPrompt">Prompt for backward search. Default: "I-search backward: ".</param>
    /// <param name="ignoreCase">Filter controlling case-insensitive search. Default: false.</param>
    public SearchToolbar(
        Buffer? searchBuffer = null,
        bool viMode = false,
        AnyFormattedText textIfNotSearching = default,
        AnyFormattedText forwardSearchPrompt = default,
        AnyFormattedText backwardSearchPrompt = default,
        FilterOrBool ignoreCase = default)
    {
        searchBuffer ??= new Buffer();
        SearchBuffer = searchBuffer;

        // Default prompts
        if (textIfNotSearching.IsEmpty)
            textIfNotSearching = "";
        if (forwardSearchPrompt.IsEmpty)
            forwardSearchPrompt = "I-search: ";
        if (backwardSearchPrompt.IsEmpty)
            backwardSearchPrompt = "I-search backward: ";

        // Capture for closure (avoid capturing 'this' before initialization)
        SearchBufferControl? controlRef = null;

        var isSearching = new Condition(() =>
            controlRef != null &&
            AppContext.GetApp().Layout.SearchLinks.ContainsKey(controlRef));

        // Capture prompts for the closure
        var fwdPrompt = forwardSearchPrompt;
        var bwdPrompt = backwardSearchPrompt;
        var notSearchingText = textIfNotSearching;
        var isVi = viMode;

        Func<AnyFormattedText> getBeforeInput = () =>
        {
            if (!isSearching.Invoke())
                return notSearchingText;
            else if (controlRef?.SearcherSearchState?.Direction == SearchDirection.Backward)
                return isVi ? "?" : bwdPrompt;
            else
                return isVi ? "/" : fwdPrompt;
        };

        Control = new SearchBufferControl(
            buffer: searchBuffer,
            inputProcessors: [new BeforeInput(getBeforeInput, style: "class:search-toolbar.prompt")],
            lexer: new SimpleLexer(style: "class:search-toolbar.text"),
            ignoreCase: ignoreCase);

        // Now set the reference for the closure
        controlRef = Control;

        Container = new ConditionalContainer(
            content: new AnyContainer(new Window(
                content: Control,
                height: new Dimension(preferred: 1),
                style: "class:search-toolbar")),
            filter: new FilterOrBool(isSearching));
    }

    /// <inheritdoc/>
    public IContainer PtContainer() => Container;
}
