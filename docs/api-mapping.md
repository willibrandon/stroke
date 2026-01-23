# Python Prompt Toolkit to Stroke API Mapping

This document provides a comprehensive mapping of every Python Prompt Toolkit public API to its Stroke (.NET) equivalent.

## Naming Conventions

| Python Convention | C# Convention | Example |
|-------------------|---------------|---------|
| `snake_case` | `PascalCase` | `get_app` → `GetApp` |
| `_private` | `private` | `_cache` → `_cache` |
| `UPPER_CASE` | `PascalCase` | `DEFAULT_BUFFER` → `DefaultBuffer` |
| `lowercase` module | `PascalCase` namespace | `prompt_toolkit.buffer` → `Stroke.Core` |

## Namespace Mapping

| Python Package | .NET Namespace |
|----------------|----------------|
| `prompt_toolkit` | `Stroke` |
| `prompt_toolkit.application` | `Stroke.Application` |
| `prompt_toolkit.auto_suggest` | `Stroke.AutoSuggest` |
| `prompt_toolkit.buffer` | `Stroke.Core` |
| `prompt_toolkit.cache` | `Stroke.Core` |
| `prompt_toolkit.clipboard` | `Stroke.Clipboard` |
| `prompt_toolkit.completion` | `Stroke.Completion` |
| `prompt_toolkit.contrib` | `Stroke.Contrib` |
| `prompt_toolkit.contrib.completers` | `Stroke.Contrib.Completers` |
| `prompt_toolkit.contrib.regular_languages` | `Stroke.Contrib.RegularLanguages` |
| `prompt_toolkit.contrib.ssh` | `Stroke.Contrib.Ssh` |
| `prompt_toolkit.contrib.telnet` | `Stroke.Contrib.Telnet` |
| `prompt_toolkit.cursor_shapes` | `Stroke.CursorShapes` |
| `prompt_toolkit.data_structures` | `Stroke.Core` |
| `prompt_toolkit.document` | `Stroke.Core` |
| `prompt_toolkit.enums` | `Stroke.Core` |
| `prompt_toolkit.eventloop` | `Stroke.EventLoop` |
| `prompt_toolkit.filters` | `Stroke.Filters` |
| `prompt_toolkit.formatted_text` | `Stroke.FormattedText` |
| `prompt_toolkit.history` | `Stroke.History` |
| `prompt_toolkit.input` | `Stroke.Input` |
| `prompt_toolkit.key_binding` | `Stroke.KeyBinding` |
| `prompt_toolkit.key_binding.bindings` | `Stroke.KeyBinding.Bindings` |
| `prompt_toolkit.keys` | `Stroke.Input` |
| `prompt_toolkit.layout` | `Stroke.Layout` |
| `prompt_toolkit.lexers` | `Stroke.Lexers` |
| `prompt_toolkit.log` | `Stroke.Logging` |
| `prompt_toolkit.mouse_events` | `Stroke.Input` |
| `prompt_toolkit.output` | `Stroke.Output` |
| `prompt_toolkit.patch_stdout` | `Stroke.Application` |
| `prompt_toolkit.renderer` | `Stroke.Rendering` |
| `prompt_toolkit.search` | `Stroke.Core` |
| `prompt_toolkit.selection` | `Stroke.Core` |
| `prompt_toolkit.shortcuts` | `Stroke.Shortcuts` |
| `prompt_toolkit.styles` | `Stroke.Styles` |
| `prompt_toolkit.token` | `Stroke.FormattedText` |
| `prompt_toolkit.utils` | `Stroke.Core` |
| `prompt_toolkit.validation` | `Stroke.Validation` |
| `prompt_toolkit.widgets` | `Stroke.Widgets` |

---

## Module: prompt_toolkit (Root)

### Classes

| Python | Stroke | Notes |
|--------|--------|-------|
| `Application` | `Application<TResult>` | Generic for result type |
| `PromptSession` | `PromptSession<TResult>` | Generic for result type |
| `HTML` | `Html` | Formatted text from HTML |
| `ANSI` | `Ansi` | Formatted text from ANSI |

### Functions

| Python | Stroke | Notes |
|--------|--------|-------|
| `prompt(message)` | `Prompt.PromptAsync(message)` | Async in .NET |
| `choice(title, values)` | `Choice.ChoiceAsync<T>(title, values)` | Generic async |
| `print_formatted_text(text)` | `FormattedTextOutput.Print(text)` | Static method |

### Constants

| Python | Stroke | Type |
|--------|--------|------|
| `__version__` | `StrokeInfo.Version` | `string` |
| `VERSION` | `StrokeInfo.VersionTuple` | `(int, int, int)` |

---

## Module: prompt_toolkit.application

### Classes

| Python | Stroke | Notes |
|--------|--------|-------|
| `Application` | `Application<TResult>` | Main application class |
| `AppSession` | `AppSession` | Application session context |
| `DummyApplication` | `DummyApplication` | Placeholder when no app running |

### Application Class

```python
# Python
class Application:
    def __init__(self, layout, style, ...) -> None
    def run(self, pre_run=None, ...) -> T
    async def run_async(self, pre_run=None, ...) -> T
    def exit(self, result=None, exception=None) -> None
    def set_result(self, result) -> None
    def set_exception(self, exception) -> None
    def invalidate(self) -> None
    async def run_system_command(self, command, ...) -> None
    def suspend_to_background(self, suspend_group=True) -> None
    @property
    def current_buffer(self) -> Buffer
    @property
    def is_done(self) -> bool
    @property
    def is_running(self) -> bool
```

```csharp
// Stroke
public class Application<TResult>
{
    public Application(IContainer? layout = null, IStyle? style = null, ...);
    public TResult Run(Action? preRun = null, ...);
    public Task<TResult> RunAsync(Action? preRun = null, ...);
    public void Exit(TResult? result = default, Exception? exception = null);
    public void SetResult(TResult result);
    public void SetException(Exception exception);
    public void Invalidate();
    public Task RunSystemCommandAsync(string command, ...);
    public void SuspendToBackground(bool suspendGroup = true);
    public Buffer CurrentBuffer { get; }
    public bool IsDone { get; }
    public bool IsRunning { get; }
}
```

### Functions

| Python | Stroke | Signature |
|--------|--------|-----------|
| `get_app()` | `Current.GetApp()` | `Application GetApp()` |
| `get_app_or_none()` | `Current.GetAppOrNone()` | `Application? GetAppOrNone()` |
| `set_app(app)` | `Current.SetApp(app)` | `void SetApp(Application? app)` |
| `get_app_session()` | `Current.GetAppSession()` | `AppSession GetAppSession()` |
| `create_app_session(input, output)` | `AppSession.Create(input, output)` | `AppSession Create(...)` |
| `create_app_session_from_tty()` | `AppSession.CreateFromTty()` | `AppSession CreateFromTty()` |
| `run_in_terminal(func)` | `RunInTerminal.RunAsync(func)` | `Task<T> RunAsync<T>(Func<T> func)` |
| `in_terminal()` | `RunInTerminal.InTerminal()` | `IAsyncDisposable InTerminal()` |

---

## Module: prompt_toolkit.auto_suggest

### Classes

| Python | Stroke | Notes |
|--------|--------|-------|
| `Suggestion` | `Suggestion` | Immutable suggestion data |
| `AutoSuggest` | `IAutoSuggest` | Interface (abstract in Python) |
| `ThreadedAutoSuggest` | `ThreadedAutoSuggest` | Threaded wrapper |
| `DummyAutoSuggest` | `DummyAutoSuggest` | No-op implementation |
| `AutoSuggestFromHistory` | `AutoSuggestFromHistory` | History-based suggestions |
| `ConditionalAutoSuggest` | `ConditionalAutoSuggest` | Conditional wrapper |
| `DynamicAutoSuggest` | `DynamicAutoSuggest` | Dynamic wrapper |

### Suggestion Class

```python
# Python
class Suggestion:
    def __init__(self, text: str) -> None
    @property
    def text(self) -> str
```

```csharp
// Stroke
public sealed record Suggestion(string Text);
```

### IAutoSuggest Interface

```python
# Python
class AutoSuggest(ABC):
    @abstractmethod
    def get_suggestion(self, buffer, document) -> Suggestion | None
    async def get_suggestion_async(self, buff, document) -> Suggestion | None
```

```csharp
// Stroke
public interface IAutoSuggest
{
    Suggestion? GetSuggestion(Buffer buffer, Document document);
    Task<Suggestion?> GetSuggestionAsync(Buffer buffer, Document document);
}
```

---

## Module: prompt_toolkit.buffer

### Classes

| Python | Stroke | Notes |
|--------|--------|-------|
| `Buffer` | `Buffer` | Mutable text buffer |
| `CompletionState` | `CompletionState` | Tracks completion state |

### Exceptions

| Python | Stroke |
|--------|--------|
| `EditReadOnlyBuffer` | `EditReadOnlyBufferException` |

### Buffer Class

