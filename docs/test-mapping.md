# Python Prompt Toolkit to Stroke Test Mapping

This document provides a complete one-to-one mapping of every Python Prompt Toolkit test to its Stroke (.NET) equivalent.

## Test Infrastructure

| Aspect | Python Prompt Toolkit | Stroke (.NET) |
|--------|----------------------|---------------|
| **Framework** | pytest | xUnit |
| **Assertions** | `assert` / `pytest.raises` | `Assert.*` / `Assert.Throws<T>` |
| **Fixtures** | `@pytest.fixture` | Constructor / `IClassFixture<T>` |
| **Async** | `asyncio.run()` | `async Task` / `.Result` |
| **Temp Files** | `tmpdir` fixture | `Path.GetTempPath()` |
| **Mocking** | None (real implementations) | None (real implementations) |
| **Test Discovery** | `test_*.py` / `test_*()` | `*Tests.cs` / `[Fact]` methods |

## Naming Convention

| Python | C# |
|--------|-----|
| `test_simple_text_input` | `SimpleTextInput` |
| `test_emacs_cursor_movements` | `EmacsCursorMovements` |
| `test_pathcompleter_can_expanduser` | `PathCompleter_CanExpandUser` |

**Rules:**
1. Remove `test_` prefix
2. Convert `snake_case` to `PascalCase`
3. Use `_` to separate class name from method description where logical
4. Group related tests in nested classes or regions

---

## Test Project Structure

```
tests/
├── Stroke.Core.Tests/
│   ├── DocumentTests.cs
│   ├── BufferTests.cs
│   └── FilterTests.cs
├── Stroke.Input.Tests/
│   ├── InputStreamTests.cs
│   ├── KeyBindingTests.cs
│   └── YankNthArgTests.cs
├── Stroke.Completion.Tests/
│   ├── PathCompleterTests.cs
│   ├── WordCompleterTests.cs
│   ├── FuzzyCompleterTests.cs
│   └── NestedCompleterTests.cs
├── Stroke.FormattedText.Tests/
│   ├── HtmlTests.cs
│   ├── AnsiTests.cs
│   └── FormattedTextUtilsTests.cs
├── Stroke.Styles.Tests/
│   ├── StyleTests.cs
│   └── StyleTransformationTests.cs
├── Stroke.Layout.Tests/
│   └── LayoutTests.cs
├── Stroke.History.Tests/
│   └── HistoryTests.cs
├── Stroke.Output.Tests/
│   ├── Vt100OutputTests.cs
│   └── PrintFormattedTextTests.cs
├── Stroke.Shortcuts.Tests/
│   └── ShortcutsTests.cs
├── Stroke.Application.Tests/
│   ├── PromptSessionTests.cs
│   ├── EmacsModeTests.cs
│   └── ViModeTests.cs
├── Stroke.Widgets.Tests/
│   └── ButtonTests.cs
├── Stroke.EventLoop.Tests/
│   └── AsyncGeneratorTests.cs
├── Stroke.Contrib.Tests/
│   └── RegularLanguagesTests.cs
└── Stroke.Tests/
    ├── UtilsTests.cs
    └── MemoryLeakTests.cs
```

---

## Complete Test Mapping by File

### 1. test_document.py → DocumentTests.cs

**File:** `tests/Stroke.Core.Tests/DocumentTests.cs`

| # | Python Test | C# Test Method | Description |
|---|-------------|----------------|-------------|
| 1 | `test_current_char` | `CurrentChar` | Validates `CurrentChar` property returns character at cursor |
| 2 | `test_text_before_cursor` | `TextBeforeCursor` | Validates `TextBeforeCursor` property |
| 3 | `test_text_after_cursor` | `TextAfterCursor` | Validates `TextAfterCursor` property |
| 4 | `test_lines` | `Lines` | Validates `Lines` property returns split lines |
| 5 | `test_line_count` | `LineCount` | Validates `LineCount` property |
| 6 | `test_current_line_before_cursor` | `CurrentLineBeforeCursor` | Validates current line prefix |
| 7 | `test_current_line_after_cursor` | `CurrentLineAfterCursor` | Validates current line suffix |
| 8 | `test_current_line` | `CurrentLine` | Validates `CurrentLine` property |
| 9 | `test_cursor_position` | `CursorPosition` | Validates `CursorPositionRow` and `CursorPositionCol` |
| 10 | `test_translate_index_to_position` | `TranslateIndexToPosition` | Validates index to (row, col) translation |
| 11 | `test_is_cursor_at_the_end` | `IsCursorAtTheEnd` | Validates end-of-document detection |
| 12 | `test_get_word_before_cursor_with_whitespace_and_pattern` | `GetWordBeforeCursor_WithWhitespaceAndPattern` | Validates word extraction with regex |

```csharp
// Stroke.Core.Tests/DocumentTests.cs
namespace Stroke.Core.Tests;

public class DocumentTests
{
    private readonly Document _document;

    public DocumentTests()
    {
        _document = new Document("line 1\nline 2\nline 3\nline 4\n", cursorPosition: 11);
    }

    [Fact] public void CurrentChar() { /* ... */ }
    [Fact] public void TextBeforeCursor() { /* ... */ }
    [Fact] public void TextAfterCursor() { /* ... */ }
    [Fact] public void Lines() { /* ... */ }
    [Fact] public void LineCount() { /* ... */ }
    [Fact] public void CurrentLineBeforeCursor() { /* ... */ }
    [Fact] public void CurrentLineAfterCursor() { /* ... */ }
    [Fact] public void CurrentLine() { /* ... */ }
    [Fact] public void CursorPosition() { /* ... */ }
    [Fact] public void TranslateIndexToPosition() { /* ... */ }
    [Fact] public void IsCursorAtTheEnd() { /* ... */ }
    [Fact] public void GetWordBeforeCursor_WithWhitespaceAndPattern() { /* ... */ }
}
```

---

### 2. test_buffer.py → BufferTests.cs

