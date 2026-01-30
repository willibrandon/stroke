using Stroke.FormattedText;

namespace Stroke.Layout.Processors;

/// <summary>
/// Utility methods for combining processors.
/// </summary>
/// <remarks>
/// Port of Python Prompt Toolkit's <c>merge_processors</c> function from
/// <c>prompt_toolkit.layout.processors</c>.
/// </remarks>
public static class ProcessorUtils
{
    /// <summary>
    /// Merge multiple processors into one.
    /// Returns <see cref="DummyProcessor"/> for empty list, the single processor for length-1,
    /// or a <see cref="MergedProcessor"/> that chains all processors.
    /// </summary>
    /// <param name="processors">The processors to merge.</param>
    /// <returns>A single processor that applies all given processors in sequence.</returns>
    public static IProcessor MergeProcessors(IReadOnlyList<IProcessor> processors)
    {
        if (processors.Count == 0)
            return new DummyProcessor();

        if (processors.Count == 1)
            return processors[0];

        return new MergedProcessor(processors);
    }
}

/// <summary>
/// Internal processor that chains multiple processors sequentially,
/// composing their position mappings.
/// </summary>
/// <remarks>
/// <para>
/// Port of Python Prompt Toolkit's <c>_MergedProcessor</c> class from
/// <c>prompt_toolkit.layout.processors</c>.
/// </para>
/// <para>
/// Named <c>MergedProcessor</c> (without underscore) since C# uses
/// <c>internal</c> visibility instead of Python's name-mangling convention.
/// </para>
/// </remarks>
internal sealed class MergedProcessor : IProcessor
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MergedProcessor"/> class.
    /// </summary>
    /// <param name="processors">The processors to chain.</param>
    public MergedProcessor(IReadOnlyList<IProcessor> processors)
    {
        Processors = processors;
    }

    /// <summary>The chained processors.</summary>
    public IReadOnlyList<IProcessor> Processors { get; }

    /// <inheritdoc/>
    public Transformation ApplyTransformation(TransformationInput ti)
    {
        var sourceToDisplayFunctions = new List<Func<int, int>> { ti.SourceToDisplay };
        var displayToSourceFunctions = new List<Func<int, int>>();
        IReadOnlyList<StyleAndTextTuple> fragments = ti.Fragments;

        // Build a source_to_display closure that chains all accumulated functions.
        int SourceToDisplay(int i)
        {
            foreach (var f in sourceToDisplayFunctions)
            {
                i = f(i);
            }
            return i;
        }

        foreach (var p in Processors)
        {
            var transformation = p.ApplyTransformation(
                new TransformationInput(
                    ti.BufferControl,
                    ti.Document,
                    ti.LineNumber,
                    SourceToDisplay,
                    fragments,
                    ti.Width,
                    ti.Height,
                    ti.GetLine));

            fragments = transformation.Fragments;
            displayToSourceFunctions.Add(transformation.DisplayToSource);
            sourceToDisplayFunctions.Add(transformation.SourceToDisplay);
        }

        int DisplayToSource(int i)
        {
            for (var idx = displayToSourceFunctions.Count - 1; idx >= 0; idx--)
            {
                i = displayToSourceFunctions[idx](i);
            }
            return i;
        }

        // In the case of a nested MergedProcessor, each processor wants to
        // receive a 'source_to_display' function (as part of the
        // TransformationInput) that has everything in the chain before
        // included, because it can be called as part of the
        // apply_transformation function. However, this first
        // source_to_display should not be part of the output that we are
        // returning. (This is the most consistent with display_to_source.)
        sourceToDisplayFunctions.RemoveAt(0);

        return new Transformation(fragments, SourceToDisplay, DisplayToSource);
    }
}
