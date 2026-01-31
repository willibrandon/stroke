# Quickstart: Mouse Bindings

**Feature**: 036-mouse-bindings
**Date**: 2026-01-30

## Implementation Order

1. **Add `internal Point CursorPos` property to `Renderer`** — one-line addition to expose the private `_cursorPos` field for the Windows mouse handler.

2. **Create `MouseBindings.cs`** with:
   - Modifier constants (8 `const MouseModifiers` values + `UnknownModifier`)
   - Button convenience aliases (5 aliases for `MouseButton` values)
   - Event type convenience aliases (5 aliases for `MouseEventType` values)
   - `XtermSgrMouseEvents` lookup table (108 entries) as `static readonly FrozenDictionary<(int, char), (MouseButton, MouseEventType, MouseModifiers)>`
   - `TypicalMouseEvents` lookup table (10 entries) as `static readonly FrozenDictionary<int, (MouseButton, MouseEventType, MouseModifiers)>`
   - `UrxvtMouseEvents` lookup table (4 entries) as `static readonly FrozenDictionary<int, (MouseButton, MouseEventType, MouseModifiers)>`
   - `LoadMouseBindings()` public method that creates `KeyBindings` with 4 handlers
   - `HandleVt100MouseEvent` handler (protocol detection, parsing, coordinate transform, dispatch)
   - `HandleScrollUp` / `HandleScrollDown` handlers (arrow key feed)
   - `HandleWindowsMouseEvent` handler (platform check, data parsing, coordinate adjust, dispatch)

3. **Create test files**:
   - `MouseBindingsLookupTableTests.cs` — validate all table entry counts and specific entries
   - `MouseBindingsTests.cs` — binding count, handler behavior, coordinate transforms

## Key Implementation Patterns

### Binding Registration

```csharp
public static KeyBindings LoadMouseBindings()
{
    var kb = new KeyBindings();
    kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.Vt100MouseEvent)])(HandleVt100MouseEvent);
    kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.ScrollUp)])(HandleScrollUp);
    kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.ScrollDown)])(HandleScrollDown);
    kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.WindowsMouseEvent)])(HandleWindowsMouseEvent);
    return kb;
}
```

### VT100 Protocol Detection

```csharp
// event.Data format:
// Typical:   "ESC[MaB*"      → Data[2] == 'M' with raw bytes at [3],[4],[5]
// URXVT:     "ESC[96;14;13M" → Data[2] != 'M', no '<' prefix
// XTerm SGR: "ESC[<64;85;12M" → Data[2] == '<'

if (@event.Data[2] == 'M')
{
    // Typical format: Data[3]=event code, Data[4]=x, Data[5]=y as char ordinals
}
else
{
    var data = @event.Data[2..];
    var sgr = data.StartsWith('<');
    if (sgr) data = data[1..];
    // Parse "code;x;y{M|m}" — split on ';', last char is suffix
}
```

### Scroll Handler Pattern

```csharp
private static NotImplementedOrNone? HandleScrollUp(KeyPressEvent @event)
{
    @event.GetApp().KeyProcessor.Feed(new KeyPress(Keys.Up), first: true);
    return null;
}
```

## Build Verification

```bash
dotnet build src/Stroke/Stroke.csproj
dotnet test tests/Stroke.Tests/Stroke.Tests.csproj --filter "FullyQualifiedName~MouseBindings"
```
