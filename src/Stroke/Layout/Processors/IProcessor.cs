namespace Stroke.Layout.Processors;

/// <summary>
/// Manipulate the fragments for a given line in a
/// <see cref="Controls.BufferControl"/>.
/// </summary>
/// <remarks>
/// <para>
/// Port of Python Prompt Toolkit's <c>Processor</c> abstract class from
/// <c>prompt_toolkit.layout.processors</c>.
/// </para>
/// </remarks>
public interface IProcessor
{
    /// <summary>
    /// Apply transformation to the given input fragments.
    /// </summary>
    /// <param name="transformationInput">The transformation input containing
    /// buffer control, document, line number, fragments, and context.</param>
    /// <returns>A <see cref="Transformation"/> with transformed fragments
    /// and position mapping functions.</returns>
    Transformation ApplyTransformation(TransformationInput transformationInput);
}
