using Stroke.Core;
using Stroke.FormattedText;
using Stroke.Layout.Controls;
using AppContext = Stroke.Application.AppContext;

namespace Stroke.Layout.Processors;

/// <summary>
/// Display reverse-i-search prompt around the search buffer.
/// Applied to the SearchBufferControl, not the main input.
/// </summary>
/// <remarks>
/// Port of Python Prompt Toolkit's <c>ReverseSearchProcessor</c> class from
/// <c>prompt_toolkit.layout.processors</c>.
/// </remarks>
public sealed class ReverseSearchProcessor : IProcessor
{
    /// <summary>
    /// Processor types excluded when filtering the main control's input processors.
    /// </summary>
    private static readonly List<Type> ExcludedInputProcessors =
    [
        typeof(HighlightSearchProcessor),
        typeof(HighlightSelectionProcessor),
        typeof(BeforeInput),
        typeof(AfterInput),
    ];

    private BufferControl? GetMainBuffer(BufferControl bufferControl)
    {
        var prevControl = AppContext.GetApp().Layout.SearchTargetBufferControl;
        if (prevControl is BufferControl bc && bc.SearchBufferControl == bufferControl)
        {
            return bc;
        }
        return null;
    }

    private UIContent GetContent(BufferControl mainControl, TransformationInput ti)
    {
        // Emulate the BufferControl through which we are searching.
        // For this we filter out some of the input processors.
        var excludedProcessors = ExcludedInputProcessors;

        IProcessor? FilterProcessor(IProcessor item)
        {
            // For a MergedProcessor, check each individual processor, recursively.
            if (item is MergedProcessor merged)
            {
                var acceptedProcessors = new List<IProcessor>();
                foreach (var p in merged.Processors)
                {
                    var filtered = FilterProcessor(p);
                    if (filtered is not null)
                        acceptedProcessors.Add(filtered);
                }
                return ProcessorUtils.MergeProcessors(acceptedProcessors);
            }

            // For a ConditionalProcessor, check the body.
            if (item is ConditionalProcessor conditional)
            {
                var p = FilterProcessor(conditional.Processor);
                if (p is not null)
                    return new ConditionalProcessor(p, new Filters.FilterOrBool(conditional.Filter));
                return null;
            }

            // Otherwise, check the processor itself.
            foreach (var excluded in excludedProcessors)
            {
                if (excluded.IsInstanceOfType(item))
                    return null;
            }
            return item;
        }

        var filteredProcessor = FilterProcessor(
            ProcessorUtils.MergeProcessors(
                mainControl.InputProcessors ?? Array.Empty<IProcessor>()));

        var highlightProcessor = new HighlightIncrementalSearchProcessor();

        IReadOnlyList<IProcessor> newProcessors = filteredProcessor is not null
            ? [filteredProcessor, highlightProcessor]
            : [highlightProcessor];

        var bufferControl = new BufferControl(
            buffer: mainControl.Buffer,
            inputProcessors: newProcessors,
            includeDefaultInputProcessors: false,
            lexer: mainControl.Lexer,
            previewSearch: true,
            searchBufferControl: (SearchBufferControl)ti.BufferControl);

        return bufferControl.CreateContent(ti.Width, ti.Height, previewSearch: true);
    }

    /// <inheritdoc/>
    public Transformation ApplyTransformation(TransformationInput ti)
    {
        var mainControl = GetMainBuffer(ti.BufferControl);

        if (ti.LineNumber == 0 && mainControl is not null)
        {
            var content = GetContent(mainControl, ti);

            // Get the line from the original document for this search.
            var lineFragments = content.GetLine(content.CursorPosition?.Y ?? 0);

            string directionText;
            if (mainControl.SearchState.Direction == SearchDirection.Forward)
            {
                directionText = "i-search";
            }
            else
            {
                directionText = "reverse-i-search";
            }

            var fragmentsBefore = new List<StyleAndTextTuple>
            {
                new("class:prompt.search", "("),
                new("class:prompt.search", directionText),
                new("class:prompt.search", ")`"),
            };

            var fragments = new List<StyleAndTextTuple>(fragmentsBefore);
            fragments.Add(new StyleAndTextTuple("class:prompt.search.text",
                FormattedTextUtils.FragmentListToText(ti.Fragments)));
            fragments.Add(new StyleAndTextTuple("", "': "));
            fragments.AddRange(lineFragments);

            var shiftPosition = FormattedTextUtils.FragmentListLen(fragmentsBefore);

            return new Transformation(
                fragments,
                sourceToDisplay: i => i + shiftPosition,
                displayToSource: i => i - shiftPosition);
        }
        else
        {
            return new Transformation(ti.Fragments);
        }
    }
}
