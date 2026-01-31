# Quickstart: Auto Suggest Bindings

**Feature**: 039-auto-suggest-bindings
**Date**: 2026-01-31

## Overview

This feature adds 1 source file and 1 test file to the existing Stroke solution. No new projects, packages, or infrastructure are needed.

## Files to Create

| File | Purpose |
|------|---------|
| `src/Stroke/Application/Bindings/AutoSuggestBindings.cs` | Static class with `LoadAutoSuggestBindings()` factory and handler methods |
| `tests/Stroke.Tests/Application/Bindings/AutoSuggestBindingsTests.cs` | xUnit tests for filter, handlers, and binding registration |

## Implementation Order

1. **AutoSuggestBindings.cs** — Implement the static class with:
   - `SuggestionAvailable` private filter (Condition)
   - `AcceptSuggestion` public handler
   - `AcceptPartialSuggestion` public handler
   - `LoadAutoSuggestBindings()` public factory method

2. **AutoSuggestBindingsTests.cs** — Test using the established pattern from `SearchBindingsTests`:
   - Create real `Buffer`, `Document`, `Window`, `Layout`, `Application` instances
   - Set `Buffer.Suggestion` to test suggestion scenarios
   - Call handlers directly with `KeyPressEvent`
   - Assert buffer state after handler execution

## Build & Test

```bash
# Build
dotnet build src/Stroke/Stroke.csproj

# Run tests
dotnet test tests/Stroke.Tests/Stroke.Tests.csproj --filter "FullyQualifiedName~AutoSuggestBindings"

# Run all tests (verify no regressions)
dotnet test tests/Stroke.Tests/Stroke.Tests.csproj
```

## Key Patterns to Follow

### Binding Registration (from `src/Stroke/Application/Bindings/SearchBindings.cs`)

Single-key binding with filter (see SearchBindings.cs lines 200-230 for examples):

```csharp
kb.Add<KeyHandlerCallable>(
    [new KeyOrChar(Keys.ControlF)],
    filter: new FilterOrBool(SuggestionAvailable))(AcceptSuggestion);
```

### Multi-Key Sequence (from `src/Stroke/Application/Bindings/PageNavigationBindings.cs`)

Multi-key sequence with composed filter (see PageNavigationBindings.cs for Escape+key patterns):

```csharp
kb.Add<KeyHandlerCallable>(
    [new KeyOrChar(Keys.Escape), new KeyOrChar('f')],
    filter: new FilterOrBool(
        ((Filter)SuggestionAvailable).And(EmacsFilters.EmacsMode)))(AcceptPartialSuggestion);
```

### Private Filter (from `src/Stroke/Application/Bindings/BasicBindings.cs`)

Private static `IFilter` field using `Condition` (see BasicBindings.cs lines 56-70 for `HasTextBeforeCursor` pattern):

```csharp
private static readonly IFilter SuggestionAvailable = new Condition(() =>
{
    var app = AppContext.GetApp();
    var buffer = app.CurrentBuffer;
    return buffer.Suggestion is not null
        && buffer.Suggestion.Text.Length > 0
        && buffer.Document.IsCursorAtTheEnd;
});
```

### Test Environment (from `tests/Stroke.Tests/Application/Bindings/SearchBindingsTests.cs`)

Real object construction pattern (see SearchBindingsTests.cs for `CreateEvent` helper and test structure):

```csharp
var buffer = new Buffer(document: new Document(text, cursorPosition: cursorPosition));
// Set suggestion:
buffer.SetSuggestion(new Suggestion(suggestionText));
// Create app context, call handler, assert buffer state
```