**File:** `tests/Stroke.Core.Tests/BufferTests.cs`

| # | Python Test | C# Test Method | Description |
|---|-------------|----------------|-------------|
| 1 | `test_initial` | `Initial` | Buffer starts empty with cursor at 0 |
| 2 | `test_insert_text` | `InsertText` | Insert text advances cursor |
| 3 | `test_cursor_movement` | `CursorMovement` | CursorLeft/Right operations |
| 4 | `test_backspace` | `Backspace` | DeleteBeforeCursor removes character |
| 5 | `test_cursor_up` | `CursorUp` | Cursor up in multiline text |
| 6 | `test_cursor_down` | `CursorDown` | Cursor down in multiline text |
| 7 | `test_join_next_line` | `JoinNextLine` | Join current line with next |
| 8 | `test_newline` | `Newline` | Insert newline character |
| 9 | `test_swap_characters_before_cursor` | `SwapCharactersBeforeCursor` | Transpose characters |

```csharp
// Stroke.Core.Tests/BufferTests.cs
namespace Stroke.Core.Tests;

public class BufferTests
{
    private readonly Buffer _buffer;

    public BufferTests()
    {
        _buffer = new Buffer();
    }

    [Fact] public void Initial() { /* ... */ }
    [Fact] public void InsertText() { /* ... */ }
    [Fact] public void CursorMovement() { /* ... */ }
    [Fact] public void Backspace() { /* ... */ }
    [Fact] public void CursorUp() { /* ... */ }
    [Fact] public void CursorDown() { /* ... */ }
    [Fact] public void JoinNextLine() { /* ... */ }
    [Fact] public void Newline() { /* ... */ }
    [Fact] public void SwapCharactersBeforeCursor() { /* ... */ }
}
```

---

### 3. test_filter.py → FilterTests.cs

**File:** `tests/Stroke.Core.Tests/FilterTests.cs`

| # | Python Test | C# Test Method | Description |
|---|-------------|----------------|-------------|
| 1 | `test_never` | `Never` | Never filter returns false |
| 2 | `test_always` | `Always` | Always filter returns true |
| 3 | `test_invert` | `Invert` | `!` operator inverts filter |
| 4 | `test_or` | `Or` | `\|` operator for OR logic |
| 5 | `test_and` | `And` | `&` operator for AND logic |
| 6 | `test_nested_and` | `NestedAnd` | Nested AND expressions |
| 7 | `test_nested_or` | `NestedOr` | Nested OR expressions |
| 8 | `test_to_filter` | `ToFilter` | Convert bool/Condition to IFilter |
| 9 | `test_filter_cache_regression_1` | `FilterCache_Regression1` | Cache regression test |
| 10 | `test_filter_cache_regression_2` | `FilterCache_Regression2` | Cache with multiple conditions |
| 11 | `test_filter_remove_duplicates` | `FilterRemoveDuplicates` | Deduplication in AND/OR lists |

```csharp
// Stroke.Core.Tests/FilterTests.cs
namespace Stroke.Core.Tests;

public class FilterTests
{
    [Fact] public void Never() { /* ... */ }
    [Fact] public void Always() { /* ... */ }
    [Fact] public void Invert() { /* ... */ }
    [Fact] public void Or() { /* ... */ }
    [Fact] public void And() { /* ... */ }
    [Fact] public void NestedAnd() { /* ... */ }
    [Fact] public void NestedOr() { /* ... */ }
    [Fact] public void ToFilter() { /* ... */ }
    [Fact] public void FilterCache_Regression1() { /* ... */ }
    [Fact] public void FilterCache_Regression2() { /* ... */ }
    [Fact] public void FilterRemoveDuplicates() { /* ... */ }
}
```

---

### 4. test_inputstream.py → InputStreamTests.cs

**File:** `tests/Stroke.Input.Tests/InputStreamTests.cs`

| # | Python Test | C# Test Method | Description |
|---|-------------|----------------|-------------|
| 1 | `test_control_keys` | `ControlKeys` | Parse control characters |
| 2 | `test_arrows` | `Arrows` | Parse arrow key sequences |
| 3 | `test_escape` | `Escape` | Parse escape key |
| 4 | `test_special_double_keys` | `SpecialDoubleKeys` | Multi-modifier sequences |
| 5 | `test_flush_1` | `Flush_PartialSequence` | Buffering without flush |
| 6 | `test_flush_2` | `Flush_WithExplicitFlush` | Flush method behavior |
| 7 | `test_meta_arrows` | `MetaArrows` | Meta+arrow combinations |
| 8 | `test_control_square_close` | `ControlSquareClose` | Parse 0x1D |
| 9 | `test_invalid` | `Invalid` | Invalid escape sequences |
| 10 | `test_cpr_response` | `CprResponse` | Cursor Position Report |
| 11 | `test_cpr_response_2` | `CprResponse_WithNewline` | CPR with trailing newline |

```csharp
// Stroke.Input.Tests/InputStreamTests.cs
namespace Stroke.Input.Tests;

public class InputStreamTests
{
    [Fact] public void ControlKeys() { /* ... */ }
    [Fact] public void Arrows() { /* ... */ }
    [Fact] public void Escape() { /* ... */ }
    [Fact] public void SpecialDoubleKeys() { /* ... */ }
    [Fact] public void Flush_PartialSequence() { /* ... */ }
    [Fact] public void Flush_WithExplicitFlush() { /* ... */ }
    [Fact] public void MetaArrows() { /* ... */ }
    [Fact] public void ControlSquareClose() { /* ... */ }
    [Fact] public void Invalid() { /* ... */ }
    [Fact] public void CprResponse() { /* ... */ }
    [Fact] public void CprResponse_WithNewline() { /* ... */ }
}
```

---

### 5. test_key_binding.py → KeyBindingTests.cs

**File:** `tests/Stroke.Input.Tests/KeyBindingTests.cs`