```python
# Python
class Buffer:
    def __init__(self, document=None, ...) -> None
    @property
    def text(self) -> str
    @text.setter
    def text(self, value: str) -> None
    @property
    def cursor_position(self) -> int
    @cursor_position.setter
    def cursor_position(self, value: int) -> None
    @property
    def document(self) -> Document
    @document.setter
    def document(self, value: Document) -> None
    def insert_text(self, data, ...) -> None
    def delete(self, count=1) -> str
    def delete_before_cursor(self, count=1) -> str
    def newline(self, copy_margin=True) -> None
    def undo(self) -> None
    def redo(self) -> None
    def validate(self, set_cursor=False) -> bool
    def validate_and_handle(self) -> None
    def start_completion(self, ...) -> None
    def complete_next(self, count=1, ...) -> None
    def complete_previous(self, count=1, ...) -> None
    def cancel_completion(self) -> None
    def apply_completion(self, completion) -> None
    def go_to_completion(self, index) -> None
    def set_completions(self, completions, ...) -> None
    def start_history_lines_completion(self) -> None
    def go_to_history(self, index) -> None
    def append_to_history(self) -> None
    def history_forward(self, count=1) -> None
    def history_backward(self, count=1) -> None
    def yank_nth_arg(self, n=None, ...) -> None
    def yank_last_arg(self, n=None) -> None
    def start_selection(self, ...) -> None
    def copy_selection(self, ...) -> ClipboardData
    def cut_selection(self) -> ClipboardData
    def paste_clipboard_data(self, data, ...) -> None
    def transform_lines(self, ...) -> None
    def transform_region(self, ...) -> None
    def cursor_left(self, count=1) -> None
    def cursor_right(self, count=1) -> None
    def cursor_up(self, count=1) -> None
    def cursor_down(self, count=1) -> None
    def auto_up(self, count=1, ...) -> None
    def auto_down(self, count=1, ...) -> None
    def swap_characters_before_cursor(self) -> None
    def go_to_matching_bracket(self) -> None
    def open_in_editor(self, ...) -> Task[None]
    @property
    def working_index(self) -> int
    @property
    def selection_state(self) -> SelectionState | None
    @property
    def preferred_column(self) -> int | None
    @property
    def complete_state(self) -> CompletionState | None
    @property
    def complete_while_typing(self) -> bool
```

```csharp
// Stroke
public class Buffer
{
    public Buffer(Document? document = null, ...);
    public string Text { get; set; }
    public int CursorPosition { get; set; }
    public Document Document { get; set; }
    public void InsertText(string data, ...);
    public string Delete(int count = 1);
    public string DeleteBeforeCursor(int count = 1);
    public void Newline(bool copyMargin = true);
    public void Undo();
    public void Redo();
    public bool Validate(bool setCursor = false);
    public void ValidateAndHandle();
    public void StartCompletion(...);
    public void CompleteNext(int count = 1, ...);
    public void CompletePrevious(int count = 1, ...);
    public void CancelCompletion();
    public void ApplyCompletion(Completion completion);
    public void GoToCompletion(int index);
    public void SetCompletions(IReadOnlyList<Completion> completions, ...);
    public void StartHistoryLinesCompletion();
    public void GoToHistory(int index);
    public void AppendToHistory();
    public void HistoryForward(int count = 1);
    public void HistoryBackward(int count = 1);
    public void YankNthArg(int? n = null, ...);
    public void YankLastArg(int? n = null);
    public void StartSelection(...);
    public ClipboardData CopySelection(...);
    public ClipboardData CutSelection();
    public void PasteClipboardData(ClipboardData data, ...);
    public void TransformLines(...);
    public void TransformRegion(...);
    public void CursorLeft(int count = 1);
    public void CursorRight(int count = 1);
    public void CursorUp(int count = 1);
    public void CursorDown(int count = 1);
    public void AutoUp(int count = 1, ...);
    public void AutoDown(int count = 1, ...);
    public void SwapCharactersBeforeCursor();
    public void GoToMatchingBracket();
    public Task OpenInEditorAsync(...);
    public int WorkingIndex { get; }
    public SelectionState? SelectionState { get; }
    public int? PreferredColumn { get; }
    public CompletionState? CompleteState { get; }
    public bool CompleteWhileTyping { get; }
}
```

### Functions

| Python | Stroke | Signature |
|--------|--------|-----------|
| `indent(buffer, from_row, to_row, count)` | `BufferOperations.Indent(...)` | `void Indent(Buffer buffer, int fromRow, int toRow, int count = 1)` |
| `unindent(buffer, from_row, to_row, count)` | `BufferOperations.Unindent(...)` | `void Unindent(Buffer buffer, int fromRow, int toRow, int count = 1)` |
| `reshape_text(buffer, from_row, to_row)` | `BufferOperations.ReshapeText(...)` | `void ReshapeText(Buffer buffer, int fromRow, int toRow)` |

---

## Module: prompt_toolkit.cache

### Classes

| Python | Stroke | Notes |
|--------|--------|-------|
| `SimpleCache` | `SimpleCache<TKey, TValue>` | Generic LRU cache |
| `FastDictCache` | `FastDictCache<TKey, TValue>` | Generic fast cache |

### Functions

| Python | Stroke | Notes |
|--------|--------|-------|
| `memoized` (decorator) | `[Memoized]` attribute or `Memoize.Create<T>()` | Attribute or factory |

### SimpleCache Class

```python
# Python
class SimpleCache:
    def __init__(self, maxsize=8) -> None
    def get(self, key, getter) -> V
```

```csharp
// Stroke
public class SimpleCache<TKey, TValue> where TKey : notnull
{
    public SimpleCache(int maxSize = 8);
    public TValue Get(TKey key, Func<TValue> getter);
}
```

---

## Module: prompt_toolkit.clipboard

### Classes

| Python | Stroke | Notes |
|--------|--------|-------|
| `Clipboard` | `IClipboard` | Interface (abstract in Python) |
| `ClipboardData` | `ClipboardData` | Immutable clipboard content |
| `DummyClipboard` | `DummyClipboard` | No-op clipboard |
| `DynamicClipboard` | `DynamicClipboard` | Dynamic wrapper |
| `InMemoryClipboard` | `InMemoryClipboard` | In-memory storage |

### ClipboardData Class

```python
# Python
class ClipboardData:
    def __init__(self, text='', type=SelectionType.CHARACTERS) -> None
    @property
    def text(self) -> str
    @property
    def type(self) -> SelectionType
```

```csharp
// Stroke
public sealed record ClipboardData(
    string Text = "",
    SelectionType Type = SelectionType.Characters);
```

### IClipboard Interface

```python
# Python
class Clipboard(ABC):
    @abstractmethod
    def set_data(self, data: ClipboardData) -> None
    @abstractmethod
    def get_data(self) -> ClipboardData
    def set_text(self, text: str) -> None
    def rotate(self) -> None
```

```csharp
// Stroke
public interface IClipboard
{
    void SetData(ClipboardData data);
    ClipboardData GetData();
    void SetText(string text);
    void Rotate();
}
```

---

## Module: prompt_toolkit.completion

### Classes

| Python | Stroke | Notes |
|--------|--------|-------|
| `Completion` | `Completion` | Single completion item |
| `Completer` | `ICompleter` | Interface (abstract in Python) |
| `CompleteEvent` | `CompleteEvent` | Completion trigger event |
| `ThreadedCompleter` | `ThreadedCompleter` | Threaded wrapper |
| `DummyCompleter` | `DummyCompleter` | No-op completer |
| `DynamicCompleter` | `DynamicCompleter` | Dynamic wrapper |
| `ConditionalCompleter` | `ConditionalCompleter` | Conditional wrapper |
| `WordCompleter` | `WordCompleter` | Word-based completion |
| `PathCompleter` | `PathCompleter` | File path completion |
| `ExecutableCompleter` | `ExecutableCompleter` | Executable completion |
| `FuzzyCompleter` | `FuzzyCompleter` | Fuzzy matching wrapper |
| `FuzzyWordCompleter` | `FuzzyWordCompleter` | Fuzzy word completion |
| `NestedCompleter` | `NestedCompleter` | Nested/hierarchical completion |
| `DeduplicateCompleter` | `DeduplicateCompleter` | Deduplication wrapper |

### Completion Class

```python
# Python
class Completion:
    def __init__(self, text, start_position=0, display=None,
                 display_meta=None, style='', selected_style='') -> None
    @property
    def text(self) -> str
    @property
    def start_position(self) -> int
    @property
    def display(self) -> AnyFormattedText
    @property
    def display_meta(self) -> AnyFormattedText
    @property
    def style(self) -> str
    @property
    def selected_style(self) -> str
    def new_completion_from_position(self, position) -> Completion
```

```csharp
// Stroke
public sealed record Completion(
    string Text,
    int StartPosition = 0,
    AnyFormattedText? Display = null,
    AnyFormattedText? DisplayMeta = null,
    string Style = "",
    string SelectedStyle = "")
{
    public Completion NewCompletionFromPosition(int position);
}
```

### ICompleter Interface

```python
# Python
class Completer(ABC):
    @abstractmethod
    def get_completions(self, document, complete_event) -> Iterable[Completion]
    async def get_completions_async(self, document, event) -> AsyncGenerator[Completion]
```

```csharp
// Stroke
public interface ICompleter
{
    IEnumerable<Completion> GetCompletions(Document document, CompleteEvent completeEvent);
    IAsyncEnumerable<Completion> GetCompletionsAsync(Document document, CompleteEvent completeEvent);
}
```

### Functions

| Python | Stroke | Signature |
|--------|--------|-----------|
| `merge_completers(completers)` | `CompleterExtensions.Merge(...)` | `ICompleter Merge(this IEnumerable<ICompleter> completers)` |
| `get_common_complete_suffix(doc, completions)` | `CompletionUtils.GetCommonSuffix(...)` | `string GetCommonSuffix(Document doc, IEnumerable<Completion> completions)` |

