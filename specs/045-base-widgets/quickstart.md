# Quickstart: Base Widgets Implementation

**Feature**: 045-base-widgets
**Date**: 2026-02-01

## Implementation Order

The implementation follows dependency order — simpler widgets first, composed widgets last:

### Phase 1: Foundation (No inter-widget dependencies)

1. **Border** — Static constants, zero dependencies. Required by Frame, VerticalLine, HorizontalLine.
2. **VerticalLine** — Trivial Window wrapper using Border constants.
3. **HorizontalLine** — Trivial Window wrapper using Border constants.
4. **Label** — FormattedTextControl + Window. Required by Frame (title Label) and ProgressBar.
5. **Button** — FormattedTextControl + Window + KeyBindings. Required by Dialog.

### Phase 2: Decorators (Depend on Phase 1 widgets)

6. **Box** — HSplit/VSplit padding composition. Required by Dialog.
7. **Frame** — HSplit/VSplit/ConditionalContainer with Label and Border. Required by Dialog.
8. **Shadow** — FloatContainer with transparent Floats. Required by Dialog.
9. **ProgressBar** — FloatContainer + VSplit + Label.

### Phase 3: Selection Widgets (Independent hierarchy)

10. **DialogList\<T\>** — Base class with full keyboard/mouse handling.
11. **RadioList\<T\>** — Thin subclass of DialogList.
12. **CheckboxList\<T\>** — Thin subclass of DialogList.
13. **Checkbox** — Thin subclass of CheckboxList\<string\>.

### Phase 4: Composition (Depends on Phase 1-2)

14. **TextArea** — Buffer + BufferControl + Window composition.
15. **Dialog** — Frame + Shadow + Box + Button composition.

### Phase 5: Tests

Tests for each widget, organized by phase. See file structure in plan.md.

## Key Patterns

### IMagicContainer Implementation

Every widget follows this pattern:

```csharp
namespace Stroke.Widgets.Base;

public class MyWidget : IMagicContainer
{
    public Window Window { get; }

    public MyWidget(/* params */)
    {
        Window = new Window(/* configuration */);
    }

    public IContainer PtContainer() => Window;
}
```

### Thread Safety Pattern (DialogList, ProgressBar)

```csharp
public class DialogList<T> : IMagicContainer
{
    private readonly Lock _lock = new();
    private int _selectedIndex;

    public int SelectedIndex
    {
        get { using (_lock.EnterScope()) return _selectedIndex; }
        private set { using (_lock.EnterScope()) _selectedIndex = value; }
    }
}
```

### FilterOrBool → Func\<bool\> Bridging (TextArea)

```csharp
// Store writable field
public FilterOrBool ReadOnly { get; set; }

// Bridge to Buffer constructor
buffer = new Buffer(
    readOnly: () => FilterUtils.IsTrue(this.ReadOnly),
    multiline: () => FilterUtils.IsTrue(multiline),
    ...
);
```

### Dynamic Container Pattern (Frame, Dialog)

```csharp
// Runtime-changeable body
public AnyContainer Body { get; set; }

// In constructor, wrap with DynamicContainer
var bodyContainer = new DynamicContainer(() => this.Body.ToContainer());
```

## Build & Test

```bash
# Build
dotnet build src/Stroke/Stroke.csproj

# Run tests
dotnet test tests/Stroke.Tests/Stroke.Tests.csproj --filter "FullyQualifiedName~Widgets"

# Run with coverage
dotnet test tests/Stroke.Tests/Stroke.Tests.csproj --collect:"XPlat Code Coverage" --filter "FullyQualifiedName~Widgets"
```

## Verification Checklist

- [ ] All 15 widget classes compile without errors
- [ ] All widgets implement `IMagicContainer`
- [ ] Border constants match Unicode values
- [ ] TextArea creates Buffer with correct dynamic delegates
- [ ] Button Enter/Space/Mouse handlers fire correctly
- [ ] Frame switches title/no-title rows dynamically
- [ ] Shadow creates 2 transparent Float windows
- [ ] Box padding resolves with fallback logic
- [ ] DialogList keyboard navigation covers all 8 binding groups
- [ ] RadioList enforces single-selection invariant
- [ ] CheckboxList supports multi-selection toggle
- [ ] Checkbox.Checked maps to underlying CurrentValues
- [ ] ProgressBar updates label and weights on percentage change
- [ ] Dialog composes Frame+Shadow+Box+Buttons with focus cycling
- [ ] Thread safety verified for DialogList and ProgressBar
- [ ] No source file exceeds 1000 LOC
- [ ] Test coverage ≥ 80%