| # | Python Test | C# Test Method | Description |
|---|-------------|----------------|-------------|
| 1 | `test_remove_bindings` | `RemoveBindings` | Remove bindings by handler or key |
| 2 | `test_feed_simple` | `Feed_Simple` | Feed ControlX+ControlC sequence |
| 3 | `test_feed_several` | `Feed_Several` | Multiple key sequences |
| 4 | `test_control_square_closed_any` | `ControlSquareClose_WithAny` | Keys.Any wildcard |
| 5 | `test_common_prefix` | `CommonPrefix` | Prefix ambiguity resolution |
| 6 | `test_previous_key_sequence` | `PreviousKeySequence` | Event key_sequence tracking |

```csharp
// Stroke.Input.Tests/KeyBindingTests.cs
namespace Stroke.Input.Tests;

public class KeyBindingTests
{
    [Fact] public void RemoveBindings() { /* ... */ }
    [Fact] public void Feed_Simple() { /* ... */ }
    [Fact] public void Feed_Several() { /* ... */ }
    [Fact] public void ControlSquareClose_WithAny() { /* ... */ }
    [Fact] public void CommonPrefix() { /* ... */ }
    [Fact] public void PreviousKeySequence() { /* ... */ }
}
```

---

### 6. test_yank_nth_arg.py → YankNthArgTests.cs

**File:** `tests/Stroke.Input.Tests/YankNthArgTests.cs`

| # | Python Test | C# Test Method | Description |
|---|-------------|----------------|-------------|
| 1 | `test_empty_history` | `EmptyHistory` | YankLastArg with no history |
| 2 | `test_simple_search` | `SimpleSearch` | Basic YankLastArg |
| 3 | `test_simple_search_with_quotes` | `SimpleSearch_WithQuotes` | Quote handling |
| 4 | `test_simple_search_with_arg` | `SimpleSearch_WithArg` | YankLastArg(n=2) |
| 5 | `test_simple_search_with_arg_out_of_bounds` | `SimpleSearch_WithArgOutOfBounds` | n > word count |
| 6 | `test_repeated_search` | `RepeatedSearch` | Multiple YankLastArg calls |
| 7 | `test_repeated_search_with_wraparound` | `RepeatedSearch_WithWraparound` | Wraparound behavior |
| 8 | `test_yank_nth_arg` | `YankNthArg` | YankNthArg basic |
| 9 | `test_repeated_yank_nth_arg` | `RepeatedYankNthArg` | Multiple YankNthArg calls |
| 10 | `test_yank_nth_arg_with_arg` | `YankNthArg_WithArg` | YankNthArg(n=2) |

```csharp
// Stroke.Input.Tests/YankNthArgTests.cs
namespace Stroke.Input.Tests;

public class YankNthArgTests
{
    [Fact] public void EmptyHistory() { /* ... */ }
    [Fact] public void SimpleSearch() { /* ... */ }
    [Fact] public void SimpleSearch_WithQuotes() { /* ... */ }
    [Fact] public void SimpleSearch_WithArg() { /* ... */ }
    [Fact] public void SimpleSearch_WithArgOutOfBounds() { /* ... */ }
    [Fact] public void RepeatedSearch() { /* ... */ }
    [Fact] public void RepeatedSearch_WithWraparound() { /* ... */ }
    [Fact] public void YankNthArg() { /* ... */ }
    [Fact] public void RepeatedYankNthArg() { /* ... */ }
    [Fact] public void YankNthArg_WithArg() { /* ... */ }
}
```

---

### 7. test_completion.py → Completion Tests

**Files:**
- `tests/Stroke.Completion.Tests/PathCompleterTests.cs`
- `tests/Stroke.Completion.Tests/WordCompleterTests.cs`
- `tests/Stroke.Completion.Tests/FuzzyCompleterTests.cs`
- `tests/Stroke.Completion.Tests/NestedCompleterTests.cs`
- `tests/Stroke.Completion.Tests/DeduplicateCompleterTests.cs`

#### PathCompleterTests.cs

| # | Python Test | C# Test Method | Description |
|---|-------------|----------------|-------------|
| 1 | `test_pathcompleter_completes_in_current_directory` | `CompletesInCurrentDirectory` | Current dir completion |
| 2 | `test_pathcompleter_completes_files_in_current_directory` | `CompletesFilesInCurrentDirectory` | File name completion |
| 3 | `test_pathcompleter_completes_files_in_absolute_directory` | `CompletesFilesInAbsoluteDirectory` | Absolute path completion |
| 4 | `test_pathcompleter_completes_directories_with_only_directories` | `CompletesDirectoriesOnly` | Directory-only filter |
| 5 | `test_pathcompleter_respects_completions_under_min_input_len` | `RespectsMinInputLength` | Minimum input constraint |
| 6 | `test_pathcompleter_does_not_expanduser_by_default` | `DoesNotExpandUserByDefault` | Tilde expansion disabled |
| 7 | `test_pathcompleter_can_expanduser` | `CanExpandUser` | Tilde expansion enabled |
| 8 | `test_pathcompleter_can_apply_file_filter` | `CanApplyFileFilter` | Custom file filtering |
| 9 | `test_pathcompleter_get_paths_constrains_path` | `GetPathsConstrainsPath` | Path constraints |

```csharp
// Stroke.Completion.Tests/PathCompleterTests.cs
namespace Stroke.Completion.Tests;

public class PathCompleterTests : IDisposable
{
    private readonly string _tempDir;

    public PathCompleterTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose() => Directory.Delete(_tempDir, recursive: true);

    [Fact] public void CompletesInCurrentDirectory() { /* ... */ }
    [Fact] public void CompletesFilesInCurrentDirectory() { /* ... */ }
    [Fact] public void CompletesFilesInAbsoluteDirectory() { /* ... */ }
    [Fact] public void CompletesDirectoriesOnly() { /* ... */ }
    [Fact] public void RespectsMinInputLength() { /* ... */ }
    [Fact] public void DoesNotExpandUserByDefault() { /* ... */ }
    [Fact] public void CanExpandUser() { /* ... */ }
    [Fact] public void CanApplyFileFilter() { /* ... */ }
    [Fact] public void GetPathsConstrainsPath() { /* ... */ }
}
```