---

## Module: prompt_toolkit.cursor_shapes

### Enums

| Python | Stroke |
|--------|--------|
| `CursorShape.BLOCK` | `CursorShape.Block` |
| `CursorShape.BEAM` | `CursorShape.Beam` |
| `CursorShape.UNDERLINE` | `CursorShape.Underline` |
| `CursorShape.BLINKING_BLOCK` | `CursorShape.BlinkingBlock` |
| `CursorShape.BLINKING_BEAM` | `CursorShape.BlinkingBeam` |
| `CursorShape.BLINKING_UNDERLINE` | `CursorShape.BlinkingUnderline` |

### Classes

| Python | Stroke | Notes |
|--------|--------|-------|
| `CursorShapeConfig` | `ICursorShapeConfig` | Interface |
| `SimpleCursorShapeConfig` | `SimpleCursorShapeConfig` | Static shape |
| `ModalCursorShapeConfig` | `ModalCursorShapeConfig` | Mode-dependent shape |
| `DynamicCursorShapeConfig` | `DynamicCursorShapeConfig` | Dynamic wrapper |

### Functions

| Python | Stroke | Signature |
|--------|--------|-----------|
| `to_cursor_shape_config(value)` | `CursorShapeConfig.From(value)` | `ICursorShapeConfig From(object? value)` |

---

## Module: prompt_toolkit.data_structures

### Types

| Python | Stroke | Notes |
|--------|--------|-------|
| `Point` | `Point` | `readonly record struct` |
| `Size` | `Size` | `readonly record struct` |

```python
# Python
class Point(NamedTuple):
    x: int
    y: int
```

```csharp
// Stroke
public readonly record struct Point(int X, int Y);
```

```python
# Python
class Size(NamedTuple):
    rows: int
    columns: int
```

```csharp
// Stroke
public readonly record struct Size(int Rows, int Columns);
```

---

## Module: prompt_toolkit.document

### Classes

| Python | Stroke | Notes |
|--------|--------|-------|
| `Document` | `Document` | Immutable text document |

### Document Class

```python
# Python
class Document:
    def __init__(self, text='', cursor_position=None, selection=None) -> None
    @property
    def text(self) -> str
    @property
    def cursor_position(self) -> int
    @property
    def selection(self) -> SelectionState | None
    @property
    def current_char(self) -> str
    @property
    def char_before_cursor(self) -> str
    @property
    def text_before_cursor(self) -> str
    @property
    def text_after_cursor(self) -> str
    @property
    def current_line_before_cursor(self) -> str
    @property
    def current_line_after_cursor(self) -> str
    @property
    def current_line(self) -> str
    @property
    def lines(self) -> list[str]
    @property
    def line_count(self) -> int
    @property
    def cursor_position_row(self) -> int
    @property
    def cursor_position_col(self) -> int
    @property
    def is_cursor_at_the_end(self) -> bool
    @property
    def is_cursor_at_the_end_of_line(self) -> bool
    @property
    def leading_whitespace_in_current_line(self) -> str
    @property
    def on_first_line(self) -> bool
    @property
    def on_last_line(self) -> bool
    @property
    def empty_line_count_at_the_end(self) -> int
    def get_word_before_cursor(self, ...) -> str
    def get_word_under_cursor(self, ...) -> str
    def find(self, sub, ...) -> int | None
    def find_all(self, sub, ...) -> list[int]
    def find_backwards(self, sub, ...) -> int | None
    def get_cursor_left_position(self, count=1) -> int
    def get_cursor_right_position(self, count=1) -> int
    def get_cursor_up_position(self, count=1, ...) -> int
    def get_cursor_down_position(self, count=1, ...) -> int
    def get_start_of_line_position(self, ...) -> int
    def get_end_of_line_position(self) -> int
    def get_start_of_document_position(self) -> int
    def get_end_of_document_position(self) -> int
    def find_matching_bracket_position(self, ...) -> int | None
    def find_enclosing_bracket_left(self, ...) -> int | None
    def find_enclosing_bracket_right(self, ...) -> int | None
    def translate_index_to_position(self, index) -> tuple[int, int]
    def translate_row_col_to_index(self, row, col) -> int
    def get_column_cursor_position(self, column) -> int
    def selection_range(self) -> tuple[int, int]
    def selection_ranges(self) -> Iterable[tuple[int, int]]
    def selection_range_at_line(self, row) -> tuple[int, int] | None
    def cut_selection(self) -> tuple[Document, ClipboardData]
    def paste_clipboard_data(self, data, ...) -> Document
    def insert_after(self, text) -> Document
    def insert_before(self, text) -> Document
```

```csharp
// Stroke
public sealed class Document
{
    public Document(string text = "", int? cursorPosition = null, SelectionState? selection = null);
    public string Text { get; }
    public int CursorPosition { get; }
    public SelectionState? Selection { get; }
    public char CurrentChar { get; }
    public char CharBeforeCursor { get; }
    public string TextBeforeCursor { get; }
    public string TextAfterCursor { get; }
    public string CurrentLineBeforeCursor { get; }
    public string CurrentLineAfterCursor { get; }
    public string CurrentLine { get; }
    public IReadOnlyList<string> Lines { get; }
    public int LineCount { get; }
    public int CursorPositionRow { get; }
    public int CursorPositionCol { get; }
    public bool IsCursorAtTheEnd { get; }
    public bool IsCursorAtTheEndOfLine { get; }
    public string LeadingWhitespaceInCurrentLine { get; }
    public bool OnFirstLine { get; }
    public bool OnLastLine { get; }
    public int EmptyLineCountAtTheEnd { get; }
    public string GetWordBeforeCursor(...);
    public string GetWordUnderCursor(...);
    public int? Find(string sub, ...);
    public IReadOnlyList<int> FindAll(string sub, ...);
    public int? FindBackwards(string sub, ...);
    public int GetCursorLeftPosition(int count = 1);
    public int GetCursorRightPosition(int count = 1);
    public int GetCursorUpPosition(int count = 1, ...);
    public int GetCursorDownPosition(int count = 1, ...);
    public int GetStartOfLinePosition(...);
    public int GetEndOfLinePosition();
    public int GetStartOfDocumentPosition();
    public int GetEndOfDocumentPosition();
    public int? FindMatchingBracketPosition(...);
    public int? FindEnclosingBracketLeft(...);
    public int? FindEnclosingBracketRight(...);
    public (int Row, int Col) TranslateIndexToPosition(int index);
    public int TranslateRowColToIndex(int row, int col);
    public int GetColumnCursorPosition(int column);
    public (int Start, int End) SelectionRange();
    public IEnumerable<(int Start, int End)> SelectionRanges();
    public (int Start, int End)? SelectionRangeAtLine(int row);
    public (Document Document, ClipboardData Data) CutSelection();
    public Document PasteClipboardData(ClipboardData data, ...);
    public Document InsertAfter(string text);
    public Document InsertBefore(string text);
}
```

---

## Module: prompt_toolkit.enums

### Enums

| Python | Stroke |
|--------|--------|
| `EditingMode.EMACS` | `EditingMode.Emacs` |
| `EditingMode.VI` | `EditingMode.Vi` |

### Constants

| Python | Stroke | Type |
|--------|--------|------|
| `DEFAULT_BUFFER` | `BufferNames.Default` | `string` |
| `SEARCH_BUFFER` | `BufferNames.Search` | `string` |
| `SYSTEM_BUFFER` | `BufferNames.System` | `string` |

---

## Module: prompt_toolkit.eventloop

### Classes

| Python | Stroke | Notes |
|--------|--------|-------|
| `InputHook` | `InputHook` | Delegate type |
| `InputHookContext` | `InputHookContext` | Context for input hooks |
| `InputHookSelector` | `InputHookSelector` | Selector with input hook |

### Functions

| Python | Stroke | Signature |
|--------|--------|-----------|
| `generator_to_async_generator(gen, buffer_size)` | `AsyncGeneratorUtils.GeneratorToAsyncGenerator<T>(...)` | `IAsyncEnumerable<T> GeneratorToAsyncGenerator<T>(Func<IEnumerable<T>> gen, int bufferSize = 1000)` |
| `aclosing(agen)` | `AsyncGeneratorUtils.Aclosing<T>(agen)` | `IAsyncDisposableValue<IAsyncEnumerable<T>> Aclosing<T>(IAsyncEnumerable<T> agen)` |
| `run_in_executor_with_context(func)` | `EventLoopUtils.RunInExecutorWithContextAsync(func)` | `Task<T> RunInExecutorWithContextAsync<T>(Func<T> func)` |
| `call_soon_threadsafe(func)` | `EventLoopUtils.CallSoonThreadSafe(action)` | `void CallSoonThreadSafe(Action action)` |
| `get_traceback_from_context(context)` | `EventLoopUtils.GetTracebackFromContext(context)` | `string? GetTracebackFromContext(IDictionary context)` |
| `new_eventloop_with_inputhook(hook)` | `EventLoopUtils.NewEventLoopWithInputHook(hook)` | `... NewEventLoopWithInputHook(InputHook hook)` |
| `set_eventloop_with_inputhook(hook)` | `EventLoopUtils.SetEventLoopWithInputHook(hook)` | `void SetEventLoopWithInputHook(InputHook hook)` |

### InputHook Delegate

```python
# Python
InputHook = Callable[['InputHookContext'], None]
```

```csharp
// Stroke
public delegate void InputHook(InputHookContext context);
```

---

