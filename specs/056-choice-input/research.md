# Research: Choice Input

**Feature**: 056-choice-input
**Date**: 2026-02-03

## Overview

This document resolves technical unknowns and documents design decisions for the ChoiceInput feature.

## Research Findings

### 1. Python PTK Reference Implementation

**Source**: `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/shortcuts/choice_input.py`

**Key Patterns Identified**:

1. **Class Structure**: `ChoiceInput` is a generic class that holds configuration and creates an `Application` on demand via `_create_application()` method
2. **Layout Composition**: Uses HSplit with Box containers for padding, ConditionalContainer for optional Frame
3. **RadioList Configuration**: Passes specific parameters to RadioList:
   - `select_on_focus=True` (highlights option immediately)
   - `show_numbers=True` (displays 1-9 for quick selection)
   - `open_character=""`, `close_character=""` (no brackets around selection)
   - `select_character=symbol` (custom selection indicator)
4. **Key Bindings**: Creates local KeyBindings, merges with DynamicKeyBindings for user-provided bindings
5. **Interrupt Handling**: Uses `@Condition` filter to dynamically evaluate `enable_interrupt` setting
6. **Suspend Support**: Platform-checked via `suspend_to_background_supported` utility

**Decision**: Port exactly as designed in Python PTK, adapting only for C# naming conventions.
**Rationale**: Constitution Principle I mandates 100% API fidelity.
**Alternatives Rejected**: N/A - deviation would violate constitution.

### 2. Existing Stroke Patterns

**Source**: `/Users/brandon/src/stroke/src/Stroke/Shortcuts/Dialogs.cs`

**Key Patterns Identified**:

1. **Dialog Factory Pattern**: Each dialog type has:
   - Static factory method returning `Application<T>`
   - Async convenience method calling `.RunAsync()`
2. **CreateApp Helper**: Internal method wraps container in Application with standard bindings
3. **Focus Navigation**: All dialogs add Tab/Shift+Tab for focus cycling
4. **Style Parameter**: Optional IStyle with null fallback

**Decision**: ChoiceInput will NOT use CreateApp helper—it needs custom full-screen=false behavior.
**Rationale**: Python PTK's ChoiceInput explicitly sets `full_screen=False`, unlike other dialogs.
**Alternatives Rejected**: Using CreateApp would require full_screen=true (different UX).

### 3. RadioList Widget Capabilities

**Source**: `/Users/brandon/src/stroke/src/Stroke/Widgets/Lists/RadioList.cs` and `DialogList.cs`

**Key Capabilities Verified**:

| Capability | Status | Notes |
|------------|--------|-------|
| Generic type parameter | ✅ | `RadioList<T>` returns typed values |
| Show numbers (1-9) | ✅ | `showNumbers` parameter |
| Custom select symbol | ✅ | `selectCharacter` parameter |
| Default value | ✅ | `defaultValue` parameter |
| Wrap navigation | ✅ | Built into DialogList (wraps at boundaries) |
| Thread-safe | ✅ | Uses Lock internally |
| Mouse support | ✅ | Handled by Application mouse_support flag |

**Decision**: Use RadioList directly—it provides all required functionality.
**Rationale**: RadioList already implements FR-002 through FR-007 internally.
**Alternatives Rejected**: Custom widget would duplicate existing functionality.

### 4. Filter System Usage

**Source**: `/Users/brandon/src/stroke/src/Stroke/Filters/`

**Key Patterns**:

```csharp
// Condition creation
var condition = Filters.Condition(() => _enableInterrupt.Evaluate());

// Composite filters
var showToolbar = Filters.Condition(() => _bottomToolbar != null)
    & ~Filters.IsDone
    & Filters.RendererHeightIsKnown;
```

**Decision**: Use `Filters.Condition()` for dynamic evaluation of FilterOrBool parameters.
**Rationale**: Matches Python PTK's `@Condition` decorator pattern.
**Alternatives Rejected**: Static boolean checks would not support dynamic FilterOrBool inputs.

### 5. Platform Suspend Support

**Source**: `/Users/brandon/src/stroke/src/Stroke/Core/PlatformUtils.cs`

**Verification**: `PlatformUtils.SuspendToBackgroundSupported` returns `true` on Unix, `false` on Windows.

