using Stroke.Filters;

namespace Stroke.Layout.Processors;

/// <summary>
/// Processor that applies another processor conditionally based on a filter.
/// </summary>
/// <remarks>
/// Port of Python Prompt Toolkit's <c>ConditionalProcessor</c> class from
/// <c>prompt_toolkit.layout.processors</c>.
/// </remarks>
public sealed class ConditionalProcessor : IProcessor
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConditionalProcessor"/> class.
    /// </summary>
    /// <param name="processor">The processor to apply conditionally.</param>
    /// <param name="filter">The filter that controls activation.</param>
    public ConditionalProcessor(IProcessor processor, FilterOrBool filter)
    {
        Processor = processor;
        Filter = FilterUtils.ToFilter(filter);
    }

    /// <summary>The wrapped processor.</summary>
    public IProcessor Processor { get; }

    /// <summary>The activation filter.</summary>
    public IFilter Filter { get; }

    /// <inheritdoc/>
    public Transformation ApplyTransformation(TransformationInput transformationInput)
    {
        // Run processor when enabled.
        if (Filter.Invoke())
        {
            return Processor.ApplyTransformation(transformationInput);
        }
        else
        {
            return new Transformation(transformationInput.Fragments);
        }
    }

    /// <summary>Returns "ConditionalProcessor(processor={Processor}, filter={Filter})".</summary>
    public override string ToString()
    {
        return $"ConditionalProcessor(processor={Processor}, filter={Filter})";
    }
}