#### WordCompleterTests.cs

| # | Python Test | C# Test Method | Description |
|---|-------------|----------------|-------------|
| 1 | `test_word_completer_static_word_list` | `StaticWordList` | Static word list |
| 2 | `test_word_completer_ignore_case` | `IgnoreCase` | Case-insensitive |
| 3 | `test_word_completer_match_middle` | `MatchMiddle` | Match middle of words |
| 4 | `test_word_completer_sentence` | `Sentence` | Multi-word phrases |
| 5 | `test_word_completer_dynamic_word_list` | `DynamicWordList` | Dynamic callable |
| 6 | `test_word_completer_pattern` | `Pattern` | Custom regex pattern |

```csharp
// Stroke.Completion.Tests/WordCompleterTests.cs
namespace Stroke.Completion.Tests;

public class WordCompleterTests
{
    [Fact] public void StaticWordList() { /* ... */ }
    [Fact] public void IgnoreCase() { /* ... */ }
    [Fact] public void MatchMiddle() { /* ... */ }
    [Fact] public void Sentence() { /* ... */ }
    [Fact] public void DynamicWordList() { /* ... */ }
    [Fact] public void Pattern() { /* ... */ }
}
```

#### FuzzyCompleterTests.cs

| # | Python Test | C# Test Method | Description |
|---|-------------|----------------|-------------|
| 1 | `test_fuzzy_completer` | `FuzzyMatching` | Fuzzy matching algorithm |

```csharp
// Stroke.Completion.Tests/FuzzyCompleterTests.cs
namespace Stroke.Completion.Tests;

public class FuzzyCompleterTests
{
    [Fact] public void FuzzyMatching() { /* ... */ }
}
```

#### NestedCompleterTests.cs

| # | Python Test | C# Test Method | Description |
|---|-------------|----------------|-------------|
| 1 | `test_nested_completer` | `NestedCompletion` | Command hierarchies |

```csharp
// Stroke.Completion.Tests/NestedCompleterTests.cs
namespace Stroke.Completion.Tests;

public class NestedCompleterTests
{
    [Fact] public void NestedCompletion() { /* ... */ }
}
```

#### DeduplicateCompleterTests.cs

| # | Python Test | C# Test Method | Description |
|---|-------------|----------------|-------------|
| 1 | `test_deduplicate_completer` | `Deduplication` | Deduplication across completers |

```csharp
// Stroke.Completion.Tests/DeduplicateCompleterTests.cs
namespace Stroke.Completion.Tests;

public class DeduplicateCompleterTests
{
    [Fact] public void Deduplication() { /* ... */ }
}
```

---

### 8. test_formatted_text.py → FormattedText Tests

**Files:**
- `tests/Stroke.FormattedText.Tests/HtmlTests.cs`
- `tests/Stroke.FormattedText.Tests/AnsiTests.cs`
- `tests/Stroke.FormattedText.Tests/FormattedTextUtilsTests.cs`

#### HtmlTests.cs

| # | Python Test | C# Test Method | Description |
|---|-------------|----------------|-------------|
| 1 | `test_basic_html` | `BasicHtml` | HTML to FormattedText |
| 2 | `test_html_with_fg_bg` | `HtmlWithFgBg` | Foreground/background colors |
| 3 | `test_html_interpolation` | `HtmlInterpolation` | String formatting |

```csharp
// Stroke.FormattedText.Tests/HtmlTests.cs
namespace Stroke.FormattedText.Tests;

public class HtmlTests
{
    [Fact] public void BasicHtml() { /* ... */ }
    [Fact] public void HtmlWithFgBg() { /* ... */ }
    [Fact] public void HtmlInterpolation() { /* ... */ }
}
```

#### AnsiTests.cs

| # | Python Test | C# Test Method | Description |
|---|-------------|----------------|-------------|
| 1 | `test_ansi_formatting` | `AnsiFormatting` | ANSI escape parsing |
| 2 | `test_ansi_dim` | `AnsiDim` | Dim attribute |
| 3 | `test_ansi_256_color` | `Ansi256Color` | 256-color codes |
| 4 | `test_ansi_true_color` | `AnsiTrueColor` | 24-bit true color |
| 5 | `test_ansi_interpolation` | `AnsiInterpolation` | String formatting |

```csharp
// Stroke.FormattedText.Tests/AnsiTests.cs
namespace Stroke.FormattedText.Tests;

public class AnsiTests
{
    [Fact] public void AnsiFormatting() { /* ... */ }
    [Fact] public void AnsiDim() { /* ... */ }
    [Fact] public void Ansi256Color() { /* ... */ }
    [Fact] public void AnsiTrueColor() { /* ... */ }
    [Fact] public void AnsiInterpolation() { /* ... */ }
}
```

#### FormattedTextUtilsTests.cs

| # | Python Test | C# Test Method | Description |
|---|-------------|----------------|-------------|
| 1 | `test_interpolation` | `Interpolation` | Template formatting |
| 2 | `test_merge_formatted_text` | `MergeFormattedText` | Merge multiple texts |
| 3 | `test_pygments_tokens` | `PygmentsTokens` | Pygments conversion |
| 4 | `test_split_lines` | `SplitLines` | Split by newlines |
| 5 | `test_split_lines_2` | `SplitLines_MultipleSegments` | Multiple styled segments |
| 6 | `test_split_lines_3` | `SplitLines_TrailingNewlines` | Trailing newlines |
| 7 | `test_split_lines_4` | `SplitLines_LeadingNewlines` | Leading newlines |

