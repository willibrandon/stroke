# Feature 30: Input Processors

## Overview

Implement the processor system that transforms fragments before BufferControl renders them to the screen. Processors can insert content, highlight text, and transform the display.

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/layout/processors.py`

## Public API

### TransformationInput Class

```csharp
namespace Stroke.Layout;

/// <summary>
/// Input to a processor transformation.
/// </summary>
public sealed class TransformationInput
{
    /// <summary>
    /// Creates a TransformationInput.
    /// </summary>
    public TransformationInput(
        BufferControl bufferControl,
        Document document,
        int lineno,
        Func<int, int> sourceToDisplay,
        StyleAndTextTuples fragments,
        int width,
        int height,
        Func<int, StyleAndTextTuples>? getLine = null);

    /// <summary>
    /// The BufferControl being rendered.
    /// </summary>
    public BufferControl BufferControl { get; }

    /// <summary>
    /// The document being rendered.
    /// </summary>
    public Document Document { get; }

    /// <summary>
    /// The line number being processed.
    /// </summary>
    public int LineNo { get; }

    /// <summary>
    /// Function to convert source position to display position.
    /// </summary>
    public Func<int, int> SourceToDisplay { get; }

    /// <summary>
    /// The fragments to transform.
    /// </summary>
    public StyleAndTextTuples Fragments { get; }

    /// <summary>
    /// The available width.
    /// </summary>
    public int Width { get; }

    /// <summary>
    /// The available height.
    /// </summary>
    public int Height { get; }

    /// <summary>
    /// Function to get fragments from another line.
    /// </summary>
    public Func<int, StyleAndTextTuples>? GetLine { get; }
}
```

### Transformation Class

```csharp
namespace Stroke.Layout;

/// <summary>
/// Transformation result from a processor.
/// </summary>
public sealed class Transformation
{
    /// <summary>
    /// Creates a Transformation.
    /// </summary>
    /// <param name="fragments">The transformed fragments.</param>
    /// <param name="sourceToDisplay">Cursor position transformation from source to display.</param>
    /// <param name="displayToSource">Cursor position transformation from display to source.</param>
    public Transformation(
        StyleAndTextTuples fragments,
        Func<int, int>? sourceToDisplay = null,
        Func<int, int>? displayToSource = null);

    /// <summary>
    /// The transformed fragments.
    /// </summary>
    public StyleAndTextTuples Fragments { get; }

    /// <summary>
    /// Source to display position transformer.
    /// </summary>
    public Func<int, int> SourceToDisplay { get; }

    /// <summary>
    /// Display to source position transformer.
    /// </summary>
    public Func<int, int> DisplayToSource { get; }
}
```

### IProcessor Interface

```csharp
namespace Stroke.Layout;

/// <summary>
/// Interface for fragment processors.
/// </summary>
public interface IProcessor
{
    /// <summary>
    /// Apply transformation to the input.
    /// </summary>
    Transformation ApplyTransformation(TransformationInput input);
}
```

### DummyProcessor Class

```csharp
namespace Stroke.Layout;

/// <summary>
/// A processor that doesn't do anything.
/// </summary>
public sealed class DummyProcessor : IProcessor
{
    public Transformation ApplyTransformation(TransformationInput input);
}
```

### HighlightSearchProcessor Class

```csharp
namespace Stroke.Layout;

/// <summary>
/// Processor that highlights search matches in the document.
/// Applies 'search' and 'search.current' style classes.
/// </summary>
public class HighlightSearchProcessor : IProcessor
{
    public Transformation ApplyTransformation(TransformationInput input);
}
```

### HighlightIncrementalSearchProcessor Class

```csharp
namespace Stroke.Layout;

/// <summary>
/// Processor that highlights incremental search matches.
/// Applies 'incsearch' and 'incsearch.current' style classes.
/// </summary>
public sealed class HighlightIncrementalSearchProcessor : HighlightSearchProcessor
{
    public override Transformation ApplyTransformation(TransformationInput input);
}
```

### HighlightSelectionProcessor Class

```csharp
namespace Stroke.Layout;

/// <summary>
/// Processor that highlights the selection in the document.
/// Applies 'selected' style class.
/// </summary>
public sealed class HighlightSelectionProcessor : IProcessor
{
    public Transformation ApplyTransformation(TransformationInput input);
}
```

### PasswordProcessor Class

```csharp
namespace Stroke.Layout;

/// <summary>
/// Processor that masks the input for passwords.
/// </summary>
public sealed class PasswordProcessor : IProcessor
{
    /// <summary>
    /// Creates a PasswordProcessor.
    /// </summary>
    /// <param name="char">Character to display instead of actual input.</param>
    public PasswordProcessor(string @char = "*");

    public string Char { get; }

    public Transformation ApplyTransformation(TransformationInput input);
}
```

### HighlightMatchingBracketProcessor Class

```csharp
namespace Stroke.Layout;

