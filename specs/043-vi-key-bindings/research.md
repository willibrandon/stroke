# Research: Vi Key Bindings

**Feature**: 043-vi-key-bindings
**Date**: 2026-01-31

## Research Task 1: TextObject Placement (KeyBinding vs Application Layer)

**Decision**: Place `TextObject` and `TextObjectType` in `Stroke.KeyBinding` namespace.

**Rationale**:
- `OperatorFuncDelegate` already lives in `Stroke.KeyBinding` and references `TextObject` as its parameter type
- `ViState` lives in `Stroke.KeyBinding` and manages operator state that produces/consumes TextObjects
- TextObject depends on `Document` (Stroke.Core) and `Buffer` (Stroke.Core) — both are lower-layer dependencies, which is valid
- TextObject depends on `ClipboardData` (Stroke.Clipboard) — this follows the same pattern as `ViState` which already references `ClipboardData`
- Placing in Application layer would create an awkward cross-layer reference from `OperatorFuncDelegate`

**Alternatives Considered**:
- `Stroke.Core`: Rejected because TextObject is Vi-specific, not a general core type
- `Stroke.Application`: Rejected because it would force `OperatorFuncDelegate` to depend on Application layer (violating layered architecture)

## Research Task 2: OperatorFuncDelegate Parameter Type Update

**Decision**: Update `OperatorFuncDelegate` from `object? textObject` to `TextObject textObject`.

**Rationale**:
- The current delegate uses `object?` as a placeholder (documented: "placeholder type until ITextObject is defined")
- Now that `TextObject` is being implemented, the parameter should be strongly typed
- Python source confirms the operator function always receives a `TextObject` instance

**Alternatives Considered**:
- Keep `object?` and cast: Rejected because it defeats type safety and is a known placeholder
- Create `ITextObject` interface: Rejected because Python has no such interface; only one concrete type exists

## Research Task 3: File Split Strategy for ViBindings

**Decision**: Split `ViBindings` into 8 partial class files by functional category.

**Rationale**:
- Python vi.py is 2,233 lines — direct translation would far exceed the 1,000 LOC limit
- EmacsBindings used 2 files (main + shift-selection); Vi needs more due to ~4x more bindings
- Categories align with Python's internal structure and spec user stories

**File breakdown estimate**:
| File | Content | Est. LOC |
|------|---------|----------|
| ViBindings.cs | Main loader, 5 condition helpers, 5 transform functions, LoadViBindings() | ~200 |
| ViBindings.Navigation.cs | Navigation handler methods (up/down/left/right, +/-, sentence, arrow keys) | ~400 |
| ViBindings.Operators.cs | RegisterTextObject/RegisterOperator helpers, 14 operators, CreateDeleteAndChangeOperators, CreateTransformHandler | ~500 |
| ViBindings.TextObjects.cs | 74 text object registrations (42 explicit + 32 dynamic via create_ci_ca_handles) | ~700 |
| ViBindings.ModeSwitch.cs | Mode switching handlers (i, a, o, v, V, R, r, Escape, Insert, Ctrl-O) | ~300 |
| ViBindings.InsertMode.cs | Insert mode bindings + replace handlers + insert-multiple handlers + digraph handlers | ~400 |
| ViBindings.VisualMode.cs | Visual/selection mode handlers (j/k extend, x cut, J join, toggle, I/A block) | ~300 |
| ViBindings.Misc.cs | Macros, registers, paste, undo/redo, dd/yy/cc, case, indent, scroll z-cmds, numeric args, */# | ~400 |

**LOC safety margin**: All files estimated well under the 1,000 LOC limit. The largest (TextObjects at ~700) has ~30% margin for growth.

**Alternatives Considered**:
- Single file: Rejected (would be ~3,000+ LOC, violating Constitution X)
- Two files (like Emacs): Rejected (still too large)
- Separate classes per category: Rejected (Python uses single module scope; partial class preserves this)

## Research Task 4: Text Object Decorator Factory Pattern in C#