**Decision**: Gate Ctrl+Z binding with platform check AND user's enableSuspend setting.
**Rationale**: Matches Python PTK behavior exactly.
**Alternatives Rejected**: None—this is the standard approach.

### 6. Default Style Creation

**Python Reference**:
```python
def create_default_choice_input_style() -> BaseStyle:
    return Style.from_dict({
        "frame.border": "#884444",
        "selected-option": "bold",
    })
```

**Decision**: Create `CreateDefaultChoiceInputStyle()` private method returning `Style.FromDict()`.
**Rationale**: Exact port of Python implementation.
**Alternatives Rejected**: Inline style would reduce readability.

## Resolved Questions

| Question | Resolution |
|----------|------------|
| Should ChoiceInput be a standalone class or part of Dialogs? | Standalone class `ChoiceInput<T>` + convenience method in `Dialogs` |
| How to handle FilterOrBool in key binding filters? | Wrap in `Filters.Condition(() => filter.Evaluate())` |
| Should we use CreateApp helper from Dialogs? | No—ChoiceInput needs `fullScreen=false` |
| How to merge user-provided key bindings? | Use `MergedKeyBindings` with `DynamicKeyBindings` wrapper |
| What exception type for interrupt? | Configurable via `interruptException` parameter, default `KeyboardInterrupt` |

## API Decisions

### Public API (C# Naming)

```csharp
namespace Stroke.Shortcuts;

public sealed class ChoiceInput<T>
{
    public ChoiceInput(
        AnyFormattedText message,
        IReadOnlyList<(T Value, AnyFormattedText Label)> options,
        T? defaultValue = default,
        bool mouseSupport = false,
        IStyle? style = null,
        string symbol = ">",
        AnyFormattedText? bottomToolbar = null,
        FilterOrBool showFrame = default,
        FilterOrBool enableSuspend = default,
        FilterOrBool enableInterrupt = default,    // default: true
        Type? interruptException = null,           // default: typeof(KeyboardInterrupt)
        IKeyBindingsBase? keyBindings = null);

    public T Prompt();
    public Task<T> PromptAsync();
}

// Convenience function in Dialogs.cs
public static partial class Dialogs
{
    public static T Choice<T>(...);        // Same parameters as ChoiceInput constructor
    public static Task<T> ChoiceAsync<T>(...);
}
```

### Python → C# Naming Mappings

| Python | C# |
|--------|-----|
| `choice_input.py` | `ChoiceInput.cs` |
| `ChoiceInput` | `ChoiceInput<T>` |
| `choice()` | `Dialogs.Choice<T>()` |
| `prompt()` | `Prompt()` |
| `prompt_async()` | `PromptAsync()` |
| `_create_application()` | `CreateApplication()` (private) |
| `create_default_choice_input_style()` | `CreateDefaultChoiceInputStyle()` (private) |
| `message` | `message` |
| `options` | `options` |
| `default` | `defaultValue` |
| `mouse_support` | `mouseSupport` |
| `style` | `style` |
| `symbol` | `symbol` |
| `bottom_toolbar` | `bottomToolbar` |
| `show_frame` | `showFrame` |
| `enable_suspend` | `enableSuspend` |
| `enable_interrupt` | `enableInterrupt` |
| `interrupt_exception` | `interruptException` |
| `key_bindings` | `keyBindings` |

## Dependencies Confirmed

All required dependencies exist in the Stroke codebase:

- ✅ `RadioList<T>` (Stroke.Widgets.Lists)
- ✅ `Box`, `Frame`, `Label` (Stroke.Widgets.Base)
- ✅ `HSplit`, `ConditionalContainer` (Stroke.Layout.Containers)
- ✅ `Window`, `FormattedTextControl` (Stroke.Layout)
- ✅ `KeyBindings`, `MergedKeyBindings`, `DynamicKeyBindings` (Stroke.KeyBinding)
- ✅ `Filters.Condition`, `FilterOrBool` (Stroke.Filters)
- ✅ `Application<T>` (Stroke.Application)
- ✅ `Style.FromDict()` (Stroke.Styles)
- ✅ `PlatformUtils.SuspendToBackgroundSupported` (Stroke.Core)
- ✅ `AnyFormattedText` (Stroke.FormattedText)
- ✅ `Dimension` (Stroke.Layout.Dimensions)
