# Quickstart: Application Filters

**Feature**: 032-application-filters
**Date**: 2026-01-30

## Build & Run

```bash
# Build the project
dotnet build src/Stroke/Stroke.csproj

# Run all tests
dotnet test tests/Stroke.Tests/Stroke.Tests.csproj

# Run only application filter tests
dotnet test tests/Stroke.Tests/Stroke.Tests.csproj --filter "FullyQualifiedName~Application"
```

## Implementation Order

### Step 1: Refactor AppFilters.cs

1. Remove `ViNavigationMode`, `ViInsertMode`, `ViMode`, `ViInsertMultipleMode` properties (moving to `ViFilters`)
2. Remove `EmacsMode` property (moving to `EmacsFilters`)
3. Remove `IsSearching` property (moving to `SearchFilters`)
4. Remove `HasFocus` property (not in Python PTK)
5. Rename `CreateHasFocus(string)` to `HasFocus(string)` overload
6. Fix `HasCompletions` semantics (check `CompleteState` count, not `Completer`)
7. Fix `CompletionIsSelected` semantics (add `CurrentCompletion is not null`)
8. Add missing filters: `HasSuggestion`, `IsDone`, `RendererHeightIsKnown`, `InPasteMode`
9. Add `HasFocus` overloads: `Buffer`, `IUIControl`, `IContainer`
10. Add `InEditingMode(EditingMode)` factory with memoization

### Step 2: Create ViFilters.cs

Port all 11 Vi filters with full guard condition logic from Python PTK `app.py` lines 220-360.

### Step 3: Create EmacsFilters.cs

Port all 3 Emacs filters from Python PTK `app.py` lines 362-385.

### Step 4: Create SearchFilters.cs

Port all 3 search filters from Python PTK `app.py` lines 388-413.

### Step 5: Write Tests

Create 6 test files covering all 6 user stories. Tests use real `Application`, `Buffer`, `Layout` instances — no mocks.

## Key Files

| File | Purpose |
|------|---------|
| `src/Stroke/Application/AppFilters.cs` | General app state filters, focus filters, InEditingMode |
| `src/Stroke/Application/ViFilters.cs` | Vi sub-mode filters with guard conditions |
| `src/Stroke/Application/EmacsFilters.cs` | Emacs mode filters |
| `src/Stroke/Application/SearchFilters.cs` | Search and selection filters |
| `src/Stroke/Filters/Condition.cs` | Filter wrapper (existing, no changes) |
| `src/Stroke/Application/AppContext.cs` | GetApp() accessor (existing, no changes) |

## Python Reference

Primary source: `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/filters/app.py`

Key line references:
- Lines 53-57: `has_focus` no-memoization comment
- Lines 58-106: `has_focus` function with type dispatch
- Lines 202-212: `in_editing_mode` with `@memoized()` decorator
- Lines 225-246: `vi_navigation_mode` with Guard A pattern
- Lines 249-265: `vi_insert_mode` with Guard B pattern
- Lines 268-284: `vi_insert_multiple_mode` with Guard B pattern
- Lines 287-303: `vi_replace_mode` with Guard B pattern
- Lines 306-322: `vi_replace_single_mode` with Guard B pattern
- Lines 325-331: `vi_selection_mode`
- Lines 334-340: `vi_waiting_for_text_object_mode`
- Lines 343-349: `vi_digraph_mode`
- Lines 352-359: `vi_recording_macro`
- Lines 362-365: `emacs_mode`
- Lines 368-377: `emacs_insert_mode`
- Lines 380-385: `emacs_selection_mode`
- Lines 388-401: `is_searching`
- Lines 404-413: `control_is_searchable`
- Lines 416-419: `vi_search_direction_reversed`

## Testing Pattern

```csharp
// All filters return false with no active application (DummyApplication)
Assert.False(AppFilters.HasSelection.Invoke());

// Filters can be composed
var combined = AppFilters.HasSelection & ViFilters.ViMode;
Assert.False(combined.Invoke());

// InEditingMode returns same instance (memoized)
var f1 = AppFilters.InEditingMode(EditingMode.Vi);
var f2 = AppFilters.InEditingMode(EditingMode.Vi);
Assert.Same(f1, f2);

// HasFocus returns distinct instances (not memoized)
var h1 = AppFilters.HasFocus("default");
var h2 = AppFilters.HasFocus("default");
Assert.NotSame(h1, h2);
```
