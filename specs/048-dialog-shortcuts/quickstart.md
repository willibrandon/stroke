# Quickstart: Dialog Shortcut Functions

**Feature**: 048-dialog-shortcuts
**Date**: 2026-02-02

## Prerequisites

- .NET 10 SDK
- Stroke project building successfully (`dotnet build`)
- All prior features (001–047) implemented and passing tests

## Implementation Order

### Step 1: Create `Dialogs.cs`

Create `src/Stroke/Shortcuts/Dialogs.cs` with:

1. **Private helpers first**: `CreateApp<T>()` and `ReturnNone()`
2. **Simple dialogs**: `YesNoDialog`, `MessageDialog` (P1 — foundation)
3. **Value dialogs**: `ButtonDialog<T>`, `InputDialog` (P2)
4. **List dialogs**: `RadioListDialog<T>`, `CheckboxListDialog<T>` (P2/P3)
5. **Complex dialog**: `ProgressDialog` (P3 — background task)
6. **Async wrappers**: All 7 `*Async` methods

### Step 2: Create `DialogsTests.cs`

Create `tests/Stroke.Tests/Shortcuts/DialogsTests.cs` with:

1. Structure verification tests (widget hierarchy per dialog type)
2. Button handler tests (exit result values)
3. Edge case tests (empty buttons, null values, exception in callback)
4. Parameter defaulting tests

### Step 3: Build & Test

```bash
dotnet build src/Stroke/Stroke.csproj
dotnet test tests/Stroke.Tests/Stroke.Tests.csproj --filter "FullyQualifiedName~Shortcuts.Dialogs"
```

## Key Implementation Notes

### CreateApp Pattern
```csharp
// Shared by all dialog functions
private static Application<T> CreateApp<T>(IContainer dialog, IStyle? style)
{
    var bindings = new KeyBindings();
    bindings.Add<KeyHandlerCallable>(
        [new KeyOrChar(Keys.Tab)],
        handler => handler.Set(FocusFunctions.FocusNext));
    bindings.Add<KeyHandlerCallable>(
        [new KeyOrChar(Keys.BackTab)],
        handler => handler.Set(FocusFunctions.FocusPrevious));

    return new Application<T>(
        layout: new Layout.Layout(dialog),
        keyBindings: new MergedKeyBindings(DefaultKeyBindings.Load(), bindings),
        mouseSupport: true,
        style: style,
        fullScreen: true);
}
```

### ReturnNone Pattern
```csharp
private static void ReturnNone()
{
    AppContext.GetApp().Exit();
}
```

### Progress Dialog Background Pattern
```csharp
app.PreRunCallables.Add(() =>
{
    _ = app.CreateBackgroundTask(async ct =>
    {
        try
        {
            await Task.Run(() => callback(setPercentage, logText), ct);
        }
        finally
        {
            app.Exit();
        }
    });
});
```

## Verification Checklist

- [ ] `dotnet build` succeeds with no warnings
- [ ] All dialog factory methods compile and return correct `Application<T>` types
- [ ] All async wrapper methods compile and return correct `Task<T>` types
- [ ] Unit tests pass: `dotnet test --filter "FullyQualifiedName~Shortcuts.Dialogs"`
- [ ] No file exceeds 1,000 LOC
- [ ] XML doc comments on all public methods