/// <summary>
/// Processor that highlights matching brackets.
/// </summary>
public sealed class HighlightMatchingBracketProcessor : IProcessor
{
    /// <summary>
    /// Creates a HighlightMatchingBracketProcessor.
    /// </summary>
    /// <param name="chars">Bracket characters to match.</param>
    /// <param name="maxCursorDistance">Maximum distance to search for match.</param>
    public HighlightMatchingBracketProcessor(
        string chars = "[](){}<>",
        int maxCursorDistance = 1000);

    public string Chars { get; }
    public int MaxCursorDistance { get; }

    public Transformation ApplyTransformation(TransformationInput input);
}
```

### DisplayMultipleCursors Class

```csharp
namespace Stroke.Layout;

/// <summary>
/// Processor that displays multiple cursors (for Vi visual block mode).
/// </summary>
public sealed class DisplayMultipleCursors : IProcessor
{
    public Transformation ApplyTransformation(TransformationInput input);
}
```

### BeforeInput Class

```csharp
namespace Stroke.Layout;

/// <summary>
/// Processor that inserts text before the input.
/// </summary>
public sealed class BeforeInput : IProcessor
{
    /// <summary>
    /// Creates a BeforeInput processor.
    /// </summary>
    /// <param name="text">Text to insert (or callable).</param>
    /// <param name="style">Style to apply.</param>
    public BeforeInput(object text, string style = "");

    public Transformation ApplyTransformation(TransformationInput input);
}
```

### AfterInput Class

```csharp
namespace Stroke.Layout;

/// <summary>
/// Processor that inserts text after the input.
/// </summary>
public sealed class AfterInput : IProcessor
{
    /// <summary>
    /// Creates an AfterInput processor.
    /// </summary>
    /// <param name="text">Text to insert (or callable).</param>
    /// <param name="style">Style to apply.</param>
    public AfterInput(object text, string style = "");

    public Transformation ApplyTransformation(TransformationInput input);
}
```

### AppendAutoSuggestion Class

```csharp
namespace Stroke.Layout;

/// <summary>
/// Processor that appends auto-suggestion text.
/// </summary>
public sealed class AppendAutoSuggestion : IProcessor
{
    /// <summary>
    /// Creates an AppendAutoSuggestion processor.
    /// </summary>
    /// <param name="style">Style for the suggestion.</param>
    public AppendAutoSuggestion(string style = "class:auto-suggestion");

    public Transformation ApplyTransformation(TransformationInput input);
}
```

### ShowArg Class

```csharp
namespace Stroke.Layout;

/// <summary>
/// Processor that displays the arg prefix (like "2x" for repetition).
/// </summary>
public sealed class ShowArg : IProcessor
{
    public Transformation ApplyTransformation(TransformationInput input);
}
```

### ConditionalProcessor Class

```csharp
namespace Stroke.Layout;

/// <summary>
/// Processor that conditionally applies another processor.
/// </summary>
public sealed class ConditionalProcessor : IProcessor
{
    /// <summary>
    /// Creates a ConditionalProcessor.
    /// </summary>
    /// <param name="processor">The processor to conditionally apply.</param>
    /// <param name="filter">The filter for when to apply.</param>
    public ConditionalProcessor(IProcessor processor, object? filter = null);

    public IProcessor Processor { get; }
    public IFilter Filter { get; }

    public Transformation ApplyTransformation(TransformationInput input);
}
```

### DynamicProcessor Class

```csharp
namespace Stroke.Layout;

/// <summary>
/// Processor that dynamically returns another processor.
/// </summary>
public sealed class DynamicProcessor : IProcessor
{
    /// <summary>
    /// Creates a DynamicProcessor.
    /// </summary>
    /// <param name="getProcessor">Callable that returns a processor.</param>
    public DynamicProcessor(Func<IProcessor?> getProcessor);

    public Transformation ApplyTransformation(TransformationInput input);
}
```

### ShowLeadingWhiteSpaceProcessor Class

```csharp
namespace Stroke.Layout;

/// <summary>
/// Processor that visualizes leading whitespace.
/// </summary>
public sealed class ShowLeadingWhiteSpaceProcessor : IProcessor
{
    /// <summary>
    /// Creates a ShowLeadingWhiteSpaceProcessor.
    /// </summary>
    /// <param name="getReplacement">Callable that returns replacement char.</param>
    /// <param name="style">Style for whitespace.</param>
    public ShowLeadingWhiteSpaceProcessor(
        Func<char>? getReplacement = null,
        string style = "class:leading-whitespace");

    public Transformation ApplyTransformation(TransformationInput input);
}
```

### ShowTrailingWhiteSpaceProcessor Class

```csharp
namespace Stroke.Layout;

/// <summary>
/// Processor that visualizes trailing whitespace.
/// </summary>
public sealed class ShowTrailingWhiteSpaceProcessor : IProcessor
{
    /// <summary>
    /// Creates a ShowTrailingWhiteSpaceProcessor.
    /// </summary>
    /// <param name="getReplacement">Callable that returns replacement char.</param>
    /// <param name="style">Style for whitespace.</param>
    public ShowTrailingWhiteSpaceProcessor(
        Func<char>? getReplacement = null,
        string style = "class:trailing-whitespace");

