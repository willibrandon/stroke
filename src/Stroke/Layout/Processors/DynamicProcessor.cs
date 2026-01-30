namespace Stroke.Layout.Processors;

/// <summary>
/// Processor that dynamically returns a processor at each invocation.
/// </summary>
/// <remarks>
/// Port of Python Prompt Toolkit's <c>DynamicProcessor</c> class from
/// <c>prompt_toolkit.layout.processors</c>.
/// </remarks>
public sealed class DynamicProcessor : IProcessor
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DynamicProcessor"/> class.
    /// </summary>
    /// <param name="getProcessor">Factory callable for the processor.</param>
    public DynamicProcessor(Func<IProcessor?> getProcessor)
    {
        GetProcessor = getProcessor;
    }

    /// <summary>Factory callable for the processor.</summary>
    public Func<IProcessor?> GetProcessor { get; }

    /// <inheritdoc/>
    public Transformation ApplyTransformation(TransformationInput ti)
    {
        var processor = GetProcessor() ?? new DummyProcessor();
        return processor.ApplyTransformation(ti);
    }
}