**Decision**: Implement as private static methods that register multiple handlers per text object.

**Rationale**:
- Python uses decorators (`@text_object('w')`) that register 1-3 handlers per call:
  1. Operator-pending mode: execute operator with text object result
  2. Navigation mode: move cursor by text object offset (optional, `no_move_handler=False`)
  3. Selection mode: extend selection by text object offset (optional, `no_selection_handler=False`)
- C# has no decorator syntax; the equivalent is a registration helper method
- Each text object handler function returns a `TextObject` describing the range

**Pattern**:
```csharp
private static void RegisterTextObject(
    KeyBindings kb,
    KeyOrChar[] keys,
    Func<KeyPressEvent, TextObject> handler,
    FilterOrBool filter = default,
    bool noMoveHandler = false,
    bool noSelectionHandler = false,
    bool eager = false)
{
    // Register in vi_waiting_for_text_object_mode
    // Register in vi_navigation_mode (if !noMoveHandler)
    // Register in vi_selection_mode (if !noSelectionHandler)
}
```

**Alternatives Considered**:
- C# attributes: Rejected (attributes are metadata, not behavior; would require reflection)
- Source generators: Rejected (over-engineering for a one-time registration pattern)

## Research Task 5: Operator Decorator Factory Pattern in C#

**Decision**: Implement as private static methods that register two handlers per operator.

**Rationale**:
- Python uses `@operator('d')` that registers 2 handlers:
  1. Navigation mode: set operator-pending state in ViState
  2. Selection mode: create TextObject from selection and execute operator immediately
- Same approach as text object: helper method that registers both handlers

**Pattern**:
```csharp
private static void RegisterOperator(
    KeyBindings kb,
    KeyOrChar[] keys,
    OperatorFuncDelegate operatorFunc,
    FilterOrBool filter = default,
    bool eager = false)
{
    // Register in vi_navigation_mode: set ViState.OperatorFunc
    // Register in vi_selection_mode: create TextObject from selection, call operatorFunc
}
```

## Research Task 6: LoadViSearchBindings Already Implemented

**Decision**: No work needed for `LoadViSearchBindings()`.

**Rationale**:
- `SearchBindings.LoadViSearchBindings()` already exists at `src/Stroke/Application/Bindings/SearchBindings.cs:282`
- Implements all 13 Vi search bindings (/, ?, Ctrl-S, Ctrl-R, Enter, Ctrl-C, Ctrl-G, Backspace, Escape)
- Properly wrapped in `ConditionalKeyBindings` gated on `ViFilters.ViMode`
- Implemented in feature 038-search-system-bindings

**Alternatives Considered**: N/A — already complete.

## Research Task 7: Existing Infrastructure Inventory

**Decision**: Leverage all existing infrastructure; no new infrastructure needed beyond TextObject/TextObjectType.

**Existing types confirmed available**:
| Type | Location | Status |
|------|----------|--------|
| ViState | Stroke.KeyBinding/ViState.cs | Complete (thread-safe, all properties) |
| InputMode | Stroke.KeyBinding/InputMode.cs | Complete (Insert, InsertMultiple, Navigation, Replace, ReplaceSingle) |
| CharacterFind | Stroke.KeyBinding/CharacterFind.cs | Complete (immutable record) |
| OperatorFuncDelegate | Stroke.KeyBinding/OperatorFuncDelegate.cs | Exists (needs parameter type update) |
| EditingMode | Stroke.KeyBinding/EditingMode.cs | Complete (Vi, Emacs) |
| ViFilters | Stroke.Application/ViFilters.cs | Complete (11 filters) |
| AppFilters | Stroke.Application/AppFilters.cs | Complete (HasSelection, IsReadOnly, etc.) |
| SearchFilters | Stroke.Application/SearchFilters.cs | Complete (IsSearching, etc.) |
| NamedCommands | Stroke.KeyBinding.Bindings/NamedCommands.cs | Complete (49 commands) |
| Digraphs | Stroke.KeyBinding/Digraphs.cs | Complete (1,356 mappings) |
| Buffer | Stroke.Core/Buffer.cs | Complete (all editing operations) |
| Document | Stroke.Core/Document.cs | Complete (all navigation/position methods) |
| SelectionType | Stroke.Core/SelectionType.cs | Complete (Characters, Lines, Block) |
| PasteMode | Stroke.Core/PasteMode.cs | Complete (Emacs, ViAfter, ViBefore) |
| ClipboardData | Stroke.Clipboard/ClipboardData.cs | Complete |
| KeyBindings | Stroke.KeyBinding/KeyBindings.cs | Complete |
| ConditionalKeyBindings | Stroke.KeyBinding/ConditionalKeyBindings.cs | Complete |
| KeyPressEvent | Stroke.KeyBinding/KeyPressEvent.cs | Complete |

