# Contract: Internal Helpers

**Namespace**: `Stroke.Shortcuts`
**Python Source**: `prompt_toolkit.shortcuts.prompt` (module-level helpers and internal class)

## PromptContinuationCallable (Delegate)

```csharp
/// <summary>
/// Delegate for generating continuation prompt text in multiline mode.
/// </summary>
/// <param name="promptWidth">The character width of the prompt on the first line.</param>
/// <param name="lineNumber">The current line number (0-based, relative to the input start).</param>
/// <param name="wrapCount">How many times the current line has wrapped.</param>
/// <returns>Formatted text to display as the line prefix.</returns>
public delegate AnyFormattedText PromptContinuationCallable(
    int promptWidth, int lineNumber, int wrapCount);
```

**Python Equivalent**: `Callable[[int, int, int], AnyFormattedText]` part of the `PromptContinuationText` union type.

---

## KeyboardInterruptException

```csharp
/// <summary>
/// Exception thrown when the user presses Ctrl-C during a prompt.
/// Default interrupt exception type for <see cref="PromptSession{TResult}"/>.
/// </summary>
public sealed class KeyboardInterruptException : Exception
{
    public KeyboardInterruptException() : base("Keyboard interrupt.") { }
    public KeyboardInterruptException(string message) : base(message) { }
    public KeyboardInterruptException(string message, Exception innerException)
        : base(message, innerException) { }
}
```

**Python Equivalent**: `KeyboardInterrupt` (built-in).

---

## EOFException

```csharp
/// <summary>
/// Exception thrown when the user presses Ctrl-D on an empty buffer during a prompt.
/// Default EOF exception type for <see cref="PromptSession{TResult}"/>.
/// </summary>
public sealed class EOFException : Exception
{
    public EOFException() : base("End of input.") { }
    public EOFException(string message) : base(message) { }
    public EOFException(string message, Exception innerException)
        : base(message, innerException) { }
}
```

**Python Equivalent**: `EOFError` (built-in).

---

## _SplitMultilinePrompt (Internal Static Helper)

```csharp
/// <summary>
/// Split a prompt message at newlines for multiline prompt rendering.
/// Returns three functions: hasBeforeFragments, getBeforeFragments, getFirstInputLineFragments.
/// </summary>
internal static (
    Func<bool> HasBeforeFragments,
    Func<StyleAndTextTuples> Before,
    Func<StyleAndTextTuples> FirstInputLine
) SplitMultilinePrompt(Func<StyleAndTextTuples> getPromptText)
```

**Python Equivalent**: `_split_multiline_prompt(get_prompt_text)` → `tuple[Callable[[], bool], callable, callable]`

**Behavior**:
1. `HasBeforeFragments()`: Scans fragments for any `\n` character → returns true if found
2. `Before()`: Returns all fragments *before* the last `\n` (text above the input line)
3. `FirstInputLine()`: Returns all fragments *after* the last `\n` (inline prompt on the input line)

Uses `LayoutUtils.ExplodeTextFragments()` to split multi-character fragments into single characters for accurate newline detection.

---

## _RPrompt (Internal Window Subclass)

```csharp
/// <summary>
/// Window that displays right-aligned prompt text.
/// </summary>
internal sealed class RPrompt : Window
{
    public RPrompt(AnyFormattedText text)
        : base(
            content: new FormattedTextControl(text: text),
            align: WindowAlign.Right,
            style: "class:rprompt")
    { }
}
```

**Python Equivalent**: `_RPrompt(Window)` class.

## Notes

- `SplitMultilinePrompt` is placed as an internal static method on the `PromptSession` partial class (in the Layout file), matching Python's module-level `_split_multiline_prompt` function.
- `RPrompt` is a minimal Window subclass that could also be implemented inline in the layout construction. Keeping it as a named class matches the Python structure and improves readability.
- Exception types are in the `Stroke.Shortcuts` namespace alongside `PromptSession`, since they're specific to the prompt API. Users catching these exceptions import from the same namespace as `PromptSession`.