## Module: prompt_toolkit.filters

### Classes

| Python | Stroke | Notes |
|--------|--------|-------|
| `Filter` | `IFilter` | Interface (abstract in Python) |
| `Condition` | `Condition` | Callable wrapper |
| `Never` | `Never` | Always false |
| `Always` | `Always` | Always true |

### Filter Functions (Conditions)

| Python | Stroke | Notes |
|--------|--------|-------|
| `has_arg` | `Filters.HasArg` | `IFilter` property |
| `has_completions` | `Filters.HasCompletions` | `IFilter` property |
| `completion_is_selected` | `Filters.CompletionIsSelected` | `IFilter` property |
| `has_focus(value)` | `Filters.HasFocus(value)` | Returns `IFilter` |
| `buffer_has_focus` | `Filters.BufferHasFocus` | `IFilter` property |
| `has_selection` | `Filters.HasSelection` | `IFilter` property |
| `has_validation_error` | `Filters.HasValidationError` | `IFilter` property |
| `is_done` | `Filters.IsDone` | `IFilter` property |
| `is_read_only` | `Filters.IsReadOnly` | `IFilter` property |
| `is_multiline` | `Filters.IsMultiline` | `IFilter` property |
| `renderer_height_is_known` | `Filters.RendererHeightIsKnown` | `IFilter` property |
| `in_editing_mode(mode)` | `Filters.InEditingMode(mode)` | Returns `IFilter` |
| `in_paste_mode` | `Filters.InPasteMode` | `IFilter` property |
| `vi_mode` | `Filters.ViMode` | `IFilter` property |
| `vi_navigation_mode` | `Filters.ViNavigationMode` | `IFilter` property |
| `vi_insert_mode` | `Filters.ViInsertMode` | `IFilter` property |
| `vi_insert_multiple_mode` | `Filters.ViInsertMultipleMode` | `IFilter` property |
| `vi_replace_mode` | `Filters.ViReplaceMode` | `IFilter` property |
| `vi_selection_mode` | `Filters.ViSelectionMode` | `IFilter` property |
| `vi_waiting_for_text_object_mode` | `Filters.ViWaitingForTextObjectMode` | `IFilter` property |
| `vi_digraph_mode` | `Filters.ViDigraphMode` | `IFilter` property |
| `vi_recording_macro` | `Filters.ViRecordingMacro` | `IFilter` property |
| `emacs_mode` | `Filters.EmacsMode` | `IFilter` property |
| `emacs_insert_mode` | `Filters.EmacsInsertMode` | `IFilter` property |
| `emacs_selection_mode` | `Filters.EmacsSelectionMode` | `IFilter` property |
| `shift_selection_mode` | `Filters.ShiftSelectionMode` | `IFilter` property |
| `is_searching` | `Filters.IsSearching` | `IFilter` property |
| `control_is_searchable` | `Filters.ControlIsSearchable` | `IFilter` property |
| `vi_search_direction_reversed` | `Filters.ViSearchDirectionReversed` | `IFilter` property |

### Utility Functions

| Python | Stroke | Signature |
|--------|--------|-----------|
| `is_true(value)` | `FilterUtils.IsTrue(value)` | `bool IsTrue(FilterOrBool value)` |
| `to_filter(value)` | `FilterUtils.ToFilter(value)` | `IFilter ToFilter(FilterOrBool value)` |

### Type Aliases

| Python | Stroke |
|--------|--------|
| `FilterOrBool` | `FilterOrBool` (union type via implicit conversion) |

### IFilter Interface

```python
# Python
class Filter(ABC):
    @abstractmethod
    def __call__(self) -> bool
    def __and__(self, other) -> Filter
    def __or__(self, other) -> Filter
    def __invert__(self) -> Filter
```

```csharp
// Stroke
public interface IFilter
{
    bool Evaluate();
    IFilter And(IFilter other);
    IFilter Or(IFilter other);
    IFilter Not();

    // Operator overloads
    static IFilter operator &(IFilter left, IFilter right);
    static IFilter operator |(IFilter left, IFilter right);
    static IFilter operator !(IFilter filter);
}
```

---

## Module: prompt_toolkit.formatted_text

### Classes

| Python | Stroke | Notes |
|--------|--------|-------|
| `FormattedText` | `FormattedText` | List of style/text tuples |
| `HTML` | `Html` | Parse HTML to formatted text |
| `ANSI` | `Ansi` | Parse ANSI to formatted text |
| `Template` | `Template` | String template with formatting |
| `PygmentsTokens` | `PygmentsTokens` | Pygments token list |

### Type Aliases

| Python | Stroke |
|--------|--------|
| `AnyFormattedText` | `AnyFormattedText` (struct with implicit conversions) |
| `OneStyleAndTextTuple` | `StyleAndTextTuple` (`record struct`) |
| `StyleAndTextTuples` | `IReadOnlyList<StyleAndTextTuple>` |

### Functions

| Python | Stroke | Signature |
|--------|--------|-----------|
| `to_formatted_text(value, style)` | `FormattedTextUtils.ToFormattedText(value, style)` | `FormattedText ToFormattedText(AnyFormattedText value, string style = "")` |
| `is_formatted_text(value)` | `FormattedTextUtils.IsFormattedText(value)` | `bool IsFormattedText(object? value)` |
| `merge_formatted_text(items)` | `FormattedTextUtils.Merge(items)` | `FormattedText Merge(IEnumerable<AnyFormattedText> items)` |
| `fragment_list_len(fragments)` | `FormattedTextUtils.FragmentListLen(fragments)` | `int FragmentListLen(IEnumerable<StyleAndTextTuple> fragments)` |
| `fragment_list_width(fragments)` | `FormattedTextUtils.FragmentListWidth(fragments)` | `int FragmentListWidth(IEnumerable<StyleAndTextTuple> fragments)` |
| `fragment_list_to_text(fragments)` | `FormattedTextUtils.FragmentListToText(fragments)` | `string FragmentListToText(IEnumerable<StyleAndTextTuple> fragments)` |
| `split_lines(fragments)` | `FormattedTextUtils.SplitLines(fragments)` | `IEnumerable<IReadOnlyList<StyleAndTextTuple>> SplitLines(...)` |
| `to_plain_text(value)` | `FormattedTextUtils.ToPlainText(value)` | `string ToPlainText(AnyFormattedText value)` |

### StyleAndTextTuple

```python
# Python
OneStyleAndTextTuple = Union[
    Tuple[str, str],
    Tuple[str, str, Callable[[MouseEvent], None]]
]
```

```csharp
// Stroke
public readonly record struct StyleAndTextTuple(
    string Style,
    string Text,
    Action<MouseEvent>? MouseHandler = null);
```

---

## Module: prompt_toolkit.history

### Classes

| Python | Stroke | Notes |
|--------|--------|-------|
| `History` | `IHistory` | Interface (abstract in Python) |
| `ThreadedHistory` | `ThreadedHistory` | Threaded wrapper |
| `DummyHistory` | `DummyHistory` | No-op history |
| `FileHistory` | `FileHistory` | File-backed history |
| `InMemoryHistory` | `InMemoryHistory` | In-memory history |

### IHistory Interface

```python
# Python
class History(ABC):
    @abstractmethod
    def load_history_strings(self) -> Iterable[str]
    @abstractmethod
    def store_string(self, string: str) -> None
    def append_string(self, string: str) -> None
    def get_strings(self) -> list[str]
```

```csharp
// Stroke
public interface IHistory
{
    IEnumerable<string> LoadHistoryStrings();
    void StoreString(string value);
    void AppendString(string value);
    IReadOnlyList<string> GetStrings();
}
```

---

## Module: prompt_toolkit.input

### Classes

| Python | Stroke | Notes |
|--------|--------|-------|
| `Input` | `IInput` | Interface (abstract in Python) |
| `PipeInput` | `PipeInput` | Pipe-based input |
| `DummyInput` | `DummyInput` | No-op input |

### Functions

| Python | Stroke | Signature |
|--------|--------|-----------|
| `create_input(stdin, always_prefer_tty)` | `InputFactory.Create(stdin, alwaysPreferTty)` | `IInput Create(Stream? stdin = null, bool alwaysPreferTty = false)` |
| `create_pipe_input()` | `InputFactory.CreatePipe()` | `PipeInput CreatePipe()` |

### IInput Interface

```python
# Python
class Input(ABC):
    @abstractmethod
    def read_keys(self) -> Iterable[KeyPress]
    @abstractmethod
    async def read_keys_async(self) -> AsyncGenerator[KeyPress]
    @abstractmethod
    def fileno(self) -> int
    @abstractmethod
    def typeahead_hash(self) -> str
    @abstractmethod
    def close(self) -> None
    @property
    @abstractmethod
    def closed(self) -> bool
    def raw_mode(self) -> ContextManager
    def cooked_mode(self) -> ContextManager
    def attach(self, callback) -> ContextManager
    def detach(self) -> ContextManager
```

```csharp
// Stroke
public interface IInput
{
    IEnumerable<KeyPress> ReadKeys();
    IAsyncEnumerable<KeyPress> ReadKeysAsync(CancellationToken cancellationToken = default);
    IntPtr FileNo();
    string TypeaheadHash();
    void Close();
    bool Closed { get; }
    IDisposable RawMode();
    IDisposable CookedMode();
    IDisposable Attach(Action<KeyPress> callback);
    IDisposable Detach();
}
```

---

## Module: prompt_toolkit.key_binding

