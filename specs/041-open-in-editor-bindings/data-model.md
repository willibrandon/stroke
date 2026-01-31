# Data Model: Open in Editor Bindings

**Feature**: 041-open-in-editor-bindings
**Date**: 2026-01-31

## Entities

This feature introduces no new data entities. It is a purely behavioral feature that registers key bindings using existing infrastructure.

### Existing Entities Used

| Entity | Role | Location |
|--------|------|----------|
| `KeyBindings` | Binding registry — stores key-to-handler mappings | `Stroke.KeyBinding.KeyBindings` |
| `MergedKeyBindings` | Combines multiple `IKeyBindingsBase` instances | `Stroke.KeyBinding.MergedKeyBindings` |
| `Binding` | Individual key binding entry (keys, handler, filter) | `Stroke.KeyBinding.Binding` |
| `KeyOrChar` | Union type for key enum value or character literal | `Stroke.KeyBinding.KeyOrChar` |
| `FilterOrBool` | Wrapper for filter or constant boolean in binding parameters | `Stroke.Filters.FilterOrBool` |
| `IFilter` | Composable boolean condition interface | `Stroke.Filters.IFilter` |
| `Buffer` | Mutable text buffer with `OpenInEditorAsync` method | `Stroke.Core.Buffer` |

### Relationships

```text
OpenInEditorBindings (static)
├── creates → KeyBindings (per-loader)
│   └── contains → Binding
│       ├── keys: KeyOrChar[] (e.g., [ControlX, ControlE] or ['v'])
│       ├── handler: Binding (from NamedCommands.GetByName)
│       └── filter: FilterOrBool (EmacsMode & ~HasSelection or ViNavigationMode)
└── creates → MergedKeyBindings (combined loader)
    ├── merges → KeyBindings (from Emacs loader)
    └── merges → KeyBindings (from Vi loader)
```

### State Transitions

None. Binding loaders are stateless factory functions. The bindings they create are immutable once registered.
