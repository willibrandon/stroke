namespace Stroke.Layout.Processors;

/// <summary>
/// A processor that doesn't do anything. Returns fragments unchanged.
/// </summary>
/// <remarks>
/// Port of Python Prompt Toolkit's <c>DummyProcessor</c> class from
/// <c>prompt_toolkit.layout.processors</c>.
/// </remarks>
public sealed class DummyProcessor : IProcessor
{
    /// <inheritdoc/>
    public Transformation ApplyTransformation(TransformationInput transformationInput)
    {
        return new Transformation(transformationInput.Fragments);
    }
}