### Classes

| Python | Stroke | Notes |
|--------|--------|-------|
| `KeyBindings` | `KeyBindings` | Key binding registry |
| `KeyBindingsBase` | `IKeyBindings` | Interface (abstract in Python) |
| `ConditionalKeyBindings` | `ConditionalKeyBindings` | Conditional wrapper |
| `DynamicKeyBindings` | `DynamicKeyBindings` | Dynamic wrapper |
| `KeyPress` | `KeyPress` | Key press event data |
| `KeyPressEvent` | `KeyPressEvent` | Event passed to handlers |

### Functions

| Python | Stroke | Signature |
|--------|--------|-----------|
| `merge_key_bindings(bindings)` | `KeyBindingsExtensions.Merge(bindings)` | `IKeyBindings Merge(this IEnumerable<IKeyBindings> bindings)` |

### KeyBindings Class

```python
# Python
class KeyBindings:
    def add(self, *keys, filter=True, eager=False, is_global=False, ...) -> Callable
    def remove(self, *keys) -> None
    def get_bindings_for_keys(self, keys) -> list[Binding]
    def get_bindings_starting_with_keys(self, keys) -> list[Binding]
```

```csharp
// Stroke
public class KeyBindings : IKeyBindings
{
    public void Add(Keys[] keys, Action<KeyPressEvent> handler,
                    IFilter? filter = null, bool eager = false, bool isGlobal = false, ...);
    public void Remove(params Keys[] keys);
    public IReadOnlyList<Binding> GetBindingsForKeys(IReadOnlyList<KeyPress> keys);
    public IReadOnlyList<Binding> GetBindingsStartingWithKeys(IReadOnlyList<KeyPress> keys);
}
```

### KeyPress Class

```python
# Python
class KeyPress:
    def __init__(self, key: Keys, data: str | None = None) -> None
    @property
    def key(self) -> Keys
    @property
    def data(self) -> str
```

```csharp
// Stroke
public readonly record struct KeyPress(Keys Key, string? Data = null);
```

---

## Module: prompt_toolkit.keys

### Enums

| Python | Stroke | Notes |
|--------|--------|-------|
| `Keys` | `Keys` | All key constants |

### Keys Enum (Partial)

| Python | Stroke |
|--------|--------|
| `Keys.Escape` | `Keys.Escape` |
| `Keys.ControlA` | `Keys.ControlA` |
| `Keys.ControlB` | `Keys.ControlB` |
| ... | ... |
| `Keys.Enter` | `Keys.Enter` |
| `Keys.Tab` | `Keys.Tab` |
| `Keys.Backspace` | `Keys.Backspace` |
| `Keys.Delete` | `Keys.Delete` |
| `Keys.Up` | `Keys.Up` |
| `Keys.Down` | `Keys.Down` |
| `Keys.Left` | `Keys.Left` |
| `Keys.Right` | `Keys.Right` |
| `Keys.Home` | `Keys.Home` |
| `Keys.End` | `Keys.End` |
| `Keys.PageUp` | `Keys.PageUp` |
| `Keys.PageDown` | `Keys.PageDown` |
| `Keys.Insert` | `Keys.Insert` |
| `Keys.F1` - `Keys.F24` | `Keys.F1` - `Keys.F24` |
| `Keys.Any` | `Keys.Any` |
| `Keys.CPRResponse` | `Keys.CprResponse` |
| `Keys.Vt100MouseEvent` | `Keys.Vt100MouseEvent` |
| `Keys.WindowsMouseEvent` | `Keys.WindowsMouseEvent` |
| `Keys.BracketedPaste` | `Keys.BracketedPaste` |
| `Keys.ScrollUp` | `Keys.ScrollUp` |
| `Keys.ScrollDown` | `Keys.ScrollDown` |

### Constants

| Python | Stroke | Type |
|--------|--------|------|
| `ALL_KEYS` | `Keys.AllKeys` | `IReadOnlyList<Keys>` |

---

## Module: prompt_toolkit.layout

### Classes - Containers

| Python | Stroke | Notes |
|--------|--------|-------|
| `Container` | `IContainer` | Interface (abstract in Python) |
| `HSplit` | `HSplit` | Horizontal split |
| `VSplit` | `VSplit` | Vertical split |
| `FloatContainer` | `FloatContainer` | Floating windows |
| `Float` | `Float` | Single float definition |
| `Window` | `Window` | Basic window |
| `ConditionalContainer` | `ConditionalContainer` | Conditional visibility |
| `DynamicContainer` | `DynamicContainer` | Dynamic container |
| `ScrollablePane` | `ScrollablePane` | Scrollable area |

### Classes - Controls

| Python | Stroke | Notes |
|--------|--------|-------|
| `UIControl` | `IUIControl` | Interface |
| `UIContent` | `UIContent` | Control content |
| `BufferControl` | `BufferControl` | Text buffer display |
| `SearchBufferControl` | `SearchBufferControl` | Search input |
| `DummyControl` | `DummyControl` | No-op control |
| `FormattedTextControl` | `FormattedTextControl` | Formatted text display |

### Classes - Layout

| Python | Stroke | Notes |
|--------|--------|-------|
| `Layout` | `Layout` | Layout manager |
| `Dimension` | `Dimension` | Size specification |
| `ScrollOffsets` | `ScrollOffsets` | Scroll margins |
| `ColorColumn` | `ColorColumn` | Colored column marker |
| `WindowRenderInfo` | `WindowRenderInfo` | Window render state |

### Classes - Margins

| Python | Stroke | Notes |
|--------|--------|-------|
| `Margin` | `IMargin` | Interface |
| `NumberedMargin` | `NumberedMargin` | Line numbers |
| `ScrollbarMargin` | `ScrollbarMargin` | Scrollbar |
| `ConditionalMargin` | `ConditionalMargin` | Conditional visibility |
| `PromptMargin` | `PromptMargin` | Prompt display |

### Classes - Menus

| Python | Stroke | Notes |
|--------|--------|-------|
| `CompletionsMenu` | `CompletionsMenu` | Completion popup |
| `MultiColumnCompletionsMenu` | `MultiColumnCompletionsMenu` | Multi-column completion |

### Enums

| Python | Stroke |
|--------|--------|
| `HorizontalAlign.LEFT` | `HorizontalAlign.Left` |
| `HorizontalAlign.CENTER` | `HorizontalAlign.Center` |
| `HorizontalAlign.RIGHT` | `HorizontalAlign.Right` |
| `HorizontalAlign.JUSTIFY` | `HorizontalAlign.Justify` |
| `VerticalAlign.TOP` | `VerticalAlign.Top` |
| `VerticalAlign.CENTER` | `VerticalAlign.Center` |
| `VerticalAlign.BOTTOM` | `VerticalAlign.Bottom` |
| `VerticalAlign.JUSTIFY` | `VerticalAlign.Justify` |
| `WindowAlign.LEFT` | `WindowAlign.Left` |
| `WindowAlign.CENTER` | `WindowAlign.Center` |
| `WindowAlign.RIGHT` | `WindowAlign.Right` |

### Exceptions

| Python | Stroke |
|--------|--------|
| `InvalidLayoutError` | `InvalidLayoutException` |

### Functions

| Python | Stroke | Signature |
|--------|--------|-----------|
| `walk(container)` | `LayoutUtils.Walk(container)` | `IEnumerable<IContainer> Walk(IContainer container)` |
| `to_container(value)` | `ContainerUtils.ToContainer(value)` | `IContainer ToContainer(AnyContainer value)` |
| `to_window(value)` | `ContainerUtils.ToWindow(value)` | `Window ToWindow(AnyContainer value)` |
| `is_container(value)` | `ContainerUtils.IsContainer(value)` | `bool IsContainer(object? value)` |
| `to_dimension(value)` | `DimensionUtils.ToDimension(value)` | `Dimension ToDimension(AnyDimension value)` |
| `is_dimension(value)` | `DimensionUtils.IsDimension(value)` | `bool IsDimension(object? value)` |
| `sum_layout_dimensions(dimensions)` | `DimensionUtils.Sum(dimensions)` | `Dimension Sum(IEnumerable<Dimension> dimensions)` |
| `max_layout_dimensions(dimensions)` | `DimensionUtils.Max(dimensions)` | `Dimension Max(IEnumerable<Dimension> dimensions)` |

### Type Aliases

| Python | Stroke |
|--------|--------|
| `AnyContainer` | `AnyContainer` (struct with implicit conversions) |
| `AnyDimension` | `AnyDimension` (struct with implicit conversions) |
| `D` | `Dimension` (alias) |

---

## Module: prompt_toolkit.lexers

### Classes

| Python | Stroke | Notes |
|--------|--------|-------|
| `Lexer` | `ILexer` | Interface (abstract in Python) |
| `SimpleLexer` | `SimpleLexer` | Single-style lexer |
| `DynamicLexer` | `DynamicLexer` | Dynamic wrapper |
| `PygmentsLexer` | `PygmentsLexer` | Pygments integration |
| `SyntaxSync` | `ISyntaxSync` | Interface |
| `SyncFromStart` | `SyncFromStart` | Sync from document start |
| `RegexSync` | `RegexSync` | Regex-based sync |

### ILexer Interface

```python
# Python
class Lexer(ABC):
    @abstractmethod
    def lex_document(self, document) -> Callable[[int], StyleAndTextTuples]
    def invalidation_hash(self) -> Hashable
```