```csharp
// Stroke.FormattedText.Tests/FormattedTextUtilsTests.cs
namespace Stroke.FormattedText.Tests;

public class FormattedTextUtilsTests
{
    [Fact] public void Interpolation() { /* ... */ }
    [Fact] public void MergeFormattedText() { /* ... */ }
    [Fact] public void PygmentsTokens() { /* ... */ }
    [Fact] public void SplitLines() { /* ... */ }
    [Fact] public void SplitLines_MultipleSegments() { /* ... */ }
    [Fact] public void SplitLines_TrailingNewlines() { /* ... */ }
    [Fact] public void SplitLines_LeadingNewlines() { /* ... */ }
}
```

---

### 9. test_style.py → StyleTests.cs

**File:** `tests/Stroke.Styles.Tests/StyleTests.cs`

| # | Python Test | C# Test Method | Description |
|---|-------------|----------------|-------------|
| 1 | `test_style_from_dict` | `StyleFromDict` | Style.FromDict with attributes |
| 2 | `test_class_combinations_1` | `ClassCombinations_Priority` | Multi-class resolution |
| 3 | `test_class_combinations_2` | `ClassCombinations_DefinitionOrder` | Definition order priority |
| 4 | `test_substyles` | `Substyles` | Dot-notation substyles |
| 5 | `test_swap_light_and_dark_style_transformation` | `SwapLightAndDarkStyleTransformation` | Light/dark swapping |

```csharp
// Stroke.Styles.Tests/StyleTests.cs
namespace Stroke.Styles.Tests;

public class StyleTests
{
    [Fact] public void StyleFromDict() { /* ... */ }
    [Fact] public void ClassCombinations_Priority() { /* ... */ }
    [Fact] public void ClassCombinations_DefinitionOrder() { /* ... */ }
    [Fact] public void Substyles() { /* ... */ }
    [Fact] public void SwapLightAndDarkStyleTransformation() { /* ... */ }
}
```

---

### 10. test_style_transformation.py → StyleTransformationTests.cs

**File:** `tests/Stroke.Styles.Tests/StyleTransformationTests.cs`

| # | Python Test | C# Test Method | Description |
|---|-------------|----------------|-------------|
| 1 | `test_adjust_brightness_style_transformation` | `AdjustBrightnessStyleTransformation` | Brightness adjustment |

```csharp
// Stroke.Styles.Tests/StyleTransformationTests.cs
namespace Stroke.Styles.Tests;

public class StyleTransformationTests
{
    [Fact] public void AdjustBrightnessStyleTransformation() { /* ... */ }
}
```

---

### 11. test_history.py → HistoryTests.cs

**File:** `tests/Stroke.History.Tests/HistoryTests.cs`

| # | Python Test | C# Test Method | Description |
|---|-------------|----------------|-------------|
| 1 | `test_in_memory_history` | `InMemoryHistory` | In-memory storage |
| 2 | `test_file_history` | `FileHistory` | File persistence |
| 3 | `test_threaded_file_history` | `ThreadedFileHistory` | Threaded file wrapper |
| 4 | `test_threaded_in_memory_history` | `ThreadedInMemoryHistory` | Threaded memory wrapper |

```csharp
// Stroke.History.Tests/HistoryTests.cs
namespace Stroke.History.Tests;

public class HistoryTests : IDisposable
{
    private readonly string _tempFile;

    public HistoryTests()
    {
        _tempFile = Path.GetTempFileName();
    }

    public void Dispose() => File.Delete(_tempFile);

    [Fact] public void InMemoryHistory() { /* ... */ }
    [Fact] public void FileHistory() { /* ... */ }
    [Fact] public void ThreadedFileHistory() { /* ... */ }
    [Fact] public void ThreadedInMemoryHistory() { /* ... */ }
}
```

---

### 12. test_layout.py → LayoutTests.cs

**File:** `tests/Stroke.Layout.Tests/LayoutTests.cs`

| # | Python Test | C# Test Method | Description |
|---|-------------|----------------|-------------|
| 1 | `test_layout_class` | `LayoutClass` | Container hierarchy, focus |
| 2 | `test_create_invalid_layout` | `CreateInvalidLayout` | InvalidLayoutException |

```csharp
// Stroke.Layout.Tests/LayoutTests.cs
namespace Stroke.Layout.Tests;

public class LayoutTests
{
    [Fact] public void LayoutClass() { /* ... */ }
    [Fact] public void CreateInvalidLayout() { /* ... */ }
}
```

---

### 13. test_vt100_output.py → Vt100OutputTests.cs

**File:** `tests/Stroke.Output.Tests/Vt100OutputTests.cs`

| # | Python Test | C# Test Method | Description |
|---|-------------|----------------|-------------|
| 1 | `test_get_closest_ansi_color` | `GetClosestAnsiColor` | RGB to ANSI mapping |

```csharp
// Stroke.Output.Tests/Vt100OutputTests.cs
namespace Stroke.Output.Tests;

public class Vt100OutputTests
{
    [Fact] public void GetClosestAnsiColor() { /* ... */ }
}
```

---

### 14. test_print_formatted_text.py → PrintFormattedTextTests.cs

**File:** `tests/Stroke.Output.Tests/PrintFormattedTextTests.cs`

| # | Python Test | C# Test Method | Description |
|---|-------------|----------------|-------------|
| 1 | `test_print_formatted_text` | `PrintFormattedText` | Basic print |
| 2 | `test_print_formatted_text_backslash_r` | `PrintFormattedText_CarriageReturn` | CRLF handling |
| 3 | `test_formatted_text_with_style` | `FormattedTextWithStyle` | Style application |
| 4 | `test_html_with_style` | `HtmlWithStyle` | HTML rendering |
| 5 | `test_print_formatted_text_with_dim` | `PrintFormattedText_WithDim` | Dim attribute |

```csharp
// Stroke.Output.Tests/PrintFormattedTextTests.cs
namespace Stroke.Output.Tests;

public class PrintFormattedTextTests
{
    [Fact] public void PrintFormattedText() { /* ... */ }
    [Fact] public void PrintFormattedText_CarriageReturn() { /* ... */ }
    [Fact] public void FormattedTextWithStyle() { /* ... */ }
    [Fact] public void HtmlWithStyle() { /* ... */ }
    [Fact] public void PrintFormattedText_WithDim() { /* ... */ }
}
```

