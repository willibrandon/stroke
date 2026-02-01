# Quickstart: Vi Key Bindings

**Feature**: 043-vi-key-bindings
**Date**: 2026-01-31

## Implementation Order

### Phase 1: Foundation Types (no dependencies on other phases)

1. **TextObjectType enum** (`src/Stroke/KeyBinding/TextObjectType.cs`)
   - 4 values: Exclusive, Inclusive, Linewise, Block
   - Unit tests: value existence, exhaustive coverage

2. **TextObject class** (`src/Stroke/KeyBinding/TextObject.cs`)
   - Constructor, properties, SelectionType mapping
   - Sorted(), OperatorRange(), GetLineNumbers(), Cut()
   - OperatorRange semantics: Exclusive → `to = cursor + end`; Inclusive → `to = cursor + end + 1`; Linewise → expand to full line boundaries
   - Unit tests: all methods with various TextObjectType values

3. **OperatorFuncDelegate update** (`src/Stroke/KeyBinding/OperatorFuncDelegate.cs`)
   - Change `object? textObject` → `TextObject textObject`
   - Verify no compile errors in downstream code

### Phase 2: ViBindings Scaffolding (depends on Phase 1: TextObject/TextObjectType)

4. **ViBindings.cs** (main file) — Create the partial class with:
   - 5 condition helper filters (IsReturnable, InBlockSelection, DigraphSymbol1Given, SearchBufferIsEmpty, TildeOperatorFilter)
   - ViTransformFunctions list (5 entries: g?, gu, gU, g~, ~)
   - `LoadViBindings()` method skeleton that creates `KeyBindings`, calls registration methods from partial files, wraps in `ConditionalKeyBindings`

### Phase 3: Registration Helpers (depends on Phase 2: scaffolding)

5. **ViBindings.Operators.cs** — Implement:
   - `RegisterTextObject()` private helper
   - `RegisterOperator()` private helper
   - All operator handlers (delete, change, yank, indent, unindent, reshape, transforms)
   - `CreateDeleteAndChangeOperators()` factory
   - `CreateTransformHandler()` factory

### Phase 4: Text Objects & Motions (depends on Phase 3: RegisterTextObject helper)

6. **ViBindings.TextObjects.cs** — Implement all 74 text object registrations (42 explicit + 32 dynamic via `create_ci_ca_handles` factory):
   - Word motions (w, W, b, B, e, E)
   - Line motions (0, ^, $, |)
   - Screen motions (H, M, L)
   - Character find (f, F, t, T, ;, ,)
   - Document motions (gg, G)
   - Paragraph motions ({, })
   - Bracket matching (%)
   - Inner/around objects (iw, aw, i", a", i(, a(, etc.)
   - Generated bracket/quote pairs via factory

### Phase 5: Mode Switching & Navigation (depends on Phase 2: scaffolding)

7. **ViBindings.ModeSwitch.cs** — Mode transitions:
   - Escape handler (back to navigation from any mode)
   - Insert mode entries (i, I, a, A, o, O)
   - Visual mode entries (v, V, Ctrl-V)
   - Replace mode entries (R, r)
   - Insert toggle (Insert key)

8. **ViBindings.Navigation.cs** — Direct navigation handlers:
   - Up/down in navigation (k/j with line wrapping logic)
   - Up/down arrow keys
   - Backspace in navigation
   - Enter (start of next line)
   - +/- (next/prev line start)
   - Sentence navigation (/, ))
   - Numeric argument handlers (0-9)

### Phase 6: Insert Mode & Visual Mode (depends on Phase 5: mode switching)

9. **ViBindings.InsertMode.cs** — Insert mode bindings:
   - Completion bindings (Ctrl-N, Ctrl-P, Ctrl-G/Y, Ctrl-E)
   - Indent/unindent (Ctrl-T, Ctrl-D)
   - Line/filename completion (Ctrl-X Ctrl-L, Ctrl-X Ctrl-F)
   - Replace mode handlers (Any key)
   - Insert-multiple mode handlers (Any, Backspace, Delete, Left, Right, Up/Down)

10. **ViBindings.VisualMode.cs** — Visual mode handlers:
    - Selection movement (j, k extend)
    - Cut selection (x)
    - Join selected lines (J, g,J)
    - Visual mode toggling (v, V, Ctrl-V between sub-modes)
    - Block selection insert/append (I, A)
    - Auto-word extension (a,w, a,W)

### Phase 7: Miscellaneous (depends on Phases 3-6)

11. **ViBindings.Misc.cs** — Remaining commands:
    - Paste (p, P, register paste)
    - Undo/redo (u, Ctrl-R)
    - Delete char (x, X)
    - Substitute (s)
    - Yank/delete line (yy, dd, Y)
    - Change/delete to end (C, D, cc, S)
    - Join (J, g,J)
    - Case transforms (~, guu, gUU, g~~)
    - Indent/unindent (>>, <<)
    - Word search (#, *)
    - Increment/decrement (Ctrl-A, Ctrl-X)
    - Scroll commands (z,z, z,t, z,b, etc.)
    - Macro recording/playback (q, @)
    - Digraph mode handlers
    - Quick normal mode (Ctrl-O)
    - Unknown text object handler (catch-all)

### Phase 8: Testing

12. Unit tests for TextObject and TextObjectType
13. Binding registration tests (verify all bindings exist with correct keys/filters)
14. Handler behavior tests (verify correct document/cursor changes)
15. Integration tests (multi-key sequences, operator+motion combos)
16. Mapped tests from test-mapping.md (13 Vi-specific tests)

## Key Patterns to Follow

### Handler Method Pattern
```csharp
private static NotImplementedOrNone? HandlerName(KeyPressEvent @event)
{
    var buff = @event.CurrentBuffer!;
    // ... manipulate buffer ...
    return null;
}
```

### Text Object Registration Pattern
```csharp
RegisterTextObject(kb,
    [new KeyOrChar('w')],
    WordForward,
    filter: new FilterOrBool(ViFilters.ViNavigationMode));

private static TextObject WordForward(KeyPressEvent @event)
{
    var doc = @event.CurrentBuffer!.Document;
    var pos = doc.FindNextWordBeginning(count: @event.Arg) ?? 0;
    return new TextObject(pos, type: TextObjectType.Exclusive);
}
```

### Operator Registration Pattern
```csharp
RegisterOperator(kb,
    [new KeyOrChar('d')],
    DeleteOperator);

private static NotImplementedOrNone DeleteOperator(KeyPressEvent @event, TextObject textObject)
{
    var buff = @event.CurrentBuffer!;
    var (doc, data) = textObject.Cut(buff);
    buff.SetDocument(doc);
    @event.GetApp().Clipboard.SetData(data);
    return default;
}
```

## Dependencies (All Pre-existing)

- ViState, InputMode, CharacterFind, OperatorFuncDelegate — `Stroke.KeyBinding`
- ViFilters, AppFilters, SearchFilters, AppContext — `Stroke.Application`
- Buffer, Document, SelectionType, PasteMode — `Stroke.Core`
- ClipboardData, IClipboard — `Stroke.Clipboard`
- KeyBindings, ConditionalKeyBindings, KeyPressEvent — `Stroke.KeyBinding`
- NamedCommands, SearchBindings — `Stroke.KeyBinding.Bindings`, `Stroke.Application.Bindings`
- Digraphs — `Stroke.KeyBinding`
- Keys — `Stroke.Input`
- IFilter, Filter, Condition, FilterOrBool — `Stroke.Filters`
