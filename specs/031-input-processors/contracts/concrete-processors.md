# Contract: Concrete Processors

**Feature**: 031-input-processors
**Namespace**: `Stroke.Layout.Processors`
**Python Source**: `prompt_toolkit/layout/processors.py` (lines 156-1017)

## DummyProcessor

```csharp
/// <summary>
/// A processor that doesn't do anything. Returns fragments unchanged.
/// </summary>
public sealed class DummyProcessor : IProcessor
{
    public Transformation ApplyTransformation(TransformationInput transformationInput);
}
```

---

## PasswordProcessor

```csharp
/// <summary>
/// Processor that masks the input for passwords.
/// </summary>
public sealed class PasswordProcessor : IProcessor
{
    public PasswordProcessor(string @char = "*");

    /// <summary>The mask character.</summary>
    public string Char { get; }

    public Transformation ApplyTransformation(TransformationInput ti);
}
```

---

## HighlightSearchProcessor

```csharp
/// <summary>
/// Processor that highlights search matches in the document.
/// Applies "search" and "search.current" style classes.
/// </summary>
public class HighlightSearchProcessor : IProcessor
{
    /// <summary>Style class for matches (default "search").</summary>
    protected string ClassName { get; } = "search";

    /// <summary>Style class for current match (default "search.current").</summary>
    protected string ClassNameCurrent { get; } = "search.current";

    /// <summary>Get the search text for this processor.</summary>
    protected virtual string GetSearchText(BufferControl bufferControl);

    public Transformation ApplyTransformation(TransformationInput transformationInput);
}
```

---

## HighlightIncrementalSearchProcessor

```csharp
/// <summary>
/// Highlight incremental search matches. Uses "incsearch" style class.
/// Reads search text from the search buffer.
/// </summary>
public class HighlightIncrementalSearchProcessor : HighlightSearchProcessor
{
    // ClassName = "incsearch"
    // ClassNameCurrent = "incsearch.current"

    protected override string GetSearchText(BufferControl bufferControl);
}
```

---

## HighlightSelectionProcessor

```csharp
/// <summary>
/// Processor that highlights the selection in the document.
/// Applies "selected" style class.
/// </summary>
public sealed class HighlightSelectionProcessor : IProcessor
{
    public Transformation ApplyTransformation(TransformationInput transformationInput);
}
```

---

## HighlightMatchingBracketProcessor

```csharp
/// <summary>
/// Highlights matching bracket pairs when cursor is on or after a bracket.
/// </summary>
public sealed class HighlightMatchingBracketProcessor : IProcessor
{
    public HighlightMatchingBracketProcessor(
        string chars = "[](){}<>",
        int maxCursorDistance = 1000);

    /// <summary>Bracket characters to match.</summary>
    public string Chars { get; }

    /// <summary>Maximum search distance from cursor.</summary>
    public int MaxCursorDistance { get; }

    public Transformation ApplyTransformation(TransformationInput transformationInput);
}
```

---

## DisplayMultipleCursors

```csharp
/// <summary>
/// Displays all cursors when in Vi block insert mode.
/// Applies "multiple-cursors" style class.
/// </summary>
public sealed class DisplayMultipleCursors : IProcessor
{
    public Transformation ApplyTransformation(TransformationInput transformationInput);
}
```

---

## BeforeInput

```csharp
/// <summary>
/// Insert text before the input (on line 0 only).
/// </summary>
public class BeforeInput : IProcessor
{
    public BeforeInput(AnyFormattedText text, string style = "");

    /// <summary>The text to prepend.</summary>
    public AnyFormattedText Text { get; }

    /// <summary>Style to apply.</summary>
    public string Style { get; }

    public Transformation ApplyTransformation(TransformationInput ti);

    /// <summary>Returns "BeforeInput({Text}, style={Style})".</summary>
    public override string ToString();
}
```

---

## ShowArg

```csharp
/// <summary>
/// Display the 'arg' (repeat count) in front of the input.
/// </summary>
public sealed class ShowArg : BeforeInput
{
    public ShowArg();

    /// <summary>Returns "ShowArg()".</summary>
    public override string ToString();
}
```

**Output fragments** (when `KeyProcessor.Arg` is not null):
```
[("class:prompt.arg", "(arg: "), ("class:prompt.arg.text", N.ToString()), ("class:prompt.arg", ") ")]
```
When `KeyProcessor.Arg` is null, returns an empty fragment list.

---

## AfterInput

```csharp
/// <summary>
/// Insert text after the input (on the last line only).
/// </summary>
public sealed class AfterInput : IProcessor
{
    public AfterInput(AnyFormattedText text, string style = "");

    /// <summary>The text to append.</summary>
    public AnyFormattedText Text { get; }

    /// <summary>Style to apply.</summary>
    public string Style { get; }

    public Transformation ApplyTransformation(TransformationInput ti);

    /// <summary>Returns "AfterInput({Text}, style={Style})".</summary>
    public override string ToString();
}
```

---

## AppendAutoSuggestion

```csharp
/// <summary>
/// Append the auto suggestion to the input (on the last line only).
/// </summary>
public sealed class AppendAutoSuggestion : IProcessor
{
    public AppendAutoSuggestion(string style = "class:auto-suggestion");

    /// <summary>Style for suggestion text.</summary>
    public string Style { get; }

    public Transformation ApplyTransformation(TransformationInput ti);
}
```

---

## ShowLeadingWhiteSpaceProcessor