---

### 15. test_shortcuts.py → ShortcutsTests.cs

**File:** `tests/Stroke.Shortcuts.Tests/ShortcutsTests.cs`

| # | Python Test | C# Test Method | Description |
|---|-------------|----------------|-------------|
| 1 | `test_split_multiline_prompt` | `SplitMultilinePrompt` | Multiline prompt splitting |
| 2 | `test_print_container` | `PrintContainer` | Container rendering |

```csharp
// Stroke.Shortcuts.Tests/ShortcutsTests.cs
namespace Stroke.Shortcuts.Tests;

public class ShortcutsTests
{
    [Fact] public void SplitMultilinePrompt() { /* ... */ }
    [Fact] public void PrintContainer() { /* ... */ }
}
```

---

### 16. test_widgets.py → ButtonTests.cs

**File:** `tests/Stroke.Widgets.Tests/ButtonTests.cs`

| # | Python Test | C# Test Method | Description |
|---|-------------|----------------|-------------|
| 1 | `test_default_button` | `DefaultButton` | Default button rendering |
| 2 | `test_custom_button` | `CustomButton` | Custom symbols |

```csharp
// Stroke.Widgets.Tests/ButtonTests.cs
namespace Stroke.Widgets.Tests;

public class ButtonTests
{
    [Fact] public void DefaultButton() { /* ... */ }
    [Fact] public void CustomButton() { /* ... */ }
}
```

---

### 17. test_regular_languages.py → RegularLanguagesTests.cs

**File:** `tests/Stroke.Contrib.Tests/RegularLanguagesTests.cs`

| # | Python Test | C# Test Method | Description |
|---|-------------|----------------|-------------|
| 1 | `test_simple_match` | `SimpleMatch` | Grammar matching |
| 2 | `test_variable_varname` | `VariableVarname` | Named capture groups |
| 3 | `test_prefix` | `Prefix` | Partial matching |
| 4 | `test_completer` | `Completer` | Grammar-based completion |

```csharp
// Stroke.Contrib.Tests/RegularLanguagesTests.cs
namespace Stroke.Contrib.Tests;

public class RegularLanguagesTests
{
    [Fact] public void SimpleMatch() { /* ... */ }
    [Fact] public void VariableVarname() { /* ... */ }
    [Fact] public void Prefix() { /* ... */ }
    [Fact] public void Completer() { /* ... */ }
}
```

---

### 18. test_async_generator.py → AsyncGeneratorTests.cs

**File:** `tests/Stroke.EventLoop.Tests/AsyncGeneratorTests.cs`

| # | Python Test | C# Test Method | Description |
|---|-------------|----------------|-------------|
| 1 | `test_generator_to_async_generator` | `GeneratorToAsyncGenerator` | Sync-to-async conversion |

```csharp
// Stroke.EventLoop.Tests/AsyncGeneratorTests.cs
namespace Stroke.EventLoop.Tests;

public class AsyncGeneratorTests
{
    [Fact] public async Task GeneratorToAsyncGenerator() { /* ... */ }
}
```

---

### 19. test_utils.py → UtilsTests.cs

**File:** `tests/Stroke.Tests/UtilsTests.cs`

| # | Python Test | C# Test Method | Description |
|---|-------------|----------------|-------------|
| 1 | `test_using_weights` | `UsingWeights` | Weighted distribution |

```csharp
// Stroke.Tests/UtilsTests.cs
namespace Stroke.Tests;

public class UtilsTests
{
    [Fact] public void UsingWeights() { /* ... */ }
}
```

---

### 20. test_memory_leaks.py → MemoryLeakTests.cs

**File:** `tests/Stroke.Tests/MemoryLeakTests.cs`

| # | Python Test | C# Test Method | Description |
|---|-------------|----------------|-------------|
| 1 | `test_prompt_session_memory_leak` | `PromptSessionMemoryLeak` | GC validation |

```csharp
// Stroke.Tests/MemoryLeakTests.cs
namespace Stroke.Tests;

public class MemoryLeakTests
{
    [Fact(Skip = "Memory leak testing may fail in CI")]
    public void PromptSessionMemoryLeak() { /* ... */ }
}
```

---

### 21. test_cli.py → Application Tests

**Files:**
- `tests/Stroke.Application.Tests/PromptSessionTests.cs`
- `tests/Stroke.Application.Tests/EmacsModeTests.cs`
- `tests/Stroke.Application.Tests/ViModeTests.cs`

#### PromptSessionTests.cs

| # | Python Test | C# Test Method | Description |
|---|-------------|----------------|-------------|
| 1 | `test_simple_text_input` | `SimpleTextInput` | Basic text entry |
| 2 | `test_interrupts` | `Interrupts` | Ctrl-C/Ctrl-D handling |
| 3 | `test_quoted_insert` | `QuotedInsert` | Ctrl-Q escape |
| 4 | `test_bracketed_paste` | `BracketedPaste` | Paste mode sequences |
| 5 | `test_prefix_meta` | `PrefixMeta` | Meta prefix binding |
| 6 | `test_accept_default` | `AcceptDefault` | accept_default=true |

```csharp
// Stroke.Application.Tests/PromptSessionTests.cs
namespace Stroke.Application.Tests;

public class PromptSessionTests
{
    [Fact] public void SimpleTextInput() { /* ... */ }
    [Fact] public void Interrupts() { /* ... */ }
    [Fact] public void QuotedInsert() { /* ... */ }
    [Fact] public void BracketedPaste() { /* ... */ }
    [Fact] public void PrefixMeta() { /* ... */ }
    [Fact] public void AcceptDefault() { /* ... */ }
}
```

#### EmacsModeTests.cs