```csharp
// Stroke
public interface ILexer
{
    Func<int, IReadOnlyList<StyleAndTextTuple>> LexDocument(Document document);
    object? InvalidationHash();
}
```

---

## Module: prompt_toolkit.log

### Objects

| Python | Stroke | Notes |
|--------|--------|-------|
| `logger` | `StrokeLogger.Instance` | ILogger instance |

---

## Module: prompt_toolkit.mouse_events

### Enums

| Python | Stroke |
|--------|--------|
| `MouseEventType.MOUSE_UP` | `MouseEventType.MouseUp` |
| `MouseEventType.MOUSE_DOWN` | `MouseEventType.MouseDown` |
| `MouseEventType.MOUSE_MOVE` | `MouseEventType.MouseMove` |
| `MouseEventType.SCROLL_UP` | `MouseEventType.ScrollUp` |
| `MouseEventType.SCROLL_DOWN` | `MouseEventType.ScrollDown` |
| `MouseButton.LEFT` | `MouseButton.Left` |
| `MouseButton.MIDDLE` | `MouseButton.Middle` |
| `MouseButton.RIGHT` | `MouseButton.Right` |
| `MouseButton.NONE` | `MouseButton.None` |
| `MouseModifier.SHIFT` | `MouseModifier.Shift` |
| `MouseModifier.ALT` | `MouseModifier.Alt` |
| `MouseModifier.CONTROL` | `MouseModifier.Control` |

### Classes

| Python | Stroke | Notes |
|--------|--------|-------|
| `MouseEvent` | `MouseEvent` | Mouse event data |

### MouseEvent Class

```python
# Python
class MouseEvent:
    def __init__(self, position, event_type, button, modifiers) -> None
    @property
    def position(self) -> Point
    @property
    def event_type(self) -> MouseEventType
    @property
    def button(self) -> MouseButton
    @property
    def modifiers(self) -> frozenset[MouseModifier]
```

```csharp
// Stroke
public readonly record struct MouseEvent(
    Point Position,
    MouseEventType EventType,
    MouseButton Button,
    MouseModifiers Modifiers);

[Flags]
public enum MouseModifiers
{
    None = 0,
    Shift = 1,
    Alt = 2,
    Control = 4
}
```

---

## Module: prompt_toolkit.output

### Classes

| Python | Stroke | Notes |
|--------|--------|-------|
| `Output` | `IOutput` | Interface (abstract in Python) |
| `DummyOutput` | `DummyOutput` | No-op output |

### Enums

| Python | Stroke |
|--------|--------|
| `ColorDepth.DEPTH_1_BIT` | `ColorDepth.Depth1Bit` |
| `ColorDepth.DEPTH_4_BIT` | `ColorDepth.Depth4Bit` |
| `ColorDepth.DEPTH_8_BIT` | `ColorDepth.Depth8Bit` |
| `ColorDepth.DEPTH_24_BIT` | `ColorDepth.Depth24Bit` |
| `ColorDepth.DEFAULT` | `ColorDepth.Default` |

### Functions

| Python | Stroke | Signature |
|--------|--------|-----------|
| `create_output(stdout, always_prefer_tty)` | `OutputFactory.Create(stdout, alwaysPreferTty)` | `IOutput Create(Stream? stdout = null, bool alwaysPreferTty = false)` |

### IOutput Interface

```python
# Python
class Output(ABC):
    @abstractmethod
    def write(self, data: str) -> None
    @abstractmethod
    def write_raw(self, data: str) -> None
    @abstractmethod
    def set_title(self, title: str) -> None
    @abstractmethod
    def clear_title(self) -> None
    @abstractmethod
    def flush(self) -> None
    @abstractmethod
    def erase_screen(self) -> None
    @abstractmethod
    def enter_alternate_screen(self) -> None
    @abstractmethod
    def quit_alternate_screen(self) -> None
    @abstractmethod
    def enable_mouse_support(self) -> None
    @abstractmethod
    def disable_mouse_support(self) -> None
    @abstractmethod
    def enable_bracketed_paste(self) -> None
    @abstractmethod
    def disable_bracketed_paste(self) -> None
    @abstractmethod
    def set_cursor_shape(self, cursor_shape) -> None
    @abstractmethod
    def reset_cursor_shape(self) -> None
    @abstractmethod
    def hide_cursor(self) -> None
    @abstractmethod
    def show_cursor(self) -> None
    @abstractmethod
    def set_cursor_position(self, row, column) -> None
    @abstractmethod
    def cursor_up(self, amount) -> None
    @abstractmethod
    def cursor_down(self, amount) -> None
    @abstractmethod
    def cursor_forward(self, amount) -> None
    @abstractmethod
    def cursor_backward(self, amount) -> None
    @abstractmethod
    def erase_end_of_line(self) -> None
    @abstractmethod
    def erase_line(self) -> None
    @abstractmethod
    def erase_down(self) -> None
    @abstractmethod
    def reset_attributes(self) -> None
    @abstractmethod
    def set_attributes(self, attrs, color, bgcolor) -> None
    @abstractmethod
    def disable_autowrap(self) -> None
    @abstractmethod
    def enable_autowrap(self) -> None
    @abstractmethod
    def scroll_buffer_up(self, amount) -> None
    @abstractmethod
    def scroll_buffer_down(self, amount) -> None
    @abstractmethod
    def get_size(self) -> Size
    @abstractmethod
    def bell(self) -> None
    @abstractmethod
    def encoding(self) -> str
    @abstractmethod
    def get_default_color_depth(self) -> ColorDepth
    @abstractmethod
    def responds_to_cpr(self) -> bool
```

```csharp
// Stroke
public interface IOutput
{
    void Write(string data);
    void WriteRaw(string data);
    void SetTitle(string title);
    void ClearTitle();
    void Flush();
    void EraseScreen();
    void EnterAlternateScreen();
    void QuitAlternateScreen();
    void EnableMouseSupport();
    void DisableMouseSupport();
    void EnableBracketedPaste();
    void DisableBracketedPaste();
    void SetCursorShape(CursorShape cursorShape);
    void ResetCursorShape();
    void HideCursor();
    void ShowCursor();
    void SetCursorPosition(int row, int column);
    void CursorUp(int amount);
    void CursorDown(int amount);
    void CursorForward(int amount);
    void CursorBackward(int amount);
    void EraseEndOfLine();
    void EraseLine();
    void EraseDown();
    void ResetAttributes();
    void SetAttributes(Attrs attrs, Color? color, Color? bgColor);
    void DisableAutowrap();
    void EnableAutowrap();
    void ScrollBufferUp(int amount);
    void ScrollBufferDown(int amount);
    Size GetSize();
    void Bell();
    string Encoding { get; }
    ColorDepth GetDefaultColorDepth();
    bool RespondsToCpr { get; }
}
```

---

## Module: prompt_toolkit.patch_stdout

### Classes

| Python | Stroke | Notes |
|--------|--------|-------|
| `StdoutProxy` | `StdoutProxy` | Proxied stdout |

### Functions

| Python | Stroke | Signature |
|--------|--------|-----------|
| `patch_stdout(raw)` | `PatchStdout.Patch(raw)` | `IDisposable Patch(bool raw = false)` |

---

## Module: prompt_toolkit.renderer

### Classes

| Python | Stroke | Notes |
|--------|--------|-------|
| `Renderer` | `Renderer` | Screen renderer |

### Functions

| Python | Stroke | Signature |
|--------|--------|-----------|
| `print_formatted_text(output, ft, style)` | `Renderer.PrintFormattedText(output, ft, style)` | `void PrintFormattedText(IOutput output, AnyFormattedText ft, IStyle? style = null)` |

### Renderer Class

```python
# Python
class Renderer:
    def __init__(self, style, output, full_screen=False, ...) -> None
    def render(self, app, layout, is_done=False) -> None
    def erase(self, leave_alternate_screen=True) -> None
    def clear(self) -> None
    def reset(self, ...) -> None
    @property
    def last_rendered_screen(self) -> Screen | None
    @property
    def height_is_known(self) -> bool
    @property
    def rows_above_layout(self) -> int
    def request_absolute_cursor_position(self) -> None
    async def wait_for_cpr_responses(self) -> None
```

```csharp
// Stroke
public class Renderer
{
    public Renderer(IStyle style, IOutput output, bool fullScreen = false, ...);
    public void Render(Application app, Layout layout, bool isDone = false);
    public void Erase(bool leaveAlternateScreen = true);
    public void Clear();
    public void Reset(...);
    public Screen? LastRenderedScreen { get; }
    public bool HeightIsKnown { get; }
    public int RowsAboveLayout { get; }
    public void RequestAbsoluteCursorPosition();
    public Task WaitForCprResponsesAsync();
}
```

---

## Module: prompt_toolkit.search

### Enums

| Python | Stroke |
|--------|--------|
| `SearchDirection.FORWARD` | `SearchDirection.Forward` |
| `SearchDirection.BACKWARD` | `SearchDirection.Backward` |

### Functions

| Python | Stroke | Signature |
|--------|--------|-----------|
| `start_search(direction)` | `SearchOperations.StartSearch(direction)` | `void StartSearch(SearchDirection direction = SearchDirection.Forward)` |
| `stop_search(...)` | `SearchOperations.StopSearch(...)` | `void StopSearch(...)` |

---

## Module: prompt_toolkit.selection

### Enums

