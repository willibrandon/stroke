using Stroke.Application;
using Stroke.Completion;
using Stroke.Core;
using Stroke.Filters;
using Stroke.FormattedText;
using Stroke.Input;
using Stroke.KeyBinding;
using Stroke.Layout;
using Stroke.Layout.Containers;
using Stroke.Layout.Controls;
using Stroke.Layout.Menus;
using Stroke.Layout.Processors;
using Stroke.Layout.Windows;
using Stroke.Lexers;
using Stroke.Rendering;
using Stroke.Widgets.Toolbars;

using Buffer = Stroke.Core.Buffer;

namespace Stroke.Shortcuts;

public partial class PromptSession<TResult>
{
    /// <summary>
    /// Creates the prompt layout with all containers, floats, and toolbars.
    /// </summary>
    /// <returns>The complete prompt layout.</returns>
    /// <remarks>
    /// Port of Python Prompt Toolkit's <c>_create_layout</c>.
    /// </remarks>
    private Layout.Layout CreateLayout()
    {
        // Split multiline prompt text into above/inline portions.
        var (hasBeforeFragments, getBeforeFragments, getFirstInputLine) =
            SplitMultilinePrompt(GetPrompt);

        var defaultBuffer = DefaultBuffer;
        var searchBuffer = SearchBuffer;

        // Placeholder condition
        var displayPlaceholder = new Condition(() =>
            Placeholder is not null && defaultBuffer.Document.Text == "");

        // Build input processors list (7 processors per FR-006).
        var allInputProcessors = new IProcessor[]
        {
            new HighlightIncrementalSearchProcessor(),
            new HighlightSelectionProcessor(),
            new ConditionalProcessor(
                new AppendAutoSuggestion(),
                new FilterOrBool(
                    ((Filter)AppFilters.HasFocus(defaultBuffer))
                        .And(((Filter)AppFilters.IsDone).Invert()))),
            new ConditionalProcessor(
                new PasswordProcessor(),
                DynCond(() => IsPassword)),
            new DisplayMultipleCursors(),
            // User-inserted processors (dynamic)
            new DynamicProcessor(() =>
            {
                var procs = InputProcessors;
                return procs is not null && procs.Count > 0
                    ? ProcessorUtils.MergeProcessors(procs)
                    : null;
            }),
            new ConditionalProcessor(
                new AfterInput((AnyFormattedText)((Func<AnyFormattedText>)(() =>
                {
                    var p = Placeholder;
                    return p is not null
                        ? FormattedTextUtils.ToFormattedText(p.Value)
                        : FormattedText.FormattedText.Empty;
                }))),
                new FilterOrBool(displayPlaceholder)),
        };

        // Bottom toolbar
        var bottomToolbar = new ConditionalContainer(
            new AnyContainer(new Window(
                content: new FormattedTextControl(
                    () => FormattedTextUtils.ToFormattedText(BottomToolbar, style: "class:bottom-toolbar.text")),
                style: "class:bottom-toolbar",
                dontExtendHeight: true,
                height: new Dimension(min: 1))),
            filter: new FilterOrBool(
                ((Filter)new Condition(() => BottomToolbar.Value is not null))
                    .And(((Filter)AppFilters.IsDone).Invert())
                    .And((Filter)AppFilters.RendererHeightIsKnown)));

        // Search toolbar and control
        var searchToolbar = new SearchToolbar(
            searchBuffer: searchBuffer,
            ignoreCase: DynCond(() => SearchIgnoreCase));

        var searchBufferControl = new SearchBufferControl(
            buffer: searchBuffer,
            inputProcessors: [new ReverseSearchProcessor()],
            ignoreCase: DynCond(() => SearchIgnoreCase));

        // Function to get search buffer control based on multiline mode
        SearchBufferControl GetSearchBufferControl()
        {
            if (FilterUtils.ToFilter(Multiline).Invoke())
                return searchToolbar.Control;
            return searchBufferControl;
        }

        // System toolbar
        var systemToolbar = new SystemToolbar(
            enableGlobalBindings: DynCond(() => EnableSystemPrompt));

        // Main buffer control
        var defaultBufferControl = new BufferControl(
            buffer: defaultBuffer,
            searchBufferControlFactory: GetSearchBufferControl,
            inputProcessors: allInputProcessors,
            includeDefaultInputProcessors: false,
            lexer: new DynamicLexer(() => Lexer),
            previewSearch: true);

        // Main buffer window
        var defaultBufferWindow = new Window(
            content: defaultBufferControl,
            heightGetter: GetDefaultBufferControlHeight,
            getLinePrefix: (lineNumber, wrapCount) =>
                GetLinePrefix(lineNumber, wrapCount, getFirstInputLine),
            wrapLines: DynCond(() => WrapLines));

        // Multi-column complete style condition
        var multiColumnCompleteStyle = new Condition(() =>
            CompleteStyle == CompleteStyle.MultiColumn);

        // Main input area with completion menus floating on top
        var mainInputContainer = new FloatContainer(
            new AnyContainer(new HSplit(
                [
                    // Multiline prompt area above input (text before last \n)
                    new ConditionalContainer(
                        new AnyContainer(new Window(
                            content: new FormattedTextControl(getBeforeFragments),
                            dontExtendHeight: true)),
                        filter: new FilterOrBool(new Condition(hasBeforeFragments))),
                    // Default buffer (shown when not in non-multiline search)
                    new ConditionalContainer(
                        new AnyContainer(defaultBufferWindow),
                        filter: new FilterOrBool(new Condition(() =>
                            Application.AppContext.GetApp().Layout.CurrentControl
                                != (IUIControl)searchBufferControl))),
                    // Search buffer control (shown when in non-multiline search)
                    new ConditionalContainer(
                        new AnyContainer(new Window(content: searchBufferControl)),
                        filter: new FilterOrBool(new Condition(() =>
                            Application.AppContext.GetApp().Layout.CurrentControl
                                == (IUIControl)searchBufferControl))),
                ])),
            floats:
            [
                // Single-column completion menu
                new Float(
                    xcursor: true,
                    ycursor: true,
                    transparent: true,
                    content: new AnyContainer(new CompletionsMenu(
                        maxHeight: 16,
                        scrollOffset: 1,
                        extraFilter: new FilterOrBool(
                            ((Filter)AppFilters.HasFocus(defaultBuffer))
                                .And(((Filter)multiColumnCompleteStyle).Invert()))))),
                // Multi-column completion menu
                new Float(
                    xcursor: true,
                    ycursor: true,
                    transparent: true,
                    content: new AnyContainer(new MultiColumnCompletionsMenu(
                        showMeta: true,
                        extraFilter: new FilterOrBool(
                            ((Filter)AppFilters.HasFocus(defaultBuffer))
                                .And((Filter)multiColumnCompleteStyle))))),
                // Right prompt
                new Float(
                    right: 0,
                    top: 0,
                    hideWhenCoveringContent: true,
                    content: new AnyContainer(new RPromptWindow(() => RPrompt))),
            ]);

        // Assemble the full layout HSplit
        var layout = new HSplit(
            [
                // Wrap main input in a frame if requested
                new ConditionalContainer(
                    new AnyContainer(new Widgets.Base.Frame(new AnyContainer(mainInputContainer))),
                    filter: DynCond(() => ShowFrame),
                    alternativeContent: new AnyContainer(mainInputContainer)),
                // Validation toolbar
                new ConditionalContainer(
                    new AnyContainer(new ValidationToolbar()),
                    filter: new FilterOrBool(((Filter)AppFilters.IsDone).Invert())),
                // System toolbar
                new ConditionalContainer(
                    new AnyContainer(systemToolbar),
                    filter: new FilterOrBool(
                        ((Filter)DynCond(() => EnableSystemPrompt))
                            .And(((Filter)AppFilters.IsDone).Invert()))),
                // Arg toolbar (multiline mode)
                new ConditionalContainer(
                    new AnyContainer(new Window(
                        content: new FormattedTextControl(GetArgText),
                        height: Dimension.Exact(1))),
                    filter: new FilterOrBool(
                        ((Filter)DynCond(() => Multiline))
                            .And((Filter)AppFilters.HasArg))),
                // Search toolbar (multiline mode)
                new ConditionalContainer(
                    new AnyContainer(searchToolbar),
                    filter: new FilterOrBool(
                        ((Filter)DynCond(() => Multiline))
                            .And(((Filter)AppFilters.IsDone).Invert()))),
                // Bottom toolbar
                bottomToolbar,
            ]);

        return new Layout.Layout(new AnyContainer(layout), new FocusableElement(defaultBufferWindow));
    }