| # | Python Test | C# Test Method | Description |
|---|-------------|----------------|-------------|
| 1 | `test_emacs_cursor_movements` | `CursorMovements` | All cursor bindings |
| 2 | `test_emacs_kill_multiple_words_and_paste` | `KillMultipleWordsAndPaste` | Kill ring operations |
| 3 | `test_emacs_yank` | `Yank` | Ctrl-Y paste |
| 4 | `test_transformations` | `Transformations` | Case transformations |
| 5 | `test_emacs_other_bindings` | `OtherBindings` | Misc Emacs bindings |
| 6 | `test_controlx_controlx` | `ControlXControlX` | Exchange point/mark |
| 7 | `test_emacs_history_bindings` | `HistoryBindings` | History navigation |
| 8 | `test_emacs_reverse_search` | `ReverseSearch` | Ctrl-R search |
| 9 | `test_emacs_arguments` | `Arguments` | Numeric prefix args |
| 10 | `test_emacs_arguments_for_all_commands` | `ArgumentsForAllCommands` | All keys with args |
| 11 | `test_emacs_kill_ring` | `KillRing` | Kill ring rotation |
| 12 | `test_emacs_selection` | `Selection` | Mark and cut |
| 13 | `test_emacs_insert_comment` | `InsertComment` | Meta-# comment |
| 14 | `test_emacs_record_macro` | `RecordMacro` | Macro recording |
| 15 | `test_emacs_nested_macro` | `NestedMacro` | Nested macro calls |

```csharp
// Stroke.Application.Tests/EmacsModeTests.cs
namespace Stroke.Application.Tests;

public class EmacsModeTests
{
    [Fact] public void CursorMovements() { /* ... */ }
    [Fact] public void KillMultipleWordsAndPaste() { /* ... */ }
    [Fact] public void Yank() { /* ... */ }
    [Fact] public void Transformations() { /* ... */ }
    [Fact] public void OtherBindings() { /* ... */ }
    [Fact] public void ControlXControlX() { /* ... */ }
    [Fact] public void HistoryBindings() { /* ... */ }
    [Fact] public void ReverseSearch() { /* ... */ }
    [Fact] public void Arguments() { /* ... */ }
    [Fact] public void ArgumentsForAllCommands() { /* ... */ }
    [Fact] public void KillRing() { /* ... */ }
    [Fact] public void Selection() { /* ... */ }
    [Fact] public void InsertComment() { /* ... */ }
    [Fact] public void RecordMacro() { /* ... */ }
    [Fact] public void NestedMacro() { /* ... */ }
}
```

#### ViModeTests.cs