| Python | Stroke |
|--------|--------|
| `SelectionType.CHARACTERS` | `SelectionType.Characters` |
| `SelectionType.LINES` | `SelectionType.Lines` |
| `SelectionType.BLOCK` | `SelectionType.Block` |
| `PasteMode.EMACS` | `PasteMode.Emacs` |
| `PasteMode.VI_AFTER` | `PasteMode.ViAfter` |
| `PasteMode.VI_BEFORE` | `PasteMode.ViBefore` |

### Classes

| Python | Stroke | Notes |
|--------|--------|-------|
| `SelectionState` | `SelectionState` | Selection state data |

### SelectionState Class

```python
# Python
class SelectionState:
    def __init__(self, original_cursor_position=0, type=SelectionType.CHARACTERS) -> None
    @property
    def original_cursor_position(self) -> int
    @property
    def type(self) -> SelectionType
    def enter_shift_mode(self) -> SelectionState
```

```csharp
// Stroke
public sealed record SelectionState(
    int OriginalCursorPosition = 0,
    SelectionType Type = SelectionType.Characters)
{
    public SelectionState EnterShiftMode();
}
```

---

## Module: prompt_toolkit.shortcuts

### Classes

| Python | Stroke | Notes |
|--------|--------|-------|
| `PromptSession` | `PromptSession<TResult>` | Interactive prompt session |
| `ProgressBar` | `ProgressBar` | Progress bar display |
| `ProgressBarCounter` | `ProgressBarCounter<T>` | Generic progress counter |

### Enums

| Python | Stroke |
|--------|--------|
| `CompleteStyle.COLUMN` | `CompleteStyle.Column` |
| `CompleteStyle.MULTI_COLUMN` | `CompleteStyle.MultiColumn` |
| `CompleteStyle.READLINE_LIKE` | `CompleteStyle.ReadlineLike` |

### Functions - Dialogs

| Python | Stroke | Signature |
|--------|--------|-----------|
| `message_dialog(title, text, ok_text)` | `Dialogs.MessageDialogAsync(title, text, okText)` | `Task MessageDialogAsync(...)` |
| `input_dialog(title, text, ok_text, cancel_text)` | `Dialogs.InputDialogAsync(...)` | `Task<string?> InputDialogAsync(...)` |
| `yes_no_dialog(title, text, yes_text, no_text)` | `Dialogs.YesNoDialogAsync(...)` | `Task<bool> YesNoDialogAsync(...)` |
| `button_dialog(title, text, buttons)` | `Dialogs.ButtonDialogAsync<T>(...)` | `Task<T> ButtonDialogAsync<T>(...)` |
| `radiolist_dialog(title, text, values)` | `Dialogs.RadioListDialogAsync<T>(...)` | `Task<T> RadioListDialogAsync<T>(...)` |
| `checkboxlist_dialog(title, text, values)` | `Dialogs.CheckboxListDialogAsync<T>(...)` | `Task<IReadOnlyList<T>> CheckboxListDialogAsync<T>(...)` |
| `progress_dialog(title, text, run_callback)` | `Dialogs.ProgressDialogAsync(...)` | `Task ProgressDialogAsync(...)` |

### Functions - Prompt

| Python | Stroke | Signature |
|--------|--------|-----------|
| `prompt(message, ...)` | `Prompt.PromptAsync(message, ...)` | `Task<string> PromptAsync(AnyFormattedText message, ...)` |
| `confirm(message, suffix)` | `Prompt.ConfirmAsync(message, suffix)` | `Task<bool> ConfirmAsync(string message, string suffix = " (y/n) ")` |
| `create_confirm_session(message, suffix)` | `Prompt.CreateConfirmSession(message, suffix)` | `PromptSession<bool> CreateConfirmSession(...)` |
| `choice(title, values)` | `Choice.ChoiceAsync<T>(title, values)` | `Task<T> ChoiceAsync<T>(...)` |

### Functions - Utilities

| Python | Stroke | Signature |
|--------|--------|-----------|
| `print_formatted_text(text, ...)` | `FormattedTextOutput.Print(text, ...)` | `void Print(AnyFormattedText text, ...)` |
| `print_container(container, ...)` | `FormattedTextOutput.PrintContainer(container, ...)` | `void PrintContainer(IContainer container, ...)` |
| `clear()` | `TerminalUtils.Clear()` | `void Clear()` |
| `set_title(title)` | `TerminalUtils.SetTitle(title)` | `void SetTitle(string title)` |
| `clear_title()` | `TerminalUtils.ClearTitle()` | `void ClearTitle()` |

---

## Module: prompt_toolkit.styles

### Classes

| Python | Stroke | Notes |
|--------|--------|-------|
| `BaseStyle` | `IStyle` | Interface (abstract in Python) |
| `Style` | `Style` | CSS-like style |
| `DummyStyle` | `DummyStyle` | No-op style |
| `DynamicStyle` | `DynamicStyle` | Dynamic wrapper |
| `Attrs` | `Attrs` | Text attributes |
| `StyleTransformation` | `IStyleTransformation` | Interface |
| `SwapLightAndDarkStyleTransformation` | `SwapLightAndDarkStyleTransformation` | Light/dark swap |
| `ReverseStyleTransformation` | `ReverseStyleTransformation` | Reverse colors |
| `SetDefaultColorStyleTransformation` | `SetDefaultColorStyleTransformation` | Set default colors |
| `AdjustBrightnessStyleTransformation` | `AdjustBrightnessStyleTransformation` | Brightness adjustment |
| `DummyStyleTransformation` | `DummyStyleTransformation` | No-op transformation |
| `ConditionalStyleTransformation` | `ConditionalStyleTransformation` | Conditional wrapper |
| `DynamicStyleTransformation` | `DynamicStyleTransformation` | Dynamic wrapper |

### Enums

| Python | Stroke |
|--------|--------|
| `Priority.LOWEST` | `Priority.Lowest` |
| `Priority.LOW` | `Priority.Low` |
| `Priority.NORMAL` | `Priority.Normal` |
| `Priority.HIGH` | `Priority.High` |
| `Priority.HIGHEST` | `Priority.Highest` |

### Constants

| Python | Stroke | Type |
|--------|--------|------|
| `DEFAULT_ATTRS` | `Attrs.Default` | `Attrs` |
| `ANSI_COLOR_NAMES` | `AnsiColorNames.All` | `IReadOnlyList<string>` |
| `NAMED_COLORS` | `NamedColors.All` | `IReadOnlyDictionary<string, string>` |

### Functions

| Python | Stroke | Signature |
|--------|--------|-----------|
| `merge_styles(styles)` | `StyleExtensions.Merge(styles)` | `IStyle Merge(this IEnumerable<IStyle?> styles)` |
| `parse_color(color)` | `ColorUtils.Parse(color)` | `Color Parse(string color)` |
| `default_ui_style()` | `DefaultStyles.UiStyle()` | `IStyle UiStyle()` |
| `default_pygments_style()` | `DefaultStyles.PygmentsStyle()` | `IStyle PygmentsStyle()` |
| `style_from_pygments_cls(cls)` | `StyleUtils.FromPygmentsClass(cls)` | `IStyle FromPygmentsClass(...)` |
| `style_from_pygments_dict(dict)` | `StyleUtils.FromPygmentsDict(dict)` | `IStyle FromPygmentsDict(...)` |
| `pygments_token_to_classname(token)` | `StyleUtils.TokenToClassName(token)` | `string TokenToClassName(...)` |
| `merge_style_transformations(transformations)` | `StyleTransformationExtensions.Merge(transformations)` | `IStyleTransformation Merge(...)` |

### Attrs Class

```python
# Python
class Attrs(NamedTuple):
    color: str | None
    bgcolor: str | None
    bold: bool | None
    underline: bool | None
    strike: bool | None
    italic: bool | None
    blink: bool | None
    reverse: bool | None
    hidden: bool | None
```

```csharp
// Stroke
public readonly record struct Attrs(
    string? Color = null,
    string? BgColor = null,
    bool? Bold = null,
    bool? Underline = null,
    bool? Strike = null,
    bool? Italic = null,
    bool? Blink = null,
    bool? Reverse = null,
    bool? Hidden = null)
{
    public static readonly Attrs Default = new();
}
```

---

## Module: prompt_toolkit.token

### Constants

| Python | Stroke | Type |
|--------|--------|------|
| `ZeroWidthEscape` | `Tokens.ZeroWidthEscape` | `string` |

---

## Module: prompt_toolkit.utils

### Classes

| Python | Stroke | Notes |
|--------|--------|-------|
| `Event` | `Event<T>` | Generic event handler |
| `DummyContext` | `DummyContext` | No-op context manager |

### Functions

| Python | Stroke | Signature |
|--------|--------|-----------|
| `get_cwidth(char)` | `UnicodeWidth.GetWidth(char)` | `int GetWidth(char c)` |
| `suspend_to_background_supported()` | `PlatformUtils.SuspendToBackgroundSupported()` | `bool SuspendToBackgroundSupported()` |
| `is_conemu_ansi()` | `PlatformUtils.IsConEmuAnsi()` | `bool IsConEmuAnsi()` |
| `is_windows()` | `PlatformUtils.IsWindows` | `bool` property |
| `in_main_thread()` | `PlatformUtils.InMainThread()` | `bool InMainThread()` |
| `get_bell_environment_variable()` | `PlatformUtils.GetBellEnvironmentVariable()` | `string? GetBellEnvironmentVariable()` |
| `get_term_environment_variable()` | `PlatformUtils.GetTermEnvironmentVariable()` | `string? GetTermEnvironmentVariable()` |
| `take_using_weights(items, weights)` | `CollectionUtils.TakeUsingWeights<T>(items, weights)` | `IEnumerable<T> TakeUsingWeights<T>(...)` |
| `to_str(value)` | `ConversionUtils.ToStr(value)` | `string ToStr(object? value)` |
| `to_int(value)` | `ConversionUtils.ToInt(value)` | `int ToInt(object? value)` |
| `to_float(value)` | `ConversionUtils.ToFloat(value)` | `double ToFloat(object? value)` |
| `is_dumb_terminal()` | `PlatformUtils.IsDumbTerminal()` | `bool IsDumbTerminal()` |

