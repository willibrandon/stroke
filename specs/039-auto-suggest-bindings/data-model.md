# Data Model: Auto Suggest Bindings

**Feature**: 039-auto-suggest-bindings
**Date**: 2026-01-31

## Entities

This feature introduces no new data entities. It operates on existing entities from the Stroke codebase.

### Referenced Entities

| Entity | Location | Role in Feature |
|--------|----------|-----------------|
| `Suggestion` | `Stroke.AutoSuggest.Suggestion` | Immutable record holding suggestion text. Read by filter and handlers. |
| `Buffer` | `Stroke.Core.Buffer` | Mutable buffer holding current text, cursor, and suggestion. Handlers call `InsertText()`. |
| `Document` | `Stroke.Core.Document` | Immutable document with `IsCursorAtTheEnd` property. Read by filter. |
| `KeyBindings` | `Stroke.KeyBinding.KeyBindings` | Mutable binding registry. Factory method creates and populates one. |
| `KeyPressEvent` | `Stroke.KeyBinding.KeyPressEvent` | Event passed to handlers. Provides `CurrentBuffer` access. |

### Entity Relationships

```text
AutoSuggestBindings (static)
├── SuggestionAvailable (IFilter)
│   ├── reads → Buffer.Suggestion (Suggestion?)
│   ├── reads → Suggestion.Text (string)
│   └── reads → Document.IsCursorAtTheEnd (bool)
├── AcceptSuggestion (handler)
│   ├── reads → Buffer.Suggestion
│   └── calls → Buffer.InsertText(suggestion.Text)
└── AcceptPartialSuggestion (handler)
    ├── reads → Buffer.Suggestion
    ├── splits → Suggestion.Text via regex
    └── calls → Buffer.InsertText(firstSegment)
```

### State Transitions

No state machines are introduced. The handlers cause the following state changes on existing entities:

| Trigger | Entity | Before | After |
|---------|--------|--------|-------|
| Full accept (Ctrl-F/Ctrl-E/Right) | `Buffer` | text="git", suggestion=" commit -m 'fix'" | text="git commit -m 'fix'", suggestion=null |
| Partial accept (Escape+F) | `Buffer` | text="git ", suggestion="commit -m 'fix'" | text="git commit ", suggestion=null |
| Partial accept with leading space | `Buffer` | text="git", suggestion=" commit -m 'fix'" | text="git ", suggestion=null |
| No suggestion present | (none) | (unchanged) | (unchanged) — binding does not activate |

**Partial accept detail**: `Regex.Split(@"([^\s/]+(?:\s+|/))", suggestion.Text)` produces segments; the first non-empty element is inserted. For `"commit -m 'fix'"` the first segment is `"commit "`. For `" commit -m 'fix'"` (leading space) the first segment is `" "` (just the space). Each Escape-F press advances by one segment boundary.

### Validation Rules

- Filter: `Buffer.Suggestion` must be non-null
- Filter: `Suggestion.Text` must be non-empty (length > 0)
- Filter: `Document.IsCursorAtTheEnd` must be true
- Handler guard: null-check on `Buffer.Suggestion` before accessing `.Text` (race condition safety)