## Research Task 8: Registration Helper Naming (Python → C# Adaptation)

**Decision**: Use `RegisterTextObject`/`RegisterOperator` as C# method names instead of `CreateTextObjectDecorator`/`CreateOperatorDecorator` from api-mapping.md.

**Rationale**:
- Python uses `create_text_object_decorator()` and `create_operator_decorator()` which return decorator functions — a Python-specific pattern
- C# has no decorator syntax; the equivalent is a registration helper method that directly adds bindings to a `KeyBindings` instance
- `RegisterTextObject`/`RegisterOperator` better communicates the C# intent (imperative registration vs. functional decorator creation)
- Both names map to the same Python functions; the semantic behavior is identical

**api-mapping.md reference**: The api-mapping.md lists `CreateTextObjectDecorator`/`CreateOperatorDecorator` as the C# names. This deviation is an intentional C# idiom adaptation per Constitution I (naming adjusted for C# conventions). The api-mapping.md entry documents the Python → C# mapping; the actual implementation uses the more idiomatic C# names.

**Alternatives Considered**:
- Keep `CreateTextObjectDecorator`: Rejected because "decorator" has no meaningful semantics in C# — it would confuse readers
- `AddTextObjectBindings`: Rejected because it's too generic and doesn't convey the factory pattern

## Research Task 9: Test Strategy (Updated)

**Decision**: Follow the same test pattern as EmacsBindings tests.

**Rationale**:
- `LoadEmacsBindingsTests.cs` provides the proven pattern:
  - `CreateEnvironment()` helper that creates real Buffer, BufferControl, Window, Layout, Application
  - `GetBindings()` helper to query bindings by key sequence
  - `FindNamedCommandBinding()` helper for named command verification
  - Tests verify binding existence, filter activation, and handler behavior
- 13 Vi-specific tests mapped in test-mapping.md
- Additional unit tests for TextObject/TextObjectType

**Test file structure** (per test-mapping.md, Constitution IX):
- `ViModeTests.cs` — 13 mapped integration tests: CursorMovements, Operators, TextObjects, Digraphs, BlockEditing, BlockEditing_EmptyLines, VisualLineCopy, VisualEmptyLine, CharacterDeleteAfterCursor, CharacterDeleteBeforeCursor, CharacterPaste, TempNavigationMode, Macros
- `TextObjectTests.cs` / `TextObjectTypeTests.cs` — unit tests for foundation types
- `LoadViBindings*Tests.cs` — supplementary binding registration and behavior tests (per-category)
- `ViBindingsIntegrationTests.cs` — additional integration tests beyond the 13 mapped tests

**Test categories**:
1. TextObject unit tests (Sorted, OperatorRange, GetLineNumbers, Cut)
2. TextObjectType enum value tests
3. Binding registration tests (verify all 151 bindings exist)
4. Navigation motion behavior tests
5. Operator + motion integration tests
6. Text object selection tests
7. Mode switching tests
8. Visual mode tests
9. Macro recording/playback tests
10. Insert mode binding tests
11. Misc command tests (indent, case, join, etc.)