### Type Aliases

| Python | Stroke |
|--------|--------|
| `AnyFloat` | `AnyFloat` (struct with implicit conversions) |

### Event Class

```python
# Python
class Event:
    def __init__(self, sender, handler=None) -> None
    def __call__(self, *args, **kwargs) -> None
    def __iadd__(self, handler) -> Event
    def __isub__(self, handler) -> Event
    def fire(self) -> None
```

```csharp
// Stroke
public class Event<TEventArgs>
{
    public Event(object sender, EventHandler<TEventArgs>? handler = null);
    public void Fire(TEventArgs args);
    public void Add(EventHandler<TEventArgs> handler);
    public void Remove(EventHandler<TEventArgs> handler);

    public static Event<TEventArgs> operator +(Event<TEventArgs> e, EventHandler<TEventArgs> handler);
    public static Event<TEventArgs> operator -(Event<TEventArgs> e, EventHandler<TEventArgs> handler);
}
```

---

## Module: prompt_toolkit.validation

### Classes

| Python | Stroke | Notes |
|--------|--------|-------|
| `Validator` | `IValidator` | Interface (abstract in Python) |
| `ValidationError` | `ValidationError` | Validation failure |
| `ThreadedValidator` | `ThreadedValidator` | Threaded wrapper |
| `DummyValidator` | `DummyValidator` | No-op validator |
| `DynamicValidator` | `DynamicValidator` | Dynamic wrapper |
| `ConditionalValidator` | `ConditionalValidator` | Conditional wrapper |

### ValidationError Class

```python
# Python
class ValidationError(Exception):
    def __init__(self, cursor_position=0, message='') -> None
    @property
    def cursor_position(self) -> int
    @property
    def message(self) -> str
```

```csharp
// Stroke
public class ValidationError : Exception
{
    public ValidationError(int cursorPosition = 0, string message = "");
    public int CursorPosition { get; }
    public override string Message { get; }
}
```

### IValidator Interface

```python
# Python
class Validator(ABC):
    @abstractmethod
    def validate(self, document: Document) -> None
    async def validate_async(self, document: Document) -> None
    @classmethod
    def from_callable(cls, validate_func, ...) -> Validator
```

```csharp
// Stroke
public interface IValidator
{
    void Validate(Document document); // Throws ValidationError on failure
    Task ValidateAsync(Document document);

    static IValidator FromCallable(Action<Document> validateFunc, ...);
    static IValidator FromCallable(Func<Document, bool> validateFunc, string errorMessage, ...);
}
```

---

## Module: prompt_toolkit.widgets

### Classes - Base Widgets

| Python | Stroke | Notes |
|--------|--------|-------|
| `TextArea` | `TextArea` | Multi-line text input |
| `Label` | `Label` | Text label |
| `Button` | `Button` | Clickable button |
| `Frame` | `Frame` | Bordered container |
| `Shadow` | `Shadow` | Shadow effect |
| `Box` | `Box` | Padded container |
| `VerticalLine` | `VerticalLine` | Vertical separator |
| `HorizontalLine` | `HorizontalLine` | Horizontal separator |
| `ProgressBar` | `ProgressBar` | Progress indicator |

### Classes - Selection Widgets

| Python | Stroke | Notes |
|--------|--------|-------|
| `CheckboxList` | `CheckboxList<T>` | Multi-select list |
| `RadioList` | `RadioList<T>` | Single-select list |
| `Checkbox` | `Checkbox` | Single checkbox |

### Classes - Toolbars

| Python | Stroke | Notes |
|--------|--------|-------|
| `ArgToolbar` | `ArgToolbar` | Argument input toolbar |
| `CompletionsToolbar` | `CompletionsToolbar` | Completion display |
| `FormattedTextToolbar` | `FormattedTextToolbar` | Formatted text toolbar |
| `SearchToolbar` | `SearchToolbar` | Search input toolbar |
| `SystemToolbar` | `SystemToolbar` | System command toolbar |
| `ValidationToolbar` | `ValidationToolbar` | Validation message display |

### Classes - Dialogs

| Python | Stroke | Notes |
|--------|--------|-------|
| `Dialog` | `Dialog` | Modal dialog container |

### Classes - Menus

| Python | Stroke | Notes |
|--------|--------|-------|
| `MenuContainer` | `MenuContainer` | Menu bar container |
| `MenuItem` | `MenuItem` | Menu item |

### TextArea Class

```python
# Python
class TextArea:
    def __init__(self, text='', multiline=True, password=False,
                 lexer=None, auto_suggest=None, completer=None,
                 complete_while_typing=True, accept_handler=None,
                 history=None, focusable=True, focus_on_click=False,
                 wrap_lines=True, read_only=False, width=None, height=None,
                 dont_extend_height=False, dont_extend_width=False,
                 line_numbers=False, get_line_prefix=None, scrollbar=False,
                 style='', search_field=None, preview_search=True,
                 prompt='', input_processors=None, name='') -> None
    @property
    def text(self) -> str
    @text.setter
    def text(self, value: str) -> None
    @property
    def document(self) -> Document
    @document.setter
    def document(self, value: Document) -> None
    @property
    def buffer(self) -> Buffer
    @property
    def control(self) -> BufferControl
    @property
    def window(self) -> Window
    @property
    def accept_handler(self) -> Callable | None
    @accept_handler.setter
    def accept_handler(self, value: Callable | None) -> None
```

```csharp
// Stroke
public class TextArea
{
    public TextArea(
        string text = "",
        bool multiline = true,
        bool password = false,
        ILexer? lexer = null,
        IAutoSuggest? autoSuggest = null,
        ICompleter? completer = null,
        bool completeWhileTyping = true,
        Action<Buffer>? acceptHandler = null,
        IHistory? history = null,
        bool focusable = true,
        bool focusOnClick = false,
        bool wrapLines = true,
        bool readOnly = false,
        AnyDimension? width = null,
        AnyDimension? height = null,
        bool dontExtendHeight = false,
        bool dontExtendWidth = false,
        bool lineNumbers = false,
        Func<int, int, AnyFormattedText>? getLinePrefix = null,
        bool scrollbar = false,
        string style = "",
        SearchToolbar? searchField = null,
        bool previewSearch = true,
        AnyFormattedText? prompt = null,
        IReadOnlyList<IProcessor>? inputProcessors = null,
        string name = "");

    public string Text { get; set; }
    public Document Document { get; set; }
    public Buffer Buffer { get; }
    public BufferControl Control { get; }
    public Window Window { get; }
    public Action<Buffer>? AcceptHandler { get; set; }
}
```

---

## Module: prompt_toolkit.contrib.completers

### Classes

| Python | Stroke | Notes |
|--------|--------|-------|
| `SystemCompleter` | `SystemCompleter` | System command completion |

---

## Module: prompt_toolkit.contrib.regular_languages

### Functions

| Python | Stroke | Signature |
|--------|--------|-----------|
| `compile(expression)` | `RegularLanguage.Compile(expression)` | `CompiledGrammar Compile(string expression)` |

---

## Summary Statistics

| Category | Python | Stroke |
|----------|--------|--------|
| **Namespaces/Packages** | 35 | 35 |
| **Classes** | ~150 | ~150 |
| **Interfaces** | 0 (abstract classes) | ~25 |
| **Functions** | ~100 | ~100 (static methods) |
| **Enums** | ~15 | ~15 |
| **Records** | 0 (classes) | ~20 |
| **Exceptions** | ~3 | ~3 |
| **Type Aliases** | ~8 | ~8 (structs with conversions) |
| **Constants** | ~10 | ~10 |

---

## Key Differences Summary

### 1. Abstract Classes → Interfaces
Python abstract base classes become C# interfaces with `I` prefix:
- `Completer` → `ICompleter`
- `Validator` → `IValidator`
- `Filter` → `IFilter`

### 2. Async Patterns
- Python `async def` → C# `async Task<T>` or `async ValueTask<T>`
- Python `AsyncGenerator` → C# `IAsyncEnumerable<T>`

### 3. Properties
- Python `@property` → C# `{ get; }` or `{ get; set; }`
- Computed properties use same lazy evaluation

### 4. Records
Immutable data classes become C# records:
- `ClipboardData` → `record ClipboardData`
- `SelectionState` → `record SelectionState`

### 5. Union Types
Python union types become C# structs with implicit conversion operators:
- `FilterOrBool` (bool | Filter)
- `AnyFormattedText` (str | list | FormattedText | ...)
- `AnyContainer` (Container | Window | ...)

### 6. Context Managers
- Python `with` → C# `using` with `IDisposable`
- Python `async with` → C# `await using` with `IAsyncDisposable`

### 7. Decorators
- Python `@abstractmethod` → C# interface member
- Python `@property` → C# property
- Python `@memoized` → C# `[Memoized]` attribute or explicit caching
