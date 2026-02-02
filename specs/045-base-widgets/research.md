# Research: Base Widgets

**Feature**: 045-base-widgets
**Date**: 2026-02-01

## Research Tasks

### RT-1: IMagicContainer Integration Pattern

**Question**: How should widgets implement `IMagicContainer` to integrate with the layout system?

**Decision**: All widgets implement `IMagicContainer.PtContainer()` returning their inner `IContainer` (typically a `Window`, `HSplit`, or `FloatContainer`).

**Rationale**: This matches Python PTK's `__pt_container__()` protocol exactly. The `AnyContainer` union type already supports `IMagicContainer`, so widgets can be passed anywhere an `AnyContainer` is expected.

**Alternatives considered**:
- Implementing `IContainer` directly: Rejected because it would require re-implementing all container methods (Reset, PreferredWidth, etc.) that are already handled by the inner container. Python widgets don't extend Container either.

### RT-2: Buffer Constructor Parameter Types

**Question**: The Python `TextArea` passes `FilterOrBool` for multiline/read_only, but the Stroke `Buffer` constructor accepts `Func<bool>?`. How to bridge?

**Decision**: TextArea constructor accepts `FilterOrBool` parameters. Internally, convert to `Func<bool>` using lambda closures:
```csharp
// Example: readOnly parameter bridging
this.ReadOnly = readOnly; // Store FilterOrBool
buffer = new Buffer(
    readOnly: () => FilterUtils.IsTrue(this.ReadOnly),
    ...
);
```

**Rationale**: The Python code uses `Condition(lambda: is_true(self.read_only))` for the same bridging. Storing the `FilterOrBool` on the widget allows runtime mutation (users can reassign `textArea.ReadOnly = true`).

**Alternatives considered**:
- Accepting only `bool` parameters: Rejected because Python PTK supports runtime-changeable conditions. The api-mapping.md shows `FilterOrBool` parameters.
- Wrapping in `Condition` objects: Works but `Func<bool>` is simpler since `Buffer` already expects `Func<bool>?`.

### RT-3: Window Callable/Dynamic Dimension Support

**Question**: Python's Window accepts callables for width, height, style, and char. The Stroke Window constructor takes `Dimension?`, `string`, and `string?`. How to handle widgets that need dynamic dimensions (Label width calculation, ProgressBar weights)?

**Decision**: Use the existing pattern in the codebase. The Window constructor stores `_widthGetter = width != null ? () => width : () => null` — it already wraps static values in lambdas. For dynamic values:
1. **Label**: Compute width in a `Func<Dimension?>` getter and pass the result. The Window's `_widthGetter` field is a `Func<Dimension?>`. To use a custom getter, the existing internal `Func<Dimension?>` constructor parameter approach will need a Window constructor overload that accepts `Func<Dimension?>` for width.
2. **ProgressBar**: Each render of the inner VSplit will re-evaluate weights. Since Window already wraps values in lambdas internally, we need constructor overloads accepting `Func<Dimension?>` for width/height and `Func<string?>` for char.

**Rationale**: Python freely passes callables. The C# Window already has the internal infrastructure (`_widthGetter`, `_heightGetter`, `_charGetter` fields). Adding overloaded constructors or factory methods exposes this capability.

**Alternatives considered**:
- Rebuilding the Window on each access: Too expensive, defeats caching
- Using DynamicContainer: Overkill for simple dimension changes

### RT-4: DialogList Thread Safety Design

**Question**: `DialogList<T>` has mutable state (`_selectedIndex`, `CurrentValue`, `CurrentValues`). What synchronization pattern?

**Decision**: Use `System.Threading.Lock` with `EnterScope()` pattern on all mutable state access.

**Rationale**: Per Constitution XI, all mutable classes must use synchronization. The `DialogList` state can be mutated by keyboard handlers (from the UI thread) or programmatic access (from any thread).

