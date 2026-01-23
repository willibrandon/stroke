# Feature 66: Auto Suggest Bindings

## Overview

Implement the key bindings for accepting and partially accepting Fish-style auto suggestions.

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/key_binding/bindings/auto_suggest.py`

## Public API

### AutoSuggestBindings Class

```csharp
namespace Stroke.KeyBinding.Bindings;

public static class AutoSuggestBindings
{
    /// <summary>
    /// Load key bindings for accepting auto suggestion text.
    /// These bindings should come after Vi bindings since they
    /// override the right arrow behavior when a suggestion is available.
    /// </summary>
    public static KeyBindings LoadAutoSuggestBindings();
}
```

## Project Structure

```
src/Stroke/
└── KeyBinding/
    └── Bindings/
        └── AutoSuggestBindings.cs
tests/Stroke.Tests/
└── KeyBinding/
    └── Bindings/
        └── AutoSuggestBindingsTests.cs
```

## Implementation Notes

### Suggestion Available Filter

```csharp
private static readonly IFilter SuggestionAvailable = Condition.Create(() =>
{
    var app = Application.Current;
    if (app == null) return false;

    var buffer = app.CurrentBuffer;
    return buffer.Suggestion != null
        && buffer.Suggestion.Text.Length > 0
        && buffer.Document.IsCursorAtTheEnd;
});
```

### Accept Suggestion Bindings

```csharp
public static KeyBindings LoadAutoSuggestBindings()
{
    var bindings = new KeyBindings();

    // Accept full suggestion with Ctrl-F, Ctrl-E, or Right arrow
    bindings.Add("c-f", AcceptSuggestion, filter: SuggestionAvailable);
    bindings.Add("c-e", AcceptSuggestion, filter: SuggestionAvailable);
    bindings.Add("right", AcceptSuggestion, filter: SuggestionAvailable);

    // Accept partial suggestion (next word) with Escape-F (Emacs mode)
    bindings.Add("escape", "f", AcceptPartialSuggestion,
        filter: SuggestionAvailable & Filters.EmacsMode);

    return bindings;
}
```

### Accept Full Suggestion

```csharp
private static void AcceptSuggestion(KeyPressEvent @event)
{
    var buffer = @event.CurrentBuffer;
    var suggestion = buffer.Suggestion;

    if (suggestion != null)
    {
        buffer.InsertText(suggestion.Text);
    }
}
```

### Accept Partial Suggestion (Word)

```csharp
private static void AcceptPartialSuggestion(KeyPressEvent @event)
{
    var buffer = @event.CurrentBuffer;
    var suggestion = buffer.Suggestion;

    if (suggestion == null) return;

    // Split suggestion text into word segments
    // Pattern: non-whitespace/slash followed by whitespace or slash
    var pattern = new Regex(@"([^\s/]+(?:\s+|/))");
    var parts = pattern.Split(suggestion.Text);

    // Find first non-empty part
    foreach (var part in parts)
    {
        if (!string.IsNullOrEmpty(part))
        {
            buffer.InsertText(part);
            break;
        }
    }
}
```

### Word Boundary Pattern

The partial acceptance splits on:
- Sequences of non-whitespace/non-slash characters
- Followed by whitespace or a slash

This allows navigating through:
- Path segments: `/home/user/documents/`
- Command arguments: `git commit -m "message"`

Example:
- Suggestion: `commit -m "message"`
- First accept: `commit `
- Second accept: `-m `
- Third accept: `"message"`

### Integration with Vi Bindings

These bindings must be loaded after Vi bindings because:
1. Vi mode also handles right arrow for cursor movement
2. When a suggestion is available, we want to accept it instead
3. The `SuggestionAvailable` filter ensures proper priority

```csharp
// In Application or PromptSession setup
var allBindings = KeyBindings.Merge(
    BasicBindings.LoadBasicBindings(),
    EmacsBindings.LoadEmacsBindings(),
    ViBindings.LoadViBindings(),
    // Auto-suggest bindings come AFTER Vi bindings
    AutoSuggestBindings.LoadAutoSuggestBindings()
);
```

### Rendering Integration

The suggestion is displayed after the cursor in a dim style:

```csharp
// In BufferControl rendering
if (buffer.Suggestion != null && document.IsCursorAtTheEnd)
{
    var suggestionStyle = "class:auto-suggestion";
    yield return new Fragment(suggestionStyle, buffer.Suggestion.Text);
}
```

### Clearing Suggestions

Suggestions are cleared when:
- Text is inserted (the suggestion is regenerated)
- Cursor moves away from end of buffer
- The suggestion no longer matches

```csharp
// In Buffer
public void InsertText(string text, bool overwrite = false)
{
    // ... insert text ...

    // Clear current suggestion (will be regenerated async)
    Suggestion = null;

    // Request new suggestion
    _ = UpdateSuggestionAsync();
}
```

## Dependencies

- `Stroke.KeyBinding.KeyBindings` (Feature 19) - KeyBindings class
- `Stroke.AutoSuggest.Suggestion` (Feature 58) - Suggestion class
- `Stroke.Core.Buffer` (Feature 06) - Buffer with suggestion
- `Stroke.Filters` (Feature 12) - Filter conditions

## Implementation Tasks

1. Implement `SuggestionAvailable` filter
2. Implement `AcceptSuggestion` handler
3. Implement `AcceptPartialSuggestion` handler
4. Implement word boundary regex pattern
5. Implement `LoadAutoSuggestBindings` method
6. Ensure proper binding priority after Vi bindings
7. Write comprehensive unit tests

## Acceptance Criteria

- [ ] Right arrow accepts suggestion at end of buffer
- [ ] Ctrl-F accepts suggestion at end of buffer
- [ ] Ctrl-E accepts suggestion at end of buffer
- [ ] Escape-F accepts next word in Emacs mode
- [ ] Word boundaries respect whitespace
- [ ] Word boundaries respect path separators
- [ ] Bindings inactive when no suggestion
- [ ] Bindings inactive when cursor not at end
- [ ] Bindings take priority over Vi right arrow
- [ ] Unit tests achieve 80% coverage