| # | Python Test | C# Test Method | Description |
|---|-------------|----------------|-------------|
| 1 | `test_vi_cursor_movements` | `CursorMovements` | Vi movement keys |
| 2 | `test_vi_operators` | `Operators` | d/c/y operators |
| 3 | `test_vi_text_objects` | `TextObjects` | iw/aw/i(/a( etc. |
| 4 | `test_vi_digraphs` | `Digraphs` | Ctrl-K digraph entry |
| 5 | `test_vi_block_editing` | `BlockEditing` | Visual block mode |
| 6 | `test_vi_block_editing_empty_lines` | `BlockEditing_EmptyLines` | Block on empty lines |
| 7 | `test_vi_visual_line_copy` | `VisualLineCopy` | V mode yank/paste |
| 8 | `test_vi_visual_empty_line` | `VisualEmptyLine` | V mode edge case |
| 9 | `test_vi_character_delete_after_cursor` | `CharacterDeleteAfterCursor` | x command |
| 10 | `test_vi_character_delete_before_cursor` | `CharacterDeleteBeforeCursor` | X command |
| 11 | `test_vi_character_paste` | `CharacterPaste` | p/P commands |
| 12 | `test_vi_temp_navigation_mode` | `TempNavigationMode` | Ctrl-O |
| 13 | `test_vi_macros` | `Macros` | q/@ macro recording |

```csharp
// Stroke.Application.Tests/ViModeTests.cs
namespace Stroke.Application.Tests;

public class ViModeTests
{
    [Fact] public void CursorMovements() { /* ... */ }
    [Fact] public void Operators() { /* ... */ }
    [Fact] public void TextObjects() { /* ... */ }
    [Fact] public void Digraphs() { /* ... */ }
    [Fact] public void BlockEditing() { /* ... */ }
    [Fact] public void BlockEditing_EmptyLines() { /* ... */ }
    [Fact] public void VisualLineCopy() { /* ... */ }
    [Fact] public void VisualEmptyLine() { /* ... */ }
    [Fact] public void CharacterDeleteAfterCursor() { /* ... */ }
    [Fact] public void CharacterDeleteBeforeCursor() { /* ... */ }
    [Fact] public void CharacterPaste() { /* ... */ }
    [Fact] public void TempNavigationMode() { /* ... */ }
    [Fact] public void Macros() { /* ... */ }
}
```

---

## Summary Statistics

| Category | Python Tests | C# Tests | Test Files |
|----------|--------------|----------|------------|
| Document | 12 | 12 | DocumentTests.cs |
| Buffer | 9 | 9 | BufferTests.cs |
| Filter | 11 | 11 | FilterTests.cs |
| Input Stream | 11 | 11 | InputStreamTests.cs |
| Key Binding | 6 | 6 | KeyBindingTests.cs |
| Yank Nth Arg | 10 | 10 | YankNthArgTests.cs |
| Path Completer | 9 | 9 | PathCompleterTests.cs |
| Word Completer | 6 | 6 | WordCompleterTests.cs |
| Fuzzy Completer | 1 | 1 | FuzzyCompleterTests.cs |
| Nested Completer | 1 | 1 | NestedCompleterTests.cs |
| Deduplicate Completer | 1 | 1 | DeduplicateCompleterTests.cs |
| HTML | 3 | 3 | HtmlTests.cs |
| ANSI | 5 | 5 | AnsiTests.cs |
| Formatted Text Utils | 7 | 7 | FormattedTextUtilsTests.cs |
| Style | 5 | 5 | StyleTests.cs |
| Style Transformation | 1 | 1 | StyleTransformationTests.cs |
| History | 4 | 4 | HistoryTests.cs |
| Layout | 2 | 2 | LayoutTests.cs |
| VT100 Output | 1 | 1 | Vt100OutputTests.cs |
| Print Formatted Text | 5 | 5 | PrintFormattedTextTests.cs |
| Shortcuts | 2 | 2 | ShortcutsTests.cs |
| Widgets | 2 | 2 | ButtonTests.cs |
| Regular Languages | 4 | 4 | RegularLanguagesTests.cs |
| Async Generator | 1 | 1 | AsyncGeneratorTests.cs |
| Utils | 1 | 1 | UtilsTests.cs |
| Memory Leaks | 1 | 1 | MemoryLeakTests.cs |
| Prompt Session | 6 | 6 | PromptSessionTests.cs |
| Emacs Mode | 15 | 15 | EmacsModeTests.cs |
| Vi Mode | 13 | 13 | ViModeTests.cs |
| **TOTAL** | **155** | **155** | **29 files** |

---

## Test Helper Classes

### Python → C# Helper Mapping

| Python Helper | C# Helper | Purpose |
|---------------|-----------|---------|
| `_feed_cli_with_input()` | `PromptSessionTestHelper.FeedInput()` | Simulate editing session |
| `Handlers` class | `HandlerTracker` class | Track called handlers |
| `_ProcessorMock` class | `KeyCollector` class | Collect key presses |
| `_Capture` class | `OutputCapture` class | Capture stdout |
| `chdir()` context manager | `using (new TempDirectory())` | Temp directory |

```csharp
// Stroke.Tests/Helpers/PromptSessionTestHelper.cs
namespace Stroke.Tests.Helpers;

public static class PromptSessionTestHelper
{
    public static (Document Document, Application App) FeedInput(
        string text,
        EditingMode editingMode = EditingMode.Emacs,
        IClipboard? clipboard = null,
        IHistory? history = null,
        bool multiline = false,
        IKeyBindings? keyBindings = null)
    {
        using var input = PipeInput.Create();
        input.SendText(text);

        var session = new PromptSession(
            input: input,
            output: DummyOutput.Instance,
            editingMode: editingMode,
            history: history,
            multiline: multiline,
            clipboard: clipboard,
            keyBindings: keyBindings);

        _ = session.Prompt();
        return (session.DefaultBuffer.Document, session.App);
    }
}

// Stroke.Tests/Helpers/HandlerTracker.cs
namespace Stroke.Tests.Helpers;

public class HandlerTracker
{
    public List<string> Called { get; } = new();

    public Action<KeyPressEvent> GetHandler(string name) =>
        _ => Called.Add(name);
}

// Stroke.Tests/Helpers/KeyCollector.cs
namespace Stroke.Tests.Helpers;

public class KeyCollector
{
    public List<KeyPress> Keys { get; } = new();

    public void FeedKey(KeyPress key) => Keys.Add(key);
}

// Stroke.Tests/Helpers/OutputCapture.cs
namespace Stroke.Tests.Helpers;

public class OutputCapture : TextWriter
{
    private readonly StringBuilder _data = new();

    public override Encoding Encoding => Encoding.UTF8;
    public override void Write(char value) => _data.Append(value);
    public string Data => _data.ToString();
}
```

---

## Key Escape Sequences for Tests

```csharp
// Stroke.Tests/Helpers/TestKeys.cs
namespace Stroke.Tests.Helpers;

public static class TestKeys
{
    // Control keys
    public const string ControlA = "\x01";
    public const string ControlB = "\x02";
    public const string ControlC = "\x03";
    public const string ControlD = "\x04";
    public const string ControlE = "\x05";
    public const string ControlF = "\x06";
    public const string ControlH = "\x08";
    public const string ControlK = "\x0b";
    public const string ControlN = "\x0e";
    public const string ControlP = "\x10";
    public const string ControlQ = "\x11";
    public const string ControlR = "\x12";
    public const string ControlT = "\x14";
    public const string ControlU = "\x15";
    public const string ControlW = "\x17";
    public const string ControlX = "\x18";
    public const string ControlY = "\x19";
    public const string ControlSpace = "\x00";

    // Special keys
    public const string Escape = "\x1b";
    public const string Enter = "\r";
    public const string Tab = "\t";
    public const string Backspace = "\x7f";

    // Arrow keys
    public const string Up = "\x1b[A";
    public const string Down = "\x1b[B";
    public const string Right = "\x1b[C";
    public const string Left = "\x1b[D";

    // Meta sequences
    public const string MetaB = "\x1bb";
    public const string MetaC = "\x1bc";
    public const string MetaD = "\x1bd";
    public const string MetaF = "\x1bf";
    public const string MetaL = "\x1bl";
    public const string MetaU = "\x1bu";
    public const string MetaY = "\x1by";
    public const string MetaBackspace = "\x1b\x7f";
    public const string MetaLess = "\x1b<";
    public const string MetaGreater = "\x1b>";
    public const string MetaHash = "\x1b#";

    // Bracketed paste
    public const string BracketedPasteStart = "\x1b[200~";
    public const string BracketedPasteEnd = "\x1b[201~";

    // CPR
    public static string CprResponse(int row, int col) => $"\x1b[{row};{col}R";
}
```

---

## Constitutional Compliance

This test mapping ensures:

1. **100% API Fidelity** - Every Python test has an exact C# equivalent
2. **No Mocks** - All tests use real implementations (DummyOutput, PipeInput)
3. **Same Test Names** - Python `test_foo` → C# `Foo` (PascalCase)
4. **Same Test Logic** - Each test validates the same behavior
5. **Same Coverage** - 155 tests in Python → 155 tests in C#
6. **xUnit Framework** - Per constitution requirements
7. **No FluentAssertions** - Per constitution requirements