**Fields requiring protection**:
- `_selectedIndex`: int — read/written by navigation handlers and mouse handlers
- `CurrentValue`: T — read/written by enter handler
- `CurrentValues`: List<T> — read/written by enter handler (add/remove)

**Alternatives considered**:
- Volatile fields only: Insufficient for compound operations (read-modify-write on CurrentValues)
- ConcurrentDictionary for CurrentValues: Overkill; Lock is simpler and matches project convention

### RT-5: FormattedTextControl Mouse Handler Pattern

**Question**: Python Button uses a mouse handler function in `StyleAndTextTuples`. How does this translate to C#?

**Decision**: `StyleAndTextTuple` in Stroke includes an optional `Action<MouseEvent>?` mouse handler as the third element, matching the Python 3-tuple pattern `(style, text, handler)`. The `FormattedTextControl` already processes these.

**Rationale**: The Python formatted text system uses 3-tuples `(style_str, text, handler)` where handler is optional. The Stroke `StyleAndTextTuple` record already supports this pattern.

**Verified**: `StyleAndTextTuple` has the mouse handler field.

### RT-6: AnyContainer Implicit Conversions for Widget Types

**Question**: Can widgets be passed directly where `AnyContainer` is expected?

**Decision**: `AnyContainer` has constructors accepting `IContainer` and `IMagicContainer`. Since C# doesn't allow implicit conversions from interfaces, widgets implementing `IMagicContainer` must be explicitly wrapped: `new AnyContainer(widget)`. However, method overloads in containers (HSplit, VSplit, etc.) that accept `IReadOnlyList<IContainer>` will need the widget's container extracted first via `PtContainer()`.

**Rationale**: The existing `AnyContainer.From(object)` method handles dynamic dispatch. For compile-time safety, explicit construction is preferred.

**Impact**: In Frame/Shadow/Box/Dialog constructors, use `new AnyContainer(widget)` or call `widget.PtContainer()` when building container hierarchies.

### RT-7: ConditionalContainer Alternative Content

**Question**: Frame uses `ConditionalContainer` with `alternative_content` for the title/no-title top row. Does the Stroke implementation support this?

**Decision**: Yes. `ConditionalContainer` already accepts `alternativeContent` parameter. Verified in `ConditionalContainer.cs`:
```csharp
public ConditionalContainer(
    AnyContainer content,
    FilterOrBool filter = default,
    AnyContainer alternativeContent = default)
```

**Rationale**: Direct match to Python PTK. No adaptation needed.

### RT-8: Namespace Organization

**Question**: What namespace should the new widgets use?

**Decision**:
- `Stroke.Widgets.Base` — Border, TextArea, Label, Button, Frame, Shadow, Box, ProgressBar, VerticalLine, HorizontalLine
- `Stroke.Widgets.Lists` — DialogList<T>, RadioList<T>, CheckboxList<T>, Checkbox
- `Stroke.Widgets.Dialogs` — Dialog

**Rationale**: Mirrors the logical grouping in Python PTK (`widgets.base`, `widgets.dialogs`) while adding `Lists` as a sub-namespace for the `DialogList` hierarchy. The existing toolbars use `Stroke.Widgets.Toolbars`.

**Alternatives considered**:
- Flat `Stroke.Widgets` namespace for everything: Would work but mixing 15+ classes in one namespace reduces discoverability
- Matching Python exactly (`Stroke.Widgets.Base` for all of base.py): Acceptable, but separating Lists improves organization since they share a class hierarchy

## Summary

All research tasks resolved. No NEEDS CLARIFICATION items remain. Key findings:
1. Widgets implement `IMagicContainer`, not `IContainer`
2. `FilterOrBool` → `Func<bool>` bridging via lambda closures
3. Window may need constructor overloads for `Func<Dimension?>` parameters
4. `DialogList` and `ProgressBar` need Lock synchronization
5. `StyleAndTextTuple` already supports mouse handlers
6. `ConditionalContainer` supports alternative content (Frame title switching)
7. Three sub-namespaces: Base, Lists, Dialogs