    /// <summary>
    /// Gets the height for the default buffer control,
    /// reserving space for the completion menu when appropriate.
    /// </summary>
    private Dimension? GetDefaultBufferControlHeight()
    {
        int space;
        if (Completer is not null && CompleteStyle != CompleteStyle.ReadlineLike)
        {
            space = ReserveSpaceForMenu;
        }
        else
        {
            space = 0;
        }

        if (space > 0 && !Application.AppContext.GetApp().IsDone)
        {
            var buff = DefaultBuffer;
            // Reserve space when completions exist or expected soon
            if (buff.CompleteWhileTypingFilter() || buff.CompleteState is not null)
            {
                return new Dimension(min: space);
            }
        }

        return new Dimension();
    }

    // ═══════════════════════════════════════════════════════════════════
    // SPLIT MULTILINE PROMPT
    // ═══════════════════════════════════════════════════════════════════

    /// <summary>
    /// Splits a prompt text at newlines for multiline rendering.
    /// Returns functions for: has-before-fragments, before-fragments, first-input-line.
    /// </summary>
    /// <remarks>
    /// Port of Python Prompt Toolkit's <c>_split_multiline_prompt</c>.
    /// Uses <see cref="LayoutUtils.ExplodeTextFragments"/> for accurate newline detection.
    /// </remarks>
    internal static (
        Func<bool> HasBeforeFragments,
        Func<IReadOnlyList<StyleAndTextTuple>> Before,
        Func<IReadOnlyList<StyleAndTextTuple>> FirstInputLine
    ) SplitMultilinePrompt(Func<IReadOnlyList<StyleAndTextTuple>> getPromptText)
    {
        bool HasBefore()
        {
            foreach (var fragment in getPromptText())
            {
                if (fragment.Text.Contains('\n'))
                    return true;
            }
            return false;
        }

        IReadOnlyList<StyleAndTextTuple> Before()
        {
            var result = new List<StyleAndTextTuple>();
            bool foundNl = false;
            var exploded = LayoutUtils.ExplodeTextFragments(getPromptText());
            for (int i = exploded.Count - 1; i >= 0; i--)
            {
                var (style, text) = (exploded[i].Style, exploded[i].Text);
                if (foundNl)
                {
                    result.Insert(0, new StyleAndTextTuple(style, text));
                }
                else if (text == "\n")
                {
                    foundNl = true;
                }
            }
            return result;
        }

        IReadOnlyList<StyleAndTextTuple> FirstInputLine()
        {
            var result = new List<StyleAndTextTuple>();
            var exploded = LayoutUtils.ExplodeTextFragments(getPromptText());
            for (int i = exploded.Count - 1; i >= 0; i--)
            {
                var (style, text) = (exploded[i].Style, exploded[i].Text);
                if (text == "\n")
                    break;
                result.Insert(0, new StyleAndTextTuple(style, text));
            }
            return result;
        }

        return (HasBefore, Before, FirstInputLine);
    }

    // ═══════════════════════════════════════════════════════════════════
    // RPROMPT WINDOW
    // ═══════════════════════════════════════════════════════════════════

    /// <summary>
    /// Window that displays right-aligned prompt text.
    /// </summary>
    /// <remarks>
    /// Port of Python Prompt Toolkit's <c>_RPrompt</c> class.
    /// </remarks>
    internal sealed class RPromptWindow : Window
    {
        /// <summary>
        /// Creates a new right-prompt window.
        /// </summary>
        /// <param name="text">Callable returning the right prompt text.</param>
        public RPromptWindow(Func<AnyFormattedText> text)
            : base(
                content: new FormattedTextControl(
                    () => FormattedTextUtils.ToFormattedText(text())),
                align: WindowAlign.Right,
                style: "class:rprompt")
        {
        }
    }
}
