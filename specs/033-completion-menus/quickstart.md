# Quickstart: Completion Menus

**Feature Branch**: `033-completion-menus`

## Prerequisites

Before implementing, ensure these existing types are available:
- `IUIControl`, `UIContent`, `GetLinePrefixCallable` (Layout/Controls)
- `ConditionalContainer`, `HSplit`, `Window` (Layout/Containers)
- `ScrollbarMargin` (Layout/Margins)
- `ScrollOffsets` (Layout/Windows)
- `Dimension` (Layout)
- `ExplodedList` (Layout)
- `CompletionState` (Core)
- `Completion` (Completion)
- `Buffer` (Core)
- `AppContext`, `AppFilters` (Application)
- `StyleAndTextTuple`, `FormattedTextUtils` (FormattedText)
- `IFilter`, `FilterOrBool`, `Condition`, `FilterUtils` (Filters)
- `MouseEvent`, `MouseEventType` (Input)
- `KeyBindings`, `NotImplementedOrNone`, `KeyPressEvent` (KeyBinding)
- `UnicodeWidth`, `Point` (Core)

## Prerequisite Changes

Before implementing the menu classes:

1. **Unseal `ConditionalContainer`** (`src/Stroke/Layout/Containers/ConditionalContainer.cs`):
   - Change `public sealed class ConditionalContainer` → `public class ConditionalContainer`
   - Python's `CompletionsMenu(ConditionalContainer)` requires inheritance

2. **Unseal `HSplit`** (`src/Stroke/Layout/Containers/HSplit.cs`):
   - Change `public sealed class HSplit` → `public class HSplit`
   - Python's `MultiColumnCompletionsMenu(HSplit)` requires inheritance

3. **Update `ScrollbarMargin`** (`src/Stroke/Layout/Margins/ScrollbarMargin.cs`):
   - Change `displayArrows` parameter type from `bool` to `FilterOrBool`
   - Store as `IFilter` internally, evaluate at render time
   - Python PTK passes `FilterOrBool` for `display_arrows`

## Implementation Order

### Phase 1: Foundation (MenuUtils + CompletionsMenuControl)
1. Create `src/Stroke/Layout/Menus/` directory
2. Implement `MenuUtils.cs` — static helpers, no dependencies on other menu classes
3. Implement `CompletionsMenuControl.cs` — uses `MenuUtils`, stateless

### Phase 2: Single-Column Container (CompletionsMenu)
4. Apply prerequisite change: unseal `ConditionalContainer`
5. Implement `CompletionsMenu.cs` — inherits `ConditionalContainer`, wraps `CompletionsMenuControl`

### Phase 3: Multi-Column Control (MultiColumnCompletionMenuControl)
6. Implement `MultiColumnCompletionMenuControl.cs` — complex rendering, mutable state with Lock

### Phase 4: Meta + Multi-Column Container
7. Implement `SelectedCompletionMetaControl.cs` — stateless, reads selected completion
8. Apply prerequisite change: unseal `HSplit`
9. Implement `MultiColumnCompletionsMenu.cs` — inherits `HSplit`, wraps multi-column control + meta

### Phase 5: Tests
10. Write tests for each class, targeting 80% coverage
11. Write thread safety tests for `MultiColumnCompletionMenuControl`

## Build Verification

```bash
# Build from repo root
dotnet build src/Stroke/Stroke.csproj

# Run tests
dotnet test tests/Stroke.Tests/Stroke.Tests.csproj --filter "FullyQualifiedName~Layout.Menus"
```
