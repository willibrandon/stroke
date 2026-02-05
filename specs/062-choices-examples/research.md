# Research: Choices Examples (Complete Set)

**Feature**: 062-choices-examples
**Date**: 2026-02-04

## Overview

This feature has minimal research requirements since:
1. The scope is clearly defined (8 Python examples → 8 C# examples)
2. All APIs are already implemented and documented
3. The project pattern is established (Stroke.Examples.Prompts)

## Findings

### Decision 1: Project Structure

**Decision**: Follow `Stroke.Examples.Prompts` pattern with dictionary-based routing

**Rationale**:
- Consistent with existing examples infrastructure
- Proven pattern that handles Ctrl+C gracefully
- Case-insensitive routing for user convenience

**Alternatives Considered**:
- Individual executable per example: Rejected (too many projects, maintenance burden)
- Switch statement routing: Rejected (dictionary is more maintainable)

### Decision 2: Html API Usage

**Decision**: Use `new Html(markup)` constructor, not a static `Parse` method

**Rationale**:
- The Stroke `Html` class uses a constructor for parsing
- Spec referenced `Html.Parse()` which doesn't exist
- Constructor throws `FormatException` on malformed markup

**Implementation**:
```csharp
// Correct:
message: new Html("<u>Please select a dish</u>:")

// Spec had (incorrect):
message: Html.Parse("<u>...</u>:")
```

### Decision 3: Filter Negation Operator

**Decision**: Use `~` operator to negate `AppFilters.IsDone`

**Rationale**:
- Stroke's `IFilter` interface supports `~` for negation (InvertFilter)
- Matches Python Prompt Toolkit's `~is_done` pattern
- `FilterOrBool` accepts both `IFilter` and `bool`

**Implementation**:
```csharp
showFrame: ~AppFilters.IsDone  // Frame visible during editing, hidden on accept
```

### Decision 4: Style Class Prefixes

**Decision**: Use "accepted" prefix for accept-state styling

**Rationale**:
- Python Prompt Toolkit uses space-separated class prefixes
- "accepted frame.border" applies when application is in done state
- Style system automatically applies class combinations

**Implementation**:
```csharp
var style = Style.FromDict(new Dictionary<string, string>
{
    ["frame.border"] = "#ff4444",           // During editing
    ["accepted frame.border"] = "#888888",  // After accept
});
```

### Decision 5: Default Example Behavior

**Decision**: Default example runs `SimpleSelection` when no arguments provided

**Rationale**:
- Consistent with user input spec showing SimpleSelection as "dotnet run" without args
- SimpleSelection is the foundation example
- Prompts project shows usage help when no args (different pattern - but Choices follows spec)

**Implementation**:
```csharp
var exampleName = args.Length > 0 ? args[0] : "SimpleSelection";
```

## API Verification

All required APIs have been verified to exist in Stroke:

| API | Location | Status |
|-----|----------|--------|
| `Dialogs.Choice<T>()` | `Stroke.Shortcuts.Dialogs` | ✅ Verified |
| `Style.FromDict()` | `Stroke.Styles.Style` | ✅ Verified |
| `Html` class | `Stroke.FormattedText.Html` | ✅ Verified |
| `AppFilters.IsDone` | `Stroke.Application.AppFilters` | ✅ Verified |
| `FilterOrBool` | `Stroke.Filters.FilterOrBool` | ✅ Verified |

## No NEEDS CLARIFICATION Items

All technical details are resolved. Implementation can proceed.
