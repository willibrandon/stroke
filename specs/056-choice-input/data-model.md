# Data Model: Choice Input

**Feature**: 056-choice-input
**Date**: 2026-02-03

## Entities

### ChoiceInput<T>

**Purpose**: Configuration container for a choice selection prompt. Stores all settings needed to create and run an Application that presents options to the user.

**Type**: `sealed class` (not inheritable, configuration object)

**Thread Safety**: Inherently thread-safe—all fields are read-only after construction.

**Fields**:

| Field | Type | Description | Default |
|-------|------|-------------|---------|
| `Message` | `AnyFormattedText` | Prompt text displayed above options | Required |
| `Options` | `IReadOnlyList<(T Value, AnyFormattedText Label)>` | Value-label pairs for selection | Required |
| `Default` | `T?` | Pre-selected value when prompt displays | `default(T)` |
| `MouseSupport` | `bool` | Enable mouse click selection | `false` |
| `Style` | `IStyle?` | Visual styling for colors/appearance | `null` (uses default) |
| `Symbol` | `string` | Character shown before selected option | `">"` |
| `BottomToolbar` | `AnyFormattedText?` | Help text at screen bottom | `null` |
| `ShowFrame` | `FilterOrBool` | Whether to draw border frame | `default` (false) |
| `EnableSuspend` | `FilterOrBool` | Allow Ctrl+Z suspend (Unix only) | `default` (false) |
| `EnableInterrupt` | `FilterOrBool` | Allow Ctrl+C to cancel | `default` (true) |
| `InterruptException` | `Type` | Exception type thrown on Ctrl+C | `typeof(KeyboardInterrupt)` |
| `KeyBindings` | `IKeyBindingsBase?` | Additional user key bindings | `null` |

**Validation Rules**:

| Rule | Condition | Exception |
|------|-----------|-----------|
| V1 | `options` is null | `ArgumentNullException` |
| V2 | `options` is empty | `ArgumentException("Options cannot be empty")` |
| V3 | `interruptException` not assignable to `BaseException` | `ArgumentException` |

**Relationships**:

```
ChoiceInput<T> ─creates─► Application<T>
                              │
                              ├─contains─► Layout
                              │               │
                              │               ├─► HSplit
                              │               │      ├─► Box(Label) [message]
                              │               │      ├─► Box(RadioList<T>) [options]
                              │               │      ├─► ConditionalContainer [spacer]
                              │               │      └─► ConditionalContainer [toolbar]
                              │               │
                              │               └─► ConditionalContainer(Frame) [optional]
                              │
                              └─uses─► RadioList<T>
                                            │
                                            └─tracks─► Selection State (internal)
```

### Selection State (Internal to RadioList)

**Purpose**: Tracks the current selection position. Managed entirely by the RadioList widget.

**Thread Safety**: Protected by RadioList's internal Lock.

**State**:

| Property | Type | Description |
|----------|------|-------------|
| `SelectedIndex` | `int` | Zero-based index of highlighted option |
| `CurrentValue` | `T` | Value at current index |

**State Transitions**:

```
┌─────────────────┐
│  Initial State  │
│ (default or 0)  │
└────────┬────────┘
         │
         ▼
┌─────────────────┐     Down/j      ┌─────────────────┐
│   Option N      │ ─────────────►  │   Option N+1    │
│   (selected)    │ ◄─────────────  │   (selected)    │
└─────────────────┘     Up/k        └─────────────────┘
         │                                   │
         │ At last ──► Wraps to first        │
         │ At first ◄── Wraps to last        │
         │                                   │
         ▼                                   ▼
┌─────────────────┐                 ┌─────────────────┐
│  Enter pressed  │                 │  Number key 1-9 │
│                 │                 │  (direct jump)  │
└────────┬────────┘                 └────────┬────────┘
         │                                   │
         ▼                                   │
┌─────────────────┐                          │
│  App.Exit()     │ ◄────────────────────────┘
│  with result    │
└─────────────────┘
```

### Default Style

**Purpose**: Visual appearance when no custom style provided.

**Definition**:

```csharp
{
    "frame.border": "#884444",    // Brownish-red border color
    "selected-option": "bold"      // Bold text for selected item
}
```

## Data Flow

### Prompt Execution Flow

```
User calls ChoiceInput.Prompt()
         │
         ▼
┌─────────────────────────┐
│ CreateApplication()     │
│ - Build RadioList       │
│ - Build Layout (HSplit) │
│ - Create KeyBindings    │
│ - Wrap in Application   │
└───────────┬─────────────┘
            │
            ▼
┌─────────────────────────┐
│ Application.Run()       │
│ - Enter rendering loop  │
│ - Process key events    │
│ - Update display        │
└───────────┬─────────────┘
            │
            ▼
┌─────────────────────────┐
│ User navigates/selects  │
│ - Arrow keys move       │
│ - Number keys jump      │
│ - Enter confirms        │
│ - Ctrl+C cancels        │
└───────────┬─────────────┘
            │
            ▼
┌─────────────────────────┐
│ App.Exit() called       │
│ - With result (Enter)   │
│ - With exception (^C)   │
└───────────┬─────────────┘
            │
            ▼
┌─────────────────────────┐
│ Return T or throw       │
└─────────────────────────┘
```

### Key Binding Priority

Key bindings are merged in this order (first match wins):

1. **Local bindings** (Enter, Ctrl+C, Ctrl+Z) - defined in ChoiceInput
2. **User bindings** (via `keyBindings` parameter) - wrapped in DynamicKeyBindings
3. **RadioList bindings** (Up/Down, numbers, mouse) - built into widget

## Invariants

1. **Non-empty options**: Options list must contain at least one item
2. **Valid default**: If default value doesn't match any option, first option is selected
3. **Single result**: Exactly one value is returned (or exception thrown)
4. **Wrap navigation**: Index wraps: `(index + 1) % count` and `(index - 1 + count) % count`
5. **Platform suspend**: Ctrl+Z only functions when `PlatformUtils.SuspendToBackgroundSupported && enableSuspend`
