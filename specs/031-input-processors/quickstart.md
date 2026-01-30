# Quickstart: Input Processors

**Feature**: 031-input-processors
**Date**: 2026-01-29

## Build & Test

```bash
# Build
dotnet build src/Stroke/Stroke.csproj

# Run all tests
dotnet test tests/Stroke.Tests/Stroke.Tests.csproj

# Run only processor tests
dotnet test tests/Stroke.Tests/Stroke.Tests.csproj --filter "FullyQualifiedName~Processors"
```

## Implementation Order

### Phase 1: Prerequisites & Core Types
1. Add `ViInsertMultipleMode` filter to `AppFilters.cs`
2. Add `ExplodedList.cs` + `ExplodeTextFragments` to `LayoutUtils.cs`
3. Create `IProcessor.cs` (interface + delegate aliases)
4. Create `TransformationInput.cs`
5. Create `Transformation.cs`
6. Create `DummyProcessor.cs`
7. Create `ProcessorUtils.cs` (MergeProcessors + _MergedProcessor)
8. Add missing properties to `BufferControl.cs`
9. Add `SearchTargetBufferControl` to `Layout.cs`

### Phase 2: Content Processors (P1)
10. `PasswordProcessor.cs`
11. `BeforeInput.cs`
12. `AfterInput.cs`
13. `ShowArg.cs` (extends BeforeInput)

### Phase 3: Highlight Processors (P1)
14. `HighlightSearchProcessor.cs`
15. `HighlightIncrementalSearchProcessor.cs` (extends HighlightSearchProcessor)
16. `HighlightSelectionProcessor.cs`

### Phase 4: Advanced Processors (P2)
17. `TabsProcessor.cs`
18. `ShowLeadingWhiteSpaceProcessor.cs`
19. `ShowTrailingWhiteSpaceProcessor.cs`
20. `HighlightMatchingBracketProcessor.cs`

### Phase 5: Specialized Processors (P2-P3)
21. `ConditionalProcessor.cs`
22. `DynamicProcessor.cs`
23. `DisplayMultipleCursors.cs`
24. `AppendAutoSuggestion.cs`
25. `ReverseSearchProcessor.cs`

### Phase 6: Tests
26. Write tests for each processor (one test file per processor or logical group)

## Key Patterns

### Creating a Simple Processor

```csharp
// In Stroke.Layout.Processors namespace
public class MyProcessor : IProcessor
{
    public Transformation ApplyTransformation(TransformationInput ti)
    {
        // Transform fragments
        var fragments = new List<StyleAndTextTuple>(ti.Fragments);
        // ... modify fragments ...
        return new Transformation(fragments);
    }
}
```

### Using Position Mappings

```csharp
// When inserting content that shifts positions
public Transformation ApplyTransformation(TransformationInput ti)
{
    if (ti.LineNumber == 0)
    {
        var prefix = FormattedTextUtils.ToFormattedText(text, style);
        var fragments = prefix.Concat(ti.Fragments).ToList();
        int shift = FormattedTextUtils.FragmentListLen(prefix);

        return new Transformation(
            fragments,
            sourceToDisplay: i => i + shift,
            displayToSource: i => i - shift);
    }
    return new Transformation(ti.Fragments);
}
```

### Using ExplodeTextFragments

```csharp
// Explode for per-character manipulation
var exploded = LayoutUtils.ExplodeTextFragments(ti.Fragments);
// Now each element is a single character
exploded[5] = new StyleAndTextTuple(
    exploded[5].Style + " class:highlight ",
    exploded[5].Text);
return new Transformation(exploded);
```

### Merging Processors

```csharp
var processors = new List<IProcessor>
{
    new HighlightSearchProcessor(),
    new HighlightSelectionProcessor(),
    new TabsProcessor(tabstop: 4)
};
var merged = ProcessorUtils.MergeProcessors(processors);
```