```csharp
/// <summary>
/// Make leading whitespace visible by replacing spaces with a visible character.
/// </summary>
public sealed class ShowLeadingWhiteSpaceProcessor : IProcessor
{
    public ShowLeadingWhiteSpaceProcessor(
        Func<string>? getChar = null,
        string style = "class:leading-whitespace");

    /// <summary>Style for replacement characters.</summary>
    public string Style { get; }

    /// <summary>Callable returning the visible replacement character.</summary>
    public Func<string> GetChar { get; }

    public Transformation ApplyTransformation(TransformationInput ti);
}
```

---

## ShowTrailingWhiteSpaceProcessor

```csharp
/// <summary>
/// Make trailing whitespace visible by replacing spaces with a visible character.
/// </summary>
/// <remarks>
/// Deviation: Python uses "class:training-whitespace" (typo). C# uses
/// "class:trailing-whitespace" (corrected per clarification session 2026-01-29).
/// </remarks>
public sealed class ShowTrailingWhiteSpaceProcessor : IProcessor
{
    public ShowTrailingWhiteSpaceProcessor(
        Func<string>? getChar = null,
        string style = "class:trailing-whitespace");

    /// <summary>Style for replacement characters.</summary>
    public string Style { get; }

    /// <summary>Callable returning the visible replacement character.</summary>
    public Func<string> GetChar { get; }

    public Transformation ApplyTransformation(TransformationInput ti);
}
```

---

## TabsProcessor

```csharp
/// <summary>
/// Render tabs as visible, column-aligned sequences.
/// </summary>
public sealed class TabsProcessor : IProcessor
{
    public TabsProcessor(
        object tabstop = null,   // int or Func<int>, default 4
        object char1 = null,     // string or Func<string>, default "|"
        object char2 = null,     // string or Func<string>, default "\u2508"
        string style = "class:tab");

    /// <summary>Tab stop width (int or callable).</summary>
    public object TabStop { get; }

    /// <summary>First tab character (string or callable).</summary>
    public object Char1 { get; }

    /// <summary>Second tab character (string or callable).</summary>
    public object Char2 { get; }

    /// <summary>Style for tab characters.</summary>
    public string Style { get; }

    public Transformation ApplyTransformation(TransformationInput ti);
}
```

**Design note**: Constructor parameter types use `object` to accept both `int`/`string` and `Func<int>`/`Func<string>`, matching Python's duck typing. `ConversionUtils.ToInt` and `ConversionUtils.ToStr` handle resolution.

---

## ReverseSearchProcessor

```csharp
/// <summary>
/// Display reverse-i-search prompt around the search buffer.
/// Applied to the SearchBufferControl, not the main input.
/// </summary>
public sealed class ReverseSearchProcessor : IProcessor
{
    /// <summary>
    /// Processor types excluded when filtering the main control's input processors.
    /// </summary>
    private static readonly List<Type> ExcludedInputProcessors = new()
    {
        typeof(HighlightSearchProcessor),
        typeof(HighlightSelectionProcessor),
        typeof(BeforeInput),
        typeof(AfterInput),
    };

    public Transformation ApplyTransformation(TransformationInput ti);
}
```

**Style classes used**:
- `"class:prompt.search"` — the direction label and delimiters: `"("`, direction text, `")\x60"`, and `"': "`
- `"class:prompt.search.text"` — the search query text

**Output format on line 0** (when main buffer found):
```
[("class:prompt.search", "("), ("class:prompt.search", direction_text), ("class:prompt.search", ")`"),
 ("class:prompt.search.text", query_text), ("", "': ")] + matched_line_fragments
```
Where `direction_text` is `"i-search"` (forward) or `"reverse-i-search"` (backward).

---

## ConditionalProcessor

```csharp
/// <summary>
/// Processor that applies another processor conditionally based on a filter.
/// </summary>
public sealed class ConditionalProcessor : IProcessor
{
    public ConditionalProcessor(IProcessor processor, FilterOrBool filter);

    /// <summary>The wrapped processor.</summary>
    public IProcessor Processor { get; }

    /// <summary>The activation filter.</summary>
    public IFilter Filter { get; }

    public Transformation ApplyTransformation(TransformationInput transformationInput);

    /// <summary>Returns "ConditionalProcessor(processor={Processor}, filter={Filter})".</summary>
    public override string ToString();
}
```

---

## DynamicProcessor

```csharp
/// <summary>
/// Processor that dynamically returns a processor at each invocation.
/// </summary>
public sealed class DynamicProcessor : IProcessor
{
    public DynamicProcessor(Func<IProcessor?> getProcessor);

    /// <summary>Factory callable for the processor.</summary>
    public Func<IProcessor?> GetProcessor { get; }

    public Transformation ApplyTransformation(TransformationInput ti);
}
```

---

## ProcessorUtils (static class)

```csharp
/// <summary>
/// Utility methods for combining processors.
/// </summary>
public static class ProcessorUtils
{
    /// <summary>
    /// Merge multiple processors into one.
    /// Returns DummyProcessor for empty list, the single processor for length-1,
    /// or a MergedProcessor that chains all processors.
    /// </summary>
    public static IProcessor MergeProcessors(IReadOnlyList<IProcessor> processors);
}
```

---

## _MergedProcessor (internal)

```csharp
/// <summary>
/// Internal processor that chains multiple processors sequentially,
/// composing their position mappings.
/// </summary>
internal sealed class MergedProcessor : IProcessor
{
    public MergedProcessor(IReadOnlyList<IProcessor> processors);

    /// <summary>The chained processors.</summary>
    public IReadOnlyList<IProcessor> Processors { get; }

    public Transformation ApplyTransformation(TransformationInput ti);
}
```

**Design note**: Named `MergedProcessor` (without underscore) since C# uses `internal` visibility instead of Python's name-mangling convention.
