# Contract: CompleteStyle

**Namespace**: `Stroke.Shortcuts`
**File**: `src/Stroke/Shortcuts/CompleteStyle.cs`
**Python Source**: `prompt_toolkit.shortcuts.prompt.CompleteStyle`

## Enum Definition

```csharp
/// <summary>
/// How to display autocompletions for the prompt.
/// </summary>
/// <remarks>
/// Port of Python Prompt Toolkit's <c>CompleteStyle</c> enum from
/// <c>prompt_toolkit.shortcuts.prompt</c>.
/// </remarks>
public enum CompleteStyle
{
    /// <summary>
    /// Display completions in a single-column dropdown menu near the cursor.
    /// </summary>
    Column,

    /// <summary>
    /// Display completions in a multi-column dropdown menu near the cursor.
    /// </summary>
    MultiColumn,

    /// <summary>
    /// Display completions below the input line, similar to GNU Readline's Tab completion.
    /// </summary>
    ReadlineLike,
}
```

## Python Mapping

| Python | C# |
|--------|-----|
| `CompleteStyle.COLUMN` | `CompleteStyle.Column` |
| `CompleteStyle.MULTI_COLUMN` | `CompleteStyle.MultiColumn` |
| `CompleteStyle.READLINE_LIKE` | `CompleteStyle.ReadlineLike` |

## Notes

- Python's `CompleteStyle` inherits from both `str` and `Enum` with string values (`"COLUMN"`, etc.). In C#, this is a simple enum with no string backing — the string representation is unnecessary for functionality.
- Default value in `PromptSession` constructor: `CompleteStyle.Column` (matching Python's `CompleteStyle.COLUMN`).
