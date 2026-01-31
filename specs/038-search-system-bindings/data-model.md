# Data Model: Search System & Search Bindings

**Feature**: 038-search-system-bindings
**Date**: 2026-01-31

## Entity Relationships

```
┌─────────────────┐     searches      ┌──────────────────────┐
│  BufferControl   │◄─────────────────│  SearchBufferControl  │
│  (target)        │                  │  (search field)       │
│                  │                  │                       │
│  .Buffer         │                  │  .Buffer (search buf) │
│  .SearchState    │──references──►   │  .SearcherSearchState │
│  .SearchBuffer   │                  │  .IgnoreCase          │
│  .SearchBufCtrl  │──references──►   │                       │
└─────────────────┘                  └──────────────────────┘
        ▲                                    │
        │                                    │
        │         ┌──────────────────┐       │
        └─────────│  Layout          │───────┘
                  │  .SearchLinks    │
                  │  {SBC → BC}      │
                  │  .IsSearching    │
                  │  .Focus()        │
                  │  .SearchTarget   │
                  │   BufferControl  │
                  └──────────────────┘
                          │
                          │ accessed via
                          ▼
                  ┌──────────────────┐
                  │  AppContext      │
                  │  .GetApp()       │
                  │    .Layout       │
                  │    .ViState      │
                  │    .CurrentBuffer│
                  └──────────────────┘
```

## Entities

### SearchState (Existing - Stroke.Core)

Mutable, thread-safe search query object.

| Field | Type | Description |
|-------|------|-------------|
| Text | `string` | Search query text (never null, defaults to "") |
| Direction | `SearchDirection` | Forward or Backward |
| IgnoreCaseFilter | `Func<bool>?` | Runtime case sensitivity |

**Modification**: Add `operator ~` (bitwise complement) that delegates to `Invert()`.

### SearchBufferControl (Existing - Stroke.Layout.Controls)

| Field | Type | Description |
|-------|------|-------------|
| Buffer | `Buffer` | The search buffer containing the query text |
| SearcherSearchState | `SearchState` | The SearchState instance associated with this search field. This is the SAME object as the target BufferControl's `SearchState` property — both point to the shared SearchState that links the search field to the target buffer. |

### SearchDirection (Existing - Stroke.Core)

| Value | Description |
|-------|-------------|
| Forward | Search downward from cursor |
| Backward | Search upward from cursor |

**No changes needed.**

### SearchOperations (Relocated - Stroke.Core → Stroke.Application)

Stateless static class. No fields. All state accessed via AppContext.GetApp().

| Method | Parameters | Returns | Description |
|--------|-----------|---------|-------------|
| StartSearch | `BufferControl? bufferControl = null, SearchDirection direction = Forward` | `void` | Begin search session |
| StopSearch | `BufferControl? bufferControl = null` | `void` | End search session |
| DoIncrementalSearch | `SearchDirection direction, int count = 1` | `void` | Navigate matches |
| AcceptSearch | (none) | `void` | Accept result and stop |
| GetReverseSearchLinks | `Layout layout` | `Dictionary<BufferControl, SearchBufferControl>` | Private helper |

### SearchBindings (New - Stroke.Application.Bindings)

Stateless static class with 7 binding handler functions.

| Method | Signature | Filter | Delegates To |
|--------|-----------|--------|-------------|
| AbortSearch | `KeyHandlerCallable` | IsSearching | StopSearch() |
| AcceptSearch | `KeyHandlerCallable` | IsSearching | AcceptSearch() |
| StartReverseIncrementalSearch | `KeyHandlerCallable` | ControlIsSearchable | StartSearch(Backward) |
| StartForwardIncrementalSearch | `KeyHandlerCallable` | ControlIsSearchable | StartSearch(Forward) |
| ReverseIncrementalSearch | `KeyHandlerCallable` | IsSearching | DoIncrementalSearch(Backward, event.Arg) |
| ForwardIncrementalSearch | `KeyHandlerCallable` | IsSearching | DoIncrementalSearch(Forward, event.Arg) |
| AcceptSearchAndAcceptInput | `KeyHandlerCallable` | IsSearching & PreviousBufferIsReturnable | AcceptSearch() + ValidateAndHandle() |

### Layout.SearchLinks (Existing - Stroke.Layout)

| Field | Type | Description |
|-------|------|-------------|
| _searchLinks | `Dictionary<SearchBufferControl, BufferControl>` | Maps search controls to target buffers |

Accessed via:
- `SearchLinks` property (returns copy)
- `AddSearchLink(sbc, bc)` (internal, locked mutation)
- `RemoveSearchLink(sbc)` (internal, locked mutation)

## Validation Rules

1. `StartSearch` silently returns if:
   - Current control is not a BufferControl (when bufferControl is null)
   - Target BufferControl has no SearchBufferControl

2. `StopSearch` silently returns if:
   - No active search session exists (SearchTargetBufferControl is null)

3. `DoIncrementalSearch` silently returns if:
   - Current control is not a BufferControl
   - SearchTargetBufferControl is null

4. `AcceptSearch` silently returns if:
   - Current control is not a BufferControl
   - SearchTargetBufferControl is null

5. `AcceptSearch` preserves search state text if search buffer text is empty (does not overwrite with "")
