# Contract: Processor Interfaces

**Feature**: 031-input-processors
**Namespace**: `Stroke.Layout.Processors`
**Python Source**: `prompt_toolkit/layout/processors.py` (lines 58-77)

## IProcessor Interface

```csharp
/// <summary>
/// Manipulate the fragments for a given line in a BufferControl.
/// </summary>
/// <remarks>
/// Port of Python Prompt Toolkit's <c>Processor</c> abstract class from
/// <c>prompt_toolkit.layout.processors</c>.
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
```

**Python equivalent**: `Processor` abstract class with `apply_transformation(self, transformation_input: TransformationInput) -> Transformation`

**Design notes**:
- Python uses an abstract class (`ABCMeta`). C# uses an interface because there is no shared state or default implementation in the base class.
- The Python base class has a default implementation that returns `Transformation(transformation_input.fragments)`, but since it's abstract, it's never called. C# omits this.

## Delegate Type Aliases

```csharp
// These are not distinct delegate types, but documented conventions:
// SourceToDisplay = Func<int, int>  — maps source column → display column
// DisplayToSource = Func<int, int>  — maps display column → source column
```

**Python equivalent**: `SourceToDisplay = Callable[[int], int]` and `DisplayToSource = Callable[[int], int]` (type aliases at module level, lines 76-77).

**Design notes**:
- Python defines these as module-level type aliases. C# does not need separate delegate types since `Func<int, int>` suffices. The names are used in XML documentation for clarity.