    public Transformation ApplyTransformation(TransformationInput input);
}
```

### TabsProcessor Class

```csharp
namespace Stroke.Layout;

/// <summary>
/// Processor that replaces tabs with spaces.
/// </summary>
public sealed class TabsProcessor : IProcessor
{
    /// <summary>
    /// Creates a TabsProcessor.
    /// </summary>
    /// <param name="tabWidth">Number of spaces per tab (default 4).</param>
    /// <param name="char">Character to display for tab (default space).</param>
    public TabsProcessor(int tabWidth = 4, string @char = " ");

    public int TabWidth { get; }
    public string Char { get; }

    public Transformation ApplyTransformation(TransformationInput input);
}
```

### ReverseSearchProcessor Class

```csharp
namespace Stroke.Layout;

/// <summary>
/// Processor for displaying reverse search prompt.
/// </summary>
public sealed class ReverseSearchProcessor : IProcessor
{
    public Transformation ApplyTransformation(TransformationInput input);
}
```

### Processor Utilities

```csharp
namespace Stroke.Layout;

/// <summary>
/// Processor utilities.
/// </summary>
public static class ProcessorUtils
{
    /// <summary>
    /// Merge multiple processors into one.
    /// </summary>
    public static IProcessor MergeProcessors(IEnumerable<IProcessor> processors);
}
```

## Project Structure

```
src/Stroke/
└── Layout/
    ├── IProcessor.cs
    ├── TransformationInput.cs
    ├── Transformation.cs
    ├── DummyProcessor.cs
    ├── HighlightSearchProcessor.cs
    ├── HighlightIncrementalSearchProcessor.cs
    ├── HighlightSelectionProcessor.cs
    ├── PasswordProcessor.cs
    ├── HighlightMatchingBracketProcessor.cs
    ├── DisplayMultipleCursors.cs
    ├── BeforeInput.cs
    ├── AfterInput.cs
    ├── AppendAutoSuggestion.cs
    ├── ShowArg.cs
    ├── ConditionalProcessor.cs
    ├── DynamicProcessor.cs
    ├── ShowLeadingWhiteSpaceProcessor.cs
    ├── ShowTrailingWhiteSpaceProcessor.cs
    ├── TabsProcessor.cs
    ├── ReverseSearchProcessor.cs
    └── ProcessorUtils.cs
tests/Stroke.Tests/
└── Layout/
    └── Processors/
        ├── HighlightSearchProcessorTests.cs
        ├── HighlightSelectionProcessorTests.cs
        ├── PasswordProcessorTests.cs
        ├── TabsProcessorTests.cs
        └── ProcessorUtilsTests.cs
```

## Implementation Notes

### Fragment Explosion

Many processors need to work on individual characters. Use `ExplodeTextFragments` to split multi-character fragments into single-character fragments.

### Position Mapping

When processors insert or remove content, they must provide `sourceToDisplay` and `displayToSource` functions to map cursor positions correctly.

### Search Highlighting

HighlightSearchProcessor:
1. Get search text from search state
2. Find all matches using regex
3. Apply 'search' class to matches
4. Apply 'search.current' class to match under cursor

### Selection Highlighting

HighlightSelectionProcessor:
1. Get selection range for current line
2. Transform source positions to display positions
3. Apply 'selected' class to selected characters
4. For empty lines in selection, insert visible space

### Bracket Matching

HighlightMatchingBracketProcessor:
1. Check character under cursor
2. If bracket, find matching bracket within distance limit
3. Highlight both brackets with 'bracket' class

### Tab Handling

TabsProcessor:
1. Find all tab characters
2. Calculate column-aligned width for each tab
3. Replace tab with appropriate number of spaces
4. Update position mapping functions

### Merged Processors

MergeProcessors creates a single processor that:
1. Applies each processor in sequence
2. Chains position mapping functions

## Dependencies

- `Stroke.Core.Document` (Feature 01) - Document class
- `Stroke.Core.FormattedText` (Feature 13) - Formatted text
- `Stroke.Layout.BufferControl` (Feature 26) - Buffer control
- `Stroke.Filters` (Feature 12) - Filter system

## Implementation Tasks

1. Implement `IProcessor` interface
2. Implement `TransformationInput` class
3. Implement `Transformation` class
4. Implement all processor classes
5. Implement `ProcessorUtils.MergeProcessors`
6. Implement fragment explosion utility
7. Write comprehensive unit tests

## Acceptance Criteria

- [ ] All processors match Python Prompt Toolkit semantics
- [ ] Search highlighting works correctly
- [ ] Selection highlighting works correctly
- [ ] Password masking works correctly
- [ ] Tab handling works correctly
- [ ] Position mapping is accurate
- [ ] Unit tests achieve 80% coverage
